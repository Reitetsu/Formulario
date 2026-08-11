using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.Models.Entities;

namespace Sysbimbo.Api.Services;

/// <summary>
/// Copia una sola vez los datos del formulario desde la base SQL Server heredada
/// hacia PostgreSQL. El proceso puede repetirse: omite las claves ya trasladadas.
/// </summary>
public class FormularioDataMigrationService(
    SysbimboDbContext sqlServer,
    FormularioDbContext postgres,
    ILogger<FormularioDataMigrationService> logger)
{
    private const int StoreBatchSize = 200;
    private const int MaterialBatchSize = 100;
    private const int PhotoBatchSize = 5;

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Aplicando migraciones del formulario en PostgreSQL...");
        await postgres.Database.MigrateAsync(cancellationToken);

        var storesCopied = await CopyStoresAsync(cancellationToken);
        var materialsCopied = await CopyMaterialsAsync(cancellationToken);
        var photosCopied = await CopyPhotosAsync(cancellationToken);

        await SynchronizeIdentitySequencesAsync(cancellationToken);
        await VerifyCountsAsync(cancellationToken);

        logger.LogInformation(
            "Migracion finalizada. Tiendas: {Stores}; materiales: {Materials}; fotos: {Photos}.",
            storesCopied,
            materialsCopied,
            photosCopied);
    }

    private async Task VerifyCountsAsync(CancellationToken cancellationToken)
    {
        var sqlStores = await sqlServer.Tiendas.AsNoTracking().LongCountAsync(cancellationToken);
        var sqlMaterials = await sqlServer.MaterialesImpulsoTienda.AsNoTracking().LongCountAsync(cancellationToken);
        var sqlPhotos = await sqlServer.FotosMaterialImpulso.AsNoTracking().LongCountAsync(cancellationToken);

        var postgresStores = await postgres.Tiendas.AsNoTracking().LongCountAsync(cancellationToken);
        var postgresMaterials = await postgres.MaterialesImpulsoTienda.AsNoTracking().LongCountAsync(cancellationToken);
        var postgresPhotos = await postgres.FotosMaterialImpulso.AsNoTracking().LongCountAsync(cancellationToken);

        logger.LogInformation(
            "Verificacion de totales - SQL Server/PostgreSQL: tiendas {SqlStores}/{PostgresStores}, materiales {SqlMaterials}/{PostgresMaterials}, fotos {SqlPhotos}/{PostgresPhotos}.",
            sqlStores,
            postgresStores,
            sqlMaterials,
            postgresMaterials,
            sqlPhotos,
            postgresPhotos);

        if (sqlStores != postgresStores ||
            sqlMaterials != postgresMaterials ||
            sqlPhotos != postgresPhotos)
        {
            throw new InvalidOperationException(
                "Los totales entre SQL Server y PostgreSQL no coinciden. La base SQL Server no fue modificada.");
        }
    }

    private async Task<int> CopyStoresAsync(CancellationToken cancellationToken)
    {
        var existingKeys = (await postgres.Tiendas
            .AsNoTracking()
            .Select(x => x.TiendaCadenaKey)
            .ToArrayAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        var copied = 0;
        var pending = 0;

        await foreach (var source in sqlServer.Tiendas
                           .AsNoTracking()
                           .OrderBy(x => x.TiendaCadenaKey)
                           .AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
        {
            if (!existingKeys.Add(source.TiendaCadenaKey))
            {
                continue;
            }

            postgres.Tiendas.Add(CloneStore(source));
            copied++;
            pending++;

            if (pending >= StoreBatchSize)
            {
                await postgres.SaveChangesAsync(cancellationToken);
                postgres.ChangeTracker.Clear();
                pending = 0;
            }
        }

        if (pending > 0)
        {
            await postgres.SaveChangesAsync(cancellationToken);
            postgres.ChangeTracker.Clear();
        }

        logger.LogInformation("Tiendas copiadas a PostgreSQL: {Count}.", copied);
        return copied;
    }

    private async Task<int> CopyMaterialsAsync(CancellationToken cancellationToken)
    {
        var existingIds = (await postgres.MaterialesImpulsoTienda
            .AsNoTracking()
            .Select(x => x.MaterialImpulsoTiendaId)
            .ToArrayAsync(cancellationToken))
            .ToHashSet();

        var copied = 0;
        var pending = 0;

        await foreach (var source in sqlServer.MaterialesImpulsoTienda
                           .AsNoTracking()
                           .OrderBy(x => x.MaterialImpulsoTiendaId)
                           .AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
        {
            if (!existingIds.Add(source.MaterialImpulsoTiendaId))
            {
                continue;
            }

            postgres.MaterialesImpulsoTienda.Add(new MaterialImpulsoTienda
            {
                MaterialImpulsoTiendaId = source.MaterialImpulsoTiendaId,
                TiendaCadenaKey = source.TiendaCadenaKey,
                NombreMaterial = source.NombreMaterial,
                Descripcion = source.Descripcion,
                CuotaDiaria = source.CuotaDiaria,
                Activo = source.Activo,
                FechaCreacion = AsUtc(source.FechaCreacion)
            });
            copied++;
            pending++;

            if (pending >= MaterialBatchSize)
            {
                await postgres.SaveChangesAsync(cancellationToken);
                postgres.ChangeTracker.Clear();
                pending = 0;
            }
        }

        if (pending > 0)
        {
            await postgres.SaveChangesAsync(cancellationToken);
            postgres.ChangeTracker.Clear();
        }

        logger.LogInformation("Materiales copiados a PostgreSQL: {Count}.", copied);
        return copied;
    }

    private async Task<int> CopyPhotosAsync(CancellationToken cancellationToken)
    {
        var existingIds = (await postgres.FotosMaterialImpulso
            .AsNoTracking()
            .Select(x => x.FotoMaterialImpulsoId)
            .ToArrayAsync(cancellationToken))
            .ToHashSet();

        var copied = 0;
        var pending = 0;

        await foreach (var source in sqlServer.FotosMaterialImpulso
                           .AsNoTracking()
                           .OrderBy(x => x.FotoMaterialImpulsoId)
                           .AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
        {
            if (!existingIds.Add(source.FotoMaterialImpulsoId))
            {
                continue;
            }

            postgres.FotosMaterialImpulso.Add(new FotoMaterialImpulso
            {
                FotoMaterialImpulsoId = source.FotoMaterialImpulsoId,
                MaterialImpulsoTiendaId = source.MaterialImpulsoTiendaId,
                TiendaCadenaKey = source.TiendaCadenaKey,
                NombreArchivo = source.NombreArchivo,
                TipoContenido = source.TipoContenido,
                TamanoBytes = source.TamanoBytes,
                Contenido = source.Contenido,
                FechaCaptura = AsUtc(source.FechaCaptura)
            });
            copied++;
            pending++;

            // Las fotos pueden medir hasta 10 MB. Un lote pequeno evita elevar
            // innecesariamente el uso de memoria durante el traslado.
            if (pending >= PhotoBatchSize)
            {
                await postgres.SaveChangesAsync(cancellationToken);
                postgres.ChangeTracker.Clear();
                pending = 0;
            }
        }

        if (pending > 0)
        {
            await postgres.SaveChangesAsync(cancellationToken);
            postgres.ChangeTracker.Clear();
        }

        logger.LogInformation("Fotografias copiadas a PostgreSQL: {Count}.", copied);
        return copied;
    }

    private async Task SynchronizeIdentitySequencesAsync(CancellationToken cancellationToken)
    {
        await postgres.Database.ExecuteSqlRawAsync(
            """
            SELECT setval(
                pg_get_serial_sequence('materiales_impulso_tienda', 'material_impulso_tienda_id'),
                COALESCE(MAX(material_impulso_tienda_id), 1),
                MAX(material_impulso_tienda_id) IS NOT NULL)
            FROM materiales_impulso_tienda;
            """,
            cancellationToken);

        await postgres.Database.ExecuteSqlRawAsync(
            """
            SELECT setval(
                pg_get_serial_sequence('fotos_material_impulso', 'foto_material_impulso_id'),
                COALESCE(MAX(foto_material_impulso_id), 1),
                MAX(foto_material_impulso_id) IS NOT NULL)
            FROM fotos_material_impulso;
            """,
            cancellationToken);
    }

    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static DimTiendaMaestraExport CloneStore(DimTiendaMaestraExport source) =>
        new()
        {
            TiendaCadenaKey = source.TiendaCadenaKey,
            CodigoTiendaB2BPrefijo = source.CodigoTiendaB2BPrefijo,
            CodigoTiendaB2B = source.CodigoTiendaB2B,
            NombreTienda = source.NombreTienda,
            NombreTiendaBimbo = source.NombreTiendaBimbo,
            Canal = source.Canal,
            Cadena = source.Cadena,
            Formato = source.Formato,
            TipoLocal = source.TipoLocal,
            LimaProvincias = source.LimaProvincias,
            Region = source.Region,
            Provincia = source.Provincia,
            Ruta = source.Ruta,
            Supervisor = source.Supervisor,
            Gestor = source.Gestor,
            Vendedor = source.Vendedor,
            UltimaFecha = source.UltimaFecha.HasValue
                ? DateTime.SpecifyKind(source.UltimaFecha.Value, DateTimeKind.Unspecified)
                : null,
            CantidadRegistros = source.CantidadRegistros,
            FuenteTienda = source.FuenteTienda
        };
}

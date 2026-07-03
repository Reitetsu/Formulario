using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Constants;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.DTOs.Campanias;
using Sysbimbo.Api.Helpers;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Repositories.Interfaces;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Services;

public class CampaniaProgramacionService(
    ICampaniaRepository campaniaRepository,
    ICampaniaProgramacionRepository campaniaProgramacionRepository,
    SysbimboDbContext dbContext,
    TimeProvider timeProvider) : ICampaniaProgramacionService
{
    public async Task<CampaniaResumenDto> GetResumenAsync(int campaniaId, CancellationToken cancellationToken)
    {
        var campania = await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);
        var programaciones = await campaniaProgramacionRepository.GetProgramacionesByCampaniaAsync(campaniaId, cancellationToken);
        var grouped = programaciones
            .GroupBy(x => ProgramacionEstados.Normalizar(x.Estado) ?? "<NULL>")
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);

        var nonCancelled = programaciones
            .Where(x => !ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Cancelada))
            .ToArray();

        var detalles = await campaniaProgramacionRepository.GetDetallesByProgramacionIdsAsync(
            nonCancelled.Select(x => x.ProgramacionId).ToArray(),
            cancellationToken);

        return new CampaniaResumenDto
        {
            CampaniaId = campania.CampaniaId,
            NombreCampania = campania.NombreCampania ?? string.Empty,
            CantidadTiendas = nonCancelled
                .Where(x => !string.IsNullOrWhiteSpace(x.TiendaCadenaKey))
                .Select(x => x.TiendaCadenaKey!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            CantidadFechas = nonCancelled
                .Where(x => x.Fecha.HasValue)
                .Select(x => DateOnly.FromDateTime(x.Fecha!.Value))
                .Distinct()
                .Count(),
            CantidadSkus = detalles
                .Where(x => !string.IsNullOrWhiteSpace(x.CodigoSkuBimbo))
                .Select(x => x.CodigoSkuBimbo)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            CantidadProgramacionesProgramadas = grouped.GetValueOrDefault(ProgramacionEstados.Programada),
            CantidadProgramacionesEjecutadas = grouped.GetValueOrDefault(ProgramacionEstados.Ejecutada),
            CantidadProgramacionesCanceladas = grouped.GetValueOrDefault(ProgramacionEstados.Cancelada),
            CantidadDetalles = detalles.Count
        };
    }

    public async Task<IReadOnlyCollection<CampaniaTiendaDto>> GetTiendasAsync(int campaniaId, CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var programaciones = await campaniaProgramacionRepository.GetProgramacionesByCampaniaAsync(campaniaId, cancellationToken);
        var tiendaKeys = programaciones
            .Where(x => !string.IsNullOrWhiteSpace(x.TiendaCadenaKey))
            .Select(x => x.TiendaCadenaKey!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var tiendas = await dbContext.Tiendas
            .AsNoTracking()
            .Where(x => tiendaKeys.Contains(x.TiendaCadenaKey))
            .ToDictionaryAsync(x => x.TiendaCadenaKey, cancellationToken);

        return programaciones
            .Where(x => !string.IsNullOrWhiteSpace(x.TiendaCadenaKey))
            .GroupBy(x => x.TiendaCadenaKey!, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                tiendas.TryGetValue(group.Key, out var tienda);
                var fechas = group
                    .Where(x => x.Fecha.HasValue)
                    .Select(x => DateOnly.FromDateTime(x.Fecha!.Value))
                    .OrderBy(x => x)
                    .ToArray();

                return new CampaniaTiendaDto
                {
                    TiendaCadenaKey = group.Key,
                    CodigoTiendaB2B = tienda?.CodigoTiendaB2B,
                    NombreTienda = tienda?.NombreTienda,
                    NombreTiendaBimbo = tienda?.NombreTiendaBimbo,
                    Cadena = tienda?.Cadena,
                    Formato = tienda?.Formato,
                    Region = tienda?.Region,
                    CantidadFechas = fechas.Distinct().Count(),
                    PrimeraFecha = fechas.FirstOrDefault(),
                    UltimaFecha = fechas.LastOrDefault(),
                    CantidadProgramadas = group.Count(x => ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Programada)),
                    CantidadEjecutadas = group.Count(x => ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Ejecutada)),
                    CantidadCanceladas = group.Count(x => ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Cancelada))
                };
            })
            .OrderBy(x => x.NombreTiendaBimbo ?? x.NombreTienda ?? x.TiendaCadenaKey)
            .ToArray();
    }

    public async Task<CampaniaOperacionResultadoDto> AddTiendasAsync(
        int campaniaId,
        AddCampaniaTiendasRequestDto request,
        CancellationToken cancellationToken)
    {
        var campania = await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var tiendaKeys = NormalizeDistinctValues(request.TiendaCadenaKeys);
        if (tiendaKeys.Count == 0)
        {
            throw new InvalidOperationException("Debes enviar al menos una tienda para programar la campania.");
        }

        var tiendasValidas = await dbContext.Tiendas
            .AsNoTracking()
            .Where(x => tiendaKeys.Contains(x.TiendaCadenaKey))
            .Select(x => x.TiendaCadenaKey)
            .ToListAsync(cancellationToken);

        var tiendasInvalidas = tiendaKeys
            .Except(tiendasValidas, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (tiendasInvalidas.Length > 0)
        {
            throw new InvalidOperationException(
                $"No se encontraron estas tiendas en la maestra: {string.Join(", ", tiendasInvalidas)}.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = timeProvider.GetLocalNow().DateTime;
        var programaciones = await dbContext.Programaciones
            .Where(x => x.CampaniaId == campaniaId)
            .ToListAsync(cancellationToken);

        var fechasSolicitadas = request.Fechas
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        var fechasActivas = fechasSolicitadas.Length > 0
            ? fechasSolicitadas
            : programaciones
                .Where(x => !ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Cancelada) && x.Fecha.HasValue)
                .Select(x => DateOnly.FromDateTime(x.Fecha!.Value))
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

        if (fechasActivas.Length == 0)
        {
            throw new InvalidOperationException(
                "La campania no tiene fechas activas. Agrega fechas antes de asociar tiendas.");
        }

        ValidateFechasWithinCampania(fechasActivas, campania);

        var programacionLookup = programaciones
            .Where(x => !string.IsNullOrWhiteSpace(x.TiendaCadenaKey) && x.Fecha.HasValue)
            .ToDictionary(
                x => BuildProgramacionKey(x.TiendaCadenaKey!, DateOnly.FromDateTime(x.Fecha!.Value)),
                StringComparer.OrdinalIgnoreCase);

        var nuevasProgramaciones = new List<Programacion>();
        var programacionesObjetivo = new List<Programacion>();
        var creados = 0;
        var reactivados = 0;
        var omitidos = 0;

        foreach (var tiendaKey in tiendaKeys)
        {
            foreach (var fecha in fechasActivas)
            {
                var key = BuildProgramacionKey(tiendaKey, fecha);
                if (programacionLookup.TryGetValue(key, out var existente))
                {
                    if (ProgramacionEstados.EsEstado(existente.Estado, ProgramacionEstados.Cancelada))
                    {
                        existente.Estado = ProgramacionEstados.Programada;
                        existente.FuenteProgramacion = ProgramacionFuentes.ModuloCampanias;
                        existente.FechaActualizacion = now;
                        reactivados++;
                        programacionesObjetivo.Add(existente);
                    }
                    else
                    {
                        omitidos++;
                    }

                    continue;
                }

                var nuevaProgramacion = new Programacion
                {
                    CampaniaId = campaniaId,
                    TiendaCadenaKey = tiendaKey,
                    Fecha = fecha.ToDateTime(TimeOnly.MinValue),
                    Estado = ProgramacionEstados.Programada,
                    FuenteProgramacion = ProgramacionFuentes.ModuloCampanias,
                    FechaCreacion = now,
                    FechaActualizacion = now
                };

                nuevasProgramaciones.Add(nuevaProgramacion);
                programacionesObjetivo.Add(nuevaProgramacion);
                programacionLookup[key] = nuevaProgramacion;
                creados++;
            }
        }

        if (nuevasProgramaciones.Count > 0)
        {
            await dbContext.Programaciones.AddRangeAsync(nuevasProgramaciones, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var codigosSku = request.ReplicarSkusExistentes
            ? await GetCampaignSkuCodesAsync(campaniaId, cancellationToken)
            : Array.Empty<string>();
        var detallesCreados = await CreateMissingDetallesAsync(programacionesObjetivo, codigosSku, now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CampaniaOperacionResultadoDto
        {
            Mensaje = "Tiendas asociadas a la campania correctamente.",
            Procesados = tiendaKeys.Count,
            Creados = creados,
            Reactivados = reactivados,
            Omitidos = omitidos,
            DetallesCreados = detallesCreados,
            Advertencias = BuildReplicationWarnings(request.ReplicarSkusExistentes, codigosSku.Count)
        };
    }

    public async Task<CampaniaOperacionResultadoDto> RemoveTiendaAsync(
        int campaniaId,
        string tiendaCadenaKey,
        CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var tiendaKey = NormalizeRequiredValue(tiendaCadenaKey, "Debes indicar la tienda a retirar.");
        var now = timeProvider.GetLocalNow().DateTime;

        var programaciones = await dbContext.Programaciones
            .Where(x => x.CampaniaId == campaniaId && x.TiendaCadenaKey == tiendaKey)
            .ToListAsync(cancellationToken);

        if (programaciones.Count == 0)
        {
            throw new KeyNotFoundException(
                $"No se encontraron programaciones para la tienda {tiendaKey} en la campania {campaniaId}.");
        }

        var actualizados = 0;
        var omitidos = 0;
        var advertencias = new List<string>();

        foreach (var programacion in programaciones)
        {
            if (ProgramacionEstados.EsEstado(programacion.Estado, ProgramacionEstados.Ejecutada))
            {
                omitidos++;
                continue;
            }

            if (ProgramacionEstados.EsEstado(programacion.Estado, ProgramacionEstados.Cancelada))
            {
                omitidos++;
                continue;
            }

            programacion.Estado = ProgramacionEstados.Cancelada;
            programacion.FechaActualizacion = now;
            actualizados++;
        }

        if (actualizados > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (omitidos > 0)
        {
            advertencias.Add("Algunas programaciones ya estaban canceladas o ejecutadas y no se modificaron.");
        }

        return new CampaniaOperacionResultadoDto
        {
            Mensaje = "Tienda retirada de la campania.",
            Procesados = programaciones.Count,
            Actualizados = actualizados,
            Omitidos = omitidos,
            Advertencias = advertencias
        };
    }

    public async Task<IReadOnlyCollection<CampaniaFechaDto>> GetFechasAsync(int campaniaId, CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var programaciones = await dbContext.Programaciones
            .AsNoTracking()
            .Where(x =>
                x.CampaniaId == campaniaId &&
                x.Fecha.HasValue &&
                x.Estado != ProgramacionEstados.Cancelada)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.ProgramacionId)
            .ToListAsync(cancellationToken);

        var programacionIds = programaciones.Select(x => x.ProgramacionId).ToArray();
        var detalles = await campaniaProgramacionRepository.GetDetallesByProgramacionIdsAsync(programacionIds, cancellationToken);
        var detallesPorProgramacion = detalles
            .GroupBy(x => x.ProgramacionId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.CodigoSkuBimbo).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        return programaciones
            .GroupBy(x => DateOnly.FromDateTime(x.Fecha!.Value))
            .Select(group => new CampaniaFechaDto
            {
                Fecha = group.Key,
                CantidadTiendas = group
                    .Where(x => !string.IsNullOrWhiteSpace(x.TiendaCadenaKey))
                    .Select(x => x.TiendaCadenaKey!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                CantidadProgramaciones = group.Count(),
                CantidadSkus = group
                    .Select(x => detallesPorProgramacion.GetValueOrDefault(x.ProgramacionId))
                    .Sum(),
                CantidadProgramadas = group.Count(x => ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Programada)),
                CantidadEjecutadas = group.Count(x => ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Ejecutada)),
                CantidadCanceladas = 0
            })
            .OrderByDescending(x => x.Fecha)
            .ToArray();
    }

    public async Task<CampaniaOperacionResultadoDto> AddFechasAsync(
        int campaniaId,
        AddCampaniaFechasRequestDto request,
        CancellationToken cancellationToken)
    {
        var campania = await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var fechas = request.Fechas
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        if (fechas.Length == 0)
        {
            throw new InvalidOperationException("Debes enviar al menos una fecha para programar la campania.");
        }

        ValidateFechasWithinCampania(fechas, campania);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = timeProvider.GetLocalNow().DateTime;
        var programaciones = await dbContext.Programaciones
            .Where(x => x.CampaniaId == campaniaId)
            .ToListAsync(cancellationToken);

        var tiendasSolicitadas = NormalizeDistinctValues(request.TiendaCadenaKeys);
        var tiendasActivas = tiendasSolicitadas.Count > 0 && !request.AplicarATodasLasTiendas
            ? tiendasSolicitadas.OrderBy(x => x).ToArray()
            : programaciones
                .Where(x => !ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Cancelada) && !string.IsNullOrWhiteSpace(x.TiendaCadenaKey))
                .Select(x => x.TiendaCadenaKey!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray();

        if (tiendasActivas.Length == 0)
        {
            throw new InvalidOperationException(
                "La campania no tiene tiendas activas. Agrega tiendas antes de registrar fechas.");
        }

        var tiendasValidas = await dbContext.Tiendas
            .AsNoTracking()
            .Where(x => tiendasActivas.Contains(x.TiendaCadenaKey))
            .Select(x => x.TiendaCadenaKey)
            .ToListAsync(cancellationToken);

        var tiendasInvalidas = tiendasActivas
            .Except(tiendasValidas, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (tiendasInvalidas.Length > 0)
        {
            throw new InvalidOperationException(
                $"No se encontraron estas tiendas en la maestra: {string.Join(", ", tiendasInvalidas)}.");
        }

        var programacionLookup = programaciones
            .Where(x => !string.IsNullOrWhiteSpace(x.TiendaCadenaKey) && x.Fecha.HasValue)
            .ToDictionary(
                x => BuildProgramacionKey(x.TiendaCadenaKey!, DateOnly.FromDateTime(x.Fecha!.Value)),
                StringComparer.OrdinalIgnoreCase);

        var nuevasProgramaciones = new List<Programacion>();
        var programacionesObjetivo = new List<Programacion>();
        var creados = 0;
        var reactivados = 0;
        var omitidos = 0;

        foreach (var fecha in fechas)
        {
            foreach (var tiendaKey in tiendasActivas)
            {
                var key = BuildProgramacionKey(tiendaKey, fecha);
                if (programacionLookup.TryGetValue(key, out var existente))
                {
                    if (ProgramacionEstados.EsEstado(existente.Estado, ProgramacionEstados.Cancelada))
                    {
                        existente.Estado = ProgramacionEstados.Programada;
                        existente.FuenteProgramacion = ProgramacionFuentes.ModuloCampanias;
                        existente.FechaActualizacion = now;
                        reactivados++;
                        programacionesObjetivo.Add(existente);
                    }
                    else
                    {
                        omitidos++;
                    }

                    continue;
                }

                var nuevaProgramacion = new Programacion
                {
                    CampaniaId = campaniaId,
                    TiendaCadenaKey = tiendaKey,
                    Fecha = fecha.ToDateTime(TimeOnly.MinValue),
                    Estado = ProgramacionEstados.Programada,
                    FuenteProgramacion = ProgramacionFuentes.ModuloCampanias,
                    FechaCreacion = now,
                    FechaActualizacion = now
                };

                nuevasProgramaciones.Add(nuevaProgramacion);
                programacionesObjetivo.Add(nuevaProgramacion);
                programacionLookup[key] = nuevaProgramacion;
                creados++;
            }
        }

        if (nuevasProgramaciones.Count > 0)
        {
            await dbContext.Programaciones.AddRangeAsync(nuevasProgramaciones, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var codigosSku = request.ReplicarSkusExistentes
            ? await GetCampaignSkuCodesAsync(campaniaId, cancellationToken)
            : Array.Empty<string>();
        var detallesCreados = await CreateMissingDetallesAsync(programacionesObjetivo, codigosSku, now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CampaniaOperacionResultadoDto
        {
            Mensaje = "Fechas asociadas a la campania correctamente.",
            Procesados = fechas.Length,
            Creados = creados,
            Reactivados = reactivados,
            Omitidos = omitidos,
            DetallesCreados = detallesCreados,
            Advertencias = BuildReplicationWarnings(request.ReplicarSkusExistentes, codigosSku.Count)
        };
    }

    public async Task<CampaniaOperacionResultadoDto> RemoveFechaAsync(
        int campaniaId,
        DateOnly fecha,
        CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var fechaBuscada = fecha.ToDateTime(TimeOnly.MinValue);
        var now = timeProvider.GetLocalNow().DateTime;

        var programaciones = await dbContext.Programaciones
            .Where(x => x.CampaniaId == campaniaId && x.Fecha == fechaBuscada)
            .ToListAsync(cancellationToken);

        if (programaciones.Count == 0)
        {
            throw new KeyNotFoundException(
                $"No se encontraron programaciones para la fecha {fecha:yyyy-MM-dd} en la campania {campaniaId}.");
        }

        var actualizados = 0;
        var omitidos = 0;
        var advertencias = new List<string>();

        foreach (var programacion in programaciones)
        {
            if (ProgramacionEstados.EsEstado(programacion.Estado, ProgramacionEstados.Ejecutada))
            {
                omitidos++;
                continue;
            }

            if (ProgramacionEstados.EsEstado(programacion.Estado, ProgramacionEstados.Cancelada))
            {
                omitidos++;
                continue;
            }

            programacion.Estado = ProgramacionEstados.Cancelada;
            programacion.FechaActualizacion = now;
            actualizados++;
        }

        if (actualizados > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (omitidos > 0)
        {
            advertencias.Add("Algunas programaciones ya estaban canceladas o ejecutadas y no se modificaron.");
        }

        return new CampaniaOperacionResultadoDto
        {
            Mensaje = "Fecha retirada de la campania.",
            Procesados = programaciones.Count,
            Actualizados = actualizados,
            Omitidos = omitidos,
            Advertencias = advertencias
        };
    }

    public async Task<IReadOnlyCollection<CampaniaSkuDto>> GetSkusAsync(int campaniaId, CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var programaciones = await campaniaProgramacionRepository.GetProgramacionesByCampaniaAsync(campaniaId, cancellationToken);
        var details = await campaniaProgramacionRepository.GetDetallesByProgramacionIdsAsync(
            programaciones.Select(x => x.ProgramacionId).ToArray(),
            cancellationToken);

        var programacionPorId = programaciones.ToDictionary(x => x.ProgramacionId);
        var skuCodes = details
            .Where(x => !string.IsNullOrWhiteSpace(x.CodigoSkuBimbo))
            .Select(x => x.CodigoSkuBimbo)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var skuMaestro = await BuildSkuMaestroLookupAsync(skuCodes, cancellationToken);

        return details
            .Where(x => !string.IsNullOrWhiteSpace(x.CodigoSkuBimbo))
            .GroupBy(x => x.CodigoSkuBimbo, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                skuMaestro.TryGetValue(group.Key, out var sku);

                var relatedProgramaciones = group
                    .Select(x => programacionPorId.GetValueOrDefault(x.ProgramacionId))
                    .Where(x => x is not null)
                    .Cast<Programacion>()
                    .ToArray();

                return new CampaniaSkuDto
                {
                    CodigoSkuBimbo = group.Key,
                    CodigoSkuB2B = sku?.CodigoSkuB2B,
                    NombreSkuBimbo = sku?.NombreSkuBimbo,
                    NombreSkuB2B = sku?.NombreSkuB2B,
                    Marca = sku?.Marca,
                    Categoria = sku?.Categoria,
                    Area = sku?.Area,
                    UnidadNegocio = sku?.UnidadNegocio,
                    CantidadProgramaciones = relatedProgramaciones.Select(x => x.ProgramacionId).Distinct().Count(),
                    CantidadTiendas = relatedProgramaciones
                        .Where(x => !string.IsNullOrWhiteSpace(x.TiendaCadenaKey))
                        .Select(x => x.TiendaCadenaKey!)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count(),
                    CantidadFechas = relatedProgramaciones
                        .Where(x => x.Fecha.HasValue)
                        .Select(x => DateOnly.FromDateTime(x.Fecha!.Value))
                        .Distinct()
                        .Count()
                };
            })
            .OrderBy(x => x.NombreSkuBimbo ?? x.CodigoSkuBimbo)
            .ToArray();
    }

    public async Task<CampaniaOperacionResultadoDto> AddSkusAsync(
        int campaniaId,
        AddCampaniaSkusRequestDto request,
        CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var codigosSku = NormalizeDistinctValues(request.CodigosSkuBimbo);
        if (codigosSku.Count == 0)
        {
            throw new InvalidOperationException("Debes enviar al menos un codigo SKU Bimbo.");
        }

        var skusValidos = await dbContext.Skus
            .AsNoTracking()
            .Where(x => x.CodigoSkuBimbo != null && codigosSku.Contains(x.CodigoSkuBimbo))
            .Select(x => x.CodigoSkuBimbo!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var skusInvalidos = codigosSku
            .Except(skusValidos, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (skusInvalidos.Length > 0)
        {
            throw new InvalidOperationException(
                $"No se encontraron estos SKU Bimbo en la maestra: {string.Join(", ", skusInvalidos)}.");
        }

        var programaciones = await dbContext.Programaciones
            .Where(x => x.CampaniaId == campaniaId)
            .ToListAsync(cancellationToken);

        var programacionesObjetivo = programaciones
            .Where(x => ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Programada))
            .ToArray();

        if (programacionesObjetivo.Length == 0)
        {
            throw new InvalidOperationException(
                "La campania no tiene programaciones en estado PROGRAMADA para agregar SKU.");
        }

        var advertencias = new List<string>();
        var ejecutadas = programaciones.Count(x => ProgramacionEstados.EsEstado(x.Estado, ProgramacionEstados.Ejecutada));
        if (ejecutadas > 0)
        {
            advertencias.Add("Las programaciones ejecutadas no se modificaron.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = timeProvider.GetLocalNow().DateTime;
        var detallesCreados = await CreateMissingDetallesAsync(programacionesObjetivo, skusValidos, now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CampaniaOperacionResultadoDto
        {
            Mensaje = "SKU asociados a la campania correctamente.",
            Procesados = codigosSku.Count,
            DetallesCreados = detallesCreados,
            Advertencias = advertencias
        };
    }

    public async Task<CampaniaOperacionResultadoDto> RemoveSkuAsync(
        int campaniaId,
        string codigoSkuBimbo,
        CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var codigo = NormalizeRequiredValue(codigoSkuBimbo, "Debes indicar el codigo SKU Bimbo a retirar.");
        var programaciones = await dbContext.Programaciones
            .Where(x => x.CampaniaId == campaniaId)
            .ToListAsync(cancellationToken);

        var programacionIds = programaciones.Select(x => x.ProgramacionId).ToArray();
        var detalles = await dbContext.DetalleProgramaciones
            .Where(x => programacionIds.Contains(x.ProgramacionId) && x.CodigoSkuBimbo == codigo)
            .ToListAsync(cancellationToken);

        if (detalles.Count == 0)
        {
            throw new KeyNotFoundException(
                $"No se encontro el SKU {codigo} en la campania {campaniaId}.");
        }

        var programacionPorId = programaciones.ToDictionary(x => x.ProgramacionId);
        var removibles = detalles
            .Where(x =>
            {
                programacionPorId.TryGetValue(x.ProgramacionId, out var programacion);
                return programacion is not null && !ProgramacionEstados.EsEstado(programacion.Estado, ProgramacionEstados.Ejecutada);
            })
            .ToArray();

        var omitidos = detalles.Count - removibles.Length;
        if (removibles.Length > 0)
        {
            dbContext.DetalleProgramaciones.RemoveRange(removibles);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var advertencias = omitidos > 0
            ? ["No se eliminaron detalles asociados a programaciones ejecutadas."]
            : Array.Empty<string>();

        return new CampaniaOperacionResultadoDto
        {
            Mensaje = "SKU retirado de la campania.",
            Procesados = detalles.Count,
            Eliminados = removibles.Length,
            Omitidos = omitidos,
            DetallesEliminados = removibles.Length,
            Advertencias = advertencias
        };
    }

    public async Task<IReadOnlyCollection<CampaniaProgramacionDto>> GetProgramacionesAsync(int campaniaId, CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var fechaActual = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var programaciones = await campaniaProgramacionRepository.GetProgramacionesByCampaniaAsync(campaniaId, cancellationToken);
        var detalles = await campaniaProgramacionRepository.GetDetallesByProgramacionIdsAsync(
            programaciones.Select(x => x.ProgramacionId).ToArray(),
            cancellationToken);

        var detalleCountByProgramacion = detalles
            .GroupBy(x => x.ProgramacionId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.CodigoSkuBimbo).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var tiendaKeys = programaciones
            .Where(x => !string.IsNullOrWhiteSpace(x.TiendaCadenaKey))
            .Select(x => x.TiendaCadenaKey!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var tiendas = await dbContext.Tiendas
            .AsNoTracking()
            .Where(x => tiendaKeys.Contains(x.TiendaCadenaKey))
            .ToDictionaryAsync(x => x.TiendaCadenaKey, cancellationToken);

        return programaciones
            .Select(programacion =>
            {
                var fecha = programacion.Fecha.HasValue
                    ? DateOnly.FromDateTime(programacion.Fecha.Value)
                    : (DateOnly?)null;

                tiendas.TryGetValue(programacion.TiendaCadenaKey ?? string.Empty, out var tienda);

                return new CampaniaProgramacionDto
                {
                    ProgramacionId = programacion.ProgramacionId,
                    CampaniaId = programacion.CampaniaId,
                    TiendaCadenaKey = programacion.TiendaCadenaKey ?? string.Empty,
                    NombreTienda = tienda?.NombreTienda,
                    NombreTiendaBimbo = tienda?.NombreTiendaBimbo,
                    Cadena = tienda?.Cadena,
                    Fecha = fecha,
                    EstadoPersistido = ProgramacionEstados.Normalizar(programacion.Estado) ?? string.Empty,
                    EstadoFuncional = ProgramacionEstadoHelper.Calcular(programacion.Estado, fecha, fechaActual),
                    FuenteProgramacion = programacion.FuenteProgramacion,
                    CantidadSkus = detalleCountByProgramacion.GetValueOrDefault(programacion.ProgramacionId),
                    FechaCreacion = programacion.FechaCreacion,
                    FechaActualizacion = programacion.FechaActualizacion
                };
            })
            .ToArray();
    }

    public async Task<IReadOnlyCollection<CampaniaProgramacionDetalleDto>> GetDetallesAsync(
        int campaniaId,
        long programacionId,
        CancellationToken cancellationToken)
    {
        await GetCampaniaOrThrowAsync(campaniaId, cancellationToken);

        var programacion = await campaniaProgramacionRepository.GetProgramacionByCampaniaAsync(
            campaniaId,
            programacionId,
            cancellationToken);

        if (programacion is null)
        {
            throw new KeyNotFoundException($"No se encontro la programacion {programacionId} para la campania {campaniaId}.");
        }

        var detalles = await campaniaProgramacionRepository.GetDetallesByProgramacionIdsAsync([programacionId], cancellationToken);
        var skuCodes = detalles
            .Where(x => !string.IsNullOrWhiteSpace(x.CodigoSkuBimbo))
            .Select(x => x.CodigoSkuBimbo)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var skuMaestro = await BuildSkuMaestroLookupAsync(skuCodes, cancellationToken);

        return detalles
            .Select(detalle =>
            {
                skuMaestro.TryGetValue(detalle.CodigoSkuBimbo, out var sku);

                return new CampaniaProgramacionDetalleDto
                {
                    DetalleProgramacionId = detalle.DetalleProgramacionId,
                    ProgramacionId = detalle.ProgramacionId,
                    CodigoSkuBimbo = detalle.CodigoSkuBimbo,
                    NombreSkuBimbo = sku?.NombreSkuBimbo,
                    CodigoSkuB2B = sku?.CodigoSkuB2B,
                    Marca = sku?.Marca,
                    Categoria = sku?.Categoria,
                    FechaCreacion = detalle.FechaCreacion
                };
            })
            .ToArray();
    }

    private async Task<Campania> GetCampaniaOrThrowAsync(int campaniaId, CancellationToken cancellationToken) =>
        await campaniaRepository.GetByIdAsync(campaniaId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la campania con id {campaniaId}.");

    private async Task<IReadOnlyCollection<string>> GetCampaignSkuCodesAsync(int campaniaId, CancellationToken cancellationToken)
    {
        var programacionIds = await dbContext.Programaciones
            .AsNoTracking()
            .Where(x => x.CampaniaId == campaniaId && x.Estado != ProgramacionEstados.Cancelada)
            .Select(x => x.ProgramacionId)
            .ToListAsync(cancellationToken);

        if (programacionIds.Count == 0)
        {
            return [];
        }

        var codigos = await dbContext.DetalleProgramaciones
            .AsNoTracking()
            .Where(x => programacionIds.Contains(x.ProgramacionId))
            .Select(x => x.CodigoSkuBimbo)
            .ToListAsync(cancellationToken);

        return codigos
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<int> CreateMissingDetallesAsync(
        IReadOnlyCollection<Programacion> programaciones,
        IReadOnlyCollection<string> codigosSkuBimbo,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (programaciones.Count == 0 || codigosSkuBimbo.Count == 0)
        {
            return 0;
        }

        var programacionIds = programaciones
            .Where(x => x.ProgramacionId > 0)
            .Select(x => x.ProgramacionId)
            .Distinct()
            .ToArray();

        if (programacionIds.Length == 0)
        {
            return 0;
        }

        var detallesExistentes = await dbContext.DetalleProgramaciones
            .Where(x => programacionIds.Contains(x.ProgramacionId) && codigosSkuBimbo.Contains(x.CodigoSkuBimbo))
            .Select(x => new { x.ProgramacionId, x.CodigoSkuBimbo })
            .ToListAsync(cancellationToken);

        var keysExistentes = detallesExistentes
            .Select(x => BuildDetalleKey(x.ProgramacionId, x.CodigoSkuBimbo))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var nuevosDetalles = new List<DetalleProgramacion>();

        foreach (var programacion in programaciones)
        {
            foreach (var codigoSku in codigosSkuBimbo)
            {
                var key = BuildDetalleKey(programacion.ProgramacionId, codigoSku);
                if (keysExistentes.Contains(key))
                {
                    continue;
                }

                nuevosDetalles.Add(new DetalleProgramacion
                {
                    ProgramacionId = programacion.ProgramacionId,
                    CodigoSkuBimbo = codigoSku,
                    FechaCreacion = now
                });

                keysExistentes.Add(key);
            }
        }

        if (nuevosDetalles.Count > 0)
        {
            await dbContext.DetalleProgramaciones.AddRangeAsync(nuevosDetalles, cancellationToken);
        }

        return nuevosDetalles.Count;
    }

    private async Task<Dictionary<string, SkuMasterInfo>> BuildSkuMaestroLookupAsync(
        IReadOnlyCollection<string> skuCodes,
        CancellationToken cancellationToken)
    {
        if (skuCodes.Count == 0)
        {
            return [];
        }

        var skuRows = await dbContext.Skus
            .AsNoTracking()
            .Where(x => x.CodigoSkuBimbo != null && skuCodes.Contains(x.CodigoSkuBimbo))
            .OrderBy(x => x.SkuKey)
            .ToListAsync(cancellationToken);

        return skuRows
            .GroupBy(x => x.CodigoSkuBimbo!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => new SkuMasterInfo
                {
                    CodigoSkuB2B = x.Select(y => y.CodigoSkuB2B).FirstOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                    NombreSkuBimbo = x.Select(y => y.NombreSkuBimbo).FirstOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                    NombreSkuB2B = x.Select(y => y.NombreSkuB2B).FirstOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                    Marca = x.Select(y => y.Marca).FirstOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                    Categoria = x.Select(y => y.Categoria).FirstOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                    Area = x.Select(y => y.Area).FirstOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                    UnidadNegocio = x.Select(y => y.UnidadNegocio).FirstOrDefault(y => !string.IsNullOrWhiteSpace(y))
                },
                StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeRequiredValue(string? value, string message)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        return normalized;
    }

    private static IReadOnlyCollection<string> NormalizeDistinctValues(IEnumerable<string>? values) =>
        values?
            .Select(x => x?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? [];

    private static IReadOnlyCollection<string> BuildReplicationWarnings(bool replicarSkusExistentes, int skuCount)
    {
        if (!replicarSkusExistentes)
        {
            return ["La operacion se guardo sin replicar los SKU existentes de la campania."];
        }

        return skuCount == 0
            ? ["La campania aun no tiene SKU activos, por eso no se generaron detalles de programacion."]
            : [];
    }

    private static void ValidateFechasWithinCampania(IEnumerable<DateOnly> fechas, Campania campania)
    {
        var fechaInicio = campania.FechaInicio.HasValue
            ? DateOnly.FromDateTime(campania.FechaInicio.Value)
            : (DateOnly?)null;
        var fechaFin = campania.FechaFin.HasValue
            ? DateOnly.FromDateTime(campania.FechaFin.Value)
            : (DateOnly?)null;

        var fueraDeRango = fechas
            .Where(fecha =>
                (fechaInicio.HasValue && fecha < fechaInicio.Value) ||
                (fechaFin.HasValue && fecha > fechaFin.Value))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        if (fueraDeRango.Length > 0)
        {
            throw new InvalidOperationException(
                $"Estas fechas estan fuera del rango de la campania: {string.Join(", ", fueraDeRango.Select(x => x.ToString("yyyy-MM-dd")))}.");
        }
    }

    private static string BuildProgramacionKey(string tiendaCadenaKey, DateOnly fecha) =>
        $"{tiendaCadenaKey.Trim().ToUpperInvariant()}|{fecha:yyyy-MM-dd}";

    private static string BuildDetalleKey(long programacionId, string codigoSkuBimbo) =>
        $"{programacionId}|{codigoSkuBimbo.Trim().ToUpperInvariant()}";

    private sealed class SkuMasterInfo
    {
        public string? CodigoSkuB2B { get; init; }
        public string? NombreSkuBimbo { get; init; }
        public string? NombreSkuB2B { get; init; }
        public string? Marca { get; init; }
        public string? Categoria { get; init; }
        public string? Area { get; init; }
        public string? UnidadNegocio { get; init; }
    }
}

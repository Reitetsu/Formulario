using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Models.Entities;

namespace Sysbimbo.Api.Data;

/// <summary>
/// Completa los catalogos indispensables sin restablecer decisiones tomadas
/// posteriormente desde el panel de administracion.
/// </summary>
public sealed class FormularioDbSeeder(
    FormularioDbContext dbContext,
    ILogger<FormularioDbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        var cliente = await SeedClienteAsync(cancellationToken);
        var formulario = await SeedFormularioAsync(cliente, cancellationToken);
        await SeedOptionsAsync(formulario, cancellationToken);
        await SeedClientStoresAsync(cliente, cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var existingNames = (await dbContext.Roles
                .AsNoTracking()
                .Select(role => role.NormalizedName)
                .ToArrayAsync(cancellationToken))
            .Where(name => name is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingRoles = FormularioSeedCatalog.CreateRoles()
            .Where(role => !existingNames.Contains(role.NormalizedName))
            .ToArray();

        if (missingRoles.Length == 0)
        {
            return;
        }

        await dbContext.Roles.AddRangeAsync(missingRoles, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Roles iniciales agregados: {Count}.", missingRoles.Length);
    }

    private async Task<Cliente> SeedClienteAsync(CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes
            .SingleOrDefaultAsync(
                item => item.Codigo == FormularioSeedCatalog.BimboCodigo,
                cancellationToken);

        if (cliente is not null)
        {
            return cliente;
        }

        cliente = FormularioSeedCatalog.CreateBimbo();
        cliente.ClienteId = 0;
        cliente.FechaCreacion = DateTime.UtcNow;
        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Cliente inicial BIMBO agregado.");
        return cliente;
    }

    private async Task<Formulario> SeedFormularioAsync(
        Cliente cliente,
        CancellationToken cancellationToken)
    {
        var formulario = await dbContext.Formularios
            .SingleOrDefaultAsync(
                item => item.ClienteId == cliente.ClienteId &&
                        item.Codigo == FormularioSeedCatalog.ControlMaterialCodigo,
                cancellationToken);

        if (formulario is not null)
        {
            return formulario;
        }

        formulario = FormularioSeedCatalog.CreateControlMaterial();
        formulario.FormularioId = 0;
        formulario.ClienteId = cliente.ClienteId;
        formulario.FechaCreacion = DateTime.UtcNow;
        dbContext.Formularios.Add(formulario);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Formulario inicial de control de material agregado.");
        return formulario;
    }

    private async Task SeedOptionsAsync(
        Formulario formulario,
        CancellationToken cancellationToken)
    {
        var existingKeys = (await dbContext.FormularioOpciones
                .AsNoTracking()
                .Where(option => option.FormularioId == formulario.FormularioId)
                .Select(option => option.Clave)
                .ToArrayAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingOptions = FormularioSeedCatalog.CreateOptions()
            .Where(option => !existingKeys.Contains(option.Clave))
            .Select(option =>
            {
                option.FormularioOpcionId = 0;
                option.FormularioId = formulario.FormularioId;
                return option;
            })
            .ToArray();

        if (missingOptions.Length == 0)
        {
            return;
        }

        await dbContext.FormularioOpciones.AddRangeAsync(missingOptions, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Opciones iniciales de formulario agregadas: {Count}.", missingOptions.Length);
    }

    private async Task SeedClientStoresAsync(
        Cliente cliente,
        CancellationToken cancellationToken)
    {
        var storeKeys = await dbContext.Tiendas
            .AsNoTracking()
            .Select(store => store.TiendaCadenaKey)
            .ToArrayAsync(cancellationToken);

        if (storeKeys.Length == 0)
        {
            return;
        }

        var assignedKeys = (await dbContext.ClientesTiendas
                .AsNoTracking()
                .Where(assignment => assignment.ClienteId == cliente.ClienteId)
                .Select(assignment => assignment.TiendaCadenaKey)
                .ToArrayAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        var assignmentDate = DateTime.UtcNow;
        var missingAssignments = storeKeys
            .Where(storeKey => !assignedKeys.Contains(storeKey))
            .Select(storeKey => new ClienteTienda
            {
                ClienteId = cliente.ClienteId,
                TiendaCadenaKey = storeKey,
                Activo = true,
                FechaAsignacion = assignmentDate
            })
            .ToArray();

        if (missingAssignments.Length == 0)
        {
            return;
        }

        await dbContext.ClientesTiendas.AddRangeAsync(missingAssignments, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Tiendas existentes asociadas al cliente BIMBO: {Count}.",
            missingAssignments.Length);
    }
}

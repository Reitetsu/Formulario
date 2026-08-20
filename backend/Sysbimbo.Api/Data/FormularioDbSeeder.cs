using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Identity;

namespace Sysbimbo.Api.Data;

/// <summary>
/// Completa los catalogos indispensables sin restablecer decisiones tomadas
/// posteriormente desde el panel de administracion.
/// </summary>
public sealed class FormularioDbSeeder(
    FormularioDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<FormularioDbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        var cliente = await SeedClienteAsync(cancellationToken);
        var formulario = await SeedFormularioAsync(cliente, cancellationToken);
        await SeedOptionsAsync(formulario, cancellationToken);
        await SeedClientStoresAsync(cliente, cancellationToken);
        await SeedAdministratorAsync(cliente, formulario, cancellationToken);
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
            var expectedRoute = FormularioSeedCatalog.CurrentControlMaterialRoute;
            if (!string.Equals(formulario.Ruta, expectedRoute, StringComparison.Ordinal))
            {
                formulario.Ruta = expectedRoute;
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Ruta del formulario de control de material actualizada.");
            }

            return formulario;
        }

        formulario = FormularioSeedCatalog.CreateControlMaterial();
        formulario.FormularioId = 0;
        formulario.ClienteId = cliente.ClienteId;
        formulario.Ruta = FormularioSeedCatalog.CurrentControlMaterialRoute;
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

    private async Task SeedAdministratorAsync(
        Cliente cliente,
        Formulario formulario,
        CancellationToken cancellationToken)
    {
        const string userName = "admin";
        const string roleName = "Administrador";

        var administrator = await userManager.FindByNameAsync(userName);
        if (administrator is null)
        {
            var password = configuration["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "SeedAdmin:Password es obligatorio para crear el usuario administrador inicial.");
            }

            administrator = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                NombreCompleto = "Administrador",
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            EnsureSucceeded(
                await userManager.CreateAsync(administrator, password),
                "crear el usuario administrador inicial");
            logger.LogInformation("Usuario administrador inicial agregado.");
        }

        if (!await userManager.IsInRoleAsync(administrator, roleName))
        {
            EnsureSucceeded(
                await userManager.AddToRoleAsync(administrator, roleName),
                "asignar el rol Administrador al usuario inicial");
        }

        var roleId = await dbContext.Roles
            .Where(role => role.NormalizedName == roleName.ToUpperInvariant())
            .Select(role => role.Id)
            .SingleAsync(cancellationToken);
        var now = DateTime.UtcNow;

        if (!await dbContext.UsuariosClientes.AnyAsync(
                item => item.UsuarioId == administrator.Id && item.ClienteId == cliente.ClienteId,
                cancellationToken))
        {
            dbContext.UsuariosClientes.Add(new UsuarioCliente
            {
                UsuarioId = administrator.Id,
                ClienteId = cliente.ClienteId,
                Activo = true,
                FechaAsignacion = now
            });
        }

        if (!await dbContext.UsuariosClientesRoles.AnyAsync(
                item => item.UsuarioId == administrator.Id &&
                        item.ClienteId == cliente.ClienteId &&
                        item.RolId == roleId,
                cancellationToken))
        {
            dbContext.UsuariosClientesRoles.Add(new UsuarioClienteRol
            {
                UsuarioId = administrator.Id,
                ClienteId = cliente.ClienteId,
                RolId = roleId,
                FechaAsignacion = now
            });
        }

        if (!await dbContext.UsuariosFormularios.AnyAsync(
                item => item.UsuarioId == administrator.Id &&
                        item.FormularioId == formulario.FormularioId,
                cancellationToken))
        {
            dbContext.UsuariosFormularios.Add(new UsuarioFormulario
            {
                UsuarioId = administrator.Id,
                FormularioId = formulario.FormularioId,
                Activo = true,
                FechaAsignacion = now
            });
        }

        var hasStoreAssignments = await dbContext.UsuariosTiendas
            .AnyAsync(item => item.UsuarioId == administrator.Id, cancellationToken);
        if (!hasStoreAssignments)
        {
            var tottusStoreKeys = await dbContext.Tiendas
                .AsNoTracking()
                .Where(store => store.Formato != null && store.Formato.ToUpper() == "TOTTUS")
                .Select(store => store.TiendaCadenaKey)
                .ToArrayAsync(cancellationToken);
            var assignmentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            dbContext.UsuariosTiendas.AddRange(tottusStoreKeys.Select(storeKey => new UsuarioTienda
            {
                UsuarioId = administrator.Id,
                ClienteId = cliente.ClienteId,
                TiendaCadenaKey = storeKey,
                TipoAsignacion = "SUPERVISOR",
                FechaInicio = assignmentDate,
                Activo = true
            }));

            if (tottusStoreKeys.Length > 0)
            {
                logger.LogInformation(
                    "Tiendas TOTTUS asignadas inicialmente al administrador: {Count}.",
                    tottusStoreKeys.Length);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"No fue posible {operation}: {errors}");
    }
}

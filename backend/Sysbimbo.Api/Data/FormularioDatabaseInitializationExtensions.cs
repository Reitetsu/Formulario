using Microsoft.EntityFrameworkCore;

namespace Sysbimbo.Api.Data;

public static class FormularioDatabaseInitializationExtensions
{
    public static async Task InitializeFormularioDatabaseAsync(
        this WebApplication application,
        CancellationToken cancellationToken = default)
    {
        await using var scope = application.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<FormularioDbContext>();
        var seeder = services.GetRequiredService<FormularioDbSeeder>();

        application.Logger.LogInformation(
            "Aplicando migraciones y datos iniciales de FormularioDbContext...");

        await dbContext.Database.MigrateAsync(cancellationToken);
        await seeder.SeedAsync(cancellationToken);

        application.Logger.LogInformation(
            "FormularioDbContext actualizado correctamente.");
    }
}

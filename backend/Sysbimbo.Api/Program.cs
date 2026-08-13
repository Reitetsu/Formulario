using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.Middleware;
using Sysbimbo.Api.Repositories;
using Sysbimbo.Api.Repositories.Interfaces;
using Sysbimbo.Api.Services;
using Sysbimbo.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SysbimboDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LegacySqlServer")));

builder.Services.AddDbContext<FormularioDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("FormularioPostgres"),
        npgsql => npgsql.EnableRetryOnFailure(3)));

builder.Services.AddScoped<ITiendaRepository, TiendaRepository>();
builder.Services.AddScoped<ISkuRepository, SkuRepository>();
builder.Services.AddScoped<ICampaniaRepository, CampaniaRepository>();
builder.Services.AddScoped<ICampaniaProgramacionRepository, CampaniaProgramacionRepository>();
builder.Services.AddScoped<ICuotaRepository, CuotaRepository>();
builder.Services.AddScoped<IProgramacionRepository, ProgramacionRepository>();
builder.Services.AddScoped<ITiendaService, TiendaService>();
builder.Services.AddScoped<ISkuService, SkuService>();
builder.Services.AddScoped<ICampaniaService, CampaniaService>();
builder.Services.AddScoped<ICampaniaProgramacionService, CampaniaProgramacionService>();
builder.Services.AddScoped<ICuotaService, CuotaService>();
builder.Services.AddScoped<IProgramacionService, ProgramacionService>();
builder.Services.AddScoped<IMaterialImpulsoService, MaterialImpulsoService>();
builder.Services.AddScoped<FormularioDbSeeder>();
builder.Services.AddScoped<FormularioDataMigrationService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (args.Any(argument => argument.Equals("--migrate-form-data", StringComparison.OrdinalIgnoreCase)))
{
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<FormularioDataMigrationService>();
        await migrationService.MigrateAsync(CancellationToken.None);
    }
    catch (Exception exception)
    {
        app.Logger.LogCritical(
            exception,
            "No fue posible completar la migracion del formulario. No se modifico SQL Server.");
        Environment.ExitCode = 1;
    }

    return;
}

await app.InitializeFormularioDatabaseAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AngularClient");
app.UseAuthorization();
app.MapControllers();

app.Run();

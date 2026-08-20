using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.Middleware;
using Sysbimbo.Api.Models.Identity;
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

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<FormularioDbContext>();

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = "sysbimbo.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = false;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

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
            .WithOrigins(
                "http://localhost:4200",
                "https://innovamsp.lat",
                "https://www.innovamsp.lat")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

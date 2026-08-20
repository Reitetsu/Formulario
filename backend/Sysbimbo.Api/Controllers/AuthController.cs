using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.DTOs.Auth;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Identity;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    FormularioDbContext dbContext,
    TimeProvider timeProvider) : ControllerBase
{
    private const string SessionExpirationClaim = "session_expires_utc";

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticatedUserDto>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var userName = request.Usuario?.Trim();
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Ingresa el usuario y la contraseña." });
        }

        var user = await userManager.FindByNameAsync(userName);
        if (user is null || !user.Activo || await userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
        }

        await userManager.ResetAccessFailedCountAsync(user);
        await OpenAttendanceAsync(user.Id, cancellationToken);
        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        var expiration = GetNextLimaMidnight();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? userName),
            new("nombre_completo", user.NombreCompleto),
            new(SessionExpirationClaim, expiration.ToString("O", CultureInfo.InvariantCulture))
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                AllowRefresh = false,
                IsPersistent = true,
                ExpiresUtc = expiration
            });

        return Ok(ToDto(user, roles, expiration));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthenticatedUserDto>> MeAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || !user.Activo)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Unauthorized(new { message = "La sesión ya no es válida." });
        }

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        var expirationClaim = User.FindFirstValue(SessionExpirationClaim);
        var expiration = DateTimeOffset.TryParse(
            expirationClaim,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var parsedExpiration)
            ? parsedExpiration
            : GetNextLimaMidnight();

        return Ok(ToDto(user, roles, expiration));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await CloseAttendanceAsync(userId, "MANUAL", cancellationToken);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return NoContent();
    }

    private async Task OpenAttendanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var businessDate = GetCurrentLimaDate();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var previousOpenAttendances = await dbContext.JornadasUsuarios
            .Where(item => item.UsuarioId == userId &&
                           item.FormularioId == FormularioSeedCatalog.ControlMaterialFormularioId &&
                           item.FechaJornada < businessDate &&
                           item.Estado == "ABIERTA")
            .ToArrayAsync(cancellationToken);

        foreach (var previous in previousOpenAttendances)
        {
            previous.HoraSalida = GetLimaMidnightUtc(previous.FechaJornada.AddDays(1));
            previous.Estado = "CERRADA";
            previous.TipoCierre = "AUTOMATICO";
        }

        var attendance = await dbContext.JornadasUsuarios.SingleOrDefaultAsync(
            item => item.UsuarioId == userId &&
                    item.FormularioId == FormularioSeedCatalog.ControlMaterialFormularioId &&
                    item.FechaJornada == businessDate,
            cancellationToken);
        if (attendance is null)
        {
            var assignedStoreKeys = await dbContext.UsuariosTiendas
                .AsNoTracking()
                .Where(item => item.UsuarioId == userId &&
                               item.Activo &&
                               item.FechaInicio <= businessDate &&
                               (item.FechaFin == null || item.FechaFin >= businessDate))
                .Select(item => item.TiendaCadenaKey)
                .Take(2)
                .ToArrayAsync(cancellationToken);
            var userAgent = Request.Headers.UserAgent.ToString();

            attendance = new JornadaUsuario
            {
                UsuarioId = userId,
                ClienteId = FormularioSeedCatalog.BimboClienteId,
                FormularioId = FormularioSeedCatalog.ControlMaterialFormularioId,
                TiendaCadenaKey = assignedStoreKeys.Length == 1 ? assignedStoreKeys[0] : null,
                FechaJornada = businessDate,
                HoraIngreso = now,
                Estado = "ABIERTA",
                DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Dispositivo = userAgent.Length <= 500 ? userAgent : userAgent[..500]
            };
            dbContext.JornadasUsuarios.Add(attendance);
        }
        else if (attendance.Estado != "ABIERTA")
        {
            attendance.HoraSalida = null;
            attendance.Estado = "ABIERTA";
            attendance.TipoCierre = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CloseAttendanceAsync(
        Guid userId,
        string closeType,
        CancellationToken cancellationToken)
    {
        var businessDate = GetCurrentLimaDate();
        var attendance = await dbContext.JornadasUsuarios.SingleOrDefaultAsync(
            item => item.UsuarioId == userId &&
                    item.FormularioId == FormularioSeedCatalog.ControlMaterialFormularioId &&
                    item.FechaJornada == businessDate,
            cancellationToken);
        if (attendance is null)
        {
            return;
        }

        attendance.HoraSalida = timeProvider.GetUtcNow().UtcDateTime;
        attendance.Estado = "CERRADA";
        attendance.TipoCierre = closeType;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private DateTimeOffset GetNextLimaMidnight()
    {
        var timeZone = GetLimaTimeZone();
        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone);
        var nextLocalMidnight = new DateTime(
            localNow.Year,
            localNow.Month,
            localNow.Day,
            0,
            0,
            0,
            DateTimeKind.Unspecified).AddDays(1);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(nextLocalMidnight, timeZone));
    }

    private DateOnly GetCurrentLimaDate()
    {
        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), GetLimaTimeZone());
        return DateOnly.FromDateTime(localNow.DateTime);
    }

    private static DateTime GetLimaMidnightUtc(DateOnly date)
    {
        var localMidnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localMidnight, GetLimaTimeZone());
    }

    private static TimeZoneInfo GetLimaTimeZone()
    {
        var identifiers = OperatingSystem.IsWindows()
            ? new[] { "SA Pacific Standard Time", "America/Lima" }
            : new[] { "America/Lima", "SA Pacific Standard Time" };

        foreach (var identifier in identifiers)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(identifier);
            }
            catch (TimeZoneNotFoundException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }

    private static AuthenticatedUserDto ToDto(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        DateTimeOffset expiration) =>
        new(
            user.Id,
            user.UserName ?? string.Empty,
            user.NombreCompleto,
            roles,
            expiration);
}

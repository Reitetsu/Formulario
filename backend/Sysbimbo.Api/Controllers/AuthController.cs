using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.Auth;
using Sysbimbo.Api.Models.Identity;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    TimeProvider timeProvider) : ControllerBase
{
    private const string SessionExpirationClaim = "session_expires_utc";

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticatedUserDto>> LoginAsync(
        [FromBody] LoginRequest request)
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
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return NoContent();
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

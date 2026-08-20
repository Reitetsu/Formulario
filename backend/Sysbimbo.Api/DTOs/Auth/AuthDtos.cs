namespace Sysbimbo.Api.DTOs.Auth;

public sealed record LoginRequest(string Usuario, string Password);

public sealed record AuthenticatedUserDto(
    Guid UsuarioId,
    string NombreUsuario,
    string NombreCompleto,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset ExpiraEn);

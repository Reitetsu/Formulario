namespace Sysbimbo.Api.Constants;

public static class ProgramacionEstados
{
    public const string Programada = "PROGRAMADA";
    public const string Ejecutada = "EJECUTADA";
    public const string Cancelada = "CANCELADA";

    public static readonly IReadOnlyCollection<string> Todos =
    [
        Programada,
        Ejecutada,
        Cancelada
    ];

    public static string? Normalizar(string? estado)
    {
        var trimmed = estado?.Trim();
        return string.IsNullOrWhiteSpace(trimmed)
            ? null
            : trimmed.ToUpperInvariant();
    }

    public static bool EsValido(string? estado) =>
        Todos.Contains(Normalizar(estado) ?? string.Empty, StringComparer.OrdinalIgnoreCase);

    public static bool EsEstado(string? estado, string esperado) =>
        string.Equals(Normalizar(estado), esperado, StringComparison.OrdinalIgnoreCase);
}

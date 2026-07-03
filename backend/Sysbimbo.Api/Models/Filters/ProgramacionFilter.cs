namespace Sysbimbo.Api.Models.Filters;

public class ProgramacionFilter
{
    public string? NombreCampania { get; init; }
    public string? NombreTiendaBimbo { get; init; }
    public DateTime? Fecha { get; init; }
    public decimal? Cuota { get; init; }
    public string? Estado { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

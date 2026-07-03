namespace Sysbimbo.Api.Models.Filters;

public class CuotaFilter
{
    public string? Campania { get; init; }
    public string? TiendaCadenaKey { get; init; }
    public DateTime? Fecha { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

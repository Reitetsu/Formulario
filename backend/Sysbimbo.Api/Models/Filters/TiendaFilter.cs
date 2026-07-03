namespace Sysbimbo.Api.Models.Filters;

public class TiendaFilter
{
    public string? Cadena { get; init; }
    public string? Region { get; init; }
    public string? Nombre { get; init; }
    public string? CodigoTiendaB2B { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

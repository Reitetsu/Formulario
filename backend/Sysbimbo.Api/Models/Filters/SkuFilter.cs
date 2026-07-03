namespace Sysbimbo.Api.Models.Filters;

public class SkuFilter
{
    public string? Categoria { get; init; }
    public string? Marca { get; init; }
    public string? Nombre { get; init; }
    public string? CodigoSkuB2B { get; init; }
    public string? CodigoSkuBimbo { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

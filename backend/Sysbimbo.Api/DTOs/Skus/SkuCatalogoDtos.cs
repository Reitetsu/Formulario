using Sysbimbo.Api.DTOs.Common;

namespace Sysbimbo.Api.DTOs.Skus;

public sealed class SkuCatalogoDto
{
    public string CodigoSkuBimbo { get; init; } = string.Empty;
    public string? CodigoSkuB2B { get; init; }
    public string? NombreSkuBimbo { get; init; }
    public string? NombreSkuB2B { get; init; }
    public string? UnidadNegocio { get; init; }
    public string? Area { get; init; }
    public string? Categoria { get; init; }
    public string? Marca { get; init; }
    public string? TipoProducto { get; init; }
    public string? Status { get; init; }
    public string? Gramaje { get; init; }
}

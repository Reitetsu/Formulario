using System.ComponentModel.DataAnnotations;

namespace Sysbimbo.Api.DTOs.Skus;

public class SkuDto
{
    public string SkuKey { get; init; } = string.Empty;
    public string? CodigoSkuB2B { get; init; }
    public string? NombreSkuB2B { get; init; }
    public string? CodigoSkuBimbo { get; init; }
    public string? NombreSkuBimbo { get; init; }
    public string? UnidadNegocio { get; init; }
    public string? Area { get; init; }
    public string? Categoria { get; init; }
    public string? Marca { get; init; }
    public string? TipoProducto { get; init; }
    public string? Status { get; init; }
    public string? Gramaje { get; init; }
    public DateTime? UltimaFecha { get; init; }
    public long? CantidadRegistros { get; init; }
    public string? FuenteSku { get; init; }
}

public class CreateSkuDto
{
    [Required]
    public string SkuKey { get; init; } = string.Empty;

    public string? CodigoSkuB2B { get; init; }
    public string? NombreSkuB2B { get; init; }
    public string? CodigoSkuBimbo { get; init; }
    public string? NombreSkuBimbo { get; init; }
    public string? UnidadNegocio { get; init; }
    public string? Area { get; init; }
    public string? Categoria { get; init; }
    public string? Marca { get; init; }
    public string? TipoProducto { get; init; }
    public string? Status { get; init; }
    public string? Gramaje { get; init; }
    public DateTime? UltimaFecha { get; init; }
    public long? CantidadRegistros { get; init; }
    public string? FuenteSku { get; init; }
}

public class UpdateSkuDto
{
    public string? CodigoSkuB2B { get; init; }
    public string? NombreSkuB2B { get; init; }
    public string? CodigoSkuBimbo { get; init; }
    public string? NombreSkuBimbo { get; init; }
    public string? UnidadNegocio { get; init; }
    public string? Area { get; init; }
    public string? Categoria { get; init; }
    public string? Marca { get; init; }
    public string? TipoProducto { get; init; }
    public string? Status { get; init; }
    public string? Gramaje { get; init; }
    public DateTime? UltimaFecha { get; init; }
    public long? CantidadRegistros { get; init; }
    public string? FuenteSku { get; init; }
}

public class SkuQueryDto
{
    public string? Categoria { get; init; }
    public string? Marca { get; init; }
    public string? Nombre { get; init; }
    public string? CodigoSkuB2B { get; init; }
    public string? CodigoSkuBimbo { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

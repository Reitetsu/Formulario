namespace Sysbimbo.Api.DTOs.Campanias;

public sealed class CampaniaSkuDto
{
    public string CodigoSkuBimbo { get; init; } = string.Empty;
    public string? CodigoSkuB2B { get; init; }
    public string? NombreSkuBimbo { get; init; }
    public string? NombreSkuB2B { get; init; }
    public string? Marca { get; init; }
    public string? Categoria { get; init; }
    public string? Area { get; init; }
    public string? UnidadNegocio { get; init; }
    public int CantidadProgramaciones { get; init; }
    public int CantidadTiendas { get; init; }
    public int CantidadFechas { get; init; }
}

namespace Sysbimbo.Api.DTOs.Campanias;

public sealed class CampaniaProgramacionDetalleDto
{
    public long DetalleProgramacionId { get; init; }
    public long ProgramacionId { get; init; }
    public string CodigoSkuBimbo { get; init; } = string.Empty;
    public string? NombreSkuBimbo { get; init; }
    public string? CodigoSkuB2B { get; init; }
    public string? Marca { get; init; }
    public string? Categoria { get; init; }
    public DateTime FechaCreacion { get; init; }
}

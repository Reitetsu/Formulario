namespace Sysbimbo.Api.DTOs.Campanias;

public sealed class CampaniaProgramacionDto
{
    public long ProgramacionId { get; init; }
    public int? CampaniaId { get; init; }
    public string TiendaCadenaKey { get; init; } = string.Empty;
    public string? NombreTienda { get; init; }
    public string? NombreTiendaBimbo { get; init; }
    public string? Cadena { get; init; }
    public DateOnly? Fecha { get; init; }
    public string EstadoPersistido { get; init; } = string.Empty;
    public string EstadoFuncional { get; init; } = string.Empty;
    public string? FuenteProgramacion { get; init; }
    public int CantidadSkus { get; init; }
    public DateTime? FechaCreacion { get; init; }
    public DateTime? FechaActualizacion { get; init; }
}

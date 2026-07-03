namespace Sysbimbo.Api.DTOs.Programaciones;

public class DetalleProgramacionDto
{
    public long DetalleProgramacionId { get; init; }
    public long ProgramacionId { get; init; }
    public string CodigoSkuBimbo { get; init; } = string.Empty;
    public string? NombreSkuBimbo { get; init; }
    public DateTime FechaCreacion { get; init; }
}

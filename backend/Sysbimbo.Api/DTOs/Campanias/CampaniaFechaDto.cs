namespace Sysbimbo.Api.DTOs.Campanias;

public sealed class CampaniaFechaDto
{
    public DateOnly Fecha { get; init; }
    public int CantidadTiendas { get; init; }
    public int CantidadProgramaciones { get; init; }
    public int CantidadSkus { get; init; }
    public int CantidadProgramadas { get; init; }
    public int CantidadEjecutadas { get; init; }
    public int CantidadCanceladas { get; init; }
}

namespace Sysbimbo.Api.DTOs.Campanias;

public sealed class CampaniaResumenDto
{
    public int CampaniaId { get; init; }
    public string NombreCampania { get; init; } = string.Empty;
    public int CantidadTiendas { get; init; }
    public int CantidadFechas { get; init; }
    public int CantidadSkus { get; init; }
    public int CantidadProgramacionesProgramadas { get; init; }
    public int CantidadProgramacionesEjecutadas { get; init; }
    public int CantidadProgramacionesCanceladas { get; init; }
    public int CantidadDetalles { get; init; }
}

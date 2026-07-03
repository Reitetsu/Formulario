namespace Sysbimbo.Api.DTOs.Campanias;

public sealed class CampaniaTiendaDto
{
    public string TiendaCadenaKey { get; init; } = string.Empty;
    public string? CodigoTiendaB2B { get; init; }
    public string? NombreTienda { get; init; }
    public string? NombreTiendaBimbo { get; init; }
    public string? Cadena { get; init; }
    public string? Formato { get; init; }
    public string? Region { get; init; }
    public int CantidadFechas { get; init; }
    public DateOnly? PrimeraFecha { get; init; }
    public DateOnly? UltimaFecha { get; init; }
    public int CantidadProgramadas { get; init; }
    public int CantidadEjecutadas { get; init; }
    public int CantidadCanceladas { get; init; }
}

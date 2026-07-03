namespace Sysbimbo.Api.Models.Filters;

public class CampaniaFilter
{
    public string? NombreCampania { get; init; }
    public string? Estado { get; init; }
    public string? Descripcion { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

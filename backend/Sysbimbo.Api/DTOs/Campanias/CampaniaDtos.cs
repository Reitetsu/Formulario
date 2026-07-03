using System.ComponentModel.DataAnnotations;

namespace Sysbimbo.Api.DTOs.Campanias;

public class CampaniaDto
{
    public int CampaniaId { get; init; }
    public string? NombreCampania { get; init; }
    public string? Descripcion { get; init; }
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
    public string? Estado { get; init; }
}

public class CreateCampaniaDto
{
    [Required]
    public string NombreCampania { get; init; } = string.Empty;

    public string? Descripcion { get; init; }
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
    public string? Estado { get; init; }
}

public class UpdateCampaniaDto
{
    [Required]
    public string NombreCampania { get; init; } = string.Empty;

    public string? Descripcion { get; init; }
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
    public string? Estado { get; init; }
}

public class CampaniaQueryDto
{
    public string? NombreCampania { get; init; }
    public string? Estado { get; init; }
    public string? Descripcion { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

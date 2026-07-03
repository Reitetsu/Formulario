using System.ComponentModel.DataAnnotations;

namespace Sysbimbo.Api.DTOs.Programaciones;

public class ProgramacionDto
{
    public long ProgramacionId { get; init; }
    public int? CampaniaId { get; init; }
    public string? NombreCampania { get; init; }
    public string? TiendaCadenaKey { get; init; }
    public string? NombreTiendaBimbo { get; init; }
    public DateTime? Fecha { get; init; }
    public decimal? Cuota { get; init; }
    public string? Estado { get; init; }
    public string? FuenteProgramacion { get; init; }
    public DateTime? FechaCreacion { get; init; }
    public DateTime? FechaActualizacion { get; init; }
}

public class CreateProgramacionDto
{
    public int? CampaniaId { get; init; }

    [Required]
    public string TiendaCadenaKey { get; init; } = string.Empty;

    [Required]
    public DateTime? Fecha { get; init; }

    public decimal? Cuota { get; init; }
    public string? Estado { get; init; }
    public string? FuenteProgramacion { get; init; }
    public DateTime? FechaCreacion { get; init; }
    public DateTime? FechaActualizacion { get; init; }
}

public class UpdateProgramacionDto
{
    public int? CampaniaId { get; init; }

    [Required]
    public string TiendaCadenaKey { get; init; } = string.Empty;

    [Required]
    public DateTime? Fecha { get; init; }

    public decimal? Cuota { get; init; }
    public string? Estado { get; init; }
    public string? FuenteProgramacion { get; init; }
    public DateTime? FechaCreacion { get; init; }
    public DateTime? FechaActualizacion { get; init; }
}

public class ProgramacionQueryDto
{
    public string? NombreCampania { get; init; }
    public string? NombreTiendaBimbo { get; init; }
    public DateTime? Fecha { get; init; }
    public decimal? Cuota { get; init; }
    public string? Estado { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

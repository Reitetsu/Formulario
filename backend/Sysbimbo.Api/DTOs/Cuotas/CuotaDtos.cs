using System.ComponentModel.DataAnnotations;

namespace Sysbimbo.Api.DTOs.Cuotas;

public class CuotaDto
{
    public long CuotaId { get; init; }
    public string? Campania { get; init; }
    public string? TiendaCadenaKey { get; init; }
    public DateTime? Fecha { get; init; }
    public decimal? Cuota { get; init; }
}

public class CreateCuotaDto
{
    [Required]
    public string Campania { get; init; } = string.Empty;

    [Required]
    public string TiendaCadenaKey { get; init; } = string.Empty;

    [Required]
    public DateTime? Fecha { get; init; }

    [Required]
    public decimal? Cuota { get; init; }
}

public class UpdateCuotaDto
{
    [Required]
    public string Campania { get; init; } = string.Empty;

    [Required]
    public string TiendaCadenaKey { get; init; } = string.Empty;

    [Required]
    public DateTime? Fecha { get; init; }

    [Required]
    public decimal? Cuota { get; init; }
}

public class CuotaQueryDto
{
    public string? Campania { get; init; }
    public string? TiendaCadenaKey { get; init; }
    public DateTime? Fecha { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

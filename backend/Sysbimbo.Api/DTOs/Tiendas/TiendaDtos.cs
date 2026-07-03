using System.ComponentModel.DataAnnotations;

namespace Sysbimbo.Api.DTOs.Tiendas;

public class TiendaDto
{
    public string TiendaCadenaKey { get; init; } = string.Empty;
    public string? CodigoTiendaB2BPrefijo { get; init; }
    public string? CodigoTiendaB2B { get; init; }
    public string? NombreTienda { get; init; }
    public string? NombreTiendaBimbo { get; init; }
    public string? Canal { get; init; }
    public string? Cadena { get; init; }
    public string? Formato { get; init; }
    public string? TipoLocal { get; init; }
    public string? LimaProvincias { get; init; }
    public string? Region { get; init; }
    public string? Provincia { get; init; }
    public string? Ruta { get; init; }
    public string? Supervisor { get; init; }
    public string? Gestor { get; init; }
    public string? Vendedor { get; init; }
    public DateTime? UltimaFecha { get; init; }
    public long? CantidadRegistros { get; init; }
    public string? FuenteTienda { get; init; }
}

public class CreateTiendaDto
{
    [Required]
    public string TiendaCadenaKey { get; init; } = string.Empty;

    public string? CodigoTiendaB2BPrefijo { get; init; }
    public string? CodigoTiendaB2B { get; init; }
    public string? NombreTienda { get; init; }
    public string? NombreTiendaBimbo { get; init; }
    public string? Canal { get; init; }
    public string? Cadena { get; init; }
    public string? Formato { get; init; }
    public string? TipoLocal { get; init; }
    public string? LimaProvincias { get; init; }
    public string? Region { get; init; }
    public string? Provincia { get; init; }
    public string? Ruta { get; init; }
    public string? Supervisor { get; init; }
    public string? Gestor { get; init; }
    public string? Vendedor { get; init; }
    public DateTime? UltimaFecha { get; init; }
    public long? CantidadRegistros { get; init; }
    public string? FuenteTienda { get; init; }
}

public class UpdateTiendaDto
{
    public string? CodigoTiendaB2BPrefijo { get; init; }
    public string? CodigoTiendaB2B { get; init; }
    public string? NombreTienda { get; init; }
    public string? NombreTiendaBimbo { get; init; }
    public string? Canal { get; init; }
    public string? Cadena { get; init; }
    public string? Formato { get; init; }
    public string? TipoLocal { get; init; }
    public string? LimaProvincias { get; init; }
    public string? Region { get; init; }
    public string? Provincia { get; init; }
    public string? Ruta { get; init; }
    public string? Supervisor { get; init; }
    public string? Gestor { get; init; }
    public string? Vendedor { get; init; }
    public DateTime? UltimaFecha { get; init; }
    public long? CantidadRegistros { get; init; }
    public string? FuenteTienda { get; init; }
}

public class TiendaQueryDto
{
    public string? Cadena { get; init; }
    public string? Region { get; init; }
    public string? Nombre { get; init; }
    public string? CodigoTiendaB2B { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

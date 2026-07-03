using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sysbimbo.Api.Models.Entities;

[Table("DimTiendaMaestra_Export")]
public class DimTiendaMaestraExport
{
    [Key]
    [Column("TiendaCadenaKey")]
    public string TiendaCadenaKey { get; set; } = string.Empty;

    [Column("C\u00F3digo Tienda B2B (prefijo)")]
    public string? CodigoTiendaB2BPrefijo { get; set; }

    [Column("C\u00F3digo Tienda B2B")]
    public string? CodigoTiendaB2B { get; set; }

    [Column("Nombre Tienda")]
    public string? NombreTienda { get; set; }

    [Column("Nombre Tienda Bimbo")]
    public string? NombreTiendaBimbo { get; set; }

    [Column("Canal")]
    public string? Canal { get; set; }

    [Column("Cadena")]
    public string? Cadena { get; set; }

    [Column("Formato")]
    public string? Formato { get; set; }

    [Column("Tipo Local")]
    public string? TipoLocal { get; set; }

    [Column("Lima | Provincias")]
    public string? LimaProvincias { get; set; }

    [Column("Regi\u00F3n")]
    public string? Region { get; set; }

    [Column("Provincia")]
    public string? Provincia { get; set; }

    [Column("Ruta")]
    public string? Ruta { get; set; }

    [Column("Supervisor")]
    public string? Supervisor { get; set; }

    [Column("Gestor")]
    public string? Gestor { get; set; }

    [Column("Vendedor")]
    public string? Vendedor { get; set; }

    [Column("UltimaFecha")]
    public DateTime? UltimaFecha { get; set; }

    [Column("CantidadRegistros")]
    public long? CantidadRegistros { get; set; }

    [Column("FuenteTienda")]
    public string? FuenteTienda { get; set; }
}

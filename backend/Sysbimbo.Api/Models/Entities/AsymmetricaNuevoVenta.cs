using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sysbimbo.Api.Models.Entities;

[Keyless]
[Table("ASYMMETRICA NUEVO")]
public class AsymmetricaNuevoVenta
{
    [Column("F-Fecha")]
    public DateTime? Fecha { get; set; }

    [Column("T-Nombre Tienda Bimbo")]
    public string? NombreTiendaBimbo { get; set; }

    [Column("T-C\u00F3digo Tienda B2B")]
    public string? CodigoTiendaB2B { get; set; }

    [Column("T-C\u00F3digo Tienda B2B (prefijo)")]
    public string? CodigoTiendaB2BPrefijo { get; set; }

    [Column("P-C\u00F3digo SKU Bimbo")]
    public string? CodigoSkuBimbo { get; set; }

    [Column("Venta (un)")]
    public decimal? VentaUnidades { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sysbimbo.Api.Models.Entities;

[Table("DimSkuMaestra_Export")]
public class DimSkuMaestraExport
{
    [Key]
    [Column("SkuKey")]
    public string SkuKey { get; set; } = string.Empty;

    [Column("C\u00F3digo SKU B2B")]
    public string? CodigoSkuB2B { get; set; }

    [Column("Nombre SKU B2B")]
    public string? NombreSkuB2B { get; set; }

    [Column("C\u00F3digo SKU Bimbo")]
    public string? CodigoSkuBimbo { get; set; }

    [Column("Nombre SKU Bimbo")]
    public string? NombreSkuBimbo { get; set; }

    [Column("Unidad de Negocio")]
    public string? UnidadNegocio { get; set; }

    [Column("\u00C1rea")]
    public string? Area { get; set; }

    [Column("Categor\u00EDa")]
    public string? Categoria { get; set; }

    [Column("Marca")]
    public string? Marca { get; set; }

    [Column("Tipo Producto")]
    public string? TipoProducto { get; set; }

    [Column("Status")]
    public string? Status { get; set; }

    [Column("Gramaje")]
    public string? Gramaje { get; set; }

    [Column("UltimaFecha")]
    public DateTime? UltimaFecha { get; set; }

    [Column("CantidadRegistros")]
    public long? CantidadRegistros { get; set; }

    [Column("FuenteSKU")]
    public string? FuenteSku { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sysbimbo.Api.Models.Entities;

[Table("FactCampaniaCuota")]
public class FactCampaniaCuota
{
    [Key]
    [Column("CuotaId")]
    public long CuotaId { get; set; }

    [Column("Campania")]
    public string? Campania { get; set; }

    [Column("TiendaCadenaKey")]
    public string? TiendaCadenaKey { get; set; }

    [Column("Fecha")]
    public DateTime? Fecha { get; set; }

    [Column("Cuota")]
    public decimal? Cuota { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sysbimbo.Api.Models.Entities;

[Table("Programacion")]
public class Programacion
{
    [Key]
    [Column("ProgramacionId")]
    public long ProgramacionId { get; set; }

    [Column("CampaniaId")]
    public int? CampaniaId { get; set; }

    [Column("TiendaCadenaKey")]
    public string? TiendaCadenaKey { get; set; }

    [Column("Fecha")]
    public DateTime? Fecha { get; set; }

    [Column("Estado")]
    public string? Estado { get; set; }

    [Column("FuenteProgramacion")]
    public string? FuenteProgramacion { get; set; }

    [Column("FechaCreacion")]
    public DateTime? FechaCreacion { get; set; }

    [Column("FechaActualizacion")]
    public DateTime? FechaActualizacion { get; set; }
}

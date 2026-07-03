using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sysbimbo.Api.Models.Entities;

[Table("Campania")]
public class Campania
{
    [Key]
    [Column("CampaniaId")]
    public int CampaniaId { get; set; }

    [Column("NombreCampania")]
    public string? NombreCampania { get; set; }

    [Column("Descripcion")]
    public string? Descripcion { get; set; }

    [Column("FechaInicio")]
    public DateTime? FechaInicio { get; set; }

    [Column("FechaFin")]
    public DateTime? FechaFin { get; set; }

    [Column("Estado")]
    public string? Estado { get; set; }
}

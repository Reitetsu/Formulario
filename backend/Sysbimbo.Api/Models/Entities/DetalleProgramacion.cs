using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sysbimbo.Api.Models.Entities;

[Table("DetalleProgramacion")]
public class DetalleProgramacion
{
    [Key]
    [Column("DetalleProgramacionId")]
    public long DetalleProgramacionId { get; set; }

    [Column("ProgramacionId")]
    public long ProgramacionId { get; set; }

    [Column("CodigoSkuBimbo")]
    public string CodigoSkuBimbo { get; set; } = string.Empty;

    [Column("FechaCreacion")]
    public DateTime FechaCreacion { get; set; }
}

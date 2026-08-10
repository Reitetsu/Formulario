using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sysbimbo.Api.Models.Entities;

[Table("MaterialImpulsoTienda")]
public class MaterialImpulsoTienda
{
    [Key]
    public long MaterialImpulsoTiendaId { get; set; }

    [MaxLength(450)]
    public string TiendaCadenaKey { get; set; } = string.Empty;

    [MaxLength(200)]
    public string NombreMaterial { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public int CuotaDiaria { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public ICollection<FotoMaterialImpulso> Fotos { get; set; } = [];
}

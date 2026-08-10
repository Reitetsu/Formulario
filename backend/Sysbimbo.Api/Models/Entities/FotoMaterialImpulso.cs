using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sysbimbo.Api.Models.Entities;

[Table("FotoMaterialImpulso")]
public class FotoMaterialImpulso
{
    [Key]
    public long FotoMaterialImpulsoId { get; set; }

    public long MaterialImpulsoTiendaId { get; set; }

    [MaxLength(450)]
    public string TiendaCadenaKey { get; set; } = string.Empty;

    [MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TipoContenido { get; set; } = string.Empty;

    public long TamanoBytes { get; set; }

    public byte[] Contenido { get; set; } = [];

    public DateTime FechaCaptura { get; set; }

    public MaterialImpulsoTienda MaterialImpulsoTienda { get; set; } = null!;
}

namespace Sysbimbo.Api.Models.Entities;

public class CanjeMaterialDiario
{
    public long CanjeMaterialDiarioId { get; set; }
    public long MaterialImpulsoTiendaId { get; set; }
    public string TiendaCadenaKey { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public int Cantidad { get; set; }
    public string FormaIngreso { get; set; } = "MANUAL";
    public Guid RegistradoPorUsuarioId { get; set; }
    public Guid? ActualizadoPorUsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public MaterialImpulsoTienda MaterialImpulsoTienda { get; set; } = null!;
}

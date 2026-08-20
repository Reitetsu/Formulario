using System.ComponentModel.DataAnnotations;

namespace Sysbimbo.Api.DTOs.Supervisores;

public sealed class SupervisorPanelDto
{
    public DateOnly Fecha { get; init; }
    public SupervisorAttendanceDto? Asistencia { get; init; }
    public IReadOnlyList<SupervisorStoreDto> Tiendas { get; init; } = [];
}

public sealed class SupervisorAttendanceDto
{
    public long JornadaUsuarioId { get; init; }
    public DateOnly Fecha { get; init; }
    public DateTime HoraIngreso { get; init; }
    public DateTime? HoraSalida { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string? TipoCierre { get; init; }
}

public sealed class SupervisorStoreDto
{
    public string TiendaCadenaKey { get; init; } = string.Empty;
    public string NombreTienda { get; init; } = string.Empty;
    public string? Formato { get; init; }
    public int TotalCanjesHoy { get; init; }
    public IReadOnlyList<SupervisorMaterialDto> Materiales { get; init; } = [];
}

public sealed class SupervisorMaterialDto
{
    public long MaterialImpulsoTiendaId { get; init; }
    public string NombreMaterial { get; init; } = string.Empty;
    public int CuotaDiaria { get; init; }
    public int CanjesHoy { get; init; }
    public int EvidenciasHoy { get; init; }
}

public sealed class UpdateSupervisorAttendanceDto
{
    [Required]
    public TimeOnly HoraIngreso { get; init; }
    public TimeOnly? HoraSalida { get; init; }
}

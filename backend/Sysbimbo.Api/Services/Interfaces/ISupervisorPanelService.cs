using Sysbimbo.Api.DTOs.Supervisores;

namespace Sysbimbo.Api.Services.Interfaces;

public interface ISupervisorPanelService
{
    Task<SupervisorPanelDto> GetAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task<SupervisorAttendanceDto> UpdateAttendanceAsync(
        Guid usuarioId,
        UpdateSupervisorAttendanceDto dto,
        CancellationToken cancellationToken);
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.Supervisores;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Supervisor")]
[Route("api/supervisores")]
public sealed class SupervisoresController(ISupervisorPanelService supervisorPanelService)
    : ControllerBase
{
    [HttpGet("panel")]
    public async Task<ActionResult<SupervisorPanelDto>> GetPanelAsync(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { message = "La sesion no contiene un usuario valido." });
        }

        return Ok(await supervisorPanelService.GetAsync(userId, cancellationToken));
    }

    [HttpPut("asistencia-hoy")]
    public async Task<ActionResult<SupervisorAttendanceDto>> UpdateAttendanceAsync(
        [FromBody] UpdateSupervisorAttendanceDto dto,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { message = "La sesion no contiene un usuario valido." });
        }

        return Ok(await supervisorPanelService.UpdateAttendanceAsync(
            userId,
            dto,
            cancellationToken));
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}

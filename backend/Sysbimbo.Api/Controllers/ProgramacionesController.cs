using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.Programaciones;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgramacionesController(IProgramacionService programacionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAllAsync([FromQuery] ProgramacionQueryDto query, CancellationToken cancellationToken)
    {
        var result = await programacionService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await programacionService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:long}/detalle")]
    public async Task<ActionResult> GetDetailAsync(long id, CancellationToken cancellationToken)
    {
        var result = await programacionService.GetDetailByProgramacionIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] CreateProgramacionDto dto, CancellationToken cancellationToken)
    {
        var result = await programacionService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.ProgramacionId }, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] UpdateProgramacionDto dto, CancellationToken cancellationToken)
    {
        var result = await programacionService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await programacionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

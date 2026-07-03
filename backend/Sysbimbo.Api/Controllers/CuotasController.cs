using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.Cuotas;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuotasController(ICuotaService cuotaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAllAsync([FromQuery] CuotaQueryDto query, CancellationToken cancellationToken)
    {
        var result = await cuotaService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await cuotaService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] CreateCuotaDto dto, CancellationToken cancellationToken)
    {
        var result = await cuotaService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.CuotaId }, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] UpdateCuotaDto dto, CancellationToken cancellationToken)
    {
        var result = await cuotaService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await cuotaService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

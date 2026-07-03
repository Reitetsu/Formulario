using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.Campanias;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaniasController(ICampaniaService campaniaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAllAsync([FromQuery] CampaniaQueryDto query, CancellationToken cancellationToken)
    {
        var result = await campaniaService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await campaniaService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] CreateCampaniaDto dto, CancellationToken cancellationToken)
    {
        var result = await campaniaService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.CampaniaId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] UpdateCampaniaDto dto, CancellationToken cancellationToken)
    {
        var result = await campaniaService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await campaniaService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

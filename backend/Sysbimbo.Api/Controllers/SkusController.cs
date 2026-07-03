using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.Skus;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkusController(ISkuService skuService) : ControllerBase
{
    [HttpGet("catalogo")]
    public async Task<ActionResult> GetCatalogoAsync([FromQuery] SkuQueryDto query, CancellationToken cancellationToken)
    {
        var result = await skuService.GetCatalogoAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllAsync([FromQuery] SkuQueryDto query, CancellationToken cancellationToken)
    {
        var result = await skuService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var result = await skuService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] CreateSkuDto dto, CancellationToken cancellationToken)
    {
        var result = await skuService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.SkuKey }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(string id, [FromBody] UpdateSkuDto dto, CancellationToken cancellationToken)
    {
        var result = await skuService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await skuService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

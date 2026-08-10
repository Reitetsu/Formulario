using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.Tiendas;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TiendasController(ITiendaService tiendaService) : ControllerBase
{
    private const string GetByIdRouteName = "GetTiendaById";

    [HttpGet]
    public async Task<ActionResult> GetAllAsync([FromQuery] TiendaQueryDto query, CancellationToken cancellationToken)
    {
        var result = await tiendaService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}", Name = GetByIdRouteName)]
    public async Task<ActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var result = await tiendaService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] CreateTiendaDto dto, CancellationToken cancellationToken)
    {
        var result = await tiendaService.CreateAsync(dto, cancellationToken);
        return CreatedAtRoute(GetByIdRouteName, new { id = result.TiendaCadenaKey }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(string id, [FromBody] UpdateTiendaDto dto, CancellationToken cancellationToken)
    {
        var result = await tiendaService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await tiendaService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

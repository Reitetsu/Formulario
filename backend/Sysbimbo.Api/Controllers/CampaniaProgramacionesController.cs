using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.Campanias;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/campanias/{campaniaId:int}")]
public class CampaniaProgramacionesController(ICampaniaProgramacionService campaniaProgramacionService) : ControllerBase
{
    [HttpPost("tiendas")]
    public async Task<ActionResult> AddTiendasAsync(
        int campaniaId,
        [FromBody] AddCampaniaTiendasRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.AddTiendasAsync(campaniaId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("resumen")]
    public async Task<ActionResult> GetResumenAsync(int campaniaId, CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.GetResumenAsync(campaniaId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("tiendas")]
    public async Task<ActionResult> GetTiendasAsync(int campaniaId, CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.GetTiendasAsync(campaniaId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("tiendas/{tiendaCadenaKey}")]
    public async Task<ActionResult> RemoveTiendaAsync(
        int campaniaId,
        string tiendaCadenaKey,
        CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.RemoveTiendaAsync(campaniaId, tiendaCadenaKey, cancellationToken);
        return Ok(result);
    }

    [HttpPost("fechas")]
    public async Task<ActionResult> AddFechasAsync(
        int campaniaId,
        [FromBody] AddCampaniaFechasRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.AddFechasAsync(campaniaId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("fechas")]
    public async Task<ActionResult> GetFechasAsync(int campaniaId, CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.GetFechasAsync(campaniaId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("fechas/{fecha}")]
    public async Task<ActionResult> RemoveFechaAsync(
        int campaniaId,
        string fecha,
        CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return BadRequest(new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = "La fecha debe enviarse en formato yyyy-MM-dd."
            });
        }

        var result = await campaniaProgramacionService.RemoveFechaAsync(campaniaId, parsedDate, cancellationToken);
        return Ok(result);
    }

    [HttpPost("skus")]
    public async Task<ActionResult> AddSkusAsync(
        int campaniaId,
        [FromBody] AddCampaniaSkusRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.AddSkusAsync(campaniaId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("skus")]
    public async Task<ActionResult> GetSkusAsync(int campaniaId, CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.GetSkusAsync(campaniaId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("skus/{codigoSkuBimbo}")]
    public async Task<ActionResult> RemoveSkuAsync(
        int campaniaId,
        string codigoSkuBimbo,
        CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.RemoveSkuAsync(campaniaId, codigoSkuBimbo, cancellationToken);
        return Ok(result);
    }

    [HttpGet("programaciones")]
    public async Task<ActionResult> GetProgramacionesAsync(int campaniaId, CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.GetProgramacionesAsync(campaniaId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("programaciones/{programacionId:long}/detalles")]
    public async Task<ActionResult> GetDetallesAsync(
        int campaniaId,
        long programacionId,
        CancellationToken cancellationToken)
    {
        var result = await campaniaProgramacionService.GetDetallesAsync(campaniaId, programacionId, cancellationToken);
        return Ok(result);
    }
}

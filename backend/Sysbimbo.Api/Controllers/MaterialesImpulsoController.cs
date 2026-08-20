using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.DTOs.MaterialesImpulso;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/materiales-impulso")]
public class MaterialesImpulsoController(IMaterialImpulsoService materialImpulsoService) : ControllerBase
{
    private const string GetByIdRouteName = "GetMaterialImpulsoById";
    private const string GetByTiendaRouteName = "GetMaterialImpulsoByTienda";

    [HttpGet]
    public async Task<ActionResult> GetAllAsync(
        [FromQuery] MaterialImpulsoQueryDto query,
        CancellationToken cancellationToken)
    {
        return Ok(await materialImpulsoService.GetAllAsync(query, cancellationToken));
    }

    [HttpGet("exportar")]
    public async Task<IActionResult> ExportExcelAsync(
        [FromQuery] MaterialImpulsoQueryDto query,
        CancellationToken cancellationToken)
    {
        var imageBaseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/materiales-impulso/fotos";
        var content = await materialImpulsoService.ExportExcelAsync(
            query,
            imageBaseUrl,
            cancellationToken);
        var fileName = $"materiales-impulso-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx";

        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("fotos/{fotoId:long}")]
    public async Task<IActionResult> GetPhotoAsync(long fotoId, CancellationToken cancellationToken)
    {
        var photo = await materialImpulsoService.GetPhotoAsync(fotoId, cancellationToken);
        Response.Headers.CacheControl = "private, max-age=3600";
        Response.Headers.Append(
            "Content-Disposition",
            $"inline; filename*=UTF-8''{Uri.EscapeDataString(photo.NombreArchivo)}");

        return File(photo.Contenido, photo.TipoContenido);
    }

    [HttpGet("{materialImpulsoTiendaId:long}/fotos")]
    public async Task<ActionResult> GetPhotosAsync(
        long materialImpulsoTiendaId,
        [FromQuery] bool soloHoy,
        CancellationToken cancellationToken)
    {
        return Ok(await materialImpulsoService.GetPhotosAsync(
            materialImpulsoTiendaId,
            soloHoy,
            cancellationToken));
    }

    [HttpDelete("{materialImpulsoTiendaId:long}/fotos/{fotoId:long}")]
    public async Task<IActionResult> DeletePhotoAsync(
        long materialImpulsoTiendaId,
        long fotoId,
        CancellationToken cancellationToken)
    {
        await materialImpulsoService.DeletePhotoAsync(
            materialImpulsoTiendaId,
            fotoId,
            cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:long}", Name = GetByIdRouteName)]
    public async Task<ActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return Ok(await materialImpulsoService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(
        [FromBody] CreateMaterialImpulsoDto dto,
        CancellationToken cancellationToken)
    {
        var result = await materialImpulsoService.CreateAsync(dto, cancellationToken);
        return CreatedAtRoute(GetByIdRouteName, new { id = result.MaterialImpulsoTiendaId }, result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult> UpdateAsync(
        long id,
        [FromBody] UpdateMaterialImpulsoDto dto,
        CancellationToken cancellationToken)
    {
        return Ok(await materialImpulsoService.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await materialImpulsoService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("tiendas/{tiendaCadenaKey}", Name = GetByTiendaRouteName)]
    public async Task<ActionResult> GetByTiendaAsync(
        string tiendaCadenaKey,
        CancellationToken cancellationToken)
    {
        var result = await materialImpulsoService.GetByTiendaAsync(tiendaCadenaKey, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Administrador,Supervisor")]
    [HttpPut("{materialImpulsoTiendaId:long}/canjes-hoy")]
    public async Task<ActionResult<CanjesDiariosDto>> UpdateDailyExchangesAsync(
        long materialImpulsoTiendaId,
        [FromBody] UpdateCanjesDiariosDto dto,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
        {
            return Unauthorized(new { message = "La sesion no contiene un usuario valido." });
        }

        return Ok(await materialImpulsoService.UpdateDailyExchangesAsync(
            materialImpulsoTiendaId,
            dto.Cantidad,
            usuarioId,
            cancellationToken));
    }

    [HttpPost("{materialImpulsoTiendaId:long}/fotos")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<ActionResult> SavePhotoAsync(
        long materialImpulsoTiendaId,
        [FromForm] IFormFile foto,
        CancellationToken cancellationToken)
    {
        var result = await materialImpulsoService.SavePhotoAsync(
            materialImpulsoTiendaId,
            foto,
            cancellationToken);

        return CreatedAtRoute(
            GetByTiendaRouteName,
            new { tiendaCadenaKey = result.TiendaCadenaKey },
            result);
    }
}

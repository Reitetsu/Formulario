using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.Cuotas;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Repositories.Interfaces;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Services;

public class CuotaService(ICuotaRepository cuotaRepository) : ICuotaService
{
    public async Task<PagedResultDto<CuotaDto>> GetAllAsync(CuotaQueryDto query, CancellationToken cancellationToken)
    {
        var result = await cuotaRepository.GetAllAsync(new CuotaFilter
        {
            Campania = query.Campania,
            TiendaCadenaKey = query.TiendaCadenaKey,
            Fecha = query.Fecha,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        }, cancellationToken);

        return new PagedResultDto<CuotaDto>
        {
            Items = result.Items.Select(MapToDto).ToArray(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };
    }

    public async Task<CuotaDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var cuota = await cuotaRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la cuota con id {id}.");

        return MapToDto(cuota);
    }

    public async Task<CuotaDto> CreateAsync(CreateCuotaDto dto, CancellationToken cancellationToken)
    {
        var entity = new FactCampaniaCuota
        {
            Campania = dto.Campania.Trim(),
            TiendaCadenaKey = dto.TiendaCadenaKey.Trim(),
            Fecha = dto.Fecha,
            Cuota = dto.Cuota
        };

        await cuotaRepository.AddAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task<CuotaDto> UpdateAsync(long id, UpdateCuotaDto dto, CancellationToken cancellationToken)
    {
        var existing = await cuotaRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la cuota con id {id}.");

        existing.Campania = dto.Campania.Trim();
        existing.TiendaCadenaKey = dto.TiendaCadenaKey.Trim();
        existing.Fecha = dto.Fecha;
        existing.Cuota = dto.Cuota;

        await cuotaRepository.UpdateAsync(cancellationToken);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var existing = await cuotaRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la cuota con id {id}.");

        await cuotaRepository.DeleteAsync(existing, cancellationToken);
    }

    private static CuotaDto MapToDto(FactCampaniaCuota entity) =>
        new()
        {
            CuotaId = entity.CuotaId,
            Campania = entity.Campania,
            TiendaCadenaKey = entity.TiendaCadenaKey,
            Fecha = entity.Fecha,
            Cuota = entity.Cuota
        };
}

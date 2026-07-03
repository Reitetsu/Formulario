using Sysbimbo.Api.DTOs.Campanias;
using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Repositories.Interfaces;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Services;

public class CampaniaService(ICampaniaRepository campaniaRepository) : ICampaniaService
{
    public async Task<PagedResultDto<CampaniaDto>> GetAllAsync(CampaniaQueryDto query, CancellationToken cancellationToken)
    {
        var result = await campaniaRepository.GetAllAsync(new CampaniaFilter
        {
            NombreCampania = query.NombreCampania,
            Estado = query.Estado,
            Descripcion = query.Descripcion,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        }, cancellationToken);

        return new PagedResultDto<CampaniaDto>
        {
            Items = result.Items.Select(MapToDto).ToArray(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };
    }

    public async Task<CampaniaDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var campania = await campaniaRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la campania con id {id}.");

        return MapToDto(campania);
    }

    public async Task<CampaniaDto> CreateAsync(CreateCampaniaDto dto, CancellationToken cancellationToken)
    {
        var entity = new Campania
        {
            NombreCampania = dto.NombreCampania.Trim(),
            Descripcion = dto.Descripcion,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Estado = dto.Estado
        };

        await campaniaRepository.AddAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task<CampaniaDto> UpdateAsync(int id, UpdateCampaniaDto dto, CancellationToken cancellationToken)
    {
        var existing = await campaniaRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la campania con id {id}.");

        existing.NombreCampania = dto.NombreCampania.Trim();
        existing.Descripcion = dto.Descripcion;
        existing.FechaInicio = dto.FechaInicio;
        existing.FechaFin = dto.FechaFin;
        existing.Estado = dto.Estado;

        await campaniaRepository.UpdateAsync(cancellationToken);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var existing = await campaniaRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la campania con id {id}.");

        await campaniaRepository.DeleteAsync(existing, cancellationToken);
    }

    private static CampaniaDto MapToDto(Campania entity) =>
        new()
        {
            CampaniaId = entity.CampaniaId,
            NombreCampania = entity.NombreCampania,
            Descripcion = entity.Descripcion,
            FechaInicio = entity.FechaInicio,
            FechaFin = entity.FechaFin,
            Estado = entity.Estado
        };
}

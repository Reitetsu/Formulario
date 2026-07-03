using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.Programaciones;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Repositories.Interfaces;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Services;

public class ProgramacionService(IProgramacionRepository programacionRepository, SysbimboDbContext dbContext) : IProgramacionService
{
    public async Task<PagedResultDto<ProgramacionDto>> GetAllAsync(ProgramacionQueryDto query, CancellationToken cancellationToken)
    {
        var result = await programacionRepository.GetAllAsync(new ProgramacionFilter
        {
            NombreCampania = query.NombreCampania,
            NombreTiendaBimbo = query.NombreTiendaBimbo,
            Fecha = query.Fecha,
            Cuota = query.Cuota,
            Estado = query.Estado,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        }, cancellationToken);

        return new PagedResultDto<ProgramacionDto>
        {
            Items = await EnrichDtosAsync(result.Items, cancellationToken),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };
    }

    public async Task<ProgramacionDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var programacion = await programacionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la programacion con id {id}.");

        return (await EnrichDtosAsync([programacion], cancellationToken)).Single();
    }

    public async Task<IReadOnlyCollection<DetalleProgramacionDto>> GetDetailByProgramacionIdAsync(
        long programacionId,
        CancellationToken cancellationToken)
    {
        var details = await programacionRepository.GetDetailByProgramacionIdAsync(programacionId, cancellationToken);
        if (details.Count == 0)
        {
            return [];
        }

        var skuCodes = details
            .Select(x => x.CodigoSkuBimbo)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToArray();

        var skuNames = skuCodes.Length == 0
            ? new Dictionary<string, string?>()
            : await dbContext.Skus
                .AsNoTracking()
                .Where(x => x.CodigoSkuBimbo != null && skuCodes.Contains(x.CodigoSkuBimbo))
                .GroupBy(x => x.CodigoSkuBimbo!)
                .ToDictionaryAsync(
                    x => x.Key,
                    x => x.Select(y => y.NombreSkuBimbo).FirstOrDefault(),
                    cancellationToken);

        return details
            .Select(detail =>
            {
                skuNames.TryGetValue(detail.CodigoSkuBimbo, out var nombreSkuBimbo);

                return new DetalleProgramacionDto
                {
                    DetalleProgramacionId = detail.DetalleProgramacionId,
                    ProgramacionId = detail.ProgramacionId,
                    CodigoSkuBimbo = detail.CodigoSkuBimbo,
                    NombreSkuBimbo = nombreSkuBimbo,
                    FechaCreacion = detail.FechaCreacion
                };
            })
            .ToArray();
    }

    public async Task<ProgramacionDto> CreateAsync(CreateProgramacionDto dto, CancellationToken cancellationToken)
    {
        var entity = new Programacion
        {
            CampaniaId = dto.CampaniaId,
            TiendaCadenaKey = dto.TiendaCadenaKey.Trim(),
            Fecha = dto.Fecha,
            Estado = dto.Estado,
            FuenteProgramacion = dto.FuenteProgramacion,
            FechaCreacion = dto.FechaCreacion,
            FechaActualizacion = dto.FechaActualizacion
        };

        await programacionRepository.AddAsync(entity, cancellationToken);
        await UpsertCuotaAsync(entity.CampaniaId, entity.TiendaCadenaKey, entity.Fecha, dto.Cuota, cancellationToken);
        return (await EnrichDtosAsync([entity], cancellationToken)).Single();
    }

    public async Task<ProgramacionDto> UpdateAsync(long id, UpdateProgramacionDto dto, CancellationToken cancellationToken)
    {
        var existing = await programacionRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la programacion con id {id}.");

        existing.CampaniaId = dto.CampaniaId;
        existing.TiendaCadenaKey = dto.TiendaCadenaKey.Trim();
        existing.Fecha = dto.Fecha;
        existing.Estado = dto.Estado;
        existing.FuenteProgramacion = dto.FuenteProgramacion;
        existing.FechaCreacion = dto.FechaCreacion;
        existing.FechaActualizacion = dto.FechaActualizacion;

        await programacionRepository.UpdateAsync(cancellationToken);
        await UpsertCuotaAsync(existing.CampaniaId, existing.TiendaCadenaKey, existing.Fecha, dto.Cuota, cancellationToken);
        return (await EnrichDtosAsync([existing], cancellationToken)).Single();
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var existing = await programacionRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la programacion con id {id}.");

        await programacionRepository.DeleteAsync(existing, cancellationToken);
    }

    private async Task<IReadOnlyCollection<ProgramacionDto>> EnrichDtosAsync(
        IEnumerable<Programacion> entities,
        CancellationToken cancellationToken)
    {
        var items = entities.ToArray();
        if (items.Length == 0)
        {
            return [];
        }

        var campaniaIds = items
            .Where(x => x.CampaniaId.HasValue)
            .Select(x => x.CampaniaId!.Value)
            .Distinct()
            .ToArray();

        var tiendaKeys = items
            .Select(x => x.TiendaCadenaKey)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct()
            .ToArray();

        var campanias = campaniaIds.Length == 0
            ? new Dictionary<int, string?>()
            : await dbContext.Campanias
                .AsNoTracking()
                .Where(x => campaniaIds.Contains(x.CampaniaId))
                .ToDictionaryAsync(x => x.CampaniaId, x => x.NombreCampania, cancellationToken);

        var tiendas = tiendaKeys.Length == 0
            ? new Dictionary<string, string?>()
            : await dbContext.Tiendas
                .AsNoTracking()
                .Where(x => tiendaKeys.Contains(x.TiendaCadenaKey))
                .ToDictionaryAsync(
                    x => x.TiendaCadenaKey,
                    x => x.NombreTiendaBimbo ?? x.NombreTienda,
                    cancellationToken);

        var campaniaNames = campanias.Values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct()
            .ToArray();

        var cuotas = campaniaNames.Length == 0 || tiendaKeys.Length == 0
            ? []
            : await dbContext.Cuotas
                .AsNoTracking()
                .Where(x =>
                    x.Campania != null &&
                    campaniaNames.Contains(x.Campania) &&
                    x.TiendaCadenaKey != null &&
                    tiendaKeys.Contains(x.TiendaCadenaKey))
                .ToListAsync(cancellationToken);

        var cuotaByKey = cuotas
            .GroupBy(x => BuildCuotaKey(x.Campania, x.TiendaCadenaKey, x.Fecha))
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.Cuota).FirstOrDefault());

        return items
            .Select(entity =>
            {
                campanias.TryGetValue(entity.CampaniaId ?? default, out var nombreCampania);
                tiendas.TryGetValue(entity.TiendaCadenaKey ?? string.Empty, out var nombreTiendaBimbo);

                var cuotaKey = BuildCuotaKey(nombreCampania, entity.TiendaCadenaKey, entity.Fecha);
                cuotaByKey.TryGetValue(cuotaKey, out var cuota);

                return new ProgramacionDto
                {
                    ProgramacionId = entity.ProgramacionId,
                    CampaniaId = entity.CampaniaId,
                    NombreCampania = nombreCampania,
                    TiendaCadenaKey = entity.TiendaCadenaKey,
                    NombreTiendaBimbo = nombreTiendaBimbo,
                    Fecha = entity.Fecha,
                    Cuota = cuota,
                    Estado = entity.Estado,
                    FuenteProgramacion = entity.FuenteProgramacion,
                    FechaCreacion = entity.FechaCreacion,
                    FechaActualizacion = entity.FechaActualizacion
                };
            })
            .ToArray();
    }

    private static string BuildCuotaKey(string? campania, string? tiendaCadenaKey, DateTime? fecha)
    {
        var fechaTexto = fecha?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
        return $"{campania ?? string.Empty}|{tiendaCadenaKey ?? string.Empty}|{fechaTexto}";
    }

    private async Task UpsertCuotaAsync(
        int? campaniaId,
        string? tiendaCadenaKey,
        DateTime? fecha,
        decimal? cuota,
        CancellationToken cancellationToken)
    {
        if (!campaniaId.HasValue || string.IsNullOrWhiteSpace(tiendaCadenaKey) || !fecha.HasValue || !cuota.HasValue)
        {
            return;
        }

        var nombreCampania = await dbContext.Campanias
            .AsNoTracking()
            .Where(x => x.CampaniaId == campaniaId.Value)
            .Select(x => x.NombreCampania)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(nombreCampania))
        {
            return;
        }

        var targetDate = fecha.Value.Date;
        var existingCuota = await dbContext.Cuotas.FirstOrDefaultAsync(
            x => x.Campania == nombreCampania &&
                 x.TiendaCadenaKey == tiendaCadenaKey &&
                 x.Fecha.HasValue &&
                 x.Fecha.Value.Date == targetDate,
            cancellationToken);

        if (existingCuota is null)
        {
            await dbContext.Cuotas.AddAsync(new FactCampaniaCuota
            {
                Campania = nombreCampania,
                TiendaCadenaKey = tiendaCadenaKey,
                Fecha = targetDate,
                Cuota = cuota.Value
            }, cancellationToken);
        }
        else
        {
            existingCuota.Cuota = cuota.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

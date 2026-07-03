using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;
using Sysbimbo.Api.Repositories.Interfaces;

namespace Sysbimbo.Api.Repositories;

public class ProgramacionRepository(SysbimboDbContext dbContext) : IProgramacionRepository
{
    public async Task<PagedResult<Programacion>> GetAllAsync(ProgramacionFilter filter, CancellationToken cancellationToken)
    {
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);

        var query = dbContext.Programaciones.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.NombreCampania))
        {
            var campaniaIds = await dbContext.Campanias
                .AsNoTracking()
                .Where(x =>
                    x.NombreCampania != null &&
                    EF.Functions.Like(x.NombreCampania, $"%{filter.NombreCampania}%"))
                .Select(x => x.CampaniaId)
                .ToArrayAsync(cancellationToken);

            query = campaniaIds.Length == 0
                ? query.Where(_ => false)
                : query.Where(x => x.CampaniaId.HasValue && campaniaIds.Contains(x.CampaniaId.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.NombreTiendaBimbo))
        {
            var tiendaKeys = await dbContext.Tiendas
                .AsNoTracking()
                .Where(x =>
                    (x.NombreTiendaBimbo != null && EF.Functions.Like(x.NombreTiendaBimbo, $"%{filter.NombreTiendaBimbo}%")) ||
                    (x.NombreTienda != null && EF.Functions.Like(x.NombreTienda, $"%{filter.NombreTiendaBimbo}%")))
                .Select(x => x.TiendaCadenaKey)
                .ToArrayAsync(cancellationToken);

            query = tiendaKeys.Length == 0
                ? query.Where(_ => false)
                : query.Where(x => x.TiendaCadenaKey != null && tiendaKeys.Contains(x.TiendaCadenaKey));
        }

        if (filter.Fecha.HasValue)
        {
            var fecha = filter.Fecha.Value.Date;
            query = query.Where(x => x.Fecha.HasValue && x.Fecha.Value.Date == fecha);
        }

        if (filter.Cuota.HasValue)
        {
            var cuota = filter.Cuota.Value;
            var cuotas = dbContext.Cuotas
                .AsNoTracking()
                .Where(x => x.Cuota.HasValue && x.Cuota.Value == cuota)
                .Select(x => new
                {
                    x.Campania,
                    x.TiendaCadenaKey,
                    Fecha = x.Fecha.HasValue ? x.Fecha.Value.Date : (DateTime?)null
                });

            query =
                from programacion in query
                join cuotaRegistro in cuotas
                    on new
                    {
                        TiendaCadenaKey = programacion.TiendaCadenaKey,
                        Fecha = programacion.Fecha.HasValue ? programacion.Fecha.Value.Date : (DateTime?)null
                    }
                    equals new
                    {
                        cuotaRegistro.TiendaCadenaKey,
                        cuotaRegistro.Fecha
                    }
                join campania in dbContext.Campanias.AsNoTracking()
                    on programacion.CampaniaId equals campania.CampaniaId
                where campania.NombreCampania == cuotaRegistro.Campania
                select programacion;
        }

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            query = query.Where(x =>
                x.Estado != null &&
                EF.Functions.Like(x.Estado, $"%{filter.Estado}%"));
        }

        query = query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.ProgramacionId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Programacion>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public Task<Programacion?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        dbContext.Programaciones.AsNoTracking().FirstOrDefaultAsync(x => x.ProgramacionId == id, cancellationToken);

    public Task<Programacion?> GetForUpdateAsync(long id, CancellationToken cancellationToken) =>
        dbContext.Programaciones.FirstOrDefaultAsync(x => x.ProgramacionId == id, cancellationToken);

    public async Task<IReadOnlyCollection<DetalleProgramacion>> GetDetailByProgramacionIdAsync(long programacionId, CancellationToken cancellationToken) =>
        await dbContext.DetalleProgramaciones
            .AsNoTracking()
            .Where(x => x.ProgramacionId == programacionId)
            .OrderBy(x => x.DetalleProgramacionId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Programacion programacion, CancellationToken cancellationToken)
    {
        await dbContext.Programaciones.AddAsync(programacion, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Programacion programacion, CancellationToken cancellationToken)
    {
        dbContext.Programaciones.Remove(programacion);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

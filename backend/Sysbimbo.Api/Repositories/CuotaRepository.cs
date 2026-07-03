using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;
using Sysbimbo.Api.Repositories.Interfaces;

namespace Sysbimbo.Api.Repositories;

public class CuotaRepository(SysbimboDbContext dbContext) : ICuotaRepository
{
    public async Task<PagedResult<FactCampaniaCuota>> GetAllAsync(CuotaFilter filter, CancellationToken cancellationToken)
    {
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);

        var query = dbContext.Cuotas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Campania))
        {
            query = query.Where(x => x.Campania != null && EF.Functions.Like(x.Campania, $"%{filter.Campania}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.TiendaCadenaKey))
        {
            query = query.Where(x =>
                x.TiendaCadenaKey != null &&
                EF.Functions.Like(x.TiendaCadenaKey, $"%{filter.TiendaCadenaKey}%"));
        }

        if (filter.Fecha.HasValue)
        {
            var fecha = filter.Fecha.Value.Date;
            query = query.Where(x => x.Fecha.HasValue && x.Fecha.Value.Date == fecha);
        }

        query = query
            .OrderByDescending(x => x.Fecha)
            .ThenBy(x => x.Campania)
            .ThenBy(x => x.TiendaCadenaKey);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FactCampaniaCuota>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public Task<FactCampaniaCuota?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        dbContext.Cuotas.AsNoTracking().FirstOrDefaultAsync(x => x.CuotaId == id, cancellationToken);

    public Task<FactCampaniaCuota?> GetForUpdateAsync(long id, CancellationToken cancellationToken) =>
        dbContext.Cuotas.FirstOrDefaultAsync(x => x.CuotaId == id, cancellationToken);

    public async Task AddAsync(FactCampaniaCuota cuota, CancellationToken cancellationToken)
    {
        await dbContext.Cuotas.AddAsync(cuota, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(FactCampaniaCuota cuota, CancellationToken cancellationToken)
    {
        dbContext.Cuotas.Remove(cuota);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

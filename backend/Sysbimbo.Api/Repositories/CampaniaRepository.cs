using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;
using Sysbimbo.Api.Repositories.Interfaces;

namespace Sysbimbo.Api.Repositories;

public class CampaniaRepository(SysbimboDbContext dbContext) : ICampaniaRepository
{
    public async Task<PagedResult<Campania>> GetAllAsync(CampaniaFilter filter, CancellationToken cancellationToken)
    {
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);

        var query = dbContext.Campanias.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.NombreCampania))
        {
            query = query.Where(x =>
                x.NombreCampania != null &&
                EF.Functions.Like(x.NombreCampania, $"%{filter.NombreCampania}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Estado))
        {
            query = query.Where(x =>
                x.Estado != null &&
                EF.Functions.Like(x.Estado, $"%{filter.Estado}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Descripcion))
        {
            query = query.Where(x =>
                x.Descripcion != null &&
                EF.Functions.Like(x.Descripcion, $"%{filter.Descripcion}%"));
        }

        query = query.OrderByDescending(x => x.CampaniaId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Campania>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public Task<Campania?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Campanias.AsNoTracking().FirstOrDefaultAsync(x => x.CampaniaId == id, cancellationToken);

    public Task<Campania?> GetForUpdateAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Campanias.FirstOrDefaultAsync(x => x.CampaniaId == id, cancellationToken);

    public async Task AddAsync(Campania campania, CancellationToken cancellationToken)
    {
        await dbContext.Campanias.AddAsync(campania, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Campania campania, CancellationToken cancellationToken)
    {
        dbContext.Campanias.Remove(campania);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

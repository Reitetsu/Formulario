using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;
using Sysbimbo.Api.Repositories.Interfaces;

namespace Sysbimbo.Api.Repositories;

public class TiendaRepository(SysbimboDbContext dbContext) : ITiendaRepository
{
    public async Task<PagedResult<DimTiendaMaestraExport>> GetAllAsync(TiendaFilter filter, CancellationToken cancellationToken)
    {
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);

        var query = dbContext.Tiendas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Cadena))
        {
            query = query.Where(x => x.Cadena != null && EF.Functions.Like(x.Cadena, $"%{filter.Cadena}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Region))
        {
            query = query.Where(x => x.Region != null && EF.Functions.Like(x.Region, $"%{filter.Region}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Nombre))
        {
            query = query.Where(x =>
                (x.NombreTienda != null && EF.Functions.Like(x.NombreTienda, $"%{filter.Nombre}%")) ||
                (x.NombreTiendaBimbo != null && EF.Functions.Like(x.NombreTiendaBimbo, $"%{filter.Nombre}%")));
        }

        if (!string.IsNullOrWhiteSpace(filter.CodigoTiendaB2B))
        {
            query = query.Where(x => x.CodigoTiendaB2B != null && EF.Functions.Like(x.CodigoTiendaB2B, $"%{filter.CodigoTiendaB2B}%"));
        }

        query = query.OrderBy(x => x.TiendaCadenaKey);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DimTiendaMaestraExport>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public Task<DimTiendaMaestraExport?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        dbContext.Tiendas.AsNoTracking().FirstOrDefaultAsync(x => x.TiendaCadenaKey == id, cancellationToken);

    public Task<DimTiendaMaestraExport?> GetForUpdateAsync(string id, CancellationToken cancellationToken) =>
        dbContext.Tiendas.FirstOrDefaultAsync(x => x.TiendaCadenaKey == id, cancellationToken);

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken) =>
        dbContext.Tiendas.AnyAsync(x => x.TiendaCadenaKey == id, cancellationToken);

    public async Task AddAsync(DimTiendaMaestraExport tienda, CancellationToken cancellationToken)
    {
        await dbContext.Tiendas.AddAsync(tienda, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(DimTiendaMaestraExport tienda, CancellationToken cancellationToken)
    {
        dbContext.Tiendas.Remove(tienda);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.DTOs.Skus;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;
using Sysbimbo.Api.Repositories.Interfaces;

namespace Sysbimbo.Api.Repositories;

public class SkuRepository(SysbimboDbContext dbContext) : ISkuRepository
{
    public async Task<PagedResult<DimSkuMaestraExport>> GetAllAsync(SkuFilter filter, CancellationToken cancellationToken)
    {
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);

        var query = dbContext.Skus.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Categoria))
        {
            query = query.Where(x => x.Categoria != null && EF.Functions.Like(x.Categoria, $"%{filter.Categoria}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Marca))
        {
            query = query.Where(x => x.Marca != null && EF.Functions.Like(x.Marca, $"%{filter.Marca}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Nombre))
        {
            query = query.Where(x =>
                (x.NombreSkuB2B != null && EF.Functions.Like(x.NombreSkuB2B, $"%{filter.Nombre}%")) ||
                (x.NombreSkuBimbo != null && EF.Functions.Like(x.NombreSkuBimbo, $"%{filter.Nombre}%")));
        }

        if (!string.IsNullOrWhiteSpace(filter.CodigoSkuB2B))
        {
            query = query.Where(x => x.CodigoSkuB2B != null && EF.Functions.Like(x.CodigoSkuB2B, $"%{filter.CodigoSkuB2B}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.CodigoSkuBimbo))
        {
            query = query.Where(x => x.CodigoSkuBimbo != null && EF.Functions.Like(x.CodigoSkuBimbo, $"%{filter.CodigoSkuBimbo}%"));
        }

        query = query.OrderBy(x => x.SkuKey);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DimSkuMaestraExport>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<PagedResult<SkuCatalogoDto>> GetCatalogoAsync(SkuFilter filter, CancellationToken cancellationToken)
    {
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);

        var query = dbContext.Skus
            .AsNoTracking()
            .Where(x => x.CodigoSkuBimbo != null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Categoria))
        {
            query = query.Where(x => x.Categoria != null && EF.Functions.Like(x.Categoria, $"%{filter.Categoria}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Marca))
        {
            query = query.Where(x => x.Marca != null && EF.Functions.Like(x.Marca, $"%{filter.Marca}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Nombre))
        {
            query = query.Where(x =>
                (x.NombreSkuB2B != null && EF.Functions.Like(x.NombreSkuB2B, $"%{filter.Nombre}%")) ||
                (x.NombreSkuBimbo != null && EF.Functions.Like(x.NombreSkuBimbo, $"%{filter.Nombre}%")));
        }

        if (!string.IsNullOrWhiteSpace(filter.CodigoSkuB2B))
        {
            query = query.Where(x => x.CodigoSkuB2B != null && EF.Functions.Like(x.CodigoSkuB2B, $"%{filter.CodigoSkuB2B}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.CodigoSkuBimbo))
        {
            query = query.Where(x => x.CodigoSkuBimbo != null && EF.Functions.Like(x.CodigoSkuBimbo, $"%{filter.CodigoSkuBimbo}%"));
        }

        var groupedQuery = query
            .GroupBy(x => x.CodigoSkuBimbo!)
            .Select(group => new SkuCatalogoDto
            {
                CodigoSkuBimbo = group.Key,
                CodigoSkuB2B = group.Min(x => x.CodigoSkuB2B),
                NombreSkuBimbo = group.Min(x => x.NombreSkuBimbo),
                NombreSkuB2B = group.Min(x => x.NombreSkuB2B),
                UnidadNegocio = group.Min(x => x.UnidadNegocio),
                Area = group.Min(x => x.Area),
                Categoria = group.Min(x => x.Categoria),
                Marca = group.Min(x => x.Marca),
                TipoProducto = group.Min(x => x.TipoProducto),
                Status = group.Min(x => x.Status),
                Gramaje = group.Min(x => x.Gramaje)
            })
            .OrderBy(x => x.CodigoSkuBimbo);

        var totalCount = await groupedQuery.CountAsync(cancellationToken);
        var items = await groupedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SkuCatalogoDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public Task<DimSkuMaestraExport?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        dbContext.Skus.AsNoTracking().FirstOrDefaultAsync(x => x.SkuKey == id, cancellationToken);

    public Task<DimSkuMaestraExport?> GetForUpdateAsync(string id, CancellationToken cancellationToken) =>
        dbContext.Skus.FirstOrDefaultAsync(x => x.SkuKey == id, cancellationToken);

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken) =>
        dbContext.Skus.AnyAsync(x => x.SkuKey == id, cancellationToken);

    public async Task AddAsync(DimSkuMaestraExport sku, CancellationToken cancellationToken)
    {
        await dbContext.Skus.AddAsync(sku, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(DimSkuMaestraExport sku, CancellationToken cancellationToken)
    {
        dbContext.Skus.Remove(sku);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

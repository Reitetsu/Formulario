using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;
using Sysbimbo.Api.DTOs.Skus;

namespace Sysbimbo.Api.Repositories.Interfaces;

public interface ISkuRepository
{
    Task<PagedResult<DimSkuMaestraExport>> GetAllAsync(SkuFilter filter, CancellationToken cancellationToken);
    Task<PagedResult<SkuCatalogoDto>> GetCatalogoAsync(SkuFilter filter, CancellationToken cancellationToken);
    Task<DimSkuMaestraExport?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<DimSkuMaestraExport?> GetForUpdateAsync(string id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);
    Task AddAsync(DimSkuMaestraExport sku, CancellationToken cancellationToken);
    Task UpdateAsync(CancellationToken cancellationToken);
    Task DeleteAsync(DimSkuMaestraExport sku, CancellationToken cancellationToken);
}

using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;

namespace Sysbimbo.Api.Repositories.Interfaces;

public interface ITiendaRepository
{
    Task<PagedResult<DimTiendaMaestraExport>> GetAllAsync(TiendaFilter filter, CancellationToken cancellationToken);
    Task<DimTiendaMaestraExport?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<DimTiendaMaestraExport?> GetForUpdateAsync(string id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);
    Task AddAsync(DimTiendaMaestraExport tienda, CancellationToken cancellationToken);
    Task UpdateAsync(CancellationToken cancellationToken);
    Task DeleteAsync(DimTiendaMaestraExport tienda, CancellationToken cancellationToken);
}

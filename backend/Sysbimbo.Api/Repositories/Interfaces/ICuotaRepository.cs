using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;

namespace Sysbimbo.Api.Repositories.Interfaces;

public interface ICuotaRepository
{
    Task<PagedResult<FactCampaniaCuota>> GetAllAsync(CuotaFilter filter, CancellationToken cancellationToken);
    Task<FactCampaniaCuota?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<FactCampaniaCuota?> GetForUpdateAsync(long id, CancellationToken cancellationToken);
    Task AddAsync(FactCampaniaCuota cuota, CancellationToken cancellationToken);
    Task UpdateAsync(CancellationToken cancellationToken);
    Task DeleteAsync(FactCampaniaCuota cuota, CancellationToken cancellationToken);
}

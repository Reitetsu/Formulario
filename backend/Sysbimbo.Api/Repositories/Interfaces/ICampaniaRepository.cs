using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;

namespace Sysbimbo.Api.Repositories.Interfaces;

public interface ICampaniaRepository
{
    Task<PagedResult<Campania>> GetAllAsync(CampaniaFilter filter, CancellationToken cancellationToken);
    Task<Campania?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Campania?> GetForUpdateAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Campania campania, CancellationToken cancellationToken);
    Task UpdateAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Campania campania, CancellationToken cancellationToken);
}

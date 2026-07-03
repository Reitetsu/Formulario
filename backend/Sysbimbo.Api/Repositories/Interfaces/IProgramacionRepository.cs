using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Models.Pagination;

namespace Sysbimbo.Api.Repositories.Interfaces;

public interface IProgramacionRepository
{
    Task<PagedResult<Programacion>> GetAllAsync(ProgramacionFilter filter, CancellationToken cancellationToken);
    Task<Programacion?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Programacion?> GetForUpdateAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DetalleProgramacion>> GetDetailByProgramacionIdAsync(long programacionId, CancellationToken cancellationToken);
    Task AddAsync(Programacion programacion, CancellationToken cancellationToken);
    Task UpdateAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Programacion programacion, CancellationToken cancellationToken);
}

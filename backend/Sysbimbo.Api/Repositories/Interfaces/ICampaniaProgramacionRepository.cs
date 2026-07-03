using Sysbimbo.Api.Models.Entities;

namespace Sysbimbo.Api.Repositories.Interfaces;

public interface ICampaniaProgramacionRepository
{
    Task<IReadOnlyCollection<Programacion>> GetProgramacionesByCampaniaAsync(int campaniaId, CancellationToken cancellationToken);
    Task<Programacion?> GetProgramacionByCampaniaAsync(int campaniaId, long programacionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DetalleProgramacion>> GetDetallesByProgramacionIdsAsync(
        IReadOnlyCollection<long> programacionIds,
        CancellationToken cancellationToken);
}

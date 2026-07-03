using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Repositories.Interfaces;

namespace Sysbimbo.Api.Repositories;

public class CampaniaProgramacionRepository(SysbimboDbContext dbContext) : ICampaniaProgramacionRepository
{
    public async Task<IReadOnlyCollection<Programacion>> GetProgramacionesByCampaniaAsync(
        int campaniaId,
        CancellationToken cancellationToken) =>
        await dbContext.Programaciones
            .AsNoTracking()
            .Where(x => x.CampaniaId == campaniaId)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.ProgramacionId)
            .ToListAsync(cancellationToken);

    public Task<Programacion?> GetProgramacionByCampaniaAsync(
        int campaniaId,
        long programacionId,
        CancellationToken cancellationToken) =>
        dbContext.Programaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.CampaniaId == campaniaId && x.ProgramacionId == programacionId,
                cancellationToken);

    public async Task<IReadOnlyCollection<DetalleProgramacion>> GetDetallesByProgramacionIdsAsync(
        IReadOnlyCollection<long> programacionIds,
        CancellationToken cancellationToken)
    {
        if (programacionIds.Count == 0)
        {
            return [];
        }

        return await dbContext.DetalleProgramaciones
            .AsNoTracking()
            .Where(x => programacionIds.Contains(x.ProgramacionId))
            .OrderBy(x => x.ProgramacionId)
            .ThenBy(x => x.DetalleProgramacionId)
            .ToListAsync(cancellationToken);
    }
}

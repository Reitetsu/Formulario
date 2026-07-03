using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.Programaciones;

namespace Sysbimbo.Api.Services.Interfaces;

public interface IProgramacionService
{
    Task<PagedResultDto<ProgramacionDto>> GetAllAsync(ProgramacionQueryDto query, CancellationToken cancellationToken);
    Task<ProgramacionDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DetalleProgramacionDto>> GetDetailByProgramacionIdAsync(long programacionId, CancellationToken cancellationToken);
    Task<ProgramacionDto> CreateAsync(CreateProgramacionDto dto, CancellationToken cancellationToken);
    Task<ProgramacionDto> UpdateAsync(long id, UpdateProgramacionDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}

using Sysbimbo.Api.DTOs.Campanias;
using Sysbimbo.Api.DTOs.Common;

namespace Sysbimbo.Api.Services.Interfaces;

public interface ICampaniaService
{
    Task<PagedResultDto<CampaniaDto>> GetAllAsync(CampaniaQueryDto query, CancellationToken cancellationToken);
    Task<CampaniaDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CampaniaDto> CreateAsync(CreateCampaniaDto dto, CancellationToken cancellationToken);
    Task<CampaniaDto> UpdateAsync(int id, UpdateCampaniaDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

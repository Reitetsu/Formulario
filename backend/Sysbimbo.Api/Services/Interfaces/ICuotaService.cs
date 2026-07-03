using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.Cuotas;

namespace Sysbimbo.Api.Services.Interfaces;

public interface ICuotaService
{
    Task<PagedResultDto<CuotaDto>> GetAllAsync(CuotaQueryDto query, CancellationToken cancellationToken);
    Task<CuotaDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<CuotaDto> CreateAsync(CreateCuotaDto dto, CancellationToken cancellationToken);
    Task<CuotaDto> UpdateAsync(long id, UpdateCuotaDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}

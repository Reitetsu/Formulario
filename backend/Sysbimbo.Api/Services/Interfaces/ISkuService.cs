using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.Skus;

namespace Sysbimbo.Api.Services.Interfaces;

public interface ISkuService
{
    Task<PagedResultDto<SkuDto>> GetAllAsync(SkuQueryDto query, CancellationToken cancellationToken);
    Task<PagedResultDto<SkuCatalogoDto>> GetCatalogoAsync(SkuQueryDto query, CancellationToken cancellationToken);
    Task<SkuDto> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<SkuDto> CreateAsync(CreateSkuDto dto, CancellationToken cancellationToken);
    Task<SkuDto> UpdateAsync(string id, UpdateSkuDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}

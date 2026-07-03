using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.Tiendas;

namespace Sysbimbo.Api.Services.Interfaces;

public interface ITiendaService
{
    Task<PagedResultDto<TiendaDto>> GetAllAsync(TiendaQueryDto query, CancellationToken cancellationToken);
    Task<TiendaDto> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<TiendaDto> CreateAsync(CreateTiendaDto dto, CancellationToken cancellationToken);
    Task<TiendaDto> UpdateAsync(string id, UpdateTiendaDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}

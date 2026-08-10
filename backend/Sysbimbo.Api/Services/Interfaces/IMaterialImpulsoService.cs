using Sysbimbo.Api.DTOs.MaterialesImpulso;
using Sysbimbo.Api.DTOs.Common;

namespace Sysbimbo.Api.Services.Interfaces;

public interface IMaterialImpulsoService
{
    Task<PagedResultDto<MaterialImpulsoAdminDto>> GetAllAsync(
        MaterialImpulsoQueryDto query,
        CancellationToken cancellationToken);
    Task<byte[]> ExportExcelAsync(
        MaterialImpulsoQueryDto query,
        string imageBaseUrl,
        CancellationToken cancellationToken);
    Task<MaterialImpulsoAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<MaterialImpulsoAdminDto> CreateAsync(
        CreateMaterialImpulsoDto dto,
        CancellationToken cancellationToken);
    Task<MaterialImpulsoAdminDto> UpdateAsync(
        long id,
        UpdateMaterialImpulsoDto dto,
        CancellationToken cancellationToken);
    Task DeleteAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<MaterialImpulsoTiendaDto>> GetByTiendaAsync(
        string tiendaCadenaKey,
        CancellationToken cancellationToken);
    Task<FotoMaterialImpulsoDto> SavePhotoAsync(
        long materialImpulsoTiendaId,
        IFormFile foto,
        CancellationToken cancellationToken);
    Task<FotoMaterialContenidoDto> GetPhotoAsync(long fotoId, CancellationToken cancellationToken);
}

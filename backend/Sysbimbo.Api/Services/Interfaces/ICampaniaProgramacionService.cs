using Sysbimbo.Api.DTOs.Campanias;

namespace Sysbimbo.Api.Services.Interfaces;

public interface ICampaniaProgramacionService
{
    Task<CampaniaResumenDto> GetResumenAsync(int campaniaId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CampaniaTiendaDto>> GetTiendasAsync(int campaniaId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CampaniaFechaDto>> GetFechasAsync(int campaniaId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CampaniaSkuDto>> GetSkusAsync(int campaniaId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CampaniaProgramacionDto>> GetProgramacionesAsync(int campaniaId, CancellationToken cancellationToken);
    Task<CampaniaOperacionResultadoDto> AddTiendasAsync(
        int campaniaId,
        AddCampaniaTiendasRequestDto request,
        CancellationToken cancellationToken);
    Task<CampaniaOperacionResultadoDto> RemoveTiendaAsync(
        int campaniaId,
        string tiendaCadenaKey,
        CancellationToken cancellationToken);
    Task<CampaniaOperacionResultadoDto> AddFechasAsync(
        int campaniaId,
        AddCampaniaFechasRequestDto request,
        CancellationToken cancellationToken);
    Task<CampaniaOperacionResultadoDto> RemoveFechaAsync(
        int campaniaId,
        DateOnly fecha,
        CancellationToken cancellationToken);
    Task<CampaniaOperacionResultadoDto> AddSkusAsync(
        int campaniaId,
        AddCampaniaSkusRequestDto request,
        CancellationToken cancellationToken);
    Task<CampaniaOperacionResultadoDto> RemoveSkuAsync(
        int campaniaId,
        string codigoSkuBimbo,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CampaniaProgramacionDetalleDto>> GetDetallesAsync(
        int campaniaId,
        long programacionId,
        CancellationToken cancellationToken);
}

using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.Tiendas;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Repositories.Interfaces;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Services;

public class TiendaService(ITiendaRepository tiendaRepository) : ITiendaService
{
    public async Task<PagedResultDto<TiendaDto>> GetAllAsync(TiendaQueryDto query, CancellationToken cancellationToken)
    {
        var result = await tiendaRepository.GetAllAsync(new TiendaFilter
        {
            Cadena = query.Cadena,
            Marca = query.Marca,
            Region = query.Region,
            Nombre = query.Nombre,
            CodigoTiendaB2B = query.CodigoTiendaB2B,
            SoloConMaterialActivo = query.SoloConMaterialActivo,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        }, cancellationToken);

        return new PagedResultDto<TiendaDto>
        {
            Items = result.Items.Select(MapToDto).ToArray(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };
    }

    public async Task<TiendaDto> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var tienda = await tiendaRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la tienda con id {id}.");

        return MapToDto(tienda);
    }

    public async Task<TiendaDto> CreateAsync(CreateTiendaDto dto, CancellationToken cancellationToken)
    {
        if (await tiendaRepository.ExistsAsync(dto.TiendaCadenaKey, cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe una tienda con la clave {dto.TiendaCadenaKey}.");
        }

        var entity = new DimTiendaMaestraExport
        {
            TiendaCadenaKey = dto.TiendaCadenaKey,
            CodigoTiendaB2BPrefijo = dto.CodigoTiendaB2BPrefijo,
            CodigoTiendaB2B = dto.CodigoTiendaB2B,
            NombreTienda = dto.NombreTienda,
            NombreTiendaBimbo = dto.NombreTiendaBimbo,
            Canal = dto.Canal,
            Cadena = dto.Cadena,
            Formato = dto.Formato,
            TipoLocal = dto.TipoLocal,
            LimaProvincias = dto.LimaProvincias,
            Region = dto.Region,
            Provincia = dto.Provincia,
            Ruta = dto.Ruta,
            Supervisor = dto.Supervisor,
            Gestor = dto.Gestor,
            Vendedor = dto.Vendedor,
            UltimaFecha = dto.UltimaFecha,
            CantidadRegistros = dto.CantidadRegistros,
            FuenteTienda = dto.FuenteTienda
        };

        await tiendaRepository.AddAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task<TiendaDto> UpdateAsync(string id, UpdateTiendaDto dto, CancellationToken cancellationToken)
    {
        var existing = await tiendaRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la tienda con id {id}.");

        existing.CodigoTiendaB2BPrefijo = dto.CodigoTiendaB2BPrefijo;
        existing.CodigoTiendaB2B = dto.CodigoTiendaB2B;
        existing.NombreTienda = dto.NombreTienda;
        existing.NombreTiendaBimbo = dto.NombreTiendaBimbo;
        existing.Canal = dto.Canal;
        existing.Cadena = dto.Cadena;
        existing.Formato = dto.Formato;
        existing.TipoLocal = dto.TipoLocal;
        existing.LimaProvincias = dto.LimaProvincias;
        existing.Region = dto.Region;
        existing.Provincia = dto.Provincia;
        existing.Ruta = dto.Ruta;
        existing.Supervisor = dto.Supervisor;
        existing.Gestor = dto.Gestor;
        existing.Vendedor = dto.Vendedor;
        existing.UltimaFecha = dto.UltimaFecha;
        existing.CantidadRegistros = dto.CantidadRegistros;
        existing.FuenteTienda = dto.FuenteTienda;

        await tiendaRepository.UpdateAsync(cancellationToken);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var existing = await tiendaRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la tienda con id {id}.");

        await tiendaRepository.DeleteAsync(existing, cancellationToken);
    }

    private static TiendaDto MapToDto(DimTiendaMaestraExport entity) =>
        new()
        {
            TiendaCadenaKey = entity.TiendaCadenaKey,
            CodigoTiendaB2BPrefijo = entity.CodigoTiendaB2BPrefijo,
            CodigoTiendaB2B = entity.CodigoTiendaB2B,
            NombreTienda = entity.NombreTienda,
            NombreTiendaBimbo = entity.NombreTiendaBimbo,
            Canal = entity.Canal,
            Cadena = entity.Cadena,
            Formato = entity.Formato,
            TipoLocal = entity.TipoLocal,
            LimaProvincias = entity.LimaProvincias,
            Region = entity.Region,
            Provincia = entity.Provincia,
            Ruta = entity.Ruta,
            Supervisor = entity.Supervisor,
            Gestor = entity.Gestor,
            Vendedor = entity.Vendedor,
            UltimaFecha = entity.UltimaFecha,
            CantidadRegistros = entity.CantidadRegistros,
            FuenteTienda = entity.FuenteTienda
        };
}

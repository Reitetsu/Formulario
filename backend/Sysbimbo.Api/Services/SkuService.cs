using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.Skus;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Filters;
using Sysbimbo.Api.Repositories.Interfaces;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Services;

public class SkuService(ISkuRepository skuRepository) : ISkuService
{
    public async Task<PagedResultDto<SkuDto>> GetAllAsync(SkuQueryDto query, CancellationToken cancellationToken)
    {
        var result = await skuRepository.GetAllAsync(new SkuFilter
        {
            Categoria = query.Categoria,
            Marca = query.Marca,
            Nombre = query.Nombre,
            CodigoSkuB2B = query.CodigoSkuB2B,
            CodigoSkuBimbo = query.CodigoSkuBimbo,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        }, cancellationToken);

        return new PagedResultDto<SkuDto>
        {
            Items = result.Items.Select(MapToDto).ToArray(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };
    }

    public async Task<PagedResultDto<SkuCatalogoDto>> GetCatalogoAsync(SkuQueryDto query, CancellationToken cancellationToken)
    {
        var result = await skuRepository.GetCatalogoAsync(new SkuFilter
        {
            Categoria = query.Categoria,
            Marca = query.Marca,
            Nombre = query.Nombre,
            CodigoSkuB2B = query.CodigoSkuB2B,
            CodigoSkuBimbo = query.CodigoSkuBimbo,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        }, cancellationToken);

        return new PagedResultDto<SkuCatalogoDto>
        {
            Items = result.Items.ToArray(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };
    }

    public async Task<SkuDto> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var sku = await skuRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro el SKU con id {id}.");

        return MapToDto(sku);
    }

    public async Task<SkuDto> CreateAsync(CreateSkuDto dto, CancellationToken cancellationToken)
    {
        if (await skuRepository.ExistsAsync(dto.SkuKey, cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe un SKU con la clave {dto.SkuKey}.");
        }

        var entity = new DimSkuMaestraExport
        {
            SkuKey = dto.SkuKey,
            CodigoSkuB2B = dto.CodigoSkuB2B,
            NombreSkuB2B = dto.NombreSkuB2B,
            CodigoSkuBimbo = dto.CodigoSkuBimbo,
            NombreSkuBimbo = dto.NombreSkuBimbo,
            UnidadNegocio = dto.UnidadNegocio,
            Area = dto.Area,
            Categoria = dto.Categoria,
            Marca = dto.Marca,
            TipoProducto = dto.TipoProducto,
            Status = dto.Status,
            Gramaje = dto.Gramaje,
            UltimaFecha = dto.UltimaFecha,
            CantidadRegistros = dto.CantidadRegistros,
            FuenteSku = dto.FuenteSku
        };

        await skuRepository.AddAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task<SkuDto> UpdateAsync(string id, UpdateSkuDto dto, CancellationToken cancellationToken)
    {
        var existing = await skuRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro el SKU con id {id}.");

        existing.CodigoSkuB2B = dto.CodigoSkuB2B;
        existing.NombreSkuB2B = dto.NombreSkuB2B;
        existing.CodigoSkuBimbo = dto.CodigoSkuBimbo;
        existing.NombreSkuBimbo = dto.NombreSkuBimbo;
        existing.UnidadNegocio = dto.UnidadNegocio;
        existing.Area = dto.Area;
        existing.Categoria = dto.Categoria;
        existing.Marca = dto.Marca;
        existing.TipoProducto = dto.TipoProducto;
        existing.Status = dto.Status;
        existing.Gramaje = dto.Gramaje;
        existing.UltimaFecha = dto.UltimaFecha;
        existing.CantidadRegistros = dto.CantidadRegistros;
        existing.FuenteSku = dto.FuenteSku;

        await skuRepository.UpdateAsync(cancellationToken);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var existing = await skuRepository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro el SKU con id {id}.");

        await skuRepository.DeleteAsync(existing, cancellationToken);
    }

    private static SkuDto MapToDto(DimSkuMaestraExport entity) =>
        new()
        {
            SkuKey = entity.SkuKey,
            CodigoSkuB2B = entity.CodigoSkuB2B,
            NombreSkuB2B = entity.NombreSkuB2B,
            CodigoSkuBimbo = entity.CodigoSkuBimbo,
            NombreSkuBimbo = entity.NombreSkuBimbo,
            UnidadNegocio = entity.UnidadNegocio,
            Area = entity.Area,
            Categoria = entity.Categoria,
            Marca = entity.Marca,
            TipoProducto = entity.TipoProducto,
            Status = entity.Status,
            Gramaje = entity.Gramaje,
            UltimaFecha = entity.UltimaFecha,
            CantidadRegistros = entity.CantidadRegistros,
            FuenteSku = entity.FuenteSku
        };
}

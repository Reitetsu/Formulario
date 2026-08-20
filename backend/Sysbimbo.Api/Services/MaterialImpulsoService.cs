using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.DTOs.Common;
using Sysbimbo.Api.DTOs.MaterialesImpulso;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Services;

public class MaterialImpulsoService(FormularioDbContext dbContext, TimeProvider timeProvider)
    : IMaterialImpulsoService
{
    private const long MaxPhotoSize = 10 * 1024 * 1024;
    private static readonly TimeZoneInfo BusinessTimeZone = ResolveBusinessTimeZone();

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/heic",
        "image/heif"
    };

    public async Task<PagedResultDto<MaterialImpulsoAdminDto>> GetAllAsync(
        MaterialImpulsoQueryDto query,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var resultQuery = BuildFilteredAdminQuery(query);

        var totalCount = await resultQuery.CountAsync(cancellationToken);
        var items = await resultQuery
            .OrderBy(x => x.Formato)
            .ThenBy(x => x.NombreTienda)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResultDto<MaterialImpulsoAdminDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<byte[]> ExportExcelAsync(
        MaterialImpulsoQueryDto query,
        string imageBaseUrl,
        CancellationToken cancellationToken)
    {
        var filteredMaterials = BuildFilteredAdminQuery(query);
        var materials = await filteredMaterials
            .OrderBy(x => x.Formato)
            .ThenBy(x => x.NombreTienda)
            .ToArrayAsync(cancellationToken);

        var evidences = await (
            from foto in dbContext.FotosMaterialImpulso.AsNoTracking()
            join material in filteredMaterials
                on foto.MaterialImpulsoTiendaId equals material.MaterialImpulsoTiendaId
            orderby foto.FechaCaptura descending
            select new EvidenceExcelRow
            {
                FotoId = foto.FotoMaterialImpulsoId,
                Formato = material.Formato,
                Tienda = material.NombreTienda,
                TiendaKey = material.TiendaCadenaKey,
                Material = material.NombreMaterial,
                CuotaDiaria = material.CuotaDiaria,
                NombreArchivo = foto.NombreArchivo,
                TipoContenido = foto.TipoContenido,
                TamanoBytes = foto.TamanoBytes,
                FechaCaptura = foto.FechaCaptura
            }).ToArrayAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        BuildMaterialsWorksheet(workbook, materials);
        BuildEvidenceWorksheet(workbook, evidences, imageBaseUrl);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<MaterialImpulsoAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await BuildAdminQuery()
            .SingleOrDefaultAsync(x => x.MaterialImpulsoTiendaId == id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro el material con id {id}.");
    }

    public async Task<MaterialImpulsoAdminDto> CreateAsync(
        CreateMaterialImpulsoDto dto,
        CancellationToken cancellationToken)
    {
        var tiendaKey = NormalizeStoreKey(dto.TiendaCadenaKey);
        var materialName = NormalizeRequired(dto.NombreMaterial, "Debes indicar el nombre del material.");

        var storeExists = await dbContext.Tiendas
            .AsNoTracking()
            .AnyAsync(x => x.TiendaCadenaKey == tiendaKey, cancellationToken);
        if (!storeExists)
        {
            throw new KeyNotFoundException("La tienda seleccionada no existe.");
        }

        var activeExists = await dbContext.MaterialesImpulsoTienda
            .AnyAsync(
                x => x.TiendaCadenaKey == tiendaKey &&
                     x.NombreMaterial == materialName &&
                     x.Activo,
                cancellationToken);
        if (activeExists)
        {
            throw new InvalidOperationException("La tienda ya tiene activo un material con el mismo nombre.");
        }

        var entity = new MaterialImpulsoTienda
        {
            TiendaCadenaKey = tiendaKey,
            NombreMaterial = materialName,
            Descripcion = NormalizeOptional(dto.Descripcion),
            CuotaDiaria = dto.CuotaDiaria,
            Activo = true,
            FechaCreacion = timeProvider.GetUtcNow().UtcDateTime
        };

        await dbContext.MaterialesImpulsoTienda.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.MaterialImpulsoTiendaId, cancellationToken);
    }

    public async Task<MaterialImpulsoAdminDto> UpdateAsync(
        long id,
        UpdateMaterialImpulsoDto dto,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.MaterialesImpulsoTienda
            .FirstOrDefaultAsync(x => x.MaterialImpulsoTiendaId == id && x.Activo, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro el material activo con id {id}.");

        var materialName = NormalizeRequired(dto.NombreMaterial, "Debes indicar el nombre del material.");
        var duplicateExists = await dbContext.MaterialesImpulsoTienda.AnyAsync(
            x => x.MaterialImpulsoTiendaId != id &&
                 x.TiendaCadenaKey == entity.TiendaCadenaKey &&
                 x.NombreMaterial == materialName &&
                 x.Activo,
            cancellationToken);
        if (duplicateExists)
        {
            throw new InvalidOperationException("La tienda ya tiene activo un material con el mismo nombre.");
        }

        entity.NombreMaterial = materialName;
        entity.Descripcion = NormalizeOptional(dto.Descripcion);
        entity.CuotaDiaria = dto.CuotaDiaria;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.MaterialesImpulsoTienda
            .FirstOrDefaultAsync(x => x.MaterialImpulsoTiendaId == id && x.Activo, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro el material activo con id {id}.");

        entity.Activo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MaterialImpulsoTiendaDto>> GetByTiendaAsync(
        string tiendaCadenaKey,
        CancellationToken cancellationToken)
    {
        var key = NormalizeStoreKey(tiendaCadenaKey);
        var (dayStartUtc, dayEndUtc) = GetCurrentBusinessDayUtcRange();
        var businessDate = GetCurrentBusinessDate();

        return await dbContext.MaterialesImpulsoTienda
            .AsNoTracking()
            .Where(x => x.TiendaCadenaKey == key && x.Activo)
            .OrderBy(x => x.NombreMaterial)
            .Select(x => new MaterialImpulsoTiendaDto
            {
                MaterialImpulsoTiendaId = x.MaterialImpulsoTiendaId,
                TiendaCadenaKey = x.TiendaCadenaKey,
                NombreMaterial = x.NombreMaterial,
                Descripcion = x.Descripcion,
                CuotaDiaria = x.CuotaDiaria,
                Acumulado = x.Fotos.Count(foto =>
                    foto.FechaCaptura >= dayStartUtc &&
                    foto.FechaCaptura < dayEndUtc),
                CanjesHoy = x.CanjesDiarios
                    .Where(canje => canje.Fecha == businessDate)
                    .Select(canje => (int?)canje.Cantidad)
                    .FirstOrDefault() ?? 0
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<CanjesDiariosDto> UpdateDailyExchangesAsync(
        long materialImpulsoTiendaId,
        int cantidad,
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        if (cantidad is < 0 or > 1_000_000)
        {
            throw new InvalidOperationException("La cantidad debe estar entre 0 y 1000000.");
        }

        var material = await dbContext.MaterialesImpulsoTienda
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.MaterialImpulsoTiendaId == materialImpulsoTiendaId && item.Activo,
                cancellationToken)
            ?? throw new KeyNotFoundException("No se encontro el material activo de la tienda.");

        var businessDate = GetCurrentBusinessDate();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var entry = await dbContext.CanjesMaterialDiarios
            .SingleOrDefaultAsync(
                item => item.MaterialImpulsoTiendaId == materialImpulsoTiendaId &&
                        item.Fecha == businessDate,
                cancellationToken);

        if (entry is null)
        {
            entry = new CanjeMaterialDiario
            {
                MaterialImpulsoTiendaId = material.MaterialImpulsoTiendaId,
                TiendaCadenaKey = material.TiendaCadenaKey,
                Fecha = businessDate,
                Cantidad = cantidad,
                FormaIngreso = "MANUAL",
                RegistradoPorUsuarioId = usuarioId,
                FechaCreacion = now,
                FechaActualizacion = now
            };
            dbContext.CanjesMaterialDiarios.Add(entry);
        }
        else
        {
            entry.Cantidad = cantidad;
            entry.FormaIngreso = "MANUAL";
            entry.ActualizadoPorUsuarioId = usuarioId;
            entry.FechaActualizacion = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new CanjesDiariosDto
        {
            CanjeMaterialDiarioId = entry.CanjeMaterialDiarioId,
            MaterialImpulsoTiendaId = entry.MaterialImpulsoTiendaId,
            TiendaCadenaKey = entry.TiendaCadenaKey,
            Fecha = entry.Fecha,
            Cantidad = entry.Cantidad,
            FormaIngreso = entry.FormaIngreso,
            RegistradoPorUsuarioId = entry.RegistradoPorUsuarioId,
            ActualizadoPorUsuarioId = entry.ActualizadoPorUsuarioId,
            FechaCreacion = entry.FechaCreacion,
            FechaActualizacion = entry.FechaActualizacion
        };
    }

    public async Task<FotoMaterialImpulsoDto> SavePhotoAsync(
        long materialImpulsoTiendaId,
        IFormFile foto,
        CancellationToken cancellationToken)
    {
        if (foto.Length == 0)
        {
            throw new InvalidOperationException("La fotografia esta vacia.");
        }

        if (foto.Length > MaxPhotoSize)
        {
            throw new InvalidOperationException("La fotografia no puede superar los 10 MB.");
        }

        if (!AllowedContentTypes.Contains(foto.ContentType))
        {
            throw new InvalidOperationException("El archivo debe ser una imagen JPG, PNG, WEBP o HEIC.");
        }

        var material = await dbContext.MaterialesImpulsoTienda
            .FirstOrDefaultAsync(
                x => x.MaterialImpulsoTiendaId == materialImpulsoTiendaId && x.Activo,
                cancellationToken)
            ?? throw new KeyNotFoundException("No se encontro el material activo de la tienda.");

        await using var memory = new MemoryStream();
        await foto.CopyToAsync(memory, cancellationToken);

        var entity = new FotoMaterialImpulso
        {
            MaterialImpulsoTiendaId = material.MaterialImpulsoTiendaId,
            TiendaCadenaKey = material.TiendaCadenaKey,
            NombreArchivo = Path.GetFileName(foto.FileName),
            TipoContenido = foto.ContentType,
            TamanoBytes = foto.Length,
            Contenido = memory.ToArray(),
            FechaCaptura = timeProvider.GetUtcNow().UtcDateTime
        };

        await dbContext.FotosMaterialImpulso.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var (dayStartUtc, dayEndUtc) = GetCurrentBusinessDayUtcRange();
        var acumulado = await dbContext.FotosMaterialImpulso
            .CountAsync(
                x => x.MaterialImpulsoTiendaId == material.MaterialImpulsoTiendaId &&
                     x.FechaCaptura >= dayStartUtc &&
                     x.FechaCaptura < dayEndUtc,
                cancellationToken);

        return new FotoMaterialImpulsoDto
        {
            FotoMaterialImpulsoId = entity.FotoMaterialImpulsoId,
            MaterialImpulsoTiendaId = entity.MaterialImpulsoTiendaId,
            TiendaCadenaKey = entity.TiendaCadenaKey,
            NombreArchivo = entity.NombreArchivo,
            FechaCaptura = entity.FechaCaptura,
            Acumulado = acumulado
        };
    }

    public async Task<FotoMaterialContenidoDto> GetPhotoAsync(
        long fotoId,
        CancellationToken cancellationToken)
    {
        return await dbContext.FotosMaterialImpulso
            .AsNoTracking()
            .Where(x => x.FotoMaterialImpulsoId == fotoId)
            .Select(x => new FotoMaterialContenidoDto
            {
                Contenido = x.Contenido,
                TipoContenido = x.TipoContenido,
                NombreArchivo = x.NombreArchivo
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la evidencia con id {fotoId}.");
    }

    public async Task<IReadOnlyList<FotoMaterialResumenDto>> GetPhotosAsync(
        long materialImpulsoTiendaId,
        bool soloHoy,
        CancellationToken cancellationToken)
    {
        var materialExists = await dbContext.MaterialesImpulsoTienda
            .AsNoTracking()
            .AnyAsync(
                material => material.MaterialImpulsoTiendaId == materialImpulsoTiendaId,
                cancellationToken);

        if (!materialExists)
        {
            throw new KeyNotFoundException(
                $"No se encontro el material con id {materialImpulsoTiendaId}.");
        }

        var query = dbContext.FotosMaterialImpulso
            .AsNoTracking()
            .Where(photo => photo.MaterialImpulsoTiendaId == materialImpulsoTiendaId);
        if (soloHoy)
        {
            var (dayStartUtc, dayEndUtc) = GetCurrentBusinessDayUtcRange();
            query = query.Where(photo =>
                photo.FechaCaptura >= dayStartUtc && photo.FechaCaptura < dayEndUtc);
        }

        return await query
            .OrderByDescending(photo => photo.FechaCaptura)
            .Select(photo => new FotoMaterialResumenDto
            {
                FotoMaterialImpulsoId = photo.FotoMaterialImpulsoId,
                MaterialImpulsoTiendaId = photo.MaterialImpulsoTiendaId,
                NombreArchivo = photo.NombreArchivo,
                TipoContenido = photo.TipoContenido,
                TamanoBytes = photo.TamanoBytes,
                FechaCaptura = photo.FechaCaptura
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task DeletePhotoAsync(
        long materialImpulsoTiendaId,
        long fotoId,
        CancellationToken cancellationToken)
    {
        var photo = await dbContext.FotosMaterialImpulso
            .SingleOrDefaultAsync(
                item => item.MaterialImpulsoTiendaId == materialImpulsoTiendaId &&
                        item.FotoMaterialImpulsoId == fotoId,
                cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontro la evidencia con id {fotoId}.");

        dbContext.FotosMaterialImpulso.Remove(photo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeStoreKey(string tiendaCadenaKey)
    {
        var key = tiendaCadenaKey.Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Debes indicar una tienda.");
        }

        return key;
    }

    private (DateTime DayStartUtc, DateTime DayEndUtc) GetCurrentBusinessDayUtcRange()
    {
        var businessDate = GetCurrentBusinessDate();
        var localDayStart = businessDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var localDayEnd = businessDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        return (
            TimeZoneInfo.ConvertTimeToUtc(localDayStart, BusinessTimeZone),
            TimeZoneInfo.ConvertTimeToUtc(localDayEnd, BusinessTimeZone));
    }

    private DateOnly GetCurrentBusinessDate()
    {
        var businessNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), BusinessTimeZone);
        return DateOnly.FromDateTime(businessNow.DateTime);
    }

    private static TimeZoneInfo ResolveBusinessTimeZone()
    {
        foreach (var timeZoneId in new[] { "America/Lima", "SA Pacific Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Se intenta el identificador equivalente del siguiente sistema operativo.
            }
            catch (InvalidTimeZoneException)
            {
                // Se intenta el identificador equivalente del siguiente sistema operativo.
            }
        }

        throw new InvalidOperationException("No fue posible configurar la zona horaria America/Lima.");
    }

    private IQueryable<MaterialImpulsoAdminDto> BuildAdminQuery() =>
        from material in dbContext.MaterialesImpulsoTienda.AsNoTracking()
        join tienda in dbContext.Tiendas.AsNoTracking()
            on material.TiendaCadenaKey equals tienda.TiendaCadenaKey
        select new MaterialImpulsoAdminDto
        {
            MaterialImpulsoTiendaId = material.MaterialImpulsoTiendaId,
            TiendaCadenaKey = material.TiendaCadenaKey,
            NombreTienda = tienda.NombreTiendaBimbo ?? tienda.NombreTienda ?? material.TiendaCadenaKey,
            Formato = tienda.Formato,
            NombreMaterial = material.NombreMaterial,
            Descripcion = material.Descripcion,
            CuotaDiaria = material.CuotaDiaria,
            Acumulado = material.Fotos.Count,
            Activo = material.Activo,
            FechaCreacion = material.FechaCreacion
        };

    private IQueryable<MaterialImpulsoAdminDto> BuildFilteredAdminQuery(MaterialImpulsoQueryDto query)
    {
        var result = BuildAdminQuery();

        if (query.SoloActivos)
        {
            result = result.Where(x => x.Activo);
        }

        if (!string.IsNullOrWhiteSpace(query.Material))
        {
            var material = query.Material.Trim();
            result = result.Where(x => EF.Functions.ILike(x.NombreMaterial, $"%{material}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Tienda))
        {
            var tienda = query.Tienda.Trim();
            result = result.Where(x => EF.Functions.ILike(x.NombreTienda, $"%{tienda}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Marca))
        {
            var marca = query.Marca.Trim();
            result = result.Where(x => x.Formato != null && EF.Functions.ILike(x.Formato, $"%{marca}%"));
        }

        return result;
    }

    private static void BuildMaterialsWorksheet(
        XLWorkbook workbook,
        IReadOnlyList<MaterialImpulsoAdminDto> materials)
    {
        var worksheet = workbook.Worksheets.Add("Materiales");
        var headers = new[]
        {
            "ID", "Marca / Formato", "Tienda", "Codigo de tienda", "Material",
            "Descripcion", "Objetivo diario", "Objetivo fin de semana", "Evidencias",
            "Estado", "Fecha de asignacion (UTC)"
        };
        WriteHeaders(worksheet, headers);

        for (var index = 0; index < materials.Count; index++)
        {
            var item = materials[index];
            var row = index + 2;
            worksheet.Cell(row, 1).Value = item.MaterialImpulsoTiendaId;
            worksheet.Cell(row, 2).Value = item.Formato ?? string.Empty;
            worksheet.Cell(row, 3).Value = item.NombreTienda;
            worksheet.Cell(row, 4).Value = item.TiendaCadenaKey;
            worksheet.Cell(row, 5).Value = item.NombreMaterial;
            worksheet.Cell(row, 6).Value = item.Descripcion ?? string.Empty;
            worksheet.Cell(row, 7).Value = item.CuotaDiaria;
            worksheet.Cell(row, 8).Value = item.CuotaDiaria * 2;
            worksheet.Cell(row, 9).Value = item.Acumulado;
            worksheet.Cell(row, 10).Value = item.Activo ? "Activo" : "Inactivo";
            worksheet.Cell(row, 11).Value = item.FechaCreacion;
        }

        worksheet.Column(11).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
        FinishWorksheet(worksheet, headers.Length, materials.Count + 1);
    }

    private static void BuildEvidenceWorksheet(
        XLWorkbook workbook,
        IReadOnlyList<EvidenceExcelRow> evidences,
        string imageBaseUrl)
    {
        var worksheet = workbook.Worksheets.Add("Evidencias");
        var headers = new[]
        {
            "ID evidencia", "Marca / Formato", "Tienda", "Codigo de tienda", "Material",
            "Objetivo diario", "Objetivo fin de semana", "Nombre de archivo", "Tipo de imagen",
            "Tamano (KB)", "Fecha de captura (UTC)", "Imagen referencial"
        };
        WriteHeaders(worksheet, headers);

        for (var index = 0; index < evidences.Count; index++)
        {
            var item = evidences[index];
            var row = index + 2;
            worksheet.Cell(row, 1).Value = item.FotoId;
            worksheet.Cell(row, 2).Value = item.Formato ?? string.Empty;
            worksheet.Cell(row, 3).Value = item.Tienda;
            worksheet.Cell(row, 4).Value = item.TiendaKey;
            worksheet.Cell(row, 5).Value = item.Material;
            worksheet.Cell(row, 6).Value = item.CuotaDiaria;
            worksheet.Cell(row, 7).Value = item.CuotaDiaria * 2;
            worksheet.Cell(row, 8).Value = item.NombreArchivo;
            worksheet.Cell(row, 9).Value = item.TipoContenido;
            worksheet.Cell(row, 10).Value = Math.Round(item.TamanoBytes / 1024d, 2);
            worksheet.Cell(row, 11).Value = item.FechaCaptura;
            var imageUrl = $"{imageBaseUrl.TrimEnd('/')}/{item.FotoId}";
            var imageCell = worksheet.Cell(row, 12);
            imageCell.Value = "Ver imagen";
            imageCell.SetHyperlink(new XLHyperlink(imageUrl, "Abrir imagen referencial"));
        }

        worksheet.Column(11).Style.DateFormat.Format = "dd/mm/yyyy hh:mm:ss";
        FinishWorksheet(worksheet, headers.Length, evidences.Count + 1);
    }

    private static void WriteHeaders(IXLWorksheet worksheet, IReadOnlyList<string> headers)
    {
        for (var column = 1; column <= headers.Count; column++)
        {
            worksheet.Cell(1, column).Value = headers[column - 1];
        }

        var header = worksheet.Range(1, 1, 1, headers.Count);
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#22543D");
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Row(1).Height = 24;
    }

    private static void FinishWorksheet(IXLWorksheet worksheet, int columnCount, int lastRow)
    {
        worksheet.SheetView.FreezeRows(1);
        if (lastRow > 1)
        {
            worksheet.Range(1, 1, lastRow, columnCount).SetAutoFilter();
        }

        worksheet.Columns(1, columnCount).AdjustToContents();
        foreach (var column in worksheet.ColumnsUsed())
        {
            column.Width = Math.Min(column.Width + 2, 55);
        }
    }

    private sealed class EvidenceExcelRow
    {
        public long FotoId { get; init; }
        public string? Formato { get; init; }
        public string Tienda { get; init; } = string.Empty;
        public string TiendaKey { get; init; } = string.Empty;
        public string Material { get; init; } = string.Empty;
        public int CuotaDiaria { get; init; }
        public string NombreArchivo { get; init; } = string.Empty;
        public string TipoContenido { get; init; } = string.Empty;
        public long TamanoBytes { get; init; }
        public DateTime FechaCaptura { get; init; }
    }

    private static string NormalizeRequired(string value, string message)
    {
        var normalized = value.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? throw new InvalidOperationException(message) : normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}

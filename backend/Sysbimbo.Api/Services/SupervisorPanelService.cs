using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Data;
using Sysbimbo.Api.DTOs.Supervisores;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Services.Interfaces;

namespace Sysbimbo.Api.Services;

public sealed class SupervisorPanelService(
    FormularioDbContext dbContext,
    TimeProvider timeProvider) : ISupervisorPanelService
{
    private static readonly TimeZoneInfo BusinessTimeZone = ResolveBusinessTimeZone();

    public async Task<SupervisorPanelDto> GetAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var businessDate = GetCurrentBusinessDate();
        var (dayStartUtc, dayEndUtc) = GetBusinessDayUtcRange(businessDate);
        var assignedStoreKeys = await dbContext.UsuariosTiendas
            .AsNoTracking()
            .Where(item => item.UsuarioId == usuarioId &&
                           item.Activo &&
                           item.FechaInicio <= businessDate &&
                           (item.FechaFin == null || item.FechaFin >= businessDate))
            .Select(item => item.TiendaCadenaKey)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var stores = await dbContext.Tiendas
            .AsNoTracking()
            .Where(store => assignedStoreKeys.Contains(store.TiendaCadenaKey))
            .OrderBy(store => store.Formato)
            .ThenBy(store => store.NombreTiendaBimbo ?? store.NombreTienda)
            .Select(store => new
            {
                store.TiendaCadenaKey,
                Nombre = store.NombreTiendaBimbo ?? store.NombreTienda ?? store.TiendaCadenaKey,
                store.Formato
            })
            .ToArrayAsync(cancellationToken);

        var materials = await dbContext.MaterialesImpulsoTienda
            .AsNoTracking()
            .Where(material => assignedStoreKeys.Contains(material.TiendaCadenaKey) && material.Activo)
            .OrderBy(material => material.NombreMaterial)
            .Select(material => new SupervisorMaterialRow
            {
                TiendaCadenaKey = material.TiendaCadenaKey,
                MaterialImpulsoTiendaId = material.MaterialImpulsoTiendaId,
                NombreMaterial = material.NombreMaterial,
                CuotaDiaria = material.CuotaDiaria,
                CanjesHoy = material.CanjesDiarios
                    .Where(canje => canje.Fecha == businessDate)
                    .Select(canje => (int?)canje.Cantidad)
                    .FirstOrDefault() ?? 0,
                EvidenciasHoy = material.Fotos.Count(photo =>
                    photo.FechaCaptura >= dayStartUtc && photo.FechaCaptura < dayEndUtc)
            })
            .ToArrayAsync(cancellationToken);

        var attendance = await dbContext.JornadasUsuarios
            .AsNoTracking()
            .Where(item => item.UsuarioId == usuarioId &&
                           item.FormularioId == FormularioSeedCatalog.ControlMaterialFormularioId &&
                           item.FechaJornada == businessDate)
            .Select(item => new SupervisorAttendanceDto
            {
                JornadaUsuarioId = item.JornadaUsuarioId,
                Fecha = item.FechaJornada,
                HoraIngreso = item.HoraIngreso,
                HoraSalida = item.HoraSalida,
                Estado = item.Estado,
                TipoCierre = item.TipoCierre
            })
            .SingleOrDefaultAsync(cancellationToken);

        var storeDtos = stores.Select(store =>
        {
            var storeMaterials = materials
                .Where(material => material.TiendaCadenaKey == store.TiendaCadenaKey)
                .Select(material => new SupervisorMaterialDto
                {
                    MaterialImpulsoTiendaId = material.MaterialImpulsoTiendaId,
                    NombreMaterial = material.NombreMaterial,
                    CuotaDiaria = material.CuotaDiaria,
                    CanjesHoy = material.CanjesHoy,
                    EvidenciasHoy = material.EvidenciasHoy
                })
                .ToArray();

            return new SupervisorStoreDto
            {
                TiendaCadenaKey = store.TiendaCadenaKey,
                NombreTienda = store.Nombre,
                Formato = store.Formato,
                TotalCanjesHoy = storeMaterials.Sum(material => material.CanjesHoy),
                Materiales = storeMaterials
            };
        }).ToArray();

        return new SupervisorPanelDto
        {
            Fecha = businessDate,
            Asistencia = attendance,
            Tiendas = storeDtos
        };
    }

    public async Task<SupervisorAttendanceDto> UpdateAttendanceAsync(
        Guid usuarioId,
        UpdateSupervisorAttendanceDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.HoraSalida is not null && dto.HoraSalida < dto.HoraIngreso)
        {
            throw new InvalidOperationException("La hora de salida no puede ser anterior al ingreso.");
        }

        var businessDate = GetCurrentBusinessDate();
        var attendance = await dbContext.JornadasUsuarios.SingleOrDefaultAsync(
            item => item.UsuarioId == usuarioId &&
                    item.FormularioId == FormularioSeedCatalog.ControlMaterialFormularioId &&
                    item.FechaJornada == businessDate,
            cancellationToken);
        if (attendance is null)
        {
            attendance = new JornadaUsuario
            {
                UsuarioId = usuarioId,
                ClienteId = FormularioSeedCatalog.BimboClienteId,
                FormularioId = FormularioSeedCatalog.ControlMaterialFormularioId,
                FechaJornada = businessDate
            };
            dbContext.JornadasUsuarios.Add(attendance);
        }

        attendance.HoraIngreso = ToUtc(businessDate, dto.HoraIngreso);
        attendance.HoraSalida = dto.HoraSalida is null
            ? null
            : ToUtc(businessDate, dto.HoraSalida.Value);
        attendance.Estado = attendance.HoraSalida is null ? "ABIERTA" : "CERRADA";
        attendance.TipoCierre = attendance.HoraSalida is null ? null : "AJUSTE_MANUAL";
        await dbContext.SaveChangesAsync(cancellationToken);

        return new SupervisorAttendanceDto
        {
            JornadaUsuarioId = attendance.JornadaUsuarioId,
            Fecha = attendance.FechaJornada,
            HoraIngreso = attendance.HoraIngreso,
            HoraSalida = attendance.HoraSalida,
            Estado = attendance.Estado,
            TipoCierre = attendance.TipoCierre
        };
    }

    private DateOnly GetCurrentBusinessDate()
    {
        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), BusinessTimeZone);
        return DateOnly.FromDateTime(localNow.DateTime);
    }

    private static (DateTime StartUtc, DateTime EndUtc) GetBusinessDayUtcRange(DateOnly date) =>
        (ToUtc(date, TimeOnly.MinValue), ToUtc(date.AddDays(1), TimeOnly.MinValue));

    private static DateTime ToUtc(DateOnly date, TimeOnly time)
    {
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, BusinessTimeZone);
    }

    private static TimeZoneInfo ResolveBusinessTimeZone()
    {
        foreach (var identifier in new[] { "America/Lima", "SA Pacific Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(identifier);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }

    private sealed class SupervisorMaterialRow
    {
        public string TiendaCadenaKey { get; init; } = string.Empty;
        public long MaterialImpulsoTiendaId { get; init; }
        public string NombreMaterial { get; init; } = string.Empty;
        public int CuotaDiaria { get; init; }
        public int CanjesHoy { get; init; }
        public int EvidenciasHoy { get; init; }
    }
}

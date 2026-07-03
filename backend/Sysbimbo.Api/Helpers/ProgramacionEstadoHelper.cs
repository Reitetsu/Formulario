using Sysbimbo.Api.Constants;

namespace Sysbimbo.Api.Helpers;

public static class ProgramacionEstadoHelper
{
    public static string Calcular(string? estadoPersistido, DateOnly? fecha, DateOnly fechaActual)
    {
        var estado = ProgramacionEstados.Normalizar(estadoPersistido);
        if (string.IsNullOrWhiteSpace(estado))
        {
            return "Desconocido";
        }

        return estado switch
        {
            ProgramacionEstados.Ejecutada => "Ejecutada",
            ProgramacionEstados.Cancelada => "Cancelada",
            ProgramacionEstados.Programada when fecha is null => "Programada",
            ProgramacionEstados.Programada when fecha > fechaActual => "Pendiente",
            ProgramacionEstados.Programada when fecha == fechaActual => "ProgramadaHoy",
            ProgramacionEstados.Programada when fecha < fechaActual => "NoEjecutada",
            _ => "Desconocido"
        };
    }
}

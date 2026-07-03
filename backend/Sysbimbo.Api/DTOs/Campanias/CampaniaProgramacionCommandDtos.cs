using System.ComponentModel.DataAnnotations;

namespace Sysbimbo.Api.DTOs.Campanias;

public class AddCampaniaTiendasRequestDto
{
    [Required]
    public IReadOnlyCollection<string> TiendaCadenaKeys { get; init; } = [];

    public IReadOnlyCollection<DateOnly> Fechas { get; init; } = [];

    public bool ReplicarSkusExistentes { get; init; } = true;
}

public class AddCampaniaFechasRequestDto
{
    [Required]
    public IReadOnlyCollection<DateOnly> Fechas { get; init; } = [];

    public IReadOnlyCollection<string> TiendaCadenaKeys { get; init; } = [];

    public bool AplicarATodasLasTiendas { get; init; }

    public bool ReplicarSkusExistentes { get; init; } = true;
}

public class AddCampaniaSkusRequestDto
{
    [Required]
    public IReadOnlyCollection<string> CodigosSkuBimbo { get; init; } = [];
}

public class CampaniaOperacionResultadoDto
{
    public string Mensaje { get; init; } = string.Empty;
    public int Procesados { get; init; }
    public int Creados { get; init; }
    public int Reactivados { get; init; }
    public int Actualizados { get; init; }
    public int Eliminados { get; init; }
    public int Omitidos { get; init; }
    public int DetallesCreados { get; init; }
    public int DetallesEliminados { get; init; }
    public IReadOnlyCollection<string> Advertencias { get; init; } = [];
}

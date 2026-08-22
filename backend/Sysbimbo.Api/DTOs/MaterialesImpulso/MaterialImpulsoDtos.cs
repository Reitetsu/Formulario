using System.ComponentModel.DataAnnotations;

namespace Sysbimbo.Api.DTOs.MaterialesImpulso;

public class MaterialImpulsoTiendaDto
{
    public long MaterialImpulsoTiendaId { get; init; }
    public string TiendaCadenaKey { get; init; } = string.Empty;
    public string NombreMaterial { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public int CuotaDiaria { get; init; }
    public int Acumulado { get; init; }
    public int CanjesHoy { get; init; }
}

public class UpdateCanjesDiariosDto
{
    [Range(0, 1_000_000, ErrorMessage = "La cantidad debe estar entre 0 y 1000000.")]
    public int Cantidad { get; init; }
}

public class CanjesDiariosDto
{
    public long CanjeMaterialDiarioId { get; init; }
    public long MaterialImpulsoTiendaId { get; init; }
    public string TiendaCadenaKey { get; init; } = string.Empty;
    public DateOnly Fecha { get; init; }
    public int Cantidad { get; init; }
    public string FormaIngreso { get; init; } = string.Empty;
    public Guid? RegistradoPorUsuarioId { get; init; }
    public Guid? ActualizadoPorUsuarioId { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime FechaActualizacion { get; init; }
}

public class FotoMaterialImpulsoDto
{
    public long FotoMaterialImpulsoId { get; init; }
    public long MaterialImpulsoTiendaId { get; init; }
    public string TiendaCadenaKey { get; init; } = string.Empty;
    public string NombreArchivo { get; init; } = string.Empty;
    public DateTime FechaCaptura { get; init; }
    public int Acumulado { get; init; }
    public int CanjesHoy { get; init; }
}

public class FotoMaterialContenidoDto
{
    public byte[] Contenido { get; init; } = [];
    public string TipoContenido { get; init; } = string.Empty;
    public string NombreArchivo { get; init; } = string.Empty;
}

public class FotoMaterialResumenDto
{
    public long FotoMaterialImpulsoId { get; init; }
    public long MaterialImpulsoTiendaId { get; init; }
    public string NombreArchivo { get; init; } = string.Empty;
    public string TipoContenido { get; init; } = string.Empty;
    public long TamanoBytes { get; init; }
    public DateTime FechaCaptura { get; init; }
}

public class MaterialImpulsoAdminDto : MaterialImpulsoTiendaDto
{
    public string NombreTienda { get; init; } = string.Empty;
    public string? Formato { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
}

public class MaterialImpulsoQueryDto
{
    public string? Tienda { get; init; }
    public string? Marca { get; init; }
    public string? Material { get; init; }
    public bool SoloActivos { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class CreateMaterialImpulsoDto
{
    [Required]
    public string TiendaCadenaKey { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NombreMaterial { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "La cuota diaria debe ser mayor que cero.")]
    public int CuotaDiaria { get; init; }
}

public class UpdateMaterialImpulsoDto
{
    [Required]
    [MaxLength(200)]
    public string NombreMaterial { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "La cuota diaria debe ser mayor que cero.")]
    public int CuotaDiaria { get; init; }
}

using Sysbimbo.Api.Models.Identity;

namespace Sysbimbo.Api.Models.Entities;

public class Cliente
{
    public long ClienteId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
}
public class Formulario
{
    public long FormularioId { get; set; }
    public long ClienteId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Ruta { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public ICollection<FormularioOpcion> Opciones { get; set; } = [];
}

public class FormularioOpcion
{
    public long FormularioOpcionId { get; set; }
    public long FormularioId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Habilitada { get; set; }
    public string? Configuracion { get; set; }
    public Formulario Formulario { get; set; } = null!;
}

public class ClienteTienda
{
    public long ClienteId { get; set; }
    public string TiendaCadenaKey { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaAsignacion { get; set; }
}

public class UsuarioCliente
{
    public Guid UsuarioId { get; set; }
    public long ClienteId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAsignacion { get; set; }
}

public class UsuarioClienteRol
{
    public Guid UsuarioId { get; set; }
    public long ClienteId { get; set; }
    public Guid RolId { get; set; }
    public DateTime FechaAsignacion { get; set; }
}

public class UsuarioFormulario
{
    public Guid UsuarioId { get; set; }
    public long FormularioId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAsignacion { get; set; }
}

public class UsuarioTienda
{
    public long UsuarioTiendaId { get; set; }
    public Guid UsuarioId { get; set; }
    public long ClienteId { get; set; }
    public string TiendaCadenaKey { get; set; } = string.Empty;
    public string TipoAsignacion { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public bool Activo { get; set; } = true;
}

public class SupervisorPersonal
{
    public long SupervisorPersonalId { get; set; }
    public long ClienteId { get; set; }
    public Guid SupervisorUsuarioId { get; set; }
    public Guid PersonalUsuarioId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public bool Activo { get; set; } = true;
}

public class ArchivoFormulario
{
    public long ArchivoFormularioId { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string ClaveObjeto { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string? HashSha256 { get; set; }
    public Guid? CreadoPorUsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class JornadaUsuario
{
    public long JornadaUsuarioId { get; set; }
    public Guid UsuarioId { get; set; }
    public long ClienteId { get; set; }
    public long FormularioId { get; set; }
    public string? TiendaCadenaKey { get; set; }
    public Guid? SupervisorUsuarioId { get; set; }
    public DateOnly FechaJornada { get; set; }
    public DateTime HoraIngreso { get; set; }
    public DateTime? HoraSalida { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? TipoCierre { get; set; }
    public long? FotoInicioArchivoId { get; set; }
    public string? DireccionIp { get; set; }
    public string? Dispositivo { get; set; }
}

public class FormularioRegistro
{
    public long FormularioRegistroId { get; set; }
    public long FormularioId { get; set; }
    public long ClienteId { get; set; }
    public Guid? UsuarioId { get; set; }
    public long? JornadaUsuarioId { get; set; }
    public string? TiendaCadenaKey { get; set; }
    public Guid? SupervisorUsuarioId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Datos { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
}

public class FormularioRegistroArchivo
{
    public long FormularioRegistroId { get; set; }
    public long ArchivoFormularioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Orden { get; set; }
}

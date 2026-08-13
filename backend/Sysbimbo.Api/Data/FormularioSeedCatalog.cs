using Sysbimbo.Api.Constants;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Identity;

namespace Sysbimbo.Api.Data;

internal static class FormularioSeedCatalog
{
    internal static readonly DateTime InitialDate =
        new(2026, 8, 12, 0, 0, 0, DateTimeKind.Utc);

    internal const long BimboClienteId = 1;
    internal const long ControlMaterialFormularioId = 1;
    internal const string BimboCodigo = "BIMBO";
    internal const string ControlMaterialCodigo = "CONTROL_MATERIAL_IMPULSO";

    internal static ApplicationRole[] CreateRoles() =>
    [
        new ApplicationRole
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Name = "Administrador",
            NormalizedName = "ADMINISTRADOR",
            ConcurrencyStamp = "10000000-0000-0000-0000-000000000001",
            Descripcion = "Administra clientes, usuarios, formularios y reportes.",
            Activo = true
        },
        new ApplicationRole
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            Name = "Supervisor",
            NormalizedName = "SUPERVISOR",
            ConcurrencyStamp = "10000000-0000-0000-0000-000000000002",
            Descripcion = "Supervisa personal y tiendas asignadas.",
            Activo = true
        },
        new ApplicationRole
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            Name = "Impulsadora",
            NormalizedName = "IMPULSADORA",
            ConcurrencyStamp = "10000000-0000-0000-0000-000000000003",
            Descripcion = "Registra actividades y evidencias de campo.",
            Activo = true
        }
    ];

    internal static Cliente CreateBimbo() => new()
    {
        ClienteId = BimboClienteId,
        Codigo = BimboCodigo,
        Nombre = "Bimbo",
        Activo = true,
        FechaCreacion = InitialDate
    };

    internal static Formulario CreateControlMaterial() => new()
    {
        FormularioId = ControlMaterialFormularioId,
        ClienteId = BimboClienteId,
        Codigo = ControlMaterialCodigo,
        Nombre = "Control de material",
        Descripcion = "Registro de evidencias y cumplimiento diario por material de impulso.",
        Ruta = "/habilitar-tienda",
        Orden = 1,
        Activo = true,
        FechaCreacion = InitialDate
    };

    internal static FormularioOpcion[] CreateOptions() =>
    [
        Option(1, FormularioOpciones.LoginRequerido, "Inicio de sesion", false),
        Option(2, FormularioOpciones.RolesHabilitados, "Control por roles", false),
        Option(3, FormularioOpciones.FotoInicioObligatoria, "Foto de inicio obligatoria", false),
        Option(4, FormularioOpciones.ControlTienda, "Control por tienda", true),
        Option(5, FormularioOpciones.ControlSupervisor, "Control por supervisor", false),
        Option(
            6,
            FormularioOpciones.CierreJornadaAutomatico,
            "Cierre automatico de jornada",
            true,
            "{\"hora\":\"23:59:59\",\"zonaHoraria\":\"America/Lima\"}")
    ];

    private static FormularioOpcion Option(
        long id,
        string key,
        string name,
        bool enabled,
        string? configuration = null) =>
        new()
        {
            FormularioOpcionId = id,
            FormularioId = ControlMaterialFormularioId,
            Clave = key,
            Nombre = name,
            Habilitada = enabled,
            Configuracion = configuration
        };
}

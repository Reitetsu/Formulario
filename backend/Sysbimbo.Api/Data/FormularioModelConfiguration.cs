using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Identity;

namespace Sysbimbo.Api.Data;

internal static class FormularioModelConfiguration
{
    internal static void ConfigureIdentity(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("usuarios");
            entity.Property(x => x.Id).HasColumnName("usuario_id");
            entity.Property(x => x.UserName).HasColumnName("nombre_usuario").HasMaxLength(100);
            entity.Property(x => x.NormalizedUserName).HasColumnName("nombre_usuario_normalizado").HasMaxLength(100);
            entity.Property(x => x.Email).HasColumnName("correo").HasMaxLength(200);
            entity.Property(x => x.NormalizedEmail).HasColumnName("correo_normalizado").HasMaxLength(200);
            entity.Property(x => x.EmailConfirmed).HasColumnName("correo_confirmado");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.SecurityStamp).HasColumnName("sello_seguridad");
            entity.Property(x => x.ConcurrencyStamp).HasColumnName("sello_concurrencia");
            entity.Property(x => x.PhoneNumber).HasColumnName("telefono").HasMaxLength(30);
            entity.Property(x => x.PhoneNumberConfirmed).HasColumnName("telefono_confirmado");
            entity.Property(x => x.TwoFactorEnabled).HasColumnName("doble_factor_habilitado");
            entity.Property(x => x.LockoutEnd).HasColumnName("bloqueo_hasta");
            entity.Property(x => x.LockoutEnabled).HasColumnName("bloqueo_habilitado");
            entity.Property(x => x.AccessFailedCount).HasColumnName("intentos_fallidos");
            entity.Property(x => x.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(200);
            entity.Property(x => x.Documento).HasColumnName("documento").HasMaxLength(30);
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.Property(x => x.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(x => x.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("ux_usuarios_nombre_normalizado");
            entity.HasIndex(x => x.NormalizedEmail)
                .HasDatabaseName("ix_usuarios_correo_normalizado");
            entity.HasIndex(x => x.Documento)
                .IsUnique()
                .HasDatabaseName("ux_usuarios_documento")
                .HasFilter("documento IS NOT NULL");
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("roles");
            entity.Property(x => x.Id).HasColumnName("rol_id");
            entity.Property(x => x.Name).HasColumnName("nombre").HasMaxLength(100);
            entity.Property(x => x.NormalizedName).HasColumnName("nombre_normalizado").HasMaxLength(100);
            entity.Property(x => x.ConcurrencyStamp).HasColumnName("sello_concurrencia");
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(250);
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.HasIndex(x => x.NormalizedName)
                .IsUnique()
                .HasDatabaseName("ux_roles_nombre_normalizado");
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("usuarios_roles");
            entity.Property(x => x.UserId).HasColumnName("usuario_id");
            entity.Property(x => x.RoleId).HasColumnName("rol_id");
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("usuarios_claims");
            entity.Property(x => x.Id).HasColumnName("usuario_claim_id");
            entity.Property(x => x.UserId).HasColumnName("usuario_id");
            entity.Property(x => x.ClaimType).HasColumnName("tipo");
            entity.Property(x => x.ClaimValue).HasColumnName("valor");
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("usuarios_logins");
            entity.Property(x => x.LoginProvider).HasColumnName("proveedor");
            entity.Property(x => x.ProviderKey).HasColumnName("clave_proveedor");
            entity.Property(x => x.ProviderDisplayName).HasColumnName("nombre_proveedor");
            entity.Property(x => x.UserId).HasColumnName("usuario_id");
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("roles_claims");
            entity.Property(x => x.Id).HasColumnName("rol_claim_id");
            entity.Property(x => x.RoleId).HasColumnName("rol_id");
            entity.Property(x => x.ClaimType).HasColumnName("tipo");
            entity.Property(x => x.ClaimValue).HasColumnName("valor");
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("usuarios_tokens");
            entity.Property(x => x.UserId).HasColumnName("usuario_id");
            entity.Property(x => x.LoginProvider).HasColumnName("proveedor");
            entity.Property(x => x.Name).HasColumnName("nombre");
            entity.Property(x => x.Value).HasColumnName("valor");
        });
    }

    internal static void ConfigureControlOperativo(this ModelBuilder modelBuilder)
    {
        ConfigureClientes(modelBuilder);
        ConfigureFormularios(modelBuilder);
        ConfigureAssignments(modelBuilder);
        ConfigureOperations(modelBuilder);
        SeedInitialCatalog(modelBuilder);
    }

    private static void ConfigureClientes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("clientes");
            entity.HasKey(x => x.ClienteId).HasName("pk_clientes");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50);
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200);
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.Property(x => x.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.Codigo).IsUnique().HasDatabaseName("ux_clientes_codigo");
        });

        modelBuilder.Entity<ClienteTienda>(entity =>
        {
            entity.ToTable("clientes_tiendas");
            entity.HasKey(x => new { x.ClienteId, x.TiendaCadenaKey }).HasName("pk_clientes_tiendas");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.TiendaCadenaKey).HasColumnName("tienda_cadena_key").HasMaxLength(450);
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.Property(x => x.FechaAsignacion)
                .HasColumnName("fecha_asignacion")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<DimTiendaMaestraExport>().WithMany().HasForeignKey(x => x.TiendaCadenaKey).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFormularios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Formulario>(entity =>
        {
            entity.ToTable("formularios");
            entity.HasKey(x => x.FormularioId).HasName("pk_formularios");
            entity.Property(x => x.FormularioId).HasColumnName("formulario_id");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(100);
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200);
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
            entity.Property(x => x.Ruta).HasColumnName("ruta").HasMaxLength(200);
            entity.Property(x => x.Orden).HasColumnName("orden").HasDefaultValue(0);
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.Property(x => x.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasAlternateKey(x => new { x.FormularioId, x.ClienteId }).HasName("ak_formularios_cliente");
            entity.HasIndex(x => new { x.ClienteId, x.Codigo }).IsUnique().HasDatabaseName("ux_formularios_cliente_codigo");
        });

        modelBuilder.Entity<FormularioOpcion>(entity =>
        {
            entity.ToTable("formularios_opciones");
            entity.HasKey(x => x.FormularioOpcionId).HasName("pk_formularios_opciones");
            entity.Property(x => x.FormularioOpcionId).HasColumnName("formulario_opcion_id");
            entity.Property(x => x.FormularioId).HasColumnName("formulario_id");
            entity.Property(x => x.Clave).HasColumnName("clave").HasMaxLength(100);
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200);
            entity.Property(x => x.Habilitada).HasColumnName("habilitada").HasDefaultValue(false);
            entity.Property(x => x.Configuracion).HasColumnName("configuracion").HasColumnType("jsonb");
            entity.HasOne(x => x.Formulario).WithMany(x => x.Opciones).HasForeignKey(x => x.FormularioId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.FormularioId, x.Clave }).IsUnique().HasDatabaseName("ux_formularios_opciones_clave");
        });
    }

    private static void ConfigureAssignments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsuarioCliente>(entity =>
        {
            entity.ToTable("usuarios_clientes");
            entity.HasKey(x => new { x.UsuarioId, x.ClienteId }).HasName("pk_usuarios_clientes");
            entity.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.Property(x => x.FechaAsignacion).HasColumnName("fecha_asignacion").HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UsuarioClienteRol>(entity =>
        {
            entity.ToTable("usuarios_clientes_roles");
            entity.HasKey(x => new { x.UsuarioId, x.ClienteId, x.RolId }).HasName("pk_usuarios_clientes_roles");
            entity.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.RolId).HasColumnName("rol_id");
            entity.Property(x => x.FechaAsignacion).HasColumnName("fecha_asignacion").HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne<UsuarioCliente>().WithMany().HasForeignKey(x => new { x.UsuarioId, x.ClienteId }).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ApplicationRole>().WithMany().HasForeignKey(x => x.RolId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsuarioFormulario>(entity =>
        {
            entity.ToTable("usuarios_formularios");
            entity.HasKey(x => new { x.UsuarioId, x.FormularioId }).HasName("pk_usuarios_formularios");
            entity.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            entity.Property(x => x.FormularioId).HasColumnName("formulario_id");
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.Property(x => x.FechaAsignacion).HasColumnName("fecha_asignacion").HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Formulario>().WithMany().HasForeignKey(x => x.FormularioId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UsuarioTienda>(entity =>
        {
            entity.ToTable("usuarios_tiendas", table =>
                table.HasCheckConstraint("ck_usuarios_tiendas_fechas", "fecha_fin IS NULL OR fecha_fin >= fecha_inicio"));
            entity.HasKey(x => x.UsuarioTiendaId).HasName("pk_usuarios_tiendas");
            entity.Property(x => x.UsuarioTiendaId).HasColumnName("usuario_tienda_id");
            entity.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.TiendaCadenaKey).HasColumnName("tienda_cadena_key").HasMaxLength(450);
            entity.Property(x => x.TipoAsignacion).HasColumnName("tipo_asignacion").HasMaxLength(30);
            entity.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date");
            entity.Property(x => x.FechaFin).HasColumnName("fecha_fin").HasColumnType("date");
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.HasOne<UsuarioCliente>().WithMany().HasForeignKey(x => new { x.UsuarioId, x.ClienteId }).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ClienteTienda>().WithMany().HasForeignKey(x => new { x.ClienteId, x.TiendaCadenaKey }).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.UsuarioId, x.ClienteId, x.TiendaCadenaKey, x.TipoAsignacion })
                .IsUnique().HasFilter("activo = TRUE").HasDatabaseName("ux_usuarios_tiendas_asignacion_activa");
        });

        modelBuilder.Entity<SupervisorPersonal>(entity =>
        {
            entity.ToTable("supervisores_personal", table =>
            {
                table.HasCheckConstraint("ck_supervisores_personal_distintos", "supervisor_usuario_id <> personal_usuario_id");
                table.HasCheckConstraint("ck_supervisores_personal_fechas", "fecha_fin IS NULL OR fecha_fin >= fecha_inicio");
            });
            entity.HasKey(x => x.SupervisorPersonalId).HasName("pk_supervisores_personal");
            entity.Property(x => x.SupervisorPersonalId).HasColumnName("supervisor_personal_id");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.SupervisorUsuarioId).HasColumnName("supervisor_usuario_id");
            entity.Property(x => x.PersonalUsuarioId).HasColumnName("personal_usuario_id");
            entity.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date");
            entity.Property(x => x.FechaFin).HasColumnName("fecha_fin").HasColumnType("date");
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.HasOne<UsuarioCliente>().WithMany().HasForeignKey(x => new { x.SupervisorUsuarioId, x.ClienteId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<UsuarioCliente>().WithMany().HasForeignKey(x => new { x.PersonalUsuarioId, x.ClienteId }).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ClienteId, x.SupervisorUsuarioId, x.PersonalUsuarioId })
                .IsUnique().HasFilter("activo = TRUE").HasDatabaseName("ux_supervisores_personal_activo");
        });
    }

    private static void ConfigureOperations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchivoFormulario>(entity =>
        {
            entity.ToTable("archivos_formulario", table =>
                table.HasCheckConstraint("ck_archivos_formulario_tamano", "tamano_bytes >= 0"));
            entity.HasKey(x => x.ArchivoFormularioId).HasName("pk_archivos_formulario");
            entity.Property(x => x.ArchivoFormularioId).HasColumnName("archivo_formulario_id");
            entity.Property(x => x.Proveedor).HasColumnName("proveedor").HasMaxLength(30);
            entity.Property(x => x.ClaveObjeto).HasColumnName("clave_objeto").HasMaxLength(500);
            entity.Property(x => x.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(260);
            entity.Property(x => x.TipoContenido).HasColumnName("tipo_contenido").HasMaxLength(100);
            entity.Property(x => x.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(x => x.HashSha256).HasColumnName("hash_sha256").HasMaxLength(64);
            entity.Property(x => x.CreadoPorUsuarioId).HasColumnName("creado_por_usuario_id");
            entity.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreadoPorUsuarioId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.Proveedor, x.ClaveObjeto }).IsUnique().HasDatabaseName("ux_archivos_formulario_objeto");
        });

        modelBuilder.Entity<JornadaUsuario>(entity =>
        {
            entity.ToTable("jornadas_usuarios", table =>
                table.HasCheckConstraint("ck_jornadas_usuarios_horas", "hora_salida IS NULL OR hora_salida >= hora_ingreso"));
            entity.HasKey(x => x.JornadaUsuarioId).HasName("pk_jornadas_usuarios");
            entity.Property(x => x.JornadaUsuarioId).HasColumnName("jornada_usuario_id");
            entity.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.FormularioId).HasColumnName("formulario_id");
            entity.Property(x => x.TiendaCadenaKey).HasColumnName("tienda_cadena_key").HasMaxLength(450);
            entity.Property(x => x.SupervisorUsuarioId).HasColumnName("supervisor_usuario_id");
            entity.Property(x => x.FechaJornada).HasColumnName("fecha_jornada").HasColumnType("date");
            entity.Property(x => x.HoraIngreso).HasColumnName("hora_ingreso").HasColumnType("timestamp with time zone");
            entity.Property(x => x.HoraSalida).HasColumnName("hora_salida").HasColumnType("timestamp with time zone");
            entity.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20);
            entity.Property(x => x.TipoCierre).HasColumnName("tipo_cierre").HasMaxLength(20);
            entity.Property(x => x.FotoInicioArchivoId).HasColumnName("foto_inicio_archivo_id");
            entity.Property(x => x.DireccionIp).HasColumnName("direccion_ip").HasMaxLength(64);
            entity.Property(x => x.Dispositivo).HasColumnName("dispositivo").HasMaxLength(500);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Formulario>().WithMany().HasForeignKey(x => new { x.FormularioId, x.ClienteId }).HasPrincipalKey(x => new { x.FormularioId, x.ClienteId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ClienteTienda>().WithMany().HasForeignKey(x => new { x.ClienteId, x.TiendaCadenaKey }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.SupervisorUsuarioId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<ArchivoFormulario>().WithMany().HasForeignKey(x => x.FotoInicioArchivoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.UsuarioId, x.FormularioId, x.FechaJornada }).IsUnique().HasDatabaseName("ux_jornadas_usuario_formulario_fecha");
            entity.HasIndex(x => new { x.FechaJornada, x.Estado }).HasDatabaseName("ix_jornadas_fecha_estado");
        });

        modelBuilder.Entity<FormularioRegistro>(entity =>
        {
            entity.ToTable("formularios_registros", table =>
                table.HasCheckConstraint("ck_formularios_registros_fechas", "fecha_finalizacion IS NULL OR fecha_finalizacion >= fecha_inicio"));
            entity.HasKey(x => x.FormularioRegistroId).HasName("pk_formularios_registros");
            entity.Property(x => x.FormularioRegistroId).HasColumnName("formulario_registro_id");
            entity.Property(x => x.FormularioId).HasColumnName("formulario_id");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            entity.Property(x => x.JornadaUsuarioId).HasColumnName("jornada_usuario_id");
            entity.Property(x => x.TiendaCadenaKey).HasColumnName("tienda_cadena_key").HasMaxLength(450);
            entity.Property(x => x.SupervisorUsuarioId).HasColumnName("supervisor_usuario_id");
            entity.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20);
            entity.Property(x => x.Datos).HasColumnName("datos").HasColumnType("jsonb");
            entity.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.FechaFinalizacion).HasColumnName("fecha_finalizacion").HasColumnType("timestamp with time zone");
            entity.HasOne<Formulario>().WithMany().HasForeignKey(x => new { x.FormularioId, x.ClienteId }).HasPrincipalKey(x => new { x.FormularioId, x.ClienteId }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<JornadaUsuario>().WithMany().HasForeignKey(x => x.JornadaUsuarioId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<ClienteTienda>().WithMany().HasForeignKey(x => new { x.ClienteId, x.TiendaCadenaKey }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.SupervisorUsuarioId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.FormularioId, x.FechaInicio }).HasDatabaseName("ix_formularios_registros_fecha");
            entity.HasIndex(x => new { x.UsuarioId, x.FechaInicio }).HasDatabaseName("ix_formularios_registros_usuario_fecha");
        });

        modelBuilder.Entity<FormularioRegistroArchivo>(entity =>
        {
            entity.ToTable("formularios_registros_archivos");
            entity.HasKey(x => new { x.FormularioRegistroId, x.ArchivoFormularioId, x.Tipo }).HasName("pk_formularios_registros_archivos");
            entity.Property(x => x.FormularioRegistroId).HasColumnName("formulario_registro_id");
            entity.Property(x => x.ArchivoFormularioId).HasColumnName("archivo_formulario_id");
            entity.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(30);
            entity.Property(x => x.Orden).HasColumnName("orden").HasDefaultValue(0);
            entity.HasOne<FormularioRegistro>().WithMany().HasForeignKey(x => x.FormularioRegistroId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ArchivoFormulario>().WithMany().HasForeignKey(x => x.ArchivoFormularioId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedInitialCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationRole>().HasData(FormularioSeedCatalog.CreateRoles());
        modelBuilder.Entity<Cliente>().HasData(FormularioSeedCatalog.CreateBimbo());
        modelBuilder.Entity<Formulario>().HasData(FormularioSeedCatalog.CreateControlMaterial());
        modelBuilder.Entity<FormularioOpcion>().HasData(FormularioSeedCatalog.CreateOptions());
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Models.Entities;
using Sysbimbo.Api.Models.Identity;

namespace Sysbimbo.Api.Data;

/// <summary>
/// Contexto PostgreSQL exclusivo del formulario publico y sus CRUD de soporte.
/// Mantiene el alcance de la migracion limitado a tiendas, materiales y fotos.
/// </summary>
public class FormularioDbContext(DbContextOptions<FormularioDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<DimTiendaMaestraExport> Tiendas => Set<DimTiendaMaestraExport>();
    public DbSet<MaterialImpulsoTienda> MaterialesImpulsoTienda => Set<MaterialImpulsoTienda>();
    public DbSet<FotoMaterialImpulso> FotosMaterialImpulso => Set<FotoMaterialImpulso>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Formulario> Formularios => Set<Formulario>();
    public DbSet<FormularioOpcion> FormularioOpciones => Set<FormularioOpcion>();
    public DbSet<ClienteTienda> ClientesTiendas => Set<ClienteTienda>();
    public DbSet<UsuarioCliente> UsuariosClientes => Set<UsuarioCliente>();
    public DbSet<UsuarioClienteRol> UsuariosClientesRoles => Set<UsuarioClienteRol>();
    public DbSet<UsuarioFormulario> UsuariosFormularios => Set<UsuarioFormulario>();
    public DbSet<UsuarioTienda> UsuariosTiendas => Set<UsuarioTienda>();
    public DbSet<SupervisorPersonal> SupervisoresPersonal => Set<SupervisorPersonal>();
    public DbSet<ArchivoFormulario> ArchivosFormulario => Set<ArchivoFormulario>();
    public DbSet<JornadaUsuario> JornadasUsuarios => Set<JornadaUsuario>();
    public DbSet<FormularioRegistro> FormulariosRegistros => Set<FormularioRegistro>();
    public DbSet<FormularioRegistroArchivo> FormulariosRegistrosArchivos => Set<FormularioRegistroArchivo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureIdentity();

        modelBuilder.Entity<DimTiendaMaestraExport>(entity =>
        {
            entity.ToTable("tiendas");
            entity.HasKey(x => x.TiendaCadenaKey)
                .HasName("pk_tiendas");

            entity.Property(x => x.TiendaCadenaKey)
                .HasColumnName("tienda_cadena_key")
                .HasMaxLength(450);
            entity.Property(x => x.CodigoTiendaB2BPrefijo).HasColumnName("codigo_tienda_b2b_prefijo");
            entity.Property(x => x.CodigoTiendaB2B).HasColumnName("codigo_tienda_b2b");
            entity.Property(x => x.NombreTienda).HasColumnName("nombre_tienda");
            entity.Property(x => x.NombreTiendaBimbo).HasColumnName("nombre_tienda_bimbo");
            entity.Property(x => x.Canal).HasColumnName("canal");
            entity.Property(x => x.Cadena).HasColumnName("cadena");
            entity.Property(x => x.Formato).HasColumnName("formato");
            entity.Property(x => x.TipoLocal).HasColumnName("tipo_local");
            entity.Property(x => x.LimaProvincias).HasColumnName("lima_provincias");
            entity.Property(x => x.Region).HasColumnName("region");
            entity.Property(x => x.Provincia).HasColumnName("provincia");
            entity.Property(x => x.Ruta).HasColumnName("ruta");
            entity.Property(x => x.Supervisor).HasColumnName("supervisor");
            entity.Property(x => x.Gestor).HasColumnName("gestor");
            entity.Property(x => x.Vendedor).HasColumnName("vendedor");
            entity.Property(x => x.UltimaFecha)
                .HasColumnName("ultima_fecha")
                .HasColumnType("timestamp without time zone");
            entity.Property(x => x.CantidadRegistros).HasColumnName("cantidad_registros");
            entity.Property(x => x.FuenteTienda).HasColumnName("fuente_tienda");

            entity.HasIndex(x => x.Formato)
                .HasDatabaseName("ix_tiendas_formato");
            entity.HasIndex(x => x.NombreTiendaBimbo)
                .HasDatabaseName("ix_tiendas_nombre_bimbo");
        });

        modelBuilder.Entity<MaterialImpulsoTienda>(entity =>
        {
            entity.ToTable("materiales_impulso_tienda", table =>
                table.HasCheckConstraint(
                    "ck_materiales_impulso_tienda_cuota_diaria",
                    "cuota_diaria >= 0"));
            entity.HasKey(x => x.MaterialImpulsoTiendaId)
                .HasName("pk_materiales_impulso_tienda");

            entity.Property(x => x.MaterialImpulsoTiendaId)
                .HasColumnName("material_impulso_tienda_id")
                .ValueGeneratedOnAdd();
            entity.Property(x => x.TiendaCadenaKey)
                .HasColumnName("tienda_cadena_key")
                .HasMaxLength(450);
            entity.Property(x => x.NombreMaterial)
                .HasColumnName("nombre_material")
                .HasMaxLength(200);
            entity.Property(x => x.Descripcion)
                .HasColumnName("descripcion")
                .HasMaxLength(500);
            entity.Property(x => x.CuotaDiaria)
                .HasColumnName("cuota_diaria")
                .HasDefaultValue(0);
            entity.Property(x => x.Activo)
                .HasColumnName("activo")
                .HasDefaultValue(true);
            entity.Property(x => x.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne<DimTiendaMaestraExport>()
                .WithMany()
                .HasForeignKey(x => x.TiendaCadenaKey)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_materiales_tienda");

            entity.HasIndex(x => new { x.TiendaCadenaKey, x.NombreMaterial })
                .IsUnique()
                .HasDatabaseName("ux_materiales_tienda_nombre_activo")
                .HasFilter("activo = TRUE");
            entity.HasIndex(x => new { x.TiendaCadenaKey, x.Activo })
                .HasDatabaseName("ix_materiales_tienda_activo");
        });

        modelBuilder.Entity<FotoMaterialImpulso>(entity =>
        {
            entity.ToTable("fotos_material_impulso", table =>
                table.HasCheckConstraint(
                    "ck_fotos_material_impulso_tamano",
                    "tamano_bytes > 0"));
            entity.HasKey(x => x.FotoMaterialImpulsoId)
                .HasName("pk_fotos_material_impulso");

            entity.Property(x => x.FotoMaterialImpulsoId)
                .HasColumnName("foto_material_impulso_id")
                .ValueGeneratedOnAdd();
            entity.Property(x => x.MaterialImpulsoTiendaId)
                .HasColumnName("material_impulso_tienda_id");
            entity.Property(x => x.TiendaCadenaKey)
                .HasColumnName("tienda_cadena_key")
                .HasMaxLength(450);
            entity.Property(x => x.NombreArchivo)
                .HasColumnName("nombre_archivo")
                .HasMaxLength(260);
            entity.Property(x => x.TipoContenido)
                .HasColumnName("tipo_contenido")
                .HasMaxLength(100);
            entity.Property(x => x.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(x => x.Contenido)
                .HasColumnName("contenido")
                .HasColumnType("bytea");
            entity.Property(x => x.FechaCaptura)
                .HasColumnName("fecha_captura")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(x => x.MaterialImpulsoTienda)
                .WithMany(x => x.Fotos)
                .HasForeignKey(x => x.MaterialImpulsoTiendaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_fotos_material");
            entity.HasIndex(x => new { x.MaterialImpulsoTiendaId, x.FechaCaptura })
                .HasDatabaseName("ix_fotos_material_fecha");
        });

        modelBuilder.ConfigureControlOperativo();
    }
}

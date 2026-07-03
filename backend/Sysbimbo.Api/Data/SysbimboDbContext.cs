using Microsoft.EntityFrameworkCore;
using Sysbimbo.Api.Models.Entities;

namespace Sysbimbo.Api.Data;

public class SysbimboDbContext(DbContextOptions<SysbimboDbContext> options) : DbContext(options)
{
    public DbSet<DimTiendaMaestraExport> Tiendas => Set<DimTiendaMaestraExport>();
    public DbSet<DimSkuMaestraExport> Skus => Set<DimSkuMaestraExport>();
    public DbSet<Campania> Campanias => Set<Campania>();
    public DbSet<FactCampaniaCuota> Cuotas => Set<FactCampaniaCuota>();
    public DbSet<Programacion> Programaciones => Set<Programacion>();
    public DbSet<DetalleProgramacion> DetalleProgramaciones => Set<DetalleProgramacion>();
    public DbSet<AsymmetricaNuevoVenta> VentasAsymmetricaNuevo => Set<AsymmetricaNuevoVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimTiendaMaestraExport>(entity =>
        {
            entity.ToTable("DimTiendaMaestra_Export");
            entity.HasKey(x => x.TiendaCadenaKey);
        });

        modelBuilder.Entity<DimSkuMaestraExport>(entity =>
        {
            entity.ToTable("DimSkuMaestra_Export");
            entity.HasKey(x => x.SkuKey);
        });

        modelBuilder.Entity<Campania>(entity =>
        {
            entity.ToTable("Campania");
            entity.HasKey(x => x.CampaniaId);
        });

        modelBuilder.Entity<FactCampaniaCuota>(entity =>
        {
            entity.ToTable("FactCampaniaCuota");
            entity.HasKey(x => x.CuotaId);
            entity.Property(x => x.Cuota).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Programacion>(entity =>
        {
            entity.ToTable("Programacion");
            entity.HasKey(x => x.ProgramacionId);
        });

        modelBuilder.Entity<DetalleProgramacion>(entity =>
        {
            entity.ToTable("DetalleProgramacion");
            entity.HasKey(x => x.DetalleProgramacionId);
        });

        modelBuilder.Entity<AsymmetricaNuevoVenta>(entity =>
        {
            entity.ToView("ASYMMETRICA NUEVO");
            entity.HasNoKey();
            entity.Property(x => x.VentaUnidades).HasPrecision(18, 2);
        });
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sysbimbo.Api.Data;

#nullable disable

namespace Sysbimbo.Api.Data.Migrations.FormularioPostgres;

[DbContext(typeof(FormularioDbContext))]
[Migration("20260822003500_AllowPhotoGeneratedExchanges")]
public partial class AllowPhotoGeneratedExchanges : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "registrado_por_usuario_id",
            table: "canjes_material_diarios",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "registrado_por_usuario_id",
            table: "canjes_material_diarios",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);
    }
}

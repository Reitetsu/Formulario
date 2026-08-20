using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sysbimbo.Api.Data.Migrations.FormularioPostgres
{
    /// <inheritdoc />
    public partial class AddDailyMaterialExchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "canjes_material_diarios",
                columns: table => new
                {
                    canje_material_diario_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_impulso_tienda_id = table.Column<long>(type: "bigint", nullable: false),
                    tienda_cadena_key = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    forma_ingreso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    registrado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canjes_material_diarios", x => x.canje_material_diario_id);
                    table.CheckConstraint("ck_canjes_material_diarios_cantidad", "cantidad >= 0");
                    table.ForeignKey(
                        name: "fk_canjes_material",
                        column: x => x.material_impulso_tienda_id,
                        principalTable: "materiales_impulso_tienda",
                        principalColumn: "material_impulso_tienda_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_canjes_usuario_actualizacion",
                        column: x => x.actualizado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_canjes_usuario_registro",
                        column: x => x.registrado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_canjes_material_diarios_actualizado_por_usuario_id",
                table: "canjes_material_diarios",
                column: "actualizado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_canjes_material_diarios_registrado_por_usuario_id",
                table: "canjes_material_diarios",
                column: "registrado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_canjes_tienda_fecha",
                table: "canjes_material_diarios",
                columns: new[] { "tienda_cadena_key", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ux_canjes_material_fecha",
                table: "canjes_material_diarios",
                columns: new[] { "material_impulso_tienda_id", "fecha" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "canjes_material_diarios");
        }
    }
}

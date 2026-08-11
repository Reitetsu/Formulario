using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sysbimbo.Api.Data.Migrations.FormularioPostgres
{
    /// <inheritdoc />
    public partial class InitialFormularioPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tiendas",
                columns: table => new
                {
                    tienda_cadena_key = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    codigo_tienda_b2b_prefijo = table.Column<string>(type: "text", nullable: true),
                    codigo_tienda_b2b = table.Column<string>(type: "text", nullable: true),
                    nombre_tienda = table.Column<string>(type: "text", nullable: true),
                    nombre_tienda_bimbo = table.Column<string>(type: "text", nullable: true),
                    canal = table.Column<string>(type: "text", nullable: true),
                    cadena = table.Column<string>(type: "text", nullable: true),
                    formato = table.Column<string>(type: "text", nullable: true),
                    tipo_local = table.Column<string>(type: "text", nullable: true),
                    lima_provincias = table.Column<string>(type: "text", nullable: true),
                    region = table.Column<string>(type: "text", nullable: true),
                    provincia = table.Column<string>(type: "text", nullable: true),
                    ruta = table.Column<string>(type: "text", nullable: true),
                    supervisor = table.Column<string>(type: "text", nullable: true),
                    gestor = table.Column<string>(type: "text", nullable: true),
                    vendedor = table.Column<string>(type: "text", nullable: true),
                    ultima_fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cantidad_registros = table.Column<long>(type: "bigint", nullable: true),
                    fuente_tienda = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tiendas", x => x.tienda_cadena_key);
                });

            migrationBuilder.CreateTable(
                name: "materiales_impulso_tienda",
                columns: table => new
                {
                    material_impulso_tienda_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tienda_cadena_key = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    nombre_material = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cuota_diaria = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_materiales_impulso_tienda", x => x.material_impulso_tienda_id);
                    table.CheckConstraint("ck_materiales_impulso_tienda_cuota_diaria", "cuota_diaria >= 0");
                    table.ForeignKey(
                        name: "fk_materiales_tienda",
                        column: x => x.tienda_cadena_key,
                        principalTable: "tiendas",
                        principalColumn: "tienda_cadena_key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fotos_material_impulso",
                columns: table => new
                {
                    foto_material_impulso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_impulso_tienda_id = table.Column<long>(type: "bigint", nullable: false),
                    tienda_cadena_key = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    tipo_contenido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamano_bytes = table.Column<long>(type: "bigint", nullable: false),
                    contenido = table.Column<byte[]>(type: "bytea", nullable: false),
                    fecha_captura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fotos_material_impulso", x => x.foto_material_impulso_id);
                    table.CheckConstraint("ck_fotos_material_impulso_tamano", "tamano_bytes > 0");
                    table.ForeignKey(
                        name: "fk_fotos_material",
                        column: x => x.material_impulso_tienda_id,
                        principalTable: "materiales_impulso_tienda",
                        principalColumn: "material_impulso_tienda_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fotos_material_fecha",
                table: "fotos_material_impulso",
                columns: new[] { "material_impulso_tienda_id", "fecha_captura" });

            migrationBuilder.CreateIndex(
                name: "ix_materiales_tienda_activo",
                table: "materiales_impulso_tienda",
                columns: new[] { "tienda_cadena_key", "activo" });

            migrationBuilder.CreateIndex(
                name: "ux_materiales_tienda_nombre_activo",
                table: "materiales_impulso_tienda",
                columns: new[] { "tienda_cadena_key", "nombre_material" },
                unique: true,
                filter: "activo = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_tiendas_formato",
                table: "tiendas",
                column: "formato");

            migrationBuilder.CreateIndex(
                name: "ix_tiendas_nombre_bimbo",
                table: "tiendas",
                column: "nombre_tienda_bimbo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fotos_material_impulso");

            migrationBuilder.DropTable(
                name: "materiales_impulso_tienda");

            migrationBuilder.DropTable(
                name: "tiendas");
        }
    }
}

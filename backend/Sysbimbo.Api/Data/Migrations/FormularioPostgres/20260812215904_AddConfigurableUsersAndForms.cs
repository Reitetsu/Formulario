using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sysbimbo.Api.Data.Migrations.FormularioPostgres
{
    /// <inheritdoc />
    public partial class AddConfigurableUsersAndForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    cliente_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes", x => x.cliente_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nombre_normalizado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sello_concurrencia = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_completo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    documento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    nombre_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nombre_usuario_normalizado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    correo_normalizado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    correo_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    sello_seguridad = table.Column<string>(type: "text", nullable: true),
                    sello_concurrencia = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    telefono_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    doble_factor_habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    bloqueo_hasta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    bloqueo_habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    intentos_fallidos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.usuario_id);
                });

            migrationBuilder.CreateTable(
                name: "clientes_tiendas",
                columns: table => new
                {
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    tienda_cadena_key = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes_tiendas", x => new { x.cliente_id, x.tienda_cadena_key });
                    table.ForeignKey(
                        name: "FK_clientes_tiendas_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clientes_tiendas_tiendas_tienda_cadena_key",
                        column: x => x.tienda_cadena_key,
                        principalTable: "tiendas",
                        principalColumn: "tienda_cadena_key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "formularios",
                columns: table => new
                {
                    formulario_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ruta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_formularios", x => x.formulario_id);
                    table.UniqueConstraint("ak_formularios_cliente", x => new { x.formulario_id, x.cliente_id });
                    table.ForeignKey(
                        name: "FK_formularios_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "roles_claims",
                columns: table => new
                {
                    rol_claim_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles_claims", x => x.rol_claim_id);
                    table.ForeignKey(
                        name: "FK_roles_claims_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "archivos_formulario",
                columns: table => new
                {
                    archivo_formulario_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proveedor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    clave_objeto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    tipo_contenido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamano_bytes = table.Column<long>(type: "bigint", nullable: false),
                    hash_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    creado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_archivos_formulario", x => x.archivo_formulario_id);
                    table.CheckConstraint("ck_archivos_formulario_tamano", "tamano_bytes >= 0");
                    table.ForeignKey(
                        name: "FK_archivos_formulario_usuarios_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_claims",
                columns: table => new
                {
                    usuario_claim_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_claims", x => x.usuario_claim_id);
                    table.ForeignKey(
                        name: "FK_usuarios_claims_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_clientes",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios_clientes", x => new { x.usuario_id, x.cliente_id });
                    table.ForeignKey(
                        name: "FK_usuarios_clientes_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuarios_clientes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_logins",
                columns: table => new
                {
                    proveedor = table.Column<string>(type: "text", nullable: false),
                    clave_proveedor = table.Column<string>(type: "text", nullable: false),
                    nombre_proveedor = table.Column<string>(type: "text", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_logins", x => new { x.proveedor, x.clave_proveedor });
                    table.ForeignKey(
                        name: "FK_usuarios_logins_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_roles",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_roles", x => new { x.usuario_id, x.rol_id });
                    table.ForeignKey(
                        name: "FK_usuarios_roles_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_tokens",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_tokens", x => new { x.usuario_id, x.proveedor, x.nombre });
                    table.ForeignKey(
                        name: "FK_usuarios_tokens_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "formularios_opciones",
                columns: table => new
                {
                    formulario_opcion_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    formulario_id = table.Column<long>(type: "bigint", nullable: false),
                    clave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    habilitada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    configuracion = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_formularios_opciones", x => x.formulario_opcion_id);
                    table.ForeignKey(
                        name: "FK_formularios_opciones_formularios_formulario_id",
                        column: x => x.formulario_id,
                        principalTable: "formularios",
                        principalColumn: "formulario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_formularios",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    formulario_id = table.Column<long>(type: "bigint", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios_formularios", x => new { x.usuario_id, x.formulario_id });
                    table.ForeignKey(
                        name: "FK_usuarios_formularios_formularios_formulario_id",
                        column: x => x.formulario_id,
                        principalTable: "formularios",
                        principalColumn: "formulario_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuarios_formularios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "jornadas_usuarios",
                columns: table => new
                {
                    jornada_usuario_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    formulario_id = table.Column<long>(type: "bigint", nullable: false),
                    tienda_cadena_key = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    supervisor_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_jornada = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hora_salida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tipo_cierre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    foto_inicio_archivo_id = table.Column<long>(type: "bigint", nullable: true),
                    direccion_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    dispositivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jornadas_usuarios", x => x.jornada_usuario_id);
                    table.CheckConstraint("ck_jornadas_usuarios_horas", "hora_salida IS NULL OR hora_salida >= hora_ingreso");
                    table.ForeignKey(
                        name: "FK_jornadas_usuarios_archivos_formulario_foto_inicio_archivo_id",
                        column: x => x.foto_inicio_archivo_id,
                        principalTable: "archivos_formulario",
                        principalColumn: "archivo_formulario_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_jornadas_usuarios_clientes_tiendas_cliente_id_tienda_cadena~",
                        columns: x => new { x.cliente_id, x.tienda_cadena_key },
                        principalTable: "clientes_tiendas",
                        principalColumns: new[] { "cliente_id", "tienda_cadena_key" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_jornadas_usuarios_formularios_formulario_id_cliente_id",
                        columns: x => new { x.formulario_id, x.cliente_id },
                        principalTable: "formularios",
                        principalColumns: new[] { "formulario_id", "cliente_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_jornadas_usuarios_usuarios_supervisor_usuario_id",
                        column: x => x.supervisor_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_jornadas_usuarios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supervisores_personal",
                columns: table => new
                {
                    supervisor_personal_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    supervisor_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    personal_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supervisores_personal", x => x.supervisor_personal_id);
                    table.CheckConstraint("ck_supervisores_personal_distintos", "supervisor_usuario_id <> personal_usuario_id");
                    table.CheckConstraint("ck_supervisores_personal_fechas", "fecha_fin IS NULL OR fecha_fin >= fecha_inicio");
                    table.ForeignKey(
                        name: "FK_supervisores_personal_usuarios_clientes_personal_usuario_id~",
                        columns: x => new { x.personal_usuario_id, x.cliente_id },
                        principalTable: "usuarios_clientes",
                        principalColumns: new[] { "usuario_id", "cliente_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supervisores_personal_usuarios_clientes_supervisor_usuario_~",
                        columns: x => new { x.supervisor_usuario_id, x.cliente_id },
                        principalTable: "usuarios_clientes",
                        principalColumns: new[] { "usuario_id", "cliente_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_clientes_roles",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios_clientes_roles", x => new { x.usuario_id, x.cliente_id, x.rol_id });
                    table.ForeignKey(
                        name: "FK_usuarios_clientes_roles_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuarios_clientes_roles_usuarios_clientes_usuario_id_client~",
                        columns: x => new { x.usuario_id, x.cliente_id },
                        principalTable: "usuarios_clientes",
                        principalColumns: new[] { "usuario_id", "cliente_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_tiendas",
                columns: table => new
                {
                    usuario_tienda_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    tienda_cadena_key = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    tipo_asignacion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios_tiendas", x => x.usuario_tienda_id);
                    table.CheckConstraint("ck_usuarios_tiendas_fechas", "fecha_fin IS NULL OR fecha_fin >= fecha_inicio");
                    table.ForeignKey(
                        name: "FK_usuarios_tiendas_clientes_tiendas_cliente_id_tienda_cadena_~",
                        columns: x => new { x.cliente_id, x.tienda_cadena_key },
                        principalTable: "clientes_tiendas",
                        principalColumns: new[] { "cliente_id", "tienda_cadena_key" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuarios_tiendas_usuarios_clientes_usuario_id_cliente_id",
                        columns: x => new { x.usuario_id, x.cliente_id },
                        principalTable: "usuarios_clientes",
                        principalColumns: new[] { "usuario_id", "cliente_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "formularios_registros",
                columns: table => new
                {
                    formulario_registro_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    formulario_id = table.Column<long>(type: "bigint", nullable: false),
                    cliente_id = table.Column<long>(type: "bigint", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    jornada_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    tienda_cadena_key = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    supervisor_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    datos = table.Column<string>(type: "jsonb", nullable: true),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_finalizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_formularios_registros", x => x.formulario_registro_id);
                    table.CheckConstraint("ck_formularios_registros_fechas", "fecha_finalizacion IS NULL OR fecha_finalizacion >= fecha_inicio");
                    table.ForeignKey(
                        name: "FK_formularios_registros_clientes_tiendas_cliente_id_tienda_ca~",
                        columns: x => new { x.cliente_id, x.tienda_cadena_key },
                        principalTable: "clientes_tiendas",
                        principalColumns: new[] { "cliente_id", "tienda_cadena_key" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formularios_registros_formularios_formulario_id_cliente_id",
                        columns: x => new { x.formulario_id, x.cliente_id },
                        principalTable: "formularios",
                        principalColumns: new[] { "formulario_id", "cliente_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formularios_registros_jornadas_usuarios_jornada_usuario_id",
                        column: x => x.jornada_usuario_id,
                        principalTable: "jornadas_usuarios",
                        principalColumn: "jornada_usuario_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_formularios_registros_usuarios_supervisor_usuario_id",
                        column: x => x.supervisor_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_formularios_registros_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "formularios_registros_archivos",
                columns: table => new
                {
                    formulario_registro_id = table.Column<long>(type: "bigint", nullable: false),
                    archivo_formulario_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_formularios_registros_archivos", x => new { x.formulario_registro_id, x.archivo_formulario_id, x.tipo });
                    table.ForeignKey(
                        name: "FK_formularios_registros_archivos_archivos_formulario_archivo_~",
                        column: x => x.archivo_formulario_id,
                        principalTable: "archivos_formulario",
                        principalColumn: "archivo_formulario_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formularios_registros_archivos_formularios_registros_formul~",
                        column: x => x.formulario_registro_id,
                        principalTable: "formularios_registros",
                        principalColumn: "formulario_registro_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "clientes",
                columns: new[] { "cliente_id", "activo", "codigo", "fecha_creacion", "nombre" },
                values: new object[] { 1L, true, "BIMBO", new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Bimbo" });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "rol_id", "activo", "sello_concurrencia", "descripcion", "nombre", "nombre_normalizado" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), true, "10000000-0000-0000-0000-000000000001", "Administra clientes, usuarios, formularios y reportes.", "Administrador", "ADMINISTRADOR" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), true, "10000000-0000-0000-0000-000000000002", "Supervisa personal y tiendas asignadas.", "Supervisor", "SUPERVISOR" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), true, "10000000-0000-0000-0000-000000000003", "Registra actividades y evidencias de campo.", "Impulsadora", "IMPULSADORA" }
                });

            migrationBuilder.InsertData(
                table: "formularios",
                columns: new[] { "formulario_id", "activo", "cliente_id", "codigo", "descripcion", "fecha_creacion", "nombre", "orden", "ruta" },
                values: new object[] { 1L, true, 1L, "CONTROL_MATERIAL_IMPULSO", "Registro de evidencias y cumplimiento diario por material de impulso.", new DateTime(2026, 8, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Control de material", 1, "/habilitar-tienda" });

            migrationBuilder.InsertData(
                table: "formularios_opciones",
                columns: new[] { "formulario_opcion_id", "clave", "configuracion", "formulario_id", "nombre" },
                values: new object[,]
                {
                    { 1L, "LOGIN_REQUERIDO", null, 1L, "Inicio de sesion" },
                    { 2L, "ROLES_HABILITADOS", null, 1L, "Control por roles" },
                    { 3L, "FOTO_INICIO_OBLIGATORIA", null, 1L, "Foto de inicio obligatoria" }
                });

            migrationBuilder.InsertData(
                table: "formularios_opciones",
                columns: new[] { "formulario_opcion_id", "clave", "configuracion", "formulario_id", "habilitada", "nombre" },
                values: new object[] { 4L, "CONTROL_TIENDA", null, 1L, true, "Control por tienda" });

            migrationBuilder.InsertData(
                table: "formularios_opciones",
                columns: new[] { "formulario_opcion_id", "clave", "configuracion", "formulario_id", "nombre" },
                values: new object[] { 5L, "CONTROL_SUPERVISOR", null, 1L, "Control por supervisor" });

            migrationBuilder.InsertData(
                table: "formularios_opciones",
                columns: new[] { "formulario_opcion_id", "clave", "configuracion", "formulario_id", "habilitada", "nombre" },
                values: new object[] { 6L, "CIERRE_JORNADA_AUTOMATICO", "{\"hora\":\"23:59:59\",\"zonaHoraria\":\"America/Lima\"}", 1L, true, "Cierre automatico de jornada" });

            migrationBuilder.CreateIndex(
                name: "IX_archivos_formulario_creado_por_usuario_id",
                table: "archivos_formulario",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ux_archivos_formulario_objeto",
                table: "archivos_formulario",
                columns: new[] { "proveedor", "clave_objeto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_clientes_codigo",
                table: "clientes",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clientes_tiendas_tienda_cadena_key",
                table: "clientes_tiendas",
                column: "tienda_cadena_key");

            migrationBuilder.CreateIndex(
                name: "ux_formularios_cliente_codigo",
                table: "formularios",
                columns: new[] { "cliente_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_formularios_opciones_clave",
                table: "formularios_opciones",
                columns: new[] { "formulario_id", "clave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_formularios_registros_cliente_id_tienda_cadena_key",
                table: "formularios_registros",
                columns: new[] { "cliente_id", "tienda_cadena_key" });

            migrationBuilder.CreateIndex(
                name: "ix_formularios_registros_fecha",
                table: "formularios_registros",
                columns: new[] { "formulario_id", "fecha_inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_formularios_registros_formulario_id_cliente_id",
                table: "formularios_registros",
                columns: new[] { "formulario_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "IX_formularios_registros_jornada_usuario_id",
                table: "formularios_registros",
                column: "jornada_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_formularios_registros_supervisor_usuario_id",
                table: "formularios_registros",
                column: "supervisor_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_formularios_registros_usuario_fecha",
                table: "formularios_registros",
                columns: new[] { "usuario_id", "fecha_inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_formularios_registros_archivos_archivo_formulario_id",
                table: "formularios_registros_archivos",
                column: "archivo_formulario_id");

            migrationBuilder.CreateIndex(
                name: "ix_jornadas_fecha_estado",
                table: "jornadas_usuarios",
                columns: new[] { "fecha_jornada", "estado" });

            migrationBuilder.CreateIndex(
                name: "IX_jornadas_usuarios_cliente_id_tienda_cadena_key",
                table: "jornadas_usuarios",
                columns: new[] { "cliente_id", "tienda_cadena_key" });

            migrationBuilder.CreateIndex(
                name: "IX_jornadas_usuarios_formulario_id_cliente_id",
                table: "jornadas_usuarios",
                columns: new[] { "formulario_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "IX_jornadas_usuarios_foto_inicio_archivo_id",
                table: "jornadas_usuarios",
                column: "foto_inicio_archivo_id");

            migrationBuilder.CreateIndex(
                name: "IX_jornadas_usuarios_supervisor_usuario_id",
                table: "jornadas_usuarios",
                column: "supervisor_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ux_jornadas_usuario_formulario_fecha",
                table: "jornadas_usuarios",
                columns: new[] { "usuario_id", "formulario_id", "fecha_jornada" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_roles_nombre_normalizado",
                table: "roles",
                column: "nombre_normalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_claims_rol_id",
                table: "roles_claims",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_supervisores_personal_personal_usuario_id_cliente_id",
                table: "supervisores_personal",
                columns: new[] { "personal_usuario_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "IX_supervisores_personal_supervisor_usuario_id_cliente_id",
                table: "supervisores_personal",
                columns: new[] { "supervisor_usuario_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ux_supervisores_personal_activo",
                table: "supervisores_personal",
                columns: new[] { "cliente_id", "supervisor_usuario_id", "personal_usuario_id" },
                unique: true,
                filter: "activo = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_correo_normalizado",
                table: "usuarios",
                column: "correo_normalizado");

            migrationBuilder.CreateIndex(
                name: "ux_usuarios_documento",
                table: "usuarios",
                column: "documento",
                unique: true,
                filter: "documento IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_usuarios_nombre_normalizado",
                table: "usuarios",
                column: "nombre_usuario_normalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_claims_usuario_id",
                table: "usuarios_claims",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_clientes_cliente_id",
                table: "usuarios_clientes",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_clientes_roles_rol_id",
                table: "usuarios_clientes_roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_formularios_formulario_id",
                table: "usuarios_formularios",
                column: "formulario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_logins_usuario_id",
                table: "usuarios_logins",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_roles_rol_id",
                table: "usuarios_roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_tiendas_cliente_id_tienda_cadena_key",
                table: "usuarios_tiendas",
                columns: new[] { "cliente_id", "tienda_cadena_key" });

            migrationBuilder.CreateIndex(
                name: "ux_usuarios_tiendas_asignacion_activa",
                table: "usuarios_tiendas",
                columns: new[] { "usuario_id", "cliente_id", "tienda_cadena_key", "tipo_asignacion" },
                unique: true,
                filter: "activo = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "formularios_opciones");

            migrationBuilder.DropTable(
                name: "formularios_registros_archivos");

            migrationBuilder.DropTable(
                name: "roles_claims");

            migrationBuilder.DropTable(
                name: "supervisores_personal");

            migrationBuilder.DropTable(
                name: "usuarios_claims");

            migrationBuilder.DropTable(
                name: "usuarios_clientes_roles");

            migrationBuilder.DropTable(
                name: "usuarios_formularios");

            migrationBuilder.DropTable(
                name: "usuarios_logins");

            migrationBuilder.DropTable(
                name: "usuarios_roles");

            migrationBuilder.DropTable(
                name: "usuarios_tiendas");

            migrationBuilder.DropTable(
                name: "usuarios_tokens");

            migrationBuilder.DropTable(
                name: "formularios_registros");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "usuarios_clientes");

            migrationBuilder.DropTable(
                name: "jornadas_usuarios");

            migrationBuilder.DropTable(
                name: "archivos_formulario");

            migrationBuilder.DropTable(
                name: "clientes_tiendas");

            migrationBuilder.DropTable(
                name: "formularios");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}

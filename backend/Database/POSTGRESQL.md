# PostgreSQL para el formulario de materiales

El modulo de tiendas, materiales y evidencias usa la conexion
`FormularioPostgres`. Los modulos heredados conservan temporalmente la conexion
`LegacySqlServer`.

## 1. Crear la base y el usuario

En Windows, desde la raiz del repositorio, ejecuta el asistente incluido:

```powershell
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h localhost -p 5432 -U postgres -d postgres -W -f "backend\Database\configurar_postgresql.psql"
```

Primero solicitara la clave administrativa `postgres`. Luego pedira dos veces
una nueva clave privada para `sysbimbo_app`. Las claves no aparecen en pantalla.

Como alternativa, en pgAdmin o `psql`, con un usuario administrador, crea:

```sql
CREATE ROLE sysbimbo_app LOGIN PASSWORD 'CAMBIAR_ESTA_CLAVE';
CREATE DATABASE sysbimbo_formulario OWNER sysbimbo_app;
```

La clave del ejemplo debe reemplazarse por una clave segura.

## 2. Configurar la clave sin guardarla en Git

Desde `backend/Sysbimbo.Api` ejecuta:

```powershell
dotnet user-secrets set "ConnectionStrings:FormularioPostgres" "Host=localhost;Port=5432;Database=sysbimbo_formulario;Username=sysbimbo_app;Password=TU_CLAVE"
```

Para una instalacion publicada se recomienda la variable de entorno:

```text
ConnectionStrings__FormularioPostgres
```

## 3. Crear las tablas y cargar los datos iniciales

No es necesario subir ni ejecutar un script SQL. Al iniciar la API, Entity
Framework aplica automaticamente las migraciones pendientes y ejecuta un seeder
idempotente:

```powershell
dotnet run --project backend/Sysbimbo.Api
```

El seeder garantiza los roles base, el cliente BIMBO, el formulario de control,
sus opciones y la relacion entre BIMBO y las tiendas existentes. Solo agrega lo
que falta: no modifica opciones ya personalizadas desde el panel.

## 4. Copiar los datos existentes desde SQL Server

La conexion `LegacySqlServer` debe apuntar a la base anterior y ambas bases
deben estar disponibles. Luego ejecuta desde la raiz del repositorio:

```powershell
dotnet run --project backend/Sysbimbo.Api -- --migrate-form-data
```

El proceso:

- crea o actualiza el esquema PostgreSQL;
- copia tiendas, materiales y fotografias en ese orden;
- conserva los IDs para no romper relaciones ni enlaces del Excel;
- omite registros que ya existen, por lo que puede reanudarse;
- copia fotografias en lotes pequenos para limitar el uso de memoria;
- sincroniza las secuencias de identidad al finalizar.

Antes de retirar SQL Server, compara los totales de las tres tablas y prueba:

```text
GET /api/health/database
GET /api/tiendas?soloConMaterialActivo=true
GET /api/materiales-impulso
```

Las imagenes siguen almacenadas en PostgreSQL como `bytea` para conservar el
comportamiento actual. Cuando se configure R2 o Google Drive, una migracion
posterior reemplazara el binario por la clave del objeto externo.

## 5. Usuarios, jornadas y formularios configurables

La migracion `AddConfigurableUsersAndForms` prepara el modelo para administrar
varios clientes y formularios sin activar todavia el inicio de sesion en la
interfaz actual. Incluye:

- usuarios y roles con ASP.NET Core Identity;
- asignaciones de personal por cliente, tienda, formulario y supervisor;
- jornadas diarias con hora de ingreso, cierre, foto inicial y dispositivo;
- formularios independientes por cliente y registros genericos en JSONB;
- archivos reutilizables para fotos de inicio y evidencias;
- opciones habilitables por formulario desde un futuro panel de control.

El formulario inicial se registra como `CONTROL_MATERIAL_IMPULSO`. Sus opciones
son `LOGIN_REQUERIDO`, `ROLES_HABILITADOS`, `FOTO_INICIO_OBLIGATORIA`,
`CONTROL_TIENDA`, `CONTROL_SUPERVISOR` y `CIERRE_JORNADA_AUTOMATICO`.

Inicialmente solo estan habilitados el control por tienda y el cierre automatico
a las 23:59:59 en la zona `America/Lima`. Esto mantiene operativo el formulario
actual hasta que se implemente el panel y el flujo de autenticacion.

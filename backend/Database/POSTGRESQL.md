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

## 3. Crear las tablas

Hay dos opciones equivalentes:

```powershell
dotnet ef database update --context FormularioDbContext
```

o ejecutar `backend/Database/formulario_postgresql.sql` dentro de la base
`sysbimbo_formulario`.

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

# Despliegue a innovamsp.lat

El despliegue construye localmente las imagenes Docker del frontend y backend,
las transfiere al VPS por SFTP y recrea solamente los contenedores requeridos.
El volumen `postgres_data` no se reemplaza ni elimina.

## Configuracion privada

No se guardan contrasenas en Git. Configura las variables en la terminal desde
la que se ejecutara el despliegue:

```powershell
python -m pip install -r requirements-deploy.txt
```

```powershell
$env:VPS_HOST = "79.143.88.66"
$env:VPS_USER = "root"
$env:VPS_SSH_KEY = "C:\ruta\privada\sysbimbo_vps"
$env:POSTGRES_USER = "postgres"
$env:POSTGRES_PASSWORD = "CLAVE_PRIVADA"
$env:SEED_ADMIN_PASSWORD = "CLAVE_INICIAL_SEGURA_PARA_ADMIN"
```

`POSTGRES_PASSWORD` solo es necesario la primera vez o cuando se rota la clave.
`SEED_ADMIN_PASSWORD` se usa para crear idempotentemente el usuario `admin` y
se almacena solo en el `.env` privado del VPS.
El script crea `/root/formulario/.env` con permisos `600`. En despliegues
posteriores puede omitirse y se reutilizara el archivo privado del VPS.

Por compatibilidad temporal se admite `VPS_PASSWORD`, pero se recomienda una
llave SSH exclusiva y retirar la autenticacion por contrasena despues de
comprobarla.

## Ejecucion

```powershell
python deploy_vps.py
python deploy_vps.py --frontend
python deploy_vps.py --backend
```

Al finalizar, el servidor verifica automaticamente:

```text
https://innovamsp.lat/api/health/database
```

Entity Framework aplica las migraciones y seeders al iniciar la API. Los
archivos SQL no se transfieren durante una actualizacion normal.

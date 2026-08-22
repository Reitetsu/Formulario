#!/bin/bash
set -e

MODE="${1:-all}"
echo "=== Despliegue de Sysbimbo (PostgreSQL 16 + HTTPS SSL) en VPS (IP: 79.143.88.66) [Modo: $MODE] ==="

# El frontend no consume la clave del seeder, pero Docker Compose interpola
# todas las variables del archivo aun cuando solo se recrea este servicio.
if [ "$MODE" = "frontend" ] && [ -z "${SEED_ADMIN_PASSWORD:-}" ]; then
  export SEED_ADMIN_PASSWORD="frontend-only-not-used"
fi

if [ ! -f .env ]; then
  echo "ERROR: Falta $PWD/.env con POSTGRES_PASSWORD." >&2
  exit 1
fi

chmod 600 .env
sudo --preserve-env=SEED_ADMIN_PASSWORD docker compose config --quiet

# 1. Cargar imágenes .tar.gz / .tar según el modo
if [ "$MODE" == "backend" ] || [ "$MODE" == "all" ]; then
  if [ -f api.tar.gz ]; then
    echo "--> Cargando imagen Backend precompilada (api.tar.gz)..."
    sudo docker load -i api.tar.gz
    rm -f api.tar.gz
  elif [ -f api.tar ]; then
    echo "--> Cargando imagen Backend precompilada (api.tar)..."
    sudo docker load -i api.tar
  fi
fi

if [ "$MODE" == "frontend" ] || [ "$MODE" == "all" ]; then
  if [ -f web.tar.gz ]; then
    echo "--> Cargando imagen Frontend precompilada (web.tar.gz)..."
    sudo docker load -i web.tar.gz
    rm -f web.tar.gz
  elif [ -f web.tar ]; then
    echo "--> Cargando imagen Frontend precompilada (web.tar)..."
    sudo docker load -i web.tar
  fi
fi

# 2. Levantar contenedores correspondientes con Docker Compose
if [ "$MODE" == "frontend" ]; then
  echo "--> Reiniciando servicio Frontend en Docker Compose..."
  sudo --preserve-env=SEED_ADMIN_PASSWORD docker compose up -d --no-deps --force-recreate frontend
elif [ "$MODE" == "backend" ]; then
  echo "--> Reiniciando servicio Backend en Docker Compose..."
  sudo --preserve-env=SEED_ADMIN_PASSWORD docker compose up -d --no-deps --force-recreate backend
else
  echo "--> Levantando todos los servicios (PostgreSQL + Backend + Frontend HTTPS)..."
  sudo --preserve-env=SEED_ADMIN_PASSWORD docker compose up -d --force-recreate
fi

sudo docker image prune -f
sudo --preserve-env=SEED_ADMIN_PASSWORD docker compose ps

echo "--> Verificando la API publicada..."
for attempt in {1..12}; do
  if curl --fail --silent --show-error https://innovamsp.lat/api/health/database; then
    echo
    break
  fi

  if [ "$attempt" -eq 12 ]; then
    echo "ERROR: La API no respondio correctamente despues del despliegue." >&2
    sudo --preserve-env=SEED_ADMIN_PASSWORD docker compose logs --tail=100 backend
    exit 1
  fi

  sleep 5
done

echo "=== ¡Despliegue completado con éxito! ==="
echo "Accede a la aplicación segura en: https://innovamsp.lat/"

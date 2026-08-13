#!/bin/bash
set -e

MODE="${1:-all}"
echo "=== Despliegue de Sysbimbo (PostgreSQL 16 + HTTPS SSL) en VPS (IP: 79.143.88.66) [Modo: $MODE] ==="

# 1. Configurar Certificados SSL Gratuitos (Let's Encrypt)
if ! command -v certbot &> /dev/null; then
  echo "--> Instalando Certbot para HTTPS..."
  sudo apt-get update && sudo apt-get install -y certbot openssl
fi

mkdir -p /etc/letsencrypt/live/innovamsp.lat

if [ ! -f /etc/letsencrypt/live/innovamsp.lat/fullchain.pem ]; then
  echo "--> Generando certificados temporales SSL para evitar fallos de arranque en Nginx..."
  openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /etc/letsencrypt/live/innovamsp.lat/privkey.pem \
    -out /etc/letsencrypt/live/innovamsp.lat/fullchain.pem \
    -subj "/CN=innovamsp.lat" || true

  echo "--> Solicitando certificado real Let's Encrypt SSL para innovamsp.lat..."
  certbot certonly --standalone -d innovamsp.lat -d www.innovamsp.lat --non-interactive --agree-tos --email admin@innovamsp.lat || true
fi

# 2. Cargar imágenes .tar.gz / .tar según el modo
if [ "$MODE" == "backend" ] || [ "$MODE" == "all" ]; then
  if [ -f api.tar.gz ]; then
    echo "--> Cargando imagen Backend precompilada (api.tar.gz)..."
    sudo docker load -i api.tar.gz
  elif [ -f api.tar ]; then
    echo "--> Cargando imagen Backend precompilada (api.tar)..."
    sudo docker load -i api.tar
  fi
fi

if [ "$MODE" == "frontend" ] || [ "$MODE" == "all" ]; then
  if [ -f web.tar.gz ]; then
    echo "--> Cargando imagen Frontend precompilada (web.tar.gz)..."
    sudo docker load -i web.tar.gz
  elif [ -f web.tar ]; then
    echo "--> Cargando imagen Frontend precompilada (web.tar)..."
    sudo docker load -i web.tar
  fi
fi

# 3. Levantar contenedores correspondientes con Docker Compose
if [ "$MODE" == "frontend" ]; then
  echo "--> Reiniciando servicio Frontend en Docker Compose..."
  sudo docker compose up -d --no-deps frontend
elif [ "$MODE" == "backend" ]; then
  echo "--> Reiniciando servicio Backend en Docker Compose..."
  sudo docker compose up -d --no-deps backend
else
  echo "--> Levantando todos los servicios (PostgreSQL + Backend + Frontend HTTPS)..."
  sudo docker compose up -d
fi

echo "=== ¡Despliegue completado con éxito! ==="
echo "Accede a la aplicación segura en: https://innovamsp.lat/"

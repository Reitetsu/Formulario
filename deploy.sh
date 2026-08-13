#!/bin/bash
set -e

VPS_IP="${VPS_HOST:-79.143.88.66}"
VPS_USER="${VPS_USER:-root}"
REMOTE_DIR="${VPS_REMOTE_DIR:-/root/formulario}"

echo "=== 1. Compilando imágenes Docker localmente ==="
docker build -t sysbimbo-backend:latest -f backend/Sysbimbo.Api/Dockerfile .
docker build -t sysbimbo-frontend:latest -f frontend/sysbimbo-app/Dockerfile .

echo "=== 2. Exportando imágenes a archivos .tar ==="
docker save sysbimbo-backend:latest -o api.tar
docker save sysbimbo-frontend:latest -o web.tar

echo "=== 3. Preparando directorio remoto en VPS ==="
ssh ${VPS_USER}@${VPS_IP} "mkdir -p ${REMOTE_DIR}/backend/Database"

echo "=== 4. Transfiriendo archivos al VPS vía SCP ==="
scp api.tar web.tar docker-compose.yml deploy-vps.sh ${VPS_USER}@${VPS_IP}:${REMOTE_DIR}/

echo "=== 5. Ejecutando despliegue remoto vía SSH ==="
ssh ${VPS_USER}@${VPS_IP} "cd ${REMOTE_DIR} && chmod +x deploy-vps.sh && ./deploy-vps.sh"

echo "=== ¡Despliegue finalizado con éxito! ==="
echo "Accede a la aplicación en: http://${VPS_IP}/"

# Script PowerShell para compilar imágenes de Docker localmente y prepararlas para el VPS

$VPS_IP = "79.143.88.66"
$VPS_USER = "root"

Write-Host "=== 1. Compilando Imagen Backend (sysbimbo-backend:latest) ===" -ForegroundColor Green
docker build -t sysbimbo-backend:latest -f backend/Sysbimbo.Api/Dockerfile .

Write-Host "=== 2. Compilando Imagen Frontend (sysbimbo-frontend:latest) ===" -ForegroundColor Green
docker build -t sysbimbo-frontend:latest -f frontend/sysbimbo-app/Dockerfile .

Write-Host "=== 3. Exportando imágenes a archivos .tar ===" -ForegroundColor Yellow
Write-Host "Exportando backend a api.tar..."
docker save sysbimbo-backend:latest -o api.tar

Write-Host "Exportando frontend a web.tar..."
docker save sysbimbo-frontend:latest -o web.tar

Write-Host "=== 4. ¡Imágenes creadas exitosamente! ===" -ForegroundColor Green
Write-Host "Archivos generados en la raíz del proyecto:"
Write-Host " - api.tar"
Write-Host " - web.tar"
Write-Host ""
Write-Host "Para enviar al VPS e iniciar los contenedores, ejecuta:" -ForegroundColor Cyan
Write-Host "scp api.tar web.tar docker-compose.yml deploy-vps.sh ${VPS_USER}@${VPS_IP}:/root/formulario/"
Write-Host "scp -r backend/Database ${VPS_USER}@${VPS_IP}:/root/formulario/backend/"
Write-Host "ssh ${VPS_USER}@${VPS_IP} 'cd /root/formulario && chmod +x deploy-vps.sh backend/Database/init-db.sh && ./deploy-vps.sh'"

#!/bin/bash
set -e

echo "Esperando a que SQL Server esté listo..."
until /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" &> /dev/null
do
    echo "SQL Server no está listo todavía, esperando 3 segundos..."
    sleep 3
done

echo "SQL Server está activo. Ejecutando scripts de inicialización..."

echo "Creando base de datos ASYMMETRIKA BIMBO..."
/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/000_create_database.sql

echo "Ejecutando script 001_material_impulso_fotos.sql..."
/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -d "ASYMMETRIKA BIMBO" -i /scripts/001_material_impulso_fotos.sql

echo "Ejecutando script 002_material_impulso_cuota_diaria.sql..."
/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -d "ASYMMETRIKA BIMBO" -i /scripts/002_material_impulso_cuota_diaria.sql

echo "Ejecutando script 003_materiales_multiples_por_tienda.sql..."
/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -d "ASYMMETRIKA BIMBO" -i /scripts/003_materiales_multiples_por_tienda.sql

echo "Inicialización de la base de datos completada exitosamente."

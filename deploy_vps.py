import os
import sys
import argparse
import subprocess
import gzip
import shutil
import paramiko

HOST = "79.143.88.66"
PORT = 22
USER = "root"
PASSWORD = "JWRdRn9RzyUziCq"
REMOTE_DIR = "/root/formulario"

def run_local(cmd):
    print(f"--> Ejecutando localmente: {cmd}")
    res = subprocess.run(cmd, shell=True)
    if res.returncode != 0:
        print(f"Error al ejecutar comando local: {cmd}")
        sys.exit(1)

def compress_gzip(source_file, target_file):
    print(f"--> Comprimiendo {source_file} -> {target_file}...")
    with open(source_file, 'rb') as f_in:
        with gzip.open(target_file, 'wb', compresslevel=6) as f_out:
            shutil.copyfileobj(f_in, f_out)
    print(f"--> Compresión finalizada: {target_file}")

def create_progress_callback(filename):
    last_percent = [-1]
    def progress_callback(transferred, total):
        if total > 0:
            percent = int((transferred / total) * 100)
            if percent != last_percent[0]:
                last_percent[0] = percent
                mb_transferred = transferred / (1024 * 1024)
                mb_total = total / (1024 * 1024)
                sys.stdout.write(f"\r--> Subiendo {filename}: {mb_transferred:.1f} MB / {mb_total:.1f} MB [{percent}%]")
                sys.stdout.flush()
                if percent == 100:
                    sys.stdout.write("\n")
    return progress_callback

def main():
    parser = argparse.ArgumentParser(description="Despliegue rápido e incremental a VPS")
    parser.add_argument("-f", "--frontend", action="store_true", help="Desplegar solo el Frontend (Angular + Nginx)")
    parser.add_argument("-b", "--backend", action="store_true", help="Desplegar solo el Backend (.NET API)")
    args = parser.parse_args()

    deploy_frontend = True
    deploy_backend = True
    mode = "all"

    if args.frontend and not args.backend:
        deploy_backend = False
        mode = "frontend"
        print("=== MODO OPTIMIZADO: Desplegando ÚNICAMENTE el Frontend ===")
    elif args.backend and not args.frontend:
        deploy_frontend = False
        mode = "backend"
        print("=== MODO OPTIMIZADO: Desplegando ÚNICAMENTE el Backend ===")
    else:
        print("=== MODO COMPLETO: Desplegando Backend y Frontend ===")

    print("\n=== 1. Compilando imágenes Docker localmente ===")
    if deploy_backend:
        run_local("docker build -t sysbimbo-backend:latest -f backend/Sysbimbo.Api/Dockerfile .")
    if deploy_frontend:
        run_local("docker build -t sysbimbo-frontend:latest -f frontend/sysbimbo-app/Dockerfile .")

    print("\n=== 2. Exportando y comprimiendo imágenes a .tar.gz ===")
    if deploy_backend:
        run_local("docker save sysbimbo-backend:latest -o api.tar")
        compress_gzip("api.tar", "api.tar.gz")

    if deploy_frontend:
        run_local("docker save sysbimbo-frontend:latest -o web.tar")
        compress_gzip("web.tar", "web.tar.gz")

    print(f"\n=== 3. Conectando por SSH a {USER}@{HOST} ===")
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    ssh.connect(HOST, port=PORT, username=USER, password=PASSWORD, timeout=30, banner_timeout=60)
    
    transport = ssh.get_transport()
    if transport:
        transport.set_keepalive(10)
    
    sftp = ssh.open_sftp()
    print("Conexión SSH / SFTP exitosa.")

    print(f"\n=== 4. Preparando directorio remoto {REMOTE_DIR} ===")
    try:
        sftp.mkdir(REMOTE_DIR)
    except IOError:
        pass
    try:
        sftp.mkdir(f"{REMOTE_DIR}/backend")
    except IOError:
        pass
    try:
        sftp.mkdir(f"{REMOTE_DIR}/backend/Database")
    except IOError:
        pass

    print("\n=== 5. Transfiriendo archivos requeridos vía SFTP ===")
    
    files_to_upload = [
        ("docker-compose.yml", f"{REMOTE_DIR}/docker-compose.yml"),
        ("deploy-vps.sh", f"{REMOTE_DIR}/deploy-vps.sh"),
        ("backend/Database/formulario_postgresql.sql", f"{REMOTE_DIR}/backend/Database/formulario_postgresql.sql"),
        ("backend/Database/datos_formulario_postgresql.sql", f"{REMOTE_DIR}/backend/Database/datos_formulario_postgresql.sql"),
    ]

    if deploy_frontend:
        files_to_upload.append(("web.tar.gz", f"{REMOTE_DIR}/web.tar.gz"))

    if deploy_backend:
        files_to_upload.append(("api.tar.gz", f"{REMOTE_DIR}/api.tar.gz"))

    for local_path, remote_path in files_to_upload:
        filename = os.path.basename(local_path)
        cb = create_progress_callback(filename)
        sftp.put(local_path, remote_path, callback=cb)

    sftp.close()
    ssh.close()
    print("Transferencia de archivos completada exitosamente.")

    print("\n=== 6. Ejecutando despliegue remoto en el VPS ===")
    ssh_exec = paramiko.SSHClient()
    ssh_exec.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    ssh_exec.connect(HOST, port=PORT, username=USER, password=PASSWORD, timeout=30, banner_timeout=60)
    
    cmd_deploy = f"cd {REMOTE_DIR} && chmod +x deploy-vps.sh backend/Database/init-db.sh && ./deploy-vps.sh {mode}"
    stdin, stdout, stderr = ssh_exec.exec_command(cmd_deploy, get_pty=True)

    for line in iter(stdout.readline, ""):
        sys.stdout.buffer.write(line.encode('utf-8', errors='ignore'))
        sys.stdout.buffer.flush()

    ssh_exec.close()
    print("\n=== ¡Despliegue finalizado con éxito! ===")
    print(f"Accede a la aplicación en: http://{HOST}/")

if __name__ == "__main__":
    main()

import os
import sys
import argparse
import subprocess
import gzip
import shutil
import shlex
import paramiko

HOST = os.environ.get("VPS_HOST", "79.143.88.66")
PORT = int(os.environ.get("VPS_PORT", "22"))
USER = os.environ.get("VPS_USER", "root")
PASSWORD = os.environ.get("VPS_PASSWORD")
SSH_KEY = os.path.expanduser(os.environ["VPS_SSH_KEY"]) if os.environ.get("VPS_SSH_KEY") else None
REMOTE_DIR = os.environ.get("VPS_REMOTE_DIR", "/root/formulario")
POSTGRES_USER = os.environ.get("POSTGRES_USER", "postgres")
POSTGRES_PASSWORD = os.environ.get("POSTGRES_PASSWORD")


def connect_ssh():
    ssh = paramiko.SSHClient()
    ssh.load_system_host_keys()
    ssh.set_missing_host_key_policy(paramiko.WarningPolicy())

    connect_args = {
        "hostname": HOST,
        "port": PORT,
        "username": USER,
        "timeout": 30,
        "banner_timeout": 60,
        "allow_agent": True,
        "look_for_keys": True,
    }
    if PASSWORD:
        connect_args["password"] = PASSWORD
    if SSH_KEY:
        connect_args["key_filename"] = SSH_KEY

    ssh.connect(**connect_args)
    transport = ssh.get_transport()
    if transport:
        transport.set_keepalive(10)
    return ssh

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
    ssh = connect_ssh()
    
    sftp = ssh.open_sftp()
    print("Conexión SSH / SFTP exitosa.")

    print(f"\n=== 4. Preparando directorio remoto {REMOTE_DIR} ===")
    try:
        sftp.mkdir(REMOTE_DIR)
    except IOError:
        pass
    print("\n=== 5. Transfiriendo archivos requeridos vía SFTP ===")
    
    files_to_upload = [
        ("docker-compose.yml", f"{REMOTE_DIR}/docker-compose.yml"),
        ("deploy-vps.sh", f"{REMOTE_DIR}/deploy-vps.sh"),
    ]

    if deploy_frontend:
        files_to_upload.append(("web.tar.gz", f"{REMOTE_DIR}/web.tar.gz"))

    if deploy_backend:
        files_to_upload.append(("api.tar.gz", f"{REMOTE_DIR}/api.tar.gz"))

    for local_path, remote_path in files_to_upload:
        filename = os.path.basename(local_path)
        cb = create_progress_callback(filename)
        sftp.put(local_path, remote_path, callback=cb)

    remote_env_path = f"{REMOTE_DIR}/.env"
    if POSTGRES_PASSWORD:
        if "\n" in POSTGRES_PASSWORD or "\r" in POSTGRES_PASSWORD:
            raise ValueError("POSTGRES_PASSWORD contiene caracteres no permitidos.")
        with sftp.open(remote_env_path, "w") as env_file:
            env_file.write(
                f"POSTGRES_USER={POSTGRES_USER}\n"
                f"POSTGRES_PASSWORD={POSTGRES_PASSWORD}\n"
            )
        sftp.chmod(remote_env_path, 0o600)
        print("Archivo .env privado actualizado en el VPS.")
    else:
        try:
            sftp.stat(remote_env_path)
        except IOError as error:
            raise RuntimeError(
                "El VPS no tiene .env. Configura POSTGRES_PASSWORD una vez antes de desplegar."
            ) from error

    sftp.close()
    print("Transferencia de archivos completada exitosamente.")

    print("\n=== 6. Ejecutando despliegue remoto en el VPS ===")
    quoted_remote_dir = shlex.quote(REMOTE_DIR)
    cmd_deploy = f"cd {quoted_remote_dir} && chmod +x deploy-vps.sh && ./deploy-vps.sh {mode}"
    stdin, stdout, stderr = ssh.exec_command(cmd_deploy, get_pty=True)

    for line in iter(stdout.readline, ""):
        sys.stdout.buffer.write(line.encode('utf-8', errors='ignore'))
        sys.stdout.buffer.flush()

    exit_code = stdout.channel.recv_exit_status()
    ssh.close()
    if exit_code != 0:
        print(f"El despliegue remoto termino con codigo {exit_code}.")
        sys.exit(exit_code)
    print("\n=== ¡Despliegue finalizado con éxito! ===")
    print(f"Accede a la aplicación en: http://{HOST}/")

if __name__ == "__main__":
    main()

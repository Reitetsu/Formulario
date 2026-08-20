export interface AuthUser {
  usuarioId: string;
  nombreUsuario: string;
  nombreCompleto: string;
  roles: string[];
  expiraEn: string;
}

export interface LoginCredentials {
  usuario: string;
  password: string;
}

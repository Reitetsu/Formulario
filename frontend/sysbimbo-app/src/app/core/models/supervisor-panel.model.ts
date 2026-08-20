export interface SupervisorPanel {
  fecha: string;
  asistencia: SupervisorAttendance | null;
  tiendas: SupervisorStore[];
}

export interface SupervisorAttendance {
  jornadaUsuarioId: number;
  fecha: string;
  horaIngreso: string;
  horaSalida: string | null;
  estado: string;
  tipoCierre: string | null;
}

export interface SupervisorStore {
  tiendaCadenaKey: string;
  nombreTienda: string;
  formato: string | null;
  totalCanjesHoy: number;
  materiales: SupervisorMaterial[];
}

export interface SupervisorMaterial {
  materialImpulsoTiendaId: number;
  nombreMaterial: string;
  cuotaDiaria: number;
  canjesHoy: number;
  evidenciasHoy: number;
}

export interface UpdateSupervisorAttendance {
  horaIngreso: string;
  horaSalida: string | null;
}

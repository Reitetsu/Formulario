export interface Programacion {
  programacionId: number;
  campaniaId: number | null;
  nombreCampania: string | null;
  tiendaCadenaKey: string | null;
  nombreTiendaBimbo: string | null;
  fecha: string | null;
  cuota: number | null;
  estado: string | null;
  fuenteProgramacion: string | null;
  fechaCreacion: string | null;
  fechaActualizacion: string | null;
}

export interface ProgramacionQuery {
  nombreCampania?: string;
  nombreTiendaBimbo?: string;
  fecha?: string;
  cuota?: number;
  estado?: string;
  pageNumber: number;
  pageSize: number;
}

export interface CreateProgramacionPayload {
  campaniaId?: number | null;
  tiendaCadenaKey: string;
  fecha: string;
  cuota?: number | null;
  estado?: string | null;
  fuenteProgramacion?: string | null;
  fechaCreacion?: string | null;
  fechaActualizacion?: string | null;
}

export type UpdateProgramacionPayload = CreateProgramacionPayload;

export interface Campania {
  campaniaId: number;
  nombreCampania: string | null;
  descripcion: string | null;
  fechaInicio: string | null;
  fechaFin: string | null;
  estado: string | null;
}

export interface CampaniaQuery {
  nombreCampania?: string;
  descripcion?: string;
  estado?: string;
  pageNumber: number;
  pageSize: number;
}

export interface CreateCampaniaPayload {
  nombreCampania: string;
  descripcion?: string | null;
  fechaInicio?: string | null;
  fechaFin?: string | null;
  estado?: string | null;
}

export type UpdateCampaniaPayload = CreateCampaniaPayload;

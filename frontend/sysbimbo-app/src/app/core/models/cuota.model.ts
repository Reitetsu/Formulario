export interface Cuota {
  cuotaId: number;
  campania: string | null;
  tiendaCadenaKey: string | null;
  fecha: string | null;
  cuota: number | null;
}

export interface CuotaQuery {
  campania?: string;
  tiendaCadenaKey?: string;
  fecha?: string;
  pageNumber: number;
  pageSize: number;
}

export interface CreateCuotaPayload {
  campania: string;
  tiendaCadenaKey: string;
  fecha: string;
  cuota: number;
}

export type UpdateCuotaPayload = CreateCuotaPayload;

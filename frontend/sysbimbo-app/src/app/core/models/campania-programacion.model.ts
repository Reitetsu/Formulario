export interface CampaniaProgramacion {
  programacionId: number;
  campaniaId: number | null;
  tiendaCadenaKey: string;
  nombreTienda: string | null;
  nombreTiendaBimbo: string | null;
  cadena: string | null;
  fecha: string | null;
  estadoPersistido: string;
  estadoFuncional: string;
  fuenteProgramacion: string | null;
  cantidadSkus: number;
  fechaCreacion: string | null;
  fechaActualizacion: string | null;
}

export interface CampaniaProgramacionDetalle {
  detalleProgramacionId: number;
  programacionId: number;
  codigoSkuBimbo: string;
  nombreSkuBimbo: string | null;
  codigoSkuB2B: string | null;
  marca: string | null;
  categoria: string | null;
  fechaCreacion: string;
}

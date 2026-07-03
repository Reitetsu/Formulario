export interface CampaniaSku {
  codigoSkuBimbo: string;
  codigoSkuB2B: string | null;
  nombreSkuBimbo: string | null;
  nombreSkuB2B: string | null;
  marca: string | null;
  categoria: string | null;
  area: string | null;
  unidadNegocio: string | null;
  cantidadProgramaciones: number;
  cantidadTiendas: number;
  cantidadFechas: number;
}

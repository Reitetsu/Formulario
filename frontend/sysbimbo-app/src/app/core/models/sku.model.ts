export interface Sku {
  skuKey: string;
  codigoSkuB2B: string | null;
  nombreSkuB2B: string | null;
  codigoSkuBimbo: string | null;
  nombreSkuBimbo: string | null;
  unidadNegocio: string | null;
  area: string | null;
  categoria: string | null;
  marca: string | null;
  tipoProducto: string | null;
  status: string | null;
  gramaje: string | null;
  ultimaFecha: string | null;
  cantidadRegistros: number | null;
  fuenteSku: string | null;
}

export interface SkuQuery {
  categoria?: string;
  marca?: string;
  nombre?: string;
  codigoSkuB2B?: string;
  codigoSkuBimbo?: string;
  pageNumber: number;
  pageSize: number;
}

export interface CreateSkuPayload {
  skuKey: string;
  codigoSkuB2B?: string | null;
  nombreSkuB2B?: string | null;
  codigoSkuBimbo?: string | null;
  nombreSkuBimbo?: string | null;
  unidadNegocio?: string | null;
  area?: string | null;
  categoria?: string | null;
  marca?: string | null;
  tipoProducto?: string | null;
  status?: string | null;
  gramaje?: string | null;
  ultimaFecha?: string | null;
  cantidadRegistros?: number | null;
  fuenteSku?: string | null;
}

export type UpdateSkuPayload = Omit<CreateSkuPayload, 'skuKey'>;

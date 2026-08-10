export interface Tienda {
  tiendaCadenaKey: string;
  codigoTiendaB2BPrefijo: string | null;
  codigoTiendaB2B: string | null;
  nombreTienda: string | null;
  nombreTiendaBimbo: string | null;
  canal: string | null;
  cadena: string | null;
  formato: string | null;
  tipoLocal: string | null;
  limaProvincias: string | null;
  region: string | null;
  provincia: string | null;
  ruta: string | null;
  supervisor: string | null;
  gestor: string | null;
  vendedor: string | null;
  ultimaFecha: string | null;
  cantidadRegistros: number | null;
  fuenteTienda: string | null;
}

export interface TiendaQuery {
  cadena?: string;
  marca?: string;
  region?: string;
  nombre?: string;
  codigoTiendaB2B?: string;
  soloConMaterialActivo?: boolean;
  pageNumber: number;
  pageSize: number;
}

export interface CreateTiendaPayload {
  tiendaCadenaKey: string;
  codigoTiendaB2BPrefijo?: string | null;
  codigoTiendaB2B?: string | null;
  nombreTienda?: string | null;
  nombreTiendaBimbo?: string | null;
  canal?: string | null;
  cadena?: string | null;
  formato?: string | null;
  tipoLocal?: string | null;
  limaProvincias?: string | null;
  region?: string | null;
  provincia?: string | null;
  ruta?: string | null;
  supervisor?: string | null;
  gestor?: string | null;
  vendedor?: string | null;
  ultimaFecha?: string | null;
  cantidadRegistros?: number | null;
  fuenteTienda?: string | null;
}

export type UpdateTiendaPayload = Omit<CreateTiendaPayload, 'tiendaCadenaKey'>;

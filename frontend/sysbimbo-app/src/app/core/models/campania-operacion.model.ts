export interface CampaniaOperacionResultado {
  mensaje: string;
  procesados: number;
  creados: number;
  reactivados: number;
  actualizados: number;
  eliminados: number;
  omitidos: number;
  detallesCreados: number;
  detallesEliminados: number;
  advertencias: string[];
}

export interface AddCampaniaTiendasPayload {
  tiendaCadenaKeys: string[];
  fechas: string[];
  replicarSkusExistentes: boolean;
}

export interface AddCampaniaFechasPayload {
  fechas: string[];
  tiendaCadenaKeys: string[];
  aplicarATodasLasTiendas: boolean;
  replicarSkusExistentes: boolean;
}

export interface AddCampaniaSkusPayload {
  codigosSkuBimbo: string[];
}

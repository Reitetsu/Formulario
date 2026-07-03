export interface CampaniaTienda {
  tiendaCadenaKey: string;
  codigoTiendaB2B: string | null;
  nombreTienda: string | null;
  nombreTiendaBimbo: string | null;
  cadena: string | null;
  formato: string | null;
  region: string | null;
  cantidadFechas: number;
  primeraFecha: string | null;
  ultimaFecha: string | null;
  cantidadProgramadas: number;
  cantidadEjecutadas: number;
  cantidadCanceladas: number;
}

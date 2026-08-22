export interface MaterialImpulsoTienda {
  materialImpulsoTiendaId: number;
  tiendaCadenaKey: string;
  nombreMaterial: string;
  descripcion: string | null;
  cuotaDiaria: number;
  acumulado: number;
  canjesHoy: number;
}

export interface CanjesDiariosResult {
  canjeMaterialDiarioId: number;
  materialImpulsoTiendaId: number;
  tiendaCadenaKey: string;
  fecha: string;
  cantidad: number;
  formaIngreso: 'MANUAL' | string;
  registradoPorUsuarioId: string | null;
  actualizadoPorUsuarioId: string | null;
  fechaCreacion: string;
  fechaActualizacion: string;
}

export interface FotoMaterialImpulsoResult {
  fotoMaterialImpulsoId: number;
  materialImpulsoTiendaId: number;
  tiendaCadenaKey: string;
  nombreArchivo: string;
  fechaCaptura: string;
  acumulado: number;
  canjesHoy: number;
}

export interface FotoMaterialResumen {
  fotoMaterialImpulsoId: number;
  materialImpulsoTiendaId: number;
  nombreArchivo: string;
  tipoContenido: string;
  tamanoBytes: number;
  fechaCaptura: string;
}

export interface MaterialImpulsoAdmin extends MaterialImpulsoTienda {
  nombreTienda: string;
  formato: string | null;
  activo: boolean;
  fechaCreacion: string;
}

export interface MaterialImpulsoQuery {
  tienda?: string;
  marca?: string;
  material?: string;
  soloActivos?: boolean;
  pageNumber: number;
  pageSize: number;
}

export interface CreateMaterialImpulsoPayload {
  tiendaCadenaKey: string;
  nombreMaterial: string;
  descripcion?: string | null;
  cuotaDiaria: number;
}

export interface UpdateMaterialImpulsoPayload {
  nombreMaterial: string;
  descripcion?: string | null;
  cuotaDiaria: number;
}

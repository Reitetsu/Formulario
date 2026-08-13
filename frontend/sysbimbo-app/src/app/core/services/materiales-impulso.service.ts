import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  CreateMaterialImpulsoPayload,
  FotoMaterialImpulsoResult,
  FotoMaterialResumen,
  MaterialImpulsoAdmin,
  MaterialImpulsoQuery,
  MaterialImpulsoTienda,
  UpdateMaterialImpulsoPayload
} from '../models/material-impulso.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class MaterialesImpulsoService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${API_BASE_URL}/materiales-impulso`;

  getByTienda(tiendaCadenaKey: string): Observable<MaterialImpulsoTienda[]> {
    return this.http.get<MaterialImpulsoTienda[]>(
      `${this.endpoint}/tiendas/${encodeURIComponent(tiendaCadenaKey)}`
    );
  }

  savePhoto(materialImpulsoTiendaId: number, foto: File): Observable<FotoMaterialImpulsoResult> {
    const body = new FormData();
    body.append('foto', foto, foto.name);

    return this.http.post<FotoMaterialImpulsoResult>(
      `${this.endpoint}/${materialImpulsoTiendaId}/fotos`,
      body
    );
  }

  getPhotos(materialImpulsoTiendaId: number): Observable<FotoMaterialResumen[]> {
    return this.http.get<FotoMaterialResumen[]>(
      `${this.endpoint}/${materialImpulsoTiendaId}/fotos`
    );
  }

  getPhotoUrl(fotoId: number): string {
    return `${this.endpoint}/fotos/${fotoId}`;
  }

  deletePhoto(materialImpulsoTiendaId: number, fotoId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.endpoint}/${materialImpulsoTiendaId}/fotos/${fotoId}`
    );
  }

  list(query: MaterialImpulsoQuery): Observable<PagedResult<MaterialImpulsoAdmin>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize)
      .set('soloActivos', query.soloActivos ?? true);

    if (query.tienda) params = params.set('tienda', query.tienda);
    if (query.marca) params = params.set('marca', query.marca);
    if (query.material) params = params.set('material', query.material);

    return this.http.get<PagedResult<MaterialImpulsoAdmin>>(this.endpoint, { params });
  }

  exportExcel(query: MaterialImpulsoQuery): Observable<Blob> {
    let params = new HttpParams()
      .set('soloActivos', query.soloActivos ?? true);

    if (query.tienda) params = params.set('tienda', query.tienda);
    if (query.marca) params = params.set('marca', query.marca);
    if (query.material) params = params.set('material', query.material);

    return this.http.get(`${this.endpoint}/exportar`, { params, responseType: 'blob' });
  }

  create(payload: CreateMaterialImpulsoPayload): Observable<MaterialImpulsoAdmin> {
    return this.http.post<MaterialImpulsoAdmin>(this.endpoint, payload);
  }

  update(id: number, payload: UpdateMaterialImpulsoPayload): Observable<MaterialImpulsoAdmin> {
    return this.http.put<MaterialImpulsoAdmin>(`${this.endpoint}/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.endpoint}/${id}`);
  }
}

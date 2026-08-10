import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { PagedResult } from '../models/paged-result.model';
import {
  CreateTiendaPayload,
  Tienda,
  TiendaQuery,
  UpdateTiendaPayload
} from '../models/tienda.model';

@Injectable({ providedIn: 'root' })
export class TiendasService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${API_BASE_URL}/tiendas`;

  list(query: TiendaQuery): Observable<PagedResult<Tienda>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);

    if (query.cadena) {
      params = params.set('cadena', query.cadena);
    }

    if (query.marca) {
      params = params.set('marca', query.marca);
    }

    if (query.region) {
      params = params.set('region', query.region);
    }

    if (query.nombre) {
      params = params.set('nombre', query.nombre);
    }

    if (query.codigoTiendaB2B) {
      params = params.set('codigoTiendaB2B', query.codigoTiendaB2B);
    }

    if (query.soloConMaterialActivo !== undefined) {
      params = params.set('soloConMaterialActivo', query.soloConMaterialActivo);
    }

    return this.http.get<PagedResult<Tienda>>(this.endpoint, { params });
  }

  create(payload: CreateTiendaPayload): Observable<Tienda> {
    return this.http.post<Tienda>(this.endpoint, payload);
  }

  update(id: string, payload: UpdateTiendaPayload): Observable<Tienda> {
    return this.http.put<Tienda>(`${this.endpoint}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.endpoint}/${id}`);
  }
}

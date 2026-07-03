import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { PagedResult } from '../models/paged-result.model';
import { CreateCuotaPayload, Cuota, CuotaQuery, UpdateCuotaPayload } from '../models/cuota.model';

@Injectable({ providedIn: 'root' })
export class CuotasService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${API_BASE_URL}/cuotas`;

  list(query: CuotaQuery): Observable<PagedResult<Cuota>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);

    if (query.campania) {
      params = params.set('campania', query.campania);
    }

    if (query.tiendaCadenaKey) {
      params = params.set('tiendaCadenaKey', query.tiendaCadenaKey);
    }

    if (query.fecha) {
      params = params.set('fecha', query.fecha);
    }

    return this.http.get<PagedResult<Cuota>>(this.endpoint, { params });
  }

  create(payload: CreateCuotaPayload): Observable<Cuota> {
    return this.http.post<Cuota>(this.endpoint, payload);
  }

  update(id: number, payload: UpdateCuotaPayload): Observable<Cuota> {
    return this.http.put<Cuota>(`${this.endpoint}/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.endpoint}/${id}`);
  }
}

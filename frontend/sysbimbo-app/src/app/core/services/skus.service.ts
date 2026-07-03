import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { PagedResult } from '../models/paged-result.model';
import { SkuCatalogo } from '../models/sku-catalogo.model';
import { CreateSkuPayload, Sku, SkuQuery, UpdateSkuPayload } from '../models/sku.model';

@Injectable({ providedIn: 'root' })
export class SkusService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${API_BASE_URL}/skus`;

  list(query: SkuQuery): Observable<PagedResult<Sku>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);

    if (query.categoria) {
      params = params.set('categoria', query.categoria);
    }

    if (query.marca) {
      params = params.set('marca', query.marca);
    }

    if (query.nombre) {
      params = params.set('nombre', query.nombre);
    }

    if (query.codigoSkuB2B) {
      params = params.set('codigoSkuB2B', query.codigoSkuB2B);
    }

    if (query.codigoSkuBimbo) {
      params = params.set('codigoSkuBimbo', query.codigoSkuBimbo);
    }

    return this.http.get<PagedResult<Sku>>(this.endpoint, { params });
  }

  listCatalogo(query: SkuQuery): Observable<PagedResult<SkuCatalogo>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);

    if (query.categoria) {
      params = params.set('categoria', query.categoria);
    }

    if (query.marca) {
      params = params.set('marca', query.marca);
    }

    if (query.nombre) {
      params = params.set('nombre', query.nombre);
    }

    if (query.codigoSkuB2B) {
      params = params.set('codigoSkuB2B', query.codigoSkuB2B);
    }

    if (query.codigoSkuBimbo) {
      params = params.set('codigoSkuBimbo', query.codigoSkuBimbo);
    }

    return this.http.get<PagedResult<SkuCatalogo>>(`${this.endpoint}/catalogo`, { params });
  }

  create(payload: CreateSkuPayload): Observable<Sku> {
    return this.http.post<Sku>(this.endpoint, payload);
  }

  update(id: string, payload: UpdateSkuPayload): Observable<Sku> {
    return this.http.put<Sku>(`${this.endpoint}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.endpoint}/${id}`);
  }
}

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  AddCampaniaFechasPayload,
  AddCampaniaSkusPayload,
  AddCampaniaTiendasPayload,
  CampaniaOperacionResultado
} from '../models/campania-operacion.model';
import { Campania, CampaniaQuery, CreateCampaniaPayload, UpdateCampaniaPayload } from '../models/campania.model';
import { CampaniaFecha } from '../models/campania-fecha.model';
import { CampaniaProgramacionDetalle } from '../models/campania-programacion-detalle.model';
import { CampaniaProgramacion } from '../models/campania-programacion.model';
import { CampaniaResumen } from '../models/campania-resumen.model';
import { CampaniaSku } from '../models/campania-sku.model';
import { CampaniaTienda } from '../models/campania-tienda.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class CampaniasService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${API_BASE_URL}/campanias`;

  list(query: CampaniaQuery): Observable<PagedResult<Campania>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);

    if (query.nombreCampania) {
      params = params.set('nombreCampania', query.nombreCampania);
    }

    if (query.descripcion) {
      params = params.set('descripcion', query.descripcion);
    }

    if (query.estado) {
      params = params.set('estado', query.estado);
    }

    return this.http.get<PagedResult<Campania>>(this.endpoint, { params });
  }

  create(payload: CreateCampaniaPayload): Observable<Campania> {
    return this.http.post<Campania>(this.endpoint, payload);
  }

  update(id: number, payload: UpdateCampaniaPayload): Observable<Campania> {
    return this.http.put<Campania>(`${this.endpoint}/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.endpoint}/${id}`);
  }

  getResumen(campaniaId: number): Observable<CampaniaResumen> {
    return this.http.get<CampaniaResumen>(`${this.endpoint}/${campaniaId}/resumen`);
  }

  getTiendas(campaniaId: number): Observable<CampaniaTienda[]> {
    return this.http.get<CampaniaTienda[]>(`${this.endpoint}/${campaniaId}/tiendas`);
  }

  addTiendas(campaniaId: number, payload: AddCampaniaTiendasPayload): Observable<CampaniaOperacionResultado> {
    return this.http.post<CampaniaOperacionResultado>(`${this.endpoint}/${campaniaId}/tiendas`, payload);
  }

  removeTienda(campaniaId: number, tiendaCadenaKey: string): Observable<CampaniaOperacionResultado> {
    return this.http.delete<CampaniaOperacionResultado>(
      `${this.endpoint}/${campaniaId}/tiendas/${encodeURIComponent(tiendaCadenaKey)}`
    );
  }

  getFechas(campaniaId: number): Observable<CampaniaFecha[]> {
    return this.http.get<CampaniaFecha[]>(`${this.endpoint}/${campaniaId}/fechas`);
  }

  addFechas(campaniaId: number, payload: AddCampaniaFechasPayload): Observable<CampaniaOperacionResultado> {
    return this.http.post<CampaniaOperacionResultado>(`${this.endpoint}/${campaniaId}/fechas`, payload);
  }

  removeFecha(campaniaId: number, fecha: string): Observable<CampaniaOperacionResultado> {
    return this.http.delete<CampaniaOperacionResultado>(`${this.endpoint}/${campaniaId}/fechas/${fecha}`);
  }

  getSkus(campaniaId: number): Observable<CampaniaSku[]> {
    return this.http.get<CampaniaSku[]>(`${this.endpoint}/${campaniaId}/skus`);
  }

  addSkus(campaniaId: number, payload: AddCampaniaSkusPayload): Observable<CampaniaOperacionResultado> {
    return this.http.post<CampaniaOperacionResultado>(`${this.endpoint}/${campaniaId}/skus`, payload);
  }

  removeSku(campaniaId: number, codigoSkuBimbo: string): Observable<CampaniaOperacionResultado> {
    return this.http.delete<CampaniaOperacionResultado>(
      `${this.endpoint}/${campaniaId}/skus/${encodeURIComponent(codigoSkuBimbo)}`
    );
  }

  getProgramaciones(campaniaId: number): Observable<CampaniaProgramacion[]> {
    return this.http.get<CampaniaProgramacion[]>(`${this.endpoint}/${campaniaId}/programaciones`);
  }

  getDetalles(campaniaId: number, programacionId: number): Observable<CampaniaProgramacionDetalle[]> {
    return this.http.get<CampaniaProgramacionDetalle[]>(
      `${this.endpoint}/${campaniaId}/programaciones/${programacionId}/detalles`
    );
  }
}

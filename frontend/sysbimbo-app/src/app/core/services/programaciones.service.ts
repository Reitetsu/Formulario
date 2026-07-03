import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { DetalleProgramacion } from '../models/detalle-programacion.model';
import { PagedResult } from '../models/paged-result.model';
import {
  CreateProgramacionPayload,
  Programacion,
  ProgramacionQuery,
  UpdateProgramacionPayload
} from '../models/programacion.model';

@Injectable({ providedIn: 'root' })
export class ProgramacionesService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${API_BASE_URL}/programaciones`;

  list(query: ProgramacionQuery): Observable<PagedResult<Programacion>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);

    if (query.nombreCampania) {
      params = params.set('nombreCampania', query.nombreCampania);
    }

    if (query.nombreTiendaBimbo) {
      params = params.set('nombreTiendaBimbo', query.nombreTiendaBimbo);
    }

    if (query.fecha) {
      params = params.set('fecha', query.fecha);
    }

    if (query.cuota !== undefined && query.cuota !== null) {
      params = params.set('cuota', query.cuota);
    }

    if (query.estado) {
      params = params.set('estado', query.estado);
    }

    return this.http.get<PagedResult<Programacion>>(this.endpoint, { params });
  }

  create(payload: CreateProgramacionPayload): Observable<Programacion> {
    return this.http.post<Programacion>(this.endpoint, payload);
  }

  update(id: number, payload: UpdateProgramacionPayload): Observable<Programacion> {
    return this.http.put<Programacion>(`${this.endpoint}/${id}`, payload);
  }

  getDetail(id: number): Observable<DetalleProgramacion[]> {
    return this.http.get<DetalleProgramacion[]>(`${this.endpoint}/${id}/detalle`);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.endpoint}/${id}`);
  }
}

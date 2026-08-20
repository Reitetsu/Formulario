import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  SupervisorAttendance,
  SupervisorPanel,
  UpdateSupervisorAttendance
} from '../models/supervisor-panel.model';

@Injectable({ providedIn: 'root' })
export class SupervisorPanelService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${API_BASE_URL}/supervisores`;

  getPanel(): Observable<SupervisorPanel> {
    return this.http.get<SupervisorPanel>(`${this.endpoint}/panel`, {
      withCredentials: true
    });
  }

  updateAttendance(payload: UpdateSupervisorAttendance): Observable<SupervisorAttendance> {
    return this.http.put<SupervisorAttendance>(
      `${this.endpoint}/asistencia-hoy`,
      payload,
      { withCredentials: true }
    );
  }
}

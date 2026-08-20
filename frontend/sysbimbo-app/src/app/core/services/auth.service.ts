import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { AuthUser, LoginCredentials } from '../models/auth-user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${API_BASE_URL}/auth`;
  private readonly currentUserSubject = new BehaviorSubject<AuthUser | null>(null);

  readonly currentUser$ = this.currentUserSubject.asObservable();

  login(credentials: LoginCredentials): Observable<AuthUser> {
    return this.http
      .post<AuthUser>(`${this.endpoint}/login`, credentials, { withCredentials: true })
      .pipe(tap(user => this.currentUserSubject.next(user)));
  }

  getSession(): Observable<AuthUser> {
    return this.http
      .get<AuthUser>(`${this.endpoint}/me`, { withCredentials: true })
      .pipe(tap(user => this.currentUserSubject.next(user)));
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${this.endpoint}/logout`, {}, { withCredentials: true })
      .pipe(tap(() => this.currentUserSubject.next(null)));
  }
}

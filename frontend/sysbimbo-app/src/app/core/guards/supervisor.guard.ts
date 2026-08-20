import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const supervisorGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const loginUrl = () => router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });

  return authService.getSession().pipe(
    map(user => user.roles.some(role => role === 'Administrador' || role === 'Supervisor')
      ? true
      : router.createUrlTree(['/canjes_Agosto'])),
    catchError(() => of(loginUrl()))
  );
};

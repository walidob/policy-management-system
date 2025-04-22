import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree, RedirectCommand } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, take, catchError, Observable, of } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.checkAuth().pipe(
    take(1),
    map((response) => {
      if (authService.isLoggedIn()) {
        return true;
      }

      const urlTree: UrlTree = router.createUrlTree(['/login'], {
        queryParams: { returnUrl: state.url },
      });

      return new RedirectCommand(urlTree);
    }),
    catchError((error) => {
      const urlTree = router.createUrlTree(['/login']);
      return of(new RedirectCommand(urlTree));
    })
  );
};

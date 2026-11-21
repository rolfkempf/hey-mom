import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@app/services/auth.service';

export const authGuard: CanActivateFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.hasRestoredSession()) await auth.restoreSession();
  return auth.currentUser() ? true : router.createUrlTree(['/login']);
};

import type { Routes } from '@angular/router';

export const routes: Routes = [
  { path: 'register', loadComponent: () => import('@app/pages/register/register.page').then(({ RegisterPage }) => RegisterPage) },
  { path: 'login', loadComponent: () => import('@app/pages/login/login.page').then(({ LoginPage }) => LoginPage) },
  { path: 'forgot-password', loadComponent: () => import('@app/pages/forgot-password/forgot-password.page').then(({ ForgotPasswordPage }) => ForgotPasswordPage) },
  { path: 'reset-password', loadComponent: () => import('@app/pages/reset-password/reset-password.page').then(({ ResetPasswordPage }) => ResetPasswordPage) },
  { path: 'confirm-email', loadComponent: () => import('@app/pages/confirm-email/confirm-email.page').then(({ ConfirmEmailPage }) => ConfirmEmailPage) },
  { path: 'confirm-email-change', loadComponent: () => import('@app/pages/confirm-email-change/confirm-email-change.page').then(({ ConfirmEmailChangePage }) => ConfirmEmailChangePage) },
  { path: 'account', loadComponent: () => import('@app/pages/account/account.page').then(({ AccountPage }) => AccountPage) },
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('@app/pages/home/home.page').then(({ HomePage }) => HomePage),
  },
];

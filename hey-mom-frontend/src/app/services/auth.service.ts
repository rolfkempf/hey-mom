import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import type { components } from '@app/infrastructure/api/openapi';

type AuthSchemas = components['schemas'];
export type CurrentUser = AuthSchemas['CurrentUserResponseDto'];

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private sessionRestoration: Promise<void> | null = null;
  readonly currentUser = signal<CurrentUser | null>(null);
  readonly isRestoringSession = signal(true);
  private readonly hasRestoredSessionState = signal(false);

  hasRestoredSession(): boolean {
    return this.hasRestoredSessionState();
  }

  restoreSession(): Promise<void> {
    if (this.sessionRestoration) return this.sessionRestoration;
    this.sessionRestoration = this.loadSession(false).finally(() => {
      this.sessionRestoration = null;
    });
    return this.sessionRestoration;
  }

  private async loadSession(throwOnFailure = false): Promise<void> {
    this.isRestoringSession.set(true);
    try {
      await this.requestToken();
      this.currentUser.set(
        await firstValueFrom(this.http.get<CurrentUser>('/api/auth/me', { withCredentials: true })),
      );
    } catch (error) {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
        console.error('Die Sitzung konnte nicht geladen werden.');
      }
      this.currentUser.set(null);
      if (throwOnFailure) throw error;
    } finally {
      this.hasRestoredSessionState.set(true);
      this.isRestoringSession.set(false);
    }
  }

  async register(request: AuthSchemas['RegisterRequestDto']): Promise<void> {
    await this.write('/api/auth/register', request);
  }

  async resendConfirmation(request: AuthSchemas['ResendConfirmationRequestDto']): Promise<void> {
    await this.write('/api/auth/resend-confirmation', request);
  }

  async confirmEmail(request: AuthSchemas['ConfirmEmailRequestDto']): Promise<void> {
    await this.write('/api/auth/confirm-email', request);
  }

  async login(request: AuthSchemas['LoginRequestDto']): Promise<void> {
    await this.write('/api/auth/login', request);
    await this.loadSession(true);
  }

  async logout(): Promise<void> {
    await this.write('/api/auth/logout', {});
    this.currentUser.set(null);
    await this.requestToken();
  }

  async requestPasswordReset(request: AuthSchemas['ForgotPasswordRequestDto']): Promise<void> {
    await this.write('/api/auth/forgot-password', request);
  }

  async resetPassword(request: AuthSchemas['ResetPasswordRequestDto']): Promise<void> {
    await this.write('/api/auth/reset-password', request);
  }

  async changePassword(request: AuthSchemas['ChangePasswordRequestDto']): Promise<void> {
    await this.write('/api/auth/change-password', request);
  }

  async requestEmailChange(request: AuthSchemas['RequestEmailChangeRequestDto']): Promise<void> {
    await this.write('/api/auth/request-email-change', request);
  }

  async confirmEmailChange(request: AuthSchemas['ConfirmEmailChangeRequestDto']): Promise<void> {
    await this.write('/api/auth/confirm-email-change', request);
    await this.restoreSession();
  }

  private async requestToken(): Promise<string> {
    const response = await firstValueFrom(
      this.http.get<AuthSchemas['CsrfResponseDto']>('/api/auth/csrf', { withCredentials: true }),
    );
    return response.requestToken;
  }

  private async write(path: string, body: object): Promise<void> {
    const requestToken = await this.requestToken();
    await firstValueFrom(
      this.http.post<void>(path, body, {
        withCredentials: true,
        headers: new HttpHeaders({ 'X-CSRF-TOKEN': requestToken }),
      }),
    );
  }
}

export function authErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 429) return 'Zu viele Versuche. Bitte warte kurz und versuche es erneut.';
    if (error.status === 0)
      return 'Der Dienst ist gerade nicht erreichbar. Bitte versuche es später erneut.';
    if (error.status === 400)
      return 'Die Angaben konnten nicht übernommen werden. Bitte prüfe sie und versuche es erneut.';
  }
  return 'Das hat gerade nicht funktioniert. Bitte versuche es erneut.';
}

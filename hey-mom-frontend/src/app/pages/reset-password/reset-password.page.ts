import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HlmButton } from '@spartan-ng/helm/button';
import { AuthService, authErrorMessage } from '@app/services/auth.service';

@Component({
  selector: 'app-reset-password-page',
  imports: [FormsModule, RouterLink, HlmButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section class="mx-auto max-w-md space-y-6">
    <h1 class="text-3xl font-semibold tracking-tight">Neues Passwort festlegen</h1>
    @if (!hasToken) {
      <p role="alert">Der Link ist unvollständig. Fordere einen neuen Link an.</p>
      <a class="underline" routerLink="/forgot-password">Passwort zurücksetzen</a>
    } @else if (done()) {
      <p role="status">Dein Passwort wurde geändert. Du kannst dich jetzt anmelden.</p>
      <a class="underline" routerLink="/login">Anmelden</a>
    } @else {
      <form class="space-y-4" (ngSubmit)="submit()">
        <label class="block space-y-1"
          >Neues Passwort<input
            class="w-full rounded-md border px-3 py-2"
            type="password"
            name="password"
            [(ngModel)]="password"
            autocomplete="new-password"
            minlength="12"
            required /></label
        ><label class="block space-y-1"
          >Passwort wiederholen<input
            class="w-full rounded-md border px-3 py-2"
            type="password"
            name="confirm"
            [(ngModel)]="confirm"
            autocomplete="new-password"
            required
        /></label>
        @if (message()) {
          <p role="alert">{{ message() }}</p>
          <a class="underline" routerLink="/forgot-password">Neuen Link anfordern</a>
        }
        <button hlmBtn class="w-full" [disabled]="busy()">Passwort speichern</button>
      </form>
    }
  </section>`,
})
export class ResetPasswordPage {
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly fragment = new URLSearchParams(this.route.snapshot.fragment ?? '');
  readonly email = this.fragment.get('email') ?? '';
  readonly code = this.fragment.get('code') ?? '';
  readonly hasToken = !!this.email && !!this.code;
  password = '';
  confirm = '';
  readonly busy = signal(false);
  readonly done = signal(false);
  readonly message = signal('');
  async submit(): Promise<void> {
    if (this.password !== this.confirm) {
      this.message.set('Die Passwörter stimmen nicht überein.');
      return;
    }
    this.busy.set(true);
    this.message.set('');
    try {
      await this.auth.resetPassword({
        email: this.email,
        code: this.code,
        newPassword: this.password,
      });
      this.done.set(true);
    } catch (error) {
      this.message.set(
        this.isInvalidReset(error)
          ? 'Der Link ist ungültig oder abgelaufen. Fordere einen neuen Link an.'
          : authErrorMessage(error),
      );
    } finally {
      this.busy.set(false);
    }
  }

  private isInvalidReset(error: unknown): boolean {
    if (
      !(error instanceof HttpErrorResponse) ||
      error.status !== 400 ||
      typeof error.error !== 'object' ||
      error.error === null ||
      !('errors' in error.error)
    ) {
      return false;
    }
    return (
      typeof error.error.errors === 'object' &&
      error.error.errors !== null &&
      'InvalidReset' in error.error.errors
    );
  }
}

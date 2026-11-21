import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { HlmButton } from '@spartan-ng/helm/button';
import { AuthService, authErrorMessage } from '@app/services/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [FormsModule, RouterLink, HlmButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="mx-auto max-w-md space-y-6">
      <div>
        <h1 class="text-3xl font-semibold tracking-tight">Anmelden</h1>
        <p class="mt-2 text-muted-foreground">Melde dich bei Hey Mom an.</p>
      </div>
      <form class="space-y-4" (ngSubmit)="submit()">
        <label class="block space-y-1"
          >E-Mail-Adresse<input
            class="w-full rounded-md border px-3 py-2"
            type="email"
            name="email"
            [(ngModel)]="email"
            autocomplete="email"
            required
        /></label>
        <label class="block space-y-1"
          >Passwort<input
            class="w-full rounded-md border px-3 py-2"
            type="password"
            name="password"
            [(ngModel)]="password"
            autocomplete="current-password"
            required
        /></label>
        @if (message()) {
          <p role="alert" class="text-destructive">{{ message() }}</p>
        }
        <button hlmBtn class="w-full" [disabled]="busy()">
          {{ busy() ? 'Anmeldung läuft …' : 'Anmelden' }}
        </button>
      </form>
      <a class="block underline" routerLink="/forgot-password">Passwort vergessen?</a>
      <a class="block underline" routerLink="/confirm-email"
        >Bestätigungs-E-Mail erneut anfordern</a
      >
      <p>Noch kein Konto? <a class="underline" routerLink="/register">Konto erstellen</a></p>
    </section>
  `,
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  email = '';
  password = '';
  readonly busy = signal(false);
  readonly message = signal('');
  async submit(): Promise<void> {
    this.busy.set(true);
    this.message.set('');
    try {
      await this.auth.login({ email: this.email, password: this.password });
      await this.router.navigateByUrl('/account');
    } catch (error) {
      this.message.set(
        error instanceof HttpErrorResponse && error.status === 401
          ? 'E-Mail-Adresse oder Passwort ist falsch, oder die Adresse wurde noch nicht bestätigt.'
          : authErrorMessage(error),
      );
    } finally {
      this.busy.set(false);
    }
  }
}

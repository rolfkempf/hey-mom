import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HlmButton } from '@spartan-ng/helm/button';
import { AuthService, authErrorMessage } from '@app/services/auth.service';

@Component({
  selector: 'app-register-page',
  imports: [FormsModule, RouterLink, HlmButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="mx-auto max-w-md space-y-6">
      <div>
        <h1 class="text-3xl font-semibold tracking-tight">Konto erstellen</h1>
        <p class="mt-2 text-muted-foreground">Lege dein Hey-Mom-Konto an.</p>
      </div>
      @if (sent()) {
        <div role="status" class="rounded-lg border p-4">
          Falls diese Adresse registriert werden kann, erhältst du eine E-Mail mit dem nächsten
          Schritt.
        </div>
        <button hlmBtn variant="outline" type="button" (click)="resend()" [disabled]="busy()">
          Bestätigungs-E-Mail erneut senden
        </button>
        @if (message()) {
          <p role="status">{{ message() }}</p>
        }
      } @else {
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
              autocomplete="new-password"
              minlength="12"
              required
          /></label>
          @if (message()) {
            <p role="alert" class="text-destructive">{{ message() }}</p>
          }
          <button hlmBtn class="w-full" [disabled]="busy()">
            {{ busy() ? 'Wird erstellt …' : 'Konto erstellen' }}
          </button>
        </form>
      }
      <p>Schon registriert? <a class="underline" routerLink="/login">Anmelden</a></p>
    </section>
  `,
})
export class RegisterPage {
  private readonly auth = inject(AuthService);
  email = '';
  password = '';
  readonly busy = signal(false);
  readonly sent = signal(false);
  readonly message = signal('');

  async submit(): Promise<void> {
    this.busy.set(true);
    this.message.set('');
    try {
      await this.auth.register({ email: this.email, password: this.password });
      this.sent.set(true);
    } catch (error) {
      this.message.set(authErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }

  async resend(): Promise<void> {
    this.busy.set(true);
    this.message.set('');
    try {
      await this.auth.resendConfirmation({ email: this.email });
      this.message.set('Falls diese Adresse registriert werden kann, wurde eine E-Mail versendet.');
    } catch (error) {
      this.message.set(authErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}

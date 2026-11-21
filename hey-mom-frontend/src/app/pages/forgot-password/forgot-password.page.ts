import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HlmButton } from '@spartan-ng/helm/button';
import { AuthService, authErrorMessage } from '@app/services/auth.service';

@Component({
  selector: 'app-forgot-password-page',
  imports: [FormsModule, RouterLink, HlmButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section class="mx-auto max-w-md space-y-6">
    <div>
      <h1 class="text-3xl font-semibold tracking-tight">Passwort zurücksetzen</h1>
      <p class="mt-2 text-muted-foreground">
        Wir senden dir einen Link, falls ein Konto zu dieser Adresse gehört.
      </p>
    </div>
    @if (sent()) {
      <p role="status" class="rounded-lg border p-4">
        Falls ein Konto zu dieser Adresse gehört, erhältst du eine E-Mail mit dem Link zum
        Zurücksetzen.
      </p>
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
        @if (message()) {
          <p role="alert">{{ message() }}</p>
        }
        <button hlmBtn class="w-full" [disabled]="busy()">Link anfordern</button>
      </form>
    }
    <a class="underline" routerLink="/login">Zur Anmeldung</a>
  </section>`,
})
export class ForgotPasswordPage {
  private readonly auth = inject(AuthService);
  email = '';
  readonly busy = signal(false);
  readonly sent = signal(false);
  readonly message = signal('');
  async submit(): Promise<void> {
    this.busy.set(true);
    this.message.set('');
    try {
      await this.auth.requestPasswordReset({ email: this.email });
      this.sent.set(true);
    } catch (error) {
      this.message.set(authErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}

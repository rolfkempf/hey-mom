import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HlmButton } from '@spartan-ng/helm/button';
import { AuthService, authErrorMessage } from '@app/services/auth.service';
@Component({
  selector: 'app-confirm-email-page',
  imports: [FormsModule, RouterLink, HlmButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section class="mx-auto max-w-md space-y-6">
    <h1 class="text-3xl font-semibold tracking-tight">E-Mail bestätigen</h1>
    @if (message()) {
      <p role="status">{{ message() }}</p>
    }
    @if (!done() && hasToken) {
      <button hlmBtn (click)="confirm()" [disabled]="busy()">E-Mail bestätigen</button>
    }
    @if (!done()) {
      <form class="space-y-3" (ngSubmit)="resend()">
        <label class="block space-y-1"
          >Registrierungs-E-Mail<input
            class="w-full rounded-md border px-3 py-2"
            type="email"
            name="email"
            [(ngModel)]="email"
            autocomplete="email"
            required /></label
        ><button hlmBtn variant="outline" [disabled]="busy()">
          Bestätigungs-E-Mail erneut senden
        </button>
      </form>
    }
    <a class="block underline" routerLink="/login">Zur Anmeldung</a>
  </section>`,
})
export class ConfirmEmailPage {
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly fragment = new URLSearchParams(this.route.snapshot.fragment ?? '');
  private readonly hasCallbackData = this.fragment.toString().length > 0;
  private readonly userId = this.fragment.get('userId') ?? '';
  private readonly code = this.fragment.get('code') ?? '';
  readonly hasToken = !!this.userId && !!this.code;
  readonly busy = signal(false);
  readonly done = signal(false);
  readonly message = signal(
    this.hasToken
      ? 'Bestätige deine E-Mail-Adresse.'
      : this.hasCallbackData
        ? 'Der Bestätigungslink ist unvollständig. Bitte fordere eine neue E-Mail an.'
        : 'Fordere eine neue Bestätigungs-E-Mail an.',
  );
  email = '';
  async confirm(): Promise<void> {
    this.busy.set(true);
    try {
      await this.auth.confirmEmail({ userId: this.userId, code: this.code });
      this.done.set(true);
      this.message.set('Deine E-Mail-Adresse ist bestätigt. Du kannst dich jetzt anmelden.');
    } catch (error) {
      this.message.set(authErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
  async resend(): Promise<void> {
    this.busy.set(true);
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

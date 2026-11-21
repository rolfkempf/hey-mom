import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HlmButton } from '@spartan-ng/helm/button';
import { AuthService, authErrorMessage } from '@app/services/auth.service';
@Component({
  selector: 'app-confirm-email-change-page',
  imports: [RouterLink, HlmButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section class="mx-auto max-w-md space-y-6">
    <h1 class="text-3xl font-semibold tracking-tight">Neue E-Mail-Adresse bestätigen</h1>
    <p>{{ message() }}</p>
    @if (!done() && hasToken) {
      <button hlmBtn (click)="confirm()" [disabled]="busy()">E-Mail-Adresse bestätigen</button>
    }
    <a class="block underline" routerLink="/account">Zum Konto</a>
  </section>`,
})
export class ConfirmEmailChangePage {
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly fragment = new URLSearchParams(this.route.snapshot.fragment ?? '');
  private readonly userId = this.fragment.get('userId') ?? '';
  private readonly newEmail = this.fragment.get('newEmail') ?? '';
  private readonly code = this.fragment.get('code') ?? '';
  readonly hasToken = !!this.userId && !!this.newEmail && !!this.code;
  readonly busy = signal(false);
  readonly done = signal(false);
  readonly message = signal(
    this.hasToken
      ? 'Bestätige die neue E-Mail-Adresse.'
      : 'Der Bestätigungslink ist unvollständig.',
  );
  async confirm(): Promise<void> {
    this.busy.set(true);
    try {
      await this.auth.confirmEmailChange({
        userId: this.userId,
        newEmail: this.newEmail,
        code: this.code,
      });
      this.done.set(true);
      this.message.set('Die neue E-Mail-Adresse ist bestätigt.');
    } catch (error) {
      this.message.set(authErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}

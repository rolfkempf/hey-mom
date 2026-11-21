import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HlmButton } from '@spartan-ng/helm/button';
import { AuthService, authErrorMessage } from '@app/services/auth.service';

@Component({
  selector: 'app-account-page',
  imports: [FormsModule, HlmButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section class="mx-auto max-w-xl space-y-8">
    <div>
      <h1 class="text-3xl font-semibold tracking-tight">Dein Konto</h1>
      <p class="mt-2">Angemeldet als {{ auth.currentUser()?.email }}</p>
    </div>
    <form class="space-y-4 rounded-xl border p-5" (ngSubmit)="changePassword()">
      <h2 class="text-xl font-semibold">Passwort ändern</h2>
      <label class="block space-y-1"
        >Aktuelles Passwort<input
          class="w-full rounded-md border px-3 py-2"
          type="password"
          name="currentPassword"
          [(ngModel)]="currentPassword"
          autocomplete="current-password"
          required /></label
      ><label class="block space-y-1"
        >Neues Passwort<input
          class="w-full rounded-md border px-3 py-2"
          type="password"
          name="newPassword"
          [(ngModel)]="newPassword"
          autocomplete="new-password"
          minlength="12"
          required
      /></label>
      @if (passwordMessage()) {
        <p role="status">{{ passwordMessage() }}</p>
      }
      <button hlmBtn variant="outline" [disabled]="busy()">Passwort ändern</button>
    </form>
    <form class="space-y-4 rounded-xl border p-5" (ngSubmit)="requestEmailChange()">
      <h2 class="text-xl font-semibold">E-Mail-Adresse ändern</h2>
      <label class="block space-y-1"
        >Neue E-Mail-Adresse<input
          class="w-full rounded-md border px-3 py-2"
          type="email"
          name="newEmail"
          [(ngModel)]="newEmail"
          autocomplete="email"
          required /></label
      ><label class="block space-y-1"
        >Aktuelles Passwort<input
          class="w-full rounded-md border px-3 py-2"
          type="password"
          name="emailCurrentPassword"
          [(ngModel)]="emailCurrentPassword"
          autocomplete="current-password"
          required
      /></label>
      @if (emailMessage()) {
        <p role="status">{{ emailMessage() }}</p>
      }
      <button hlmBtn variant="outline" [disabled]="busy()">Bestätigungs-E-Mail senden</button>
    </form>
    <button hlmBtn variant="outline" (click)="logout()" [disabled]="busy()">Abmelden</button>
  </section>`,
})
export class AccountPage {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  currentPassword = '';
  newPassword = '';
  newEmail = '';
  emailCurrentPassword = '';
  readonly busy = signal(false);
  readonly passwordMessage = signal('');
  readonly emailMessage = signal('');
  async changePassword(): Promise<void> {
    this.busy.set(true);
    this.passwordMessage.set('');
    try {
      await this.auth.changePassword({
        currentPassword: this.currentPassword,
        newPassword: this.newPassword,
      });
      this.currentPassword = '';
      this.newPassword = '';
      this.passwordMessage.set('Dein Passwort wurde geändert.');
    } catch (error) {
      this.passwordMessage.set(authErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
  async requestEmailChange(): Promise<void> {
    this.busy.set(true);
    this.emailMessage.set('');
    try {
      await this.auth.requestEmailChange({
        newEmail: this.newEmail,
        currentPassword: this.emailCurrentPassword,
      });
      this.emailMessage.set(
        'Falls die Adresse verwendet werden kann, erhältst du eine Bestätigungs-E-Mail.',
      );
    } catch (error) {
      this.emailMessage.set(authErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
  async logout(): Promise<void> {
    this.busy.set(true);
    try {
      await this.auth.logout();
      await this.router.navigateByUrl('/login');
    } catch (error) {
      this.emailMessage.set(authErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}

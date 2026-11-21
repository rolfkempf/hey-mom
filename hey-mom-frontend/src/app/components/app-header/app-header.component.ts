import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '@app/services/auth.service';
import { HlmButton } from '@spartan-ng/helm/button';

@Component({
  selector: 'app-header',
  imports: [HlmButton, RouterLink],
  template: `
    <header class="border-b">
      <nav
        class="mx-auto flex max-w-5xl items-center justify-between px-4 py-3"
        aria-label="Hauptnavigation"
      >
        <a hlmBtn variant="outline" href="/">Website</a>
        <div class="flex gap-2">
          @if (auth.currentUser(); as user) {
            <a hlmBtn variant="outline" routerLink="/account">{{ user.email }}</a>
          } @else {
            <a hlmBtn variant="outline" routerLink="/login">Anmelden</a
            ><a hlmBtn routerLink="/register">Konto erstellen</a>
          }
        </div>
      </nav>
    </header>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppHeaderComponent {
  protected readonly auth = inject(AuthService);
}

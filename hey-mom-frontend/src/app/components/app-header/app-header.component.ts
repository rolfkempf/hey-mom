import { ChangeDetectionStrategy, Component } from '@angular/core';
import { HlmButton } from '@spartan-ng/helm/button';

@Component({
  selector: 'app-header',
  imports: [HlmButton],
  template: `
    <header class="border-b">
      <nav class="mx-auto flex max-w-5xl px-4 py-3" aria-label="Main navigation">
        <a hlmBtn variant="outline" href="/">Website</a>
      </nav>
    </header>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppHeaderComponent {}

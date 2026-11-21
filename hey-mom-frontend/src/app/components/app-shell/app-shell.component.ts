import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppHeaderComponent } from '@app/components/app-header/app-header.component';

@Component({
  selector: 'app-root',
  imports: [AppHeaderComponent, RouterOutlet],
  template: `
    <app-header />
    <main class="mx-auto max-w-5xl px-4 py-8">
      <router-outlet />
    </main>
  `,
})
export class AppShellComponent {}

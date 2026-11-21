import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-home-page',
  template: `<h1 class="text-3xl font-semibold tracking-tight">Hey Mom</h1>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomePage {}

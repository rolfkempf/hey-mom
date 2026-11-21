import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { routes } from '@app/app.routes';
import { AppShellComponent } from './app-shell.component';

describe('AppShellComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppShellComponent],
      providers: [provideRouter(routes)],
    }).compileComponents();
  });

  it('renders the home page in the app shell', async () => {
    const fixture = TestBed.createComponent(AppShellComponent);
    fixture.detectChanges();
    await TestBed.inject(Router).navigateByUrl('/');
    await fixture.whenStable();

    const view = fixture.nativeElement as HTMLElement;
    expect(view.querySelector('main h1')?.textContent).toBe('Hey Mom');
    const websiteLink = view.querySelector('nav a');
    expect(websiteLink?.getAttribute('href')).toBe('/');
    expect(websiteLink?.classList.contains('spartan-button-variant-outline')).toBe(true);
  });
});

import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { routes } from '@app/app.routes';
import { AuthService, type CurrentUser } from '@app/services/auth.service';
import { AppShellComponent } from './app-shell.component';

describe('AppShellComponent', () => {
  const currentUser = signal<CurrentUser | null>(null);

  beforeEach(async () => {
    currentUser.set(null);
    await TestBed.configureTestingModule({
      imports: [AppShellComponent],
      providers: [
        provideRouter(routes),
        {
          provide: AuthService,
          useValue: {
            currentUser,
            restoreSession: () => Promise.resolve(),
            hasRestoredSession: () => true,
          },
        },
      ],
    }).compileComponents();
  });

  it('renders the app home for a signed-in user', async () => {
    currentUser.set({ id: 'account-id', email: 'person@example.test' });
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

  it('redirects signed-out visitors from the app home to login', async () => {
    const fixture = TestBed.createComponent(AppShellComponent);
    fixture.detectChanges();
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/');
    await fixture.whenStable();

    expect(router.url).toBe('/login');
    const view = fixture.nativeElement as HTMLElement;
    expect(view.querySelector('main h1')?.textContent).toBe('Anmelden');
  });
});

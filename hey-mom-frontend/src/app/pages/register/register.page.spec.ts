import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '@app/services/auth.service';
import { vi } from 'vitest';
import { RegisterPage } from './register.page';

describe('RegisterPage', () => {
  let register: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    register = vi.fn();
    TestBed.configureTestingModule({
      imports: [RegisterPage],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { register, resendConfirmation: vi.fn() } },
      ],
    });
  });

  it('shows the confirmation instructions after a successful registration', async () => {
    const fixture = TestBed.createComponent(RegisterPage);
    const page = fixture.componentInstance;
    page.email = 'person@example.test';
    page.password = 'A-long-test-password1!';

    await page.submit();
    fixture.detectChanges();

    expect(register).toHaveBeenCalledWith({
      email: 'person@example.test',
      password: 'A-long-test-password1!',
    });
    expect(fixture.nativeElement.textContent).toContain('erhältst du eine E-Mail');
  });

  it('keeps the registration form available and displays a recoverable error', async () => {
    register.mockRejectedValue(new Error('request failed'));
    const fixture = TestBed.createComponent(RegisterPage);
    const page = fixture.componentInstance;
    page.email = 'person@example.test';
    page.password = 'A-long-test-password1!';

    await page.submit();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Das hat gerade nicht funktioniert');
    expect(fixture.nativeElement.querySelector('form')).toBeTruthy();
  });
});

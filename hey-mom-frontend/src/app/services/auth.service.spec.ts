import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let http: HttpTestingController;
  let auth: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
  });

  afterEach(() => http.verify());

  it('fetches an antiforgery token and sends it with writes', async () => {
    const registration = auth.register({
      email: 'person@example.test',
      password: 'A-long-test-password1!',
    });
    const writeToken = http.expectOne('/api/auth/csrf');
    writeToken.flush({ requestToken: 'write-token' });
    await Promise.resolve();
    await Promise.resolve();
    const request = http.expectOne('/api/auth/register');
    expect(request.request.method).toBe('POST');
    expect(request.request.headers.get('X-CSRF-TOKEN')).toBe('write-token');
    expect(request.request.body).toEqual({
      email: 'person@example.test',
      password: 'A-long-test-password1!',
    });
    request.flush(null, { status: 204, statusText: 'No Content' });
    await registration;
  });

  it('surfaces a failed session lookup after the login endpoint succeeds', async () => {
    const login = auth.login({ email: 'person@example.test', password: 'A-long-test-password1!' });
    http.expectOne('/api/auth/csrf').flush({ requestToken: 'login-token' });
    await Promise.resolve();
    await Promise.resolve();
    http.expectOne('/api/auth/login').flush(null, { status: 204, statusText: 'No Content' });
    await Promise.resolve();
    await Promise.resolve();
    http.expectOne('/api/auth/csrf').flush({ requestToken: 'session-token' });
    await Promise.resolve();
    await Promise.resolve();
    http
      .expectOne('/api/auth/me')
      .flush({ title: 'Unavailable' }, { status: 503, statusText: 'Service Unavailable' });

    await expect(login).rejects.toMatchObject({ status: 503 });
    expect(auth.currentUser()).toBeNull();
  });
});

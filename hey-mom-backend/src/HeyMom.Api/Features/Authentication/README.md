# Authentication

Hey Mom uses ASP.NET Core Identity with EF Core and PostgreSQL. Identity
manages accounts, password hashes, and email tokens. Angular handles the
account screens and keeps the current account in memory. Traefik serves
Angular under `/web/` and the API under `/api/` on the same HTTPS origin.
The Astro site links to the Angular login and registration pages.

## Sign-in and session lifecycle

1. Angular gets a request token from `GET /api/auth/csrf` and submits the
   email and password to `POST /api/auth/login` with the `X-CSRF-TOKEN`
   header. Identity checks the password hash and requires a confirmed email
   address. The API returns `204` and sets `__Host-HeyMom.Session` on success.
2. Angular obtains a fresh CSRF token for the signed-in identity and calls
   `GET /api/auth/me`. The API returns the current account ID and email.
   The login page then navigates to the protected app home at `/web/`.
3. When the app loads again, Angular restores the account through
   `GET /api/auth/me`. An absent, expired, or invalid session yields
   `401`. The route guard sends signed-out visitors from `/web/` or
   `/web/account` to `/web/login`. Protected API endpoints enforce
   authorization on the server.
4. `POST /api/auth/logout` clears the session cookie. Angular clears the
   account from memory and gets a CSRF token for the signed-out identity.

The browser sends the session cookie automatically. It is `Secure`,
`HttpOnly`, and `SameSite=Lax`. Sign-in issues a browser-session cookie
with an eight-hour authentication ticket and a fixed expiration.
ASP.NET Core Data Protection protects the ticket and the email tokens.
A persistent key ring preserves both across API restarts.

Every auth POST validates the request token against the antiforgery cookie
and the current identity. The antiforgery cookie is `Secure`, `HttpOnly`,
and `SameSite=Strict`. The API also checks the Identity security stamp on
authenticated requests. Password resets and confirmed email changes
invalidate existing sessions. Changing a password from account settings
refreshes the session that made the change and invalidates older sessions.

The HTTP routes and access rules live in [AuthenticationEndpoints.cs](AuthenticationEndpoints.cs).
Angular coordinates requests and account state in [auth.service.ts](../../../../../hey-mom-frontend/src/app/services/auth.service.ts).

## Account flows

| Slice | API operations relative to `/api/auth` | Angular pages |
| --- | --- | --- |
| Registration | `POST /register`, `POST /resend-confirmation`, `POST /confirm-email` | `/web/register`, `/web/confirm-email` |
| Session | `GET /csrf`, `POST /login`, `GET /me`, `POST /logout` | `/web/login`, `/web/`, `/web/account` |
| Recovery | `POST /forgot-password`, `POST /reset-password` | `/web/forgot-password`, `/web/reset-password` |
| Account settings | `POST /change-password`, `POST /request-email-change`, `POST /confirm-email-change` | `/web/account`, `/web/confirm-email-change` |

The initial EF migration creates the Identity tables and a unique normalized
email index. Registration sends a confirmation link. An authenticated password
change or a request to replace the email address requires the current password.
An email change takes effect after the user confirms the new address.

Email tokens are valid for one hour. Links use the configured HTTPS origin
and carry their callback data in a URL fragment. The Angular page submits the
token to the API when the user confirms the action. A successful password
reset invalidates its token. Public confirmation-resend and password-reset
requests give the same response for known and unknown addresses. The SMTP
sender logs delivery failures by type. A user can request a fresh link.

Five failed password attempts on a confirmed account trigger a 15-minute
Identity lockout. In-process rate limits apply per route and client IP.
Mail-sending routes allow five requests per minute, other auth writes allow
twenty, and CSRF issuance allows 120.

## Ownership for future features

The Identity account ID exposed by `GET /api/auth/me` stays stable after an
email change. When implementing user-owned data, derive the owner ID from the
authenticated principal on the server and scope EF Core reads and writes
to that owner.

## Local verification

Use the [local account setup](../../../../../README.md#account-setup) to start
PostgreSQL and apply the migration. Register through Angular and open the
confirmation or reset email in [Mailpit](http://localhost:18025/). Angular's
development build also offers **Test-Zugangsdaten eintragen** on the login
page for the prepared account in the local development database. The button
fills the form, then the user submits it through the normal sign-in flow.

[GitHub issue #1](https://github.com/rolfkempf/hey-mom/issues/1) describes the
feature scope. See Microsoft Learn for
[Identity for SPAs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0)
and [ASP.NET Core antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0).

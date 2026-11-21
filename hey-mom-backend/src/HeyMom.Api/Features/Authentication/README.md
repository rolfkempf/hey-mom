# Authentication

[GitHub issue #1](https://github.com/rolfkempf/hey-mom/issues/1) tracks
public account registration and recovery. The API uses ASP.NET Core Identity
with PostgreSQL and exposes the account ID through `GET /api/auth/me`. Future
user-owned features take the owner ID from the authenticated principal and scope
their EF Core queries to that account.

| Slice | API operations | Angular pages |
| --- | --- | --- |
| Registration | `POST /register`, `POST /resend-confirmation`, `POST /confirm-email` | `/web/register`, `/web/confirm-email` |
| Session | `GET /csrf`, `POST /login`, `GET /me`, `POST /logout` | `/web/login`, protected `/web/account` |
| Recovery | `POST /forgot-password`, `POST /reset-password` | `/web/forgot-password`, `/web/reset-password` |
| Account settings | `POST /change-password`, `POST /request-email-change`, `POST /confirm-email-change` | `/web/account`, `/web/confirm-email-change` |

API paths in the table are relative to `/api/auth`. The Astro site's account
link points to Angular registration.

- The initial EF migration creates the Identity tables and a unique normalized
  email index. The account ID remains stable after an email change.
- Identity owns password hashing, email verification, one-hour email tokens,
  lockout and security stamps. Password or email changes invalidate older
  sessions. Changing a password from account settings preserves the initiating
  session.
- The same-origin session cookie is Secure, HttpOnly and SameSite=Lax.
  Angular obtains a request token from `GET /csrf` for the current identity
  and sends it in `X-CSRF-TOKEN` on JSON writes.
- Confirmation and recovery links use the configured HTTPS origin and carry
  the callback data in a URL fragment. Opening them displays an Angular page.
  Submitting that page changes the account.
- Public confirmation and reset requests return the same status for known and
  unknown addresses. The SMTP sender logs delivery failures by failure type.
  A user can request a fresh link after a failed delivery.
- Lockout applies after five failed password attempts for confirmed accounts.
  In-process limits partition by route and client IP. Mail-sending routes
  allow five requests per minute per client. Account writes allow twenty,
  and antiforgery-token issuance allows 120.

See [Microsoft Learn on Identity for SPAs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0)
and [ASP.NET Core antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0).

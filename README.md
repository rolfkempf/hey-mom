# Hey Mom

This monorepo contains an Astro marketing website, an Angular 22 frontend,
and a .NET 10 Minimal API.
The API uses vertical slices. PostgreSQL persistence lives in a separate project.

## Local development

Use Node.js **22.22.3** and pnpm **11.9.0**. The repository pins Node in
`.node-version` and `.nvmrc`, and pnpm in `package.json`. With nvm and Corepack:

```sh
nvm use
corepack enable
pnpm install --frozen-lockfile
pnpm --dir hey-mom-frontend install --frozen-lockfile
pnpm --dir hey-mom-website install --frozen-lockfile
(cd hey-mom-backend && dotnet tool restore && dotnet restore HeyMom.sln)
```

Use the .NET SDK pinned in `hey-mom-backend/global.json`, Docker with Compose v2,
and mkcert. Add the following line to your local hosts file:

```text
127.0.0.1 hey-mom.test
```

Trust the local mkcert CA once, then generate the certificate for the
configured development domain:

```sh
mkcert -install
pnpm dev:cert
```

Run `pnpm dev` to start all three projects and the Docker infrastructure.
Stop the hot-reload processes with Ctrl+C, then run `pnpm dev:stop` to stop
the Docker services while keeping their volumes.

### Development ports

The defaults in [`.env.example`](.env.example) allow the JBY development
stack to run alongside Hey Mom.

| Service | Host port | Browser URL or endpoint |
| --- | ---: | --- |
| Traefik HTTPS | 8443 | <https://hey-mom.test:8443/> |
| Traefik HTTP redirect | 8088 | <http://hey-mom.test:8088/> |
| Astro website | 14321 | <https://hey-mom.test:8443/> |
| Angular app | 14200 | <https://hey-mom.test:8443/web/> |
| .NET API | 15080 | [Health](https://hey-mom.test:8443/api/health) · [OpenAPI](https://hey-mom.test:8443/api/openapi/v1.json) |
| PostgreSQL | 15433 | `127.0.0.1:15433` |
| RustFS API | 19000 | <http://localhost:19000/> |
| RustFS console | 19001 | <http://localhost:19001/> |
| Mailpit SMTP | 11025 | `127.0.0.1:11025` |
| Mailpit inbox | 18025 | <http://localhost:18025/> |

Use the Traefik URLs for browser workflows. Email confirmation and password
reset links use the same HTTPS origin.

## Account setup

The [Authentication feature guide](hey-mom-backend/src/HeyMom.Api/Features/Authentication/README.md)
describes sign-in, session restoration, account recovery, and ownership.

On a fresh local database, start PostgreSQL and apply the Identity migration
before running the registration flow:

```sh
pnpm dev:services
(cd hey-mom-backend && dotnet tool restore)
pnpm dev:migrate
```

The Astro homepage links to registration at <https://hey-mom.test:8443/web/register>.
Check Mailpit for confirmation and password reset links.

Outside development, set `Authentication__PublicOrigin` to the public HTTPS
origin and configure an SMTP relay through `Authentication__Smtp__Host`,
`Authentication__Smtp__Port`, `Authentication__Smtp__FromAddress`,
`Authentication__Smtp__UserName` and `Authentication__Smtp__Password`. STARTTLS is enabled by default. Put SMTP
credentials in the environment or secret store. For a deployment behind a
proxy, set `Authentication__TrustedProxy` to the proxy's direct IP address,
so ASP.NET Core can use trusted HTTPS and client-IP forwarding. Set
`Authentication__DataProtectionKeyPath` to a persistent, access-restricted
directory when the host's default key ring is ephemeral. Instances serving
the same deployment need a shared key ring. Apply migrations as a separate
deployment step.

## Using Hey Mom as a starter

1. Copy the repository into a separate checkout and run `cp .env.example .env`
   in the repository root. Set a unique `DEV_PROJECT_NAME`, a local
   `DEV_DOMAIN` under `.test`, and free host ports for any projects that will
   run alongside it. The project name scopes the Compose project, containers,
   and data volumes.
2. Set the local PostgreSQL database and user, RustFS credentials, and
   `DEV_MAIL_FROM` in `.env`. The sample values are for development only.
   PostgreSQL applies its initialization settings when its volume is first
   created. Use a new project name for the starter's own data volumes.
3. Map the chosen domain to `127.0.0.1` in your hosts file. Install the
   dependencies from [Local development](#local-development) and run
   `pnpm dev:cert`, `pnpm dev:services`, `pnpm dev:migrate`, then `pnpm dev`.
   Use the HTTPS links printed by the startup script.
4. Register and confirm an account through the app. If you want the local
   login shortcut, register the account listed in `DEV_TEST_EMAIL` and
   `DEV_TEST_PASSWORD`. Leave both values empty to hide the button.
   Rename the app titles and package or namespace names for the new product
   as needed.

The dev commands read checked-in defaults from `.env.example`. A local
`.env` overrides them, and exported `DEV_*` variables take precedence.
Docker Compose receives the same values as the local .NET, Angular and
Astro processes. The configured domain and HTTPS port determine the
browser URLs, Traefik's redirect, account email links, and the OpenAPI
URL. Internal container ports and the browser's relative `/api/` calls
are fixed conventions.

The local `.env` and TLS files are ignored by Git. Configure
production settings and secrets in the deployment environment.

## Verify

```sh
(cd hey-mom-backend && dotnet test HeyMom.sln)
pnpm --dir hey-mom-frontend test --watch=false
pnpm --dir hey-mom-frontend build
pnpm --dir hey-mom-website build
docker compose --env-file .env.example -f deployment/development/compose.yml config --quiet
```

To also run the PostgreSQL authentication tests, set `HEYMOM_TEST_POSTGRES`
to a connection string for a local test PostgreSQL server. The test role
needs permission to create databases. The fixture applies migrations to its
own temporary database and drops it after the run. CI provides its own
PostgreSQL service.

[Health](hey-mom-backend/src/HeyMom.Api/Features/Health/README.md) is the first
vertical slice. Its endpoints own HTTP mapping. The query and response DTO live
together in the use-case directory and the feature registers its own services.

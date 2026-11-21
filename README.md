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
(cd hey-mom-backend && dotnet restore HeyMom.sln)
```

Use the .NET SDK pinned in `hey-mom-backend/global.json`, Docker with Compose v2,
and mkcert. Add the following line to your local hosts file:

```text
127.0.0.1 hey-mom.test
```

Create and trust a local TLS certificate:

```sh
mkcert -install
mkdir -p deployment/development/traefik/certs
mkcert -cert-file deployment/development/traefik/certs/hey-mom.test.pem \
  -key-file deployment/development/traefik/certs/hey-mom.test-key.pem hey-mom.test
```

Run `pnpm dev` to start all three projects and the Docker infrastructure.
Traefik serves the Astro website at <https://hey-mom.test/> (port 4321),
the Angular app at <https://hey-mom.test/web/> (port 4200), and
the .NET API at <https://hey-mom.test/api/> (port 5080).
Check PostgreSQL through <https://hey-mom.test/api/health>.
The development OpenAPI document is at <https://hey-mom.test/api/openapi/v1.json>.

| Local service | Port on the host |
| --- | --- |
| Traefik HTTP and HTTPS | 80 and 443 |
| PostgreSQL | 5433 |
| RustFS API and console | 9000 and 9001 |
| Mailpit SMTP and web | 1025 and 8025 |

Open the [Mailpit inbox](http://localhost:8025/) to inspect local mail.
Use `127.0.0.1:1025` as the SMTP server for processes on the host.

Hey Mom uses the JBY ports. Stop JBY before starting Hey Mom. Stop the
hot-reload processes with Ctrl+C, then run `pnpm dev:stop` to stop Docker
services while keeping their volumes.

## Verify

```sh
(cd hey-mom-backend && dotnet test HeyMom.sln)
pnpm --dir hey-mom-frontend test --watch=false
pnpm --dir hey-mom-frontend build
pnpm --dir hey-mom-website build
docker compose -f deployment/development/compose.yml config --quiet
```

[Health](hey-mom-backend/src/HeyMom.Api/Features/Health/README.md) is the first
vertical slice. Its endpoints own HTTP mapping. The query and response DTO live
together in the use-case directory and the feature registers its own services.

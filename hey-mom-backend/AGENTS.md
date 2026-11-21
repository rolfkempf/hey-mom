# Hey Mom Backend

- API endpoints and their use cases live in [Features](src/HeyMom.Api/Features/AGENTS.md). Read the owning slice README before changing its behavior.
- Schema and EF Core changes follow the [persistence guide](src/HeyMom.Persistence/AGENTS.md).
- Keep API composition in `Program.cs` and feature registration in `FeatureRegistration`. Put shared technical integrations in `HeyMom.Api/Infrastructure/<Concern>` when a feature needs them. Keep decisions about when to call an integration in the owning use case.
- Use EF Core directly and write projections and mappings explicitly. Introduce shared code when it reduces existing duplication or provides a concrete technical boundary. Keep data access, mapping and dispatch free of generic repository, unit-of-work, AutoMapper and mediator layers.
- Propagate `CancellationToken` through database calls and external I/O. For time-dependent behavior, use an injected `TimeProvider` and pass the instant into domain code.
- Keep credentials in local user secrets or environment variables. Log failures with structured, safe metadata and preserve correlation. Exclude tokens, passwords, connection strings and private payloads from logs and HTTP errors.

## Verification

Run the backend build and tests from this directory after backend code changes.

```sh
dotnet build HeyMom.sln
dotnet test HeyMom.sln --no-build
```

Keep endpoint tests under the owning feature in `tests/HeyMom.Api.Tests`.
Exercise the HTTP endpoint for status, validation and failure behavior. Use
isolated PostgreSQL when a test depends on database behavior. Use controlled
time and deterministic substitutes for external providers in normal tests.

# Hey Mom Apps

- Local startup, ports, TLS and verification are documented in the [root README](README.md).
- Backend changes follow the [.NET guide](hey-mom-backend/AGENTS.md). Angular changes follow the [frontend guide](hey-mom-frontend/AGENTS.md).
- For HTTP contract changes, inspect the endpoint, its OpenAPI metadata and any Angular callers. Verify both projects when both are affected.
- For Docker or routing changes, validate the development Compose configuration.
- “Board” refers to the [Hey Mom MVP GitHub Project](https://github.com/users/rolfkempf/projects/3).

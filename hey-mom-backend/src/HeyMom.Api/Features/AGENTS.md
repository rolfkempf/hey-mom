# API Feature Slices

Use [Health](Health/README.md) as the working reference.

- Organize code by `<Feature>/<UseCase>`. Introduce a subarea when it owns a distinct group of routes or responsibilities. Keep shared types with their nearest owning feature or subarea.
- Give each operation a `*Query` or `*Command` with a matching filename and an `ExecuteAsync` entry point. Queries are read-only. Commands own state changes. Keep `*RequestDto` and `*ResponseDto` beside their operation when they are local to it.
- Keep one `*Endpoints.cs` per HTTP slice. It owns routes, request binding, HTTP result mapping, authorization, antiforgery when applicable, and OpenAPI metadata. The operation owns execution and business rules.
- Register use cases and other feature-local dependencies explicitly in the feature's `*ServiceCollectionExtensions.cs`. Wire features from `FeatureRegistration`.
- Use route groups for shared prefixes and conventions. Give endpoints stable operation names, summaries, tags and response metadata. Prefer typed results. Validate request shape at the HTTP boundary and rules that depend on current state in the operation.
- When a feature handles user-owned data, scope reads and writes to the authenticated owner in the data query. Return the same result for missing and inaccessible resources when revealing their existence would expose private information. Validate antiforgery for state-changing requests if the API uses cookie authentication.
- Keep a short README with the slice's purpose, use cases and relevant security or external effects. Update it when those change. Keep field-level contracts in the DTOs and OpenAPI.

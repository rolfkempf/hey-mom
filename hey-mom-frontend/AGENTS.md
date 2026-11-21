# Hey Mom Frontend

Use the [frontend README](README.md) for setup. For browser checks, use the Angular URL in the [development port table](../README.md#development-ports) to exercise the base path and same-origin API routing.

## Angular and state

- Use standalone components, explicit imports, `inject()`, and `@if` / `@for`. Prefer `OnPush` for new components.
- Use signals for UI state and `computed()` for derived values. Declare signal inputs, outputs, models and queries as `readonly` references. Use RxJS for HTTP streams or async coordination where it fits.
- Keep state with its owning component or feature. Pass values into presentational components and keep one owner for shared server state. Keep private account data in memory when a signed-in workflow is added.
- Name booleans with `is`, `has` or `can`. Use intent names such as `deleteRequested`, value-change names such as `selectionChange`, and completed-event names such as `saved`. Name handlers for the action they perform.
- Use relative `/api/` URLs for browser API calls. Keep HTTP calls in injectable services and use generated backend contract types once available.

## Application layout

- Keep providers and routes in `src/app/`. Put route targets in `pages/<page>/`
  and app components, including the shell, in `components/<component>/`.
- Put each component's template and any local styles in its TypeScript file.
  Place its `*.spec.ts` beside it in the same directory.
- Keep injectable workflows and API access in `services/`. Place technical
  integrations, HTTP interceptors and the generated API client in
  `infrastructure/` when introduced. Add other role folders when implementation
  needs them, such as `guards/` for route guards.
- Use `@app/*` across role folders and relative imports for local files.

## UI and verification

- Use Spartan Helm primitives from `@spartan-ng/helm/*` for UI controls they cover. Reserve `src/app/ui/` for the local Spartan Helm kit.
- Use native controls and labels, Flexbox or Grid, and existing component or styling conventions. Keep one-off styles local and introduce reusable UI when the pattern recurs.
- For affected flows, handle loading, empty and error states and preserve input on recoverable failures. Check responsive behavior, keyboard access and focus in a browser when those behaviors change.
- Colocate Vitest/TestBed specs with the affected code. Cover observable behavior. Use a browser for native focus, layout and routing checks.

```sh
pnpm --dir hey-mom-frontend test --watch=false
pnpm --dir hey-mom-frontend build
```

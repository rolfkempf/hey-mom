# Hey Mom Frontend

See [local development](../README.md#local-development) for the Node and pnpm
pins and the normal same-origin startup through Traefik.

Use `pnpm start`, `pnpm test --watch=false`, and `pnpm build` from this
directory. The frontend is served under `/web/`.
`pnpm start` and `pnpm api:types` use the root development configuration
documented in [Using Hey Mom as a starter](../README.md#using-hey-mom-as-a-starter).

## Application layout

Route targets and app components each have a directory. Their TypeScript
files contain the template and any local styles. Tests live alongside them.
The Angular CLI generates this layout by default. For example, use
`pnpm exec ng generate component components/example`.

See the [frontend guide](AGENTS.md) for role folders.

## API contract

With the API running through Traefik, regenerate the TypeScript contract after
an API change:

```sh
NODE_EXTRA_CA_CERTS="$(mkcert -CAROOT)/rootCA.pem" pnpm api:types
```

The command reads the development OpenAPI document and writes
`src/app/infrastructure/api/openapi.ts`. Commit the updated contract with the
API change.

## Spartan UI

Spartan Brain supplies the Angular behavior. The local Helm primitives live in
`src/app/ui/` and are imported via `@spartan-ng/helm/*`. Tailwind 4 uses the [Angular PostCSS setup](https://angular.dev/guide/tailwind)
with `.postcssrc.json` and `@import "tailwindcss"` in `src/styles.css`. The same
stylesheet loads the Spartan preset and neutral theme. The header's Website
link uses the Vega button styles.

To add primitives, use the [component documentation](https://spartan.ng/components/button)
and the [manual setup](https://spartan.ng/documentation/installation). Keep Helm
code and its styles under `src/app/ui/`, and import component styles from the
global entry point. The copied button, styles and class helper come from Spartan
1.10.0 and carry the [upstream MIT license](src/app/ui/LICENSE.spartan).

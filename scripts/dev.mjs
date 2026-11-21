import { spawn } from "node:child_process";
import { mkdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dev, publicOrigin } from "./dev-config.mjs";

const root = fileURLToPath(new URL("../", import.meta.url));
const compose = [
  "docker", "compose", "--env-file", ".env.example", "-f", "deployment/development/compose.yml",
];
const api = "src/HeyMom.Api";
const certDir = "deployment/development/traefik/certs";
const command = process.argv[2];

const define = (name, value) => `${name}=${JSON.stringify(value)}`;
const connectionValue = (value) => `"${value.replaceAll('"', '""')}"`;

const commands = {
  services: [...compose, "up", "-d", "--wait", "--wait-timeout", "120"],
  stop: [...compose, "down"],
  "wait-services": [
    "pnpm", "exec", "wait-on", "--timeout", "120000",
    `tcp:127.0.0.1:${dev.DEV_POSTGRES_PORT}`, `tcp:127.0.0.1:${dev.DEV_RUSTFS_API_PORT}`,
  ],
  backend: ["dotnet", "watch", "--non-interactive", "--project", api, "run", "--no-launch-profile"],
  migrate: [
    "dotnet", "ef", "database", "update", "--project", "src/HeyMom.Persistence",
    "--startup-project", api,
  ],
  frontend: [
    "pnpm", "--dir", "hey-mom-frontend", "exec", "ng", "serve",
    "--port", dev.DEV_ANGULAR_PORT, "--allowed-hosts", dev.DEV_DOMAIN,
    "--define", define("DEV_TEST_EMAIL", dev.DEV_TEST_EMAIL),
    "--define", define("DEV_TEST_PASSWORD", dev.DEV_TEST_PASSWORD),
  ],
  website: [
    "pnpm", "--dir", "hey-mom-website", "exec", "astro", "dev",
    "--port", dev.DEV_ASTRO_PORT, "--allowed-hosts", dev.DEV_DOMAIN,
  ],
  "api-types": [
    "pnpm", "--dir", "hey-mom-frontend", "exec", "openapi-typescript",
    `${publicOrigin}/api/openapi/v1.json`, "-o", "src/app/infrastructure/api/openapi.ts",
  ],
  cert: [
    "mkcert", "-cert-file", `${certDir}/${dev.DEV_DOMAIN}.pem`,
    "-key-file", `${certDir}/${dev.DEV_DOMAIN}-key.pem`, dev.DEV_DOMAIN,
  ],
};

if (!Object.hasOwn(commands, command)) {
  throw new Error(`Unknown dev command: ${command}`);
}

if (command === "cert") mkdirSync(new URL(`../${certDir}/`, import.meta.url), { recursive: true });

const isDotnet = command === "backend" || command === "migrate";
const env = { ...process.env, ...dev };
if (isDotnet) {
  Object.assign(env, {
    ASPNETCORE_ENVIRONMENT: "Development",
    ASPNETCORE_URLS: `http://0.0.0.0:${dev.DEV_API_PORT}`,
    AllowedHosts: `${dev.DEV_DOMAIN};localhost;127.0.0.1`,
    ConnectionStrings__Database: [
      "Host=127.0.0.1",
      `Port=${dev.DEV_POSTGRES_PORT}`,
      `Database=${connectionValue(dev.DEV_POSTGRES_DB)}`,
      `Username=${connectionValue(dev.DEV_POSTGRES_USER)}`,
      `Password=${connectionValue(dev.DEV_POSTGRES_PASSWORD)}`,
    ].join(";"),
    Authentication__PublicOrigin: publicOrigin,
    Authentication__Smtp__Host: "127.0.0.1",
    Authentication__Smtp__Port: dev.DEV_SMTP_PORT,
    Authentication__Smtp__FromAddress: dev.DEV_MAIL_FROM,
  });
}

const [executable, ...args] = commands[command];
const child = spawn(executable, args, {
  cwd: isDotnet ? new URL("../hey-mom-backend/", import.meta.url) : root,
  env,
  stdio: "inherit",
});

child.on("error", (error) => {
  console.error(error.message);
  process.exitCode = 1;
});
child.on("exit", (code, signal) => {
  process.exitCode = code ?? (signal === "SIGINT" ? 130 : 1);
});

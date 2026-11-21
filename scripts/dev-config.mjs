import { readFileSync } from "node:fs";
import { parseEnv } from "node:util";

const defaults = readEnv("../.env.example");
const overrides = readEnv("../.env", { optional: true });

export const dev = Object.fromEntries(
  Object.keys(defaults).map((key) => [key, process.env[key] ?? overrides[key] ?? defaults[key]]),
);

if (!/^[a-z0-9][a-z0-9_-]*$/.test(dev.DEV_PROJECT_NAME)) {
  throw new Error("DEV_PROJECT_NAME must use lowercase letters, digits, hyphens or underscores.");
}

const domainLabel = "[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?";
if (!new RegExp(`^(?:${domainLabel}\\.)+test$`).test(dev.DEV_DOMAIN)) {
  throw new Error("DEV_DOMAIN must be a lowercase hostname under .test.");
}

const ports = new Map();
for (const [key, value] of Object.entries(dev).filter(([key]) => key.endsWith("_PORT"))) {
  if (!/^[1-9][0-9]*$/.test(value) || Number(value) > 65535) {
    throw new Error(`${key} must be a TCP port between 1 and 65535.`);
  }
  if (ports.has(value)) {
    throw new Error(`${key} and ${ports.get(value)} both use port ${value}.`);
  }
  ports.set(value, key);
}

export const publicOrigin = new URL(`https://${dev.DEV_DOMAIN}:${dev.DEV_HTTPS_PORT}`).origin;

function readEnv(filename, { optional = false } = {}) {
  try {
    return parseEnv(readFileSync(new URL(filename, import.meta.url), "utf8"));
  } catch (error) {
    if (optional && error.code === "ENOENT") return {};
    throw error;
  }
}

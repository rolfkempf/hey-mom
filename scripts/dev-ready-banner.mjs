import { dev, publicOrigin } from "./dev-config.mjs";

const urls = [
  `http://127.0.0.1:${dev.DEV_ANGULAR_PORT}/web/`,
  `http://127.0.0.1:${dev.DEV_API_PORT}/api/health`,
  `http://127.0.0.1:${dev.DEV_ASTRO_PORT}/`,
];
const deadline = Date.now() + 120_000;

while (!(await Promise.all(urls.map(isReady))).every(Boolean)) {
  if (Date.now() >= deadline) {
    console.error(`Hey Mom did not become ready at ${urls.join(", ")} within 120 seconds.`);
    process.exit(1);
  }

  await new Promise((resolve) => setTimeout(resolve, 250));
}

console.log(`Development is ready. Website: ${publicOrigin}/ | App: ${publicOrigin}/web/`);

async function isReady(url) {
  try {
    const response = await fetch(url, { signal: AbortSignal.timeout(2_000) });
    return response.ok;
  } catch {
    return false;
  }
}

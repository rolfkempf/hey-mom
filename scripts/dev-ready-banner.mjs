const urls = [
  "http://127.0.0.1:4200/web/",
  "http://127.0.0.1:5080/api/health",
  "http://127.0.0.1:4321/",
];
const deadline = Date.now() + 120_000;

while (!(await Promise.all(urls.map(isReady))).every(Boolean)) {
  if (Date.now() >= deadline) {
    console.error(`Hey Mom did not become ready at ${urls.join(", ")} within 120 seconds.`);
    process.exit(1);
  }

  await new Promise((resolve) => setTimeout(resolve, 250));
}

console.log("Hey Mom is ready. Website: https://hey-mom.test/ | App: https://hey-mom.test/web/");

async function isReady(url) {
  try {
    const response = await fetch(url, { signal: AbortSignal.timeout(2_000) });
    return response.ok;
  } catch {
    return false;
  }
}

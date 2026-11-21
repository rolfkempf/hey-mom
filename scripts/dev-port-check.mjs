import net from "node:net";
import { dev } from "./dev-config.mjs";

const developmentPorts = [
  { port: Number(dev.DEV_ANGULAR_PORT), process: "Angular" },
  { port: Number(dev.DEV_API_PORT), process: ".NET API" },
  { port: Number(dev.DEV_ASTRO_PORT), process: "Astro website" },
];

const occupied = (
  await Promise.all(
    developmentPorts.map(async (entry) => ({
      ...entry,
      available: await portIsAvailable(entry.port),
    })),
  )
).filter((entry) => !entry.available);

if (occupied.length > 0) {
  for (const entry of occupied) {
    console.error(`Port ${entry.port} is already in use (${entry.process}).`);
  }
  console.error(
    "Stop the existing development processes before running pnpm dev again.",
  );
  process.exitCode = 1;
}

function portIsAvailable(port) {
  return new Promise((resolve) => {
    const server = net.createServer();

    server.once("error", () => resolve(false));
    server.once("listening", () => server.close(() => resolve(true)));
    server.listen(port, "127.0.0.1");
  });
}

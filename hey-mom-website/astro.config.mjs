import { defineConfig } from 'astro/config';

export default defineConfig({
  server: {
    host: true,
    port: 4321,
    allowedHosts: ['hey-mom.test'],
  },
  vite: {
    server: { strictPort: true },
  },
});

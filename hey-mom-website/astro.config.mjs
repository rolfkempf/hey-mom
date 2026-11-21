import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'astro/config';

export default defineConfig({
  server: {
    host: true,
  },
  vite: {
    plugins: [tailwindcss()],
    server: { strictPort: true },
  },
});

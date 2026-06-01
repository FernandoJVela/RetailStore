import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@app': path.resolve(__dirname, './src/app'),
      '@shared': path.resolve(__dirname, './src/shared'),
      '@features': path.resolve(__dirname, './src/features'),
    },
  },
  server: {
    // host: true binds to 0.0.0.0 so the Vite dev server is reachable from
    // outside the container when running inside Docker. Without this it only
    // listens on 127.0.0.1 (the container's loopback) and Docker cannot
    // route traffic to it. Locally this has no visible effect.
    host: true,
    port: 3000,
    proxy: {
      '/api': {
        // In Docker Compose, VITE_API_PROXY_TARGET is set to http://backend:8080
        // so the proxy reaches the backend service by its Docker network name.
        // When running locally (npm run dev) the variable is not set and the
        // fallback http://localhost:5240 is used instead.
        target: process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:5240',
        changeOrigin: true,
      },
    },
  },
});
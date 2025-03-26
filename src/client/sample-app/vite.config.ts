// vite.config.js
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      // Map 'shared' to the shared folder
      'shared': path.resolve(__dirname, '../shared'),
    },
  },
  server: {
    port: 3002,
    fs: {
      // Allow access to the parent directory (client) which contains both shared and tools
      allow: [path.resolve(__dirname, '../')],
    },
  },
  optimizeDeps: {
    include: ['shared'],
  },
});

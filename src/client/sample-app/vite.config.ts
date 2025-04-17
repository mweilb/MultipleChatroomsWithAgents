import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';
import { fileURLToPath } from 'url';

// Define __dirname in an ESM environment
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      // Map 'shared' to the shared folder
      shared: path.resolve(__dirname, '../shared'),
    },
    preserveSymlinks: true,
  },
  server: {
    port: 3002,
    fs: {
      // Allow access to the parent directory (client) which contains both shared and tools
      allow: [path.resolve(__dirname, '../')],
    },
  },
  optimizeDeps: {
    // Do not pre-bundle shared to allow live updates
    exclude: ['shared'],
  },
});

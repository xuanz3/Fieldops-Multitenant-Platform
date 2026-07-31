import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

const apiTarget =
  process.env.VITE_API_PROXY_TARGET ??
  'http://127.0.0.1:5204'

export default defineConfig({
  plugins: [react()],
  server: {
    host: '127.0.0.1',
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: apiTarget,
        changeOrigin: true,
      },
      '/health': {
        target: apiTarget,
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles:
      './src/test/setup.ts',
    restoreMocks: true,
    clearMocks: true,
  },
})

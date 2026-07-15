import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Served from the API under /admin in production; standalone on 5174 in dev.
export default defineConfig({
  plugins: [react()],
  base: '/admin/',
  server: {
    port: 5174,
  },
})

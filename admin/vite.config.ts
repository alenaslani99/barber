import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue(), tailwindcss()],
  // Fixed port so it never collides with web/ (5173) and matches the API CORS list.
  server: { port: 5180, strictPort: true },
})

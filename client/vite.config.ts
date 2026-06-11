import { defineConfig } from 'vite'
import { fileURLToPath, URL } from 'node:url'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      // "@" -> src kökü. Importları kısaltır (örn. "@/lib/api/client").
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    // Backend CORS politikası localhost:5173'e izin veriyor; portu sabitliyoruz.
    // strictPort: port doluysa rastgele porta düşmek yerine hata ver (CORS uyumu garanti).
    port: 5173,
    strictPort: true,
  },
})

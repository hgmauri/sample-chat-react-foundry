import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react()],
  server: {
    // Em desenvolvimento, o Vite repassa /api para a API .NET. Sem CORS para configurar.
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
})

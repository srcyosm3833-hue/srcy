/// <reference types="vite/client" />

/**
 * Uygulamanin kullandigi ortam degiskenlerinin tip sozlesmesi.
 * import.meta.env.VITE_API_BASE_URL erisimini tip guvenli kilar.
 */
interface ImportMetaEnv {
  /** Backend API kok adresi (sondaki "/" olmadan). Orn: http://localhost:5241 */
  readonly VITE_API_BASE_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

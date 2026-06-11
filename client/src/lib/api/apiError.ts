import { AxiosError } from 'axios'
import {
  isValidationProblem,
  type ProblemDetails,
  type ValidationProblemDetails,
} from '@/types'

/**
 * Backend hatalarini (ProblemDetails / ValidationProblemDetails) UI'da kullanilabilir
 * bir sekle indirger. Komponentler axios'un ham hatasini bilmek zorunda kalmaz.
 */
export interface NormalizedApiError {
  /** Kullaniciya gosterilebilecek tek satirlik mesaj. */
  message: string
  /** Varsa HTTP durum kodu. */
  status?: number
  /** Alan bazli dogrulama hatalari (form alanlarini isaretlemek icin). */
  fieldErrors?: Record<string, string[]>
}

/** Bilinmeyen bir hatayi NormalizedApiError'a cevirir. */
export function normalizeApiError(error: unknown): NormalizedApiError {
  if (error instanceof AxiosError) {
    const status = error.response?.status
    const data = error.response?.data as ProblemDetails | undefined

    // Alan bazli dogrulama hatasi (400).
    if (isValidationProblem(data)) {
      return {
        status,
        message: data.title ?? 'Dogrulama hatasi olustu.',
        fieldErrors: (data as ValidationProblemDetails).errors,
      }
    }

    // Standart ProblemDetails.
    if (data && typeof data === 'object' && 'title' in data && data.title) {
      return { status, message: String(data.title) }
    }

    // Ag hatasi (yanit yok).
    if (!error.response) {
      return {
        message:
          'Sunucuya ulasilamadi. Internet baglantinizi ve API adresini kontrol edin.',
      }
    }

    return { status, message: error.message }
  }

  if (error instanceof Error) {
    return { message: error.message }
  }

  return { message: 'Beklenmeyen bir hata olustu.' }
}

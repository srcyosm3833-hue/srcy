/**
 * RFC 7807 ProblemDetails ve ValidationProblemDetails sozlesmelerinin
 * TypeScript karsiligi. Backend hatalari ApiControllerBase.Problem() ile
 * bu sekilde doner (Core .../Controllers/ApiControllerBase.cs).
 */

/** Standart ProblemDetails (tek mesajli hatalar). */
export interface ProblemDetails {
  /** Insan okunabilir baslik/ozet (backend'de Error.Message). */
  title?: string
  /** HTTP durum kodu. */
  status?: number
  /** Hata tipini niteleyen URI. */
  type?: string
  /** Ek detay metni (opsiyonel). */
  detail?: string
  /** Backend'in eklediti uygulama-ici hata kodu (Extensions["code"]). */
  code?: string
  /** Olasi diger genisletme alanlari. */
  [key: string]: unknown
}

/** Alan bazli dogrulama hatalari (400). errors: alanAdi -> mesaj listesi. */
export interface ValidationProblemDetails extends ProblemDetails {
  /** Alan adi -> o alana ait hata mesajlari. */
  errors?: Record<string, string[]>
}

/** Bir nesnenin ValidationProblemDetails olup olmadigini daraltir (type guard). */
export function isValidationProblem(
  value: unknown,
): value is ValidationProblemDetails {
  return (
    typeof value === 'object' &&
    value !== null &&
    'errors' in value &&
    typeof (value as ValidationProblemDetails).errors === 'object'
  )
}

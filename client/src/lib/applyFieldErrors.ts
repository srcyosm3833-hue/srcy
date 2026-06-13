import type {
  FieldValues,
  Path,
  UseFormSetError,
} from 'react-hook-form'

import type { NormalizedApiError } from '@/lib/api'

/**
 * Backend ValidationProblemDetails alan hatalarini (fieldErrors) react-hook-form'un
 * setError'una baglar. Backend alan adlari genelde PascalCase gelir (orn. "Email"),
 * form alanlari ise camelCase ("email"); ilk harfi kuculterek eslestiririz.
 *
 * Bilinen form alanlarina denk gelmeyen hatalar (orn. genel "" anahtari) burada
 * ele alinmaz; cagiran taraf genel hata mesajini ayrica gosterir.
 *
 * @returns Bir form alanina eslenebilen en az bir hata uygulandiysa true.
 */
export function applyFieldErrors<TFieldValues extends FieldValues>(
  error: NormalizedApiError,
  setError: UseFormSetError<TFieldValues>,
  knownFields: readonly Path<TFieldValues>[],
): boolean {
  if (!error.fieldErrors) return false

  let applied = false

  for (const [rawKey, messages] of Object.entries(error.fieldErrors)) {
    if (!messages || messages.length === 0) continue

    // "Email" -> "email"
    const camelKey = rawKey.charAt(0).toLowerCase() + rawKey.slice(1)
    const field = knownFields.find((f) => f === camelKey)
    if (!field) continue

    setError(field, { type: 'server', message: messages[0] })
    applied = true
  }

  return applied
}

/**
 * Bir kullanicidan avatar icin bas harf(ler) uretir. Once userName, yoksa email
 * kullanilir. Bos/null durumda "?" doner. En fazla 2 harf.
 * Ornek: "ahmet yilmaz" -> "AY", "demo@site.com" -> "D"
 */
export function getInitials(
  userName: string | null | undefined,
  email: string | null | undefined,
): string {
  const source = (userName ?? email ?? '').trim()
  if (!source) return '?'

  const words = source.split(/\s+/).filter(Boolean)
  if (words.length >= 2) {
    return (words[0][0] + words[1][0]).toUpperCase()
  }
  return source.slice(0, 1).toUpperCase()
}

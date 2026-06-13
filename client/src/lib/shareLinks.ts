/**
 * Sosyal paylasim "intent" / web paylasim URL'lerini ureten saf yardimcilar.
 * Kullanici adina otomatik post yapilmaz; her URL ilgili saglayicinin paylasim
 * penceresini acar. Baslik ve URL encodeURIComponent ile guvenle kodlanir.
 */

export interface ShareTarget {
  /** Paylasilan icerigin basligi (genelde blog title). */
  title: string
  /** Paylasilan kanonik URL. */
  url: string
}

/** X (Twitter) tweet-intent linki: baslik + url. */
export function getTwitterShareUrl({ title, url }: ShareTarget): string {
  const params = new URLSearchParams({ text: title, url })
  return `https://twitter.com/intent/tweet?${params.toString()}`
}

/** Facebook sharer linki: yalniz url kabul eder (baslik OG meta'dan gelir). */
export function getFacebookShareUrl({ url }: ShareTarget): string {
  const params = new URLSearchParams({ u: url })
  return `https://www.facebook.com/sharer/sharer.php?${params.toString()}`
}

/** WhatsApp paylasim linki: baslik ve url tek metinde birlestirilir. */
export function getWhatsAppShareUrl({ title, url }: ShareTarget): string {
  const params = new URLSearchParams({ text: `${title} ${url}` })
  return `https://wa.me/?${params.toString()}`
}

/** LinkedIn paylasim linki: yalniz url kabul eder. */
export function getLinkedInShareUrl({ url }: ShareTarget): string {
  const params = new URLSearchParams({ url })
  return `https://www.linkedin.com/sharing/share-offsite/?${params.toString()}`
}

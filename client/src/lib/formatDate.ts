/**
 * Tarih formatlama yardimcilari. Backend ISO 8601 UTC string dondurur
 * (orn. "2026-06-10T14:30:00Z"). Burada Turkce locale ile insan-okur formata cevirilir.
 */

/** Ortak Intl formatlayicilar (tekrar olusturmamak icin modul seviyesinde tutulur). */
const dayMonthYear = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
})

const dayMonthYearTime = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

/**
 * ISO tarih string'ini "10 Haz 2026" gibi kisa formata cevirir.
 * Gecersiz/bos girdide bos string doner (UI'da gosterilmez).
 */
export function formatDate(iso: string | null | undefined): string {
  if (!iso) return ''
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  return dayMonthYear.format(date)
}

/** ISO tarih string'ini "10 Haz 2026 14:30" gibi tarih+saat formatina cevirir. */
export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return ''
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  return dayMonthYearTime.format(date)
}

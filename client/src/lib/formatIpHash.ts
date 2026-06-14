/**
 * Audit IP hash gosterim yardimcisi. Tam SHA-256 hash uzun ve okunmaz oldugundan
 * UI'da yalnizca son birkac karakteri gosterilir; tam deger tooltip'te (title) sunulur.
 */

/** Bir IP hash'in son `count` karakterini doner (hash kisaysa oldugu gibi). */
export function shortenIpHash(hash: string, count = 8): string {
  return hash.length <= count ? hash : hash.slice(-count)
}

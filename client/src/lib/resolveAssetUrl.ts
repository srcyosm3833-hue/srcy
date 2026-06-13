/**
 * Backend'den gelen gorsel/asset URL'lerini <img src> icin kullanilabilir hale getirir.
 *
 * Backend yukleme ucu (POST /api/uploads) goreli bir URL dondurur (orn. "/uploads/abc.jpg")
 * — host/port icermez. Bu deger oldugu gibi <img src>'e konulursa tarayici onu Vite
 * origin'ine (http://localhost:5173) gore cozer ve 404 alir. Burada goreli yollar
 * VITE_API_BASE_URL ile mutlak hale getirilir; zaten mutlak olan URL'lere (http/https,
 * data:) dokunulmaz (orn. harici gorseller, gravatar avatarlari).
 */

/** Backend kok adresi (sondaki "/" temizlenmis halde). */
const apiBase = (import.meta.env.VITE_API_BASE_URL as string).replace(/\/+$/, '')

/**
 * Verilen asset yolunu <img src> icin guvenli mutlak URL'e cevirir.
 *
 * - Bos/null/undefined girdide bos string doner (cagiran taraf placeholder gosterir).
 * - Zaten mutlak URL ise (http://, https://) veya data: URL ise oldugu gibi dondurulur.
 * - Goreli yol ise (orn. "/uploads/x.jpg") VITE_API_BASE_URL ile prefix'lenir;
 *   base sonu ve path basindaki "/" ciftlenmesi ("//") onlenir.
 *
 * @param path Backend'den gelen ham gorsel yolu/URL'i.
 * @returns <img src> icin kullanilabilir URL; girdi bos ise bos string.
 *
 * @example
 * resolveAssetUrl('/uploads/abc.jpg')      // -> 'https://localhost:7253/uploads/abc.jpg'
 * resolveAssetUrl('https://cdn.x/y.png')   // -> 'https://cdn.x/y.png'
 * resolveAssetUrl('')                       // -> ''
 */
export function resolveAssetUrl(path: string | null | undefined): string {
  if (!path) return ''

  // Mutlak URL'ler ve gomulu (data:) gorseller oldugu gibi gecirilir.
  if (/^(https?:)?\/\//i.test(path) || path.startsWith('data:')) {
    return path
  }

  // Goreli yol: base ile birlestir, "//" ciftlenmesini onle.
  const normalizedPath = path.startsWith('/') ? path : `/${path}`
  return `${apiBase}${normalizedPath}`
}

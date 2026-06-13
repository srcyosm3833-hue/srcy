/** Sayfa listesinde "..." atlama isaretcisi. */
export const ELLIPSIS = 'ellipsis' as const

export type PaginationItem = number | typeof ELLIPSIS

/**
 * Sayfalama dugmeleri icin gosterilecek ogeleri uretir. Her zaman ilk ve son
 * sayfayi, gecerli sayfanin etrafindaki komsulari gosterir; aradaki bosluklara
 * "..." (ELLIPSIS) koyar. Toplam sayfa azsa hepsi gosterilir.
 *
 * @param current   1 tabanli gecerli sayfa
 * @param total     toplam sayfa sayisi
 * @param siblings  gecerli sayfanin her iki yanindaki komsu sayisi (varsayilan 1)
 */
export function getPaginationRange(
  current: number,
  total: number,
  siblings = 1,
): PaginationItem[] {
  // Ilk + son + gecerli + 2*komsu + 2*ellipsis kadar yer varsa hepsini goster.
  const totalShown = siblings * 2 + 5
  if (total <= totalShown) {
    return Array.from({ length: total }, (_, i) => i + 1)
  }

  const leftSibling = Math.max(current - siblings, 1)
  const rightSibling = Math.min(current + siblings, total)

  const showLeftEllipsis = leftSibling > 2
  const showRightEllipsis = rightSibling < total - 1

  const items: PaginationItem[] = [1]

  if (showLeftEllipsis) {
    items.push(ELLIPSIS)
  } else {
    // Bosluk dar: ellipsis yerine ara sayfalari ekle.
    for (let page = 2; page < leftSibling; page++) {
      items.push(page)
    }
  }

  for (let page = leftSibling; page <= rightSibling; page++) {
    if (page !== 1 && page !== total) {
      items.push(page)
    }
  }

  if (showRightEllipsis) {
    items.push(ELLIPSIS)
  } else {
    for (let page = rightSibling + 1; page < total; page++) {
      items.push(page)
    }
  }

  items.push(total)
  return items
}

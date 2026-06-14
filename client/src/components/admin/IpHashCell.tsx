import { shortenIpHash } from '@/lib/formatIpHash'

/**
 * Audit IP hash gosterimi (mesajlar, arama loglari, blog audit detayinda ortak).
 * Tam hash uzun ve okunmaz oldugundan yalnizca son 8 karakter monospace gosterilir;
 * tam deger native `title` tooltip'inde sunulur. null ise "—" gosterilir.
 *
 * Tooltip icin shadcn Tooltip yerine native `title` kullanilir (projede Tooltip
 * UI bileseni yok; audit alani icin native ipucu yeterli ve a11y dostudur).
 */

interface IpHashCellProps {
  /** Tuzlu SHA-256 IP hash'i; cozulemediyse null. */
  hash: string | null | undefined
}

/** Audit IP hash hucresi: son 8 karakter + tam deger tooltip'te; null -> "—". */
export function IpHashCell({ hash }: IpHashCellProps) {
  if (!hash) {
    return (
      <span className="text-muted-foreground" aria-label="IP hash yok">
        —
      </span>
    )
  }

  return (
    <span
      title={hash}
      className="font-mono text-xs text-muted-foreground"
      aria-label={`IP hash, tam deger: ${hash}`}
    >
      …{shortenIpHash(hash)}
    </span>
  )
}

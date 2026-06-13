import { cn } from '@/lib/utils'
import { formatDate } from '@/lib/formatDate'

interface PostMetaProps {
  authorName: string
  /** ISO 8601 olusturulma ani. */
  createdAt: string
  /** ISO 8601 guncelleme ani (varsa "Guncellendi" notu gosterilir). */
  updatedAt?: string | null
  className?: string
}

/**
 * Blog meta bilgi satiri: yazar adi + nokta ayraci + tarih (+ guncelleme notu).
 * Hem kart hem detay sayfasinda kullanilir.
 */
export function PostMeta({
  authorName,
  createdAt,
  updatedAt,
  className,
}: PostMetaProps) {
  return (
    <div
      className={cn(
        'flex flex-wrap items-center gap-x-2 gap-y-1 text-xs text-muted-foreground',
        className,
      )}
    >
      <span className="font-medium text-foreground/80">{authorName}</span>
      <span aria-hidden>·</span>
      <time dateTime={createdAt}>{formatDate(createdAt)}</time>
      {updatedAt ? (
        <>
          <span aria-hidden>·</span>
          <span>Güncellendi: {formatDate(updatedAt)}</span>
        </>
      ) : null}
    </div>
  )
}

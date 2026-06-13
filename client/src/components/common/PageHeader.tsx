import { cn } from '@/lib/utils'

interface PageHeaderProps {
  /** Sayfa basligi (h1). */
  title: string
  /** Baslik altinda kisa aciklama (opsiyonel). */
  description?: string
  /** Sag tarafa hizalanan aksiyon (orn. bir Button) (opsiyonel). */
  action?: React.ReactNode
  className?: string
}

/**
 * Sayfa basligi bloku. Sol: baslik + aciklama, sag: opsiyonel aksiyon.
 * Her sayfada tek <h1> kuralini destekler.
 */
export function PageHeader({
  title,
  description,
  action,
  className,
}: PageHeaderProps) {
  return (
    <div
      className={cn(
        'flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between',
        className,
      )}
    >
      <div className="space-y-1">
        <h1 className="font-heading text-3xl font-bold tracking-tight sm:text-4xl">
          {title}
        </h1>
        {description ? (
          <p className="text-muted-foreground">{description}</p>
        ) : null}
      </div>
      {action ? <div className="shrink-0">{action}</div> : null}
    </div>
  )
}

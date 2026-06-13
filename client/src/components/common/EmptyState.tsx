import type { LucideIcon } from 'lucide-react'
import { Inbox } from 'lucide-react'

import { cn } from '@/lib/utils'

interface EmptyStateProps {
  /** Ust ikon (lucide). Verilmezse varsayilan Inbox. */
  icon?: LucideIcon
  /** Ana baslik. */
  title: string
  /** Aciklama metni (opsiyonel). */
  description?: string
  /** Sag alt aksiyon alani (orn. bir Button) (opsiyonel). */
  action?: React.ReactNode
  className?: string
}

/**
 * Bos durum (empty state) bilesenleri. Liste/sonuc bos oldugunda gosterilir.
 * Hata DEGILDIR — notr/yonlendirici bir his verir.
 */
export function EmptyState({
  icon: Icon = Inbox,
  title,
  description,
  action,
  className,
}: EmptyStateProps) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center rounded-xl border border-dashed border-border px-6 py-16 text-center',
        className,
      )}
    >
      <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-secondary text-muted-foreground">
        <Icon className="h-6 w-6" />
      </div>
      <h3 className="font-sans text-lg font-semibold text-foreground">
        {title}
      </h3>
      {description ? (
        <p className="mt-1 max-w-sm text-sm text-muted-foreground">
          {description}
        </p>
      ) : null}
      {action ? <div className="mt-6">{action}</div> : null}
    </div>
  )
}

import { AlertTriangle, RotateCw } from 'lucide-react'

import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'

interface ErrorStateProps {
  /** Kullaniciya gosterilecek hata mesaji. */
  message?: string
  /** Verilirse "Tekrar Dene" butonu cikar ve tiklaninca cagrilir. */
  onRetry?: () => void
  className?: string
}

/**
 * Hata durumu (error state) bileseni. Veri cekme basarisiz oldugunda gosterilir.
 * onRetry verilirse retry imkani sunar (TanStack Query refetch'e baglanir).
 */
export function ErrorState({
  message = 'Bir şeyler ters gitti. Lütfen tekrar deneyin.',
  onRetry,
  className,
}: ErrorStateProps) {
  return (
    <div
      role="alert"
      className={cn(
        'flex flex-col items-center justify-center rounded-xl border border-destructive/30 bg-destructive/5 px-6 py-16 text-center',
        className,
      )}
    >
      <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-destructive/10 text-destructive">
        <AlertTriangle className="h-6 w-6" />
      </div>
      <h3 className="font-sans text-lg font-semibold text-foreground">
        Yüklenemedi
      </h3>
      <p className="mt-1 max-w-sm text-sm text-muted-foreground">{message}</p>
      {onRetry ? (
        <Button variant="outline" className="mt-6" onClick={onRetry}>
          <RotateCw className="h-4 w-4" />
          Tekrar Dene
        </Button>
      ) : null}
    </div>
  )
}

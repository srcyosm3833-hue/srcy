import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Check, Mail } from 'lucide-react'
import { toast } from 'sonner'

import type { Message } from '@/types'
import { useMessages, useSetMessageRead } from '@/features/message'
import { normalizeApiError } from '@/lib/api'
import { formatDateTime } from '@/lib/formatDate'
import { cn } from '@/lib/utils'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { PaginationBar } from '@/components/common/PaginationBar'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'

const PAGE_SIZE = 15

/**
 * Admin mesaj kutusu. Iki bolmeli (e-posta istemcisi benzeri) duzen: solda liste
 * (okunmamislar once — backend sirasi), sagda secili mesaj detayi. Bir mesaj
 * acildiginda okunmamissa otomatik okundu isaretlenir; ayrica manuel buton var.
 * PATCH basarisi mesaj listelerini invalidate eder (sidebar rozeti senkron).
 */
export default function AdminMessagesPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const page = Math.max(1, Number(searchParams.get('page')) || 1)

  const { data, isPending, isError, refetch, isFetching } = useMessages(
    page,
    PAGE_SIZE,
  )
  const setRead = useSetMessageRead()

  const [selectedId, setSelectedId] = useState<string | null>(null)
  const selected = data?.items.find((m) => m.id === selectedId) ?? null

  function goToPage(next: number) {
    setSearchParams((prev) => {
      const params = new URLSearchParams(prev)
      params.set('page', String(next))
      return params
    })
    // Sayfa degisince secimi temizle (eski mesaj yeni sayfada olmayabilir).
    setSelectedId(null)
  }

  /** Mesaj sec; okunmamissa otomatik okundu isaretle. */
  function handleSelect(message: Message) {
    setSelectedId(message.id)
    if (!message.isRead) {
      void markRead(message.id)
    }
  }

  async function markRead(id: string) {
    try {
      await setRead.mutateAsync({ id, isRead: true })
    } catch (error) {
      toast.error('Mesaj güncellenemedi.', {
        description: normalizeApiError(error).message,
      })
    }
  }

  const unreadCount = data
    ? data.items.filter((m) => !m.isRead).length
    : 0

  const header = (
    <PageHeader
      title="Mesaj Kutusu"
      description="İletişim formundan gelen mesajlar."
      action={
        unreadCount > 0 ? (
          <Badge variant="default" className="bg-accent text-accent-foreground">
            {unreadCount} okunmamış
          </Badge>
        ) : undefined
      }
    />
  )

  if (isPending) {
    return (
      <div className="space-y-8">
        {header}
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <div className="space-y-3 lg:col-span-1">
            {Array.from({ length: 6 }).map((_, index) => (
              <Skeleton key={index} className="h-20 w-full rounded-lg" />
            ))}
          </div>
          <Skeleton className="h-80 w-full rounded-lg lg:col-span-2" />
        </div>
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState message="Mesajlar yüklenemedi." onRetry={() => void refetch()} />
      </div>
    )
  }

  if (data.items.length === 0) {
    return (
      <div className="space-y-8">
        {header}
        <EmptyState
          icon={Mail}
          title="Henüz mesaj yok."
          description="İletişim formundan gelen mesajlar burada listelenir."
        />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {header}

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Liste */}
        <div className="space-y-2 lg:col-span-1">
          {data.items.map((message) => (
            <button
              key={message.id}
              type="button"
              onClick={() => handleSelect(message)}
              className={cn(
                'w-full rounded-lg border border-border bg-card p-3 text-left transition-colors hover:bg-muted/50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                selectedId === message.id && 'border-primary/40 bg-muted',
              )}
              aria-pressed={selectedId === message.id}
            >
              <div className="flex items-start gap-2">
                <span
                  className={cn(
                    'mt-1.5 h-2 w-2 shrink-0 rounded-full',
                    message.isRead ? 'bg-transparent' : 'bg-accent',
                  )}
                  aria-hidden
                />
                <div className="min-w-0 flex-1">
                  <div className="flex items-center justify-between gap-2">
                    <span
                      className={cn(
                        'truncate text-sm',
                        message.isRead ? 'font-medium' : 'font-semibold',
                      )}
                    >
                      {message.name}
                    </span>
                    <time className="shrink-0 text-xs text-muted-foreground">
                      {formatDateTime(message.createdAt)}
                    </time>
                  </div>
                  <p className="truncate text-sm text-muted-foreground">
                    {message.subject}
                  </p>
                </div>
              </div>
            </button>
          ))}

          <div className="pt-2">
            <PaginationBar
              page={data.page}
              totalPages={data.totalPages}
              hasPreviousPage={data.hasPreviousPage}
              hasNextPage={data.hasNextPage}
              onPageChange={goToPage}
              disabled={isFetching}
            />
          </div>
        </div>

        {/* Detay */}
        <div className="lg:col-span-2">
          {selected ? (
            <Card className="p-6">
              <div className="flex flex-col gap-4 border-b border-border pb-4 sm:flex-row sm:items-start sm:justify-between">
                <div className="min-w-0">
                  <h2 className="font-heading text-xl font-bold">
                    {selected.subject}
                  </h2>
                  <p className="mt-1 text-sm text-muted-foreground">
                    {selected.name} · {selected.email} ·{' '}
                    {formatDateTime(selected.createdAt)}
                  </p>
                </div>
                <div className="flex shrink-0 items-center gap-2">
                  {selected.isRead ? (
                    <Badge variant="secondary">Okundu</Badge>
                  ) : (
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => void markRead(selected.id)}
                      disabled={setRead.isPending}
                    >
                      <Check className="h-4 w-4" />
                      Okundu İşaretle
                    </Button>
                  )}
                </div>
              </div>

              <p className="mt-4 whitespace-pre-wrap text-sm leading-relaxed text-foreground">
                {selected.messageBody}
              </p>

              <div className="mt-6">
                <Button asChild variant="ghost" size="sm">
                  <a href={`mailto:${selected.email}?subject=Re: ${selected.subject}`}>
                    <Mail className="h-4 w-4" />
                    Yanıtla
                  </a>
                </Button>
              </div>
            </Card>
          ) : (
            <EmptyState
              icon={Mail}
              title="Bir mesaj seçin"
              description="Detayını görmek için soldaki listeden bir mesaja tıklayın."
            />
          )}
        </div>
      </div>
    </div>
  )
}

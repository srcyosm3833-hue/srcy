import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Search, ShieldAlert, X } from 'lucide-react'

import { useSearchLogs } from '@/features/searchLog'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import { formatDateTime } from '@/lib/formatDate'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { PaginationBar } from '@/components/common/PaginationBar'
import { TableSkeleton } from '@/components/admin/TableSkeleton'
import { IpHashCell } from '@/components/admin/IpHashCell'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

const PAGE_SIZE = 20
const COLUMNS = ['Terim', 'Kullanıcı', 'IP Hash', 'Tarih']

/**
 * Admin arama audit log sayfasi (YALNIZ Admin — KVKK kapsaminda kisisel veri).
 * Aramalarin kim/ne/ne zaman/hangi IP (hash'li) bilgisini sayfali listeler;
 * ustte KVKK uyari bandi; terim kutusu debounce'lu (filtre degisince page=1).
 *
 * Terim filtresi URL'de (?term=) ve sayfa (?page=) tutulur. Debounce edilmis
 * terim URL'e yazilir; URL disardan (geri/ileri) degisirse input senkronlanir.
 */
export default function AdminSearchLogsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const page = Math.max(1, Number(searchParams.get('page')) || 1)
  const urlTerm = searchParams.get('term') ?? ''

  // Lokal input state -> debounce -> URL. Boylece her tusa istek atilmaz.
  const [termInput, setTermInput] = useState(urlTerm)
  const debouncedTerm = useDebouncedValue(termInput, 350)

  // Geri/ileri ile URL (?term=) disardan degisirse input'u senkronla. Effect yerine
  // render sirasinda "disardan gelen degisiklikte state'i ayarla" deseni (React onerisi;
  // set-state-in-effect kuralina takilmaz, cascading render olusturmaz).
  const [prevUrlTerm, setPrevUrlTerm] = useState(urlTerm)
  if (urlTerm !== prevUrlTerm) {
    setPrevUrlTerm(urlTerm)
    setTermInput(urlTerm)
  }

  // Debounce edilmis terim URL'dekinden farkliysa URL'i guncelle (ilk sayfaya don).
  useEffect(() => {
    if (debouncedTerm === urlTerm) return
    setSearchParams(
      (prev) => {
        const params = new URLSearchParams(prev)
        if (debouncedTerm) {
          params.set('term', debouncedTerm)
        } else {
          params.delete('term')
        }
        params.delete('page')
        return params
      },
      { replace: true },
    )
  }, [debouncedTerm, urlTerm, setSearchParams])

  const { data, isPending, isError, refetch, isFetching } = useSearchLogs({
    page,
    pageSize: PAGE_SIZE,
    term: urlTerm || undefined,
  })

  function goToPage(next: number) {
    setSearchParams((prev) => {
      const params = new URLSearchParams(prev)
      params.set('page', String(next))
      return params
    })
  }

  const header = (
    <PageHeader
      title="Arama Logları"
      description="Kullanıcıların yaptığı aramaların denetim kayıtları."
    />
  )

  const kvkkBanner = (
    <Alert variant="destructive">
      <ShieldAlert className="h-4 w-4" />
      <AlertTitle>KVKK Uyarısı</AlertTitle>
      <AlertDescription>
        Bu veriler KVKK kapsamında kişisel veri içerebilir. Yalnızca yasal
        amaçlarla inceleyin.
      </AlertDescription>
    </Alert>
  )

  const filterBox = (
    <div className="relative max-w-sm">
      <Search
        className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
        aria-hidden
      />
      <Input
        type="search"
        value={termInput}
        onChange={(event) => setTermInput(event.target.value)}
        placeholder="Terime göre filtrele…"
        aria-label="Arama terimine göre filtrele"
        className="pl-9 pr-9"
      />
      {termInput ? (
        <button
          type="button"
          onClick={() => setTermInput('')}
          aria-label="Filtreyi temizle"
          className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-1 text-muted-foreground hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          <X className="h-4 w-4" />
        </button>
      ) : null}
    </div>
  )

  if (isPending) {
    return (
      <div className="space-y-6">
        {header}
        {kvkkBanner}
        {filterBox}
        <TableSkeleton columns={COLUMNS} rows={8} />
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-6">
        {header}
        {kvkkBanner}
        {filterBox}
        <ErrorState
          message="Arama logları yüklenemedi."
          onRetry={() => void refetch()}
        />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {header}
      {kvkkBanner}
      {filterBox}

      {data.items.length === 0 ? (
        <EmptyState
          icon={Search}
          title={urlTerm ? 'Eşleşen kayıt yok.' : 'Henüz arama kaydı yok.'}
          description={
            urlTerm
              ? `"${urlTerm}" terimini içeren bir arama logu bulunamadı.`
              : 'Kullanıcılar arama yaptığında kayıtlar burada listelenir.'
          }
          action={
            urlTerm ? (
              <Button variant="outline" onClick={() => setTermInput('')}>
                Filtreyi temizle
              </Button>
            ) : undefined
          }
        />
      ) : (
        <>
          <div className="rounded-lg border border-border bg-card">
            <Table>
              <TableHeader>
                <TableRow>
                  {COLUMNS.map((col) => (
                    <TableHead key={col}>{col}</TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((log) => (
                  <TableRow key={log.id}>
                    <TableCell className="max-w-xs truncate font-medium">
                      {log.term}
                    </TableCell>
                    <TableCell className="text-sm">
                      {log.userFullName ? (
                        log.userFullName
                      ) : (
                        <span className="text-muted-foreground">Anonim</span>
                      )}
                    </TableCell>
                    <TableCell>
                      <IpHashCell hash={log.ipHash} />
                    </TableCell>
                    <TableCell className="whitespace-nowrap text-sm text-muted-foreground">
                      {formatDateTime(log.searchedAt)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          <PaginationBar
            page={data.page}
            totalPages={data.totalPages}
            hasPreviousPage={data.hasPreviousPage}
            hasNextPage={data.hasNextPage}
            onPageChange={goToPage}
            disabled={isFetching}
          />
        </>
      )}
    </div>
  )
}

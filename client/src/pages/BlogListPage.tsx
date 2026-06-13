import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Search, X } from 'lucide-react'

import { useBlogList, useBlogSearch } from '@/features/blog'
import { useCategories } from '@/features/category'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { PaginationBar } from '@/components/common/PaginationBar'
import { BlogCard } from '@/components/blog/BlogCard'
import { BlogCardSkeletonGrid } from '@/components/blog/BlogCardSkeleton'
import { CategoryFilter } from '@/components/blog/CategoryFilter'

/** Liste sayfasinda gosterilecek blog sayisi. */
const PAGE_SIZE = 9

/** Arama input'u icin debounce suresi (ms). */
const SEARCH_DEBOUNCE_MS = 350

/**
 * Blog listesi sayfasi. Kategori filtresi + sayfalama + arama terimi URL'de
 * (search params) tutulur; boylece sayfa paylasilabilir/yer imlenebilir ve
 * geri/ileri calisir. Arama terimi bossa duz listeleme (GET /api/blogs),
 * terim varsa arama (GET /api/blogs/search) sorgusu calisir. Input debounce
 * edilerek URL'e (?q=) yazilir; bu da TanStack Query'yi tetikler.
 */
export default function BlogListPage() {
  const [searchParams, setSearchParams] = useSearchParams()

  const pageParam = Number(searchParams.get('page'))
  const page = Number.isInteger(pageParam) && pageParam > 0 ? pageParam : 1
  const categoryId = searchParams.get('categoryId')
  const queryTerm = searchParams.get('q') ?? ''

  // Input'un anlik degeri (URL'den baslatilir). Debounce sonrasi URL'e yansir.
  const [searchInput, setSearchInput] = useState(queryTerm)
  const debouncedInput = useDebouncedValue(searchInput, SEARCH_DEBOUNCE_MS)

  // Geri/ileri ile URL disardan degisirse input'u senkronla.
  useEffect(() => {
    setSearchInput(queryTerm)
  }, [queryTerm])

  // Debounce edilmis terim URL'deki ile farkliysa URL'i guncelle (ilk sayfaya don).
  useEffect(() => {
    const next = debouncedInput.trim()
    if (next === queryTerm) return
    setSearchParams(
      (prev) => {
        const params = new URLSearchParams(prev)
        if (next) {
          params.set('q', next)
        } else {
          params.delete('q')
        }
        // Terim degisince sayfayi sifirla.
        params.delete('page')
        return params
      },
      { replace: true },
    )
  }, [debouncedInput, queryTerm, setSearchParams])

  const isSearching = queryTerm.trim().length > 0

  // Iki sorgu: terim varsa arama, yoksa duz liste. enabled bayraklari ile yalniz
  // biri aktif calisir (hook'larda: search q bos ise enabled=false).
  const listQuery = useBlogList({
    page,
    pageSize: PAGE_SIZE,
    categoryId: categoryId ?? undefined,
  })
  const searchQuery = useBlogSearch({
    q: queryTerm,
    page,
    pageSize: PAGE_SIZE,
    categoryId: categoryId ?? undefined,
  })
  const categoriesQuery = useCategories()

  // Aktif sorgu (arama modunda searchQuery, normalde listQuery).
  const activeQuery = isSearching ? searchQuery : listQuery

  /** URL search param'larini gunceller. */
  function updateParams(next: { page?: number; categoryId?: string | null }) {
    setSearchParams((prev) => {
      const params = new URLSearchParams(prev)

      if (next.categoryId !== undefined) {
        if (next.categoryId) {
          params.set('categoryId', next.categoryId)
        } else {
          params.delete('categoryId')
        }
        // Kategori degisince ilk sayfaya don.
        params.delete('page')
      }

      if (next.page !== undefined) {
        if (next.page > 1) {
          params.set('page', String(next.page))
        } else {
          params.delete('page')
        }
      }

      return params
    })
  }

  function clearSearch() {
    setSearchInput('')
  }

  const hasFilter = Boolean(categoryId)
  // Sayfa/terim gecisi sirasinda (fetching ama veri var) hafif "mesgul" durumu.
  const isBusy = activeQuery.isFetching

  return (
    <section className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
      <PageHeader title="Yazılar" description="Tüm blog yazılarını keşfedin." />

      {/* Arama + filtre cubugu */}
      <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative w-full sm:max-w-sm">
          <Search
            className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
            aria-hidden
          />
          <Input
            type="search"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Yazılarda ara…"
            aria-label="Yazılarda ara"
            className="pl-9 pr-9"
          />
          {searchInput ? (
            <button
              type="button"
              onClick={clearSearch}
              aria-label="Aramayı temizle"
              className="absolute right-2 top-1/2 -translate-y-1/2 rounded-sm p-1 text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              <X className="h-4 w-4" />
            </button>
          ) : null}
        </div>

        <CategoryFilter
          categories={categoriesQuery.data ?? []}
          selectedId={categoryId}
          onChange={(id) => updateParams({ categoryId: id })}
          disabled={categoriesQuery.isPending}
        />
      </div>

      {/* Icerik */}
      <div className="mt-8">
        {activeQuery.isPending ? (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            <BlogCardSkeletonGrid count={PAGE_SIZE} />
          </div>
        ) : activeQuery.isError ? (
          <ErrorState
            message={
              isSearching ? 'Arama yapılamadı.' : 'Yazılar yüklenemedi.'
            }
            onRetry={() => activeQuery.refetch()}
          />
        ) : activeQuery.data.items.length === 0 ? (
          isSearching ? (
            <EmptyState
              icon={Search}
              title="Sonuç bulunamadı"
              description={`"${queryTerm}" için eşleşen yazı yok. Farklı bir terim deneyin.`}
              action={
                <Button variant="outline" onClick={clearSearch}>
                  Aramayı Temizle
                </Button>
              }
            />
          ) : hasFilter ? (
            <EmptyState
              title="Bu kategoride henüz yazı yok"
              description="Farklı bir kategori seçebilir veya filtreyi kaldırabilirsiniz."
              action={
                <Button
                  variant="outline"
                  onClick={() => updateParams({ categoryId: null })}
                >
                  Filtreyi Kaldır
                </Button>
              }
            />
          ) : (
            <EmptyState
              title="Henüz hiç yazı yayımlanmamış"
              description="İlk yazı yayımlandığında burada görünecek."
            />
          )
        ) : (
          <>
            <div
              aria-busy={isBusy}
              className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3"
            >
              {activeQuery.data.items.map((blog) => (
                <BlogCard key={blog.id} blog={blog} />
              ))}
            </div>

            <div className="mt-10">
              <PaginationBar
                page={activeQuery.data.page}
                totalPages={activeQuery.data.totalPages}
                hasPreviousPage={activeQuery.data.hasPreviousPage}
                hasNextPage={activeQuery.data.hasNextPage}
                onPageChange={(next) => updateParams({ page: next })}
                disabled={isBusy}
              />
            </div>
          </>
        )}
      </div>
    </section>
  )
}

import { Link } from 'react-router-dom'
import { ArrowRight } from 'lucide-react'

import { useBlogList } from '@/features/blog'
import { useCategories } from '@/features/category'
import { paths } from '@/routes/paths'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { BlogCard } from '@/components/blog/BlogCard'
import { BlogCardSkeletonGrid } from '@/components/blog/BlogCardSkeleton'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'

/** Anasayfada gosterilecek son blog sayisi. */
const LATEST_COUNT = 6

/**
 * Public anasayfa: hero + son yazilar vitrini + kategori cipleri.
 * Veriler TanStack Query ile cekilir; loading/error/empty durumlari ele alinir.
 */
export default function HomePage() {
  const blogsQuery = useBlogList({ page: 1, pageSize: LATEST_COUNT })
  const categoriesQuery = useCategories()

  return (
    <div>
      <HeroSection />

      {/* Son Yazilar */}
      <section className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex items-end justify-between">
          <h2 className="font-heading text-3xl font-bold tracking-tight">
            Son Yazılar
          </h2>
          <Button asChild variant="ghost">
            <Link to={paths.blogs}>
              Tümünü Gör
              <ArrowRight className="h-4 w-4" />
            </Link>
          </Button>
        </div>

        <div className="mt-8">
          {blogsQuery.isPending ? (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              <BlogCardSkeletonGrid count={LATEST_COUNT} />
            </div>
          ) : blogsQuery.isError ? (
            <ErrorState
              message="Son yazılar yüklenemedi."
              onRetry={() => blogsQuery.refetch()}
            />
          ) : blogsQuery.data.items.length === 0 ? (
            <EmptyState
              title="Henüz yazı yok"
              description="İlk yazı yayımlandığında burada görünecek."
            />
          ) : (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {blogsQuery.data.items.map((blog) => (
                <BlogCard key={blog.id} blog={blog} />
              ))}
            </div>
          )}
        </div>
      </section>

      {/* Kategoriler (varsa) */}
      {categoriesQuery.data && categoriesQuery.data.length > 0 ? (
        <section className="mx-auto max-w-7xl px-4 pb-16 sm:px-6 lg:px-8">
          <h2 className="font-heading text-2xl font-bold tracking-tight">
            Kategoriler
          </h2>
          <div className="mt-4 flex flex-wrap gap-2">
            {categoriesQuery.data.map((category) => (
              <Link
                key={category.id}
                to={`${paths.blogs}?categoryId=${category.id}`}
              >
                <Badge
                  variant="secondary"
                  className="px-3 py-1 text-sm transition-colors hover:bg-secondary/70"
                >
                  {category.categoryName}
                </Badge>
              </Link>
            ))}
          </div>
        </section>
      ) : null}
    </div>
  )
}

/** Karsilama bolumu: gradyan zemin + baslik + CTA. */
function HeroSection() {
  return (
    <section className="bg-gradient-to-br from-primary to-slate-800 text-primary-foreground dark:from-slate-900 dark:to-slate-950">
      <div className="mx-auto max-w-7xl px-4 py-20 text-center sm:px-6 sm:py-28 lg:px-8">
        <h1 className="font-heading text-4xl font-bold tracking-tight sm:text-5xl">
          ZnBlog
        </h1>
        <p className="mx-auto mt-4 max-w-xl text-lg text-primary-foreground/80">
          Düşünceler, yazılar, hikayeler. Yeni bakış açılarını keşfedin.
        </p>
        <div className="mt-8">
          <Button asChild size="lg" variant="secondary">
            <Link to={paths.blogs}>
              Blogları Keşfet
              <ArrowRight className="h-4 w-4" />
            </Link>
          </Button>
        </div>
      </div>
    </section>
  )
}

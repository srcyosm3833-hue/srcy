import { Link } from 'react-router-dom'
import { ImageOff } from 'lucide-react'

import type { BlogListItem } from '@/types'
import { paths } from '@/routes/paths'
import { cn } from '@/lib/utils'
import { resolveAssetUrl } from '@/lib/resolveAssetUrl'
import { Card, CardContent } from '@/components/ui/card'
import { CategoryBadge } from './CategoryBadge'
import { PostMeta } from './PostMeta'

interface BlogCardProps {
  blog: BlogListItem
  /** featured: daha buyuk gorsel + vurgu (anasayfada ilk yazi icin). */
  variant?: 'default' | 'featured'
}

/**
 * Tek bir blog ogesi karti. Tum listelerde (anasayfa, blog listesi) kullanilir.
 * Tum kart detay sayfasina linklenir; kategori badge'i ayri bir link (ic ice
 * link sorunu olmamasi icin badge kart linkinin disinda tutulur).
 */
export function BlogCard({ blog, variant = 'default' }: BlogCardProps) {
  const isFeatured = variant === 'featured'

  return (
    <Card className="group flex h-full flex-col overflow-hidden transition-shadow hover:shadow-md">
      <Link
        to={paths.blogDetail(blog.id)}
        className="block overflow-hidden"
        aria-label={blog.title}
        tabIndex={-1}
      >
        <div
          className={cn(
            'relative w-full overflow-hidden bg-secondary',
            isFeatured ? 'aspect-[16/8]' : 'aspect-video',
          )}
        >
          {blog.coverImage ? (
            <img
              src={resolveAssetUrl(blog.coverImage)}
              alt={blog.title}
              loading="lazy"
              className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center text-muted-foreground">
              <ImageOff className="h-8 w-8" />
            </div>
          )}
        </div>
      </Link>

      <CardContent className="flex flex-1 flex-col p-4 pt-4">
        <div>
          <CategoryBadge
            categoryName={blog.categoryName}
            categoryId={blog.categoryId}
          />
        </div>

        <h3
          className={cn(
            'mt-2 font-heading font-semibold leading-snug',
            isFeatured ? 'text-2xl' : 'text-xl',
          )}
        >
          <Link
            to={paths.blogDetail(blog.id)}
            className="line-clamp-2 transition-colors hover:text-primary/80"
          >
            {blog.title}
          </Link>
        </h3>

        <PostMeta
          authorName={blog.authorName}
          createdAt={blog.createdAt}
          className="mt-2"
        />

        <div className="mt-auto pt-4">
          <Link
            to={paths.blogDetail(blog.id)}
            className="text-sm font-medium text-primary hover:underline"
          >
            Devamını Oku →
          </Link>
        </div>
      </CardContent>
    </Card>
  )
}

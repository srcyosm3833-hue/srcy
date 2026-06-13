import { Link, useParams } from 'react-router-dom'

import { useBlogDetail } from '@/features/blog'
import { normalizeApiError } from '@/lib/api'
import { paths } from '@/routes/paths'
import { getInitials } from '@/lib/initials'
import { formatDate } from '@/lib/formatDate'
import { resolveAssetUrl } from '@/lib/resolveAssetUrl'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { CategoryBadge } from '@/components/blog/CategoryBadge'
import { LikeButton } from '@/components/blog/LikeButton'
import { BlogDetailSkeleton } from '@/components/blog/BlogDetailSkeleton'
import { ErrorState } from '@/components/common/ErrorState'
import { EmptyState } from '@/components/common/EmptyState'
import { CommentsSection } from '@/components/comments/CommentsSection'

/**
 * Blog detay sayfasi: tek blogun makalesi (yazar + kategori + icerik) + yorumlar.
 * id URL parametresinden alinir. Loading (tam-sayfa iskelet), 404 (yazi
 * bulunamadi) ve diger hata durumlari ayri ele alinir.
 */
export default function BlogDetailPage() {
  const { id } = useParams<{ id: string }>()
  const blogQuery = useBlogDetail(id)

  return (
    <article className="mx-auto max-w-3xl px-4 py-12 sm:px-6 lg:px-8">
      {blogQuery.isPending ? (
        <BlogDetailSkeleton />
      ) : blogQuery.isError ? (
        <DetailError onRetry={() => blogQuery.refetch()} error={blogQuery.error} />
      ) : (
        <>
          {/* Makale basligi */}
          <header>
            <CategoryBadge
              categoryName={blogQuery.data.categoryName}
              categoryId={blogQuery.data.categoryId}
            />
            <h1 className="mt-4 font-heading text-4xl font-bold leading-tight tracking-tight sm:text-5xl">
              {blogQuery.data.title}
            </h1>

            <div className="mt-5 flex flex-wrap items-center gap-3 text-sm text-muted-foreground">
              <div className="flex items-center gap-2">
                <Avatar className="h-8 w-8">
                  <AvatarFallback>
                    {getInitials(blogQuery.data.authorName, null)}
                  </AvatarFallback>
                </Avatar>
                <span className="font-medium text-foreground">
                  {blogQuery.data.authorName}
                </span>
              </div>
              <span aria-hidden>·</span>
              <time dateTime={blogQuery.data.createdAt}>
                {formatDate(blogQuery.data.createdAt)}
              </time>
              {blogQuery.data.updatedAt ? (
                <>
                  <span aria-hidden>·</span>
                  <span>
                    Güncellendi: {formatDate(blogQuery.data.updatedAt)}
                  </span>
                </>
              ) : null}
            </div>

            {/* Begeni butonu */}
            <div className="mt-6">
              <LikeButton
                blogId={blogQuery.data.id}
                likeCount={blogQuery.data.likeCount}
                isLiked={blogQuery.data.isLikedByCurrentUser}
              />
            </div>
          </header>

          {/* Kapak gorseli */}
          {blogQuery.data.coverImage ? (
            <img
              src={resolveAssetUrl(blogQuery.data.coverImage)}
              alt={blogQuery.data.title}
              className="my-8 aspect-video w-full rounded-xl object-cover"
            />
          ) : (
            <div className="my-8" />
          )}

          {/* Icerik gorseli (varsa) */}
          {blogQuery.data.blogImage ? (
            <img
              src={resolveAssetUrl(blogQuery.data.blogImage)}
              alt=""
              className="my-6 w-full rounded-lg object-cover"
            />
          ) : null}

          {/* Icerik metni: duz metin, satir sonlari korunur (prose-blog tipografi). */}
          <div className="prose-blog whitespace-pre-wrap">
            {blogQuery.data.description}
          </div>

          {/* Yorumlar */}
          {id ? <CommentsSection blogId={id} /> : null}
        </>
      )}
    </article>
  )
}

/** Hata ayrimi: 404 -> "yazi bulunamadi" (empty benzeri), diger -> retry'li hata. */
function DetailError({
  error,
  onRetry,
}: {
  error: unknown
  onRetry: () => void
}) {
  const normalized = normalizeApiError(error)

  if (normalized.status === 404) {
    return (
      <EmptyState
        title="Yazı bulunamadı"
        description="Aradığınız yazı kaldırılmış veya hiç var olmamış olabilir."
        action={
          <Button asChild>
            <Link to={paths.blogs}>Yazılara Dön</Link>
          </Button>
        }
      />
    )
  }

  return <ErrorState message="Yazı yüklenemedi." onRetry={onRetry} />
}

import { useState } from 'react'
import { Link } from 'react-router-dom'
import { MessageSquare } from 'lucide-react'
import { toast } from 'sonner'

import { useAddComment, useComments } from '@/features/comment'
import { useAuth } from '@/features/auth'
import { normalizeApiError } from '@/lib/api'
import { paths } from '@/routes/paths'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { PaginationBar } from '@/components/common/PaginationBar'
import { CommentForm } from './CommentForm'
import { CommentItem } from './CommentItem'
import { CommentListSkeleton } from './CommentItemSkeleton'

interface CommentsSectionProps {
  blogId: string
}

/**
 * Blog detayinin yorum bolumu. Yorumlari sayfali ceker; loading/error/empty
 * durumlarini ele alir. Giris yapan kullaniciya yorum formu, anonime giris CTA'si
 * gosterilir. Yorum ekleme/duzenleme/silme ve alt yorumlar (lazy) CommentItem
 * uzerinden yonetilir; mutation'lar ilgili query key'leri invalidate eder.
 */
export function CommentsSection({ blogId }: CommentsSectionProps) {
  const { isAuthenticated } = useAuth()
  const [page, setPage] = useState(1)

  const commentsQuery = useComments(blogId, page)
  const addComment = useAddComment(blogId)

  const total = commentsQuery.data?.totalCount ?? 0

  async function handleAddComment(text: string) {
    try {
      await addComment.mutateAsync({ commentText: text })
      // Yeni yorum en yeni listede; ilk sayfaya don.
      setPage(1)
    } catch (error) {
      const normalized = normalizeApiError(error)
      toast.error('Yorum gönderilemedi.', { description: normalized.message })
      throw error
    }
  }

  return (
    <section className="mt-16 border-t border-border pt-8">
      <h2 className="font-heading text-2xl font-bold tracking-tight">
        Yorumlar
        {commentsQuery.data ? (
          <span className="ml-2 text-lg font-normal text-muted-foreground">
            ({total})
          </span>
        ) : null}
      </h2>

      {/* Yorum yapma alani: giris yapana form, anonime CTA. */}
      <div className="mt-4">
        {isAuthenticated ? (
          <CommentForm
            fieldName="commentText"
            placeholder="Yorumunuzu yazın…"
            submitLabel="Yorum Yap"
            onSubmit={handleAddComment}
          />
        ) : (
          <div className="flex flex-wrap items-center gap-3 rounded-lg border border-border bg-secondary/40 px-4 py-3">
            <p className="text-sm text-muted-foreground">
              Yorum yapmak için giriş yapın.
            </p>
            <Button asChild variant="outline" size="sm">
              <Link to={paths.login}>Giriş Yap</Link>
            </Button>
          </div>
        )}
      </div>

      <Separator className="my-6" />

      {/* Yorum listesi durumlari */}
      {commentsQuery.isPending ? (
        <CommentListSkeleton count={3} />
      ) : commentsQuery.isError ? (
        <ErrorState
          message="Yorumlar yüklenemedi."
          onRetry={() => commentsQuery.refetch()}
        />
      ) : commentsQuery.data.items.length === 0 ? (
        <EmptyState
          icon={MessageSquare}
          title="Henüz yorum yok"
          description="İlk yorumu siz yapın!"
        />
      ) : (
        <div className="space-y-6">
          {commentsQuery.data.items.map((comment) => (
            <CommentItem key={comment.id} blogId={blogId} comment={comment} />
          ))}

          <PaginationBar
            page={commentsQuery.data.page}
            totalPages={commentsQuery.data.totalPages}
            hasPreviousPage={commentsQuery.data.hasPreviousPage}
            hasNextPage={commentsQuery.data.hasNextPage}
            onPageChange={setPage}
            disabled={commentsQuery.isFetching}
          />
        </div>
      )}
    </section>
  )
}

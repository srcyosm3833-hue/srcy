import { useState } from 'react'
import { toast } from 'sonner'

import { useAuth } from '@/features/auth'
import { useAddReply, useReplies } from '@/features/reply'
import { normalizeApiError } from '@/lib/api'
import { ErrorState } from '@/components/common/ErrorState'
import { PaginationBar } from '@/components/common/PaginationBar'
import { CommentForm } from './CommentForm'
import { ReplyItem } from './ReplyItem'
import { CommentItemSkeleton } from './CommentItemSkeleton'

interface RepliesSectionProps {
  blogId: string
  commentId: string
}

/**
 * Bir yorumun alt yorumlari (yanitlari). Kullanici "Yanitlar" toggle'ina basinca
 * acilir; veriler bu noktada lazy yuklenir (yeni GET /api/comments/{id}/replies).
 * Sayfali; PaginationBar ile gezilir. Giris yapan kullaniciya yanit formu gosterilir.
 * loading/error/empty durumlari ele alinir.
 */
export function RepliesSection({ blogId, commentId }: RepliesSectionProps) {
  const { isAuthenticated } = useAuth()
  const [page, setPage] = useState(1)

  const repliesQuery = useReplies(commentId, page)
  const addReply = useAddReply(blogId, commentId)

  async function handleAddReply(text: string) {
    try {
      await addReply.mutateAsync({ subCommentText: text })
      // Yeni yanit en yeni sayfada; ilk sayfaya don.
      setPage(1)
    } catch (error) {
      const normalized = normalizeApiError(error)
      toast.error('Yanıt gönderilemedi.', { description: normalized.message })
      throw error
    }
  }

  return (
    <div className="mt-3 space-y-4 border-l-2 border-border/60 pl-4">
      {/* Yanit listesi durumlari */}
      {repliesQuery.isPending ? (
        <div className="space-y-4">
          <CommentItemSkeleton />
          <CommentItemSkeleton />
        </div>
      ) : repliesQuery.isError ? (
        <ErrorState
          message="Yanıtlar yüklenemedi."
          onRetry={() => repliesQuery.refetch()}
          className="py-8"
        />
      ) : repliesQuery.data.items.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          Henüz yanıt yok. İlk yanıtı siz yazın.
        </p>
      ) : (
        <>
          <div className="space-y-4">
            {repliesQuery.data.items.map((reply) => (
              <ReplyItem key={reply.id} blogId={blogId} reply={reply} />
            ))}
          </div>
          <PaginationBar
            page={repliesQuery.data.page}
            totalPages={repliesQuery.data.totalPages}
            hasPreviousPage={repliesQuery.data.hasPreviousPage}
            hasNextPage={repliesQuery.data.hasNextPage}
            onPageChange={setPage}
            disabled={repliesQuery.isFetching}
          />
        </>
      )}

      {/* Yanit ekleme (yalnizca giris yapan). */}
      {isAuthenticated ? (
        <div className="pt-1">
          <CommentForm
            fieldName="subCommentText"
            placeholder="Yanıtınızı yazın…"
            submitLabel="Yanıtla"
            onSubmit={handleAddReply}
            compact
          />
        </div>
      ) : null}
    </div>
  )
}

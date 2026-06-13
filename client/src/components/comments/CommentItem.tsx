import { useState } from 'react'
import { MessageSquare, Pencil } from 'lucide-react'
import { toast } from 'sonner'

import type { Comment } from '@/types'
import { useAuth } from '@/features/auth'
import { useDeleteComment, useUpdateComment } from '@/features/comment'
import { normalizeApiError } from '@/lib/api'
import { formatDate } from '@/lib/formatDate'
import { getInitials } from '@/lib/initials'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import { CommentForm } from './CommentForm'
import { DeleteConfirmButton } from './DeleteConfirmButton'
import { RepliesSection } from './RepliesSection'

interface CommentItemProps {
  blogId: string
  comment: Comment
}

/**
 * Tek bir yorum ogesi. Goruntuleme + sahiplik aksiyonlari + yanit (reply) toggle:
 *  - Duzenle: yalnizca sahip (inline form).
 *  - Sil: sahip veya admin (AlertDialog onayi). Alt yorumlar DB'de cascade silinir.
 *  - Yanitlar: subCommentCount rozetli toggle; acilinca RepliesSection lazy yuklenir.
 * Sahiplik comment.userId === giris yapan kullanici id ile karsilastirilir.
 */
export function CommentItem({ blogId, comment }: CommentItemProps) {
  const { user, isAdmin } = useAuth()
  const [isEditing, setIsEditing] = useState(false)
  const [repliesOpen, setRepliesOpen] = useState(false)

  const updateComment = useUpdateComment(blogId)
  const deleteComment = useDeleteComment(blogId)

  const isOwner = Boolean(user?.id) && user?.id === comment.userId
  const canEdit = isOwner
  const canDelete = isOwner || isAdmin

  async function handleUpdate(text: string) {
    try {
      await updateComment.mutateAsync({
        commentId: comment.id,
        payload: { commentText: text },
      })
      setIsEditing(false)
    } catch (error) {
      const normalized = normalizeApiError(error)
      toast.error('Yorum güncellenemedi.', { description: normalized.message })
      throw error
    }
  }

  async function handleDelete() {
    try {
      await deleteComment.mutateAsync(comment.id)
    } catch (error) {
      const normalized = normalizeApiError(error)
      toast.error('Yorum silinemedi.', { description: normalized.message })
    }
  }

  const replyButtonLabel =
    comment.subCommentCount > 0
      ? `${comment.subCommentCount} yanıt`
      : 'Yanıtla'

  return (
    <article className="flex gap-3">
      <Avatar className="h-9 w-9 shrink-0">
        <AvatarFallback>{getInitials(comment.displayName, null)}</AvatarFallback>
      </Avatar>

      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-x-2 gap-y-0.5">
          <strong className="text-sm font-semibold text-foreground">
            {comment.displayName}
          </strong>
          <time
            dateTime={comment.createdAt}
            className="text-xs text-muted-foreground"
          >
            {formatDate(comment.createdAt)}
          </time>
          {comment.isEdited ? (
            <span className="text-xs text-muted-foreground">(düzenlendi)</span>
          ) : null}
        </div>

        {isEditing ? (
          <div className="mt-2">
            <CommentForm
              fieldName="commentText"
              defaultValue={comment.commentText}
              placeholder="Yorumunuzu yazın…"
              submitLabel="Kaydet"
              onSubmit={handleUpdate}
              onCancel={() => setIsEditing(false)}
              compact
              autoFocus
            />
          </div>
        ) : (
          <p className="mt-1 whitespace-pre-wrap text-sm leading-relaxed text-foreground/90">
            {comment.commentText}
          </p>
        )}

        {/* Aksiyon cubugu: yanitlar toggle + (sahip/admin ise) duzenle/sil. */}
        {!isEditing ? (
          <div className="mt-1 flex flex-wrap items-center gap-1">
            <Button
              variant="ghost"
              size="sm"
              className="h-auto px-2 py-1 text-xs text-muted-foreground hover:text-foreground"
              onClick={() => setRepliesOpen((open) => !open)}
              aria-expanded={repliesOpen}
            >
              <MessageSquare className="h-3.5 w-3.5" />
              {replyButtonLabel}
            </Button>

            {canEdit ? (
              <Button
                variant="ghost"
                size="sm"
                className="h-auto px-2 py-1 text-xs text-muted-foreground hover:text-foreground"
                onClick={() => setIsEditing(true)}
              >
                <Pencil className="h-3.5 w-3.5" />
                Düzenle
              </Button>
            ) : null}

            {canDelete ? (
              <DeleteConfirmButton
                title="Yorumu sil"
                description="Bu yorumu silmek istediğinize emin misiniz? Yoruma ait yanıtlar da silinir. Bu işlem geri alınamaz."
                onConfirm={handleDelete}
              />
            ) : null}
          </div>
        ) : null}

        {/* Yanitlar (lazy): yalnizca acikken mount edilir -> tiklayinca veri cekilir. */}
        {repliesOpen ? (
          <RepliesSection blogId={blogId} commentId={comment.id} />
        ) : null}
      </div>
    </article>
  )
}

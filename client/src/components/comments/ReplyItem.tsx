import { useState } from 'react'
import { Pencil } from 'lucide-react'
import { toast } from 'sonner'

import type { SubComment } from '@/types'
import { useAuth } from '@/features/auth'
import { useDeleteReply, useUpdateReply } from '@/features/reply'
import { normalizeApiError } from '@/lib/api'
import { formatDate } from '@/lib/formatDate'
import { getInitials } from '@/lib/initials'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import { CommentForm } from './CommentForm'
import { DeleteConfirmButton } from './DeleteConfirmButton'

interface ReplyItemProps {
  blogId: string
  reply: SubComment
}

/**
 * Tek bir alt yorum (yanit). Goruntuleme + sahiplik aksiyonlari:
 *  - Duzenle: yalnizca sahip (inline form).
 *  - Sil: sahip veya admin (AlertDialog onayi).
 * Sahiplik reply.userId === giris yapan kullanici id ile karsilastirilir.
 */
export function ReplyItem({ blogId, reply }: ReplyItemProps) {
  const { user, isAdmin } = useAuth()
  const [isEditing, setIsEditing] = useState(false)

  const updateReply = useUpdateReply(blogId, reply.commentId)
  const deleteReply = useDeleteReply(blogId, reply.commentId)

  const isOwner = Boolean(user?.id) && user?.id === reply.userId
  const canEdit = isOwner
  const canDelete = isOwner || isAdmin

  async function handleUpdate(text: string) {
    try {
      await updateReply.mutateAsync({
        replyId: reply.id,
        payload: { subCommentText: text },
      })
      setIsEditing(false)
    } catch (error) {
      // Alana eslenemeyen hatalar buraya gelir -> toast (alan hatasi formda gosterilir).
      const normalized = normalizeApiError(error)
      toast.error('Yanıt güncellenemedi.', { description: normalized.message })
      throw error
    }
  }

  async function handleDelete() {
    try {
      await deleteReply.mutateAsync(reply.id)
    } catch (error) {
      const normalized = normalizeApiError(error)
      toast.error('Yanıt silinemedi.', { description: normalized.message })
    }
  }

  return (
    <article className="flex gap-3">
      <Avatar className="h-8 w-8 shrink-0">
        <AvatarFallback className="text-xs">
          {getInitials(reply.displayName, null)}
        </AvatarFallback>
      </Avatar>

      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-x-2 gap-y-0.5">
          <strong className="text-sm font-semibold text-foreground">
            {reply.displayName}
          </strong>
          <time
            dateTime={reply.createdAt}
            className="text-xs text-muted-foreground"
          >
            {formatDate(reply.createdAt)}
          </time>
          {reply.isEdited ? (
            <span className="text-xs text-muted-foreground">(düzenlendi)</span>
          ) : null}
        </div>

        {isEditing ? (
          <div className="mt-2">
            <CommentForm
              fieldName="subCommentText"
              defaultValue={reply.subCommentText}
              placeholder="Yanıtınızı yazın…"
              submitLabel="Kaydet"
              onSubmit={handleUpdate}
              onCancel={() => setIsEditing(false)}
              compact
              autoFocus
            />
          </div>
        ) : (
          <>
            <p className="mt-1 whitespace-pre-wrap text-sm leading-relaxed text-foreground/90">
              {reply.subCommentText}
            </p>

            {canEdit || canDelete ? (
              <div className="mt-1 flex items-center gap-1">
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
                    title="Yanıtı sil"
                    description="Bu yanıtı silmek istediğinize emin misiniz? Bu işlem geri alınamaz."
                    onConfirm={handleDelete}
                  />
                ) : null}
              </div>
            ) : null}
          </>
        )}
      </div>
    </article>
  )
}

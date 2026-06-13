import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { MessageSquare, Trash2 } from 'lucide-react'
import { toast } from 'sonner'

import type { CommentModerationItem } from '@/types'
import { useAdminComments, useDeleteCommentModeration } from '@/features/comment'
import { normalizeApiError } from '@/lib/api'
import { formatDate } from '@/lib/formatDate'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { PaginationBar } from '@/components/common/PaginationBar'
import { TableSkeleton } from '@/components/admin/TableSkeleton'
import { ConfirmDeleteDialog } from '@/components/admin/ConfirmDeleteDialog'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

const PAGE_SIZE = 20
const COLUMNS = ['Blog', 'Yazar', 'Yorum', 'Tür', 'Tarih', 'İşlemler']

/**
 * Admin yorum moderasyonu. Tum yorum ve yanitlar tek sayfali tabloda listelenir;
 * her satirda blog basligi, yazar, kisaltilmis metin, tur rozeti (Yorum/Yanit) ve
 * tarih gosterilir. Silme isReply bayragina gore dogru ucu cagirir (queries.ts).
 *
 * Yetki: backend yalnizca Admin'e acar (Manager 403). Sidebar'da link de yalniz
 * Admin'e gosterildigi icin bu sayfa pratikte yalniz Admin'e gorunur.
 */
export default function AdminCommentsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const page = Math.max(1, Number(searchParams.get('page')) || 1)

  const { data, isPending, isError, refetch, isFetching } = useAdminComments(
    page,
    PAGE_SIZE,
  )
  const deleteComment = useDeleteCommentModeration()

  const [toDelete, setToDelete] = useState<CommentModerationItem | null>(null)

  function goToPage(next: number) {
    setSearchParams((prev) => {
      const params = new URLSearchParams(prev)
      params.set('page', String(next))
      return params
    })
  }

  async function handleDelete() {
    if (!toDelete) return
    try {
      await deleteComment.mutateAsync(toDelete)
      toast.success(toDelete.isReply ? 'Yanıt silindi.' : 'Yorum silindi.')
    } catch (error) {
      toast.error('Silme işlemi başarısız.', {
        description: normalizeApiError(error).message,
      })
      throw error
    }
  }

  const header = (
    <PageHeader
      title="Yorum Yönetimi"
      description="Tüm yorum ve yanıtları görüntüleyin, uygunsuz olanları kaldırın."
    />
  )

  if (isPending) {
    return (
      <div className="space-y-8">
        {header}
        <TableSkeleton columns={COLUMNS} rows={8} />
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState
          message="Yorumlar yüklenemedi."
          onRetry={() => void refetch()}
        />
      </div>
    )
  }

  if (data.items.length === 0) {
    return (
      <div className="space-y-8">
        {header}
        <EmptyState
          icon={MessageSquare}
          title="Henüz yorum yok."
          description="Bloglara yorum yapıldığında burada listelenir."
        />
      </div>
    )
  }

  return (
    <div className="space-y-8">
      {header}

      <div className="rounded-lg border border-border bg-card">
        <Table>
          <TableHeader>
            <TableRow>
              {COLUMNS.map((col) => (
                <TableHead
                  key={col}
                  className={col === 'İşlemler' ? 'text-right' : undefined}
                >
                  {col}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.map((item) => (
              <TableRow key={item.id}>
                <TableCell className="max-w-40 truncate font-medium">
                  {item.blogTitle}
                </TableCell>
                <TableCell className="whitespace-nowrap text-sm">
                  {item.authorName}
                </TableCell>
                <TableCell className="max-w-sm">
                  <span className="line-clamp-2 text-sm text-muted-foreground">
                    {item.text}
                  </span>
                </TableCell>
                <TableCell>
                  <Badge variant={item.isReply ? 'outline' : 'secondary'}>
                    {item.isReply ? 'Yanıt' : 'Yorum'}
                  </Badge>
                </TableCell>
                <TableCell className="whitespace-nowrap text-sm text-muted-foreground">
                  {formatDate(item.createdAt)}
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label="Sil"
                    className="text-destructive hover:text-destructive"
                    onClick={() => setToDelete(item)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
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

      <ConfirmDeleteDialog
        open={toDelete !== null}
        onOpenChange={(open) => {
          if (!open) setToDelete(null)
        }}
        title={toDelete?.isReply ? 'Yanıtı sil' : 'Yorumu sil'}
        description={
          toDelete
            ? `${
                toDelete.isReply ? 'Bu yanıt' : 'Bu yorum'
              } kalıcı olarak silinecek. Bu işlem geri alınamaz.`
            : ''
        }
        onConfirm={handleDelete}
      />
    </div>
  )
}

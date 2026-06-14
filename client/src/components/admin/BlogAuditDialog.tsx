import { useBlogAuditDetail } from '@/features/blog'
import { normalizeApiError } from '@/lib/api'
import { formatDateTime } from '@/lib/formatDate'
import { IpHashCell } from '@/components/admin/IpHashCell'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Skeleton } from '@/components/ui/skeleton'
import { Badge } from '@/components/ui/badge'

interface BlogAuditDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  /** Audit detayi gosterilecek blog id'si (null ise dialog kapali, sorgu durur). */
  blogId: string | null
}

/**
 * Blog audit detay dialog'u (Admin/Manager). GET /api/admin/blogs/{id}'den yazar,
 * oluşturma/güncelleme tarihi, begeni sayisi ve audit IP hash'ini ceker. IP Hash
 * yalnizca bu admin ucundan gelir (public detayda yoktur). Acilinca lazy ceker
 * (enabled: blogId varsa).
 */
export function BlogAuditDialog({
  open,
  onOpenChange,
  blogId,
}: BlogAuditDialogProps) {
  const { data, isPending, isError, error } = useBlogAuditDetail(
    blogId ?? undefined,
  )

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Blog Denetim Detayı</DialogTitle>
          <DialogDescription>
            Yönetimsel denetim bilgileri (yazar, tarihler ve IP hash'i).
          </DialogDescription>
        </DialogHeader>

        {blogId === null ? null : isPending ? (
          <div className="space-y-3">
            {Array.from({ length: 5 }).map((_, index) => (
              <Skeleton key={index} className="h-5 w-full" />
            ))}
          </div>
        ) : isError ? (
          <p className="text-sm text-destructive">
            Detay yüklenemedi: {normalizeApiError(error).message}
          </p>
        ) : (
          <dl className="space-y-3 text-sm">
            <div className="flex items-start justify-between gap-4">
              <dt className="text-muted-foreground">Başlık</dt>
              <dd className="max-w-[60%] text-right font-medium">
                {data.title}
              </dd>
            </div>
            <div className="flex items-center justify-between gap-4">
              <dt className="text-muted-foreground">Kategori</dt>
              <dd>
                <Badge variant="secondary">{data.categoryName}</Badge>
              </dd>
            </div>
            <div className="flex items-center justify-between gap-4">
              <dt className="text-muted-foreground">Yazar</dt>
              <dd className="font-medium">{data.authorName}</dd>
            </div>
            <div className="flex items-center justify-between gap-4">
              <dt className="text-muted-foreground">Oluşturma Tarihi</dt>
              <dd>{formatDateTime(data.createdAt)}</dd>
            </div>
            <div className="flex items-center justify-between gap-4">
              <dt className="text-muted-foreground">Güncelleme Tarihi</dt>
              <dd>
                {data.updatedAt ? (
                  formatDateTime(data.updatedAt)
                ) : (
                  <span className="text-muted-foreground">—</span>
                )}
              </dd>
            </div>
            <div className="flex items-center justify-between gap-4">
              <dt className="text-muted-foreground">Beğeni Sayısı</dt>
              <dd className="tabular-nums">{data.likeCount}</dd>
            </div>
            <div className="flex items-center justify-between gap-4">
              <dt className="text-muted-foreground">Oluşturan IP (hash)</dt>
              <dd>
                <IpHashCell hash={data.creatorIpHash} />
              </dd>
            </div>
          </dl>
        )}
      </DialogContent>
    </Dialog>
  )
}

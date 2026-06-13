import { useState } from 'react'
import {
  ExternalLink,
  MoreHorizontal,
  Pencil,
  Plus,
  Share2,
  Trash2,
} from 'lucide-react'
import { toast } from 'sonner'

import type { SocialMedia } from '@/types'
import { useSocialMedia, useDeleteSocialMedia } from '@/features/contact'
import { normalizeApiError } from '@/lib/api'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { ConfirmDeleteDialog } from '@/components/admin/ConfirmDeleteDialog'
import { SocialMediaFormDialog } from '@/components/admin/SocialMediaFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'

/**
 * Admin sosyal medya yonetimi. Kart izgarasi + dialog tabanli CRUD. Her kart
 * ikon/baslik/url ve aksiyon menusu gosterir. Silme onayli; basari toast +
 * invalidation. Bos durumda EmptyState.
 */
export default function AdminSocialMediaPage() {
  const { data, isPending, isError, refetch } = useSocialMedia()
  const deleteSocial = useDeleteSocialMedia()

  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<SocialMedia | null>(null)
  const [toDelete, setToDelete] = useState<SocialMedia | null>(null)

  function openCreate() {
    setEditing(null)
    setFormOpen(true)
  }

  function openEdit(item: SocialMedia) {
    setEditing(item)
    setFormOpen(true)
  }

  async function handleDelete() {
    if (!toDelete) return
    try {
      await deleteSocial.mutateAsync(toDelete.id)
      toast.success('Bağlantı silindi.')
    } catch (error) {
      toast.error('Bağlantı silinemedi.', {
        description: normalizeApiError(error).message,
      })
      throw error
    }
  }

  const header = (
    <PageHeader
      title="Sosyal Medya"
      description="Sitede gösterilen sosyal medya bağlantılarını yönetin."
      action={
        <Button onClick={openCreate}>
          <Plus className="h-4 w-4" />
          Ekle
        </Button>
      }
    />
  )

  if (isPending) {
    return (
      <div className="space-y-8">
        {header}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, index) => (
            <Skeleton key={index} className="h-24 w-full rounded-lg" />
          ))}
        </div>
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState
          message="Sosyal medya bağlantıları yüklenemedi."
          onRetry={() => void refetch()}
        />
      </div>
    )
  }

  return (
    <div className="space-y-8">
      {header}

      {data.length === 0 ? (
        <EmptyState
          icon={Share2}
          title="Henüz sosyal medya linki eklenmemiş."
          description="Ziyaretçilerin sizi bulabilmesi için bir bağlantı ekleyin."
          action={
            <Button onClick={openCreate}>
              <Plus className="h-4 w-4" />
              Ekle
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {data.map((item) => (
            <Card key={item.id}>
              <CardContent className="flex items-center gap-4 p-4">
                <span
                  className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-secondary text-lg text-secondary-foreground"
                  aria-hidden
                >
                  {item.icon.length <= 2 ? (
                    item.icon
                  ) : (
                    <Share2 className="h-5 w-5" />
                  )}
                </span>
                <div className="min-w-0 flex-1">
                  <p className="truncate font-semibold">{item.title}</p>
                  <a
                    href={item.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="flex items-center gap-1 truncate text-sm text-muted-foreground hover:text-foreground"
                  >
                    <span className="truncate">{item.url}</span>
                    <ExternalLink className="h-3 w-3 shrink-0" />
                  </a>
                </div>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon" aria-label="İşlemler">
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem onSelect={() => openEdit(item)}>
                      <Pencil className="h-4 w-4" />
                      Düzenle
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      className="text-destructive focus:text-destructive"
                      onSelect={() => setToDelete(item)}
                    >
                      <Trash2 className="h-4 w-4" />
                      Sil
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <SocialMediaFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        socialMedia={editing}
      />

      <ConfirmDeleteDialog
        open={toDelete !== null}
        onOpenChange={(open) => {
          if (!open) setToDelete(null)
        }}
        title="Bağlantıyı sil"
        description={
          toDelete
            ? `"${toDelete.title}" bağlantısı silinecek. Bu işlem geri alınamaz.`
            : ''
        }
        onConfirm={handleDelete}
      />
    </div>
  )
}

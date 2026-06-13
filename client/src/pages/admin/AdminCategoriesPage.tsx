import { useState } from 'react'
import { MoreHorizontal, Pencil, Plus, Tag, Trash2 } from 'lucide-react'
import { toast } from 'sonner'

import type { Category } from '@/types'
import { useCategories, useDeleteCategory } from '@/features/category'
import { normalizeApiError } from '@/lib/api'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { TableSkeleton } from '@/components/admin/TableSkeleton'
import { ConfirmDeleteDialog } from '@/components/admin/ConfirmDeleteDialog'
import { CategoryFormDialog } from '@/components/admin/CategoryFormDialog'
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'

const COLUMNS = ['Kategori Adı', 'Blog Sayısı', 'İşlemler']

/**
 * Admin kategori yonetimi. Tablo + dialog tabanli CRUD (basit tek-alanli form).
 * Silme: kategoriye bagli blog varsa (blogCount > 0) silme ENGELLENIR ve uyari
 * gosterilir (backend de Restrict ile 409 doner — istemcide on-engel + sunucu
 * yedek). Bossa normal onayli silme.
 */
export default function AdminCategoriesPage() {
  const { data, isPending, isError, refetch } = useCategories()
  const deleteCategory = useDeleteCategory()

  // Dialog/onay durumlari.
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<Category | null>(null)
  const [toDelete, setToDelete] = useState<Category | null>(null)
  const [blockedCategory, setBlockedCategory] = useState<Category | null>(null)

  function openCreate() {
    setEditing(null)
    setFormOpen(true)
  }

  function openEdit(category: Category) {
    setEditing(category)
    setFormOpen(true)
  }

  function requestDelete(category: Category) {
    // Bagli blog varsa silmeyi engelle ve uyari goster.
    if (category.blogCount > 0) {
      setBlockedCategory(category)
    } else {
      setToDelete(category)
    }
  }

  async function handleDelete() {
    if (!toDelete) return
    try {
      await deleteCategory.mutateAsync(toDelete.id)
      toast.success('Kategori silindi.')
    } catch (error) {
      const normalized = normalizeApiError(error)
      // Yari yolda bagli blog tespit edilirse backend 409 doner: ozel mesaj.
      if (normalized.status === 409) {
        toast.error('Kategori silinemedi.', {
          description: 'Bu kategoriye bağlı bloglar var.',
        })
      } else {
        toast.error('Kategori silinemedi.', { description: normalized.message })
      }
      throw error
    }
  }

  const header = (
    <PageHeader
      title="Kategori Yönetimi"
      description="Blog kategorilerini ekleyin, düzenleyin veya silin."
      action={
        <Button onClick={openCreate}>
          <Plus className="h-4 w-4" />
          Yeni Kategori
        </Button>
      }
    />
  )

  if (isPending) {
    return (
      <div className="space-y-8">
        {header}
        <TableSkeleton columns={COLUMNS} rows={5} />
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState
          message="Kategoriler yüklenemedi."
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
          icon={Tag}
          title="Henüz kategori eklenmemiş."
          description="Bloglarınızı gruplamak için ilk kategoriyi oluşturun."
          action={
            <Button onClick={openCreate}>
              <Plus className="h-4 w-4" />
              Yeni Kategori
            </Button>
          }
        />
      ) : (
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
              {data.map((category) => (
                <TableRow key={category.id}>
                  <TableCell className="font-medium">
                    {category.categoryName}
                  </TableCell>
                  <TableCell>
                    <Badge variant="secondary">{category.blogCount}</Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button
                          variant="ghost"
                          size="icon"
                          aria-label="İşlemler"
                        >
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onSelect={() => openEdit(category)}>
                          <Pencil className="h-4 w-4" />
                          Düzenle
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                          className="text-destructive focus:text-destructive"
                          onSelect={() => requestDelete(category)}
                        >
                          <Trash2 className="h-4 w-4" />
                          Sil
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Olustur/Duzenle dialog'u */}
      <CategoryFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        category={editing}
      />

      {/* Normal silme onayi (bagli blog yok) */}
      <ConfirmDeleteDialog
        open={toDelete !== null}
        onOpenChange={(open) => {
          if (!open) setToDelete(null)
        }}
        title="Kategoriyi sil"
        description={
          toDelete
            ? `"${toDelete.categoryName}" kategorisi silinecek. Bu işlem geri alınamaz.`
            : ''
        }
        onConfirm={handleDelete}
      />

      {/* Bagli blog varken silinemez uyarisi (sadece bilgilendirir) */}
      <AlertDialog
        open={blockedCategory !== null}
        onOpenChange={(open) => {
          if (!open) setBlockedCategory(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Kategori silinemiyor</AlertDialogTitle>
            <AlertDialogDescription>
              {blockedCategory
                ? `Bu kategoriye bağlı ${blockedCategory.blogCount} blog var. Silmeden önce blogları başka bir kategoriye taşıyın veya silin.`
                : ''}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogAction onClick={() => setBlockedCategory(null)}>
              Tamam
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

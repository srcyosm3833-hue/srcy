import { useState } from 'react'
import { Pencil, Plus, Shield, Trash2 } from 'lucide-react'
import { toast } from 'sonner'

import type { Role } from '@/types'
import { useDeleteRole, useRoles } from '@/features/role'
import { normalizeApiError } from '@/lib/api'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { TableSkeleton } from '@/components/admin/TableSkeleton'
import { ConfirmDeleteDialog } from '@/components/admin/ConfirmDeleteDialog'
import { RoleFormDialog } from '@/components/admin/RoleFormDialog'
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

const COLUMNS = ['Ad', 'Kullanıcı Sayısı', 'İşlemler']

/**
 * Admin rol yonetimi sayfasi (YALNIZ Admin — A6 matrisi). Rolleri kullanici
 * sayilariyla listeler; yeni rol olusturma + duzenleme dialog'u + onayli silme.
 * Korumali roller (isProtected) duzenlenemez/silinemez (butonlar disabled).
 * Kullanicisi olan rol (userCount > 0) silinemez (sil butonu disabled + uyari).
 */
export default function AdminRolesPage() {
  const { data: roles, isPending, isError, refetch } = useRoles()
  const deleteRole = useDeleteRole()

  // Dialog state'leri: olusturma/duzenleme ortak dialog + silme onayi.
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<Role | null>(null)
  const [toDelete, setToDelete] = useState<Role | null>(null)

  function openCreate() {
    setEditing(null)
    setFormOpen(true)
  }

  function openEdit(role: Role) {
    setEditing(role)
    setFormOpen(true)
  }

  async function handleDelete() {
    if (!toDelete) return
    try {
      await deleteRole.mutateAsync(toDelete.id)
      toast.success('Rol silindi.')
    } catch (error) {
      toast.error('Rol silinemedi.', {
        description: normalizeApiError(error).message,
      })
      throw error
    }
  }

  const header = (
    <PageHeader
      title="Rol Yönetimi"
      description="Sistem rollerini görüntüleyin ve özel roller tanımlayın."
      action={
        <Button onClick={openCreate}>
          <Plus className="h-4 w-4" />
          Yeni Rol
        </Button>
      }
    />
  )

  if (isPending) {
    return (
      <div className="space-y-8">
        {header}
        <TableSkeleton columns={COLUMNS} rows={4} />
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState message="Roller yüklenemedi." onRetry={() => void refetch()} />
      </div>
    )
  }

  return (
    <div className="space-y-8">
      {header}

      {roles.length === 0 ? (
        <EmptyState
          icon={Shield}
          title="Henüz rol yok."
          description="İlk özel rolünüzü oluşturarak başlayın."
          action={
            <Button onClick={openCreate}>
              <Plus className="h-4 w-4" />
              Yeni Rol
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
              {roles.map((role) => (
                <TableRow key={role.id}>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{role.name}</span>
                      {role.isProtected ? (
                        <Badge variant="secondary" className="gap-1">
                          <Shield className="h-3 w-3" />
                          Korumalı
                        </Badge>
                      ) : null}
                    </div>
                  </TableCell>
                  <TableCell className="tabular-nums text-muted-foreground">
                    {role.userCount}
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-1">
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label={`${role.name} rolünü düzenle`}
                        disabled={role.isProtected}
                        onClick={() => openEdit(role)}
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label={`${role.name} rolünü sil`}
                        className="text-destructive hover:text-destructive"
                        // Korumali rol VEYA kullanicisi olan rol silinemez.
                        disabled={role.isProtected || role.userCount > 0}
                        title={
                          role.userCount > 0
                            ? 'Bu role atanmış kullanıcılar var; önce rolü kaldırın.'
                            : undefined
                        }
                        onClick={() => setToDelete(role)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <RoleFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        role={editing}
      />

      <ConfirmDeleteDialog
        open={toDelete !== null}
        onOpenChange={(open) => {
          if (!open) setToDelete(null)
        }}
        title="Rolü sil"
        description={
          toDelete
            ? `"${toDelete.name}" rolü kalıcı olarak silinecek. Bu işlem geri alınamaz.`
            : ''
        }
        onConfirm={handleDelete}
      />
    </div>
  )
}

import { useState } from 'react'
import { Loader2, Plus, X } from 'lucide-react'
import { toast } from 'sonner'

import type { User } from '@/types'
import { useRoles } from '@/features/role'
import { useAssignRole, useRemoveRole } from '@/features/user'
import { normalizeApiError } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

interface UserRolesDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  /** Rolleri yonetilecek kullanici (null ise dialog kapali). */
  user: User | null
}

/**
 * Kullanici rol yonetimi dialog'u (YALNIZ Admin). Mevcut roller chip olarak
 * gosterilir (her birinde "Kaldir"); atanmamis roller dropdown'dan secilip eklenir.
 *
 * Rol listesi GET /api/admin/roles'ten gelir; kullanicida zaten olan roller
 * dropdown'dan filtrelenir. Mutasyonlar idempotent (assign) / korumali (son Admin
 * kaldirma -> 400) backend kurallarini toast ile ele alir. Basarida kullanici
 * listesi invalidate edilir (hook icinde); dialog acik kalir (ardisik islem icin).
 *
 * NOT: `user` prop'u parent listesinden gelir; liste invalidate edilince guncel
 * roller yansir (parent yeniden render eder).
 */
export function UserRolesDialog({
  open,
  onOpenChange,
  user,
}: UserRolesDialogProps) {
  const { data: roles, isPending: rolesLoading } = useRoles()
  const assignRole = useAssignRole()
  const removeRole = useRemoveRole()

  // Dropdown'da secili (henuz eklenmemis) rol adi.
  const [selectedRole, setSelectedRole] = useState('')
  // Hangi rol kaldiriliyor (spinner icin); null ise islem yok.
  const [removing, setRemoving] = useState<string | null>(null)

  if (!user) return null

  const currentRoles = user.roles
  // Kullanicida olmayan roller atanabilir.
  const assignableRoles = (roles ?? []).filter(
    (role) => !currentRoles.includes(role.name),
  )

  async function handleAssign() {
    if (!user || !selectedRole) return
    try {
      await assignRole.mutateAsync({ id: user.id, roleName: selectedRole })
      toast.success(`"${selectedRole}" rolü atandı.`)
      setSelectedRole('')
    } catch (error) {
      toast.error('Rol atanamadı.', {
        description: normalizeApiError(error).message,
      })
    }
  }

  async function handleRemove(roleName: string) {
    if (!user) return
    setRemoving(roleName)
    try {
      await removeRole.mutateAsync({ id: user.id, roleName })
      toast.success(`"${roleName}" rolü kaldırıldı.`)
    } catch (error) {
      // Son Admin korumasi (400) gibi durumlar kullanici dostu mesajla.
      toast.error('Rol kaldırılamadı.', {
        description: normalizeApiError(error).message,
      })
    } finally {
      setRemoving(null)
    }
  }

  const fullName = `${user.firstName} ${user.lastName}`.trim() || user.email

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Rolleri Yönet</DialogTitle>
          <DialogDescription>
            {fullName} ({user.email}) kullanıcısının rollerini düzenleyin.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-5">
          {/* Mevcut roller */}
          <div className="space-y-2">
            <p className="text-sm font-medium">Mevcut Roller</p>
            {currentRoles.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                Bu kullanıcıya henüz rol atanmamış.
              </p>
            ) : (
              <div className="flex flex-wrap gap-2">
                {currentRoles.map((roleName) => (
                  <Badge
                    key={roleName}
                    variant="secondary"
                    className="gap-1 py-1 pl-2.5 pr-1"
                  >
                    {roleName}
                    <button
                      type="button"
                      aria-label={`${roleName} rolünü kaldır`}
                      disabled={removing !== null}
                      onClick={() => void handleRemove(roleName)}
                      className="ml-0.5 rounded-full p-0.5 text-muted-foreground hover:bg-destructive/20 hover:text-destructive focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:opacity-50"
                    >
                      {removing === roleName ? (
                        <Loader2 className="h-3 w-3 animate-spin" />
                      ) : (
                        <X className="h-3 w-3" />
                      )}
                    </button>
                  </Badge>
                ))}
              </div>
            )}
          </div>

          {/* Rol ata */}
          <div className="space-y-2">
            <p className="text-sm font-medium">Rol Ata</p>
            {rolesLoading ? (
              <p className="text-sm text-muted-foreground">
                Roller yükleniyor…
              </p>
            ) : assignableRoles.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                Atanabilecek başka rol yok.
              </p>
            ) : (
              <div className="flex gap-2">
                <Select value={selectedRole} onValueChange={setSelectedRole}>
                  <SelectTrigger className="flex-1" aria-label="Atanacak rol">
                    <SelectValue placeholder="Rol seçin…" />
                  </SelectTrigger>
                  <SelectContent>
                    {assignableRoles.map((role) => (
                      <SelectItem key={role.id} value={role.name}>
                        {role.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Button
                  type="button"
                  onClick={() => void handleAssign()}
                  disabled={!selectedRole || assignRole.isPending}
                >
                  {assignRole.isPending ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Plus className="h-4 w-4" />
                  )}
                  Ata
                </Button>
              </div>
            )}
          </div>
        </div>

        <DialogFooter className="mt-2">
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
          >
            Kapat
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

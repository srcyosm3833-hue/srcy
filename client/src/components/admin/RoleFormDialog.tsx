import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'

import type { Role } from '@/types'
import { useCreateRole, useUpdateRole } from '@/features/role'
import { normalizeApiError } from '@/lib/api'
import { applyFieldErrors } from '@/lib/applyFieldErrors'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'

const NAME_MAX = 100

const roleSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Rol adı zorunludur.')
    .max(NAME_MAX, `En fazla ${NAME_MAX} karakter girebilirsiniz.`),
})

type RoleFormValues = z.infer<typeof roleSchema>

interface RoleFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  /** Verilirse duzenleme modu (mevcut ad ile dolu); verilmezse olusturma. */
  role?: Role | null
}

/**
 * Rol olusturma/duzenleme dialog'u (create + edit ortak). Tek alanli form (name).
 * Basari: toast + dialog kapanir + rol listesi invalidate (hook'lar icinde). 400
 * alan hatasi alana yazilir; 409 (ayni isim) alana + bildirim; 400 (korumali rol)
 * genel toast.
 */
export function RoleFormDialog({
  open,
  onOpenChange,
  role,
}: RoleFormDialogProps) {
  const isEdit = Boolean(role)
  const createRole = useCreateRole()
  const updateRole = useUpdateRole()

  const form = useForm<RoleFormValues>({
    resolver: zodResolver(roleSchema),
    defaultValues: { name: '' },
  })

  // Dialog her acildiginda formu mevcut rolle (veya bos) senkronla.
  useEffect(() => {
    if (open) {
      form.reset({ name: role?.name ?? '' })
    }
  }, [open, role, form])

  async function onSubmit(values: RoleFormValues) {
    const payload = { name: values.name.trim() }
    try {
      if (isEdit && role) {
        await updateRole.mutateAsync({ id: role.id, payload })
        toast.success('Rol güncellendi.')
      } else {
        await createRole.mutateAsync(payload)
        toast.success('Rol oluşturuldu.')
      }
      onOpenChange(false)
    } catch (error) {
      const normalized = normalizeApiError(error)
      const applied = applyFieldErrors(normalized, form.setError, ['name'])
      if (applied) return

      // 409: ayni isimde rol zaten var -> alana yaz.
      if (normalized.status === 409) {
        form.setError('name', {
          type: 'server',
          message: 'Bu isimde bir rol zaten var.',
        })
        return
      }
      // 400 (korumali rol) ve diger alan disi hatalar: genel toast.
      toast.error('Rol kaydedilemedi.', {
        description: normalized.message,
      })
    }
  }

  const { isSubmitting } = form.formState

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Rolü Düzenle' : 'Yeni Rol'}</DialogTitle>
          <DialogDescription>
            {isEdit
              ? 'Rolün adını güncelleyin.'
              : 'Kullanıcılara atayabileceğiniz yeni bir rol ekleyin.'}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate>
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Rol Adı</FormLabel>
                  <FormControl>
                    <Input placeholder="Örn. Editor" autoFocus {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter className="mt-6">
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={isSubmitting}
              >
                İptal
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Kaydediliyor…
                  </>
                ) : (
                  'Kaydet'
                )}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  )
}

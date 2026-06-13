import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'

import type { Category } from '@/types'
import { useCreateCategory, useUpdateCategory } from '@/features/category'
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

const categorySchema = z.object({
  categoryName: z
    .string()
    .trim()
    .min(1, 'Kategori adı zorunludur.')
    .max(NAME_MAX, `En fazla ${NAME_MAX} karakter girebilirsiniz.`),
})

type CategoryFormValues = z.infer<typeof categorySchema>

interface CategoryFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  /** Verilirse duzenleme modu (mevcut ad ile dolu); verilmezse olusturma. */
  category?: Category | null
}

/**
 * Kategori olusturma/duzenleme dialog'u (create + edit ortak). Basit tek alanli
 * form (categoryName). Basari: toast + dialog kapanir + liste invalidate (hook'lar
 * icinde). 400 alan hatasi alana yazilir; 409 (ayni isim) genel toast.
 */
export function CategoryFormDialog({
  open,
  onOpenChange,
  category,
}: CategoryFormDialogProps) {
  const isEdit = Boolean(category)
  const createCategory = useCreateCategory()
  const updateCategory = useUpdateCategory()

  const form = useForm<CategoryFormValues>({
    resolver: zodResolver(categorySchema),
    defaultValues: { categoryName: '' },
  })

  // Dialog her acildiginda formu mevcut kategoriyle (veya bos) senkronla.
  useEffect(() => {
    if (open) {
      form.reset({ categoryName: category?.categoryName ?? '' })
    }
  }, [open, category, form])

  async function onSubmit(values: CategoryFormValues) {
    const payload = { categoryName: values.categoryName.trim() }
    try {
      if (isEdit && category) {
        await updateCategory.mutateAsync({ id: category.id, payload })
        toast.success('Kategori güncellendi.')
      } else {
        await createCategory.mutateAsync(payload)
        toast.success('Kategori oluşturuldu.')
      }
      onOpenChange(false)
    } catch (error) {
      const normalized = normalizeApiError(error)
      const applied = applyFieldErrors(normalized, form.setError, ['categoryName'])
      if (applied) return

      // 409 (ayni isim) gibi alan disi hatalar: hem alana hem toast.
      if (normalized.status === 409) {
        form.setError('categoryName', {
          type: 'server',
          message: 'Bu isimde bir kategori zaten var.',
        })
        return
      }
      toast.error('Kategori kaydedilemedi.', {
        description: normalized.message,
      })
    }
  }

  const { isSubmitting } = form.formState

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {isEdit ? 'Kategoriyi Düzenle' : 'Yeni Kategori'}
          </DialogTitle>
          <DialogDescription>
            {isEdit
              ? 'Kategori adını güncelleyin.'
              : 'Bloglarınızı gruplamak için yeni bir kategori ekleyin.'}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate>
            <FormField
              control={form.control}
              name="categoryName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Kategori Adı</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Örn. Teknoloji"
                      autoFocus
                      {...field}
                    />
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

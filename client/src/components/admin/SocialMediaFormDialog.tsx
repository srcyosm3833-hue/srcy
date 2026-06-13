import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'

import type { SocialMedia } from '@/types'
import {
  useCreateSocialMedia,
  useUpdateSocialMedia,
} from '@/features/contact'
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
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'

const socialMediaSchema = z.object({
  title: z.string().trim().min(1, 'Platform adı zorunludur.'),
  url: z
    .string()
    .trim()
    .min(1, 'URL zorunludur.')
    .url('Geçerli bir URL girin (https://…).'),
  icon: z.string().trim().min(1, 'İkon zorunludur.'),
})

type SocialMediaFormValues = z.infer<typeof socialMediaSchema>

interface SocialMediaFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  /** Verilirse duzenleme modu; verilmezse olusturma. */
  socialMedia?: SocialMedia | null
}

const EMPTY: SocialMediaFormValues = { title: '', url: '', icon: '' }

/**
 * Sosyal medya baglantisi olusturma/duzenleme dialog'u (create + edit ortak).
 * Alanlar: title (platform adi), url, icon (CSS sinifi veya emoji). Basari: toast +
 * dialog kapanir + liste invalidate (hook'larda). 400 alan hatalari alanlara yazilir.
 */
export function SocialMediaFormDialog({
  open,
  onOpenChange,
  socialMedia,
}: SocialMediaFormDialogProps) {
  const isEdit = Boolean(socialMedia)
  const createSocial = useCreateSocialMedia()
  const updateSocial = useUpdateSocialMedia()

  const form = useForm<SocialMediaFormValues>({
    resolver: zodResolver(socialMediaSchema),
    defaultValues: EMPTY,
  })

  useEffect(() => {
    if (open) {
      form.reset(
        socialMedia
          ? {
              title: socialMedia.title,
              url: socialMedia.url,
              icon: socialMedia.icon,
            }
          : EMPTY,
      )
    }
  }, [open, socialMedia, form])

  async function onSubmit(values: SocialMediaFormValues) {
    const payload = {
      title: values.title.trim(),
      url: values.url.trim(),
      icon: values.icon.trim(),
    }
    try {
      if (isEdit && socialMedia) {
        await updateSocial.mutateAsync({ id: socialMedia.id, payload })
        toast.success('Bağlantı güncellendi.')
      } else {
        await createSocial.mutateAsync(payload)
        toast.success('Bağlantı eklendi.')
      }
      onOpenChange(false)
    } catch (error) {
      const normalized = normalizeApiError(error)
      const applied = applyFieldErrors(normalized, form.setError, [
        'title',
        'url',
        'icon',
      ])
      if (applied) return
      toast.error('Bağlantı kaydedilemedi.', {
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
            {isEdit ? 'Bağlantıyı Düzenle' : 'Sosyal Medya Ekle'}
          </DialogTitle>
          <DialogDescription>
            Sitede gösterilecek sosyal medya bağlantısı.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <FormField
              control={form.control}
              name="title"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Platform Adı</FormLabel>
                  <FormControl>
                    <Input placeholder="Instagram" autoFocus {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="url"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>URL</FormLabel>
                  <FormControl>
                    <Input
                      type="url"
                      inputMode="url"
                      placeholder="https://instagram.com/kullanici"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="icon"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>İkon</FormLabel>
                  <FormControl>
                    <Input placeholder="fab fa-instagram veya 📷" {...field} />
                  </FormControl>
                  <FormDescription>
                    CSS ikon sınıfı veya emoji girebilirsiniz.
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
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

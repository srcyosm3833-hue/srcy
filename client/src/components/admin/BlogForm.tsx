import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2 } from 'lucide-react'

import type { Category } from '@/types'
import type { NormalizedApiError } from '@/lib/api'
import { applyFieldErrors } from '@/lib/applyFieldErrors'
import { Button } from '@/components/ui/button'
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
import { Textarea } from '@/components/ui/textarea'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { ImageUploadField } from './ImageUploadField'

const TITLE_MAX = 200

/**
 * Blog form dogrulama semasi (istemci). Backend ayrica NotEmpty + uzunluk + gecerli
 * kategori dogrular. Gorsel alanlari URL string; bos olabilir mi backend belirler —
 * burada zorunlu tutuyoruz (kapak/icerik gorseli olmadan blog anlamsiz).
 */
const blogSchema = z.object({
  title: z
    .string()
    .trim()
    .min(1, 'Başlık zorunludur.')
    .max(TITLE_MAX, `En fazla ${TITLE_MAX} karakter girebilirsiniz.`),
  categoryId: z.string().min(1, 'Kategori seçiniz.'),
  coverImage: z
    .string()
    .trim()
    .min(1, 'Kapak görseli zorunludur (URL girin veya yükleyin).'),
  blogImage: z
    .string()
    .trim()
    .min(1, 'İçerik görseli zorunludur (URL girin veya yükleyin).'),
  description: z.string().trim().min(1, 'İçerik zorunludur.'),
})

export type BlogFormValues = z.infer<typeof blogSchema>

const EMPTY_VALUES: BlogFormValues = {
  title: '',
  categoryId: '',
  coverImage: '',
  blogImage: '',
  description: '',
}

interface BlogFormProps {
  /** Kategori secenekleri. */
  categories: Category[]
  /** Duzenleme icin baslangic degerleri (verilmezse bos = olusturma). */
  initialValues?: BlogFormValues
  /** Kaydet etiketi (orn. "Oluştur" / "Kaydet"). */
  submitLabel: string
  /**
   * Form gonderildiginde cagrilir. Sunucu dogrulama hatasi (400) firlatirsa,
   * dondurulen NormalizedApiError alan hatalari forma baglanir. Diger hatalari
   * cagiran taraf (sayfa) toast ile gosterir, bu yuzden burada yeniden firlatilir.
   */
  onSubmit: (values: BlogFormValues) => Promise<void>
  /** İptal aksiyonu (geri don). */
  onCancel: () => void
}

/**
 * Blog olusturma/duzenleme formu (create + edit ayni komponent). Baslik, kategori
 * (Select), iki gorsel alani (ImageUploadField), ve uzun icerik (Textarea) icerir.
 * Submit'te buton disabled + spinner; sunucu alan hatalari ilgili alana yazilir.
 */
export function BlogForm({
  categories,
  initialValues,
  submitLabel,
  onSubmit,
  onCancel,
}: BlogFormProps) {
  const form = useForm<BlogFormValues>({
    resolver: zodResolver(blogSchema),
    defaultValues: initialValues ?? EMPTY_VALUES,
  })

  const { isSubmitting } = form.formState

  async function handleSubmit(values: BlogFormValues) {
    try {
      await onSubmit(values)
    } catch (error) {
      // onSubmit, normalize edilmis hatayi firlatabilir; alan hatalarini bagla.
      const normalized = error as NormalizedApiError
      if (normalized && typeof normalized === 'object' && 'message' in normalized) {
        const applied = applyFieldErrors(normalized, form.setError, [
          'title',
          'categoryId',
          'coverImage',
          'blogImage',
          'description',
        ])
        if (applied) return
      }
      // Eslesmeyen hata: cagiran taraf toast gosterir (yeniden firlat).
      throw error
    }
  }

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(handleSubmit)}
        className="space-y-6"
        noValidate
      >
        <FormField
          control={form.control}
          name="title"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Başlık</FormLabel>
              <FormControl>
                <Input placeholder="Blog başlığı" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="categoryId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Kategori</FormLabel>
              <Select
                value={field.value || undefined}
                onValueChange={field.onChange}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Kategori seçin" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {categories.map((category) => (
                    <SelectItem key={category.id} value={category.id}>
                      {category.categoryName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="coverImage"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Kapak Görseli</FormLabel>
              <FormControl>
                <ImageUploadField
                  label="Kapak görseli"
                  value={field.value}
                  onChange={field.onChange}
                  disabled={isSubmitting}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="blogImage"
          render={({ field }) => (
            <FormItem>
              <FormLabel>İçerik Görseli</FormLabel>
              <FormControl>
                <ImageUploadField
                  label="İçerik görseli"
                  value={field.value}
                  onChange={field.onChange}
                  disabled={isSubmitting}
                />
              </FormControl>
              <FormDescription>
                Yazının içinde gösterilecek görsel.
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>İçerik</FormLabel>
              <FormControl>
                <Textarea
                  rows={12}
                  placeholder="Blog içeriğini yazın…"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-3">
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
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
              submitLabel
            )}
          </Button>
        </div>
      </form>
    </Form>
  )
}

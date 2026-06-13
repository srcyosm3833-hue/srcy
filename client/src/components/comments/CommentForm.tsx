import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2 } from 'lucide-react'

import { normalizeApiError } from '@/lib/api'
import { Button } from '@/components/ui/button'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Textarea } from '@/components/ui/textarea'

/** Backend metin alani azami uzunlugu (Comment/SubComment TextMaxLength = 1000). */
const TEXT_MAX_LENGTH = 1000

/**
 * Backend alan hatalarindan (PascalCase anahtar) bu formun metin alanina ait ilk
 * mesaji bulur. Anahtar buyuk/kucuk harf duyarsiz eslenir (CommentText/commentText).
 */
function pickFieldMessage(
  fieldErrors: Record<string, string[]> | undefined,
  fieldName: string,
): string | undefined {
  if (!fieldErrors) return undefined
  const target = fieldName.toLowerCase()
  for (const [key, messages] of Object.entries(fieldErrors)) {
    if (key.toLowerCase() === target && messages.length > 0) {
      return messages[0]
    }
  }
  return undefined
}

/**
 * Ortak yorum/yanit metin formu. Hem yeni yorum/yanit eklemede hem mevcut olani
 * duzenlemede kullanilir. Backend alan adi PascalCase (CommentText/SubCommentText)
 * gelebilir; applyFieldErrors eslemesi icin `fieldName` prop'u alir.
 *
 * Tek alanli (text) bir form; rhf + zod ile client-side dogrulama, sunucu hatalari
 * applyFieldErrors ile alana, alana eslenemeyen hatalar toast ile gosterilir
 * (cagiran mutation onError'da). Submit sirasinda buton disabled + spinner.
 */
interface CommentFormProps {
  /** Sunucu alan adi (camelCase): "commentText" veya "subCommentText". */
  fieldName: 'commentText' | 'subCommentText'
  /** Form gonderildiginde cagrilir; cozulurse form resetlenir, reddolursa hata gosterilir. */
  onSubmit: (text: string) => Promise<void>
  /** Duzenleme modunda mevcut metin (varsayilan deger). */
  defaultValue?: string
  placeholder?: string
  /** Gonder butonu etiketi. */
  submitLabel?: string
  /** Iptal butonu (duzenleme/yanit modunda). Verilirse gosterilir. */
  onCancel?: () => void
  /** Daha kompakt gorunum (yanit/duzenleme alanlari icin). */
  compact?: boolean
  /** Metin alani etiketi (gizli degilse). compact modda gizlenir. */
  label?: string
  /** Form alani autoFocus olsun mu (duzenleme/yanit acilisinda). */
  autoFocus?: boolean
}

export function CommentForm({
  fieldName,
  onSubmit,
  defaultValue = '',
  placeholder = 'Yorumunuzu yazın…',
  submitLabel = 'Gönder',
  onCancel,
  compact = false,
  label,
  autoFocus = false,
}: CommentFormProps) {
  // fieldName'e gore dinamik sema (tek alan). Backend de NotEmpty + max 1000 uygular.
  const schema = z.object({
    text: z
      .string()
      .trim()
      .min(1, 'Bu alan boş bırakılamaz.')
      .max(TEXT_MAX_LENGTH, `En fazla ${TEXT_MAX_LENGTH} karakter girebilirsiniz.`),
  })

  type FormValues = z.infer<typeof schema>

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { text: defaultValue },
  })

  const { isSubmitting } = form.formState

  async function handleSubmit(values: FormValues) {
    try {
      await onSubmit(values.text.trim())
      // Basari: ekleme modunda formu temizle (duzenlemede cagiran kapatir).
      form.reset({ text: '' })
    } catch (error) {
      const normalized = normalizeApiError(error)

      // Backend metin alani hatasi (orn. "CommentText"/"SubCommentText", PascalCase)
      // -> tek alanli formda her zaman `text` alaninin altina yaz.
      const serverMessage = pickFieldMessage(normalized.fieldErrors, fieldName)
      if (serverMessage) {
        form.setError('text', { type: 'server', message: serverMessage })
        return
      }

      // Alana eslenemeyen hata (genel): cagiran mutation onError'da toast gosterir.
      throw error
    }
  }

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(handleSubmit)}
        className="space-y-3"
        noValidate
      >
        <FormField
          control={form.control}
          name="text"
          render={({ field }) => (
            <FormItem>
              {label && !compact ? <FormLabel>{label}</FormLabel> : null}
              <FormControl>
                <Textarea
                  placeholder={placeholder}
                  rows={compact ? 3 : 4}
                  autoFocus={autoFocus}
                  aria-label={label ?? placeholder}
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex items-center gap-2">
          <Button
            type="submit"
            size={compact ? 'sm' : 'default'}
            disabled={isSubmitting}
          >
            {isSubmitting ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                Gönderiliyor…
              </>
            ) : (
              submitLabel
            )}
          </Button>
          {onCancel ? (
            <Button
              type="button"
              variant="ghost"
              size={compact ? 'sm' : 'default'}
              onClick={onCancel}
              disabled={isSubmitting}
            >
              İptal
            </Button>
          ) : null}
        </div>
      </form>
    </Form>
  )
}

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { CheckCircle2, Loader2 } from 'lucide-react'
import { toast } from 'sonner'

import { messageApi, normalizeApiError } from '@/lib/api'
import { applyFieldErrors } from '@/lib/applyFieldErrors'
import { Button } from '@/components/ui/button'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Alert, AlertDescription } from '@/components/ui/alert'

// Backend uzunluk sinirlari (Message entity sabitleri ile senkron).
const NAME_MAX = 100
const EMAIL_MAX = 150
const SUBJECT_MAX = 200
const BODY_MAX = 2000

/**
 * Iletisim formu dogrulama semasi (client-side). Backend de NotEmpty + max length +
 * email format uygular; sunucu hatalari applyFieldErrors ile alanlara baglanir.
 */
const contactSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Ad Soyad zorunludur.')
    .max(NAME_MAX, `En fazla ${NAME_MAX} karakter girebilirsiniz.`),
  email: z
    .string()
    .trim()
    .min(1, 'E-posta zorunludur.')
    .email('Geçerli bir e-posta adresi girin.')
    .max(EMAIL_MAX, `En fazla ${EMAIL_MAX} karakter girebilirsiniz.`),
  subject: z
    .string()
    .trim()
    .min(1, 'Konu zorunludur.')
    .max(SUBJECT_MAX, `En fazla ${SUBJECT_MAX} karakter girebilirsiniz.`),
  messageBody: z
    .string()
    .trim()
    .min(1, 'Mesaj zorunludur.')
    .max(BODY_MAX, `En fazla ${BODY_MAX} karakter girebilirsiniz.`),
})

type ContactFormValues = z.infer<typeof contactSchema>

const EMPTY_VALUES: ContactFormValues = {
  name: '',
  email: '',
  subject: '',
  messageBody: '',
}

/**
 * Iletisim (mesaj gonderme) formu. POST /api/messages cagirir. Basarida:
 * inline basari Alert'i + toast gosterir ve formu temizler. Sunucu dogrulama
 * hatalari (400) ilgili alanlarin altina yazilir; diger hatalar toast ile bildirilir.
 * Submit sirasinda buton disabled + spinner.
 */
export function ContactForm() {
  const [submitted, setSubmitted] = useState(false)

  const form = useForm<ContactFormValues>({
    resolver: zodResolver(contactSchema),
    defaultValues: EMPTY_VALUES,
  })

  const { isSubmitting } = form.formState

  async function onSubmit(values: ContactFormValues) {
    setSubmitted(false)
    try {
      await messageApi.send({
        name: values.name.trim(),
        email: values.email.trim(),
        subject: values.subject.trim(),
        messageBody: values.messageBody.trim(),
      })
      setSubmitted(true)
      form.reset(EMPTY_VALUES)
      toast.success('Mesajınız gönderildi.')
    } catch (error) {
      const normalized = normalizeApiError(error)

      // 400: alan bazli dogrulama hatalari -> form alanlarinin altina yaz.
      const applied = applyFieldErrors(normalized, form.setError, [
        'name',
        'email',
        'subject',
        'messageBody',
      ])
      if (applied) return

      toast.error('Mesaj gönderilemedi. Lütfen tekrar deneyin.', {
        description: normalized.message,
      })
    }
  }

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="space-y-4"
        noValidate
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Ad Soyad</FormLabel>
                <FormControl>
                  <Input
                    autoComplete="name"
                    placeholder="Ad Soyad"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>E-posta</FormLabel>
                <FormControl>
                  <Input
                    type="email"
                    autoComplete="email"
                    placeholder="ornek@eposta.com"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="subject"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Konu</FormLabel>
              <FormControl>
                <Input placeholder="Mesajınızın konusu" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="messageBody"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mesaj</FormLabel>
              <FormControl>
                <Textarea
                  rows={6}
                  placeholder="Bize iletmek istediğiniz mesaj…"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {submitted ? (
          <Alert variant="success">
            <CheckCircle2 className="h-4 w-4" />
            <AlertDescription>
              Mesajınız alındı! En kısa sürede yanıt vereceğiz.
            </AlertDescription>
          </Alert>
        ) : null}

        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              Gönderiliyor…
            </>
          ) : (
            'Gönder'
          )}
        </Button>
      </form>
    </Form>
  )
}

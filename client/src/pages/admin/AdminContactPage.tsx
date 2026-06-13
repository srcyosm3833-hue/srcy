import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { AxiosError } from 'axios'
import { Loader2, Save } from 'lucide-react'
import { toast } from 'sonner'

import { useContact, useUpsertContact } from '@/features/contact'
import { normalizeApiError } from '@/lib/api'
import { applyFieldErrors } from '@/lib/applyFieldErrors'
import { PageHeader } from '@/components/common/PageHeader'
import { ErrorState } from '@/components/common/ErrorState'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
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

const contactSchema = z.object({
  address: z.string().trim().min(1, 'Adres zorunludur.'),
  email: z
    .string()
    .trim()
    .min(1, 'E-posta zorunludur.')
    .email('Geçerli bir e-posta girin.'),
  phone: z.string().trim().min(1, 'Telefon zorunludur.'),
  mapUrl: z
    .string()
    .trim()
    .min(1, 'Harita URL’i zorunludur.')
    .url('Geçerli bir URL girin (https://…).'),
})

type ContactFormValues = z.infer<typeof contactSchema>

const EMPTY: ContactFormValues = {
  address: '',
  email: '',
  phone: '',
  mapUrl: '',
}

const KNOWN_FIELDS = ['address', 'email', 'phone', 'mapUrl'] as const

/**
 * Admin iletisim bilgisi yonetimi. Uygulama TEK bir Contact kaydi tutar; sayfa
 * mevcut kaydi bir formda gosterir, Kaydet -> PUT /api/admin/contact (upsert).
 *
 * 404 (kayit henuz yok) bir HATA degildir: bu durumda bos formla "ilk kez
 * olustur" akisi gosterilir (ErrorState gosterilmez). 404 disindaki gercek
 * hatalarda ErrorState + tekrar dene gosterilir.
 *
 * Yetki: PUT yalnizca Admin'e acik; sidebar linki de yalniz Admin'e gosterilir.
 */
export default function AdminContactPage() {
  const { data, isPending, isError, error, refetch } = useContact()

  // 404 = kayit yok (beklenen bos durum). Gercek hata: 404 disindaki yanitlar.
  const isNotFound =
    error instanceof AxiosError && error.response?.status === 404
  const isRealError = isError && !isNotFound
  const isFirstTime = isNotFound

  const upsertContact = useUpsertContact()

  const form = useForm<ContactFormValues>({
    resolver: zodResolver(contactSchema),
    defaultValues: EMPTY,
  })

  // Veri yuklendiginde formu doldur (404 ise bos kalir). form.reset imperative
  // bir cagri oldugu icin set-state-in-effect kuralini ihlal etmez.
  useEffect(() => {
    if (data) {
      form.reset({
        address: data.address,
        email: data.email,
        phone: data.phone,
        mapUrl: data.mapUrl,
      })
    }
  }, [data, form])

  async function onSubmit(values: ContactFormValues) {
    const payload = {
      address: values.address.trim(),
      email: values.email.trim(),
      phone: values.phone.trim(),
      mapUrl: values.mapUrl.trim(),
    }
    try {
      await upsertContact.mutateAsync(payload)
      toast.success(
        isFirstTime
          ? 'İletişim bilgisi oluşturuldu.'
          : 'İletişim bilgisi güncellendi.',
      )
    } catch (err) {
      const normalized = normalizeApiError(err)
      const applied = applyFieldErrors(normalized, form.setError, KNOWN_FIELDS)
      if (applied) return
      toast.error('İletişim bilgisi kaydedilemedi.', {
        description: normalized.message,
      })
    }
  }

  const header = (
    <PageHeader
      title="İletişim Bilgileri"
      description="Sitenin iletişim sayfasında gösterilen bilgileri düzenleyin."
    />
  )

  if (isPending) {
    return (
      <div className="space-y-8">
        {header}
        <Card>
          <CardContent className="space-y-6 p-6">
            {Array.from({ length: 4 }).map((_, index) => (
              <div key={index} className="space-y-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-10 w-full" />
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    )
  }

  if (isRealError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState
          message="İletişim bilgileri yüklenemedi."
          onRetry={() => void refetch()}
        />
      </div>
    )
  }

  const { isSubmitting } = form.formState

  return (
    <div className="space-y-8">
      {header}

      {isFirstTime ? (
        <p className="rounded-md border border-dashed border-border bg-muted/40 px-4 py-3 text-sm text-muted-foreground">
          Henüz iletişim bilgisi tanımlanmamış. Aşağıdaki formu doldurarak ilk
          kaydı oluşturun.
        </p>
      ) : null}

      <Card>
        <CardContent className="p-6">
          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(onSubmit)}
              className="space-y-6"
              noValidate
            >
              <FormField
                control={form.control}
                name="address"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Adres</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Örn. Atatürk Cad. No:1, İstanbul"
                        rows={3}
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
                        inputMode="email"
                        placeholder="iletisim@site.com"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="phone"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Telefon</FormLabel>
                    <FormControl>
                      <Input
                        type="tel"
                        inputMode="tel"
                        placeholder="+90 555 123 45 67"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="mapUrl"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Harita URL’i</FormLabel>
                    <FormControl>
                      <Input
                        type="url"
                        inputMode="url"
                        placeholder="https://maps.google.com/…"
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      Google Maps embed veya konum bağlantısı.
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="flex justify-end">
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Kaydediliyor…
                    </>
                  ) : (
                    <>
                      <Save className="h-4 w-4" />
                      Kaydet
                    </>
                  )}
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  )
}

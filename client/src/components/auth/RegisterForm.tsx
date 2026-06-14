import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'

import { useAuth } from '@/features/auth'
import { normalizeApiError } from '@/lib/api'
import { applyFieldErrors } from '@/lib/applyFieldErrors'
import type { RegisterRequest } from '@/types'
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
import { Checkbox } from '@/components/ui/checkbox'
import { PasswordInput } from '@/components/forms/PasswordInput'
import { paths } from '@/routes/paths'

/**
 * Kayit form semasi. Sifre politikasi backend ile uyumlu: min 8 karakter,
 * en az bir buyuk harf ve bir rakam. imageUrl opsiyonel (bos string gonderilir).
 */
const registerSchema = z.object({
  firstName: z.string().min(1, 'Ad zorunludur.'),
  lastName: z.string().min(1, 'Soyad zorunludur.'),
  email: z
    .string()
    .min(1, 'E-posta zorunludur.')
    .email('Geçerli bir e-posta adresi girin.'),
  password: z
    .string()
    .min(8, 'Şifre en az 8 karakter olmalıdır.')
    .regex(/[A-Z]/, 'Şifre en az bir büyük harf içermelidir.')
    .regex(/[0-9]/, 'Şifre en az bir rakam içermelidir.'),
  // Opsiyonel URL: bos olabilir; doluysa gecerli URL olmali.
  imageUrl: z
    .string()
    .trim()
    .url('Geçerli bir URL girin.')
    .or(z.literal(''))
    .optional(),
  // KVKK aydinlatma metni onayi: yalnizca form-ici alan, backend'e GONDERILMEZ.
  // boolean modeli (default false gecerli olsun) + refine ile zorunlu true:
  // isaretlenmeden submit edilince zodResolver formu bloklar.
  kvkkConsent: z
    .boolean()
    .refine((v) => v === true, {
      message: 'Devam etmek için aydınlatma metnini onaylamalısınız.',
    }),
})

type RegisterFormValues = z.infer<typeof registerSchema>

interface RegisterFormProps {
  /**
   * Basarili kayit sonrasi cagrilir. Backend token DONDURMEZ; otomatik login
   * yapilmaz. Cagiran taraf (sayfa/overlay) sonrasinda login paneline gecirir.
   */
  onSuccess?: () => void
  /** "Giris Yap" gecis linkine basildiginda. Overlay'de panel gecisi yapar. */
  onSwitchToLogin?: () => void
}

/**
 * Paylasilan kayit formu — hem `RegisterPage` (tam sayfa) hem `AuthOverlay`
 * (modal) tarafindan kullanilir. Form mantigi ve AuthProvider cagrisi burada;
 * basari sonrasi davranis `onSuccess` ile cagirana birakilir.
 */
export function RegisterForm({ onSuccess, onSwitchToLogin }: RegisterFormProps) {
  const { register } = useAuth()

  // Genel (alan disi) hata mesaji.
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
      imageUrl: '',
      kvkkConsent: false,
    },
  })

  const { isSubmitting } = form.formState

  async function onSubmit(values: RegisterFormValues) {
    setFormError(null)

    // imageUrl backend'de zorunlu alan; bos olabilir -> bos string gonder.
    const payload: RegisterRequest = {
      firstName: values.firstName,
      lastName: values.lastName,
      email: values.email,
      password: values.password,
      imageUrl: values.imageUrl ?? '',
    }

    try {
      await register(payload)
      // Backend token DONDURMEZ -> otomatik login YAPMA. Login'e yonlendir/gecis + toast.
      toast.success('Hesabınız oluşturuldu.', {
        description: 'Giriş yaparak devam edebilirsiniz.',
      })
      onSuccess?.()
    } catch (error) {
      const normalized = normalizeApiError(error)

      // 409: e-posta zaten kayitli -> email alan hatasi.
      if (normalized.status === 409) {
        form.setError('email', {
          type: 'server',
          message: 'Bu e-posta adresi zaten kullanımda.',
        })
        return
      }

      // 400: alan bazli dogrulama hatalari.
      const applied = applyFieldErrors(normalized, form.setError, [
        'firstName',
        'lastName',
        'email',
        'password',
        'imageUrl',
      ])
      if (applied) return

      // Diger hatalar: genel mesaj + toast.
      setFormError(normalized.message)
      toast.error('Kayıt yapılamadı.', { description: normalized.message })
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
            name="firstName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Ad</FormLabel>
                <FormControl>
                  <Input autoComplete="given-name" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="lastName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Soyad</FormLabel>
                <FormControl>
                  <Input autoComplete="family-name" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

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

        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Şifre</FormLabel>
              <FormControl>
                <PasswordInput
                  autoComplete="new-password"
                  placeholder="••••••••"
                  {...field}
                />
              </FormControl>
              {/* Sifre politikasi ipucu (FormMessage hata varsa onun yerine gosterilir) */}
              <FormDescription>
                En az 8 karakter, bir büyük harf ve bir rakam içermelidir.
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="imageUrl"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Profil Görseli URL (opsiyonel)</FormLabel>
              <FormControl>
                <Input
                  type="url"
                  autoComplete="off"
                  placeholder="https://…"
                  {...field}
                />
              </FormControl>
              <FormDescription>Boş bırakabilirsiniz.</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* KVKK onayi: zorunlu. Link yeni sekmede acilir (modal/overlay icinde de
            kullanildigindan form verisi kaybolmasin). */}
        <FormField
          control={form.control}
          name="kvkkConsent"
          render={({ field }) => (
            <FormItem className="flex flex-row items-start gap-2 space-y-0">
              <FormControl>
                <Checkbox
                  checked={field.value}
                  onCheckedChange={field.onChange}
                  aria-describedby="kvkk-consent-text"
                />
              </FormControl>
              <div className="space-y-1 leading-none">
                <FormLabel
                  id="kvkk-consent-text"
                  className="text-sm font-normal"
                >
                  <a
                    href={paths.privacyPolicy}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="font-medium text-primary underline-offset-4 hover:underline"
                  >
                    Aydınlatma metnini
                  </a>{' '}
                  okudum ve onaylıyorum.
                </FormLabel>
                <FormMessage />
              </div>
            </FormItem>
          )}
        />

        {formError ? (
          <p
            role="alert"
            className="rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm font-medium text-destructive"
          >
            {formError}
          </p>
        ) : null}

        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              Kaydediliyor…
            </>
          ) : (
            'Kayıt Ol'
          )}
        </Button>

        {/* Giris'e gecis: overlay'de onSwitchToLogin verilirse panel gecisi yapar. */}
        {onSwitchToLogin ? (
          <p className="text-center text-sm text-muted-foreground">
            Zaten hesabınız var mı?{' '}
            <button
              type="button"
              onClick={onSwitchToLogin}
              className="font-medium text-primary underline-offset-4 hover:underline"
            >
              Giriş Yap
            </button>
          </p>
        ) : null}
      </form>
    </Form>
  )
}

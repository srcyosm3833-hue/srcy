import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'

import { useAuth } from '@/features/auth'
import { normalizeApiError } from '@/lib/api'
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
import { PasswordInput } from '@/components/forms/PasswordInput'

/** Login form dogrulama semasi (client-side). Backend kurallari submit'te uygulanir. */
const loginSchema = z.object({
  email: z
    .string()
    .min(1, 'E-posta zorunludur.')
    .email('Geçerli bir e-posta adresi girin.'),
  password: z.string().min(1, 'Şifre zorunludur.'),
})

type LoginFormValues = z.infer<typeof loginSchema>

interface LoginFormProps {
  /**
   * Basarili giris sonrasi cagrilir. Sayfa kullaniminda yonlendirme,
   * overlay kullaniminda overlay'i kapatma icin kullanilir.
   */
  onSuccess?: () => void
  /** "Kayit Ol" gecis linkine basildiginda. Overlay'de panel gecisi yapar. */
  onSwitchToRegister?: () => void
}

/**
 * Paylasilan login formu — hem `LoginPage` (tam sayfa) hem `AuthOverlay`
 * (modal) tarafindan kullanilir. Form mantigi, validation ve AuthProvider
 * cagrisi burada; yonlendirme/kapanma kararlari `onSuccess` ile cagirana birakilir.
 */
export function LoginForm({ onSuccess, onSwitchToRegister }: LoginFormProps) {
  const { login } = useAuth()

  // Genel (alan disi) hata mesaji — orn. 401 "gecersiz kimlik".
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  const { isSubmitting } = form.formState

  async function onSubmit(values: LoginFormValues) {
    setFormError(null)
    try {
      await login(values)
      onSuccess?.()
    } catch (error) {
      const normalized = normalizeApiError(error)

      // 401: kimlik dogrulama basarisiz -> genel form hatasi.
      if (normalized.status === 401) {
        setFormError('Geçersiz e-posta veya şifre.')
        return
      }

      // 400: alan bazli dogrulama hatalari -> form alanlarinin altina yaz.
      const applied = applyFieldErrors(normalized, form.setError, [
        'email',
        'password',
      ])
      if (applied) return

      // Diger hatalar: genel mesaj + toast.
      setFormError(normalized.message)
      toast.error('Giriş yapılamadı.', { description: normalized.message })
    }
  }

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="space-y-4"
        noValidate
      >
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
                  autoComplete="current-password"
                  placeholder="••••••••"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Genel form hatasi (alan disi) */}
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
              Giriş yapılıyor…
            </>
          ) : (
            'Giriş Yap'
          )}
        </Button>

        {/* Kayit'a gecis: overlay'de onSwitchToRegister verilirse panel gecisi yapar. */}
        {onSwitchToRegister ? (
          <p className="text-center text-sm text-muted-foreground">
            Hesabınız yok mu?{' '}
            <button
              type="button"
              onClick={onSwitchToRegister}
              className="font-medium text-primary underline-offset-4 hover:underline"
            >
              Kayıt Ol
            </button>
          </p>
        ) : null}
      </form>
    </Form>
  )
}

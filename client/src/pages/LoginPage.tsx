import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'

import { useAuth } from '@/features/auth'
import { paths } from '@/routes/paths'
import { normalizeApiError } from '@/lib/api'
import { applyFieldErrors } from '@/lib/applyFieldErrors'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
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

/** react-router location.state.from sekli (korumali sayfadan yonlendirme). */
interface LocationState {
  from?: { pathname?: string }
}

export default function LoginPage() {
  const { login, isAuthenticated, isInitializing } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  // Genel (alan disi) hata mesaji — orn. 401 "gecersiz kimlik".
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  const { isSubmitting } = form.formState

  // Geri donulecek hedef (korumali sayfadan geldiyse oraya, yoksa anasayfa).
  const fromPath =
    (location.state as LocationState | null)?.from?.pathname ?? paths.home

  // Zaten giris yapilmissa login sayfasini gosterme; hedefe yonlendir.
  useEffect(() => {
    if (!isInitializing && isAuthenticated) {
      navigate(fromPath, { replace: true })
    }
  }, [isInitializing, isAuthenticated, navigate, fromPath])

  async function onSubmit(values: LoginFormValues) {
    setFormError(null)
    try {
      await login(values)
      navigate(fromPath, { replace: true })
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
    <section className="flex min-h-[calc(100vh-4rem)] items-center justify-center bg-muted/40 px-4 py-12">
      <Card className="w-full max-w-md shadow-lg">
        <CardHeader className="space-y-2 text-center">
          <Link
            to={paths.home}
            className="font-heading text-2xl font-bold text-foreground"
          >
            Zn<span className="text-accent">Blog</span>
          </Link>
          <CardTitle className="font-heading text-2xl">Giriş Yap</CardTitle>
          <CardDescription>Hesabınıza devam edin</CardDescription>
        </CardHeader>

        <CardContent>
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
            </form>
          </Form>
        </CardContent>

        <CardFooter className="justify-center">
          <p className="text-sm text-muted-foreground">
            Hesabınız yok mu?{' '}
            <Link
              to={paths.register}
              className="font-medium text-primary underline-offset-4 hover:underline"
            >
              Kayıt Ol
            </Link>
          </p>
        </CardFooter>
      </Card>
    </section>
  )
}

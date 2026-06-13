import { useEffect } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'

import { useAuth } from '@/features/auth'
import { paths } from '@/routes/paths'
import { LoginForm } from '@/components/auth/LoginForm'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

/** react-router location.state.from sekli (korumali sayfadan yonlendirme). */
interface LocationState {
  from?: { pathname?: string }
}

/**
 * Tam sayfa giris ekrani. Deep-link ve ProtectedRoute yonlendirmesi icin korunur
 * (A-AO1). Form mantigi paylasilan `LoginForm` bileseninde; bu sayfa yalnizca
 * yerlesim (Card) + zaten-giris-yapilmis ve basari-sonrasi yonlendirmeyi yonetir.
 */
export default function LoginPage() {
  const { isAuthenticated, isInitializing } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  // Geri donulecek hedef (korumali sayfadan geldiyse oraya, yoksa anasayfa).
  const fromPath =
    (location.state as LocationState | null)?.from?.pathname ?? paths.home

  // Zaten giris yapilmissa login sayfasini gosterme; hedefe yonlendir.
  useEffect(() => {
    if (!isInitializing && isAuthenticated) {
      navigate(fromPath, { replace: true })
    }
  }, [isInitializing, isAuthenticated, navigate, fromPath])

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
          <LoginForm onSuccess={() => navigate(fromPath, { replace: true })} />
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

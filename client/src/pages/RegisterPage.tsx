import { useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'

import { useAuth } from '@/features/auth'
import { paths } from '@/routes/paths'
import { RegisterForm } from '@/components/auth/RegisterForm'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

/**
 * Tam sayfa kayit ekrani. Deep-link icin korunur (A-AO1). Form mantigi paylasilan
 * `RegisterForm` bileseninde; bu sayfa yalnizca yerlesim (Card) + zaten-giris
 * kontrolu ve basari-sonrasi login'e yonlendirmeyi yonetir (backend otomatik
 * login yapmadigindan kayit sonrasi login sayfasina gidilir).
 */
export default function RegisterPage() {
  const { isAuthenticated, isInitializing } = useAuth()
  const navigate = useNavigate()

  // Zaten giris yapilmissa kayit sayfasini gosterme.
  useEffect(() => {
    if (!isInitializing && isAuthenticated) {
      navigate(paths.home, { replace: true })
    }
  }, [isInitializing, isAuthenticated, navigate])

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
          <CardTitle className="font-heading text-2xl">Hesap Oluştur</CardTitle>
          <CardDescription>Topluluğa katılın</CardDescription>
        </CardHeader>

        <CardContent>
          <RegisterForm
            onSuccess={() => navigate(paths.login, { replace: true })}
          />
        </CardContent>

        <CardFooter className="justify-center">
          <p className="text-sm text-muted-foreground">
            Zaten hesabınız var mı?{' '}
            <Link
              to={paths.login}
              className="font-medium text-primary underline-offset-4 hover:underline"
            >
              Giriş Yap
            </Link>
          </p>
        </CardFooter>
      </Card>
    </section>
  )
}

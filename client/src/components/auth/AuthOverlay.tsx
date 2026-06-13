import { Link } from 'react-router-dom'

import { useAuthOverlay } from '@/features/auth/useAuthOverlay'
import { paths } from '@/routes/paths'
import { cn } from '@/lib/utils'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import { LoginForm } from './LoginForm'
import { RegisterForm } from './RegisterForm'
import { SocialLoginSlot } from './SocialLoginSlot'

/**
 * Animasyonlu login/register overlay — sag kenardan iceri kayan (sagdan sola)
 * tam boy drawer (shadcn Sheet, side="right"). SiteHeader butonlari bu overlay'i
 * acar; route sayfalari (LoginPage/RegisterPage) korunur (deep-link + ProtectedRoute).
 *
 * Davranis kararlari:
 *  - A-AO2: yalniz ESC ve X butonu kapatir; arka plana (backdrop) tiklayinca
 *    KAPANMAZ (onPointerDownOutside / onInteractOutside preventDefault). Form
 *    doldururken yanlislikla kapanma riskini onler.
 *  - Acilis/kapanis: Sheet'in yerlesik slide-in-from-right / slide-out-to-right
 *    animasyonu (Radix data-[state], 500ms ac / 300ms kapa).
 *  - login <-> register gecisi: panel ici yatay kayis (300ms). Iki panel de
 *    DOM'da kalir; aktif olmayan aria-hidden + pointer-events-none. Boylece
 *    Radix focus trap dogru calisir ve gecis akici olur.
 *  - prefers-reduced-motion: motion-reduce variant'i ile ic gecis kapatilir.
 *
 * Login basarili -> overlay kapanir (AuthContext guncellenir, header avatar gosterir).
 * Register basarili -> login paneline gecer (backend otomatik login yapmaz).
 */
export function AuthOverlay() {
  const { isOpen, activePanel, setPanel, close } = useAuthOverlay()

  const isLogin = activePanel === 'login'

  return (
    <Sheet open={isOpen} onOpenChange={(open) => (open ? undefined : close())}>
      <SheetContent
        side="right"
        className="flex w-full flex-col gap-0 overflow-y-auto p-0 sm:max-w-md"
        // A-AO2: dis tik / backdrop ile kapatma engellenir; yalniz ESC + X kapatir.
        onPointerDownOutside={(e) => e.preventDefault()}
        onInteractOutside={(e) => e.preventDefault()}
      >
        {/* Dikeyde ortalanan, kisa icerikte ortada duran / uzun icerikte kayan govde */}
        <div className="flex min-h-full flex-col justify-center px-6 py-12 sm:px-10">
          <SheetHeader className="mb-8 space-y-2 text-center sm:text-center">
            <Link
              to={paths.home}
              onClick={close}
              className="mx-auto font-heading text-2xl font-bold text-foreground"
            >
              Zn<span className="text-accent">Blog</span>
            </Link>
            <SheetTitle className="font-heading text-2xl">
              {isLogin ? 'Giriş Yap' : 'Hesap Oluştur'}
            </SheetTitle>
            <SheetDescription>
              {isLogin ? 'Hesabınıza devam edin' : 'Topluluğa katılın'}
            </SheetDescription>
          </SheetHeader>

          {/*
            Yatay kayan iki panel. Distaki kapsayici overflow'u gizler; ictesi
            (track) iki paneli yan yana tutar ve translate-x ile kaydirir.
          */}
          <div className="relative overflow-hidden">
            <div
              className={cn(
                'flex w-full transition-transform duration-300 ease-in-out motion-reduce:transition-none',
                isLogin ? 'translate-x-0' : '-translate-x-1/2',
              )}
              style={{ width: '200%' }}
            >
              {/* Login paneli */}
              <div
                className={cn(
                  'w-1/2 shrink-0 px-0.5',
                  isLogin ? '' : 'pointer-events-none',
                )}
                aria-hidden={!isLogin}
              >
                <LoginForm
                  onSuccess={close}
                  onSwitchToRegister={() => setPanel('register')}
                />
                <div className="mt-6">
                  <SocialLoginSlot />
                </div>
              </div>

              {/* Register paneli */}
              <div
                className={cn(
                  'w-1/2 shrink-0 px-0.5',
                  isLogin ? 'pointer-events-none' : '',
                )}
                aria-hidden={isLogin}
              >
                <RegisterForm
                  onSuccess={() => setPanel('login')}
                  onSwitchToLogin={() => setPanel('login')}
                />
                <div className="mt-6">
                  <SocialLoginSlot />
                </div>
              </div>
            </div>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  )
}

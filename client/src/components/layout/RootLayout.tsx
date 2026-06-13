import { Suspense } from 'react'
import { Outlet } from 'react-router-dom'

import { AuthOverlay } from '@/components/auth/AuthOverlay'
import { SiteHeader } from './SiteHeader'
import { SiteFooter } from './SiteFooter'

/**
 * Public kok yerlesimi: yapiskan SiteHeader, sayfa icerigi (<Outlet />) ve
 * SiteFooter. Lazy yuklenen sayfalar icin <Suspense> fallback'i saglar.
 */
export function RootLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <SiteHeader />

      <main className="flex-1">
        <Suspense
          fallback={
            <div
              className="flex min-h-[40vh] items-center justify-center text-muted-foreground"
              aria-busy="true"
            >
              Yükleniyor…
            </div>
          }
        >
          <Outlet />
        </Suspense>
      </main>

      <SiteFooter />

      {/* Login/register overlay — tum public sayfalardan acilabilir. Router
          baglami icinde render edilir (icindeki <Link>'ler icin gerekli). */}
      <AuthOverlay />
    </div>
  )
}

import type { ReactNode } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/features/auth'
import { paths } from './paths'

interface ProtectedRouteProps {
  /**
   * true ise yalnizca Admin rolundeki kullanici erisebilir. false/undefined ise
   * giris yapmis herhangi bir kullanici yeterlidir.
   */
  requireAdmin?: boolean
  /** Sarmalanan icerik. Verilmezse <Outlet /> (ic route'lar) render edilir. */
  children?: ReactNode
}

/**
 * Korumali route iskeleti. Auth durumuna gore erisimi yonetir:
 *  - isInitializing iken (acilista /api/me cozulurken) basit bir bekleme gosterir.
 *  - Giris yoksa -> /login (donus icin gelinen yol state'te tutulur).
 *  - requireAdmin ve kullanici Admin degilse -> ana sayfaya yonlendirir.
 *
 * Hem wrapper olarak (children) hem layout route olarak (Outlet) kullanilabilir.
 */
export function ProtectedRoute({ requireAdmin, children }: ProtectedRouteProps) {
  const { isAuthenticated, isAdmin, isInitializing } = useAuth()
  const location = useLocation()

  if (isInitializing) {
    return (
      <div className="flex min-h-[40vh] items-center justify-center text-gray-500">
        Yukleniyor...
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to={paths.login} replace state={{ from: location }} />
  }

  if (requireAdmin && !isAdmin) {
    return <Navigate to={paths.home} replace />
  }

  return <>{children ?? <Outlet />}</>
}

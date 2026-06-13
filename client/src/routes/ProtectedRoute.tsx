import type { ReactNode } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/features/auth'
import { paths } from './paths'

interface ProtectedRouteProps {
  /**
   * true ise yalnizca Admin rolundeki kullanici erisebilir. false/undefined ise
   * giris yapmis herhangi bir kullanici yeterlidir.
   * Geriye donuk uyumluluk icin korunur; yeni kodda `requireRole` tercih edilir.
   */
  requireAdmin?: boolean
  /**
   * Erisim icin gerekli rollerden en az birine sahip olmak yeterlidir (herhangi-biri
   * eslemesi). Orn. `['Admin', 'Manager']` -> Admin veya Manager erisir. Bos/undefined ise
   * rol kontrolu yapilmaz (yalniz giris yeterli). `requireAdmin` ile birlikte verilirse
   * her iki kosul da saglanmalidir.
   */
  requireRole?: string[]
  /** Sarmalanan icerik. Verilmezse <Outlet /> (ic route'lar) render edilir. */
  children?: ReactNode
}

/**
 * Korumali route iskeleti. Auth durumuna gore erisimi yonetir:
 *  - isInitializing iken (acilista /api/me cozulurken) basit bir bekleme gosterir.
 *  - Giris yoksa -> /login (donus icin gelinen yol state'te tutulur).
 *  - requireAdmin ve kullanici Admin degilse -> ana sayfaya yonlendirir.
 *  - requireRole verilmis ve kullanici listelenen rollerden hicbirine sahip degilse
 *    -> ana sayfaya yonlendirir.
 *
 * Hem wrapper olarak (children) hem layout route olarak (Outlet) kullanilabilir.
 */
export function ProtectedRoute({
  requireAdmin,
  requireRole,
  children,
}: ProtectedRouteProps) {
  const { isAuthenticated, isAdmin, isInitializing, user } = useAuth()
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

  if (
    requireRole &&
    requireRole.length > 0 &&
    !requireRole.some((role) => user?.roles.includes(role))
  ) {
    return <Navigate to={paths.home} replace />
  }

  return <>{children ?? <Outlet />}</>
}

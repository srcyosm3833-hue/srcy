import { Suspense } from 'react'
import { Outlet, useLocation } from 'react-router-dom'

import { paths } from '@/routes/paths'
import { AdminSidebar } from './AdminSidebar'
import { AdminTopbar } from './AdminTopbar'

/**
 * Gecerli admin yoluna gore ust cubuk basligini cozer. En uzun eslesen prefix
 * kazanir (orn. `/admin/blogs/create` -> "Blog Yönetimi"). Boylece her sayfa
 * basligi tek yerde tutulur ve sayfalar baslik gondermek zorunda kalmaz.
 */
const titleByPath: { prefix: string; title: string }[] = [
  { prefix: paths.adminBlogs, title: 'Blog Yönetimi' },
  { prefix: paths.adminCategories, title: 'Kategori Yönetimi' },
  { prefix: paths.adminMessages, title: 'Mesaj Kutusu' },
  { prefix: paths.adminSocialMedia, title: 'Sosyal Medya' },
  // En genel olan (sadece /admin) en sona: Dashboard.
  { prefix: paths.admin, title: 'Dashboard' },
]

function resolveTitle(pathname: string): string {
  const match = titleByPath.find((entry) => pathname.startsWith(entry.prefix))
  return match?.title ?? 'Yönetim'
}

/**
 * Admin paneli kok yerlesimi. Masaustunde (>= lg) sol sabit sidebar + sag icerik;
 * mobilde sidebar gizli (AdminTopbar'daki hamburger ile Sheet olarak acilir).
 * ProtectedRoute(requireAdmin) tarafindan sarmalanir; yalnizca Admin gorebilir.
 */
export function AdminLayout() {
  const location = useLocation()
  const title = resolveTitle(location.pathname)

  return (
    <div className="min-h-screen bg-muted/30">
      {/* Masaustu sabit sidebar */}
      <aside className="fixed inset-y-0 left-0 z-40 hidden w-64 border-r border-sidebar-border lg:block">
        <AdminSidebar />
      </aside>

      {/* Icerik alani (masaustunde sidebar genisligi kadar bosluk) */}
      <div className="flex min-h-screen flex-col lg:pl-64">
        <AdminTopbar title={title} />

        <main className="flex-1 p-4 sm:p-6">
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
      </div>
    </div>
  )
}

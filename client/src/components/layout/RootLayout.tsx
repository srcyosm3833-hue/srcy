import { Suspense } from 'react'
import { Link, Outlet } from 'react-router-dom'
import { paths } from '@/routes/paths'

/**
 * Uygulamanin kok yerlesimi (iskelet): basit bir ust navigasyon ve sayfa icerigi
 * icin <Outlet />. Lazy yuklenen sayfalar icin <Suspense> fallback'i saglar.
 * Gercek Header/Footer komponentleri sablon donusumunde eklenecek.
 */
export function RootLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <header className="border-b border-gray-200">
        <nav className="mx-auto flex max-w-5xl items-center gap-6 px-4 py-4 text-sm">
          <Link to={paths.home} className="font-semibold text-gray-900">
            ZnBlog
          </Link>
          <Link to={paths.blogs} className="text-gray-600 hover:text-gray-900">
            Blogs
          </Link>
          <Link to={paths.login} className="text-gray-600 hover:text-gray-900">
            Login
          </Link>
          <Link
            to={paths.register}
            className="text-gray-600 hover:text-gray-900"
          >
            Register
          </Link>
          <Link
            to={paths.admin}
            className="ml-auto text-gray-600 hover:text-gray-900"
          >
            Admin
          </Link>
        </nav>
      </header>

      <main className="flex-1">
        <Suspense
          fallback={
            <div className="flex min-h-[40vh] items-center justify-center text-gray-500">
              Yukleniyor...
            </div>
          }
        >
          <Outlet />
        </Suspense>
      </main>
    </div>
  )
}

import { lazy } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { RootLayout } from '@/components/layout/RootLayout'
import { ProtectedRoute } from './ProtectedRoute'
import { paths } from './paths'

/**
 * Data router tanimi (createBrowserRouter). Sayfalar React.lazy ile kod boler;
 * RootLayout icindeki <Suspense> fallback'i yukleme sirasinda gosterilir.
 *
 * Tum route'lar su an placeholder icerik gosterir (API cagrisi yok). Gercek
 * sayfa icerikleri Faz 4'un ilerleyen adimlarinda doldurulacak.
 */

// Lazy sayfalar (default export'lar).
const HomePage = lazy(() => import('@/pages/HomePage'))
const LoginPage = lazy(() => import('@/pages/LoginPage'))
const RegisterPage = lazy(() => import('@/pages/RegisterPage'))
const BlogListPage = lazy(() => import('@/pages/BlogListPage'))
const BlogDetailPage = lazy(() => import('@/pages/BlogDetailPage'))
const AdminDashboardPage = lazy(() => import('@/pages/AdminDashboardPage'))
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage'))

export const router = createBrowserRouter([
  {
    path: paths.home,
    element: <RootLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: paths.login, element: <LoginPage /> },
      { path: paths.register, element: <RegisterPage /> },
      { path: paths.blogs, element: <BlogListPage /> },
      { path: paths.blogDetail(), element: <BlogDetailPage /> },
      {
        // Korumali admin alani: giris + Admin rolu gerektirir.
        element: <ProtectedRoute requireAdmin />,
        children: [{ path: paths.admin, element: <AdminDashboardPage /> }],
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])

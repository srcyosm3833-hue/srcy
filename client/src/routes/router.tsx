import { lazy } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { RootLayout } from '@/components/layout/RootLayout'
import { AdminLayout } from '@/components/layout/admin/AdminLayout'
import { ProtectedRoute } from './ProtectedRoute'
import { paths } from './paths'

/**
 * Data router tanimi (createBrowserRouter). Sayfalar React.lazy ile kod boler;
 * RootLayout / AdminLayout icindeki <Suspense> fallback'i yukleme sirasinda gosterir.
 *
 * Public alan RootLayout altinda; admin alan AdminLayout altinda ve
 * ProtectedRoute(requireRole=['Admin','Manager']) ile korunur (icerik yonetimi
 * yetkisi olan roller erisebilir). Yalniz-Admin islemler (kullanici/rol yonetimi)
 * ileride ilgili route'larda requireRole=['Admin'] ile ayrica daraltilir.
 */

// Lazy public sayfalar.
const HomePage = lazy(() => import('@/pages/HomePage'))
const LoginPage = lazy(() => import('@/pages/LoginPage'))
const RegisterPage = lazy(() => import('@/pages/RegisterPage'))
const BlogListPage = lazy(() => import('@/pages/BlogListPage'))
const BlogDetailPage = lazy(() => import('@/pages/BlogDetailPage'))
const ContactPage = lazy(() => import('@/pages/ContactPage'))
const PrivacyPolicyPage = lazy(() => import('@/pages/PrivacyPolicyPage'))
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage'))

// Lazy admin sayfalari.
const AdminDashboardPage = lazy(() => import('@/pages/AdminDashboardPage'))
const AdminBlogListPage = lazy(() => import('@/pages/admin/AdminBlogListPage'))
const AdminBlogFormPage = lazy(() => import('@/pages/admin/AdminBlogFormPage'))
const AdminCategoriesPage = lazy(
  () => import('@/pages/admin/AdminCategoriesPage'),
)
const AdminCommentsPage = lazy(() => import('@/pages/admin/AdminCommentsPage'))
const AdminMessagesPage = lazy(() => import('@/pages/admin/AdminMessagesPage'))
const AdminContactPage = lazy(() => import('@/pages/admin/AdminContactPage'))
const AdminSocialMediaPage = lazy(
  () => import('@/pages/admin/AdminSocialMediaPage'),
)
const AdminUsersPage = lazy(() => import('@/pages/admin/AdminUsersPage'))
const AdminRolesPage = lazy(() => import('@/pages/admin/AdminRolesPage'))
const AdminSearchLogsPage = lazy(
  () => import('@/pages/admin/AdminSearchLogsPage'),
)

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
      { path: paths.contact, element: <ContactPage /> },
      { path: paths.privacyPolicy, element: <PrivacyPolicyPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
  {
    // Korumali admin alani: giris + Admin veya Manager rolu gerektirir. Kendi layout'u var.
    element: <ProtectedRoute requireRole={['Admin', 'Manager']} />,
    children: [
      {
        path: paths.admin,
        element: <AdminLayout />,
        children: [
          { index: true, element: <AdminDashboardPage /> },
          { path: paths.adminBlogs, element: <AdminBlogListPage /> },
          { path: paths.adminBlogCreate, element: <AdminBlogFormPage /> },
          { path: paths.adminBlogEdit(), element: <AdminBlogFormPage /> },
          { path: paths.adminCategories, element: <AdminCategoriesPage /> },
          { path: paths.adminComments, element: <AdminCommentsPage /> },
          { path: paths.adminMessages, element: <AdminMessagesPage /> },
          { path: paths.adminContact, element: <AdminContactPage /> },
          { path: paths.adminSocialMedia, element: <AdminSocialMediaPage /> },
          // Kullanici listeleme Admin + Manager'a acik (parent guard yeterli).
          { path: paths.adminUsers, element: <AdminUsersPage /> },
          // Yalniz-Admin alanlar: ek bir ProtectedRoute(requireRole=['Admin'])
          // ile daraltilir. Manager deep-link ile gelse bile ana sayfaya yonlenir.
          {
            element: <ProtectedRoute requireRole={['Admin']} />,
            children: [
              { path: paths.adminRoles, element: <AdminRolesPage /> },
              {
                path: paths.adminSearchLogs,
                element: <AdminSearchLogsPage />,
              },
            ],
          },
        ],
      },
    ],
  },
])

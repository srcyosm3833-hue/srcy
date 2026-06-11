import { useAuth } from '@/features/auth'

/**
 * Korumali admin paneli placeholder'i (iskelet). ProtectedRoute tarafindan
 * sarmalanir; yalnizca giris yapmis + Admin rolundeki kullanici gorebilir.
 */
export default function AdminDashboardPage() {
  const { user } = useAuth()

  return (
    <section className="mx-auto max-w-3xl px-4 py-16">
      <h1 className="text-2xl font-semibold text-gray-900">Admin</h1>
      <p className="mt-2 text-gray-600">
        Korumali admin paneli placeholder'i. Giris yapan: {user?.email ?? '—'}
      </p>
    </section>
  )
}

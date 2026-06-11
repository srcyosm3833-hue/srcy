import { Link } from 'react-router-dom'
import { paths } from '@/routes/paths'

/**
 * 404 placeholder'i. Router'da eslesmeyen tum yollar buraya duser.
 */
export default function NotFoundPage() {
  return (
    <section className="mx-auto max-w-md px-4 py-16 text-center">
      <h1 className="text-3xl font-bold text-gray-900">404</h1>
      <p className="mt-2 text-gray-600">Sayfa bulunamadi.</p>
      <Link
        to={paths.home}
        className="mt-4 inline-block text-violet-600 underline"
      >
        Ana sayfaya don
      </Link>
    </section>
  )
}

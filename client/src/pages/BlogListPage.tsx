import { Link } from 'react-router-dom'
import { paths } from '@/routes/paths'

/**
 * Blog listesi sayfasi placeholder'i (iskelet). Gercek liste + API baglantisi
 * Faz 4'te eklenecek. Detay route'una ornek bir link icerir.
 */
export default function BlogListPage() {
  return (
    <section className="mx-auto max-w-3xl px-4 py-16">
      <h1 className="text-2xl font-semibold text-gray-900">Blogs</h1>
      <p className="mt-2 text-gray-600">Blog listesi placeholder'i.</p>
      <Link
        to={paths.blogDetail('ornek-id')}
        className="mt-4 inline-block text-violet-600 underline"
      >
        Ornek blog detayina git
      </Link>
    </section>
  )
}

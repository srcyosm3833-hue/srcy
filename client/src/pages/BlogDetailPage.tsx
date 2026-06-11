import { useParams } from 'react-router-dom'

/**
 * Blog detay sayfasi placeholder'i (iskelet). URL parametresini (:id) tip guvenli
 * okur ve gosterir. Gercek detay + API baglantisi Faz 4'te eklenecek.
 */
export default function BlogDetailPage() {
  const { id } = useParams<{ id: string }>()

  return (
    <section className="mx-auto max-w-3xl px-4 py-16">
      <h1 className="text-2xl font-semibold text-gray-900">Blog Detail</h1>
      <p className="mt-2 text-gray-600">
        Detay placeholder'i. Route param id:{' '}
        <code className="rounded bg-gray-100 px-1.5 py-0.5">{id}</code>
      </p>
    </section>
  )
}

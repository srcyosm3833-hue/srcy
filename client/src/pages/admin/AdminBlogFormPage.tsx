import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { toast } from 'sonner'

import { useBlogDetail, useCreateBlog, useUpdateBlog } from '@/features/blog'
import { useCategories } from '@/features/category'
import { normalizeApiError } from '@/lib/api'
import { paths } from '@/routes/paths'
import { BlogForm, type BlogFormValues } from '@/components/admin/BlogForm'
import { ErrorState } from '@/components/common/ErrorState'
import { EmptyState } from '@/components/common/EmptyState'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'

/**
 * Blog olusturma VE duzenleme sayfasi (tek komponent). Route param `:id` varsa
 * duzenleme modudur: blog detayi cekilip forma doldurulur. Kategori listesi her
 * iki modda gereklidir. Basari: toast + /admin/blogs'a yonlendirme.
 */
export default function AdminBlogFormPage() {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const isEdit = Boolean(id)

  const categories = useCategories()
  const blog = useBlogDetail(id) // id yoksa enabled=false (calismaz)
  const createBlog = useCreateBlog()
  const updateBlog = useUpdateBlog()

  function goBack() {
    navigate(paths.adminBlogs)
  }

  /**
   * Form gonderimi. Hata durumunda NormalizedApiError firlatir; BlogForm 400 alan
   * hatalarini forma baglar, eslesmeyenler buraya geri gelir (toast).
   */
  async function handleSubmit(values: BlogFormValues) {
    const payload = {
      title: values.title.trim(),
      description: values.description.trim(),
      coverImage: values.coverImage.trim(),
      blogImage: values.blogImage.trim(),
      categoryId: values.categoryId,
    }

    try {
      if (isEdit && id) {
        await updateBlog.mutateAsync({ id, payload })
        toast.success('Blog güncellendi.')
      } else {
        await createBlog.mutateAsync(payload)
        toast.success('Blog oluşturuldu.')
      }
      navigate(paths.adminBlogs)
    } catch (error) {
      // BlogForm bunu alir; alan hatasi yoksa yeniden firlatir -> burada toast.
      const normalized = normalizeApiError(error)
      if (normalized.fieldErrors) {
        throw normalized
      }
      toast.error('Blog kaydedilemedi.', { description: normalized.message })
      throw normalized
    }
  }

  const header = (
    <div className="flex items-center gap-4">
      <Button variant="ghost" size="icon" asChild aria-label="Geri">
        <Link to={paths.adminBlogs}>
          <ArrowLeft className="h-4 w-4" />
        </Link>
      </Button>
      <h1 className="font-heading text-2xl font-bold tracking-tight">
        {isEdit ? 'Blogu Düzenle' : 'Yeni Blog'}
      </h1>
    </div>
  )

  // Kategori yuklenemezse form anlamsiz; once kategori durumunu ele al.
  if (categories.isError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState
          message="Kategoriler yüklenemedi. Blog formu açılamıyor."
          onRetry={() => void categories.refetch()}
        />
      </div>
    )
  }

  // Duzenleme modunda blog detayi henuz gelmediyse iskelet goster.
  const isLoading =
    categories.isPending || (isEdit && blog.isPending)

  if (isLoading) {
    return (
      <div className="space-y-8">
        {header}
        <Card className="mx-auto max-w-3xl">
          <CardContent className="space-y-6 pt-6">
            {Array.from({ length: 5 }).map((_, index) => (
              <div key={index} className="space-y-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-10 w-full" />
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    )
  }

  // Duzenleme modunda blog bulunamadiysa (404 vb.).
  if (isEdit && blog.isError) {
    const status = normalizeApiError(blog.error).status
    return (
      <div className="space-y-8">
        {header}
        {status === 404 ? (
          <EmptyState
            title="Blog bulunamadı."
            description="Düzenlemek istediğiniz yazı silinmiş olabilir."
            action={
              <Button asChild variant="outline">
                <Link to={paths.adminBlogs}>Listeye dön</Link>
              </Button>
            }
          />
        ) : (
          <ErrorState
            message="Blog yüklenemedi."
            onRetry={() => void blog.refetch()}
          />
        )}
      </div>
    )
  }

  const initialValues: BlogFormValues | undefined =
    isEdit && blog.data
      ? {
          title: blog.data.title,
          categoryId: blog.data.categoryId,
          coverImage: blog.data.coverImage,
          blogImage: blog.data.blogImage,
          description: blog.data.description,
        }
      : undefined

  return (
    <div className="space-y-8">
      {header}
      <Card className="mx-auto max-w-3xl">
        <CardContent className="pt-6">
          <BlogForm
            categories={categories.data ?? []}
            initialValues={initialValues}
            submitLabel={isEdit ? 'Kaydet' : 'Oluştur'}
            onSubmit={handleSubmit}
            onCancel={goBack}
          />
        </CardContent>
      </Card>
    </div>
  )
}

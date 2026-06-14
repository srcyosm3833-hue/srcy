import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import {
  Eye,
  FileText,
  MoreHorizontal,
  Pencil,
  Plus,
  ShieldQuestion,
  Trash2,
} from 'lucide-react'
import { toast } from 'sonner'

import type { BlogListItem } from '@/types'
import { useBlogList, useDeleteBlog } from '@/features/blog'
import { normalizeApiError } from '@/lib/api'
import { formatDate } from '@/lib/formatDate'
import { resolveAssetUrl } from '@/lib/resolveAssetUrl'
import { paths } from '@/routes/paths'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { PaginationBar } from '@/components/common/PaginationBar'
import { TableSkeleton } from '@/components/admin/TableSkeleton'
import { ConfirmDeleteDialog } from '@/components/admin/ConfirmDeleteDialog'
import { BlogAuditDialog } from '@/components/admin/BlogAuditDialog'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'

const PAGE_SIZE = 20
const COLUMNS = ['Başlık', 'Kategori', 'Yazar', 'Tarih', 'İşlemler']

/**
 * Admin blog liste sayfasi. Sayfali tablo + satir aksiyonlari (goruntule/duzenle/sil).
 * Liste verisi public GET /api/blogs'tan gelir (ayri admin blog ucu yok). Sayfa
 * URL'de (?page=) tutulur. Silme onayli ve toast + invalidation ile.
 */
export default function AdminBlogListPage() {
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = Math.max(1, Number(searchParams.get('page')) || 1)

  const { data, isPending, isError, refetch, isFetching } = useBlogList({
    page,
    pageSize: PAGE_SIZE,
  })
  const deleteBlog = useDeleteBlog()

  // Silinmek uzere secilen blog (ConfirmDeleteDialog kontrollu open state'i).
  const [toDelete, setToDelete] = useState<BlogListItem | null>(null)
  // Audit detayi acilan blog id'si (BlogAuditDialog kontrollu open state'i).
  const [auditId, setAuditId] = useState<string | null>(null)

  function goToPage(next: number) {
    setSearchParams((prev) => {
      const params = new URLSearchParams(prev)
      params.set('page', String(next))
      return params
    })
  }

  async function handleDelete() {
    if (!toDelete) return
    try {
      await deleteBlog.mutateAsync(toDelete.id)
      toast.success('Blog silindi.')
    } catch (error) {
      toast.error('Blog silinemedi.', {
        description: normalizeApiError(error).message,
      })
      throw error
    }
  }

  const header = (
    <PageHeader
      title="Blog Yönetimi"
      description="Tüm blog yazılarını yönetin."
      action={
        <Button asChild>
          <Link to={paths.adminBlogCreate}>
            <Plus className="h-4 w-4" />
            Yeni Blog
          </Link>
        </Button>
      }
    />
  )

  if (isPending) {
    return (
      <div className="space-y-8">
        {header}
        <TableSkeleton columns={COLUMNS} rows={6} />
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState message="Bloglar yüklenemedi." onRetry={() => void refetch()} />
      </div>
    )
  }

  const blogs = data.items

  return (
    <div className="space-y-8">
      {header}

      {blogs.length === 0 ? (
        <EmptyState
          icon={FileText}
          title="Henüz blog eklenmemiş."
          description="İlk blog yazınızı oluşturarak başlayın."
          action={
            <Button asChild>
              <Link to={paths.adminBlogCreate}>
                <Plus className="h-4 w-4" />
                Yeni Blog
              </Link>
            </Button>
          }
        />
      ) : (
        <>
          <div className="rounded-lg border border-border bg-card">
            <Table>
              <TableHeader>
                <TableRow>
                  {COLUMNS.map((col) => (
                    <TableHead
                      key={col}
                      className={col === 'İşlemler' ? 'text-right' : undefined}
                    >
                      {col}
                    </TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {blogs.map((blog) => (
                  <TableRow key={blog.id}>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        {blog.coverImage ? (
                          <img
                            src={resolveAssetUrl(blog.coverImage)}
                            alt=""
                            className="h-10 w-10 shrink-0 rounded object-cover"
                            loading="lazy"
                          />
                        ) : (
                          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded bg-muted text-muted-foreground">
                            <FileText className="h-4 w-4" />
                          </div>
                        )}
                        <span className="line-clamp-2 max-w-xs font-medium">
                          {blog.title}
                        </span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">{blog.categoryName}</Badge>
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {blog.authorName}
                    </TableCell>
                    <TableCell className="whitespace-nowrap text-muted-foreground">
                      {formatDate(blog.createdAt)}
                    </TableCell>
                    <TableCell className="text-right">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon"
                            aria-label="İşlemler"
                          >
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem asChild>
                            <a
                              href={paths.blogDetail(blog.id)}
                              target="_blank"
                              rel="noopener noreferrer"
                            >
                              <Eye className="h-4 w-4" />
                              Görüntüle
                            </a>
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onSelect={() =>
                              navigate(paths.adminBlogEdit(blog.id))
                            }
                          >
                            <Pencil className="h-4 w-4" />
                            Düzenle
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onSelect={() => setAuditId(blog.id)}
                          >
                            <ShieldQuestion className="h-4 w-4" />
                            Denetim Detayı
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            className="text-destructive focus:text-destructive"
                            onSelect={() => setToDelete(blog)}
                          >
                            <Trash2 className="h-4 w-4" />
                            Sil
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          <PaginationBar
            page={data.page}
            totalPages={data.totalPages}
            hasPreviousPage={data.hasPreviousPage}
            hasNextPage={data.hasNextPage}
            onPageChange={goToPage}
            disabled={isFetching}
          />
        </>
      )}

      <ConfirmDeleteDialog
        open={toDelete !== null}
        onOpenChange={(open) => {
          if (!open) setToDelete(null)
        }}
        title="Blogu sil"
        description={
          toDelete
            ? `"${toDelete.title}" başlıklı blog kalıcı olarak silinecek. Bu işlem geri alınamaz.`
            : ''
        }
        onConfirm={handleDelete}
      />

      <BlogAuditDialog
        open={auditId !== null}
        onOpenChange={(open) => {
          if (!open) setAuditId(null)
        }}
        blogId={auditId}
      />
    </div>
  )
}

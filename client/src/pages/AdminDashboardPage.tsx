import type { LucideIcon } from 'lucide-react'
import { FileText, Mail, Share2, Tag } from 'lucide-react'

import { useBlogList } from '@/features/blog'
import { useCategories } from '@/features/category'
import { useSocialMedia } from '@/features/contact'
import { useUnreadMessageCount } from '@/features/message'
import { PageHeader } from '@/components/common/PageHeader'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

interface StatCardProps {
  title: string
  icon: LucideIcon
  value: number | undefined
  isLoading: boolean
  isError: boolean
  description: string
  /** Deger vurgusu (orn. okunmamis mesaj amber). */
  emphasis?: boolean
}

/**
 * Tek bir istatistik karti. Bagimsiz query'e baglanir: yuklenirken iskelet,
 * hata varsa "—" gosterir (tum sayfayi dusurmez).
 */
function StatCard({
  title,
  icon: Icon,
  value,
  isLoading,
  isError,
  description,
  emphasis,
}: StatCardProps) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        <Icon className="h-4 w-4 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <Skeleton className="h-8 w-16" />
        ) : (
          <div
            className={cn(
              'text-3xl font-bold',
              emphasis && value ? 'text-accent' : 'text-foreground',
            )}
          >
            {isError ? '—' : (value ?? 0)}
          </div>
        )}
        <p className="mt-1 text-xs text-muted-foreground">{description}</p>
      </CardContent>
    </Card>
  )
}

/**
 * Admin Dashboard. Ayri bir /api/admin/stats ucu olmadigindan ozet kartlari mevcut
 * liste uclarinin totalCount/length degerlerinden TUREVDIR:
 *  - Toplam blog:    GET /api/blogs (pageSize=1) -> totalCount
 *  - Kategoriler:    GET /api/categories -> length
 *  - Okunmamis:      GET /api/admin/messages -> isRead=false sayisi
 *  - Sosyal medya:   GET /api/social-media -> length
 * Her kart bagimsiz; biri hata verirse yalnizca o kart "—" gosterir.
 */
export default function AdminDashboardPage() {
  // pageSize=1: yalnizca toplam sayiyi almak icin minimum veri ceker.
  const blogs = useBlogList({ page: 1, pageSize: 1 })
  const categories = useCategories()
  const social = useSocialMedia()
  const messages = useUnreadMessageCount()

  return (
    <div className="space-y-8">
      <PageHeader
        title="Dashboard"
        description="Site içeriğinizin hızlı bir özeti."
      />

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Toplam Blog"
          icon={FileText}
          value={blogs.data?.totalCount}
          isLoading={blogs.isPending}
          isError={blogs.isError}
          description="Yayımlanan yazı sayısı"
        />
        <StatCard
          title="Kategoriler"
          icon={Tag}
          value={categories.data?.length}
          isLoading={categories.isPending}
          isError={categories.isError}
          description="Tanımlı kategori sayısı"
        />
        <StatCard
          title="Okunmamış Mesaj"
          icon={Mail}
          value={messages.unreadCount}
          isLoading={messages.isPending}
          isError={messages.isError}
          description="Bekleyen iletişim mesajı"
          emphasis
        />
        <StatCard
          title="Sosyal Medya"
          icon={Share2}
          value={social.data?.length}
          isLoading={social.isPending}
          isError={social.isError}
          description="Bağlı sosyal hesap"
        />
      </div>
    </div>
  )
}

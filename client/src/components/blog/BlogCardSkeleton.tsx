import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'

/**
 * BlogCard'in yukleme (loading) yer tutucusu. BlogCard ile ayni iskeleti
 * taklit eder ki yukleme sonrasi layout kaymasin.
 */
export function BlogCardSkeleton() {
  return (
    <Card className="flex h-full flex-col overflow-hidden" aria-hidden>
      <Skeleton className="aspect-video w-full rounded-none" />
      <CardContent className="flex flex-1 flex-col p-4">
        <Skeleton className="h-5 w-20 rounded-md" />
        <Skeleton className="mt-3 h-6 w-full" />
        <Skeleton className="mt-2 h-6 w-3/4" />
        <Skeleton className="mt-3 h-4 w-40" />
        <Skeleton className="mt-5 h-4 w-28" />
      </CardContent>
    </Card>
  )
}

/** Belirtilen sayida iskelet kart uretir (grid icinde kullanim icin). */
export function BlogCardSkeletonGrid({ count = 6 }: { count?: number }) {
  return (
    <>
      {Array.from({ length: count }).map((_, index) => (
        <BlogCardSkeleton key={index} />
      ))}
    </>
  )
}

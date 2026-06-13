import { Skeleton } from '@/components/ui/skeleton'

/** Blog detay sayfasinin tam-sayfa yukleme yer tutucusu. */
export function BlogDetailSkeleton() {
  return (
    <div aria-hidden>
      {/* Baslik alani */}
      <Skeleton className="h-5 w-24 rounded-md" />
      <Skeleton className="mt-4 h-10 w-full" />
      <Skeleton className="mt-2 h-10 w-2/3" />
      <Skeleton className="mt-4 h-4 w-48" />

      {/* Kapak gorseli */}
      <Skeleton className="mt-8 aspect-video w-full rounded-xl" />

      {/* Icerik satirlari */}
      <div className="mt-8 space-y-3">
        {Array.from({ length: 8 }).map((_, index) => (
          <Skeleton
            key={index}
            className="h-4"
            style={{ width: `${85 + ((index * 7) % 15)}%` }}
          />
        ))}
      </div>
    </div>
  )
}

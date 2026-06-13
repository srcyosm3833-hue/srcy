import { Skeleton } from '@/components/ui/skeleton'

/** CommentItem yukleme yer tutucusu. */
export function CommentItemSkeleton() {
  return (
    <div className="flex gap-3" aria-hidden>
      <Skeleton className="h-9 w-9 shrink-0 rounded-full" />
      <div className="flex-1">
        <Skeleton className="h-4 w-40" />
        <Skeleton className="mt-2 h-4 w-full" />
        <Skeleton className="mt-1.5 h-4 w-2/3" />
      </div>
    </div>
  )
}

/** Belirtilen sayida yorum iskeleti uretir. */
export function CommentListSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-6">
      {Array.from({ length: count }).map((_, index) => (
        <CommentItemSkeleton key={index} />
      ))}
    </div>
  )
}

import { cn } from '@/lib/utils'

/** shadcn/ui Skeleton (new-york). Loading state'lerinde yer tutucu (pulse). */
function Skeleton({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('animate-pulse rounded-md bg-primary/10', className)}
      {...props}
    />
  )
}

export { Skeleton }

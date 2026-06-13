import * as React from 'react'
import { ChevronLeft, ChevronRight, MoreHorizontal } from 'lucide-react'

import { cn } from '@/lib/utils'
import { type ButtonProps, buttonVariants } from '@/components/ui/button'

/**
 * shadcn/ui Pagination (new-york). Yapisal (presentational) bilesenler;
 * sayfa hesaplama mantigi cagiran tarafta tutulur. Linkler <button> tabanli
 * (SPA'da onClick ile state degisir; ayri sayfaya gitmez).
 */
function Pagination({ className, ...props }: React.ComponentProps<'nav'>) {
  return (
    <nav
      role="navigation"
      aria-label="Sayfalama"
      className={cn('mx-auto flex w-full justify-center', className)}
      {...props}
    />
  )
}

const PaginationContent = React.forwardRef<
  HTMLUListElement,
  React.ComponentProps<'ul'>
>(({ className, ...props }, ref) => (
  <ul
    ref={ref}
    className={cn('flex flex-row items-center gap-1', className)}
    {...props}
  />
))
PaginationContent.displayName = 'PaginationContent'

const PaginationItem = React.forwardRef<
  HTMLLIElement,
  React.ComponentProps<'li'>
>(({ className, ...props }, ref) => (
  <li ref={ref} className={cn('', className)} {...props} />
))
PaginationItem.displayName = 'PaginationItem'

type PaginationButtonProps = {
  isActive?: boolean
} & Pick<ButtonProps, 'size'> &
  React.ComponentProps<'button'>

/** Tek bir sayfa numarasi/aksiyon dugmesi. Aktif sayfa outline ile vurgulanir. */
function PaginationButton({
  className,
  isActive,
  size = 'icon',
  ...props
}: PaginationButtonProps) {
  return (
    <button
      type="button"
      aria-current={isActive ? 'page' : undefined}
      className={cn(
        buttonVariants({
          variant: isActive ? 'outline' : 'ghost',
          size,
        }),
        className,
      )}
      {...props}
    />
  )
}

function PaginationPrevious({
  className,
  ...props
}: React.ComponentProps<'button'>) {
  return (
    <PaginationButton
      aria-label="Onceki sayfaya git"
      size="default"
      className={cn('gap-1 pl-2.5', className)}
      {...props}
    >
      <ChevronLeft className="h-4 w-4" />
      <span>Önceki</span>
    </PaginationButton>
  )
}

function PaginationNext({
  className,
  ...props
}: React.ComponentProps<'button'>) {
  return (
    <PaginationButton
      aria-label="Sonraki sayfaya git"
      size="default"
      className={cn('gap-1 pr-2.5', className)}
      {...props}
    >
      <span>Sonraki</span>
      <ChevronRight className="h-4 w-4" />
    </PaginationButton>
  )
}

function PaginationEllipsis({
  className,
  ...props
}: React.ComponentProps<'span'>) {
  return (
    <span
      aria-hidden
      className={cn('flex h-9 w-9 items-center justify-center', className)}
      {...props}
    >
      <MoreHorizontal className="h-4 w-4" />
      <span className="sr-only">Daha fazla sayfa</span>
    </span>
  )
}

export {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationButton,
  PaginationPrevious,
  PaginationNext,
  PaginationEllipsis,
}

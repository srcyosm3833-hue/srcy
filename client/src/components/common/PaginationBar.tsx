import { ELLIPSIS, getPaginationRange } from '@/lib/paginationRange'
import {
  Pagination,
  PaginationButton,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination'

interface PaginationBarProps {
  page: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  onPageChange: (page: number) => void
  /** Veri yenilenirken butonlari devre disi birakmak icin. */
  disabled?: boolean
}

/**
 * SPA sayfalama cubugu. Sayfa numaralarini getPaginationRange ile uretir;
 * tiklamada onPageChange(page) cagrilir (URL yonetimi cagiran tarafta).
 * Tek sayfa varsa hic render etmez.
 */
export function PaginationBar({
  page,
  totalPages,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
  disabled,
}: PaginationBarProps) {
  if (totalPages <= 1) return null

  const items = getPaginationRange(page, totalPages)

  return (
    <Pagination>
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious
            onClick={() => onPageChange(page - 1)}
            disabled={disabled || !hasPreviousPage}
          />
        </PaginationItem>

        {items.map((item, index) =>
          item === ELLIPSIS ? (
            <PaginationItem key={`ellipsis-${index}`}>
              <PaginationEllipsis />
            </PaginationItem>
          ) : (
            <PaginationItem key={item}>
              <PaginationButton
                isActive={item === page}
                disabled={disabled}
                onClick={() => onPageChange(item)}
              >
                {item}
              </PaginationButton>
            </PaginationItem>
          ),
        )}

        <PaginationItem>
          <PaginationNext
            onClick={() => onPageChange(page + 1)}
            disabled={disabled || !hasNextPage}
          />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  )
}

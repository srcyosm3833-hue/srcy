import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

interface TableSkeletonProps {
  /** Baslik etiketleri (sutun sayisini da belirler). */
  columns: string[]
  /** Iskelet satir sayisi. */
  rows?: number
}

/**
 * Admin liste tablolari icin yukleme iskeleti. Gercek tabloyla ayni sutun
 * yapisini taklit eder (layout kaymasini onler).
 */
export function TableSkeleton({ columns, rows = 5 }: TableSkeletonProps) {
  return (
    <div className="rounded-lg border border-border bg-card">
      <Table>
        <TableHeader>
          <TableRow>
            {columns.map((col) => (
              <TableHead key={col}>{col}</TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: rows }).map((_, rowIndex) => (
            <TableRow key={rowIndex}>
              {columns.map((col) => (
                <TableCell key={col}>
                  <Skeleton className="h-4 w-full max-w-[160px]" />
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

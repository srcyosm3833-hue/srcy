import { Link } from 'react-router-dom'

import { paths } from '@/routes/paths'
import { Badge } from '@/components/ui/badge'

interface CategoryBadgeProps {
  categoryName: string
  /** Verilirse badge tiklanabilir olur ve o kategoriyle filtrelenmis listeye gider. */
  categoryId?: string
}

/**
 * Kategori etiketi. categoryId verilirse blog listesini o kategoriyle filtreleyen
 * bir linke donusur; verilmezse statik rozet.
 */
export function CategoryBadge({ categoryName, categoryId }: CategoryBadgeProps) {
  if (categoryId) {
    return (
      <Link to={`${paths.blogs}?categoryId=${categoryId}`}>
        <Badge
          variant="secondary"
          className="transition-colors hover:bg-secondary/70"
        >
          {categoryName}
        </Badge>
      </Link>
    )
  }

  return <Badge variant="secondary">{categoryName}</Badge>
}

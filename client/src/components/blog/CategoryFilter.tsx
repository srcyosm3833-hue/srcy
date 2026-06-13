import type { Category } from '@/types'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

/** "Tüm Kategoriler" secenegi icin sentinel deger (Radix Select bos string kabul etmez). */
const ALL_VALUE = '__all__'

interface CategoryFilterProps {
  categories: Category[]
  /** Secili kategori id'si; null = tum kategoriler. */
  selectedId: string | null
  /** Secim degisince cagrilir. null = filtre kaldirildi. */
  onChange: (id: string | null) => void
  disabled?: boolean
}

/**
 * Blog listesi kategori filtresi (shadcn Select). "Tüm Kategoriler" + kategori
 * listesi. Secim URL parametresine yansitilir (cagiran tarafta).
 */
export function CategoryFilter({
  categories,
  selectedId,
  onChange,
  disabled,
}: CategoryFilterProps) {
  return (
    <Select
      value={selectedId ?? ALL_VALUE}
      onValueChange={(value) => onChange(value === ALL_VALUE ? null : value)}
      disabled={disabled}
    >
      <SelectTrigger className="w-full sm:w-56" aria-label="Kategoriye göre filtrele">
        <SelectValue placeholder="Tüm Kategoriler" />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={ALL_VALUE}>Tüm Kategoriler</SelectItem>
        {categories.map((category) => (
          <SelectItem key={category.id} value={category.id}>
            {category.categoryName}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}

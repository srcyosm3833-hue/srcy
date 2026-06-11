/**
 * Kategori sozlesmeleri. Kaynak: Features/Categories/Common/CategoryResponse.cs
 */
export interface Category {
  id: string
  categoryName: string
  /** Bu kategoriye bagli blog sayisi. */
  blogCount: number
  createdAt: string
  /** Hic guncellenmediyse null. */
  updatedAt: string | null
}

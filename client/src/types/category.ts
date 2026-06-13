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

/**
 * POST /api/admin/categories govdesi.
 * Kaynak: Features/Categories/Create/CreateCategoryCommand.cs (CategoryName).
 */
export interface CreateCategoryRequest {
  categoryName: string
}

/**
 * PUT /api/admin/categories/{id} govdesi (Id route'tan gelir).
 * Kaynak: AdminCategoriesController.UpdateCategoryRequest (CategoryName).
 */
export interface UpdateCategoryRequest {
  categoryName: string
}

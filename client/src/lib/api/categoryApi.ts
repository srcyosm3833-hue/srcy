import { request } from './client'
import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '@/types'

/**
 * Kategori API cagrilari.
 *
 * Endpoint'ler:
 *  GET    /api/categories            -> Category[] (public; DUZ DIZI, blogCount dahil)
 *  POST   /api/admin/categories      -> 201 Category (Authorize: Admin; 409 ayni isim)
 *  PUT    /api/admin/categories/{id} -> 200 Category (Authorize: Admin)
 *  DELETE /api/admin/categories/{id} -> 204 (Authorize: Admin; bagli blog varsa 409)
 */
export const categoryApi = {
  /** Tum kategorileri (blog sayilariyla) doner. */
  getAll(): Promise<Category[]> {
    return request<Category[]>({ method: 'get', url: '/api/categories' })
  },

  /** Yeni kategori olusturur (admin). 201 + kategori. */
  create(payload: CreateCategoryRequest): Promise<Category> {
    return request<Category>({
      method: 'post',
      url: '/api/admin/categories',
      data: payload,
    })
  },

  /** Kategoriyi gunceller (admin). 200 + guncel kategori. */
  update(id: string, payload: UpdateCategoryRequest): Promise<Category> {
    return request<Category>({
      method: 'put',
      url: `/api/admin/categories/${id}`,
      data: payload,
    })
  },

  /** Kategoriyi siler (admin). Bagli blog varsa 409. Basarida 204. */
  remove(id: string): Promise<void> {
    return request<void>({
      method: 'delete',
      url: `/api/admin/categories/${id}`,
    })
  },
}

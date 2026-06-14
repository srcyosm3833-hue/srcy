import { request } from './client'
import type {
  BlogAuditDetail,
  BlogDetail,
  BlogLikeToggleResponse,
  BlogListItem,
  BlogListQuery,
  BlogSearchQuery,
  CreateBlogRequest,
  PagedResult,
  UpdateBlogRequest,
} from '@/types'

/**
 * Blog API cagrilarini tek yerde toplayan servis katmani. Komponentler/hook'lar
 * dogrudan axios'a degil bu fonksiyonlara baglanir.
 *
 * Endpoint'ler (Controllers/BlogsController.cs):
 *  GET    /api/blogs           -> PagedResult<BlogListItem> (page/pageSize/categoryId)
 *  GET    /api/blogs/{id}      -> BlogDetail
 *  GET    /api/blogs/search    -> PagedResult<BlogListItem> (AllowAnonymous; q + page/pageSize/categoryId)
 *  POST   /api/blogs           -> 201 BlogDetail (Authorize; yazar token'dan)
 *  PUT    /api/blogs/{id}      -> 200 BlogDetail (Authorize; yazar veya Admin)
 *  DELETE /api/blogs/{id}      -> 204 (Authorize; yazar veya Admin)
 *  POST   /api/blogs/{id}/like -> 200 { liked, likeCount } (Authorize; anonim 401)
 *
 * NOT: Ayri bir GET /api/admin/blogs ucu YOKTUR. Admin liste tablosu da bu public
 * sayfali listeyi (getAll) kullanir; backend tum bloglari createdAt azalan doner.
 */
export const blogApi = {
  /** Sayfali blog listesi (createdAt azalan) + opsiyonel kategori filtresi. */
  getAll(query: BlogListQuery = {}): Promise<PagedResult<BlogListItem>> {
    return request<PagedResult<BlogListItem>>({
      method: 'get',
      url: '/api/blogs',
      params: {
        page: query.page,
        pageSize: query.pageSize,
        // Bos/undefined categoryId gonderilmez (params'ta undefined atlanir).
        categoryId: query.categoryId || undefined,
      },
    })
  },

  /**
   * Serbest metin arama (baslik + aciklama). getAll ile ayni sayfali sekli doner.
   * q bos/whitespace VEYA 200+ karakter ise backend 400 doner -> cagiran q'yu
   * gondermeden once gecerliligini kontrol etmeli (sayfada bos terimde getAll kullanilir).
   */
  search(query: BlogSearchQuery): Promise<PagedResult<BlogListItem>> {
    return request<PagedResult<BlogListItem>>({
      method: 'get',
      url: '/api/blogs/search',
      params: {
        q: query.q,
        page: query.page,
        pageSize: query.pageSize,
        categoryId: query.categoryId || undefined,
      },
    })
  },

  /** Tek blogun tam detayi. Bulunamazsa istek 404 ile reddeder. */
  getById(id: string): Promise<BlogDetail> {
    return request<BlogDetail>({ method: 'get', url: `/api/blogs/${id}` })
  },

  /**
   * Admin blog audit detayi (creatorIpHash dahil). Yalnizca Admin/Manager.
   * Public getById'den farki: audit alanlarini icerir, isLikedByCurrentUser icermez.
   * Soft delete edilmis bloglar da denetlenebilir. Bulunamazsa 404.
   */
  getAuditById(id: string): Promise<BlogAuditDetail> {
    return request<BlogAuditDetail>({
      method: 'get',
      url: `/api/admin/blogs/${id}`,
    })
  },

  /** Yeni blog olusturur. Yazar token'dan alinir (govdede UserId yoktur). 201 + detay. */
  create(payload: CreateBlogRequest): Promise<BlogDetail> {
    return request<BlogDetail>({
      method: 'post',
      url: '/api/blogs',
      data: payload,
    })
  },

  /** Var olan blogu gunceller (yazar veya Admin). 200 + guncel detay. */
  update(id: string, payload: UpdateBlogRequest): Promise<BlogDetail> {
    return request<BlogDetail>({
      method: 'put',
      url: `/api/blogs/${id}`,
      data: payload,
    })
  },

  /** Blogu siler (yazar veya Admin). Basarida govde donmez (204). */
  remove(id: string): Promise<void> {
    return request<void>({ method: 'delete', url: `/api/blogs/${id}` })
  },

  /**
   * Blog begenisini toggle eder (Authorize). Anonim cagri 401 ile reddedilir.
   * Donen { liked, likeCount } ile UI guncellenir; idempotent toggle.
   */
  toggleLike(id: string): Promise<BlogLikeToggleResponse> {
    return request<BlogLikeToggleResponse>({
      method: 'post',
      url: `/api/blogs/${id}/like`,
    })
  },
}

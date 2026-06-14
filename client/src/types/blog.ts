/**
 * Blog sozlesmeleri. Kaynaklar:
 *  - Features/Blogs/Common/BlogListItemResponse.cs (liste ogesi)
 *  - Features/Blogs/Common/BlogDetailResponse.cs (detay)
 *  - Controllers/BlogsController.cs (Create/Update request record'lari)
 */

/** Sayfali blog listesindeki tek oge (hafif: agir alanlar tasinmaz). */
export interface BlogListItem {
  id: string
  title: string
  /** Kapak gorseli URL'i. */
  coverImage: string
  categoryId: string
  categoryName: string
  /** Yazar tam adi (FirstName + LastName). */
  authorName: string
  /** ISO 8601 UTC olusturulma ani. */
  createdAt: string
  /** Blogun toplam begeni sayisi. */
  likeCount: number
  /** Istegi yapan kullanici bu blogu begenmis mi (anonimde false). */
  isLikedByCurrentUser: boolean
}

/** Tek blogun tam detayi (agir alanlar dahil). */
export interface BlogDetail {
  id: string
  title: string
  coverImage: string
  /** Icerik gorseli URL'i. */
  blogImage: string
  /** Blog icerigi/aciklamasi. */
  description: string
  categoryId: string
  categoryName: string
  authorId: string
  authorName: string
  createdAt: string
  /** Hic guncellenmediyse null. */
  updatedAt: string | null
  /** Blogun toplam begeni sayisi. */
  likeCount: number
  /** Istegi yapan kullanici bu blogu begenmis mi (anonimde false). */
  isLikedByCurrentUser: boolean
}

/**
 * Admin blog audit detayi. Kaynak: Features/Blogs/Common/BlogAuditDetailResponse.cs.
 * Public BlogDetail'den farki: audit alani `creatorIpHash` icerir, `isLikedByCurrentUser`
 * icermez. YALNIZCA GET /api/admin/blogs/{id} (Authorize: Admin,Manager) ucundan doner;
 * public uclarda bu alan ASLA yer almaz.
 */
export interface BlogAuditDetail {
  id: string
  title: string
  coverImage: string
  blogImage: string
  description: string
  categoryId: string
  categoryName: string
  authorId: string
  authorName: string
  createdAt: string
  /** Hic guncellenmediyse null. */
  updatedAt: string | null
  likeCount: number
  /** Olusturanin tuzlu SHA-256 IP hash'i (audit); cozulemediyse null. */
  creatorIpHash: string | null
}

/** POST /api/blogs/{id}/like yaniti (toggle sonucu). */
export interface BlogLikeToggleResponse {
  /** Islem sonunda blog begenili mi (true = like eklendi, false = kaldirildi). */
  liked: boolean
  /** Islem sonrasi toplam begeni sayisi. */
  likeCount: number
}

/** GET /api/blogs/search sorgu parametreleri. */
export interface BlogSearchQuery {
  /** Arama terimi (bos/whitespace gonderme; backend 400 doner). */
  q: string
  page?: number
  pageSize?: number
  categoryId?: string
}

/** POST /api/blogs govdesi (yazar token'dan alinir, govdede UserId yoktur). */
export interface CreateBlogRequest {
  title: string
  description: string
  coverImage: string
  blogImage: string
  categoryId: string
}

/** PUT /api/blogs/{id} govdesi. */
export interface UpdateBlogRequest {
  title: string
  description: string
  coverImage: string
  blogImage: string
  categoryId: string
}

/** GET /api/blogs sorgu parametreleri. */
export interface BlogListQuery {
  page?: number
  pageSize?: number
  categoryId?: string
}

import { request } from './client'
import type { CommentModerationItem, PagedResult } from '@/types'

/**
 * Admin yorum moderasyon API cagrilari.
 *
 * Endpoint'ler:
 *  GET    /api/admin/comments?page&pageSize -> PagedResult<CommentModerationItem>
 *                                              (Authorize: Admin; Manager 403 alir)
 *
 * Silme icin AYRI uclar kullanilir (yorum vs alt yorum):
 *  DELETE /api/blogs/{blogId}/comments/{id}              (ust yorum; isReply=false)
 *  DELETE /api/comments/{parentCommentId}/replies/{id}  (alt yorum; isReply=true)
 *
 * Listede her oge isReply bayragini tasir; cagiran taraf buna gore dogru
 * silme fonksiyonunu secer.
 */
export const adminCommentApi = {
  /** Tum yorum ve yanitlari moderasyon icin sayfali doner (admin). */
  getCommentsForAdmin(
    page = 1,
    pageSize = 20,
  ): Promise<PagedResult<CommentModerationItem>> {
    return request<PagedResult<CommentModerationItem>>({
      method: 'get',
      url: '/api/admin/comments',
      params: { page, pageSize },
    })
  },

  /** Ust seviye yorumu siler (isReply=false). */
  deleteComment(blogId: string, id: string): Promise<void> {
    return request<void>({
      method: 'delete',
      url: `/api/blogs/${blogId}/comments/${id}`,
    })
  },

  /** Alt yorumu (yaniti) siler (isReply=true). */
  deleteReply(parentCommentId: string, id: string): Promise<void> {
    return request<void>({
      method: 'delete',
      url: `/api/comments/${parentCommentId}/replies/${id}`,
    })
  },
}

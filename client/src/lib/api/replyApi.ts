import { request } from './client'
import type {
  AddReplyRequest,
  PagedResult,
  SubComment,
  UpdateReplyRequest,
} from '@/types'

/**
 * Alt yorum (reply / SubComment) API cagrilari.
 *
 * Endpoint'ler (Controllers/RepliesController.cs):
 *  GET    /api/comments/{commentId}/replies       -> PagedResult<SubComment> (SAYFALI; createdAt azalan)
 *  POST   /api/comments/{commentId}/replies       -> SubComment (201; Authorize)
 *  PUT    /api/comments/{commentId}/replies/{id}  -> SubComment (200; yalnizca sahip)
 *  DELETE /api/comments/{commentId}/replies/{id}  -> 204 (sahip veya admin)
 *
 * Alt yorumlar ana yorum yaniti icinde GOMULU DEGIL; tiklayinca bu GET ile lazy
 * cekilir. Ana yorum yoksa 404. Yazar token'dan; govdede UserId yoktur.
 */
export const replyApi = {
  /** Bir ana yoruma ait alt yorumlari sayfali doner. */
  getByCommentId(
    commentId: string,
    page = 1,
    pageSize = 10,
  ): Promise<PagedResult<SubComment>> {
    return request<PagedResult<SubComment>>({
      method: 'get',
      url: `/api/comments/${commentId}/replies`,
      params: { page, pageSize },
    })
  },

  /** Yoruma yeni yanit ekler (giris yapmis kullanici). Olusan yaniti doner. */
  add(commentId: string, payload: AddReplyRequest): Promise<SubComment> {
    return request<SubComment>({
      method: 'post',
      url: `/api/comments/${commentId}/replies`,
      data: payload,
    })
  },

  /** Mevcut yaniti gunceller (yalnizca sahip). Guncellenmis yaniti doner. */
  update(
    commentId: string,
    replyId: string,
    payload: UpdateReplyRequest,
  ): Promise<SubComment> {
    return request<SubComment>({
      method: 'put',
      url: `/api/comments/${commentId}/replies/${replyId}`,
      data: payload,
    })
  },

  /** Yaniti siler (sahip veya admin). */
  remove(commentId: string, replyId: string): Promise<void> {
    return request<void>({
      method: 'delete',
      url: `/api/comments/${commentId}/replies/${replyId}`,
    })
  },
}

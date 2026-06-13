import { request } from './client'
import type {
  AddCommentRequest,
  Comment,
  PagedResult,
  UpdateCommentRequest,
} from '@/types'

/**
 * Yorum API cagrilari.
 *
 * Endpoint'ler (Controllers/CommentsController.cs):
 *  GET    /api/blogs/{blogId}/comments       -> PagedResult<Comment> (SAYFALI; createdAt azalan)
 *  POST   /api/blogs/{blogId}/comments       -> Comment (201; Authorize)
 *  PUT    /api/blogs/{blogId}/comments/{id}  -> Comment (200; yalnizca sahip)
 *  DELETE /api/blogs/{blogId}/comments/{id}  -> 204 (sahip veya admin)
 *
 * Not: Yorum endpoint'i sayfalidir (duz dizi DEGIL). Blog yoksa 404 doner.
 * Yazar/istek sahibi token'dan alinir; govdede UserId yoktur.
 */
export const commentApi = {
  /** Bir bloga ait yorumlari sayfali doner. */
  getByBlogId(
    blogId: string,
    page = 1,
    pageSize = 20,
  ): Promise<PagedResult<Comment>> {
    return request<PagedResult<Comment>>({
      method: 'get',
      url: `/api/blogs/${blogId}/comments`,
      params: { page, pageSize },
    })
  },

  /** Bloga yeni yorum ekler (giris yapmis kullanici). Olusan yorumu doner. */
  add(blogId: string, payload: AddCommentRequest): Promise<Comment> {
    return request<Comment>({
      method: 'post',
      url: `/api/blogs/${blogId}/comments`,
      data: payload,
    })
  },

  /** Mevcut yorumu gunceller (yalnizca sahip). Guncellenmis yorumu doner. */
  update(
    blogId: string,
    commentId: string,
    payload: UpdateCommentRequest,
  ): Promise<Comment> {
    return request<Comment>({
      method: 'put',
      url: `/api/blogs/${blogId}/comments/${commentId}`,
      data: payload,
    })
  },

  /** Yorumu siler (sahip veya admin). */
  remove(blogId: string, commentId: string): Promise<void> {
    return request<void>({
      method: 'delete',
      url: `/api/blogs/${blogId}/comments/${commentId}`,
    })
  },
}

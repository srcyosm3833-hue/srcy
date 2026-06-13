import { request } from './client'
import type {
  MarkMessageAsReadRequest,
  Message,
  PagedResult,
  SendMessageRequest,
} from '@/types'

/**
 * Iletisim formu mesaj API cagrilari.
 *
 * Endpoint'ler:
 *  POST  /api/messages            -> 201 (govdesiz; AllowAnonymous)
 *  GET   /api/admin/messages      -> PagedResult<Message> (Authorize: Admin;
 *                                    siralama: okunmamislar once, sonra CreatedAt azalan)
 *  PATCH /api/admin/messages/{id} -> 200 Message (Authorize: Admin; {isRead} set eder)
 */
export const messageApi = {
  /** Iletisim formundan mesaj gonderir (anonim). */
  send(payload: SendMessageRequest): Promise<void> {
    return request<void>({
      method: 'post',
      url: '/api/messages',
      data: payload,
    })
  },

  /** Admin mesaj kutusu: sayfali liste (okunmamislar once). */
  getAll(page = 1, pageSize = 10): Promise<PagedResult<Message>> {
    return request<PagedResult<Message>>({
      method: 'get',
      url: '/api/admin/messages',
      params: { page, pageSize },
    })
  },

  /** Mesajin okunma durumunu acikca set eder (admin). 200 + guncel mesaj. */
  setRead(id: string, payload: MarkMessageAsReadRequest): Promise<Message> {
    return request<Message>({
      method: 'patch',
      url: `/api/admin/messages/${id}`,
      data: payload,
    })
  },
}

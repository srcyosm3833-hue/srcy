import { request } from './client'
import type { Contact, UpsertContactRequest } from '@/types'

/**
 * Iletisim bilgisi API cagrilari.
 *
 * Endpoint'ler:
 *  GET /api/contact         -> Contact (200; AllowAnonymous)
 *  PUT /api/admin/contact   -> Contact (200 varsa / 201 ilk kez; Authorize: Admin)
 *
 * Uygulama TEK bir Contact kaydi tutar (upsert). Henuz kayit yapilandirilmadiysa
 * GET 404 ile reddeder; cagiran taraf bunu zarif "bilgi yok" durumu olarak ele
 * alir ve PUT ile ilk kaydi olusturur.
 */
export const contactApi = {
  /** Tekil iletisim bilgisini doner. Kayit yoksa 404. */
  get(): Promise<Contact> {
    return request<Contact>({ method: 'get', url: '/api/contact' })
  },

  /** Iletisim bilgisini olusturur veya gunceller (admin upsert). Guncel kaydi doner. */
  update(payload: UpsertContactRequest): Promise<Contact> {
    return request<Contact>({
      method: 'put',
      url: '/api/admin/contact',
      data: payload,
    })
  },
}

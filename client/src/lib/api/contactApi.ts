import { request } from './client'
import type { Contact } from '@/types'

/**
 * Iletisim bilgisi API cagrilari.
 *
 * Endpoint (Controllers/ContactController.cs):
 *  GET /api/contact -> Contact (200; AllowAnonymous)
 *
 * Uygulama TEK bir Contact kaydi tutar. Henuz kayit yapilandirilmadiysa istek
 * 404 ile reddeder; cagiran taraf bunu zarif "bilgi yok" durumu olarak ele alir.
 */
export const contactApi = {
  /** Tekil iletisim bilgisini doner. Kayit yoksa 404. */
  get(): Promise<Contact> {
    return request<Contact>({ method: 'get', url: '/api/contact' })
  },
}

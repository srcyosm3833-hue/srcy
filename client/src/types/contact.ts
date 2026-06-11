/**
 * Iletisim bilgisi sozlesmeleri. Uygulama TEK bir Contact kaydi tutar (upsert).
 * Kaynaklar:
 *  - Features/Contact/Common/ContactResponse.cs
 *  - Features/Contact/Upsert/UpsertContactCommand.cs
 *
 * Public okuma:  GET /api/contact  (kayit yoksa 404)
 * Admin upsert:  PUT /api/admin/contact  (Authorize: Admin)
 */
export interface Contact {
  id: string
  address: string
  email: string
  phone: string
  /** Harita (embed/konum) URL'i. */
  mapUrl: string
}

/** PUT /api/admin/contact govdesi (upsert). */
export interface UpsertContactRequest {
  address: string
  email: string
  phone: string
  mapUrl: string
}

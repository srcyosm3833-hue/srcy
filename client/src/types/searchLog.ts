/**
 * Arama audit log sozlesmeleri. Kaynaklar:
 *  - Features/SearchLogs/Common/SearchLogResponse.cs (admin listesi)
 *  - Controllers/AdminSearchLogsController.cs (GET /api/admin/search-logs)
 *
 * Admin listeleme: GET /api/admin/search-logs?page&pageSize&term  (Authorize: YALNIZ Admin)
 *
 * KVKK NOTU: Bu kayitlar kisisel veri icerebilir; IP daima hash'lidir (ham IP donmez).
 * Erisim bilincli olarak en dar role (Admin) sinirlandirilmistir.
 */

/** Admin arama log listesindeki tek kayit. */
export interface SearchLog {
  id: string
  /** Aranan terim. */
  term: string
  /** Aramayi yapan kullanicinin kimligi; anonim aramada null. */
  userId: string | null
  /** Log anindaki kullanici tam adi snapshot'i; anonimde null. */
  userFullName: string | null
  /** Tuzlu SHA-256 IP hash'i; cozumlenemediyse null. */
  ipHash: string | null
  /** ISO 8601 UTC aramanin gerceklestigi an. */
  searchedAt: string
}

/** GET /api/admin/search-logs sorgu parametreleri. */
export interface SearchLogQuery {
  page?: number
  pageSize?: number
  /** Terim filtresi (bos/undefined ise tum kayitlar). */
  term?: string
}

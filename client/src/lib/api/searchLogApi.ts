import { request } from './client'
import type { PagedResult, SearchLog, SearchLogQuery } from '@/types'

/**
 * Admin arama audit log API cagrilari.
 *
 * Endpoint:
 *  GET /api/admin/search-logs?page&pageSize&term -> PagedResult<SearchLog>
 *      (Authorize: YALNIZ Admin; Manager 403 alir)
 *
 * Backend SearchedAt azalan (en yeni once) sirali doner. term verilirse yalnizca
 * terimi iceren kayitlar (buyuk/kucuk harf duyarsiz) doner.
 */
export const searchLogApi = {
  /** Arama loglarini sayfali (+ opsiyonel terim filtresi) doner (admin). */
  getAll(query: SearchLogQuery = {}): Promise<PagedResult<SearchLog>> {
    return request<PagedResult<SearchLog>>({
      method: 'get',
      url: '/api/admin/search-logs',
      params: {
        page: query.page,
        pageSize: query.pageSize,
        // Bos/undefined term gonderilmez (params'ta undefined atlanir).
        term: query.term || undefined,
      },
    })
  },
}

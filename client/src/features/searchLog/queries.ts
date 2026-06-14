import { useQuery } from '@tanstack/react-query'
import { searchLogApi } from '@/lib/api'
import type { SearchLogQuery } from '@/types'

/**
 * Arama audit log server-state hook'lari (yalniz Admin). Query key'ler tek yerde
 * (searchLogKeys) toplanir; sayfa + terim filtresi key'e dahildir (degisince refetch).
 */
export const searchLogKeys = {
  all: ['searchLogs'] as const,
  /** Sayfali/filtreli liste: parametreler key'e dahil. */
  list: (params: SearchLogQuery) =>
    [...searchLogKeys.all, 'list', params] as const,
}

/**
 * Sayfali arama log listesi (+ opsiyonel terim filtresi). page/term degisince
 * otomatik refetch eder. Sayfa/filtre gecislerinde onceki veri korunur
 * (placeholderData -> titreme onlenir).
 */
export function useSearchLogs(params: SearchLogQuery) {
  return useQuery({
    queryKey: searchLogKeys.list(params),
    queryFn: () => searchLogApi.getAll(params),
    placeholderData: (previous) => previous,
  })
}

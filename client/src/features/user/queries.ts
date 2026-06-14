import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { userApi } from '@/lib/api'
import type { UserListQuery } from '@/types'

/**
 * Admin kullanici yonetimi server-state hook'lari. Listeleme Admin + Manager;
 * rol atama/kaldirma mutasyonlari yalniz Admin (backend zorlar). Mutasyonlar
 * sonrasi tum kullanici listeleri invalidate edilir (roller tazelenir).
 */
export const userKeys = {
  all: ['adminUsers'] as const,
  /** Sayfali kullanici listesi (belirli sayfa + includeDeleted). */
  list: (params: UserListQuery) => [...userKeys.all, 'list', params] as const,
}

/**
 * Sayfali kullanici listesi. page/includeDeleted degisince otomatik refetch;
 * sayfa gecisinde onceki veri korunur (placeholderData -> titreme onlenir).
 */
export function useUsers(params: UserListQuery) {
  return useQuery({
    queryKey: userKeys.list(params),
    queryFn: () => userApi.getAll(params),
    placeholderData: (previous) => previous,
  })
}

/**
 * Kullaniciya rol atar (yalniz Admin; idempotent). Basarida tum kullanici
 * listelerini tazeler (rol rozetleri guncellensin).
 */
export function useAssignRole() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: string; roleName: string }) =>
      userApi.assignRole(vars.id, { roleName: vars.roleName }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: userKeys.all })
    },
  })
}

/**
 * Kullanicidan rol kaldirir (yalniz Admin). Son Admin korumasi backend'de 400
 * doner; cagiran taraf bunu toast ile ele alir. Basarida kullanici listelerini tazeler.
 */
export function useRemoveRole() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: string; roleName: string }) =>
      userApi.removeRole(vars.id, vars.roleName),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: userKeys.all })
    },
  })
}

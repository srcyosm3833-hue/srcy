import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { roleApi } from '@/lib/api'
import type { CreateRoleRequest, UpdateRoleRequest } from '@/types'

/**
 * Rol yonetimi server-state hook'lari (yalniz Admin). Query key'ler tek yerde
 * (roleKeys) toplanir; her mutasyon sonrasi liste invalidate edilir (kullanici
 * sayilari + yeni/silinen roller tazelenir).
 */
export const roleKeys = {
  all: ['adminRoles'] as const,
}

/** Tum roller (kullanici sayilariyla). Admin rol yonetimi sayfasi kullanir. */
export function useRoles() {
  return useQuery({
    queryKey: roleKeys.all,
    queryFn: () => roleApi.getAll(),
  })
}

/** Yeni rol olusturma. Basarida rol listesini tazeler. 409 (ayni ad) cagiranda ele alinir. */
export function useCreateRole() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateRoleRequest) => roleApi.create(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: roleKeys.all })
    },
  })
}

/** Rol yeniden adlandirma. Basarida rol listesini tazeler. */
export function useUpdateRole() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: string; payload: UpdateRoleRequest }) =>
      roleApi.update(vars.id, vars.payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: roleKeys.all })
    },
  })
}

/**
 * Rol silme. Korumali rol 400, kullanicisi olan rol 409 doner; cagiran taraf
 * bunlari ozel toast ile ele alir. Basarida rol listesini tazeler.
 */
export function useDeleteRole() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => roleApi.remove(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: roleKeys.all })
    },
  })
}

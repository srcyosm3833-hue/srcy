import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { categoryApi } from '@/lib/api'
import type { CreateCategoryRequest, UpdateCategoryRequest } from '@/types'

/** Kategori server-state hook'lari. */
export const categoryKeys = {
  all: ['categories'] as const,
}

/**
 * Tum kategoriler. Nadiren degistiginden uzun staleTime ile cache'lenir;
 * hem liste filtresi hem anasayfa ayni cache'i paylasir.
 */
export function useCategories() {
  return useQuery({
    queryKey: categoryKeys.all,
    queryFn: () => categoryApi.getAll(),
    staleTime: 5 * 60_000,
  })
}

/** Yeni kategori olusturma (admin). Basarida kategori listesini tazeler. */
export function useCreateCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateCategoryRequest) => categoryApi.create(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: categoryKeys.all })
    },
  })
}

/** Kategori guncelleme (admin). Basarida kategori listesini tazeler. */
export function useUpdateCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: string; payload: UpdateCategoryRequest }) =>
      categoryApi.update(vars.id, vars.payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: categoryKeys.all })
    },
  })
}

/**
 * Kategori silme (admin). Bagli blog varsa backend 409 doner; cagiran taraf bunu
 * ozel bir uyari olarak ele alir. Basarida kategori listesini tazeler.
 */
export function useDeleteCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => categoryApi.remove(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: categoryKeys.all })
    },
  })
}

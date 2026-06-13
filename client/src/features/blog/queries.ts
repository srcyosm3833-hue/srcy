import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { blogApi } from '@/lib/api'
import type {
  BlogDetail,
  BlogListQuery,
  BlogSearchQuery,
  CreateBlogRequest,
  UpdateBlogRequest,
} from '@/types'

/**
 * Blog server-state hook'lari (TanStack Query). Query key'ler tek yerde
 * (blogKeys) toplanir; boylece invalidasyon tutarli olur.
 */
export const blogKeys = {
  all: ['blogs'] as const,
  /** Sayfali/filtreli liste: parametreler key'e dahil (degisince otomatik refetch). */
  list: (params: BlogListQuery) => [...blogKeys.all, 'list', params] as const,
  /** Sayfali arama: parametreler key'e dahil (q/page/categoryId degisince refetch). */
  search: (params: BlogSearchQuery) =>
    [...blogKeys.all, 'search', params] as const,
  /** Tek blog detayi. */
  detail: (id: string) => [...blogKeys.all, 'detail', id] as const,
}

/**
 * Sayfali blog listesi. page/categoryId degisince otomatik refetch eder.
 * `placeholderData` ile sayfa gecislerinde onceki veri korunur (titreme onlenir).
 */
export function useBlogList(params: BlogListQuery) {
  return useQuery({
    queryKey: blogKeys.list(params),
    queryFn: () => blogApi.getAll(params),
    // Sayfa degisirken eski listeyi koru (TanStack v5: identity placeholder).
    placeholderData: (previous) => previous,
  })
}

/**
 * Sayfali blog arama. Yalnizca gecerli (bos olmayan) terimde calisir (enabled);
 * boylece terim temizlenince bu sorgu durur ve sayfa duz listelemeye doner.
 * Sayfa gecislerinde onceki veri korunur (placeholderData).
 */
export function useBlogSearch(params: BlogSearchQuery) {
  return useQuery({
    queryKey: blogKeys.search(params),
    queryFn: () => blogApi.search(params),
    enabled: params.q.trim().length > 0,
    placeholderData: (previous) => previous,
  })
}

/** Tek blog detayi. id yoksa sorgu calismaz (enabled). */
export function useBlogDetail(id: string | undefined) {
  return useQuery({
    queryKey: blogKeys.detail(id ?? ''),
    queryFn: () => blogApi.getById(id as string),
    enabled: Boolean(id),
  })
}

/**
 * Yeni blog olusturma (admin/yazar). Basarida tum blog listelerini invalidate eder
 * (yeni blog listede gorunsun).
 */
export function useCreateBlog() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateBlogRequest) => blogApi.create(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: blogKeys.all })
    },
  })
}

/** Blog guncelleme. Basarida listeleri ve ilgili detayi tazeler. */
export function useUpdateBlog() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: string; payload: UpdateBlogRequest }) =>
      blogApi.update(vars.id, vars.payload),
    onSuccess: (_data, vars) => {
      void queryClient.invalidateQueries({ queryKey: blogKeys.all })
      void queryClient.invalidateQueries({
        queryKey: blogKeys.detail(vars.id),
      })
    },
  })
}

/** Blog silme. Basarida tum blog listelerini tazeler. */
export function useDeleteBlog() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => blogApi.remove(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: blogKeys.all })
    },
  })
}

/**
 * Blog begeni toggle'i. Optimistic update: tiklaninca blog detayi cache'i hemen
 * (liked/likeCount) guncellenir; hata olursa onceki degere geri alinir. Sunucu
 * yaniti gelince kesin degerle senkronlanir ve liste sorgulari invalidate edilir
 * (kart uzerindeki begeni sayisi da tazelensin).
 */
export function useToggleBlogLike(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => blogApi.toggleLike(id),
    onMutate: async () => {
      // Devam eden detay refetch'i optimistic degerin uzerine yazmasin diye iptal et.
      await queryClient.cancelQueries({ queryKey: blogKeys.detail(id) })

      const previous = queryClient.getQueryData<BlogDetail>(blogKeys.detail(id))

      if (previous) {
        const liked = !previous.isLikedByCurrentUser
        queryClient.setQueryData<BlogDetail>(blogKeys.detail(id), {
          ...previous,
          isLikedByCurrentUser: liked,
          likeCount: Math.max(0, previous.likeCount + (liked ? 1 : -1)),
        })
      }

      return { previous }
    },
    onError: (_error, _vars, context) => {
      // Optimistic degisikligi geri al.
      if (context?.previous) {
        queryClient.setQueryData(blogKeys.detail(id), context.previous)
      }
    },
    onSuccess: (data) => {
      // Sunucunun kesin degerleriyle detay cache'ini hizala.
      const current = queryClient.getQueryData<BlogDetail>(blogKeys.detail(id))
      if (current) {
        queryClient.setQueryData<BlogDetail>(blogKeys.detail(id), {
          ...current,
          isLikedByCurrentUser: data.liked,
          likeCount: data.likeCount,
        })
      }
    },
    onSettled: () => {
      // Liste/arama kartlarindaki begeni sayisi da guncel kalsin.
      void queryClient.invalidateQueries({ queryKey: blogKeys.all })
    },
  })
}

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { AxiosError } from 'axios'
import { contactApi, socialMediaApi } from '@/lib/api'
import type {
  CreateSocialMediaRequest,
  UpdateSocialMediaRequest,
  UpsertContactRequest,
} from '@/types'

/** Iletisim ve sosyal medya server-state hook'lari. */
export const contactKeys = {
  all: ['contact'] as const,
  /** Tekil iletisim bilgisi. */
  info: () => [...contactKeys.all, 'info'] as const,
}

export const socialMediaKeys = {
  all: ['socialMedia'] as const,
  /** Tum sosyal medya baglantilari. */
  list: () => [...socialMediaKeys.all, 'list'] as const,
}

/**
 * Tekil iletisim bilgisi. Kayit yoksa backend 404 doner; bu BEKLENEN bir
 * durumdur (hard error degil), bu yuzden 404'te tekrar deneme yapilmaz. Cagiran
 * taraf `error.status === 404`'u zarif "bilgi yok" olarak ele alir.
 */
export function useContact() {
  return useQuery({
    queryKey: contactKeys.info(),
    queryFn: () => contactApi.get(),
    retry: (failureCount, error) => {
      // 404 beklenen sonuc: tekrar deneme. Diger hatalarda en fazla 2 kez dene.
      if (error instanceof AxiosError && error.response?.status === 404) {
        return false
      }
      return failureCount < 2
    },
  })
}

/**
 * Iletisim bilgisini olusturur/gunceller (admin upsert). Basarida tekil iletisim
 * sorgusunu tazeler (404'tan donmusse de yeni veriyle yenilenir).
 */
export function useUpsertContact() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: UpsertContactRequest) => contactApi.update(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: contactKeys.all })
    },
  })
}

/** Tum sosyal medya baglantilari (bos dizi gelebilir). */
export function useSocialMedia() {
  return useQuery({
    queryKey: socialMediaKeys.list(),
    queryFn: () => socialMediaApi.getAll(),
  })
}

/** Yeni sosyal medya baglantisi olusturma (admin). Basarida listeyi tazeler. */
export function useCreateSocialMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateSocialMediaRequest) =>
      socialMediaApi.create(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: socialMediaKeys.all })
    },
  })
}

/** Sosyal medya baglantisi guncelleme (admin). Basarida listeyi tazeler. */
export function useUpdateSocialMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: string; payload: UpdateSocialMediaRequest }) =>
      socialMediaApi.update(vars.id, vars.payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: socialMediaKeys.all })
    },
  })
}

/** Sosyal medya baglantisi silme (admin). Basarida listeyi tazeler. */
export function useDeleteSocialMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => socialMediaApi.remove(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: socialMediaKeys.all })
    },
  })
}

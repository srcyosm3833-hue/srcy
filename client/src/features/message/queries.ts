import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { messageApi } from '@/lib/api'

/**
 * Mesaj kutusu (admin) server-state hook'lari. Query key'ler tek yerde (messageKeys)
 * toplanir; okundu isaretleme mutasyonu sonrasi liste tutarli sekilde tazelenir.
 */
export const messageKeys = {
  all: ['adminMessages'] as const,
  /** Sayfali mesaj listesi (belirli sayfa). */
  list: (page: number, pageSize: number) =>
    [...messageKeys.all, 'list', page, pageSize] as const,
}

/**
 * Admin mesaj listesi (sayfali). Backend siralamasi: okunmamislar once, ardindan
 * her grup icinde CreatedAt azalan. page degisince otomatik refetch; sayfa
 * gecisinde onceki veri korunur (titreme onlenir).
 */
export function useMessages(page = 1, pageSize = 10) {
  return useQuery({
    queryKey: messageKeys.list(page, pageSize),
    queryFn: () => messageApi.getAll(page, pageSize),
    placeholderData: (previous) => previous,
  })
}

/**
 * Sidebar badge'i icin okunmamis mesaj sayisi. Ayri bir backend sayac ucu
 * olmadigindan ilk sayfanin totalCount/isRead bilgisinden TUREVDIR: backend
 * okunmamislari basa koydugu ve totalCount tum mesajlari verdigi icin, ilk
 * sayfadaki okunmamis sayisi pratik bir gostergedir (kesin sayim icin yeterli
 * degildir; yalnizca rozet icin yaklasik kullanilir).
 *
 * NOT: Kesin okunmamis sayisi icin pageSize buyuk tutulur (ilk sayfada tum
 * okunmamislarin gelmesi olasiligini artirir).
 */
export function useUnreadMessageCount() {
  const query = useQuery({
    queryKey: messageKeys.list(1, 100),
    queryFn: () => messageApi.getAll(1, 100),
    staleTime: 60_000,
  })

  const unreadCount = query.data
    ? query.data.items.filter((m) => !m.isRead).length
    : 0

  return { ...query, unreadCount }
}

/**
 * Mesajin okunma durumunu set eder (admin). Basarida tum mesaj listelerini
 * invalidate eder (liste sirasi + sidebar rozeti guncellensin).
 */
export function useSetMessageRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { id: string; isRead: boolean }) =>
      messageApi.setRead(vars.id, { isRead: vars.isRead }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: messageKeys.all })
    },
  })
}

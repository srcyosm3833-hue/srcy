import { QueryClient } from '@tanstack/react-query'

/**
 * Uygulama genelinde paylasilan TanStack Query istemcisi.
 * Server state cache'ini yonetir. Varsayilanlar iskelet asamasi icin makuldur;
 * ileride feature'lara gore ayarlanabilir.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Pencere odaklaninca otomatik refetch'i kapat (geliştirme sirasinda gureltuyu azaltir).
      refetchOnWindowFocus: false,
      // Veriyi 30 sn taze say.
      staleTime: 30_000,
      retry: 1,
    },
  },
})

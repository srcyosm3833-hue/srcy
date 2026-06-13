import { useEffect, useState } from 'react'

/**
 * Bir degeri gecikmeli (debounce) doner: deger her degistiginde sayac sifirlanir,
 * yalnizca `delayMs` boyunca degismeden kaldiginda sonuc guncellenir. Hizli
 * yazimda (orn. arama input'u) her tus icin istek atilmasini onler.
 *
 * @param value Takip edilen anlik deger.
 * @param delayMs Bekleme suresi (ms).
 * @returns Gecikmeli (stabilize olmus) deger.
 */
export function useDebouncedValue<T>(value: T, delayMs: number): T {
  const [debounced, setDebounced] = useState<T>(value)

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs)
    return () => clearTimeout(timer)
  }, [value, delayMs])

  return debounced
}

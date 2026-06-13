import type { ReactNode } from 'react'
import { ThemeProvider as NextThemesProvider } from 'next-themes'

/**
 * Tema saglayicisi. next-themes uzerine ince bir sarmalayici:
 *  - <html> elementine `class` (light/dark) ekler -> Tailwind `dark:` varyanti.
 *  - Tercihi localStorage'da `zn.theme` anahtarinda saklar.
 *  - defaultTheme="system" -> isletim sistemi tercihini izler (prefers-color-scheme).
 *  - disableTransitionOnChange: tema gecisinde renk caymasini (flash) engeller.
 *
 * App'te RouterProvider'i sarmalar ki tum sayfalar ve Toaster temaya erisebilsin.
 */
export function ThemeProvider({ children }: { children: ReactNode }) {
  return (
    <NextThemesProvider
      attribute="class"
      defaultTheme="system"
      enableSystem
      storageKey="zn.theme"
      disableTransitionOnChange
    >
      {children}
    </NextThemesProvider>
  )
}

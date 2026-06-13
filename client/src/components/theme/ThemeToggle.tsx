import { useTheme } from 'next-themes'
import { Moon, Sun } from 'lucide-react'

import { Button } from '@/components/ui/button'

/**
 * Light/Dark tema gecis butonu. Tek tikla aktif temayi ters cevirir.
 * `system` tercihinde gercek goruntulenen tema `resolvedTheme`'ten okunur.
 *
 * Not: SPA (SSR yok) oldugu icin next-themes ilk client render'da temayi dogru
 * cozer; ayri bir `mounted` bayragina gerek yoktur. resolvedTheme henuz tanimli
 * degilse (cok kisa an) varsayilan olarak ay ikonu gosterilir.
 */
export function ThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  function toggle() {
    setTheme(isDark ? 'light' : 'dark')
  }

  return (
    <Button
      variant="ghost"
      size="icon"
      onClick={toggle}
      aria-label={isDark ? 'Aydinlik temaya gec' : 'Karanlik temaya gec'}
      title={isDark ? 'Aydinlik tema' : 'Karanlik tema'}
    >
      {isDark ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
    </Button>
  )
}

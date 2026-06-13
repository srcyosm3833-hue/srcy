import { useTheme } from 'next-themes'
import { Toaster as Sonner, type ToasterProps } from 'sonner'

/**
 * shadcn/ui Toaster (Sonner). Tema (light/dark/system) next-themes'ten okunur.
 * Renkler index.css token'larina baglanir (--normal-bg vb. css degiskenleri).
 * Global olarak App'te bir kez render edilir; toast() cagrilari her yerden yapilir.
 */
function Toaster({ ...props }: ToasterProps) {
  const { theme = 'system' } = useTheme()

  return (
    <Sonner
      theme={theme as ToasterProps['theme']}
      className="toaster group"
      style={
        {
          '--normal-bg': 'hsl(var(--popover))',
          '--normal-text': 'hsl(var(--popover-foreground))',
          '--normal-border': 'hsl(var(--border))',
        } as React.CSSProperties
      }
      {...props}
    />
  )
}

export { Toaster }

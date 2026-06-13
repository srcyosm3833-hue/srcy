import { Button } from '@/components/ui/button'

/**
 * Sosyal giris butonlari icin rezerve gorsel slot (SOCIAL-LOGIN-PLAN.md / AO-F9).
 *
 * Henuz GERCEK sosyal giris YOK. Bu bilesen yalnizca yerlesimi ayirtmak ve
 * gelecekteki Google/Facebook butonlarinin gelecegi yeri belirgin kilmak icin
 * var; butonlar `disabled` ve islevsizdir. Sosyal giris ozelligi tamamlaninca
 * burasi gercek `SocialLoginButtons` ile degistirilecek.
 */
export function SocialLoginSlot() {
  return (
    <div className="space-y-3">
      {/* Ayrac: "veya" */}
      <div className="relative">
        <div className="absolute inset-0 flex items-center" aria-hidden="true">
          <span className="w-full border-t border-border" />
        </div>
        <div className="relative flex justify-center text-xs uppercase">
          <span className="bg-background px-2 text-muted-foreground">veya</span>
        </div>
      </div>

      {/* Placeholder butonlar — islevsiz (yakinda). */}
      <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
        <Button
          type="button"
          variant="outline"
          className="w-full"
          disabled
          aria-label="Google ile giriş (yakında)"
          title="Yakında"
        >
          Google
        </Button>
        <Button
          type="button"
          variant="outline"
          className="w-full"
          disabled
          aria-label="Facebook ile giriş (yakında)"
          title="Yakında"
        >
          Facebook
        </Button>
      </div>

      <p className="text-center text-xs text-muted-foreground">
        Sosyal giriş yakında eklenecek.
      </p>
    </div>
  )
}

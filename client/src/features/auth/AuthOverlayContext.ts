import { createContext } from 'react'

/** Overlay'de aktif panel: giris mi kayit mi. */
export type AuthOverlayPanel = 'login' | 'register'

/**
 * Auth overlay (login/register modal) durumunu disa acan sozlesme. Header butonu
 * ve route sayfalari bu context uzerinden overlay'i acar/kapatir. Provider
 * implementasyonu AuthOverlayProvider'da; tuketim useAuthOverlay() hook'uyla.
 */
export interface AuthOverlayContextValue {
  /** Overlay acik mi. */
  isOpen: boolean
  /** Acikken hangi panel gosteriliyor. */
  activePanel: AuthOverlayPanel
  /** Overlay'i giris paneliyle ac. */
  openLogin: () => void
  /** Overlay'i kayit paneliyle ac. */
  openRegister: () => void
  /** Acikken paneller arasi gecis yap (animasyonlu). */
  setPanel: (panel: AuthOverlayPanel) => void
  /** Overlay'i kapat. */
  close: () => void
}

/**
 * undefined varsayilan: Provider disinda kullanim useAuthOverlay icinde
 * yakalanir (acik bir hata firlatilir).
 */
export const AuthOverlayContext = createContext<
  AuthOverlayContextValue | undefined
>(undefined)

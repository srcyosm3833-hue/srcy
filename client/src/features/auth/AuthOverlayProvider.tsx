import { useCallback, useMemo, useState } from 'react'
import type { ReactNode } from 'react'

import {
  AuthOverlayContext,
  type AuthOverlayContextValue,
  type AuthOverlayPanel,
} from './AuthOverlayContext'

/**
 * Auth overlay (login/register modal) durumunu yoneten Provider. Overlay
 * herhangi bir sayfadan acilabilmesi icin uygulama agacinin ustunde
 * (RouterProvider'i sarmalayacak sekilde) konumlanir.
 *
 * State sadece acik/kapali + aktif panelden ibaret; gercek auth islemleri
 * (login/register) AuthProvider'da. Modal'in kendisi (<AuthOverlay />) ise
 * RootLayout icinde render edilir ki icindeki react-router <Link>'leri router
 * baglamina erissin (Provider RouterProvider'i sarmaladigi icin burada degil).
 */
export function AuthOverlayProvider({ children }: { children: ReactNode }) {
  const [isOpen, setIsOpen] = useState(false)
  const [activePanel, setActivePanel] = useState<AuthOverlayPanel>('login')

  const openLogin = useCallback(() => {
    setActivePanel('login')
    setIsOpen(true)
  }, [])

  const openRegister = useCallback(() => {
    setActivePanel('register')
    setIsOpen(true)
  }, [])

  const setPanel = useCallback((panel: AuthOverlayPanel) => {
    setActivePanel(panel)
  }, [])

  const close = useCallback(() => {
    setIsOpen(false)
  }, [])

  const value = useMemo<AuthOverlayContextValue>(
    () => ({
      isOpen,
      activePanel,
      openLogin,
      openRegister,
      setPanel,
      close,
    }),
    [isOpen, activePanel, openLogin, openRegister, setPanel, close],
  )

  return (
    <AuthOverlayContext.Provider value={value}>
      {children}
    </AuthOverlayContext.Provider>
  )
}

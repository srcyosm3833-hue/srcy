import { useContext } from 'react'
import {
  AuthOverlayContext,
  type AuthOverlayContextValue,
} from './AuthOverlayContext'

/**
 * Auth overlay context'ine erisim hook'u. AuthOverlayProvider disinda
 * cagrilirsa acik hata verir.
 */
export function useAuthOverlay(): AuthOverlayContextValue {
  const context = useContext(AuthOverlayContext)
  if (context === undefined) {
    throw new Error(
      'useAuthOverlay, bir <AuthOverlayProvider> icinde kullanilmalidir.',
    )
  }
  return context
}

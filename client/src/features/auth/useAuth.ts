import { useContext } from 'react'
import { AuthContext, type AuthContextValue } from './AuthContext'

/**
 * Auth context'ine erisim hook'u. AuthProvider disinda cagrilirsa acik hata verir.
 */
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth, bir <AuthProvider> icinde kullanilmalidir.')
  }
  return context
}

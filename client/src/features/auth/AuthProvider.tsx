import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import {
  authApi,
  clearTokens,
  getAccessToken,
  setTokens,
} from '@/lib/api'
import type {
  CurrentUser,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
} from '@/types'
import { AuthContext, type AuthContextValue } from './AuthContext'

/**
 * Auth durumunu yoneten Provider. Sorumluluklari:
 *  - Acilista: localStorage'da access token varsa /api/me ile kullaniciyi cozer.
 *  - login/register/logout fonksiyonlarini saglar.
 *  - Token saklama/temizlemeyi tek noktada yonetir.
 *
 * Not (iskelet asamasi): kullanici state'i burada useState ile tutulur. Ileride
 * TanStack Query ile (useQuery['me']) entegre edilebilir; sozlesme degismeden
 * ic implementasyon degistirilebilir.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [isInitializing, setIsInitializing] = useState<boolean>(true)

  // Acilista: token varsa kullaniciyi yukle. Yoksa dogrudan baslatmayi bitir.
  useEffect(() => {
    let cancelled = false

    async function bootstrap(): Promise<void> {
      if (!getAccessToken()) {
        setIsInitializing(false)
        return
      }
      try {
        const me = await authApi.getMe()
        if (!cancelled) {
          setUser(me)
        }
      } catch {
        // Token gecersiz/suresi dolmus ve refresh de basarisiz olduysa interceptor
        // zaten temizler; burada defansif olarak yerel state'i sifirliyoruz.
        if (!cancelled) {
          clearTokens()
          setUser(null)
        }
      } finally {
        if (!cancelled) {
          setIsInitializing(false)
        }
      }
    }

    void bootstrap()
    return () => {
      cancelled = true
    }
  }, [])

  const login = useCallback(async (payload: LoginRequest): Promise<void> => {
    const tokens = await authApi.login(payload)
    setTokens(tokens.accessToken, tokens.refreshToken)
    const me = await authApi.getMe()
    setUser(me)
  }, [])

  const register = useCallback(
    (payload: RegisterRequest): Promise<RegisterResponse> => {
      // Backend register'i token DONDURMEZ (yalniz id+email). Otomatik giris yapmiyoruz;
      // cagiran isterse ardindan login() cagirabilir.
      return authApi.register(payload)
    },
    [],
  )

  const logout = useCallback(async (): Promise<void> => {
    try {
      await authApi.logout()
    } finally {
      // Backend cagrisi basarisiz olsa bile istemci tarafinda cikis kesinlesir.
      clearTokens()
      setUser(null)
    }
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      isAdmin: user?.roles.includes('Admin') ?? false,
      isInitializing,
      login,
      register,
      logout,
    }),
    [user, isInitializing, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

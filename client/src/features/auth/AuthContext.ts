import { createContext } from 'react'
import type {
  CurrentUser,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
} from '@/types'

/**
 * Auth context'inin disa actigi sozlesme. Provider implementasyonu AuthProvider'da;
 * tuketim useAuth() hook'u uzerinden yapilir.
 */
export interface AuthContextValue {
  /** Gecerli kullanici (giris yapilmamissa null). */
  user: CurrentUser | null
  /** Elde gecerli bir oturum (access token) var mi. */
  isAuthenticated: boolean
  /** Kullanici Admin rolunde mi (korumali admin route'lari icin). */
  isAdmin: boolean
  /** Ilk yuklemede /api/me cozulurken true (uygulama acilis kontrolu). */
  isInitializing: boolean
  /** Giris yap: token'lari saklar ve kullaniciyi yukler. */
  login: (payload: LoginRequest) => Promise<void>
  /** Kayit ol (otomatik giris YAPMAZ; cagiran ayrica login cagirabilir). */
  register: (payload: RegisterRequest) => Promise<RegisterResponse>
  /** Cikis: backend'de refresh token revoke + yerel temizlik. */
  logout: () => Promise<void>
}

/**
 * undefined varsayilan: Provider disinda kullanim useAuth icinde yakalanir
 * (acik bir hata firlatilir).
 */
export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

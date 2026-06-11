import { apiClient, request } from './client'
import { getRefreshToken } from './tokenStorage'
import type {
  AuthTokensResponse,
  CurrentUser,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
} from '@/types'

/**
 * Kimlik dogrulama API cagrilarini tek yerde toplayan servis katmani.
 * Komponentler/hook'lar dogrudan axios'a degil bu fonksiyonlara baglanir.
 *
 * Endpoint'ler (Controllers/AuthController.cs, MeController.cs):
 *  POST /api/auth/register
 *  POST /api/auth/login
 *  POST /api/auth/refresh
 *  POST /api/auth/logout
 *  GET  /api/me
 */
export const authApi = {
  /** Yeni kullanici kaydi. 201 + { id, email } doner. */
  register(payload: RegisterRequest): Promise<RegisterResponse> {
    return request<RegisterResponse>({
      method: 'post',
      url: '/api/auth/register',
      data: payload,
    })
  },

  /** Giris. Access + refresh token cifti doner. */
  login(payload: LoginRequest): Promise<AuthTokensResponse> {
    return request<AuthTokensResponse>({
      method: 'post',
      url: '/api/auth/login',
      data: payload,
    })
  },

  /**
   * Cikis: elde bir refresh token varsa backend'de revoke ettirir (idempotent).
   * Token'larin localStorage'dan temizlenmesi cagiran (AuthProvider) sorumlulugundadir.
   */
  async logout(): Promise<void> {
    const refreshToken = getRefreshToken()
    if (!refreshToken) {
      return
    }
    // Logout basarisiz olsa bile istemci tarafinda cikis yapilabilmeli; bu yuzden
    // hata yutulmaz ama cagiran finally ile token temizligini garantiler.
    await apiClient.post('/api/auth/logout', { refreshToken })
  },

  /** Korumali: gecerli kullanicinin kimlik bilgileri (rol dahil). */
  getMe(): Promise<CurrentUser> {
    return request<CurrentUser>({ method: 'get', url: '/api/me' })
  },
}

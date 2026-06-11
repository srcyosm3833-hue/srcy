import axios, {
  AxiosError,
  type AxiosInstance,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios'
import { paths } from '@/routes/paths'
import type { AuthTokensResponse } from '@/types'
import {
  clearTokens,
  getAccessToken,
  getRefreshToken,
  setTokens,
} from './tokenStorage'

/**
 * Merkezi axios instance ve interceptor'lar.
 *
 * REQUEST: localStorage'daki access token Authorization: Bearer ... olarak eklenir.
 *
 * RESPONSE (401): access token suresi dolmus olabilir. refresh token ile
 *   /api/auth/refresh cagirilir; yeni cift saklanir ve ORIJINAL istek retry edilir.
 *   Refresh de basarisizsa token'lar temizlenir ve /login'e yonlendirilir.
 *
 * REFRESH STORM ONLEME: Es zamanli birden cok istek 401 alirsa, refresh YALNIZCA
 *   BIR KEZ yapilir. Devam eden bir refresh varsa diger istekler ayni promise'i
 *   bekler (single-flight). Boylece backend'e tek bir /refresh cagrisi gider ve
 *   token rotation yarisi (race) onlenir.
 */

const baseURL = import.meta.env.VITE_API_BASE_URL

/** Genel kullanim icin paylasilan instance. */
export const apiClient: AxiosInstance = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
})

/**
 * Interceptor'in kendi tetikledigi /auth/* isteklerinde sonsuz donguye girmemek
 * ve retry'i isaretlemek icin config'e eklenen ic bayraklar.
 */
interface RetriableConfig extends InternalAxiosRequestConfig {
  /** Bu istek bir kez retry edildi mi (ikinci 401'de tekrar refresh denenmesin). */
  _retry?: boolean
}

/** /api/auth/* yollari refresh akisinin disinda tutulur (sonsuz dongu engeli). */
function isAuthEndpoint(url: string | undefined): boolean {
  if (!url) return false
  return url.includes('/api/auth/')
}

// --- Request interceptor: Bearer token ekle ---
apiClient.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

// --- Single-flight refresh durumu ---
// Devam eden refresh'in promise'i. null ise aktif refresh yok.
let refreshPromise: Promise<string> | null = null

/**
 * Refresh token ile yeni access+refresh cifti alir, saklar ve yeni access token'i
 * dondurur. Es zamanli cagrilar ayni promise'i paylasir (single-flight).
 * Basarisizlikta token'lari temizler ve hata firlatir.
 */
function refreshAccessToken(): Promise<string> {
  if (refreshPromise) {
    return refreshPromise
  }

  refreshPromise = (async () => {
    const refreshToken = getRefreshToken()
    if (!refreshToken) {
      throw new Error('No refresh token available.')
    }

    try {
      // Onemli: interceptor'lara takilmamak icin saf axios.post kullaniyoruz
      // (apiClient degil). Boylece bu istek tekrar 401 interceptor'una girmez.
      const response = await axios.post<AuthTokensResponse>(
        `${baseURL}/api/auth/refresh`,
        { refreshToken },
        { headers: { 'Content-Type': 'application/json' } },
      )

      const { accessToken, refreshToken: newRefreshToken } = response.data
      setTokens(accessToken, newRefreshToken)
      return accessToken
    } catch (error) {
      // Refresh basarisiz: oturum gecersiz. Token'lari temizle.
      clearTokens()
      throw error
    } finally {
      // Promise'i serbest birak; sonraki 401'ler yeni bir refresh baslatabilsin.
      refreshPromise = null
    }
  })()

  return refreshPromise
}

/**
 * Oturum tam olarak gecersizlestiginde cagirilir: token'lari temizler ve
 * login sayfasina yonlendirir. Router disinda (interceptor) oldugumuz icin
 * basit bir window.location yonlendirmesi kullaniyoruz.
 */
function forceLogoutRedirect(): void {
  clearTokens()
  // Zaten login/register sayfasindaysak tekrar yonlendirme (gereksiz reload) yapma.
  const onAuthPage =
    window.location.pathname === paths.login ||
    window.location.pathname === paths.register
  if (!onAuthPage) {
    window.location.assign(paths.login)
  }
}

// --- Response interceptor: 401'de refresh + retry ---
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableConfig | undefined

    // Yalnizca: yanit var + 401 + config var + auth endpoint degil + henuz retry edilmedi.
    const shouldAttemptRefresh =
      error.response?.status === 401 &&
      originalRequest !== undefined &&
      !originalRequest._retry &&
      !isAuthEndpoint(originalRequest.url)

    if (!shouldAttemptRefresh) {
      return Promise.reject(error)
    }

    originalRequest._retry = true

    try {
      const newAccessToken = await refreshAccessToken()
      // Orijinal istegin Authorization basligini yeni token ile guncelle ve retry et.
      originalRequest.headers.set('Authorization', `Bearer ${newAccessToken}`)
      return apiClient(originalRequest)
    } catch (refreshError) {
      // Refresh basarisiz: oturumu sonlandir ve login'e yonlendir.
      forceLogoutRedirect()
      return Promise.reject(refreshError)
    }
  },
)

/**
 * Yardimci: bir isteyi tip guvenli sekilde yapip yalnizca response.data'yi doner.
 * Cogu servis fonksiyonu bunu kullanir.
 */
export async function request<T>(config: AxiosRequestConfig): Promise<T> {
  const response = await apiClient.request<T>(config)
  return response.data
}

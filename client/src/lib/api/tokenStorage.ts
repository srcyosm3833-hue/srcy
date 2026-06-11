/**
 * Token saklama katmani. KARAR (A2): access + refresh token localStorage'da tutulur.
 * XSS riski bilinerek alinmis bilincli bir kullanici tercihidir.
 *
 * Bu modul, depolama detayini (localStorage anahtarlari) tek bir yerde toplar;
 * uygulamanin geri kalani yalnizca buradaki fonksiyonlari cagirir. Boylece ileride
 * baska bir saklama stratejisine gecmek istenirse tek dosya degisir.
 */

const ACCESS_TOKEN_KEY = 'zn.accessToken'
const REFRESH_TOKEN_KEY = 'zn.refreshToken'

/** localStorage'daki access token (yoksa null). */
export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY)
}

/** localStorage'daki refresh token (yoksa null). */
export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY)
}

/** Access + refresh token ciftini birlikte saklar (login/refresh sonrasi). */
export function setTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
}

/** Her iki token'i da temizler (logout veya refresh basarisizliginda). */
export function clearTokens(): void {
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

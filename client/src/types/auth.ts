/**
 * Kimlik dogrulama (auth) sozlesmeleri. Alan adlari backend response/command
 * record'lariyla birebir (camelCase serialization).
 * Kaynaklar:
 *  - Features/Auth/Common/AuthTokensResponse.cs
 *  - Features/Auth/Login/LoginCommand.cs
 *  - Features/Auth/Register/RegisterCommand.cs, RegisterResponse.cs
 *  - Features/Auth/Refresh/RefreshTokenCommand.cs
 *  - Features/Auth/Logout/LogoutCommand.cs
 *  - Controllers/MeController.cs (CurrentUserResponse)
 */

/** POST /api/auth/login govdesi. */
export interface LoginRequest {
  email: string
  password: string
}

/** POST /api/auth/register govdesi. */
export interface RegisterRequest {
  firstName: string
  lastName: string
  email: string
  password: string
  /** Profil gorseli URL'i (zorunlu alan; bos string verilebilir). */
  imageUrl: string
}

/** POST /api/auth/register basarili yaniti (201). */
export interface RegisterResponse {
  id: string
  email: string
}

/** POST /api/auth/refresh govdesi. */
export interface RefreshRequest {
  refreshToken: string
}

/** POST /api/auth/logout govdesi. */
export interface LogoutRequest {
  refreshToken: string
}

/**
 * Login ve refresh ortak yaniti: access + refresh token cifti ve UTC son
 * gecerlilik anlari (ISO 8601 string). Refresh rotation'li oldugundan refresh
 * her cagrida YENI bir cift dondurur.
 */
export interface AuthTokensResponse {
  accessToken: string
  /** ISO 8601 UTC tarih-saat. */
  accessTokenExpiresAtUtc: string
  refreshToken: string
  /** ISO 8601 UTC tarih-saat. */
  refreshTokenExpiresAtUtc: string
}

/** GET /api/me yaniti: token'dan okunan kullanici kimligi. */
export interface CurrentUser {
  id: string | null
  email: string | null
  userName: string | null
  /** Kullanicinin rolleri (orn. ["Admin"], ["User"]). */
  roles: string[]
}

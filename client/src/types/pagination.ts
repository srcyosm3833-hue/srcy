/**
 * Backend PagedResult<T> sozlesmesinin TypeScript karsiligi.
 * Kaynak: Core/Zn.Application/Common/Pagination/PagedResult.cs
 * Serialization camelCase (System.Text.Json varsayilani).
 */
export interface PagedResult<T> {
  /** Gecerli sayfadaki ogeler. */
  items: T[]
  /** Tum sayfalardaki toplam kayit sayisi. */
  totalCount: number
  /** 1 tabanli gecerli sayfa numarasi. */
  page: number
  /** Sayfa basina oge sayisi. */
  pageSize: number
  /** Toplam sayfa sayisi. */
  totalPages: number
  /** Onceki sayfa var mi. */
  hasPreviousPage: boolean
  /** Sonraki sayfa var mi. */
  hasNextPage: boolean
}

/**
 * Admin kullanici yonetimi sozlesmeleri. Kaynaklar:
 *  - Features/Users/Common/UserResponse.cs (kullanici temsili)
 *  - Controllers/AdminUsersController.cs (listeleme + rol atama/kaldirma uclari)
 *
 * Yetki (A6): listeleme -> Admin + Manager; rol atama/kaldirma -> YALNIZ Admin.
 *  GET    /api/admin/users                       -> PagedResult<User> (page/pageSize/includeDeleted)
 *  GET    /api/admin/users/{id}                  -> User
 *  POST   /api/admin/users/{id}/roles            -> 200 User  (govde: { roleName }; idempotent)
 *  DELETE /api/admin/users/{id}/roles/{roleName} -> 204
 */

/** Admin kullanici listesindeki tek kullanici. */
export interface User {
  id: string
  firstName: string
  lastName: string
  email: string
  /** Profil gorseli yolu/URL'i. */
  imageUrl: string
  /** ISO 8601 UTC hesap olusturulma ani. */
  createdAt: string
  /** Kullanici soft delete edilmisse true. */
  isDeleted: boolean
  /** ISO 8601 UTC soft delete ani; aktif kullanicida null. */
  deletedAt: string | null
  /** Kullaniciya atanmis rollerin adlari. */
  roles: string[]
}

/** GET /api/admin/users sorgu parametreleri. */
export interface UserListQuery {
  page?: number
  pageSize?: number
  /** true ise soft delete edilmis kullanicilar da dahil edilir. */
  includeDeleted?: boolean
}

/** POST /api/admin/users/{id}/roles govdesi (rol atama). */
export interface AssignRoleRequest {
  roleName: string
}

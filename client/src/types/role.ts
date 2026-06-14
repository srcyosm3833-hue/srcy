/**
 * Rol yonetimi sozlesmeleri. Kaynaklar:
 *  - Features/Roles/Common/RoleResponse.cs (rol temsili)
 *  - Controllers/AdminRolesController.cs (CRUD uclari + UpdateRoleRequest)
 *  - Features/Roles/CreateRole/CreateRoleCommand.cs (POST govdesi)
 *
 * Tum rol yonetimi uclari YALNIZ Admin yetkisindedir (A6 matrisi).
 *  GET    /api/admin/roles       -> RoleResponse[]
 *  POST   /api/admin/roles       -> 201 RoleResponse   (govde: { name })
 *  PUT    /api/admin/roles/{id}  -> 200 RoleResponse   (govde: { name })
 *  DELETE /api/admin/roles/{id}  -> 204
 */

/** Rol yonetimi uclarinin dondurdugu rol temsili. */
export interface Role {
  id: string
  /** Rol adi (orn. "Admin"). */
  name: string
  /** Bu role atanmis kullanici sayisi. */
  userCount: number
  /**
   * Sistem tarafindan korunan rol mu (Admin/Manager/User). Korumali roller
   * guncellenemez/silinemez; frontend Duzenle/Sil butonlarini devre disi birakir.
   */
  isProtected: boolean
}

/** POST /api/admin/roles govdesi (yeni rol). */
export interface CreateRoleRequest {
  name: string
}

/** PUT /api/admin/roles/{id} govdesi (yeniden adlandirma). */
export interface UpdateRoleRequest {
  name: string
}

import { request } from './client'
import type { CreateRoleRequest, Role, UpdateRoleRequest } from '@/types'

/**
 * Admin rol yonetimi API cagrilari (hepsi YALNIZ Admin — A6 matrisi).
 *
 * Endpoint'ler (Controllers/AdminRolesController.cs):
 *  GET    /api/admin/roles       -> Role[]
 *  POST   /api/admin/roles       -> 201 Role   (govde: { name }; ayni ad 409)
 *  PUT    /api/admin/roles/{id}  -> 200 Role   (govde: { name }; korumali rol 400, cakisma 409)
 *  DELETE /api/admin/roles/{id}  -> 204        (korumali rol 400, kullanicili rol 409)
 */
export const roleApi = {
  /** Tum rolleri kullanici sayilariyla doner (admin). */
  getAll(): Promise<Role[]> {
    return request<Role[]>({
      method: 'get',
      url: '/api/admin/roles',
    })
  },

  /** Yeni ozel rol olusturur. 201 + rol temsili. */
  create(payload: CreateRoleRequest): Promise<Role> {
    return request<Role>({
      method: 'post',
      url: '/api/admin/roles',
      data: payload,
    })
  },

  /** Rolu yeniden adlandirir. 200 + guncel rol temsili. */
  update(id: string, payload: UpdateRoleRequest): Promise<Role> {
    return request<Role>({
      method: 'put',
      url: `/api/admin/roles/${id}`,
      data: payload,
    })
  },

  /** Rolu siler. Basarida govde donmez (204). */
  remove(id: string): Promise<void> {
    return request<void>({
      method: 'delete',
      url: `/api/admin/roles/${id}`,
    })
  },
}

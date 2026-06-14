import { request } from './client'
import type {
  AssignRoleRequest,
  PagedResult,
  User,
  UserListQuery,
} from '@/types'

/**
 * Admin kullanici yonetimi API cagrilari.
 *
 * Endpoint'ler (Controllers/AdminUsersController.cs):
 *  GET    /api/admin/users                       -> PagedResult<User> (Admin + Manager)
 *  POST   /api/admin/users/{id}/roles            -> 200 User          (YALNIZ Admin; idempotent)
 *  DELETE /api/admin/users/{id}/roles/{roleName} -> 204               (YALNIZ Admin; son Admin 400)
 *
 * Rol atama/kaldirma sonrasi backend kullanicinin guncel temsilini (rolleriyle) doner;
 * cagiran taraf kullanici listesini invalidate eder.
 */
export const userApi = {
  /** Kullanicilari sayfali doner (admin/manager). */
  getAll(query: UserListQuery = {}): Promise<PagedResult<User>> {
    return request<PagedResult<User>>({
      method: 'get',
      url: '/api/admin/users',
      params: {
        page: query.page,
        pageSize: query.pageSize,
        includeDeleted: query.includeDeleted,
      },
    })
  },

  /** Kullaniciya rol atar (yalniz Admin). Idempotent: zaten roldeyse de 200 doner. */
  assignRole(id: string, payload: AssignRoleRequest): Promise<User> {
    return request<User>({
      method: 'post',
      url: `/api/admin/users/${id}/roles`,
      data: payload,
    })
  },

  /** Kullanicidan rol kaldirir (yalniz Admin). Son Admin korumasi -> 400. 204 doner. */
  removeRole(id: string, roleName: string): Promise<void> {
    return request<void>({
      method: 'delete',
      url: `/api/admin/users/${id}/roles/${encodeURIComponent(roleName)}`,
    })
  },
}

import { request } from './client'
import type {
  CreateSocialMediaRequest,
  SocialMedia,
  UpdateSocialMediaRequest,
} from '@/types'

/**
 * Sosyal medya baglanti API cagrilari.
 *
 * Endpoint'ler:
 *  GET    /api/social-media            -> SocialMedia[] (public; kayit yoksa BOS DIZI)
 *  POST   /api/admin/social-media      -> 201 SocialMedia (Authorize: Admin)
 *  PUT    /api/admin/social-media/{id} -> 200 SocialMedia (Authorize: Admin)
 *  DELETE /api/admin/social-media/{id} -> 204 (Authorize: Admin)
 */
export const socialMediaApi = {
  /** Tum sosyal medya baglantilarini doner (bos olabilir). */
  getAll(): Promise<SocialMedia[]> {
    return request<SocialMedia[]>({ method: 'get', url: '/api/social-media' })
  },

  /** Yeni baglanti olusturur (admin). 201 + kayit. */
  create(payload: CreateSocialMediaRequest): Promise<SocialMedia> {
    return request<SocialMedia>({
      method: 'post',
      url: '/api/admin/social-media',
      data: payload,
    })
  },

  /** Baglantiyi gunceller (admin). 200 + guncel kayit. */
  update(id: string, payload: UpdateSocialMediaRequest): Promise<SocialMedia> {
    return request<SocialMedia>({
      method: 'put',
      url: `/api/admin/social-media/${id}`,
      data: payload,
    })
  },

  /** Baglantiyi siler (admin). Basarida 204. */
  remove(id: string): Promise<void> {
    return request<void>({
      method: 'delete',
      url: `/api/admin/social-media/${id}`,
    })
  },
}

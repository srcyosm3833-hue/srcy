/**
 * Sosyal medya baglantisi sozlesmeleri. Kaynaklar:
 *  - Features/SocialMedia/Common/SocialMediaResponse.cs
 *  - Features/SocialMedia/Create/CreateSocialMediaCommand.cs
 *  - Controllers/AdminSocialMediaController.cs (UpdateSocialMediaRequest)
 *
 * Public okuma:  GET /api/social-media  (kayit yoksa bos dizi)
 * Admin yazma:   POST/PUT/DELETE /api/admin/social-media  (Authorize: Admin)
 */
export interface SocialMedia {
  id: string
  /** Platform adi (Instagram, X, LinkedIn vb.). */
  title: string
  url: string
  /** Ikon CSS sinifi veya ikon dosya yolu. */
  icon: string
}

/** POST /api/admin/social-media govdesi. */
export interface CreateSocialMediaRequest {
  title: string
  url: string
  icon: string
}

/** PUT /api/admin/social-media/{id} govdesi. */
export interface UpdateSocialMediaRequest {
  title: string
  url: string
  icon: string
}

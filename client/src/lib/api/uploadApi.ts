import { request } from './client'
import type { UploadImageResponse } from '@/types'

/**
 * Gorsel yukleme API cagrisi.
 *
 * Endpoint (Controllers/UploadsController.cs):
 *  POST /api/uploads -> 201 { url } (Authorize)
 *
 * multipart/form-data; dosya alan adi "file". Izinli turler jpg/jpeg/png/webp,
 * maksimum 5 MB (sinirlar backend'de uygulanir; asimda 400 ValidationProblemDetails).
 *
 * NOT: apiClient varsayilan Content-Type'i application/json'dur. FormData icin bu
 * basligi acikca KALDIRIYORUZ (undefined) ki axios multipart sinir (boundary)
 * degerini kendisi uretsin. Authorization basligi request interceptor'unda eklenir.
 */
export const uploadApi = {
  /** Bir gorsel dosyasini yukler ve erisilebilir URL'ini doner. */
  upload(file: File): Promise<UploadImageResponse> {
    const formData = new FormData()
    formData.append('file', file)

    return request<UploadImageResponse>({
      method: 'post',
      url: '/api/uploads',
      data: formData,
      headers: { 'Content-Type': undefined },
    })
  },
}

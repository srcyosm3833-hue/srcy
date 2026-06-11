/**
 * Gorsel yukleme sozlesmesi. Kaynak: Features/Uploads/UploadImageResponse.cs
 * Endpoint: POST /api/uploads (multipart/form-data, "file" alani; Authorize)
 */
export interface UploadImageResponse {
  /** Statik dosya olarak servis edilen goreli URL (orn. "/uploads/abc.jpg"). */
  url: string
}

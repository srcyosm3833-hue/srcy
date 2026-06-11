/**
 * Iletisim formu mesaj sozlesmeleri. Kaynaklar:
 *  - Features/Messages/Common/MessageResponse.cs (admin listesi)
 *  - Features/Messages/Send/SendMessageCommand.cs (public gonderim govdesi)
 *  - Controllers/AdminMessagesController.cs (MarkMessageAsReadRequest)
 *
 * Public gonderim:  POST /api/messages
 * Admin listeleme:  GET  /api/admin/messages  (Authorize: Admin)
 * Okundu isaretle:  PATCH /api/admin/messages/{id}
 */

/** Admin mesaj kutusundaki tek mesaj. */
export interface Message {
  id: string
  name: string
  email: string
  subject: string
  messageBody: string
  isRead: boolean
  createdAt: string
}

/** POST /api/messages govdesi (iletisim formu). */
export interface SendMessageRequest {
  name: string
  email: string
  subject: string
  messageBody: string
}

/** PATCH /api/admin/messages/{id} govdesi. */
export interface MarkMessageAsReadRequest {
  isRead: boolean
}

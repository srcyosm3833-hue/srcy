/**
 * Yorum ve alt yorum (reply) sozlesmeleri. Kaynaklar:
 *  - Features/Comments/Common/CommentResponse.cs
 *  - Features/SubComments/Common/SubCommentResponse.cs
 *  - Controllers/CommentsController.cs, RepliesController.cs (request record'lari)
 *
 * Route yapisi:
 *  - Yorumlar:   /api/blogs/{blogId}/comments
 *  - Alt yorumlar: /api/comments/{commentId}/replies
 */

/** Bir bloga ait yorum. */
export interface Comment {
  id: string
  commentText: string
  userId: string
  /** Yazarin gorunen adi (FirstName + LastName). */
  displayName: string
  createdAt: string
  updatedAt: string | null
  /** En az bir kez duzenlendi mi (updatedAt != null). */
  isEdited: boolean
  /** Bu yoruma bagli alt yorum sayisi. */
  subCommentCount: number
}

/** Bir yoruma verilen yanit (alt yorum). */
export interface SubComment {
  id: string
  subCommentText: string
  commentId: string
  userId: string
  displayName: string
  createdAt: string
  updatedAt: string | null
  isEdited: boolean
}

/** POST /api/blogs/{blogId}/comments govdesi. */
export interface AddCommentRequest {
  commentText: string
}

/** PUT /api/blogs/{blogId}/comments/{id} govdesi. */
export interface UpdateCommentRequest {
  commentText: string
}

/**
 * Admin moderasyon listesi ogesi (yorum VEYA alt yorum birlikte).
 * Kaynak: Features/.../CommentModerationResponse.cs
 * Endpoint: GET /api/admin/comments (Authorize: Admin)
 *
 * isReply alanina gore silme route'u degisir:
 *  - isReply=false -> DELETE /api/blogs/{blogId}/comments/{id}
 *  - isReply=true  -> DELETE /api/comments/{parentCommentId}/replies/{id}
 */
export interface CommentModerationItem {
  id: string
  /** true ise alt yorum (reply); false ise ust seviye yorum. */
  isReply: boolean
  blogId: string
  blogTitle: string
  userId: string
  /** Yazarin gorunen adi. */
  authorName: string
  /** Yorum/yanit metni. */
  text: string
  createdAt: string
  /** isReply=true oldugunda dolu (ust yorum id'si); aksi halde null. */
  parentCommentId: string | null
}

/** POST /api/comments/{commentId}/replies govdesi. */
export interface AddReplyRequest {
  subCommentText: string
}

/** PUT /api/comments/{commentId}/replies/{id} govdesi. */
export interface UpdateReplyRequest {
  subCommentText: string
}

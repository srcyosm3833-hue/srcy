import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { replyApi } from '@/lib/api'
import type { AddReplyRequest, UpdateReplyRequest } from '@/types'
import { commentKeys } from '@/features/comment'

/** Alt yorum (reply) server-state hook'lari (TanStack Query). */
export const replyKeys = {
  all: ['replies'] as const,
  /** Bir ana yoruma ait tum yanit sorgulari (page bagimsiz prefix; invalidation icin). */
  byCommentAll: (commentId: string) =>
    [...replyKeys.all, 'byComment', commentId] as const,
  /** Bir ana yoruma ait yanit listesi (belirli sayfa). */
  byComment: (commentId: string, page: number) =>
    [...replyKeys.byCommentAll(commentId), page] as const,
}

/**
 * Bir yoruma ait yanitlar (sayfali). commentId yoksa veya `enabled=false` ise
 * sorgu calismaz (yanitlar yalnizca kullanici acinca lazy yuklenir).
 */
export function useReplies(
  commentId: string,
  page = 1,
  options: { enabled?: boolean } = {},
) {
  return useQuery({
    queryKey: replyKeys.byComment(commentId, page),
    queryFn: () => replyApi.getByCommentId(commentId, page),
    enabled: Boolean(commentId) && (options.enabled ?? true),
    placeholderData: (previous) => previous,
  })
}

/**
 * Ortak invalidation: yanit degisince (1) o yorumun yanit listesini ve
 * (2) ana yorumun subCommentCount'u guncellensin diye o bloga ait yorum
 * listesini tazeler.
 */
function invalidateReplyAndComments(
  queryClient: ReturnType<typeof useQueryClient>,
  blogId: string,
  commentId: string,
) {
  void queryClient.invalidateQueries({
    queryKey: replyKeys.byCommentAll(commentId),
  })
  void queryClient.invalidateQueries({
    queryKey: commentKeys.byBlogAll(blogId),
  })
}

/** Yanit ekleme. blogId, ana yorumun count'u icin yorum listesini tazelemekte kullanilir. */
export function useAddReply(blogId: string, commentId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: AddReplyRequest) => replyApi.add(commentId, payload),
    onSuccess: () => invalidateReplyAndComments(queryClient, blogId, commentId),
  })
}

/** Yanit guncelleme (yalnizca sahip). */
export function useUpdateReply(blogId: string, commentId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { replyId: string; payload: UpdateReplyRequest }) =>
      replyApi.update(commentId, vars.replyId, vars.payload),
    onSuccess: () => invalidateReplyAndComments(queryClient, blogId, commentId),
  })
}

/** Yanit silme (sahip veya admin). */
export function useDeleteReply(blogId: string, commentId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (replyId: string) => replyApi.remove(commentId, replyId),
    onSuccess: () => invalidateReplyAndComments(queryClient, blogId, commentId),
  })
}

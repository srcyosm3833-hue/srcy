import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { adminCommentApi, commentApi } from '@/lib/api'
import type {
  AddCommentRequest,
  CommentModerationItem,
  UpdateCommentRequest,
} from '@/types'

/** Yorum server-state hook'lari (TanStack Query). */
export const commentKeys = {
  all: ['comments'] as const,
  /** Bir bloga ait tum yorum sorgulari (page bagimsiz prefix; invalidation icin). */
  byBlogAll: (blogId: string) =>
    [...commentKeys.all, 'byBlog', blogId] as const,
  /** Bir bloga ait yorum listesi (belirli sayfa). */
  byBlog: (blogId: string, page: number) =>
    [...commentKeys.byBlogAll(blogId), page] as const,
}

/** Admin yorum moderasyonu query key'leri (public yorum key'lerinden ayri). */
export const adminCommentKeys = {
  all: ['adminComments'] as const,
  /** Sayfali moderasyon listesi (belirli sayfa). */
  list: (page: number, pageSize: number) =>
    [...adminCommentKeys.all, 'list', page, pageSize] as const,
}

/** Bir bloga ait yorumlar (sayfali). blogId yoksa sorgu calismaz. */
export function useComments(blogId: string | undefined, page = 1) {
  return useQuery({
    queryKey: commentKeys.byBlog(blogId ?? '', page),
    queryFn: () => commentApi.getByBlogId(blogId as string, page),
    enabled: Boolean(blogId),
    placeholderData: (previous) => previous,
  })
}

/**
 * Yorum ekleme. Basarida o bloga ait TUM yorum sayfalarini invalidate eder
 * (yeni yorum en yeni listede gorunsun + sayim guncellensin).
 */
export function useAddComment(blogId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: AddCommentRequest) => commentApi.add(blogId, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: commentKeys.byBlogAll(blogId),
      })
    },
  })
}

/** Yorum guncelleme (yalnizca sahip). Basarida yorum listesini tazeler. */
export function useUpdateComment(blogId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { commentId: string; payload: UpdateCommentRequest }) =>
      commentApi.update(blogId, vars.commentId, vars.payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: commentKeys.byBlogAll(blogId),
      })
    },
  })
}

/** Yorum silme (sahip veya admin). Basarida yorum listesini tazeler. */
export function useDeleteComment(blogId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (commentId: string) => commentApi.remove(blogId, commentId),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: commentKeys.byBlogAll(blogId),
      })
    },
  })
}

/**
 * Admin moderasyon listesi (sayfali; yorum + yanit birlikte). Yalnizca Admin
 * erisebilir (Manager 403 alir). Sayfa gecisinde onceki veri korunur.
 */
export function useAdminComments(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: adminCommentKeys.list(page, pageSize),
    queryFn: () => adminCommentApi.getCommentsForAdmin(page, pageSize),
    placeholderData: (previous) => previous,
  })
}

/**
 * Moderasyon silme (admin). Verilen ogenin isReply bayragina gore dogru ucu secer:
 * yorum -> /api/blogs/{blogId}/comments/{id}, yanit -> /api/comments/{parentId}/replies/{id}.
 * Basarida hem moderasyon listesini hem de public yorum listelerini tazeler.
 */
export function useDeleteCommentModeration() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (item: CommentModerationItem) =>
      item.isReply
        ? adminCommentApi.deleteReply(item.parentCommentId ?? '', item.id)
        : adminCommentApi.deleteComment(item.blogId, item.id),
    onSuccess: (_data, item) => {
      void queryClient.invalidateQueries({ queryKey: adminCommentKeys.all })
      // Public yorum listeleri de senkron kalsin (ilgili blogun yorumlari).
      void queryClient.invalidateQueries({
        queryKey: commentKeys.byBlogAll(item.blogId),
      })
    },
  })
}

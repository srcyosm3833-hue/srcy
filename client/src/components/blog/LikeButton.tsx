import { useLocation, useNavigate } from 'react-router-dom'
import { Heart } from 'lucide-react'
import { toast } from 'sonner'

import { useToggleBlogLike } from '@/features/blog'
import { useAuth } from '@/features/auth'
import { normalizeApiError } from '@/lib/api'
import { paths } from '@/routes/paths'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'

interface LikeButtonProps {
  /** Begenisi toggle edilecek blogun kimligi. */
  blogId: string
  /** Blogun guncel toplam begeni sayisi (detay query'sinden). */
  likeCount: number
  /** Mevcut kullanici bu blogu begenmis mi (anonimde false). */
  isLiked: boolean
}

/**
 * Blog detayindaki begeni butonu (kalp ikonu + sayi). Giris yapmis kullanici
 * tiklayinca POST /api/blogs/{id}/like ile toggle edilir (optimistic update
 * hook icinde). Anonim kullanici tiklayinca login'e yonlendirilir; giristen
 * sonra ayni blog detayina geri doner (location.state.from).
 *
 * likeCount/isLiked degerleri detay query cache'inden gelir; toggle sonrasi hook
 * cache'i gunceller, boylece bu buton prop'lar uzerinden otomatik tazelenir.
 */
export function LikeButton({ blogId, likeCount, isLiked }: LikeButtonProps) {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const toggleLike = useToggleBlogLike(blogId)

  function handleClick() {
    // Anonim: begeni icin once giris -> login sonrasi bu sayfaya don.
    if (!isAuthenticated) {
      navigate(paths.login, { state: { from: location } })
      return
    }

    toggleLike.mutate(undefined, {
      onError: (error) => {
        const normalized = normalizeApiError(error)
        toast.error('Beğeni işlemi başarısız.', {
          description: normalized.message,
        })
      },
    })
  }

  const liked = isAuthenticated && isLiked

  return (
    <Button
      type="button"
      variant="outline"
      onClick={handleClick}
      disabled={toggleLike.isPending}
      aria-pressed={liked}
      aria-label={
        !isAuthenticated
          ? 'Beğenmek için giriş yapın'
          : liked
            ? 'Beğeniyi kaldır'
            : 'Beğen'
      }
      title={!isAuthenticated ? 'Beğenmek için giriş yapın' : undefined}
      className={cn(liked && 'border-rose-200 text-rose-600 hover:text-rose-700')}
    >
      <Heart
        className={cn('h-4 w-4 transition-colors', liked && 'fill-current')}
        aria-hidden
      />
      <span className="tabular-nums">{likeCount}</span>
      <span className="sr-only">beğeni</span>
    </Button>
  )
}

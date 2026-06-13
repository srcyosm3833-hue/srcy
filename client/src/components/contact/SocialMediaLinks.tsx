import { ExternalLink } from 'lucide-react'

import { useSocialMedia } from '@/features/contact'
import { Skeleton } from '@/components/ui/skeleton'

interface SocialMediaLinksProps {
  /**
   * Kompakt mod (footer): yalnizca platform adlari, baslik gosterilmez, hata/bos
   * durumda sessizce hicbir sey render etmez. Iletisim sayfasinda (false) liste +
   * yukleme iskeleti gosterilir.
   */
  compact?: boolean
}

/**
 * Sosyal medya baglantilari (GET /api/social-media). Kayit yoksa BOS DIZI gelir
 * (404 degil). icon alani CSS sinifi/dosya yolu olabildiginden guvenilir degil;
 * bu yuzden notr bir dis baglanti ikonu + platform adi gosterilir.
 *
 * Hata durumu kullanici akisini bloklamaz: iletisim sayfasinin yan bilgisidir,
 * bu yuzden hata/bos durumda sessizce gizlenir (footer'da da ayni).
 */
export function SocialMediaLinks({ compact = false }: SocialMediaLinksProps) {
  const socialQuery = useSocialMedia()

  if (socialQuery.isPending) {
    if (compact) return null
    return (
      <div className="flex flex-wrap gap-2">
        <Skeleton className="h-9 w-24" />
        <Skeleton className="h-9 w-24" />
        <Skeleton className="h-9 w-24" />
      </div>
    )
  }

  // Hata veya bos liste. Footer (compact): sessizce gizle. Iletisim sayfasi:
  // kartin bos kalmamasi icin zarif bir placeholder goster.
  if (socialQuery.isError || socialQuery.data.length === 0) {
    if (compact) return null
    return (
      <p className="text-sm text-muted-foreground">
        Henüz sosyal medya bağlantısı eklenmemiş.
      </p>
    )
  }

  if (compact) {
    return (
      <ul className="mt-3 space-y-2 text-sm">
        {socialQuery.data.map((item) => (
          <li key={item.id}>
            <a
              href={item.url}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 text-muted-foreground transition-colors hover:text-foreground"
            >
              <ExternalLink className="h-3.5 w-3.5" />
              {item.title}
            </a>
          </li>
        ))}
      </ul>
    )
  }

  return (
    <div className="flex flex-wrap gap-2">
      {socialQuery.data.map((item) => (
        <a
          key={item.id}
          href={item.url}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-2 rounded-md border border-border px-3 py-2 text-sm font-medium text-foreground transition-colors hover:bg-accent/10"
        >
          <ExternalLink className="h-4 w-4 text-accent" />
          {item.title}
        </a>
      ))}
    </div>
  )
}

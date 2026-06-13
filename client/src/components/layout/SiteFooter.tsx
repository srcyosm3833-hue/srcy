import { Link } from 'react-router-dom'

import { paths } from '@/routes/paths'
import { SocialMediaLinks } from '@/components/contact/SocialMediaLinks'

/**
 * Public site alt bilgisi: logo + kisa aciklama + hizli linkler + sosyal medya + telif.
 * Sosyal medya baglantilari GET /api/social-media'dan compact modda cekilir;
 * kayit yoksa/hata olursa o blok sessizce gizlenir (footer akisini bozmaz).
 */
export function SiteFooter() {
  const currentYear = new Date().getFullYear()

  return (
    <footer className="border-t border-border bg-secondary/30">
      <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
          {/* Marka + aciklama */}
          <div>
            <Link
              to={paths.home}
              className="font-heading text-xl font-bold text-foreground"
            >
              Zn<span className="text-accent">Blog</span>
            </Link>
            <p className="mt-3 max-w-xs text-sm text-muted-foreground">
              Düşünceler, yazılar, hikayeler. Okumayı sevenler için modern bir
              blog platformu.
            </p>
          </div>

          {/* Hizli linkler */}
          <nav aria-label="Alt bilgi navigasyonu">
            <h2 className="font-sans text-sm font-semibold text-foreground">
              Keşfet
            </h2>
            <ul className="mt-3 space-y-2 text-sm">
              <li>
                <Link
                  to={paths.home}
                  className="text-muted-foreground transition-colors hover:text-foreground"
                >
                  Anasayfa
                </Link>
              </li>
              <li>
                <Link
                  to={paths.blogs}
                  className="text-muted-foreground transition-colors hover:text-foreground"
                >
                  Bloglar
                </Link>
              </li>
              <li>
                <Link
                  to={paths.contact}
                  className="text-muted-foreground transition-colors hover:text-foreground"
                >
                  İletişim
                </Link>
              </li>
            </ul>
          </nav>

          {/* Sosyal medya (GET /api/social-media; bos/hata durumunda gizlenir) */}
          <div>
            <h2 className="font-sans text-sm font-semibold text-foreground">
              Bizi Takip Edin
            </h2>
            <SocialMediaLinks compact />
          </div>
        </div>

        <div className="mt-10 border-t border-border pt-6">
          <p className="text-center text-xs text-muted-foreground">
            © {currentYear} ZnBlog. Tüm hakları saklıdır.
          </p>
        </div>
      </div>
    </footer>
  )
}

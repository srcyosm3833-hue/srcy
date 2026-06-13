import { useState } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { LogOut, Menu, PenLine, ShieldCheck } from 'lucide-react'

import { useAuth } from '@/features/auth'
import { paths } from '@/routes/paths'
import { cn } from '@/lib/utils'
import { getInitials } from '@/lib/initials'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet'
import { ThemeToggle } from '@/components/theme/ThemeToggle'

/** Ana navigasyon linkleri (tek dogruluk kaynagi; desktop + mobil ayni listeyi kullanir). */
const navLinks: { to: string; label: string; end?: boolean }[] = [
  { to: paths.home, label: 'Anasayfa', end: true },
  { to: paths.blogs, label: 'Bloglar' },
  { to: paths.contact, label: 'İletişim' },
]

/** Aktif/pasif NavLink stilleri (desktop). */
function desktopNavClass({ isActive }: { isActive: boolean }): string {
  return cn(
    'text-sm font-medium transition-colors hover:text-foreground',
    isActive ? 'text-foreground' : 'text-muted-foreground',
  )
}

/** Logo (Link to home). Playfair baslik fontu + amber vurgu. */
function Logo({ onClick }: { onClick?: () => void }) {
  return (
    <Link
      to={paths.home}
      onClick={onClick}
      className="font-heading text-xl font-bold tracking-tight text-foreground"
    >
      Zn<span className="text-accent">Blog</span>
    </Link>
  )
}

/**
 * Public site basligi. Auth durumuna gore sag taraf degisir:
 *  - isInitializing -> Skeleton (layout kaymasini onlemek icin sabit boyut)
 *  - anonim         -> Giris + Kayit butonlari
 *  - giris yapilmis -> avatar dropdown (cikis); admin ise ek "Admin Panel" linki
 */
export function SiteHeader() {
  const { user, isAuthenticated, isAdmin, isInitializing, logout } = useAuth()
  const navigate = useNavigate()
  const [mobileOpen, setMobileOpen] = useState(false)

  async function handleLogout() {
    await logout()
    navigate(paths.home)
  }

  return (
    <header className="sticky top-0 z-40 w-full border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        {/* Sol: logo + desktop nav */}
        <div className="flex items-center gap-8">
          <Logo />
          <nav
            aria-label="Ana navigasyon"
            className="hidden items-center gap-6 md:flex"
          >
            {navLinks.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                end={link.end}
                className={desktopNavClass}
              >
                {link.label}
              </NavLink>
            ))}
          </nav>
        </div>

        {/* Sag: tema + auth alani */}
        <div className="flex items-center gap-2">
          <ThemeToggle />

          {/* Auth durumu (desktop) */}
          <div className="hidden items-center gap-2 md:flex">
            {isInitializing ? (
              <Skeleton className="h-9 w-24" />
            ) : isAuthenticated ? (
              <AuthenticatedNav
                isAdmin={isAdmin}
                userName={user?.userName}
                email={user?.email}
                onLogout={handleLogout}
              />
            ) : (
              <AnonymousNav />
            )}
          </div>

          {/* Mobil menu tetikleyici */}
          <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
            <SheetTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                className="md:hidden"
                aria-label="Menuyu ac"
              >
                <Menu className="h-5 w-5" />
              </Button>
            </SheetTrigger>
            <SheetContent side="left" className="w-72">
              <SheetHeader>
                <SheetTitle className="text-left">
                  <Logo onClick={() => setMobileOpen(false)} />
                </SheetTitle>
              </SheetHeader>

              <nav
                aria-label="Mobil navigasyon"
                className="mt-6 flex flex-col gap-1"
              >
                {navLinks.map((link) => (
                  <SheetClose asChild key={link.to}>
                    <NavLink
                      to={link.to}
                      end={link.end}
                      className={({ isActive }) =>
                        cn(
                          'rounded-md px-3 py-2 text-sm font-medium transition-colors',
                          isActive
                            ? 'bg-secondary text-foreground'
                            : 'text-muted-foreground hover:bg-secondary/60',
                        )
                      }
                    >
                      {link.label}
                    </NavLink>
                  </SheetClose>
                ))}
              </nav>

              <div className="mt-6 border-t border-border pt-6">
                {isInitializing ? (
                  <Skeleton className="h-9 w-full" />
                ) : isAuthenticated ? (
                  <div className="flex flex-col gap-2">
                    {user?.email ? (
                      <p className="px-3 text-xs text-muted-foreground">
                        {user.email}
                      </p>
                    ) : null}
                    {isAdmin ? (
                      <SheetClose asChild>
                        <Button asChild variant="ghost" className="justify-start">
                          <Link to={paths.admin}>
                            <ShieldCheck className="h-4 w-4" />
                            Admin Panel
                          </Link>
                        </Button>
                      </SheetClose>
                    ) : null}
                    <Button
                      variant="outline"
                      className="justify-start"
                      onClick={() => {
                        setMobileOpen(false)
                        void handleLogout()
                      }}
                    >
                      <LogOut className="h-4 w-4" />
                      Çıkış Yap
                    </Button>
                  </div>
                ) : (
                  <div className="flex flex-col gap-2">
                    <SheetClose asChild>
                      <Button asChild variant="outline">
                        <Link to={paths.login}>Giriş Yap</Link>
                      </Button>
                    </SheetClose>
                    <SheetClose asChild>
                      <Button asChild>
                        <Link to={paths.register}>Kayıt Ol</Link>
                      </Button>
                    </SheetClose>
                  </div>
                )}
              </div>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </header>
  )
}

/** Anonim kullanici: Giris (outline) + Kayit (primary). */
function AnonymousNav() {
  return (
    <>
      <Button asChild variant="ghost" size="sm">
        <Link to={paths.login}>Giriş Yap</Link>
      </Button>
      <Button asChild size="sm">
        <Link to={paths.register}>Kayıt Ol</Link>
      </Button>
    </>
  )
}

interface AuthenticatedNavProps {
  isAdmin: boolean
  userName: string | null | undefined
  email: string | null | undefined
  onLogout: () => void
}

/** Giris yapmis kullanici: (admin ise) Admin Panel linki + avatar dropdown. */
function AuthenticatedNav({
  isAdmin,
  userName,
  email,
  onLogout,
}: AuthenticatedNavProps) {
  return (
    <>
      {isAdmin ? (
        <Button asChild variant="ghost" size="sm">
          <Link to={paths.admin}>
            <ShieldCheck className="h-4 w-4" />
            Admin Panel
          </Link>
        </Button>
      ) : null}

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            className="rounded-full"
            aria-label="Kullanıcı menüsü"
          >
            <Avatar className="h-8 w-8">
              <AvatarImage src={undefined} alt="" />
              <AvatarFallback>{getInitials(userName, email)}</AvatarFallback>
            </Avatar>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-56">
          <DropdownMenuLabel className="font-normal">
            <p className="text-sm font-medium">Hesabım</p>
            {email ? (
              <p className="truncate text-xs text-muted-foreground">{email}</p>
            ) : null}
          </DropdownMenuLabel>
          <DropdownMenuSeparator />
          {isAdmin ? (
            <DropdownMenuItem asChild>
              <Link to={paths.admin}>
                <PenLine className="h-4 w-4" />
                Admin Panel
              </Link>
            </DropdownMenuItem>
          ) : null}
          <DropdownMenuItem onSelect={onLogout} className="text-destructive">
            <LogOut className="h-4 w-4" />
            Çıkış Yap
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </>
  )
}

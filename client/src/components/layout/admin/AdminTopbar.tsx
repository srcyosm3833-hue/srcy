import { useState } from 'react'
import { Menu } from 'lucide-react'

import { useAuth } from '@/features/auth'
import { getInitials } from '@/lib/initials'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Sheet, SheetContent, SheetTrigger } from '@/components/ui/sheet'
import { ThemeToggle } from '@/components/theme/ThemeToggle'
import { AdminSidebar } from './AdminSidebar'

interface AdminTopbarProps {
  /** Gecerli sayfa basligi (her admin sayfasi kendi basligini verir). */
  title: string
}

/**
 * Admin ust cubugu. Mobilde (< lg) hamburger ile sidebar'i Sheet olarak acar;
 * tum genisliklerde sayfa basligini, tema anahtarini ve kullanici avatarini gosterir.
 */
export function AdminTopbar({ title }: AdminTopbarProps) {
  const { user } = useAuth()
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center gap-4 border-b border-border bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/80 sm:px-6">
      {/* Mobil sidebar tetikleyici */}
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            className="lg:hidden"
            aria-label="Menüyü aç"
          >
            <Menu className="h-5 w-5" />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="w-72 p-0">
          <AdminSidebar onNavigate={() => setMobileOpen(false)} />
        </SheetContent>
      </Sheet>

      <h1 className="flex-1 truncate text-lg font-semibold text-foreground">
        {title}
      </h1>

      <div className="flex items-center gap-2">
        <ThemeToggle />
        <Avatar className="h-8 w-8">
          <AvatarFallback>
            {getInitials(user?.userName, user?.email)}
          </AvatarFallback>
        </Avatar>
      </div>
    </header>
  )
}

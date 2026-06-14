import { Link, NavLink, useNavigate } from 'react-router-dom'
import {
  FileText,
  LayoutDashboard,
  LogOut,
  Mail,
  MessageSquare,
  Phone,
  Search,
  Share2,
  Shield,
  Tag,
  Users,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

import { paths } from '@/routes/paths'
import { cn } from '@/lib/utils'
import { useAuth } from '@/features/auth'
import { useUnreadMessageCount } from '@/features/message'
import { Button } from '@/components/ui/button'

/** Tek bir sidebar navigasyon ogesinin tanimi. */
interface AdminNavItem {
  to: string
  label: string
  icon: LucideIcon
  /** Yalnizca tam eslemede aktif (orn. Dashboard `/admin`). */
  end?: boolean
  /**
   * true ise oge yalnizca Admin rolune gosterilir (Manager'a gizlenir). Kullanici/rol
   * yonetimi gibi yalniz-Admin islemler icin (A6 yetki matrisi). Verilmezse icerik
   * yonetimi sayilir ve hem Admin hem Manager gorur.
   */
  adminOnly?: boolean
}

const navItems: AdminNavItem[] = [
  { to: paths.admin, label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: paths.adminBlogs, label: 'Bloglar', icon: FileText },
  { to: paths.adminCategories, label: 'Kategoriler', icon: Tag },
  // Yorum moderasyonu backend'de yalniz Admin'e acik (Manager 403 alir) -> adminOnly.
  {
    to: paths.adminComments,
    label: 'Yorumlar',
    icon: MessageSquare,
    adminOnly: true,
  },
  { to: paths.adminMessages, label: 'Mesajlar', icon: Mail },
  // Iletisim guncelleme (PUT /api/admin/contact) yalniz Admin -> adminOnly.
  {
    to: paths.adminContact,
    label: 'İletişim',
    icon: Phone,
    adminOnly: true,
  },
  { to: paths.adminSocialMedia, label: 'Sosyal Medya', icon: Share2 },
  // Kullanici listeleme Admin + Manager'a acik (rol atama aksiyonu sayfa icinde
  // yalniz Admin'e gosterilir) -> adminOnly DEGIL.
  { to: paths.adminUsers, label: 'Kullanıcılar', icon: Users },
  // Rol yonetimi yalniz Admin (A6 matrisi) -> adminOnly.
  { to: paths.adminRoles, label: 'Rol Yönetimi', icon: Shield, adminOnly: true },
  // Arama loglari KVKK kapsaminda; yalniz Admin (A-AU5) -> adminOnly.
  {
    to: paths.adminSearchLogs,
    label: 'Arama Logları',
    icon: Search,
    adminOnly: true,
  },
]

interface AdminSidebarProps {
  /** Mobil cekmecede bir linke tiklaninca cekmeceyi kapatmak icin (opsiyonel). */
  onNavigate?: () => void
}

/**
 * Admin sidebar icerigi. Hem masaustu sabit kolon hem mobil Sheet icinde ayni
 * bilesen kullanilir. Aktif route NavLink ile vurgulanir; Mesajlar ogesinde
 * okunmamis mesaj rozeti gosterilir. Alt kisimda kullanici e-postasi + cikis.
 */
export function AdminSidebar({ onNavigate }: AdminSidebarProps) {
  const { user, isAdmin, logout } = useAuth()
  const { unreadCount } = useUnreadMessageCount()
  const navigate = useNavigate()

  // Yalniz-Admin ogeleri Manager'a gizle; icerik yonetimi ogeleri her ikisine acik.
  const visibleNavItems = navItems.filter((item) => !item.adminOnly || isAdmin)

  async function handleLogout() {
    onNavigate?.()
    await logout()
    // Cikis: logout state'i temizler. SPA ici yonlendirme ile ana sayfaya don
    // (ProtectedRoute artik admin'i koruyamayacagindan zaten erisim kesilir).
    navigate(paths.home)
  }

  return (
    <div className="flex h-full flex-col bg-sidebar text-sidebar-foreground">
      {/* Logo */}
      <div className="flex h-16 items-center border-b border-sidebar-border px-6">
        <Link
          to={paths.home}
          onClick={onNavigate}
          className="font-heading text-xl font-bold tracking-tight text-sidebar-foreground"
        >
          Zn<span className="text-accent">Blog</span>
          <span className="ml-2 align-middle text-xs font-normal text-sidebar-foreground/60">
            Yönetim
          </span>
        </Link>
      </div>

      {/* Navigasyon */}
      <nav aria-label="Yönetim navigasyonu" className="flex-1 space-y-1 p-3">
        {visibleNavItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            onClick={onNavigate}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-ring',
                isActive
                  ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                  : 'text-sidebar-foreground/70 hover:bg-sidebar-accent/60 hover:text-sidebar-foreground',
              )
            }
          >
            <item.icon className="h-4 w-4 shrink-0" />
            <span className="flex-1">{item.label}</span>
            {item.to === paths.adminMessages && unreadCount > 0 ? (
              <span
                className="inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-accent px-1.5 text-xs font-semibold text-accent-foreground"
                aria-label={`${unreadCount} okunmamış mesaj`}
              >
                {unreadCount}
              </span>
            ) : null}
          </NavLink>
        ))}
      </nav>

      {/* Alt: kullanici + cikis */}
      <div className="border-t border-sidebar-border p-3">
        {user?.email ? (
          <p className="truncate px-3 pb-2 text-xs text-sidebar-foreground/60">
            {user.email}
          </p>
        ) : null}
        <Button
          variant="ghost"
          className="w-full justify-start text-sidebar-foreground/80 hover:bg-sidebar-accent hover:text-sidebar-foreground"
          onClick={handleLogout}
        >
          <LogOut className="h-4 w-4" />
          Çıkış Yap
        </Button>
      </div>
    </div>
  )
}

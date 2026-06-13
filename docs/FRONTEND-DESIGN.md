# ZnBlogApp — Frontend Tasarım Sistemi ve Sayfa Planı

> Hazırlayan: UI/UX Designer Agent  
> Tarih: 11 Haziran 2026  
> Hedef: React Frontend Developer'ın doğrudan implementasyona başlayabileceği netlikte tasarım spesifikasyonu  
> Kapsam: shadcn/ui + Tailwind CSS v4, React Router, TanStack Query, mevcut iskelet üzerine

---

## İçindekiler

1. [Tasarım Sistemi](#1-tasarım-sistemi)
2. [Ortak Layout Planı](#2-ortak-layout-planı)
3. [Sayfa Planları](#3-sayfa-planları)
   - 3.1 Public — Anasayfa
   - 3.2 Public — Blog Listesi
   - 3.3 Public — Blog Detay
   - 3.4 Public — Login
   - 3.5 Public — Register
   - 3.6 Public — İletişim
   - 3.7 Admin — Dashboard
   - 3.8 Admin — Blog Yönetimi
   - 3.9 Admin — Kategori Yönetimi
   - 3.10 Admin — Mesaj Kutusu
   - 3.11 Admin — Sosyal Medya Yönetimi
4. [Komponent Envanteri](#4-komponent-envanteri)
5. [Uygulama Sırası](#5-uygulama-sırası)

---

## 1. Tasarım Sistemi

### 1.1 Renk Paleti — HSL Token'ları (Light + Dark)

**Marka Rengi Seçimi: Slate-700 tabanlı koyu mavi-gri primary**

Gerekçe: Blog uygulaması için güvenilir, entelektüel ve modern bir his verir. Violet/mor hem çok yoğun hem de blog içeriğiyle rekabet eder (mevcut placeholder'da violet-600 kullanılmış ama bu iskelet rengidir, değiştirilmeli). Slate primary; uzun metinle çalışan okuyucu için göz yormaz, kategoriler ve badge'ler için accent rengi yer bırakır.

**Accent Rengi: Amber** — öne çıkan içerik, featured badge, okunmamış mesaj vurgusu için.

shadcn/ui CSS değişken sistemi Tailwind v4'te `@theme` bloğuyla entegre edilir. `client/src/index.css` dosyasına aşağıdaki token'lar eklenir:

```css
/* client/src/index.css */
@import 'tailwindcss';

@layer base {
  :root {
    /* Zemin ve ön plan */
    --background: 0 0% 100%;          /* #ffffff */
    --foreground: 222 47% 11%;        /* #0f172a — slate-900 */

    /* Kart / yükseltilmiş yüzey */
    --card: 0 0% 100%;
    --card-foreground: 222 47% 11%;

    /* Popover */
    --popover: 0 0% 100%;
    --popover-foreground: 222 47% 11%;

    /* Birincil — Slate 700 */
    --primary: 217 33% 17%;           /* #1e293b */
    --primary-foreground: 210 40% 98%; /* #f8fafc */

    /* İkincil — Slate 100 */
    --secondary: 210 40% 96%;         /* #f1f5f9 */
    --secondary-foreground: 222 47% 11%;

    /* Soluk / yardımcı metin */
    --muted: 210 40% 96%;             /* #f1f5f9 */
    --muted-foreground: 215 16% 47%;  /* #64748b — slate-500 */

    /* Vurgu — Amber 500 */
    --accent: 43 96% 56%;             /* #f59e0b */
    --accent-foreground: 222 47% 11%;

    /* Hata/tehlike */
    --destructive: 0 84% 60%;         /* #ef4444 */
    --destructive-foreground: 0 0% 98%;

    /* Kenarlık */
    --border: 214 32% 91%;            /* #e2e8f0 — slate-200 */
    --input: 214 32% 91%;
    --ring: 217 33% 17%;              /* primary ile aynı */

    /* Radius */
    --radius: 0.5rem;                 /* 8px */

    /* Blog-specific: öne çıkan içerik şeridi */
    --featured: 43 96% 56%;           /* amber-500 */
    --featured-foreground: 222 47% 11%;

    /* Sidebar (admin) */
    --sidebar-background: 222 47% 11%;         /* slate-900 */
    --sidebar-foreground: 210 40% 98%;          /* slate-50 */
    --sidebar-border: 217 33% 17%;             /* slate-800 */
    --sidebar-accent: 217 19% 27%;             /* slate-700 */
    --sidebar-accent-foreground: 210 40% 98%;
    --sidebar-ring: 43 96% 56%;               /* amber vurgu */
  }

  .dark {
    --background: 222 47% 11%;        /* #0f172a */
    --foreground: 210 40% 98%;        /* #f8fafc */

    --card: 217 33% 17%;              /* #1e293b */
    --card-foreground: 210 40% 98%;

    --popover: 217 33% 17%;
    --popover-foreground: 210 40% 98%;

    --primary: 210 40% 98%;           /* light modda dark olan; dark'ta açık */
    --primary-foreground: 222 47% 11%;

    --secondary: 217 19% 27%;         /* #334155 — slate-700 */
    --secondary-foreground: 210 40% 98%;

    --muted: 217 19% 27%;
    --muted-foreground: 215 20% 65%;  /* #94a3b8 — slate-400 */

    --accent: 43 96% 56%;             /* amber korunur */
    --accent-foreground: 222 47% 11%;

    --destructive: 0 63% 31%;
    --destructive-foreground: 210 40% 98%;

    --border: 217 19% 27%;
    --input: 217 19% 27%;
    --ring: 210 40% 98%;

    --sidebar-background: 215 28% 9%; /* slate-950 */
    --sidebar-foreground: 210 40% 98%;
    --sidebar-border: 222 47% 11%;
    --sidebar-accent: 217 33% 17%;
    --sidebar-accent-foreground: 210 40% 98%;
    --sidebar-ring: 43 96% 56%;
  }
}
```

**Semantik Renk Kullanım Rehberi:**

| Token | Kullanım yeri |
|---|---|
| `background` / `foreground` | Sayfa zemini, ana metin |
| `card` | BlogCard, mesaj kartı, istatistik kutusu |
| `primary` | Birincil düğmeler, aktif nav linki, logo |
| `muted-foreground` | Tarih, meta bilgi, placeholder metin |
| `accent` (amber) | Featured badge, okunmamış mesaj nokta, yıldız |
| `destructive` | Sil düğmesi, hata toast, form hata mesajı |
| `border` | Kart sınırı, ayırıcı (Separator), input |
| `sidebar-*` | Admin sidebar alanı |

---

### 1.2 Tipografi

**Font Ailesi Seçimi:**

- **Başlık (heading):** `'Playfair Display', Georgia, serif`  
  Gerekçe: Blog okuyucusuna premium, editorial bir his verir. Büyük başlıklarda güçlü bir karakteri vardır. Google Fonts'tan yüklenir.

- **Gövde (body):** `'Inter', system-ui, -apple-system, sans-serif`  
  Gerekçe: Ekranda mükemmel okunabilirlik, çok ağırlık seçeneği. Admin panel ve form alanlarında da seamless çalışır.

- **Mono (kod bloğu, etiket):** `'JetBrains Mono', 'Fira Code', monospace`  
  Gerekçe: Admin panelinde ID görüntüleme, olası kod snippet'ler için.

**Tailwind v4 `@theme` bloğuna eklenmesi gereken font tanımları:**

```css
@theme {
  --font-heading: 'Playfair Display', Georgia, serif;
  --font-body: 'Inter', system-ui, -apple-system, sans-serif;
  --font-mono: 'JetBrains Mono', 'Fira Code', monospace;
}
```

**Tip Ölçeği:**

| Kullanım | Tailwind class | px eşdeğeri | Satır yüksekliği | Ağırlık |
|---|---|---|---|---|
| H1 (sayfa başlığı, hero) | `text-5xl` / `text-4xl` (mobile) | 48px / 36px | `leading-tight` (1.25) | `font-bold` (700) |
| H2 (bölüm başlığı) | `text-3xl` | 30px | `leading-snug` (1.375) | `font-bold` |
| H3 (kart başlığı, sidebar) | `text-xl` | 20px | `leading-snug` | `font-semibold` (600) |
| H4 (alt bölüm) | `text-lg` | 18px | `leading-snug` | `font-semibold` |
| Body Large | `text-base` | 16px | `leading-relaxed` (1.625) | `font-normal` (400) |
| Body | `text-sm` | 14px | `leading-relaxed` | `font-normal` |
| Small / meta | `text-xs` | 12px | `leading-normal` | `font-normal` / `font-medium` |
| Blog içeriği | `text-base` / `text-lg` | 16-18px | `leading-loose` (2.0) | `font-normal` |

**Blog Detay İçeriği İçin Özel Dikkat:**
Blog `description` alanı uzun metin içerdiği için ayrı bir tipografi sınıfı tanımlanır:

```css
/* Tailwind v4 @layer utilities içinde */
.prose-blog {
  font-family: var(--font-body);
  font-size: 1.0625rem;       /* 17px — biraz daha rahat okuma */
  line-height: 1.875;          /* ~30px satır yüksekliği */
  color: hsl(var(--foreground));
  max-width: 68ch;             /* ~680px okunabilirlik sınırı */
}

.prose-blog p + p {
  margin-top: 1.25em;
}
```

---

### 1.3 Spacing Ölçeği

4px base, Tailwind'in varsayılan ölçeği kullanılır (geçerli Tailwind v4). Ek özel token'a gerek yoktur.

**Kullanılan aralık konvansiyonları:**

| Kullanım | Token |
|---|---|
| Section arası boşluk | `py-16` (64px) |
| Kart iç dolgu | `p-6` (24px) |
| Liste item arası | `gap-4` (16px) |
| Form alan arası | `space-y-4` (16px) |
| Inline eleman arası | `gap-2` (8px) |
| Nav link arası | `gap-6` (24px) |
| Container max-width | `max-w-7xl mx-auto px-4 sm:px-6 lg:px-8` |
| İçerik max-width | `max-w-3xl` (blog metni), `max-w-5xl` (liste/grid) |

---

### 1.4 Border, Shadow ve Geçiş Token'ları

```css
/* Radius seviyeleri */
--radius: 0.5rem;       /* 8px — varsayılan (shadcn default) */
/* sm: 4px, md: 8px, lg: 12px, xl: 16px, full: 9999px */

/* Gölge seviyeleri — Tailwind varsayılanlarıyla uyumlu */
/* shadow-sm  — card hover öncesi */
/* shadow-md  — card hover sonrası, dropdown */
/* shadow-lg  — modal, dialog */
/* shadow-xl  — sheet/drawer */

/* Geçiş — tüm interaktif elemanlarda */
transition: colors 150ms ease, shadow 150ms ease, transform 150ms ease;
/* Tailwind: transition-colors duration-150 */
```

---

### 1.5 Dark Mode Stratejisi

shadcn/ui standardına uygun **class tabanlı** dark mode. `<html>` elementine `dark` class'ı eklenmesiyle aktive olur.

`ThemeProvider` komponenti:
- localStorage'da `theme` değeri saklar (`"light"` | `"dark"` | `"system"`)
- `system` seçiliyse `prefers-color-scheme` media query'i izler
- shadcn/ui'nin `<ThemeProvider>` komponenti (`next-themes` kütüphanesi) kullanılır

**Kurulum notu:** `npx shadcn add` ile eklenen komponentler otomatik olarak CSS değişken tokenlarını kullanır; dark mode ek iş gerektirmez.

---

## 2. Ortak Layout Planı

### 2.1 Public Layout — RootLayout Doldurma Planı

Mevcut `RootLayout.tsx` iskelet olarak var. Şu şekilde doldurulacak:

```
RootLayout
├── SiteHeader                        ← yeni komponent
│   ├── div.container (max-w-7xl)
│   │   ├── Logo                      ← Link to="/" + SVG/metin logo
│   │   ├── DesktopNav                ← hidden md:flex, nav linkleri
│   │   │   ├── NavLink (Anasayfa)
│   │   │   ├── NavLink (Bloglar)
│   │   │   └── NavLink (İletişim)
│   │   ├── AuthNav                   ← auth durumuna göre render
│   │   │   ├── [anonim] Button(Giriş) + Button(Kayıt, variant=outline)
│   │   │   └── [giriş yapılmış]
│   │   │       ├── [admin] Button(Admin Panel, variant=ghost) → /admin
│   │   │       └── UserMenu (DropdownMenu)
│   │   │           ├── DropdownMenuTrigger → Avatar(initials)
│   │   │           └── DropdownMenuContent
│   │   │               ├── DropdownMenuItem: e-posta (disabled, sadece gösterim)
│   │   │               └── DropdownMenuItem: Çıkış Yap (logout)
│   │   └── MobileMenuButton         ← md:hidden, Sheet trigger
│
├── MobileNav (Sheet/Drawer)          ← SheetContent side="left"
│   ├── Logo
│   ├── nav linkleri (dikey)
│   └── auth butonları
│
├── main.flex-1
│   └── Suspense (fallback: PageSkeleton)
│       └── Outlet
│
└── SiteFooter                        ← yeni komponent
    ├── div.container
    │   ├── Logo + kısa açıklama
    │   ├── SocialMediaLinks           ← GET /api/social-media ile dinamik
    │   │   └── SocialLink[] (title + icon + url)
    │   ├── ContactSummary             ← GET /api/contact ile dinamik
    │   │   ├── adres, telefon, e-posta
    │   └── Copyright
    │       └── "© 2026 ZnBlog. Tüm hakları saklıdır."
```

**Responsive Davranış:**

| Breakpoint | Header |
|---|---|
| `< md` (< 768px) | Logo + hamburger ikonu; nav linkleri ve auth Sheet içinde |
| `md` (≥ 768px) | Logo + DesktopNav + AuthNav yatay; Sheet gizli |
| `lg` (≥ 1024px) | Aynı + Container genişler |

**Header durumları:**
- `isInitializing = true`: AuthNav alanında `Skeleton` (w-24 h-9) göster; layout'u kaydırma
- `isInitializing = false, user = null`: Giriş + Kayıt butonları
- `isInitializing = false, user != null, !isAdmin`: Kullanıcı avatarı + dropdown
- `isInitializing = false, user != null, isAdmin`: Avatar + "Admin Panel" linki + dropdown

**Veri Gereksinimleri (SiteFooter):**
- `GET /api/social-media` → `SocialMedia[]` (public)
- `GET /api/contact` → `Contact` (public, 404 gelebilir — footer graceful handle etmeli)

---

### 2.2 Admin Layout

Router'da yeni bir layout route'u açılacak. Mevcut `paths.admin` tek sayfaya işaret ediyor; admin altına birden fazla route eklenecek. `paths.ts` genişletilmeli:

```typescript
// paths.ts genişletmesi (kavramsal)
admin: '/admin',
adminBlogs: '/admin/blogs',
adminBlogCreate: '/admin/blogs/create',
adminBlogEdit: (id) => `/admin/blogs/${id}/edit`,
adminCategories: '/admin/categories',
adminMessages: '/admin/messages',
adminSocialMedia: '/admin/social-media',
```

```
AdminLayout (ProtectedRoute requireAdmin ile sarılı)
├── AdminSidebar                        ← fixed, w-64
│   ├── Logo / site adı
│   ├── SidebarNav
│   │   ├── SidebarNavItem (Dashboard)       → /admin
│   │   ├── SidebarNavItem (Bloglar)         → /admin/blogs
│   │   ├── SidebarNavItem (Kategoriler)     → /admin/categories
│   │   ├── SidebarNavItem (Mesajlar)        → /admin/messages
│   │   │   └── Badge (okunmamış sayısı)     ← GET /api/admin/messages ile
│   │   └── SidebarNavItem (Sosyal Medya)   → /admin/social-media
│   └── SidebarFooter
│       └── kullanıcı e-postası + Çıkış Yap
│
└── div.flex-1 (ml-64)
    ├── AdminTopbar
    │   ├── MobileSidebarToggle (md:hidden)  ← Sheet trigger
    │   ├── PageTitle (dinamik, her sayfadan)
    │   └── UserMenu (Avatar + DropdownMenu)
    └── main.p-6
        └── Outlet (admin sayfa içerikleri)
```

**Responsive Davranış:**

| Breakpoint | Admin Layout |
|---|---|
| `< lg` (< 1024px) | Sidebar gizli (Sheet/Drawer olarak açılır), hamburger Topbar'da |
| `lg` (≥ 1024px) | Sidebar sabit sol (`fixed left-0 top-0 h-full w-64`), içerik `ml-64` |

**SidebarNavItem aktif durumu:**
`useMatch` veya `NavLink` ile aktif route tespit edilir; aktif item `bg-sidebar-accent text-sidebar-accent-foreground` alır.

---

## 3. Sayfa Planları

### 3.1 Anasayfa (HomePage)

**Amaç:** Yeni ziyaretçiyi karşılamak, en son/öne çıkan blogları vitrine koymak, okumaya teşvik etmek.

**Veri İhtiyaçları:**
- Son bloglar: `GET /api/blogs?page=1&pageSize=6` → `PagedResult<BlogListItem>`
- Kategoriler: `GET /api/categories` → `Category[]` (hero kategori filtresinde kullanılabilir)

**TanStack Query:**
- `useQuery(['blogs', 'latest'])` → son 6 blog
- `useQuery(['categories'])` → kategori listesi

**Komponent Hiyerarşisi:**

```
HomePage
├── HeroSection
│   ├── div.hero-bg (gradient: from-primary to-slate-800)
│   │   ├── h1 "ZnBlog" (font-heading, text-5xl, text-primary-foreground)
│   │   ├── p  "Düşünceler, yazılar, hikayeler." (text-xl, muted-foreground)
│   │   └── Button "Blogları Keşfet" (→ /blogs, size=lg)
│   └── [opsiyonel] FeaturedBlogCard (ilk blog öne çıkarılabilir)
│
├── LatestBlogsSection
│   ├── SectionHeader
│   │   ├── h2 "Son Yazılar" (text-3xl, font-heading)
│   │   └── Button "Tümünü Gör" (variant=ghost, → /blogs)
│   ├── BlogGrid (grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6)
│   │   └── BlogCard[] (× 6)
│   └── [loading] BlogCardSkeleton[] (× 6)
│
└── CategoryChipsSection (opsiyonel)
    ├── h2 "Kategoriler"
    └── CategoryChip[] (Badge variant=secondary, tıklanınca /blogs?categoryId=X)
```

**BlogCard Komponenti (ortak — tüm listelerde kullanılır):**

```
BlogCard (Card shadcn)
├── CardImage (aspect-video, object-cover, rounded-t-lg)
│   └── img src={coverImage} alt={title}
├── CardContent (p-4)
│   ├── CategoryBadge (Badge variant=secondary, text-xs)
│   ├── h3 (text-xl, font-semibold, font-heading, line-clamp-2, mt-2)
│   ├── PostMeta (flex, gap-3, mt-2, text-xs, text-muted-foreground)
│   │   ├── span authorName
│   │   ├── span "·"
│   │   └── time dateTime={createdAt} (formatlanmış: "10 Haz 2026")
│   └── Link "Devamını Oku →" (text-sm, text-primary, font-medium, mt-3, block)
```

**Props (kavramsal):**
```typescript
interface BlogCardProps {
  blog: BlogListItem
  variant?: 'default' | 'featured'  // featured: daha büyük görsel
}
```

**Durumlar:**
- Loading: 6 adet `BlogCardSkeleton` (Skeleton komponentleriyle kart şekli)
- Error: `ErrorState` komponenti — "Yazılar yüklenemedi. Tekrar dene." + retry Button
- Empty: `EmptyState` — "Henüz yazı yok." (bu sayfada nadiren karşılaşılır)

---

### 3.2 Blog Listesi (BlogListPage)

**Amaç:** Tüm blogları listelemek, kategori filtrelemek, sayfalama yapmak.

**URL Parametreleri:** `?page=1&categoryId=xxx` (URL'de tutulur, Filter persistence için)

**Veri İhtiyaçları:**
- `GET /api/blogs?page={page}&pageSize=9&categoryId={categoryId}` → `PagedResult<BlogListItem>`
- `GET /api/categories` → `Category[]`

**TanStack Query:**
- `useQuery(['blogs', { page, categoryId }])` — kategorId ve page değişince otomatik refetch
- `useQuery(['categories'])` — filtreleme için

**Komponent Hiyerarşisi:**

```
BlogListPage
├── PageHeader
│   └── h1 "Yazılar" (text-4xl, font-heading)
│
├── FilterBar (flex, flex-wrap, gap-3, mb-8)
│   ├── CategoryFilter
│   │   └── Select (shadcn) — "Tüm Kategoriler" + Category[]
│   │       (onChange → URL param güncelle + page=1'e resetle)
│   └── [opsiyonel gelecek] SortSelect
│
├── BlogGrid (grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6)
│   ├── [loading] BlogCardSkeleton[] (× 9)
│   ├── [success] BlogCard[] (items'dan)
│   └── [empty] EmptyState "Bu kategoride henüz yazı yok."
│
├── [error] ErrorState
│
└── PaginationBar (mt-8, flex, justify-center)
    └── Pagination (shadcn)
        ├── PaginationPrevious
        ├── PaginationContent (sayfa numaraları)
        └── PaginationNext
        (totalPages'a göre render; max 7 sayfa göster, araya "...")
```

**Props (kavramsal):**
```typescript
interface CategoryFilterProps {
  categories: Category[]
  selectedId: string | null
  onChange: (id: string | null) => void
}
```

**Durumlar:**
- Loading: İskelet kartlar + disabled pagination
- Error: `ErrorState` orta sayfa
- Empty (filtre ile): "Bu kategoride henüz yazı bulunmuyor." + "Filtreyi Kaldır" button
- Empty (filtre yok): "Henüz hiç yazı yayımlanmamış."

**Erişim:** Anonim dahil herkes.

---

### 3.3 Blog Detay (BlogDetailPage)

**Amaç:** Tek bir blog yazısını, içeriğini, yorumları ve yorum formunu göstermek.

**URL:** `/blogs/:id`

**Veri İhtiyaçları:**
- `GET /api/blogs/{id}` → `BlogDetail`
- `GET /api/blogs/{id}/comments` → `Comment[]`
- (Genişletilmiş reply için) `GET /api/comments/{commentId}/replies` → `SubComment[]` (tıklayınca lazy load)

**TanStack Query:**
- `useQuery(['blog', id])` → blog detay
- `useQuery(['comments', id])` → yorum listesi

**Komponent Hiyerarşisi:**

```
BlogDetailPage
├── [loading] BlogDetailSkeleton (tam sayfa iskelet)
├── [error] ErrorState (404: "Yazı bulunamadı" | diğer: "Yüklenemedi")
│
└── [success]
    ├── BlogArticle (article tag — semantik HTML)
    │   ├── BlogPostHeader
    │   │   ├── CategoryBadge
    │   │   ├── h1 (text-4xl sm:text-5xl, font-heading, leading-tight)
    │   │   ├── PostMeta
    │   │   │   ├── AuthorInfo (Avatar initials + authorName)
    │   │   │   ├── time (createdAt)
    │   │   │   └── [updatedAt varsa] span "Güncellendi: {updatedAt}"
    │   │   └── PostActions (yalnızca yazar veya admin görür)
    │   │       ├── Button "Düzenle" (→ /admin/blogs/:id/edit)
    │   │       └── AlertDialog ile "Sil" (DELETE /api/blogs/:id)
    │   │
    │   ├── CoverImage (w-full, aspect-video, object-cover, rounded-xl, my-8)
    │   │
    │   ├── BlogImage (varsa, w-full, rounded-lg, my-6)
    │   │
    │   └── BlogContent (div.prose-blog)
    │       └── {description} — plain text veya markdown. Backend'den geldiği
    │           formata göre: plain text ise <p> tag'larına böl; markdown ise
    │           react-markdown kütüphanesi (implementasyon kararı)
    │
    └── CommentsSection (mt-16, border-t, pt-8)
        ├── h2 "Yorumlar ({comments.length})"
        │
        ├── AddCommentForm (giriş yapılmışsa görünür)
        │   ├── Textarea (shadcn, placeholder="Yorumunuzu yazın...")
        │   ├── CharacterCount (text-xs, text-muted-foreground)
        │   └── Button "Yorum Yap" (POST /api/blogs/{id}/comments)
        │
        ├── [anonim] CallToAction "Yorum yapmak için giriş yapın"
        │   └── Button(variant=outline) → /login
        │
        ├── [loading] CommentItemSkeleton[] (× 3)
        ├── [empty] EmptyState "Henüz yorum yok. İlk yorumu siz yapın!"
        │
        └── CommentList
            └── CommentItem[] (Comment)
                ├── Avatar (initials, w-8 h-8)
                ├── div
                │   ├── CommentHeader
                │   │   ├── strong displayName
                │   │   ├── time createdAt
                │   │   └── [isEdited] span "(düzenlendi)"
                │   ├── p commentText
                │   ├── CommentActions (yalnızca sahip veya admin)
                │   │   ├── Button "Düzenle" (inline edit moda geç)
                │   │   └── AlertDialog ile "Sil" (DELETE)
                │   ├── InlineEditForm (düzenleme modu aktifse)
                │   │   ├── Textarea (mevcut metin ile dolu)
                │   │   ├── Button "Kaydet"
                │   │   └── Button "İptal" (variant=ghost)
                │   ├── Button "Yanıtla" (subCommentCount badge ile)
                │   │
                │   └── RepliesSection (açık/kapalı toggle)
                │       ├── SubCommentItem[] (SubComment)
                │       │   └── [CommentItem ile aynı yapı, sadece reply]
                │       └── AddReplyForm
                │           ├── Textarea (placeholder="Yanıtınızı yazın...")
                │           └── Button "Yanıtla"
```

**Yetki Mantığı:**
- `PostActions` (düzenle/sil): `user.id === blog.authorId || isAdmin`
- `CommentActions`: `user.id === comment.userId || isAdmin`
- `AddCommentForm`: `isAuthenticated`

**Durumlar:**
- Blog yükleme: tam sayfa skeleton (header + büyük görsel + içerik çizgileri)
- Blog 404: "Bu yazı bulunamadı" + "Ana Sayfaya Dön" button
- Yorum submit loading: Button `disabled + spinner`
- Yorum submit error: Sonner toast "Yorum gönderilemedi."
- Yorum submit success: Form temizlenir, `invalidateQueries(['comments', id])`
- Sil onayı: `AlertDialog` — "Bu yorumu silmek istediğinize emin misiniz?"

---

### 3.4 Login (LoginPage)

**Amaç:** Kayıtlı kullanıcının sisteme giriş yapması.

**Komponent Hiyerarşisi:**

```
LoginPage (flex min-h-screen items-center justify-center, bg-muted/40)
└── Card (w-full max-w-md, shadow-lg)
    ├── CardHeader
    │   ├── Logo (Link → /, merkeze hizalı)
    │   ├── CardTitle "Giriş Yap" (text-2xl, font-heading)
    │   └── CardDescription "Hesabınıza devam edin"
    │
    └── CardContent
        └── LoginForm
            ├── FormField: E-posta
            │   ├── Label "E-posta"
            │   └── Input (type=email, autocomplete=email)
            │
            ├── FormField: Şifre
            │   ├── Label "Şifre"
            │   ├── PasswordInput (Input + göster/gizle toggle Button)
            │   └── [hata varsa] FormMessage (text-destructive)
            │
            ├── [form hata] Alert (variant=destructive)
            │   └── "E-posta veya şifre hatalı." (401) | "Hesap kilitlendi." (423)
            │
            └── Button "Giriş Yap" (w-full, loading state ile spinner)
                ├── [loading] Loader2 icon + "Giriş yapılıyor..."
                └── [default] "Giriş Yap"

        └── CardFooter (mt-4)
            └── p "Hesabınız yok mu?"
                └── Link "Kayıt Ol" → /register (text-primary)
```

**Form Validasyon (client-side — backend'den önce):**
- E-posta: zorunlu, geçerli format
- Şifre: zorunlu, min 1 karakter (backend kuralları login'i yönetir)

**Başarı Akışı:**
1. `login()` çağrılır
2. Başarılıysa: `navigate(state.from ?? paths.home, { replace: true })`
3. Başarısızsa: Alert göster, form temizleme yapma

**Durumlar:**
- Submit loading: Button disabled + spinner + "Giriş yapılıyor..."
- 401: "E-posta veya şifre hatalı." alert
- 423: "Hesabınız kilitlendi. Lütfen daha sonra deneyin." alert
- Zaten giriş yapılmışsa: `navigate(paths.home)` — sayfa yüklendiğinde redirect

---

### 3.5 Register (RegisterPage)

**Amaç:** Yeni kullanıcı kaydı.

**Önemli Not:** Backend register sonrası token DÖNDÜRMEZ. Başarılı kayıt sonrası kullanıcı otomatik login YAPILAMAZ (AuthProvider'da `register()` fonksiyonu bunu söylüyor). Kullanıcı login sayfasına yönlendirilir + başarı mesajı gösterilir.

**Komponent Hiyerarşisi:**

```
RegisterPage (flex min-h-screen items-center justify-center, bg-muted/40)
└── Card (w-full max-w-md, shadow-lg)
    ├── CardHeader
    │   ├── Logo
    │   ├── CardTitle "Hesap Oluştur"
    │   └── CardDescription "Topluluğa katılın"
    │
    └── CardContent
        └── RegisterForm
            ├── div.grid.grid-cols-2.gap-4
            │   ├── FormField: Ad
            │   │   ├── Label "Ad"
            │   │   └── Input (type=text, autocomplete=given-name)
            │   └── FormField: Soyad
            │       ├── Label "Soyad"
            │       └── Input (type=text, autocomplete=family-name)
            │
            ├── FormField: E-posta
            │   ├── Label "E-posta"
            │   └── Input (type=email, autocomplete=email)
            │
            ├── FormField: Şifre
            │   ├── Label "Şifre"
            │   ├── PasswordInput (toggle görünürlük)
            │   └── PasswordStrengthHint
            │       └── ul text-xs, text-muted-foreground
            │           ├── li "En az 8 karakter"
            │           ├── li "En az bir büyük harf"
            │           └── li "En az bir rakam"
            │
            ├── FormField: Profil Görseli URL (imageUrl)
            │   ├── Label "Profil Görseli URL (opsiyonel)"
            │   ├── Input (type=url, placeholder="https://...")
            │   └── FormDescription "Boş bırakabilirsiniz"
            │
            ├── [form hata] Alert (variant=destructive)
            │   └── "Bu e-posta adresi zaten kayıtlı." (409) | validation mesajları
            │
            └── Button "Kayıt Ol" (w-full, loading state)

        └── CardFooter
            └── p "Zaten hesabınız var mı?"
                └── Link "Giriş Yap" → /login
```

**Başarı Akışı:**
1. `register()` çağrılır → 201
2. `navigate(paths.login)` + Sonner toast: "Hesabınız oluşturuldu. Giriş yapabilirsiniz."
3. Hata 409: Alert "Bu e-posta adresi zaten kullanımda."
4. Hata 400: Her alan hatası için FormMessage (backend validation mesajları parse edilir)

---

### 3.6 İletişim (ContactPage)

**Not:** `paths.ts`'de henüz `/contact` yolu yok. Eklenmesi gerekir.

**Amaç:** Ziyaretçinin iletişim formu göndermesi, site iletişim bilgilerini ve haritayı görmesi.

**Veri İhtiyaçları:**
- `GET /api/contact` → `Contact` (public, 404 gelebilir)
- `GET /api/social-media` → `SocialMedia[]`
- `POST /api/messages` (form gönderimi)

**Komponent Hiyerarşisi:**

```
ContactPage
├── PageHeader
│   ├── h1 "İletişim" (font-heading)
│   └── p "Bizimle iletişime geçin" (text-muted-foreground)
│
└── div.grid.grid-cols-1.lg:grid-cols-2.gap-12 (max-w-5xl mx-auto)
    │
    ├── ContactFormCard (Card)
    │   ├── CardHeader: "Mesaj Gönder"
    │   └── CardContent
    │       └── ContactForm
    │           ├── div.grid.grid-cols-2.gap-4
    │           │   ├── FormField: Ad Soyad (Input)
    │           │   └── FormField: E-posta (Input type=email)
    │           ├── FormField: Konu (Input)
    │           ├── FormField: Mesaj (Textarea, rows=6)
    │           ├── [success] Alert (variant=default, green ikon)
    │           │   └── "Mesajınız alındı! En kısa sürede yanıt vereceğiz."
    │           └── Button "Gönder" (w-full, loading state)
    │
    └── ContactInfoCard
        ├── ContactDetails (GET /api/contact'tan — 404'te "Henüz bilgi eklenmemiş")
        │   ├── ContactDetailRow (MapPin ikon + address)
        │   ├── ContactDetailRow (Phone ikon + phone)
        │   ├── ContactDetailRow (Mail ikon + email)
        │   └── [mapUrl varsa] MapEmbed (iframe src={mapUrl}, rounded-xl)
        │
        └── SocialMediaLinks (GET /api/social-media'dan)
            └── SocialLink[] (ikon + title + href={url})
```

**Durumlar:**
- Form submit loading: Button disabled + spinner
- Form submit success: Alert göster + form resetle (Sonner toast da ek)
- Form submit error: Alert destructive "Mesaj gönderilemedi. Tekrar deneyin."
- Contact 404: ContactInfoCard gracefully "Henüz iletişim bilgisi eklenmemiş." gösterir (hard error değil)

---

### 3.7 Admin Dashboard (AdminDashboardPage)

**Amaç:** Hızlı istatistik özeti ve navigasyon noktası.

**Veri İhtiyaçları (TanStack Query):**
- `useQuery(['admin', 'blogs', 'count'])` → `GET /api/admin/blogs?pageSize=1` + `totalCount`
- `useQuery(['admin', 'categories'])` → `GET /api/categories` + `length`
- `useQuery(['admin', 'messages', 'unread'])` → `GET /api/admin/messages` + filter `!isRead` count
- `useQuery(['categories'])` → kategori sayısı (aynı endpoint, cache'lenir)

**Not:** Backend'de ayrı bir `/api/admin/stats` endpoint'i yoksa yukarıdaki çözüm gerekli. Sayfa sayfalı listeyi sadece `totalCount` için çeker. Bu kabul edilebilir bir trade-off (admin sayfa, nadir açılır).

**Komponent Hiyerarşisi:**

```
AdminDashboardPage
├── PageHeader "Dashboard"
│
└── div.grid.grid-cols-1.sm:grid-cols-2.lg:grid-cols-4.gap-6
    ├── StatCard (bloglar)
    │   ├── Card ikon: FileText
    │   ├── CardTitle "Toplam Blog"
    │   └── value: totalCount
    │
    ├── StatCard (kategoriler)
    │   ├── Card ikon: Tag
    │   ├── CardTitle "Kategoriler"
    │   └── value: categories.length
    │
    ├── StatCard (okunmamış mesajlar)
    │   ├── Card ikon: Mail + amber badge
    │   ├── CardTitle "Okunmamış Mesaj"
    │   └── value: unreadCount (kırmızı/amber renk)
    │
    └── StatCard (sosyal medya)
        ├── Card ikon: Share2
        ├── CardTitle "Sosyal Medya"
        └── value: socialMedia.length

StatCard (shadcn Card):
├── CardHeader (flex-row, justify-between, pb-2)
│   ├── CardTitle (text-sm, font-medium, text-muted-foreground)
│   └── Icon (w-4 h-4, text-muted-foreground)
└── CardContent
    ├── div (text-3xl, font-bold) — değer
    └── p (text-xs, text-muted-foreground) — açıklama
```

**Durumlar:**
- Loading: 4 adet StatCard skeleton
- Her kart bağımsız query'e bağlı — biri hata verse sadece o kart "—" gösterir

---

### 3.8 Admin Blog Yönetimi

**Amaç:** Blog oluşturma, düzenleme, silme. Tablo + form sayfaları.

**Alt Sayfalar:**
- `/admin/blogs` — liste (tablo)
- `/admin/blogs/create` — yeni blog formu (ayrı sayfa)
- `/admin/blogs/:id/edit` — düzenleme formu (ayrı sayfa)

**Neden ayrı sayfa (dialog değil):** Blog formu zengin (2 görsel upload + uzun textarea + kategori select) — dialog içinde kullanıcı deneyimi kötü olur. Modal olmayan sayfa tercih edildi.

**Veri İhtiyaçları:**
- `GET /api/admin/blogs?page=1&pageSize=20` (admin endpoint'i, yazar filtresi olmadan)
- `GET /api/categories` — form için kategori seçimi
- `POST /api/uploads` — görsel yükleme
- `POST /api/blogs` — oluştur
- `PUT /api/blogs/:id` — güncelle
- `DELETE /api/blogs/:id` — sil

**AdminBlogListPage Hiyerarşisi:**

```
AdminBlogListPage (/admin/blogs)
├── AdminPageHeader
│   ├── h1 "Blog Yönetimi"
│   └── Button "Yeni Blog" (→ /admin/blogs/create, Plus ikonu)
│
├── DataTable (shadcn Table wrapper)
│   ├── TableHeader
│   │   └── TableRow
│   │       ├── TableHead "Başlık"
│   │       ├── TableHead "Kategori"
│   │       ├── TableHead "Yazar"
│   │       ├── TableHead "Tarih"
│   │       └── TableHead "İşlemler"
│   │
│   └── TableBody
│       └── TableRow[] (BlogListItem'dan)
│           ├── TableCell (img thumbnail 40x40 + title, truncate)
│           ├── TableCell (categoryName, Badge)
│           ├── TableCell (authorName)
│           ├── TableCell (createdAt, formatlanmış)
│           └── TableCell (ActionMenu)
│               └── DropdownMenu
│                   ├── DropdownMenuItem "Görüntüle" (→ /blogs/:id, yeni sekme)
│                   ├── DropdownMenuItem "Düzenle" (→ /admin/blogs/:id/edit)
│                   └── DropdownMenuItem "Sil" (AlertDialog tetikler, text-destructive)
│
├── [loading] TableSkeleton (5 satır)
├── [empty] EmptyState "Henüz blog eklenmemiş." + "Yeni Blog" button
└── PaginationBar
```

**BlogFormPage (Create + Edit — aynı form komponenti, farklı props):**

```
BlogFormPage (/admin/blogs/create veya /admin/blogs/:id/edit)
├── AdminPageHeader
│   ├── Button "Geri" (← /admin/blogs, ArrowLeft ikon)
│   └── h1 "Yeni Blog" / "Blogu Düzenle"
│
└── Card (max-w-3xl mx-auto)
    └── CardContent
        └── BlogForm
            ├── FormField: Başlık
            │   └── Input (placeholder="Blog başlığı")
            │
            ├── FormField: Kategori
            │   └── Select (shadcn) — Category[] listesi
            │       └── SelectItem (key={id}) "{categoryName}"
            │
            ├── FormField: Kapak Görseli
            │   └── ImageUploadField
            │       ├── [URL preview varsa] img (aspect-video, object-cover)
            │       ├── Input (type=url, placeholder="URL girin veya yükleyin")
            │       ├── — VEYA —
            │       └── FileUploadButton
            │           ├── input[type=file] (gizli, accept="image/*", max 5MB)
            │           ├── Button "Görsel Seç" (onClick → input.click())
            │           ├── [yükleniyor] Progress + "Yükleniyor..."
            │           └── [hata] FormMessage "Dosya 5MB'ı aşmamalıdır."
            │
            ├── FormField: İçerik Görseli (blogImage) — aynı ImageUploadField yapısı
            │
            ├── FormField: İçerik
            │   └── Textarea (rows=12, placeholder="Blog içeriğini yazın...")
            │
            ├── [form hata] Alert (destructive)
            │
            └── div.flex.gap-3.justify-end
                ├── Button "İptal" (variant=outline, → /admin/blogs)
                └── Button "Kaydet" (loading state ile)
```

**ImageUploadField Props (kavramsal):**
```typescript
interface ImageUploadFieldProps {
  label: string
  value: string          // mevcut URL
  onChange: (url: string) => void
  onError: (msg: string) => void
}
```

**Akış:**
1. Dosya seçilir → `POST /api/uploads` (multipart)
2. Başarıyla URL döner → `onChange(url)` çağrılır
3. Form submit'te bu URL `coverImage` / `blogImage` alanına yazılır

**Durumlar:**
- Edit sayfası: önce `GET /api/blogs/:id` → form alanlarını doldur
- Edit yükleme: tam form skeleton
- Save loading: Button disabled + spinner
- Save success: Sonner toast + `navigate('/admin/blogs')`
- Delete: AdminBlogListPage'de AlertDialog → DELETE → toast + query invalidate

---

### 3.9 Admin Kategori Yönetimi

**Amaç:** Kategori ekleme, düzenleme, silme. Basit tablo + inline/dialog CRUD.

**Tercih:** Dialog-based CRUD — kategori formu çok basit (sadece `categoryName`), ayrı sayfaya gerek yok.

**Veri İhtiyaçları:**
- `GET /api/categories` → `Category[]` (id, categoryName, blogCount)
- `POST /api/admin/categories`
- `PUT /api/admin/categories/:id`
- `DELETE /api/admin/categories/:id` (blog bağlıysa 409 → özel uyarı)

**Komponent Hiyerarşisi:**

```
AdminCategoriesPage (/admin/categories)
├── AdminPageHeader
│   ├── h1 "Kategori Yönetimi"
│   └── Button "Yeni Kategori" (Plus ikon → CreateCategoryDialog açar)
│
├── DataTable
│   ├── TableHeader: "Kategori Adı" | "Blog Sayısı" | "İşlemler"
│   └── TableBody
│       └── TableRow[] (Category'den)
│           ├── TableCell categoryName
│           ├── TableCell blogCount (Badge)
│           └── TableCell ActionMenu
│               ├── DropdownMenuItem "Düzenle" (EditCategoryDialog açar)
│               └── DropdownMenuItem "Sil" (AlertDialog — 409 kontrolü ile)
│
├── CreateCategoryDialog (Dialog shadcn)
│   ├── DialogHeader "Yeni Kategori"
│   └── DialogContent
│       └── form
│           ├── FormField: Kategori Adı (Input)
│           └── div.flex.gap-3.justify-end
│               ├── Button "İptal" (DialogClose)
│               └── Button "Kaydet"
│
├── EditCategoryDialog — aynı yapı, mevcut categoryName ile dolu
│
└── DeleteCategoryAlertDialog
    ├── [blogCount === 0] Normal silme onayı
    └── [blogCount > 0] Özel uyarı Alert (destructive)
        └── "Bu kategoriye bağlı {blogCount} blog var. Silmeden önce blogları
             başka bir kategoriye taşıyın veya silin."
        └── Button "Tamam" (sadece kapat, silme yapma)
```

**409 Yönetimi:** DELETE isteği 409 dönerse catch bloğunda özel mesaj gösterilir. Backend `DeleteBehavior.Restrict` ile bu hatayı üretir.

---

### 3.10 Admin Mesaj Kutusu

**Amaç:** İletişim formundan gelen mesajları yönetmek. Okunmamışları öne çıkarmak.

**Veri İhtiyaçları:**
- `GET /api/admin/messages` → `Message[]` (okunmamışlar önce, backend sıralaması)
- `PATCH /api/admin/messages/:id` → `{ isRead: true }` (okundu işaretle)

**Komponent Hiyerarşisi:**

```
AdminMessagesPage (/admin/messages)
├── AdminPageHeader
│   ├── h1 "Mesaj Kutusu"
│   └── Badge (okunmamış sayısı, amber/destructive renk)
│
└── div.grid.grid-cols-1.lg:grid-cols-3.gap-6 (email client layout)
    │
    ├── MessageList (lg:col-span-1, border-r)
    │   ├── [loading] MessageItemSkeleton[] (× 5)
    │   └── MessageListItem[] (Message'dan)
    │       └── button (tıklanınca sağda detay açılır, aktif: bg-muted)
    │           ├── [!isRead] div.w-2.h-2.rounded-full.bg-amber-500 (okunmamış nokta)
    │           ├── div.font-semibold {name}
    │           ├── div.text-sm.truncate {subject}
    │           └── time.text-xs.text-muted-foreground {createdAt}
    │
    └── MessageDetail (lg:col-span-2)
        ├── [seçili mesaj yoksa] EmptyState "Bir mesaj seçin"
        └── [seçili mesaj varsa]
            ├── MessageDetailHeader
            │   ├── div
            │   │   ├── h2 {subject} (font-heading)
            │   │   └── p {name} • {email} • {createdAt}
            │   └── div
            │       ├── [!isRead] Button "Okundu İşaretle" (PATCH → isRead: true)
            │       └── Badge (isRead ? "Okundu" : "Okunmamış")
            │
            └── MessageBody
                └── p (whitespace-pre-wrap) {messageBody}
```

**Okundu İşaretleme Akışı:**
1. Mesaj listesinde tıklanınca: `setSelectedMessage(msg)` + eğer `!isRead` → otomatik `PATCH`
2. Mesaj detayda "Okundu İşaretle" butonu: manuel PATCH
3. PATCH başarısında: `invalidateQueries(['admin', 'messages'])` → liste güncellenir

**SidebarNavItem Bağlantısı:**
Mesaj badge'i (`AdminSidebar`'daki okunmamış sayısı) bu sayfanın query'siyle senkronize olmalı. `useQuery(['admin', 'messages'])` global cache'te tutulur.

---

### 3.11 Admin Sosyal Medya Yönetimi

**Amaç:** Site sosyal medya linklerini yönetmek.

**Veri İhtiyaçları:**
- `GET /api/social-media` → `SocialMedia[]`
- `POST /api/admin/social-media`
- `PUT /api/admin/social-media/:id`
- `DELETE /api/admin/social-media/:id`

**Komponent Hiyerarşisi:**

```
AdminSocialMediaPage (/admin/social-media)
├── AdminPageHeader
│   ├── h1 "Sosyal Medya"
│   └── Button "Ekle" (Plus ikon)
│
├── SocialMediaGrid (grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4)
│   └── SocialMediaCard[] (SocialMedia'dan)
│       └── Card
│           ├── CardContent (flex, items-center, gap-4)
│           │   ├── span {icon} (CSS class veya emoji — backend'den gelen string)
│           │   ├── div
│           │   │   ├── p {title} (font-semibold)
│           │   │   └── a {url} (text-sm, text-muted-foreground, truncate, external link)
│           │   └── ActionMenu (DropdownMenu)
│           │       ├── DropdownMenuItem "Düzenle"
│           │       └── DropdownMenuItem "Sil"
│
├── [empty] EmptyState "Henüz sosyal medya linki eklenmemiş."
│
├── SocialMediaFormDialog (Create + Edit)
│   ├── DialogHeader "Sosyal Medya Ekle" / "Düzenle"
│   └── DialogContent
│       └── form
│           ├── FormField: Platform Adı (Input, placeholder="Instagram")
│           ├── FormField: URL (Input type=url)
│           └── FormField: İkon
│               ├── Input (placeholder="fab fa-instagram veya 📷")
│               └── FormDescription "CSS sınıfı veya emoji girebilirsiniz"
│
└── DeleteAlertDialog (AlertDialog)
```

---

## 4. Komponent Envanteri

### 4.1 shadcn/ui — Kurulacak Komponentler

Tek komut olarak çalıştırılabilecek liste:

```bash
npx shadcn@latest add \
  button \
  card \
  form \
  input \
  textarea \
  label \
  select \
  dialog \
  alert-dialog \
  sheet \
  table \
  badge \
  avatar \
  dropdown-menu \
  pagination \
  skeleton \
  alert \
  separator \
  tabs \
  progress \
  sonner \
  navigation-menu
```

**Açıklamalar:**

| Komponent | Kullanım yeri |
|---|---|
| `button` | Tüm CTA'lar, form submit, ikon butonlar |
| `card` | BlogCard, StatCard, form sarmalayıcı |
| `form` | Login, Register, Blog formu (react-hook-form entegrasyonu) |
| `input` | Tüm text input'lar |
| `textarea` | Blog içeriği, yorum formu, mesaj formu |
| `label` | Form etiketleri |
| `select` | Kategori filtresi, blog formu kategori seçimi |
| `dialog` | Kategori CRUD, sosyal medya CRUD |
| `alert-dialog` | Sil onayı (blog, yorum, kategori, sosyal medya) |
| `sheet` | Mobil nav drawer, (opsiyonel) admin sidebar mobil |
| `table` | Admin blog listesi, kategori listesi |
| `badge` | Kategori etiketi, okunmamış mesaj sayısı, blog sayısı |
| `avatar` | Header kullanıcı menüsü, yorum author |
| `dropdown-menu` | Kullanıcı menüsü, tablo satır aksiyonları |
| `pagination` | Blog listesi sayfalama |
| `skeleton` | Tüm loading state'leri |
| `alert` | Form hata blokları (login hatası, kategori-sil uyarısı) |
| `separator` | Layout ayırıcılar |
| `tabs` | (Gelecek genişleme için — şimdilik admin sayfalarında kullanılmayabilir) |
| `progress` | Görsel yükleme progress bar |
| `sonner` | Global toast bildirimleri (başarı/hata) |
| `navigation-menu` | Header desktop nav (opsiyonel — basit Link'lerle de çalışır) |

---

### 4.2 Projeye Özel Ortak Komponentler

Oluşturulacak dosya yolları `client/src/components/` altında:

**Layout Komponentleri** (`components/layout/`):
- `SiteHeader.tsx` — Public header (logo + nav + auth)
- `SiteFooter.tsx` — Footer (sosyal medya + iletişim özeti + telif)
- `AdminLayout.tsx` — Admin iki sütun layout (sidebar + içerik)
- `AdminSidebar.tsx` — Admin sol sidebar nav
- `AdminTopbar.tsx` — Admin üst bar (sayfa başlığı + kullanıcı menüsü)

**Ortak UI** (`components/common/`):
- `EmptyState.tsx` — Props: `icon`, `title`, `description`, `action?`
- `ErrorState.tsx` — Props: `message`, `onRetry?`
- `PageHeader.tsx` — Props: `title`, `description?`, `action?` (sağa hizalı button)
- `DataTable.tsx` — TanStack Table veya basit shadcn Table wrapper

**Blog Komponentleri** (`components/blog/`):
- `BlogCard.tsx` — Props: `blog: BlogListItem`, `variant?: 'default' | 'featured'`
- `BlogCardSkeleton.tsx` — Skeleton ile BlogCard şekli
- `CategoryBadge.tsx` — Props: `categoryName: string`, `categoryId?: string`
- `PostMeta.tsx` — Props: `authorName`, `createdAt`, `updatedAt?`
- `CategoryFilter.tsx` — Props: `categories`, `selected`, `onChange`

**Yorum Komponentleri** (`components/comments/`):
- `CommentItem.tsx` — Props: `comment: Comment`, `currentUserId?`, `isAdmin`
- `CommentForm.tsx` — Props: `blogId`, `onSuccess`
- `ReplyItem.tsx` — Props: `reply: SubComment`, `currentUserId?`, `isAdmin`
- `ReplyForm.tsx` — Props: `commentId`, `onSuccess`
- `CommentItemSkeleton.tsx`

**Form Komponentleri** (`components/forms/`):
- `PasswordInput.tsx` — Input + göster/gizle toggle (Eye/EyeOff icon)
- `ImageUploadField.tsx` — URL input + dosya seç + preview + progress

**Admin Komponentleri** (`components/admin/`):
- `StatCard.tsx` — Props: `title`, `value`, `icon`, `description?`, `accent?`
- `AdminPageHeader.tsx` — Props: `title`, `action?`
- `ConfirmDeleteDialog.tsx` — Props: `open`, `onConfirm`, `onCancel`, `itemName`, `warning?`

---

## 5. Uygulama Sırası

### Önerilen Dikey Dilim Sırası

```
ADIM 1 — Altyapı (1 gün)
├── Design system kurulumu: index.css CSS token'ları
├── Font'ları yükle (Google Fonts)
├── shadcn/ui init + tüm komponentleri ekle (npx shadcn add ...)
├── paths.ts genişletme (admin alt rotalar + contact)
└── router.tsx güncelleme (AdminLayout + yeni route'lar)

ADIM 2 — Layout (1 gün)
├── SiteHeader (logo + nav + auth + mobil sheet)
├── SiteFooter (statik versiyon, API entegrasyonu sonra)
├── AdminLayout + AdminSidebar + AdminTopbar
└── EmptyState + ErrorState + PageHeader ortak komponentleri

ADIM 3 — Auth Sayfaları (1 gün)
├── LoginPage (form + validation + error handling)
├── RegisterPage (form + validation + başarı redirect)
└── PasswordInput komponenti

ADIM 4 — Public Okuma (2 gün)
├── BlogCard + BlogCardSkeleton + CategoryBadge + PostMeta
├── BlogListPage (grid + CategoryFilter + Pagination)
├── HomePage (HeroSection + LatestBlogsSection)
└── BlogDetailPage (makale + yorum listesi — yorum form sonraki adımda)

ADIM 5 — Yorum Sistemi (1.5 gün)
├── CommentItem + ReplyItem + ilgili Skeleton'lar
├── CommentForm + ReplyForm (giriş yapılmışsa)
└── BlogDetailPage'e yorum bölümü entegrasyonu

ADIM 6 — İletişim Sayfası (0.5 gün)
├── ContactPage (form + iletişim kartı + harita embed)
└── SiteFooter API entegrasyonu (social media + contact)

ADIM 7 — Admin Blog Yönetimi (2 gün)
├── StatCard + AdminDashboardPage
├── AdminBlogListPage (DataTable + ActionMenu)
├── ImageUploadField komponenti
└── BlogFormPage (create + edit, aynı form)

ADIM 8 — Admin Diğer Sayfalar (1.5 gün)
├── AdminCategoriesPage (dialog CRUD + 409 uyarısı)
├── AdminMessagesPage (email client layout + okundu akışı)
└── AdminSocialMediaPage (card grid + dialog CRUD)

ADIM 9 — Cilalama (1 gün)
├── Dark mode ThemeProvider + toggle düğmesi
├── Sonner toast global kurulumu
├── Loading/error/empty state kontrolleri
└── Erişilebilirlik: focus trap'ler, ARIA, kontrast kontrol
```

**Tahmini toplam:** 11.5 gün (ROADMAP'taki 10–15 gün tahminiyle örtüşüyor)

---

## Erişilebilirlik (a11y) Notları

### Semantik HTML
- Sayfa başlıkları `<h1>` → `<h2>` hiyerarşisi korunmalı (her sayfada tek `<h1>`)
- Blog makalesi `<article>` tag'i içinde
- Nav linkleri `<nav>` içinde, `aria-label="Ana navigasyon"` ile
- Footer `<footer>` tag'i
- Form elemanları `<label for="...">` veya `aria-label` ile etiketli
- Admin sidebar `<nav aria-label="Admin navigasyonu">`

### ARIA Gereksinimleri
- `DropdownMenu` açıkken `aria-expanded="true"`, `aria-haspopup="true"`
- `Dialog` açıkken focus trap (shadcn otomatik yönetir)
- Loading Skeleton'larda `aria-busy="true"` ebeveyn üzerinde
- Toast bildirimleri `role="status"` veya `aria-live="polite"` (Sonner yönetir)
- AlertDialog: `role="alertdialog"`, `aria-labelledby`, `aria-describedby`
- Pagination: `aria-label="Sayfalama"` nav + mevcut sayfa için `aria-current="page"`
- Resimler: `alt` attribute zorunlu (BlogCard, CoverImage, BlogImage)
- İkon butonları: `aria-label` zorunlu (sadece ikon içerdiğinde)

### Kontrast
- `primary` (#1e293b) üzerinde `primary-foreground` (#f8fafc): **15.8:1** (AAA)
- `muted-foreground` (#64748b) üzerinde `background` (#fff): **4.6:1** (AA) — sınırda, meta için kabul edilebilir
- `accent` (amber-500) metin olarak kullanılmamalı — sadece dekoratif/background
- Dark mode: tüm token'lar shadcn standardını takip ettiğinden otomatik AA uyumlu

### Klavye Navigasyonu
- Header nav: Tab sırası mantıklı (logo → nav linkleri → auth butonları)
- Mobil Sheet: Esc ile kapanır (shadcn otomatik)
- Tablo satırları: satır içi ActionMenu Tab ile erişilebilir
- Comment inline edit: Escape ile iptal, Enter ile gönder (Textarea'da Ctrl+Enter)
- Admin sidebar: `NavLink` bileşeni focus state'i göstermeli (`ring-2 ring-sidebar-ring`)

---

## Tailwind v4 Uyumluluk Notu

Proje Tailwind v4 kullanıyor (CSS-first konfigürasyon). Bu doküman boyunca önerilen tüm token'lar `@theme` ve CSS değişken sistemiyle v4'e uygundur. `tailwind.config.js` dosyası yoktur — tüm konfigürasyon `index.css` içindeki `@theme` bloğunda tutulur. shadcn/ui `tailwind-variants` veya `cva` (class-variance-authority) ile Tailwind v4'te sorunsuz çalışır.

---

*Doküman sonu. Bu spesifikasyon, react-frontend-dev'in başka soru sormadan implementasyona başlayabileceği netlikte hazırlanmıştır.*

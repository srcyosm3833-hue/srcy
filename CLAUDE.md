# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Proje

ZnBlogApp — .NET 10, çok yazarlı blog uygulaması. Onion/Clean Architecture, `ZnBlogApp.slnx` solution dosyası. Detaylı faz planı ve iş kalemleri: `docs/ROADMAP.md`.

**Mevcut durum (ROADMAP Faz 0–4 tamamlandı):** Backend uçtan uca çalışır durumda — auth (JWT + refresh), Blog/Category/Comment/SubComment/Message/Contact/SocialMedia tam CRUD, hepsi dikey dilim olarak. **104 integration testi** yeşil (xUnit + WebApplicationFactory, izole LocalDB). Frontend (`client/`) iskeleti kuruldu (API client + auth altyapısı + placeholder route'lar); gerçek sayfalar henüz yazılmadı.

## Komutlar

```powershell
# Build
dotnet build ZnBlogApp.slnx

# API'yi çalıştır
dotnet run --project Presentation/Zn.ClientWebApi

# EF Core migration ekle / uygula (Persistence projesi + WebApi startup)
dotnet ef migrations add <Ad> --project Infrastructure/Zn.Persistence --startup-project Presentation/Zn.ClientWebApi
dotnet ef database update --project Infrastructure/Zn.Persistence --startup-project Presentation/Zn.ClientWebApi
```

Bağlantı cümlesi `ConnectionStrings:DefaultConnection` **user secrets**'ta tutulur (Zn.ClientWebApi, LocalDB / `ZnApp` veritabanı) — appsettings.json'a yazma.

## Mimari

Katmanlar ve referans yönü: `Zn.Domain` ← `Zn.Application` ← `Zn.Persistence` / `Zn.Infrastructure` ← `Zn.ClientWebApi`.

- **Core/Zn.Domain** — Entity'ler: Blog, Category, Comment, SubComment (iki seviyeli yorum), Contact, Message, SocialMedia + `User : IdentityUser`, `Role : IdentityRole` (string PK). Diğerleri `BaseEntity` (Guid Id, CreatedAt, UpdatedAt, Id-bazlı equality) tabanlı. Klasik namespace stili kullanılır (file-scoped değil), XML doc yorumları Türkçe.
- **Core/Zn.Application** — CQRS dikey dilimleri (`Features/<Feature>/<Action>/`), `Common/Results` (`Result`/`Result<T>`, `Error`, `ErrorType`), `Common/Pagination` (`PagedResult<T>`), `Interfaces/`. Her dilim: command/query (record) + Wolverine static handler + FluentValidation validator + Mapperly `[Mapper]`. Hata fabrikaları feature başına `*Errors` sınıfında toplanır.
- **Infrastructure/Zn.Persistence** — `ZnBlogDbContext : IdentityDbContext<User, Role, string>`; tüm `IEntityTypeConfiguration` sınıfları assembly taramasıyla uygulanır (`Configurations/` klasörüne ekleme yeterli). DI kaydı `AddPersistenceServices` (DbContext + Identity politikaları + repository'ler). Blog→Category `DeleteBehavior.Restrict`; RefreshToken/Comment→Blog `Cascade`. Repository'ler okurken `AsNoTracking` + DB seviyesinde projeksiyon, mutasyonda tracked döner.
- **Infrastructure/Zn.Infrastructure** — Dış servisler: `JwtTokenService` (access + refresh üretimi), `Sha256TokenHasher` (refresh token DB'de hash'li saklanır), `LocalFileStorageService`. DI kaydı `AddInfrastructureServices`.
- **Presentation/Zn.ClientWebApi** — Controller tabanlı API. `ApiControllerBase.HandleResult` `Result`'ı HTTP'ye eşler (Validation 400, Unauthorized 401, Forbidden 403, NotFound 404, Conflict 409, Locked 423); `GetUserId()`/`IsAdmin()` token'dan. `IdentityDataSeeder` açılışta rol + ilk admin seed eder. Public/admin controller ayrımı; yazma uçlarında yazar token'dan alınır.

**Konvansiyonlar:** Entity'ler factory + private set + `Update`/mutator ile invariant korur (anemik değil). Handler'da yetki sırası **önce 404 sonra 403** (kayıt varlığı sızdırılmaz). Wolverine 6 host'ta `UseWolverine` + `WolverineFx.RuntimeCompilation` zorunlu; opaque kayıtlar (`UserManager`/`SignInManager`/`DbContext`) için `AlwaysUseServiceLocationFor<>` allow-list gerekir. Entity davranış değişiklikleri şemayı bozmamalı — `dotnet ef migrations has-pending-model-changes` ile doğrula.

## Kesinleşmiş Teknik Kararlar (detay: docs/ROADMAP.md)

1. **Mediator = Wolverine** (MediatR değil) — handler'lar Wolverine konvansiyonlarıyla yazılır.
2. **Mapping = Mapperly** — source generator tabanlı `[Mapper]` sınıfları; AutoMapper kullanma.
3. **Validation = FluentValidation.**
4. **Blog çok yazarlı** — `Blog.UserId` (string FK → AspNetUsers) eklendi (`AddBlogAuthor` migration); create'te yazar token'dan alınır, update/delete sadece yazar veya admin. Yorum yetki modeli: oluştur = giriş yapan herkes, düzenle = yalnızca sahip, sil = sahip veya admin (blog yazarının yorum silme ayrıcalığı yok).
5. **Auth = JWT access token (~15 dk) + refresh token (rotation'lı), baştan tam kurulum** — `RefreshToken` entity'si, `/api/auth/refresh` ve `/api/auth/logout` endpoint'leri Faz 1 kapsamında; revoke edilmiş token tekrar kullanılırsa kullanıcının tüm refresh token'ları iptal edilir.
6. **Test:** backend xUnit + WebApplicationFactory (InMemory EF kullanma — TestContainers/LocalDB tercih edilir), frontend Vitest + React Testing Library.

7. **Görsel yükleme:** Blog create/update görsel alanları URL kabul eder; dosya yükleme ayrı `POST /api/uploads` endpoint'iyle `IFileStorageService` soyutlaması üzerinden yapılır (development: `LocalFileStorageService` → `wwwroot/uploads`, jpg/jpeg/png/webp, max 5 MB). Production'da aynı arayüzle cloud storage implementasyonuna geçilir — çağıran kod değişmez.
8. **Frontend (`client/`):** Vite + React + TypeScript. Routing = React Router (data router / `createBrowserRouter`), server state = TanStack Query, stil = Tailwind CSS (v4, CSS-first), HTTP = axios. Dev server portu **5173** (CORS bu porta izin verir); API base URL `VITE_API_BASE_URL` env'inden (`http://localhost:5241`).
9. **Token saklama = localStorage** (A2 kapandı). accessToken + refreshToken localStorage'da; axios interceptor 401'de tek-uçuşlu (single-flight) refresh yapar, başarısızsa `/login`'e yönlendirir. XSS riski bilinerek seçildi.
10. **UI = shadcn/ui** (A3 kapandı) — Tailwind tabanlı, kopyala-yapıştır komponent modeli. Bileşenler `client/` içine eklenir; ayrı UI kütüphanesi (MUI/Ant) kullanılmaz.

11. **Soft Delete (Faz 5 — A7/A8).** Silme işlemleri kalıcı değil; `ISoftDeletable` (`IsDeleted`, `DeletedAt`) + EF Core **global query filter** ile yapılır. Kapsam: Category, Blog, Message, User (ve Faz 5'te eklenenler). Kurallar: soft-delete edilmiş kayıt **public API'de asla dönmez**, yalnızca Admin/Manager görebilir (**A7**). Kullanıcı silinince kendisi soft-delete edilir ama **blogları silinmez/anonimleştirilmez**, sadece listelenmez (**A8**). Soft-delete edilen kullanıcı login olamaz.

12. **Roller = Admin / Manager / User; Manager = "içerik yöneticisi" (Faz 5 — A6).** `Manager` rolü `RoleNames`'e eklenir, seed edilir; yetki kontrolü `[Authorize(Roles="Admin,Manager")]` + handler içi rol kontrolüyle. **A6 yetki matrisi (bağlayıcı):**
    - **Blog/Kategori/Mesaj CRUD** → Admin + Manager. Manager blogda yalnız **kendi** kaydını günceller/siler; **başkasının** blogunu/yorumunu yönetmek (ve herkesin yorumunu silmek) **yalnız Admin**.
    - **Kullanıcı güncelleme/silme ve rol atama/kaldırma → yalnız Admin.** Kullanıcı **listeleme** → Admin + Manager.
    - **Like/Unlike, Yorum oluşturma** → tüm giriş yapmış kullanıcılar (User dahil). **Arama** → anonim dahil herkes.
    - Frontend: admin alanı `requireRole={['Admin','Manager']}`; menü itemları role göre gösterilir; `AuthProvider`'da `isManager`/`isAdminOrManager`.

13. **Like + Sosyal Paylaşım + Arama (Faz 5 — A9/A10/A11).** Beğeni **yalnız giriş yapmış kullanıcılara** açık (anonim yok), idempotent toggle (**A9**); ayrı `BlogLike` ve yorum/alt-yorum için ayrı like tabloları (**A10**). Sosyal paylaşım = Web Share API / paylaş linkleri. Arama ilk aşamada **EF Core LIKE** (başlık + açıklama), `PagedResult` uyumlu; ileride Elasticsearch geçiş kapısı açık (**A11**). Detaylı faz planı + görevlendirme prompt'ları: `docs/features/FAZ5-PLAN.md`.

> Not: Madde 8'deki API base URL geliştirme tercihi `https://localhost:7253` (https profili) olarak güncellendi; `--launch-profile https` ile çalıştırılır, dev sertifikası güvenilir. HTTP isteyen `http://localhost:5241` + `--launch-profile http` kullanabilir.

## Açık Kararlar

Şu an açık mimari karar yok — tüm kesinleşmiş kararlar yukarıda (Faz 5 kararları A6–A11 dahil; detay: `docs/features/FAZ5-PLAN.md`). Yeni bir belirsizlik çıkarsa uygulamadan önce kullanıcıya sor ve buraya işle.

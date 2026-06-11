# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Proje

ZnBlogApp — .NET 10, çok yazarlı blog uygulaması. Onion/Clean Architecture, `ZnBlogApp.slnx` solution dosyası. Detaylı faz planı ve iş kalemleri: `docs/ROADMAP.md`.

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
- **Core/Zn.Application** — CQRS dikey dilimleri (`Features/`), `Common/` (Result tipi, pagination), `Interfaces/`. Henüz boş; ROADMAP Faz 0'da kurulacak.
- **Infrastructure/Zn.Persistence** — `ZnBlogDbContext : IdentityDbContext<User, Role, string>`; tüm `IEntityTypeConfiguration` sınıfları assembly taramasıyla uygulanır (`Configurations/` klasörüne ekleme yeterli). DI kaydı `AddPersistenceServices` extension'ında (DbContext + Identity politikaları). Blog→Category ilişkisi `DeleteBehavior.Restrict`.
- **Infrastructure/Zn.Infrastructure** — Dış servisler (JWT token, e-posta, dosya depolama). Henüz boş.
- **Presentation/Zn.ClientWebApi** — Controller tabanlı API. Henüz endpoint yok.

Her yeni özellik dikey dilim olarak gelir: command/query + handler + validator + mapper + endpoint.

## Kesinleşmiş Teknik Kararlar (detay: docs/ROADMAP.md)

1. **Mediator = Wolverine** (MediatR değil) — handler'lar Wolverine konvansiyonlarıyla yazılır.
2. **Mapping = Mapperly** — source generator tabanlı `[Mapper]` sınıfları; AutoMapper kullanma.
3. **Validation = FluentValidation.**
4. **Blog çok yazarlı** — `Blog.UserId` (string FK → AspNetUsers) eklenecek (Faz 2 başında migration); create'te yazar token'dan alınır, update/delete sadece yazar veya admin.
5. **Auth = JWT access token (~15 dk) + refresh token (rotation'lı), baştan tam kurulum** — `RefreshToken` entity'si, `/api/auth/refresh` ve `/api/auth/logout` endpoint'leri Faz 1 kapsamında; revoke edilmiş token tekrar kullanılırsa kullanıcının tüm refresh token'ları iptal edilir.
6. **Test:** backend xUnit + WebApplicationFactory (InMemory EF kullanma — TestContainers/LocalDB tercih edilir), frontend Vitest + React Testing Library.

7. **Görsel yükleme:** Blog create/update görsel alanları URL kabul eder; dosya yükleme ayrı `POST /api/uploads` endpoint'iyle `IFileStorageService` soyutlaması üzerinden yapılır (development: `LocalFileStorageService` → `wwwroot/uploads`, jpg/jpeg/png/webp, max 5 MB). Production'da aynı arayüzle cloud storage implementasyonuna geçilir — çağıran kod değişmez.

## Açık Kararlar (uygulamadan önce kullanıcıya sor)

- Frontend token saklama (localStorage vs httpOnly cookie) ve frontend mimarisi (router, state, UI kütüphanesi) — Faz 4'ten önce.

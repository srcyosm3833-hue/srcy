# ZnBlogApp — Tamamlanma Yol Haritası

> Durum tarihi: 10 Haziran 2026. Teknik kararlar işlenmiş sürüm (v2).

---

## Kesinleşmiş Teknik Kararlar

| # | Karar | Seçim | Etkisi |
|---|---|---|---|
| K1 | Mediator | **Wolverine** | Application katmanı handler'ları Wolverine konvansiyonlarıyla yazılır (IRequest/IRequestHandler yok; plain handler metotları). |
| K2 | Nesne eşleme | **Mapperly** | Source generator tabanlı, zero-reflection mapper sınıfları (`[Mapper]` attribute). |
| K3 | Blog yazarlığı | **Çok yazarlı** | `Blog` entity'sine `UserId` (string FK → AspNetUsers) + `User` navigation eklenecek, yeni migration gerekecek. Faz 2'nin ilk işi. |
| K4 | Auth mimarisi | **JWT access token + refresh token, baştan tam kurulum** | Kısa ömürlü access token (~15 dk) + uzun ömürlü refresh token (rotation'lı). `RefreshToken` entity'si + migration + `/api/auth/refresh` ve `/api/auth/logout` endpoint'leri Faz 1 kapsamına dahil. |

### Hâlâ Açık Olan Kararlar

| # | Konu | Son karar tarihi | Not |
|---|---|---|---|
| ~~A1~~ | ~~Görsel yükleme stratejisi~~ — **KAPANDI:** Blog alanları URL kabul eder + ayrı `POST /api/uploads` endpoint'i `IFileStorageService` ile (dev: local `wwwroot/uploads`; production'da cloud implementasyonuna geçilecek). | ✅ | Faz 2'de uygulandı. |
| A2 | Frontend token saklama (localStorage vs httpOnly cookie + CSRF) | Faz 4 başlamadan | httpOnly cookie önerilir; refresh token zaten K4 ile var, cookie'ye taşımak API'de küçük değişiklik gerektirir. |
| A3 | Frontend mimarisi (React Router vs Next.js, state yönetimi, UI kütüphanesi) | Faz 4 başlamadan | SEO ihtiyacı belirleyici. |
| A4 | Test DB izolasyonu (TestContainers vs LocalDB) | Faz 5 / ilk integration test | InMemory EF kullanılmayacak — Restrict delete ve unique index davranışlarını simüle etmez. |
| A5 | Yorum silme yetki modeli (kendi yorumu vs admin her yorumu) | Faz 3 başlamadan | Öneri: kendi yorumu + admin override. |

---

## Mevcut Durum Özeti

| Katman | Durum | Not |
|---|---|---|
| Zn.Domain | ✅ Tamamlanmış | 9 entity, `BaseEntity<TId>` (Guid, CreatedAt, UpdatedAt). K3 ve K4 gereği iki ekleme yapılacak: `Blog.UserId` ve `RefreshToken` entity'si. |
| Zn.Persistence | ✅ Tamamlanmış | `ZnBlogDbContext : IdentityDbContext<User, Role, string>`, 8 Fluent config, `migBir` migration, LocalDB + user secrets. |
| Zn.Application | ❌ Boş | Sadece csproj. Wolverine + FluentValidation + Mapperly eklenecek. |
| Zn.Infrastructure | ❌ Boş | JWT/refresh token servisi, e-posta, dosya yükleme buraya. |
| Zn.ClientWebApi | ⚠️ İskelet | Controller yok; `UseAuthentication()`, JWT Bearer, CORS, global exception handler eksik. |
| Frontend | ❌ Yok | React + TypeScript, Faz 4. |
| Test | ❌ Yok | xUnit (backend), Vitest + RTL (frontend). |

---

## Faz Haritası

```
FAZ 0 ──► FAZ 1 ──► FAZ 2 ──► FAZ 3 ──► FAZ 4
Altyapı    Auth      Blog      Yorumlar   Frontend &
Kurulumu   (dikey    CRUD      & İletişim Admin Panel
           dilim)
4-5 gün    5-7 gün   7-8 gün   5-6 gün    10-15 gün

FAZ 5 (Test) — Faz 1'den itibaren her fazın sonuna dağıtılarak paralel yürür.
```

**Toplam tahmin:** 32–40 iş günü (tek geliştirici; K4'ün baştan tam kurulumu Faz 1'e +1–2 gün ekledi).

---

## Faz 0 — Application & Infrastructure Altyapı Kurulumu

**Amaç:** Boş katmanların kodlanabilir hale getirilmesi. Hiçbir feature bu faz olmadan başlayamaz.
**Öncelik:** Must · **Efor:** 4–5 gün

| ID | İş Kalemi | Bağımlılık | Efor |
|---|---|---|---|
| F0-B1 | **Wolverine** paketinin Zn.Application'a eklenmesi, `AddWolverine` host entegrasyonu | — | 0.5 g |
| F0-B2 | FluentValidation + **Mapperly** paketlerinin eklenmesi | — | 0.5 g |
| F0-B3 | Application klasör yapısı: `Features/` (dikey dilimler), `Common/` (Result tipi, pagination), `Interfaces/` | F0-B1 | 1 g |
| F0-B4 | `Result<T>` / hata modeli tasarımı (validation, not-found, conflict, unauthorized ayrımı) | F0-B3 | 0.5 g |
| F0-B5 | Zn.Infrastructure: `IJwtTokenService` (access + refresh üretimi), `IEmailService`, `IFileStorageService` iskeletleri | F0-B3 | 1 g |
| F0-B6 | Program.cs: `UseAuthentication()`, JWT Bearer config, CORS, global exception handler middleware | F0-B5 | 1 g |
| F0-B7 | `AddApplicationServices` + `AddInfrastructureServices` DI extension'ları | F0-B5, F0-B6 | 0.5 g |

---

## Faz 1 — Auth Dikey Dilimi (İLK YAPILACAK İŞ)

**Amaç:** Register, login, refresh ve logout uçtan uca çalışır; bir HTTP istemcisiyle token alınıp korunan endpoint'e girilebilir.
**Öncelik:** Must · **Efor:** 5–7 gün (K4 tam kurulum dahil)

### Neden ilk bu dilim?
1. Tüm korunan endpoint'ler JWT'ye bağlı — auth olmadan Faz 2/3/4 test edilemez.
2. Faz 0 altyapısının (Wolverine pipeline, FluentValidation, exception handling, Mapperly) çalıştığı ilk kanıt burada üretilir.
3. Refresh token mimarisi (K4) en baştan kurulduğu için sonraki fazlarda auth'a geri dönülmez.

### İş Kalemleri

| ID | İş Kalemi | Bağımlılık | Efor |
|---|---|---|---|
| F1-B0 | **`RefreshToken` entity'si** (Token hash, UserId, ExpiresAt, RevokedAt, ReplacedByToken) + configuration + migration | Faz 0 | 1 g |
| F1-B1 | `RegisterCommand` + handler (`UserManager.CreateAsync`) | Faz 0 | 1 g |
| F1-B2 | `RegisterCommandValidator` (e-posta format, şifre politikası, zorunlu alanlar) | F1-B1 | 0.5 g |
| F1-B3 | `LoginCommand` + handler (`SignInManager` + access/refresh token üretimi) | F1-B1 | 1 g |
| F1-B4 | JWT token servisi implementasyonu (Zn.Infrastructure): access ~15 dk, refresh ~7 gün, **rotation** (her refresh'te eski token revoke + yenisi üretilir) | F0-B5, F1-B0 | 1 g |
| F1-B5 | `RefreshTokenCommand` + `LogoutCommand` (revoke) handler'ları | F1-B4 | 0.5 g |
| F1-B6 | `AuthController`: POST `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`, `/api/auth/logout` + korunan test endpoint'i GET `/api/me` | F1-B1..B5 | 0.5 g |
| F1-B7 | Exception → HTTP eşlemesi: validation → 400, unauthorized → 401, conflict → 409, lockout → 423 | F0-B6 | 0.5 g |
| F1-T1 | Integration testler: happy path'ler, duplicate email, zayıf şifre, yanlış şifre, lockout, refresh rotation, revoke edilen token reddi | F1-B6 | 1.5 g |

### User Story'ler ve Kabul Kriterleri

**US-1 — Kayıt:** *Bir ziyaretçi olarak e-posta ve şifremle hesap oluşturmak istiyorum.*
- Geçerli ad/soyad/e-posta/şifre (≥8 karakter, büyük harf + rakam) + benzersiz e-posta → **201**, response'ta kullanıcı Id + e-posta; şifre hiçbir alanda dönmez.
- Zayıf şifre → **400** + hangi kuralın ihlal edildiği.
- Kayıtlı e-posta → **409**.
- Boş zorunlu alan → **400** + alan adı.

**US-2 — Giriş:** *Kayıtlı kullanıcı olarak giriş yapıp access + refresh token almak istiyorum.*
- Doğru bilgiler → **200**, response'ta accessToken, refreshToken, expiresAt.
- Yanlış şifre → **401**, jenerik "geçersiz kimlik bilgileri" (alan belirtilmez).
- Var olmayan e-posta → **401** (404 değil — kullanıcı varlığı sızdırılmaz).
- 5 hatalı denemeden sonra → **423/401** + kilit süresi bilgisi.

**US-2b — Token Yenileme:** *Oturumumun kesintisiz sürmesini istiyorum.*
- Geçerli refresh token → **200**, yeni access + yeni refresh token; eski refresh token revoke edilir (rotation).
- Süresi dolmuş / revoke edilmiş / bilinmeyen refresh token → **401**.
- Revoke edilmiş bir token tekrar kullanılırsa kullanıcının tüm refresh token'ları iptal edilir (replay saldırı önlemi).

### "Bitti" Tanımı
- 4 auth endpoint'i + `/api/me` Swagger/Postman üzerinden uçtan uca çalışıyor.
- Tüm hata senaryoları doğru HTTP kodlarıyla dönüyor.
- Integration testler yeşil.

---

## Faz 2 — Blog ve Kategori CRUD

**Öncelik:** Must · **Efor:** 7–8 gün

| ID | İş Kalemi | Bağımlılık | Efor |
|---|---|---|---|
| F2-B0 | **K3 uygulaması:** `Blog.UserId` (string FK) + `User.Blogs` navigation + configuration güncellemesi + migration | Faz 1 | 0.5 g |
| F2-B1 | `GetBlogsQuery` (sayfalama + kategori filtresi + yazar bilgisi) | F2-B0 | 1 g |
| F2-B2 | `GetBlogByIdQuery` | F2-B0 | 0.5 g |
| F2-B3 | `CreateBlogCommand` + validator (yazar = token'daki kullanıcı) | F2-B1 | 1 g |
| F2-B4 | `UpdateBlogCommand` + validator (sadece yazarın kendisi veya admin) | F2-B3 | 0.5 g |
| F2-B5 | `DeleteBlogCommand` (sadece yazar veya admin) | F2-B3 | 0.5 g |
| F2-B6 | Category CRUD (list herkese açık; create/update/delete admin) | Faz 1 | 1 g |
| F2-B7 | `BlogsController` (public): GET `/api/blogs`, GET `/api/blogs/{id}` | F2-B1, F2-B2 | 0.5 g |
| F2-B8 | `AdminBlogsController` + `AdminCategoriesController` | F2-B3..B6 | 0.5 g |
| F2-B9 | Görsel yükleme stratejisinin uygulanması (**A1 kararı sonrası**) | F2-B3, A1 | 1 g |
| F2-T1 | Blog + Category integration testleri (happy path + yetki: 401/403 senaryoları) | F2-B7, F2-B8 | 1.5 g |

Kabul kriterleri özeti: sayfalı listede `items/totalCount/page/pageSize/totalPages`; geçersiz GUID → 400; bulunamayan → 404; tokensız create → 401; yetkisiz rol → 403; var olmayan kategori → 400/422.

---

## Faz 3 — Yorumlar, Mesajlar, Sosyal Medya

**Öncelik:** Should · **Efor:** 5–6 gün

| ID | İş Kalemi | Bağımlılık | Efor |
|---|---|---|---|
| F3-B1 | `AddCommentCommand` + validator (kayıtlı kullanıcı) | Faz 2 | 0.5 g |
| F3-B2 | `GetCommentsByBlogIdQuery` (alt yorum sayısı dahil, herkese açık) | F3-B1 | 0.5 g |
| F3-B3 | `AddSubCommentCommand` + validator | F3-B1 | 0.5 g |
| F3-B4 | `DeleteCommentCommand` (**A5 kararına göre** yetki modeli) | F3-B1, A5 | 0.5 g |
| F3-B5 | `CommentsController`: GET/POST `/api/blogs/{blogId}/comments`, POST `/api/comments/{id}/replies` | F3-B1..B3 | 0.5 g |
| F3-B6 | `SendMessageCommand` (iletişim formu, herkese açık, IsRead=false) | Faz 1 | 0.5 g |
| F3-B7 | `MarkMessageAsReadCommand` (admin) | F3-B6 | 0.5 g |
| F3-B8 | `MessagesController` + `AdminMessagesController` (okunmamışlar önce) | F3-B6, F3-B7 | 0.5 g |
| F3-B9 | SocialMedia CRUD + Contact güncelleme (admin) | Faz 1 | 0.5 g |
| F3-T1 | Yorum + mesaj integration testleri | F3-B5, F3-B8 | 1 g |

---

## Faz 4 — React + TypeScript Frontend

**Öncelik:** Must · **Efor:** 10–15 gün · **Ön koşul:** Faz 1–3 API'leri çalışır durumda + A2/A3 kararları alınmış.

**Public:** anasayfa (son yazılar), blog listesi (filtre + sayfalama), blog detay (içerik + yorumlar + yorum formu), iletişim, login/register.
**Admin:** dashboard (sayaçlar), blog CRUD, kategori/sosyal medya/iletişim yönetimi, mesaj kutusu (okunmamış badge).
**Kapsam dışı (Could):** bildirimler (SignalR), tam metin arama, profil sayfası, dark mode.

| ID | İş Kalemi | Efor |
|---|---|---|
| F4-F1 | Vite + React + TS iskeleti + UI kütüphanesi (A3) | 0.5 g |
| F4-F2 | API istemci katmanı: fetch/axios wrapper, **access token enjeksiyonu + 401'de otomatik refresh akışı**, hata yönetimi | 1.5 g |
| F4-F3 | Login/Register sayfaları + form validation | 1 g |
| F4-F4 | Auth store / token yaşam döngüsü (A2 kararına göre) | 0.5 g |
| F4-F5 | Blog listesi (loading/error/empty state'ler) | 1 g |
| F4-F6 | Blog detay + yorumlar | 1.5 g |
| F4-F7 | İletişim sayfası | 0.5 g |
| F4-F8 | Admin route koruması (rol bazlı) | 0.5 g |
| F4-F9 | Admin blog CRUD sayfaları | 2 g |
| F4-F10 | Admin kategori/sosyal medya/iletişim sayfaları | 1 g |
| F4-F11 | Admin mesaj kutusu | 0.5 g |
| F4-T1/T2 | Vitest + RTL testleri (auth formları, blog liste/detay) | 1.5 g |

---

## Faz 5 — Test Altyapısı ve Kalite

**Öncelik:** Should · **Efor:** 3–4 gün · Faz 1'den itibaren paralel yürütülür (her fazın T-kalemleri bu altyapıyı kullanır).

| ID | İş Kalemi | Efor |
|---|---|---|
| F5-T1 | xUnit projesi + `WebApplicationFactory` kurulumu | 0.5 g |
| F5-T2 | Test DB stratejisi (A4: TestContainers önerilir, Docker gerektirir) | 1 g |
| F5-T3..T5 | Auth / Blog-Kategori / Yorum-Mesaj test paketleri | 1.5 g |
| F5-T6 | Vitest + RTL kurulumu | 0.5 g |

---

## Riskler

| Risk | Seviye | Durum |
|---|---|---|
| JWT/refresh mimarisi belirsizliği | Yüksek | ✅ K4 ile kapatıldı — baştan tam kurulum. |
| Blog yazar alanı eksikliği | Yüksek | ✅ K3 ile kapatıldı — F2-B0'da migration. |
| Mediator/mapping seçimi gecikmesi | Orta | ✅ K1/K2 ile kapatıldı — Wolverine + Mapperly. |
| Görsel depolama stratejisi | Orta | ⏳ A1 — Faz 2 başlamadan kapatılmalı. |
| Frontend mimari kararları | Orta | ⏳ A2/A3 — Faz 4'ten ~1 hafta önce kick-off önerilir. |
| Refresh token replay saldırısı | Orta | Rotation + revoke zinciri F1-B4/B5 kapsamında; testi F1-T1'de zorunlu. |
| Efor tahminleri tek geliştirici varsayımlı | Bilgi | Paralel backend/frontend çalışması süreyi ~yarıya indirir. |

---

*Bu belge 10 Haziran 2026 tarihli codebase durumuna ve K1–K4 kararlarına dayanır. Açık kararlar (A1–A5) kapandıkça güncellenmelidir.*

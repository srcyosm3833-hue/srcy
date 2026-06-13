# ZnBlogApp — Faz 5: Arama, Kullanıcı/Rol Yönetimi, Like/Paylaşım, Soft Delete, Manager Rolü

> Belge tarihi: 12 Haziran 2026. Faz 0–4 tamamlandıktan sonraki genişleme fazı.
> Bu belge kod içermez; yalnızca planlama, user story, kabul kriteri, görev kırılımı ve agent delegation prompt'larından oluşur.

---

## Bağlam ve Ön Durum

| Katman | Durum (Faz 4 sonu) |
|---|---|
| Backend | Auth, Blog, Category, Comment, SubComment, Message, Contact, SocialMedia tam CRUD; 104 integration testi yeşil. |
| Domain | Soft delete yok (ISoftDeletable arayüzü mevcut değil). |
| Roller | Yalnızca "Admin" ve "User". "Manager" rolü tanımlı değil. |
| Frontend | Admin: Dashboard, Blog, Category, Messages, SocialMedia sayfaları. Public: HomePage, BlogList, BlogDetail, Contact, Login, Register. Kullanıcı/rol yönetimi sayfası yok. ProtectedRoute yalnızca requireAdmin destekliyor. |
| ApiControllerBase | IsAdmin() var; IsManager() yok. |
| AuthProvider | isAdmin = roles.includes('Admin'); Manager desteği yok. |

---

## Mimari Konvansiyonlar (Tüm Görevler İçin Geçerli)

- Entity'ler factory + private set + mutator yapısında; anemik model yasak.
- Her dilim: command/query (record) + Wolverine static handler + FluentValidation validator + Mapperly mapper.
- Handler yetki sırası: **önce 404, sonra 403** (kayıt varlığı sızdırılmaz).
- Hata fabrikaları feature başına `*Errors` sınıfında toplanır.
- Repository'ler okurken AsNoTracking + DB projeksiyon; mutasyonda tracked döner.
- Test: xUnit + WebApplicationFactory + LocalDB (InMemory kullanılmaz).
- Wolverine 6: opaque servisler (UserManager, DbContext) için `AlwaysUseServiceLocationFor<>` allow-list.
- Frontend: TanStack Query (server state), shadcn/ui bileşenler, axios interceptor ile 401'de otomatik refresh.

---

## Açık Kararlar

> **DURUM (2026-06-12): A6–A11 ONAYLANDI.** Kullanıcı önerilen varsayılanları kabul etti.
> A6 = "İçerik yöneticisi" seçeneği (Manager içerik yönetir; kullanıcı silme/güncelleme ve
> rol atama yalnız Admin; başkasının blog/yorumunu silmek yalnız Admin). A7–A11 aşağıdaki
> varsayılanlarıyla kabul edildi. Uygulama agent'ları bu kararlara göre kod yazabilir.

Aşağıdaki kararlar plan önerileriyle birlikte listelenmiştir. ~~Geliştirmeye başlamadan önce ekip onayı gerekir.~~ **(Onaylandı — yukarıdaki nota bakınız.)**

| # | Konu | Varsayılan Öneri | Gerekçe |
|---|---|---|---|
| A6 | **Admin vs Manager yetki matrisi** | Aşağıdaki tablo | Rol bazlı politika tasarımı bu karara göre şekillenir |
| A7 | **Soft delete edilmiş kaydın görünürlüğü** | Yalnızca Admin/Manager görebilir; public API'de asla dönmez | GDPR uyumu, veri karmaşıklığını azaltır |
| A8 | **Kullanıcı silindiğinde bloglarına ne olur?** | Kullanıcı soft delete edilir; bloglar anonimleştirilmez, sadece listelenmez | Sert silme tercih edilirse veri kaybı riski |
| A9 | **Like anonim mi sadece üye mi?** | Yalnızca giriş yapmış kullanıcılar beğenebilir | İdempotent like/unlike için userId gerekli |
| A10 | **CommentLike kapsamı** | Blog yorumlarına (Comment) ve alt yorumlara (SubComment) ayrı like tablolarıyla | Ayrı entity idempotent kontrolü kolaylaştırır |
| A11 | **Arama tam metin mi LIKE mi?** | İlk aşamada EF Core LIKE (title + description); sonraki fazda Elasticsearch geçiş kapısı bırakılır | Basit kurulum, sonradan genişletilebilir |

### Önerilen Admin vs Manager Yetki Matrisi (A6)

| İşlem | Admin | Manager | User (giriş yapmış) | Anonim |
|---|---|---|---|---|
| Blog oluştur | Evet | Evet | Hayır | Hayır |
| Blog güncelle (kendi) | Evet | Evet | Hayır | Hayır |
| Blog güncelle (herkesinkini) | Evet | Hayır | Hayır | Hayır |
| Blog soft delete (kendi) | Evet | Evet | Hayır | Hayır |
| Blog soft delete (herkesinkini) | Evet | Hayır | Hayır | Hayır |
| Kategori yönet (CRUD) | Evet | Evet | Hayır | Hayır |
| Yorum sil (herkesinkini) | Evet | Hayır | Hayır | Hayır |
| Yorum sil (kendi) | Evet | Evet | Evet | Hayır |
| Kullanıcıları listele | Evet | Evet | Hayır | Hayır |
| Kullanıcı güncelle | Evet | Hayır | Hayır | Hayır |
| Kullanıcı soft delete | Evet | Hayır | Hayır | Hayır |
| Rol ata/kaldır | Evet | Hayır | Hayır | Hayır |
| Mesajları listele/yönet | Evet | Evet | Hayır | Hayır |
| Like/Unlike | Evet | Evet | Evet | Hayır |
| Arama | Evet | Evet | Evet | Evet |

> **✅ A6 ONAYLANDI (2026-06-12):** Bu matris kabul edildi ve bağlayıcıdır. Tüm yetki
> kontrolleri (controller `[Authorize]` attribute'ları ve handler içi rol kontrolleri)
> bu tabloya uygun yazılmalıdır.

---

## Kesişen (Cross-Cutting) Altyapı Görevleri

Bu görevler bağımsız özellik hikayelerinden önce tamamlanmalıdır. Birden fazla özelliğin ön koşuludur.

---

### INFRA-1: Soft Delete Altyapısı

**Bağımlılıklar:** Yok (ilk iş kalemi)
**Etkilenen özellikler:** Kullanıcı Yönetimi (US-2), Kategori Yönetimi (US-5), Blog Yönetimi (US-6), Mesaj Yönetimi (US-7)

#### User Story

Bir geliştirici olarak, domain entity'lerini kalıcı silmek yerine işaretleyerek "silinmiş" olarak işaretlemek istiyorum, böylece veri bütünlüğü korunur ve silinen kayıtlar gerektiğinde kurtarılabilir.

#### Kabul Kriterleri

**Verilen** ISoftDeletable arayüzü ve EF Core global query filter kurulmuş,
**Ne zaman** herhangi bir entity üzerinde soft delete işlemi yapılırsa,
**O zaman** IsDeleted=true ve DeletedAt=şimdiki zaman set edilir, kayıt veritabanında kalır.

**Verilen** Soft delete uygulanmış bir kategori,
**Ne zaman** public /api/categories endpoint'i çağrılırsa,
**O zaman** silinmiş kategori listede yer almaz (global query filter devrede).

**Verilen** Soft delete uygulanmış bir kayıt,
**Ne zaman** Admin bir kullanıcı yönetim endpoint'i üzerinden silinmiş kayıtları sorgularsa,
**O zaman** includeDeleted=true parametresiyle silinmiş kayıtlar da listelenebilir.

#### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| INFRA1-B1 | `ISoftDeletable` arayüzü: IsDeleted (bool), DeletedAt (DateTime?) alanlarını tanımla | Backend/Domain | — | 0.5 g |
| INFRA1-B2 | Category, Blog, Message entity'lerine ISoftDeletable uygulaması; User için ISoftDeletable + `SoftDelete()` mutator | Backend/Domain | INFRA1-B1 | 1 g |
| INFRA1-B3 | EF Core global query filter: `modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted)` — tüm ISoftDeletable entity'ler için | Backend/Persistence | INFRA1-B2 | 0.5 g |
| INFRA1-B4 | Repository'lere `includeDeleted` parametresi eklenmesi (Admin sorgularında bypass için) | Backend/Persistence | INFRA1-B3 | 0.5 g |
| INFRA1-B5 | EF Core migration: `AddSoftDelete` — tüm etkilenen tablolara IsDeleted (bool, default false) + DeletedAt (datetime2, nullable) kolonu | Backend/Persistence | INFRA1-B2 | 0.5 g |
| INFRA1-B6 | `dotnet ef migrations has-pending-model-changes` doğrulaması; migration çakışması yoksa uygula | Backend/Persistence | INFRA1-B5 | 0.25 g |
| INFRA1-T1 | Global query filter integration testi: silinmiş kayıtların public sorguda dönmediğini doğrula | Test | INFRA1-B5 | 0.5 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp'e Soft Delete altyapısı ekle.

KAPSAM:
1. Zn.Domain projesinde `ISoftDeletable` arayüzü oluştur: bool IsDeleted ve DateTime? DeletedAt alanları.
2. Şu entity'leri ISoftDeletable yap: Category, Blog, Message ve User.
   - Her birinde `SoftDelete()` mutator'ı ekle (IsDeleted=true, DeletedAt=DateTime.UtcNow set eder).
   - BaseEntity miras hiyerarşisine dokunma; ISoftDeletable ayrı arayüz olarak implement edilsin.
3. ZnBlogDbContext.OnModelCreating içinde ISoftDeletable uygulayan tüm entity tipler için global query filter ekle (IsDeleted == false).
4. Etkilenen repository'lere opsiyonel `includeDeleted` parametresi ekle — admin sorgularında global filter bypass edilebilsin (IgnoreQueryFilters() ile).
5. EF Core migration ekle: `AddSoftDelete`. Migration adı, kolonu ve default değeri CLAUDE.md konvansiyonlarına uygun olsun.
6. `dotnet ef migrations has-pending-model-changes` ile model ile migration'ın eşleştiğini doğrula.

KISITLAR:
- Hiçbir mevcut hard-delete endpoint'ini değiştirme — bu görev yalnızca altyapıyı hazırlar; delete handler'larını soft-delete'e çevirmek bir sonraki görevin konusu.
- Test: xUnit + WebApplicationFactory + LocalDB (InMemory kullanma). Global query filter integration testi yaz.
- Klasik namespace stili kullan (file-scoped değil). XML doc Türkçe.

KABUL KRİTERİ:
- Migration uygulandıktan sonra etkilenen tablolarda IsDeleted ve DeletedAt kolonları görünür.
- Public list sorgusu silinmiş kayıtları döndürmez.
- includeDeleted=true ile admin sorgusu silinmiş kayıtları da döndürür.
```

---

### INFRA-2: Manager Rolü ve Yetki Altyapısı

**Bağımlılıklar:** Yok (INFRA-1 ile paralel çalışabilir)
**Etkilenen özellikler:** Rol/Yetki Yönetimi (US-3), Kategori Yönetimi (US-5), Blog Yönetimi (US-6), Mesaj Yönetimi (US-7)

#### User Story

Bir sistem yöneticisi olarak, "Manager" rolünü sisteme tanımlamak istiyorum, böylece içerik yönetimi yetkisi Admin'den bağımsız olarak devredilebilir.

#### Kabul Kriterleri

**Verilen** Uygulama açılışında IdentityDataSeeder çalışırken,
**Ne zaman** rol seed işlemi gerçekleşirse,
**O zaman** "Admin", "User" ve "Manager" rolleri yoksa oluşturulur; zaten varsa tekrar oluşturulmaz.

**Verilen** Manager rolündeki bir kullanıcı,
**Ne zaman** [Authorize(Roles = "Admin,Manager")] ile korunan bir endpoint'i çağırırsa,
**O zaman** 200 yanıt alır.

**Verilen** Manager rolündeki bir kullanıcı,
**Ne zaman** yalnızca [Authorize(Roles = "Admin")] ile korunan bir endpoint'i çağırırsa,
**O zaman** 403 yanıt alır.

#### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| INFRA2-B1 | RoleNames.cs: `Manager` sabiti ekle; `All` listesini güncelle | Backend/Domain | — | 0.25 g |
| INFRA2-B2 | IdentityDataSeeder: Manager rolünü seed listesine ekle | Backend/WebApi | INFRA2-B1 | 0.25 g |
| INFRA2-B3 | ApiControllerBase: `IsManager()` ve `IsAdminOrManager()` yardımcı metodları | Backend/WebApi | INFRA2-B1 | 0.25 g |
| INFRA2-B4 | Mevcut Admin-only controller/endpoint'lerin (Category, Blog, Message yönetim) attribute'larını `[Authorize(Roles = "Admin,Manager")]` olarak güncelle — A6 matrisine uygun | Backend/WebApi | INFRA2-B3 | 0.5 g |
| INFRA2-F1 | AuthProvider: `isManager` ve `isAdminOrManager` türetilmiş değerleri ekle | Frontend | — | 0.25 g |
| INFRA2-F2 | ProtectedRoute: `requireAdmin` yanına `requireRole` prop'u ekle (string array) | Frontend | INFRA2-F1 | 0.5 g |
| INFRA2-F3 | AdminLayout menü: Manager rolüne göre bazı menü itemlarını göster/gizle | Frontend | INFRA2-F2 | 0.5 g |
| INFRA2-T1 | Integration testi: Manager rolüyle korunan endpoint'ler, Admin-only endpoint'lerde 403 | Test | INFRA2-B4 | 0.5 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp'e Manager rolü ve yetki altyapısı ekle.

KAPSAM:
1. Zn.Domain/Authorization/RoleNames.cs içine `Manager = "Manager"` sabiti ekle; `All` readonly listesini güncelle.
2. IdentityDataSeeder (Zn.ClientWebApi): "Manager" rolünü seed listesine ekle.
3. ApiControllerBase: `protected bool IsManager()` ve `protected bool IsAdminOrManager()` yardımcı metodları ekle.
4. Mevcut içerik yönetimi controller'larında (AdminCategoriesController, blog yönetim action'ları, AdminMessagesController) yetki attribute'larını FAZ5-PLAN.md'deki A6 yetki matrisine uygun olarak güncelle.
   - Kategori CRUD, mesaj listeleme: Admin + Manager
   - Kullanıcı silme, rol yönetimi: yalnızca Admin
5. Integration testi: Manager token ile Admin+Manager korumalı endpoint → 200; Admin-only endpoint → 403.

KISITLAR:
- Klasik namespace, XML doc Türkçe.
- Sadece attr/seed değişikliği — yeni handler yazmak bu görevin kapsamında değil.
```

#### Delegation Prompt — react-frontend-dev

```
GÖREV: ZnBlogApp frontend'ine Manager rol desteği ekle.

KAPSAM:
1. client/src/features/auth/AuthContext.ts: `isManager: boolean` ve `isAdminOrManager: boolean` alanları ekle.
2. AuthProvider.tsx: bu değerleri roles.includes('Manager') ile türet.
3. client/src/routes/ProtectedRoute.tsx (veya mevcut ProtectedRoute dosyası): `requireRole?: string[]` prop'u ekle — kullanıcının rollerinden en az biri eşleşmezse /login'e yönlendir.
4. client/src/routes/router.tsx: admin alanını `requireRole={['Admin','Manager']}` ile koru (requireAdmin yerine).
5. AdminLayout menü: rol bazlı görünürlük — örn. kullanıcı/rol yönetimi yalnızca Admin görür.

KISITLAR:
- Mevcut requireAdmin kullanımlarını kırmadan geriye dönük uyumlu şekilde genişlet.
- TypeScript strict mode; shadcn/ui bileşenler.
```

---

## Özellik 1: Arama (Search)

**MoSCoW:** Should
**Efor:** M (3–4 gün backend + frontend)
**Ön Koşul:** Yok (bağımsız özellik; soft delete altyapısı tamamlandıktan sonra global filter otomatik devreye girer)

### Özet ve Amaç

Kullanıcıların blog başlığı ve içeriğinde anahtar kelimeyle arama yapabilmesi; opsiyonel kategori filtresiyle daraltılabilmesi. Sayfalama mevcut `PagedResult<T>` yapısıyla uyumlu. Herkese açık (anonim erişim dahil).

### Kapsam

**Dahil:** Blog başlık + açıklama LIKE araması, kategori filtresi, sayfalama, frontend arama kutusu + sonuç sayfası.
**Hariç:** Tam metin indeksleme (Elasticsearch/Solr), kategori/yazar adında arama, gerçek zamanlı öneri (autocomplete).

### User Story'ler

#### US-1: Blog Arama

Bir site ziyaretçisi olarak, arama kutusuna anahtar kelime yazarak blog yazılarını başlık ve içerik üzerinden aramak istiyorum, böylece ilgimi çeken konulara hızla ulaşabilirim.

**Kabul Kriterleri:**

Verilen ziyaretçi GET /api/blogs/search?q=dotnet&page=1&pageSize=10 isteği gönderdiğinde, başlık veya açıklamasında "dotnet" geçen aktif (silinmemiş) blogların sayfalı listesi 200 ile döner; yanıt yapısı items, totalCount, page, pageSize, totalPages içerir.

Verilen arama terimi boşluktan oluşuyorsa, 400 hatası "Arama terimi boş olamaz" mesajıyla döner.

Verilen q=dotnet&categoryId={gecerliId} gönderildiğinde, yalnızca o kategoriye ait eşleşmeler döner.

Verilen eşleşen sonuç yoksa, 200 ile boş items dizisi ve totalCount=0 döner.

Verilen q parametresi 200 karakteri aşıyorsa, 400 hatası döner.

#### US-2: Kategori Filtreli Arama

Bir site ziyaretçisi olarak, arama sonuçlarını kategori ile daraltmak istiyorum, böylece spesifik alanlardaki içeriklere odaklanabilirim.

**Kabul Kriterleri:**

Verilen GET /api/blogs/search?q=test&categoryId={gecerliId} gönderildiğinde, yalnızca o kategoriye ait eşleşmeler döner.

Verilen geçersiz UUID formatında categoryId gönderildiğinde, 400 hatası döner.

Verilen var olmayan ama geçerli UUID formatında categoryId gönderildiğinde, boş liste döner (404 değil).

### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| S1-B1 | `SearchBlogsQuery` record: q, page, pageSize, categoryId parametreleri | Backend/Application | INFRA1-B3 | 0.25 g |
| S1-B2 | `SearchBlogsQueryValidator`: q zorunlu, maxLength 200, pageSize 1–50 | Backend/Application | S1-B1 | 0.25 g |
| S1-B3 | `IBlogRepository.SearchAsync(q, categoryId, page, pageSize)` metodu arayüze ekle | Backend/Application | S1-B1 | 0.25 g |
| S1-B4 | `BlogRepository.SearchAsync` implementasyonu: EF Core LIKE query, projeksiyon, global filter otomatik devrede | Backend/Persistence | S1-B3, INFRA1-B3 | 0.75 g |
| S1-B5 | `SearchBlogsQueryHandler`: validator → repo → PagedResult map | Backend/Application | S1-B2, S1-B4 | 0.5 g |
| S1-B6 | `BlogsController`: GET `/api/blogs/search` endpoint'i ekle (public, [AllowAnonymous]) | Backend/WebApi | S1-B5 | 0.25 g |
| S1-F1 | `searchApi.ts`: `GET /api/blogs/search` için axios çağrısı | Frontend | S1-B6 | 0.25 g |
| S1-F2 | TanStack Query hook: `useSearchBlogs(params)` | Frontend | S1-F1 | 0.25 g |
| S1-F3 | SiteHeader arama kutusu bileşeni: debounce 300ms, minimum 2 karakter | Frontend | S1-F2 | 0.75 g |
| S1-F4 | `/search` sayfası (SearchResultsPage): loading/error/empty state'ler, sayfalama, kategori filtresi | Frontend | S1-F2 | 1 g |
| S1-F5 | paths.ts + router.tsx güncelleme: /search rotası | Frontend | S1-F4 | 0.25 g |
| S1-T1 | Integration testi: başlıkta eşleşme, içerikte eşleşme, kategori filtresi, boş sonuç, geçersiz q | Test | S1-B6 | 0.75 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp'e Blog Arama özelliği ekle — backend dikey dilim.

KAPSAM:
1. Features/Blogs/Search/ klasörü oluştur. SearchBlogsQuery record (q: string, page: int, pageSize: int, categoryId: Guid?) tanımla.
2. SearchBlogsQueryValidator: q zorunlu ve maxLength 200, pageSize 1–50 aralığında.
3. IBlogRepository arayüzüne SearchAsync(q, categoryId, page, pageSize, ct) imzası ekle.
4. BlogRepository'de EF Core LIKE implementasyonu: EF.Functions.Like ile başlık ve açıklamada arama. Soft delete global query filter otomatik devrede olduğundan silinmiş kayıtlar zaten hariç. DB seviyesinde projeksiyon, AsNoTracking kullan.
5. SearchBlogsQueryHandler: validator tetikle, repo çağır, PagedResult<BlogListItemResponse> döndür.
6. BlogsController: GET /api/blogs/search?q=...&page=1&pageSize=10&categoryId=... endpoint'i. Public (AllowAnonymous), query string parametreler.
7. Integration testler: başlık eşleşmesi, açıklama eşleşmesi, kategori filtresi, boş sonuç, q boş → 400, q>200 karakter → 400.

KISITLAR:
- Dikey dilim deseni: command/query record + static handler + FluentValidation validator + Mapperly mapper.
- Klasik namespace stili, XML doc Türkçe.
- PagedResult<T> mevcut Common/Pagination yapısını kullan.
```

#### Delegation Prompt — react-frontend-dev

```
GÖREV: ZnBlogApp frontend'ine Arama özelliği ekle.

KAPSAM:
1. client/src/lib/api/blogApi.ts: searchBlogs(params: SearchBlogsParams) fonksiyonu ekle — GET /api/blogs/search.
2. client/src/features/blog/queries.ts: useSearchBlogs(params) TanStack Query hook'u — q boşsa sorgu disable edilsin.
3. SiteHeader bileşenine arama kutusu ekle: debounce 300ms, minimum 2 karakter yazdıktan sonra sorgu tetiklenir, Enter veya arama butonuna basınca /search sayfasına yönlendirir.
4. client/src/pages/SearchResultsPage.tsx oluştur:
   - URL query param: ?q=...&categoryId=...&page=1
   - Blog kartları mevcut BlogListPage kartlarıyla tutarlı görünüm
   - Yükleniyor / hata / boş sonuç state'leri shadcn/ui Skeleton + Alert bileşenleriyle
   - Sayfalama mevcut paginationRange yardımcı fonksiyonuyla
   - Kategori filtresi (dropdown/select)
5. paths.ts: search: '/search' rotası ekle. router.tsx güncellensin.

KISITLAR:
- TypeScript strict mode. TanStack Query v5 sözdizimi. shadcn/ui bileşenler.
- Arama kutusu erişilebilir olmalı (aria-label, keyboard navigation).
```

---

## Özellik 2: Kullanıcı Yönetimi (Admin)

**MoSCoW:** Must
**Efor:** M (3–4 gün backend + frontend)
**Ön Koşul:** INFRA-1 (Soft Delete), INFRA-2 (Manager Rolü)

### Özet ve Amaç

Admin'in kayıtlı kullanıcıları sayfalı listeleyebilmesi, kullanıcı bilgilerini güncelleyebilmesi, soft delete ile silebilmesi ve yeni kullanıcı ekleyebilmesi.

### Kapsam

**Dahil:** Sayfalı kullanıcı listesi, kullanıcı güncelleme (ad/soyad/görsel), soft delete, admin tarafından kullanıcı oluşturma.
**Hariç:** Şifre sıfırlama e-postası, kullanıcı profil sayfası (public), kullanıcının kendi profilini düzenlemesi.

### User Story'ler

#### US-3: Kullanıcıları Listeleme

Bir admin olarak, kayıtlı tüm kullanıcıları sayfalı listede görmek istiyorum, böylece sistem kullanıcılarını yönetebilirim.

**Kabul Kriterleri:**

Verilen geçerli Admin token'ı ile GET /api/admin/users?page=1&pageSize=20 çağrıldığında, kullanıcıların (Id, FirstName, LastName, Email, Roles, CreatedAt, IsDeleted) listesi PagedResult olarak 200 ile döner.

Verilen token olmadan çağrıldığında, 401 döner.

Verilen User rolündeki kullanıcı token'ıyla çağrıldığında, 403 döner.

Verilen includeDeleted=true parametresi eklendiğinde, soft delete edilmiş kullanıcılar da listede görünür.

#### US-4: Kullanıcı Soft Delete

Bir admin olarak, kullanıcıyı sistemden kaldırmak istiyorum (soft delete), böylece kullanıcının içerikleri kaybolmadan hesabı devre dışı kalır.

**Kabul Kriterleri:**

Verilen Admin token'ı ile DELETE /api/admin/users/{id} çağrıldığında, kullanıcının IsDeleted=true ve DeletedAt set edilir; 204 döner.

Verilen soft delete edilmiş kullanıcı giriş yapmaya çalışırsa, 401 "Hesap devre dışı" döner.

Verilen var olmayan kullanıcı Id'si ile çağrıldığında, 404 döner.

Verilen Admin kullanıcısı kendi hesabını silmeye çalışırsa, 400 "Kendi hesabınızı silemezsiniz" döner.

#### US-5: Kullanıcı Güncelleme

Bir admin olarak, kullanıcının ad, soyad ve profil görselini güncellemek istiyorum.

**Kabul Kriterleri:**

Verilen geçerli veri ile PUT /api/admin/users/{id} çağrıldığında, güncelleme uygulanır ve 200 + güncellenmiş kullanıcı bilgisi döner.

Verilen boş firstName veya lastName gönderildiğinde, 400 ValidationProblemDetails döner.

Verilen var olmayan kullanıcı Id'si ile çağrıldığında, 404 döner.

#### US-6: Admin Tarafından Kullanıcı Oluşturma

Bir admin olarak, e-posta ve şifre belirterek yeni kullanıcı oluşturmak istiyorum, böylece sisteme önceden kayıtlı hesaplar ekleyebilirim.

**Kabul Kriterleri:**

Verilen geçerli veri ile POST /api/admin/users çağrıldığında, kullanıcı oluşturulur, varsayılan "User" rolü atanır; 201 + kullanıcı Id'si döner.

Verilen zaten kayıtlı e-posta gönderildiğinde, 409 Conflict döner.

Verilen şifre politikasına uymayan değer gönderildiğinde, 400 + kural ihlali detayı döner.

### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| UM-B1 | `GetUsersQuery` + `GetUsersQueryHandler`: sayfalı, includeDeleted parametreli, rol bilgisi dahil | Backend/Application | INFRA1-B3 | 0.75 g |
| UM-B2 | `UpdateUserCommand` + validator + handler | Backend/Application | — | 0.5 g |
| UM-B3 | `SoftDeleteUserCommand` + handler: self-delete engeli, login kontrolü | Backend/Application | INFRA1-B2 | 0.5 g |
| UM-B4 | `CreateUserByAdminCommand` + validator + handler (UserManager.CreateAsync + rol ataması) | Backend/Application | — | 0.5 g |
| UM-B5 | Login handler'ında soft delete kontrolü: IsDeleted=true ise 401 döndür | Backend/Application | INFRA1-B2 | 0.25 g |
| UM-B6 | `AdminUsersController`: GET/POST /api/admin/users, PUT/DELETE /api/admin/users/{id} | Backend/WebApi | UM-B1..B4, INFRA2-B1 | 0.5 g |
| UM-F1 | `userApi.ts`: admin kullanıcı CRUD API çağrıları | Frontend | UM-B6 | 0.25 g |
| UM-F2 | TanStack Query hooks: useAdminUsers, useUpdateUser, useSoftDeleteUser, useCreateUser | Frontend | UM-F1 | 0.5 g |
| UM-F3 | `/admin/users` sayfası: sayfalı tablo, arama/filtre, soft delete butonu, yeni kullanıcı formu | Frontend | UM-F2, INFRA2-F2 | 1.5 g |
| UM-F4 | paths.ts + router.tsx: adminUsers rotası (Admin-only) | Frontend | UM-F3 | 0.25 g |
| UM-T1 | Integration testleri: listeleme (200/401/403), soft delete (204/404/400 self), güncelleme, oluşturma, soft delete sonrası login engeli | Test | UM-B6 | 1 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp'e Admin Kullanıcı Yönetimi backend dikey dilimi ekle.

KAPSAM:
1. Features/Users/ klasörü oluştur. Şu dikey dilimleri yaz:
   a. GetUsers: GetUsersQuery(page, pageSize, includeDeleted=false) + handler. Response: Id, FirstName, LastName, Email, ImageUrl, CreatedAt, IsDeleted, DeletedAt, Roles (string listesi).
   b. UpdateUser: UpdateUserCommand(userId, firstName, lastName, imageUrl) + validator + handler.
   c. SoftDeleteUser: SoftDeleteUserCommand(targetUserId, requestingUserId) + handler. Kendi kendini silmeye çalışırsa Validation hatası döndür.
   d. CreateUserByAdmin: CreateUserByAdminCommand(firstName, lastName, email, password) + validator + handler. UserManager.CreateAsync + "User" rolü ata.
2. Features/Auth/Login handler'ına: kullanıcı IsDeleted=true ise Unauthorized hatası döndür.
3. AdminUsersController: [Authorize(Roles = "Admin")] — tüm action'lar yalnızca Admin.
   - GET /api/admin/users?page&pageSize&includeDeleted
   - GET /api/admin/users/{id}
   - POST /api/admin/users
   - PUT /api/admin/users/{id}
   - DELETE /api/admin/users/{id}
4. Wolverine opaque servisler için AlwaysUseServiceLocationFor<UserManager<User>> zaten kayıtlı olmalı; kontrol et.
5. Integration testleri: her endpoint için happy path + 401 + 403 + 404; soft delete sonrası login 401.

KISITLAR:
- Dikey dilim + Wolverine + FluentValidation + Mapperly.
- Handler yetki sırası: önce 404 sonra 403.
- Klasik namespace, XML doc Türkçe.
- UserManager/SignInManager için AlwaysUseServiceLocationFor gerekiyorsa ekle.
```

#### Delegation Prompt — react-frontend-dev

```
GÖREV: ZnBlogApp admin paneline Kullanıcı Yönetimi sayfası ekle.

KAPSAM:
1. client/src/lib/api/userApi.ts: getAdminUsers, updateUser, softDeleteUser, createUser axios fonksiyonları.
2. client/src/features/user/queries.ts: useAdminUsers, useUpdateUser, useSoftDeleteUser, useCreateUser TanStack Query hook'ları.
3. client/src/pages/admin/AdminUsersPage.tsx:
   - Sayfalı tablo: Ad Soyad, E-posta, Roller, Kayıt Tarihi, Durum (aktif/silinmiş)
   - "Silinmişleri Göster" toggle
   - Her satırda düzenle ve sil (soft) butonları
   - Sil işlemi: shadcn/ui AlertDialog onay modalı
   - Yeni Kullanıcı Ekle: shadcn/ui Dialog içinde form (ad, soyad, e-posta, şifre)
   - loading/error/empty state'ler
4. paths.ts: adminUsers: '/admin/users' ekle.
5. router.tsx: /admin/users rotası — ProtectedRoute requireRole=['Admin'].
6. AdminLayout menü: "Kullanıcılar" linki (yalnızca isAdmin gösterilsin).

KISITLAR:
- TypeScript strict. shadcn/ui Table, Dialog, AlertDialog, Badge bileşenleri.
- TanStack Query optimistic update veya invalidateQueries tercih et.
```

---

## Özellik 3: Rol / Yetki Yönetimi + Manager Rolü

**MoSCoW:** Must
**Efor:** S (2–3 gün backend + frontend)
**Ön Koşul:** INFRA-2 (Manager Rolü altyapısı), UM-B6 (AdminUsersController var)

### Özet ve Amaç

Admin'in kullanıcılara rol atayabilmesi, rol kaldırabilmesi. Manager rolünün sisteme tanıtılması (INFRA-2 bağımlılığı). Yetkili kullanıcıların (Admin/Manager) listelenmesi.

### Kapsam

**Dahil:** Kullanıcıya rol atama/kaldırma (Admin-only), yetkili kullanıcıları listeleme.
**Hariç:** Özel izin (claim) sistemi, kullanıcı başına granüler yetki.

### User Story'ler

#### US-7: Kullanıcıya Rol Atama

Bir admin olarak, kullanıcıya rol atamak veya rolünü kaldırmak istiyorum, böylece yetki seviyesini yönetebilirim.

**Kabul Kriterleri:**

Verilen Admin token'ı ile POST /api/admin/users/{id}/roles body {role: "Manager"} gönderildiğinde, kullanıcıya Manager rolü atanır; 200 + güncellenmiş roller listesi döner.

Verilen aynı endpoint ile DELETE /api/admin/users/{id}/roles/{roleName} çağrıldığında, rol kaldırılır; 200 döner.

Verilen tanımsız bir rol adı gönderildiğinde (RoleNames.All'da olmayan), 400 döner.

Verilen Admin kullanıcısının kendi Admin rolü kaldırılmaya çalışılırsa, 400 "Son admin rolü kaldırılamaz" (sistem bütünlüğü için en az bir Admin zorunlu) döner.

#### US-8: Yetkili Kullanıcıları Listeleme

Bir admin olarak, Admin ve Manager rolüne sahip kullanıcıları listelemek istiyorum.

**Kabul Kriterleri:**

Verilen GET /api/admin/privileged-users çağrıldığında, Admin veya Manager rolüne sahip kullanıcıların listesi döner.

Verilen User token'ıyla çağrıldığında, 403 döner.

### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| RM-B1 | `AssignRoleCommand` + validator + handler: RoleNames.All kontrolü, son-admin engeli | Backend/Application | INFRA2-B1 | 0.5 g |
| RM-B2 | `RemoveRoleCommand` + validator + handler: son-admin engeli | Backend/Application | INFRA2-B1 | 0.5 g |
| RM-B3 | `GetPrivilegedUsersQuery` + handler: Admin + Manager rolündeki kullanıcılar | Backend/Application | INFRA2-B1 | 0.25 g |
| RM-B4 | AdminUsersController endpoint'leri ekle: POST /api/admin/users/{id}/roles, DELETE /api/admin/users/{id}/roles/{roleName}, GET /api/admin/privileged-users | Backend/WebApi | RM-B1..B3 | 0.5 g |
| RM-F1 | `roleApi.ts` veya userApi.ts güncelleme: assignRole, removeRole, getPrivilegedUsers | Frontend | RM-B4 | 0.25 g |
| RM-F2 | AdminUsersPage'e rol yönetimi bölümü: her kullanıcı satırında mevcut roller + atama/kaldırma butonları | Frontend | RM-F1, UM-F3 | 0.75 g |
| RM-F3 | Ayrı `/admin/roles` özet sayfası (opsiyonel — Could) | Frontend | RM-F2 | 0.5 g |
| RM-T1 | Integration testleri: rol atama/kaldırma, geçersiz rol 400, son-admin engeli 400 | Test | RM-B4 | 0.5 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp'e Rol Yönetimi backend dikey dilimi ekle.

KAPSAM:
1. Features/Users/AssignRole: AssignRoleCommand(targetUserId, roleName) + validator + handler.
   - Validator: roleName RoleNames.All listesinde olmalı.
   - Handler: RoleManager.RoleExistsAsync kontrol, UserManager.AddToRoleAsync.
2. Features/Users/RemoveRole: RemoveRoleCommand(targetUserId, roleName) + validator + handler.
   - Handler: Admin rolü kaldırılacaksa, sistemde başka Admin yoksa Validation hatası döndür ("Son admin rolü kaldırılamaz").
3. Features/Users/GetPrivilegedUsers: GetPrivilegedUsersQuery + handler. UserManager.GetUsersInRoleAsync("Admin") ∪ GetUsersInRoleAsync("Manager") döndür, deduplikasyon yap.
4. AdminUsersController'a şu endpoint'leri ekle:
   - POST /api/admin/users/{id}/roles
   - DELETE /api/admin/users/{id}/roles/{roleName}
   - GET /api/admin/privileged-users
5. Integration testleri.

KISITLAR:
- Tüm rol yönetimi endpoint'leri [Authorize(Roles = "Admin")] — yalnızca Admin.
- Klasik namespace, Wolverine, FluentValidation, Mapperly.
```

---

## Özellik 4: Beğeni (Like) ve Paylaşım

**MoSCoW:** Should
**Efor:** L (5–6 gün backend + frontend)
**Ön Koşul:** INFRA-1 (Soft Delete), INFRA-2 (Manager Rolü) tamamlanmış olmalı (yetki politikası için)

### Özet ve Amaç

Giriş yapmış kullanıcıların blog yazılarını ve yorumları beğenebilmesi (idempotent — bir kez beğen/geri al). Ayrıca blog paylaşım linkleri (Web Share API / sosyal paylaşım butonları).

### Kapsam

**Dahil:** BlogLike entity (blog başına tek like/unlike), CommentLike entity, idempotent toggle endpoint'leri, beğeni sayısının listeleme sorgularına eklenmesi, frontend like butonu, sosyal paylaşım linkleri.
**Hariç:** Beğeni bildirimleri (SignalR), SubComment like (Could seviyesi), like leaderboard/analytics.

### User Story'ler

#### US-9: Blog Beğenme

Giriş yapmış bir kullanıcı olarak, bir blog yazısını beğenmek istiyorum; tekrar tıklarsam beğenimden vazgeçebilmeliyim (toggle), böylece içerik kalitesine katkı sağlayabilirim.

**Kabul Kriterleri:**

Verilen giriş yapmış kullanıcı POST /api/blogs/{id}/like gönderdiğinde, blog beğenilir; 200 + {liked: true, likeCount: N} döner.

Verilen aynı kullanıcı aynı bloga tekrar POST /api/blogs/{id}/like gönderdiğinde, beğeni kaldırılır; 200 + {liked: false, likeCount: N-1} döner. (İdempotent toggle)

Verilen anonim kullanıcı çağrırdığında, 401 döner.

Verilen var olmayan blog Id'si ile çağrıldığında, 404 döner.

Verilen public GET /api/blogs listesi veya /api/blogs/{id} çağrıldığında, her blog için likeCount ve (giriş yapmışsa) isLikedByCurrentUser bilgisi döner.

#### US-10: Yorum Beğenme

Giriş yapmış bir kullanıcı olarak, bir yorumu beğenmek istiyorum, böylece kaliteli yorumları öne çıkarabilirim.

**Kabul Kriterleri:**

Verilen POST /api/comments/{id}/like gönderildiğinde, toggle mantığıyla yorum beğenisi eklenir/kaldırılır; 200 + {liked: bool, likeCount: N} döner.

Verilen anonim kullanıcı çağrırdığında, 401 döner.

Verilen yorum listesi (GET /api/blogs/{id}/comments) döndüğünde, her yorumda likeCount ve isLikedByCurrentUser alanları bulunur.

#### US-11: Blog Sosyal Paylaşımı

Bir site ziyaretçisi olarak, blog yazısını sosyal medyada paylaşmak istiyorum.

**Kabul Kriterleri:**

Verilen blog detay sayfasında paylaş butonuna tıklandığında, Web Share API destekleyen tarayıcılarda native paylaşım ekranı açılır.

Verilen Web Share API desteklenmeyen tarayıcıda, Twitter/X ve LinkedIn paylaşım linkleri açılır.

Verilen paylaşım URL'si, ilgili blog'un canonical URL'ini içerir.

### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| LK-B1 | `BlogLike` entity: BlogId (FK), UserId (FK), CreatedAt — composite unique key (BlogId, UserId) | Backend/Domain | — | 0.5 g |
| LK-B2 | `CommentLike` entity: CommentId (FK), UserId (FK), CreatedAt — composite unique key | Backend/Domain | — | 0.5 g |
| LK-B3 | EF Core konfigürasyon + migration: BlogLikes ve CommentLikes tablolar, composite unique index | Backend/Persistence | LK-B1, LK-B2 | 0.5 g |
| LK-B4 | `ToggleBlogLikeCommand` + handler: upsert/delete pattern, idempotent; dönen DTO: liked, likeCount | Backend/Application | LK-B3 | 0.75 g |
| LK-B5 | `ToggleCommentLikeCommand` + handler | Backend/Application | LK-B3 | 0.5 g |
| LK-B6 | Blog list/detail response'larına likeCount + isLikedByCurrentUser eklenmesi (opsiyonel userId parametresiyle) | Backend/Application | LK-B3 | 0.5 g |
| LK-B7 | Comment list response'larına likeCount + isLikedByCurrentUser eklenmesi | Backend/Application | LK-B3 | 0.5 g |
| LK-B8 | `BlogsController`: POST /api/blogs/{id}/like, `CommentsController`: POST /api/comments/{id}/like — her ikisi [Authorize] | Backend/WebApi | LK-B4, LK-B5 | 0.25 g |
| LK-F1 | `likeApi.ts`: toggleBlogLike, toggleCommentLike | Frontend | LK-B8 | 0.25 g |
| LK-F2 | TanStack Query: useToggleBlogLike, useToggleCommentLike (optimistic update) | Frontend | LK-F1 | 0.5 g |
| LK-F3 | BlogCard + BlogDetail: like butonu bileşeni (beğenildi/beğenilmedi state, sayaç) | Frontend | LK-F2 | 0.75 g |
| LK-F4 | BlogDetailPage: yorum listesinde like butonu | Frontend | LK-F2 | 0.5 g |
| LK-F5 | ShareButton bileşeni: Web Share API + fallback Twitter/LinkedIn linkleri | Frontend | — | 0.5 g |
| LK-T1 | Integration testleri: toggle (2x like = unlike), anonim 401, var olmayan kayıt 404, likeCount doğruluğu | Test | LK-B8 | 0.75 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp'e Blog ve Yorum Beğeni özelliği ekle — backend dikey dilim.

KAPSAM:
1. Zn.Domain: BlogLike entity (BlogId, UserId, CreatedAt; composite unique key BlogId+UserId) ve CommentLike entity (CommentId, UserId, CreatedAt; composite unique key) oluştur. BaseEntity miras almaz — bu entity'ler sadece ilişki tablosudur (Id yok, composite PK).

   Teknik Karar Noktası: Composite PK mı yoksa surrogate Guid Id + unique index mı tercih edileceğini ekiple tartış. Önerimiz composite PK — surrogate Id gereksiz.

2. EF Core konfigürasyon + migration: BlogLikes, CommentLikes tabloları, composite unique index veya PK.

3. Features/Blogs/ToggleLike: ToggleBlogLikeCommand(blogId, userId) + handler.
   - Mevcut like varsa sil (unlike), yoksa ekle (like).
   - Dönen DTO: { liked: bool, likeCount: int }
   - [Authorize] gerektirir.

4. Features/Comments/ToggleLike: ToggleCommentLikeCommand(commentId, userId) + handler. Aynı toggle mantığı.

5. Blog list/detail response'larına likeCount ekle (userId varsa isLikedByCurrentUser da).
   Comment list response'larına likeCount + isLikedByCurrentUser ekle.

6. BlogsController: POST /api/blogs/{id}/like
   CommentsController: POST /api/comments/{id}/like

7. Integration testleri: toggle çift tıklama unlike, anonim 401, 404, likeCount senkronluğu.

KISITLAR:
- Dikey dilim + Wolverine + FluentValidation (blogId/commentId boş olamaz) + Mapperly.
- Klasik namespace, XML doc Türkçe.
- Concurrency: aynı kullanıcının eş zamanlı iki like isteği duplicate vermemeli (unique constraint + exception handler).
```

#### Delegation Prompt — react-frontend-dev

```
GÖREV: ZnBlogApp frontend'ine Like butonu ve Sosyal Paylaşım özelliği ekle.

KAPSAM:
1. client/src/lib/api/likeApi.ts: toggleBlogLike(blogId), toggleCommentLike(commentId).
2. Features/like: useToggleBlogLike, useToggleCommentLike — optimistic update (likeCount ±1, liked toggle) + hata durumunda rollback.
3. LikeButton bileşeni (client/src/components/common/LikeButton.tsx):
   - Props: liked (bool), count (number), onToggle (fn), disabled (bool — anonim kullanıcı için)
   - Beğenildi: dolu kalp ikonu + kırmızı renk
   - Beğenilmedi: boş kalp ikonu
   - Anonim kullanıcı tıklarsa /login sayfasına yönlendir veya toast uyarısı göster
4. BlogCard bileşenine LikeButton ekle.
5. BlogDetailPage: başlık altına LikeButton ekle; yorum listesindeki her CommentItem bileşenine LikeButton ekle.
6. ShareButton bileşeni (client/src/components/common/ShareButton.tsx):
   - Web Share API kontrolü (navigator.share)
   - Desteklenmiyorsa: Twitter/X ve LinkedIn paylaşım URL'leri ikon butonları
   - BlogDetailPage başlık altına ekle.

KISITLAR:
- TypeScript strict. TanStack Query v5 optimistic update.
- Anonim kullanıcı için isAuthenticated kontrolü (useAuth hook'u kullan).
- shadcn/ui Button, Tooltip bileşenler. Erişilebilir (aria-label).
```

---

## Özellik 5: Kategori Yönetimi (Soft Delete + Manager Yetkisi)

**MoSCoW:** Must
**Efor:** S (1.5–2 gün backend + frontend)
**Ön Koşul:** INFRA-1 (Soft Delete), INFRA-2 (Manager Rolü)

### Özet ve Amaç

Mevcut Category CRUD'una soft delete ve Manager yetkisi eklenmesi. Mevcut `DeleteCategoryCommand` hard delete'ten soft delete'e dönüştürülür.

### Kapsam

**Dahil:** DeleteCategoryCommand'ı soft delete'e çevirme, admin listesinde silinmişleri gösterme, Manager yetkisi.
**Hariç:** Kategori geri yükleme (restore) — Could seviyesi.

### User Story'ler

#### US-12: Kategori Soft Delete

Bir admin veya manager olarak, bir kategoriyi silmek istiyorum; kategoriye bağlı bloglar etkilenmesin, gerektiğinde kurtarılabilsin.

**Kabul Kriterleri:**

Verilen bağlı blogu olan bir kategori soft delete edildiğinde, IsDeleted=true set edilir; 204 döner. (Blog.Category ilişkisi DeleteBehavior.Restrict olduğundan hard delete hata verirdi — soft delete bu sorunu çözer.)

Verilen public GET /api/categories çağrıldığında, silinmiş kategoriler listelenmez.

Verilen Admin veya Manager token'ı ile DELETE /api/admin/categories/{id} çağrıldığında, soft delete uygulanır.

Verilen var olmayan Id ile çağrıldığında, 404 döner.

#### US-13: Admin Kategori Listesi (Silinmişler Dahil)

Bir admin olarak, silinmiş kategorileri de görmek istiyorum.

**Kabul Kriterleri:**

Verilen GET /api/admin/categories?includeDeleted=true çağrıldığında, silinmiş kategoriler de listede görünür; IsDeleted ve DeletedAt bilgisi response'ta bulunur.

### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| CAT-B1 | `DeleteCategoryCommand` handler'ını hard delete'ten soft delete'e çevir | Backend/Application | INFRA1-B2 | 0.25 g |
| CAT-B2 | `GetCategoriesQuery` ve `GetCategoriesQueryHandler`: includeDeleted parametresi ekle | Backend/Application | INFRA1-B3 | 0.25 g |
| CAT-B3 | AdminCategoriesController: [Authorize("Admin,Manager")] güncelle; includeDeleted query param ekle | Backend/WebApi | INFRA2-B1, CAT-B1 | 0.25 g |
| CAT-F1 | AdminCategoriesPage: silinmiş kategorileri göster toggle, silme butonu onay dialog'u | Frontend | CAT-B3 | 0.5 g |
| CAT-T1 | Integration testi: soft delete 204, public listede görünmeme, admin listede includeDeleted ile görünme, bağlı blog varken silme (hard delete engeli yokluğunu doğrula) | Test | CAT-B3 | 0.5 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp Category CRUD'una soft delete ve Manager yetkisi ekle.

KAPSAM:
1. Features/Categories/Delete/DeleteCategoryCommandHandler: `context.Categories.Remove()` yerine `category.SoftDelete()` + SaveChangesAsync kullan.
2. Features/Categories/GetAll/GetCategoriesQueryHandler: includeDeleted parametresini işle — true ise IgnoreQueryFilters() uygula.
3. AdminCategoriesController: [Authorize(Roles = "Admin,Manager")] olarak güncelle. GET endpoint'ine includeDeleted query parametresi ekle.
4. Dikkat: Blog.Category ilişkisi DeleteBehavior.Restrict tanımlı. Soft delete bu constraint'i tetiklemez — doğru davranış.
5. Integration testleri: soft delete 204, public listede görünmeme, bağlı blogu olan kategorinin silinmesi (hard delete'te hata verirdi; soft delete ile sorunsuz).

KISITLAR:
- Sadece handler + controller güncelleme; entity factory değişmiyor (SoftDelete mutator INFRA-1'de eklendi).
- Klasik namespace, XML doc Türkçe.
```

---

## Özellik 6: Blog Yönetimi (Soft Delete + Manager Yetkisi)

**MoSCoW:** Must
**Efor:** S (1.5–2 gün backend + frontend)
**Ön Koşul:** INFRA-1 (Soft Delete), INFRA-2 (Manager Rolü)

### Özet ve Amaç

Mevcut Blog CRUD'una soft delete ve Manager yetkisi eklenmesi. Çok yazarlı model korunur: yazar kendi blogunu silebilir; admin her blogu silebilir; manager yalnızca kendi blogunu silebilir (A6 matrisi).

### User Story'ler

#### US-14: Blog Soft Delete

Bir admin/manager (veya yazarın kendisi) olarak, bir blog yazısını soft delete ile kaldırmak istiyorum.

**Kabul Kriterleri:**

Verilen blog yazısının yazarı DELETE /api/admin/blogs/{id} çağırdığında, kendi blogu soft delete edilir; 204 döner.

Verilen Admin DELETE /api/admin/blogs/{id} çağırdığında, herhangi bir blog soft delete edilir; 204 döner.

Verilen Manager, başkasının blogunu silmeye çalıştığında, 403 döner.

Verilen public GET /api/blogs çağrıldığında, silinmiş bloglar listelenmez.

#### US-15: Admin Blog Listesi (Silinmişler Dahil)

Bir admin olarak, silinmiş blogları da görmek istiyorum.

**Kabul Kriterleri:**

Verilen GET /api/admin/blogs?includeDeleted=true çağrıldığında, silinmiş bloglar da listelenir; IsDeleted bilgisi response'ta bulunur.

### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| BL-B1 | `DeleteBlogCommand` handler'ını soft delete'e çevir; yetki: yazar veya Admin; Manager yalnızca kendi bloğunu silebilir | Backend/Application | INFRA1-B2, INFRA2-B1 | 0.5 g |
| BL-B2 | `GetBlogsQuery` ve handler: includeDeleted ekle (admin sorgusu için) | Backend/Application | INFRA1-B3 | 0.25 g |
| BL-B3 | AdminBlogsController: Manager yetkisi ekle; includeDeleted parametresi; delete endpoint'i güncelle | Backend/WebApi | INFRA2-B1, BL-B1 | 0.25 g |
| BL-F1 | AdminBlogListPage: silinmiş bloglar toggle, silme onay dialog'u | Frontend | BL-B3 | 0.5 g |
| BL-T1 | Integration testleri: yazar siler 204, admin başkasını siler 204, manager başkasını silemez 403, public listede görünmeme | Test | BL-B3 | 0.5 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp Blog CRUD'una soft delete ve Manager yetkisi ekle.

KAPSAM:
1. Features/Blogs/Delete/DeleteBlogCommandHandler:
   - Kayıt bulunamazsa 404.
   - Yetki kontrolü (A6 matrisine göre):
     a. Yazar → kendi blogunu soft delete edebilir (Admin ve Manager dahil).
     b. Admin → herhangi bir blogu soft delete edebilir.
     c. Manager → yalnızca kendi blogunu silebilir; başkasının bloğu için 403.
   - `blog.SoftDelete()` çağır + SaveChanges.
2. Features/Blogs/GetAll/GetBlogsQueryHandler: includeDeleted parametresiyle IgnoreQueryFilters() seçeneği ekle.
3. AdminBlogsController (veya mevcut blog controller yetki update): Manager rolünü dahil et; includeDeleted query param ekle.
4. Integration testleri: 4 senaryo (yazar siler, admin siler, manager kendi siler, manager başkasını silemez).

KISITLAR:
- Blog silmek yorumları/sub-yorumları kaldırmaz (soft delete kaskatına gitmez); bağlı kayıtlar kalır.
- Klasik namespace, Wolverine, XML doc Türkçe.
```

---

## Özellik 7: Mesaj Yönetimi (Soft Delete + Manager Yetkisi)

**MoSCoW:** Should
**Efor:** XS (0.75 gün backend + frontend)
**Ön Koşul:** INFRA-1 (Soft Delete), INFRA-2 (Manager Rolü)

### Özet ve Amaç

Mevcut mesaj yönetimine soft delete ve Manager yetkisi eklenmesi.

### User Story'ler

#### US-16: Mesaj Soft Delete

Bir admin veya manager olarak, işlediğim iletişim mesajlarını soft delete ile arşivlemek istiyorum.

**Kabul Kriterleri:**

Verilen Admin veya Manager token'ı ile DELETE /api/admin/messages/{id} çağrıldığında, mesaj soft delete edilir; 204 döner.

Verilen var olmayan Id ile çağrıldığında, 404 döner.

Verilen Admin mesaj listesinde includeDeleted=true gönderildiğinde, silinmiş mesajlar da listelenir.

### Görev Kırılımı

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| MSG-B1 | `DeleteMessageCommand` + handler (soft delete) | Backend/Application | INFRA1-B2 | 0.25 g |
| MSG-B2 | `GetMessagesQueryHandler`: includeDeleted parametresi | Backend/Application | INFRA1-B3 | 0.25 g |
| MSG-B3 | AdminMessagesController: [Authorize("Admin,Manager")], DELETE endpoint ekle, includeDeleted | Backend/WebApi | INFRA2-B1, MSG-B1 | 0.25 g |
| MSG-F1 | AdminMessagesPage: silme butonu + onay dialog'u, silinmiş mesajlar toggle | Frontend | MSG-B3 | 0.5 g |
| MSG-T1 | Integration testi: soft delete 204, 404, admin + manager yetki doğrulaması | Test | MSG-B3 | 0.25 g |

#### Delegation Prompt — dotnet-blog-backend-dev

```
GÖREV: ZnBlogApp Mesaj Yönetimine soft delete ve Manager yetkisi ekle.

KAPSAM:
1. Features/Messages/Delete/DeleteMessageCommand + handler: message.SoftDelete() + SaveChanges.
2. GetMessagesQueryHandler: includeDeleted parametresi.
3. AdminMessagesController: [Authorize(Roles = "Admin,Manager")]; DELETE /api/admin/messages/{id}; includeDeleted query param.
4. Integration testleri: 204 soft delete, 404, Manager 200 (mesaj listesi), User 403.

KISITLAR: Klasik namespace, Wolverine, XML doc Türkçe.
```

---

## Önceliklendirme ve Sıralama (MoSCoW + Bağımlılık)

```
AŞAMA 1 — Kesişen Altyapı (paralel yürütülebilir)
  INFRA-1: Soft Delete altyapısı          [Must, 3 gün]
  INFRA-2: Manager Rolü altyapısı         [Must, 2 gün]

AŞAMA 2 — Must Özellikler (INFRA-1 + INFRA-2 tamamlanınca)
  Özellik 2: Kullanıcı Yönetimi          [Must, 3.5 gün]
  Özellik 3: Rol/Yetki Yönetimi          [Must, 2 gün]
  Özellik 5: Kategori Soft Delete+Manager [Must, 1.5 gün]
  Özellik 6: Blog Soft Delete+Manager     [Must, 1.5 gün]

AŞAMA 3 — Should Özellikler (Aşama 2 bitmeden paralel başlanabilir)
  Özellik 1: Arama                        [Should, 3.5 gün]
  Özellik 4: Like + Paylaşım              [Should, 5.5 gün]
  Özellik 7: Mesaj Soft Delete+Manager    [Should, 1 gün]
```

**Toplam tahmin (tek geliştirici):** ~25–28 iş günü. Paralel backend/frontend çalışmasıyla ~14–16 iş gününe indirilebilir.

### RICE Skoru Özeti

| Özellik | Reach | Impact | Confidence | Effort | RICE |
|---|---|---|---|---|---|
| Soft Delete Altyapısı | 5 | 5 | 5 | 2 | 62.5 — altyapı, ertelenirse tüm plan bloke |
| Manager Rolü Altyapısı | 4 | 5 | 5 | 1 | 100 — yüksek impact, düşük efor |
| Blog Yönetimi (SD+Manager) | 5 | 5 | 5 | 2 | 62.5 — blog merkezi içerik |
| Kategori Yönetimi (SD+Manager) | 4 | 4 | 5 | 1 | 80 |
| Kullanıcı Yönetimi | 3 | 5 | 4 | 3.5 | 17 — admin odaklı, daha yüksek efor |
| Rol Yönetimi | 3 | 5 | 4 | 2 | 30 |
| Arama | 5 | 4 | 4 | 3.5 | 23 — yüksek kullanıcı değeri |
| Like + Paylaşım | 5 | 3 | 4 | 5.5 | 11 — en yüksek efor |
| Mesaj Soft Delete | 2 | 3 | 5 | 1 | 30 |

---

## Riskler ve Açık Sorular

| # | Risk | Seviye | Önlem |
|---|---|---|---|
| R1 | Soft delete global query filter + IgnoreQueryFilters yanlış uygulanırsa admin endpoint'i de silinmişleri kaçırır | Yüksek | Her entity için birim + integration test; AdminController testlerinde includeDeleted=true senaryosu zorunlu |
| R2 | BlogLike/CommentLike concurrent insert: aynı anda iki like isteği unique constraint ihlali atar | Orta | DB unique constraint + exception handling (Conflict → idempotent 200 döndür); integration testinde simüle et |
| R3 | Manager rolü müdahalesi mevcut integration testlerini bozabilir | Orta | Mevcut 104 test Manager rolüyle ilgili kısıtları test etmiyor; INFRA-2 sonrası regresyon testi çalıştır |
| R4 | Kullanıcı soft delete + login engeli: LoginCommandHandler'da IsDeleted kontrolü eklenmezse silinmiş kullanıcı giriş yapabilir | Yüksek | UM-B5 görevi; integration testi zorunlu |
| R5 | Arama EF LIKE sorgusu büyük veri setinde yavaş kalabilir | Düşük (ilk aşama) | İlk aşamada MaxPageSize=20 sınırı; ileride full-text index/Elasticsearch kararı (A11) |
| R6 | DeleteBehavior.Restrict (Blog→Category): Kategori soft delete edilse bile hard delete çağrılırsa kısıt hata verir | Bilgi | Soft delete bu kısıtı tetiklemez — ancak geliştirici hard delete bırakırsa tetiklenir. CAT-B1 görevi hard delete'i tamamen kaldırmalı |
| R7 | A6 yetki matrisi henüz ekip onayı almadı | Yüksek (ürün riski) | Geliştirmeye başlamadan önce matrisin onaylanması zorunlu; özellikle "Manager kendi olmayan bloğu silemez" kuralı |
| R8 | Frontend ProtectedRoute genişletmesi requireAdmin kullanan mevcut sayfaları bozabilir | Orta | Geriye dönük uyumluluk: requireAdmin=true → requireRole=['Admin'] olarak map et |

---

*Bu belge yalnızca planlama çıktısıdır. Kod içermez. Tüm görevler ilgili uzman agent'lara delegation prompt'larıyla devredilmelidir. Açık kararlar (A6–A11) onaylanmadan ilgili özellik geliştirmesine başlanmamalıdır.*

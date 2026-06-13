# Özellik: Admin Panel — Audit Alanları, Arama Logu ve Rol Yönetimi

> Belge tarihi: 13 Haziran 2026.
> Faz 5 (A6–A11) tamamlandıktan sonra gelen ikinci genişleme paketi.
> Bu belge **yalnızca planlama dokümantasyonudur**; kod içermez.

---

## Mevcut Durum Tablosu

Aşağıdaki tablo, codebase'den doğrudan tespit edilen gerçek durumu yansıtır.

| Özellik Alanı | Durum | Dosya / Kanıt |
|---|---|---|
| Blog CRUD (çok yazarlı, soft-delete) | **Var** | `BlogsController`, `AdminCategoriesController`, `Blog.cs` |
| Blog alanları | **Var:** Title, Description, CoverImage, BlogImage, CategoryId, UserId, CreatedAt, UpdatedAt, IsDeleted, DeletedAt | `Blog.cs` |
| Blog'da IP adresi alanı | **Eksik** | `Blog.cs` — IP alanı yok |
| Blog admin görünümünde yazar + audit | Kısmen: AuthorId, AuthorName, CreatedAt `BlogDetail`'da mevcut; IP yok | `BlogDetail.cs` |
| Mesaj CRUD (soft-delete) | **Var** | `AdminMessagesController`, `Message.cs` |
| Mesaj alanları | **Var:** Name, Email, Subject, MessageBody, IsRead, CreatedAt, UpdatedAt, IsDeleted, DeletedAt | `Message.cs` |
| Mesaj'da "okuyan kişi" alanı | **Eksik** | `Message.cs` — ReadByUserId yok |
| Mesaj'da IP adresi alanı | **Eksik** | `Message.cs` — IP alanı yok |
| Kullanıcı listeleme / getirme | **Var** | `AdminUsersController` GET / GET {id} |
| Kullanıcı oluşturma / güncelleme / soft-delete | **Var** | `AdminUsersController` POST / PUT / DELETE |
| Kullanıcıya rol **atama / kaldırma** endpoint'i | **Eksik** | `AdminUsersController` + `Features/Users/` — rol atama/kaldırma hiçbir yerde yok |
| Rol **CRUD** (ekleme/silme/düzenleme) endpoint'i | **Eksik** | `Features/Roles/` klasörü mevcut değil; `RoleManager<Role>` hiç kullanılmıyor |
| Rol CRUD admin sayfası | **Eksik** | Frontend'de yok |
| Kullanıcı–rol atama admin sayfası | **Eksik** | Frontend'de yok |
| Arama endpoint'i | **Var** | `SearchBlogsQuery`, `SearchBlogsQueryHandler` — EF Core LIKE |
| Arama logu / audit | **Eksik** | Arama geçmişi hiçbir yerde kaydedilmiyor |
| IP / audit log altyapısı | **Eksik** | Tüm codebase'de `IpAddress`, `AuditLog`, `AuditEntry` içeren dosya yok |
| `ReaderId` / `ReadByUserId` Message alanı | **Eksik** | `Message.cs` |
| `RoleNames.All` sabiti | **Var:** Admin, Manager, User | `RoleNames.cs` |
| `Role` entity'si | **Var** (boş uzantı; sadece IdentityRole miras) | `Role.cs` |
| `IdentityDataSeeder` | **Var** (seed'leme var) | CLAUDE.md |

**Tespit özeti:**
- Rol atama/kaldırma: **YOK** — CreateUserByAdminCommandHandler içinde tek satır `AddToRoleAsync(user, RoleNames.User)` var; bu kayıt anında sabit User atamasıdır. Sonradan değiştirmeye yönelik endpoint yok.
- Rol CRUD: **YOK** — `RoleManager<Role>` codebase'de hiç çağrılmıyor; `Features/Roles` klasörü mevcut değil.
- IP / Audit: **YOK** — Hiçbir entity'de IP alanı ve audit log tablosu yok.
- Arama logu: **YOK** — `SearchBlogsQueryHandler` sonuçları döndürüyor ama kayıt almıyor.

---

## Kapsam

### Dahil

1. **Blog Audit Alanı:** Blog oluşturulurken gönderen IP adresi kaydedilir; admin detay görünümünde gösterilir.
2. **Mesaj Audit Alanları:** Mesaj gönderilirken gönderen IP kaydedilir; mesaj okunduğunda okuyan Admin/Manager kullanıcı ID'si ve okunma zamanı kaydedilir.
3. **Arama Audit Logu:** Her arama isteğinde aranan terim, tarih, IP ve (giriş yapılmışsa) kullanıcı ID'si `SearchLog` tablosuna yazılır. Admin panelinde listelenir ve filtrelenir.
4. **Kullanıcıya Rol Atama / Kaldırma:** Admin, mevcut bir kullanıcının rolünü değiştirebilir (atama + kaldırma). Bağlayıcı kural: yalnızca Admin yapabilir (A6 matrisi).
5. **Rol CRUD:** Admin, sistemde yeni özel rol oluşturabilir, adını güncelleyebilir veya silebilir. Admin ve Manager rolleri silinemez (korumalı).
6. **Admin Dashboard sayfaları:** Audit log listesi, arama logu listesi, rol yönetimi ve kullanıcı–rol atama sayfaları.

### Hariç

- Audit log dışa aktarma (CSV/Excel) — ileride değerlendirilebilir.
- Rol bazlı izin (permission/claim) sistemi — sadece rol adı yönetimi.
- Gerçek zamanlı arama analitik dashboard'u.
- Elasticsearch entegrasyonu (A11 kararıyla sonraki fazda).
- Kayıt anlamlandırma/IP coğrafi konum.
- Farklı entity'lere (Comment, SubComment) IP logu — kapsam dışı tutulmuştur.

---

## KVKK / GDPR Değerlendirmesi

> **DİKKAT — KULLANICI ONAYI GEREKTİREN BÖLÜM.**
> Aşağıdaki kararlar hukuki ve mimari sonuçlar doğurur; uygulama öncesinde ekip / hukuk danışmanlığı ile onaylanmalıdır. Bu bölümdeki her karar "Açık Kararlar" listesine de alınmıştır (A-AU kodlarıyla).

### 1. Hangi veriler kişisel veridir?

| Veri | Kişisel veri? | Neden |
|---|---|---|
| IP adresi (Blog / Mesaj / Arama) | **Evet** | KVKK m.3/1-d ve GDPR Recital 30 uyarınca IP adresi bir kişiye bağlanabiliyorsa kişisel veridir. |
| Arama terimi + kullanıcı ID | **Evet** | Kullanıcıyla eşleştirilebildiğinde davranış verisi olarak kişisel veri sayılır. |
| Arama terimi + sadece IP (anonim) | **Kısmen** — Pseudonymous | IP ile yeniden bağlanabilme riski nedeniyle salt anonim değildir. |
| ReadByUserId (okuyan admin kimliği) | **Evet** | Çalışan/operatör verisi — işveren–çalışan ilişkisinde ayrı bir yasal dayanak gerektirebilir. |

### 2. Yasal dayanak

**Meşru menfaat (KVKK m.5/2-f / GDPR Art. 6(1)(f))** tercih edilen yaklaşımdır:

- Blog IP'si: Haksız içerik / spam tespiti ve içerik sahipliği doğrulama amacıyla sistemin güvenliğini koruma meşru menfaati.
- Mesaj IP'si: İletişim formunda kötüye kullanım (bot, tehdit vb.) tespiti.
- Arama logu: Hizmet kalitesinin iyileştirilmesi ve olası kötüye kullanımın tespiti.
- ReadByUserId: İç süreç denetimi (audit trail) — şirket içi operasyonel amaç.

Açık rıza yoluna gidilmesi durumunda (alternatif): her kullanıcıya kayıt / giriş sırasında aydınlatma gösterilmeli ve onay kaydedilmeli; bu daha fazla geliştirme gerektirir.

**Öneri:** Meşru menfaat gerekçesiyle ilerlemek, **Aydınlatma Metni** zorunluluğunu ortadan kaldırmaz — yalnızca açık onay alma yükümlülüğünü kaldırır. Aydınlatma metni yine yayımlanmalıdır.

### 3. Saklama süresi

**Ekip onayı gerektiren kritik karar (A-AU1):**

| Log türü | Önerilen azami süre | Gerekçe |
|---|---|---|
| Blog IP logu | 6 ay | Olası hukuki itiraz / DMCA penceresiyle uyumlu |
| Mesaj IP logu | 6 ay | Kötüye kullanım tespiti amaçlı kısa dönem yeterli |
| Arama logu (giriş yapmış) | 3 ay | Davranışsal veri; daha uzun süre rıza gerektirebilir |
| Arama logu (anonim IP) | 90 gün | Anonim olsa da pseudonymous risk nedeniyle sınırlı süre |
| ReadByUserId logu | 2 yıl | İç denetim kaydı olarak daha uzun süre savunulabilir |

Otomatik silme/anonimleştirme için bir **background job veya scheduled task** gerekir. Bu planda kapsam dahilinde tutulmuştur (T görevleri arasında yer alır) ancak tercihli olarak sonraya bırakılabilir — bu durumda A-AU1 kararında belirtilmelidir.

### 4. IP saklama biçimi — ham mı, hash mi?

**Ekip onayı gerektiren kritik karar (A-AU2):**

| Seçenek | Avantaj | Dezavantaj |
|---|---|---|
| Ham IP sakla | Gerçek IP ile doğrudan sorgulama / yargı yardımı mümkün | Veri ihlali durumunda kişi tanımlanabilir; daha yüksek KVKK riski |
| SHA-256 hash (tuzlu) | Veri ihlalinde kişi tanımlanamaz; daha az yasal risk | Aynı IP'yi yeniden aramak için aynı tuz gerekli; coğrafi anlam kaybolur |
| Anonimleştirme (son octet sıfırla) | Gerçek anlamda anonim; GDPR'dan muaf tutulabilir | Bireysel takip imkansız hale gelir; kötüye kullanım tespiti zayıflar |

**Varsayılan öneri:** Tuzlu SHA-256 hash. Proje zaten `Sha256TokenHasher` altyapısını refresh token için kullanıyor — aynı soyutlama yeniden kullanılabilir. Bu sayede KVKK riski düşük tutulurken kötüye kullanım tespiti (aynı hash tekrar görünürse) korunur.

### 5. Aydınlatma metni / gizlilik politikası

Zorunludur — meşru menfaat gerekçesiyle bile KVKK m.10 / GDPR Art. 13 kapsamında bilgilendirme yükümlülüğü devam eder.

**Nerede gösterilir:**
- Kayıt (register) sayfasında: "Kişisel Verilerin Korunması Aydınlatma Metni" bağlantısı.
- İletişim formu (mesaj gönderme) sayfasında: form altında kısa uyarı + bağlantı.
- Footer'da: kalıcı bağlantı.

Bu plan kapsamında aydınlatma metninin **içeriği** oluşturulmaz (hukuki metin); yalnızca nereye bağlantı ekleneceği belirtilir.

### 6. Arama loglarında kişiselleştirme riski

Arama terimleri hassas kategori içeriği (sağlık, siyasi görüş vb.) taşıyabilir. Önerimiz:
- Arama terimi içeriğini analiz etme veya profil oluşturma amaçlı kullanmama.
- Log erişimini yalnızca Admin ile sınırlandırma (Manager göremez).
- Log listeleme ekranında "Bu veriler KVKK kapsamında kişisel veri içerebilir" uyarısı gösterme.

### 7. Veri sahibinin hakları (minimum gereksinim)

| Hak | Minimum Uygulama |
|---|---|
| Erişim hakkı | `/api/admin/users/{id}` zaten var; arama ve IP logları için kullanıcı başına filtreleme eklenebilir (ileri faz) |
| Silme hakkı | Kullanıcı soft-delete (A8): bloglar kalmaya devam eder, bu arama ve IP logları için de geçerli mi? (A-AU3 açık kararı) |
| İtiraz hakkı | Meşru menfaat gerekçesi kullanılıyorsa itiraz kanalı oluşturulmalı (iletişim formu yeterli olabilir) |

---

## User Story'ler

---

### US-1: Blog Oluşturulurken IP Kaydı

**Bir Admin olarak**, blog oluşturan kullanıcının IP adresini admin panelinde görmek istiyorum, **böylece** kötüye kullanım durumlarını veya içerik sahipliği anlaşmazlıklarını inceleleyebileyim.

#### Kabul Kriterleri

**Senaryo 1 — Blog oluşturulduğunda IP kaydedilir:**
- Verilen: Bir kullanıcı `POST /api/blogs` endpoint'ine istek gönderir.
- Verilen: İstek `X-Forwarded-For` veya `RemoteIpAddress` üzerinden çözümlenebilir bir IP içerir.
- Yapıldığında: Blog başarıyla oluşturulur.
- Beklenen: `Blogs` tablosundaki yeni kayıtta `CreatorIpHash` alanı, IP'nin SHA-256 hash'ini içerir (düz metin değil).

**Senaryo 2 — Admin blog detayında IP hash'ini görür:**
- Verilen: Admin, `GET /api/admin/blogs/{id}` endpoint'ine istek yapar.
- Yapıldığında: Yanıt döner.
- Beklenen: Yanıt gövdesi `creatorIpHash` alanını içerir.
- Beklenen: Aynı IP'den oluşturulan iki farklı blog için `creatorIpHash` değerleri eşittir.

**Senaryo 3 — IP çözümlenemezse blog yine de oluşturulur:**
- Verilen: Gelen istek IP adresi içermiyorsa (örn. test ortamı).
- Yapıldığında: Blog create çağrısı yapılır.
- Beklenen: `CreatorIpHash` null veya boş olarak kaydedilir; blog işlemi başarıyla tamamlanır (hata fırlatılmaz).

**Senaryo 4 — Public blog listesi ve detayında IP bilgisi dönmez:**
- Verilen: Anonim veya giriş yapmış bir kullanıcı `GET /api/blogs` veya `GET /api/blogs/{id}` yapar.
- Yapıldığında: Yanıt döner.
- Beklenen: Yanıt gövdesinde `creatorIpHash` veya IP ile ilgili hiçbir alan bulunmaz.

---

### US-2: Mesaj Gönderilirken IP Kaydı

**Bir Admin olarak**, iletişim formu aracılığıyla mesaj gönderen kişinin IP adresini (hash'lenmiş) admin panelinde görmek istiyorum, **böylece** spam veya taciz içerikli mesajların kaynağını izleyebileyim.

#### Kabul Kriterleri

**Senaryo 1 — Mesaj gönderildiğinde IP hash'i kaydedilir:**
- Verilen: Herhangi bir ziyaretçi `POST /api/messages` endpoint'ine istek gönderir.
- Yapıldığında: Mesaj başarıyla oluşturulur.
- Beklenen: `Messages` tablosundaki yeni kayıtta `SenderIpHash` alanı doldurulmuştur.

**Senaryo 2 — Admin mesaj detayında IP hash'ini görür:**
- Verilen: Admin `GET /api/admin/messages` yanıtına bakar.
- Yapıldığında: Liste döner.
- Beklenen: Her mesaj nesnesinde `senderIpHash` alanı bulunur.

**Senaryo 3 — IP çözümlenemezse mesaj yine de gönderilir:**
- Verilen: Gelen istek IP bilgisi taşımıyorsa.
- Yapıldığında: `POST /api/messages` çağrısı yapılır.
- Beklenen: `SenderIpHash` null olarak kaydedilir; mesaj gönderimi başarılı olur.

---

### US-3: Mesaj Okunduğunda Okuyan Kişi ve Zaman Kaydı

**Bir Admin olarak**, bir mesajı kimin, ne zaman okuduğunu görmek istiyorum, **böylece** mesaj kutusu sorumluluk zincirini takip edebileyim.

#### Kabul Kriterleri

**Senaryo 1 — Mesaj okundu olarak işaretlendiğinde okuyan kaydedilir:**
- Verilen: Admin veya Manager, `PATCH /api/admin/messages/{id}` ile `isRead: true` gönderir.
- Yapıldığında: İşlem başarıyla tamamlanır.
- Beklenen: `Messages` tablosundaki kayıtta `ReadByUserId` (token'dan alınan kullanıcı ID'si) ve `ReadAt` (UTC) alanları doldurulur.

**Senaryo 2 — Mesaj tekrar "okunmadı" yapılırsa okuyan bilgisi temizlenir:**
- Verilen: Admin, daha önce okunmuş bir mesajı `isRead: false` ile PATCH eder.
- Yapıldığında: İşlem tamamlanır.
- Beklenen: `ReadByUserId` ve `ReadAt` alanları null/boş'a döner.

**Senaryo 3 — Admin mesaj listesinde okuyan bilgisini görür:**
- Verilen: Admin `GET /api/admin/messages` yapar.
- Yapıldığında: Liste döner.
- Beklenen: Okunmuş mesajlarda `readByUserId` ve `readAt` alanları doldurulmuş olarak gelir; okunmamış mesajlarda null gelir.

**Senaryo 4 — Yetki kontrolü:**
- Verilen: "User" rolündeki bir kullanıcı `PATCH /api/admin/messages/{id}` çağırır.
- Yapıldığında: İstek işlenir.
- Beklenen: 403 Forbidden döner; `ReadByUserId` ve `ReadAt` değişmez.

---

### US-4: Arama Audit Logu Kaydetme

**Bir Admin olarak**, bloglarımda yapılan tüm aramaların (kim, ne aradı, ne zaman, hangi IP'den) kaydedilmesini istiyorum, **böylece** içerik ilgisini analiz edebileyim ve olası kötüye kullanımı tespit edebileyim.

#### Kabul Kriterleri

**Senaryo 1 — Giriş yapmış kullanıcı arama yaptığında log kaydedilir:**
- Verilen: Giriş yapmış bir kullanıcı `GET /api/blogs/search?q=örnek` isteği yapar.
- Yapıldığında: Arama sonuçları döner.
- Beklenen: `SearchLogs` tablosuna; kullanıcı ID'si, aranan terim, UTC zaman damgası ve IP hash'i içeren yeni bir kayıt eklenir.

**Senaryo 2 — Anonim kullanıcı arama yaptığında log kaydedilir (kullanıcı alanı boş):**
- Verilen: Giriş yapılmamış bir kullanıcı `GET /api/blogs/search?q=örnek` yapar.
- Yapıldığında: Arama sonuçları döner.
- Beklenen: `SearchLogs` tablosuna; `UserId` null, `IpHash` doldurulmuş, `Term` doldurulmuş kayıt eklenir.

**Senaryo 3 — Arama başarısız olsa bile (0 sonuç) log kaydedilir:**
- Verilen: Herhangi bir kullanıcı hiçbir blog döndürmeyen bir terim arar.
- Yapıldığında: Boş sayfalı sonuç döner.
- Beklenen: `SearchLogs` tablosuna yine de kayıt eklenir.

**Senaryo 4 — Boş/geçersiz arama terimi log'a yazılmaz:**
- Verilen: `GET /api/blogs/search` isteği boş `q` ile gönderilir.
- Yapıldığında: Validator 400 döndürür.
- Beklenen: `SearchLogs` tablosuna kayıt eklenmez.

---

### US-5: Arama Loglarını Admin Panelinde Listeleme

**Bir Admin olarak**, tüm arama loglarını filtreleyerek ve sayfalı biçimde görmek istiyorum, **böylece** hangi terimlerin ne kadar arandığını ve hangi kullanıcıların hangi aramaları yaptığını inceleyebileyim.

#### Kabul Kriterleri

**Senaryo 1 — Admin arama log listesini getirir:**
- Verilen: Admin `GET /api/admin/search-logs` çağırır.
- Yapıldığında: Yanıt döner.
- Beklenen: Sayfalı (`PagedResult<SearchLogResponse>`) liste döner; her kayıt; `id`, `term`, `userId` (null ise "Anonim"), `ipHash`, `searchedAt` alanlarını içerir.

**Senaryo 2 — Terim filtrelemesi çalışır:**
- Verilen: Admin `GET /api/admin/search-logs?term=react` çağırır.
- Yapıldığında: Yanıt döner.
- Beklenen: Yalnızca `Term` alanı "react" içeren kayıtlar döner (büyük/küçük harf duyarsız LIKE).

**Senaryo 3 — Manager arama loglarına erişemez:**
- Verilen: Manager rolündeki kullanıcı `GET /api/admin/search-logs` yapar.
- Yapıldığında: Yanıt döner.
- Beklenen: 403 Forbidden döner. (Arama logları kişisel veri riski taşıdığından yalnızca Admin erişebilir.)

**Senaryo 4 — Yetki yoksa 401:**
- Verilen: Anonim kullanıcı `GET /api/admin/search-logs` yapar.
- Yapıldığında: Yanıt döner.
- Beklenen: 401 Unauthorized döner.

---

### US-6: Kullanıcıya Rol Atama / Kaldırma

**Bir Admin olarak**, mevcut bir kullanıcıya rol atamak veya rolünü kaldırmak istiyorum, **böylece** erişim yetkilerini dinamik olarak yönetiyor olayım.

#### Kabul Kriterleri

**Senaryo 1 — Admin, kullanıcıya mevcut bir rol atar:**
- Verilen: Kullanıcı ID'si ve atanacak rol adı (`POST /api/admin/users/{id}/roles` gövdesiyle) gönderilir.
- Verilen: Belirtilen rol sistemde mevcuttur.
- Yapıldığında: İstek işlenir.
- Beklenen: 200 OK; kullanıcının `Roles` listesinde yeni rol görünür.
- Beklenen: Kullanıcı halihazırda bu rolde ise sonuç idempotent olarak 200 OK döner (hata değil).

**Senaryo 2 — Admin, kullanıcıdan rol kaldırır:**
- Verilen: `DELETE /api/admin/users/{id}/roles/{roleName}` çağrısı yapılır.
- Yapıldığında: İstek işlenir.
- Beklenen: 204 No Content; kullanıcının `Roles` listesinde rol artık görünmez.

**Senaryo 3 — Son Admin'den Admin rolü kaldırılamaz:**
- Verilen: Sistemde yalnızca bir Admin bulunmaktadır.
- Verilen: `DELETE /api/admin/users/{id}/roles/Admin` çağrısı yapılır.
- Yapıldığında: İstek işlenir.
- Beklenen: 400 Bad Request; hata mesajı "Son Admin kullanıcısından Admin rolü kaldırılamaz" içerir.

**Senaryo 4 — Kullanıcı bulunamazsa 404:**
- Verilen: Sistemde olmayan bir ID ile rol atama denenirse.
- Yapıldığında: İstek işlenir.
- Beklenen: 404 Not Found döner.

**Senaryo 5 — Rol adı sistemde yoksa 404:**
- Verilen: Sistemde tanımlanmamış bir rol adı gönderilir.
- Yapıldığında: İstek işlenir.
- Beklenen: 404 Not Found döner (rol bulunamadı).

**Senaryo 6 — Manager bu endpoint'i çağıramaz:**
- Verilen: Manager rolündeki kullanıcı `POST /api/admin/users/{id}/roles` çağırır.
- Yapıldığında: İstek işlenir.
- Beklenen: 403 Forbidden döner.

---

### US-7: Rol CRUD Yönetimi

**Bir Admin olarak**, sistemde özel roller oluşturmak, var olan rollerin adını güncellemek ve kullanılmayan rolleri silmek istiyorum, **böylece** rol yapısını ihtiyaçlarıma göre genişletebileyim.

#### Kabul Kriterleri

**Senaryo 1 — Admin yeni bir rol oluşturur:**
- Verilen: Admin `POST /api/admin/roles` endpoint'ine benzersiz bir rol adı gönderir.
- Yapıldığında: İstek işlenir.
- Beklenen: 201 Created; yanıtta yeni rolün `id` ve `name` alanları bulunur.

**Senaryo 2 — Aynı adda ikinci rol oluşturulamaz:**
- Verilen: Sistemde zaten var olan bir rol adı gönderilir.
- Yapıldığında: İstek işlenir.
- Beklenen: 409 Conflict döner.

**Senaryo 3 — Admin tüm rolleri listeler:**
- Verilen: Admin `GET /api/admin/roles` çağırır.
- Yapıldığında: Yanıt döner.
- Beklenen: Tüm rollerin `id`, `name`, `userCount` (bu roldeki kullanıcı sayısı) alanlarını içeren liste döner.

**Senaryo 4 — Admin bir rolün adını günceller:**
- Verilen: Admin `PUT /api/admin/roles/{id}` ile yeni rol adı gönderir.
- Verilen: Güncellenen rol "Admin", "Manager" veya "User" değildir (korumalı roller).
- Yapıldığında: İstek işlenir.
- Beklenen: 200 OK; rol adı güncellenir.

**Senaryo 5 — Korumalı rollerin adı güncellenemez:**
- Verilen: Admin "Admin", "Manager" veya "User" adlı rolü güncellemeye çalışır.
- Yapıldığında: İstek işlenir.
- Beklenen: 400 Bad Request; "Bu rol sistem tarafından korunmaktadır" mesajı döner.

**Senaryo 6 — Admin bir rolü siler:**
- Verilen: Admin `DELETE /api/admin/roles/{id}` çağırır.
- Verilen: Silinen rol "Admin", "Manager" veya "User" değildir.
- Verilen: Bu rolde hiç kullanıcı yoktur.
- Yapıldığında: İstek işlenir.
- Beklenen: 204 No Content; rol silinir.

**Senaryo 7 — Kullanıcıya atanmış rol silinemez:**
- Verilen: En az bir kullanıcıya atanmış bir rol silinmeye çalışılır.
- Yapıldığında: İstek işlenir.
- Beklenen: 409 Conflict; "Bu rolde aktif kullanıcılar var" mesajı döner.

**Senaryo 8 — Korumalı roller silinemez:**
- Verilen: "Admin", "Manager" veya "User" adlı rol silinmeye çalışılır.
- Yapıldığında: İstek işlenir.
- Beklenen: 400 Bad Request.

---

### US-8: Admin Paneli — Audit ve Arama Log Sayfaları

**Bir Admin olarak**, Blog audit bilgilerini (yazar, tarih, IP hash) ve arama loglarını admin dashboard'da listeleyerek filtreleyebilmek istiyorum, **böylece** operasyonel denetimi tek bir arayüzden yürütebileyim.

#### Kabul Kriterleri

**Senaryo 1 — Blog listesinde audit sütunları görünür:**
- Verilen: Admin, admin paneli Blog listesi sayfasını açar.
- Yapıldığında: Sayfa yüklenir.
- Beklenen: Her satırda Yazar adı, Oluşturulma tarihi ve IP Hash (maskelenmiş — son 8 karakter görünür) sütunları bulunur.

**Senaryo 2 — Mesaj listesinde okuyan + IP hash görünür:**
- Verilen: Admin, Mesajlar sayfasını açar.
- Yapıldığında: Sayfa yüklenir.
- Beklenen: Okunmuş mesajlarda "Okuyan" ve "Okunma Tarihi" sütunları dolu; okunmamışlarda "—" gösterilir.

**Senaryo 3 — Arama logları sayfası yüklenir ve filtrelenir:**
- Verilen: Admin "Arama Logları" sayfasını açar.
- Yapıldığında: Sayfa yüklenir.
- Beklenen: Sayfa, `term`, `userId`, `searchedAt`, `ipHash` sütunlarını içeren bir tablo gösterir.
- Verilen: Admin "term" arama kutusuna bir kelime yazar.
- Yapıldığında: Filtre uygulanır.
- Beklenen: Tablo yalnızca eşleşen kayıtları gösterir; toplam kayıt sayısı güncellenir.

**Senaryo 4 — KVKK uyarısı görünür:**
- Verilen: Admin, Arama Logları sayfasını açar.
- Yapıldığında: Sayfa yüklenir.
- Beklenen: Sayfanın üstünde "Bu veriler KVKK kapsamında kişisel veri içerebilir. Yalnızca yasal amaçlarla inceleyin." uyarı bandı görünür.

---

### US-9: Admin Paneli — Rol Yönetimi ve Kullanıcı–Rol Atama Sayfaları

**Bir Admin olarak**, web arayüzünden roller oluşturabilmek, güncelleyebilmek, silebilmek ve kullanıcılara rol atayabilmek/kaldırabilmek istiyorum, **böylece** backend API çağrısı yapmadan yetki yönetimini gerçekleştirebileyim.

#### Kabul Kriterleri

**Senaryo 1 — Rol listesi sayfası yüklenir:**
- Verilen: Admin `/admin/roles` sayfasını açar.
- Yapıldığında: Sayfa yüklenir.
- Beklenen: Tüm roller, her satırda adı ve kullanıcı sayısını gösterecek biçimde listelenir. Korumalı roller (Admin, Manager, User) Düzenle/Sil butonları devre dışı (disabled) olarak gösterilir.

**Senaryo 2 — Yeni rol oluşturulur:**
- Verilen: Admin yeni bir rol adı girerek "Oluştur" butonuna tıklar.
- Yapıldığında: Form gönderilir.
- Beklenen: Başarıda, yeni rol liste tablosuna anlık eklenir (optimistic update değil, refetch). Hata durumunda toast ile hata mesajı gösterilir.

**Senaryo 3 — Kullanıcı detay sayfasında rol atama / kaldırma çalışır:**
- Verilen: Admin, bir kullanıcının detay sayfasını açar.
- Yapıldığında: Sayfa yüklenir.
- Beklenen: Kullanıcının mevcut rolleri gösterilir; her rolün yanında "Kaldır" butonu bulunur; "Rol Ekle" dropdown'ı ile atanmamış roller arasından seçim yapılabilir.
- Verilen: Admin bir rol seçer ve "Ekle" yapar.
- Yapıldığında: API çağrısı yapılır.
- Beklened: Başarıda kullanıcının rol listesi güncellenir.

---

## Task Dağılımı

### Dikey Dilim 1 — Audit Altyapısı (Blog IP + Mesaj IP + Okuyan Kaydı)

> Bu dilim, diğer tüm audit özelliklerinin ortak altyapısını kurar. Önce tamamlanmalıdır.

#### Backend

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| B-AU1 | `IIpHasher` arayüzü ve `Sha256IpHasher` implementasyonu. Tuz, user secrets'tan okunur. Projedeki `Sha256TokenHasher` ile aynı altyapıyı kullanır. | — | S |
| B-AU2 | `Blog` entity'sine `CreatorIpHash` (string?, max 64) alanı ve private setter eklenir. `Create` factory metodu opsiyonel `ipHash` parametresi alır. | — | S |
| B-AU3 | `Message` entity'sine `SenderIpHash` (string?, max 64), `ReadByUserId` (string? FK → Users), `ReadAt` (DateTime?) alanları eklenir. `MarkAsRead(bool, string? readerId)` mutator güncellenir. | — | S |
| B-AU4 | EF Core migration: `Blogs.CreatorIpHash`, `Messages.SenderIpHash`, `Messages.ReadByUserId`, `Messages.ReadAt` kolonları eklenir. `MessageConfiguration`'a FK tanımı ve `UsersConfiguration`'a `ReadByUserId` ilişkisi eklenir. `dotnet ef migrations add AddAuditFields`. | B-AU2, B-AU3 | S |
| B-AU5 | `IHttpContextAccessor`'dan IP çözümleyen yardımcı servis (`IClientIpResolver`) tanımlanır. `X-Forwarded-For` başlığı öncelikli, fallback `RemoteIpAddress`. DI kaydı yapılır. | B-AU1 | S |
| B-AU6 | `CreateBlogCommandHandler` güncellenir: `IClientIpResolver` + `IIpHasher` enjekte edilir; hash'lenmiş IP `Blog.Create` factory'sine geçirilir. | B-AU2, B-AU5 | S |
| B-AU7 | `SendMessageCommandHandler` güncellenir: IP hash'i `Message.Create` factory'sine geçirilir. | B-AU3, B-AU5 | S |
| B-AU8 | `MarkMessageAsReadCommandHandler` güncellenir: `readerId` (token'dan alınan kullanıcı ID'si) + `ReadAt` (UtcNow) `MarkAsRead` mutator'ına geçirilir. Komut `ReaderId` alanı alacak şekilde güncellenir; controller `GetUserId()` ile doldurur. | B-AU3 | S |
| B-AU9 | `BlogDetail` ve `BlogDetailResponse` record'larına `creatorIpHash` alanı eklenir. Yalnızca admin sorgusu döndürür (public GET /api/blogs/{id}'de bu alan YOK). `GetBlogByIdQueryHandler` admin flag'i destekleyecek şekilde güncellenir veya ayrı admin endpoint oluşturulur. **Teknik Karar Noktası:** Ayrı `/api/admin/blogs/{id}` endpoint mi, yoksa mevcut handler'a `isAdmin` parametresi mi? | B-AU4 | M |
| B-AU10 | `MessageResponse` record'una `senderIpHash`, `readByUserId`, `readAt` alanları eklenir. `MessageMapper` güncellenir. `GetMessagesQueryHandler` + repository bu alanları projeksiyon'a dahil eder. | B-AU4 | S |

#### Frontend

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| F-AU1 | Admin Blog detay sayfasına (veya list tablosuna) `creatorIpHash` sütunu eklenir. Hash'in son 8 karakteri gösterilir, tam değer tooltip'te görünür. | B-AU9 tamamlanmadan başlanamaz | S |
| F-AU2 | Admin Mesajlar sayfasına `senderIpHash`, `readByUserId`, `readAt` sütunları eklenir. Okunmamış mesajlarda "—" gösterilir. | B-AU10 tamamlanmadan başlanamaz | S |

#### Test

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| T-AU1 | `Sha256IpHasher` birim testi: aynı IP + tuz → aynı hash; farklı IP → farklı hash. | B-AU1 | S |
| T-AU2 | `CreateBlogCommand` integration testi: blog oluştururken `CreatorIpHash`'in dolu geldiğini doğrula; IP yoksa null. | B-AU6 | S |
| T-AU3 | `SendMessageCommand` integration testi: `SenderIpHash` kaydedildi mi. | B-AU7 | S |
| T-AU4 | `MarkMessageAsRead` integration testi: `ReadByUserId` ve `ReadAt` dolduruldu mu; `isRead=false` yapılınca temizlendi mi. | B-AU8 | S |
| T-AU5 | Public `GET /api/blogs/{id}` yanıtında `creatorIpHash` alanının olmadığını doğrula. | B-AU9 | S |

---

### Dikey Dilim 2 — Arama Audit Logu

#### Backend

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| B-SL1 | `SearchLog` entity'si oluşturulur: `Id` (Guid), `Term` (string, max 200), `UserId` (string?, nullable FK), `IpHash` (string?, max 64), `SearchedAt` (DateTime). `BaseEntity` kullanılmaz (UpdatedAt gerekmez); ayrı ve yalın entity. | — | S |
| B-SL2 | `SearchLogConfiguration` (EF Core): `SearchLogs` tablosu, `Term` HasMaxLength(200), `UserId` nullable FK, `IpHash` HasMaxLength(64), `SearchedAt` index. | B-SL1 | S |
| B-SL3 | EF Core migration: `SearchLogs` tablosu eklenir. `dotnet ef migrations add AddSearchLog`. | B-SL2 | S |
| B-SL4 | `ISearchLogRepository` arayüzü: `AddAsync(SearchLog)` ve `GetPagedAsync(page, pageSize, termFilter, CancellationToken)` metodları. | B-SL1 | S |
| B-SL5 | `SearchLogRepository` implementasyonu. `AddAsync` yazar; `GetPagedAsync` LIKE filtreli, tarih azalan sıralı döner. DI kaydı yapılır. | B-SL4 | S |
| B-SL6 | `SearchBlogsQueryHandler` güncellenir: `ISearchLogRepository.AddAsync` ile log yazılır. Log yazma hatası aramayı bloklamamalıdır (try/catch ile sessiz hata — **Teknik Karar Noktası:** fire-and-forget mi, background task mı?). | B-SL5, Dilim 1'deki B-AU5 | S |
| B-SL7 | `SearchLogResponse` record, `GetSearchLogsQuery` ve `GetSearchLogsQueryHandler` (Admin + Manager DEĞİL, yalnızca Admin) dikey dilimi oluşturulur. `PagedResult<SearchLogResponse>` döner. | B-SL5 | M |
| B-SL8 | `AdminSearchLogsController`: `GET /api/admin/search-logs` endpoint; `[Authorize(Roles = RoleNames.Admin)]`. Sayfa + pageSize + term filtresi query param. | B-SL7 | S |

#### Frontend

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| F-SL1 | Admin sol menüye "Arama Logları" navigasyon öğesi eklenir (yalnızca Admin rolünde görünür). | — | S |
| F-SL2 | `/admin/search-logs` sayfası: TanStack Query ile `GET /api/admin/search-logs` çeker; `term`, `userId`, `searchedAt`, `ipHash` sütunlarını gösteren tablo. Sayfalama (önceki/sonraki). | B-SL8 tamamlanmadan başlanamaz | M |
| F-SL3 | Sayfa üstünde KVKK uyarı bandı bileşeni eklenir. | F-SL2 | S |
| F-SL4 | Term filtre girişi: debounce'lu, URL query param'a yansıyan filtre kutusu. | F-SL2 | S |

#### Test

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| T-SL1 | `SearchBlogsQuery` integration testi: arama yapılınca `SearchLogs` tablosuna kayıt düştüğünü doğrula. | B-SL6 | S |
| T-SL2 | Anonim arama integration testi: `UserId` null, `IpHash` dolu. | B-SL6 | S |
| T-SL3 | Geçersiz `q` (boş) → 400, log kaydı yok. | B-SL6 | S |
| T-SL4 | `GET /api/admin/search-logs` yetki testi: Admin → 200, Manager → 403, anonim → 401. | B-SL8 | S |
| T-SL5 | Term filtre testi: `?term=xyz` yalnızca eşleşen kayıtları döndürür. | B-SL8 | S |

---

### Dikey Dilim 3 — Kullanıcıya Rol Atama / Kaldırma

#### Backend

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| B-RA1 | `AssignRoleCommand` (userId, roleName) + `AssignRoleCommandValidator` + `AssignRoleCommandHandler`: kullanıcı yoksa 404; rol yoksa 404; zaten bu rolde ise idempotent 200; `UserManager.AddToRoleAsync`. | — | M |
| B-RA2 | `RemoveRoleCommand` (userId, roleName) + validator + handler: kullanıcı yoksa 404; rol yoksa 404; "Son Admin" koruması (Admin sayısı = 1 ve istek Admin rolünü kaldırmak ise 400); `UserManager.RemoveFromRoleAsync`. | — | M |
| B-RA3 | `AdminUsersController`'a iki yeni endpoint eklenir: `POST /api/admin/users/{id}/roles` (Assign) ve `DELETE /api/admin/users/{id}/roles/{roleName}` (Remove). Her ikisi de `[Authorize(Roles = RoleNames.Admin)]`. | B-RA1, B-RA2 | S |
| B-RA4 | `UserErrors` sınıfına yeni hata fabrikaları: `RoleNotFound`, `LastAdminCannotLoseRole`, `UserAlreadyInRole`. | B-RA1, B-RA2 | S |

#### Frontend

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| F-RA1 | Admin Kullanıcı Detay sayfasına "Roller" bölümü eklenir: mevcut roller chip olarak gösterilir; her chip'te "Kaldır" (×) butonu bulunur (korumalı Son Admin korumasıyla devre dışı bırakılır). | B-RA3 tamamlanmadan başlanamaz | M |
| F-RA2 | "Rol Ekle" dropdown: sistemdeki rollerden kullanıcıda olmayanlara filtrelenmiş; seçim sonrası API çağrısı yapılır, başarıda liste güncellenir. | F-RA1, B-RA3 | M |
| F-RA3 | İşlem geri bildirimi: başarıda toast ("Rol atandı"), hata durumunda toast (API hata mesajıyla). | F-RA1, F-RA2 | S |

#### Test

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| T-RA1 | `AssignRole` integration testi: happy path (kullanıcı bu rolde değil → 200, rol listesine eklendi). | B-RA1 | S |
| T-RA2 | İdempotent test: aynı rolü iki kez atama → ikincisi de 200. | B-RA1 | S |
| T-RA3 | `RemoveRole` integration testi: rol listesinden çıkarıldı mı. | B-RA2 | S |
| T-RA4 | Son Admin koruması: sistemde tek Admin varken Admin rolü kaldırma → 400. | B-RA2 | S |
| T-RA5 | Yetki testi: Manager `POST /api/admin/users/{id}/roles` → 403. | B-RA3 | S |
| T-RA6 | Kullanıcı bulunamadı → 404. Rol bulunamadı → 404. | B-RA1, B-RA2 | S |

---

### Dikey Dilim 4 — Rol CRUD (Oluştur / Listele / Güncelle / Sil)

#### Backend

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| B-RC1 | `RoleErrors` sınıfı: `NotFound`, `Conflict` (duplicate), `ProtectedRole`, `RoleHasUsers`. | — | S |
| B-RC2 | `GetRolesQuery` + `GetRolesQueryHandler`: `RoleManager<Role>.Roles` sorgusu; her rol için `UserManager.GetUsersInRoleAsync` ile `userCount` hesaplanır. `RoleResponse` record (id, name, userCount). | — | M |
| B-RC3 | `CreateRoleCommand` + validator + handler: `RoleManager.CreateAsync`; duplicate → 409; başarı → 201 + `RoleResponse`. | B-RC1 | S |
| B-RC4 | `UpdateRoleCommand` + validator + handler: korumalı rol kontrolü (Admin/Manager/User adı → 400); `RoleManager.UpdateAsync`; bulunamadı → 404; duplicate → 409. | B-RC1 | S |
| B-RC5 | `DeleteRoleCommand` + validator + handler: korumalı rol kontrolü → 400; `userCount > 0` ise → 409; `RoleManager.DeleteAsync`. | B-RC1, B-RC2 | M |
| B-RC6 | `AdminRolesController`: `GET /api/admin/roles`, `POST /api/admin/roles`, `PUT /api/admin/roles/{id}`, `DELETE /api/admin/roles/{id}`. Tümü `[Authorize(Roles = RoleNames.Admin)]`. | B-RC2, B-RC3, B-RC4, B-RC5 | S |
| B-RC7 | Wolverine `AlwaysUseServiceLocationFor<RoleManager<Role>>()` kaydı (opaque servis — Wolverine için allow-list gerekir). | — | S |

#### Frontend

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| F-RC1 | `/admin/roles` sayfası: `GET /api/admin/roles` ile TanStack Query; `name`, `userCount` sütunlu tablo; Oluştur butonu. Korumalı rollerin Düzenle/Sil butonları disabled. | B-RC6 tamamlanmadan başlanamaz | M |
| F-RC2 | Rol oluşturma modal (shadcn Dialog): ad girişi + submit. Başarı → liste invalidate, hata → toast. | F-RC1 | S |
| F-RC3 | Rol düzenleme modal: mevcut ad gösterilir, güncellenir. | F-RC1 | S |
| F-RC4 | Rol silme onay dialog: "Bu rolde X kullanıcı var" uyarısı varsa silme butonu disabled; yoksa onay isteği. | F-RC1 | S |
| F-RC5 | Sol menüye "Rol Yönetimi" öğesi eklenir (yalnızca Admin). | — | S |

#### Test

| ID | Task | Bağımlılık | Efor |
|---|---|---|---|
| T-RC1 | `GetRoles` integration testi: Admin + Manager + User en az döner; `userCount` doğru hesaplanır. | B-RC2 | S |
| T-RC2 | `CreateRole` happy path: yeni benzersiz rol → 201. | B-RC3 | S |
| T-RC3 | Duplicate rol adı → 409. | B-RC3 | S |
| T-RC4 | `UpdateRole` korumalı rol → 400. | B-RC4 | S |
| T-RC5 | `DeleteRole` kullanıcısı olan rol → 409; korumalı rol → 400; başarılı silme → 204. | B-RC5 | S |
| T-RC6 | Yetki testi: Manager `POST /api/admin/roles` → 403. | B-RC6 | S |

---

### Dilimler Arası Bağımlılık Özeti

```
Dilim 1 (Audit Altyapı) → Dilim 2 (Arama Logu) bağımlı (IClientIpResolver)
Dilim 3 (Rol Atama)    → Dilim 4 (Rol CRUD) bağımlı değil; paralel yürütülebilir
Dilim 1 + 2            → Dilim 3 + 4 ile tamamen bağımsız; paralel yürütülebilir
```

Önerilen sıra (tek geliştirici varsayımıyla):
1. Dilim 1 (Audit Altyapı — temel bağımlılık)
2. Dilim 2 (Arama Logu — Dilim 1'in IP altyapısını kullanır)
3. Dilim 3 (Rol Atama)
4. Dilim 4 (Rol CRUD — Dilim 3'ü tamamlar)

---

## Önceliklendirme (MoSCoW)

| Özellik | Öncelik | Gerekçe |
|---|---|---|
| US-6: Kullanıcıya Rol Atama/Kaldırma | **Must** | A6 yetki matrisi tanımlı ama runtime rol değiştirme endpoint'i yok; eksik temel işlevsellik. |
| US-7: Rol CRUD | **Must** | Özel rol oluşturma olmadan rol sistemi statik kalır; UI yönetimi imkansız. |
| US-3: Mesaj — Okuyan + Zaman Kaydı | **Must** | İş süreçleri için kritik audit izi; az efor (entity değişikliği minimal). |
| US-9: Rol Yönetimi Admin Sayfaları | **Should** | US-6 + US-7 backend tamamlanınca değer taşır; UI eklemedikçe kullanılamaz. |
| US-2: Mesaj IP Kaydı | **Should** | Spam/taciz tespiti için değerli; KVKK kararları (A-AU1, A-AU2) onaylanınca ilerlenebilir. |
| US-1: Blog IP Kaydı | **Should** | İçerik sahipliği/anlaşmazlık senaryoları için değerli; KVKK kararları öncelikli. |
| US-4: Arama Audit Logu | **Could** | Analitik değeri yüksek ama operasyonel aciliyet düşük; kişisel veri riski yüksek. |
| US-5: Arama Log Admin Sayfası | **Could** | US-4 tamamlanmadan anlamsız. |
| US-8: Audit Admin Sayfaları | **Could** | US-1, US-2, US-3 backend'i olmadan içerik yoktur. |

---

## Açık Kararlar

Aşağıdaki kararlar **uygulamaya başlamadan önce kullanıcı / ekip onayı gerektirir.** Her biri CLAUDE.md'ye işlenecektir.

| # | Konu | Seçenekler | Öneri | Risk |
|---|---|---|---|---|
| **A-AU1** | **IP log saklama süresi** | (a) 3 ay, (b) 6 ay, (c) 12 ay, (d) Saklama yok | Blog+Mesaj: 6 ay; Arama logu: 90 gün; ReadByUserId: 2 yıl | KVKK m.4/2-e ihlali riski; otomatik silme background job gerektirir |
| **A-AU2** | **IP adresi saklama biçimi** | (a) Ham IP, (b) SHA-256 tuzlu hash, (c) Son octet sıfırlama | Tuzlu SHA-256 hash (`Sha256TokenHasher` altyapısı yeniden kullanılır) | Ham IP tercih edilirse veri ihlali senaryosunda yasal sorumluluk artar |
| **A-AU3** | **Kullanıcı silindiğinde audit logları ne olur?** | (a) Loglar kalır (UserId'li), (b) Loglar anonimleştirilir (UserId null), (c) Loglar silinir | Loglar anonimleştirilir (UserId null yapılır) — A8 kararıyla tutarlı | Veri sahibinin "silinme hakkı" KVKK m.7 kapsamında; loglarda UserId kalırsa risk |
| **A-AU4** | **ReadByUserId alanı — çalışan veri gizliliği** | (a) Admin/Manager kimliği kayıt altına alınır (önerimiz), (b) Sadece "okundu" kaydı, kullanıcı yok | Kimlik kaydedilir; iç denetim gerektirir | İş hukukunda çalışan davranış takibi için işyeri politikası gerekebilir |
| **A-AU5** | **Arama logu — Manager erişimi** | (a) Yalnızca Admin, (b) Admin + Manager | Yalnızca Admin (kişisel veri riski; sınırlı erişim tercih edilir) | Manager'ın içerik yönetimi için arama verisi görmesi gerekiyor mu? |
| **A-AU6** | **Arama logu yazma hatası davranışı** | (a) Arama engellenmez (try/catch + log), (b) Atomic — log yazılmazsa arama da hata döner | Arama engellenmez (fire-and-forget yerine try/catch + structured log) | Fire-and-forget üretim ortamında kayıp sorunları yaratabilir; background task eklenmesi gerekebilir |
| **A-AU7** | **Admin rolü silinebilir mi?** | (a) Admin, Manager, User korumalı — silinemez (önerimiz), (b) Yalnızca aktif user yoksa silinebilir | Korumalı; silinemez ve adı değiştirilemez | Admin rolü silinirse sistem erişimsiz kalır; kritik güvenlik riski |
| **A-AU8** | **Rol silme — kullanıcısı varsa?** | (a) 409 Conflict — önce kullanıcıları bu rolden çıkar, sonra sil, (b) Cascade — tüm kullanıcılardan kaldır ve sil | 409 Conflict (kullanıcıyı bilinçli aksiyona zorlar) | Cascade silmede yetki kaybı riski |
| **A-AU9** | **Aydınlatma metni zorunluluğu** | Hukuk departmanı onayı + yayım lokasyonu belirlenmeli | Footer bağlantısı + iletişim formu uyarısı (US-5 uyarı bandı) | Yayımlanmadan IP logu başlatılmamalıdır — yasal ön koşul |

---

## Riskler ve Açık Sorular

| # | Risk / Soru | Etki | Olasılık | Önlem |
|---|---|---|---|---|
| R1 | **Yasal risk:** Aydınlatma metni yayımlanmadan IP logu başlatılır | Yüksek (KVKK cezası) | Orta | A-AU9 kararı onaylanmadan Dilim 1 ve 2 üretime alınmaz |
| R2 | **Performans:** `SearchBlogsQueryHandler`'da senkron `AddAsync` arama süresine eklenir | Orta | Düşük-Orta | Fire-and-forget yerine try/catch tercih edildi (A-AU6); yük testinde ölçülür |
| R3 | **Son Admin koruması delinmesi:** Birden fazla Admin varken eşzamanlı kaldırma isteği son birini de siler | Yüksek | Düşük | Handler'da transaction veya locking ile `AdminCount` kontrolü; sıralı değil atomik |
| R4 | **`RoleManager<Role>` Wolverine kaydı:** Opaque servis allow-list'e eklenmezse runtime exception | Yüksek | Kesin (eğer eklenmezse) | B-RC7 görevi; CLAUDE.md'deki Wolverine konvansiyonu uyarısına göre zorunlu |
| R5 | **Rol adı değiştirince mevcut token'lar:** Token içindeki rol claim'i eski adı taşır; access token expire olmadan eski rol geçerli sayılır | Orta | Orta | Rol güncelleme scope'u "özel roller" ile sınırlandırıldı (Admin/Manager/User değişmez); özel rol değişikliğinde kullanıcıların token yenileme etkisi değerlendirilmeli |
| R6 | **`X-Forwarded-For` sahteciliği:** CDN/proxy yoksa istemci başlığı sahte IP ile doldurabilir | Düşük-Orta | Orta | `IClientIpResolver`, güvenilir proxy listesi kontrolü yapmalı; üretim konfigürasyonu gerektirir |
| R7 | **SearchLog tablo hacmi:** Yoğun trafikte `SearchLogs` tablosu hızla büyür | Orta | Orta | Otomatik silme background job (A-AU1); tablo partition'lama ileride değerlendirilebilir |
| R8 | **Çalışan takibi (ReadByUserId):** İş hukuku kapsamında çalışan gözetimi soruları | Orta | Düşük | A-AU4 kararı; iç politika belgesi ile desteklenmeli |
| R9 | **Migration kesinti riski:** `Blog` tablosuna yeni kolon ekleme büyük tablolarda tablo kilidi açabilir | Düşük | Düşük | Prodüksiyon migration'ı maintenance window'da veya `DEFAULT NULL` ile online olarak yapılmalı |
| R10 | **Arama log yazma başarısızlığı, aramanın yanlışlıkla engellenmesi:** try/catch yeterli değilse 500 kullanıcıya ulaşabilir | Orta | Düşük | A-AU6 kararı gereği try/catch + structured logging zorunlu |

---

## Bağımlılık Haritası (Özet)

```
B-AU1 (IIpHasher) ──────────────────────────────────────────┐
B-AU5 (IClientIpResolver) ──────────┐                        │
                                    ├── B-AU6 (Blog IP)      │
                                    ├── B-AU7 (Message IP)   │
                                    └── B-SL6 (Arama logu)   │
B-AU2 (Blog entity) ──────────────────────────────────────────┤
B-AU3 (Message entity) ───────────────────────────────────────┤
                                                              ▼
                                              B-AU4 (Migration)
                                              B-SL3 (Migration)

B-RC7 (Wolverine allow-list) ──► B-RC2..6 (Rol CRUD)
B-RA1, B-RA2 (Rol Atama/Kaldırma) ◄── bağımsız; Dilim 4 ile paralel
```

# Özellik: Sosyal Giriş (Google + Facebook)

> Belge tarihi: 2026-06-13. Faz 5 sonrası ek özellik planı.
> Bu belge kod içermez; yalnızca planlama, user story, kabul kriteri ve görev kırılımından oluşur.

---

## Instagram Kararı (Araştırma Bulgusu)

**Karar: Instagram bu projeye sosyal giriş sağlayıcısı olarak UYGUN DEĞİLDİR. Kapsam dışı bırakılmıştır.**

### Gerekçe

1. **Basic Display API tamamen kapatıldı (4 Aralık 2024).** Instagram'ın kişisel hesaplara yönelik OAuth akışını sağlayan temel API artık çalışmıyor. Yeni bağlantılar kurulamıyor; eski entegrasyonlar da devre dışı.

2. **Yerine gelen Instagram Graph API yalnızca Profesyonel hesapları (Business veya Creator) destekliyor.** Standart bir kullanıcının kişisel Instagram hesabıyla uygulamaya giriş yapması teknik olarak mümkün değil. Tipik bir blog okuyucusunun profesyonel hesabı bulunmak zorunda olmadığı için bu kısıtlama kullanım senaryomuzu temelden geçersiz kılıyor.

3. **Uygulama incelemesi ve onay yükü.** Instagram Graph API entegrasyonu, Meta'nın uygulama incelemesinden (App Review) geçmeyi ve "Live moduna" geçişi gerektiriyor. Bu süreç, blog uygulaması ölçeğinde orantısız bir maliyet oluşturuyor.

4. **Standart OIDC / OAuth2 sağlayıcısı değil.** Instagram, OpenID Connect desteklemez. Dolayısıyla ASP.NET Identity'nin standart `AddOpenIdConnect` entegrasyonu çalışmaz; özel bir OAuth2 handler gerekir. Topluluk paketi (`AspNet.Security.OAuth.Instagram`) mevcutsa da temeldeki API kısıtlamaları nedeniyle değeri düşmüştür.

**Sonuç:** Google ve Facebook, evrensel kişisel hesap desteğiyle sosyal giriş için standart ve uygulanabilir sağlayıcılardır. Instagram bu planın kapsamından çıkarılmıştır.

---

## Özet ve Amaç

Kullanıcıların Google veya Facebook hesaplarıyla tek tıklamayla uygulamaya giriş yapabilmesi veya yeni hesap oluşturabilmesi. Başarılı sosyal giriş sonrasında sistemin kendi JWT access + refresh token çifti üretilir; mevcut token altyapısı (JwtTokenService, refresh rotation) değişmez.

Sosyal giriş, mevcut e-posta/şifre akışının **alternatifi** olarak sunulur; mevcut kullanıcılar her iki yöntemi de kullanabilmelidir.

---

## Kapsam

**Dahil:**
- Google OAuth2 entegrasyonu (Authorization Code Flow, backend-merkezli)
- Facebook OAuth2 entegrasyonu (Authorization Code Flow, backend-merkezli)
- External provider login → AspNetUserLogins tablosuna kayıt
- E-posta eşleşmesi ile mevcut hesaba bağlama (birleştirme politikası — açık karar A-SL1)
- Sosyal giriş sonrası kendi JWT access + refresh token üretimi
- Backend callback endpoint'i ve token döndürme akışı
- Frontend: Login overlay / sayfasına Google ve Facebook butonları eklenmesi (Auth-Overlay özelliğiyle koordineli — bkz. AUTH-OVERLAY-PLAN.md)

**Hariç:**
- Instagram (yukarıdaki karara bakınız)
- Apple ile giriş (Sign in with Apple)
- Hesaptan provider bağlantısını koparma (unlink) — Could olarak nitelendirilebilir, bu fazda yok
- Frontend SDK tabanlı akış (Google Identity Services JS SDK ile id_token gönderme) — açık karar A-SL2

---

## Entegrasyon Akışı (Mimari Karar)

### Seçilen Yaklaşım: Backend-Merkezli Authorization Code Flow

ASP.NET Core'un `AddAuthentication().AddGoogle().AddFacebook()` altyapısı kullanılır. Akış:

1. Frontend'de "Google ile Giriş" butonuna basılır.
2. Tarayıcı backend'in `/api/auth/external-login?provider=Google&returnUrl=...` endpoint'ine yönlendirilir.
3. Backend, ASP.NET Identity'nin `HttpContext.ChallengeAsync("Google")` mekanizmasıyla tarayıcıyı Google'a yönlendirir. PKCE ve state parametreleri ASP.NET altyapısı tarafından otomatik yönetilir.
4. Kullanıcı Google'da onay verir; Google `redirect_uri` olarak belirlenen `/api/auth/external-login-callback` endpoint'ine döner.
5. Backend, gelen `code`'u exchange ederek provider'dan kullanıcı bilgilerini (email, sub/provider ID) alır.
6. Hesap eşleştirme/oluşturma mantığı çalışır (aşağıda).
7. Backend kendi JWT access + refresh çiftini üretir ve frontend'i `returnUrl`'e yönlendirir; token'lar URL fragment veya özel bir "token teslim" sayfası aracılığıyla frontend'e iletilir (açık karar A-SL3).

### Hesap Eşleştirme / Oluşturma Mantığı

Sırasıyla şu adımlar izlenir:

1. `AspNetUserLogins` tablosunda (LoginProvider, ProviderKey) ikilisi var mı? Varsa ilgili kullanıcıya doğrudan giriş yapılır.
2. Yoksa provider'dan gelen e-posta, `AspNetUsers` tablosunda mevcut mu? Varsa — ve hesap birleştirme politikası açıksa (A-SL1) — mevcut kullanıcıya bu provider bağlanır (UserManager.AddLoginAsync) ve giriş yapılır.
3. Hiçbiri yoksa: otomatik yeni kullanıcı oluşturulur (FirstName/LastName provider claim'lerinden alınır; ImageUrl provider'ın profil fotoğrafı URL'iyle doldurulur ya da varsayılan gravatar kullanılır), "User" rolü atanır, provider girişi eklenir.

### Soft-Delete Edilmiş Kullanıcı

Sosyal giriş akışında da mevcut `LoginCommandHandler` mantığıyla paralel kontrol uygulanır: `UserManager.FindByEmailAsync` veya `FindByIdAsync` global query filter nedeniyle null dönerse, `IUserRepository.IsDeletedByEmailAsync` ile silinmiş hesap ayrımı yapılır ve `AuthErrors.AccountDisabled` (401) döndürülür.

---

## Kullanıcı Hikayeleri

### US-SL1: Google ile Giriş

Bir site ziyaretçisi olarak, Google hesabımla uygulamaya giriş yapmak istiyorum, böylece ayrı şifre oluşturup hatırlamak zorunda kalmayayım.

#### Kabul Kriterleri

**Verilen** anonim kullanıcı login ekranındaki "Google ile Giriş Yap" butonuna bastığında,
**Ne zaman** Google onay ekranında izin verip geri döndüğünde,
**O zaman** backend kendi JWT access token ve refresh token'ını üretir, frontend bu token'ları localStorage'a kaydeder ve kullanıcı ana sayfaya yönlendirilir.

**Verilen** kullanıcı daha önce aynı e-postayla e-posta/şifre ile kayıt olmuşsa,
**Ne zaman** Google ile giriş yaparsa,
**O zaman** mevcut hesaba Google provider'ı eklenir (e-posta eşleşmesi politikası A-SL1'e göre), kullanıcı ayrı bir hesap oluşturmadan giriş yapar.

**Verilen** Google OAuth akışı sırasında kullanıcı onayı iptal ederse veya bir hata oluşursa,
**Ne zaman** callback'e hata parametresiyle dönülürse,
**O zaman** kullanıcı hata mesajı içeren login sayfasına yönlendirilir; uygulama kırılmaz.

**Verilen** soft-delete edilmiş bir hesabın e-postasıyla Google'dan giriş denenirse,
**Ne zaman** callback işlenirken,
**O zaman** 401 "Bu hesap devre dışı bırakılmış" hatası döner, yeni hesap oluşturulmaz.

### US-SL2: Facebook ile Giriş

Bir site ziyaretçisi olarak, Facebook hesabımla uygulamaya giriş yapmak istiyorum.

#### Kabul Kriterleri

**Verilen** anonim kullanıcı login ekranındaki "Facebook ile Giriş Yap" butonuna bastığında,
**Ne zaman** Facebook onay ekranında izin verip geri döndüğünde,
**O zaman** backend JWT çiftini üretir ve kullanıcı giriş yapmış hale gelir.

**Verilen** Facebook hesabında e-posta adresi paylaşılmamışsa (bazı kullanıcılar e-posta paylaşmayı reddedebilir),
**Ne zaman** callback işlenirken e-posta claim'i boş gelirse,
**O zaman** kullanıcı "Bu sağlayıcıdan e-posta adresi alınamadı; e-posta ile kayıt olmanız gerekmektedir" mesajıyla login sayfasına yönlendirilir, hesap oluşturulmaz.

**Verilen** kullanıcı hem Google hem Facebook ile giriş denemişse ve her ikisi de aynı e-postayı döndürüyorsa,
**Ne zaman** ikinci provider'la giriş yapıldığında,
**O zaman** her iki provider da aynı kullanıcı hesabına bağlanır, tekrar hesap oluşturulmaz.

### US-SL3: Sosyal Giriş Sonrası Tam Oturum

Bir kullanıcı olarak, sosyal giriş sonrasında e-posta/şifre ile giriş yapanlarla aynı deneyimi yaşamak istiyorum (token yenileme, çıkış, korumalı sayfalara erişim).

#### Kabul Kriterleri

**Verilen** Google ile giriş yapmış kullanıcının access token'ı süresi dolduğunda,
**Ne zaman** axios interceptor 401 alıp refresh endpoint'ini çağırdığında,
**O zaman** yeni access + refresh token çifti döner; kullanıcı yeniden giriş yapmak zorunda kalmaz.

**Verilen** sosyal giriş yapmış kullanıcı "Çıkış Yap" butonuna bastığında,
**Ne zaman** logout işlemi gerçekleştiğinde,
**O zaman** mevcut refresh token revoke edilir, localStorage temizlenir, kullanıcı çıkmış olur.

---

## Kullanıcı Tarafından Yapılacak Kurulum Adımları

Bu bölüm geliştiricilerin kod yazmadan önce tamamlaması gereken dışsal kurulumları içermektedir.

### Google Cloud Console

1. `https://console.cloud.google.com/` adresine gidin.
2. Yeni bir proje oluşturun (veya mevcut projeyi seçin).
3. "APIs & Services" > "OAuth consent screen" bölümüne gidin.
   - User Type: "External" seçin.
   - Uygulama adı, destek e-postası ve geliştirici e-posta adresini doldurun.
   - Scopes: `email` ve `profile` kapsamlarını ekleyin.
   - Test kullanıcıları ekleyin (uygulama henüz yayınlanmadıysa yalnızca test kullanıcıları giriş yapabilir).
4. "APIs & Services" > "Credentials" > "Create Credentials" > "OAuth 2.0 Client ID" seçin.
   - Application type: "Web application".
   - Authorized redirect URIs'a şunu ekleyin: `https://localhost:7253/api/auth/google-callback` (development) ve production URL'inizi.
5. Oluşturulan **Client ID** ve **Client Secret** değerlerini kaydedin.
6. `dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>"` komutunu çalıştırın (Zn.ClientWebApi projesi için).
7. `dotnet user-secrets set "Authentication:Google:ClientSecret" "<client-secret>"` komutunu çalıştırın.

### Meta for Developers (Facebook)

1. `https://developers.facebook.com/` adresine gidin ve hesap oluşturun/giriş yapın.
2. "My Apps" > "Create App" seçin.
   - Use case: "Authenticate and request data from users with Facebook Login" seçin.
   - App tipi: "Consumer" veya "Business" seçin.
3. "Facebook Login" ürününü uygulamaya ekleyin.
4. "Settings" > "Valid OAuth Redirect URIs" bölümüne şunu ekleyin: `https://localhost:7253/api/auth/facebook-callback` (development) ve production URL'inizi.
5. "Settings" > "Basic" bölümünden **App ID** ve **App Secret** değerlerini alın.
6. `dotnet user-secrets set "Authentication:Facebook:AppId" "<app-id>"` komutunu çalıştırın.
7. `dotnet user-secrets set "Authentication:Facebook:AppSecret" "<app-secret>"` komutunu çalıştırın.
8. Uygulama "Development" modunda yalnızca listelenmiş test kullanıcıları giriş yapabilir. Genel kullanıma açmak için App Review gerekir.

---

## Görev Kırılımı

### Backend Görevleri

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| SL-B1 | `AddExternalAuthProviders` extension metodu: `AddGoogle(ClientId, ClientSecret)` ve `AddFacebook(AppId, AppSecret)` — `InfrastructureRegistration` veya yeni `AuthenticationRegistration` extension'ına ekleme | Backend/WebApi | Dışsal kurulum tamamlanmış olmalı | 0.5 g |
| SL-B2 | `ExternalLoginCommand` record ve `ExternalLoginCommandHandler`: provider claim'lerinden kullanıcı bilgisi çıkarma, hesap eşleştirme/oluşturma mantığı, AuthTokenFactory ile JWT üretimi | Backend/Application | SL-B1 | 1.5 g |
| SL-B3 | `ExternalLoginCommandValidator`: provider adı ve claim doğrulaması; e-posta boşsa Validation hatası | Backend/Application | SL-B2 | 0.25 g |
| SL-B4 | `ExternalAuthErrors`: EmailNotProvided, AccountDisabled, ProviderError hata fabrikaları | Backend/Application | — | 0.25 g |
| SL-B5 | `AuthController`'a iki endpoint eklenmesi: GET `/api/auth/external-login?provider=Google|Facebook&returnUrl=...` (Challenge başlatır) ve GET `/api/auth/external-login-callback` (callback, token üretir, frontend'e yönlendirir) | Backend/WebApi | SL-B2, SL-B3 | 1 g |
| SL-B6 | Callback sonrası frontend'e token iletme mekanizması (A-SL3 kararına bağlı): ya URL fragment, ya tek seferlik token endpoint'i | Backend/WebApi | SL-B5, A-SL3 kararı | 0.75 g |
| SL-B7 | Soft-delete kontrolü: mevcut `IUserRepository.IsDeletedByEmailAsync` sosyal giriş akışında da çağrılır | Backend/Application | SL-B2 | 0.25 g |
| SL-B8 | Wolverine: `UserManager<User>`, `SignInManager<User>` ve `IUserLoginStore` için `AlwaysUseServiceLocationFor<>` kaydı kontrol ve tamamlama | Backend/WebApi | SL-B2 | 0.25 g |
| SL-T1 | Integration testleri: yeni kullanıcı oluşturma (mock provider), mevcut e-postayla birleştirme, e-posta yok → hata, soft-deleted hesap → 401, replay güvenliği | Test | SL-B6 | 1.5 g |

**Not — Teknik Karar Noktası (Backend):** `ExternalLoginCommandHandler` ASP.NET Core'un `HttpContext.AuthenticateAsync("Google")` metoduna ihtiyaç duyar. Bu, Wolverine'in standart handler bağımlılık modelinin dışında ASP.NET Core'a özgü bir bağımlılıktır. Handler'ın `IHttpContextAccessor` üzerinden `HttpContext`'e erişmesi ya da callback mantığının controller action'ına yerleştirilmesi değerlendirilmelidir. Ekip bu tasarım kararını uygulamaya başlamadan netleştirmelidir.

### Frontend Görevleri

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| SL-F1 | `authApi.ts` güncelleme: `initiateExternalLogin(provider: 'Google' | 'Facebook')` — `/api/auth/external-login` URL'ine `window.location.assign` yönlendirmesi | Frontend | SL-B5 | 0.25 g |
| SL-F2 | `ExternalLoginCallback` işleme sayfası veya hook: callback sonrası URL'den token çıkarma (A-SL3 kararına göre), `setTokens` çağrısı, `/api/me` ile kullanıcı yükleme, `AuthContext` güncelleme | Frontend | SL-B6, A-SL3 kararı | 1 g |
| SL-F3 | `paths.ts` güncelleme: `externalLoginCallback: '/auth/callback'` rotası eklenmesi | Frontend | SL-F2 | 0.25 g |
| SL-F4 | `router.tsx` güncelleme: `/auth/callback` rotası eklenmesi (lazy) | Frontend | SL-F3 | 0.25 g |
| SL-F5 | `SocialLoginButtons` bileşeni: Google ve Facebook butonları, ikonu ve "ile Giriş Yap" etiketi, disabled state (yükleniyor), erişilebilir `aria-label` | Frontend | SL-F1 | 0.75 g |
| SL-F6 | `LoginPage` ve auth overlay (AUTH-OVERLAY-PLAN.md ile koordineli): ayraç ("veya") ve `SocialLoginButtons` bileşeni eklenmesi | Frontend | SL-F5, AUTH-OVERLAY ile koordineli | 0.5 g |
| SL-F7 | Hata senaryosu: callback sayfasında hata parametresi gelirse (örn. e-posta yok, hesap devre dışı) toast + login sayfasına yönlendirme | Frontend | SL-F2 | 0.25 g |

### Test Görevleri

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| SL-T1 | Integration testi: Mock external provider ile yeni kullanıcı oluşturma (JWT döner) | Test/Backend | SL-B6 | 0.5 g |
| SL-T2 | Integration testi: Mevcut e-postayla eşleştirme — UserLogins tablosuna satır eklenir, ayrı User oluşturulmaz | Test/Backend | SL-B6 | 0.5 g |
| SL-T3 | Integration testi: Provider e-posta dönmüyor → 400 hatası | Test/Backend | SL-B5 | 0.25 g |
| SL-T4 | Integration testi: Soft-deleted hesap → 401 AccountDisabled | Test/Backend | SL-B7 | 0.25 g |

---

## Açık Kararlar

Bu kararlar uygulamaya başlamadan önce onay gerektirir.

| # | Konu | Varsayılan Öneri | Gerekçe |
|---|---|---|---|
| A-SL1 | **E-posta eşleşmesiyle otomatik hesap birleştirme politikası** | Etkin: aynı e-posta ile daha önce e-posta/şifre kaydı varsa, sosyal provider o hesaba otomatik eklenir | Kullanıcı deneyimi açısından sezgisel; ancak e-posta sahipliğini doğrulamayan bir provider gelirse (teorik) farklı kişinin hesabına girilmesi riski oluşur. Google ve Facebook e-postayı doğruladığından bu risk düşük. |
| A-SL2 | **Frontend SDK akışı mı, backend-merkezli akış mı?** | Backend-merkezli (ASP.NET AddGoogle/AddFacebook challenge): PKCE, state yönetimi, client secret server-side kalır | Frontend SDK (Google Identity Services JS) daha hızlı gözüküyor ama client secret'ı gerektirmez; id_token doğrulaması backend'de yapılır. İki yaklaşımın da farklı implementation noktaları var. Bu plan backend-merkezli varsayar. |
| A-SL3 | **Callback sonrası frontend'e token iletme yöntemi** | Önerilen: `/auth/callback#access_token=...&refresh_token=...` URL fragment ile; fragment sunucuya gitmez, loglanmaz | Alternatif: backend, tek seferlik kısa ömürlü bir "exchange code" üretir; frontend bunu `/api/auth/exchange` endpoint'ine göndererek gerçek token'ları alır (daha güvenli ama daha fazla iş). XSS riski her iki yöntemde de localStorage'da eşit olduğundan fragment yeterli görünüyor. |
| A-SL4 | **Facebook e-posta yoksa ne olur?** | Facebook'ta e-posta izni reddedilirse kullanıcı "e-posta paylaşmanız gerekmektedir" mesajıyla geri gönderilir, hesap oluşturulmaz | Alternatif: kullanıcıdan manuel e-posta girmesi istenir (ek bir form). Daha iyi UX ama daha fazla iş. |
| A-SL5 | **Aynı e-posta farklı provider üzerinden tekrar gelirse (Google + Facebook aynı e-posta)** | A-SL1 mantığı gereği her iki provider da aynı hesaba bağlanır (UserLogins tablosuna iki satır) | Bu davranış beklenen ve istenilen; ancak test edilmesi gereken bir kenar durumdur. |

---

## Önceliklendirme

**MoSCoW: Should**

Kullanıcı deneyimini önemli ölçüde iyileştiriyor (sürtünmesiz giriş), ancak uygulama çalışmak için buna muhtaç değil. Faz 5 tamamlandıktan sonra, Auth Overlay (AUTH-OVERLAY-PLAN.md) ile birlikte uygulanması tavsiye edilir — sosyal butonlar doğrudan overlay'e entegre edilir.

**Tahmini toplam efor:** L (6–8 gün backend + frontend + test)

**Uygulama sırası önerisi:**
1. Dışsal kurulum adımları (Google Cloud Console + Meta for Developers) tamamlanır.
2. A-SL1, A-SL2, A-SL3 kararları netleştirilir.
3. Backend SL-B1 → SL-B4 → SL-B5 → SL-B6 → SL-B7 (sırasıyla).
4. Frontend SL-F1 → SL-F2 → SL-F3/F4 → SL-F5 → SL-F6/F7 (SL-B5 tamamlanınca başlanabilir).
5. Test görevleri SL-B6 tamamlanınca paralelleştirilir.

**Auth Overlay ile bağımlılık:** SL-F6 doğrudan AUTH-OVERLAY-PLAN.md'deki overlay bileşenine sosyal login butonları ekler. İki özellik aynı sprint'te planlanıyorsa SL-F5 Auth Overlay bitenek sonra entegre edilmeli; Auth Overlay önce teslim ediliyorsa SL-F6 onu bekler.

---

## Riskler ve Açık Sorular

| Risk | Etki | Olasılık | Azaltma Yolu |
|---|---|---|---|
| Google veya Facebook uygulama inceleme süreci üretim açılışını geciktirir | Yüksek | Orta | Geliştirmeyi test modunda tamamlayın; uygulama incelemesini paralelde başlatın |
| A-SL2'de backend-merkezli seçilirse, `IHttpContextAccessor` bağımlılığı Wolverine handler model eşleşmesiyle çakışabilir | Yüksek | Orta | Teknik Karar Noktası olarak işaretlendi; callback mantığı controller action'ında kalabilir |
| Facebook bazı hesaplarda e-posta dönmez; A-SL4 kararı net olmazsa UX'te çıkmaz sokak | Orta | Orta | A-SL4'ü uygulamadan önce kesinleştirin |
| Sosyal girişle oluşturulan hesaplarda şifre yok; kullanıcı daha sonra e-posta/şifre ile giriş denemesi yapabilir | Düşük | Düşük | "Şifre sıfırlama" özelliği yoksa, bu senaryoyu RegisterPage'de belgeleme veya "şifre belirle" akışı Could olarak değerlendir |
| Güvenli olmayan redirect_uri production'da hatalı yapılandırılırsa OAuth callback çalışmaz | Yüksek | Düşük | Deployment checklist'e redirect URI doğrulama adımı ekleyin |
| Aynı e-posta birden fazla provider ile gelirse (A-SL5): provider bağlantısı UserLogins'e yazılırken yarış koşulu | Düşük | Düşük | Unique index (LoginProvider, ProviderKey) zaten bunu önler; ek olarak try/catch ile idempotent yap |

# Özellik: Animasyonlu Login/Register Overlay

> Belge tarihi: 2026-06-13. Faz 5 sonrası ek özellik planı.
> Bu belge kod içermez; yalnızca planlama, user story, kabul kriteri ve görev kırılımından oluşur.

---

## Özet ve Amaç

Kullanıcıların giriş ve kayıt işlemlerini ayrı bir sayfa yüklenmesini beklemeden, SiteHeader'daki "Giriş Yap" / "Kayıt Ol" butonlarından açılan bir modal overlay üzerinden yapabilmesi. Login ↔ Register arası geçiş animasyonlu ve akıcı olmalı. Mevcut `LoginPage` / `RegisterPage` içerikleri (form mantığı, validation, AuthProvider çağrıları) yeniden kullanılabilir form bileşenlerine ayrılacak; böylece hem sayfa hem overlay aynı kodu paylaşır.

---

## Kapsam

**Dahil:**
- SiteHeader'daki anonim navigasyon butonlarından (Giriş Yap / Kayıt Ol) açılan modal overlay
- Overlay içinde animasyonlu Login ↔ Register sekme / panel geçişi
- Mevcut form mantığının paylaşılan bileşenlere (`LoginForm`, `RegisterForm`) taşınması
- Mevcut `/login` ve `/register` route'larının korunması (deep-link ve ProtectedRoute redirect desteği için — açık karar A-AO1)
- Overlay içinde sosyal giriş butonları için slot (SOCIAL-LOGIN-PLAN.md ile koordineli — bkz. A-AO4)
- Erişilebilirlik: focus trap, ESC ile kapanma, ARIA rolleri
- Mobil uyum: Sheet yerine Dialog veya özel overlay (aşağıda seçim gerekçesi)

**Hariç:**
- Şifre sıfırlama akışı (overlay'e taşınmaz, Could olarak nitelendirilebilir)
- Profil düzenleme modalı (ayrı özellik)
- Animasyon için Framer Motion eklenmesi (aşağıda gerekçeye bakınız — CSS/Tailwind yeterli)

---

## Animasyon Yaklaşımı — Framer Motion Yok, Tailwind Transitions

`client/package.json` incelendiğinde Framer Motion bağımlılığı **mevcut değil.** Projeye eklenmesi:

- Paket boyutunu artırır (~50 KB gzip)
- Blog uygulamasının mevcut animasyon ihtiyacı basit bir panel geçişiyle sınırlı
- `tw-animate-css` paketi zaten kurulu (`"tw-animate-css": "^1.4.0"`)

**Karar: Framer Motion eklenmeyecek.** Animasyon için Tailwind CSS `transition`, `transform`, `opacity`, `translate-x` utility'leri ve `tw-animate-css` kullanılacak. Overlay açılış/kapanışı için shadcn/ui Dialog'un built-in Radix animation `data-[state=open]` / `data-[state=closed]` attribute'ları yeterlidir. Login ↔ Register geçişi için CSS transform + opacity ile slayt efekti uygulanabilir.

---

## shadcn/ui Dialog mi, Sheet mi?

Projede her ikisi de kurulu:
- `client/src/components/ui/dialog.tsx` — mevcut
- `client/src/components/ui/sheet.tsx` — mevcut (SiteHeader'da mobil menü için kullanılıyor)

**Seçim: Dialog** — gerekçe:

| Kriter | Dialog | Sheet |
|---|---|---|
| Görsel konum | Ekranın ortasında, içeriğe odaklı | Kenardan kayar (sol/sağ/alt) |
| Semantik | Modal dialog (giriş formu için doğal) | Panel/Drawer (navigasyon, sepet vb.) |
| Mobil | Dialog responsive; max-w-md ile mobile sığar | Sheet mobil'de kenar panel hissi verir |
| Mevcut kullanım | Projede henüz auth için kullanılmamış | SiteHeader mobil menüde Sheet zaten kullanılıyor — çakışmaz |

Dialog, auth modalı için daha doğru semantiktir. Zaten kurulu olduğundan ek bağımlılık gerektirmez.

---

## Route Stratejisi (Açık Karar A-AO1)

### Mevcut Durum

`/login` ve `/register` route'ları `router.tsx`'te `RootLayout` altında tanımlı. `ProtectedRoute` login'e yönlendirirken `state={{ from: location }}` geçiyor; `LoginPage` bu state'i okuyor.

### Önerilen Yaklaşım: Hibrit (Her İki Yol da Korunur)

1. `/login` ve `/register` route'ları ve bağımsız sayfaları **korunur.**
   - ProtectedRoute redirect senaryosu değişmez: korumalı sayfaya gidilince `/login`'e yönlendirilir, `LoginPage` açılır (overlay değil sayfa).
   - Doğrudan URL ile `/login` açıldığında tam sayfa gösterilir (deep-link, e-posta linki vb.).
   
2. SiteHeader'daki "Giriş Yap" / "Kayıt Ol" butonları **önce overlay'i** açar (sayfaya gitmez).

3. Overlay içindeki "Kayıt Ol" / "Giriş Yap" geçiş linkleri de sayfa navigasyonu yapmaz; overlay içinde panel geçişi yapar.

4. Overlay'de başarılı giriş sonrası: eğer `from` state'i yoksa ana sayfada kalınır (overlay kapanır); varsa (teorik: overlay açıkken arka planda korumalı sayfa — pratikte olmaz) `from` path'e yönlendirilir.

Bu hibrit yaklaşım, ProtectedRoute mekanizmasını bozmadan kullanıcı deneyimini iyileştirir. Dezavantajı: form mantığı iki yerde bulunur gibi görünür, ancak paylaşılan `LoginForm` / `RegisterForm` bileşenleriyle bu tekrar ortadan kalkar.

---

## Kullanıcı Hikayeleri

### US-AO1: Header Butonundan Overlay Açma

Bir site ziyaretçisi olarak, "Giriş Yap" butonuna bastığımda sayfa yüklenmeden bir modal overlay'de giriş yapabilmek istiyorum, böylece okumakta olduğum içeriği kaybetmeden hızlıca giriş yapabilirim.

#### Kabul Kriterleri

**Verilen** anonim kullanıcı herhangi bir public sayfadayken SiteHeader'daki "Giriş Yap" butonuna bastığında,
**Ne zaman** butona tıklandığında,
**O zaman** mevcut sayfa değişmez, ekran ortasında animasyonlu login formu içeren bir modal overlay açılır; arka plan bulanıklaşır ve etkileşime kapanır.

**Verilen** overlay açıkken kullanıcı ESC tuşuna bastığında,
**Ne zaman** tuşa basıldığında,
**O zaman** overlay kapanır, odak overlay'i açan butona geri döner, arka plan normale döner.

**Verilen** overlay açıkken kullanıcı overlay dışına (arka plana) tıkladığında,
**Ne zaman** dış alana tıklanırken,
**O zaman** overlay kapanır (kapatma davranışı A-AO2'ye bağlı).

**Verilen** overlay açıkken klavye Tab ile gezinilirken,
**Ne zaman** focus overlay içindeki son element'ten Tab'a basıldığında,
**O zaman** focus modal içinde döngüsel olarak dolaşır (focus trap); arka plandaki elemanlara geçiş olmaz.

### US-AO2: Login ↔ Register Animasyonlu Geçiş

Bir site ziyaretçisi olarak, overlay içinde "Hesabınız yok mu? Kayıt Ol" linkine bastığımda overlay kapanıp açılmadan register formuna geçmek istiyorum.

#### Kabul Kriterleri

**Verilen** overlay Login panelindeyken kullanıcı "Kayıt Ol" geçiş linkine bastığında,
**Ne zaman** linke tıklandığında,
**O zaman** Login paneli kayarak sola çekilir, Register paneli sağdan kayarak gelir; geçiş animasyonu 200-300 ms aralığında tamamlanır; `prefers-reduced-motion: reduce` medya sorgusu aktifse animasyon atlanır, doğrudan geçiş yapılır.

**Verilen** Register panelindeyken "Zaten hesabınız var mı? Giriş Yap" linkine basıldığında,
**Ne zaman** linke tıklandığında,
**O zaman** tersi animasyonla Login paneline dönülür.

**Verilen** overlay Login panelindeyken başarılı giriş yapıldığında,
**Ne zaman** login işlemi tamamlandığında,
**O zaman** overlay kapanır, AuthContext güncellenir, SiteHeader'da kullanıcı avatar dropdown'ı görünür.

### US-AO3: Mevcut Sayfa Form İşlevselliğinin Korunması

Bir geliştirici olarak, mevcut `LoginPage` ve `RegisterPage` işlevselliğinin (form validation, hata gösterimi, AuthProvider çağrıları) hem sayfa hem overlay'de çalışmasını istiyorum.

#### Kabul Kriterleri

**Verilen** overlay'deki login formuna geçersiz e-posta girildiğinde,
**Ne zaman** form submit edildiğinde,
**O zaman** alan altında validasyon hatası gösterilir (mevcut `LoginPage` davranışıyla özdeş).

**Verilen** overlay'deki login formuna yanlış şifre girildiğinde,
**Ne zaman** backend 401 dönünce,
**O zaman** "Geçersiz e-posta veya şifre" genel form hatası overlay içinde görünür.

**Verilen** `/login` route'una doğrudan gidildiğinde,
**Ne zaman** sayfa yüklenince,
**O zaman** mevcut tam sayfa `LoginPage` görünür; overlay açılmaz. Davranış bugünkü gibi korunmuştur.

### US-AO4: Header "Kayıt Ol" Butonu da Overlay Açmalı

Bir site ziyaretçisi olarak, "Kayıt Ol" butonuna bastığımda doğrudan Register paneli açık şekilde overlay görmek istiyorum.

#### Kabul Kriterleri

**Verilen** SiteHeader'daki "Kayıt Ol" butonuna basıldığında,
**Ne zaman** butona tıklandığında,
**O zaman** overlay doğrudan Register paneli ile açılır (Login paneline geçiş animasyonu olmaz).

---

## Görev Kırılımı

### Frontend Görevleri

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| AO-F1 | `LoginForm` paylaşılan bileşeni: `LoginPage`'deki form mantığını (zod schema, react-hook-form, submit, hata gösterimi) bileşene çıkar; prop: `onSuccess?: () => void` (overlay kapatmak için) | Frontend | — | 1 g |
| AO-F2 | `RegisterForm` paylaşılan bileşeni: `RegisterPage`'deki form mantığını benzer şekilde çıkar; prop: `onSuccess?: () => void`, `onSwitchToLogin?: () => void` | Frontend | — | 1 g |
| AO-F3 | `LoginPage` ve `RegisterPage` güncelleme: kendi iç form mantıklarını sil, `LoginForm` / `RegisterForm` bileşenlerini çağır; mevcut route/redirect davranışı korunur | Frontend | AO-F1, AO-F2 | 0.5 g |
| AO-F4 | `AuthOverlay` bileşeni (`client/src/components/auth/AuthOverlay.tsx`): shadcn Dialog wrapper; state: `activePanel: 'login' | 'register'`; panel geçiş animasyonu (Tailwind translate-x + opacity + transition); ESC/dış tık kapatma (Dialog onOpenChange); focus trap (Radix Dialog otomatik sağlar) | Frontend | AO-F1, AO-F2 | 1.5 g |
| AO-F5 | `useAuthOverlay` hook veya context: `open`, `activePanel`, `openLogin()`, `openRegister()`, `close()` — SiteHeader ve diğer bileşenler buradan tüketir | Frontend | AO-F4 | 0.5 g |
| AO-F6 | `SiteHeader` güncelleme: "Giriş Yap" butonu `/login`'e Link yerine `openLogin()`, "Kayıt Ol" butonu `openRegister()` çağırır; mobil Sheet'teki butonlar da güncellenir | Frontend | AO-F5 | 0.5 g |
| AO-F7 | `RootLayout` veya `App`'e `AuthOverlay` render eklenmesi (her sayfadan erişilebilsin) | Frontend | AO-F4, AO-F5 | 0.25 g |
| AO-F8 | `prefers-reduced-motion` medya sorgusu: animasyon sınıfları bu tercihe göre koşullu uygulanır | Frontend | AO-F4 | 0.25 g |
| AO-F9 | Sosyal giriş butonları için slot: `AuthOverlay` içinde ayraç + `SocialLoginButtons` bileşeni için rezerve alan (SOCIAL-LOGIN-PLAN.md SL-F5 bağımlılığı; henüz hazır değilse boş bırakılabilir) | Frontend | AO-F4, SL-F5 (koordineli) | 0.25 g |
| AO-F10 | Mobil Sheet (SiteHeader) güncelleme: SheetClose ile sarılı butonlar `openLogin()` / `openRegister()` çağırır + Sheet kendisi de kapanır | Frontend | AO-F6 | 0.25 g |

**Bağımlılık sırası:**
- AO-F1 ve AO-F2 paralel çalışabilir (birbirinden bağımsız).
- AO-F3, AO-F1 ve AO-F2 bittikten sonra.
- AO-F4, AO-F1 ve AO-F2 bittikten sonra başlayabilir.
- AO-F5, AO-F4 bittikten sonra.
- AO-F6, AO-F7, AO-F10 → AO-F5 bittikten sonra, paralel çalışabilir.
- AO-F8 → AO-F4 ile birlikte veya hemen ardından.
- AO-F9 → AO-F4 sonrası, SL-F5 hazır olmasa da slot olarak bırakılabilir.

### Backend Görevleri

Bu özellik yalnızca frontend değişikliğidir. Backend'de değişiklik gerekmez. Mevcut auth endpoint'leri (`/api/auth/login`, `/api/auth/register`) olduğu gibi kullanılır.

### Test Görevleri

| ID | Görev | Katman | Bağımlılık | Efor |
|---|---|---|---|---|
| AO-T1 | `LoginForm` bileşen testi (Vitest + React Testing Library): başarılı submit → `onSuccess` çağrılır, 401 hatası → hata mesajı görünür, alan validasyonu | Test/Frontend | AO-F1 | 0.75 g |
| AO-T2 | `RegisterForm` bileşen testi: başarılı kayıt → `onSuccess` çağrılır, 409 → e-posta alanında hata | Test/Frontend | AO-F2 | 0.75 g |
| AO-T3 | `AuthOverlay` bileşen testi: açılma/kapanma, panel geçişi, ESC ile kapanma, focus trap (Radix Dialog davranışı) | Test/Frontend | AO-F4 | 0.5 g |

---

## Açık Kararlar

| # | Konu | Varsayılan Öneri | Gerekçe |
|---|---|---|---|
| A-AO1 | **`/login` ve `/register` route'ları korunacak mı?** | Evet, hibrit yaklaşım: her iki route korunur; header butonları overlay açar | ProtectedRoute redirect, deep-link ve e-posta doğrulama bağlantıları için gerekli; tamamen kaldırmak mevcut güvenlik akışını kırar |
| A-AO2 | **Overlay arka plana tıklayınca kapanmalı mı?** | Hayır, kapatmamalı (içeriği kaybetme riski): Dialog `onInteractOutside={e => e.preventDefault()}` ile dış tık engellenir; yalnızca ESC ve "X" butonu kapatır | Form doldurulurken yanlışlıkla kapanma riskini ortadan kaldırır; Radix Dialog kolayca yapılandırılabilir |
| A-AO3 | **Overlay state yönetimi — local state mi, global hook/context mi?** | Global `useAuthOverlay` hook'u (küçük React context veya Zustand'a gerek kalmadan `useState` + context ile): SiteHeader ve RootLayout aynı state'i paylaşması lazım | Local state yeterli olmaz çünkü SiteHeader ve overlay renderlanma yerleri farklı ağaçlarda |
| A-AO4 | **Sosyal giriş butonlarının overlay'e entegrasyonu ne zaman?** | Auth Overlay önce teslim edilir, sosyal butonlar için slot bırakılır; SOCIAL-LOGIN-PLAN.md tamamlanınca AO-F9 doldurulur | İki özellik ayrı sprint'e düşebilir; overlay'i sosyal girişe bağımlı kılmak gereksiz gecikmeye neden olur |
| A-AO5 | **Panel geçiş animasyonu yönü ve süresi** | Login → Register: sola kayış (Login -translateX, Register +translateX'ten gelir); Register → Login: tersi. Süre: 250ms ease-in-out; `prefers-reduced-motion` aktifse 0ms | Sezgisel yön: ileri = sola, geri = sağa |

---

## Sosyal Giriş (SOCIAL-LOGIN-PLAN.md) ile Kesişim ve Sıralama

Bu iki özellik doğrudan etkileşim halindedir:

- Auth Overlay (`AO-F4`, `AO-F9`): sosyal giriş butonları için boş bir slot içerir.
- Sosyal Giriş (`SL-F5`, `SL-F6`): `SocialLoginButtons` bileşeni overlay'e enjekte edilir.

**Önerilen uygulama sırası:**

1. Auth Overlay tamamlanır (AO-F1 → AO-F10). Sosyal butonlar henüz yok.
2. Sosyal Giriş tamamlanır (SL-B1 → SL-T1). `SocialLoginButtons` bileşeni hazır.
3. AO-F9 doldurulur: overlay'e `SocialLoginButtons` eklenir.

Bu sıralama her iki özelliğin bağımsız teslim edilmesine ve test edilmesine imkân tanır.

---

## Önceliklendirme

**MoSCoW: Could**

Mevcut `/login` ve `/register` sayfaları tamamen işlevsel. Overlay bir UX iyileştirmesidir; uygulamanın temel işleyişi için zorunlu değil. Faz 5 ve Sosyal Giriş özelliğinden sonra planlanması uygundur.

**Tahmini toplam efor:** M (4–5 gün yalnızca frontend + test; backend yok)

---

## Riskler ve Açık Sorular

| Risk | Etki | Olasılık | Azaltma Yolu |
|---|---|---|---|
| `useAuthOverlay` context'i `RootLayout`'a taşınırken AdminLayout veya diğer layout'larla çakışma | Orta | Düşük | Context'i `App.tsx` veya router'ın en üst seviyesinde tanımla; her layout'un erişebileceği yerde olsun |
| ProtectedRoute'un `/login`'e yönlendirip overlay yerine tam sayfa açması kullanıcıyı şaşırtabilir | Düşük | Yüksek (beklenen davranış) | Bu tasarımsal bir tercih; A-AO1'de hibrit yaklaşım kabul edilirse belgeleme yeterli |
| Radix Dialog'un built-in animasyonu özelleştirilmeye direnç göstermesi (CSS variable override gerekmesi) | Düşük | Orta | shadcn/ui Dialog zaten `tw-animate-css` ile uyumlu; `data-[state]` attribute'ları üzerinden Tailwind keyframe özelleştirmesi mümkün |
| `prefers-reduced-motion` kontrolü unutulursa erişilebilirlik ihlali oluşur | Orta | Orta | AO-F8 görevi bu kontrolü zorunlu kılar; test kriterlerine erişilebilirlik ekle |
| İki form bileşenine ayrıştırma (`LoginForm`, `RegisterForm`) mevcut sayfaları geçici olarak kırarsa | Yüksek | Düşük | AO-F3 görevinin kritik geçiş adımı olduğu not edilmeli; dikkatli refactor ile tek commit'te yapılabilir |
| Sosyal giriş butonları overlay içinde ek yükseklik gerektirirse küçük ekranlarda scroll gerekeceği durumda UX bozulabilir | Orta | Orta | Dialog max-height + overflow-y-auto ile sınırlandırma; sosyal butonlar ayraçla aşağıya alınır |

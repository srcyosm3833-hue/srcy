# Özellik: Sosyal Paylaşım — Blog Yazılarını Paylaşma

> **CLAUDE.md Madde 13 uyum notu:** Bu doküman, kesinleşmiş teknik kararların 13. maddesiyle
> ("Sosyal paylaşım = Web Share API / paylaş linkleri") doğrudan örtüşür.
> Kapsam; kullanıcı adına otomatik gönderi atmayı kesinlikle dışarıda bırakır — yalnızca
> paylaşım penceresini/uygulamasını açan butonlar ve tarayıcı-yerel Web Share API entegrasyonu.

---

> **ÖNEMLİ KAVRAM AYRIMI — Site Sosyal Medyası ile Blog Paylaşımı Karıştırılmamalı:**
>
> - **Site sosyal medyası** (`GET /api/social-media`): Sitenin kendi hesaplarına yönelik
>   bağlantılar. `SiteFooter` → `SocialMediaLinks` bileşeni tarafından gösterilir.
>   Bu bileşen backend verisi çeker ve sitenin varlığını tanıtır.
>
> - **Blog paylaşım butonları** (bu özellik): Bir okuyucunun *okuduğu* yazıyı kendi
>   sosyal medya hesabında paylaşmasını sağlayan butonlar. Tamamen istemci tarafında
>   çalışır, backend çağrısı yoktur.
>
> İki özelliğin bileşenleri, hook'ları ve mantığı birbirinden tamamen ayrı tutulmalıdır.

---

## Özet ve Amaç

Okuyucular blog yazılarını kolayca X/Twitter, Facebook, WhatsApp ve LinkedIn'de
paylaşabilmeli; link kopyalayabilmeli; mobilde native paylaşım diyaloğunu
açabilmelidir. Bu özellik blog detay sayfasının etkileşimini ve organik erişimini
artırır. Tamamen frontend özelliğidir — backend değişikliği gerektirmez.

**Hedef metrik:** Blog başına dış trafik kaynağı çeşitliliği artar; ölçüm
Google Analytics / UTM parametreleri üzerinden izlenebilir (bkz. Açık Karar A5).

---

## Kapsam

### Dahil
- Paylaşım bağlantısı üreten `ShareButtons` bileşeni (X/Twitter, Facebook, WhatsApp, LinkedIn).
- Link kopyala butonu + "Kopyalandı" geri bildirimi.
- Mobilde `navigator.share` (Web Share API) feature-detect + desteklemediğinde buton
  grubuna otomatik fallback.
- Blog detay sayfasına (`BlogDetailPage.tsx`) yerleşim.
- Paylaşım URL'i üretimi için istemci tarafı util fonksiyonu (`buildShareLinks`).
- Erişilebilirlik: `aria-label`, klavye odağı, `title` tooltip.

### Hariç
- Kullanıcı adına otomatik gönderi atma (OAuth akışı yok).
- Instagram paylaşım linki — Instagram'ın web ortamında programatik paylaşım URL'i
  yoktur; yalnızca mobil uygulama `navigator.share` ile yakalanabilir.
- Pinterest, Telegram, Reddit gibi ek sağlayıcılar (Could/Won't bu sürümde).
- Paylaşım sayacı / analitik olayı izleme backend'e yazılmaz (UTM sadece link üretimi).
- E-posta ile paylaşım (`mailto:`) buton seti dışında tutulur (Açık Karar A6).

---

## User Story'ler

### US-1: Yazıyı Sosyal Medyada Paylaşma

**Bir okuyucu olarak,** okuduğum blog yazısını X, Facebook, WhatsApp veya LinkedIn'de
paylaşmak istiyorum, böylece çevrem de bu yazıyı okuyabilsin.

#### Kabul Kriterleri

**Given** bir blog detay sayfasındayım (`/blogs/:id`),
**When** sayfanın paylaşım alanına bakıyorum,
**Then** X/Twitter, Facebook, WhatsApp ve LinkedIn için ayrı paylaşım butonları görürüm.

**Given** X/Twitter paylaşım butonuna tıkladım,
**When** tarayıcı yeni sekme açıyor,
**Then** `https://twitter.com/intent/tweet?text=<başlık>&url=<kanonik-URL>` şeklinde
doğru parametrelerle Twitter paylaşım penceresi açılır; başlık ve URL bloğun
gerçek verileriyle doldurulmuştur.

**Given** Facebook paylaşım butonuna tıkladım,
**When** tarayıcı yeni sekme açıyor,
**Then** `https://www.facebook.com/sharer/sharer.php?u=<kanonik-URL>` adresi açılır.

**Given** WhatsApp paylaşım butonuna tıkladım,
**When** tarayıcı yeni sekme/uygulama açıyor,
**Then** `https://api.whatsapp.com/send?text=<başlık>%20<kanonik-URL>` açılır.

**Given** LinkedIn paylaşım butonuna tıkladım,
**When** tarayıcı yeni sekme açıyor,
**Then** `https://www.linkedin.com/sharing/share-offsite/?url=<kanonik-URL>` açılır.

**Given** herhangi bir paylaşım butonuna tıkladım,
**Then** bağlantı `target="_blank" rel="noopener noreferrer"` ile açılır ve kullanıcı
mevcut sayfada kalır.

---

### US-2: Yazı Linkini Kopyalama

**Bir okuyucu olarak,** blog yazısının linkini tek tıkla panoya kopyalamak istiyorum,
böylece istediğim platforma veya kişiye yapıştırabilirim.

#### Kabul Kriterleri

**Given** blog detay sayfasındayım,
**When** "Link Kopyala" butonuna tıklıyorum,
**Then** `window.location.href` değeri panoya kopyalanır.

**Given** kopyalama başarılı oldu,
**When** kopyalama işlemi tamamlandı,
**Then** kullanıcıya 2 saniye boyunca "Link kopyalandı!" geri bildirimi gösterilir
(toast veya inline — bkz. Açık Karar A3); 2 saniye sonra geri bildirim kaybolur.

**Given** tarayıcı `navigator.clipboard.writeText` API'sini desteklemiyor
(eski tarayıcı veya güvensiz bağlam),
**When** kopyalama butonu tıklanıyor,
**Then** sessizce başarısız olmak yerine kullanıcıya hata toastı gösterilir:
"Link kopyalanamadı, lütfen elle kopyalayın."

---

### US-3: Mobilde Native Paylaşım Diyaloğu

**Mobil bir okuyucu olarak,** telefonimdeki native paylaşım menüsünü açmak istiyorum,
böylece WhatsApp, Instagram DM, Mesajlar gibi tüm uygulamalarıma kolayca iletebilirim.

#### Kabul Kriterleri

**Given** `navigator.share` API'sini destekleyen bir cihaz/tarayıcıdayım
(genel olarak modern Android Chrome, iOS Safari),
**When** blog detay sayfasını açıyorum,
**Then** paylaşım alanında platform butonlarına ek olarak (veya yerine — bkz. Açık Karar A2)
"Paylaş" (native) butonu görünür.

**Given** "Paylaş" butonuna tıkladım,
**When** `navigator.share({ title, url })` çağrıldı,
**Then** cihazın native paylaşım diyaloğu açılır; `title` bloğun başlığı, `url`
kanonik URL olarak iletilir.

**Given** `navigator.share` çağrısı kullanıcı tarafından iptal edildi
(ESC veya diyalog kapatıldı),
**Then** hata toastı gösterilmez; işlem sessizce sonlanır.

**Given** `navigator.share` mevcut değil (masaüstü tarayıcı veya eski cihaz),
**When** sayfa yükleniyor,
**Then** native "Paylaş" butonu hiç render edilmez; yalnızca platform butonları
ve link kopyala butonu gösterilir (graceful fallback).

---

### US-4: Erişilebilir Paylaşım Butonları

**Klavye ve ekran okuyucu kullanan bir okuyucu olarak,** paylaşım butonlarına klavyeyle
ulaşmak ve anlamlı etiketler duymak istiyorum, böylece bu özelliği diğer kullanıcılar
gibi kullanabilirim.

#### Kabul Kriterleri

**Given** sayfadaki herhangi bir paylaşım butonu,
**Then** butonun `aria-label` değeri şu formatta olmalıdır:
"Bu yazıyı X'te paylaş", "Bu yazıyı Facebook'ta paylaş" vb.

**Given** klavyeyle Tab tuşuyla gezinim yapılıyor,
**Then** tüm paylaşım butonları Tab sırasında odak alır ve görsel odak halkası
(`focus-visible` ring) gösterilir.

**Given** bir paylaşım butonu odak aldı ve Enter tuşuna basıldı,
**Then** ilgili platform linki yeni sekmede açılır (klavyeyle tetiklenebilir).

**Given** ikonlar yalnızca dekoratif amaçlıdır,
**Then** ikon elementleri `aria-hidden="true"` ile işaretlenir; butonun görünür
veya `sr-only` metni her zaman mevcuttur.

---

## Task Dağılımı

> Tüm görevler **frontend katmanındadır** — backend değişikliği yoktur.

### Frontend Tasks

| ID | Task | Bağımlılık | Efor |
|----|------|------------|------|
| FE-T1 | `src/lib/shareLinks.ts` util dosyası: `buildShareLinks(title, url)` fonksiyonu — X, Facebook, WhatsApp, LinkedIn URL'lerini döner; URL encode işlemi dahil | — | 0.5 gün |
| FE-T2 | Lucide-react ikon teyidi: `Twitter`/`X`, `Facebook`, `Linkedin`, `MessageCircle` (WhatsApp için) ikonlarının mevcut lucide versiyonunda hangi adla export edildiğini kontrol et; eksik veya farklı adlandırılmış ikon için SVG embed alternatifleri değerlendir | — | 0.5 gün |
| FE-T3 | `ShareButtons` bileşeni (`src/components/blog/ShareButtons.tsx`): `title` ve `url` prop'larını alır; FE-T1 util'ini kullanarak platform linkleri üretir; her buton `<a>` olarak `target="_blank" rel="noopener noreferrer"` ile açılır; ikon + `sr-only` etiket içerir | FE-T1, FE-T2 tamamlanmadan başlayamaz | 1 gün |
| FE-T4 | "Link Kopyala" butonu: `ShareButtons` içinde veya ayrı küçük bileşen; `navigator.clipboard.writeText` kullanır; başarıda 2 sn "Kopyalandı" geri bildirimi (bkz. Açık Karar A3); `catch` bloğunda hata toastı | FE-T3 başlandıktan sonra paralel ilerleyebilir | 0.5 gün |
| FE-T5 | Web Share API entegrasyonu: `ShareButtons` içinde `'share' in navigator` feature-detect; destekliyorsa native "Paylaş" butonunu render et; `navigator.share({ title, url })` çağrısı; `AbortError` sessizce yutulur; diğer hatalar toast gösterir (bkz. Açık Karar A2) | FE-T3 tamamlanmadan başlayamaz | 0.5 gün |
| FE-T6 | `BlogDetailPage.tsx` içinde `ShareButtons` bileşenini yerleştir: `blogQuery.data.title` ve `window.location.href` (veya `paths.blogDetail(id)` tabanlı kanonik URL) iletilir; `LikeButton` ile birlikte düzenlenir (bkz. Açık Karar A1) | FE-T3, FE-T4, FE-T5 tamamlanmadan başlayamaz | 0.5 gün |
| FE-T7 | Erişilebilirlik: tüm butonlara `aria-label` ekle; Tab sırasını ve `focus-visible` ring görünürlüğünü doğrula; ikon `aria-hidden="true"` kontrolü | FE-T6 tamamlanmadan başlayamaz | 0.5 gün |

**Toplam tahmini efor: ~4 gün** (paralel yürütülürse 2–2.5 güne iner)

**Bağımlılık zinciri:**
```
FE-T1 ──┐
         ├──► FE-T3 ──► FE-T4 ──┐
FE-T2 ──┘         └──► FE-T5 ──┴──► FE-T6 ──► FE-T7
```

### Test Tasks

| ID | Task | Bağımlılık | Efor |
|----|------|------------|------|
| TE-T1 | `shareLinks.ts` birim testleri (Vitest): her sağlayıcı için beklenen URL çıktıları; özel karakter / Türkçe başlık encode testi; boş title/url edge case'leri | FE-T1 | 0.5 gün |
| TE-T2 | `ShareButtons` bileşen testleri (Vitest + React Testing Library): platform link href'lerinin doğruluğu; `aria-label` varlığı; kopyalama başarı/hata state geçişleri; `navigator.share` mock testi (destekli / desteksiz) | FE-T3, FE-T4, FE-T5 | 1 gün |
| TE-T3 | `BlogDetailPage` entegrasyon testi: `ShareButtons`'ın render edildiğini; `LikeButton` ile aynı anda göründüğünü doğrula | FE-T6 | 0.5 gün |

---

## Önceliklendirme (MoSCoW)

| Öncelik | Kapsam | Gerekçe |
|---------|--------|---------|
| **Must** | X, Facebook, WhatsApp, LinkedIn butonları + link kopyala | Çekirdek kullanıcı değeri; teknik riski düşük; tamamen statik link üretimi |
| **Must** | Fallback (native Share yoksa buton grubu) | Masaüstü kullanıcılarında bozuk deneyim olmaması için zorunlu |
| **Should** | Web Share API (mobil native) | Mobil UX'i belirgin şekilde iyileştirir; feature-detect ile risksiz |
| **Should** | Erişilebilirlik (aria-label, klavye) | Kullanıcı kitlesinin bir bölümü için gerekli; standart pratik |
| **Could** | UTM parametresi eklenmesi | Analitik değer yüksek; ancak bağımsız olarak sonradan eklenebilir |
| **Could** | E-posta ile paylaşım (`mailto:`) | Niş kullanım; buton grubunu büyütür |
| **Won't** (bu sürüm) | Instagram, Pinterest, Telegram butonları | Instagram web paylaşım URL'i yok; diğerleri ZnBlog hedef kitlesiyle örtüşmüyor |
| **Won't** (bu sürüm) | Paylaşım sayacı backend | Ayrı özellik; EF migration + entity gerektiriyor |

---

## Açık Kararlar (Onay Bekliyor)

Aşağıdaki kararlar ürün/tasarım tercihidir — geliştiriciler uygulamadan önce onay gerektirir.

### A1 — Paylaşım Çubuğunun Konumu
**Seçenekler:**
- **(a) Başlık altı / LikeButton yanı:** `BlogDetailPage` header bölümünde, mevcut
  `LikeButton`'ın hemen sağında veya altında. En yüksek görünürlük; başlık ve
  tarih meta bilgisiyle aynı alanda kalır.
- **(b) İçerik sonu:** `description` div'inin hemen altında, yorumlar başlamadan önce.
  Okuyucu yazıyı bitirdikten sonra paylaşım kararı verir — conversion açısından
  mantıklı.
- **(c) İkisi birden:** Başlıktan sonra compact ikon sırasına, içerik sonunda tam buton
  grubuna. Kod tekrarı artar; gereksiz yere karar sayısını ikiye katlar.
- **(d) Sticky sidebar (masaüstü):** Orta-uzun vadeli seçenek; şu anki max-w-3xl
  layout'u sidebar için uygun değil; Faz 5 sonrasına ertelenmeli.

**Öneri:** **(b) içerik sonu** — okuyucu yazıyı tükettiğinde paylaşım eylemini
tetiklemesi daha doğal bir kullanıcı akışıdır.

---

### A2 — Web Share API Görünüm Modeli
**Seçenekler:**
- **(a) Ek buton:** Mevcut platform butonlarına ek olarak "Paylaş" native butonu gösterilir.
  Destekleyen cihazlarda 5 buton görünür.
- **(b) Yerine geçer:** Destekleyen cihazlarda platform butonları gizlenir, yalnızca
  native "Paylaş" butonu gösterilir.
- **(c) En üstte yer alır:** Native destekleniyorsa "Paylaş" butonu en önce, platform
  butonları yanında görünür; her iki yol sunulur.

**Öneri:** **(c) en üstte yer alır** — native deneyim öne çıkarılır ama fallback
her zaman görünür kalır; kullanıcı tercihini kullanabilir.

---

### A3 — Link Kopyala Geri Bildirimi
**Seçenekler:**
- **(a) Sonner toast:** Mevcut `toast()` altyapısı kullanılır. Tutarlı; her sayfadan
  zaten kullanılan pattern.
- **(b) Inline buton değişimi:** Buton metni/ikonu "Link Kopyala" → "Kopyalandı!"
  olarak geçici değişir, 2 sn sonra eski haline döner.

**Öneri:** **(b) inline** — kopyalama eylemi küçük ve yerel; toast'ı tetiklemek
sayfanın diğer köşesine dikkat çeker. Butondaki anlık değişim daha az rahatsız
edicidir. Hata durumunda ise **(a) toast** kullanılmalı (kritik geri bildirim).

---

### A4 — Kanonik URL Üretimi
**Seçenekler:**
- **(a) `window.location.href`:** Anlık URL; query string veya hash varsa dahil olur.
- **(b) `paths.blogDetail(id)` + `window.location.origin`:** Temiz kanonik URL;
  `?page=2` veya `#yorum-5` gibi geçici parametreler kesilir.

**Öneri:** **(b)** — paylaşım linklerinin temiz ve tutarlı olması, arama motorları
ve sosyal medya önizlemeleri için daha güvenlidir.

---

### A5 — UTM Parametresi
**Seçenekler:**
- **(a) Bu sürümde yok:** Linkler UTM içermez; analitik izleme ileride eklenir.
- **(b) Bu sürümde ekle:** `?utm_source=twitter&utm_medium=social&utm_campaign=blog-share`
  formatında her sağlayıcıya özel UTM eklenir.

**Öneri:** **(b)** — UTM eklemek FE-T1 util katmanında birkaç satırlık ek iştir.
Analytics değeri yüksek; sonradan eklemek için yeni deployment gerekir. Ancak
sitenin Google Analytics / herhangi bir analitik aracı entegrasyonu yoksa **(a)**
tercih edilmeli (anlamsız parametre kirliliği önlenir).

**Teknik Karar Noktası:** Proje şu an herhangi bir web analitik servisi kullanıyor mu?
Bu sorunun cevabı A5 tercihini doğrudan belirler.

---

### A6 — E-posta ile Paylaş Butonu
**Seçenekler:**
- **(a) Kapsam dışı:** Sağlayıcı setine dahil edilmez.
- **(b) Ekle:** `mailto:?subject=<başlık>&body=<URL>` formatında bir buton eklenir.

**Öneri:** **(a) kapsam dışı** — e-posta paylaşımı niş kullanımdır; buton grubunu
büyütür ve görsel kalabalık yaratır. İstenen kitleye (X/WhatsApp kullanıcıları)
değer katmaz.

---

## Riskler ve Açık Sorular

### Risk 1 — Lucide İkon Adları (Orta)
Lucide-react kütüphanesi Twitter/X ikonunu farklı sürümlerde farklı isimlendirmiştir
(`Twitter`, `BrandTwitter`, vb.). Proje hangi lucide versiyonunu kullandığını
FE-T2 görevi sırasında teyit etmelidir. Eksikse alternatif SVG embed yolu gerekir.

**Azaltma:** FE-T2 görevini diğer görevlerle paralel olarak ilk günde tamamla;
ikon yoksa üretici (developer) marka SVG'lerini `public/icons/` altına alarak
`<img>` veya inline SVG ile servis et.

### Risk 2 — Web Share API Tarayıcı Desteği (Düşük)
`navigator.share` Chrome 61+, Safari 12.1+, Firefox (yalnızca Android) destekler.
Masaüstü Firefox ve eski tarayıcılarda yoktur. Ancak feature-detect zorunlu olduğundan
fallback zaten planlanmıştır — risk yönetilebilir.

### Risk 3 — `navigator.clipboard` Güvensiz Bağlam (Düşük-Orta)
`navigator.clipboard.writeText` yalnızca HTTPS veya `localhost`'ta çalışır.
Geliştirme ortamı `https://localhost:7253` (CLAUDE.md madde 8 — https profili)
kullandığından sorun yok. Farklı bir test ortamı HTTP üzerinden çalışıyorsa
kopyalama sessiz başarısız olabilir — US-2 kabul kriterindeki hata toastı bu
durumu yakalamalıdır.

### Risk 4 — Facebook Open Graph Meta Etiketleri (Orta-Yüksek)
Facebook ve LinkedIn paylaşım penceresindeki önizleme (başlık, açıklama, görsel)
`<head>` içindeki Open Graph meta etiketlerinden (`og:title`, `og:description`,
`og:image`) çekilir. Bu etiketler şu an `BlogDetailPage` içinde bulunmadığından
paylaşım önizlemesi boş veya yanlış görünebilir.

**Azaltma:** Bu özelliğin kapsamına OG meta eklenmesi dahil edilmemiştir.
Geliştiriciler `react-helmet-async` veya React 19 native `<title>`/`<meta>`
desteğini ayrı bir görev olarak ele almalıdır. Paylaşım *linkleri* OG meta
olmadan da çalışır; önizleme kalitesi etkilenir.

**Teknik Karar Noktası:** OG meta etiket desteği bu sprint'e dahil edilsin mi,
yoksa Faz 6 SEO çalışmalarına ertelensin mi?

### Risk 5 — Sosyal Medya Sağlayıcı API Değişiklikleri (Düşük)
Twitter/X paylaşım URL'i (`twitter.com/intent/tweet`) X Corp tarafından değiştirilebilir.
Şu an `x.com/intent/tweet` de çalışmaktadır. Uzun vadede `x.com` adresine geçiş
düşünülmeli; FE-T1 util katmanında sabit string yerine bir config nesnesi
kullanmak gelecekteki değişikliği kolaylaştırır.

**Teknik Karar Noktası:** X paylaşım domain'i `twitter.com` mu `x.com` mu kullanılmalı?
Her ikisi şu an çalışıyor; ancak `x.com` daha güncel ve uzun vadeli.

---

## Madde 13 Uyum Özeti

| CLAUDE.md Madde 13 İfadesi | Bu Plan Karşılığı |
|----------------------------|-------------------|
| "Sosyal paylaşım = Web Share API" | US-3, FE-T5: `navigator.share` feature-detect + çağrısı |
| "/ paylaş linkleri" | US-1, FE-T1 + FE-T3: X/Facebook/WhatsApp/LinkedIn URL üretimi |
| Backend gerektirmez (istemci tarafı) | Tüm task'lar frontend; backend task yok |
| Kullanıcı adına otomatik post atma YOK | Kapsam "Hariç" bölümünde açıkça belirtildi |

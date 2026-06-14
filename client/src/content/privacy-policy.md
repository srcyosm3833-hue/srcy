# Kişisel Verilerin Korunması Aydınlatma Metni ve Gizlilik Politikası

**Yürürlük Tarihi:** 13 Haziran 2026

---

> **UYARI — TASLAK BELGE**
>
> Bu belge bir TASLAКТIR. İçerdiği bilgiler, ZnBlogApp projesinin teknik planına (bkz. `docs/features/ADMIN-AUDIT-ROLES-PLAN.md`) dayanılarak hazırlanmış olup hukuki bağlayıcılığı yoktur. **Yayına almadan önce mutlaka bir hukuk danışmanına / avukata danışılmalıdır.** Bilgilerin güncelliği ve doğruluğu için hukuki inceleme zorunludur.

---

## 1. Veri Sorumlusu

**ZnBlogApp**, 6698 sayılı Kişisel Verilerin Korunması Kanunu (KVKK) uyarınca veri sorumlusu sıfatını taşır.

İletişim: Sitemizde yer alan iletişim formu aracılığıyla bize ulaşabilirsiniz.

---

## 2. Toplanan Kişisel Veriler ve Saklama Yöntemleri

### 2.1 Hesap ve Kimlik Verileri

Platforma kayıt olurken aşağıdaki veriler toplanır:

| Veri | Açıklama |
|---|---|
| Ad ve Soyad | Profil bilgisi olarak kullanılır |
| E-posta Adresi | Giriş kimlik doğrulaması ve iletişim için kullanılır |
| Şifreli Parola | Güvenli şifreleme (hash) ile saklanır; düz metin olarak tutulmaz |
| Hesap Oluşturma Tarihi | Kayıt zamanı damgası |

**Sosyal Giriş (Google / Facebook):** Sosyal kimlik doğrulama tercih edilmesi durumunda ilgili sağlayıcı (Google veya Facebook) tarafından paylaşılan ad, soyad ve e-posta adresi işlenir. Ham şifre tutulmaz. Facebook gibi sağlayıcıların e-posta adresini paylaşmaması halinde hesap oluşturulmaz ve bu durum kullanıcıya bildirilir.

### 2.2 Blog İçeriği Oluşturma — IP Adresi Kaydı

Blog gönderisi oluşturulduğunda, içeriği oluşturan kullanıcının IP adresi güvenlik amacıyla kaydedilir.

**Önemli:** IP adresi **ham biçimde saklanmaz.** SHA-256 algoritması ve platforma özgü bir tuz (salt) değeri kullanılarak tek yönlü bir hash'e dönüştürülür. Bu yöntemle:

- Orijinal IP adresine hash'ten geri dönmek teknik olarak mümkün değildir.
- Aynı IP adresinden gelen istekler hash eşleşmesiyle tespit edilebilir; ancak IP'nin kendisi hiçbir sistemde düz metin olarak yer almaz.

| Alan | İçerik | Saklama Biçimi |
|---|---|---|
| Oluşturanın IP Hash'i | Gönderinin geldiği IP adresinin hash karşılığı | SHA-256 tuzlu hash |
| Kayıt Tarihi | İçeriğin oluşturulduğu tarih ve saat (UTC) | Zaman damgası |

**Saklama Süresi:** Blog IP hash kayıtları oluşturulma tarihinden itibaren en fazla **6 (altı) ay** saklanır. Süre dolduğunda otomatik silme veya anonimleştirme mekanizması devreye girer.

**Bu veri genel kullanıcılara gösterilmez.** Yalnızca yetkili platform yöneticileri (Admin) tarafından incelenebilir.

### 2.3 İletişim Formu Mesajları — IP Adresi Kaydı

İletişim formu aracılığıyla mesaj gönderildiğinde, spam ve kötüye kullanım tespiti amacıyla gönderenin IP adresi kaydedilir.

**Önemli:** Bu kayıt da 2.2 bölümünde açıklanan yöntemle SHA-256 tuzlu hash olarak tutulur; ham IP adresi saklanmaz.

| Alan | İçerik | Saklama Biçimi |
|---|---|---|
| Gönderenin IP Hash'i | Mesajın geldiği IP adresinin hash karşılığı | SHA-256 tuzlu hash |
| Kayıt Tarihi | Mesajın gönderildiği tarih ve saat (UTC) | Zaman damgası |

**Saklama Süresi:** Mesaj IP hash kayıtları gönderilme tarihinden itibaren en fazla **6 (altı) ay** saklanır.

### 2.4 Arama Geçmişi (Arama Audit Logu)

Platformdaki blog arama işlevini kullandığınızda aşağıdaki veriler kaydedilir:

| Alan | İçerik | Notlar |
|---|---|---|
| Aranan Terim | Arama kutusuna girilen metin | En fazla 200 karakter |
| Kullanıcı Kimliği | Giriş yapılmışsa hesap kimliği (UserId) | Anonim aramalarda boş bırakılır |
| Kullanıcı Adı-Soyadı | Giriş yapılmışsa ad-soyad bilgisi | Anonim aramalarda boş bırakılır |
| IP Hash | Aramanın yapıldığı cihazın IP hash'i | SHA-256 tuzlu hash; ham IP saklanmaz |
| Arama Tarihi | İşlemin gerçekleştiği tarih ve saat (UTC) | Zaman damgası |

**Anonim Aramalar:** Platforma giriş yapılmadan gerçekleştirilen aramalarda kullanıcı kimliği ve ad-soyad alanları boş kalır; yalnızca IP hash ve arama terimi kaydedilir.

**Saklama Süreleri:**

- Giriş yapmış kullanıcıya ait arama kayıtları: en fazla **3 (üç) ay**
- Anonim (kimliksiz) arama kayıtları: en fazla **90 (doksan) gün**

Süre dolduğunda kayıtlar otomatik olarak silinir veya anonimleştirilir.

**Erişim Kısıtlaması:** Arama logu kayıtları kişisel veri niteliği taşıyabileceğinden yalnızca platform yöneticisi (Admin) tarafından erişilebilir. İçerik yöneticileri (Manager) bu verilere erişemez.

**Kullanım Amacı Sınırlaması:** Arama terimleri yalnızca hizmet kalitesi ve güvenlik amacıyla incelenir; kullanıcı profili oluşturma veya hedefli reklam gibi amaçlarla kullanılmaz.

### 2.5 Mesaj Okunma Kaydı (İç Denetim)

Bir platform yöneticisi veya içerik yöneticisi iletişim formu mesajını okuduğunda şu bilgiler kaydedilir:

| Alan | İçerik |
|---|---|
| Okuyan Yöneticinin Kimliği | Mesajı okuyan personelin sistem kimliği |
| Okunma Tarihi ve Saati | İşlemin gerçekleştiği UTC zaman damgası |

Bu kayıt yalnızca iç süreç denetimi (audit trail) amacıyla tutulur. Personel faaliyetlerine ilişkin bu verilerin işlenmesinde meşru menfaat ve iç denetim yükümlülüğü yasal dayanağı oluşturur.

---

## 3. Verilerin İşlenme Amaçları ve Yasal Dayanağı

Platformumuzda kişisel veriler aşağıdaki amaçlarla işlenmektedir:

| Amaç | Yasal Dayanak |
|---|---|
| Kullanıcı hesabı oluşturma ve kimlik doğrulama | KVKK m.5/2-c — Sözleşmenin ifası |
| İçerik güvenliği: spam, taciz ve kötüye kullanım tespiti | KVKK m.5/2-f — Meşru menfaat |
| İçerik sahipliği doğrulama ve anlaşmazlık çözümü | KVKK m.5/2-f — Meşru menfaat |
| Hizmet kalitesinin ölçülmesi ve iyileştirilmesi | KVKK m.5/2-f — Meşru menfaat |
| İç süreç denetimi ve sorumluluk zinciri takibi | KVKK m.5/2-f — Meşru menfaat |

**Meşru menfaat gerekçesinin açıklaması:** 6698 sayılı KVKK'nın 5. maddesinin 2. fıkrasının (f) bendi uyarınca, ilgili kişinin temel hak ve özgürlüklerine zarar vermemek kaydıyla, veri sorumlusunun meşru menfaatleri için veri işlenmesi mümkündür. Platformun güvenliğinin sağlanması, kötüye kullanımın engellenmesi ve içerik bütünlüğünün korunması bu kapsamda değerlendirilmektedir.

---

## 4. Veri Güvenliği

Kişisel verilerinizin güvenliğini sağlamak için aşağıdaki teknik önlemler uygulanmaktadır:

- **IP Adresleri:** Hiçbir IP adresi ham (düz metin) biçimde veritabanında tutulmaz. Tüm IP verileri SHA-256 algoritmasıyla platforma özgü bir tuz değeri kullanılarak hash'lenir. Orijinal IP'ye geri dönmek teknik olarak imkansızdır.
- **Parolalar:** Kullanıcı parolaları güçlü kriptografik hash algoritmasıyla saklanır; düz metin olarak hiçbir yerde yer almaz.
- **Erişim Kontrolü:** Hassas veriler (IP hash'leri, arama logları, audit kayıtları) yalnızca yetkili personele açıktır; her erişim rol tabanlı yetkilendirmeyle denetlenir.
- **Token Güvenliği:** Kimlik doğrulama token'ları kısa ömürlüdür (yaklaşık 15 dakika); yenileme token'ları da hash'li biçimde saklanır ve her kullanımda yenilenir (rotation).
- **Aktarım Güvenliği:** Platform ile sunucu arasındaki tüm iletişim HTTPS üzerinden şifreli olarak gerçekleştirilir.

---

## 5. Verilerin Paylaşımı

Kişisel verileriniz üçüncü taraflarla **ticari amaçlarla paylaşılmaz.** Aşağıdaki durumlar saklıdır:

- **Sosyal Giriş Sağlayıcıları:** Google veya Facebook üzerinden giriş yapılması durumunda bu sağlayıcıların kimlik doğrulama altyapısı kullanılır. Aktarılan veriler giriş süreciyle sınırlıdır ve ilgili sağlayıcıların gizlilik politikaları geçerlidir.
- **Yasal Zorunluluk:** Yetkili makamların yasal talepleri doğrultusunda gerekli bilgiler paylaşılabilir.

---

## 6. Verilerin Saklanma Süreleri (Özet Tablo)

| Veri Kategorisi | Azami Saklama Süresi | Süre Sonrası İşlem |
|---|---|---|
| Hesap ve kimlik verileri | Hesap aktif olduğu sürece; silme talebinde derhal | Soft-delete + kalıcı silme |
| Blog oluşturucu IP hash'i | 6 ay | Otomatik silme / anonimleştirme |
| Mesaj gönderici IP hash'i | 6 ay | Otomatik silme / anonimleştirme |
| Arama logu (giriş yapmış) | 3 ay | Otomatik silme / anonimleştirme |
| Arama logu (anonim) | 90 gün | Otomatik silme |
| Mesaj okunma kaydı (ReadByUserId) | 2 yıl | İç denetim arşivi; ardından silme |

Saklama süreleri, platforma yerleşik otomatik temizleme mekanizması tarafından yönetilir.

---

## 7. Veri Sahibinin Hakları (KVKK Madde 11)

6698 sayılı Kişisel Verilerin Korunması Kanunu'nun 11. maddesi uyarınca aşağıdaki haklara sahipsiniz:

| Hak | Açıklama |
|---|---|
| Bilgi edinme hakkı | Kişisel verilerinizin işlenip işlenmediğini öğrenme |
| Erişim hakkı | İşlenen kişisel verilerinizi ve bunlara ilişkin bilgileri talep etme |
| Düzeltme hakkı | Yanlış veya eksik verilerin düzeltilmesini isteme |
| Silme hakkı | Kişisel verilerin silinmesini talep etme (KVKK m.7 koşulları dahilinde) |
| İşlemenin kısıtlanması | Belirli koşullarda veri işlemenin kısıtlanmasını isteme |
| İtiraz hakkı | Meşru menfaat gerekçesiyle yürütülen işleme itiraz etme |
| Otomatik kararlardan korunma | Tamamen otomatik işlemler sonucu aleyhinize çıkan kararların incelenmesini isteme |
| Zararın giderilmesi | Kanuna aykırı işleme nedeniyle uğranılan zararın tazmin edilmesini talep etme |

**Başvuru Kanalı:** Yukarıdaki haklarınızı kullanmak için platformdaki **iletişim formu** aracılığıyla bize ulaşabilirsiniz. Başvurularınız KVKK'nın 13. maddesi kapsamında en geç 30 (otuz) gün içinde yanıtlanır.

**Şikayet Hakkı:** Başvurunuzun yanıtsız kalması veya yanıtın yetersiz bulunması durumunda Kişisel Verileri Koruma Kurumu'na (KVKK) başvurma hakkınız saklıdır.

---

## 8. Çerezler (Cookies)

Platform, temel işlevsellik için oturum yönetimi amacıyla kimlik doğrulama token'larını tarayıcı localStorage alanında saklar. Üçüncü taraf izleme veya reklam çerezi kullanılmamaktadır.

---

## 9. Bu Politikadaki Değişiklikler

Bu aydınlatma metni, platformun özellikleri veya yasal yükümlülükler değiştiğinde güncellenebilir. Önemli değişiklikler, güncellenmiş tarih ve değişiklik özeti ile birlikte bu sayfada duyurulur.

---

## 10. İletişim

Gizlilik politikamıza ilişkin sorularınız için platformdaki iletişim formunu kullanabilirsiniz.

---

> **SON NOT — TASLAK UYARISI**
>
> Bu belge bir TASLAКТIR. Hukuki bağlayıcılığı yoktur. IP logu, arama logu ve audit özellikleri üretime alınmadan önce bu metnin bir hukuk danışmanı / avukat tarafından incelenmesi ve onaylanması zorunludur. KVKK m.10 ve GDPR Art. 13 kapsamındaki aydınlatma yükümlülüğü, bu metnin hukuki denetime tabi tutularak yayımlanmasını gerektirmektedir.

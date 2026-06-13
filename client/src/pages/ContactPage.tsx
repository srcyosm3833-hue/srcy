import { PageHeader } from '@/components/common/PageHeader'
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { ContactForm } from '@/components/contact/ContactForm'
import { ContactInfo } from '@/components/contact/ContactInfo'
import { SocialMediaLinks } from '@/components/contact/SocialMediaLinks'

/**
 * Iletisim sayfasi: ziyaretci mesaj gonderir (POST /api/messages) ve site iletisim
 * bilgileri (GET /api/contact, 404 zarif) + sosyal medya baglantilarini (GET
 * /api/social-media, bos olabilir) gorur. Form ve bilgi kartlari iki sutunlu
 * (lg) yerlesimde; mobilde tek sutun.
 */
export default function ContactPage() {
  return (
    <div className="mx-auto max-w-5xl px-4 py-12 sm:px-6 lg:px-8">
      <PageHeader
        title="İletişim"
        description="Bizimle iletişime geçin. Sorularınızı, önerilerinizi bekliyoruz."
      />

      <div className="mt-10 grid grid-cols-1 gap-8 lg:grid-cols-2 lg:gap-12">
        {/* Sol: mesaj formu */}
        <Card>
          <CardHeader>
            <CardTitle className="font-heading text-xl">Mesaj Gönder</CardTitle>
          </CardHeader>
          <CardContent>
            <ContactForm />
          </CardContent>
        </Card>

        {/* Sag: iletisim bilgileri + sosyal medya */}
        <div className="space-y-8">
          <Card>
            <CardHeader>
              <CardTitle className="font-heading text-xl">
                İletişim Bilgileri
              </CardTitle>
            </CardHeader>
            <CardContent>
              <ContactInfo />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="font-heading text-xl">
                Bizi Takip Edin
              </CardTitle>
            </CardHeader>
            <CardContent>
              <SocialMediaLinks />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

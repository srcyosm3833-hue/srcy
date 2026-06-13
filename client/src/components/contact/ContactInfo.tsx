import { Mail, MapPin, Phone } from 'lucide-react'

import { useContact } from '@/features/contact'
import { normalizeApiError } from '@/lib/api'
import { Skeleton } from '@/components/ui/skeleton'
import { ErrorState } from '@/components/common/ErrorState'

/**
 * Site iletisim bilgilerini gosterir (GET /api/contact). Uygulama tek bir Contact
 * kaydi tutar; henuz yapilandirilmadiysa backend 404 doner — bu BEKLENEN bir
 * durumdur ve zarif "bilgi yok" mesajiyla ele alinir (hard error degil). Yalnizca
 * 404 disindaki gercek hatalar ErrorState ile gosterilir. mapUrl varsa harita iframe.
 */
export function ContactInfo() {
  const contactQuery = useContact()

  if (contactQuery.isPending) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-5 w-1/2" />
        <Skeleton className="h-5 w-2/3" />
        <Skeleton className="aspect-video w-full rounded-xl" />
      </div>
    )
  }

  if (contactQuery.isError) {
    const normalized = normalizeApiError(contactQuery.error)

    // 404: kayit yok -> zarif placeholder (hata degil).
    if (normalized.status === 404) {
      return (
        <p className="text-sm text-muted-foreground">
          Henüz iletişim bilgisi eklenmemiş.
        </p>
      )
    }

    return (
      <ErrorState
        message="İletişim bilgileri yüklenemedi."
        onRetry={() => contactQuery.refetch()}
        className="py-8"
      />
    )
  }

  const contact = contactQuery.data

  return (
    <div className="space-y-4">
      <ul className="space-y-3">
        {contact.address ? (
          <ContactDetailRow icon={<MapPin className="h-5 w-5" />}>
            {contact.address}
          </ContactDetailRow>
        ) : null}
        {contact.phone ? (
          <ContactDetailRow icon={<Phone className="h-5 w-5" />}>
            <a
              href={`tel:${contact.phone}`}
              className="transition-colors hover:text-foreground"
            >
              {contact.phone}
            </a>
          </ContactDetailRow>
        ) : null}
        {contact.email ? (
          <ContactDetailRow icon={<Mail className="h-5 w-5" />}>
            <a
              href={`mailto:${contact.email}`}
              className="transition-colors hover:text-foreground"
            >
              {contact.email}
            </a>
          </ContactDetailRow>
        ) : null}
      </ul>

      {contact.mapUrl ? (
        <iframe
          src={contact.mapUrl}
          title="Konum haritası"
          loading="lazy"
          referrerPolicy="no-referrer-when-downgrade"
          className="aspect-video w-full rounded-xl border border-border"
        />
      ) : null}
    </div>
  )
}

/** Ikon + metin satiri (adres/telefon/e-posta). */
function ContactDetailRow({
  icon,
  children,
}: {
  icon: React.ReactNode
  children: React.ReactNode
}) {
  return (
    <li className="flex items-start gap-3 text-sm text-muted-foreground">
      <span className="mt-0.5 shrink-0 text-accent">{icon}</span>
      <span className="min-w-0 break-words">{children}</span>
    </li>
  )
}

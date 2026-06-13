import { useRef, useState } from 'react'
import { ImageIcon, Loader2, Upload, X } from 'lucide-react'

import { uploadApi, normalizeApiError } from '@/lib/api'
import { resolveAssetUrl } from '@/lib/resolveAssetUrl'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

/** Backend ile uyumlu istemci tarafi on-dogrulama sinirlari. */
const MAX_SIZE_BYTES = 5 * 1024 * 1024 // 5 MB
const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

interface ImageUploadFieldProps {
  /** Alan etiketi (ekran okuyucu icin). */
  label: string
  /** Mevcut URL degeri (form'dan gelir). */
  value: string
  /** URL degisince cagrilir (manuel girisle veya yukleme sonrasi). */
  onChange: (url: string) => void
  /** Girdi devre disi mi (form submit sirasinda). */
  disabled?: boolean
}

/**
 * Gorsel alani: ya bir URL elle girilir ya da bir dosya secilip POST /api/uploads
 * ile yuklenir; donen URL forma yazilir. Onizleme gosterir. Dosya boyutu/turu
 * istemcide on-dogrulanir (5 MB; jpg/jpeg/png/webp), kesin dogrulama backend'de.
 *
 * NOT: Backend goreli bir URL dondurebilir (orn. "/uploads/abc.jpg"). Onizlemede
 * VITE_API_BASE_URL ile mutlak hale getirilir; forma yazilan deger backend'in
 * dondurdugu ham degerdir (blog alanlari URL string saklar).
 */
export function ImageUploadField({
  label,
  value,
  onChange,
  disabled,
}: ImageUploadFieldProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [isUploading, setIsUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Goreli URL'i onizleme icin mutlaklastir; mutlak/bos ise oldugu gibi kullan.
  const previewSrc = resolveAssetUrl(value)

  async function handleFileSelected(file: File) {
    setError(null)

    if (!ACCEPTED_TYPES.includes(file.type)) {
      setError('Yalnızca JPG, PNG veya WEBP yükleyebilirsiniz.')
      return
    }
    if (file.size > MAX_SIZE_BYTES) {
      setError("Dosya boyutu 5 MB'ı aşmamalıdır.")
      return
    }

    setIsUploading(true)
    try {
      const result = await uploadApi.upload(file)
      onChange(result.url)
    } catch (err) {
      setError(normalizeApiError(err).message)
    } finally {
      setIsUploading(false)
      // Ayni dosyanin tekrar secilebilmesi icin input'u sifirla.
      if (inputRef.current) inputRef.current.value = ''
    }
  }

  return (
    <div className="space-y-3">
      {/* Onizleme */}
      {previewSrc ? (
        <div className="relative w-full overflow-hidden rounded-md border border-border">
          <img
            src={previewSrc}
            alt={`${label} önizleme`}
            className="aspect-video w-full object-cover"
          />
          <Button
            type="button"
            variant="secondary"
            size="icon"
            className="absolute right-2 top-2 h-7 w-7"
            onClick={() => onChange('')}
            disabled={disabled || isUploading}
            aria-label="Görseli kaldır"
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      ) : (
        <div className="flex aspect-video w-full items-center justify-center rounded-md border border-dashed border-border bg-muted/30 text-muted-foreground">
          <ImageIcon className="h-8 w-8" />
        </div>
      )}

      {/* URL girisi + yukleme butonu */}
      <div className="flex flex-col gap-2 sm:flex-row">
        <Input
          type="url"
          inputMode="url"
          placeholder="https://… (URL girin veya yükleyin)"
          value={value}
          onChange={(event) => onChange(event.target.value)}
          disabled={disabled || isUploading}
          aria-label={`${label} URL`}
        />
        <input
          ref={inputRef}
          type="file"
          accept="image/jpeg,image/png,image/webp"
          className="hidden"
          onChange={(event) => {
            const file = event.target.files?.[0]
            if (file) void handleFileSelected(file)
          }}
        />
        <Button
          type="button"
          variant="outline"
          className="shrink-0"
          onClick={() => inputRef.current?.click()}
          disabled={disabled || isUploading}
        >
          {isUploading ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              Yükleniyor…
            </>
          ) : (
            <>
              <Upload className="h-4 w-4" />
              Görsel Seç
            </>
          )}
        </Button>
      </div>

      {error ? (
        <p className="text-sm font-medium text-destructive">{error}</p>
      ) : null}
    </div>
  )
}

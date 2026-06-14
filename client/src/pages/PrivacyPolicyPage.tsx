import Markdown from 'react-markdown'
import remarkGfm from 'remark-gfm'

// Aydinlatma metni icerigi build'e dahil edilsin diye ham string olarak import
// edilir. Kaynak: docs/PRIVACY-POLICY.md ile birebir tutulan kopya.
import privacyMd from '@/content/privacy-policy.md?raw'

/**
 * KVKK aydinlatma metni / gizlilik politikasi sayfasi (public, anonim erisime
 * acik). Icerik markdown'dan render edilir; tablolar icin remark-gfm kullanilir.
 * Register formundaki onay kutusu bu sayfaya (yeni sekmede) link verir.
 */
export default function PrivacyPolicyPage() {
  return (
    <div className="mx-auto max-w-3xl px-4 py-12 sm:px-6 lg:px-8">
      <div className="space-y-4 text-sm leading-relaxed text-foreground">
        <Markdown
          remarkPlugins={[remarkGfm]}
          components={{
            h1: ({ children }) => (
              <h1 className="font-heading text-3xl font-bold tracking-tight sm:text-4xl">
                {children}
              </h1>
            ),
            h2: ({ children }) => (
              <h2 className="mt-10 font-heading text-2xl font-semibold tracking-tight">
                {children}
              </h2>
            ),
            h3: ({ children }) => (
              <h3 className="mt-6 font-heading text-lg font-semibold">
                {children}
              </h3>
            ),
            p: ({ children }) => (
              <p className="text-muted-foreground">{children}</p>
            ),
            ul: ({ children }) => (
              <ul className="list-disc space-y-1 pl-6 text-muted-foreground">
                {children}
              </ul>
            ),
            ol: ({ children }) => (
              <ol className="list-decimal space-y-1 pl-6 text-muted-foreground">
                {children}
              </ol>
            ),
            li: ({ children }) => <li>{children}</li>,
            strong: ({ children }) => (
              <strong className="font-semibold text-foreground">
                {children}
              </strong>
            ),
            a: ({ href, children }) => (
              <a
                href={href}
                className="font-medium text-primary underline-offset-4 hover:underline"
              >
                {children}
              </a>
            ),
            hr: () => <hr className="my-8 border-border" />,
            blockquote: ({ children }) => (
              <blockquote className="rounded-md border-l-4 border-primary/40 bg-muted/50 px-4 py-2 text-muted-foreground">
                {children}
              </blockquote>
            ),
            // GFM tablolari: yatay kaydirilabilir, ince cizgili.
            table: ({ children }) => (
              <div className="overflow-x-auto">
                <table className="w-full border-collapse text-left text-sm">
                  {children}
                </table>
              </div>
            ),
            thead: ({ children }) => (
              <thead className="border-b border-border">{children}</thead>
            ),
            th: ({ children }) => (
              <th className="px-3 py-2 font-semibold text-foreground">
                {children}
              </th>
            ),
            td: ({ children }) => (
              <td className="border-b border-border px-3 py-2 align-top text-muted-foreground">
                {children}
              </td>
            ),
          }}
        >
          {privacyMd}
        </Markdown>
      </div>
    </div>
  )
}

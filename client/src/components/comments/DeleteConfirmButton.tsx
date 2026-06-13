import { useState } from 'react'
import { Loader2, Trash2 } from 'lucide-react'

import { Button } from '@/components/ui/button'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'

interface DeleteConfirmButtonProps {
  /** Onay penceresi basligi. */
  title: string
  /** Onay aciklamasi. */
  description: string
  /** Onaylandiginda cagrilir (silme mutation'i). Promise cozulunce pencere kapanir. */
  onConfirm: () => Promise<void>
  /** Tetikleyici buton etiketi (gizli ekran okuyucu icin de). */
  triggerLabel?: string
}

/**
 * Yikici silme aksiyonu icin onayli buton. Tetikleyici kompakt bir "Sil" link/buton;
 * AlertDialog ile onay alir. Silme sirasinda action butonu disabled + spinner gosterir.
 * Yorum ve yanit silmede ortak kullanilir.
 */
export function DeleteConfirmButton({
  title,
  description,
  onConfirm,
  triggerLabel = 'Sil',
}: DeleteConfirmButtonProps) {
  const [open, setOpen] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)

  async function handleConfirm() {
    setIsDeleting(true)
    try {
      await onConfirm()
      setOpen(false)
    } finally {
      // Hata olsa bile butonu tekrar aktif et; toast cagiran mutation onError'da gosterilir.
      setIsDeleting(false)
    }
  }

  return (
    <AlertDialog open={open} onOpenChange={setOpen}>
      <AlertDialogTrigger asChild>
        <Button
          variant="ghost"
          size="sm"
          className="h-auto px-2 py-1 text-xs text-muted-foreground hover:text-destructive"
        >
          <Trash2 className="h-3.5 w-3.5" />
          {triggerLabel}
        </Button>
      </AlertDialogTrigger>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription>{description}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isDeleting}>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            // onClick ile preventDefault: pencere, silme cozulene kadar acik kalsin.
            onClick={(event) => {
              event.preventDefault()
              void handleConfirm()
            }}
            disabled={isDeleting}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {isDeleting ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                Siliniyor…
              </>
            ) : (
              'Sil'
            )}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

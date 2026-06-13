import { useState } from 'react'
import { Loader2 } from 'lucide-react'

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'

interface ConfirmDeleteDialogProps {
  /** Kontrollu acik durumu (parent tutar; dropdown-menu icinden tetiklenir). */
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  description: string
  /** Onaylandiginda cagrilir (silme mutation'i). Promise cozulunce pencere kapanir. */
  onConfirm: () => Promise<void>
  /** Action butonu etiketi. */
  confirmLabel?: string
}

/**
 * Kontrollu (parent'in open state'ini tuttugu) silme onay penceresi. Admin
 * tablolarinda dropdown menusunden tetiklenir. Silme sirasinda action disabled +
 * spinner; pencere islem cozulene kadar acik kalir (preventDefault). Hata olsa
 * bile butonu tekrar aktif eder (toast cagiran mutation'da gosterilir).
 */
export function ConfirmDeleteDialog({
  open,
  onOpenChange,
  title,
  description,
  onConfirm,
  confirmLabel = 'Sil',
}: ConfirmDeleteDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false)

  async function handleConfirm() {
    setIsDeleting(true)
    try {
      await onConfirm()
      onOpenChange(false)
    } finally {
      setIsDeleting(false)
    }
  }

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription>{description}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isDeleting}>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
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
              confirmLabel
            )}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

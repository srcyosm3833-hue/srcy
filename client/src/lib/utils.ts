import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

/**
 * shadcn/ui standart yardimcisi: kosullu class adlarini (clsx) birlestirir ve
 * cakisan Tailwind utility'lerini (twMerge) sadelestirir.
 * Ornek: cn("px-2", isActive && "px-4") -> "px-4"
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs))
}

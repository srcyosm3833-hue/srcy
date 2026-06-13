import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { AuthProvider } from '@/features/auth'
import { queryClient } from '@/lib/api'
import { router } from '@/routes/router'
import { ThemeProvider } from '@/components/theme/ThemeProvider'
import { Toaster } from '@/components/ui/sonner'

/**
 * Uygulama kok bileseni. Saglayicilarin sirasi:
 *  ThemeProvider          -> light/dark tema (html.class + localStorage)
 *    QueryClientProvider  -> server state cache (TanStack Query)
 *      AuthProvider       -> kullanici/oturum durumu (router guard'lari buna bagli)
 *        RouterProvider   -> data router (createBrowserRouter)
 *
 * AuthProvider, RouterProvider'i sarmalamali ki route guard'lari (ProtectedRoute)
 * useAuth'a erisebilsin. Toaster (Sonner) en distaki tema baglamini paylasir.
 */
export default function App() {
  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <RouterProvider router={router} />
          <Toaster richColors position="top-right" />
        </AuthProvider>
      </QueryClientProvider>
    </ThemeProvider>
  )
}

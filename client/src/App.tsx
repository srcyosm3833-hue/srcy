import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { AuthProvider } from '@/features/auth'
import { queryClient } from '@/lib/api'
import { router } from '@/routes/router'

/**
 * Uygulama kok bileseni. Saglayicilarin sirasi:
 *  QueryClientProvider  -> server state cache (TanStack Query)
 *    AuthProvider       -> kullanici/oturum durumu (router guard'lari buna bagli)
 *      RouterProvider   -> data router (createBrowserRouter)
 *
 * AuthProvider, RouterProvider'i sarmalamali ki route guard'lari (ProtectedRoute)
 * useAuth'a erisebilsin.
 */
export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </QueryClientProvider>
  )
}

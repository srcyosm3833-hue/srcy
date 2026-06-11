/**
 * Uygulama rota yollarinin tek dogruluk kaynagi. Hem router tanimi hem de
 * programatik yonlendirmeler (orn. axios interceptor'da login'e atma) bunu kullanir.
 */
export const paths = {
  home: '/',
  login: '/login',
  register: '/register',
  blogs: '/blogs',
  /** Tek blog detayi yolu uretici. */
  blogDetail: (id: string | number = ':id') => `/blogs/${id}`,
  admin: '/admin',
} as const

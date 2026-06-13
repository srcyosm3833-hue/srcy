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
  contact: '/contact',

  // --- Admin alani ---
  admin: '/admin',
  adminBlogs: '/admin/blogs',
  adminBlogCreate: '/admin/blogs/create',
  /** Blog duzenleme yolu uretici. */
  adminBlogEdit: (id: string | number = ':id') => `/admin/blogs/${id}/edit`,
  adminCategories: '/admin/categories',
  adminComments: '/admin/comments',
  adminMessages: '/admin/messages',
  adminSocialMedia: '/admin/social-media',
} as const

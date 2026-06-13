import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      globals: globals.browser,
    },
  },
  {
    // Router/route yapilandirma dosyalari komponent dosyasi degildir; lazy() ile
    // tanimlanan rota bilesenleri react-refresh kuralini gereksiz tetikler.
    files: ['src/routes/router.tsx'],
    rules: {
      'react-refresh/only-export-components': 'off',
    },
  },
  {
    // shadcn/ui komponentleri tasarim geregi komponentle birlikte variant/yardimci
    // (orn. buttonVariants, useFormField) export eder. Bu dosyalar elle duzenlenmez;
    // react-refresh "yalniz komponent export" kuralini burada kapatiyoruz (shadcn standardi).
    files: ['src/components/ui/**/*.{ts,tsx}'],
    rules: {
      'react-refresh/only-export-components': 'off',
    },
  },
])

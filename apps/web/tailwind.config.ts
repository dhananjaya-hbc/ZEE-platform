import type { Config } from 'tailwindcss';

/**
 * Tailwind configuration.
 *
 * Two colour systems coexist deliberately:
 *   - `brand`  — the "Ink wash" monochrome scale, used directly in hand-written
 *     markup (`bg-brand-600`, `text-brand-500`, etc.).
 *   - shadcn's semantic tokens (`background`, `primary`, `border`, ...) — used
 *     by shadcn/ui components (`src/components/ui/`), each mapped to a CSS
 *     variable defined in `globals.css`. `primary` and `ring` are set to the
 *     brand scale there, so shadcn components pick up the same accent colour
 *     as hand-built ones automatically.
 */
const config: Config = {
  content: ['./src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        brand: {
          100: '#cfcfcf',
          500: '#7d7d7d',
          600: '#545454',
          700: '#252525',
        },
        background: 'var(--background)',
        foreground: 'var(--foreground)',
        card: {
          DEFAULT: 'var(--card)',
          foreground: 'var(--card-foreground)',
        },
        popover: {
          DEFAULT: 'var(--popover)',
          foreground: 'var(--popover-foreground)',
        },
        primary: {
          DEFAULT: 'var(--primary)',
          foreground: 'var(--primary-foreground)',
        },
        secondary: {
          DEFAULT: 'var(--secondary)',
          foreground: 'var(--secondary-foreground)',
        },
        muted: {
          DEFAULT: 'var(--muted)',
          foreground: 'var(--muted-foreground)',
        },
        accent: {
          DEFAULT: 'var(--accent)',
          foreground: 'var(--accent-foreground)',
        },
        destructive: {
          DEFAULT: 'var(--destructive)',
          foreground: 'var(--destructive-foreground)',
        },
        border: 'var(--border)',
        input: 'var(--input)',
        ring: 'var(--ring)',
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
      fontFamily: {
        sans: ['var(--font-sans)', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
};

export default config;

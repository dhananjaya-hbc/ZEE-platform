import type { Config } from 'tailwindcss';

/**
 * Tailwind configuration.
 *
 * TODO: Replace the placeholder palette below with ZEE's real brand colours.
 * They are referenced from components as `bg-brand-500` etc., so changing them
 * here updates the whole app.
 */
const config: Config = {
  content: ['./src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // Placeholder brand ramp. Swap for the real values.
        brand: {
          50: '#eef2ff',
          100: '#e0e7ff',
          500: '#6366f1',
          600: '#4f46e5',
          700: '#4338ca',
        },
      },
      fontFamily: {
        sans: ['var(--font-sans)', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [],
};

export default config;

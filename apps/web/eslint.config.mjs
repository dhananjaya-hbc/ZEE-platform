import next from 'eslint-config-next';

/**
 * Flat ESLint config.
 *
 * `eslint-config-next` v16 exports a ready-made flat config ARRAY, so it is spread
 * directly. (In v15 and earlier it was a function you had to call — if you find an
 * older snippet doing `...next()`, that is why it no longer works.)
 *
 * TODO: Extend with project-specific rules as conventions settle. Two worth
 * considering early:
 *   - no-restricted-imports banning direct `fetch` outside src/lib, so every
 *     backend call goes through the api client and picks up auth and error handling.
 *   - jsx-a11y rules; this is a social product and screen-reader support is not
 *     optional.
 */
const config = [
  ...next,
  {
    ignores: ['.next/**', 'node_modules/**', 'next-env.d.ts'],
  },
];

export default config;

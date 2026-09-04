/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,

  // Fail the production build on type or lint errors rather than shipping them.
  // Next's defaults are permissive here; for a project many people will contribute
  // to, a broken build is a much better signal than a broken page.
  typescript: { ignoreBuildErrors: false },
  eslint: { ignoreDuringBuilds: false },

  async headers() {
    return [
      {
        source: '/:path*',
        headers: [
          { key: 'X-Content-Type-Options', value: 'nosniff' },
          { key: 'Referrer-Policy', value: 'strict-origin-when-cross-origin' },
          { key: 'X-Frame-Options', value: 'DENY' },
        ],
      },
    ];
  },
};

// TODO (PWA): wire up service worker generation and offline caching.
//
// Phase 1 ships the manifest and the install prompt only — the app is installable
// but not offline-capable. Adding a service worker is deliberately a separate task
// because caching an authenticated feed wrongly is worse than not caching it:
// a shared device could serve one student's cached posts to the next.
//
// Suggested approach: @serwist/next, with a network-first strategy for /api and
// cache-first for static assets. Never cache authenticated API responses.

export default nextConfig;

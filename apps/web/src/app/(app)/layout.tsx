import Link from 'next/link';

const NAV = [
  { href: '/feed', label: 'Feed' },
  { href: '/chatbot', label: 'Assistant' },
  { href: '/groups', label: 'Groups' },
  { href: '/messages', label: 'Messages' },
  { href: '/profile', label: 'Profile' },
] as const;

/**
 * Shell for the signed-in area.
 *
 * The (app) route group shares this layout without adding a path segment, so the
 * URL stays /feed rather than /app/feed.
 *
 * TODO: Guard this layout — redirect to / when there is no session, so no signed-in
 * page renders for an anonymous visitor. Middleware or a server-side session check
 * both work; do NOT rely on the client hiding things.
 *
 * TODO: Replace the top nav with a bottom tab bar on mobile. ZEE is a PWA and this
 * is the primary navigation on a phone.
 */
export default function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="mx-auto flex min-h-dvh max-w-2xl flex-col">
      <header className="sticky top-0 z-10 border-b border-gray-200 bg-white/80 backdrop-blur dark:border-gray-800 dark:bg-gray-950/80">
        <nav className="flex items-center gap-4 px-4 py-3 text-sm">
          <Link href="/feed" className="font-bold">
            ZEE
          </Link>
          <div className="flex flex-1 justify-end gap-4">
            {NAV.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className="text-gray-600 hover:text-brand-600 dark:text-gray-400"
              >
                {item.label}
              </Link>
            ))}
          </div>
        </nav>
      </header>

      <main className="flex-1 px-4 py-6">{children}</main>
    </div>
  );
}

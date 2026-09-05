import { AppHeader } from '@/components/AppHeader';
import { BottomNav } from '@/components/BottomNav';

/**
 * Shell for the signed-in area — shared header and bottom nav so every page
 * in this route group (feed, chatbot, groups, messages, profile) looks
 * identical, rather than each page building its own chrome.
 *
 * The (app) route group shares this layout without adding a path segment, so
 * the URL stays /feed rather than /app/feed.
 *
 * TODO: Guard this layout — redirect to / when there is no session, so no
 * signed-in page renders for an anonymous visitor. Middleware or a
 * server-side session check both work; do NOT rely on the client hiding
 * things.
 */
export default function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-dvh bg-gray-50 pb-24 dark:bg-gray-950">
      <AppHeader />
      <main className="mx-auto max-w-6xl px-6 py-6">{children}</main>
      <BottomNav />
    </div>
  );
}

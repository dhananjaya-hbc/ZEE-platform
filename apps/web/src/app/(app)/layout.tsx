import { redirect } from 'next/navigation';
import { AppHeader } from '@/components/AppHeader';
import { BottomNav } from '@/components/BottomNav';
import { getServerSession } from '@/lib/auth-server';

/**
 * Shell for the signed-in area — shared header and bottom nav so every page
 * in this route group (feed, chatbot, groups, messages, profile) looks
 * identical, rather than each page building its own chrome.
 *
 * The (app) route group shares this layout without adding a path segment, so
 * the URL stays /feed rather than /app/feed.
 *
 * Guarded server-side: anonymous visitors are immediately redirected to /
 * before any page content or layout chrome is rendered.
 */
export default async function AppLayout({ children }: { children: React.ReactNode }) {
  const session = await getServerSession();

  if (!session) {
    redirect('/');
  }

  return (
    <div className="min-h-dvh bg-gray-50 pb-24 dark:bg-gray-950">
      <AppHeader />
      <main className="mx-auto max-w-6xl px-6 py-6">{children}</main>
      <BottomNav />
    </div>
  );
}

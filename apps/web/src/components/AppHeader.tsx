import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { AvatarPlaceholder } from '@/components/AvatarPlaceholder';

/**
 * Shared top bar for the signed-in area: logo, search, notifications, "New
 * post" and the profile avatar. Rendered once in (app)/layout.tsx so every
 * page (feed, chatbot, groups, messages, profile) gets an identical header
 * instead of each page building its own.
 *
 * The search input, notification count and "New post" button are static
 * visual mocks - same Phase 1 status as the rest of the scaffolding, not
 * wired to the API.
 */
export function AppHeader() {
  return (
    <header className="flex items-center gap-4 border-b border-gray-200 bg-white px-6 py-4 dark:border-gray-800 dark:bg-gray-900">
      <Link href="/feed" className="text-xl font-bold tracking-wide">
        ZEE
      </Link>

      <Input
        type="text"
        placeholder="Search students, groups, competitions"
        className="max-w-xl bg-gray-100 dark:bg-gray-800"
      />

      <div className="ml-auto flex items-center gap-4 text-sm">
        <span className="tracking-wide text-gray-500">
          NOTIFICATIONS <span className="text-gray-400">· 3</span>
        </span>
        <Button>
          New post
        </Button>
        <AvatarPlaceholder size="md" />
      </div>
    </header>
  );
}

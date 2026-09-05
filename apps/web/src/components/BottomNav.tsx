'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

/**
 * Bottom nav destinations. `href: null` means no page exists there yet -
 * rendered as visually present but genuinely inert, rather than a link that
 * goes nowhere or silently to the wrong place.
 */
const NAV_ITEMS: ReadonlyArray<{ label: string; href: string | null }> = [
  { label: 'Feed', href: '/feed' },
  { label: 'Explore', href: '/explore' },
  { label: 'Ask Zee', href: '/chatbot' },
  { label: 'Messages', href: '/messages' },
  { label: 'Profile', href: '/profile' },
];

/**
 * Floating bottom nav shared by every page in the (app) route group.
 * Client Component because usePathname() (to highlight the active page) is a
 * client-only hook.
 */
export function BottomNav() {
  const pathname = usePathname();

  return (
    <nav className="fixed inset-x-0 bottom-4 mx-auto flex w-fit gap-8 rounded-lg border border-gray-300 bg-white px-8 py-3 shadow-lg dark:border-gray-700 dark:bg-gray-900">
      {NAV_ITEMS.map(({ label, href }) => {
        const isActive = href !== null && pathname === href;

        if (!href) {
          return (
            <span
              key={label}
              aria-disabled="true"
              className="flex cursor-not-allowed flex-col items-center gap-1 text-xs font-semibold uppercase tracking-wide text-gray-300 dark:text-gray-700"
            >
              <span className="h-4 w-4 rounded-sm border border-gray-300 dark:border-gray-700" />
              {label}
            </span>
          );
        }

        return (
          <Link
            key={label}
            href={href}
            className={`flex flex-col items-center gap-1 text-xs font-semibold uppercase tracking-wide ${
              isActive
                ? 'text-brand-700 dark:text-brand-500'
                : 'text-gray-600 hover:text-brand-600 dark:text-gray-400'
            }`}
          >
            <span
              className={`h-4 w-4 rounded-sm border ${
                isActive
                  ? 'border-brand-700 bg-brand-100 dark:border-brand-500'
                  : 'border-gray-400'
              }`}
            />
            {label}
          </Link>
        );
      })}
    </nav>
  );
}

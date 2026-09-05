import { cn } from '@/lib/utils';

/**
 * A grey placeholder bar standing in for real text/content that isn't wired up
 * to the API yet. Width is controlled via `className` (e.g. `w-40`), matching
 * how Tailwind width utilities are used everywhere else in the app.
 */
export function SkeletonLine({ className }: { className?: string }) {
  return <span className={cn('block h-3 rounded bg-gray-200 dark:bg-gray-800', className)} />;
}

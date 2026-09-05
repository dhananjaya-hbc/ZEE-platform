import { cn } from '@/lib/utils';

const SIZES = {
  sm: 'h-8 w-8',
  md: 'h-9 w-9',
  lg: 'h-10 w-10',
} as const;

/** A plain circle standing in for a student's real profile picture. */
export function AvatarPlaceholder({
  size = 'md',
  className,
}: {
  size?: keyof typeof SIZES;
  className?: string;
}) {
  return (
    <span
      className={cn('shrink-0 rounded-full bg-gray-300 dark:bg-gray-700', SIZES[size], className)}
    />
  );
}

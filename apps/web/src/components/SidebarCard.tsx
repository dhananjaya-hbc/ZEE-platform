import type { ReactNode } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

/**
 * One section in the feed's right sidebar (Students like you, Deadlines &
 * events, Your groups, Ask ZEE). Extracted because all four were an identical
 * shell wrapping different content.
 */
export function SidebarCard({
  title,
  action,
  className,
  children,
}: {
  title: string;
  /** Optional element shown to the right of the title, e.g. a Badge. */
  action?: ReactNode;
  className?: string;
  children: ReactNode;
}) {
  return (
    <Card className={className}>
      <CardHeader className="flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-semibold uppercase tracking-wide text-primary">
          {title}
        </CardTitle>
        {action}
      </CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
  );
}

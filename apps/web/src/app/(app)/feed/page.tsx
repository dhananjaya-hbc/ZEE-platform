import type { Metadata } from 'next';
import { AvatarPlaceholder } from '@/components/AvatarPlaceholder';
import { SidebarCard } from '@/components/SidebarCard';
import { SkeletonLine } from '@/components/SkeletonLine';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';

export const metadata: Metadata = { title: 'Feed' };

/**
 * Feed — static visual mock only.
 *
 * Matches the shared wireframe's structure using the "Ink wash" palette and
 * the shadcn-derived components in src/components/ui/, with placeholder
 * shapes standing in for real content everywhere the design uses a grey
 * skeleton bar. Nothing here is wired to the API: no feed query, no post
 * composer submission, no likes/comments/shares, no notifications, no
 * search, no groups/events data.
 *
 * The "Students like you" panel is a static mock, not the real AI matching
 * feature (IAiServiceClient.GetRecommendationsAsync) - that stays Phase 2
 * per docs/AI_DESIGN.md. Every interactive-looking element here (tabs,
 * composer, buttons) is visual only.
 *
 * The header and bottom nav are NOT here - they live in (app)/layout.tsx and
 * are shared by every page in this route group, so the structure stays
 * identical across feed, chatbot, groups, messages and profile.
 */
export default function FeedPage() {
  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_320px]">
      {/* Main column */}
      <div className="flex flex-col gap-6">
        {/* Tabs + sort */}
        <div className="flex items-center justify-between">
          <div className="flex gap-2 rounded-md border border-gray-300 bg-white p-1 text-sm dark:border-gray-700 dark:bg-gray-900">
            <span className="rounded bg-brand-600 px-3 py-1.5 font-medium text-white">
              For you
            </span>
            <span className="px-3 py-1.5 text-gray-600 dark:text-gray-400">Following</span>
            <span className="px-3 py-1.5 text-gray-600 dark:text-gray-400">My courses</span>
          </div>
          <span className="text-xs font-semibold uppercase tracking-wide text-gray-500">
            Newest first
          </span>
        </div>

        {/* Composer */}
        <Card className="flex flex-col gap-3 p-4">
          <div className="flex items-center gap-3">
            <AvatarPlaceholder size="lg" />
            <Input type="text" placeholder="Share something with your courses…" />
          </div>
          <div className="flex items-center justify-between border-t border-gray-100 pt-3 text-sm dark:border-gray-800">
            <div className="flex gap-4 text-primary">
              <span>Photo</span>
              <span>Poll</span>
              <span>Event</span>
              <span>Looking for team</span>
            </div>
            <Button>Post</Button>
          </div>
        </Card>

        {/* Post: with image attachment */}
        <Card className="flex flex-col gap-4 p-5">
          <div className="flex items-start justify-between">
            <div className="flex items-center gap-3">
              <AvatarPlaceholder size="lg" />
              <div className="flex flex-col gap-1">
                <SkeletonLine className="w-40 bg-gray-300 dark:bg-gray-700" />
                <SkeletonLine className="w-56" />
              </div>
            </div>
            <Badge className="bg-primary/10 text-primary">
              CS 214 group
            </Badge>
          </div>

          <div className="flex flex-col gap-2">
            <SkeletonLine className="w-full" />
            <SkeletonLine className="w-4/5" />
            <SkeletonLine className="w-3/5" />
          </div>

          <div className="flex h-64 items-center justify-center rounded-md border-2 border-dashed border-primary/40 bg-primary/10 text-sm font-semibold uppercase tracking-wide text-primary">
            Image attachment
          </div>

          <div className="flex gap-6 border-t border-gray-100 pt-3 text-sm font-medium uppercase tracking-wide text-gray-500 dark:border-gray-800">
            <span>Like 41</span>
            <span>Comment 12</span>
            <span>Share</span>
            <span>Save</span>
          </div>

          <div className="flex items-center gap-3 border-t border-gray-100 pt-3 dark:border-gray-800">
            <AvatarPlaceholder size="sm" />
            <SkeletonLine className="w-64" />
          </div>
          <span className="text-xs font-semibold uppercase tracking-wide text-gray-400">
            View all 12 comments
          </span>
        </Card>

        {/* Post: competition / team recruiting */}
        <Card className="flex flex-col gap-4 p-5">
          <div className="flex items-start justify-between">
            <div className="flex items-center gap-3">
              <AvatarPlaceholder size="lg" />
              <div className="flex flex-col gap-1">
                <SkeletonLine className="w-40 bg-gray-300 dark:bg-gray-700" />
                <SkeletonLine className="w-32" />
              </div>
            </div>
            <Badge variant="outline" className="border-primary text-primary">
              competition
            </Badge>
          </div>

          <div className="flex flex-col gap-2">
            <SkeletonLine className="w-full" />
            <SkeletonLine className="w-3/4" />
          </div>

          <div className="flex gap-2 text-sm">
            <span className="rounded-md bg-gray-100 px-3 py-1 dark:bg-gray-800">
              need 1 backend
            </span>
            <span className="rounded-md bg-gray-100 px-3 py-1 dark:bg-gray-800">
              need 1 designer
            </span>
          </div>

          <div className="flex gap-3">
            <Button>Join team</Button>
            <Button variant="outline">View competition</Button>
          </div>

          <div className="flex gap-6 border-t border-gray-100 pt-3 text-sm font-medium uppercase tracking-wide text-gray-500 dark:border-gray-800">
            <span>Like 18</span>
            <span>Comment 5</span>
            <span>Share</span>
          </div>
        </Card>
      </div>

      {/* Sidebar */}
      <aside className="flex flex-col gap-6">
        <SidebarCard title="Students like you">
          <p className="text-xs text-gray-400">
            Static preview only — real matching is a Phase 2 feature (see
            docs/AI_DESIGN.md).
          </p>

          <ul className="mt-4 flex flex-col gap-4">
            {[92, 88, 81].map((score) => (
              <li key={score} className="flex items-center gap-3">
                <AvatarPlaceholder size="md" />
                <SkeletonLine className="flex-1" />
                <span className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                  {score}%
                </span>
              </li>
            ))}
          </ul>

          <Button className="mt-4 w-full">
            See all matches
          </Button>
        </SidebarCard>

        <SidebarCard title="Deadlines & events">
          <ul className="flex flex-col gap-3 text-sm">
            {[
              { width: 'w-40', due: '2D' },
              { width: 'w-32', due: '5D' },
              { width: 'w-36', due: '1W' },
            ].map(({ width, due }) => (
              <li key={due} className="flex items-center justify-between">
                <SkeletonLine className={width} />
                <span className="font-semibold text-gray-500">{due}</span>
              </li>
            ))}
          </ul>
        </SidebarCard>

        <SidebarCard
          title="Your groups"
          action={
            <Badge className="bg-primary/10 text-primary">
              4 new
            </Badge>
          }
        >
          <ul className="flex flex-col gap-3">
            <li>
              <SkeletonLine className="w-full" />
            </li>
            <li>
              <SkeletonLine className="w-4/5" />
            </li>
            <li>
              <SkeletonLine className="w-3/5" />
            </li>
          </ul>
        </SidebarCard>

        <SidebarCard title="Ask ZEE">
          <p className="text-sm text-gray-600 dark:text-gray-400">
            Questions about deadlines, dining or clubs — answered from campus
            sources.
          </p>
          <Button variant="link" className="mt-3 h-auto p-0">
            Open chat
          </Button>
        </SidebarCard>
      </aside>
    </div>
  );
}

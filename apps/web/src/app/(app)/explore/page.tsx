import { PagePlaceholder } from '@/components/PagePlaceholder';

export const metadata = { title: 'Explore' };

export default function ExplorePage() {
  return (
    <PagePlaceholder
      title="Explore"
      description="Discover groups, competitions and students across every campus."
      buildNotes={[
        'No API endpoint exists for this yet — needs a browse/search surface across Groups, Competitions and Events.',
        'Decide what "Explore" actually means before building: cross-campus discovery, trending posts, or a directory? These need different queries.',
        'Search bar in the shared header is currently a static mock — wiring it up likely belongs here.',
      ]}
    />
  );
}

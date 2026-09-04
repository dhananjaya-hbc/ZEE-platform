import { PagePlaceholder } from '@/components/PagePlaceholder';

export const metadata = { title: 'Feed' };

export default function FeedPage() {
  return (
    <PagePlaceholder
      title="Feed"
      description="Posts from your campus and the groups you have joined."
      buildNotes={[
        'Call api.getFeed() and render a list of PostDto.',
        'Phase 1 is reverse-chronological only — no ranking, no personalisation.',
        'Paginate with the opaque nextCursor; pass it back verbatim, never construct one.',
        'Add a composer that calls api.createPost().',
        'Handle ApiError.isNotImplemented while the backend handler is still a stub.',
      ]}
    />
  );
}

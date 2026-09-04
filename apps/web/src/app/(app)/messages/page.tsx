import { PagePlaceholder } from '@/components/PagePlaceholder';

export const metadata = { title: 'Messages' };

export default function MessagesPage() {
  return (
    <PagePlaceholder
      title="Messages"
      description="Direct messages with other students."
      buildNotes={[
        'Needs a messages endpoint on the API first — none exists yet.',
        'Conversation list is a group-by over conversationKey; thread view filters on it.',
        'Phase 1 can poll. Real-time delivery is a separate task.',
      ]}
    />
  );
}

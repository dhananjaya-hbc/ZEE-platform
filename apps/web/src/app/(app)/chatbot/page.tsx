import { PagePlaceholder } from '@/components/PagePlaceholder';

export const metadata = { title: 'Assistant' };

export default function ChatbotPage() {
  return (
    <PagePlaceholder
      title="Assistant"
      description="Ask questions about your campus."
      buildNotes={[
        'Call api.askChatbot() and render the answer with its sources.',
        'Phase 1 returns MOCK answers — label them clearly so nobody mistakes it for real.',
        'Always render the sources list; citations are part of the contract, not a Phase 2 extra.',
        'Never render the answer as raw HTML.',
      ]}
    />
  );
}

import { PagePlaceholder } from '@/components/PagePlaceholder';

export const metadata = { title: 'Profile' };

export default function ProfilePage() {
  return (
    <PagePlaceholder
      title="Profile"
      description="Your courses, interests and achievements."
      buildNotes={[
        'Needs profile endpoints on the API first — none exist yet.',
        'Editable: name, major, year, courses, interests. NOT editable: email and university.',
        'Achievements are self-reported; render proofUrl as a link, and never auto-trust it.',
      ]}
    />
  );
}

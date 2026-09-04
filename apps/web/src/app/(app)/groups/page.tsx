import { PagePlaceholder } from '@/components/PagePlaceholder';

export const metadata = { title: 'Groups' };

export default function GroupsPage() {
  return (
    <PagePlaceholder
      title="Groups"
      description="Courses, clubs, dorms and cross-campus interests."
      buildNotes={[
        'Needs a groups endpoint on the API first — none exists yet.',
        'Course, Club and Dorm groups belong to one campus; GlobalInterest spans all of them.',
        'Show joined groups separately from discoverable ones.',
      ]}
    />
  );
}

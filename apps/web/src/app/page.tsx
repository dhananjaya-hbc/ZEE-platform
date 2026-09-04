import Link from 'next/link';

/**
 * Landing page.
 *
 * TODO: Build the signed-out experience — what ZEE is, and the sign-in form that
 * takes an institutional email and starts the OTP flow.
 *
 * Once auth exists, this should redirect signed-in students straight to /feed.
 */
export default function HomePage() {
  return (
    <main className="mx-auto flex min-h-dvh max-w-2xl flex-col justify-center gap-6 px-6">
      <h1 className="text-4xl font-bold tracking-tight">ZEE</h1>
      <p className="text-lg text-gray-600 dark:text-gray-400">
        A global campus social network for verified students.
      </p>

      <p className="rounded-lg border border-dashed border-gray-300 p-4 text-sm text-gray-500 dark:border-gray-700">
        Phase 1 scaffolding. Sign-in is not built yet — see{' '}
        <code className="font-mono">src/lib/auth.ts</code>.
      </p>

      <nav className="flex flex-wrap gap-3 text-sm">
        <Link className="text-brand-600 underline" href="/feed">
          Feed
        </Link>
        <Link className="text-brand-600 underline" href="/chatbot">
          Assistant
        </Link>
        <Link className="text-brand-600 underline" href="/groups">
          Groups
        </Link>
        <Link className="text-brand-600 underline" href="/messages">
          Messages
        </Link>
        <Link className="text-brand-600 underline" href="/profile">
          Profile
        </Link>
      </nav>
    </main>
  );
}

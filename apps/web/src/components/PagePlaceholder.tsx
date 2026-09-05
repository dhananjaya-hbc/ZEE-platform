/**
 * Placeholder shown on routes that are scaffolded but not yet built.
 *
 * Exists so every route in the app renders something honest during Phase 1 rather
 * than 404ing or showing a blank screen — and so it is obvious at a glance which
 * screens are still open work.
 *
 * Delete this component once every page is implemented.
 */
export function PagePlaceholder({
  title,
  description,
  buildNotes,
}: {
  title: string;
  description: string;
  buildNotes: readonly string[];
}) {
  return (
    <section className="flex flex-col gap-4">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
        <p className="mt-1 text-gray-600 dark:text-gray-400">{description}</p>
      </div>

      <div className="rounded-lg border border-dashed border-gray-300 p-4 dark:border-gray-700">
        <p className="text-sm font-medium text-gray-700 dark:text-gray-300">
          Not built yet. To implement this screen:
        </p>
        <ul className="mt-2 list-inside list-disc space-y-1 text-sm text-gray-600 dark:text-gray-400">
          {buildNotes.map((note) => (
            <li key={note}>{note}</li>
          ))}
        </ul>
      </div>
    </section>
  );
}

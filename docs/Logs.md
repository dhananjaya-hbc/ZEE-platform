# Development Log

Running record of significant decisions and milestones. Newest first.

Add an entry when you make a decision that a future contributor would otherwise have
to reverse-engineer from the code. Routine feature work belongs in the git history,
not here.

---

## 2026-09-05 — Institutional sign-in implemented end to end; shadcn/ui added; feed page mocked

The OTP auth flow described in [Architecture.md](Architecture.md#authentication) is no
longer a plan — it is implemented, migrated to a real Neon database, and verified by
actually signing in through the browser.

**What changed on the API:**

- Domain: `Guard`, `University` (including the exact-match domain check and its
  lookalike-domain regression tests), `User`, `EmailVerificationCode` (constant-time
  verify, attempt cap) — all implemented, no longer stubs.
- Infrastructure: `OtpService` (SHA-256, not a slow password hash — the code is dead
  in minutes regardless), `TokenService` (HMAC-SHA256 JWTs), `DevConsoleEmailSender`
  (Development-only, logs the code instead of sending mail), plus the EF configurations
  and repository methods the flow touches (`UniversityConfiguration`,
  `UserConfiguration`, `EmailVerificationCodeConfiguration`,
  `FindByEmailDomainAsync`, `GetByEmailAsync`, `GetActiveByEmailAsync`,
  `CountIssuedSinceAsync`).
- Application: `RequestOtpCommand` and `VerifyOtpCommand`, each with a rate limit
  (`TooManyRequestsException` → 429) and a single fixed message for every OTP failure
  (`InvalidOtpException` → 400) so a wrong guess, an expired code, and a code that was
  never requested are indistinguishable to the caller.
- Api: `AuthController` — the one controller marked `[AllowAnonymous]` at the class
  level, since these two endpoints are the only ones reachable before a student has a
  token at all.
- `InitialCreate` migration generated and applied to a real Neon database. Verified live:
  inserted a test `University` row (`uom.lk`, University of Moratuwa) and signed in
  through the browser, code delivered via the dev logs.

**What changed on the web app:**

- `lib/auth.ts` / `lib/api-client.ts` implemented. Token storage is an **httpOnly
  cookie** set by `app/api/session/route.ts` — chosen over localStorage specifically so
  a stored-XSS bug elsewhere on the page cannot read the token. Consequence worth
  remembering: client JS can never read that cookie again, so any *future* authenticated
  call (feed, posts, chatbot) will need a Next.js server-side proxy route that reads the
  cookie itself and attaches the Authorization header - not built yet, since nothing
  today needs one.
- The two-step sign-in screen (`app/page.tsx`) — email, then a 6-digit code, one step
  visible at a time.
- shadcn/ui initialised, then made to actually work: the CLI's generated code assumes
  Tailwind v4 (`--spacing()` calc calls, `in-data-[...]` variants, `color-mix()`), which
  this project's deliberately-v3 Tailwind setup cannot parse. `Button`, `Input`, `Card`,
  `Badge` in `src/components/ui/` are hand-authored to the same API shape in classes
  that actually compile — see [TechStack.md](TechStack.md) for the full note, including
  why `npx shadcn add` should not be trusted verbatim in this repo. The CLI also tried
  to switch `darkMode` to `'class'`, which would have silently broken every existing
  `dark:` utility in the app (nothing anywhere applies a `.dark` class) - reverted to
  the default media-query behaviour.
- Feed page rebuilt as a static visual mock matching a shared wireframe, using the
  "Ink wash" monochrome palette (`apps/web/tailwind.config.ts`). Reusable pieces
  extracted to `src/components/`: `AppHeader`, `BottomNav` (shared by every page in the
  `(app)` route group, not just feed), `SidebarCard`, `SkeletonLine`,
  `AvatarPlaceholder`. The "Students like you" panel is explicitly labelled a static
  preview - it is not the real AI matching feature, which stays Phase 2.

**Deviations worth flagging:**

- `getSession()` in `lib/auth.ts` is still a TODO. The JWT's id claim is
  `ClaimTypes.NameIdentifier`, which .NET serialises as a long URI rather than a short
  `"sub"` claim; decoding that here would mean hard-coding the URI in TypeScript. Left
  as a flagged gap pending a decision on whether to switch the API to a short claim
  type instead.
- An `/explore` placeholder page was added because the bottom nav referenced it before
  any such page existed - clicking it previously did nothing.

## 2026-09-04 — Switched the database to Neon

Replaced the local Postgres container with [Neon](https://neon.com) (serverless
PostgreSQL) in every environment, including development. Full setup, reasoning and
troubleshooting: [Database.md](Database.md).

**What changed:**

- `infra/docker-compose.yml` — the `postgres` service and its volume are gone.
  `infra/postgres/init.sql` is deleted; extensions moved onto the EF Core model
  (`AppDbContext.OnModelCreating`) so they are applied by migration instead of a
  Compose-only init hook that a managed database has no equivalent of.
- `Zee.Infrastructure/DependencyInjection.cs` — `EnableRetryOnFailure` widened to
  5 attempts / 10s ceiling (was 3 / 5s) to absorb a Neon cold-start resume, plus an
  explicit 30s `CommandTimeout` now that every query crosses a real network.
- **Health checks split.** `/health` (liveness, no DB) and `/health/ready`
  (readiness, hits the DB). A single DB-touching health check polled every 10s would
  keep Neon's compute permanently awake, defeating scale-to-zero. Point monitors at
  `/health`.
- All `.env.example` files and `appsettings.Development.json` updated; the dev
  Postgres connection string is now intentionally blank rather than a working local
  default.

**Why not keep the container for local dev and use Neon only in production?** That
hybrid was the initial recommendation — it needs no account to run the stack locally.
It was overridden in favour of one database engine everywhere: no "works with the
container, breaks on Neon" class of bug, and Neon's per-branch isolation solves the
problem the container was mainly there for (contributors not clobbering each other's
data). The real cost is a signup step before  works at all — worth
watching if it turns out to block first-time contributors.

---

## 2026-09-04 — Phase 1 scaffolding

Initial repository structure. The platform is scaffolded end to end: structure,
contracts, tests and infrastructure are in place, with most method bodies left as
`TODO` stubs carrying acceptance criteria.

**What exists and runs:**

- Monorepo: `apps/{api,web,ai-service}`, `infra/`, `docs/`, `.github/workflows/`
- .NET solution, 7 projects, Clean Architecture dependency rule enforced by project
  references. Builds clean under `-warnaserror`.
- 12 domain entities with signatures, XML docs and invariant specifications
- Application layer: 4 vertical slices (CreatePost, GetFeed, CreateEvent,
  CreateCompetition) plus the chatbot query, wired through MediatR with an automatic
  validation pipeline
- Infrastructure: `AppDbContext`, 12 EF configuration skeletons, 6 repositories,
  `AiServiceClient`, full DI wiring
- Api: 5 thin controllers, global exception middleware, JWT auth, CORS, health checks
- Next.js app: 6 routes building and prerendering, typed API client, PWA manifest
- FastAPI service: 3 endpoints + health, router-level internal-key enforcement
- Docker Compose: Postgres (pgvector), Redis, all three services
- CI: per-app workflows with path filters, plus Discord notifications

**Verified:** `dotnet build -warnaserror` clean; `dotnet test` green (56 tests —
53 skipped stubs, 2 passing smoke tests, 0 failures); `npm run lint`, `tsc --noEmit`
and `npm run build` all clean; `npm audit` reports 0 vulnerabilities;
`ruff check` and `ruff format --check` clean; `docker compose config` valid.

### Decisions

| Decision | Reasoning |
| --- | --- |
| **OTP over magic link** | Works in a PWA without deep-link handling, easier to test, and not vulnerable to link-scanning email security products consuming the token before the student does. |
| **Stubs over full implementation** | The repository is for contributors. A finished codebase leaves nothing to claim; a specified skeleton leaves a lot. Every stub carries acceptance criteria and a matching skipped test. |
| **Tests skipped, not failing** | CI must be green on a fresh clone. A red baseline makes it impossible for a contributor to distinguish their own breakage from the scaffolding's. |
| **`NotImplementedException` → HTTP 501** | An unbuilt endpoint reports itself honestly instead of returning a confusing 500. The web client surfaces it as `ApiError.isNotImplemented`. |
| **EF configurations empty, not throwing** | `OnModelCreating` runs them all at startup; throwing would stop the API booting and block everyone. Empty means conventions apply and the app still runs. |
| **MediatR pinned to 12.4.1** | Last Apache-2.0 release. v13+ requires a commercial licence, which does not suit an open-source project. Shouldly replaces FluentAssertions for the same reason. |
| **UUIDv7 primary keys** | Time-ordered, so inserts append to the index edge rather than fragmenting it as random v4 GUIDs do. |
| **Keyset pagination** | `OFFSET` duplicates and drops rows on a feed receiving new posts, and degrades linearly with depth. |
| **Next 16, not 15.1** | Next 15.x has open advisories via `postcss`. `postcss` and `vitest` are also pinned above their default ranges so `npm audit` is clean. |
| **Python 3.12 in the Dockerfile** | `pydantic-core` ships prebuilt wheels for it; on 3.14 the install compiles Rust from source. |
| **pgvector enabled now** | Turning it on later needs elevated rights at an awkward moment. Unused extensions cost nothing. |
| **CA1716 suppressed** | Lets the `Event` entity keep the name the data model documents. The rule targets cross-language consumers; ZEE is C#-only. |

### Deviations from the original brief

- **Two extra repository interfaces.** `IUniversityRepository` and
  `IEmailVerificationCodeRepository` were added — the OTP flow cannot resolve an email
  domain to a campus or store a pending code without them.
- **`CONTRIBUTING.md` is at the repository root**, not in `docs/`. The distribution
  guideline requires that exact filename at the root; `docs/` links to it.
- **AI service mocks are stubbed, not written.** Originally scoped as working mocks;
  reduced to skeletons for consistency with the rest of the scaffolding.

### Open questions

- Token storage on the web client — httpOnly cookie (recommended) or localStorage?
  The tradeoff is written up in `apps/web/src/lib/auth.ts`.
- Email provider for OTP delivery. Development currently logs the code.
- Moderation and reporting are not modelled at all. Needed before any real pilot.

### Next up

See the known-gaps table in [Architecture.md](Architecture.md#known-gaps). The highest
value work, roughly in order: implement `Guard` and the domain entities (unblocks
everything), then the repositories, then rate limiting on `request-otp`.

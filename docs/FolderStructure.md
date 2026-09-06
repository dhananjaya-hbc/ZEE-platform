# File Structure & Code Organization Rules

A practical, direct guide for contributors on organizing, adding, and modifying code
across **`apps/api`**, **`apps/web`**, and **`apps/ai-service`**. For *why* it is shaped
this way, see [Architecture.md](Architecture.md).

**A note before you start:** ZEE is not a NestJS + per-route-colocated-Next.js stack —
if you've used a folder-structure guide from a project like that before, don't carry its
specific patterns over here. The backend is **Clean Architecture** (four strictly
layered projects, not feature modules), and the frontend keeps shared UI centralized
rather than colocated per route. Both are explained below.

---

## 1. Golden Rules

1. **A feature is a folder, at the layer that owns it.** In `apps/api`, everything for
   one use case lives together: `Posts/Commands/CreatePost/` holds the command, its
   validator, and its handler — never a `Handlers/` junk drawer of forty unrelated
   classes. In `apps/ai-service`, a feature spans three peer folders instead
   (`routers/` + `schemas/` + `services/`) — see [§4](#4-ai-service-guidelines-appsai-service).
2. **Orchestrators stay thin.** Controllers (`apps/api`) and `page.tsx` files
   (`apps/web`) bind, dispatch, and shape a response or render — never business logic,
   raw queries, or an `if` of real consequence. If you're writing one of those inside a
   controller or a page component, it belongs one layer in.
3. **Naming follows the language, not one house style.** C#: PascalCase, one public
   type per file, named after it. TypeScript: PascalCase for components
   (`AppHeader.tsx`), camelCase for everything else (`api-client.ts` is the one
   deliberate kebab-case exception — see below). Python: snake_case, enforced by `ruff`.
4. **Interfaces live where they're used, not where they're implemented.** `IPostRepository`
   is declared in `Zee.Domain` (the consumer) and implemented in `Zee.Infrastructure`
   (the provider) — never the reverse. This is what lets Infrastructure depend on
   Application/Domain instead of the other way around.

---

## 2. Backend Guidelines (`apps/api`)

The .NET API is **four strictly layered projects**, dependencies pointing inward only —
enforced by `.csproj` references, not convention. Full explanation, including *why*:
[CONTRIBUTING.md § The four layers](../CONTRIBUTING.md#the-four-layers).

```
apps/api/
├── Zee.slnx                        Solution (the new XML format; .NET 9.0.200+)
├── Directory.Build.props           Settings shared by all 7 projects (net10.0, nullable…)
├── Directory.Packages.props        EVERY NuGet version, pinned once
├── Dockerfile                      Multi-stage; runs as non-root
├── .env.example
│
├── src/
│   ├── Zee.Domain/                 ── no dependencies whatsoever ──
│   │   ├── Common/
│   │   │   ├── Entity.cs           Base class, UUIDv7 identity, equality
│   │   │   ├── DomainException.cs  Thrown when an invariant is violated
│   │   │   ├── Guard.cs            Argument checks used by entity factories
│   │   │   └── FeedCursor.cs       Keyset pagination position
│   │   ├── Entities/               12 entities, each with private setters and a
│   │   │                           static Create factory — no way to build an
│   │   │                           invalid one
│   │   ├── Enums/                  Persisted by integer value; never renumber
│   │   ├── Repositories/           INTERFACES only. Implementations are in
│   │   │                           Infrastructure.
│   │   └── AssemblyInfo.cs         InternalsVisibleTo for the test project
│   │
│   ├── Zee.Application/            ── depends on Domain ──
│   │   ├── Common/
│   │   │   ├── Ai/                 Wire contract with the AI service
│   │   │   ├── Behaviours/         MediatR pipeline (validation)
│   │   │   ├── Exceptions/         Validation / NotFound / ForbiddenAccess
│   │   │   ├── Interfaces/         IAiServiceClient, ICurrentUser
│   │   │   └── Models/             CursorPage, CursorCodec
│   │   ├── Posts/                  ─┐
│   │   ├── Events/                  ├─ one folder per feature, each with
│   │   ├── Competitions/            │  Commands/ Queries/ Dtos/
│   │   ├── Chatbot/                ─┘
│   │   └── DependencyInjection.cs  AddApplication() — scans this assembly
│   │
│   ├── Zee.Infrastructure/         ── depends on Application ──
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs     Also implements IUnitOfWork
│   │   │   ├── Configurations/     One IEntityTypeConfiguration per entity
│   │   │   └── Repositories/       EF Core implementations
│   │   ├── Ai/                     AiServiceClient + its options
│   │   └── DependencyInjection.cs  AddInfrastructure() — the ONLY place
│   │                               interfaces get bound to implementations
│   │
│   └── Zee.Api/                    ── depends on Application + Infrastructure ──
│       ├── Controllers/            Thin. Bind, dispatch, shape the response.
│       ├── Middleware/             Global exception → RFC 9457 problem details
│       ├── Services/               CurrentUser (reads JWT claims)
│       ├── Program.cs              Wires layers, auth, CORS, OpenAPI, health
│       └── appsettings*.json       No real secrets — these are committed
│
└── tests/
    ├── .editorconfig               Relaxes CA1707 so snake_case test names work
    ├── Zee.Domain.UnitTests/       Invariants. No database, no mocks.
    ├── Zee.Application.UnitTests/  Handlers + validators, repositories substituted
    └── Zee.Api.IntegrationTests/   Real pipeline, in-memory database
```

### Contributor workflow: adding a backend feature

1. **Ask "does Domain need a new rule?"** first — if a check must hold no matter how the
   object is reached, it's an entity invariant, not handler logic. Often the answer is
   no and Domain is untouched.
2. Create a feature folder under `Zee.Application/<Feature>/Commands/<Verb><Feature>/`
   (or `Queries/`): the command/query record, its `AbstractValidator`, its
   `IRequestHandler`. Neither needs registering — `AddApplication()` scans the assembly.
3. Only touch `Zee.Infrastructure` if the feature needs a new repository method or EF
   query — most features reuse what already exists.
4. Add a thin controller action in `Zee.Api/Controllers/` — bind, `Sender.Send(...)`,
   shape the response. No logic.
5. Write tests alongside the layer you changed (`Zee.Domain.UnitTests`,
   `Zee.Application.UnitTests`).

A full worked example, code included: [CONTRIBUTING.md § Worked example](../CONTRIBUTING.md#worked-example-adding-delete-a-post).

### Deleting a backend feature

Delete the feature folder under `Zee.Application/<Feature>/`, its controller actions,
and its EF configuration/repository methods if nothing else uses them. Nothing needs
unregistering — assembly scanning means there's no central list to edit.

---

## 3. Frontend Guidelines (`apps/web`)

**Unlike a per-route `_components`/`_services`/`_models` colocation pattern, ZEE
centralizes shared frontend code.** A route's `page.tsx` composes components from
`src/components/`; it does not own a private folder of its own. This is deliberate for
Phase 1's route count — revisit it if a route grows enough private, single-use pieces
that centralizing them starts to hurt.

```
apps/web/
├── package.json
├── components.json              shadcn/ui config — see TechStack.md
├── next.config.mjs              Security headers; PWA TODO
├── tailwind.config.ts           Maps shadcn tokens to CSS variables; "Ink wash"
│                                 brand scale (monochrome) — see TechStack.md
├── tsconfig.json                strict + noUncheckedIndexedAccess
├── eslint.config.mjs            Flat config (v16 exports an array, not a function)
├── Dockerfile
├── .env.example                 WARNING: NEXT_PUBLIC_* is public
│
├── public/
│   ├── manifest.webmanifest     PWA manifest
│   └── icons/README.md          Icons still to be produced
│
└── src/
    ├── app/
    │   ├── layout.tsx           Root layout, PWA metadata, viewport, Geist font
    │   ├── globals.css          Tailwind layers + shadcn theme tokens (plain hex,
    │   │                        Tailwind v3 compatible — see TechStack.md)
    │   ├── page.tsx             Sign-in: institutional email → OTP → session cookie
    │   ├── api/session/         Route Handler — sets/clears the httpOnly session cookie
    │   └── (app)/               Route group — shares a layout WITHOUT adding a
    │       │                    path segment, so URLs stay /feed not /app/feed
    │       ├── layout.tsx       Shared header (AppHeader) + bottom nav (BottomNav)
    │       ├── feed/            Static visual mock (Ink wash palette). Not wired
    │       │                    to the API - GetFeedQuery/CreatePostCommand are
    │       │                    still stubs.
    │       ├── explore/         Placeholder
    │       ├── chatbot/         Placeholder
    │       ├── groups/          Placeholder
    │       ├── messages/        Placeholder
    │       └── profile/         Placeholder
    ├── components/
    │   ├── ui/                  shadcn-API components (Button, Input, Card, Badge) -
    │   │                        hand-authored for Tailwind v3; see TechStack.md for why
    │   ├── AppHeader.tsx         Shared top bar: logo, search, notifications, "New post"
    │   ├── BottomNav.tsx         Shared bottom nav; highlights the active page
    │   ├── SidebarCard.tsx       One feed sidebar section (title + content)
    │   ├── SkeletonLine.tsx      Grey placeholder bar for unwired content
    │   ├── AvatarPlaceholder.tsx Placeholder profile picture circle
    │   └── PagePlaceholder.tsx   "Not built yet" body for scaffolded routes
    ├── lib/
    │   ├── api-client.ts        ALL backend access goes through here — a component
    │   │                        never calls fetch() directly against the API
    │   ├── auth.ts               OTP flow; token storage decision documented
    │   └── utils.ts              cn() — shadcn-style class merging
    └── types/
        └── api.ts               TypeScript mirrors of the C# DTOs
```

### Contributor workflow: adding a page/route

1. Create `src/app/(app)/<route-name>/page.tsx`. Route segment names are lowercase —
   that's a Next.js constraint, not a house-style choice.
2. Need a new API call? Add it to `src/lib/api-client.ts`'s `api` object — never call
   `fetch()` against the API directly from a component. Add the response shape to
   `src/types/api.ts` if it's new.
3. Need UI? Reach for `src/components/ui/` (shadcn primitives) or an existing shared
   component in `src/components/` first. Only add a new component to `src/components/`
   if nothing existing fits — there is currently no per-route private component folder
   to put it in instead.
4. Wire data into the page. Keep `page.tsx` itself thin — fetching/orchestration only.

### Deleting a page/route

Delete the route folder under `src/app/(app)/<route-name>/`. Check
`src/components/BottomNav.tsx` and `AppHeader.tsx` for a now-dead link to it — nothing
removes those automatically, since navigation isn't generated from the route tree.

---

## 4. AI Service Guidelines (`apps/ai-service`)

**Owner-maintained.** See [../CONTRIBUTING.md](../CONTRIBUTING.md#what-is-reserved)
before proposing changes here.

```
apps/ai-service/
├── pyproject.toml              ruff, mypy, pytest config
├── requirements.txt            Runtime. NO ML dependencies in Phase 1.
├── requirements-dev.txt
├── Dockerfile                  Python 3.12 — prebuilt pydantic-core wheels
├── .env.example
│
├── app/
│   ├── main.py                 FastAPI app. Auth applied at ROUTER level, so a
│   │                           new route is protected by default.
│   ├── core/
│   │   ├── config.py           Settings; refuses to start without the key
│   │   └── security.py         X-Internal-Key check (constant-time)
│   ├── routers/                One module per endpoint group + health
│   ├── schemas/                Pydantic models — the wire contract
│   └── services/                The logic. Mocked in Phase 1.
└── tests/
```

`routers/` handles HTTP, `services/` handles logic, `schemas/` defines the contract —
three peer folders, not one feature folder, because keeping them apart is what lets
Phase 2 replace a service function without touching a route, a schema, or the .NET side
at all.

### Contributor workflow: adding an endpoint (mock work only — see the note above)

1. Define the request/response shape in `app/schemas/<feature>.py`.
2. Implement the (mocked, Phase 1) logic in `app/services/<feature>_service.py`.
3. Add the route in `app/routers/<feature>.py`, depending on the service function.
4. Wire the router into `app/main.py` if it's a new router module.

---

## 5. `infra`

```
infra/
├── docker-compose.yml          Redis + all three services — NO database container
└── .env.example                Neon connection strings + ONE shared INTERNAL_API_KEY
```

**The database is [Neon](https://neon.com)**, not part of Compose — see
[Database.md](Database.md). Extensions (`vector`, `citext`, `pg_trgm`) are declared on
the EF Core model in `AppDbContext.OnModelCreating` and applied by migration, so the
schema has exactly one source of truth.

## `.github/workflows`

| File | Runs |
| --- | --- |
| `api-ci.yml` | build + test, `-warnaserror`, on `apps/api/**` changes |
| `web-ci.yml` | lint, typecheck, build, `npm audit`, on `apps/web/**` changes |
| `ai-service-ci.yml` | ruff, mypy, pytest, on `apps/ai-service/**` changes |
| `discord-notify.yml` | posts pushes, PRs, issues and releases to Discord |

Path filters mean a docs-only PR triggers nothing, and a web PR does not wait on .NET.

---

## 6. Summary Table

| Location | Responsibility | Example |
| :--- | :--- | :--- |
| `apps/api/src/Zee.Domain/Entities/` | Entities, invariants, no dependencies | `Post.cs` |
| `apps/api/src/Zee.Domain/Repositories/` | Repository *interfaces* only | `IPostRepository.cs` |
| `apps/api/src/Zee.Application/<Feature>/Commands\|Queries/` | One use case: command/query + validator + handler | `Posts/Commands/CreatePost/` |
| `apps/api/src/Zee.Infrastructure/Persistence/Repositories/` | Repository *implementations* (EF Core) | `PostRepository.cs` |
| `apps/api/src/Zee.Infrastructure/Persistence/Configurations/` | One EF mapping per entity | `PostConfiguration.cs` |
| `apps/api/src/Zee.Api/Controllers/` | Thin HTTP endpoints — bind, dispatch, shape | `PostsController.cs` |
| `apps/web/src/app/(app)/<route>/` | One page per route, thin orchestration | `feed/page.tsx` |
| `apps/web/src/components/ui/` | Shadcn-API primitives, hand-authored for v3 | `button.tsx` |
| `apps/web/src/components/` | Shared, cross-route components | `AppHeader.tsx` |
| `apps/web/src/lib/api-client.ts` | The only place that calls the backend | — |
| `apps/web/src/types/` | TypeScript mirrors of the C# DTOs | `api.ts` |
| `apps/ai-service/app/routers/` | HTTP routes only | `chatbot.py` |
| `apps/ai-service/app/services/` | Logic — mocked in Phase 1 | `chatbot_service.py` |
| `apps/ai-service/app/schemas/` | Pydantic wire-contract models | `chatbot.py` |

## Conventions

| | |
| --- | --- |
| **C# files** | One public type per file, named after it |
| **Feature folders** | Group by feature, not by technical role |
| **Interfaces** | Declared in the layer that *uses* them, implemented further out |
| **Tests** | Mirror the source tree; `Method_does_x_when_y` naming |
| **`TODO:`** | Marks unimplemented scaffolding, with acceptance criteria |
| **Secrets** | Only ever in `.env` (gitignored) or `*.env.example` (no real values) |

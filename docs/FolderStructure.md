# Folder Structure

Annotated tour of the repository. For *why* it is shaped this way, see
[Architecture.md](Architecture.md).

```
ZEE-platform/
├── .github/workflows/          CI — one workflow per app, plus Discord notifications
├── apps/
│   ├── api/                    ASP.NET Core backend (Clean Architecture)
│   ├── web/                    Next.js frontend (PWA)
│   └── ai-service/             Python FastAPI service — OWNER-MAINTAINED
├── docs/                       This documentation
├── infra/                      Docker Compose and database init
├── .editorconfig               Shared formatting + analyzer suppressions
├── .gitignore
├── CONTRIBUTING.md
├── LICENSE
└── README.md
```

## `apps/api` — the .NET backend

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

**A feature is a folder.** Everything for "create a post" lives under
`Posts/Commands/CreatePost/`: the command, its validator, its handler. You are never
hunting through a `Handlers/` directory of forty unrelated classes.

## `apps/web` — the Next.js frontend

```
apps/web/
├── package.json
├── next.config.mjs             Security headers; PWA TODO
├── tailwind.config.ts          Placeholder brand palette — swap for the real one
├── tsconfig.json               strict + noUncheckedIndexedAccess
├── eslint.config.mjs           Flat config (v16 exports an array, not a function)
├── Dockerfile
├── .env.example                WARNING: NEXT_PUBLIC_* is public
│
├── public/
│   ├── manifest.webmanifest    PWA manifest
│   └── icons/README.md         Icons still to be produced
│
└── src/
    ├── app/
    │   ├── layout.tsx          Root layout, PWA metadata, viewport
    │   ├── globals.css         Tailwind layers
    │   ├── page.tsx            Landing / sign-in
    │   └── (app)/              Route group — shares a layout WITHOUT adding a
    │       │                   path segment, so URLs stay /feed not /app/feed
    │       ├── layout.tsx      Signed-in shell + nav
    │       ├── feed/
    │       ├── chatbot/
    │       ├── groups/
    │       ├── messages/
    │       └── profile/
    ├── components/             Shared React components
    ├── lib/
    │   ├── api-client.ts       ALL backend access goes through here
    │   └── auth.ts             OTP flow; token storage decision documented
    └── types/
        └── api.ts              TypeScript mirrors of the C# DTOs
```

## `apps/ai-service` — the Python service

**Owner-maintained.** See [../CONTRIBUTING.md](../CONTRIBUTING.md#what-is-reserved).

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
│   └── services/               The logic. Mocked in Phase 1.
└── tests/
```

`routers/` handles HTTP, `services/` handles logic, `schemas/` defines the contract.
Keeping them apart is what lets Phase 2 replace the service functions without touching
a route or a schema — and therefore without touching the .NET side at all.

## `infra`

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

## Conventions

| | |
| --- | --- |
| **C# files** | One public type per file, named after it |
| **Feature folders** | Group by feature, not by technical role |
| **Interfaces** | Declared in the layer that *uses* them, implemented further out |
| **Tests** | Mirror the source tree; `Method_does_x_when_y` naming |
| **`TODO:`** | Marks unimplemented scaffolding, with acceptance criteria |
| **Secrets** | Only ever in `.env` (gitignored) or `*.env.example` (no real values) |

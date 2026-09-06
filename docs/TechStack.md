# Technology Stack

A direct, categorized breakdown of the core technologies, libraries, and frameworks
powering **ZEE**. If you are adding a dependency, add a bullet and a reason.

---

## 1. Monorepo & Tooling

* **Docker Compose (v2)**: Orchestrates all three apps plus Redis for local
  development — ZEE has no local database container; every environment, including
  development, points at a [Neon](https://neon.com) branch.
* **.NET 10 / Node.js `>= 20` / Python 3.12**: Three independently-versioned apps, not a
  shared-runtime workspace — Python/ML has a completely different dependency and deploy
  profile from .NET, and keeping them isolated means Phase 2 AI work cannot destabilise
  the platform.
* **`.editorconfig`**: Shared formatting rules and analyzer suppressions across the C#
  tree.
* **GitHub Actions**: One CI workflow per app, path-filtered, so a web-only change never
  waits on a .NET build. Plus a Discord-notification workflow for repo activity.

---

## 2. Backend & API (`apps/api`)

* **.NET 10 / ASP.NET Core Web API**: Clean Architecture
  (Domain → Application → Infrastructure → Api), with the dependency direction enforced
  by project references, not just convention.
* **MediatR (`12.4.1`, pinned)**: One command/query per use case, dispatched through a
  pipeline. Pinned to the last Apache-2.0 release — v13+ requires a commercial licence,
  which doesn't suit an open-source project.
* **FluentValidation**: Declarative validators, discovered by assembly scan and run
  automatically by a MediatR pipeline behaviour before any handler executes.
* **Entity Framework Core + Npgsql.EntityFrameworkCore.PostgreSQL**: Change tracking is
  what makes `IUnitOfWork` a real transaction boundary; maps `List<string>` directly to
  native Postgres `text[]`, so courses/interests/verified-domains need no join tables.
* **Microsoft.AspNetCore.Authentication.JwtBearer**: Validates JWTs issued after OTP
  redemption — there is no password field anywhere in the system.
* **Microsoft.Extensions.Caching.StackExchangeRedis**: Distributed cache; falls back to
  in-memory automatically when no Redis is configured, so `dotnet run` works with zero
  containers.
* **Central Package Management**: Every NuGet version pinned once in
  `Directory.Packages.props` — across seven projects, per-project versions drift within
  weeks.
* **Testing**: xUnit, **Shouldly** (MIT — chosen over FluentAssertions, whose v8 licence
  has the same problem as MediatR v13), **NSubstitute**,
  `Microsoft.AspNetCore.Mvc.Testing`, and the EF Core in-memory provider.

---

## 3. Database

* **PostgreSQL 17 on [Neon](https://neon.com) (serverless)**: One database, shared by
  the API and the AI service, in every environment — see
  [Database.md](Database.md) for setup and branching.
* **`pgvector` / `citext` / `pg_trgm`**: Declared on the EF Core model and enabled by
  migration on day one, even though Phase 1 doesn't use them yet — adding an extension
  later needs elevated rights at an awkward moment, and an unused one costs nothing.
* **Redis 7**: Caching and background jobs.

---

## 4. AI Service (`apps/ai-service`) — owner-maintained

* **FastAPI**: Async, and generates OpenAPI from type hints — the contract with the
  .NET side is derived from code, not hand-maintained separately.
* **uvicorn**: ASGI server.
* **pydantic / pydantic-settings**: Response models double as the wire contract;
  settings fail fast when `INTERNAL_API_KEY` is absent, rather than booting into a
  broken state.
* **No ML dependencies in Phase 1**: No torch, no transformers, no vector client. Every
  endpoint returns a mock, so the image stays small and builds in seconds — see
  [AI_DESIGN.md](AI_DESIGN.md) for what Phase 2 adds.
* **Python 3.12, not the newest release**: `pydantic-core` ships prebuilt wheels for it;
  on 3.14 the install compiles Rust from source and takes minutes.
* **ruff**: Lint and format in one tool, replacing flake8 + isort + black.
* **mypy**: Strict type checking.
* **pytest + httpx**: Tests run against the real ASGI app, not a mocked one.

---

## 5. Frontend & UI (`apps/web`)

* **Next.js 16 (App Router) + React 19**: Server components and file-based routing —
  ZEE is content-heavy and mobile-first, so rendering on the server matters for first
  paint on a phone on campus wifi.
* **TypeScript, `strict` + `noUncheckedIndexedAccess`**: The API returns nullable fields
  throughout; without strict null checks, a missing field becomes a runtime crash in a
  student's browser instead of a build error.
* **Tailwind CSS v3 (deliberately not v4)**: A small contributor team benefits more from
  a shared utility vocabulary than scoped stylesheets, and v4's CSS-first config is still
  unfamiliar to most contributors.
* **`radix-ui` + `class-variance-authority` + `lucide-react`**: The primitives behind
  `src/components/ui/` — unstyled accessible behaviour, variant-driven class composition,
  and icons, respectively.
* **`tailwindcss-animate`**: The v3 animation plugin shadcn components expect — **not**
  `tw-animate-css`, the v4-only equivalent shadcn's installer reaches for by default,
  which does not work here.
* **"Ink wash" colour theme**: A deliberate monochrome brand scale
  (`#252525` / `#545454` / `#7d7d7d` / `#cfcfcf`) rather than a hue, configured once as
  the `brand` colour in `tailwind.config.ts` and picked up everywhere.
* **ESLint + eslint-config-next**: Catches React/Next-specific mistakes the compiler
  can't.
* **Vitest**: Shares Vite's transform pipeline — faster than Jest.
* **PWA manifest, no service worker yet**: The app installs but isn't offline-capable —
  deliberately deferred, since caching an authenticated feed wrongly is worse than not
  caching it at all: a shared device could serve one student's posts to the next.

> **shadcn/ui note:** `npx shadcn add` generates Tailwind **v4**-only code
> (`--spacing()` calc calls, `in-data-[...]` variants, `color-mix(in oklch, ...)`) that
> will not compile against this project's v3 setup. Components in `src/components/ui/`
> keep the same name, props and colour tokens shadcn expects
> (`bg-primary`, `border-input`, etc.), hand-written in plain classes that actually
> compile. Port a new component by hand — don't paste `npx shadcn add` output verbatim.

---

## Version pins worth knowing

Next is on 16.x rather than the 15.1 originally planned — 15.x has open advisories via
`postcss`. `postcss` and `vitest` are pinned above their default resolved ranges for the
same reason, and `npm audit --audit-level=high` is enforced in CI.

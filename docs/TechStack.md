# Tech Stack

Every dependency ZEE uses, and why it is there. If you are adding one, add a row and
a reason.

## Overview

| Concern | Choice | Version |
| --- | --- | --- |
| Frontend | Next.js (App Router) + TypeScript + Tailwind | 16.x / 5.7 / 3.4 |
| Main backend | ASP.NET Core Web API, Clean Architecture | .NET 10 |
| AI service | Python + FastAPI | 3.12 / 0.115 |
| Database | PostgreSQL + pgvector | 17 |
| Cache / queue | Redis | 7 |
| Local dev | Docker Compose | v2 |

## Frontend — `apps/web`

| Package | Why |
| --- | --- |
| **next** | App Router gives server components and file-based routing. ZEE is content-heavy and mobile-first, so rendering on the server matters for first paint on a phone on campus wifi. |
| **react** 19 | Required by Next 16. |
| **typescript** | `strict` plus `noUncheckedIndexedAccess`. The API returns nullable fields throughout; without strict null checks those become runtime crashes in a student's browser instead of build errors. |
| **tailwindcss** | Utility CSS keeps styling next to markup. Chosen over CSS Modules because a small contributor team benefits more from a shared vocabulary than from scoped stylesheets. v3 rather than v4 — v4's CSS-first config is still unfamiliar to most contributors. |
| **eslint** + **eslint-config-next** | Catches React and Next-specific mistakes the compiler cannot. |
| **vitest** | Faster than Jest and shares Vite's transform pipeline. |

**Version pins worth knowing:** Next is on 16.x, not the 15.1 originally planned —
15.x has open advisories via `postcss`. `postcss` and `vitest` are pinned above their
default resolved ranges for the same reason. `npm audit` is clean and CI enforces it.

**PWA:** manifest and install prompt only in Phase 1. A service worker is deliberately
deferred: caching an authenticated feed wrongly is worse than not caching it, since a
shared device could serve one student's posts to the next.

## Main backend — `apps/api`

| Package | Why |
| --- | --- |
| **MediatR** | One command/query per use case, dispatched through a pipeline. Gives cross-cutting validation for free and keeps controllers to three lines. **Pinned to 12.4.1** — the last Apache-2.0 release; v13+ requires a commercial licence, which does not suit an open-source project. |
| **FluentValidation** | Declarative validators discovered by assembly scan and run by `ValidationBehaviour`, so a handler cannot skip validation by forgetting to call one. |
| **Entity Framework Core** | Change tracking is what makes `IUnitOfWork` a real transaction boundary rather than a wrapper. |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | PostgreSQL provider. Maps `List<string>` to native `text[]`, which is why courses, interests and verified domains need no join tables. |
| **Microsoft.Extensions.Caching.StackExchangeRedis** | Distributed cache. Falls back to in-memory when no Redis is configured, so `dotnet run` works with no container. |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | Validates the JWTs issued after OTP redemption. |
| **Microsoft.Extensions.Options.DataAnnotations** | `ValidateOnStart()` — a missing internal key stops the container coming up rather than surfacing as a confusing 401 hours later. |

**Testing:** xUnit, **Shouldly** (readable assertions, MIT — chosen over FluentAssertions,
whose v8 licence has the same problem as MediatR v13), **NSubstitute**,
`Microsoft.AspNetCore.Mvc.Testing`, and the EF Core in-memory provider.

**Central Package Management:** every version is pinned once in
`Directory.Packages.props`; project files reference packages without a version. With
seven projects, per-project versions drift within weeks.

## AI service — `apps/ai-service`

| Package | Why |
| --- | --- |
| **fastapi** | Async, and generates OpenAPI from type hints — so the contract with the .NET side is derived from the code rather than maintained separately. |
| **uvicorn** | ASGI server. |
| **pydantic** / **pydantic-settings** | Response models double as the wire contract; settings fail fast when `INTERNAL_API_KEY` is absent. |
| **ruff** | Lint and format in one tool, replacing flake8 + isort + black and the three-way config disagreements that come with running them separately. |
| **mypy** | Strict type checking. |
| **pytest** + **httpx** | Tests against the real ASGI app. |

**No ML dependencies in Phase 1.** No torch, no transformers, no vector client. The
endpoints return mocks, so the image stays small and builds in seconds. Phase 2 adds
them — see [AI_DESIGN.md](AI_DESIGN.md).

**Python 3.12, not the newest release:** `pydantic-core` ships prebuilt wheels for it.
On 3.14 the install compiles Rust from source and takes minutes.

## Data

**PostgreSQL 17 with pgvector.** One database, shared by both services. The extension
is enabled from the first `docker compose up` even though nothing in Phase 1 uses it —
turning it on later would mean a migration needing elevated rights at an awkward
moment, and an unused extension costs nothing.

Also enabled: `citext` and `pg_trgm` (for search later).

**Redis 7** for caching and background jobs.

## Notable decisions

| Decision | Reasoning |
| --- | --- |
| UUIDv7 primary keys | Time-ordered, so inserts append to the right edge of the B-tree instead of scattering across it and fragmenting the index, as random v4 GUIDs do. Same opaque 128-bit id to clients. |
| Keyset pagination, not `OFFSET` | On a feed receiving new posts constantly, `OFFSET` shows duplicates and skips rows between pages, and gets linearly slower with depth. |
| No password field anywhere | Auth is an OTP to a verified institutional inbox. Nothing to leak, nothing to reuse. |
| Separate AI service | Python/ML has a completely different dependency and deploy profile from .NET. Phase 2 work cannot destabilise the platform. |
| Browser never calls the AI service | It has no public route. All AI traffic proxies through the API, which holds `X-Internal-Key` server-side. |

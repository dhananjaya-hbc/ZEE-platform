# ZEE

**A global campus social network — open source, multi-university, student-first.**

ZEE connects students across campuses worldwide: a shared feed, course and club groups,
competitions and team recruiting, campus events, achievements, and direct messaging.
Access is gated by **institutional email verification**, so every account is tied to a
real, verified university.

---

## 🚧 Project status: Phase 1 scaffolding

**This repository is a scaffold, not a working product yet.** The structure, contracts,
tests and infrastructure are in place. Most method bodies are `TODO` stubs carrying
acceptance criteria, waiting to be implemented.

| | |
| --- | --- |
| ✅ Builds and runs | The whole stack starts with one `docker compose up` |
| ✅ CI is green | 56 API tests: 53 skipped stubs, 2 passing smoke tests, 0 failures |
| ⚠️ Endpoints return **501** | Routed and reachable, handler not implemented yet |
| ⚠️ No EF migrations yet | The schema comes from EF conventions until the configurations are written |

**This is deliberate.** The point is that there is a lot of well-specified,
self-contained work available to pick up. Every stub states exactly what it must do and
has a matching test written as a skipped fact:

```bash
grep -rn "TODO:" apps/ --include=*.cs --include=*.ts --include=*.py
```

Start at **[CONTRIBUTING.md](CONTRIBUTING.md)**.

### Phase 1 vs Phase 2

**Phase 1 (this repository): no AI features.** The feed is plain reverse-chronological.
The AI service exists with real routes, real auth and real schemas, but its logic
returns nothing yet.

**Phase 2 (later): the real RAG chatbot, student matching, and ranked feed** — built on
actual student usage data, because none of the three can be evaluated without it.
`/apps/ai-service` is owner-maintained; see
[CONTRIBUTING.md](CONTRIBUTING.md#what-is-reserved) and [docs/AI_DESIGN.md](docs/AI_DESIGN.md).

---

## Contents

- [Architecture](#architecture)
- [Repository layout](#repository-layout)
- [Quick start](#quick-start)
- [Running services individually](#running-services-individually)
- [Environment variables](#environment-variables)
- [Auth flow](#auth-flow)
- [Testing](#testing)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [License](#license)

---

## Architecture

```
                    ┌──────────────────────────────┐
                    │   apps/web — Next.js PWA     │
                    │   TypeScript · Tailwind      │
                    └──────────────┬───────────────┘
                                   │ HTTPS, Bearer JWT
                                   ▼
                    ┌──────────────────────────────┐
                    │  apps/api — ASP.NET Core     │
                    │  Clean Architecture + CQRS   │
                    │  auth · posts · feed · groups│
                    │  competitions · events · DMs │
                    └───┬──────────────────────┬───┘
                        │                      │ X-Internal-Key
        ┌───────────────┴──────┐               │ (never exposed to browsers)
        ▼                      ▼               ▼
┌───────────────┐      ┌─────────────┐  ┌─────────────────────────┐
│  PostgreSQL   │      │    Redis    │  │ apps/ai-service         │
│  + pgvector   │      │ cache/queue │  │ Python · FastAPI        │
└───────────────┘      └─────────────┘  └─────────────────────────┘
```

| Decision | Why |
| --- | --- |
| Separate AI service | Python/ML has a completely different dependency and deploy profile from .NET. Phase 2 work cannot destabilise the platform. |
| The browser never calls the AI service | It has no public route. All AI traffic proxies through the API, which holds the internal key server-side. |
| `IAiServiceClient` in the Application layer | Handlers depend on the interface; Infrastructure supplies the HTTP implementation. Phase 2 changes the Python side only. |
| pgvector enabled on day one | Turning it on later needs elevated rights at an awkward moment. Unused extensions cost nothing. |
| Universities are data, not config | Email-domain allowlists live in a `University` table. Nothing is hardcoded to one campus. |
| No passwords anywhere | Proving you can read mail at a verified campus domain *is* the credential. |

Full detail: **[docs/Architecture.md](docs/Architecture.md)**.

## Repository layout

```
ZEE-platform/
├── apps/
│   ├── web/                  Next.js frontend (PWA)
│   ├── api/                  ASP.NET Core backend — Clean Architecture
│   │   ├── src/
│   │   │   ├── Zee.Domain/          entities, enums, repository interfaces
│   │   │   ├── Zee.Application/     MediatR commands/queries, validators, DTOs
│   │   │   ├── Zee.Infrastructure/  EF Core, repositories, AiServiceClient
│   │   │   └── Zee.Api/             controllers, middleware, Program.cs
│   │   └── tests/                   one test project per layer
│   └── ai-service/           Python FastAPI service — owner-maintained
├── infra/
│   └── docker-compose.yml    Postgres + Redis + all three services
├── docs/                     architecture, tech stack, folder structure, AI design
├── .github/workflows/        CI per app + Discord notifications
├── CONTRIBUTING.md
└── README.md
```

File-by-file tour: [docs/FolderStructure.md](docs/FolderStructure.md).

## Quick start

**Prerequisites:** Docker Desktop (or Docker Engine + Compose v2). Nothing else.

```bash
git clone https://github.com/dhananjaya-hbc/ZEE-platform.git
cd ZEE-platform

cp infra/.env.example infra/.env
# Edit infra/.env:
#   - NEON_NPGSQL_CONNECTION_STRING and NEON_DATABASE_URL — from a free Neon
#     project at https://neon.com. See docs/Database.md.
#   - INTERNAL_API_KEY and JWT_KEY:
#       openssl rand -hex 32     # INTERNAL_API_KEY
#       openssl rand -base64 48  # JWT_KEY

docker compose -f infra/docker-compose.yml up --build
```

**ZEE has no local database container** — it runs on
[Neon](https://neon.com) (serverless PostgreSQL) in every environment, including
development. See [docs/Database.md](docs/Database.md) for setup, why, and how to give
yourself an isolated branch.

| Service | URL |
| --- | --- |
| Web app | http://localhost:3000 |
| API | http://localhost:5080 |
| API OpenAPI document | http://localhost:5080/openapi/v1.json |
| API liveness | http://localhost:5080/health |
| API readiness (hits the DB) | http://localhost:5080/health/ready |
| AI service docs | http://localhost:8000/docs |
| Redis | `localhost:6379` |
| Database | your Neon branch — see [docs/Database.md](docs/Database.md) |

> **Note:** most endpoints currently return **501 Not Implemented** — that is the
> scaffolding reporting itself honestly. `/health` works, and `/api/feed` correctly
> returns 401 without a token.

## Running services individually

Start just the datastores:

```bash
docker compose -f infra/docker-compose.yml up redis   # the database is Neon, not a container
```

**API** (.NET 10)

```bash
cd apps/api
dotnet restore
dotnet run --project src/Zee.Api      # http://localhost:5080
```

`appsettings.Development.json` already points at the Compose datastores, so no `.env`
is needed for local runs.

**Web** (Node 20+)

```bash
cd apps/web
cp .env.example .env.local
npm install
npm run dev                            # http://localhost:3000
```

**AI service** (Python 3.12)

```bash
cd apps/ai-service
cp .env.example .env
python -m venv .venv && source .venv/bin/activate
pip install -r requirements-dev.txt
uvicorn app.main:app --reload --port 8000
```

## Environment variables

Every app ships a `.env.example`. Copy it, never edit it with real values, never commit
the result — `.gitignore` blocks `.env` files.

| File | Covers |
| --- | --- |
| `infra/.env.example` | Compose: Neon connection strings, Redis, ports, and the shared internal key |
| `apps/api/.env.example` | Connection strings, JWT signing, AI service URL + internal key |
| `apps/web/.env.example` | Public API base URL — note every `NEXT_PUBLIC_*` value ships to the browser |
| `apps/ai-service/.env.example` | The internal key it validates, database URL for Phase 2 |

`AiService__InternalKey` (API) and `INTERNAL_API_KEY` (AI service) **must match** — that
shared secret is the only thing standing between the AI service and anyone who can reach
its port. Compose injects one value into both so they cannot drift.

## Auth flow

Signup and login are the same flow: **a one-time code to a verified institutional inbox.**

```
1. POST /api/auth/request-otp   { email: "ada@mit.edu" }
        │
        ├─ Extract the domain → look up University by verified email domain
        ├─ Unknown domain → 404 "university not onboarded"   (onboarding is manual)
        └─ Known domain   → generate 6 digits, HASH it, store with a 10-minute
                            expiry, email the plaintext code

2. POST /api/auth/verify-otp    { email, code }
        │
        ├─ Check consumed → expired → attempt cap, BEFORE comparing
        ├─ Constant-time hash comparison, max 5 attempts
        └─ Valid → create or load the User, issue a JWT

3. Client sends  Authorization: Bearer <jwt>  on every subsequent request.
```

Domain matching is **exact, never a suffix** — `notmit.edu` ends with `mit.edu` under a
naive `EndsWith`, which would let anyone registering a lookalike domain join that
campus. There is a regression test for exactly this.

University onboarding is **owner-reviewed and manual**. There is no self-serve endpoint;
a new `University` row is inserted deliberately. That is the anti-abuse boundary for the
whole platform.

> In Development the OTP email is not sent — the code goes to the API logs.

## Testing

```bash
cd apps/api        && dotnet test
cd apps/web        && npm run lint && npm run typecheck && npm run build
cd apps/ai-service && ruff check . && pytest
```

Tests for unimplemented stubs are **skipped, not failing**, so CI is green on a fresh
clone — a red baseline would make it impossible to tell your own breakage from the
scaffolding's. Implementing a stub means removing its `Skip`.

CI runs per app on every PR, with path filters, so a web-only change does not wait on a
.NET build.

## Documentation

| Doc | What's in it |
| --- | --- |
| [CONTRIBUTING.md](CONTRIBUTING.md) | **Start here.** Layer guide, a full worked example, how to claim a stub |
| [docs/Database.md](docs/Database.md) | Setting up Neon, branching, pooled vs direct endpoints, migrations |
| [docs/Architecture.md](docs/Architecture.md) | Layer boundaries, request lifecycle, data model, auth, feed, known gaps |
| [docs/TechStack.md](docs/TechStack.md) | Every dependency and the reason it's there |
| [docs/FolderStructure.md](docs/FolderStructure.md) | Annotated directory tour |
| [docs/AI_DESIGN.md](docs/AI_DESIGN.md) | The Phase 2 plan and the constraints it must respect |
| [docs/Logs.md](docs/Logs.md) | Development log — decisions and their reasoning |

## Contributing

Contributions are very welcome — the scaffolding exists so there is clear work to pick
up. Read [CONTRIBUTING.md](CONTRIBUTING.md) first.

Two things to know up front:

- **Branch from `dev`, PR into `dev`.** `main` is the released state.
- **`/apps/ai-service` is owner-maintained.** Bug fixes, tooling and infrastructure PRs
  are welcome; the Phase 2 AI logic is reserved. Open an issue to discuss instead.

## License

MIT — see [LICENSE](LICENSE).

# ZEE

**A global campus social network — open source, multi-university, student-first.**

ZEE connects students across campuses worldwide: a shared feed, course and club groups,
competitions and team recruiting, campus events, achievements, and direct messaging.
Access is gated by **institutional email verification**, so every account is tied to a
real, verified university.

> **Phase 1 (this repository, today):** the complete student-facing platform with **no AI
> features**. The feed is plain reverse-chronological. The AI service exists but returns
> **mocked** responses.
>
> **Phase 2 (later):** the real RAG chatbot, student matching, and ranked feed get built
> on top of actual student usage data. `/apps/ai-service` is owner-maintained — see
> [CONTRIBUTING.md](CONTRIBUTING.md).

---

## Table of contents

- [Architecture at a glance](#architecture-at-a-glance)
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

## Architecture at a glance

Three services and two datastores, all wired together by Docker Compose for local development.

```
                    ┌──────────────────────────────┐
                    │   apps/web — Next.js 14 PWA  │
                    │   TypeScript · Tailwind      │
                    └──────────────┬───────────────┘
                                   │ HTTPS (public API)
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
│               │      │             │  │ Phase 1: mocked only    │
└───────────────┘      └─────────────┘  └─────────────────────────┘
```

**Key decisions**

| Decision | Why |
| --- | --- |
| Separate AI service | The AI stack (Python, embeddings, vector search) has a totally different dependency and deploy profile from the .NET API. Keeping it separate means Phase 2 work never destabilises the platform. |
| The browser never calls the AI service | All AI traffic is proxied through the .NET API, which attaches an internal API key. The AI service is not internet-facing. |
| `IAiServiceClient` lives in Application | The Application layer depends on the *interface*; Infrastructure supplies the HTTP implementation. Swapping mocks for the real service is a one-line DI change. |
| pgvector enabled on day one | The extension is turned on now so Phase 2 needs no migration scramble. Nothing in Phase 1 uses it. |
| Universities are data, not config | Email-domain allowlists live in a `University` table. ZEE spans many campuses; nothing is hardcoded to one school. |

Full detail: [docs/Architecture.md](docs/Architecture.md).

---

## Repository layout

```
ZEE-platform/
├── apps/
│   ├── web/                  Next.js 14 App Router frontend (PWA)
│   ├── api/                  ASP.NET Core Web API — Clean Architecture
│   │   ├── src/
│   │   │   ├── Zee.Domain/          entities, enums, repository interfaces
│   │   │   ├── Zee.Application/     MediatR commands/queries, validators, DTOs
│   │   │   ├── Zee.Infrastructure/  EF Core, repositories, AiServiceClient
│   │   │   └── Zee.Api/             controllers, middleware, Program.cs
│   │   └── tests/                   one test project per layer
│   └── ai-service/           Python FastAPI service (mocked in Phase 1)
├── infra/
│   └── docker-compose.yml    Postgres + Redis + all three services
├── docs/                     architecture, tech stack, folder structure, AI design
├── .github/workflows/        CI per app + Discord notifications
├── CONTRIBUTING.md
└── README.md
```

A file-by-file tour lives in [docs/FolderStructure.md](docs/FolderStructure.md).

---

## Quick start

**Prerequisites:** Docker Desktop (or Docker Engine + Compose v2). Nothing else is required
to run the whole stack.

```bash
git clone https://github.com/dhananjaya-hbc/ZEE-platform.git
cd ZEE-platform

# 1. Create your local env file from the template
cp infra/.env.example infra/.env

# 2. Bring the whole stack up
docker compose -f infra/docker-compose.yml up --build
```

Then open:

| Service | URL |
| --- | --- |
| Web app | http://localhost:3000 |
| API (Scalar/OpenAPI) | http://localhost:5080/scalar |
| AI service (Swagger) | http://localhost:8000/docs |
| Postgres | `localhost:5432` (user `zee`, db `zee`) |
| Redis | `localhost:6379` |

The API applies EF Core migrations on startup in Development, so the schema is ready as
soon as the container is healthy.

> **Heads-up:** in Phase 1 the OTP email is not actually sent. In Development the code is
> written to the API logs — grab it from `docker compose logs api`.

---

## Running services individually

Useful when you're iterating on one app and don't want the whole stack rebuilding.

Start just the datastores:

```bash
docker compose -f infra/docker-compose.yml up postgres redis
```

**API (.NET 10)**

```bash
cd apps/api
cp .env.example .env
dotnet restore
dotnet run --project src/Zee.Api
```

**Web (Node 20+)**

```bash
cd apps/web
cp .env.example .env.local
npm install
npm run dev
```

**AI service (Python 3.11+)**

```bash
cd apps/ai-service
cp .env.example .env
python -m venv .venv && source .venv/bin/activate
pip install -r requirements-dev.txt
uvicorn app.main:app --reload --port 8000
```

---

## Environment variables

Every app ships a `.env.example`. Copy it, never edit it with real values, never commit
the result — `.gitignore` blocks `.env` files by default.

| File | Covers |
| --- | --- |
| `infra/.env.example` | Docker Compose: Postgres credentials, Redis, the shared internal key |
| `apps/api/.env.example` | Connection strings, JWT signing, AI service base URL + internal key |
| `apps/web/.env.example` | Public API base URL, PWA toggles |
| `apps/ai-service/.env.example` | Internal key it validates, database URL for Phase 2 |

`AI_SERVICE__INTERNALKEY` (API) and `INTERNAL_API_KEY` (AI service) **must match** — that
shared secret is the only thing standing between the AI service and anyone who can reach
its port.

---

## Auth flow

Signup and login are the same flow: **one-time password to a verified institutional inbox.**

```
1. POST /api/auth/request-otp   { email: "ada@mit.edu" }
        │
        ├─ Extract domain → look up University by verified email domain
        ├─ Unknown domain → 404 "university not onboarded"  (onboarding is manual)
        └─ Known domain   → generate 6-digit code, hash it, store with 10-min expiry,
                            email the plaintext code
2. POST /api/auth/verify-otp    { email, code }
        │
        ├─ Compare hash, check expiry + attempt count
        └─ Valid → create User (first time) or load them, issue JWT
3. Client sends  Authorization: Bearer <jwt>  on every subsequent request.
```

University onboarding is **owner-reviewed and manual** in Phase 1. There is no self-serve
"add my university" endpoint — a new `University` row (with its verified domains) is
inserted deliberately. This is the anti-abuse boundary for the whole platform.

---

## Testing

```bash
# API — unit + integration
cd apps/api && dotnet test

# Web — lint + typecheck + unit
cd apps/web && npm run lint && npm run typecheck && npm test

# AI service — lint + unit
cd apps/ai-service && ruff check . && pytest
```

CI runs all three on every pull request. See [.github/workflows/](.github/workflows/).

---

## Documentation

| Doc | What's in it |
| --- | --- |
| [docs/Architecture.md](docs/Architecture.md) | Layer boundaries, request lifecycle, data model, why each choice was made |
| [docs/TechStack.md](docs/TechStack.md) | Every dependency and the reason it's there |
| [docs/FolderStructure.md](docs/FolderStructure.md) | Annotated directory tree |
| [docs/AI_DESIGN.md](docs/AI_DESIGN.md) | The Phase 2 plan: RAG, matching, ranking — and the contract Phase 1 must honour |
| [docs/Logs.md](docs/Logs.md) | Running development log |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Branching, commits, and a worked example of adding a feature through all four layers |

---

## Contributing

Contributions are welcome. Start with [CONTRIBUTING.md](CONTRIBUTING.md) — it walks the
Clean Architecture layers with a complete worked example (adding "delete a post" end to
end).

Two things worth knowing up front:

- **Branch from `dev`, PR into `dev`.** `main` is the released state.
- **`/apps/ai-service` is owner-maintained.** Phase 2 AI logic is deliberately reserved.
  Bug fixes and infrastructure PRs there are welcome; new AI features are not — please
  open an issue to discuss instead.

## License

MIT — see [LICENSE](LICENSE).

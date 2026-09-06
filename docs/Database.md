# Database — Neon

ZEE runs on [Neon](https://neon.com), serverless PostgreSQL. **There is no local
database container.** Every environment, including your development machine, points at
a Neon branch.

## Contents

- [Why Neon](#why-neon)
- [First-time setup](#first-time-setup)
- [Pooled vs direct endpoints](#pooled-vs-direct-endpoints)
- [Connection string format](#connection-string-format)
- [Migrations](#migrations)
- [Seeding a university](#seeding-a-university)
- [Branching](#branching)
- [Scale-to-zero and cold starts](#scale-to-zero-and-cold-starts)
- [Extensions](#extensions)
- [Troubleshooting](#troubleshooting)

## Why Neon

| Reason | Detail |
| --- | --- |
| **pgvector is supported** | The one hard requirement Phase 2 has. No self-hosting, no extension wrangling. |
| **Branching** | A database branch is instant and copy-on-write. That gives every contributor an isolated database and makes per-PR preview environments practical. |
| **Scales to zero** | An idle project costs no compute, which suits a project with bursty, unpredictable usage. |
| **It is just Postgres** | Same wire protocol, same SQL, same Npgsql driver. Nothing in the codebase is Neon-specific. |

The tradeoff, stated plainly: **you need a Neon account and an internet connection to
run ZEE.** There is no offline path. If that becomes a barrier for contributors, the
fallback is to add a Postgres container back to Compose for local work — nothing in the
code would change, because nothing depends on Neon specifically.

## First-time setup

1. **Create a free project** at [neon.com](https://neon.com). Name it `zee`.
2. **Create your own branch.** Do not develop against `main`, and do not share a branch
   with another contributor — you will overwrite each other's data. From the dashboard:
   *Branches → New Branch*, named something like `dev-yourname`, created from `main`.
3. **Copy the connection details** for your branch. You need two formats:
   - the **.NET / ADO.NET** string, for the API
   - the **`postgresql://` URI**, for the AI service
4. **Fill in your env file:**
   ```bash
   cp infra/.env.example infra/.env
   ```
   Set `NEON_NPGSQL_CONNECTION_STRING` and `NEON_DATABASE_URL`, plus `INTERNAL_API_KEY`
   and `JWT_KEY`. Compose refuses to start without any of them.
5. **Apply migrations** (see [below](#migrations)).
6. **Start the stack:**
   ```bash
   docker compose -f infra/docker-compose.yml up --build
   ```

> `infra/.env` is gitignored. Your connection string contains a password — never commit
> it, and never paste it into an issue or a PR.

## Pooled vs direct endpoints

Neon gives every branch two hostnames. The difference matters and is the most common
source of confusion.

| | Hostname | Use for |
| --- | --- | --- |
| **Pooled** | `ep-xxx-pooler.region.aws.neon.tech` | The running application |
| **Direct** | `ep-xxx.region.aws.neon.tech` | Migrations, and any admin/DDL work |

The pooled endpoint is PgBouncer in **transaction mode**. It multiplexes many client
connections onto few Postgres ones, which is what makes a serverless database viable
under load — but transaction-mode pooling has no stable session, so anything relying on
session state does not work through it.

Two consequences for us:

- **The app must set `No Reset On Close=true`.** Npgsql sends a session reset when
  returning a connection to its pool; PgBouncer in transaction mode does not support it.
- **Migrations should use the direct endpoint.** Schema changes want a real session.

## Connection string format

**.NET (Npgsql):**

```
Host=ep-xxx-pooler.region.aws.neon.tech;Database=zee;Username=USER;Password=PASS;SSL Mode=VerifyFull;Channel Binding=Require;No Reset On Close=true
```

| Parameter | Why |
| --- | --- |
| `SSL Mode=VerifyFull` | Neon requires TLS. `VerifyFull` checks the certificate *and* the hostname. **Do not downgrade this to `Trust Server Certificate=true`** — that accepts any certificate, which is exactly the check that stops someone intercepting your database traffic. Neon's certificates are publicly trusted, so verification works with no extra configuration. |
| `Channel Binding=Require` | Binds authentication to the TLS channel, so credentials cannot be replayed over a different connection. |
| `No Reset On Close=true` | Required for the pooled endpoint. See above. |

**Python (libpq URI):**

```
postgresql://USER:PASS@ep-xxx-pooler.region.aws.neon.tech/zee?sslmode=require
```

## Migrations

EF Core owns the schema. There is no init script — extensions are declared on the model
in `AppDbContext.OnModelCreating`, so `CREATE EXTENSION` lands in the migration like
everything else and the schema has exactly one source of truth.

> **Current status:** `InitialCreate` exists and is applied — it covers the entities
> implemented so far (`University`, `User`, `EmailVerificationCode`). As each remaining
> `IEntityTypeConfiguration` stub gets implemented (see the open issues), generate a new
> migration for it with `dotnet ef migrations add <Name>` using the same command below —
> don't fold unrelated schema changes into `InitialCreate`.

Run migrations against the **direct** endpoint:

```bash
cd apps/api

# One-off: the EF tooling
dotnet tool install --global dotnet-ef

# Point at the DIRECT endpoint (no "-pooler" in the hostname)
export ConnectionStrings__Postgres="Host=ep-xxx.region.aws.neon.tech;Database=zee;Username=USER;Password=PASS;SSL Mode=VerifyFull;Channel Binding=Require"

dotnet ef migrations add InitialCreate \
  --project src/Zee.Infrastructure \
  --startup-project src/Zee.Api

dotnet ef database update \
  --project src/Zee.Infrastructure \
  --startup-project src/Zee.Api
```

**In production, migrations are a deployment step, not a startup step.** `Program.cs`
calls `MigrateAsync` only in Development — two instances starting together would
otherwise race to migrate the same database.

## Seeding a university

Sign-in only works for an email domain an onboarded `University` row has claimed —
without one, `request-otp` always 404s. Every contributor works on their **own** Neon
branch ([Branching](#branching), and see `CONTRIBUTING.md`), so nobody's branch has any
universities in it until you put one there. Seeding your own branch with a test row is
expected and fine; it's not the same thing as onboarding a university on the real,
shared platform.

> **This is your own branch, not the shared database.** Onboarding a university on the
> real platform is deliberately owner-reviewed and manual — see
> [Architecture.md](Architecture.md#authentication) for why. Nothing below grants anyone
> else that access; it only seeds data into the private branch you already control.

There is no seed script or endpoint (that's the same deliberate gap, applied
consistently). Insert the row directly, against your branch's connection string:

```bash
psql "$NEON_DATABASE_URL" <<'SQL'
INSERT INTO universities (id, name, country, verified_email_domains, is_active, created_at)
VALUES (
  gen_random_uuid(),
  'University of Moratuwa',
  'LK',                    -- ISO 3166-1 alpha-2, exactly 2 letters, uppercase
  ARRAY['uom.lk'],         -- lowercase, no leading '@' — the exact form
                           -- FindByEmailDomainAsync matches against
  true,
  now()
);
SQL
```

`gen_random_uuid()` gives a plain random UUID rather than the app's usual UUIDv7 — fine
here specifically, since `universities` is a tiny, rarely-inserted table where the
time-ordering property that matters for high-volume tables (posts, etc.) buys nothing.

Once the row exists, `you@uom.lk` (any address at that domain) can request and verify an
OTP against your branch.

## Branching

This is the feature worth actually using.

```
main                    ← production schema and data
├── dev-alice           ← Alice's isolated database
├── dev-bob             ← Bob's isolated database
└── preview/pr-42       ← ephemeral, created per pull request
```

Branches are copy-on-write, so creating one is near-instant and costs storage only for
what diverges. Practical uses:

- **Per-contributor development.** Break the schema freely; you affect nobody.
- **Per-PR previews.** Create a branch in CI, run migrations against it, tear it down on
  merge. Neon has a GitHub Action for this.
- **Realistic integration tests.** The clearest path to closing the known gap where
  `Zee.Api.IntegrationTests` uses EF Core's in-memory provider — which ignores unique
  indexes and foreign keys and cannot verify that a LINQ query translates to SQL. A
  branch per CI run gives real Postgres with no container.

## Scale-to-zero and cold starts

Neon suspends compute after a few minutes of inactivity. The first query afterwards pays
a resume — usually well under a second, but it can surface as a transient connection
failure rather than merely slow success.

The scaffolding accounts for this in two places:

1. **`EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: 10s)`** in
   `Zee.Infrastructure/DependencyInjection.cs`. Absorbs the resume so it is invisible
   rather than a 500 for whoever makes the first request of the morning.

2. **Split health endpoints**, because a health check that touches the database on every
   poll would keep waking the compute so it never idles down at all:

   | Endpoint | Checks | Poll it |
   | --- | --- | --- |
   | `/health` | Nothing — process liveness only | Frequently. This is what Compose and uptime monitors use. |
   | `/health/ready` | The database | Rarely, or on deploy only |

   If you add a monitor, point it at `/health`. Pointing it at `/health/ready` on a
   short interval will quietly consume your compute allowance.

## Extensions

Declared in `AppDbContext.OnModelCreating`, applied by migration. Neon permits all three
without superuser rights.

| Extension | Status |
| --- | --- |
| `vector` (pgvector) | Enabled, **unused in Phase 1**. Present so Phase 2 needs no migration scramble. |
| `citext` | Enabled, unused. Emails are normalised to lowercase in the domain layer instead, so ordinary indexes still apply. |
| `pg_trgm` | Enabled, unused. For search over post and group names later. |

## Troubleshooting

| Symptom | Cause |
| --- | --- |
| `Connection string 'Postgres' is not configured` | `infra/.env` missing or `NEON_NPGSQL_CONNECTION_STRING` unset. |
| `The SSL connection could not be established` | Missing `SSL Mode=VerifyFull`. Add it — do not switch to `Trust Server Certificate=true`. |
| `prepared statement "_p1" already exists` | Using the pooled endpoint without `No Reset On Close=true`, or with auto-prepare enabled. |
| Migrations hang or behave oddly | Running against the pooled endpoint. Use the direct one. |
| First request after idle fails | A cold start that exhausted the retries. Check `EnableRetryOnFailure` is still configured. |
| Compute hours draining with no traffic | Something is polling `/health/ready` instead of `/health`. |

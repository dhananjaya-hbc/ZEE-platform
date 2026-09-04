# Architecture

How ZEE is put together, and why. For the layer-by-layer contribution guide with a
worked example, see [../CONTRIBUTING.md](../CONTRIBUTING.md).

## Contents

- [System shape](#system-shape)
- [Request lifecycle](#request-lifecycle)
- [The four layers](#the-four-layers)
- [Data model](#data-model)
- [Authentication](#authentication)
- [The feed](#the-feed)
- [The AI boundary](#the-ai-boundary)
- [Known gaps](#known-gaps)

## System shape

```
                    ┌──────────────────────────────┐
                    │   apps/web — Next.js PWA     │
                    └──────────────┬───────────────┘
                                   │ HTTPS, Bearer JWT
                                   ▼
                    ┌──────────────────────────────┐
                    │  apps/api — ASP.NET Core     │
                    │  Clean Architecture + CQRS   │
                    └───┬──────────────────────┬───┘
                        │                      │ X-Internal-Key
        ┌───────────────┴──────┐               │
        ▼                      ▼               ▼
┌───────────────┐      ┌─────────────┐  ┌─────────────────────────┐
│  PostgreSQL   │      │    Redis    │  │ apps/ai-service         │
│  + pgvector   │      │             │  │ FastAPI (mocked)        │
└───────────────┘      └─────────────┘  └─────────────────────────┘
```

Three services, two datastores. The web app talks only to the API. The API is the only
thing that talks to the AI service.

## Request lifecycle

A `POST /api/posts` from a signed-in student:

```
1. ExceptionHandlingMiddleware      registered FIRST, so it wraps everything after it
2. CORS                             origin checked against the configured allowlist
3. Authentication                   JWT validated; claims become the ClaimsPrincipal
4. Authorization                    [Authorize] on ApiControllerBase; 401 if anonymous
5. PostsController.Create           binds the body, dispatches — no logic
6. ValidationBehaviour              runs every registered validator; throws on failure
7. CreatePostCommandHandler         reads the author from ICurrentUser
                                    loads/checks group membership
                                    calls Post.Create — invariants enforced here
                                    stages via IPostRepository.Add
                                    commits via IUnitOfWork.SaveChangesAsync
8. 201 Created + PostDto
```

Anything thrown along the way unwinds to step 1 and becomes an RFC 9457 problem
document:

| Thrown | Status |
| --- | --- |
| `ValidationException` | 400, with `errors` keyed by field |
| `DomainException` | 400 |
| `NotFoundException` | 404 |
| `ForbiddenAccessException` | 403 |
| `NotImplementedException` | **501** — scaffolded but not built yet |
| anything else | 500, generic body, detail logged server-side only |

The 501 mapping is Phase 1 scaffolding: it tells a contributor (and the frontend) that
an endpoint is routed and reachable but its handler is still a stub, which is far more
useful than a generic 500.

## The four layers

The rule: **dependencies point inward.**

| Layer | Owns | Depends on |
| --- | --- | --- |
| Domain | Entities, enums, invariants, repository interfaces | nothing — zero NuGet packages |
| Application | Commands, queries, handlers, validators, DTOs, `IAiServiceClient` | Domain |
| Infrastructure | `AppDbContext`, EF configs, repository impls, `AiServiceClient`, Redis | Application, Domain |
| Api | Controllers, middleware, `Program.cs`, `CurrentUser` | Application, Infrastructure |

This is enforced by the `.csproj` files, not by convention. `Zee.Domain.csproj`
declares no packages at all.

**Where things live, and why it matters:**

- `IAiServiceClient` is declared in **Application**; `AiServiceClient` (HttpClient,
  JSON, the internal key) lives in **Infrastructure**. Phase 2 replaces the
  implementation without touching a single handler.
- `IUnitOfWork` is in **Domain**; `AppDbContext` implements it. Repositories have no
  `SaveChanges` of their own — if each could commit independently, a handler touching
  two aggregates could half-succeed.
- `ICurrentUser` is declared in **Application**, implemented in **Api** over JWT
  claims. Commands carry no caller identity, so a client cannot express "act as
  another student".

## Data model

```
University 1───* User
    │              │
    │              ├──* Post ──* Comment
    │              ├──* Achievement ──?── Competition
    │              ├──* GroupMembership *──1 Group
    │              ├──* Rsvp *──1 Event
    │              └──* Message (sender / receiver)
    │
    ├──* Group          (null for GlobalInterest — spans campuses)
    ├──* Competition    (null when open to all campuses)
    ├──* Event          (required — events are physical and campus-bound)
    └──* EmailVerificationCode
```

Three nullable foreign keys carry real meaning:

| Field | Null means |
| --- | --- |
| `Group.UniversityId` | A `GlobalInterest` group — cross-campus by design |
| `Competition.UniversityId` | Open to students at any university |
| `Achievement.CompetitionId` | Not tied to a ZEE competition listing |

`Event.UniversityId` is **not** nullable: events have a location and a wall-clock time
and belong to a campus by nature.

**Identity:** every entity uses a UUIDv7 primary key. Time-ordered, so inserts append
to the right edge of the index instead of fragmenting it the way random v4 GUIDs do.

**Arrays:** `User.Courses`, `User.Interests` and `University.VerifiedEmailDomains` map
to PostgreSQL `text[]` via Npgsql — no join tables. The domains column gets a GIN
index, because every sign-in queries it.

## Authentication

**There are no passwords.** Proving you can read mail at a verified campus domain *is*
the credential.

```
POST /api/auth/request-otp   { email }
   │
   ├─ Extract the domain after the LAST '@'
   ├─ UniversityRepository.FindByEmailDomainAsync — EXACT array match, active only
   ├─ No match  -> 404. This is the gate on the whole platform.
   └─ Match     -> generate 6 digits, HASH it, store with a 10-minute expiry,
                   email the plaintext (Development: log it instead)

POST /api/auth/verify-otp    { email, code }
   │
   ├─ Load newest live code for the address
   ├─ Check consumed -> expired -> attempt cap, BEFORE comparing
   ├─ Constant-time compare of the hashes
   └─ Valid -> create or load the User, issue a JWT with
               NameIdentifier and zee:university_id claims
```

Three properties that must not regress:

1. **Domain matching is exact, never a suffix.** `notmit.edu` ends with `mit.edu`
   under a naive `EndsWith`, which would let anyone registering a lookalike domain
   join that campus. There is a regression test for this.
2. **Only the hash is stored.** A database leak yields no live login codes.
3. **Attempts are capped at 5.** A 6-digit code is only a million possibilities;
   expiry alone is not enough. Once the cap is hit the code is dead even if the next
   guess is correct.

**University onboarding is manual and owner-reviewed.** There is no self-serve
endpoint. A new `University` row with its verified domains is inserted deliberately —
this is the single choke point stopping someone registering `gmail.com` as a campus.

## The feed

**Phase 1 is reverse-chronological and nothing else.** `ORDER BY CreatedAt DESC, Id
DESC`. No engagement signal, no personalisation, no score column on `Post`.

Paging is **keyset**, not `OFFSET`:

```sql
WHERE (created_at, id) < (@cursorCreatedAt, @cursorId)
ORDER BY created_at DESC, id DESC
LIMIT @limit
```

Two reasons. Correctness: with `OFFSET 20`, anything posted between page 1 and page 2
shifts every row down, so the reader sees an item twice and misses another. Cost:
`OFFSET n` makes PostgreSQL walk and discard n rows, so deep pages get linearly slower.

The `Id` component is not optional — a timestamp alone is not a total order, and an
unstable sort drops or repeats rows at page boundaries.

The cursor goes to clients base64url-encoded and documented as **opaque**. That is for
API evolution, not security: it stops clients constructing their own and freezing the
ordering key as public API. It carries no secrets.

**Visibility filtering happens in SQL**, not after materialising. Filtering in memory
applies the limit before the filter, so pages come back short and eventually empty
while posts still exist.

## The AI boundary

```
browser ──X──► ai-service        never. It has no public route.
browser ─────► api ─────────────► ai-service
                    X-Internal-Key
```

The contract, mirrored on both sides
(`Zee.Application/Common/Ai/AiContracts.cs` ↔ `app/schemas/`):

| Endpoint | Returns |
| --- | --- |
| `POST /api/chatbot/ask` | `{ answer, sources[] }` |
| `GET /api/recommendations/{user_id}` | `{ matches: [{ user_id, score, reason }] }` |
| `GET /api/feed/{user_id}` | `{ posts: [{ post_id, score }] }` |

Three properties hold from day one:

- **Every call may fail, and every caller copes.** `IAiServiceClient` returns fallbacks
  rather than throwing: the chatbot degrades to an "unavailable" message, matching and
  ranking to empty lists. A student must never see the platform break because an
  optional enrichment service is down. The timeout is 5 seconds for the same reason.
- **The AI service returns ids, never content.** It does not own posts and must not
  become a second source of truth for them. The API hydrates ids from PostgreSQL,
  which is also where visibility rules are applied.
- **Citations are in the contract already.** A campus assistant that says "the deadline
  is Friday" with no way to check where that came from is worse than useless when it is
  wrong, and retrofitting sources into a UI built without them is far more work.

See [AI_DESIGN.md](AI_DESIGN.md) for the Phase 2 plan.

## Known gaps

Honest list of what Phase 1 does not do. Several are good contributions.

| Gap | Notes |
| --- | --- |
| **Most handlers are stubs** | By design — see CONTRIBUTING.md. They return 501. |
| **Integration tests use the in-memory provider** | Not a relational database: ignores unique indexes and foreign keys, and cannot verify a query translates to SQL. Right for "does the pipeline work", wrong for "is this query correct". Moving to Testcontainers with a real pgvector Postgres is a wanted contribution. |
| **No EF Core migrations yet** | The configurations are stubs, so the schema comes from EF conventions. Generate the initial migration once they are implemented. |
| **No rate limiting** | `request-otp` especially needs it — without it that endpoint is an open mail relay pointed at any institutional inbox. `IEmailVerificationCodeRepository.CountIssuedSinceAsync` exists to back it. |
| **No email sending** | Development logs the OTP instead. A real provider is needed before any pilot. |
| **No service worker** | The PWA installs but is not offline-capable. Caching an authenticated feed needs care — a shared device could serve one student's posts to the next. |
| **No moderation or reporting** | A social platform needs both before real users. Not modelled at all yet. |
| **No structured logging or tracing** | Serilog plus OpenTelemetry would be the obvious addition. |

# AI Design

> **Status: Phase 2, not built.** Everything below is a plan, not a description of
> working code. Phase 1 ships the contract, the transport and mocked responses.
>
> **`/apps/ai-service` is owner-maintained.** See [../CONTRIBUTING.md](../CONTRIBUTING.md#what-is-reserved).

## Why the AI is deliberately absent from Phase 1

The three AI features ZEE wants — a campus assistant, student matching, and a ranked
feed — all need something Phase 1 does not have: **real students doing real things.**

- A **RAG chatbot** with no campus content to retrieve has nothing to answer from.
- A **matching model** with no interaction history can only compare profile fields,
  which is a `WHERE` clause wearing a costume.
- A **ranked feed** with no engagement signal cannot be evaluated. You cannot tell a
  good ranker from a bad one without knowing what people actually engaged with.

Building them now would mean tuning against imagined behaviour, then discovering the
assumptions were wrong. So Phase 1 ships the *seams* — real HTTP, real auth, real
schemas — and Phase 2 fills them in against real data.

## What Phase 1 guarantees

The boundary is finished even though the logic is not:

- Three endpoints, routed, authenticated with `X-Internal-Key`, and typed.
- Schemas mirrored on both sides — `Zee.Application/Common/Ai/AiContracts.cs` and
  `apps/ai-service/app/schemas/`.
- `IAiServiceClient` in the Application layer, its HTTP implementation in
  Infrastructure. **Phase 2 changes the Python side only.**
- Failure handling already in place: every method returns a fallback rather than
  throwing.
- pgvector enabled on the database from the first `docker compose up`.

## The contract

### `POST /api/chatbot/ask`

```jsonc
// request
{ "question": "When is the CS3300 project due?", "user_id": "018f..." }

// response
{
  "answer": "The CS3300 project is due Friday 14 March at 17:00.",
  "sources": [
    { "id": "post_018f...", "title": "CS3300 — project deadline", "url": "/posts/018f..." }
  ]
}
```

`sources` may be empty but is never null. **Citations are part of the contract from day
one**, not a Phase 2 addition — an assistant that asserts a deadline with no way to
check it is worse than useless when it is wrong.

### `GET /api/recommendations/{user_id}`

```jsonc
{
  "matches": [
    { "user_id": "018f...", "score": 0.87,
      "reason": "Also taking CS3300 and interested in robotics" }
  ]
}
```

Descending score, `0.0`–`1.0`. `reason` is **required**. An unexplained "you should
meet this person" is uncomfortable and unactionable, and forcing an explanation keeps
the model honest about whether its matches make sense.

### `GET /api/feed/{user_id}`

```jsonc
{ "posts": [ { "post_id": "018f...", "score": 12.4 } ] }
```

**Ids and scores only, never post content.** The AI service does not own posts and must
not become a second source of truth. The API hydrates these via
`IPostRepository.GetByIdsAsync`, which preserves rank order — and, critically, applies
visibility rules in the service that actually knows them.

## Phase 2 plan

### 1. RAG chatbot

```
question ──► embed ──► pgvector similarity search ──► top-k chunks
                              │                            │
                    filtered by what THIS                  ▼
                    student may see                   prompt + context
                                                            │
                                                            ▼
                                                    answer + citations
```

Corpus: posts, group descriptions, event and competition listings, and campus content
a university chooses to supply.

**The hard part is not retrieval, it is authorisation.** Embeddings do not carry
permissions. A naive vector search over all posts will happily surface a private group's
content to someone who is not in it. Retrieval must be filtered by the asking student's
visibility *before* ranking, not after — filtering afterwards silently returns fewer
results than requested and leaks through timing and result counts.

Open questions:
- Chunking strategy. Posts are short; event descriptions are not.
- Whether to embed per-campus or globally. Per-campus keeps the index small and
  authorisation simpler; global allows cross-campus answers.
- Refusal behaviour. The assistant must say "I don't know" rather than guess a deadline.

### 2. Student matching

Signals available: shared courses, overlapping interests, same university, group
co-membership, competition participation, interaction history.

Start with the boring version — weighted overlap on courses and interests — and only
reach for embeddings if it is measurably insufficient. A linear model that can be
explained in one sentence produces good `reason` strings for free; a neural one does
not.

**Constraints:**
- Never recommend a student who has blocked or been blocked by the requester.
- Never surface a student who has opted out of discovery.
- The `reason` must be true. A plausible-sounding but fabricated justification is worse
  than no recommendation.
- Recommendations must not leak profile fields the requester cannot otherwise see.

### 3. Feed ranking

Once engagement data exists:

```
candidates (recent, visible)  ──► features ──► score ──► top-k ids
```

Features: recency, author affinity, group membership, engagement rate, course overlap.

**Design rules:**
- The chronological path stays. Ranking is an enhancement with chronological as the
  fallback whenever the AI service is slow or down — that is already how
  `IAiServiceClient` behaves.
- Recency must stay heavily weighted. A campus feed that surfaces last month's popular
  post over today's event is broken regardless of its metrics.
- Ranking must be explainable enough to debug. "Why is this at the top" needs an
  answer.
- Never optimise engagement alone. That is the objective function that produces
  outrage-maximising feeds, and it would be actively harmful on a campus network.

## Data and privacy

Non-negotiable, and easier to hold now than to retrofit:

- **The AI service reads; it does not write.** No user-facing state lives here.
- **`user_id` always comes from the .NET API's JWT**, never from a client. In Phase 2
  it scopes retrieval to what the student may see, so a client-supplied id would be a
  read of someone else's context.
- **Never send personal data to a third-party model provider** without an explicit,
  informed opt-in. If a hosted model is used, document exactly what is sent.
- **Students can opt out** of matching and ranked ordering.
- **Retention**: embeddings derived from a deleted post must be deleted with it.

## How to help without touching the AI

The AI logic is reserved, but everything it depends on is not:

- Implement the Phase 1 handlers and repositories so real data exists.
- Move the integration tests to Testcontainers, so `pgvector` queries can be tested.
- Add engagement tracking — likes, opens, dwell time — modelled thoughtfully.
- Build the chatbot UI against the mocked endpoint, including source rendering.
- Add the moderation and reporting the platform will need before any of this ships.

Open an issue if you want to discuss the AI direction itself. Design conversation is
welcome; the implementation is reserved.

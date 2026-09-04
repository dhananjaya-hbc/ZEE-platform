# Contributing to ZEE

Thanks for wanting to help build ZEE. This guide covers how the codebase is laid out,
how to pick up work, and the conventions a pull request needs to follow.

**New here? Read [Picking up a task](#picking-up-a-task) first.** ZEE Phase 1 is
scaffolding: the structure, contracts and tests are in place, and most method bodies
are `TODO` stubs waiting to be implemented. That is deliberate — it means there is a
lot of well-specified, self-contained work available.

---

## Contents

- [Ground rules](#ground-rules)
- [What is reserved](#what-is-reserved)
- [Local setup](#local-setup)
- [Picking up a task](#picking-up-a-task)
- [The four layers](#the-four-layers)
- [Worked example: adding "delete a post"](#worked-example-adding-delete-a-post)
- [Branching and commits](#branching-and-commits)
- [Pull request checklist](#pull-request-checklist)
- [Testing](#testing)

---

## Ground rules

- **Be decent to each other.** Assume good faith, review the code and not the person.
- **One concern per pull request.** A PR that implements `Post.Create` *and* refactors
  the feed query is two PRs.
- **Ask before large changes.** Open an issue first if you want to change a public
  contract, add a dependency, or restructure a layer.
- **Never commit secrets.** `.env` files are gitignored; keep it that way. If you push
  a real key by accident, tell a maintainer immediately — rotating it is quick,
  finding out from a stranger is not.

## What is reserved

**`/apps/ai-service` is owner-maintained.** The Phase 2 AI work — the RAG chatbot,
student matching, and feed ranking — is deliberately reserved for the project owner
to build once there is real student usage data to build it against.

What that means in practice:

| Change to `/apps/ai-service` | Welcome? |
| --- | --- |
| Bug fixes, typos, docs | Yes |
| Dockerfile, CI, tooling, dependency bumps | Yes |
| Implementing the Phase 1 **mock** responses | Yes — open an issue first |
| Real AI/ML logic, model integration, embeddings, retrieval | **No** — please open an issue to discuss instead |

The same applies to feed ranking on the .NET side: `GetFeedQueryHandler` must stay
purely chronological in Phase 1. See [docs/AI_DESIGN.md](docs/AI_DESIGN.md) for the
plan and the reasoning.

Everything else — the domain model, the API, the web app, the infrastructure — is
open to contributions.

## Local setup

The whole stack runs in Docker:

```bash
cp infra/.env.example infra/.env      # then edit the two "change-me" secrets
docker compose -f infra/docker-compose.yml up --build
```

To work on one app at a time, start just the datastores and run the app directly:

```bash
docker compose -f infra/docker-compose.yml up postgres redis

cd apps/api        && dotnet run --project src/Zee.Api    # http://localhost:5080
cd apps/web        && npm install && npm run dev          # http://localhost:3000
cd apps/ai-service && pip install -r requirements-dev.txt && uvicorn app.main:app --reload
```

## Picking up a task

The scaffolding marks unimplemented work with `TODO`, and each one carries the
acceptance criteria for that piece. Find them with:

```bash
# Every open stub across the repo
grep -rn "TODO:" apps/ --include=*.cs --include=*.ts --include=*.tsx --include=*.py

# Or run the tests and see what is skipped
cd apps/api && dotnet test
```

A typical stub looks like this:

```csharp
/// TODO: Implement.
/// Acceptance criteria:
///   - Empty or whitespace-only content throws DomainException.
///   - Content longer than MaxContentLength throws.
///   - Visibility == Group REQUIRES groupId; any other visibility REQUIRES null.
///   - Unit tests in tests/Zee.Domain.UnitTests/Entities/PostTests.cs.
public static Post Create(...) => throw new NotImplementedException();
```

The workflow is:

1. **Comment on the issue** (or open one) so nobody duplicates your work.
2. **Read the acceptance criteria** in the `TODO` — it is the specification.
3. **Find the matching test file.** Most stubs already have tests written as skipped
   facts, with names that spell out the expected behaviour.
4. **Implement it, then remove the `Skip`** from those tests and make them pass.
5. **Do not widen the scope.** If you spot something else broken, note it in the PR or
   open a separate issue.

> Tests are `Skip`ped rather than failing so that CI is green on a fresh clone.
> A red baseline makes it impossible for a contributor to tell their own breakage
> from the scaffolding's.

## The four layers

The .NET API uses Clean Architecture. There is one rule and everything else follows
from it:

> **Dependencies point inward. Nothing in an inner layer knows an outer layer exists.**

```
        ┌───────────────────────────────────────────────┐
        │  Api            controllers, middleware,      │
        │                 Program.cs                    │
        │   ┌───────────────────────────────────────┐   │
        │   │  Infrastructure   EF Core, Postgres,  │   │
        │   │                   Redis, HTTP clients │   │
        │   │   ┌───────────────────────────────┐   │   │
        │   │   │  Application   commands,      │   │   │
        │   │   │                queries,       │   │   │
        │   │   │                validators     │   │   │
        │   │   │   ┌───────────────────────┐   │   │   │
        │   │   │   │  Domain   entities,   │   │   │   │
        │   │   │   │           invariants  │   │   │   │
        │   │   │   └───────────────────────┘   │   │   │
        │   │   └───────────────────────────────┘   │   │
        │   └───────────────────────────────────────┘   │
        └───────────────────────────────────────────────┘
```

| Layer | Contains | Must NOT contain | May reference |
| --- | --- | --- | --- |
| **Domain** | Entities, enums, invariants, repository *interfaces* | EF Core, MediatR, ASP.NET, HTTP, **any NuGet package at all** | nothing |
| **Application** | MediatR commands/queries/handlers, FluentValidation validators, DTOs, `IAiServiceClient` | `DbContext`, `HttpClient`, `HttpContext`, SQL | Domain |
| **Infrastructure** | `AppDbContext`, EF configurations, repository *implementations*, `AiServiceClient`, Redis | business rules, request validation | Application, Domain |
| **Api** | Controllers, middleware, `Program.cs`, `CurrentUser` | business logic, EF Core types in signatures, `if` statements of consequence | Application, Infrastructure |

**Why bother?** Two concrete payoffs you will feel immediately:

- Handler tests need no database, no HTTP and no container — every collaborator is an
  interface, so `NSubstitute` stands in for all of them and tests run in microseconds.
- The Phase 2 AI swap touches one class. `IAiServiceClient` lives in Application;
  `AiServiceClient` lives in Infrastructure. Nothing above the interface changes.

**The dependency rule is enforced by the `.csproj` files**, not by convention.
`Zee.Domain.csproj` has no `PackageReference` at all — if you find yourself needing to
add one, what you are building almost certainly belongs in another layer.

## Worked example: adding "delete a post"

Here is a complete feature through all four layers. Follow this shape for any new
endpoint.

### 1. Domain — does a new rule belong here?

Deleting needs no new entity or invariant, so **Domain is untouched**. `IPostRepository`
already has `Remove`.

This step is not a formality. Ask it every time: if the feature has a rule that must
hold *however* the object is reached, it belongs in the entity, not in a handler.

### 2. Application — the use case

Three files, in `src/Zee.Application/Posts/Commands/DeletePost/`:

**`DeletePostCommand.cs`** — the request. Note it carries no caller identity:

```csharp
public sealed record DeletePostCommand(Guid PostId) : IRequest;
```

**`DeletePostCommandValidator.cs`** — shape only:

```csharp
public sealed class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(c => c.PostId)
            .NotEmpty().WithMessage("A post id is required.");
    }
}
```

**`DeletePostCommandHandler.cs`** — the behaviour:

```csharp
public sealed class DeletePostCommandHandler(
    IPostRepository posts,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeletePostCommand>
{
    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var callerId = currentUser.UserId
            ?? throw new ForbiddenAccessException();

        var post = await posts.GetByIdAsync(request.PostId, cancellationToken)
            ?? throw NotFoundException.For("Post", request.PostId);

        // Authorisation lives in the handler, because it depends on the loaded
        // entity. [Authorize] can only answer "are you signed in", never
        // "is this yours".
        if (post.AuthorId != callerId)
        {
            throw new ForbiddenAccessException();
        }

        posts.Remove(post);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

Notice what is *absent*: no validation (the pipeline ran it), no `try`/`catch` (the
middleware maps exceptions), no `DbContext`, no `HttpContext`.

**Neither the handler nor the validator needs registering.** `AddApplication()` scans
the assembly for both.

### 3. Infrastructure — usually nothing

`IPostRepository.Remove` and `GetByIdAsync` already exist, so **Infrastructure is
untouched**. You would only come here to add a new query.

### 4. Api — the endpoint

Add to `PostsController`:

```csharp
[HttpDelete("{id:guid}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
{
    await Sender.Send(new DeletePostCommand(id), cancellationToken);
    return NoContent();
}
```

That is the whole controller change. Three lines, no logic.

### 5. Tests

`tests/Zee.Application.UnitTests/Posts/DeletePostCommandHandlerTests.cs`:

```csharp
[Fact]
public async Task Handle_throws_when_deleting_someone_elses_post()
{
    var owner = Guid.CreateVersion7();
    var attacker = Guid.CreateVersion7();
    _currentUser.UserId.Returns(attacker);
    _posts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
          .Returns(PostFor(owner));

    await Should.ThrowAsync<ForbiddenAccessException>(
        () => CreateHandler().Handle(new DeletePostCommand(Guid.CreateVersion7()), default));

    _posts.DidNotReceive().Remove(Arg.Any<Post>());
}
```

Cover the happy path, the missing post, and the wrong owner. **Authorisation branches
always get a test** — they are the ones that hurt when they regress.

### Summary

| Layer | Files changed |
| --- | --- |
| Domain | 0 |
| Application | 3 new |
| Infrastructure | 0 |
| Api | 1 method |
| Tests | 1 new file |

## Branching and commits

`main` is the released state. `dev` is the integration branch.

**Branch from `dev`, and open your PR against `dev`.**

```bash
git checkout dev && git pull
git checkout -b feat/delete-post
```

Branch names: `feat/…`, `fix/…`, `docs/…`, `test/…`, `chore/…`, `refactor/…`.

Commits follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short imperative summary>

<optional body explaining WHY, not what>
```

Types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`, `build`, `ci`, `perf`.
Scopes: `domain`, `application`, `infrastructure`, `api`, `web`, `ai-service`, `infra`.

```
feat(application): add DeletePost command with owner authorisation
fix(domain): reject whitespace-only post content
docs(api): document the OTP verification flow
```

Write the body when the change is not self-evident. "What" is in the diff; "why" is
not, and it is what a reviewer — or you in six months — actually needs.

## Pull request checklist

Before you open it:

- [ ] Branched from `dev` and targeting `dev`
- [ ] `dotnet build Zee.slnx -warnaserror` passes (or `npm run lint && npm run build`,
      or `ruff check . && pytest`)
- [ ] All tests pass; new behaviour has new tests
- [ ] Any `Skip` you resolved is removed
- [ ] No `.env` file, key, or token in the diff
- [ ] The dependency rule holds — nothing new referenced from an inner layer
- [ ] Conventional commit messages
- [ ] PR description says **why**, and links the issue

CI runs lint, typecheck, build and tests for whichever apps you touched. Path filters
mean a web-only PR does not wait on a .NET build.

## Testing

```bash
cd apps/api        && dotnet test
cd apps/web        && npm run lint && npm run typecheck && npm test
cd apps/ai-service && ruff check . && pytest
```

**What to test where:**

| Kind | Location | Uses a database? |
| --- | --- | --- |
| Entity invariants | `Zee.Domain.UnitTests` | No |
| Handlers, validators | `Zee.Application.UnitTests` | No — substitute the repositories |
| Routing, middleware, DI wiring | `Zee.Api.IntegrationTests` | In-memory provider |

A known gap worth understanding: the integration tests use EF Core's **in-memory
provider**, which is not a relational database. It ignores unique indexes and foreign
keys and cannot tell you whether a LINQ query translates to SQL. So it is right for
"does the pipeline work" and wrong for "is this query correct". Moving to
Testcontainers with a real Postgres is a task we would welcome help with — see
[docs/Architecture.md](docs/Architecture.md).

---

Questions? Open an issue. Nothing here is set in stone, and a good argument for
changing it is a welcome contribution in itself.

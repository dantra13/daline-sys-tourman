# Competition Endpoints — Design

**Date:** 2026-05-28
**Status:** Draft (pending user review)
**Scope:** First HTTP surface for the `Competition` aggregate. Sets the architectural pattern that subsequent aggregates (Structure, Participants, Officials) and disciplines will follow.

## 1. Goals & non-goals

### In scope
- Create, list, and get-by-id endpoints for `Competition` (with its `CompetitionDiscipline` children).
- A new `Sport.Application` project housing use-case handlers organised as vertical slices.
- Wolverine as the in-process message bus dispatching commands/queries from endpoints to handlers.
- A unified HTTP error envelope (RFC 9457 Problem Details + machine-readable `code`) emitted by a single middleware.
- Refactor of `DomainException` in `Sport.Core` to carry a structured `Code` + `Params` instead of free-form strings.
- New architecture tests enforcing the layering of `Sport.Application` and the completeness of the error catalog.

### Out of scope
- Update / delete / lifecycle operations on `Competition` (no domain methods for those exist yet).
- Pagination, filtering, sorting on the list endpoint.
- Idempotency keys on POST.
- Endpoints for other aggregates or for any discipline.
- Authentication / authorization.
- FluentValidation (explicitly rejected — see §5).

## 2. Architecture

### Projects

| Project | Action | References |
|---|---|---|
| `Sport.Application` | **new** | `Sport.Core`, `WolverineFx` |
| `Sport.Application.Tests` | **new** | `Sport.Application`, xUnit, FluentAssertions |
| `Sport.Core` | modify | (refactor `DomainException`) |
| `Sport.Infrastructure` | modify | + `Sport.Application` |
| `Sport.Infrastructure.Tests` | modify | (new repo tests) |
| `Sport.Api` | modify | + `Sport.Application`, + `WolverineFx` |
| `Sport.Api.Tests` | modify | (new endpoint tests) |
| `Sport.Architecture.Tests` | modify | (new layering + catalog rules) |
| `Sport.slnx` | modify | add `Sport.Application`, `Sport.Application.Tests` |

### Dependency graph

```
Sport.Api ──▶ Sport.Application ──▶ Sport.Core
   │                ▲
   └──▶ Sport.Infrastructure ──┘
```

`Sport.Application` stays persistence-agnostic: it defines repository interfaces, `Sport.Infrastructure` implements them against `SportDbContext`.

### Pattern choice — handlers as vertical slices via Wolverine

- One folder per use case under `Sport.Application/Features/<Aggregate>/<UseCase>/`.
- Each handler is a `static class` with a `public static Task<TResponse> Handle(...)` method, auto-discovered by Wolverine conventions.
- Endpoints in `Sport.Api` inject `IMessageBus` and call `bus.InvokeAsync<TResponse>(command, ct)`.
- We do **not** adopt `WolverineFx.Http` (handler-as-endpoint) yet — keeping endpoints explicit preserves the existing Scalar/OpenAPI surface and gives us a clear DTO boundary.

## 3. HTTP contracts

### Routes

| Method | Route | Wolverine message | Success |
|---|---|---|---|
| `POST` | `/competitions` | `CreateCompetition` | `201 Created` + `Location: /competitions/{id}` |
| `GET`  | `/competitions` | `ListCompetitions`  | `200 OK` |
| `GET`  | `/competitions/{id}` | `GetCompetition` | `200 OK` |

### `POST /competitions` — Request body

```json
{
  "code": "jud-2026",
  "name": "Judo Open 2026",
  "dates": { "start": "2026-08-01", "end": "2026-08-05" },
  "disciplines": [
    { "code": "FBL", "genders": ["M", "F"] },
    { "code": "ATH", "genders": ["M"] }
  ]
}
```

`code` is lowercase kebab-case (a–z, 0–9, `-`), enforced by `CompetitionCode` Vogen validation.

### `POST /competitions` — Response 201 / `GET /competitions/{id}` — Response 200

```json
{
  "id": "c-019301...",
  "code": "jud-2026",
  "name": "Judo Open 2026",
  "dates": { "start": "2026-08-01", "end": "2026-08-05" },
  "disciplines": [
    { "id": "cd-...", "code": "FBL", "genders": ["M", "F"] },
    { "id": "cd-...", "code": "ATH", "genders": ["M"] }
  ]
}
```

### `GET /competitions` — Response 200

```json
{ "items": [ /* CompetitionDto[] */ ] }
```

The `{ items }` envelope is adopted from day one so adding pagination metadata later (`page`, `pageSize`, `total`) does not break the contract.

### Status codes for errors

| Case | Status | Example `code` |
|---|---|---|
| Body malformed / wrong type | `400` | `request.malformed` |
| Missing required field / invalid format | `422` | `competition.name_required` |
| Domain invariant violated (`I-COMP-*`) | `422` | `competition.duplicate_discipline` |
| Unique `code` already exists | `409` | `competition.code_already_exists` |
| `GET /competitions/{id}` not found | `404` | `competition.not_found` |
| Unhandled exception | `500` | `internal.unexpected` |

Rationale: `422` for semantically invalid input (well-formed but rejected), `400` only for parse failures. Aligned with RFC 9110.

## 4. Error envelope

All non-2xx responses share one shape: RFC 9457 Problem Details extended with `code`, `errors[]`, and `traceId`.

```json
{
  "type": "https://daline.sys/errors/competition.duplicate_discipline",
  "title": "Discipline already added",
  "status": 422,
  "detail": "Discipline 'FBL' appears more than once.",
  "code": "competition.duplicate_discipline",
  "errors": [
    {
      "code": "competition.duplicate_discipline",
      "target": "disciplines[1].code",
      "params": { "discipline": "FBL" }
    }
  ],
  "traceId": "00-a1b2c3-..."
}
```

- `code` is the machine-readable, stable identifier the client uses for i18n routing.
- `errors[]` always carries the structured failures (single entry for domain errors, multiple for surface validation).
- `detail` is human fallback only — never the source of truth.
- `traceId` comes from `Activity.Current?.Id` or the incoming `traceparent` header.
- `Content-Type: application/problem+json`.

### Client-facing code naming

Format: `<aggregate>.<snake_case>`.

- `competition.name_required` (← internal `I-COMP-5`)
- `competition.code_invalid` (surface — Vogen rejection)
- `competition.code_already_exists` (surface — uniqueness conflict)
- `competition.date_range_invalid` (← internal `I-DR-1`)
- `competition.disciplines_required` (← internal `I-COMP-1`)
- `competition.discipline_not_registered` (← internal `I-COMP-2`)
- `competition.duplicate_discipline` (← internal `I-COMP-3`)
- `competition.gender_not_supported` (← internal `I-COMP-4`)
- `competition.not_found`
- `request.malformed` (parse-level, emitted by the binder override — see §8)
- `internal.unexpected`

Internal `I-*` IDs remain as the domain's invariant identity (documented in the domain exception's XML doc, traceable to the domain spec). The client never sees them.

Codes with no `←` mapping are **surface codes**: produced directly by the handler (`ValidationException`) or by middleware. They are not part of `DomainErrorCatalog` and are not covered by the catalog architecture test (§9).

## 5. Validation strategy

**FluentValidation is not used.** Reasons:

- It produces its own `ValidationResult` shape; without a custom adapter the response envelope diverges from domain errors and clients see two formats.
- It duplicates domain invariants that `Competition.Create` already enforces.
- It is an extra dependency for a small surface of guard clauses.

Instead:

1. **Surface guards inside each handler.** The handler accumulates `ValidationFailure` records (`code`, `target`, `params`) and throws `ValidationException(failures)` if any are present, before touching the domain.
2. **Domain invariants** raised by `Competition.Create` throw `DomainException` carrying their `Code` (e.g. `I-COMP-3`) and `Params`.
3. **One middleware** (`ExceptionHandlingMiddleware`) translates `ValidationException`, `DomainException`, and `NotFoundException` to the unified envelope. Nothing else writes the error shape.

If a future feature has 15+ fields with complex cross-field rules, FluentValidation may be reintroduced for that feature only, behind an adapter that emits the same envelope.

### Refactor — `Sport.Core/Shared/DomainException`

```csharp
public sealed class DomainException : Exception
{
    public string Code { get; }
    public IReadOnlyDictionary<string, object?> Params { get; }

    public DomainException(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? @params = null)
        : base(message)
    {
        Code = code;
        Params = @params ?? new Dictionary<string, object?>();
    }
}
```

The five existing call sites in `Competition.Create` are updated to pass `Code` (`I-COMP-1` through `I-COMP-4` and a code for `Name required`) plus `Params` with the relevant values (discipline code, gender, etc.). Other aggregates that may throw `DomainException` later follow the same pattern.

## 6. `Sport.Application` — internals

### Layout

```
apps/api/src/Sport.Application/
├── Sport.Application.csproj
├── Abstractions/
│   ├── ICompetitionRepository.cs
│   └── IUnitOfWork.cs
├── Common/
│   ├── ValidationFailure.cs        (record Code, Target, Params)
│   ├── ValidationException.cs      (carries IReadOnlyList<ValidationFailure>)
│   └── NotFoundException.cs        (carries Code + Params)
└── Features/
    └── Competitions/
        ├── CompetitionDto.cs
        ├── CreateCompetition/
        │   ├── CreateCompetition.cs
        │   └── CreateCompetitionHandler.cs
        ├── GetCompetition/
        │   ├── GetCompetition.cs
        │   └── GetCompetitionHandler.cs
        └── ListCompetitions/
            ├── ListCompetitions.cs
            └── ListCompetitionsHandler.cs
```

### Repository / UoW interfaces

```csharp
public interface ICompetitionRepository
{
    Task AddAsync(Competition competition, CancellationToken ct);
    Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct);
    Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct);
    Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
```

`GetByIdAsync` and `ListAsync` return the loaded aggregate (with `Disciplines`). No read-model projections yet — introduced later if list volume justifies it.

### Commands & queries

```csharp
public sealed record CreateCompetition(
    string Code,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<CreateCompetition.DisciplineInput> Disciplines)
{
    public sealed record DisciplineInput(string Code, IReadOnlyList<string> Genders);
}

public sealed record GetCompetition(Guid Id);

public sealed record ListCompetitions();
```

### Handler shape (Create example)

```csharp
public static class CreateCompetitionHandler
{
    public static async Task<CompetitionDto> Handle(
        CreateCompetition cmd,
        ICompetitionRepository repo,
        IDisciplineRegistry registry,
        IUnitOfWork uow,
        CancellationToken ct)
    {
        // 1. Surface guards (null/empty checks on cmd fields) → List<ValidationFailure>.
        // 2. Build VOs via Vogen's generated TryFrom:
        //      CompetitionCode.TryFrom(cmd.Code, out var code)
        //      DisciplineCode.TryFrom(item.Code, out var disc)
        //      GenderCode.TryFrom(g, out var gender)
        //    On failure → add a ValidationFailure with code "competition.code_invalid" etc.
        // 3. Build DateRange via DateRange.Create (throws DomainException on inversion);
        //    catch and translate to ValidationFailure, or pre-check (start <= end) before calling.
        // 4. If any surface failures accumulated → throw ValidationException(failures).
        // 5. Conflict check: ExistsByCodeAsync → ValidationException("competition.code_already_exists")
        //    if present (middleware maps to 409 — single-failure rule, see §8).
        // 6. Competition.Create(...) — may throw DomainException with I-COMP-* code.
        // 7. repo.AddAsync + uow.SaveChangesAsync.
        // 8. return CompetitionDto.From(competition).
    }
}
```

Get and List handlers are trivial: repo query → `NotFoundException("competition.not_found", { id })` if null (Get) → `CompetitionDto.From`.

### DTO mapping

`CompetitionDto.From(Competition)` — static method on the DTO. No AutoMapper. All three endpoints share the same DTO; one place to update when the shape evolves.

### Wolverine wiring (in `Sport.Api/Program.cs`)

```csharp
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(CreateCompetition).Assembly);
});
```

## 7. `Sport.Infrastructure` — persistence

### New files

```
apps/api/src/Sport.Infrastructure/
└── Persistence/
    ├── CompetitionRepository.cs    (internal sealed)
    └── UnitOfWork.cs               (internal sealed)
```

### `CompetitionRepository`

- Constructor takes `SportDbContext`.
- `AddAsync` → `_db.Competitions.AddAsync(...)`.
- `GetByIdAsync` → `_db.Competitions.Include(c => c.Disciplines).FirstOrDefaultAsync(c => c.Id == id, ct)`.
- `ExistsByCodeAsync` → `_db.Competitions.AsNoTracking().AnyAsync(c => c.Code == code, ct)`.
- `ListAsync` → `_db.Competitions.AsNoTracking().Include(c => c.Disciplines).OrderBy(c => c.Code).ToListAsync(ct)`. Stable ordering avoids non-deterministic listings.

### `UnitOfWork`

Thin wrapper over `SportDbContext.SaveChangesAsync(ct)`. Kept separate from the repo so the handler controls the commit point without coupling to EF.

### DI registration

Extend `AddSportInfrastructure(...)`:
```csharp
services.AddScoped<ICompetitionRepository, CompetitionRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

Both implementations are `internal sealed`, consistent with the existing rule for `IEntityTypeConfiguration<T>`.

## 8. `Sport.Api` — endpoints, errors, wiring

### Layout

```
apps/api/src/Sport.Api/
├── Endpoints/
│   └── Competitions/
│       ├── CompetitionEndpoints.cs        (MapCompetitionEndpoints extension)
│       └── CreateCompetitionRequest.cs    (HTTP-shape DTO + ToCommand())
├── ErrorHandling/
│   ├── ProblemDetailsWriter.cs            (builds the envelope)
│   ├── DomainErrorCatalog.cs              (I-COMP-* → client code)
│   └── ExceptionHandlingMiddleware.cs     (single translator)
└── Program.cs                             (UseWolverine + MapCompetitionEndpoints + middleware)
```

### Endpoints

```csharp
public static class CompetitionEndpoints
{
    public static IEndpointRouteBuilder MapCompetitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/competitions").WithTags("Competitions");

        group.MapPost("/", async (CreateCompetitionRequest req, IMessageBus bus, CancellationToken ct) =>
        {
            var dto = await bus.InvokeAsync<CompetitionDto>(req.ToCommand(), ct);
            return Results.Created($"/competitions/{dto.Id}", dto);
        });

        group.MapGet("/", async (IMessageBus bus, CancellationToken ct) =>
            Results.Ok(new { items = await bus.InvokeAsync<IReadOnlyList<CompetitionDto>>(new ListCompetitions(), ct) }));

        group.MapGet("/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            Results.Ok(await bus.InvokeAsync<CompetitionDto>(new GetCompetition(id), ct)));

        return app;
    }
}
```

`CreateCompetitionRequest` is the HTTP-wire DTO, separate from the command. `ToCommand()` translates wire to command. Small indirection now, frees the endpoint from churn as the command evolves.

### `DomainErrorCatalog`

```csharp
internal static class DomainErrorCatalog
{
    public static readonly IReadOnlyDictionary<string, ClientError> Map = new Dictionary<string, ClientError>
    {
        ["I-COMP-1"] = new("competition.disciplines_required",      "At least one discipline is required.", 422),
        ["I-COMP-2"] = new("competition.discipline_not_registered", "Discipline is not registered.",         422),
        ["I-COMP-3"] = new("competition.duplicate_discipline",      "Discipline appears more than once.",    422),
        ["I-COMP-4"] = new("competition.gender_not_supported",      "Gender is not supported by discipline.",422),
        ["I-COMP-5"] = new("competition.name_required",             "Competition name is required.",         422),
        ["I-DR-1"]   = new("competition.date_range_invalid",        "End date must be on or after start.",   422),
    };
}

public sealed record ClientError(string Code, string Title, int Status);
```

Existing throws in `Sport.Core` updated to use these codes:

| File | Throw | Code |
|---|---|---|
| `Competitions/Competition.cs` | `"Competition.Name is required."` | `I-COMP-5` |
| `Competitions/Competition.cs` | `"A Competition must have at least 1 discipline..."` | `I-COMP-1` |
| `Competitions/Competition.cs` | `"Discipline ... is not registered..."` | `I-COMP-2` |
| `Competitions/Competition.cs` | `"Duplicate discipline ..."` | `I-COMP-3` |
| `Competitions/Competition.cs` | `"Gender ... is not supported..."` | `I-COMP-4` |
| `Competitions/DateRange.cs` | `"DateRange.Start must be on or before DateRange.End."` | `I-DR-1` |

### Middleware

`ExceptionHandlingMiddleware` is registered first in the pipeline. It handles:

- `ValidationException` → `422` (or `409` when the only failure code is `competition.code_already_exists`), envelope with `errors[]` populated from `Failures`.
- `DomainException` → look up `Code` in `DomainErrorCatalog`; build envelope with the client-facing code, title, status, and `params`. If the code is missing from the catalog → log + return `500 internal.unexpected` (the architecture test prevents this in practice).
- `NotFoundException` → `404` with envelope.
- Anything else → `500 internal.unexpected` (no stack trace in the body, logged server-side).

The framework's `app.UseExceptionHandler` and `AddProblemDetails()` are **not** used. One producer of the error shape, no invisible collaborators.

### 400 `request.malformed`

Body parse failures (invalid JSON, type mismatch, missing required body) happen in the model binder, **before** the middleware sees the request. Two options for the implementation plan to choose between:

1. Configure `JsonOptions` + a custom `InvalidModelStateResponseFactory` (or a Minimal API binder-failure hook) that emits the same envelope shape with `code = "request.malformed"`.
2. Register an `IProblemDetailsService` implementation that intercepts framework-generated `ProblemDetails` (400/415) and rewrites them to the envelope.

Either is acceptable as long as malformed-body responses end up with the same `application/problem+json` shape as the rest. The decision is deferred to the implementation plan.

### Program.cs wiring (order matters)

```csharp
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(CreateCompetition).Assembly);
});

// after Build():
app.UseMiddleware<ExceptionHandlingMiddleware>();   // first
app.MapCompetitionEndpoints();
```

## 9. Tests

### `Sport.Architecture.Tests` (new rules)

- `Sport_Application_does_not_reference_EntityFrameworkCore`
- `Sport_Application_does_not_reference_Sport_Infrastructure`
- `Sport_Application_does_not_reference_any_discipline_module`
- `Sport_Core_does_not_reference_Sport_Application`
- `Sport_Core_does_not_reference_Wolverine`
- `Sport_Infrastructure_references_Sport_Application`
- `Implementations_of_Sport_Application_Abstractions_in_Infrastructure_are_internal_sealed`
- `DomainErrorCatalog_covers_every_domain_code_thrown_by_Sport_Core` — drives all known failure paths (`Competition.Create` × each invariant + `DateRange.Create` inversion) and asserts every observed `DomainException.Code` is a key in `DomainErrorCatalog.Map`. Surface codes (handler-emitted `ValidationFailure`) are out of scope for this test by design.

### `Sport.Application.Tests` (new project)

Handler unit tests with fake `ICompetitionRepository` (`InMemoryCompetitionRepository`) and `IUnitOfWork`:

- `CreateCompetitionHandler_happy_path_returns_dto_and_persists`
- `CreateCompetitionHandler_with_blank_name_throws_ValidationException_with_code`
- `CreateCompetitionHandler_with_duplicate_code_throws_ValidationException_with_code_already_exists`
- `CreateCompetitionHandler_propagates_DomainException_from_aggregate`
- `GetCompetitionHandler_throws_NotFoundException_when_missing`
- `GetCompetitionHandler_returns_dto_when_present`
- `ListCompetitionsHandler_returns_empty_items_when_none`
- `ListCompetitionsHandler_returns_items_in_code_order`

### `Sport.Infrastructure.Tests` (additions)

Postgres round-trip tests (existing pattern):

- `CompetitionRepository_AddAsync_persists_with_disciplines`
- `CompetitionRepository_GetByIdAsync_returns_null_when_missing`
- `CompetitionRepository_ExistsByCodeAsync_true_when_present_false_otherwise`
- `UnitOfWork_SaveChangesAsync_commits_pending_changes`

### `Sport.Api.Tests` (additions, `WebApplicationFactory`)

- `POST_competitions_creates_and_returns_201_with_location`
- `POST_competitions_with_blank_name_returns_422_with_code_name_required`
- `POST_competitions_with_duplicate_disciplines_returns_422_with_code_duplicate_discipline`
- `POST_competitions_with_unregistered_discipline_returns_422_with_code_discipline_not_registered`
- `POST_competitions_with_duplicate_code_returns_409_with_code_code_already_exists`
- `POST_competitions_with_malformed_json_returns_400_with_code_request_malformed`
- `GET_competitions_returns_200_with_items_envelope`
- `GET_competitions_id_returns_200_when_found`
- `GET_competitions_id_returns_404_with_code_not_found`
- `Error_envelope_includes_traceId_matching_response_header`

## 10. Open questions / future work

- **Pagination:** the `{ items }` envelope is forward-compatible; add `{ items, page, pageSize, total }` when needed.
- **Idempotency-Key on POST:** deferred. Today, duplicate `code` returns `409`.
- **Read-model projections:** if `ListAsync` becomes expensive, introduce a projection separate from the aggregate; the `ICompetitionRepository` interface absorbs the change without touching handlers.
- **WolverineFx.Http adoption:** revisit once the endpoint surface grows; the current explicit-endpoint pattern can coexist with it per-route.
- **Auth:** orthogonal; this design assumes authenticated/authorised requests once that layer lands.

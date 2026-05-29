# Competition Endpoints Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement `POST /competitions`, `GET /competitions`, and `GET /competitions/{id}` with a vertical-slice application layer dispatched by Wolverine, behind a unified RFC 9457 error envelope.

**Architecture:** New `Sport.Application` project holds use-case records + static handlers + persistence interfaces. `Sport.Infrastructure` implements those interfaces against `SportDbContext`. `Sport.Api` exposes Minimal API endpoints that inject `IMessageBus` and delegate to handlers. A single middleware translates `DomainException` / `ValidationException` / `NotFoundException` to a Problem Details + `code` envelope.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, WolverineFx 4.x, EF Core 10 (Npgsql), Vogen 8, xUnit 2.9, FluentAssertions 7, Respawn (existing test infra).

**Spec:** `docs/superpowers/specs/2026-05-28-competition-endpoints-design.md`

---

## File map

### Created
- `apps/api/src/Sport.Application/Sport.Application.csproj`
- `apps/api/src/Sport.Application/Abstractions/ICompetitionRepository.cs`
- `apps/api/src/Sport.Application/Abstractions/IUnitOfWork.cs`
- `apps/api/src/Sport.Application/Common/ValidationFailure.cs`
- `apps/api/src/Sport.Application/Common/ValidationException.cs`
- `apps/api/src/Sport.Application/Common/NotFoundException.cs`
- `apps/api/src/Sport.Application/Features/Competitions/CompetitionDto.cs`
- `apps/api/src/Sport.Application/Features/Competitions/CreateCompetition/CreateCompetition.cs`
- `apps/api/src/Sport.Application/Features/Competitions/CreateCompetition/CreateCompetitionHandler.cs`
- `apps/api/src/Sport.Application/Features/Competitions/GetCompetition/GetCompetition.cs`
- `apps/api/src/Sport.Application/Features/Competitions/GetCompetition/GetCompetitionHandler.cs`
- `apps/api/src/Sport.Application/Features/Competitions/ListCompetitions/ListCompetitions.cs`
- `apps/api/src/Sport.Application/Features/Competitions/ListCompetitions/ListCompetitionsHandler.cs`
- `apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`
- `apps/api/tests/Sport.Application.Tests/Fakes/InMemoryCompetitionRepository.cs`
- `apps/api/tests/Sport.Application.Tests/Fakes/NoopUnitOfWork.cs`
- `apps/api/tests/Sport.Application.Tests/Features/Competitions/GetCompetitionHandlerTests.cs`
- `apps/api/tests/Sport.Application.Tests/Features/Competitions/ListCompetitionsHandlerTests.cs`
- `apps/api/tests/Sport.Application.Tests/Features/Competitions/CreateCompetitionHandlerTests.cs`
- `apps/api/src/Sport.Infrastructure/Persistence/CompetitionRepository.cs`
- `apps/api/src/Sport.Infrastructure/Persistence/UnitOfWork.cs`
- `apps/api/tests/Sport.Infrastructure.Tests/Persistence/CompetitionRepositoryTests.cs`
- `apps/api/tests/Sport.Infrastructure.Tests/Persistence/UnitOfWorkTests.cs`
- `apps/api/src/Sport.Api/Endpoints/Competitions/CompetitionEndpoints.cs`
- `apps/api/src/Sport.Api/Endpoints/Competitions/CreateCompetitionRequest.cs`
- `apps/api/src/Sport.Api/ErrorHandling/ClientError.cs`
- `apps/api/src/Sport.Api/ErrorHandling/DomainErrorCatalog.cs`
- `apps/api/src/Sport.Api/ErrorHandling/ProblemDetailsWriter.cs`
- `apps/api/src/Sport.Api/ErrorHandling/ExceptionHandlingMiddleware.cs`
- `apps/api/src/Sport.Api/ErrorHandling/MalformedRequestProblemDetails.cs`
- `apps/api/tests/Sport.Api.Tests/CompetitionEndpointsTests.cs`
- `apps/api/tests/Sport.Api.Tests/Fakes/InMemoryTestRepositories.cs`

### Modified
- `apps/api/Sport.slnx` — add Sport.Application, Sport.Application.Tests
- `apps/api/src/Sport.Core/Shared/DomainException.cs` — add `Code`, `Params`
- `apps/api/src/Sport.Core/Competitions/Competition.cs` — pass codes when throwing
- `apps/api/src/Sport.Core/Competitions/DateRange.cs` — pass code when throwing
- `apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj` — ref Sport.Application
- `apps/api/src/Sport.Infrastructure/DependencyInjection.cs` — register repo + UoW
- `apps/api/src/Sport.Api/Sport.Api.csproj` — ref Sport.Application + WolverineFx
- `apps/api/src/Sport.Api/Program.cs` — UseWolverine + middleware + MapCompetitionEndpoints
- `apps/api/tests/Sport.Architecture.Tests/ArchitectureRules.cs` — new layering rules + catalog coverage
- `apps/api/tests/Sport.Core.Tests/CompetitionTests.cs` (or new file) — assert codes on throws
- `apps/api/src/Sport.Core/Sport.Core.csproj` — no change expected

---

## Conventions

- All projects target `net10.0`, `Nullable=enable`, `TreatWarningsAsErrors=true` (inherited from `Directory.Build.props`).
- All commits use Conventional Commits: `feat(scope): ...`, `refactor(scope): ...`, `test(scope): ...`. Scope examples: `core`, `application`, `infra`, `api`, `arch`.
- Test naming: `MethodOrEndpoint_State_ExpectedOutcome`.
- Run the full suite from the API solution: `dotnet test apps/api/Sport.slnx`.
- For tests that hit Postgres, the existing `PostgresFixture` / `PostgresCollection` already manage the container; new tests join the collection.

---

## Phase 1 — `Sport.Core` `DomainException` refactor

### Task 1: Extend `DomainException` with `Code` and `Params`

**Files:**
- Modify: `apps/api/src/Sport.Core/Shared/DomainException.cs`
- Test: `apps/api/tests/Sport.Core.Tests/Shared/DomainExceptionTests.cs` (create)

- [ ] **Step 1: Write the failing test**

Create `apps/api/tests/Sport.Core.Tests/Shared/DomainExceptionTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Shared;

public class DomainExceptionTests
{
    [Fact]
    public void Ctor_with_code_message_params_exposes_all_three()
    {
        var ex = new DomainException(
            code: "I-COMP-3",
            message: "Duplicate discipline.",
            @params: new Dictionary<string, object?> { ["discipline"] = "FBL" });

        ex.Code.Should().Be("I-COMP-3");
        ex.Message.Should().Be("Duplicate discipline.");
        ex.Params.Should().ContainKey("discipline").WhoseValue.Should().Be("FBL");
    }

    [Fact]
    public void Ctor_with_null_params_defaults_to_empty_dictionary()
    {
        var ex = new DomainException("I-X-1", "msg");

        ex.Params.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter FullyQualifiedName~DomainExceptionTests`

Expected: FAIL — compile error (`Code` / `Params` don't exist).

- [ ] **Step 3: Implement**

Replace the body of `apps/api/src/Sport.Core/Shared/DomainException.cs` with:

```csharp
namespace Sport.Core.Shared;

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
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("DomainException.Code is required.", nameof(code));

        Code = code;
        Params = @params ?? new Dictionary<string, object?>();
    }
}
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter FullyQualifiedName~DomainExceptionTests`

Expected: PASS (2 tests).

- [ ] **Step 5: Build the full Core project to surface call-site breakages**

Run: `dotnet build apps/api/src/Sport.Core/Sport.Core.csproj`

Expected: FAIL — `Competition.Create` and `DateRange.Create` call the old 1-arg constructor. They get fixed in Task 2.

- [ ] **Step 6: Commit**

```bash
git add apps/api/src/Sport.Core/Shared/DomainException.cs apps/api/tests/Sport.Core.Tests/Shared/DomainExceptionTests.cs
git commit -m "refactor(core): DomainException carries Code and Params"
```

---

### Task 2: Update `Sport.Core` throw sites to use codes

> **Scope correction:** initial drafting assumed only `Competition.cs` and `DateRange.cs` raised `DomainException`. In fact 41 throw sites exist across the Core, spread over Competitions, Structure, Participants, Officials, and DisciplineRegistry. This task assigns a code to every site so the codebase compiles. Only Competition + DateRange codes are exercised by tests / catalog in this plan; other aggregates' codes are documented here for traceability and will be exercised as their endpoints come online.

**Files (production):**
- Modify: `apps/api/src/Sport.Core/Competitions/Competition.cs`
- Modify: `apps/api/src/Sport.Core/Competitions/CompetitionDiscipline.cs`
- Modify: `apps/api/src/Sport.Core/Competitions/DateRange.cs`
- Modify: `apps/api/src/Sport.Core/Structure/Event.cs`
- Modify: `apps/api/src/Sport.Core/Structure/Phase.cs`
- Modify: `apps/api/src/Sport.Core/Structure/Unit.cs`
- Modify: `apps/api/src/Sport.Core/Structure/Subunit.cs`
- Modify: `apps/api/src/Sport.Core/Participants/Entry.cs`
- Modify: `apps/api/src/Sport.Core/Participants/Person.cs`
- Modify: `apps/api/src/Sport.Core/Participants/Organisation.cs`
- Modify: `apps/api/src/Sport.Core/Participants/Team.cs`
- Modify: `apps/api/src/Sport.Core/Officials/OfficialAssignment.cs`
- Modify: `apps/api/src/Sport.Core/Officials/OfficialScope.cs`
- Modify: `apps/api/src/Sport.Core/DisciplineRegistry/DisciplineRegistry.cs`

**Files (tests, new):**
- Test: `apps/api/tests/Sport.Core.Tests/Competitions/CompetitionInvariantCodesTests.cs`
- Test: `apps/api/tests/Sport.Core.Tests/Competitions/DateRangeInvariantCodesTests.cs`

**Code assignment (clean text + structured Code):**

Keep each message verbatim minus any trailing `(I-X-N)` marker (the marker now lives structurally in `Code`). Use these codes:

| File | Existing message snippet | Code |
|---|---|---|
| `Competitions/Competition.cs` | `Competition.Name is required.` | `I-COMP-5` |
| `Competitions/Competition.cs` | `A Competition must have at least 1 discipline` | `I-COMP-1` |
| `Competitions/Competition.cs` | `Discipline '...' is not registered` | `I-COMP-2` |
| `Competitions/Competition.cs` | `Duplicate discipline '...'` | `I-COMP-3` |
| `Competitions/Competition.cs` | `Gender '...' is not supported by discipline` | `I-COMP-4` |
| `Competitions/CompetitionDiscipline.cs` | `must enable at least one gender` | `I-COMP-6` |
| `Competitions/DateRange.cs` | `DateRange.Start must be on or before DateRange.End.` | `I-DR-1` |
| `Structure/Event.cs:52` | `Event.Name is required.` | `I-STR-2` |
| `Structure/Event.cs:55` | `Gender ... not supported ... (I-STR-1)` | `I-STR-1` |
| `Structure/Event.cs:58` | (from `validation.Error!`, RSC unit) | `I-STR-12` |
| `Structure/Event.cs:67` | (from `validation.Error!`, RSC subunit) | `I-STR-13` |
| `Structure/Event.cs:70` | `Phase.Order ... already exists in Event (I-STR-4)` | `I-STR-4` |
| `Structure/Event.cs:72` | `PhaseCode ... already exists in Event (I-STR-5)` | `I-STR-5` |
| `Structure/Phase.cs:24` | `Phase.Order must be non-negative.` | `I-STR-3` |
| `Structure/Phase.cs:36` | `Unit.PhaseId must match parent Phase.Id.` | `I-STR-9` |
| `Structure/Phase.cs:38` | `UnitCode ... already exists in Phase (I-STR-6)` | `I-STR-6` |
| `Structure/Unit.cs:37` | `UnitCode for a parent of subunits must end with '00' (I-STR-7)` | `I-STR-7` |
| `Structure/Unit.cs:45` | `Subunit.UnitId must match parent Unit.Id.` | `I-STR-10` |
| `Structure/Unit.cs:47` | `SubunitCode ... already exists in Unit (I-STR-8)` | `I-STR-8` |
| `Structure/Unit.cs:54` | `Unit already linked to a discipline-specific entity.` | `I-STR-11` |
| `Structure/Subunit.cs:21` | `Parent Unit RSC must end with '00' (I-STR-7)` | `I-STR-7` |
| `Officials/OfficialAssignment.cs:38` | `ScopeLevel ... not allowed for function (I-OFF-2)` | `I-OFF-2` |
| `Officials/OfficialAssignment.cs:41` | `Function ... Organisation is required (I-OFF-3)` | `I-OFF-3` |
| `Officials/OfficialScope.cs:19` | `OfficialScope.TargetId must not be empty.` | `I-OFF-1` |
| `Participants/Entry.cs:64` | `Type=Team requires TeamId (I-PAR-2)` | `I-PAR-2` |
| `Participants/Entry.cs:66` | `Type=Athlete: TeamId must be null (I-PAR-2)` | `I-PAR-2` |
| `Participants/Entry.cs:68` | `Type=Group: TeamId must be null (I-PAR-2)` | `I-PAR-2` |
| `Participants/Entry.cs:76` | `Composition is required.` | `I-PAR-7` |
| `Participants/Entry.cs:81` | `Athlete entry must contain exactly 1 composition member (I-PAR-1)` | `I-PAR-1` |
| `Participants/Entry.cs:83` | `Team entry must contain at least 1 composition member (I-PAR-1)` | `I-PAR-1` |
| `Participants/Entry.cs:85` | `Group entry must contain at least 2 composition members (I-PAR-1)` | `I-PAR-1` |
| `Participants/Entry.cs:93` | `CompositionMember.Order must be unique within Entry (I-PAR-6)` | `I-PAR-6` |
| `Participants/Entry.cs:95` | `PersonId duplicated within the same Entry composition.` | `I-PAR-8` |
| `Participants/Organisation.cs:20` | `Organisation.Name is required.` | `I-PAR-3` |
| `Participants/Team.cs:22` | `Team.Name is required.` | `I-PAR-4` |
| `Participants/Person.cs:28` | `Person.FamilyName is required.` | `I-PAR-9` |
| `Participants/Person.cs:30` | `Person.FamilyName must be at most 50 characters.` | `I-PAR-10` |
| `Participants/Person.cs:32` | `Person.GivenName must be at most 50 characters.` | `I-PAR-11` |
| `Participants/Person.cs:34` | `Person.IFId must be at most 20 characters.` | `I-PAR-12` |
| `DisciplineRegistry/DisciplineRegistry.cs:12` | `Discipline ... is already registered.` | `I-REG-1` |
| `DisciplineRegistry/DisciplineRegistry.cs:19` | `Discipline ... is not registered.` | `I-REG-2` |

When the existing message contains a trailing `(I-X-N)` marker, strip it (the structured `Code` replaces it). Otherwise leave the message identical.

Add `Params` only for the throws used by `Competition.Create` and `DateRange.Create` (`discipline`, `gender`, `start`, `end`). For other aggregates leave `Params` defaulted (`null` argument); we'll fill them in when those endpoints land.

- [ ] **Step 1: Write the failing test for `Competition`**

Create `apps/api/tests/Sport.Core.Tests/Competitions/CompetitionInvariantCodesTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Competitions;

public class CompetitionInvariantCodesTests
{
    private static readonly DisciplineCode FBL = DisciplineCode.From("FBL");
    private static readonly GenderCode M = GenderCode.From("M");

    private static (CompetitionId id, CompetitionCode code, DateRange dates, IDisciplineRegistry registry) ValidArgs()
    {
        var registry = TestDisciplineRegistry.With(FBL, new[] { M });
        return (
            CompetitionId.From(Guid.NewGuid()),
            CompetitionCode.From("jud-2026"),
            DateRange.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 5)),
            registry);
    }

    [Fact]
    public void Create_with_blank_name_throws_with_code_I_COMP_5()
    {
        var (id, code, dates, registry) = ValidArgs();

        var act = () => Competition.Create(id, code, name: "  ", dates,
            new[] { (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { M }) }, registry);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-COMP-5");
    }

    [Fact]
    public void Create_with_empty_disciplines_throws_with_code_I_COMP_1()
    {
        var (id, code, dates, registry) = ValidArgs();

        var act = () => Competition.Create(id, code, "Name", dates,
            Array.Empty<(DisciplineCode, IReadOnlySet<GenderCode>)>(), registry);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-COMP-1");
    }

    [Fact]
    public void Create_with_unregistered_discipline_throws_with_code_I_COMP_2()
    {
        var (id, code, dates, _) = ValidArgs();
        var emptyRegistry = TestDisciplineRegistry.Empty();

        var act = () => Competition.Create(id, code, "Name", dates,
            new[] { (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { M }) }, emptyRegistry);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("I-COMP-2");
    }

    [Fact]
    public void Create_with_duplicate_disciplines_throws_with_code_I_COMP_3()
    {
        var (id, code, dates, registry) = ValidArgs();

        var act = () => Competition.Create(id, code, "Name", dates,
            new[]
            {
                (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { M }),
                (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { M }),
            }, registry);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-COMP-3");
    }

    [Fact]
    public void Create_with_unsupported_gender_throws_with_code_I_COMP_4()
    {
        var (id, code, dates, registry) = ValidArgs();
        var F = GenderCode.From("F");

        var act = () => Competition.Create(id, code, "Name", dates,
            new[] { (FBL, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { F }) }, registry);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-COMP-4");
    }
}
```

Look at existing Core tests under `apps/api/tests/Sport.Core.Tests/Competitions/` to confirm the `TestDisciplineRegistry` helper exists. If it does **not**, search the test project for a similar helper (e.g. `FakeDisciplineRegistry`) and use that name instead. If no helper exists, add a tiny test-only one in the same folder with `Empty()` and `With(DisciplineCode, IEnumerable<GenderCode>)`.

- [ ] **Step 2: Write the failing test for `DateRange`**

Create `apps/api/tests/Sport.Core.Tests/Competitions/DateRangeInvariantCodesTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Competitions;

public class DateRangeInvariantCodesTests
{
    [Fact]
    public void Create_with_end_before_start_throws_with_code_I_DR_1()
    {
        var act = () => DateRange.Create(new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 1));

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-DR-1");
    }
}
```

- [ ] **Step 3: Run both test files; expect failure**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~InvariantCodes"`

Expected: COMPILE FAIL (old `DomainException` 1-arg ctor missing — actually it doesn't exist anymore, so the production code itself won't build). That's fine — Task 1 already left it in this state.

- [ ] **Step 4: Update `Competition.Create` throws**

In `apps/api/src/Sport.Core/Competitions/Competition.cs`, replace the five throws with:

```csharp
if (string.IsNullOrWhiteSpace(name))
    throw new DomainException("I-COMP-5", "Competition.Name is required.");
if (disciplines is null || disciplines.Count < 1)
    throw new DomainException("I-COMP-1", "A Competition must have at least 1 discipline.");
```

```csharp
if (!seen.Add(disciplineCode))
    throw new DomainException(
        "I-COMP-3",
        $"Duplicate discipline '{disciplineCode.Value}' in competition.",
        new Dictionary<string, object?> { ["discipline"] = disciplineCode.Value });

if (!registry.IsRegistered(disciplineCode))
    throw new DomainException(
        "I-COMP-2",
        $"Discipline '{disciplineCode.Value}' is not registered in the DisciplineRegistry.",
        new Dictionary<string, object?> { ["discipline"] = disciplineCode.Value });

var module = registry.Get(disciplineCode);
foreach (var g in genders)
    if (!module.SupportedGenders.Contains(g))
        throw new DomainException(
            "I-COMP-4",
            $"Gender '{g}' is not supported by discipline '{disciplineCode.Value}'.",
            new Dictionary<string, object?>
            {
                ["discipline"] = disciplineCode.Value,
                ["gender"] = g.ToString(),
            });
```

- [ ] **Step 5: Update `DateRange.Create` throw**

In `apps/api/src/Sport.Core/Competitions/DateRange.cs` replace the throw with:

```csharp
if (start > end)
    throw new DomainException(
        "I-DR-1",
        "DateRange.Start must be on or before DateRange.End.",
        new Dictionary<string, object?>
        {
            ["start"] = start.ToString("yyyy-MM-dd"),
            ["end"] = end.ToString("yyyy-MM-dd"),
        });
```

- [ ] **Step 6: Run the new tests + existing tests**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj`

Expected: ALL PASS. If existing `CompetitionTests` were asserting message strings containing `(I-COMP-N)`, update those assertions to read `.Code` instead.

- [ ] **Step 7: Commit**

```bash
git add apps/api/src/Sport.Core apps/api/tests/Sport.Core.Tests
git commit -m "refactor(core): assign invariant codes to all domain exceptions"
```

---

## Phase 2 — `Sport.Application` skeleton

### Task 3: Create `Sport.Application` project + add Wolverine

**Files:**
- Create: `apps/api/src/Sport.Application/Sport.Application.csproj`
- Create: `apps/api/src/Sport.Application/AssemblyMarker.cs`
- Modify: `apps/api/Sport.slnx`

- [ ] **Step 1: Create the project file**

`apps/api/src/Sport.Application/Sport.Application.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\Sport.Core\Sport.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="WolverineFx" Version="4.10.0" />
  </ItemGroup>

</Project>
```

Note: confirm the latest stable WolverineFx 4.x on NuGet before pinning. If 4.10.0 is unavailable, pick the highest available 4.x and use that — do **not** pin a preview build.

- [ ] **Step 2: Create the assembly marker**

`apps/api/src/Sport.Application/AssemblyMarker.cs`:

```csharp
namespace Sport.Application;

/// <summary>
/// Marker type used to anchor reflection (Wolverine discovery, test assembly references).
/// Do not add behaviour here.
/// </summary>
public sealed class AssemblyMarker;
```

- [ ] **Step 3: Add the project to the solution**

In `apps/api/Sport.slnx`, inside `<Folder Name="/src/">` add (alphabetical order):

```xml
    <Project Path="src/Sport.Application/Sport.Application.csproj" />
```

- [ ] **Step 4: Build the solution**

Run: `dotnet build apps/api/Sport.slnx`

Expected: PASS. Solution rebuilds with the new project, Wolverine restores.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Application apps/api/Sport.slnx
git commit -m "feat(application): scaffold Sport.Application project with Wolverine"
```

---

### Task 4: Create `Sport.Application.Tests` project

**Files:**
- Create: `apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`
- Create: `apps/api/tests/Sport.Application.Tests/Smoke.cs`
- Modify: `apps/api/Sport.slnx`

- [ ] **Step 1: Create the test project file**

`apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="10.0.1" />
    <PackageReference Include="FluentAssertions" Version="7.2.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Sport.Application\Sport.Application.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Add a smoke test**

`apps/api/tests/Sport.Application.Tests/Smoke.cs`:

```csharp
using FluentAssertions;
using Sport.Application;

namespace Sport.Application.Tests;

public class Smoke
{
    [Fact]
    public void AssemblyMarker_is_resolvable()
    {
        typeof(AssemblyMarker).Assembly.GetName().Name.Should().Be("Sport.Application");
    }
}
```

- [ ] **Step 3: Add the project to the solution**

In `apps/api/Sport.slnx`, inside `<Folder Name="/tests/">`, alphabetically place:

```xml
    <Project Path="tests/Sport.Application.Tests/Sport.Application.Tests.csproj" />
```

- [ ] **Step 4: Run tests**

Run: `dotnet test apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`

Expected: PASS (1 test).

- [ ] **Step 5: Commit**

```bash
git add apps/api/tests/Sport.Application.Tests apps/api/Sport.slnx
git commit -m "test(application): scaffold Sport.Application.Tests project"
```

---

### Task 5: Add Application abstractions and common types

**Files:**
- Create: `apps/api/src/Sport.Application/Abstractions/ICompetitionRepository.cs`
- Create: `apps/api/src/Sport.Application/Abstractions/IUnitOfWork.cs`
- Create: `apps/api/src/Sport.Application/Common/ValidationFailure.cs`
- Create: `apps/api/src/Sport.Application/Common/ValidationException.cs`
- Create: `apps/api/src/Sport.Application/Common/NotFoundException.cs`

- [ ] **Step 1: Create `ICompetitionRepository`**

`apps/api/src/Sport.Application/Abstractions/ICompetitionRepository.cs`:

```csharp
using Sport.Core.Competitions;

namespace Sport.Application.Abstractions;

public interface ICompetitionRepository
{
    Task AddAsync(Competition competition, CancellationToken ct);
    Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct);
    Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct);
    Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct);
}
```

- [ ] **Step 2: Create `IUnitOfWork`**

`apps/api/src/Sport.Application/Abstractions/IUnitOfWork.cs`:

```csharp
namespace Sport.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
```

- [ ] **Step 3: Create `ValidationFailure`**

`apps/api/src/Sport.Application/Common/ValidationFailure.cs`:

```csharp
namespace Sport.Application.Common;

public sealed record ValidationFailure(
    string Code,
    string? Target = null,
    IReadOnlyDictionary<string, object?>? Params = null);
```

- [ ] **Step 4: Create `ValidationException`**

`apps/api/src/Sport.Application/Common/ValidationException.cs`:

```csharp
namespace Sport.Application.Common;

public sealed class ValidationException : Exception
{
    public IReadOnlyList<ValidationFailure> Failures { get; }

    public ValidationException(IReadOnlyList<ValidationFailure> failures)
        : base($"Validation failed with {failures.Count} failure(s).")
    {
        if (failures.Count == 0)
            throw new ArgumentException("ValidationException requires at least one failure.", nameof(failures));
        Failures = failures;
    }

    public ValidationException(ValidationFailure failure)
        : this(new[] { failure })
    {
    }
}
```

- [ ] **Step 5: Create `NotFoundException`**

`apps/api/src/Sport.Application/Common/NotFoundException.cs`:

```csharp
namespace Sport.Application.Common;

public sealed class NotFoundException : Exception
{
    public string Code { get; }
    public IReadOnlyDictionary<string, object?> Params { get; }

    public NotFoundException(string code, IReadOnlyDictionary<string, object?>? @params = null)
        : base($"Resource not found ({code}).")
    {
        Code = code;
        Params = @params ?? new Dictionary<string, object?>();
    }
}
```

- [ ] **Step 6: Build**

Run: `dotnet build apps/api/src/Sport.Application/Sport.Application.csproj`

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add apps/api/src/Sport.Application/Abstractions apps/api/src/Sport.Application/Common
git commit -m "feat(application): repository abstractions and common error types"
```

---

## Phase 3 — Vertical slices (TDD)

### Task 6: `CompetitionDto` + `GetCompetition` slice

**Files:**
- Create: `apps/api/src/Sport.Application/Features/Competitions/CompetitionDto.cs`
- Create: `apps/api/src/Sport.Application/Features/Competitions/GetCompetition/GetCompetition.cs`
- Create: `apps/api/src/Sport.Application/Features/Competitions/GetCompetition/GetCompetitionHandler.cs`
- Create: `apps/api/tests/Sport.Application.Tests/Fakes/InMemoryCompetitionRepository.cs`
- Create: `apps/api/tests/Sport.Application.Tests/Fakes/NoopUnitOfWork.cs`
- Create: `apps/api/tests/Sport.Application.Tests/Features/Competitions/GetCompetitionHandlerTests.cs`

- [ ] **Step 1: Create `CompetitionDto`**

`apps/api/src/Sport.Application/Features/Competitions/CompetitionDto.cs`:

```csharp
using Sport.Core.Competitions;

namespace Sport.Application.Features.Competitions;

public sealed record CompetitionDto(
    Guid Id,
    string Code,
    string Name,
    CompetitionDto.DateRangeDto Dates,
    IReadOnlyList<CompetitionDto.DisciplineDto> Disciplines)
{
    public sealed record DateRangeDto(DateOnly Start, DateOnly End);
    public sealed record DisciplineDto(Guid Id, string Code, IReadOnlyList<string> Genders);

    public static CompetitionDto From(Competition c) => new(
        Id: c.Id.Value,
        Code: c.Code.Value,
        Name: c.Name,
        Dates: new DateRangeDto(c.Dates.Start, c.Dates.End),
        Disciplines: c.Disciplines
            .Select(d => new DisciplineDto(
                Id: d.Id.Value,
                Code: d.Code.Value,
                Genders: d.EnabledGenders.Select(g => g.ToString()).ToList()))
            .ToList());
}
```

If a property name on `Competition` / `CompetitionDiscipline` differs (e.g. `Disciplines` vs `_disciplines` exposure, `EnabledGenders` vs another name), inspect the type via `mcp__serena__find_symbol` with `name_path="Competition"` / `"CompetitionDiscipline"` and adjust this mapping accordingly. Do **not** add new public surface to the domain to make the DTO work.

- [ ] **Step 2: Create `GetCompetition` query record**

`apps/api/src/Sport.Application/Features/Competitions/GetCompetition/GetCompetition.cs`:

```csharp
namespace Sport.Application.Features.Competitions.GetCompetition;

public sealed record GetCompetition(Guid Id);
```

- [ ] **Step 3: Create the fakes**

`apps/api/tests/Sport.Application.Tests/Fakes/InMemoryCompetitionRepository.cs`:

```csharp
using Sport.Application.Abstractions;
using Sport.Core.Competitions;

namespace Sport.Application.Tests.Fakes;

public sealed class InMemoryCompetitionRepository : ICompetitionRepository
{
    private readonly List<Competition> _items = new();

    public IReadOnlyList<Competition> Snapshot => _items;

    public Task AddAsync(Competition competition, CancellationToken ct)
    {
        _items.Add(competition);
        return Task.CompletedTask;
    }

    public Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct)
        => Task.FromResult(_items.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct)
        => Task.FromResult(_items.Any(c => c.Code == code));

    public Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Competition>>(_items.OrderBy(c => c.Code.Value).ToList());
}
```

`apps/api/tests/Sport.Application.Tests/Fakes/NoopUnitOfWork.cs`:

```csharp
using Sport.Application.Abstractions;

namespace Sport.Application.Tests.Fakes;

public sealed class NoopUnitOfWork : IUnitOfWork
{
    public int SaveCalls { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveCalls++;
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 4: Write the failing handler tests**

`apps/api/tests/Sport.Application.Tests/Features/Competitions/GetCompetitionHandlerTests.cs`:

```csharp
using FluentAssertions;
using Sport.Application.Common;
using Sport.Application.Features.Competitions.GetCompetition;
using Sport.Application.Tests.Fakes;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Application.Tests.Features.Competitions;

public class GetCompetitionHandlerTests
{
    [Fact]
    public async Task Returns_dto_when_competition_exists()
    {
        var repo = new InMemoryCompetitionRepository();
        var registry = TestRegistry.WithFblM();
        var existing = CompetitionFactory.JudOpen(registry);
        await repo.AddAsync(existing, CancellationToken.None);

        var dto = await GetCompetitionHandler.Handle(
            new GetCompetition(existing.Id.Value), repo, CancellationToken.None);

        dto.Id.Should().Be(existing.Id.Value);
        dto.Code.Should().Be("jud-2026");
        dto.Disciplines.Should().HaveCount(1);
    }

    [Fact]
    public async Task Throws_NotFoundException_with_code_when_missing()
    {
        var repo = new InMemoryCompetitionRepository();

        var act = () => GetCompetitionHandler.Handle(
            new GetCompetition(Guid.NewGuid()), repo, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.Code.Should().Be("competition.not_found");
    }
}
```

Also create a small `TestRegistry` + `CompetitionFactory` in `apps/api/tests/Sport.Application.Tests/Fakes/`:

`apps/api/tests/Sport.Application.Tests/Fakes/TestRegistry.cs`:

```csharp
using Sport.Core.DisciplineRegistry;

namespace Sport.Application.Tests.Fakes;

internal static class TestRegistry
{
    public static IDisciplineRegistry WithFblM()
        => new FakeRegistry(new Dictionary<DisciplineCode, IReadOnlySet<GenderCode>>
        {
            [DisciplineCode.From("FBL")] = new HashSet<GenderCode> { GenderCode.From("M") },
        });

    private sealed class FakeRegistry : IDisciplineRegistry
    {
        private readonly Dictionary<DisciplineCode, FakeModule> _modules;

        public FakeRegistry(IReadOnlyDictionary<DisciplineCode, IReadOnlySet<GenderCode>> map)
            => _modules = map.ToDictionary(kv => kv.Key, kv => new FakeModule(kv.Key, kv.Value));

        public bool IsRegistered(DisciplineCode code) => _modules.ContainsKey(code);
        public IDisciplineModule Get(DisciplineCode code) => _modules[code];
        public IReadOnlyCollection<DisciplineCode> RegisteredCodes => _modules.Keys;
    }

    private sealed class FakeModule : IDisciplineModule
    {
        public FakeModule(DisciplineCode code, IReadOnlySet<GenderCode> supported)
        {
            Code = code;
            SupportedGenders = supported;
        }
        public DisciplineCode Code { get; }
        public IReadOnlySet<GenderCode> SupportedGenders { get; }
    }
}
```

Before using this, open `Sport.Core.DisciplineRegistry.IDisciplineModule` (`mcp__serena__find_symbol` with `name_path="IDisciplineModule"`) and confirm the interface members. If it exposes more than `Code` and `SupportedGenders`, add the missing members to `FakeModule` with trivial implementations. Do the same for `IDisciplineRegistry` (members beyond `IsRegistered`, `Get`, `RegisteredCodes`).

`apps/api/tests/Sport.Application.Tests/Fakes/CompetitionFactory.cs`:

```csharp
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Application.Tests.Fakes;

internal static class CompetitionFactory
{
    public static Competition JudOpen(IDisciplineRegistry registry) =>
        Competition.Create(
            id: CompetitionId.From(Guid.NewGuid()),
            code: CompetitionCode.From("jud-2026"),
            name: "Judo Open 2026",
            dates: DateRange.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 5)),
            disciplines: new[]
            {
                (DisciplineCode.From("FBL"),
                    (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.From("M") }),
            },
            registry: registry);
}
```

- [ ] **Step 5: Run tests; expect COMPILE FAIL (handler doesn't exist)**

Run: `dotnet test apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`

Expected: COMPILE FAIL — `GetCompetitionHandler` undefined.

- [ ] **Step 6: Implement the handler**

`apps/api/src/Sport.Application/Features/Competitions/GetCompetition/GetCompetitionHandler.cs`:

```csharp
using Sport.Application.Abstractions;
using Sport.Application.Common;
using Sport.Core.Competitions;

namespace Sport.Application.Features.Competitions.GetCompetition;

public static class GetCompetitionHandler
{
    public static async Task<CompetitionDto> Handle(
        GetCompetition query,
        ICompetitionRepository repo,
        CancellationToken ct)
    {
        var id = CompetitionId.From(query.Id);
        var competition = await repo.GetByIdAsync(id, ct);
        if (competition is null)
        {
            throw new NotFoundException(
                code: "competition.not_found",
                @params: new Dictionary<string, object?> { ["id"] = query.Id });
        }

        return CompetitionDto.From(competition);
    }
}
```

- [ ] **Step 7: Run tests; expect PASS**

Run: `dotnet test apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`

Expected: PASS (2 new tests + earlier smoke).

- [ ] **Step 8: Commit**

```bash
git add apps/api/src/Sport.Application/Features/Competitions apps/api/tests/Sport.Application.Tests
git commit -m "feat(application): GetCompetition handler with DTO"
```

---

### Task 7: `ListCompetitions` slice (TDD)

**Files:**
- Create: `apps/api/src/Sport.Application/Features/Competitions/ListCompetitions/ListCompetitions.cs`
- Create: `apps/api/src/Sport.Application/Features/Competitions/ListCompetitions/ListCompetitionsHandler.cs`
- Create: `apps/api/tests/Sport.Application.Tests/Features/Competitions/ListCompetitionsHandlerTests.cs`

- [ ] **Step 1: Write the failing tests**

`apps/api/tests/Sport.Application.Tests/Features/Competitions/ListCompetitionsHandlerTests.cs`:

```csharp
using FluentAssertions;
using Sport.Application.Features.Competitions.ListCompetitions;
using Sport.Application.Tests.Fakes;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Application.Tests.Features.Competitions;

public class ListCompetitionsHandlerTests
{
    [Fact]
    public async Task Returns_empty_list_when_no_competitions()
    {
        var repo = new InMemoryCompetitionRepository();

        var result = await ListCompetitionsHandler.Handle(
            new ListCompetitions(), repo, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_competitions_in_code_order()
    {
        var registry = TestRegistry.WithFblM();
        var repo = new InMemoryCompetitionRepository();
        await repo.AddAsync(CompetitionFactory.JudOpen(registry), CancellationToken.None);
        await repo.AddAsync(
            CompetitionFactory.Custom(registry, code: "abc-2026", name: "Earlier"),
            CancellationToken.None);

        var result = await ListCompetitionsHandler.Handle(
            new ListCompetitions(), repo, CancellationToken.None);

        result.Select(c => c.Code).Should().Equal("abc-2026", "jud-2026");
    }
}
```

Extend `CompetitionFactory` with a `Custom` overload (mirrors `JudOpen` but lets the test pick `code` and `name`).

- [ ] **Step 2: Run; expect COMPILE FAIL**

Run: `dotnet test apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`

Expected: FAIL.

- [ ] **Step 3: Implement query + handler**

`apps/api/src/Sport.Application/Features/Competitions/ListCompetitions/ListCompetitions.cs`:

```csharp
namespace Sport.Application.Features.Competitions.ListCompetitions;

public sealed record ListCompetitions;
```

`apps/api/src/Sport.Application/Features/Competitions/ListCompetitions/ListCompetitionsHandler.cs`:

```csharp
using Sport.Application.Abstractions;

namespace Sport.Application.Features.Competitions.ListCompetitions;

public static class ListCompetitionsHandler
{
    public static async Task<IReadOnlyList<CompetitionDto>> Handle(
        ListCompetitions _,
        ICompetitionRepository repo,
        CancellationToken ct)
    {
        var competitions = await repo.ListAsync(ct);
        return competitions.Select(CompetitionDto.From).ToList();
    }
}
```

- [ ] **Step 4: Run; expect PASS**

Run: `dotnet test apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Application/Features/Competitions/ListCompetitions apps/api/tests/Sport.Application.Tests
git commit -m "feat(application): ListCompetitions handler"
```

---

### Task 8: `CreateCompetition` slice (TDD)

**Files:**
- Create: `apps/api/src/Sport.Application/Features/Competitions/CreateCompetition/CreateCompetition.cs`
- Create: `apps/api/src/Sport.Application/Features/Competitions/CreateCompetition/CreateCompetitionHandler.cs`
- Create: `apps/api/tests/Sport.Application.Tests/Features/Competitions/CreateCompetitionHandlerTests.cs`

- [ ] **Step 1: Write the failing tests**

`apps/api/tests/Sport.Application.Tests/Features/Competitions/CreateCompetitionHandlerTests.cs`:

```csharp
using FluentAssertions;
using Sport.Application.Common;
using Sport.Application.Features.Competitions.CreateCompetition;
using Sport.Application.Tests.Fakes;
using Sport.Core.Shared;

namespace Sport.Application.Tests.Features.Competitions;

public class CreateCompetitionHandlerTests
{
    private static CreateCompetition ValidCommand() => new(
        Code: "jud-2026",
        Name: "Judo Open 2026",
        StartDate: new DateOnly(2026, 8, 1),
        EndDate: new DateOnly(2026, 8, 5),
        Disciplines: new[]
        {
            new CreateCompetition.DisciplineInput("FBL", new[] { "M" }),
        });

    [Fact]
    public async Task Happy_path_persists_and_returns_dto()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();

        var dto = await CreateCompetitionHandler.Handle(
            ValidCommand(), repo, registry, uow, CancellationToken.None);

        dto.Code.Should().Be("jud-2026");
        dto.Disciplines.Should().HaveCount(1);
        repo.Snapshot.Should().HaveCount(1);
        uow.SaveCalls.Should().Be(1);
    }

    [Fact]
    public async Task Blank_name_throws_ValidationException_with_code()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        var cmd = ValidCommand() with { Name = "   " };

        var act = () => CreateCompetitionHandler.Handle(cmd, repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Failures.Should().ContainSingle(f => f.Code == "competition.name_required");
        repo.Snapshot.Should().BeEmpty();
        uow.SaveCalls.Should().Be(0);
    }

    [Fact]
    public async Task Invalid_code_format_throws_ValidationException_with_code_invalid()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        var cmd = ValidCommand() with { Code = "INVALID-Has-Upper" };

        var act = () => CreateCompetitionHandler.Handle(cmd, repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Failures.Should().ContainSingle(f => f.Code == "competition.code_invalid");
    }

    [Fact]
    public async Task End_before_start_throws_ValidationException_with_date_range_invalid()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        var cmd = ValidCommand() with
        {
            StartDate = new DateOnly(2026, 8, 10),
            EndDate = new DateOnly(2026, 8, 1),
        };

        var act = () => CreateCompetitionHandler.Handle(cmd, repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Failures.Should().ContainSingle(f => f.Code == "competition.date_range_invalid");
    }

    [Fact]
    public async Task Duplicate_code_throws_ValidationException_with_code_already_exists()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        await repo.AddAsync(CompetitionFactory.JudOpen(registry), CancellationToken.None);

        var act = () => CreateCompetitionHandler.Handle(
            ValidCommand(), repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Failures.Should().ContainSingle(f => f.Code == "competition.code_already_exists");
    }

    [Fact]
    public async Task Domain_invariant_violation_propagates_DomainException()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        // ATH is not in the test registry → I-COMP-2.
        var cmd = ValidCommand() with
        {
            Disciplines = new[]
            {
                new CreateCompetition.DisciplineInput("ATH", new[] { "M" }),
            },
        };

        var act = () => CreateCompetitionHandler.Handle(cmd, repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Code.Should().Be("I-COMP-2");
    }
}
```

- [ ] **Step 2: Run; expect COMPILE FAIL**

Run: `dotnet test apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`

Expected: FAIL.

- [ ] **Step 3: Define the command record**

`apps/api/src/Sport.Application/Features/Competitions/CreateCompetition/CreateCompetition.cs`:

```csharp
namespace Sport.Application.Features.Competitions.CreateCompetition;

public sealed record CreateCompetition(
    string Code,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<CreateCompetition.DisciplineInput> Disciplines)
{
    public sealed record DisciplineInput(string Code, IReadOnlyList<string> Genders);
}
```

- [ ] **Step 4: Implement the handler**

`apps/api/src/Sport.Application/Features/Competitions/CreateCompetition/CreateCompetitionHandler.cs`:

```csharp
using Sport.Application.Abstractions;
using Sport.Application.Common;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Application.Features.Competitions.CreateCompetition;

public static class CreateCompetitionHandler
{
    public static async Task<CompetitionDto> Handle(
        CreateCompetition cmd,
        ICompetitionRepository repo,
        IDisciplineRegistry registry,
        IUnitOfWork uow,
        CancellationToken ct)
    {
        var failures = new List<ValidationFailure>();

        // 1. Surface guards.
        if (string.IsNullOrWhiteSpace(cmd.Name))
            failures.Add(new ValidationFailure("competition.name_required", "name"));

        if (!CompetitionCode.TryFrom(cmd.Code, out var code))
            failures.Add(new ValidationFailure("competition.code_invalid", "code"));

        if (cmd.StartDate > cmd.EndDate)
            failures.Add(new ValidationFailure(
                "competition.date_range_invalid",
                "dates",
                new Dictionary<string, object?>
                {
                    ["start"] = cmd.StartDate.ToString("yyyy-MM-dd"),
                    ["end"]   = cmd.EndDate.ToString("yyyy-MM-dd"),
                }));

        if (cmd.Disciplines is null || cmd.Disciplines.Count == 0)
            failures.Add(new ValidationFailure("competition.disciplines_required", "disciplines"));

        var disciplineInputs = new List<(DisciplineCode Code, IReadOnlySet<GenderCode> Genders)>();
        if (cmd.Disciplines is not null)
        {
            for (var i = 0; i < cmd.Disciplines.Count; i++)
            {
                var item = cmd.Disciplines[i];
                if (!DisciplineCode.TryFrom(item.Code, out var disciplineCode))
                {
                    failures.Add(new ValidationFailure(
                        "competition.discipline_code_invalid",
                        $"disciplines[{i}].code"));
                    continue;
                }

                var genders = new HashSet<GenderCode>();
                for (var g = 0; g < item.Genders.Count; g++)
                {
                    if (!GenderCode.TryFrom(item.Genders[g], out var gender))
                    {
                        failures.Add(new ValidationFailure(
                            "competition.gender_code_invalid",
                            $"disciplines[{i}].genders[{g}]"));
                    }
                    else
                    {
                        genders.Add(gender);
                    }
                }

                disciplineInputs.Add((disciplineCode, genders));
            }
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        // 2. Conflict check.
        if (await repo.ExistsByCodeAsync(code, ct))
        {
            throw new ValidationException(new ValidationFailure(
                "competition.code_already_exists",
                "code",
                new Dictionary<string, object?> { ["code"] = cmd.Code }));
        }

        // 3. Build the aggregate (may throw DomainException).
        var dateRange = DateRange.Create(cmd.StartDate, cmd.EndDate);
        var competition = Competition.Create(
            id: CompetitionId.From(Guid.NewGuid()),
            code: code,
            name: cmd.Name,
            dates: dateRange,
            disciplines: disciplineInputs,
            registry: registry);

        // 4. Persist.
        await repo.AddAsync(competition, ct);
        await uow.SaveChangesAsync(ct);

        return CompetitionDto.From(competition);
    }
}
```

If `DisciplineCode` / `GenderCode` are not Vogen value objects (no `TryFrom`), inspect each via `mcp__serena__find_symbol` and adapt the guard (try/catch on `From`, or use whatever existing validation API they expose).

- [ ] **Step 5: Run; expect PASS**

Run: `dotnet test apps/api/tests/Sport.Application.Tests/Sport.Application.Tests.csproj`

Expected: PASS — all CreateCompetitionHandler tests + earlier tests.

- [ ] **Step 6: Commit**

```bash
git add apps/api/src/Sport.Application/Features/Competitions/CreateCompetition apps/api/tests/Sport.Application.Tests
git commit -m "feat(application): CreateCompetition handler with surface validation"
```

---

## Phase 4 — `Sport.Infrastructure` persistence

### Task 9: Add Sport.Application reference + scaffold persistence types

**Files:**
- Modify: `apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj`
- Create: `apps/api/src/Sport.Infrastructure/Persistence/CompetitionRepository.cs`
- Create: `apps/api/src/Sport.Infrastructure/Persistence/UnitOfWork.cs`

- [ ] **Step 1: Add project reference**

In `apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj`, inside the existing `<ItemGroup>` for `ProjectReference`:

```xml
    <ProjectReference Include="..\Sport.Application\Sport.Application.csproj" />
```

- [ ] **Step 2: Create `CompetitionRepository`**

`apps/api/src/Sport.Infrastructure/Persistence/CompetitionRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Sport.Application.Abstractions;
using Sport.Core.Competitions;

namespace Sport.Infrastructure.Persistence;

internal sealed class CompetitionRepository : ICompetitionRepository
{
    private readonly SportDbContext _db;

    public CompetitionRepository(SportDbContext db) => _db = db;

    public async Task AddAsync(Competition competition, CancellationToken ct)
        => await _db.Competitions.AddAsync(competition, ct);

    public Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct)
        => _db.Competitions
            .Include(c => c.Disciplines)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct)
        => _db.Competitions.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    public async Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct)
        => await _db.Competitions
            .AsNoTracking()
            .Include(c => c.Disciplines)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
}
```

If `SportDbContext.Competitions` is not a public `DbSet<Competition>`, locate it via `mcp__serena__find_symbol` with `name_path="SportDbContext/Competitions"` and adjust accordingly. Same applies to navigation property name `Disciplines`.

- [ ] **Step 3: Create `UnitOfWork`**

`apps/api/src/Sport.Infrastructure/Persistence/UnitOfWork.cs`:

```csharp
using Sport.Application.Abstractions;

namespace Sport.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly SportDbContext _db;

    public UnitOfWork(SportDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
```

- [ ] **Step 4: Build**

Run: `dotnet build apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Infrastructure
git commit -m "feat(infra): CompetitionRepository and UnitOfWork against SportDbContext"
```

---

### Task 10: Register repository and UoW in DI

**Files:**
- Modify: `apps/api/src/Sport.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Update `AddSportInfrastructure`**

In `apps/api/src/Sport.Infrastructure/DependencyInjection.cs`, add the following using:

```csharp
using Sport.Application.Abstractions;
using Sport.Infrastructure.Persistence;
```

Inside `AddSportInfrastructure`, immediately before `return services;`:

```csharp
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
```

- [ ] **Step 2: Build**

Run: `dotnet build apps/api/src/Sport.Infrastructure/Sport.Infrastructure.csproj`

Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add apps/api/src/Sport.Infrastructure/DependencyInjection.cs
git commit -m "feat(infra): register CompetitionRepository and UnitOfWork"
```

---

### Task 11: Persistence integration tests

**Files:**
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/CompetitionRepositoryTests.cs`
- Create: `apps/api/tests/Sport.Infrastructure.Tests/Persistence/UnitOfWorkTests.cs`

Before writing, open one of the existing persistence test files (e.g. `CompetitionPersistenceTests.cs`) with `mcp__serena__find_symbol` to confirm the test class attributes (`[Collection("Postgres")]`, fixture injection). Mirror that pattern exactly.

- [ ] **Step 1: Write `CompetitionRepositoryTests`**

`apps/api/tests/Sport.Infrastructure.Tests/Persistence/CompetitionRepositoryTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Infrastructure.Persistence;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public class CompetitionRepositoryTests : IAsyncLifetime
{
    private readonly SportDbContextFixture _fixture;

    public CompetitionRepositoryTests(SportDbContextFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_persists_with_disciplines()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);
        var registry = InfraTestRegistry.WithFblM();

        var comp = Competition.Create(
            CompetitionId.From(Guid.NewGuid()),
            CompetitionCode.From("jud-2026"),
            "Judo Open 2026",
            DateRange.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 5)),
            new[]
            {
                (DisciplineCode.From("FBL"),
                    (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.From("M") }),
            },
            registry);

        await repo.AddAsync(comp, CancellationToken.None);
        await ctx.SaveChangesAsync();

        await using var verify = _fixture.CreateContext();
        var verifyRepo = new CompetitionRepository(verify);
        var loaded = await verifyRepo.GetByIdAsync(comp.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Code.Value.Should().Be("jud-2026");
        loaded.Disciplines.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);

        var result = await repo.GetByIdAsync(
            CompetitionId.From(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByCodeAsync_true_when_present_false_otherwise()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);
        var registry = InfraTestRegistry.WithFblM();
        var comp = Competition.Create(
            CompetitionId.From(Guid.NewGuid()),
            CompetitionCode.From("dup-test"),
            "T",
            DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)),
            new[]
            {
                (DisciplineCode.From("FBL"),
                    (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.From("M") }),
            },
            registry);
        await repo.AddAsync(comp, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var present = await repo.ExistsByCodeAsync(CompetitionCode.From("dup-test"), CancellationToken.None);
        var absent  = await repo.ExistsByCodeAsync(CompetitionCode.From("missing"),   CancellationToken.None);

        present.Should().BeTrue();
        absent.Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_returns_items_ordered_by_code()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);
        var registry = InfraTestRegistry.WithFblM();

        foreach (var code in new[] { "zzz-late", "aaa-early", "mmm-mid" })
        {
            var comp = Competition.Create(
                CompetitionId.From(Guid.NewGuid()),
                CompetitionCode.From(code),
                code,
                DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)),
                new[]
                {
                    (DisciplineCode.From("FBL"),
                        (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.From("M") }),
                },
                registry);
            await repo.AddAsync(comp, CancellationToken.None);
        }
        await ctx.SaveChangesAsync();

        await using var verify = _fixture.CreateContext();
        var verifyRepo = new CompetitionRepository(verify);
        var list = await verifyRepo.ListAsync(CancellationToken.None);

        list.Select(c => c.Code.Value).Should().Equal("aaa-early", "mmm-mid", "zzz-late");
    }
}
```

Add `InfraTestRegistry` as a sibling helper under `apps/api/tests/Sport.Infrastructure.Tests/Persistence/` mirroring the `TestRegistry.WithFblM()` body from Task 6 (same `FakeRegistry` / `FakeModule` shape). Do **not** share the type across test assemblies — keep one definition per assembly to avoid a coupling between test projects.

- [ ] **Step 2: Write `UnitOfWorkTests`**

`apps/api/tests/Sport.Infrastructure.Tests/Persistence/UnitOfWorkTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Infrastructure.Persistence;
using Sport.Infrastructure.Tests.Fixtures;

namespace Sport.Infrastructure.Tests.Persistence;

[Collection("Postgres")]
public class UnitOfWorkTests : IAsyncLifetime
{
    private readonly SportDbContextFixture _fixture;

    public UnitOfWorkTests(SportDbContextFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SaveChangesAsync_commits_pending_changes()
    {
        await using var ctx = _fixture.CreateContext();
        var repo = new CompetitionRepository(ctx);
        var uow = new UnitOfWork(ctx);
        var registry = InfraTestRegistry.WithFblM();

        var comp = Competition.Create(
            CompetitionId.From(Guid.NewGuid()),
            CompetitionCode.From("uow-test"),
            "UoW",
            DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)),
            new[]
            {
                (DisciplineCode.From("FBL"),
                    (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.From("M") }),
            },
            registry);

        await repo.AddAsync(comp, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);

        await using var verify = _fixture.CreateContext();
        var verifyRepo = new CompetitionRepository(verify);
        (await verifyRepo.GetByIdAsync(comp.Id, CancellationToken.None)).Should().NotBeNull();
    }
}
```

- [ ] **Step 3: Run integration tests**

Run: `dotnet test apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj`

Expected: PASS (4 new tests + existing).

- [ ] **Step 4: Commit**

```bash
git add apps/api/tests/Sport.Infrastructure.Tests/Persistence
git commit -m "test(infra): CompetitionRepository and UnitOfWork integration tests"
```

---

## Phase 5 — `Sport.Api` endpoints and error pipeline

### Task 12: Add Wolverine + Sport.Application references to Sport.Api

**Files:**
- Modify: `apps/api/src/Sport.Api/Sport.Api.csproj`

- [ ] **Step 1: Add references**

In `apps/api/src/Sport.Api/Sport.Api.csproj`, append to the existing `ProjectReference` ItemGroup:

```xml
    <ProjectReference Include="..\Sport.Application\Sport.Application.csproj" />
```

And to the `PackageReference` ItemGroup:

```xml
    <PackageReference Include="WolverineFx" Version="4.10.0" />
```

(Match the version pinned in Task 3.)

- [ ] **Step 2: Build**

Run: `dotnet build apps/api/src/Sport.Api/Sport.Api.csproj`

Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add apps/api/src/Sport.Api/Sport.Api.csproj
git commit -m "feat(api): reference Sport.Application and WolverineFx"
```

---

### Task 13: Error envelope types — `ClientError`, `DomainErrorCatalog`, `ProblemDetailsWriter`

**Files:**
- Create: `apps/api/src/Sport.Api/ErrorHandling/ClientError.cs`
- Create: `apps/api/src/Sport.Api/ErrorHandling/DomainErrorCatalog.cs`
- Create: `apps/api/src/Sport.Api/ErrorHandling/ProblemDetailsWriter.cs`

- [ ] **Step 1: `ClientError`**

`apps/api/src/Sport.Api/ErrorHandling/ClientError.cs`:

```csharp
namespace Sport.Api.ErrorHandling;

public sealed record ClientError(string Code, string Title, int Status);
```

- [ ] **Step 2: `DomainErrorCatalog`**

`apps/api/src/Sport.Api/ErrorHandling/DomainErrorCatalog.cs`:

```csharp
namespace Sport.Api.ErrorHandling;

internal static class DomainErrorCatalog
{
    public static readonly IReadOnlyDictionary<string, ClientError> Map = new Dictionary<string, ClientError>
    {
        ["I-COMP-1"] = new("competition.disciplines_required",      "At least one discipline is required.",   422),
        ["I-COMP-2"] = new("competition.discipline_not_registered", "Discipline is not registered.",          422),
        ["I-COMP-3"] = new("competition.duplicate_discipline",      "Discipline appears more than once.",     422),
        ["I-COMP-4"] = new("competition.gender_not_supported",      "Gender is not supported by discipline.", 422),
        ["I-COMP-5"] = new("competition.name_required",             "Competition name is required.",          422),
        ["I-DR-1"]   = new("competition.date_range_invalid",        "End date must be on or after start.",    422),
    };

    public static ClientError? TryGet(string domainCode)
        => Map.TryGetValue(domainCode, out var error) ? error : null;
}
```

- [ ] **Step 3: `ProblemDetailsWriter`**

`apps/api/src/Sport.Api/ErrorHandling/ProblemDetailsWriter.cs`:

```csharp
using System.Diagnostics;
using System.Text.Json;

namespace Sport.Api.ErrorHandling;

internal static class ProblemDetailsWriter
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public sealed record ErrorEntry(
        string Code,
        string? Target,
        IReadOnlyDictionary<string, object?>? Params);

    public static async Task WriteAsync(
        HttpContext context,
        int status,
        string code,
        string title,
        string detail,
        IReadOnlyList<ErrorEntry> errors,
        CancellationToken ct)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var payload = new
        {
            type = $"https://daline.sys/errors/{code}",
            title,
            status,
            detail,
            code,
            errors = errors.Select(e => new
            {
                code = e.Code,
                target = e.Target,
                @params = e.Params,
            }),
            traceId,
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, payload, JsonOpts, ct);
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build apps/api/src/Sport.Api/Sport.Api.csproj`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Api/ErrorHandling
git commit -m "feat(api): error envelope types and domain error catalog"
```

---

### Task 14: `ExceptionHandlingMiddleware`

**Files:**
- Create: `apps/api/src/Sport.Api/ErrorHandling/ExceptionHandlingMiddleware.cs`

- [ ] **Step 1: Implement**

`apps/api/src/Sport.Api/ErrorHandling/ExceptionHandlingMiddleware.cs`:

```csharp
using Microsoft.Extensions.Logging;
using Sport.Application.Common;
using Sport.Core.Shared;

namespace Sport.Api.ErrorHandling;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException vex)
        {
            await HandleValidationAsync(ctx, vex);
        }
        catch (NotFoundException nfx)
        {
            await ProblemDetailsWriter.WriteAsync(
                ctx,
                status: 404,
                code: nfx.Code,
                title: "Resource not found.",
                detail: nfx.Message,
                errors: new[]
                {
                    new ProblemDetailsWriter.ErrorEntry(nfx.Code, null, nfx.Params),
                },
                ct: ctx.RequestAborted);
        }
        catch (DomainException dex)
        {
            await HandleDomainAsync(ctx, dex);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception in pipeline.");
            await ProblemDetailsWriter.WriteAsync(
                ctx,
                status: 500,
                code: "internal.unexpected",
                title: "An unexpected error occurred.",
                detail: "Unexpected server error.",
                errors: new[]
                {
                    new ProblemDetailsWriter.ErrorEntry("internal.unexpected", null, null),
                },
                ct: ctx.RequestAborted);
        }
    }

    private static async Task HandleValidationAsync(HttpContext ctx, ValidationException vex)
    {
        // 409 if and only if the single failure is the uniqueness conflict.
        var isConflict =
            vex.Failures.Count == 1 &&
            vex.Failures[0].Code == "competition.code_already_exists";

        var status = isConflict ? 409 : 422;
        var top = vex.Failures[0];
        var entries = vex.Failures
            .Select(f => new ProblemDetailsWriter.ErrorEntry(f.Code, f.Target, f.Params))
            .ToArray();

        await ProblemDetailsWriter.WriteAsync(
            ctx,
            status: status,
            code: top.Code,
            title: isConflict ? "Resource conflict." : "Request validation failed.",
            detail: vex.Message,
            errors: entries,
            ct: ctx.RequestAborted);
    }

    private static async Task HandleDomainAsync(HttpContext ctx, DomainException dex)
    {
        var mapped = DomainErrorCatalog.TryGet(dex.Code);
        if (mapped is null)
        {
            // Domain code without catalog entry — shouldn't happen (arch test enforces it).
            await ProblemDetailsWriter.WriteAsync(
                ctx,
                status: 500,
                code: "internal.unexpected",
                title: "An unexpected error occurred.",
                detail: $"Unmapped domain code: {dex.Code}.",
                errors: new[]
                {
                    new ProblemDetailsWriter.ErrorEntry("internal.unexpected", null, null),
                },
                ct: ctx.RequestAborted);
            return;
        }

        await ProblemDetailsWriter.WriteAsync(
            ctx,
            status: mapped.Status,
            code: mapped.Code,
            title: mapped.Title,
            detail: dex.Message,
            errors: new[]
            {
                new ProblemDetailsWriter.ErrorEntry(mapped.Code, null, dex.Params),
            },
            ct: ctx.RequestAborted);
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build apps/api/src/Sport.Api/Sport.Api.csproj`

Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add apps/api/src/Sport.Api/ErrorHandling/ExceptionHandlingMiddleware.cs
git commit -m "feat(api): single exception-handling middleware for the unified envelope"
```

---

### Task 15: `CreateCompetitionRequest` (HTTP DTO) + `CompetitionEndpoints`

**Files:**
- Create: `apps/api/src/Sport.Api/Endpoints/Competitions/CreateCompetitionRequest.cs`
- Create: `apps/api/src/Sport.Api/Endpoints/Competitions/CompetitionEndpoints.cs`

- [ ] **Step 1: Create the HTTP request DTO**

`apps/api/src/Sport.Api/Endpoints/Competitions/CreateCompetitionRequest.cs`:

```csharp
using Sport.Application.Features.Competitions.CreateCompetition;

namespace Sport.Api.Endpoints.Competitions;

public sealed record CreateCompetitionRequest(
    string Code,
    string Name,
    CreateCompetitionRequest.DatesDto Dates,
    IReadOnlyList<CreateCompetitionRequest.DisciplineDto> Disciplines)
{
    public sealed record DatesDto(DateOnly Start, DateOnly End);
    public sealed record DisciplineDto(string Code, IReadOnlyList<string> Genders);

    public CreateCompetition ToCommand() => new(
        Code: Code,
        Name: Name,
        StartDate: Dates.Start,
        EndDate: Dates.End,
        Disciplines: Disciplines
            .Select(d => new CreateCompetition.DisciplineInput(d.Code, d.Genders))
            .ToList());
}
```

- [ ] **Step 2: Create the endpoint mapper**

`apps/api/src/Sport.Api/Endpoints/Competitions/CompetitionEndpoints.cs`:

```csharp
using Sport.Application.Features.Competitions;
using Sport.Application.Features.Competitions.GetCompetition;
using Sport.Application.Features.Competitions.ListCompetitions;
using Wolverine;

namespace Sport.Api.Endpoints.Competitions;

public static class CompetitionEndpoints
{
    public static IEndpointRouteBuilder MapCompetitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/competitions").WithTags("Competitions");

        group.MapPost("/", async (
            CreateCompetitionRequest req,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var dto = await bus.InvokeAsync<CompetitionDto>(req.ToCommand(), ct);
            return Results.Created($"/competitions/{dto.Id}", dto);
        });

        group.MapGet("/", async (IMessageBus bus, CancellationToken ct) =>
        {
            var items = await bus.InvokeAsync<IReadOnlyList<CompetitionDto>>(
                new ListCompetitions(), ct);
            return Results.Ok(new { items });
        });

        group.MapGet("/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var dto = await bus.InvokeAsync<CompetitionDto>(new GetCompetition(id), ct);
            return Results.Ok(dto);
        });

        return app;
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build apps/api/src/Sport.Api/Sport.Api.csproj`

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add apps/api/src/Sport.Api/Endpoints
git commit -m "feat(api): CompetitionEndpoints mapping Wolverine commands and queries"
```

---

### Task 16: Wire Wolverine + middleware + endpoints in `Program.cs`

**Files:**
- Modify: `apps/api/src/Sport.Api/Program.cs`

- [ ] **Step 1: Update `Program.cs`**

Replace the body of `apps/api/src/Sport.Api/Program.cs` with:

```csharp
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Sport.Api.Endpoints.Competitions;
using Sport.Api.ErrorHandling;
using Sport.Application;
using Sport.Core.DisciplineRegistry;
using Sport.Disciplines.ATH;
using Sport.Disciplines.BDM;
using Sport.Disciplines.BKB;
using Sport.Disciplines.BOX;
using Sport.Disciplines.FBL;
using Sport.Disciplines.VBV;
using Sport.Infrastructure;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSportCore()
    .AddDisciplineModule<FblModule>()
    .AddDisciplineModule<BkbModule>()
    .AddDisciplineModule<BdmModule>()
    .AddDisciplineModule<VbvModule>()
    .AddDisciplineModule<BoxModule>()
    .AddDisciplineModule<AthModule>();

builder.Services.AddSportInfrastructure();
builder.Services.AddOpenApi();

builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(AssemblyMarker).Assembly);
});

var app = builder.Build();

app.Services.BuildSportRegistry();

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<SportMigrationRunner>();
    await runner.ApplyAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/",       () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health", () => Results.Ok(new { status = "alive" }));
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.MapCompetitionEndpoints();

app.Run();

public partial class Program { }
```

- [ ] **Step 2: Build**

Run: `dotnet build apps/api/src/Sport.Api/Sport.Api.csproj`

Expected: PASS.

- [ ] **Step 3: Smoke-run the host locally**

Run (foreground): `dotnet run --project apps/api/src/Sport.Api/Sport.Api.csproj`

Wait for `Now listening on:`. Open `http://localhost:5080/openapi/v1.json` in a browser and confirm `/competitions` paths are present. Stop with Ctrl+C.

(Skip this step if Postgres isn't running locally; the migration runner will fail. In that case run `docker compose up -d postgres` first or skip and rely on the integration tests.)

- [ ] **Step 4: Commit**

```bash
git add apps/api/src/Sport.Api/Program.cs
git commit -m "feat(api): wire Wolverine, exception middleware, and competition endpoints"
```

---

### Task 17: 400 `request.malformed` for parse failures

**Files:**
- Create: `apps/api/src/Sport.Api/ErrorHandling/MalformedRequestProblemDetails.cs`
- Modify: `apps/api/src/Sport.Api/Program.cs`

- [ ] **Step 1: Implement the customizer**

`apps/api/src/Sport.Api/ErrorHandling/MalformedRequestProblemDetails.cs`:

```csharp
using Microsoft.AspNetCore.Http;

namespace Sport.Api.ErrorHandling;

internal static class MalformedRequestProblemDetails
{
    public static IServiceCollection AddUnifiedProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(opts =>
        {
            opts.CustomizeProblemDetails = ctx =>
            {
                var status = ctx.ProblemDetails.Status ?? ctx.HttpContext.Response.StatusCode;
                var (code, title) = status switch
                {
                    400 => ("request.malformed",   "Malformed request."),
                    415 => ("request.unsupported", "Unsupported media type."),
                    _   => ("request.invalid",    ctx.ProblemDetails.Title ?? "Invalid request."),
                };

                ctx.ProblemDetails.Type   = $"https://daline.sys/errors/{code}";
                ctx.ProblemDetails.Title  = title;
                ctx.ProblemDetails.Extensions["code"] = code;
                ctx.ProblemDetails.Extensions["errors"] = new[]
                {
                    new { code, target = (string?)null, @params = (IReadOnlyDictionary<string, object?>?)null },
                };
                ctx.ProblemDetails.Extensions["traceId"] =
                    System.Diagnostics.Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
            };
        });

        return services;
    }
}
```

- [ ] **Step 2: Register in `Program.cs`**

In `apps/api/src/Sport.Api/Program.cs`, add `using Sport.Api.ErrorHandling;` (already present) and before `builder.Host.UseWolverine(...)` add:

```csharp
builder.Services.AddUnifiedProblemDetails();
```

And after `var app = builder.Build();` and before the `UseMiddleware<ExceptionHandlingMiddleware>()` line, add:

```csharp
app.UseStatusCodePages();
```

This is what triggers the `IProblemDetailsService` for status-code-only failures (e.g. body parse failures that result in a 400 with no body by default).

- [ ] **Step 3: Build**

Run: `dotnet build apps/api/src/Sport.Api/Sport.Api.csproj`

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add apps/api/src/Sport.Api/ErrorHandling/MalformedRequestProblemDetails.cs apps/api/src/Sport.Api/Program.cs
git commit -m "feat(api): unify 400/415 responses under the same problem-details envelope"
```

---

## Phase 6 — End-to-end API tests

### Task 18: Replace SportDbContext with an in-memory test repository in `Sport.Api.Tests`

The existing `TestApiFactory` uses `UseEnvironment("Testing")` but otherwise spins up the full pipeline including `SportDbContext` (Postgres). For endpoint tests we want fast, isolated runs without Postgres.

**Files:**
- Modify: `apps/api/tests/Sport.Api.Tests/HostSmokeTests.cs` (extract `TestApiFactory` to its own file if it isn't already)
- Create: `apps/api/tests/Sport.Api.Tests/Fakes/InMemoryTestRepositories.cs`
- Create: `apps/api/tests/Sport.Api.Tests/TestApiFactory.cs` (move from HostSmokeTests if collocated)

- [ ] **Step 1: Extract `TestApiFactory` to its own file**

If `TestApiFactory` is in `HostSmokeTests.cs`, move it to `apps/api/tests/Sport.Api.Tests/TestApiFactory.cs` verbatim. Update `HostSmokeTests.cs` to remove the duplicate and add a `using` if needed.

- [ ] **Step 2: Make `TestApiFactory` swap repository registrations**

Edit `TestApiFactory.cs` so it removes the EF-backed `ICompetitionRepository`/`IUnitOfWork` registrations and replaces them with in-memory fakes:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sport.Api.Tests.Fakes;
using Sport.Application.Abstractions;

namespace Sport.Api.Tests;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICompetitionRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<InMemoryStore>();
            services.AddScoped<ICompetitionRepository, InMemoryCompetitionRepository>();
            services.AddScoped<IUnitOfWork, NoopUnitOfWork>();
        });
    }
}
```

Add `using Microsoft.Extensions.DependencyInjection.Extensions;` for `RemoveAll`.

If `SportMigrationRunner` still tries to apply migrations against Postgres in the `Testing` environment, gate the migration block in `Program.cs` by environment (already gated to non-Testing? check) — if not gated, wrap `using (var scope = ...) { await runner.ApplyAsync(); }` with `if (!app.Environment.IsEnvironment("Testing"))`. That keeps production behaviour and lets test runs proceed without Postgres.

- [ ] **Step 3: Implement the in-memory fakes**

`apps/api/tests/Sport.Api.Tests/Fakes/InMemoryTestRepositories.cs`:

```csharp
using Sport.Application.Abstractions;
using Sport.Core.Competitions;

namespace Sport.Api.Tests.Fakes;

public sealed class InMemoryStore
{
    public List<Competition> Competitions { get; } = new();
}

public sealed class InMemoryCompetitionRepository : ICompetitionRepository
{
    private readonly InMemoryStore _store;
    public InMemoryCompetitionRepository(InMemoryStore store) => _store = store;

    public Task AddAsync(Competition competition, CancellationToken ct)
    {
        _store.Competitions.Add(competition);
        return Task.CompletedTask;
    }

    public Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct)
        => Task.FromResult(_store.Competitions.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExistsByCodeAsync(CompetitionCode code, CancellationToken ct)
        => Task.FromResult(_store.Competitions.Any(c => c.Code == code));

    public Task<IReadOnlyList<Competition>> ListAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Competition>>(
            _store.Competitions.OrderBy(c => c.Code.Value).ToList());
}

public sealed class NoopUnitOfWork : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
```

- [ ] **Step 4: Build + smoke**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj --filter HostSmokeTests`

Expected: existing smoke tests still PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/tests/Sport.Api.Tests
git commit -m "test(api): in-memory repositories for endpoint tests"
```

---

### Task 19: Endpoint tests — happy paths and error envelope

**Files:**
- Create: `apps/api/tests/Sport.Api.Tests/CompetitionEndpointsTests.cs`

- [ ] **Step 1: Write the tests**

`apps/api/tests/Sport.Api.Tests/CompetitionEndpointsTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;

namespace Sport.Api.Tests;

public class CompetitionEndpointsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public CompetitionEndpointsTests(TestApiFactory factory) => _client = factory.CreateClient();

    private static object ValidPayload(string code = "jud-2026") => new
    {
        code,
        name = "Judo Open 2026",
        dates = new { start = "2026-08-01", end = "2026-08-05" },
        disciplines = new[] { new { code = "FBL", genders = new[] { "M" } } },
    };

    [Fact]
    public async Task POST_creates_and_returns_201_with_location()
    {
        var resp = await _client.PostAsJsonAsync("/competitions", ValidPayload("e2e-create"));

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location!.ToString().Should().StartWith("/competitions/");

        var body = await resp.Content.ReadFromJsonAsync<CompetitionDtoBody>();
        body!.Code.Should().Be("e2e-create");
        body.Disciplines.Should().HaveCount(1);
    }

    [Fact]
    public async Task POST_with_blank_name_returns_422_with_code_name_required()
    {
        var payload = new
        {
            code = "e2e-blank-name",
            name = "   ",
            dates = new { start = "2026-08-01", end = "2026-08-05" },
            disciplines = new[] { new { code = "FBL", genders = new[] { "M" } } },
        };

        var resp = await _client.PostAsJsonAsync("/competitions", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.name_required");
        body.Errors.Should().ContainSingle(e => e.Code == "competition.name_required");
    }

    [Fact]
    public async Task POST_with_duplicate_disciplines_returns_422_with_code_duplicate_discipline()
    {
        var payload = new
        {
            code = "e2e-dup-disc",
            name = "Dup",
            dates = new { start = "2026-08-01", end = "2026-08-05" },
            disciplines = new[]
            {
                new { code = "FBL", genders = new[] { "M" } },
                new { code = "FBL", genders = new[] { "M" } },
            },
        };

        var resp = await _client.PostAsJsonAsync("/competitions", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.duplicate_discipline");
    }

    [Fact]
    public async Task POST_with_unregistered_discipline_returns_422_with_code_discipline_not_registered()
    {
        var payload = new
        {
            code = "e2e-unreg",
            name = "Unreg",
            dates = new { start = "2026-08-01", end = "2026-08-05" },
            disciplines = new[] { new { code = "ZZZ", genders = new[] { "M" } } },
        };

        var resp = await _client.PostAsJsonAsync("/competitions", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.discipline_not_registered");
    }

    [Fact]
    public async Task POST_with_duplicate_code_returns_409_with_code_code_already_exists()
    {
        (await _client.PostAsJsonAsync("/competitions", ValidPayload("e2e-conflict"))).EnsureSuccessStatusCode();

        var second = await _client.PostAsJsonAsync("/competitions", ValidPayload("e2e-conflict"));

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await second.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.code_already_exists");
    }

    [Fact]
    public async Task POST_with_malformed_json_returns_400_with_code_request_malformed()
    {
        var resp = await _client.PostAsync(
            "/competitions",
            new StringContent("{this is not json", Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("request.malformed");
    }

    [Fact]
    public async Task GET_returns_200_with_items_envelope()
    {
        var resp = await _client.GetAsync("/competitions");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<ItemsEnvelope>();
        body!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GET_by_id_returns_200_when_found()
    {
        var create = await _client.PostAsJsonAsync("/competitions", ValidPayload("e2e-get-found"));
        var dto = await create.Content.ReadFromJsonAsync<CompetitionDtoBody>();

        var resp = await _client.GetAsync($"/competitions/{dto!.Id}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var loaded = await resp.Content.ReadFromJsonAsync<CompetitionDtoBody>();
        loaded!.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task GET_by_id_returns_404_with_code_not_found_when_missing()
    {
        var resp = await _client.GetAsync($"/competitions/{Guid.NewGuid()}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();
        body!.Code.Should().Be("competition.not_found");
    }

    [Fact]
    public async Task Error_envelope_includes_traceId()
    {
        var resp = await _client.GetAsync($"/competitions/{Guid.NewGuid()}");
        var body = await resp.Content.ReadFromJsonAsync<ProblemBody>();

        body!.TraceId.Should().NotBeNullOrWhiteSpace();
    }

    private sealed record CompetitionDtoBody(
        Guid Id,
        string Code,
        string Name,
        DateRangeBody Dates,
        IReadOnlyList<DisciplineBody> Disciplines);

    private sealed record DateRangeBody(DateOnly Start, DateOnly End);
    private sealed record DisciplineBody(Guid Id, string Code, IReadOnlyList<string> Genders);

    private sealed record ItemsEnvelope(IReadOnlyList<CompetitionDtoBody> Items);

    private sealed record ProblemBody(
        string Type,
        string Title,
        int Status,
        string? Detail,
        string Code,
        IReadOnlyList<ProblemErrorEntry> Errors,
        string? TraceId);

    private sealed record ProblemErrorEntry(string Code, string? Target);
}
```

- [ ] **Step 2: Run the tests**

Run: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj`

Expected: ALL PASS.

If the malformed-JSON test fails because the framework returns no body for 400, double-check Task 17 — `app.UseStatusCodePages()` must run before the endpoint mapping and `AddProblemDetails` must be configured. If it still doesn't work, switch to overriding the JSON binder error directly by adding to `Program.cs`:

```csharp
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o => { /* no-op, ensures Json pipeline */ });
```

and write a custom endpoint filter on the `MapPost` that catches `BadHttpRequestException` and renders via `ProblemDetailsWriter`. Document whichever path works in the commit message.

- [ ] **Step 3: Commit**

```bash
git add apps/api/tests/Sport.Api.Tests/CompetitionEndpointsTests.cs
git commit -m "test(api): end-to-end coverage for /competitions endpoints and error envelope"
```

---

## Phase 7 — Architecture rules

### Task 20: Add layering and catalog architecture tests

**Files:**
- Modify: `apps/api/tests/Sport.Architecture.Tests/ArchitectureRules.cs`

- [ ] **Step 1: Append the new rules**

In `apps/api/tests/Sport.Architecture.Tests/ArchitectureRules.cs`, add these usings if missing:

```csharp
using Sport.Application;
using Sport.Application.Abstractions;
using Sport.Api.ErrorHandling;
using Sport.Core.Shared;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
```

Add a static field for the Application assembly:

```csharp
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(AssemblyMarker).Assembly;
```

Append these test methods to the class:

```csharp
    [Fact]
    public void Sport_Application_does_not_reference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should().NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Application_does_not_reference_Sport_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should().NotHaveDependencyOn("Sport.Infrastructure")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Application_does_not_reference_any_discipline_module()
    {
        foreach (var disciplineAsm in DisciplineAssemblies)
        {
            var ns = disciplineAsm.GetName().Name!;
            var result = Types.InAssembly(ApplicationAssembly)
                .Should().NotHaveDependencyOn(ns)
                .GetResult();
            result.IsSuccessful.Should().BeTrue($"Sport.Application must not depend on {ns}");
        }
    }

    [Fact]
    public void Sport_Core_does_not_reference_Sport_Application()
    {
        var result = Types.InAssembly(CoreAssembly)
            .Should().NotHaveDependencyOn("Sport.Application")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Core_does_not_reference_Wolverine()
    {
        var result = Types.InAssembly(CoreAssembly)
            .Should().NotHaveDependencyOn("Wolverine")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Infrastructure_references_Sport_Application()
    {
        var refs = InfrastructureAssembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
        refs.Should().Contain("Sport.Application");
    }

    [Fact]
    public void Implementations_of_Application_Abstractions_in_Infrastructure_are_internal_sealed()
    {
        var abstractionInterfaces = new[]
        {
            typeof(ICompetitionRepository),
            typeof(IUnitOfWork),
        };

        foreach (var iface in abstractionInterfaces)
        {
            var result = Types.InAssembly(InfrastructureAssembly)
                .That().ImplementInterface(iface)
                .Should().BeSealed()
                .And().NotBePublic()
                .GetResult();
            result.IsSuccessful.Should().BeTrue(
                $"All {iface.Name} implementations in Sport.Infrastructure must be internal sealed.");
        }
    }

    [Fact]
    public void DomainErrorCatalog_covers_every_domain_code_thrown_by_Sport_Core()
    {
        var observed = new HashSet<string>();

        // Exercise each known throw path on Competition.Create + DateRange.Create.
        var registry = AssertCatalogTestRegistry.WithFblM();
        var validDates = DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2));
        var validId = CompetitionId.From(Guid.NewGuid());
        var validCode = CompetitionCode.From("ac-1");
        var fbl = DisciplineCode.From("FBL");
        var m = GenderCode.From("M");
        var f = GenderCode.From("F");

        Capture(() => Competition.Create(validId, validCode, "  ", validDates,
            new[] { (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { m }) }, registry), observed);

        Capture(() => Competition.Create(validId, validCode, "N", validDates,
            Array.Empty<(DisciplineCode, IReadOnlySet<GenderCode>)>(), registry), observed);

        Capture(() => Competition.Create(validId, validCode, "N", validDates,
            new[] { (DisciplineCode.From("ZZZ"), (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { m }) }, registry), observed);

        Capture(() => Competition.Create(validId, validCode, "N", validDates,
            new[]
            {
                (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { m }),
                (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { m }),
            }, registry), observed);

        Capture(() => Competition.Create(validId, validCode, "N", validDates,
            new[] { (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { f }) }, registry), observed);

        Capture(() => DateRange.Create(new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 1)), observed);

        observed.Should().BeSubsetOf(DomainErrorCatalog.Map.Keys,
            "every domain code observed in Sport.Core must have a catalog entry");
    }

    private static void Capture(Action act, HashSet<string> observed)
    {
        try { act(); }
        catch (DomainException ex) { observed.Add(ex.Code); }
    }
```

Add `AssertCatalogTestRegistry` as a private nested class (or sibling file) inside `Sport.Architecture.Tests` with the same `FakeRegistry`/`FakeModule` shape used in Task 6. Independent definition per test assembly — no shared helper project.

- [ ] **Step 2: Run architecture tests**

Run: `dotnet test apps/api/tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj`

Expected: ALL PASS.

- [ ] **Step 3: Commit**

```bash
git add apps/api/tests/Sport.Architecture.Tests
git commit -m "test(arch): enforce Application layering and catalog coverage"
```

---

## Phase 8 — Final verification

### Task 21: Full-suite green run

- [ ] **Step 1: Run the entire solution test suite**

Run: `dotnet test apps/api/Sport.slnx`

Expected: ALL test projects PASS, no warnings (warnings-as-errors is on).

- [ ] **Step 2: If anything is red, fix in place and re-run**

Do not commit broken state. Common causes:
- Helper signature drift between test projects (each project has its own `FakeRegistry`/`FakeModule` — keep them in sync if `IDisciplineRegistry` or `IDisciplineModule` evolves).
- `Competition.Disciplines` or `CompetitionDiscipline.EnabledGenders` exposure differs from what the DTO mapping assumed — adjust `CompetitionDto.From` accordingly.
- `DateRange.Create` throws `DomainException` *before* surface guards run if the handler builds it ahead of the validation block — make sure step 3 of the surface-validation phase pre-checks `start > end` and only calls `DateRange.Create` after guards pass.

- [ ] **Step 3: Smoke-run end to end against Postgres**

```bash
docker compose up -d postgres
dotnet run --project apps/api/src/Sport.Api/Sport.Api.csproj
```

In another terminal:

```bash
curl -X POST http://localhost:5080/competitions \
  -H "Content-Type: application/json" \
  -d '{"code":"smoke-1","name":"Smoke","dates":{"start":"2026-08-01","end":"2026-08-05"},"disciplines":[{"code":"FBL","genders":["M"]}]}'
curl http://localhost:5080/competitions
```

Confirm 201 then a 200 with the created item. Stop the host with Ctrl+C.

- [ ] **Step 4: Final commit (optional fixes from step 2)**

If any fixes were needed, commit them with an explanatory message (e.g. `fix(application): align registry helper naming across test projects`).

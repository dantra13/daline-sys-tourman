# Judo (JUD) Discipline + Subunit-Hosting Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Judo discipline module whose mixed-team event decomposes a team-match unit into weight-category subunits, and make subunit-hosting first-class in the discipline registry — validating the core's subunit machinery against a real discipline.

**Architecture:** Subunit-hosting becomes a property the discipline declares per event type (on `EventTypeDescriptor`), exposed through default-implemented methods on `IDisciplineModule` (so no existing module or test double breaks). The `Event` aggregate root gains governed assembly methods that consult the module to build atomic units vs. team-match parents with subunits. Two RSC invariants confirmed against ODF Foundation Principles §10.3.1.7 are hardened (atomic units end in `--`; `SubunitCode` `00` is reserved for the parent marker).

**Tech Stack:** .NET 10, C# (Vogen value objects, xUnit, FluentAssertions). Spec: `docs/superpowers/specs/2026-05-29-judo-discipline-subunits-design.md`.

**Conventions:**
- All paths are relative to repo root `C:\Users\mella\WebstormProjects\daline-sys`.
- Solution file: `apps/api/Sport.slnx`. Build: `dotnet build apps/api/Sport.slnx`.
- Per-task tests run the specific project to avoid the Postgres/Docker dependency of `Sport.Infrastructure.Tests`.
- Work happens on branch `feat/judo-discipline-subunits` (already created; spec already committed there).

**Out of scope:** competition formats (phases are a provisional allow-list), Structure via Application/API, official-functions catalog completeness, judo technique codes, team rosters/substitutes (`Team (6,6)` for the contesting six).

**RSC reference (ODF Foundation Principles §10.3.1.7), unit slot = 8 chars `[26..34)`:**
- Atomic unit: 6-char body + positions 7&8 = `--` → e.g. `000100--`.
- Parent unit (hosts subunits): positions 7&8 = `00` → e.g. `00010000`.
- Subunits: `00010001`, `00010002`, …

---

## Phase 1 — Core: subunit-hosting contracts + hardened invariants

### Task 1: `EventTypeDescriptor` declares optional subunit structure

**Files:**
- Modify: `apps/api/src/Sport.Core/DisciplineRegistry/EventTypeDescriptor.cs`
- Test: `apps/api/tests/Sport.Core.Tests/DisciplineRegistry/EventTypeDescriptorTests.cs` (create)

- [ ] **Step 1: Write the failing test**

Create `apps/api/tests/Sport.Core.Tests/DisciplineRegistry/EventTypeDescriptorTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

public class EventTypeDescriptorTests
{
    [Fact]
    public void Defaults_to_no_subunit_hosting()
    {
        var d = new EventTypeDescriptor(
            EventTypeCode.From("57KG"), "Women's 57KG",
            new HashSet<GenderCode> { GenderCode.W }, ModifierContract.Forbidden);

        d.HostsSubunits.Should().BeFalse();
        d.CanonicalSubunits.Should().BeEmpty();
    }

    [Fact]
    public void Can_declare_subunit_hosting()
    {
        var d = new EventTypeDescriptor(
            EventTypeCode.From("TEAM6"), "Mixed Team",
            new HashSet<GenderCode> { GenderCode.X }, ModifierContract.Forbidden)
        {
            HostsSubunits = true,
            CanonicalSubunits = new[] { SubunitCode.From("01"), SubunitCode.From("02") },
        };

        d.HostsSubunits.Should().BeTrue();
        d.CanonicalSubunits.Should().HaveCount(2);
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~EventTypeDescriptorTests"`
Expected: FAIL — compile error, `EventTypeDescriptor` has no `HostsSubunits` / `CanonicalSubunits`.

- [ ] **Step 3: Add the two init properties**

Replace the body of `apps/api/src/Sport.Core/DisciplineRegistry/EventTypeDescriptor.cs`:

```csharp
using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public sealed record EventTypeDescriptor(
    EventTypeCode Code,
    string DisplayName,
    IReadOnlySet<GenderCode> AppliesToGenders,
    ModifierContract ModifierContract)
{
    public bool HostsSubunits { get; init; }
    public IReadOnlyCollection<SubunitCode> CanonicalSubunits { get; init; } = Array.Empty<SubunitCode>();
}
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~EventTypeDescriptorTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/DisciplineRegistry/EventTypeDescriptor.cs apps/api/tests/Sport.Core.Tests/DisciplineRegistry/EventTypeDescriptorTests.cs
git commit -m "feat(core): EventTypeDescriptor declares optional subunit-hosting"
```

---

### Task 2: `IDisciplineModule` exposes subunit-hosting via default interface methods

Default interface methods read from `EventTypes`, so the five existing modules and every fake test double keep compiling with no changes.

**Files:**
- Modify: `apps/api/src/Sport.Core/DisciplineRegistry/IDisciplineModule.cs`
- Create (shared test double): `apps/api/tests/Sport.Core.Tests/DisciplineRegistry/SubunitHostingTestModule.cs`
- Test: `apps/api/tests/Sport.Core.Tests/DisciplineRegistry/SubunitHostingDefaultsTests.cs` (create)

- [ ] **Step 1: Create the shared test double** (reused in Task 5)

Create `apps/api/tests/Sport.Core.Tests/DisciplineRegistry/SubunitHostingTestModule.cs`:

```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

/// <summary>
/// Test double whose EventTypes include one subunit-hosting team type (TEAM2, X, contests 01/02)
/// and one atomic individual type (57KG, W). Phase/unit-code validation always succeeds so the
/// double can drive Event assembly tests.
/// </summary>
internal sealed class SubunitHostingTestModule : IDisciplineModule
{
    public DisciplineCode Code => DisciplineCode.From("TST");
    public string DisplayName => "Test";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.X, GenderCode.W };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } = new[]
    {
        new EventTypeDescriptor(EventTypeCode.From("TEAM2"), "Test Team",
            new HashSet<GenderCode> { GenderCode.X }, ModifierContract.Forbidden)
        {
            HostsSubunits = true,
            CanonicalSubunits = new[] { SubunitCode.From("01"), SubunitCode.From("02") },
        },
        new EventTypeDescriptor(EventTypeCode.From("57KG"), "Test 57KG",
            new HashSet<GenderCode> { GenderCode.W }, ModifierContract.Forbidden),
    };

    public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
    public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
    public IReadOnlyCollection<FunctionDescriptor> Functions => Array.Empty<FunctionDescriptor>();
    public IEntryRules EntryRules => throw new NotImplementedException();

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => Result.Ok();
    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => Result.Ok();
    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Result.Ok();
    public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Result.Ok();
}
```

- [ ] **Step 2: Write the failing test**

Create `apps/api/tests/Sport.Core.Tests/DisciplineRegistry/SubunitHostingDefaultsTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

public class SubunitHostingDefaultsTests
{
    [Fact]
    public void Default_members_read_subunit_structure_from_event_types()
    {
        // Default interface members are only visible through the interface type.
        IDisciplineModule m = new SubunitHostingTestModule();

        m.HostsSubunits(EventTypeCode.From("TEAM2")).Should().BeTrue();
        m.HostsSubunits(EventTypeCode.From("57KG")).Should().BeFalse();
        m.HostsSubunits(EventTypeCode.From("ZZZ")).Should().BeFalse();

        m.SubunitsFor(EventTypeCode.From("TEAM2")).Should().HaveCount(2);
        m.SubunitsFor(EventTypeCode.From("57KG")).Should().BeEmpty();

        m.ValidateSubunitCode(EventTypeCode.From("TEAM2"), SubunitCode.From("01")).IsSuccess.Should().BeTrue();
        m.ValidateSubunitCode(EventTypeCode.From("TEAM2"), SubunitCode.From("09")).IsSuccess.Should().BeFalse();
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~SubunitHostingDefaultsTests"`
Expected: FAIL — compile error, `IDisciplineModule` has no `HostsSubunits` / `SubunitsFor` / `ValidateSubunitCode`.

- [ ] **Step 4: Add default interface members**

Edit `apps/api/src/Sport.Core/DisciplineRegistry/IDisciplineModule.cs`. Add these three members inside the interface (after `ValidateOfficialFunctionInScope`):

```csharp
    // Subunit-hosting (default implementations read from EventTypes; disciplines opt in
    // by declaring HostsSubunits/CanonicalSubunits on the matching EventTypeDescriptor).
    bool HostsSubunits(EventTypeCode type) =>
        EventTypes.FirstOrDefault(e => e.Code == type)?.HostsSubunits ?? false;

    IReadOnlyCollection<SubunitCode> SubunitsFor(EventTypeCode type) =>
        EventTypes.FirstOrDefault(e => e.Code == type)?.CanonicalSubunits ?? Array.Empty<SubunitCode>();

    Result ValidateSubunitCode(EventTypeCode type, SubunitCode code) =>
        SubunitsFor(type).Contains(code)
            ? Result.Ok()
            : Result.Fail($"SubunitCode '{code.Value}' is not valid for event type '{type.Value}'.");
```

(`System.Linq` and `System.Array` are available via ImplicitUsings; `Result` and `SubunitCode` are already imported by the file's existing `using Sport.Core.Shared;` and `using Sport.Core.Structure;`.)

- [ ] **Step 5: Run the test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~SubunitHostingDefaultsTests"`
Expected: PASS

- [ ] **Step 6: Build the whole solution to confirm no implementor broke**

Run: `dotnet build apps/api/Sport.slnx`
Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```bash
git add apps/api/src/Sport.Core/DisciplineRegistry/IDisciplineModule.cs apps/api/tests/Sport.Core.Tests/DisciplineRegistry/SubunitHostingTestModule.cs apps/api/tests/Sport.Core.Tests/DisciplineRegistry/SubunitHostingDefaultsTests.cs
git commit -m "feat(core): IDisciplineModule subunit-hosting via default interface methods"
```

---

### Task 3: `SubunitCode` rejects `00` (reserved parent marker)

**Files:**
- Modify: `apps/api/src/Sport.Core/Structure/SubunitCode.cs`
- Test: `apps/api/tests/Sport.Core.Tests/Structure/SubunitCodeTests.cs` (create)

- [ ] **Step 1: Write the failing test**

Create `apps/api/tests/Sport.Core.Tests/Structure/SubunitCodeTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Structure;
using Vogen;

namespace Sport.Core.Tests.Structure;

public class SubunitCodeTests
{
    [Fact]
    public void Rejects_00_parent_marker()
    {
        var act = () => SubunitCode.From("00");
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void Accepts_normal_contest_code()
    {
        SubunitCode.From("01").Value.Should().Be("01");
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~SubunitCodeTests"`
Expected: FAIL — `From("00")` currently succeeds, so `Rejects_00_parent_marker` fails (no exception thrown).

- [ ] **Step 3: Add the guard**

Edit `apps/api/src/Sport.Core/Structure/SubunitCode.cs`. Inside `Validate`, after the char-loop and before `return Validation.Ok;`, add:

```csharp
        if (value == "00")
            return Validation.Invalid("SubunitCode '00' is reserved for the parent-unit marker.");
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~SubunitCodeTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Structure/SubunitCode.cs apps/api/tests/Sport.Core.Tests/Structure/SubunitCodeTests.cs
git commit -m "feat(core): SubunitCode rejects reserved '00' parent marker"
```

---

### Task 4: `Unit.CreateAtomic` enforces `--` trailing (I-STR-14)

**Files:**
- Modify: `apps/api/src/Sport.Core/Structure/Unit.cs:28-32`
- Test: `apps/api/tests/Sport.Core.Tests/Structure/UnitTests.cs`

- [ ] **Step 1: Write the failing test**

Append to `apps/api/tests/Sport.Core.Tests/Structure/UnitTests.cs` (inside the existing test class):

```csharp
    [Fact]
    public void CreateAtomic_rejects_non_filler_trailing()
    {
        var act = () => Unit.CreateAtomic(
            UnitId.New(), PhaseId.New(), UnitCode.From("00010000"), PhaseRsc, null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-14");
    }
```

(`PhaseRsc` is the static RSC fixture already defined in `UnitTests.cs`; `DomainException` lives in `Sport.Core.Shared` — confirm the file's usings include `using Sport.Core.Shared;`, and add it if missing.)

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~UnitTests.CreateAtomic_rejects_non_filler_trailing"`
Expected: FAIL — no exception thrown (`CreateAtomic` accepts any code today).

- [ ] **Step 3: Add the invariant**

Edit `apps/api/src/Sport.Core/Structure/Unit.cs`, replacing the `CreateAtomic` method body:

```csharp
    public static Unit CreateAtomic(UnitId id, PhaseId phaseId, UnitCode code, Rsc phaseRsc, DateTimeOffset? scheduledStart)
    {
        if (!code.Value.EndsWith("--", StringComparison.Ordinal))
            throw new DomainException("I-STR-14", "Atomic UnitCode must end with '--' (positions 7-8 are reserved for subunits).");
        var rsc = ComposeUnitRsc(code, phaseRsc);
        return new Unit(id, phaseId, code, scheduledStart, rsc, new List<Subunit>());
    }
```

- [ ] **Step 4: Run the test to verify it passes (and existing UnitTests stay green)**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~UnitTests"`
Expected: PASS (all UnitTests; existing tests use `"000100--"` which ends in `--`).

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Structure/Unit.cs apps/api/tests/Sport.Core.Tests/Structure/UnitTests.cs
git commit -m "feat(core): enforce '--' trailing on atomic units (I-STR-14)"
```

---

### Task 5: Governed assembly on `Event` (`AddAtomicUnit` / `AddTeamMatchUnit`)

**Files:**
- Modify: `apps/api/src/Sport.Core/Structure/Event.cs`
- Test: `apps/api/tests/Sport.Core.Tests/Structure/EventGovernedAssemblyTests.cs` (create)

Error codes introduced: `I-STR-15` (team-match on non-hosting type), `I-STR-16` (atomic on hosting type), `I-STR-17` (invalid subunit code), `I-STR-18` (phase not found in event).

- [ ] **Step 1: Write the failing tests**

Create `apps/api/tests/Sport.Core.Tests/Structure/EventGovernedAssemblyTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;
using Sport.Core.Structure;
using Sport.Core.Tests.DisciplineRegistry;

namespace Sport.Core.Tests.Structure;

public class EventGovernedAssemblyTests
{
    private static readonly IDisciplineModule Module = new SubunitHostingTestModule();

    private static Event TeamEvent()
    {
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.From(Guid.NewGuid()),
            DisciplineCode.From("TST"), GenderCode.X,
            EventTypeCode.From("TEAM2"), modifier: null, name: "Test Team",
            disciplineModule: Module);
        ev.AddPhase(PhaseCode.From("FNL"), 1, Module);
        return ev;
    }

    private static Event IndividualEvent()
    {
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.From(Guid.NewGuid()),
            DisciplineCode.From("TST"), GenderCode.W,
            EventTypeCode.From("57KG"), modifier: null, name: "Test 57KG",
            disciplineModule: Module);
        ev.AddPhase(PhaseCode.From("FNL"), 1, Module);
        return ev;
    }

    [Fact]
    public void AddTeamMatchUnit_builds_parent_with_governed_subunits()
    {
        var ev = TeamEvent();

        var match = ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            new[] { SubunitCode.From("01"), SubunitCode.From("02") },
            Module, scheduledStart: null);

        match.Subunits.Should().HaveCount(2);
        match.Code.Value.Should().EndWith("00");
        match.Subunits.Select(s => s.Rsc.Value[^2..]).Should().BeEquivalentTo(new[] { "01", "02" });
    }

    [Fact]
    public void AddTeamMatchUnit_rejects_out_of_catalog_subunit()
    {
        var ev = TeamEvent();

        var act = () => ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            new[] { SubunitCode.From("09") }, Module, null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-17");
    }

    [Fact]
    public void AddTeamMatchUnit_rejects_non_hosting_event()
    {
        var ev = IndividualEvent();

        var act = () => ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            new[] { SubunitCode.From("01") }, Module, null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-15");
    }

    [Fact]
    public void AddAtomicUnit_builds_atomic_unit_for_individual_event()
    {
        var ev = IndividualEvent();

        var unit = ev.AddAtomicUnit(PhaseCode.From("FNL"), UnitCode.From("000100--"), Module, null);

        unit.Subunits.Should().BeEmpty();
        unit.Code.Value.Should().EndWith("--");
    }

    [Fact]
    public void AddAtomicUnit_rejects_hosting_event()
    {
        var ev = TeamEvent();

        var act = () => ev.AddAtomicUnit(PhaseCode.From("FNL"), UnitCode.From("000100--"), Module, null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-16");
    }

    [Fact]
    public void Assembly_rejects_unknown_phase()
    {
        var ev = IndividualEvent();

        var act = () => ev.AddAtomicUnit(PhaseCode.From("SFNL"), UnitCode.From("000100--"), Module, null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-18");
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~EventGovernedAssemblyTests"`
Expected: FAIL — compile error, `Event` has no `AddAtomicUnit` / `AddTeamMatchUnit`.

- [ ] **Step 3: Implement the governed methods**

Edit `apps/api/src/Sport.Core/Structure/Event.cs`. Add the `IDisciplineModule` is already imported via `using Sport.Core.DisciplineRegistry;`. Add these methods after `AddPhase`:

```csharp
    public Unit AddAtomicUnit(
        PhaseCode phaseCode,
        UnitCode code,
        IDisciplineModule disciplineModule,
        DateTimeOffset? scheduledStart)
    {
        if (disciplineModule.HostsSubunits(EventType))
            throw new DomainException("I-STR-16",
                $"EventType '{EventType.Value}' hosts subunits; use AddTeamMatchUnit.");

        var phase = FindPhase(phaseCode);
        var unit = Unit.CreateAtomic(UnitId.New(), phase.Id, code, phase.Rsc, scheduledStart);
        phase.AddUnit(unit);
        return unit;
    }

    public Unit AddTeamMatchUnit(
        PhaseCode phaseCode,
        UnitCode parentCode,
        IReadOnlyCollection<SubunitCode> contests,
        IDisciplineModule disciplineModule,
        DateTimeOffset? scheduledStart)
    {
        if (!disciplineModule.HostsSubunits(EventType))
            throw new DomainException("I-STR-15",
                $"EventType '{EventType.Value}' does not host subunits.");

        foreach (var contest in contests)
        {
            var validation = disciplineModule.ValidateSubunitCode(EventType, contest);
            if (!validation.IsSuccess) throw new DomainException("I-STR-17", validation.Error!);
        }

        var phase = FindPhase(phaseCode);
        var parent = Unit.CreateParentForSubunits(UnitId.New(), phase.Id, parentCode, phase.Rsc, scheduledStart);
        foreach (var contest in contests)
            parent.AddSubunit(Subunit.Create(SubunitId.New(), parent.Id, contest, parent.Rsc));
        phase.AddUnit(parent);
        return parent;
    }

    private Phase FindPhase(PhaseCode code) =>
        _phases.FirstOrDefault(p => p.Code == code)
            ?? throw new DomainException("I-STR-18", $"Phase '{code.Value}' not found in Event.");
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~EventGovernedAssemblyTests"`
Expected: PASS (all 6)

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Structure/Event.cs apps/api/tests/Sport.Core.Tests/Structure/EventGovernedAssemblyTests.cs
git commit -m "feat(core): governed unit/subunit assembly on Event (I-STR-15..18)"
```

---

## Phase 2 — JUD discipline module

### Task 6: Scaffold `Sport.Disciplines.JUD` project

**Files:**
- Create: `apps/api/src/Sport.Disciplines.JUD/Sport.Disciplines.JUD.csproj`
- Modify: `apps/api/Sport.slnx` (via CLI)

- [ ] **Step 1: Create the project file**

Create `apps/api/src/Sport.Disciplines.JUD/Sport.Disciplines.JUD.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\Sport.Core\Sport.Core.csproj" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

- [ ] **Step 2: Add the project to the solution**

Run: `dotnet sln apps/api/Sport.slnx add apps/api/src/Sport.Disciplines.JUD/Sport.Disciplines.JUD.csproj`
Expected: "Project ... added to the solution."

- [ ] **Step 3: Build to confirm the empty project compiles**

Run: `dotnet build apps/api/src/Sport.Disciplines.JUD/Sport.Disciplines.JUD.csproj`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add apps/api/src/Sport.Disciplines.JUD/Sport.Disciplines.JUD.csproj apps/api/Sport.slnx
git commit -m "chore(judo): scaffold Sport.Disciplines.JUD project"
```

---

### Task 7: Scaffold `Sport.Disciplines.JUD.Tests` project

**Files:**
- Create: `apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj`
- Modify: `apps/api/Sport.slnx` (via CLI)

- [ ] **Step 1: Create the test project file**

Create `apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="FluentAssertions" Version="7.2.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Sport.Disciplines.JUD\Sport.Disciplines.JUD.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Add the project to the solution**

Run: `dotnet sln apps/api/Sport.slnx add apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj`
Expected: "Project ... added to the solution."

- [ ] **Step 3: Build to confirm**

Run: `dotnet build apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj apps/api/Sport.slnx
git commit -m "chore(judo): scaffold Sport.Disciplines.JUD.Tests project"
```

---

### Task 8: `JudEntryRules` (Athlete + Team)

**Files:**
- Create: `apps/api/src/Sport.Disciplines.JUD/JudEntryRules.cs`
- Test: `apps/api/tests/Sport.Disciplines.JUD.Tests/JudEntryRulesTests.cs` (create)

- [ ] **Step 1: Write the failing test**

Create `apps/api/tests/Sport.Disciplines.JUD.Tests/JudEntryRulesTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Disciplines.JUD.Tests;

public class JudEntryRulesTests
{
    private readonly JudEntryRules _rules = new();

    [Fact]
    public void Accepts_single_athlete()
        => _rules.Validate(new EntryCandidate(EntryType.Athlete, 1, false, true)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Rejects_athlete_pair()
        => _rules.Validate(new EntryCandidate(EntryType.Athlete, 2, false, true)).IsSuccess.Should().BeFalse();

    [Fact]
    public void Accepts_team_of_six()
        => _rules.Validate(new EntryCandidate(EntryType.Team, 6, true, true)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Rejects_team_of_five()
        => _rules.Validate(new EntryCandidate(EntryType.Team, 5, true, true)).IsSuccess.Should().BeFalse();

    [Fact]
    public void Rejects_group_entry()
        => _rules.Validate(new EntryCandidate(EntryType.Group, 6, true, true)).IsSuccess.Should().BeFalse();
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj --filter "FullyQualifiedName~JudEntryRulesTests"`
Expected: FAIL — `JudEntryRules` does not exist.

- [ ] **Step 3: Implement `JudEntryRules`**

Create `apps/api/src/Sport.Disciplines.JUD/JudEntryRules.cs`:

```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Disciplines.JUD;

internal sealed class JudEntryRules : IEntryRules
{
    public IReadOnlyCollection<EntryType> AllowedTypes { get; } = new[] { EntryType.Athlete, EntryType.Team };

    public (int Min, int Max) CompositionSize(EntryType type) => type switch
    {
        EntryType.Athlete => (1, 1),
        EntryType.Team => (6, 6),
        _ => (0, 0),
    };

    public Result Validate(EntryCandidate candidate)
    {
        if (!AllowedTypes.Contains(candidate.Type))
            return Result.Fail($"JUD only accepts Athlete or Team entries, got '{candidate.Type}'.");
        var (min, max) = CompositionSize(candidate.Type);
        if (candidate.CompositionSize < min || candidate.CompositionSize > max)
            return Result.Fail($"JUD {candidate.Type} composition must be {min}..{max}, got {candidate.CompositionSize}.");
        return Result.Ok();
    }
}
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj --filter "FullyQualifiedName~JudEntryRulesTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.JUD/JudEntryRules.cs apps/api/tests/Sport.Disciplines.JUD.Tests/JudEntryRulesTests.cs
git commit -m "feat(judo): JudEntryRules for athlete and team entries"
```

---

### Task 9: `JudPhaseCatalog` (provisional, per-event-type)

**Files:**
- Create: `apps/api/src/Sport.Disciplines.JUD/JudPhaseCatalog.cs`
- Test: `apps/api/tests/Sport.Disciplines.JUD.Tests/JudPhaseCatalogTests.cs` (create)

- [ ] **Step 1: Write the failing test**

Create `apps/api/tests/Sport.Disciplines.JUD.Tests/JudPhaseCatalogTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudPhaseCatalogTests
{
    private readonly JudPhaseCatalog _catalog = new();

    [Fact]
    public void Individual_event_allows_repechage()
    {
        _catalog.IsAllowedForEventType(EventTypeCode.From("73KG"), PhaseCode.From("REP1")).Should().BeTrue();
        _catalog.IsAllowedForEventType(EventTypeCode.From("73KG"), PhaseCode.From("FNL")).Should().BeTrue();
    }

    [Fact]
    public void Team_event_excludes_repechage()
    {
        _catalog.IsAllowedForEventType(EventTypeCode.From("TEAM6"), PhaseCode.From("REP1")).Should().BeFalse();
        _catalog.IsAllowedForEventType(EventTypeCode.From("TEAM6"), PhaseCode.From("FNL")).Should().BeTrue();
    }

    [Fact]
    public void Unknown_phase_rejected()
        => _catalog.IsAllowedForEventType(EventTypeCode.From("73KG"), PhaseCode.From("R8")).Should().BeFalse();
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj --filter "FullyQualifiedName~JudPhaseCatalogTests"`
Expected: FAIL — `JudPhaseCatalog` does not exist.

- [ ] **Step 3: Implement `JudPhaseCatalog`**

Create `apps/api/src/Sport.Disciplines.JUD/JudPhaseCatalog.cs`:

```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

// Provisional phase allow-list. Phases are really a competition-format concern; this declares
// judo's typical single-elimination + repechage shape until the format subsystem exists.
internal sealed class JudPhaseCatalog : IPhaseCatalog
{
    private static readonly HashSet<string> Individual = new(StringComparer.Ordinal)
    {
        "R64", "R32", "8FNL", "QFNL", "SFNL", "FNL", "REP1", "REPF",
    };

    private static readonly HashSet<string> Team = new(StringComparer.Ordinal)
    {
        "R32", "8FNL", "QFNL", "SFNL", "FNL",
    };

    public IReadOnlyCollection<PhaseCode> AllowedPhases { get; } =
        Individual.Union(Team).Select(PhaseCode.From).ToArray();

    public bool IsAllowedForEventType(EventTypeCode eventType, PhaseCode phase) =>
        eventType.Value.StartsWith("TEAM", StringComparison.Ordinal)
            ? Team.Contains(phase.Value)
            : Individual.Contains(phase.Value);
}
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj --filter "FullyQualifiedName~JudPhaseCatalogTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.JUD/JudPhaseCatalog.cs apps/api/tests/Sport.Disciplines.JUD.Tests/JudPhaseCatalogTests.cs
git commit -m "feat(judo): provisional JudPhaseCatalog per event type"
```

---

### Task 10: `JudUnitCodeStrategy`

**Files:**
- Create: `apps/api/src/Sport.Disciplines.JUD/JudUnitCodeStrategy.cs`
- Test: `apps/api/tests/Sport.Disciplines.JUD.Tests/JudUnitCodeStrategyTests.cs` (create)

- [ ] **Step 1: Write the failing test**

Create `apps/api/tests/Sport.Disciplines.JUD.Tests/JudUnitCodeStrategyTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudUnitCodeStrategyTests
{
    private readonly JudUnitCodeStrategy _strategy = new();

    [Fact]
    public void First_atomic_code_is_000100_filler()
        => _strategy.NextUnitCode(Array.Empty<UnitCode>()).Value.Should().Be("000100--");

    [Fact]
    public void Next_atomic_code_increments_body()
        => _strategy.NextUnitCode(new[] { UnitCode.From("000100--") }).Value.Should().Be("000200--");
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj --filter "FullyQualifiedName~JudUnitCodeStrategyTests"`
Expected: FAIL — `JudUnitCodeStrategy` does not exist.

- [ ] **Step 3: Implement `JudUnitCodeStrategy`**

Create `apps/api/src/Sport.Disciplines.JUD/JudUnitCodeStrategy.cs`:

```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

// Produces atomic ('--'-trailing) unit codes for individual contests. Team-match parent codes
// (ending '00') are supplied explicitly when assembling a team match via Event.AddTeamMatchUnit.
internal sealed class JudUnitCodeStrategy : IUnitCodeStrategy
{
    public UnitCode NextUnitCode(IEnumerable<UnitCode> existing)
    {
        var max = existing
            .Select(u => int.TryParse(u.Value.AsSpan(0, 6), out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return UnitCode.From($"{(max + 100).ToString("D6")}--");
    }

    public bool IsValid(UnitCode code) => true;
}
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj --filter "FullyQualifiedName~JudUnitCodeStrategyTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.JUD/JudUnitCodeStrategy.cs apps/api/tests/Sport.Disciplines.JUD.Tests/JudUnitCodeStrategyTests.cs
git commit -m "feat(judo): JudUnitCodeStrategy for atomic contest codes"
```

---

### Task 11: `JudWeightCategories` + `JudModule`

**Files:**
- Create: `apps/api/src/Sport.Disciplines.JUD/JudWeightCategories.cs`
- Create: `apps/api/src/Sport.Disciplines.JUD/JudModule.cs`
- Test: `apps/api/tests/Sport.Disciplines.JUD.Tests/JudModuleTests.cs` (create)

- [ ] **Step 1: Write the failing test**

Create `apps/api/tests/Sport.Disciplines.JUD.Tests/JudModuleTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudModuleTests
{
    private readonly JudModule _module = new();

    [Fact]
    public void Advertises_JUD_code_and_three_genders()
    {
        _module.Code.Value.Should().Be("JUD");
        _module.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W, GenderCode.X });
    }

    [Fact]
    public void Accepts_men_women_and_team_event_types()
    {
        _module.ValidateEventType(EventTypeCode.From("73KG"), null).IsSuccess.Should().BeTrue();
        _module.ValidateEventType(EventTypeCode.From("O100KG"), null).IsSuccess.Should().BeTrue();
        _module.ValidateEventType(EventTypeCode.From("57KG"), null).IsSuccess.Should().BeTrue();
        _module.ValidateEventType(EventTypeCode.From("TEAM6"), null).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Rejects_unknown_event_type_and_modifier()
    {
        _module.ValidateEventType(EventTypeCode.From("99KG"), null).IsSuccess.Should().BeFalse();
        _module.ValidateEventType(EventTypeCode.From("73KG"), EventModifierCode.From("X")).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Team_event_hosts_six_weight_category_subunits()
    {
        IDisciplineModule m = _module;
        m.HostsSubunits(EventTypeCode.From("TEAM6")).Should().BeTrue();
        m.SubunitsFor(EventTypeCode.From("TEAM6")).Should().HaveCount(6);
        m.ValidateSubunitCode(EventTypeCode.From("TEAM6"), SubunitCode.From("06")).IsSuccess.Should().BeTrue();
        m.ValidateSubunitCode(EventTypeCode.From("TEAM6"), SubunitCode.From("07")).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Individual_event_hosts_no_subunits()
        => ((IDisciplineModule)_module).HostsSubunits(EventTypeCode.From("73KG")).Should().BeFalse();
}
```

(`EventModifierCode.From("X")` is used only to assert the modifier is rejected; `EventModifierCode` is in `Sport.Core.Structure`, already imported.)

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj --filter "FullyQualifiedName~JudModuleTests"`
Expected: FAIL — `JudModule` does not exist.

- [ ] **Step 3: Implement `JudWeightCategories`**

Create `apps/api/src/Sport.Disciplines.JUD/JudWeightCategories.cs`:

```csharp
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

// Paris 2024 categories. "Over/+" categories use an 'O' prefix because EventTypeCode is A-Z0-9.
internal static class JudWeightCategories
{
    public static readonly string[] Men =
        { "60KG", "66KG", "73KG", "81KG", "90KG", "100KG", "O100KG" };

    public static readonly string[] Women =
        { "48KG", "52KG", "57KG", "63KG", "70KG", "78KG", "O78KG" };

    public const string TeamEventType = "TEAM6";

    public static readonly SubunitCode[] TeamContests =
    {
        SubunitCode.From("01"), SubunitCode.From("02"), SubunitCode.From("03"),
        SubunitCode.From("04"), SubunitCode.From("05"), SubunitCode.From("06"),
    };
}
```

- [ ] **Step 4: Implement `JudModule`**

Create `apps/api/src/Sport.Disciplines.JUD/JudModule.cs`:

```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

public sealed class JudModule : IDisciplineModule
{
    public DisciplineCode Code { get; } = DisciplineCode.From("JUD");
    public string DisplayName => "Judo";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.M, GenderCode.W, GenderCode.X };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } = BuildEventTypes();

    public IPhaseCatalog PhaseCatalog { get; } = new JudPhaseCatalog();
    public IUnitCodeStrategy UnitCodeStrategy { get; } = new JudUnitCodeStrategy();

    public IReadOnlyCollection<FunctionDescriptor> Functions { get; } = new[]
    {
        new FunctionDescriptor(FunctionCode.From("JUD.REF"),  "Referee",     new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("JUD.JUD1"), "Judge 1",     new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("JUD.JUD2"), "Judge 2",     new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("JUD.CARE"), "CARE System", new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
    };

    public IEntryRules EntryRules { get; } = new JudEntryRules();

    private static EventTypeDescriptor[] BuildEventTypes()
    {
        var list = new List<EventTypeDescriptor>();

        foreach (var c in JudWeightCategories.Men)
            list.Add(new EventTypeDescriptor(
                EventTypeCode.From(c), $"Men's {c}",
                new HashSet<GenderCode> { GenderCode.M }, ModifierContract.Forbidden));

        foreach (var c in JudWeightCategories.Women)
            list.Add(new EventTypeDescriptor(
                EventTypeCode.From(c), $"Women's {c}",
                new HashSet<GenderCode> { GenderCode.W }, ModifierContract.Forbidden));

        list.Add(new EventTypeDescriptor(
            EventTypeCode.From(JudWeightCategories.TeamEventType), "Mixed Team",
            new HashSet<GenderCode> { GenderCode.X }, ModifierContract.Forbidden)
        {
            HostsSubunits = true,
            CanonicalSubunits = JudWeightCategories.TeamContests,
        });

        return list.ToArray();
    }

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier)
    {
        if (!EventTypes.Any(e => e.Code == type))
            return Result.Fail($"EventType '{type.Value}' not supported by JUD.");
        if (modifier is not null)
            return Result.Fail("JUD EventType does not accept a modifier.");
        return Result.Ok();
    }

    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) =>
        PhaseCatalog.IsAllowedForEventType(type, phase)
            ? Result.Ok()
            : Result.Fail($"Phase '{phase.Value}' is not allowed for JUD EventType '{type.Value}'.");

    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) =>
        UnitCodeStrategy.IsValid(code) ? Result.Ok() : Result.Fail("Invalid UnitCode for JUD.");

    public Result ValidateEntry(EntryCandidate candidate) => EntryRules.Validate(candidate);

    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level)
    {
        var f = Functions.FirstOrDefault(x => x.Code == function);
        if (f is null) return Result.Fail($"Function '{function.Value}' not registered in JUD.");
        if (!f.ValidScopes.Contains(level)) return Result.Fail($"Function '{function.Value}' not valid at scope '{level}'.");
        return Result.Ok();
    }
}
```

> Note (matches existing modules): `ValidateEventType` does not check event-type/gender consistency because its signature has no gender parameter. Each `EventTypeDescriptor.AppliesToGenders` records the intended gender for future use; gender is validated at the module level by `Event.Create` against `SupportedGenders`, exactly as BOX/FBL do today.

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj`
Expected: PASS (all JUD test classes)

- [ ] **Step 6: Commit**

```bash
git add apps/api/src/Sport.Disciplines.JUD/JudWeightCategories.cs apps/api/src/Sport.Disciplines.JUD/JudModule.cs apps/api/tests/Sport.Disciplines.JUD.Tests/JudModuleTests.cs
git commit -m "feat(judo): JudModule with Paris 2024 categories and subunit-hosting team event"
```

---

## Phase 3 — Integration and coverage

### Task 12: Register JUD in the API host

**Files:**
- Modify: `apps/api/src/Sport.Api/Sport.Api.csproj` (add ProjectReference)
- Modify: `apps/api/src/Sport.Api/Program.cs`

- [ ] **Step 1: Add the project reference**

In `apps/api/src/Sport.Api/Sport.Api.csproj`, add the following line to the `<ItemGroup>` that already lists the `Sport.Disciplines.*` project references:

```xml
    <ProjectReference Include="..\..\src\Sport.Disciplines.JUD\Sport.Disciplines.JUD.csproj" />
```

(Match the relative-path style already used by the sibling discipline references in that file.)

- [ ] **Step 2: Register the module in `Program.cs`**

In `apps/api/src/Sport.Api/Program.cs`:

Add the using alongside the other discipline usings (after `using Sport.Disciplines.FBL;`):

```csharp
using Sport.Disciplines.JUD;
```

Add the registration call to the `builder.Services` chain (after `.AddDisciplineModule<AthModule>()`):

```csharp
    .AddDisciplineModule<JudModule>();
```

Move the trailing `;` so the chain still ends correctly — the final line becomes:

```csharp
    .AddDisciplineModule<AthModule>()
    .AddDisciplineModule<JudModule>();
```

- [ ] **Step 3: Build and run the API host tests**

Run: `dotnet build apps/api/src/Sport.Api/Sport.Api.csproj`
Then: `dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj`
Expected: Build succeeded; Api.Tests PASS (host boots with JUD registered).

- [ ] **Step 4: Commit**

```bash
git add apps/api/src/Sport.Api/Sport.Api.csproj apps/api/src/Sport.Api/Program.cs
git commit -m "feat(judo): register JUD module in the API host"
```

---

### Task 13: Architecture coverage for JUD

**Files:**
- Modify: `apps/api/tests/Sport.Architecture.Tests/ArchitectureRules.cs:16-24`

- [ ] **Step 1: Add JUD to the discipline-assemblies fixture**

In `apps/api/tests/Sport.Architecture.Tests/ArchitectureRules.cs`, add JUD to the `DisciplineAssemblies` array:

```csharp
    private static readonly System.Reflection.Assembly[] DisciplineAssemblies =
    {
        typeof(Sport.Disciplines.FBL.FblModule).Assembly,
        typeof(Sport.Disciplines.BKB.BkbModule).Assembly,
        typeof(Sport.Disciplines.BDM.BdmModule).Assembly,
        typeof(Sport.Disciplines.VBV.VbvModule).Assembly,
        typeof(Sport.Disciplines.BOX.BoxModule).Assembly,
        typeof(Sport.Disciplines.ATH.AthModule).Assembly,
        typeof(Sport.Disciplines.JUD.JudModule).Assembly,
    };
```

This requires a project reference from the architecture test project to JUD. Add to `apps/api/tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj` (in the `<ItemGroup>` with the other discipline references):

```xml
    <ProjectReference Include="..\..\src\Sport.Disciplines.JUD\Sport.Disciplines.JUD.csproj" />
```

- [ ] **Step 2: Run the architecture tests**

Run: `dotnet test apps/api/tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj`
Expected: PASS — JUD obeys the same isolation rules (no cross-discipline / infrastructure dependencies).

- [ ] **Step 3: Commit**

```bash
git add apps/api/tests/Sport.Architecture.Tests/ArchitectureRules.cs apps/api/tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj
git commit -m "test(arch): cover JUD discipline isolation rules"
```

---

### Task 14: End-to-end core validation — JUD team match into 6 subunits

**Files:**
- Test: `apps/api/tests/Sport.Disciplines.JUD.Tests/JudTeamMatchSubunitTests.cs` (create)

This is the headline test: a real `JudModule` drives the full `Competition → Event → Phase → team-match Unit → Subunits` assembly through the governed `Event` methods, proving the core handles subunits with a real discipline.

- [ ] **Step 1: Write the test**

Create `apps/api/tests/Sport.Disciplines.JUD.Tests/JudTeamMatchSubunitTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudTeamMatchSubunitTests
{
    private static readonly DisciplineCode Jud = DisciplineCode.From("JUD");

    private static (Event ev, IDisciplineModule module) NewMixedTeamEvent()
    {
        var registry = new DisciplineRegistry();
        registry.Register(new JudModule());
        var module = registry.Get(Jud);

        var comp = Competition.Create(
            CompetitionId.New(), CompetitionCode.From("jud-2026"), "Judo 2026",
            DateRange.Create(new DateOnly(2026, 7, 27), new DateOnly(2026, 8, 3)),
            new[] { (Jud, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.X }) },
            registry);
        var compDisc = comp.Disciplines.Single();

        var ev = Event.Create(
            EventId.New(), compDisc.Id, Jud, GenderCode.X,
            EventTypeCode.From("TEAM6"), modifier: null, name: "Mixed Team",
            disciplineModule: module);
        ev.AddPhase(PhaseCode.From("FNL"), 1, module);
        return (ev, module);
    }

    [Fact]
    public void Team_match_decomposes_into_six_weight_category_subunits()
    {
        var (ev, module) = NewMixedTeamEvent();

        var match = ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            module.SubunitsFor(EventTypeCode.From("TEAM6")), module, scheduledStart: null);

        match.Subunits.Should().HaveCount(6);
        match.Code.Value.Should().EndWith("00");
        match.Subunits.Select(s => s.Rsc.Value[^2..]).Should()
            .BeEquivalentTo(new[] { "01", "02", "03", "04", "05", "06" });
        match.Rsc.Value.Should().EndWith("00");
    }

    [Fact]
    public void Individual_event_cannot_be_assembled_as_team_match()
    {
        var registry = new DisciplineRegistry();
        registry.Register(new JudModule());
        var module = registry.Get(Jud);

        var comp = Competition.Create(
            CompetitionId.New(), CompetitionCode.From("jud-ind-2026"), "Judo Individual 2026",
            DateRange.Create(new DateOnly(2026, 7, 27), new DateOnly(2026, 8, 3)),
            new[] { (Jud, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);

        var ev = Event.Create(
            EventId.New(), comp.Disciplines.Single().Id, Jud, GenderCode.M,
            EventTypeCode.From("73KG"), modifier: null, name: "Men's 73KG",
            disciplineModule: module);
        ev.AddPhase(PhaseCode.From("FNL"), 1, module);

        var act = () => ev.AddTeamMatchUnit(
            PhaseCode.From("FNL"), UnitCode.From("00010000"),
            new[] { SubunitCode.From("01") }, module, null);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("I-STR-15");
    }
}
```

- [ ] **Step 2: Run the test**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj --filter "FullyQualifiedName~JudTeamMatchSubunitTests"`
Expected: PASS (both)

- [ ] **Step 3: Commit**

```bash
git add apps/api/tests/Sport.Disciplines.JUD.Tests/JudTeamMatchSubunitTests.cs
git commit -m "test(judo): end-to-end team match decomposes into six subunits"
```

---

### Task 15: Full solution build + test sweep

**Files:** none (verification + final state).

- [ ] **Step 1: Build the whole solution**

Run: `dotnet build apps/api/Sport.slnx`
Expected: Build succeeded, 0 warnings, 0 errors.

- [ ] **Step 2: Run the non-Postgres test projects**

Run each and confirm PASS:

```bash
dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj
dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj
dotnet test apps/api/tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj
dotnet test apps/api/tests/Sport.Api.Tests/Sport.Api.Tests.csproj
```

Expected: PASS for all four.

> `Sport.Infrastructure.Tests` requires a Postgres container (Testcontainers/Docker). Run it only if Docker is available:
> `dotnet test apps/api/tests/Sport.Infrastructure.Tests/Sport.Infrastructure.Tests.csproj`. This plan does not change persistence; it should remain green.

- [ ] **Step 3: Final verification of the JUD test count**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests/Sport.Disciplines.JUD.Tests.csproj`
Expected: all JUD tests PASS (EntryRules, PhaseCatalog, UnitCodeStrategy, Module, TeamMatchSubunit).

- [ ] **Step 4: Commit any final adjustments (if needed)**

If Steps 1-3 required no changes, no commit is necessary (each task committed individually). Otherwise:

```bash
git add -A
git commit -m "chore(judo): finalize JUD discipline + subunit-hosting"
```

---

## Self-Review

**Spec coverage:**
- JUD module (categories, entry rules, phases, units, officials) → Tasks 6-11. ✓
- Subunit-hosting first-class on registry contracts → Tasks 1-2. ✓
- Governed assembly on `Event` → Task 5. ✓
- RSC `00`-parent kept; atomic `--` + subunit `00` hardening → Tasks 3-4. ✓
- Registration in host → Task 12. ✓
- Tests incl. core-governance + end-to-end JUD subunit + arch coverage → Tasks 5, 13, 14. ✓
- Formats / Structure-via-API explicitly out of scope. ✓

**Deviations from the spec (intentional, noted inline):**
- No per-module `ServiceCollectionExtensions` — registration uses the existing generic `AddDisciplineModule<TModule>()`.
- `IDisciplineModule` subunit methods are **default interface implementations** (spec said "methods"); this avoids editing the five existing modules and all fake doubles.
- `ValidateEventType` does not enforce event-type/gender consistency (signature has no gender param); matches existing module behavior. `AppliesToGenders` remains informational.
- New `I-STR-14..18` codes are **not** added to `DomainErrorCatalog` — consistent with existing `I-STR-*` codes being absent (Structure is not API-exposed yet).

**Type consistency:** `HostsSubunits` / `SubunitsFor` / `ValidateSubunitCode` signatures match between Task 2 (definition), Task 5 (Event usage), Task 11 (JudModule), and Task 14 (test). `EventTypeDescriptor` init properties `HostsSubunits` / `CanonicalSubunits` consistent across Tasks 1, 2, 11. Error codes `I-STR-14..18` used consistently in Tasks 4-5 and asserted in tests.

**Placeholder scan:** none — every code/test/command step is concrete.

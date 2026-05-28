# Sport Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the conceptual sport core (`Sport.Core` + 6 discipline modules) — entities, value objects, invariants, and the `IDisciplineModule` plug-in contract — exactly as specified in `docs/superpowers/specs/2026-05-27-core-deportivo-design.md`.

**Architecture:** A single bounded context `Sport` with five internal submodules (`Competitions`, `Structure`, `Participants`, `Officials`, `DisciplineRegistry`) materialized as folders inside one `Sport.Core` project. Each discipline (FBL, BKB, BDM, VBV, BOX, ATH) is its own `csproj` that references only `Sport.Core`. Identities are Vogen-typed (Guid v7); RSC is a derived value object. NetArchTest fitness tests guard the core ↔ disciplines boundary.

**Tech Stack:** .NET 10 · C# 14 · Vogen (typed VOs) · xUnit · FluentAssertions · NetArchTest

**Reference:** Source spec at `docs/superpowers/specs/2026-05-27-core-deportivo-design.md`. Invariants are tagged (I-COMP-*, I-STR-*, I-PAR-*, I-OFF-*, I-REG-*) and referenced from tasks.

---

## Conventions used in this plan

- All paths are relative to repository root.
- Vogen value objects are `partial readonly struct` with `[ValueObject<TUnderlying>]`.
- Invariant violations on entity construction/mutation throw `DomainException` (a simple custom exception type defined in `Sport.Core/Shared/`).
- Validation calls from `IDisciplineModule` return a `Result` (success or error with message). No exceptions for "expected" validation failures.
- All aggregate-root IDs use `Guid.CreateVersion7()`.
- Tests live in `tests/Sport.Core.Tests/` mirroring the source folder structure.
- Each task ends with a commit. Commit messages follow the prefix style already in the repo (`docs:`, `feat:`, `test:`, `chore:`).

---

## File map (created across the plan)

```
src/
  Sport.Core/
    Sport.Core.csproj
    Shared/
      DomainException.cs
      Result.cs
    Competitions/
      Competition.cs
      CompetitionDiscipline.cs
      CompetitionId.cs
      CompetitionDisciplineId.cs
      CompetitionCode.cs
      DateRange.cs
    Structure/
      Event.cs
      Phase.cs
      Unit.cs
      Subunit.cs
      EventId.cs
      PhaseId.cs
      UnitId.cs
      SubunitId.cs
      EventTypeCode.cs
      EventModifierCode.cs
      PhaseCode.cs
      UnitCode.cs
      SubunitCode.cs
      Rsc.cs
    Participants/
      Person.cs
      Organisation.cs
      Team.cs
      Entry.cs
      CompositionMember.cs
      PersonId.cs
      OrganisationId.cs
      OrganisationCode.cs
      OrganisationType.cs
      TeamId.cs
      TeamCode.cs
      EntryId.cs
      EntryType.cs
      EntryStatus.cs
      Bib.cs
    Officials/
      OfficialAssignment.cs
      OfficialAssignmentId.cs
      OfficialAssignmentStatus.cs
      OfficialScope.cs
      ScopeLevel.cs
      FunctionCode.cs
    DisciplineRegistry/
      DisciplineCode.cs
      GenderCode.cs
      IDisciplineModule.cs
      IDisciplineRegistry.cs
      DisciplineRegistry.cs
      EventTypeDescriptor.cs
      ModifierContract.cs
      IPhaseCatalog.cs
      IUnitCodeStrategy.cs
      IEntryRules.cs
      EntryCandidate.cs
      FunctionDescriptor.cs
      CommonFunctions.cs
      ServiceCollectionExtensions.cs

  Sport.Disciplines.FBL/  Sport.Disciplines.FBL.csproj  FblModule.cs
  Sport.Disciplines.BKB/  Sport.Disciplines.BKB.csproj  BkbModule.cs
  Sport.Disciplines.BDM/  Sport.Disciplines.BDM.csproj  BdmModule.cs
  Sport.Disciplines.VBV/  Sport.Disciplines.VBV.csproj  VbvModule.cs
  Sport.Disciplines.BOX/  Sport.Disciplines.BOX.csproj  BoxModule.cs
  Sport.Disciplines.ATH/  Sport.Disciplines.ATH.csproj  AthModule.cs

tests/
  Sport.Core.Tests/  ...mirrors src/Sport.Core/
  Sport.Architecture.Tests/  ArchitectureRules.cs

Sport.sln
Directory.Build.props
.gitignore
```

---

## Phase A — Repository scaffolding

### Task 1: Solution, projects, and shared configuration

**Files:**
- Create: `Sport.sln`
- Create: `Directory.Build.props`
- Create: `.gitignore`
- Create: `src/Sport.Core/Sport.Core.csproj`
- Create: `tests/Sport.Core.Tests/Sport.Core.Tests.csproj`

- [ ] **Step 1: Verify .NET 10 SDK is available**

Run: `dotnet --version`
Expected: `10.0.x` or newer.

- [ ] **Step 2: Create solution file**

Run from repo root:
```
dotnet new sln -n Sport
```

- [ ] **Step 3: Create `Directory.Build.props`**

Path: `Directory.Build.props`
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

- [ ] **Step 4: Create `.gitignore`**

Path: `.gitignore`
```
bin/
obj/
*.user
*.suo
.vs/
.idea/
.vscode/
TestResults/
```

- [ ] **Step 5: Create `Sport.Core` class library**

```
dotnet new classlib -n Sport.Core -o src/Sport.Core -f net10.0
```

- [ ] **Step 6: Add Vogen and Microsoft.Extensions.DependencyInjection.Abstractions to Sport.Core**

```
dotnet add src/Sport.Core/Sport.Core.csproj package Vogen
dotnet add src/Sport.Core/Sport.Core.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
```

- [ ] **Step 7: Create `Sport.Core.Tests` xUnit project**

```
dotnet new xunit -n Sport.Core.Tests -o tests/Sport.Core.Tests -f net10.0
```

- [ ] **Step 8: Add FluentAssertions to test project and reference Sport.Core**

```
dotnet add tests/Sport.Core.Tests/Sport.Core.Tests.csproj package FluentAssertions
dotnet add tests/Sport.Core.Tests/Sport.Core.Tests.csproj reference src/Sport.Core/Sport.Core.csproj
```

- [ ] **Step 9: Add both projects to the solution**

```
dotnet sln add src/Sport.Core/Sport.Core.csproj tests/Sport.Core.Tests/Sport.Core.Tests.csproj
```

- [ ] **Step 10: Delete the default `Class1.cs` and `UnitTest1.cs` files**

Remove `src/Sport.Core/Class1.cs` and `tests/Sport.Core.Tests/UnitTest1.cs`.

- [ ] **Step 11: Build to confirm everything wires together**

Run: `dotnet build Sport.sln`
Expected: build succeeds, zero warnings, zero errors.

- [ ] **Step 12: Commit**

```
git add Sport.sln Directory.Build.props .gitignore src/ tests/
git commit -m "chore: scaffold sport core solution and test project"
```

---

## Phase B — Shared kernel

### Task 2: `DomainException` and `Result`

**Files:**
- Create: `src/Sport.Core/Shared/DomainException.cs`
- Create: `src/Sport.Core/Shared/Result.cs`
- Create: `tests/Sport.Core.Tests/Shared/ResultTests.cs`

- [ ] **Step 1: Write failing test for `Result`**

Path: `tests/Sport.Core.Tests/Shared/ResultTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Shared;

public class ResultTests
{
    [Fact]
    public void Ok_creates_a_success_result_with_no_error()
    {
        var result = Result.Ok();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_creates_a_failure_result_with_error_message()
    {
        var result = Result.Fail("bad input");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("bad input");
    }

    [Fact]
    public void Fail_with_empty_message_throws()
    {
        var act = () => Result.Fail("");
        act.Should().Throw<System.ArgumentException>();
    }
}
```

- [ ] **Step 2: Run the test to confirm it fails (Result type does not exist)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~ResultTests"`
Expected: build fails ("Result not found").

- [ ] **Step 3: Implement `DomainException`**

Path: `src/Sport.Core/Shared/DomainException.cs`
```csharp
namespace Sport.Core.Shared;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

- [ ] **Step 4: Implement `Result`**

Path: `src/Sport.Core/Shared/Result.cs`
```csharp
namespace Sport.Core.Shared;

public readonly record struct Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new(true, null);

    public static Result Fail(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message is required for a failure result.", nameof(error));
        return new Result(false, error);
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~ResultTests"`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```
git add src/Sport.Core/Shared/ tests/Sport.Core.Tests/Shared/
git commit -m "feat(core): add DomainException and Result shared kernel"
```

---

### Task 3: `DisciplineCode` and `GenderCode` value objects

**Files:**
- Create: `src/Sport.Core/DisciplineRegistry/DisciplineCode.cs`
- Create: `src/Sport.Core/DisciplineRegistry/GenderCode.cs`
- Create: `tests/Sport.Core.Tests/DisciplineRegistry/DisciplineCodeTests.cs`
- Create: `tests/Sport.Core.Tests/DisciplineRegistry/GenderCodeTests.cs`

- [ ] **Step 1: Write failing tests for `DisciplineCode`**

Path: `tests/Sport.Core.Tests/DisciplineRegistry/DisciplineCodeTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Vogen;
using Xunit;

namespace Sport.Core.Tests.DisciplineRegistry;

public class DisciplineCodeTests
{
    [Theory]
    [InlineData("FBL")]
    [InlineData("BKB")]
    [InlineData("ATH")]
    public void Accepts_three_uppercase_letters(string value)
    {
        var act = () => DisciplineCode.From(value);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("FB")]
    [InlineData("FBLX")]
    [InlineData("fbl")]
    [InlineData("F1L")]
    public void Rejects_invalid_inputs(string value)
    {
        var act = () => DisciplineCode.From(value);
        act.Should().Throw<ValueObjectValidationException>();
    }
}
```

- [ ] **Step 2: Write failing tests for `GenderCode`**

Path: `tests/Sport.Core.Tests/DisciplineRegistry/GenderCodeTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Xunit;

namespace Sport.Core.Tests.DisciplineRegistry;

public class GenderCodeTests
{
    [Theory]
    [InlineData(GenderCode.M, 'M')]
    [InlineData(GenderCode.W, 'W')]
    [InlineData(GenderCode.X, 'X')]
    [InlineData(GenderCode.O, 'O')]
    public void Each_value_maps_to_one_rsc_character(GenderCode value, char rscChar)
    {
        value.ToRscChar().Should().Be(rscChar);
    }
}
```

- [ ] **Step 3: Run the tests to confirm they fail (types missing)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~DisciplineCodeTests|FullyQualifiedName~GenderCodeTests"`
Expected: build fails.

- [ ] **Step 4: Implement `DisciplineCode`**

Path: `src/Sport.Core/DisciplineRegistry/DisciplineCode.cs`
```csharp
using Vogen;

namespace Sport.Core.DisciplineRegistry;

[ValueObject<string>]
public readonly partial struct DisciplineCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("DisciplineCode is required.");
        if (value.Length != 3) return Validation.Invalid("DisciplineCode must be exactly 3 characters.");
        foreach (var c in value)
            if (c is < 'A' or > 'Z')
                return Validation.Invalid("DisciplineCode must be 3 uppercase ASCII letters.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 5: Implement `GenderCode`**

Path: `src/Sport.Core/DisciplineRegistry/GenderCode.cs`
```csharp
namespace Sport.Core.DisciplineRegistry;

public enum GenderCode
{
    M,
    W,
    X,
    O,
}

public static class GenderCodeExtensions
{
    public static char ToRscChar(this GenderCode code) => code switch
    {
        GenderCode.M => 'M',
        GenderCode.W => 'W',
        GenderCode.X => 'X',
        GenderCode.O => 'O',
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj`
Expected: all tests pass.

- [ ] **Step 7: Commit**

```
git add src/Sport.Core/DisciplineRegistry/ tests/Sport.Core.Tests/DisciplineRegistry/
git commit -m "feat(core): add DisciplineCode and GenderCode value objects"
```

---

## Phase C — RSC value object

### Task 4: `Rsc` validation (length, charset, uppercase)

**Files:**
- Create: `src/Sport.Core/Structure/Rsc.cs`
- Create: `tests/Sport.Core.Tests/Structure/RscValidationTests.cs`

- [ ] **Step 1: Write failing tests for `Rsc.From`**

Path: `tests/Sport.Core.Tests/Structure/RscValidationTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Structure;
using Vogen;
using Xunit;

namespace Sport.Core.Tests.Structure;

public class RscValidationTests
{
    private const string ValidUnitRsc      = "FBLMTEAM11------------QFNL000100--";
    private const string ValidPhaseRsc     = "FBLMTEAM11------------GPA---------";
    private const string ValidEventRsc     = "FBLMTEAM11----------------------"; // 34 chars
    private const string ValidDisciplineRsc = "FBL-------------------------------"; // 34

    [Theory]
    [InlineData(ValidUnitRsc)]
    [InlineData(ValidPhaseRsc)]
    public void Accepts_valid_34_char_uppercase_input(string value)
    {
        var act = () => Rsc.From(value);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]                                       // empty
    [InlineData("FBL")]                                    // too short
    [InlineData("FBLMTEAM11------------QFNL000100---")]    // 35 chars
    [InlineData("fblmteam11------------qfnl000100--")]    // lowercase
    [InlineData("FBL_TEAM11------------QFNL000100--")]    // underscore not allowed
    public void Rejects_invalid_input(string value)
    {
        var act = () => Rsc.From(value);
        act.Should().Throw<ValueObjectValidationException>();
    }
}
```

- [ ] **Step 2: Run the tests (fail — type missing)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~RscValidationTests"`
Expected: build fails.

- [ ] **Step 3: Implement `Rsc` with `From` validation**

Path: `src/Sport.Core/Structure/Rsc.cs`
```csharp
using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct Rsc
{
    public const int Length = 34;
    public const char Filler = '-';

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("RSC is required.");
        if (value.Length != Length) return Validation.Invalid($"RSC must be exactly {Length} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z'
                  || c is >= '0' and <= '9'
                  || c == '.'
                  || c == '-';
            if (!ok) return Validation.Invalid("RSC may only contain A-Z, 0-9, '.' and '-'.");
        }
        return Validation.Ok;
    }
}
```

- [ ] **Step 4: Run the tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~RscValidationTests"`
Expected: tests pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Core/Structure/Rsc.cs tests/Sport.Core.Tests/Structure/RscValidationTests.cs
git commit -m "feat(core): add Rsc value object with format validation"
```

---

### Task 5: `Rsc.Compose` and extraction (level, slots)

**Files:**
- Modify: `src/Sport.Core/Structure/Rsc.cs`
- Create: `tests/Sport.Core.Tests/Structure/RscCompositionTests.cs`

- [ ] **Step 1: Write failing tests for `Rsc.Compose` and extraction**

Path: `tests/Sport.Core.Tests/Structure/RscCompositionTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;
using Xunit;

namespace Sport.Core.Tests.Structure;

public class RscCompositionTests
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");

    [Fact]
    public void Composes_event_level_with_filler_in_phase_and_unit()
    {
        var rsc = Rsc.Compose(
            discipline: Fbl,
            gender: GenderCode.M,
            eventType: EventTypeCode.From("TEAM11"),
            modifier: null,
            phase: null,
            unit: null,
            subunit: null);

        rsc.Value.Should().Be("FBLMTEAM11----------------------"); // 34 chars exactly
        rsc.Value.Length.Should().Be(34);
    }

    [Fact]
    public void Composes_phase_level()
    {
        var rsc = Rsc.Compose(
            Fbl, GenderCode.M, EventTypeCode.From("TEAM11"),
            modifier: null,
            phase: PhaseCode.From("GPA"),
            unit: null,
            subunit: null);

        rsc.Value.Should().Be("FBLMTEAM11------------GPA---------");
    }

    [Fact]
    public void Composes_unit_level_atomic()
    {
        var rsc = Rsc.Compose(
            Fbl, GenderCode.M, EventTypeCode.From("TEAM11"),
            modifier: null,
            phase: PhaseCode.From("QFNL"),
            unit: UnitCode.From("000100--"),
            subunit: null);

        rsc.Value.Should().Be("FBLMTEAM11------------QFNL000100--");
    }

    [Fact]
    public void Level_reports_correct_value_for_each_input()
    {
        Rsc.From("FBL-------------------------------").Level.Should().Be(RscLevel.Discipline);
        Rsc.From("FBLMTEAM11----------------------").Level.Should().Be(RscLevel.Event);
        Rsc.From("FBLMTEAM11------------GPA---------").Level.Should().Be(RscLevel.Phase);
        Rsc.From("FBLMTEAM11------------QFNL000100--").Level.Should().Be(RscLevel.Unit);
    }
}
```

- [ ] **Step 2: Run the tests (fail — Compose/Level missing, plus types EventTypeCode/PhaseCode/UnitCode not yet defined)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~RscCompositionTests"`
Expected: build fails. Note: `EventTypeCode`, `PhaseCode`, `UnitCode` are introduced in Task 7 (Structure VOs). To unblock this task, define those minimal placeholders inline now.

- [ ] **Step 3: Create minimal placeholders for `EventTypeCode`, `EventModifierCode`, `PhaseCode`, `UnitCode`, `SubunitCode`**

These are fleshed out properly in Task 7 with full validation. For now, keep them simple Vogen string wrappers so `Rsc.Compose` compiles.

Path: `src/Sport.Core/Structure/EventTypeCode.cs`
```csharp
using Vogen;
namespace Sport.Core.Structure;
[ValueObject<string>]
public readonly partial struct EventTypeCode { }
```

Path: `src/Sport.Core/Structure/EventModifierCode.cs`
```csharp
using Vogen;
namespace Sport.Core.Structure;
[ValueObject<string>]
public readonly partial struct EventModifierCode { }
```

Path: `src/Sport.Core/Structure/PhaseCode.cs`
```csharp
using Vogen;
namespace Sport.Core.Structure;
[ValueObject<string>]
public readonly partial struct PhaseCode { }
```

Path: `src/Sport.Core/Structure/UnitCode.cs`
```csharp
using Vogen;
namespace Sport.Core.Structure;
[ValueObject<string>]
public readonly partial struct UnitCode { }
```

Path: `src/Sport.Core/Structure/SubunitCode.cs`
```csharp
using Vogen;
namespace Sport.Core.Structure;
[ValueObject<string>]
public readonly partial struct SubunitCode { }
```

(Task 7 will replace these with validated versions.)

- [ ] **Step 4: Add `RscLevel` enum**

Path: `src/Sport.Core/Structure/RscLevel.cs`
```csharp
namespace Sport.Core.Structure;

public enum RscLevel
{
    Discipline,
    Event,
    Phase,
    Unit,
    Subunit,
}
```

- [ ] **Step 5: Extend `Rsc` with `Compose` and `Level`**

Modify `src/Sport.Core/Structure/Rsc.cs` to add (after the existing `Validate`):

```csharp
// Slot offsets within the 34-char layout:
//   [0..3)   Discipline (3)
//   [3..4)   Gender     (1)
//   [4..12)  EventType  (8)
//   [12..22) EventMod   (10)
//   [22..26) Phase      (4)
//   [26..32) Unit (6 chars used by core; ATH may extend)
//   [32..34) SubUnit slot

public RscLevel Level => ComputeLevel(Value);

public static Rsc Compose(
    DisciplineCode discipline,
    GenderCode gender,
    EventTypeCode eventType,
    EventModifierCode? modifier,
    PhaseCode? phase,
    UnitCode? unit,
    SubunitCode? subunit)
{
    Span<char> buf = stackalloc char[Length];
    buf.Fill(Filler);

    discipline.Value.AsSpan().CopyTo(buf[..3]);
    buf[3] = gender.ToRscChar();
    PadInto(eventType.Value, buf[4..12]);
    if (modifier is { } m) PadInto(m.Value, buf[12..22]);

    if (phase is { } p) PadInto(p.Value, buf[22..26]);
    if (unit is { } u) PadInto(u.Value, buf[26..34]);

    if (subunit is { } s)
    {
        // Subunit overrides the trailing 2 chars of the unit slot.
        var sub = s.Value;
        if (sub.Length != 2)
            throw new ArgumentException("SubunitCode must be exactly 2 characters.", nameof(subunit));
        buf[32] = sub[0];
        buf[33] = sub[1];
    }

    return From(new string(buf));
}

private static void PadInto(string source, Span<char> dest)
{
    if (source.Length > dest.Length)
        throw new ArgumentException($"Value '{source}' too long for {dest.Length}-char slot.");
    source.AsSpan().CopyTo(dest[..source.Length]);
    // Remaining chars already filled with '-' by buf.Fill(Filler).
}

private static RscLevel ComputeLevel(string value)
{
    static bool SlotFilled(string s, Range r)
    {
        foreach (var c in s.AsSpan()[r])
            if (c != Filler) return true;
        return false;
    }

    var hasEvent   = SlotFilled(value, 4..22);
    var hasPhase   = SlotFilled(value, 22..26);
    var hasUnit    = SlotFilled(value, 26..32);
    var hasSubunit = SlotFilled(value, 32..34);

    if (hasSubunit) return RscLevel.Subunit;
    if (hasUnit) return RscLevel.Unit;
    if (hasPhase) return RscLevel.Phase;
    if (hasEvent) return RscLevel.Event;
    return RscLevel.Discipline;
}
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~RscCompositionTests"`
Expected: 4 tests pass.

- [ ] **Step 7: Commit**

```
git add src/Sport.Core/Structure/ tests/Sport.Core.Tests/Structure/
git commit -m "feat(core): add Rsc.Compose and RscLevel extraction"
```

---

## Phase D — Discipline registry contracts

> Phases D builds the plug-in contract before the entities that depend on it, so all subsequent invariants can call into the registry.

### Task 6: Descriptor types (`EventTypeDescriptor`, `ModifierContract`, `IPhaseCatalog`, `IUnitCodeStrategy`, `IEntryRules`, `EntryCandidate`, `FunctionDescriptor`)

**Files:**
- Create: `src/Sport.Core/DisciplineRegistry/EventTypeDescriptor.cs`
- Create: `src/Sport.Core/DisciplineRegistry/ModifierContract.cs`
- Create: `src/Sport.Core/DisciplineRegistry/IPhaseCatalog.cs`
- Create: `src/Sport.Core/DisciplineRegistry/IUnitCodeStrategy.cs`
- Create: `src/Sport.Core/DisciplineRegistry/IEntryRules.cs`
- Create: `src/Sport.Core/DisciplineRegistry/EntryCandidate.cs`
- Create: `src/Sport.Core/DisciplineRegistry/FunctionDescriptor.cs`
- Create: `src/Sport.Core/Officials/FunctionCode.cs`
- Create: `src/Sport.Core/Officials/ScopeLevel.cs`
- Create: `src/Sport.Core/Participants/EntryType.cs`

> Note: `EntryType`, `ScopeLevel`, and `FunctionCode` are referenced by the contract types, so they live in their natural submodule folders but are introduced here.

- [ ] **Step 1: Define `EntryType` enum**

Path: `src/Sport.Core/Participants/EntryType.cs`
```csharp
namespace Sport.Core.Participants;

public enum EntryType
{
    Athlete,
    Team,
    Group,
}
```

- [ ] **Step 2: Define `ScopeLevel` enum**

Path: `src/Sport.Core/Officials/ScopeLevel.cs`
```csharp
namespace Sport.Core.Officials;

public enum ScopeLevel
{
    Competition,
    CompetitionDiscipline,
    Event,
    Phase,
    Unit,
}
```

- [ ] **Step 3: Define `FunctionCode` value object**

Path: `src/Sport.Core/Officials/FunctionCode.cs`
```csharp
using Vogen;

namespace Sport.Core.Officials;

[ValueObject<string>]
public readonly partial struct FunctionCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return Validation.Invalid("FunctionCode is required.");
        if (value.Length > 20) return Validation.Invalid("FunctionCode must be at most 20 characters.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 4: Define `ModifierContract`**

Path: `src/Sport.Core/DisciplineRegistry/ModifierContract.cs`
```csharp
namespace Sport.Core.DisciplineRegistry;

public enum ModifierContract
{
    Forbidden,
    Optional,
    Required,
}
```

- [ ] **Step 5: Define `EventTypeDescriptor`**

Path: `src/Sport.Core/DisciplineRegistry/EventTypeDescriptor.cs`
```csharp
using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public sealed record EventTypeDescriptor(
    EventTypeCode Code,
    string DisplayName,
    IReadOnlySet<GenderCode> AppliesToGenders,
    ModifierContract ModifierContract);
```

- [ ] **Step 6: Define `IPhaseCatalog`**

Path: `src/Sport.Core/DisciplineRegistry/IPhaseCatalog.cs`
```csharp
using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public interface IPhaseCatalog
{
    IReadOnlyCollection<PhaseCode> AllowedPhases { get; }
    bool IsAllowedForEventType(EventTypeCode eventType, PhaseCode phase);
}
```

- [ ] **Step 7: Define `IUnitCodeStrategy`**

Path: `src/Sport.Core/DisciplineRegistry/IUnitCodeStrategy.cs`
```csharp
using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public interface IUnitCodeStrategy
{
    UnitCode NextUnitCode(IEnumerable<UnitCode> existing);
    bool IsValid(UnitCode code);
}
```

- [ ] **Step 8: Define `EntryCandidate` and `IEntryRules`**

Path: `src/Sport.Core/DisciplineRegistry/EntryCandidate.cs`
```csharp
using Sport.Core.Participants;

namespace Sport.Core.DisciplineRegistry;

public sealed record EntryCandidate(
    EntryType Type,
    int CompositionSize,
    bool HasTeam,
    bool HasOrganisation);
```

Path: `src/Sport.Core/DisciplineRegistry/IEntryRules.cs`
```csharp
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Core.DisciplineRegistry;

public interface IEntryRules
{
    IReadOnlyCollection<EntryType> AllowedTypes { get; }
    (int Min, int Max) CompositionSize(EntryType type);
    Result Validate(EntryCandidate candidate);
}
```

- [ ] **Step 9: Define `FunctionDescriptor`**

Path: `src/Sport.Core/DisciplineRegistry/FunctionDescriptor.cs`
```csharp
using Sport.Core.Officials;

namespace Sport.Core.DisciplineRegistry;

public sealed record FunctionDescriptor(
    FunctionCode Code,
    string DisplayName,
    IReadOnlySet<ScopeLevel> ValidScopes,
    bool IsTeamOfficial,
    bool RequiresOrganisation);
```

- [ ] **Step 10: Build to verify everything compiles**

Run: `dotnet build Sport.sln`
Expected: build succeeds.

- [ ] **Step 11: Commit**

```
git add src/Sport.Core/DisciplineRegistry/ src/Sport.Core/Officials/ src/Sport.Core/Participants/EntryType.cs
git commit -m "feat(core): add discipline registry descriptor and contract types"
```

---

### Task 7: Replace placeholder structure VOs with full validation

**Files:**
- Modify: `src/Sport.Core/Structure/EventTypeCode.cs`
- Modify: `src/Sport.Core/Structure/EventModifierCode.cs`
- Modify: `src/Sport.Core/Structure/PhaseCode.cs`
- Modify: `src/Sport.Core/Structure/UnitCode.cs`
- Modify: `src/Sport.Core/Structure/SubunitCode.cs`
- Create: `tests/Sport.Core.Tests/Structure/StructureValueObjectsTests.cs`

- [ ] **Step 1: Write failing tests covering each VO's length/charset rules**

Path: `tests/Sport.Core.Tests/Structure/StructureValueObjectsTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Structure;
using Vogen;
using Xunit;

namespace Sport.Core.Tests.Structure;

public class StructureValueObjectsTests
{
    [Theory]
    [InlineData("TEAM11")]
    [InlineData("HJ")]
    [InlineData("75KG")]
    public void EventTypeCode_accepts_1_to_8_uppercase_alphanumeric(string value)
        => ((Action)(() => EventTypeCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("")]
    [InlineData("verylongcode")]
    [InlineData("team11")]
    public void EventTypeCode_rejects_invalid(string value)
        => ((Action)(() => EventTypeCode.From(value))).Should().Throw<ValueObjectValidationException>();

    [Theory]
    [InlineData("QFNL")]
    [InlineData("GPA")]
    [InlineData("FNL")]
    public void PhaseCode_accepts_1_to_4_uppercase_alphanumeric(string value)
        => ((Action)(() => PhaseCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("QFNLZ")] // too long
    [InlineData("qfnl")]
    public void PhaseCode_rejects_invalid(string value)
        => ((Action)(() => PhaseCode.From(value))).Should().Throw<ValueObjectValidationException>();

    [Theory]
    [InlineData("000100--")]
    [InlineData("0001SJ--")]
    [InlineData("00010000")]
    public void UnitCode_accepts_exactly_8_chars_in_allowed_charset(string value)
        => ((Action)(() => UnitCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("000100-")]   // too short
    [InlineData("000100---")] // too long
    [InlineData("00010o--")] // lowercase
    public void UnitCode_rejects_invalid(string value)
        => ((Action)(() => UnitCode.From(value))).Should().Throw<ValueObjectValidationException>();

    [Theory]
    [InlineData("01")]
    [InlineData("SJ")]
    public void SubunitCode_accepts_exactly_2_chars(string value)
        => ((Action)(() => SubunitCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("1")]
    [InlineData("111")]
    public void SubunitCode_rejects_invalid_length(string value)
        => ((Action)(() => SubunitCode.From(value))).Should().Throw<ValueObjectValidationException>();
}
```

- [ ] **Step 2: Run the tests (fail — current placeholders accept anything)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~StructureValueObjectsTests"`
Expected: tests fail.

- [ ] **Step 3: Replace `EventTypeCode.cs`**

Path: `src/Sport.Core/Structure/EventTypeCode.cs`
```csharp
using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct EventTypeCode
{
    public const int MaxLength = 8;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("EventTypeCode is required.");
        if (value.Length is < 1 or > MaxLength)
            return Validation.Invalid($"EventTypeCode must be 1..{MaxLength} characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9'))
                return Validation.Invalid("EventTypeCode must be uppercase alphanumeric.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 4: Replace `EventModifierCode.cs`**

Path: `src/Sport.Core/Structure/EventModifierCode.cs`
```csharp
using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct EventModifierCode
{
    public const int MaxLength = 10;

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("EventModifierCode is required.");
        if (value.Length > MaxLength)
            return Validation.Invalid($"EventModifierCode must be at most {MaxLength} characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9'))
                return Validation.Invalid("EventModifierCode must be uppercase alphanumeric.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 5: Replace `PhaseCode.cs`**

Path: `src/Sport.Core/Structure/PhaseCode.cs`
```csharp
using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct PhaseCode
{
    public const int MaxLength = 4;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("PhaseCode is required.");
        if (value.Length is < 1 or > MaxLength)
            return Validation.Invalid($"PhaseCode must be 1..{MaxLength} characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9'))
                return Validation.Invalid("PhaseCode must be uppercase alphanumeric.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 6: Replace `UnitCode.cs`**

Path: `src/Sport.Core/Structure/UnitCode.cs`
```csharp
using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct UnitCode
{
    public const int Length = 8;

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("UnitCode is required.");
        if (value.Length != Length)
            return Validation.Invalid($"UnitCode must be exactly {Length} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z' || c is >= '0' and <= '9' || c == '-';
            if (!ok) return Validation.Invalid("UnitCode chars must be uppercase alphanumeric or '-'.");
        }
        return Validation.Ok;
    }
}
```

- [ ] **Step 7: Replace `SubunitCode.cs`**

Path: `src/Sport.Core/Structure/SubunitCode.cs`
```csharp
using Vogen;

namespace Sport.Core.Structure;

[ValueObject<string>]
public readonly partial struct SubunitCode
{
    public const int Length = 2;

    private static Validation Validate(string value)
    {
        if (value is null) return Validation.Invalid("SubunitCode is required.");
        if (value.Length != Length)
            return Validation.Invalid($"SubunitCode must be exactly {Length} characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9'))
                return Validation.Invalid("SubunitCode chars must be uppercase alphanumeric.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 8: Run tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj`
Expected: all tests pass (RscValidationTests, RscCompositionTests, StructureValueObjectsTests, etc.).

- [ ] **Step 9: Commit**

```
git add src/Sport.Core/Structure/ tests/Sport.Core.Tests/Structure/StructureValueObjectsTests.cs
git commit -m "feat(core): validate Structure value objects (EventType/EventMod/Phase/Unit/Subunit codes)"
```

---

### Task 8: `IDisciplineModule` interface

**Files:**
- Create: `src/Sport.Core/DisciplineRegistry/IDisciplineModule.cs`

- [ ] **Step 1: Implement `IDisciplineModule`**

Path: `src/Sport.Core/DisciplineRegistry/IDisciplineModule.cs`
```csharp
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public interface IDisciplineModule
{
    DisciplineCode Code { get; }
    string DisplayName { get; }
    IReadOnlySet<GenderCode> SupportedGenders { get; }

    IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; }
    IPhaseCatalog PhaseCatalog { get; }
    IUnitCodeStrategy UnitCodeStrategy { get; }
    IReadOnlyCollection<FunctionDescriptor> Functions { get; }
    IEntryRules EntryRules { get; }

    Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier);
    Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase);
    Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code);
    Result ValidateEntry(EntryCandidate candidate);
    Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level);
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build Sport.sln`
Expected: succeeds.

- [ ] **Step 3: Commit**

```
git add src/Sport.Core/DisciplineRegistry/IDisciplineModule.cs
git commit -m "feat(core): add IDisciplineModule contract"
```

---

### Task 9: `CommonFunctions` (Coach / Manager / Medical) and `IDisciplineRegistry`

**Files:**
- Create: `src/Sport.Core/DisciplineRegistry/CommonFunctions.cs`
- Create: `src/Sport.Core/DisciplineRegistry/IDisciplineRegistry.cs`
- Create: `src/Sport.Core/DisciplineRegistry/DisciplineRegistry.cs`
- Create: `tests/Sport.Core.Tests/DisciplineRegistry/DisciplineRegistryTests.cs`

- [ ] **Step 1: Write failing tests for the registry**

Path: `tests/Sport.Core.Tests/DisciplineRegistry/DisciplineRegistryTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;
using Xunit;

namespace Sport.Core.Tests.DisciplineRegistry;

public class DisciplineRegistryTests
{
    [Fact]
    public void Register_then_get_returns_module()
    {
        var module = new FakeModule(DisciplineCode.From("FBL"));
        var registry = new DisciplineRegistry();
        registry.Register(module);

        registry.IsRegistered(DisciplineCode.From("FBL")).Should().BeTrue();
        registry.Get(DisciplineCode.From("FBL")).Should().BeSameAs(module);
    }

    [Fact]
    public void Registering_the_same_discipline_twice_throws()
    {
        var registry = new DisciplineRegistry();
        registry.Register(new FakeModule(DisciplineCode.From("FBL")));

        var act = () => registry.Register(new FakeModule(DisciplineCode.From("FBL")));
        act.Should().Throw<DomainException>().WithMessage("*already registered*");
    }

    [Fact]
    public void Get_unknown_discipline_throws()
    {
        var registry = new DisciplineRegistry();
        var act = () => registry.Get(DisciplineCode.From("XXX"));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CommonFunctions_exposes_coach_manager_medical()
    {
        var codes = CommonFunctions.All.Select(f => f.Code.Value).ToHashSet();
        codes.Should().Contain(new[] { "COMMON.COACH", "COMMON.MANAGER", "COMMON.MEDICAL" });
    }

    private sealed class FakeModule(DisciplineCode code) : IDisciplineModule
    {
        public DisciplineCode Code { get; } = code;
        public string DisplayName => "Fake";
        public IReadOnlySet<GenderCode> SupportedGenders { get; } = new HashSet<GenderCode> { GenderCode.M };
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => [];
        public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
        public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
        public IReadOnlyCollection<FunctionDescriptor> Functions => [];
        public IEntryRules EntryRules => throw new NotImplementedException();
        public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => Result.Ok();
        public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => Result.Ok();
        public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Result.Ok();
        public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
        public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Result.Ok();
    }
}
```

- [ ] **Step 2: Run tests (fail — registry/common functions missing)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~DisciplineRegistryTests"`
Expected: build fails.

- [ ] **Step 3: Implement `CommonFunctions`**

Path: `src/Sport.Core/DisciplineRegistry/CommonFunctions.cs`
```csharp
using Sport.Core.Officials;

namespace Sport.Core.DisciplineRegistry;

public static class CommonFunctions
{
    public static FunctionDescriptor Coach { get; } = new(
        FunctionCode.From("COMMON.COACH"),
        "Coach",
        new HashSet<ScopeLevel> { ScopeLevel.Event, ScopeLevel.Unit, ScopeLevel.CompetitionDiscipline },
        IsTeamOfficial: true,
        RequiresOrganisation: true);

    public static FunctionDescriptor Manager { get; } = new(
        FunctionCode.From("COMMON.MANAGER"),
        "Manager",
        new HashSet<ScopeLevel> { ScopeLevel.Event, ScopeLevel.CompetitionDiscipline, ScopeLevel.Competition },
        IsTeamOfficial: true,
        RequiresOrganisation: true);

    public static FunctionDescriptor Medical { get; } = new(
        FunctionCode.From("COMMON.MEDICAL"),
        "Medical Officer",
        new HashSet<ScopeLevel> { ScopeLevel.Event, ScopeLevel.Unit, ScopeLevel.Competition },
        IsTeamOfficial: true,
        RequiresOrganisation: false);

    public static IReadOnlyCollection<FunctionDescriptor> All { get; } =
        new[] { Coach, Manager, Medical };
}
```

- [ ] **Step 4: Implement `IDisciplineRegistry`**

Path: `src/Sport.Core/DisciplineRegistry/IDisciplineRegistry.cs`
```csharp
namespace Sport.Core.DisciplineRegistry;

public interface IDisciplineRegistry
{
    IDisciplineModule Get(DisciplineCode code);
    bool IsRegistered(DisciplineCode code);
    IReadOnlyCollection<DisciplineCode> RegisteredCodes { get; }
    IReadOnlyCollection<FunctionDescriptor> CommonFunctions { get; }
}
```

- [ ] **Step 5: Implement `DisciplineRegistry`**

Path: `src/Sport.Core/DisciplineRegistry/DisciplineRegistry.cs`
```csharp
using Sport.Core.Shared;

namespace Sport.Core.DisciplineRegistry;

public sealed class DisciplineRegistry : IDisciplineRegistry
{
    private readonly Dictionary<DisciplineCode, IDisciplineModule> _modules = new();

    public void Register(IDisciplineModule module)
    {
        if (_modules.ContainsKey(module.Code))
            throw new DomainException($"Discipline '{module.Code.Value}' is already registered.");
        _modules[module.Code] = module;
    }

    public IDisciplineModule Get(DisciplineCode code) =>
        _modules.TryGetValue(code, out var m)
            ? m
            : throw new DomainException($"Discipline '{code.Value}' is not registered.");

    public bool IsRegistered(DisciplineCode code) => _modules.ContainsKey(code);

    public IReadOnlyCollection<DisciplineCode> RegisteredCodes => _modules.Keys.ToArray();

    public IReadOnlyCollection<FunctionDescriptor> CommonFunctions => DisciplineRegistry_CommonFunctions;

    private static readonly IReadOnlyCollection<FunctionDescriptor> DisciplineRegistry_CommonFunctions
        = Sport.Core.DisciplineRegistry.CommonFunctions.All;
}
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~DisciplineRegistryTests"`
Expected: 4 tests pass.

- [ ] **Step 7: Commit**

```
git add src/Sport.Core/DisciplineRegistry/ tests/Sport.Core.Tests/DisciplineRegistry/DisciplineRegistryTests.cs
git commit -m "feat(core): add CommonFunctions, IDisciplineRegistry and DisciplineRegistry"
```

---

### Task 10: `ServiceCollectionExtensions` (`AddSportCore`, `AddDisciplineModule<T>`)

**Files:**
- Create: `src/Sport.Core/DisciplineRegistry/ServiceCollectionExtensions.cs`
- Create: `tests/Sport.Core.Tests/DisciplineRegistry/ServiceCollectionExtensionsTests.cs`

- [ ] **Step 1: Write failing tests**

Path: `tests/Sport.Core.Tests/DisciplineRegistry/ServiceCollectionExtensionsTests.cs`
```csharp
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;
using Xunit;

namespace Sport.Core.Tests.DisciplineRegistry;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSportCore_registers_a_singleton_registry()
    {
        var services = new ServiceCollection();
        services.AddSportCore();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IDisciplineRegistry>();
        registry.RegisteredCodes.Should().BeEmpty();
    }

    [Fact]
    public void AddDisciplineModule_registers_module_into_registry()
    {
        var services = new ServiceCollection();
        services.AddSportCore();
        services.AddDisciplineModule<FakeFblModule>();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IDisciplineRegistry>();

        registry.IsRegistered(DisciplineCode.From("FBL")).Should().BeTrue();
    }

    private sealed class FakeFblModule : IDisciplineModule
    {
        public DisciplineCode Code { get; } = DisciplineCode.From("FBL");
        public string DisplayName => "Football";
        public IReadOnlySet<GenderCode> SupportedGenders { get; } = new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => [];
        public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
        public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
        public IReadOnlyCollection<FunctionDescriptor> Functions => [];
        public IEntryRules EntryRules => throw new NotImplementedException();
        public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => Result.Ok();
        public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => Result.Ok();
        public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Result.Ok();
        public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
        public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Result.Ok();
    }
}
```

- [ ] **Step 2: Run tests (fail — extension methods missing)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~ServiceCollectionExtensionsTests"`
Expected: build fails.

- [ ] **Step 3: Implement extensions**

Path: `src/Sport.Core/DisciplineRegistry/ServiceCollectionExtensions.cs`
```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Sport.Core.DisciplineRegistry;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSportCore(this IServiceCollection services)
    {
        services.AddSingleton<DisciplineRegistry>();
        services.AddSingleton<IDisciplineRegistry>(sp => sp.GetRequiredService<DisciplineRegistry>());
        return services;
    }

    public static IServiceCollection AddDisciplineModule<TModule>(this IServiceCollection services)
        where TModule : class, IDisciplineModule, new()
    {
        services.AddSingleton<IDisciplineModule, TModule>();

        // Eagerly register into the concrete registry on first resolution.
        services.AddSingleton<ModuleRegistrationHook<TModule>>();
        return services;
    }

    private sealed class ModuleRegistrationHook<TModule>
        where TModule : class, IDisciplineModule, new()
    {
        public ModuleRegistrationHook(DisciplineRegistry registry, IEnumerable<IDisciplineModule> allModules)
        {
            foreach (var m in allModules)
                if (!registry.IsRegistered(m.Code))
                    registry.Register(m);
        }
    }
}
```

> Note: the hook fires when the host requests `ModuleRegistrationHook<TModule>` (or any module). For deterministic ordering we also expose a `BuildRegistry` helper that the host can call explicitly during startup. Add that helper in the next step.

- [ ] **Step 4: Add explicit `BuildSportRegistry` extension to force registration**

Add inside the same file, in the `ServiceCollectionExtensions` class:
```csharp
public static IDisciplineRegistry BuildSportRegistry(this IServiceProvider provider)
{
    var registry = provider.GetRequiredService<DisciplineRegistry>();
    foreach (var module in provider.GetServices<IDisciplineModule>())
        if (!registry.IsRegistered(module.Code))
            registry.Register(module);
    return registry;
}
```

- [ ] **Step 5: Update the failing test to call `BuildSportRegistry` so registration is deterministic**

Modify `ServiceCollectionExtensionsTests.AddDisciplineModule_registers_module_into_registry`:
```csharp
[Fact]
public void AddDisciplineModule_registers_module_into_registry()
{
    var services = new ServiceCollection();
    services.AddSportCore();
    services.AddDisciplineModule<FakeFblModule>();

    var provider = services.BuildServiceProvider();
    var registry = provider.BuildSportRegistry();

    registry.IsRegistered(DisciplineCode.From("FBL")).Should().BeTrue();
}
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~ServiceCollectionExtensionsTests"`
Expected: tests pass.

- [ ] **Step 7: Commit**

```
git add src/Sport.Core/DisciplineRegistry/ServiceCollectionExtensions.cs tests/Sport.Core.Tests/DisciplineRegistry/ServiceCollectionExtensionsTests.cs
git commit -m "feat(core): add AddSportCore and AddDisciplineModule DI extensions"
```

---

## Phase E — Competitions submodule

### Task 11: Competitions value objects (`CompetitionId`, `CompetitionDisciplineId`, `CompetitionCode`, `DateRange`)

**Files:**
- Create: `src/Sport.Core/Competitions/CompetitionId.cs`
- Create: `src/Sport.Core/Competitions/CompetitionDisciplineId.cs`
- Create: `src/Sport.Core/Competitions/CompetitionCode.cs`
- Create: `src/Sport.Core/Competitions/DateRange.cs`
- Create: `tests/Sport.Core.Tests/Competitions/DateRangeTests.cs`
- Create: `tests/Sport.Core.Tests/Competitions/CompetitionCodeTests.cs`

- [ ] **Step 1: Write failing tests for `DateRange`**

Path: `tests/Sport.Core.Tests/Competitions/DateRangeTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Competitions;

public class DateRangeTests
{
    [Fact]
    public void Start_equal_to_end_is_a_single_day_range()
    {
        var d = new DateOnly(2026, 6, 1);
        var range = DateRange.Create(d, d);
        range.Days.Should().Be(1);
    }

    [Fact]
    public void Start_after_end_throws()
    {
        var act = () => DateRange.Create(new DateOnly(2026, 6, 2), new DateOnly(2026, 6, 1));
        act.Should().Throw<DomainException>();
    }
}
```

- [ ] **Step 2: Write failing tests for `CompetitionCode`**

Path: `tests/Sport.Core.Tests/Competitions/CompetitionCodeTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Vogen;
using Xunit;

namespace Sport.Core.Tests.Competitions;

public class CompetitionCodeTests
{
    [Theory]
    [InlineData("copa-2026")]
    [InlineData("torneo-clausura")]
    [InlineData("c1")]
    public void Accepts_lowercase_kebab_slugs(string value)
        => ((Action)(() => CompetitionCode.From(value))).Should().NotThrow();

    [Theory]
    [InlineData("")]
    [InlineData("Copa-2026")]
    [InlineData("copa 2026")]
    [InlineData("copa_2026")]
    public void Rejects_invalid_input(string value)
        => ((Action)(() => CompetitionCode.From(value))).Should().Throw<ValueObjectValidationException>();
}
```

- [ ] **Step 3: Run tests (fail — types missing)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~Competitions"`
Expected: build fails.

- [ ] **Step 4: Implement `CompetitionId`**

Path: `src/Sport.Core/Competitions/CompetitionId.cs`
```csharp
using Vogen;

namespace Sport.Core.Competitions;

[ValueObject<Guid>]
public readonly partial struct CompetitionId
{
    public static CompetitionId New() => From(Guid.CreateVersion7());
}
```

- [ ] **Step 5: Implement `CompetitionDisciplineId`**

Path: `src/Sport.Core/Competitions/CompetitionDisciplineId.cs`
```csharp
using Vogen;

namespace Sport.Core.Competitions;

[ValueObject<Guid>]
public readonly partial struct CompetitionDisciplineId
{
    public static CompetitionDisciplineId New() => From(Guid.CreateVersion7());
}
```

- [ ] **Step 6: Implement `CompetitionCode`**

Path: `src/Sport.Core/Competitions/CompetitionCode.cs`
```csharp
using Vogen;

namespace Sport.Core.Competitions;

[ValueObject<string>]
public readonly partial struct CompetitionCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("CompetitionCode is required.");
        if (value.Length is < 1 or > 64)
            return Validation.Invalid("CompetitionCode must be 1..64 characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'a' and <= 'z' || c is >= '0' and <= '9' || c == '-';
            if (!ok) return Validation.Invalid("CompetitionCode must be lowercase kebab-case (a-z, 0-9, '-').");
        }
        if (value[0] == '-' || value[^1] == '-')
            return Validation.Invalid("CompetitionCode cannot start or end with '-'.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 7: Implement `DateRange`**

Path: `src/Sport.Core/Competitions/DateRange.cs`
```csharp
using Sport.Core.Shared;

namespace Sport.Core.Competitions;

public readonly record struct DateRange
{
    public DateOnly Start { get; }
    public DateOnly End { get; }
    public int Days => End.DayNumber - Start.DayNumber + 1;

    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Create(DateOnly start, DateOnly end)
    {
        if (start > end)
            throw new DomainException("DateRange.Start must be on or before DateRange.End.");
        return new DateRange(start, end);
    }
}
```

- [ ] **Step 8: Run tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~Competitions"`
Expected: pass.

- [ ] **Step 9: Commit**

```
git add src/Sport.Core/Competitions/ tests/Sport.Core.Tests/Competitions/
git commit -m "feat(core): add Competitions value objects (Id, Code, DateRange)"
```

---

### Task 12: `CompetitionDiscipline` entity

**Files:**
- Create: `src/Sport.Core/Competitions/CompetitionDiscipline.cs`
- Create: `tests/Sport.Core.Tests/Competitions/CompetitionDisciplineTests.cs`

- [ ] **Step 1: Write failing tests**

Path: `tests/Sport.Core.Tests/Competitions/CompetitionDisciplineTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Competitions;

public class CompetitionDisciplineTests
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");

    [Fact]
    public void Create_with_valid_genders_succeeds()
    {
        var cd = CompetitionDiscipline.Create(
            CompetitionDisciplineId.New(),
            CompetitionId.New(),
            Fbl,
            new HashSet<GenderCode> { GenderCode.M });

        cd.Code.Should().Be(Fbl);
        cd.EnabledGenders.Should().ContainSingle().Which.Should().Be(GenderCode.M);
    }

    [Fact]
    public void Create_with_empty_enabled_genders_throws()
    {
        var act = () => CompetitionDiscipline.Create(
            CompetitionDisciplineId.New(),
            CompetitionId.New(),
            Fbl,
            new HashSet<GenderCode>());
        act.Should().Throw<DomainException>().WithMessage("*at least one gender*");
    }
}
```

- [ ] **Step 2: Run tests (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~CompetitionDisciplineTests"`
Expected: build fails.

- [ ] **Step 3: Implement `CompetitionDiscipline`**

Path: `src/Sport.Core/Competitions/CompetitionDiscipline.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Competitions;

public sealed class CompetitionDiscipline
{
    public CompetitionDisciplineId Id { get; }
    public CompetitionId CompetitionId { get; }
    public DisciplineCode Code { get; }
    public IReadOnlySet<GenderCode> EnabledGenders { get; }

    private CompetitionDiscipline(
        CompetitionDisciplineId id,
        CompetitionId competitionId,
        DisciplineCode code,
        IReadOnlySet<GenderCode> enabledGenders)
    {
        Id = id;
        CompetitionId = competitionId;
        Code = code;
        EnabledGenders = enabledGenders;
    }

    public static CompetitionDiscipline Create(
        CompetitionDisciplineId id,
        CompetitionId competitionId,
        DisciplineCode code,
        IReadOnlySet<GenderCode> enabledGenders)
    {
        if (enabledGenders is null || enabledGenders.Count == 0)
            throw new DomainException("A CompetitionDiscipline must enable at least one gender.");
        return new CompetitionDiscipline(id, competitionId, code, enabledGenders);
    }
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~CompetitionDisciplineTests"`
Expected: pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Core/Competitions/CompetitionDiscipline.cs tests/Sport.Core.Tests/Competitions/CompetitionDisciplineTests.cs
git commit -m "feat(core): add CompetitionDiscipline entity (I-COMP-2/I-COMP-4 partial)"
```

---

### Task 13: `Competition` aggregate root

**Files:**
- Create: `src/Sport.Core/Competitions/Competition.cs`
- Create: `tests/Sport.Core.Tests/Competitions/CompetitionTests.cs`

- [ ] **Step 1: Write failing tests for `Competition` invariants (I-COMP-1, I-COMP-3, I-COMP-4)**

Path: `tests/Sport.Core.Tests/Competitions/CompetitionTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Competitions;

public class CompetitionTests
{
    private static readonly DateOnly D1 = new(2026, 6, 1);
    private static readonly DateOnly D2 = new(2026, 6, 10);
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private static readonly DisciplineCode Bkb = DisciplineCode.From("BKB");

    private static FakeRegistry MakeRegistry(params DisciplineCode[] supported)
    {
        var reg = new FakeRegistry();
        foreach (var d in supported) reg.SupportedCodes.Add(d);
        reg.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
        reg.GendersByCode[Bkb] = new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
        return reg;
    }

    [Fact]
    public void Create_with_one_registered_discipline_succeeds()
    {
        var registry = MakeRegistry(Fbl);
        var comp = Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa 2026",
            DateRange.Create(D1, D2),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);

        comp.Disciplines.Should().HaveCount(1);
    }

    [Fact]
    public void Create_with_zero_disciplines_throws_I_COMP_1()
    {
        var registry = MakeRegistry(Fbl);
        var act = () => Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa",
            DateRange.Create(D1, D2),
            Array.Empty<(DisciplineCode, IReadOnlySet<GenderCode>)>(),
            registry);
        act.Should().Throw<DomainException>().WithMessage("*at least 1*");
    }

    [Fact]
    public void Create_with_unregistered_discipline_throws_I_COMP_2()
    {
        var registry = MakeRegistry(Fbl); // BKB not registered
        var act = () => Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa",
            DateRange.Create(D1, D2),
            new[] { (Bkb, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);
        act.Should().Throw<DomainException>().WithMessage("*not registered*");
    }

    [Fact]
    public void Create_with_duplicate_discipline_throws_I_COMP_3()
    {
        var registry = MakeRegistry(Fbl);
        var act = () => Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa",
            DateRange.Create(D1, D2),
            new[]
            {
                (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }),
                (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.W }),
            },
            registry);
        act.Should().Throw<DomainException>().WithMessage("*duplicate*");
    }

    [Fact]
    public void Create_with_unsupported_gender_throws_I_COMP_4()
    {
        var registry = MakeRegistry(Fbl);
        registry.GendersByCode[Fbl] = new HashSet<GenderCode> { GenderCode.M }; // W not supported by module
        var act = () => Competition.Create(
            CompetitionId.New(),
            CompetitionCode.From("copa-2026"),
            "Copa",
            DateRange.Create(D1, D2),
            new[] { (Fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.W }) },
            registry);
        act.Should().Throw<DomainException>().WithMessage("*not supported*");
    }

    private sealed class FakeRegistry : IDisciplineRegistry
    {
        public HashSet<DisciplineCode> SupportedCodes { get; } = new();
        public Dictionary<DisciplineCode, IReadOnlySet<GenderCode>> GendersByCode { get; } = new();

        public IDisciplineModule Get(DisciplineCode code) =>
            new FakeModule(code, GendersByCode[code]);
        public bool IsRegistered(DisciplineCode code) => SupportedCodes.Contains(code);
        public IReadOnlyCollection<DisciplineCode> RegisteredCodes => SupportedCodes.ToArray();
        public IReadOnlyCollection<FunctionDescriptor> CommonFunctions => Array.Empty<FunctionDescriptor>();

        private sealed class FakeModule(DisciplineCode code, IReadOnlySet<GenderCode> genders) : IDisciplineModule
        {
            public DisciplineCode Code { get; } = code;
            public string DisplayName => "fake";
            public IReadOnlySet<GenderCode> SupportedGenders { get; } = genders;
            public IReadOnlyCollection<EventTypeDescriptor> EventTypes => [];
            public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
            public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
            public IReadOnlyCollection<FunctionDescriptor> Functions => [];
            public IEntryRules EntryRules => throw new NotImplementedException();
            public Result ValidateEventType(Structure.EventTypeCode type, Structure.EventModifierCode? modifier) => Result.Ok();
            public Result ValidatePhaseForEventType(Structure.EventTypeCode type, Structure.PhaseCode phase) => Result.Ok();
            public Result ValidateUnitCode(Structure.EventTypeCode type, Structure.PhaseCode phase, Structure.UnitCode code) => Result.Ok();
            public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
            public Result ValidateOfficialFunctionInScope(Officials.FunctionCode function, Officials.ScopeLevel level) => Result.Ok();
        }
    }
}
```

- [ ] **Step 2: Run tests (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~CompetitionTests"`
Expected: build fails.

- [ ] **Step 3: Implement `Competition`**

Path: `src/Sport.Core/Competitions/Competition.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Competitions;

public sealed class Competition
{
    public CompetitionId Id { get; }
    public CompetitionCode Code { get; }
    public string Name { get; }
    public DateRange Dates { get; }

    private readonly List<CompetitionDiscipline> _disciplines;
    public IReadOnlyList<CompetitionDiscipline> Disciplines => _disciplines;

    private Competition(
        CompetitionId id,
        CompetitionCode code,
        string name,
        DateRange dates,
        List<CompetitionDiscipline> disciplines)
    {
        Id = id;
        Code = code;
        Name = name;
        Dates = dates;
        _disciplines = disciplines;
    }

    public static Competition Create(
        CompetitionId id,
        CompetitionCode code,
        string name,
        DateRange dates,
        IReadOnlyCollection<(DisciplineCode Code, IReadOnlySet<GenderCode> Genders)> disciplines,
        IDisciplineRegistry registry)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Competition.Name is required.");
        if (disciplines is null || disciplines.Count < 1)
            throw new DomainException("A Competition must have at least 1 discipline (I-COMP-1).");

        var seen = new HashSet<DisciplineCode>();
        var children = new List<CompetitionDiscipline>(disciplines.Count);

        foreach (var (disciplineCode, genders) in disciplines)
        {
            if (!seen.Add(disciplineCode))
                throw new DomainException($"Duplicate discipline '{disciplineCode.Value}' in competition (I-COMP-3).");

            if (!registry.IsRegistered(disciplineCode))
                throw new DomainException($"Discipline '{disciplineCode.Value}' is not registered in the DisciplineRegistry (I-COMP-2).");

            var module = registry.Get(disciplineCode);
            foreach (var g in genders)
                if (!module.SupportedGenders.Contains(g))
                    throw new DomainException($"Gender '{g}' is not supported by discipline '{disciplineCode.Value}' (I-COMP-4).");

            children.Add(CompetitionDiscipline.Create(
                CompetitionDisciplineId.New(),
                id,
                disciplineCode,
                genders));
        }

        return new Competition(id, code, name, dates, children);
    }
}
```

- [ ] **Step 4: Run the tests**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~CompetitionTests"`
Expected: 5 tests pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Core/Competitions/Competition.cs tests/Sport.Core.Tests/Competitions/CompetitionTests.cs
git commit -m "feat(core): add Competition aggregate root enforcing I-COMP-1..4"
```

---

> **Plan continues in separate tasks for Structure, Participants, Officials, discipline module stubs, and architecture tests. The remaining tasks follow the same TDD shape (failing test → minimal implementation → green → commit) and are listed below in condensed form so this document stays readable.**

---

## Phase F — Structure submodule

### Task 14: Structure IDs (`EventId`, `PhaseId`, `UnitId`, `SubunitId`)

**Files:**
- Create: `src/Sport.Core/Structure/EventId.cs`, `PhaseId.cs`, `UnitId.cs`, `SubunitId.cs`

- [ ] **Step 1: Implement four Vogen Guid IDs identical in shape to `CompetitionId`**

For each ID file, e.g. `src/Sport.Core/Structure/EventId.cs`:
```csharp
using Vogen;

namespace Sport.Core.Structure;

[ValueObject<Guid>]
public readonly partial struct EventId
{
    public static EventId New() => From(Guid.CreateVersion7());
}
```

Repeat changing the type name for `PhaseId`, `UnitId`, `SubunitId`.

- [ ] **Step 2: Build**

Run: `dotnet build Sport.sln`
Expected: succeeds.

- [ ] **Step 3: Commit**

```
git add src/Sport.Core/Structure/EventId.cs src/Sport.Core/Structure/PhaseId.cs src/Sport.Core/Structure/UnitId.cs src/Sport.Core/Structure/SubunitId.cs
git commit -m "feat(core): add Structure typed IDs"
```

---

### Task 15: `Subunit` entity

**Files:**
- Create: `src/Sport.Core/Structure/Subunit.cs`
- Create: `tests/Sport.Core.Tests/Structure/SubunitTests.cs`

- [ ] **Step 1: Write failing test for I-STR-8 (SubunitCode unique within Unit) and basic creation**

Path: `tests/Sport.Core.Tests/Structure/SubunitTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Structure;
using Xunit;

namespace Sport.Core.Tests.Structure;

public class SubunitTests
{
    [Fact]
    public void Create_sets_rsc_with_subunit_chars()
    {
        var parentUnitRsc = Rsc.From("FBLMTEAM11------------QFNL00010000"); // unit ends in 00 (parent)
        var subunit = Subunit.Create(SubunitId.New(), UnitId.New(), SubunitCode.From("01"), parentUnitRsc);

        subunit.Rsc.Value.Substring(32, 2).Should().Be("01");
    }
}
```

- [ ] **Step 2: Run test (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~SubunitTests"`
Expected: build fails.

- [ ] **Step 3: Implement `Subunit`**

Path: `src/Sport.Core/Structure/Subunit.cs`
```csharp
using Sport.Core.Shared;

namespace Sport.Core.Structure;

public sealed class Subunit
{
    public SubunitId Id { get; }
    public UnitId UnitId { get; }
    public SubunitCode Code { get; }
    public Rsc Rsc { get; }

    private Subunit(SubunitId id, UnitId unitId, SubunitCode code, Rsc rsc)
    {
        Id = id; UnitId = unitId; Code = code; Rsc = rsc;
    }

    public static Subunit Create(SubunitId id, UnitId unitId, SubunitCode code, Rsc parentUnitRsc)
    {
        var s = parentUnitRsc.Value;
        if (s[32] != '0' || s[33] != '0')
            throw new DomainException("Parent Unit RSC must end with '00' to host subunits (I-STR-7).");

        var composed = string.Concat(s.AsSpan(0, 32), code.Value);
        return new Subunit(id, unitId, code, Rsc.From(composed));
    }
}
```

- [ ] **Step 4: Run test**

Expected: pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Core/Structure/Subunit.cs tests/Sport.Core.Tests/Structure/SubunitTests.cs
git commit -m "feat(core): add Subunit entity with RSC composition"
```

---

### Task 16: `Unit` entity with `LinkDisciplineRef`

**Files:**
- Create: `src/Sport.Core/Structure/Unit.cs`
- Create: `tests/Sport.Core.Tests/Structure/UnitTests.cs`

- [ ] **Step 1: Write failing tests** for: Unit creation; I-STR-7 (subunit parent ends in 00); `LinkDisciplineRef` sets the opaque FK; cannot link twice.

Path: `tests/Sport.Core.Tests/Structure/UnitTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Shared;
using Sport.Core.Structure;
using Xunit;

namespace Sport.Core.Tests.Structure;

public class UnitTests
{
    private static readonly Rsc PhaseRsc = Rsc.From("FBLMTEAM11------------QFNL--------");

    [Fact]
    public void Create_atomic_unit_composes_rsc_with_unit_code_and_dashes()
    {
        var unit = Unit.CreateAtomic(UnitId.New(), PhaseId.New(), UnitCode.From("000100--"), PhaseRsc, null);
        unit.Rsc.Value.Should().Be("FBLMTEAM11------------QFNL000100--");
        unit.Subunits.Should().BeEmpty();
    }

    [Fact]
    public void Create_parent_unit_requires_code_ending_in_00()
    {
        var act = () => Unit.CreateParentForSubunits(UnitId.New(), PhaseId.New(), UnitCode.From("000100--"), PhaseRsc, null);
        act.Should().Throw<DomainException>().WithMessage("*end with '00'*");
    }

    [Fact]
    public void LinkDisciplineRef_sets_value_once()
    {
        var unit = Unit.CreateAtomic(UnitId.New(), PhaseId.New(), UnitCode.From("000100--"), PhaseRsc, null);
        var refId = Guid.NewGuid();
        unit.LinkDisciplineRef(refId);
        unit.DisciplineUnitRef.Should().Be(refId);

        var act = () => unit.LinkDisciplineRef(Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*already linked*");
    }
}
```

- [ ] **Step 2: Run tests (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~UnitTests"`
Expected: build fails.

- [ ] **Step 3: Implement `Unit`**

Path: `src/Sport.Core/Structure/Unit.cs`
```csharp
using Sport.Core.Shared;

namespace Sport.Core.Structure;

public sealed class Unit
{
    public UnitId Id { get; }
    public PhaseId PhaseId { get; }
    public UnitCode Code { get; }
    public DateTimeOffset? ScheduledStart { get; private set; }
    public Rsc Rsc { get; }

    private readonly List<Subunit> _subunits;
    public IReadOnlyList<Subunit> Subunits => _subunits;

    public Guid? DisciplineUnitRef { get; private set; }

    private Unit(UnitId id, PhaseId phaseId, UnitCode code, DateTimeOffset? start, Rsc rsc, List<Subunit> subunits)
    {
        Id = id; PhaseId = phaseId; Code = code; ScheduledStart = start; Rsc = rsc; _subunits = subunits;
    }

    public static Unit CreateAtomic(UnitId id, PhaseId phaseId, UnitCode code, Rsc phaseRsc, DateTimeOffset? scheduledStart)
    {
        var rsc = ComposeUnitRsc(code, phaseRsc);
        return new Unit(id, phaseId, code, scheduledStart, rsc, new List<Subunit>());
    }

    public static Unit CreateParentForSubunits(UnitId id, PhaseId phaseId, UnitCode code, Rsc phaseRsc, DateTimeOffset? scheduledStart)
    {
        if (!(code.Value.EndsWith("00", StringComparison.Ordinal)))
            throw new DomainException("UnitCode for a parent of subunits must end with '00' (I-STR-7).");
        var rsc = ComposeUnitRsc(code, phaseRsc);
        return new Unit(id, phaseId, code, scheduledStart, rsc, new List<Subunit>());
    }

    public void AddSubunit(Subunit subunit)
    {
        if (subunit.UnitId != Id)
            throw new DomainException("Subunit.UnitId must match parent Unit.Id.");
        if (_subunits.Any(s => s.Code == subunit.Code))
            throw new DomainException($"SubunitCode '{subunit.Code.Value}' already exists in Unit (I-STR-8).");
        _subunits.Add(subunit);
    }

    public void LinkDisciplineRef(Guid ref_)
    {
        if (DisciplineUnitRef is not null)
            throw new DomainException("Unit already linked to a discipline-specific entity.");
        DisciplineUnitRef = ref_;
    }

    private static Rsc ComposeUnitRsc(UnitCode code, Rsc phaseRsc)
    {
        var s = phaseRsc.Value;
        // overwrite chars [26..34) with the 8-char UnitCode.
        var composed = string.Concat(s.AsSpan(0, 26), code.Value);
        return Rsc.From(composed);
    }
}
```

- [ ] **Step 4: Run tests**

Expected: pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Core/Structure/Unit.cs tests/Sport.Core.Tests/Structure/UnitTests.cs
git commit -m "feat(core): add Unit entity with subunit support and DisciplineUnitRef linking"
```

---

### Task 17: `Phase` entity

**Files:**
- Create: `src/Sport.Core/Structure/Phase.cs`
- Create: `tests/Sport.Core.Tests/Structure/PhaseTests.cs`

- [ ] **Step 1: Write failing tests** for: Phase creation; I-STR-4 (Order unique) and I-STR-5 (PhaseCode unique) enforced when adding to an Event (those invariants live on the Event aggregate but Phase computes its RSC here); I-STR-6 (UnitCode unique within Phase).

Path: `tests/Sport.Core.Tests/Structure/PhaseTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Shared;
using Sport.Core.Structure;
using Xunit;

namespace Sport.Core.Tests.Structure;

public class PhaseTests
{
    private static readonly Rsc EventRsc = Rsc.From("FBLMTEAM11----------------------");

    [Fact]
    public void Create_composes_phase_rsc()
    {
        var p = Phase.Create(PhaseId.New(), EventId.New(), PhaseCode.From("QFNL"), 1, EventRsc);
        p.Rsc.Value.Should().Be("FBLMTEAM11------------QFNL--------");
    }

    [Fact]
    public void Adding_unit_with_duplicate_code_throws_I_STR_6()
    {
        var p = Phase.Create(PhaseId.New(), EventId.New(), PhaseCode.From("QFNL"), 1, EventRsc);
        p.AddUnit(Unit.CreateAtomic(UnitId.New(), p.Id, UnitCode.From("000100--"), p.Rsc, null));

        var act = () => p.AddUnit(Unit.CreateAtomic(UnitId.New(), p.Id, UnitCode.From("000100--"), p.Rsc, null));
        act.Should().Throw<DomainException>().WithMessage("*UnitCode*already*");
    }
}
```

- [ ] **Step 2: Run tests (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~PhaseTests"`

- [ ] **Step 3: Implement `Phase`**

Path: `src/Sport.Core/Structure/Phase.cs`
```csharp
using Sport.Core.Shared;

namespace Sport.Core.Structure;

public sealed class Phase
{
    public PhaseId Id { get; }
    public EventId EventId { get; }
    public PhaseCode Code { get; }
    public int Order { get; }
    public Rsc Rsc { get; }

    private readonly List<Unit> _units;
    public IReadOnlyList<Unit> Units => _units;

    private Phase(PhaseId id, EventId eventId, PhaseCode code, int order, Rsc rsc)
    {
        Id = id; EventId = eventId; Code = code; Order = order; Rsc = rsc;
        _units = new List<Unit>();
    }

    public static Phase Create(PhaseId id, EventId eventId, PhaseCode code, int order, Rsc eventRsc)
    {
        if (order < 0) throw new DomainException("Phase.Order must be non-negative.");
        var composed = string.Concat(eventRsc.Value.AsSpan(0, 22), code.Value.PadRight(4, Rsc.Filler), "--------".AsSpan());
        var rsc = Rsc.From(composed);
        return new Phase(id, eventId, code, order, rsc);
    }

    public void AddUnit(Unit unit)
    {
        if (unit.PhaseId != Id)
            throw new DomainException("Unit.PhaseId must match parent Phase.Id.");
        if (_units.Any(u => u.Code == unit.Code))
            throw new DomainException($"UnitCode '{unit.Code.Value}' already exists in Phase (I-STR-6).");
        _units.Add(unit);
    }
}
```

- [ ] **Step 4: Run tests**

Expected: pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Core/Structure/Phase.cs tests/Sport.Core.Tests/Structure/PhaseTests.cs
git commit -m "feat(core): add Phase entity with unit uniqueness (I-STR-6)"
```

---

### Task 18: `Event` aggregate root

**Files:**
- Create: `src/Sport.Core/Structure/Event.cs`
- Create: `tests/Sport.Core.Tests/Structure/EventTests.cs`

- [ ] **Step 1: Write failing tests** for invariants I-STR-1 through I-STR-5 plus I-STR-2/I-STR-3 calling the module.

Path: `tests/Sport.Core.Tests/Structure/EventTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;
using Xunit;

namespace Sport.Core.Tests.Structure;

public class EventTests
{
    private static readonly DisciplineCode Fbl = DisciplineCode.From("FBL");
    private static readonly EventTypeCode Team11 = EventTypeCode.From("TEAM11");

    [Fact]
    public void Create_with_valid_inputs_composes_event_rsc()
    {
        var module = new ValidatingModule(Fbl);
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M,
            Team11, modifier: null, name: "Men's Football",
            disciplineModule: module);

        ev.Rsc.Value.Should().Be("FBLMTEAM11----------------------");
    }

    [Fact]
    public void Create_rejects_gender_not_supported_I_STR_1()
    {
        var module = new ValidatingModule(Fbl, supportedGenders: new HashSet<GenderCode> { GenderCode.M });
        var act = () => Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.W,
            Team11, null, "x", module);
        act.Should().Throw<DomainException>().WithMessage("*not supported*");
    }

    [Fact]
    public void Create_rejects_invalid_event_type_I_STR_2()
    {
        var module = new ValidatingModule(Fbl, eventTypeResult: Result.Fail("bad event"));
        var act = () => Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M,
            Team11, null, "x", module);
        act.Should().Throw<DomainException>().WithMessage("*bad event*");
    }

    [Fact]
    public void AddPhase_calls_module_to_validate_phase_I_STR_3()
    {
        var module = new ValidatingModule(Fbl, phaseResult: Result.Fail("bad phase"));
        var ev = Event.Create(
            EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M,
            Team11, null, "x", module);

        var act = () => ev.AddPhase(PhaseCode.From("QFNL"), 1, module);
        act.Should().Throw<DomainException>().WithMessage("*bad phase*");
    }

    [Fact]
    public void AddPhase_rejects_duplicate_order_I_STR_4()
    {
        var module = new ValidatingModule(Fbl);
        var ev = Event.Create(EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M, Team11, null, "x", module);
        ev.AddPhase(PhaseCode.From("QFNL"), 1, module);

        var act = () => ev.AddPhase(PhaseCode.From("SFNL"), 1, module);
        act.Should().Throw<DomainException>().WithMessage("*Order*");
    }

    [Fact]
    public void AddPhase_rejects_duplicate_phase_code_I_STR_5()
    {
        var module = new ValidatingModule(Fbl);
        var ev = Event.Create(EventId.New(), CompetitionDisciplineId.New(), Fbl, GenderCode.M, Team11, null, "x", module);
        ev.AddPhase(PhaseCode.From("QFNL"), 1, module);

        var act = () => ev.AddPhase(PhaseCode.From("QFNL"), 2, module);
        act.Should().Throw<DomainException>().WithMessage("*PhaseCode*already*");
    }

    private sealed class ValidatingModule : IDisciplineModule
    {
        private readonly Result _eventTypeResult;
        private readonly Result _phaseResult;

        public ValidatingModule(
            DisciplineCode code,
            IReadOnlySet<GenderCode>? supportedGenders = null,
            Result? eventTypeResult = null,
            Result? phaseResult = null)
        {
            Code = code;
            SupportedGenders = supportedGenders ?? new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
            _eventTypeResult = eventTypeResult ?? Result.Ok();
            _phaseResult = phaseResult ?? Result.Ok();
        }

        public DisciplineCode Code { get; }
        public string DisplayName => "fake";
        public IReadOnlySet<GenderCode> SupportedGenders { get; }
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => [];
        public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
        public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
        public IReadOnlyCollection<FunctionDescriptor> Functions => [];
        public IEntryRules EntryRules => throw new NotImplementedException();
        public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => _eventTypeResult;
        public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => _phaseResult;
        public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Result.Ok();
        public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
        public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Result.Ok();
    }
}
```

- [ ] **Step 2: Run tests (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~EventTests"`

- [ ] **Step 3: Implement `Event`**

Path: `src/Sport.Core/Structure/Event.cs`
```csharp
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Structure;

public sealed class Event
{
    public EventId Id { get; }
    public CompetitionDisciplineId CompetitionDisciplineId { get; }
    public DisciplineCode DisciplineCode { get; }
    public GenderCode Gender { get; }
    public EventTypeCode EventType { get; }
    public EventModifierCode? EventModifier { get; }
    public string Name { get; }
    public Rsc Rsc { get; }

    private readonly List<Phase> _phases;
    public IReadOnlyList<Phase> Phases => _phases;

    private Event(
        EventId id,
        CompetitionDisciplineId competitionDisciplineId,
        DisciplineCode disciplineCode,
        GenderCode gender,
        EventTypeCode eventType,
        EventModifierCode? eventModifier,
        string name,
        Rsc rsc)
    {
        Id = id;
        CompetitionDisciplineId = competitionDisciplineId;
        DisciplineCode = disciplineCode;
        Gender = gender;
        EventType = eventType;
        EventModifier = eventModifier;
        Name = name;
        Rsc = rsc;
        _phases = new List<Phase>();
    }

    public static Event Create(
        EventId id,
        CompetitionDisciplineId competitionDisciplineId,
        DisciplineCode disciplineCode,
        GenderCode gender,
        EventTypeCode eventType,
        EventModifierCode? modifier,
        string name,
        IDisciplineModule disciplineModule)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Event.Name is required.");

        if (!disciplineModule.SupportedGenders.Contains(gender))
            throw new DomainException($"Gender '{gender}' is not supported by discipline '{disciplineCode.Value}' (I-STR-1).");

        var validation = disciplineModule.ValidateEventType(eventType, modifier);
        if (!validation.IsSuccess) throw new DomainException(validation.Error!);

        var rsc = Rsc.Compose(disciplineCode, gender, eventType, modifier, phase: null, unit: null, subunit: null);
        return new Event(id, competitionDisciplineId, disciplineCode, gender, eventType, modifier, name, rsc);
    }

    public Phase AddPhase(PhaseCode code, int order, IDisciplineModule disciplineModule)
    {
        var validation = disciplineModule.ValidatePhaseForEventType(EventType, code);
        if (!validation.IsSuccess) throw new DomainException(validation.Error!);

        if (_phases.Any(p => p.Order == order))
            throw new DomainException($"Phase.Order {order} already exists in Event (I-STR-4).");
        if (_phases.Any(p => p.Code == code))
            throw new DomainException($"PhaseCode '{code.Value}' already exists in Event (I-STR-5).");

        var phase = Phase.Create(PhaseId.New(), Id, code, order, Rsc);
        _phases.Add(phase);
        return phase;
    }
}
```

- [ ] **Step 4: Run tests**

Expected: pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Core/Structure/Event.cs tests/Sport.Core.Tests/Structure/EventTests.cs
git commit -m "feat(core): add Event aggregate enforcing I-STR-1..5"
```

---

## Phase G — Participants submodule

### Task 19: Participants value objects

**Files:**
- Create: `src/Sport.Core/Participants/PersonId.cs`, `OrganisationId.cs`, `OrganisationCode.cs`, `OrganisationType.cs`, `TeamId.cs`, `TeamCode.cs`, `EntryId.cs`, `EntryStatus.cs`, `Bib.cs`

- [ ] **Step 1: Implement Guid v7 IDs**

For `PersonId.cs`, `OrganisationId.cs`, `TeamId.cs`, `EntryId.cs` — each identical in shape to `CompetitionId`:
```csharp
using Vogen;

namespace Sport.Core.Participants;

[ValueObject<Guid>]
public readonly partial struct PersonId
{
    public static PersonId New() => From(Guid.CreateVersion7());
}
```

- [ ] **Step 2: Implement `OrganisationCode`**

Path: `src/Sport.Core/Participants/OrganisationCode.cs`
```csharp
using Vogen;
namespace Sport.Core.Participants;

[ValueObject<string>]
public readonly partial struct OrganisationCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("OrganisationCode is required.");
        if (value.Length is < 3 or > 10) return Validation.Invalid("OrganisationCode must be 3..10 characters.");
        foreach (var c in value)
            if (!(c is >= 'A' and <= 'Z' || c is >= '0' and <= '9' || c == '-'))
                return Validation.Invalid("OrganisationCode chars must be uppercase A-Z, 0-9 or '-'.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 3: Implement `OrganisationType`**

Path: `src/Sport.Core/Participants/OrganisationType.cs`
```csharp
namespace Sport.Core.Participants;
public enum OrganisationType { Noc, Club, Federation, School, Group, Other }
```

- [ ] **Step 4: Implement `TeamCode`**

Path: `src/Sport.Core/Participants/TeamCode.cs`
```csharp
using Vogen;
namespace Sport.Core.Participants;

[ValueObject<string>]
public readonly partial struct TeamCode
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("TeamCode is required.");
        if (value.Length > 20) return Validation.Invalid("TeamCode must be at most 20 characters.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 5: Implement `Bib`**

Path: `src/Sport.Core/Participants/Bib.cs`
```csharp
using Vogen;
namespace Sport.Core.Participants;

[ValueObject<string>]
public readonly partial struct Bib
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("Bib is required.");
        if (value.Length > 20) return Validation.Invalid("Bib must be at most 20 characters.");
        return Validation.Ok;
    }
}
```

- [ ] **Step 6: Implement `EntryStatus`**

Path: `src/Sport.Core/Participants/EntryStatus.cs`
```csharp
namespace Sport.Core.Participants;
public enum EntryStatus { Registered, Withdrawn, Disqualified, Replaced }
```

- [ ] **Step 7: Build**

Run: `dotnet build Sport.sln`
Expected: succeeds.

- [ ] **Step 8: Commit**

```
git add src/Sport.Core/Participants/
git commit -m "feat(core): add Participants value objects and enums"
```

---

### Task 20: `Person`, `Organisation`, `Team` entities

**Files:**
- Create: `src/Sport.Core/Participants/Person.cs`
- Create: `src/Sport.Core/Participants/Organisation.cs`
- Create: `src/Sport.Core/Participants/Team.cs`
- Create: `tests/Sport.Core.Tests/Participants/PersonTests.cs`
- Create: `tests/Sport.Core.Tests/Participants/OrganisationTests.cs`
- Create: `tests/Sport.Core.Tests/Participants/TeamTests.cs`

- [ ] **Step 1: Tests for Person (basic creation, empty FamilyName throws)**

Path: `tests/Sport.Core.Tests/Participants/PersonTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Participants;

public class PersonTests
{
    [Fact]
    public void Create_with_family_name_succeeds()
    {
        var p = Person.Create(PersonId.New(), "Pérez", "Juan", GenderCode.M, new DateOnly(1990, 1, 1), ifId: null);
        p.FamilyName.Should().Be("Pérez");
    }

    [Fact]
    public void Create_without_family_name_throws()
    {
        var act = () => Person.Create(PersonId.New(), " ", null, GenderCode.M, null, null);
        act.Should().Throw<DomainException>();
    }
}
```

- [ ] **Step 2: Tests for Organisation**

Path: `tests/Sport.Core.Tests/Participants/OrganisationTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Participants;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Participants;

public class OrganisationTests
{
    [Fact]
    public void Create_with_valid_data_succeeds()
    {
        var o = Organisation.Create(OrganisationId.New(), OrganisationCode.From("ESP"), "Spain", OrganisationType.Noc);
        o.Code.Value.Should().Be("ESP");
    }

    [Fact]
    public void Create_with_empty_name_throws()
    {
        var act = () => Organisation.Create(OrganisationId.New(), OrganisationCode.From("ESP"), " ", OrganisationType.Noc);
        act.Should().Throw<DomainException>();
    }
}
```

- [ ] **Step 3: Tests for Team**

Path: `tests/Sport.Core.Tests/Participants/TeamTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Participants;

public class TeamTests
{
    [Fact]
    public void Create_with_valid_data_succeeds()
    {
        var t = Team.Create(TeamId.New(), TeamCode.From("FCB-FBL-M"), "Barcelona FBL M", OrganisationId.New(), DisciplineCode.From("FBL"));
        t.Code.Value.Should().Be("FCB-FBL-M");
    }
}
```

- [ ] **Step 4: Run tests (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~Participants"`

- [ ] **Step 5: Implement `Person`**

Path: `src/Sport.Core/Participants/Person.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Participants;

public sealed class Person
{
    public PersonId Id { get; }
    public string FamilyName { get; }
    public string? GivenName { get; }
    public GenderCode Gender { get; }
    public DateOnly? BirthDate { get; }
    public string? IFId { get; }

    private Person(PersonId id, string family, string? given, GenderCode gender, DateOnly? birth, string? ifId)
    {
        Id = id; FamilyName = family; GivenName = given; Gender = gender; BirthDate = birth; IFId = ifId;
    }

    public static Person Create(PersonId id, string familyName, string? givenName, GenderCode gender, DateOnly? birthDate, string? ifId)
    {
        if (string.IsNullOrWhiteSpace(familyName))
            throw new DomainException("Person.FamilyName is required.");
        if (familyName.Length > 50)
            throw new DomainException("Person.FamilyName must be at most 50 characters.");
        if (givenName is { Length: > 50 })
            throw new DomainException("Person.GivenName must be at most 50 characters.");
        if (ifId is { Length: > 20 })
            throw new DomainException("Person.IFId must be at most 20 characters.");
        return new Person(id, familyName, givenName, gender, birthDate, ifId);
    }
}
```

- [ ] **Step 6: Implement `Organisation`**

Path: `src/Sport.Core/Participants/Organisation.cs`
```csharp
using Sport.Core.Shared;

namespace Sport.Core.Participants;

public sealed class Organisation
{
    public OrganisationId Id { get; }
    public OrganisationCode Code { get; }
    public string Name { get; }
    public OrganisationType Type { get; }

    private Organisation(OrganisationId id, OrganisationCode code, string name, OrganisationType type)
    {
        Id = id; Code = code; Name = name; Type = type;
    }

    public static Organisation Create(OrganisationId id, OrganisationCode code, string name, OrganisationType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Organisation.Name is required.");
        return new Organisation(id, code, name, type);
    }
}
```

- [ ] **Step 7: Implement `Team`**

Path: `src/Sport.Core/Participants/Team.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Participants;

public sealed class Team
{
    public TeamId Id { get; }
    public TeamCode Code { get; }
    public string Name { get; }
    public OrganisationId OrganisationId { get; }
    public DisciplineCode DisciplineCode { get; }

    private Team(TeamId id, TeamCode code, string name, OrganisationId organisationId, DisciplineCode disciplineCode)
    {
        Id = id; Code = code; Name = name; OrganisationId = organisationId; DisciplineCode = disciplineCode;
    }

    public static Team Create(TeamId id, TeamCode code, string name, OrganisationId organisationId, DisciplineCode disciplineCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Team.Name is required.");
        return new Team(id, code, name, organisationId, disciplineCode);
    }
}
```

- [ ] **Step 8: Run tests**

Expected: pass.

- [ ] **Step 9: Commit**

```
git add src/Sport.Core/Participants/Person.cs src/Sport.Core/Participants/Organisation.cs src/Sport.Core/Participants/Team.cs tests/Sport.Core.Tests/Participants/
git commit -m "feat(core): add Person, Organisation and Team entities"
```

---

### Task 21: `CompositionMember` and `Entry` aggregate root

**Files:**
- Create: `src/Sport.Core/Participants/CompositionMember.cs`
- Create: `src/Sport.Core/Participants/Entry.cs`
- Create: `tests/Sport.Core.Tests/Participants/EntryTests.cs`

- [ ] **Step 1: Write failing tests covering I-PAR-1..I-PAR-8**

Path: `tests/Sport.Core.Tests/Participants/EntryTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Participants;
using Sport.Core.Shared;
using Sport.Core.Structure;
using Xunit;

namespace Sport.Core.Tests.Participants;

public class EntryTests
{
    private static readonly EventId Ev = EventId.New();
    private static readonly OrganisationId Org = OrganisationId.New();
    private static readonly TeamId TeamA = TeamId.New();

    [Fact]
    public void Athlete_entry_must_have_exactly_one_composition_member_I_PAR_1()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Athlete, Org, teamId: null,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null), (PersonId.New(), 2, (Bib?)null) });
        act.Should().Throw<DomainException>().WithMessage("*exactly 1*");
    }

    [Fact]
    public void Team_entry_requires_team_id_I_PAR_2()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Team, Org, teamId: null,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null) });
        act.Should().Throw<DomainException>().WithMessage("*TeamId*required*");
    }

    [Fact]
    public void Group_entry_must_have_at_least_two_members_I_PAR_1()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Group, Org, teamId: null,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null) });
        act.Should().Throw<DomainException>().WithMessage("*at least 2*");
    }

    [Fact]
    public void Group_entry_rejects_team_id_I_PAR_2()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Group, Org, teamId: TeamA,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null), (PersonId.New(), 2, (Bib?)null) });
        act.Should().Throw<DomainException>().WithMessage("*Group*TeamId*null*");
    }

    [Fact]
    public void Duplicate_order_in_composition_throws_I_PAR_6()
    {
        var act = () => Entry.Create(EntryId.New(), Ev, EntryType.Team, Org, teamId: TeamA,
            bib: null, seed: null,
            new[]
            {
                (PersonId.New(), 1, (Bib?)null),
                (PersonId.New(), 1, (Bib?)null),
            });
        act.Should().Throw<DomainException>().WithMessage("*Order*unique*");
    }

    [Fact]
    public void Initial_status_is_Registered_I_PAR_8()
    {
        var e = Entry.Create(EntryId.New(), Ev, EntryType.Athlete, Org, teamId: null,
            bib: null, seed: null,
            new[] { (PersonId.New(), 1, (Bib?)null) });
        e.Status.Should().Be(EntryStatus.Registered);
    }
}
```

- [ ] **Step 2: Run tests (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~EntryTests"`

- [ ] **Step 3: Implement `CompositionMember`**

Path: `src/Sport.Core/Participants/CompositionMember.cs`
```csharp
namespace Sport.Core.Participants;

public sealed record CompositionMember(EntryId EntryId, PersonId PersonId, int Order, Bib? Bib);
```

- [ ] **Step 4: Implement `Entry`**

Path: `src/Sport.Core/Participants/Entry.cs`
```csharp
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Participants;

public sealed class Entry
{
    public EntryId Id { get; }
    public EventId EventId { get; }
    public EntryType Type { get; }
    public OrganisationId OrganisationId { get; }
    public TeamId? TeamId { get; }
    public Bib? Bib { get; }
    public int? Seed { get; }
    public EntryStatus Status { get; private set; }

    private readonly List<CompositionMember> _composition;
    public IReadOnlyList<CompositionMember> Composition => _composition;

    private Entry(
        EntryId id, EventId eventId, EntryType type, OrganisationId organisationId,
        TeamId? teamId, Bib? bib, int? seed, List<CompositionMember> composition)
    {
        Id = id; EventId = eventId; Type = type; OrganisationId = organisationId;
        TeamId = teamId; Bib = bib; Seed = seed;
        _composition = composition;
        Status = EntryStatus.Registered;
    }

    public static Entry Create(
        EntryId id,
        EventId eventId,
        EntryType type,
        OrganisationId organisationId,
        TeamId? teamId,
        Bib? bib,
        int? seed,
        IReadOnlyCollection<(PersonId PersonId, int Order, Bib? Bib)> members)
    {
        ValidateTeamRules(type, teamId);
        ValidateComposition(type, members);

        var composition = members
            .Select(m => new CompositionMember(id, m.PersonId, m.Order, m.Bib))
            .ToList();

        return new Entry(id, eventId, type, organisationId, teamId, bib, seed, composition);
    }

    public void Withdraw()    => Status = EntryStatus.Withdrawn;
    public void Disqualify()  => Status = EntryStatus.Disqualified;
    public void Replace()     => Status = EntryStatus.Replaced;

    private static void ValidateTeamRules(EntryType type, TeamId? teamId)
    {
        switch (type)
        {
            case EntryType.Team when teamId is null:
                throw new DomainException("Type=Team requires TeamId (I-PAR-2).");
            case EntryType.Athlete when teamId is not null:
                throw new DomainException("Type=Athlete: TeamId must be null (I-PAR-2).");
            case EntryType.Group when teamId is not null:
                throw new DomainException("Type=Group: TeamId must be null (I-PAR-2).");
        }
    }

    private static void ValidateComposition(
        EntryType type,
        IReadOnlyCollection<(PersonId PersonId, int Order, Bib? Bib)> members)
    {
        if (members is null) throw new DomainException("Composition is required.");

        switch (type)
        {
            case EntryType.Athlete when members.Count != 1:
                throw new DomainException("Athlete entry must contain exactly 1 composition member (I-PAR-1).");
            case EntryType.Team when members.Count < 1:
                throw new DomainException("Team entry must contain at least 1 composition member (I-PAR-1).");
            case EntryType.Group when members.Count < 2:
                throw new DomainException("Group entry must contain at least 2 composition members (I-PAR-1).");
        }

        var seenOrders = new HashSet<int>();
        var seenPersons = new HashSet<PersonId>();
        foreach (var m in members)
        {
            if (!seenOrders.Add(m.Order))
                throw new DomainException($"CompositionMember.Order must be unique within Entry (I-PAR-6).");
            if (!seenPersons.Add(m.PersonId))
                throw new DomainException($"PersonId duplicated within the same Entry composition.");
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Expected: pass.

- [ ] **Step 6: Commit**

```
git add src/Sport.Core/Participants/CompositionMember.cs src/Sport.Core/Participants/Entry.cs tests/Sport.Core.Tests/Participants/EntryTests.cs
git commit -m "feat(core): add Entry aggregate enforcing I-PAR-1, I-PAR-2, I-PAR-6, I-PAR-8"
```

---

## Phase H — Officials submodule

### Task 22: `OfficialAssignmentId`, `OfficialAssignmentStatus`, `OfficialScope`

**Files:**
- Create: `src/Sport.Core/Officials/OfficialAssignmentId.cs`
- Create: `src/Sport.Core/Officials/OfficialAssignmentStatus.cs`
- Create: `src/Sport.Core/Officials/OfficialScope.cs`
- Create: `tests/Sport.Core.Tests/Officials/OfficialScopeTests.cs`

- [ ] **Step 1: Implement `OfficialAssignmentId`**

Same shape as other Guid v7 IDs.

- [ ] **Step 2: Implement `OfficialAssignmentStatus`**

Path: `src/Sport.Core/Officials/OfficialAssignmentStatus.cs`
```csharp
namespace Sport.Core.Officials;
public enum OfficialAssignmentStatus { Active, Replaced, Removed }
```

- [ ] **Step 3: Write failing tests for `OfficialScope`**

Path: `tests/Sport.Core.Tests/Officials/OfficialScopeTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Officials;

public class OfficialScopeTests
{
    [Fact]
    public void Create_with_valid_level_and_target_succeeds()
    {
        var scope = OfficialScope.Create(ScopeLevel.Unit, Guid.NewGuid());
        scope.Level.Should().Be(ScopeLevel.Unit);
    }

    [Fact]
    public void Create_with_empty_target_throws()
    {
        var act = () => OfficialScope.Create(ScopeLevel.Unit, Guid.Empty);
        act.Should().Throw<DomainException>();
    }
}
```

- [ ] **Step 4: Implement `OfficialScope`**

Path: `src/Sport.Core/Officials/OfficialScope.cs`
```csharp
using Sport.Core.Shared;

namespace Sport.Core.Officials;

public readonly record struct OfficialScope
{
    public ScopeLevel Level { get; }
    public Guid TargetId { get; }

    private OfficialScope(ScopeLevel level, Guid targetId)
    {
        Level = level;
        TargetId = targetId;
    }

    public static OfficialScope Create(ScopeLevel level, Guid targetId)
    {
        if (targetId == Guid.Empty)
            throw new DomainException("OfficialScope.TargetId must not be empty.");
        return new OfficialScope(level, targetId);
    }
}
```

- [ ] **Step 5: Run tests**

Expected: pass.

- [ ] **Step 6: Commit**

```
git add src/Sport.Core/Officials/ tests/Sport.Core.Tests/Officials/
git commit -m "feat(core): add OfficialAssignmentId, OfficialAssignmentStatus, OfficialScope"
```

---

### Task 23: `OfficialAssignment` aggregate root

**Files:**
- Create: `src/Sport.Core/Officials/OfficialAssignment.cs`
- Create: `tests/Sport.Core.Tests/Officials/OfficialAssignmentTests.cs`

- [ ] **Step 1: Write failing tests for I-OFF-1, I-OFF-2, I-OFF-3, I-OFF-5**

Path: `tests/Sport.Core.Tests/Officials/OfficialAssignmentTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Shared;
using Xunit;

namespace Sport.Core.Tests.Officials;

public class OfficialAssignmentTests
{
    private static readonly FunctionDescriptor CoachDesc = new(
        FunctionCode.From("COMMON.COACH"),
        "Coach",
        new HashSet<ScopeLevel> { ScopeLevel.Event, ScopeLevel.Unit },
        IsTeamOfficial: true,
        RequiresOrganisation: true);

    private static readonly FunctionDescriptor RefDesc = new(
        FunctionCode.From("FBL.REF"),
        "Referee",
        new HashSet<ScopeLevel> { ScopeLevel.Unit },
        IsTeamOfficial: false,
        RequiresOrganisation: false);

    [Fact]
    public void Create_with_valid_descriptor_and_scope_succeeds()
    {
        var a = OfficialAssignment.Create(
            OfficialAssignmentId.New(),
            PersonId.New(),
            RefDesc,
            OfficialScope.Create(ScopeLevel.Unit, Guid.NewGuid()),
            organisationId: null);

        a.Status.Should().Be(OfficialAssignmentStatus.Active);
    }

    [Fact]
    public void Create_rejects_scope_level_not_in_valid_scopes_I_OFF_2()
    {
        var act = () => OfficialAssignment.Create(
            OfficialAssignmentId.New(),
            PersonId.New(),
            RefDesc,
            OfficialScope.Create(ScopeLevel.Competition, Guid.NewGuid()),
            organisationId: null);
        act.Should().Throw<DomainException>().WithMessage("*ScopeLevel*not*allowed*");
    }

    [Fact]
    public void Create_requires_organisation_when_descriptor_demands_it_I_OFF_3()
    {
        var act = () => OfficialAssignment.Create(
            OfficialAssignmentId.New(),
            PersonId.New(),
            CoachDesc,
            OfficialScope.Create(ScopeLevel.Event, Guid.NewGuid()),
            organisationId: null);
        act.Should().Throw<DomainException>().WithMessage("*Organisation*required*");
    }
}
```

- [ ] **Step 2: Run tests (fail)**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~OfficialAssignmentTests"`

- [ ] **Step 3: Implement `OfficialAssignment`**

Path: `src/Sport.Core/Officials/OfficialAssignment.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Core.Officials;

public sealed class OfficialAssignment
{
    public OfficialAssignmentId Id { get; }
    public PersonId PersonId { get; }
    public FunctionCode FunctionCode { get; }
    public OfficialScope Scope { get; }
    public OrganisationId? OrganisationId { get; }
    public OfficialAssignmentStatus Status { get; private set; }

    private OfficialAssignment(
        OfficialAssignmentId id, PersonId personId, FunctionCode functionCode,
        OfficialScope scope, OrganisationId? organisationId)
    {
        Id = id; PersonId = personId; FunctionCode = functionCode;
        Scope = scope; OrganisationId = organisationId;
        Status = OfficialAssignmentStatus.Active;
    }

    public static OfficialAssignment Create(
        OfficialAssignmentId id,
        PersonId personId,
        FunctionDescriptor descriptor,
        OfficialScope scope,
        OrganisationId? organisationId)
    {
        if (!descriptor.ValidScopes.Contains(scope.Level))
            throw new DomainException($"ScopeLevel '{scope.Level}' is not allowed for function '{descriptor.Code.Value}' (I-OFF-2).");

        if (descriptor.RequiresOrganisation && organisationId is null)
            throw new DomainException($"Function '{descriptor.Code.Value}' Organisation is required (I-OFF-3).");

        return new OfficialAssignment(id, personId, descriptor.Code, scope, organisationId);
    }

    public void Replace() => Status = OfficialAssignmentStatus.Replaced;
    public void Remove()  => Status = OfficialAssignmentStatus.Removed;
}
```

- [ ] **Step 4: Run tests**

Expected: pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Core/Officials/OfficialAssignment.cs tests/Sport.Core.Tests/Officials/OfficialAssignmentTests.cs
git commit -m "feat(core): add OfficialAssignment aggregate enforcing I-OFF-1..3, I-OFF-5"
```

---

## Phase I — Discipline modules

Each of the next six tasks creates one `Sport.Disciplines.<X>` project containing a minimal `IDisciplineModule` implementation. The level of detail per module is intentionally small: each module fills in `EventTypes`, `PhaseCatalog`, `UnitCodeStrategy`, `Functions`, `EntryRules` so the contract resolves. Live-ops behavior and richer validation belong to later specs.

All six tasks share the structure of **Task 24**; subsequent tasks 25–29 differ only in the disciplines's enumerated values.

### Task 24: `Sport.Disciplines.FBL` module

**Files:**
- Create: `src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj`
- Create: `src/Sport.Disciplines.FBL/FblModule.cs`
- Create: `src/Sport.Disciplines.FBL/FblPhaseCatalog.cs`
- Create: `src/Sport.Disciplines.FBL/FblUnitCodeStrategy.cs`
- Create: `src/Sport.Disciplines.FBL/FblEntryRules.cs`
- Create: `tests/Sport.Disciplines.FBL.Tests/Sport.Disciplines.FBL.Tests.csproj`
- Create: `tests/Sport.Disciplines.FBL.Tests/FblModuleTests.cs`

- [ ] **Step 1: Create the FBL class library and add to solution**

```
dotnet new classlib -n Sport.Disciplines.FBL -o src/Sport.Disciplines.FBL -f net10.0
dotnet add src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj reference src/Sport.Core/Sport.Core.csproj
dotnet sln add src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj
rm src/Sport.Disciplines.FBL/Class1.cs
```

- [ ] **Step 2: Create the test project**

```
dotnet new xunit -n Sport.Disciplines.FBL.Tests -o tests/Sport.Disciplines.FBL.Tests -f net10.0
dotnet add tests/Sport.Disciplines.FBL.Tests/Sport.Disciplines.FBL.Tests.csproj package FluentAssertions
dotnet add tests/Sport.Disciplines.FBL.Tests/Sport.Disciplines.FBL.Tests.csproj reference src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj
dotnet sln add tests/Sport.Disciplines.FBL.Tests/Sport.Disciplines.FBL.Tests.csproj
rm tests/Sport.Disciplines.FBL.Tests/UnitTest1.cs
```

- [ ] **Step 3: Implement supporting classes**

Path: `src/Sport.Disciplines.FBL/FblPhaseCatalog.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL;

internal sealed class FblPhaseCatalog : IPhaseCatalog
{
    private static readonly HashSet<string> AllowedCodes = new(StringComparer.Ordinal)
    {
        "GPA","GPB","GPC","GPD","GPE","GPF",
        "R32","R16","QFNL","SFNL","FNL",
    };

    public IReadOnlyCollection<PhaseCode> AllowedPhases { get; } =
        AllowedCodes.Select(PhaseCode.From).ToArray();

    public bool IsAllowedForEventType(EventTypeCode eventType, PhaseCode phase) =>
        AllowedCodes.Contains(phase.Value);
}
```

Path: `src/Sport.Disciplines.FBL/FblUnitCodeStrategy.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL;

internal sealed class FblUnitCodeStrategy : IUnitCodeStrategy
{
    public UnitCode NextUnitCode(IEnumerable<UnitCode> existing)
    {
        var max = existing
            .Select(u => int.TryParse(u.Value.AsSpan(0, 6), out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return UnitCode.From($"{(max + 100).ToString("D6")}--");
    }

    public bool IsValid(UnitCode code) => true; // core already enforces shape
}
```

Path: `src/Sport.Disciplines.FBL/FblEntryRules.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Shared;

namespace Sport.Disciplines.FBL;

internal sealed class FblEntryRules : IEntryRules
{
    public IReadOnlyCollection<EntryType> AllowedTypes { get; } = new[] { EntryType.Team };

    public (int Min, int Max) CompositionSize(EntryType type) => type switch
    {
        EntryType.Team => (11, 23),
        _ => (0, 0),
    };

    public Result Validate(EntryCandidate candidate)
    {
        if (!AllowedTypes.Contains(candidate.Type))
            return Result.Fail($"FBL only accepts Team entries, got '{candidate.Type}'.");
        var (min, max) = CompositionSize(candidate.Type);
        if (candidate.CompositionSize < min || candidate.CompositionSize > max)
            return Result.Fail($"FBL Team composition must be {min}..{max}, got {candidate.CompositionSize}.");
        return Result.Ok();
    }
}
```

Path: `src/Sport.Disciplines.FBL/FblModule.cs`
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL;

public sealed class FblModule : IDisciplineModule
{
    public DisciplineCode Code { get; } = DisciplineCode.From("FBL");
    public string DisplayName => "Football";
    public IReadOnlySet<GenderCode> SupportedGenders { get; } =
        new HashSet<GenderCode> { GenderCode.M, GenderCode.W };

    public IReadOnlyCollection<EventTypeDescriptor> EventTypes { get; } = new[]
    {
        new EventTypeDescriptor(
            EventTypeCode.From("TEAM11"),
            "11-a-side Football",
            new HashSet<GenderCode> { GenderCode.M, GenderCode.W },
            ModifierContract.Forbidden),
    };

    public IPhaseCatalog PhaseCatalog { get; } = new FblPhaseCatalog();
    public IUnitCodeStrategy UnitCodeStrategy { get; } = new FblUnitCodeStrategy();

    public IReadOnlyCollection<FunctionDescriptor> Functions { get; } = new[]
    {
        new FunctionDescriptor(FunctionCode.From("FBL.REF"),  "Referee",           new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("FBL.AREF"), "Assistant Referee", new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("FBL.4OFF"), "Fourth Official",   new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
        new FunctionDescriptor(FunctionCode.From("FBL.VAR"),  "VAR Official",      new HashSet<ScopeLevel> { ScopeLevel.Unit }, IsTeamOfficial: false, RequiresOrganisation: false),
    };

    public IEntryRules EntryRules { get; } = new FblEntryRules();

    public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier)
    {
        if (!EventTypes.Any(e => e.Code == type))
            return Result.Fail($"EventType '{type.Value}' not supported by FBL.");
        if (modifier is not null)
            return Result.Fail("FBL EventType does not accept a modifier.");
        return Result.Ok();
    }

    public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) =>
        PhaseCatalog.IsAllowedForEventType(type, phase)
            ? Result.Ok()
            : Result.Fail($"Phase '{phase.Value}' is not allowed for FBL EventType '{type.Value}'.");

    public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) =>
        UnitCodeStrategy.IsValid(code) ? Result.Ok() : Result.Fail("Invalid UnitCode for FBL.");

    public Result ValidateEntry(EntryCandidate candidate) => EntryRules.Validate(candidate);

    public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level)
    {
        var f = Functions.FirstOrDefault(x => x.Code == function);
        if (f is null) return Result.Fail($"Function '{function.Value}' not registered in FBL.");
        if (!f.ValidScopes.Contains(level)) return Result.Fail($"Function '{function.Value}' not valid at scope '{level}'.");
        return Result.Ok();
    }
}
```

- [ ] **Step 4: Add a smoke test for the module**

Path: `tests/Sport.Disciplines.FBL.Tests/FblModuleTests.cs`
```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Structure;
using Sport.Disciplines.FBL;
using Xunit;

namespace Sport.Disciplines.FBL.Tests;

public class FblModuleTests
{
    [Fact]
    public void Module_advertises_FBL_code_and_supported_genders()
    {
        var m = new FblModule();
        m.Code.Value.Should().Be("FBL");
        m.SupportedGenders.Should().Contain(new[] { GenderCode.M, GenderCode.W });
    }

    [Fact]
    public void Rejects_unsupported_event_type()
    {
        var m = new FblModule();
        m.ValidateEventType(EventTypeCode.From("HJ"), null).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validates_team_entry_size()
    {
        var m = new FblModule();
        var ok = m.ValidateEntry(new EntryCandidate(EntryType.Team, CompositionSize: 18, HasTeam: true, HasOrganisation: true));
        ok.IsSuccess.Should().BeTrue();

        var bad = m.ValidateEntry(new EntryCandidate(EntryType.Team, CompositionSize: 5, HasTeam: true, HasOrganisation: true));
        bad.IsSuccess.Should().BeFalse();
    }
}
```

- [ ] **Step 5: Run tests**

Run: `dotnet test tests/Sport.Disciplines.FBL.Tests/Sport.Disciplines.FBL.Tests.csproj`
Expected: pass.

- [ ] **Step 6: Commit**

```
git add src/Sport.Disciplines.FBL/ tests/Sport.Disciplines.FBL.Tests/
git commit -m "feat(fbl): add FblModule with phases, unit code strategy, entry rules and functions"
```

---

### Tasks 25 – 29: Other discipline modules

For each of `BKB`, `BDM`, `VBV`, `BOX`, `ATH` repeat the **exact shape** of Task 24, substituting:

| Module class | DisciplineCode | EventTypes | Phases | Allowed EntryTypes | Comp size | Functions specific |
|---|---|---|---|---|---|---|
| `BkbModule` | `BKB` | `TEAM5` | `GPA..GPF`, `R32`, `R16`, `QFNL`, `SFNL`, `FNL` | `Team` | 5..12 | `BKB.REF`, `BKB.UMP` |
| `BdmModule` | `BDM` | `SINGLES`, `DOUBLES`, `MIXEDDOUB` | `GPA..`, `R32..FNL` | `Athlete`, `Team` | 1 (Athlete) / 2 (Team) | `BDM.UMP`, `BDM.SVJU`, `BDM.LIJU` |
| `VbvModule` | `VBV` | `TEAM2` | Pool + KO (`GPA..`, `QFNL`, `SFNL`, `FNL`) | `Team` | 2 | `VBV.REF1`, `VBV.REF2`, `VBV.LIJU` |
| `BoxModule` | `BOX` | `48KG`, `54KG`, `60KG`, `75KG` (one per category) | `R32`, `R16`, `QFNL`, `SFNL`, `FNL` | `Athlete` | 1 | `BOX.REF`, `BOX.JUD1..JUD5`, `BOX.TIMK` |
| `AthModule` | `ATH` | `HJ` | `QUAL`, `FNL` | `Athlete` | 1 | `ATH.STJU`, `ATH.PHOTOFIN` |

**For each module, follow these steps (using BKB as the worked example, repeat shape for BDM/VBV/BOX/ATH):**

- [ ] **Step 1: Scaffold project and tests, add to solution**

```
dotnet new classlib -n Sport.Disciplines.BKB -o src/Sport.Disciplines.BKB -f net10.0
dotnet add src/Sport.Disciplines.BKB/Sport.Disciplines.BKB.csproj reference src/Sport.Core/Sport.Core.csproj
dotnet sln add src/Sport.Disciplines.BKB/Sport.Disciplines.BKB.csproj
rm src/Sport.Disciplines.BKB/Class1.cs

dotnet new xunit -n Sport.Disciplines.BKB.Tests -o tests/Sport.Disciplines.BKB.Tests -f net10.0
dotnet add tests/Sport.Disciplines.BKB.Tests/Sport.Disciplines.BKB.Tests.csproj package FluentAssertions
dotnet add tests/Sport.Disciplines.BKB.Tests/Sport.Disciplines.BKB.Tests.csproj reference src/Sport.Disciplines.BKB/Sport.Disciplines.BKB.csproj
dotnet sln add tests/Sport.Disciplines.BKB.Tests/Sport.Disciplines.BKB.Tests.csproj
rm tests/Sport.Disciplines.BKB.Tests/UnitTest1.cs
```

- [ ] **Step 2: Implement supporting types and the `<X>Module`**

Copy the FBL files into the new project, rename classes and substitute the row values from the table above. For BOX in particular, the `EventTypes` collection enumerates one descriptor per weight category; the `ModifierContract` is `Forbidden`. For BDM, `AllowedTypes` is `{ Athlete, Team }` and `CompositionSize` differs by type.

- [ ] **Step 3: Smoke tests mirroring `FblModuleTests`**

Adapt the three tests from Task 24 to the new module (advertised code, rejects an unsupported event type, validates entry size at boundaries).

- [ ] **Step 4: Run all tests**

Run: `dotnet test Sport.sln`
Expected: pass.

- [ ] **Step 5: Commit**

```
git add src/Sport.Disciplines.BKB/ tests/Sport.Disciplines.BKB.Tests/
git commit -m "feat(bkb): add BkbModule"
```

Repeat the entire task structure (project scaffold + module + smoke tests + commit) for **BDM (Task 26)**, **VBV (Task 27)**, **BOX (Task 28)**, and **ATH (Task 29)**. Each commit message follows the same shape: `feat(<discipline>): add <X>Module`.

---

## Phase J — Architecture tests

### Task 30: `Sport.Architecture.Tests` enforcing module boundaries

**Files:**
- Create: `tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj`
- Create: `tests/Sport.Architecture.Tests/ArchitectureRules.cs`

- [ ] **Step 1: Create test project and add NetArchTest**

```
dotnet new xunit -n Sport.Architecture.Tests -o tests/Sport.Architecture.Tests -f net10.0
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj package FluentAssertions
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj package NetArchTest.Rules
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj reference src/Sport.Core/Sport.Core.csproj
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj reference src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj reference src/Sport.Disciplines.BKB/Sport.Disciplines.BKB.csproj
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj reference src/Sport.Disciplines.BDM/Sport.Disciplines.BDM.csproj
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj reference src/Sport.Disciplines.VBV/Sport.Disciplines.VBV.csproj
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj reference src/Sport.Disciplines.BOX/Sport.Disciplines.BOX.csproj
dotnet add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj reference src/Sport.Disciplines.ATH/Sport.Disciplines.ATH.csproj
dotnet sln add tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj
rm tests/Sport.Architecture.Tests/UnitTest1.cs
```

- [ ] **Step 2: Implement fitness tests**

Path: `tests/Sport.Architecture.Tests/ArchitectureRules.cs`
```csharp
using FluentAssertions;
using NetArchTest.Rules;
using Sport.Core.DisciplineRegistry;
using Xunit;

namespace Sport.Architecture.Tests;

public class ArchitectureRules
{
    private static readonly System.Reflection.Assembly CoreAssembly = typeof(IDisciplineModule).Assembly;

    private static readonly System.Reflection.Assembly[] DisciplineAssemblies =
    {
        typeof(Sport.Disciplines.FBL.FblModule).Assembly,
        typeof(Sport.Disciplines.BKB.BkbModule).Assembly,
        typeof(Sport.Disciplines.BDM.BdmModule).Assembly,
        typeof(Sport.Disciplines.VBV.VbvModule).Assembly,
        typeof(Sport.Disciplines.BOX.BoxModule).Assembly,
        typeof(Sport.Disciplines.ATH.AthModule).Assembly,
    };

    [Fact]
    public void Core_does_not_reference_any_discipline_module()
    {
        foreach (var disciplineAsm in DisciplineAssemblies)
        {
            var disciplineNs = disciplineAsm.GetName().Name!;
            var result = Types.InAssembly(CoreAssembly)
                .Should()
                .NotHaveDependencyOn(disciplineNs)
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Sport.Core must not depend on {disciplineNs}");
        }
    }

    [Fact]
    public void Each_discipline_does_not_reference_another_discipline()
    {
        foreach (var asm in DisciplineAssemblies)
        {
            var selfNs = asm.GetName().Name!;
            foreach (var other in DisciplineAssemblies)
            {
                var otherNs = other.GetName().Name!;
                if (otherNs == selfNs) continue;

                var result = Types.InAssembly(asm)
                    .Should()
                    .NotHaveDependencyOn(otherNs)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue($"{selfNs} must not depend on {otherNs}");
            }
        }
    }

    [Fact]
    public void All_aggregate_root_IDs_use_Vogen_value_objects_not_raw_Guid()
    {
        // Aggregate roots in this design: Competition, Event, Phase, Unit, Subunit,
        // Person, Organisation, Team, Entry, OfficialAssignment, CompetitionDiscipline.
        // Each must expose an Id property whose type is a Vogen-generated struct
        // (i.e. not System.Guid directly).
        var rootClassNames = new[]
        {
            "Competition", "CompetitionDiscipline",
            "Event", "Phase", "Unit", "Subunit",
            "Person", "Organisation", "Team", "Entry",
            "OfficialAssignment",
        };

        foreach (var name in rootClassNames)
        {
            var type = CoreAssembly.GetTypes().SingleOrDefault(t => t.Name == name);
            type.Should().NotBeNull($"aggregate root '{name}' must exist in Sport.Core");

            var idProp = type!.GetProperty("Id");
            idProp.Should().NotBeNull($"'{name}.Id' must be defined");
            idProp!.PropertyType.Should().NotBe(typeof(Guid),
                $"'{name}.Id' must be a Vogen typed ID, not raw Guid");
        }
    }
}
```

- [ ] **Step 3: Run tests**

Run: `dotnet test tests/Sport.Architecture.Tests/Sport.Architecture.Tests.csproj`
Expected: all 3 pass.

- [ ] **Step 4: Commit**

```
git add tests/Sport.Architecture.Tests/
git commit -m "test(arch): enforce core/disciplines boundaries and Vogen ID usage via NetArchTest"
```

---

## Phase K — Final smoke test

### Task 31: End-to-end domain smoke test

This test wires everything together (registry + module + Competition + Event + Phase + Unit + Entry + OfficialAssignment) without any persistence to validate the conceptual model holds across submodules.

**Files:**
- Create: `tests/Sport.Core.Tests/EndToEnd/SportCoreSmokeTests.cs`

- [ ] **Step 1: Make the test project reference at least one discipline module**

```
dotnet add tests/Sport.Core.Tests/Sport.Core.Tests.csproj reference src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj
```

- [ ] **Step 2: Write the smoke test**

Path: `tests/Sport.Core.Tests/EndToEnd/SportCoreSmokeTests.cs`
```csharp
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Structure;
using Sport.Disciplines.FBL;
using Xunit;

namespace Sport.Core.Tests.EndToEnd;

public class SportCoreSmokeTests
{
    [Fact]
    public void Build_a_minimal_FBL_competition_end_to_end()
    {
        // Wire registry + module
        var services = new ServiceCollection()
            .AddSportCore()
            .AddDisciplineModule<FblModule>();
        var registry = services.BuildServiceProvider().BuildSportRegistry();

        // Competition with FBL Men's
        var compId = CompetitionId.New();
        var fbl = DisciplineCode.From("FBL");
        var comp = Competition.Create(
            compId,
            CompetitionCode.From("copa-2026"),
            "Copa 2026",
            DateRange.Create(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)),
            new[] { (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { GenderCode.M }) },
            registry);

        var compDisc = comp.Disciplines.Single();
        var fblModule = registry.Get(fbl);

        // Event
        var ev = Event.Create(EventId.New(), compDisc.Id, fbl, GenderCode.M,
            EventTypeCode.From("TEAM11"), null, "Men's Football", fblModule);

        // Phase + Unit
        var phase = ev.AddPhase(PhaseCode.From("QFNL"), 1, fblModule);
        var unit = Unit.CreateAtomic(UnitId.New(), phase.Id, UnitCode.From("000100--"), phase.Rsc, null);
        phase.AddUnit(unit);

        unit.Rsc.Value.Should().Be("FBLMTEAM11------------QFNL000100--");

        // Organisations, teams, entries
        var orgA = Organisation.Create(OrganisationId.New(), OrganisationCode.From("ESP"), "Spain", OrganisationType.Noc);
        var orgB = Organisation.Create(OrganisationId.New(), OrganisationCode.From("BRA"), "Brazil", OrganisationType.Noc);
        var teamA = Team.Create(TeamId.New(), TeamCode.From("ESP-FBL-M"), "Spain", orgA.Id, fbl);
        var teamB = Team.Create(TeamId.New(), TeamCode.From("BRA-FBL-M"), "Brazil", orgB.Id, fbl);

        var members = Enumerable.Range(1, 18)
            .Select(i => (PersonId.New(), i, (Bib?)null))
            .ToArray();

        var entryA = Entry.Create(EntryId.New(), ev.Id, EntryType.Team, orgA.Id, teamA.Id, null, null, members);
        var entryB = Entry.Create(EntryId.New(), ev.Id, EntryType.Team, orgB.Id, teamB.Id, null, null, members);

        entryA.Composition.Should().HaveCount(18);
        entryB.Composition.Should().HaveCount(18);

        // OfficialAssignment (referee on the unit)
        var refereeDescriptor = fblModule.Functions.Single(f => f.Code.Value == "FBL.REF");
        var assignment = OfficialAssignment.Create(
            OfficialAssignmentId.New(),
            PersonId.New(),
            refereeDescriptor,
            OfficialScope.Create(ScopeLevel.Unit, unit.Id.Value),
            organisationId: null);

        assignment.Status.Should().Be(OfficialAssignmentStatus.Active);
    }
}
```

- [ ] **Step 3: Run the test**

Run: `dotnet test tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~SportCoreSmokeTests"`
Expected: pass.

- [ ] **Step 4: Commit**

```
git add tests/Sport.Core.Tests/EndToEnd/SportCoreSmokeTests.cs
git commit -m "test(e2e): add end-to-end smoke test wiring registry, competition, event, units, entries and officials"
```

---

## Self-review checklist (for the engineer)

After completing all tasks, verify against the spec:

1. **Spec coverage** — every section of `docs/superpowers/specs/2026-05-27-core-deportivo-design.md` (Competitions, Structure, Participants, Officials, DisciplineRegistry, RSC, plus all invariants tagged I-COMP-*, I-STR-*, I-PAR-*, I-OFF-*, I-REG-*) maps to at least one task. Sketches in §11 of the spec are realized by Tasks 24-29.
2. **No placeholders** — no `TBD`, `TODO`, or `Class1.cs` stubs remain in `src/`.
3. **Type consistency** — `Rsc.Compose` signature, `Event.Create`, `Phase.Create`, and `Unit.CreateAtomic`/`CreateParentForSubunits` use the same parameter order across tests and implementation.
4. **All tests green** — `dotnet test Sport.sln` reports 0 failures.
5. **NetArchTest passes** — boundaries between `Sport.Core` and `Sport.Disciplines.*` are enforced.

If any item fails, fix inline before declaring the plan complete.

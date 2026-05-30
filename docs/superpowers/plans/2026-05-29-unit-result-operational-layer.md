# Unit Result Operational Layer — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the conceptual domain model of the operational result layer (`Sport.Core.Results`) plus the discipline result-schema contract, validated end-to-end with FBL, ATH, BOX and JUD.

**Architecture:** A core aggregate `UnitResultDocument` carries the common envelope (status, per-competitor rows, segments, ODF-style extensions). All mutations go through governed methods (`ApplySnapshot`, `TransitionTo`, `ApplyRollup`) that consult the discipline's `IResultSchema` (sibling of `IPhaseCatalog`) and bump `Version` exactly once per logical mutation. Each discipline contributes a typed schema that validates the generic components, projects a typed read view, and — for subunit-hosting events — aggregates contest results into the parent (JUD team match). No persistence, API, play-by-play or standings in this plan.

**Tech Stack:** .NET 10, C#, Vogen (value objects), xUnit + FluentAssertions. New code lives in the existing `Sport.Core` and `Sport.Disciplines.*` projects; tests live in the existing `Sport.Core.Tests` and `Sport.Disciplines.*.Tests` projects. No new `.csproj`. Solution: `apps/api/Sport.slnx`.

**Spec:** `docs/superpowers/specs/2026-05-29-unit-result-operational-layer-design.md`

---

## File Structure

All paths are relative to `apps/api/`.

**New — core model (`src/Sport.Core/Results/`):**
- `UnitResultId.cs`, `ResultStatus.cs`, `Wlt.cs`, `OutcomeMode.cs`
- `Irm.cs`, `ResultTypeCode.cs`, `SegmentCode.cs`, `ExtensionType.cs`, `ExtensionCode.cs`
- `ResultExtension.cs`, `SegmentScore.cs`, `CompetitorMemberResult.cs`, `ResultSegment.cs`, `CompetitorResult.cs`
- `UnitResultDocument.cs`

**New — registry contract (`src/Sport.Core/DisciplineRegistry/`):**
- `DisciplineResultProjection.cs`, `ResultRollup.cs`, `IResultSchema.cs`
- `IDisciplineModule.cs` — *modify*: add `ResultSchema` with permissive default.

**New — discipline schemas (each `src/Sport.Disciplines.<D>/`):**
- `FblResultSchema.cs` + `FootballMatchResult.cs`
- `AthResultSchema.cs` + `HighJumpResult.cs`
- `BoxResultSchema.cs` + `BoxingBoutResult.cs`
- `JudResultSchema.cs` + `JudoTeamMatchResult.cs`

**New — tests:** `tests/Sport.Core.Tests/Results/*`, `tests/Sport.Disciplines.<D>.Tests/<D>ResultSchemaTests.cs`, `tests/Sport.Architecture.Tests/ResultsLayerArchitectureTests.cs`.

### Canonical signatures (anchor for all tasks)

```csharp
// Sport.Core.Results
public enum ResultStatus { StartList, Live, Intermediate, Unconfirmed, Unofficial, Official, Partial, Protested, Provisional }
public enum Wlt { W, L, T }
public enum OutcomeMode { HeadToHead, Ranked }

public sealed record ResultExtension(ExtensionType Type, ExtensionCode Code)
{
    public string? Pos { get; init; }
    public string? Value { get; init; }
    public string? Value2 { get; init; }
    public int? Rank { get; init; }
    public bool RankEqual { get; init; }
    public int? SortOrder { get; init; }
    public string? Diff { get; init; }
    public string? Move { get; init; }
    public string? Attempt { get; init; }
    public IReadOnlyList<ResultExtension> Children { get; init; } = Array.Empty<ResultExtension>();
}

public sealed record SegmentScore(EntryId EntryId)
{
    public string? CumulativeValue { get; init; }
    public string? SegmentValue { get; init; }
}

public sealed record CompetitorMemberResult(PersonId PersonId, int Order)
{
    public Bib? Bib { get; init; }
    public int? StartSortOrder { get; init; }
    public IReadOnlyList<ResultExtension> Extensions { get; init; } = Array.Empty<ResultExtension>();
}

public sealed record ResultSegment(SegmentCode Code, int Order)
{
    public IReadOnlyList<SegmentScore> Scores { get; init; } = Array.Empty<SegmentScore>();
    public IReadOnlyList<ResultExtension> Extensions { get; init; } = Array.Empty<ResultExtension>();
}

public sealed record CompetitorResult(EntryId EntryId, int SortOrder)
{
    public string? ResultValue { get; init; }
    public ResultTypeCode? ResultType { get; init; }
    public Wlt? Wlt { get; init; }
    public int? Rank { get; init; }
    public bool RankEqual { get; init; }
    public Irm? Irm { get; init; }
    public int? StartOrder { get; init; }
    public int? StartSortOrder { get; init; }
    public IReadOnlyList<CompetitorMemberResult> Composition { get; init; } = Array.Empty<CompetitorMemberResult>();
    public IReadOnlyList<ResultExtension> Extensions { get; init; } = Array.Empty<ResultExtension>();
}

// Sport.Core.DisciplineRegistry
public abstract record DisciplineResultProjection;

public sealed record ResultRollup(
    IReadOnlyList<CompetitorResult> Competitors,
    IReadOnlyList<ResultExtension> Extensions,
    ResultStatus? SuggestedStatus);

public interface IResultSchema
{
    OutcomeMode OutcomeModeFor(EventTypeCode type);
    IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type);
    bool CanTransition(ResultStatus from, ResultStatus to, EventTypeCode type);
    IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type);
    Result Validate(UnitResultDocument document);
    DisciplineResultProjection Project(UnitResultDocument document);
    ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults);
}
```

### Aggregate mutation contract (the heart of the model)

`UnitResultDocument` is a **domain aggregate, not a dumb container**. All writes go through three governed methods, each bumping `Version` exactly once:

- `ApplySnapshot(competitors, segments, extensions, schema)` — enforces core invariant **I-RES-6** (unique `SortOrder`), then runs `schema.Validate(this)`; on failure it **restores the previous state** (atomic) and throws `DomainException("I-RES-8")`.
- `TransitionTo(status, schema)` — enforces **I-RES-4** via `StatusesFor` + `CanTransition`.
- `ApplyRollup(rollup, schema)` — parent-only (**I-RES-10**); replaces competitor rows + extensions and optionally transitions, as **one** mutation.

There are no raw public setters. This guarantees no write bypasses the schema (closes the P2 gap) and that `Version` reflects logical mutations (closes the rollup-atomicity gap).

---

## Task 1: Identity and enum value objects

**Files:**
- Create: `src/Sport.Core/Results/UnitResultId.cs`, `ResultStatus.cs`, `Wlt.cs`, `OutcomeMode.cs`
- Test: `tests/Sport.Core.Tests/Results/ResultEnumsTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.Results;

namespace Sport.Core.Tests.Results;

public class ResultEnumsTests
{
    [Fact]
    public void UnitResultId_New_produces_unique_non_empty_ids()
    {
        var a = UnitResultId.New();
        a.Value.Should().NotBe(Guid.Empty);
        a.Should().NotBe(UnitResultId.New());
    }

    [Fact]
    public void ResultStatus_includes_all_nine_odf_common_codes()
    {
        Enum.GetValues<ResultStatus>().Should().HaveCount(9);
        Enum.IsDefined(ResultStatus.Partial).Should().BeTrue();
        Enum.IsDefined(ResultStatus.Protested).Should().BeTrue();
        Enum.IsDefined(ResultStatus.Provisional).Should().BeTrue();
    }

    [Fact]
    public void Wlt_and_OutcomeMode_expose_expected_members()
    {
        Enum.GetValues<Wlt>().Should().HaveCount(3);
        Enum.GetValues<OutcomeMode>().Should().BeEquivalentTo(new[] { OutcomeMode.HeadToHead, OutcomeMode.Ranked });
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultEnumsTests`
Expected: FAIL — namespace/types not found.

- [ ] **Step 3: Write the implementation**

`UnitResultId.cs`:
```csharp
using Vogen;

namespace Sport.Core.Results;

[ValueObject<Guid>]
public readonly partial struct UnitResultId
{
    public static UnitResultId New() => From(Guid.CreateVersion7());
}
```

`ResultStatus.cs`:
```csharp
namespace Sport.Core.Results;

public enum ResultStatus
{
    StartList, Live, Intermediate, Unconfirmed, Unofficial, Official, Partial, Protested, Provisional,
}
```

`Wlt.cs`:
```csharp
namespace Sport.Core.Results;

public enum Wlt { W, L, T }
```

`OutcomeMode.cs`:
```csharp
namespace Sport.Core.Results;

public enum OutcomeMode { HeadToHead, Ranked }
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultEnumsTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Results apps/api/tests/Sport.Core.Tests/Results
git commit -m "feat(results): identity and enum value objects"
```

---

## Task 2: String value objects (`SegmentCode` accepts `-`)

**Files:**
- Create: `src/Sport.Core/Results/Irm.cs`, `ResultTypeCode.cs`, `SegmentCode.cs`, `ExtensionType.cs`, `ExtensionCode.cs`
- Test: `tests/Sport.Core.Tests/Results/ResultStringVoTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.Results;
using Vogen;

namespace Sport.Core.Tests.Results;

public class ResultStringVoTests
{
    [Theory]
    [InlineData("DNS")]
    [InlineData("DQB")]
    [InlineData("WDR")]
    public void Irm_accepts_uppercase_codes(string code) => Irm.From(code).Value.Should().Be(code);

    [Theory]
    [InlineData("")]
    [InlineData("dns")]
    [InlineData("TOO-LONG-IRM")]
    public void Irm_rejects_invalid_codes(string code) =>
        FluentActions.Invoking(() => Irm.From(code)).Should().Throw<ValueObjectValidationException>();

    [Fact]
    public void SegmentCode_accepts_hyphenated_odf_period_codes()
    {
        // ODF FBL uses ET-H1 / ET-H2 for extra-time halves.
        SegmentCode.From("ET-H1").Value.Should().Be("ET-H1");
        SegmentCode.From("H1").Value.Should().Be("H1");
    }

    [Fact]
    public void Vocab_codes_round_trip()
    {
        ResultTypeCode.From("RM_POINTS").Value.Should().Be("RM_POINTS");
        ExtensionType.From("ER").Value.Should().Be("ER");
        ExtensionCode.From("INTERMEDIATE").Value.Should().Be("INTERMEDIATE");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultStringVoTests`
Expected: FAIL — types not found.

- [ ] **Step 3: Write the implementation**

`Irm.cs` (uppercase alphanumeric, `MaxLength = 6`):
```csharp
using Vogen;

namespace Sport.Core.Results;

[ValueObject<string>]
public readonly partial struct Irm
{
    public const int MaxLength = 6;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("Irm is required.");
        if (value.Length > MaxLength) return Validation.Invalid($"Irm must be at most {MaxLength} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z' || c is >= '0' and <= '9';
            if (!ok) return Validation.Invalid("Irm chars must be uppercase alphanumeric.");
        }
        return Validation.Ok;
    }
}
```

`ResultTypeCode.cs` — same shape, `MaxLength = 12`, allowed chars `A–Z 0–9 _`.

`SegmentCode.cs` — same shape, `MaxLength = 8`, allowed chars **`A–Z 0–9 - _`** (the `-` is required for `ET-H1`):
```csharp
using Vogen;

namespace Sport.Core.Results;

[ValueObject<string>]
public readonly partial struct SegmentCode
{
    public const int MaxLength = 8;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("SegmentCode is required.");
        if (value.Length > MaxLength) return Validation.Invalid($"SegmentCode must be at most {MaxLength} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z' || c is >= '0' and <= '9' || c == '-' || c == '_';
            if (!ok) return Validation.Invalid("SegmentCode chars must be uppercase alphanumeric, '-' or '_'.");
        }
        return Validation.Ok;
    }
}
```

`ExtensionType.cs` — same shape as `ResultTypeCode`, `MaxLength = 8`, allowed `A–Z 0–9 _`.
`ExtensionCode.cs` — same shape, `MaxLength = 24`, allowed `A–Z 0–9 _`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultStringVoTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Results apps/api/tests/Sport.Core.Tests/Results
git commit -m "feat(results): string vocabulary value objects (SegmentCode allows '-')"
```

---

## Task 3: Leaf component records

**Files:**
- Create: `src/Sport.Core/Results/ResultExtension.cs`, `SegmentScore.cs`, `CompetitorMemberResult.cs`, `ResultSegment.cs`, `CompetitorResult.cs`
- Test: `tests/Sport.Core.Tests/Results/ResultComponentsTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.Participants;
using Sport.Core.Results;

namespace Sport.Core.Tests.Results;

public class ResultComponentsTests
{
    [Fact]
    public void ResultExtension_nests_children_for_team_comp()
    {
        var ext = new ResultExtension(ExtensionType.From("TEAM"), ExtensionCode.From("COMP"))
        {
            Pos = "1",
            Children = new[]
            {
                new ResultExtension(ExtensionType.From("TEAM"), ExtensionCode.From("WEIGHT_CATEGORY")) { Value = "JUDW57KG" },
            },
        };
        ext.Children.Should().ContainSingle().Which.Value.Should().Be("JUDW57KG");
    }

    [Fact]
    public void CompetitorMemberResult_carries_lineup_order()
    {
        var m = new CompetitorMemberResult(PersonId.New(), Order: 1) { StartSortOrder = 1 };
        m.Order.Should().Be(1);
        m.Extensions.Should().BeEmpty();
    }

    [Fact]
    public void SegmentScore_separates_cumulative_and_segment_values()
    {
        var s = new SegmentScore(EntryId.New()) { CumulativeValue = "2", SegmentValue = "1" };
        s.CumulativeValue.Should().Be("2");
        s.SegmentValue.Should().Be("1");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultComponentsTests`
Expected: FAIL — types not found.

- [ ] **Step 3: Write the implementation**

Create the five records exactly as in "Canonical signatures" (`namespace Sport.Core.Results;`, `using Sport.Core.Participants;` where `EntryId`/`PersonId`/`Bib` appear).

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultComponentsTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Results apps/api/tests/Sport.Core.Tests/Results
git commit -m "feat(results): leaf component records"
```

---

## Task 4: Contract + `UnitResultDocument` aggregate

These two are mutually type-dependent (the interface's `Validate` takes the aggregate; the aggregate's methods take the interface). They live in the same assembly, so **write all production files in Step 3 before running the test suite.**

**Files:**
- Create: `src/Sport.Core/DisciplineRegistry/DisciplineResultProjection.cs`, `ResultRollup.cs`, `IResultSchema.cs`
- Create: `src/Sport.Core/Results/UnitResultDocument.cs`
- Test: `tests/Sport.Core.Tests/Results/UnitResultDocumentTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Results;

public class UnitResultDocumentTests
{
    private static readonly DisciplineCode Disc = DisciplineCode.From("FBL");
    private static readonly EventTypeCode Type = EventTypeCode.From("TEAM11");
    private static readonly Rsc UnitRsc = Rsc.From("FBLMTEAM11------------FNL-000100--");
    private static readonly IResultSchema Schema = new DefaultResultSchema();

    private static UnitResultDocument NewDoc() =>
        UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, Disc, Type);

    [Fact]
    public void CreateForUnit_starts_in_StartList_with_no_subunit_and_version_1()
    {
        var doc = NewDoc();
        doc.Status.Should().Be(ResultStatus.StartList);
        doc.TargetSubunitId.Should().BeNull();
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void CreateForSubunit_sets_target_subunit()
    {
        var unitId = UnitId.New();
        var doc = UnitResultDocument.CreateForSubunit(UnitResultId.New(), unitId, SubunitId.New(), UnitRsc, Disc, Type);
        doc.TargetUnitId.Should().Be(unitId);
        doc.TargetSubunitId.Should().NotBeNull();
    }

    [Fact]
    public void ApplySnapshot_sets_rows_and_bumps_version_once()
    {
        var doc = NewDoc();
        doc.ApplySnapshot(
            new[] { new CompetitorResult(EntryId.New(), 1) { Wlt = Wlt.W }, new CompetitorResult(EntryId.New(), 2) { Wlt = Wlt.L } },
            Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), Schema);
        doc.Competitors.Should().HaveCount(2);
        doc.Version.Should().Be(2);
    }

    [Fact]
    public void ApplySnapshot_rejects_duplicate_sort_order_atomically()
    {
        var doc = NewDoc();
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1), new CompetitorResult(EntryId.New(), 1) },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), Schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-6");
        doc.Competitors.Should().BeEmpty();   // atomic: nothing applied
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void ApplySnapshot_rejects_when_schema_validation_fails_and_restores_state()
    {
        var doc = NewDoc();
        var rejecting = new RejectingSchema();
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), rejecting))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-8");
        doc.Competitors.Should().BeEmpty();
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void TransitionTo_is_rejected_when_status_outside_schema_set()
    {
        var schema = new FixedStatusSchema(new HashSet<ResultStatus> { ResultStatus.StartList, ResultStatus.Live });
        FluentActions.Invoking(() => NewDoc().TransitionTo(ResultStatus.Official, schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-4");
    }

    [Fact]
    public void ApplyRollup_is_atomic_when_suggested_transition_is_illegal()
    {
        var schema = new FixedStatusSchema(new HashSet<ResultStatus> { ResultStatus.StartList }); // Official not allowed
        var doc = NewDoc();
        var rollup = new ResultRollup(
            new[] { new CompetitorResult(EntryId.New(), 1) },
            Array.Empty<ResultExtension>(),
            ResultStatus.Official);
        FluentActions.Invoking(() => doc.ApplyRollup(rollup, schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-4");
        doc.Competitors.Should().BeEmpty();   // nothing applied
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void TransitionTo_rejects_an_illegal_intra_set_transition()
    {
        var schema = new DefaultResultSchema();   // full status set, coarse transition rules
        var doc = NewDoc();
        doc.TransitionTo(ResultStatus.Official, schema);   // allowed
        FluentActions.Invoking(() => doc.TransitionTo(ResultStatus.Live, schema))   // Official is terminal
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-4");
    }

    private sealed class RejectingSchema : DefaultResultSchema
    {
        public override Result Validate(UnitResultDocument document) => Result.Fail("nope");
    }

    private sealed class FixedStatusSchema(IReadOnlySet<ResultStatus> statuses) : DefaultResultSchema
    {
        public override IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type) => statuses;
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter UnitResultDocumentTests`
Expected: FAIL — `IResultSchema` / `UnitResultDocument` not found.

- [ ] **Step 3: Write the implementation**

`DisciplineResultProjection.cs`:
```csharp
namespace Sport.Core.DisciplineRegistry;

public abstract record DisciplineResultProjection;

public sealed record DefaultResultProjection : DisciplineResultProjection;
```

`ResultRollup.cs`:
```csharp
using Sport.Core.Results;

namespace Sport.Core.DisciplineRegistry;

public sealed record ResultRollup(
    IReadOnlyList<CompetitorResult> Competitors,
    IReadOnlyList<ResultExtension> Extensions,
    ResultStatus? SuggestedStatus);
```

`IResultSchema.cs`:
```csharp
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.DisciplineRegistry;

public interface IResultSchema
{
    OutcomeMode OutcomeModeFor(EventTypeCode type);
    IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type);
    bool CanTransition(ResultStatus from, ResultStatus to, EventTypeCode type);
    IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type);
    Result Validate(UnitResultDocument document);
    DisciplineResultProjection Project(UnitResultDocument document);
    ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults);
}

// Permissive default used by disciplines without a real grammar yet.
public class DefaultResultSchema : IResultSchema
{
    private static readonly IReadOnlySet<ResultStatus> AllStatuses =
        new HashSet<ResultStatus>(Enum.GetValues<ResultStatus>());

    private static readonly IReadOnlySet<Irm> CoreIrms =
        new HashSet<Irm> { Irm.From("DNS"), Irm.From("DNF"), Irm.From("DSQ"), Irm.From("WDR") };

    public virtual OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;
    public virtual IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type) => AllStatuses;
    // Coarse-grained default (non-vacuous): no self-transition, never rewind to StartList,
    // Official is terminal except on protest. Disciplines may override with a finer graph.
    public virtual bool CanTransition(ResultStatus from, ResultStatus to, EventTypeCode type)
    {
        if (from == to) return false;
        if (to == ResultStatus.StartList) return false;
        if (from == ResultStatus.Official && to != ResultStatus.Protested) return false;
        return true;
    }
    public virtual IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type) => CoreIrms;
    public virtual Result Validate(UnitResultDocument document) => Result.Ok();
    public virtual DisciplineResultProjection Project(UnitResultDocument document) => new DefaultResultProjection();

    public virtual ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults) =>
        throw new NotSupportedException("This discipline does not host subunits.");
}
```

`UnitResultDocument.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Results;

public sealed class UnitResultDocument
{
    public UnitResultId Id { get; }
    public UnitId TargetUnitId { get; }
    public SubunitId? TargetSubunitId { get; }
    public Rsc TargetRsc { get; }
    public DisciplineCode DisciplineCode { get; }
    public EventTypeCode EventType { get; }
    public ResultStatus Status { get; private set; }
    public int Version { get; private set; }

    private readonly List<CompetitorResult> _competitors = new();
    public IReadOnlyList<CompetitorResult> Competitors => _competitors;

    private readonly List<ResultSegment> _segments = new();
    public IReadOnlyList<ResultSegment> Segments => _segments;

    private readonly List<ResultExtension> _extensions = new();
    public IReadOnlyList<ResultExtension> Extensions => _extensions;

    private UnitResultDocument(
        UnitResultId id, UnitId unitId, SubunitId? subunitId, Rsc rsc,
        DisciplineCode discipline, EventTypeCode eventType)
    {
        Id = id; TargetUnitId = unitId; TargetSubunitId = subunitId; TargetRsc = rsc;
        DisciplineCode = discipline; EventType = eventType;
        Status = ResultStatus.StartList; Version = 1;
    }

    public static UnitResultDocument CreateForUnit(
        UnitResultId id, UnitId unitId, Rsc unitRsc, DisciplineCode discipline, EventTypeCode eventType) =>
        new(id, unitId, null, unitRsc, discipline, eventType);

    public static UnitResultDocument CreateForSubunit(
        UnitResultId id, UnitId unitId, SubunitId subunitId, Rsc subunitRsc, DisciplineCode discipline, EventTypeCode eventType) =>
        new(id, unitId, subunitId, subunitRsc, discipline, eventType);

    public void ApplySnapshot(
        IReadOnlyList<CompetitorResult> competitors,
        IReadOnlyList<ResultSegment> segments,
        IReadOnlyList<ResultExtension> extensions,
        IResultSchema schema)
    {
        EnsureUniqueSortOrder(competitors);

        var prevC = _competitors.ToList();
        var prevS = _segments.ToList();
        var prevE = _extensions.ToList();

        Replace(_competitors, competitors);
        Replace(_segments, segments);
        Replace(_extensions, extensions);

        var validation = schema.Validate(this);
        if (!validation.IsSuccess)
        {
            Replace(_competitors, prevC);
            Replace(_segments, prevS);
            Replace(_extensions, prevE);
            throw new DomainException("I-RES-8", validation.Error!);
        }
        Version++;
    }

    public void TransitionTo(ResultStatus to, IResultSchema schema)
    {
        EnsureTransition(to, schema);
        Status = to;
        Version++;
    }

    public void ApplyRollup(ResultRollup rollup, IResultSchema schema)
    {
        if (TargetSubunitId is not null)
            throw new DomainException("I-RES-10", "Rollup applies only to a parent (unit-level) document.");
        EnsureUniqueSortOrder(rollup.Competitors);

        // Validate the suggested transition BEFORE mutating, so a rejected status leaves the document untouched.
        var willTransition = rollup.SuggestedStatus is { } s && s != Status;
        if (willTransition)
            EnsureTransition(rollup.SuggestedStatus!.Value, schema);

        Replace(_competitors, rollup.Competitors);
        Replace(_extensions, rollup.Extensions);
        if (willTransition)
            Status = rollup.SuggestedStatus!.Value;
        Version++;
    }

    private void EnsureTransition(ResultStatus to, IResultSchema schema)
    {
        if (!schema.StatusesFor(EventType).Contains(to))
            throw new DomainException("I-RES-4", $"ResultStatus '{to}' is not used by this discipline/event.");
        if (!schema.CanTransition(Status, to, EventType))
            throw new DomainException("I-RES-4", $"Illegal ResultStatus transition '{Status}' -> '{to}'.");
    }

    private static void EnsureUniqueSortOrder(IReadOnlyList<CompetitorResult> competitors)
    {
        var orders = competitors.Select(c => c.SortOrder).ToList();
        if (orders.Count != orders.Distinct().Count())
            throw new DomainException("I-RES-6", "CompetitorResult.SortOrder must be unique within a document.");
    }

    private static void Replace<T>(List<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter UnitResultDocumentTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/DisciplineRegistry apps/api/src/Sport.Core/Results apps/api/tests/Sport.Core.Tests/Results
git commit -m "feat(results): result-schema contract and governed UnitResultDocument aggregate"
```

---

## Task 5: Wire `ResultSchema` into `IDisciplineModule` (permissive default)

**Files:**
- Modify: `src/Sport.Core/DisciplineRegistry/IDisciplineModule.cs`
- Test: `tests/Sport.Core.Tests/DisciplineRegistry/ModuleResultSchemaDefaultTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

public class ModuleResultSchemaDefaultTests
{
    [Fact]
    public void Module_without_override_exposes_permissive_default_schema()
    {
        IDisciplineModule module = new BareModule();
        module.ResultSchema.Should().BeOfType<DefaultResultSchema>();
        module.ResultSchema.OutcomeModeFor(EventTypeCode.From("ANY")).Should().Be(OutcomeMode.HeadToHead);
    }

    private sealed class BareModule : IDisciplineModule
    {
        public DisciplineCode Code => DisciplineCode.From("ZZZ");
        public string DisplayName => "Bare";
        public IReadOnlySet<GenderCode> SupportedGenders => new HashSet<GenderCode> { GenderCode.M };
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => Array.Empty<EventTypeDescriptor>();
        public IPhaseCatalog PhaseCatalog => null!;
        public IUnitCodeStrategy UnitCodeStrategy => null!;
        public IReadOnlyCollection<FunctionDescriptor> Functions => Array.Empty<FunctionDescriptor>();
        public IEntryRules EntryRules => null!;
        public Sport.Core.Shared.Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => Sport.Core.Shared.Result.Ok();
        public Sport.Core.Shared.Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => Sport.Core.Shared.Result.Ok();
        public Sport.Core.Shared.Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Sport.Core.Shared.Result.Ok();
        public Sport.Core.Shared.Result ValidateEntry(EntryCandidate candidate) => Sport.Core.Shared.Result.Ok();
        public Sport.Core.Shared.Result ValidateOfficialFunctionInScope(FunctionCode function, Sport.Core.Officials.ScopeLevel level) => Sport.Core.Shared.Result.Ok();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ModuleResultSchemaDefaultTests`
Expected: FAIL — `IDisciplineModule` has no `ResultSchema`.

- [ ] **Step 3: Write the implementation**

In `src/Sport.Core/DisciplineRegistry/IDisciplineModule.cs`, add inside the interface (after the existing default subunit-hosting members):
```csharp
    // Operational-result grammar. Disciplines override with a typed schema; default is permissive.
    IResultSchema ResultSchema => SharedDefaultResultSchema;

    private static readonly IResultSchema SharedDefaultResultSchema = new DefaultResultSchema();
```
(`DefaultResultSchema` is in the same `DisciplineRegistry` namespace — no extra `using` needed.)

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ModuleResultSchemaDefaultTests`
Then: `dotnet build apps/api/Sport.slnx`
Expected: test PASS; all 7 discipline modules still build (BKB/BDM/VBV inherit the default).

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/DisciplineRegistry apps/api/tests/Sport.Core.Tests/DisciplineRegistry
git commit -m "feat(results): expose ResultSchema on IDisciplineModule with permissive default"
```

---

## Task 6: FBL schema + projection (head-to-head + periods)

**Files:**
- Create: `src/Sport.Disciplines.FBL/FblResultSchema.cs`, `FootballMatchResult.cs`
- Modify: `src/Sport.Disciplines.FBL/FblModule.cs`
- Test: `tests/Sport.Disciplines.FBL.Tests/FblResultSchemaTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL.Tests;

public class FblResultSchemaTests
{
    private static readonly EventTypeCode Team11 = EventTypeCode.From("TEAM11");
    private static readonly Rsc UnitRsc = Rsc.From("FBLMTEAM11------------FNL-000100--");

    private static UnitResultDocument Match(FblResultSchema schema, out EntryId home, out EntryId away)
    {
        home = EntryId.New(); away = EntryId.New();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("FBL"), Team11);
        doc.ApplySnapshot(
            new[]
            {
                new CompetitorResult(home, 1) { Wlt = Wlt.W, ResultValue = "2", ResultType = ResultTypeCode.From("POINTS") },
                new CompetitorResult(away, 2) { Wlt = Wlt.L, ResultValue = "1", ResultType = ResultTypeCode.From("POINTS") },
            },
            new[]
            {
                new ResultSegment(SegmentCode.From("H1"), 1) { Scores = new[] { new SegmentScore(home) { CumulativeValue = "1", SegmentValue = "1" }, new SegmentScore(away) { CumulativeValue = "0", SegmentValue = "0" } } },
                new ResultSegment(SegmentCode.From("H2"), 2) { Scores = new[] { new SegmentScore(home) { CumulativeValue = "2", SegmentValue = "1" }, new SegmentScore(away) { CumulativeValue = "1", SegmentValue = "1" } } },
            },
            Array.Empty<ResultExtension>(), schema);
        return doc;
    }

    [Fact]
    public void Schema_is_head_to_head_and_validates_a_two_row_match_with_periods()
    {
        var schema = new FblResultSchema();
        var doc = Match(schema, out _, out _);
        schema.OutcomeModeFor(Team11).Should().Be(OutcomeMode.HeadToHead);
        doc.Segments.Should().HaveCount(2);
    }

    [Fact]
    public void Schema_rejects_ranked_outcome_with_a_rank_field()
    {
        var schema = new FblResultSchema();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("FBL"), Team11);
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) { Rank = 1 } },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-8");
    }

    [Fact]
    public void Projection_exposes_team_scores_and_per_period_scores()
    {
        var schema = new FblResultSchema();
        var doc = Match(schema, out var home, out _);
        var proj = (FootballMatchResult)schema.Project(doc);
        proj.TeamScores[home].Should().Be("2");
        proj.Periods.Select(p => p.Code).Should().ContainInOrder("H1", "H2");
        var h2 = proj.Periods.Single(p => p.Code == "H2");
        h2.Scores.Single(s => s.EntryId == home).Cumulative.Should().Be("2");
        h2.Scores.Single(s => s.EntryId == home).Segment.Should().Be("1");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.FBL.Tests --filter FblResultSchemaTests`
Expected: FAIL — `FblResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`FootballMatchResult.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Disciplines.FBL;

public sealed record FootballPeriodScore(EntryId EntryId, string? Cumulative, string? Segment);

public sealed record FootballPeriod(string Code, IReadOnlyList<FootballPeriodScore> Scores);

public sealed record FootballMatchResult(
    IReadOnlyDictionary<EntryId, string> TeamScores,
    IReadOnlyList<FootballPeriod> Periods) : DisciplineResultProjection;
```

`FblResultSchema.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL;

public sealed class FblResultSchema : DefaultResultSchema
{
    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;

    public override Result Validate(UnitResultDocument document)
    {
        foreach (var row in document.Competitors)
        {
            if (row.Rank is not null)
                return Result.Fail("FBL does not use Rank; use WLT.");
            var scored = document.Status != ResultStatus.StartList && row.Irm is null;
            if (scored && row.Wlt is null)
                return Result.Fail("FBL is head-to-head: each scored competitor must carry a WLT.");
        }
        return Result.Ok();
    }

    public override DisciplineResultProjection Project(UnitResultDocument document)
    {
        var scores = document.Competitors
            .Where(c => c.ResultValue is not null)
            .ToDictionary(c => c.EntryId, c => c.ResultValue!);
        var periods = document.Segments
            .OrderBy(s => s.Order)
            .Select(s => new FootballPeriod(
                s.Code.Value,
                s.Scores.Select(sc => new FootballPeriodScore(sc.EntryId, sc.CumulativeValue, sc.SegmentValue)).ToArray()))
            .ToArray();
        return new FootballMatchResult(scores, periods);
    }
}
```

In `src/Sport.Disciplines.FBL/FblModule.cs`, add `using Sport.Core.DisciplineRegistry;` if missing and the member:
```csharp
    public IResultSchema ResultSchema { get; } = new FblResultSchema();
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.FBL.Tests --filter FblResultSchemaTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.FBL apps/api/tests/Sport.Disciplines.FBL.Tests
git commit -m "feat(results): FBL head-to-head schema with periods projection"
```

---

## Task 7: ATH schema + projection (ranked + attempts)

**Files:**
- Create: `src/Sport.Disciplines.ATH/AthResultSchema.cs`, `HighJumpResult.cs`
- Modify: `src/Sport.Disciplines.ATH/AthModule.cs`
- Test: `tests/Sport.Disciplines.ATH.Tests/AthResultSchemaTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH.Tests;

public class AthResultSchemaTests
{
    private static readonly EventTypeCode Hj = EventTypeCode.From("HJ");
    private static readonly Rsc UnitRsc = Rsc.From("ATHWHJ----------------FNL-000100--");

    private static UnitResultDocument HighJump(AthResultSchema schema)
    {
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("ATH"), Hj);
        doc.ApplySnapshot(
            new[]
            {
                new CompetitorResult(EntryId.New(), 1)
                {
                    Rank = 1, ResultValue = "1.92", ResultType = ResultTypeCode.From("DISTANCE"),
                    Extensions = new[]
                    {
                        new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("INTERMEDIATE")) { Pos = "1", Value = "o" },
                        new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("INTERMEDIATE")) { Pos = "2", Value = "xo" },
                    },
                },
            },
            Array.Empty<ResultSegment>(),
            new[]
            {
                new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("INTERMEDIATE")) { Pos = "1", Value = "1.88" },
                new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("INTERMEDIATE")) { Pos = "2", Value = "1.92" },
            },
            schema);
        return doc;
    }

    [Fact]
    public void Schema_is_ranked_and_validates_without_wlt()
    {
        var schema = new AthResultSchema();
        schema.OutcomeModeFor(Hj).Should().Be(OutcomeMode.Ranked);
        HighJump(schema).Competitors.Should().ContainSingle();
    }

    [Fact]
    public void Schema_rejects_a_wlt_outcome()
    {
        var schema = new AthResultSchema();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("ATH"), Hj);
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) { Wlt = Wlt.W } },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-8");
    }

    [Fact]
    public void Projection_exposes_heights_and_attempt_series()
    {
        var schema = new AthResultSchema();
        var proj = (HighJumpResult)schema.Project(HighJump(schema));
        proj.Heights.Should().ContainInOrder("1.88", "1.92");
        proj.Athletes.Should().ContainSingle();
        proj.Athletes[0].BestMark.Should().Be("1.92");
        proj.Athletes[0].Attempts.Should().ContainInOrder("o", "xo");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.ATH.Tests --filter AthResultSchemaTests`
Expected: FAIL — `AthResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`HighJumpResult.cs`:
```csharp
using Sport.Core.DisciplineRegistry;

namespace Sport.Disciplines.ATH;

public sealed record HighJumpAthleteResult(string? BestMark, int? Rank, IReadOnlyList<string> Attempts);

public sealed record HighJumpResult(
    IReadOnlyList<string> Heights,
    IReadOnlyList<HighJumpAthleteResult> Athletes) : DisciplineResultProjection;
```

`AthResultSchema.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH;

public sealed class AthResultSchema : DefaultResultSchema
{
    private static readonly ExtensionCode Intermediate = ExtensionCode.From("INTERMEDIATE");

    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.Ranked;

    public override Result Validate(UnitResultDocument document)
    {
        foreach (var row in document.Competitors)
        {
            if (row.Wlt is not null)
                return Result.Fail("ATH is ranked: WLT must be absent.");
            if (row.SortOrder <= 0)
                return Result.Fail("ATH requires a positive SortOrder on every row.");
        }
        return Result.Ok();
    }

    public override DisciplineResultProjection Project(UnitResultDocument document)
    {
        var heights = document.Extensions
            .Where(e => e.Code == Intermediate)
            .OrderBy(e => e.Pos)
            .Select(e => e.Value ?? string.Empty)
            .ToArray();

        var athletes = document.Competitors
            .Select(c => new HighJumpAthleteResult(
                c.ResultValue,
                c.Rank,
                c.Extensions.Where(e => e.Code == Intermediate).OrderBy(e => e.Pos).Select(e => e.Value ?? string.Empty).ToArray()))
            .ToArray();

        return new HighJumpResult(heights, athletes);
    }
}
```

In `src/Sport.Disciplines.ATH/AthModule.cs`, add:
```csharp
    public IResultSchema ResultSchema { get; } = new AthResultSchema();
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.ATH.Tests --filter AthResultSchemaTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.ATH apps/api/tests/Sport.Disciplines.ATH.Tests
git commit -m "feat(results): ATH ranked schema with attempt projection"
```

---

## Task 8: BOX schema + projection (judges + decision, Official-only finish)

**Files:**
- Create: `src/Sport.Disciplines.BOX/BoxResultSchema.cs`, `BoxingBoutResult.cs`
- Modify: `src/Sport.Disciplines.BOX/BoxModule.cs`
- Test: `tests/Sport.Disciplines.BOX.Tests/BoxResultSchemaTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Disciplines.BOX.Tests;

public class BoxResultSchemaTests
{
    private static readonly EventTypeCode Cat = EventTypeCode.From("75KG");
    private static readonly Rsc UnitRsc = Rsc.From("BOXM75KG--------------FNL-000100--");

    private static UnitResultDocument Bout(BoxResultSchema schema)
    {
        var red = EntryId.New(); var blue = EntryId.New();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("BOX"), Cat);
        doc.ApplySnapshot(
            new[]
            {
                new CompetitorResult(red, 1)
                {
                    Wlt = Wlt.W, ResultValue = "WP 3:0", ResultType = ResultTypeCode.From("RM_POINTS"),
                    Extensions = new[] { new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("JUDGE")) { Pos = "J1", Value = "30" } },
                },
                new CompetitorResult(blue, 2) { Wlt = Wlt.L, ResultValue = "0:3", ResultType = ResultTypeCode.From("RM_POINTS") },
            },
            Array.Empty<ResultSegment>(),
            new[] { new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("RES_CODE")) { Value = "WP" } },
            schema);
        return doc;
    }

    [Fact]
    public void Status_set_omits_Unofficial()
    {
        var schema = new BoxResultSchema();
        schema.StatusesFor(Cat).Should().NotContain(ResultStatus.Unofficial);
        schema.StatusesFor(Cat).Should().Contain(ResultStatus.Official);
    }

    [Fact]
    public void Validates_a_two_row_bout()
    {
        var schema = new BoxResultSchema();
        Bout(schema).Competitors.Should().HaveCount(2);
    }

    [Fact]
    public void Projection_exposes_judge_totals_and_decision()
    {
        var schema = new BoxResultSchema();
        var proj = (BoxingBoutResult)schema.Project(Bout(schema));
        proj.JudgeTotals.Should().ContainSingle(j => j.Judge == "J1" && j.Value == "30");
        proj.Decision.Should().Be("WP");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.BOX.Tests --filter BoxResultSchemaTests`
Expected: FAIL — `BoxResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`BoxingBoutResult.cs`:
```csharp
using Sport.Core.DisciplineRegistry;

namespace Sport.Disciplines.BOX;

public sealed record BoxJudgeTotal(string Judge, string? Value);

public sealed record BoxingBoutResult(IReadOnlyList<BoxJudgeTotal> JudgeTotals, string? Decision) : DisciplineResultProjection;
```

`BoxResultSchema.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.BOX;

public sealed class BoxResultSchema : DefaultResultSchema
{
    // BOX moves START_LIST -> LIVE -> OFFICIAL (no UNOFFICIAL stage), plus PROVISIONAL.
    private static readonly IReadOnlySet<ResultStatus> BoxStatuses = new HashSet<ResultStatus>
    {
        ResultStatus.StartList, ResultStatus.Live, ResultStatus.Official, ResultStatus.Provisional,
    };

    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;
    public override IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type) => BoxStatuses;

    public override Result Validate(UnitResultDocument document)
    {
        foreach (var row in document.Competitors)
        {
            var scored = document.Status != ResultStatus.StartList && row.Irm is null;
            if (scored && row.Wlt is null)
                return Result.Fail("BOX is head-to-head: each scored competitor must carry a WLT.");
        }
        return Result.Ok();
    }

    public override DisciplineResultProjection Project(UnitResultDocument document)
    {
        var totals = document.Competitors
            .SelectMany(c => c.Extensions)
            .Where(e => e.Code == ExtensionCode.From("JUDGE"))
            .Select(e => new BoxJudgeTotal(e.Pos ?? string.Empty, e.Value))
            .ToArray();
        var decision = document.Extensions.FirstOrDefault(e => e.Code == ExtensionCode.From("RES_CODE"))?.Value;
        return new BoxingBoutResult(totals, decision);
    }
}
```

In `src/Sport.Disciplines.BOX/BoxModule.cs`, add:
```csharp
    public IResultSchema ResultSchema { get; } = new BoxResultSchema();
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.BOX.Tests --filter BoxResultSchemaTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.BOX apps/api/tests/Sport.Disciplines.BOX.Tests
git commit -m "feat(results): BOX schema with judge+decision projection and Official-only finish"
```

---

## Task 9: JUD schema + rollup + projection (team match)

**Rollup convention:** in each contest document the competitor with `SortOrder == 1` is the home side, `SortOrder == 2` away; each is an athlete whose `Composition` has one member. The parent's two rows are the teams (`SortOrder == 1` home team, `2` away). `AggregateSubunits` counts contest wins per side and emits one `TEAM/COMP` extension per contest whose children are **`WEIGHT_CATEGORY`** (copied from the contest's own extension), **`HOME`** and **`AWAY`** (the **athlete `PersonId`** of each side, matching ODF where HOME/AWAY are athlete IDs). It suggests `Official` only when every contest is `Official` **and** the tie is broken. **A genuine tie yields no fabricated winner** (`Wlt == null`, `SuggestedStatus == null`); golden score enters the model simply as an additional decisive contest that tips the count. ODF's further `TEAM/COMP` children `DURATION`/`GOLD_SCORE`/`STATUS` are deferred — `STATUS` is a `ScheduleStatus`, which is out of scope (spec §2).

**Files:**
- Create: `src/Sport.Disciplines.JUD/JudResultSchema.cs`, `JudoTeamMatchResult.cs`
- Modify: `src/Sport.Disciplines.JUD/JudModule.cs`
- Test: `tests/Sport.Disciplines.JUD.Tests/JudResultSchemaTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudResultSchemaTests
{
    private static readonly EventTypeCode Team6 = EventTypeCode.From("TEAM6");
    private static readonly DisciplineCode Jud = DisciplineCode.From("JUD");

    private static UnitResultDocument Contest(UnitId parentUnitId, int pos, Wlt homeOutcome)
    {
        var rsc = Rsc.From($"JUDXTEAM6-------------FNL-0001000{pos}");
        var doc = UnitResultDocument.CreateForSubunit(UnitResultId.New(), parentUnitId, SubunitId.New(), rsc, Jud, Team6);
        doc.ApplySnapshot(
            new[]
            {
                new CompetitorResult(EntryId.New(), 1) { Wlt = homeOutcome, Composition = new[] { new CompetitorMemberResult(PersonId.New(), 1) } },
                new CompetitorResult(EntryId.New(), 2) { Wlt = homeOutcome == Wlt.W ? Wlt.L : Wlt.W, Composition = new[] { new CompetitorMemberResult(PersonId.New(), 1) } },
            },
            Array.Empty<ResultSegment>(),
            new[] { new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("WEIGHT_CATEGORY")) { Value = $"JUDWCAT{pos}" } },
            new JudResultSchema());
        doc.TransitionTo(ResultStatus.Official, new JudResultSchema());
        return doc;
    }

    private static UnitResultDocument Parent(UnitId unitId, out EntryId home, out EntryId away)
    {
        home = EntryId.New(); away = EntryId.New();
        var parent = UnitResultDocument.CreateForUnit(UnitResultId.New(), unitId, Rsc.From("JUDXTEAM6-------------FNL-00010000"), Jud, Team6);
        parent.ApplySnapshot(
            new[] { new CompetitorResult(home, 1), new CompetitorResult(away, 2) },
            Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), new JudResultSchema());
        return parent;
    }

    [Fact]
    public void Irm_set_is_judo_specific()
    {
        var schema = new JudResultSchema();
        schema.IrmCodesFor(Team6).Should().Contain(Irm.From("DQB"));
        schema.IrmCodesFor(Team6).Should().NotContain(Irm.From("DNF"));
    }

    [Fact]
    public void Aggregate_counts_wins_and_emits_team_comp_with_children()
    {
        var schema = new JudResultSchema();
        var unitId = UnitId.New();
        var parent = Parent(unitId, out var home, out var away);
        var contests = new[]
        {
            Contest(unitId, 1, Wlt.W), Contest(unitId, 2, Wlt.W),
            Contest(unitId, 3, Wlt.L), Contest(unitId, 4, Wlt.W),
        };

        var rollup = schema.AggregateSubunits(parent, contests);

        rollup.Competitors.Single(c => c.EntryId == home).ResultValue.Should().Be("3");
        rollup.Competitors.Single(c => c.EntryId == home).Wlt.Should().Be(Wlt.W);
        rollup.Competitors.Single(c => c.EntryId == away).ResultValue.Should().Be("1");
        rollup.Extensions.Should().HaveCount(4);
        rollup.Extensions[0].Code.Should().Be(ExtensionCode.From("COMP"));
        rollup.Extensions[0].Children.Select(ch => ch.Code).Should()
            .Contain(new[] { ExtensionCode.From("WEIGHT_CATEGORY"), ExtensionCode.From("HOME"), ExtensionCode.From("AWAY") });
        rollup.SuggestedStatus.Should().Be(ResultStatus.Official);
    }

    [Fact]
    public void Aggregate_does_not_fabricate_a_tie_outcome()
    {
        var schema = new JudResultSchema();
        var unitId = UnitId.New();
        var parent = Parent(unitId, out var home, out _);
        var contests = new[]
        {
            Contest(unitId, 1, Wlt.W), Contest(unitId, 2, Wlt.W),
            Contest(unitId, 3, Wlt.L), Contest(unitId, 4, Wlt.L),
        };

        var rollup = schema.AggregateSubunits(parent, contests);

        rollup.Competitors.Single(c => c.EntryId == home).Wlt.Should().BeNull();
        rollup.SuggestedStatus.Should().BeNull();   // unresolved, awaiting golden-score contest
    }

    [Fact]
    public void Aggregate_rejects_a_contest_that_is_not_a_subunit_of_the_parent()
    {
        var schema = new JudResultSchema();
        var parent = Parent(UnitId.New(), out _, out _);
        var foreign = Contest(UnitId.New(), 1, Wlt.W);   // belongs to a different unit
        FluentActions.Invoking(() => schema.AggregateSubunits(parent, new[] { foreign }))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-2");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests --filter JudResultSchemaTests`
Expected: FAIL — `JudResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`JudoTeamMatchResult.cs`:
```csharp
using Sport.Core.DisciplineRegistry;

namespace Sport.Disciplines.JUD;

public sealed record JudoTeamMatchResult(int ContestCount) : DisciplineResultProjection;
```

`JudResultSchema.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

public sealed class JudResultSchema : DefaultResultSchema
{
    private static readonly ExtensionCode Comp = ExtensionCode.From("COMP");
    private static readonly ExtensionCode WeightCategory = ExtensionCode.From("WEIGHT_CATEGORY");
    private static readonly ExtensionType Team = ExtensionType.From("TEAM");
    private static readonly IReadOnlySet<Irm> JudIrms = new HashSet<Irm>
    {
        Irm.From("DNS"), Irm.From("DQB"), Irm.From("DSQ"), Irm.From("WDR"),
    };

    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;
    public override IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type) => JudIrms;

    public override DisciplineResultProjection Project(UnitResultDocument document) =>
        new JudoTeamMatchResult(document.Extensions.Count(e => e.Code == Comp));

    public override ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults)
    {
        // Structural guard (don't trust the caller): parent is unit-level, every contest is a subunit of THIS unit,
        // and each contest has two sides of one athlete.
        if (parent.TargetSubunitId is not null)
            throw new DomainException("I-RES-10", "AggregateSubunits requires a unit-level parent document.");
        foreach (var c in contestResults)
        {
            if (c.TargetSubunitId is null || c.TargetUnitId != parent.TargetUnitId)
                throw new DomainException("I-RES-2", "Each contest must be a subunit of the parent unit.");
            if (c.Competitors.Count != 2 || c.Competitors.Any(x => x.Composition.Count != 1))
                throw new DomainException("I-RES-2", "Each JUD contest must have two sides, each with one athlete.");
        }

        var homeTeam = parent.Competitors.Single(c => c.SortOrder == 1);
        var awayTeam = parent.Competitors.Single(c => c.SortOrder == 2);

        var homeWins = 0;
        var awayWins = 0;
        var extensions = new List<ResultExtension>();
        var pos = 1;

        foreach (var contest in contestResults)
        {
            var homeSide = contest.Competitors.Single(c => c.SortOrder == 1);
            var awaySide = contest.Competitors.Single(c => c.SortOrder == 2);
            if (homeSide.Wlt == Wlt.W) homeWins++;
            else if (awaySide.Wlt == Wlt.W) awayWins++;

            var children = new List<ResultExtension>();
            var weight = contest.Extensions.FirstOrDefault(e => e.Code == WeightCategory)?.Value;
            if (weight is not null)
                children.Add(new ResultExtension(Team, WeightCategory) { Value = weight });
            // ODF HOME/AWAY are athlete IDs: the contest side's single composition member.
            children.Add(new ResultExtension(Team, ExtensionCode.From("HOME")) { Value = homeSide.Composition.Single().PersonId.Value.ToString() });
            children.Add(new ResultExtension(Team, ExtensionCode.From("AWAY")) { Value = awaySide.Composition.Single().PersonId.Value.ToString() });

            extensions.Add(new ResultExtension(Team, Comp)
            {
                Pos = pos.ToString(),
                Value = contest.TargetRsc.Value,
                Children = children,
            });
            pos++;
        }

        // Decisive => winner; genuine tie => unresolved (golden score is just an extra decisive contest).
        (Wlt? homeWlt, Wlt? awayWlt) =
            homeWins > awayWins ? (Wlt.W, Wlt.L) :
            awayWins > homeWins ? (Wlt.L, Wlt.W) :
            ((Wlt?)null, (Wlt?)null);

        var resolved = homeWlt is not null;
        var allOfficial = contestResults.Count > 0 && contestResults.All(c => c.Status == ResultStatus.Official);
        ResultStatus? suggested = resolved && allOfficial ? ResultStatus.Official : null;

        var competitors = new[]
        {
            homeTeam with { ResultValue = homeWins.ToString(), Wlt = homeWlt, ResultType = ResultTypeCode.From("POINTS") },
            awayTeam with { ResultValue = awayWins.ToString(), Wlt = awayWlt, ResultType = ResultTypeCode.From("POINTS") },
        };

        return new ResultRollup(competitors, extensions, suggested);
    }
}
```

In `src/Sport.Disciplines.JUD/JudModule.cs`, add:
```csharp
    public IResultSchema ResultSchema { get; } = new JudResultSchema();
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests --filter JudResultSchemaTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.JUD apps/api/tests/Sport.Disciplines.JUD.Tests
git commit -m "feat(results): JUD team-match schema with subunit rollup (no fabricated tie)"
```

---

## Task 10: JUD end-to-end (parent + six contests sharing the parent unit id)

**Files:**
- Test: `tests/Sport.Disciplines.JUD.Tests/JudTeamMatchEndToEndTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudTeamMatchEndToEndTests
{
    private static readonly EventTypeCode Team6 = EventTypeCode.From("TEAM6");
    private static readonly DisciplineCode Jud = DisciplineCode.From("JUD");

    [Fact]
    public void Six_contests_decompose_into_a_stored_parent_aggregate_consumed_by_progression()
    {
        var schema = new JudResultSchema();
        var parentUnitId = UnitId.New();          // contests hang off the SAME unit id
        var homeTeam = EntryId.New(); var awayTeam = EntryId.New();

        var parent = UnitResultDocument.CreateForUnit(
            UnitResultId.New(), parentUnitId, Rsc.From("JUDXTEAM6-------------FNL-00010000"), Jud, Team6);
        parent.ApplySnapshot(
            new[] { new CompetitorResult(homeTeam, 1), new CompetitorResult(awayTeam, 2) },
            Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), schema);

        var outcomes = new[] { Wlt.W, Wlt.W, Wlt.W, Wlt.W, Wlt.L, Wlt.L };   // home 4, away 2
        var contests = new List<UnitResultDocument>();
        for (var i = 0; i < 6; i++)
        {
            var rsc = Rsc.From($"JUDXTEAM6-------------FNL-0001000{i + 1}");
            var c = UnitResultDocument.CreateForSubunit(UnitResultId.New(), parentUnitId, SubunitId.New(), rsc, Jud, Team6);
            c.ApplySnapshot(
                new[]
                {
                    new CompetitorResult(EntryId.New(), 1) { Wlt = outcomes[i], Composition = new[] { new CompetitorMemberResult(PersonId.New(), 1) } },
                    new CompetitorResult(EntryId.New(), 2) { Wlt = outcomes[i] == Wlt.W ? Wlt.L : Wlt.W, Composition = new[] { new CompetitorMemberResult(PersonId.New(), 1) } },
                },
                Array.Empty<ResultSegment>(),
                new[] { new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("WEIGHT_CATEGORY")) { Value = $"JUDWCAT{i + 1}" } },
                schema);
            c.TransitionTo(ResultStatus.Official, schema);
            contests.Add(c);
        }

        // Each contest is a real subunit of the parent unit.
        contests.Should().OnlyContain(c => c.TargetSubunitId != null && c.TargetUnitId == parentUnitId);

        parent.ApplyRollup(schema.AggregateSubunits(parent, contests), schema);

        parent.Status.Should().Be(ResultStatus.Official);
        parent.Competitors.Single(c => c.EntryId == homeTeam).ResultValue.Should().Be("4");
        parent.Competitors.Single(c => c.EntryId == awayTeam).ResultValue.Should().Be("2");
        parent.Extensions.Count(e => e.Code == ExtensionCode.From("COMP")).Should().Be(6);

        // What a progression layer reads: ONLY the parent outcome, never the contests.
        parent.Competitors.Single(c => c.Wlt == Wlt.W).EntryId.Should().Be(homeTeam);
    }
}
```

- [ ] **Step 2: Run test to verify it fails (or passes if Task 9 complete)**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests --filter JudTeamMatchEndToEndTests`
Expected: PASS once Task 9 is in place (this is a composition check; if it fails, fix Task 9 or Task 4, not here).

- [ ] **Step 3: (No new production code)**

- [ ] **Step 4: Re-run to confirm PASS**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests --filter JudTeamMatchEndToEndTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/tests/Sport.Disciplines.JUD.Tests
git commit -m "test(results): JUD team match decomposes into six subunits with stored aggregate"
```

---

## Task 11: Architecture test (Results-layer isolation)

**Files:**
- Test: `tests/Sport.Architecture.Tests/ResultsLayerArchitectureTests.cs`

- [ ] **Step 1: Write the failing test**

Inspect an existing test in `tests/Sport.Architecture.Tests/` first to copy the exact NetArchTest entry point / assembly helper used in this repo. Then:

```csharp
using FluentAssertions;
using NetArchTest.Rules;
using Sport.Core.Results;

namespace Sport.Architecture.Tests;

public class ResultsLayerArchitectureTests
{
    [Fact]
    public void Results_namespace_does_not_depend_on_any_discipline()
    {
        var result = Types.InAssembly(typeof(UnitResultDocument).Assembly)
            .That().ResideInNamespace("Sport.Core.Results")
            .ShouldNot().HaveDependencyOn("Sport.Disciplines")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run test**

Run: `dotnet test apps/api/tests/Sport.Architecture.Tests --filter ResultsLayerArchitectureTests`
Expected: PASS (the layer is discipline-free). If FAIL, remove the leaked `using Sport.Disciplines.*` from `src/Sport.Core/Results/`.

- [ ] **Step 3: (No production code unless the test fails)**

- [ ] **Step 4: Run the full suite**

Run: `dotnet test apps/api/Sport.slnx`
Expected: all tests PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/tests/Sport.Architecture.Tests
git commit -m "test(results): enforce Results-layer isolation from disciplines"
```

---

## Self-Review

**Spec coverage:**
- §3 model + §5 identity/`TargetSubunitId` → Tasks 1–4. ✓
- §6 structural placement, controlled ODF attributes, `SegmentScore` cumulative/segment, `SegmentCode` with `-` → Tasks 2–3. ✓
- §7 VOs incl. 9-code `ResultStatus`, schema-declared `Irm` → Tasks 1, 2, 4, 9. ✓
- §8 `IResultSchema` full surface + `ResultRollup` → Tasks 4, 6–9. ✓
- §9 flows (governed snapshot; JUD rollup stored-and-derived, parent owns its status) → Tasks 4, 9, 10. ✓
- §10 invariants: I-RES-2 (Task 9 — JUD structural guard rejects non-subunit/foreign contests), I-RES-4 incl. a **non-vacuous `CanTransition`** default exercised by a Task 4 illegal-transition test, I-RES-5 (Tasks 6–7), I-RES-6 (Task 4), I-RES-8 (Task 4), I-RES-10 (Tasks 4, 9). `CanTransition` is deliberately coarse-grained in the first slice (no self-transition / no rewind to StartList / Official terminal except on protest); finer per-discipline graphs are deferred. I-RES-1/3/7 are persistence-level (unique index / FK / structure) and are explicitly deferred with persistence in §2 — no task here, by design.
- §11 four-discipline sketch with the promised features → Tasks 6–10. ✓
  - FBL: per-period scores (cumulative **and** segment) projected, not just period codes.
  - ATH: heights + per-athlete attempt series.
  - BOX: judge totals **and** decision (`RES_CODE`); `StatusesFor` omits `Unofficial`.
  - JUD: `TEAM/COMP` children `WEIGHT_CATEGORY` + `HOME`/`AWAY` as athlete `PersonId`. Deferred (not a gap): `DURATION`/`GOLD_SCORE` and `STATUS` (a `ScheduleStatus`, out of scope per §2).
- §12 errors (`DomainException` I-RES-*, schema `Result`) → Tasks 4, 6–9. ✓
- §13 testing → Tasks 1–11. ✓

**Deferred by design (not gaps):** persistence/EF, API, play-by-play, standings/ranking/cumulative, full ODF vocabularies, `ScheduleStatus`, placeholder competitors — all out of scope in spec §2.

**Placeholder scan:** no TBD/TODO; every code step contains complete, compilable code.

**Type consistency:** all writes go through `ApplySnapshot`/`TransitionTo`/`ApplyRollup` (no raw setters); `IResultSchema` surface, `ResultRollup` shape, `CompetitorResult`/`ResultExtension` property names, the `SortOrder==1 home / 2 away` rollup convention, and projection record shapes are used identically across Tasks 4, 6–10.

> Note on RSC fixtures: every `Rsc.From("…")` literal must be **exactly 34 characters** (charset `A–Z 0–9 . -`). If a fixture throws a length error during execution, pad/trim the filler hyphens to 34 — `Rsc.From` validates length and charset only, not internal structure.

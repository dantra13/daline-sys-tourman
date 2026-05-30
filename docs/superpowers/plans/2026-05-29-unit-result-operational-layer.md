# Unit Result Operational Layer — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the conceptual domain model of the operational result layer (`Sport.Core.Results`) plus the discipline result-schema contract, validated end-to-end with FBL, ATH, BOX and JUD.

**Architecture:** A core aggregate `UnitResultDocument` carries the common envelope (status, per-competitor rows, segments, ODF-style extensions); each discipline contributes an `IResultSchema` (sibling of `IPhaseCatalog`) that validates the generic components, projects a typed read view, and — for subunit-hosting events — aggregates contest results into the parent (JUD team match). No persistence, API, play-by-play or standings in this plan.

**Tech Stack:** .NET 10, C#, Vogen (value objects), xUnit + FluentAssertions. New code lives in the existing `Sport.Core` and `Sport.Disciplines.*` projects; tests live in the existing `Sport.Core.Tests` and `Sport.Disciplines.*.Tests` projects. No new `.csproj`.

**Spec:** `docs/superpowers/specs/2026-05-29-unit-result-operational-layer-design.md`

---

## File Structure

All paths are relative to `apps/api/`.

**New — core model (`src/Sport.Core/Results/`):**
- `UnitResultId.cs` — Vogen Guid id.
- `ResultStatus.cs` — enum (9 ODF common codes).
- `Wlt.cs` — enum `{ W, L, T }`.
- `OutcomeMode.cs` — enum `{ HeadToHead, Ranked }`.
- `Irm.cs`, `ResultTypeCode.cs`, `SegmentCode.cs`, `ExtensionType.cs`, `ExtensionCode.cs` — Vogen string VOs.
- `ResultExtension.cs` — recursive value record (controlled ODF attributes + `Children`).
- `SegmentScore.cs` — value record (`CumulativeValue`/`SegmentValue`).
- `CompetitorMemberResult.cs` — value record (athlete-level facts).
- `ResultSegment.cs` — value record (period/round).
- `CompetitorResult.cs` — value record (per-competitor row).
- `UnitResultDocument.cs` — aggregate root.

**New — registry contract (`src/Sport.Core/DisciplineRegistry/`):**
- `DisciplineResultProjection.cs` — abstract base for typed read views.
- `ResultRollup.cs` — return type of `AggregateSubunits`.
- `IResultSchema.cs` — the per-discipline contract + `DefaultResultSchema`.
- `IDisciplineModule.cs` — *modify*: add `ResultSchema` with a permissive default.

**New — discipline schemas (in each `src/Sport.Disciplines.<D>/`):**
- `FblResultSchema.cs` + `FootballMatchResult.cs`
- `AthResultSchema.cs` + `HighJumpResult.cs`
- `BoxResultSchema.cs` + `BoxingBoutResult.cs`
- `JudResultSchema.cs` + `JudoTeamMatchResult.cs`

**New — tests:**
- `tests/Sport.Core.Tests/Results/*` — VOs, components, aggregate, JUD end-to-end.
- `tests/Sport.Disciplines.<D>.Tests/<D>ResultSchemaTests.cs` — per discipline.
- `tests/Sport.Architecture.Tests/*` — Results-layer isolation (extend existing).

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

`Sport.Core.DisciplineRegistry` already references `Sport.Core.Structure`/`Participants`; it will also reference `Sport.Core.Results` (same project, just a new folder — no project reference needed).

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
        var b = UnitResultId.New();
        a.Value.Should().NotBe(Guid.Empty);
        a.Should().NotBe(b);
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
Expected: FAIL — `Sport.Core.Results` namespace / types not found.

- [ ] **Step 3: Write the implementation**

`src/Sport.Core/Results/UnitResultId.cs`:
```csharp
using Vogen;

namespace Sport.Core.Results;

[ValueObject<Guid>]
public readonly partial struct UnitResultId
{
    public static UnitResultId New() => From(Guid.CreateVersion7());
}
```

`src/Sport.Core/Results/ResultStatus.cs`:
```csharp
namespace Sport.Core.Results;

public enum ResultStatus
{
    StartList,
    Live,
    Intermediate,
    Unconfirmed,
    Unofficial,
    Official,
    Partial,
    Protested,
    Provisional,
}
```

`src/Sport.Core/Results/Wlt.cs`:
```csharp
namespace Sport.Core.Results;

public enum Wlt { W, L, T }
```

`src/Sport.Core/Results/OutcomeMode.cs`:
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

## Task 2: String value objects

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
    [InlineData("DSQ")]
    [InlineData("WDR")]
    public void Irm_accepts_uppercase_codes(string code) =>
        Irm.From(code).Value.Should().Be(code);

    [Theory]
    [InlineData("")]
    [InlineData("dns")]
    [InlineData("TOO-LONG-IRM-CODE")]
    public void Irm_rejects_invalid_codes(string code) =>
        FluentActions.Invoking(() => Irm.From(code)).Should().Throw<ValueObjectValidationException>();

    [Fact]
    public void Vocab_codes_round_trip()
    {
        ResultTypeCode.From("POINTS").Value.Should().Be("POINTS");
        SegmentCode.From("H1").Value.Should().Be("H1");
        ExtensionType.From("ER").Value.Should().Be("ER");
        ExtensionCode.From("INTERMEDIATE").Value.Should().Be("INTERMEDIATE");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultStringVoTests`
Expected: FAIL — types not found.

- [ ] **Step 3: Write the implementation**

`src/Sport.Core/Results/Irm.cs`:
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

`src/Sport.Core/Results/ResultTypeCode.cs` (repeat the same shape, `MaxLength = 12`, allow `_`):
```csharp
using Vogen;

namespace Sport.Core.Results;

[ValueObject<string>]
public readonly partial struct ResultTypeCode
{
    public const int MaxLength = 12;

    private static Validation Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return Validation.Invalid("ResultTypeCode is required.");
        if (value.Length > MaxLength) return Validation.Invalid($"ResultTypeCode must be at most {MaxLength} characters.");
        foreach (var c in value)
        {
            var ok = c is >= 'A' and <= 'Z' || c is >= '0' and <= '9' || c == '_';
            if (!ok) return Validation.Invalid("ResultTypeCode chars must be uppercase alphanumeric or '_'.");
        }
        return Validation.Ok;
    }
}
```

`src/Sport.Core/Results/SegmentCode.cs` — identical body to `ResultTypeCode` with `MaxLength = 8` and the type/message renamed to `SegmentCode`.

`src/Sport.Core/Results/ExtensionType.cs` — identical body with `MaxLength = 8`, renamed `ExtensionType`.

`src/Sport.Core/Results/ExtensionCode.cs` — identical body with `MaxLength = 24`, renamed `ExtensionCode`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultStringVoTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Results apps/api/tests/Sport.Core.Tests/Results
git commit -m "feat(results): string vocabulary value objects"
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
    public void ResultExtension_carries_controlled_attributes_and_children()
    {
        var ext = new ResultExtension(ExtensionType.From("TEAM"), ExtensionCode.From("COMP"))
        {
            Pos = "1",
            Children = new[]
            {
                new ResultExtension(ExtensionType.From("TEAM"), ExtensionCode.From("WEIGHT_CATEGORY")) { Value = "JUDW57KG" },
            },
        };
        ext.Children.Should().HaveCount(1);
        ext.Children[0].Value.Should().Be("JUDW57KG");
    }

    [Fact]
    public void CompetitorResult_defaults_collections_to_empty()
    {
        var row = new CompetitorResult(EntryId.New(), SortOrder: 1) { Wlt = Wlt.W };
        row.Composition.Should().BeEmpty();
        row.Extensions.Should().BeEmpty();
        row.Wlt.Should().Be(Wlt.W);
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

Create the five records exactly as in the "Canonical signatures" block above:
`ResultExtension.cs`, `SegmentScore.cs`, `CompetitorMemberResult.cs`, `ResultSegment.cs`, `CompetitorResult.cs` (each `namespace Sport.Core.Results;`, with `using Sport.Core.Participants;` where `EntryId`/`PersonId`/`Bib` are referenced).

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ResultComponentsTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Results apps/api/tests/Sport.Core.Tests/Results
git commit -m "feat(results): leaf component records"
```

---

## Task 4: Registry contract (projection, rollup, IResultSchema + default)

**Files:**
- Create: `src/Sport.Core/DisciplineRegistry/DisciplineResultProjection.cs`, `ResultRollup.cs`, `IResultSchema.cs`
- Test: `tests/Sport.Core.Tests/Results/DefaultResultSchemaTests.cs`

`IResultSchema` references `UnitResultDocument` (built in Task 5). To keep tasks independently compilable, **Task 5 is implemented before this task's test is run** — implement `UnitResultDocument` (Task 5 Step 3) first if you hit a missing-type error, or reorder locally. The contract code itself only needs the type to exist.

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Results;

public class DefaultResultSchemaTests
{
    private static readonly EventTypeCode AnyType = EventTypeCode.From("ANY");

    [Fact]
    public void Default_schema_is_head_to_head_with_full_status_set()
    {
        var schema = new DefaultResultSchema();
        schema.OutcomeModeFor(AnyType).Should().Be(OutcomeMode.HeadToHead);
        schema.StatusesFor(AnyType).Should().Contain(ResultStatus.Official);
        schema.CanTransition(ResultStatus.StartList, ResultStatus.Live, AnyType).Should().BeTrue();
    }

    [Fact]
    public void Default_schema_aggregation_throws_when_not_supported()
    {
        var schema = new DefaultResultSchema();
        FluentActions
            .Invoking(() => schema.AggregateSubunits(null!, Array.Empty<UnitResultDocument>()))
            .Should().Throw<NotSupportedException>();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter DefaultResultSchemaTests`
Expected: FAIL — `DefaultResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`src/Sport.Core/DisciplineRegistry/DisciplineResultProjection.cs`:
```csharp
namespace Sport.Core.DisciplineRegistry;

public abstract record DisciplineResultProjection;
```

`src/Sport.Core/DisciplineRegistry/ResultRollup.cs`:
```csharp
using Sport.Core.Results;

namespace Sport.Core.DisciplineRegistry;

public sealed record ResultRollup(
    IReadOnlyList<CompetitorResult> Competitors,
    IReadOnlyList<ResultExtension> Extensions,
    ResultStatus? SuggestedStatus);
```

`src/Sport.Core/DisciplineRegistry/IResultSchema.cs`:
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

public sealed record DefaultResultProjection : DisciplineResultProjection;

// Permissive default used by disciplines without a real grammar yet.
public class DefaultResultSchema : IResultSchema
{
    private static readonly IReadOnlySet<ResultStatus> AllStatuses =
        new HashSet<ResultStatus>(Enum.GetValues<ResultStatus>());

    private static readonly IReadOnlySet<Irm> CoreIrms =
        new HashSet<Irm> { Irm.From("DNS"), Irm.From("DNF"), Irm.From("DSQ"), Irm.From("WDR") };

    public virtual OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;
    public virtual IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type) => AllStatuses;
    public virtual bool CanTransition(ResultStatus from, ResultStatus to, EventTypeCode type) => true;
    public virtual IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type) => CoreIrms;
    public virtual Result Validate(UnitResultDocument document) => Result.Ok();
    public virtual DisciplineResultProjection Project(UnitResultDocument document) => new DefaultResultProjection();

    public virtual ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults) =>
        throw new NotSupportedException("This discipline does not host subunits.");
}
```

- [ ] **Step 4: Run test to verify it passes** (after Task 5 Step 3 exists)

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter DefaultResultSchemaTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/DisciplineRegistry apps/api/tests/Sport.Core.Tests/Results
git commit -m "feat(results): discipline result-schema contract and permissive default"
```

---

## Task 5: `UnitResultDocument` aggregate

**Files:**
- Create: `src/Sport.Core/Results/UnitResultDocument.cs`
- Test: `tests/Sport.Core.Tests/Results/UnitResultDocumentTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.Results;

public class UnitResultDocumentTests
{
    private static readonly DisciplineCode Disc = DisciplineCode.From("FBL");
    private static readonly EventTypeCode Type = EventTypeCode.From("TEAM11");
    private static readonly Rsc UnitRsc = Rsc.From("FBLMTEAM11------------FNL-000100--");

    private static UnitResultDocument NewUnitDoc() => UnitResultDocument.CreateForUnit(
        UnitResultId.New(), UnitId.New(), UnitRsc, Disc, Type);

    [Fact]
    public void CreateForUnit_starts_in_StartList_with_no_subunit_and_version_1()
    {
        var doc = NewUnitDoc();
        doc.Status.Should().Be(ResultStatus.StartList);
        doc.TargetSubunitId.Should().BeNull();
        doc.Version.Should().Be(1);
    }

    [Fact]
    public void TransitionTo_advances_status_and_bumps_version_when_schema_allows()
    {
        var doc = NewUnitDoc();
        doc.TransitionTo(ResultStatus.Live, new DefaultResultSchema());
        doc.Status.Should().Be(ResultStatus.Live);
        doc.Version.Should().Be(2);
    }

    [Fact]
    public void TransitionTo_is_rejected_when_status_outside_schema_set()
    {
        var schema = new FixedSchema(new HashSet<ResultStatus> { ResultStatus.StartList, ResultStatus.Live });
        var doc = NewUnitDoc();
        FluentActions.Invoking(() => doc.TransitionTo(ResultStatus.Official, schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-4");
    }

    [Fact]
    public void SetCompetitors_replaces_rows()
    {
        var doc = NewUnitDoc();
        doc.SetCompetitors(new[] { new CompetitorResult(EntryId.New(), 1) { Wlt = Wlt.W } });
        doc.Competitors.Should().HaveCount(1);
    }

    private sealed class FixedSchema(IReadOnlySet<ResultStatus> statuses) : DefaultResultSchema
    {
        public override IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type) => statuses;
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter UnitResultDocumentTests`
Expected: FAIL — `UnitResultDocument` not found.

- [ ] **Step 3: Write the implementation**

`src/Sport.Core/Results/UnitResultDocument.cs`:
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

    public void SetCompetitors(IReadOnlyList<CompetitorResult> competitors)
    {
        _competitors.Clear();
        _competitors.AddRange(competitors);
        Version++;
    }

    public void SetSegments(IReadOnlyList<ResultSegment> segments)
    {
        _segments.Clear();
        _segments.AddRange(segments);
        Version++;
    }

    public void SetExtensions(IReadOnlyList<ResultExtension> extensions)
    {
        _extensions.Clear();
        _extensions.AddRange(extensions);
        Version++;
    }

    public void TransitionTo(ResultStatus to, IResultSchema schema)
    {
        if (!schema.StatusesFor(EventType).Contains(to))
            throw new DomainException("I-RES-4", $"ResultStatus '{to}' is not used by this discipline/event.");
        if (!schema.CanTransition(Status, to, EventType))
            throw new DomainException("I-RES-4", $"Illegal ResultStatus transition '{Status}' -> '{to}'.");
        Status = to;
        Version++;
    }

    public void ApplyRollup(ResultRollup rollup, IResultSchema schema)
    {
        if (TargetSubunitId is not null)
            throw new DomainException("I-RES-10", "Rollup can only be applied to a parent (unit-level) document.");
        SetCompetitors(rollup.Competitors);
        SetExtensions(rollup.Extensions);
        if (rollup.SuggestedStatus is { } status && status != Status)
            TransitionTo(status, schema);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter UnitResultDocumentTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Results apps/api/tests/Sport.Core.Tests/Results
git commit -m "feat(results): UnitResultDocument aggregate with governed status machine"
```

---

## Task 6: Wire `ResultSchema` into `IDisciplineModule` (permissive default)

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

    // Minimal module implementing only the required members, relying on default ResultSchema.
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
    // Operational-result grammar. Disciplines override with a typed schema; the default is permissive.
    IResultSchema ResultSchema => SharedDefaultResultSchema;

    private static readonly IResultSchema SharedDefaultResultSchema = new DefaultResultSchema();
```

(Default interface members can reference a `private static readonly` field in C# 11+/.NET 10. Keep `using Sport.Core.Results;` if needed — `DefaultResultSchema` lives in the same `DisciplineRegistry` namespace, so no extra using is required.)

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Core.Tests --filter ModuleResultSchemaDefaultTests`
Expected: PASS. Also run the full discipline test suite to confirm all 7 modules still compile/satisfy the interface:
Run: `dotnet build apps/api/Sport.slnx`
Expected: build succeeds; BKB/BDM/VBV/FBL/ATH/BOX/JUD modules inherit the default schema.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/DisciplineRegistry apps/api/tests/Sport.Core.Tests/DisciplineRegistry
git commit -m "feat(results): expose ResultSchema on IDisciplineModule with permissive default"
```

---

## Task 7: FBL result schema + projection (head-to-head + periods)

**Files:**
- Create: `src/Sport.Disciplines.FBL/FblResultSchema.cs`, `src/Sport.Disciplines.FBL/FootballMatchResult.cs`
- Modify: `src/Sport.Disciplines.FBL/FblModule.cs` (override `ResultSchema`)
- Test: `tests/Sport.Disciplines.FBL.Tests/FblResultSchemaTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Disciplines.FBL.Tests;

public class FblResultSchemaTests
{
    private static readonly EventTypeCode Team11 = EventTypeCode.From("TEAM11");
    private static readonly Rsc UnitRsc = Rsc.From("FBLMTEAM11------------FNL-000100--");

    private static UnitResultDocument Match(out EntryId home, out EntryId away)
    {
        home = EntryId.New(); away = EntryId.New();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("FBL"), Team11);
        doc.SetCompetitors(new[]
        {
            new CompetitorResult(home, 1) { Wlt = Wlt.W, ResultValue = "2", ResultType = ResultTypeCode.From("POINTS") },
            new CompetitorResult(away, 2) { Wlt = Wlt.L, ResultValue = "1", ResultType = ResultTypeCode.From("POINTS") },
        });
        return doc;
    }

    [Fact]
    public void Schema_is_head_to_head_and_validates_a_two_row_match()
    {
        var schema = new FblResultSchema();
        var doc = Match(out _, out _);
        schema.OutcomeModeFor(Team11).Should().Be(OutcomeMode.HeadToHead);
        schema.Validate(doc).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Schema_rejects_ranked_outcome_without_wlt()
    {
        var schema = new FblResultSchema();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("FBL"), Team11);
        doc.SetCompetitors(new[] { new CompetitorResult(EntryId.New(), 1) { Rank = 1 } });
        schema.Validate(doc).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Projection_exposes_team_scores()
    {
        var schema = new FblResultSchema();
        var doc = Match(out var home, out _);
        var proj = (FootballMatchResult)schema.Project(doc);
        proj.TeamScores.Should().ContainKey(home).WhoseValue.Should().Be("2");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.FBL.Tests --filter FblResultSchemaTests`
Expected: FAIL — `FblResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`src/Sport.Disciplines.FBL/FootballMatchResult.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Disciplines.FBL;

public sealed record FootballMatchResult(IReadOnlyDictionary<EntryId, string> TeamScores) : DisciplineResultProjection;
```

`src/Sport.Disciplines.FBL/FblResultSchema.cs`:
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
            var live = document.Status != ResultStatus.StartList && row.Irm is null;
            if (live && row.Wlt is null)
                return Result.Fail("FBL is head-to-head: each scored competitor must carry a WLT.");
            if (row.Rank is not null)
                return Result.Fail("FBL does not use Rank; use WLT.");
        }
        return Result.Ok();
    }

    public override DisciplineResultProjection Project(UnitResultDocument document)
    {
        var scores = document.Competitors
            .Where(c => c.ResultValue is not null)
            .ToDictionary(c => c.EntryId, c => c.ResultValue!);
        return new FootballMatchResult(scores);
    }
}
```

In `src/Sport.Disciplines.FBL/FblModule.cs`, add the property (next to the other `IDisciplineModule` members):
```csharp
    public IResultSchema ResultSchema { get; } = new FblResultSchema();
```
(Add `using Sport.Core.DisciplineRegistry;` if not already imported.)

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.FBL.Tests --filter FblResultSchemaTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.FBL apps/api/tests/Sport.Disciplines.FBL.Tests
git commit -m "feat(results): FBL head-to-head result schema and projection"
```

---

## Task 8: ATH result schema + projection (ranked + attempts)

**Files:**
- Create: `src/Sport.Disciplines.ATH/AthResultSchema.cs`, `src/Sport.Disciplines.ATH/HighJumpResult.cs`
- Modify: `src/Sport.Disciplines.ATH/AthModule.cs`
- Test: `tests/Sport.Disciplines.ATH.Tests/AthResultSchemaTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH.Tests;

public class AthResultSchemaTests
{
    private static readonly EventTypeCode Hj = EventTypeCode.From("HJ");
    private static readonly Rsc UnitRsc = Rsc.From("ATHWHJ----------------FNL-000100--");

    private static UnitResultDocument HighJump()
    {
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("ATH"), Hj);
        doc.SetExtensions(new[]
        {
            new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("INTERMEDIATE")) { Pos = "1", Value = "1.88" },
            new ResultExtension(ExtensionType.From("UI"), ExtensionCode.From("INTERMEDIATE")) { Pos = "2", Value = "1.92" },
        });
        doc.SetCompetitors(new[]
        {
            new CompetitorResult(EntryId.New(), 1)
            {
                Rank = 1, SortOrder = 1, ResultValue = "1.92", ResultType = ResultTypeCode.From("DISTANCE"),
                Extensions = new[]
                {
                    new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("INTERMEDIATE")) { Pos = "1", Value = "o" },
                    new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("INTERMEDIATE")) { Pos = "2", Value = "xo" },
                },
            },
        });
        return doc;
    }

    [Fact]
    public void Schema_is_ranked_and_validates_without_wlt()
    {
        var schema = new AthResultSchema();
        schema.OutcomeModeFor(Hj).Should().Be(OutcomeMode.Ranked);
        schema.Validate(HighJump()).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Schema_rejects_row_without_sort_order()
    {
        var schema = new AthResultSchema();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("ATH"), Hj);
        // SortOrder defaults required; build a row missing it by using 0 sentinel rejected by schema.
        doc.SetCompetitors(new[] { new CompetitorResult(EntryId.New(), 0) { Wlt = Wlt.W } });
        schema.Validate(doc).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Projection_exposes_heights_and_attempt_series()
    {
        var schema = new AthResultSchema();
        var proj = (HighJumpResult)schema.Project(HighJump());
        proj.Heights.Should().BeEquivalentTo(new[] { "1.88", "1.92" });
        proj.Athletes.Should().HaveCount(1);
        proj.Athletes[0].BestMark.Should().Be("1.92");
        proj.Athletes[0].Attempts.Should().BeEquivalentTo(new[] { "o", "xo" });
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.ATH.Tests --filter AthResultSchemaTests`
Expected: FAIL — `AthResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`src/Sport.Disciplines.ATH/HighJumpResult.cs`:
```csharp
using Sport.Core.DisciplineRegistry;

namespace Sport.Disciplines.ATH;

public sealed record HighJumpAthleteResult(string? BestMark, int? Rank, IReadOnlyList<string> Attempts);

public sealed record HighJumpResult(
    IReadOnlyList<string> Heights,
    IReadOnlyList<HighJumpAthleteResult> Athletes) : DisciplineResultProjection;
```

`src/Sport.Disciplines.ATH/AthResultSchema.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.ATH;

public sealed class AthResultSchema : DefaultResultSchema
{
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
            .Where(e => e.Code == ExtensionCode.From("INTERMEDIATE"))
            .OrderBy(e => e.Pos)
            .Select(e => e.Value ?? string.Empty)
            .ToArray();

        var athletes = document.Competitors
            .Select(c => new HighJumpAthleteResult(
                c.ResultValue,
                c.Rank,
                c.Extensions
                    .Where(e => e.Code == ExtensionCode.From("INTERMEDIATE"))
                    .OrderBy(e => e.Pos)
                    .Select(e => e.Value ?? string.Empty)
                    .ToArray()))
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
git commit -m "feat(results): ATH ranked result schema with attempt projection"
```

---

## Task 9: BOX result schema + projection (judge scores, decision, no Unofficial)

**Files:**
- Create: `src/Sport.Disciplines.BOX/BoxResultSchema.cs`, `src/Sport.Disciplines.BOX/BoxingBoutResult.cs`
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

    private static UnitResultDocument Bout()
    {
        var red = EntryId.New(); var blue = EntryId.New();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("BOX"), Cat);
        doc.SetCompetitors(new[]
        {
            new CompetitorResult(red, 1)
            {
                Wlt = Wlt.W, ResultValue = "WP 3:0", ResultType = ResultTypeCode.From("RM_POINTS"),
                Extensions = new[] { new ResultExtension(ExtensionType.From("ER"), ExtensionCode.From("JUDGE")) { Pos = "J1", Value = "30" } },
            },
            new CompetitorResult(blue, 2) { Wlt = Wlt.L, ResultValue = "0:3", ResultType = ResultTypeCode.From("RM_POINTS") },
        });
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
        new BoxResultSchema().Validate(Bout()).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Projection_exposes_judge_totals()
    {
        var proj = (BoxingBoutResult)new BoxResultSchema().Project(Bout());
        proj.JudgeTotals.Should().ContainSingle(j => j.Judge == "J1" && j.Value == "30");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.BOX.Tests --filter BoxResultSchemaTests`
Expected: FAIL — `BoxResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`src/Sport.Disciplines.BOX/BoxingBoutResult.cs`:
```csharp
using Sport.Core.DisciplineRegistry;

namespace Sport.Disciplines.BOX;

public sealed record BoxJudgeTotal(string Judge, string? Value);

public sealed record BoxingBoutResult(IReadOnlyList<BoxJudgeTotal> JudgeTotals) : DisciplineResultProjection;
```

`src/Sport.Disciplines.BOX/BoxResultSchema.cs`:
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
            var live = document.Status != ResultStatus.StartList && row.Irm is null;
            if (live && row.Wlt is null)
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
        return new BoxingBoutResult(totals);
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
git commit -m "feat(results): BOX result schema with judge projection and Official-only finish"
```

---

## Task 10: JUD result schema + rollup + projection (team match)

**Files:**
- Create: `src/Sport.Disciplines.JUD/JudResultSchema.cs`, `src/Sport.Disciplines.JUD/JudoTeamMatchResult.cs`
- Modify: `src/Sport.Disciplines.JUD/JudModule.cs`
- Test: `tests/Sport.Disciplines.JUD.Tests/JudResultSchemaTests.cs`

**Rollup convention (decided in the spec):** in each contest document, the competitor with `SortOrder == 1` is the home side, `SortOrder == 2` is the away side. The parent's competitor rows are the two teams: `SortOrder == 1` home team, `SortOrder == 2` away team. `AggregateSubunits` counts contest wins per side, sets the parent rows' `ResultValue`/`Wlt`, emits one `TEAM/COMP` extension per contest, and suggests `Official` once every contest is `Official`.

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD.Tests;

public class JudResultSchemaTests
{
    private static readonly EventTypeCode Team6 = EventTypeCode.From("TEAM6");

    private static UnitResultDocument Contest(int pos, Wlt homeOutcome, ResultStatus status)
    {
        var rsc = Rsc.From($"JUDXTEAM6-------------FNL-0001000{pos}");
        var doc = UnitResultDocument.CreateForSubunit(
            UnitResultId.New(), UnitId.New(), SubunitId.New(), rsc, DisciplineCode.From("JUD"), Team6);
        doc.SetCompetitors(new[]
        {
            new CompetitorResult(EntryId.New(), 1) { Wlt = homeOutcome },
            new CompetitorResult(EntryId.New(), 2) { Wlt = homeOutcome == Wlt.W ? Wlt.L : Wlt.W },
        });
        if (status != ResultStatus.StartList) doc.TransitionTo(status, new JudResultSchema());
        return doc;
    }

    [Fact]
    public void Aggregate_counts_contest_wins_into_parent_score()
    {
        var schema = new JudResultSchema();
        var homeTeam = EntryId.New(); var awayTeam = EntryId.New();
        var parentRsc = Rsc.From("JUDXTEAM6-------------FNL-00010000");
        var parent = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), parentRsc, DisciplineCode.From("JUD"), Team6);
        parent.SetCompetitors(new[]
        {
            new CompetitorResult(homeTeam, 1),
            new CompetitorResult(awayTeam, 2),
        });

        var contests = new[]
        {
            Contest(1, Wlt.W, ResultStatus.Official),
            Contest(2, Wlt.W, ResultStatus.Official),
            Contest(3, Wlt.L, ResultStatus.Official),
            Contest(4, Wlt.W, ResultStatus.Official),
        };

        var rollup = schema.AggregateSubunits(parent, contests);

        rollup.Competitors.Single(c => c.EntryId == homeTeam).ResultValue.Should().Be("3");
        rollup.Competitors.Single(c => c.EntryId == homeTeam).Wlt.Should().Be(Wlt.W);
        rollup.Competitors.Single(c => c.EntryId == awayTeam).ResultValue.Should().Be("1");
        rollup.Extensions.Should().HaveCount(4);
        rollup.SuggestedStatus.Should().Be(ResultStatus.Official);
    }

    [Fact]
    public void Projection_exposes_contest_refs()
    {
        var schema = new JudResultSchema();
        var parentRsc = Rsc.From("JUDXTEAM6-------------FNL-00010000");
        var parent = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), parentRsc, DisciplineCode.From("JUD"), Team6);
        parent.SetCompetitors(new[] { new CompetitorResult(EntryId.New(), 1), new CompetitorResult(EntryId.New(), 2) });
        var rollup = schema.AggregateSubunits(parent, new[] { Contest(1, Wlt.W, ResultStatus.Official) });
        parent.ApplyRollup(rollup, schema);

        var proj = (JudoTeamMatchResult)schema.Project(parent);
        proj.ContestCount.Should().Be(1);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests --filter JudResultSchemaTests`
Expected: FAIL — `JudResultSchema` not found.

- [ ] **Step 3: Write the implementation**

`src/Sport.Disciplines.JUD/JudoTeamMatchResult.cs`:
```csharp
using Sport.Core.DisciplineRegistry;

namespace Sport.Disciplines.JUD;

public sealed record JudoTeamMatchResult(int ContestCount) : DisciplineResultProjection;
```

`src/Sport.Disciplines.JUD/JudResultSchema.cs`:
```csharp
using Sport.Core.DisciplineRegistry;
using Sport.Core.Results;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Disciplines.JUD;

public sealed class JudResultSchema : DefaultResultSchema
{
    public override OutcomeMode OutcomeModeFor(EventTypeCode type) => OutcomeMode.HeadToHead;

    public override DisciplineResultProjection Project(UnitResultDocument document)
    {
        var contests = document.Extensions.Count(e => e.Code == ExtensionCode.From("COMP"));
        return new JudoTeamMatchResult(contests);
    }

    public override ResultRollup AggregateSubunits(UnitResultDocument parent, IReadOnlyList<UnitResultDocument> contestResults)
    {
        var homeTeam = parent.Competitors.Single(c => c.SortOrder == 1);
        var awayTeam = parent.Competitors.Single(c => c.SortOrder == 2);

        var homeWins = 0;
        var awayWins = 0;
        var extensions = new List<ResultExtension>();
        var pos = 1;

        foreach (var contest in contestResults)
        {
            var homeSide = contest.Competitors.Single(c => c.SortOrder == 1);
            if (homeSide.Wlt == Wlt.W) homeWins++;
            else if (homeSide.Wlt == Wlt.L) awayWins++;

            extensions.Add(new ResultExtension(ExtensionType.From("TEAM"), ExtensionCode.From("COMP"))
            {
                Pos = pos.ToString(),
                Value = contest.TargetRsc.Value,
            });
            pos++;
        }

        var (homeWlt, awayWlt) =
            homeWins > awayWins ? (Wlt.W, Wlt.L) :
            awayWins > homeWins ? (Wlt.L, Wlt.W) :
            (Wlt.T, Wlt.T);

        var competitors = new[]
        {
            homeTeam with { ResultValue = homeWins.ToString(), Wlt = homeWlt, ResultType = ResultTypeCode.From("POINTS") },
            awayTeam with { ResultValue = awayWins.ToString(), Wlt = awayWlt, ResultType = ResultTypeCode.From("POINTS") },
        };

        var allOfficial = contestResults.All(c => c.Status == ResultStatus.Official);
        ResultStatus? suggested = allOfficial ? ResultStatus.Official : null;

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
git commit -m "feat(results): JUD team-match result schema with subunit rollup"
```

---

## Task 11: JUD end-to-end (parent + six contests → stored aggregate)

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

    [Fact]
    public void Six_contests_decompose_into_a_stored_parent_aggregate_consumed_by_progression()
    {
        var schema = new JudResultSchema();
        var homeTeam = EntryId.New(); var awayTeam = EntryId.New();
        var parentRsc = Rsc.From("JUDXTEAM6-------------FNL-00010000");
        var parent = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), parentRsc, DisciplineCode.From("JUD"), Team6);
        parent.SetCompetitors(new[] { new CompetitorResult(homeTeam, 1), new CompetitorResult(awayTeam, 2) });

        // Six contests: home wins 4, away wins 2.
        var outcomes = new[] { Wlt.W, Wlt.W, Wlt.W, Wlt.W, Wlt.L, Wlt.L };
        var contests = new System.Collections.Generic.List<UnitResultDocument>();
        for (var i = 0; i < 6; i++)
        {
            var rsc = Rsc.From($"JUDXTEAM6-------------FNL-0001000{i + 1}");
            var c = UnitResultDocument.CreateForSubunit(UnitResultId.New(), UnitId.New(), SubunitId.New(), rsc, DisciplineCode.From("JUD"), Team6);
            c.SetCompetitors(new[]
            {
                new CompetitorResult(EntryId.New(), 1) { Wlt = outcomes[i] },
                new CompetitorResult(EntryId.New(), 2) { Wlt = outcomes[i] == Wlt.W ? Wlt.L : Wlt.W },
            });
            c.TransitionTo(ResultStatus.Official, schema);
            contests.Add(c);
        }

        var rollup = schema.AggregateSubunits(parent, contests);
        parent.ApplyRollup(rollup, schema);

        // The parent now carries the operational aggregate.
        parent.Status.Should().Be(ResultStatus.Official);
        parent.Competitors.Single(c => c.EntryId == homeTeam).ResultValue.Should().Be("4");
        parent.Competitors.Single(c => c.EntryId == awayTeam).ResultValue.Should().Be("2");
        parent.Extensions.Count(e => e.Code == ExtensionCode.From("COMP")).Should().Be(6);

        // What a progression layer would read: ONLY the parent outcome, never the contests.
        var winner = parent.Competitors.Single(c => c.Wlt == Wlt.W);
        winner.EntryId.Should().Be(homeTeam);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests --filter JudTeamMatchEndToEndTests`
Expected: FAIL initially only if Task 10 incomplete; otherwise it exercises the assembled behavior. If Task 10 is done, run to confirm it PASSES directly (this is an integration check, no new production code).

- [ ] **Step 3: (No new production code)**

This task asserts the composed behavior of Tasks 5 and 10. If it fails, fix the relevant task rather than adding code here.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.JUD.Tests --filter JudTeamMatchEndToEndTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add apps/api/tests/Sport.Disciplines.JUD.Tests
git commit -m "test(results): JUD team match decomposes into six contests with stored aggregate"
```

---

## Task 12: Architecture tests (Results-layer isolation)

**Files:**
- Test: `tests/Sport.Architecture.Tests/ResultsLayerArchitectureTests.cs`

- [ ] **Step 1: Write the failing test**

Inspect an existing test in `tests/Sport.Architecture.Tests/` first to copy the exact NetArchTest entry point and the assembly-loading helper used in this repo (the helper that returns the `Sport.Core` assembly). Then:

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

- [ ] **Step 2: Run test to verify it fails or passes**

Run: `dotnet test apps/api/tests/Sport.Architecture.Tests --filter ResultsLayerArchitectureTests`
Expected: PASS (the layer should already be discipline-free). If it FAILS, a discipline reference leaked into `Sport.Core.Results` — remove it.

- [ ] **Step 3: (No production code unless the test fails)**

If the test fails, find and remove the offending `using Sport.Disciplines.*` from the Results folder.

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
- §3 model (`UnitResultDocument`, components) → Tasks 1–5. ✓
- §5 identity / `TargetSubunitId` → Task 5 factories. ✓
- §6 structural placement, controlled ODF attributes, `SegmentScore` cumulative/segment → Tasks 2–3. ✓
- §7 VOs incl. 9-code `ResultStatus`, schema-declared `Irm` → Tasks 1, 2, 4. ✓
- §8 `IResultSchema` (`OutcomeModeFor`/`StatusesFor`/`CanTransition`/`IrmCodesFor`/`Validate`/`Project`/`AggregateSubunits` → `ResultRollup`) → Tasks 4, 7–10. ✓
- §9 flows (atomic, JUD rollup stored-and-derived) → Tasks 5, 10, 11. ✓
- §10 invariants: I-RES-4 (Task 5), I-RES-5 (Tasks 7–8), I-RES-10 (Tasks 5, 10). I-RES-1/3/7 are persistence-level (unique index / FK / structure) and are explicitly deferred with persistence in §2 — **no task here**, by design.
- §11 four-discipline sketch (FBL/ATH/BOX/JUD) → Tasks 7–11. ✓
- §12 errors (`DomainException` I-RES-*, schema `Result`) → Tasks 5, 7–10. ✓
- §13 testing → Tasks 1–12. ✓

**Deferred by design (not gaps):** persistence/EF mapping, API, play-by-play, standings/ranking/cumulative, full ODF vocabularies, `ScheduleStatus`, placeholder competitors — all listed out of scope in spec §2.

**Placeholder scan:** no TBD/TODO; every code step contains complete, compilable code.

**Type consistency:** `IResultSchema` method names, `ResultRollup` shape, `CompetitorResult`/`ResultExtension` property names, and the `SortOrder==1 home / SortOrder==2 away` rollup convention are used identically across Tasks 4, 5, 7–11.

> Note on RSC fixtures: every `Rsc.From("…")` literal must be **exactly 34 characters** (charset `A–Z 0–9 . -`). If a fixture throws a length validation error during execution, pad/trim the filler hyphens to 34 — the internal structure is not re-validated by `Rsc.From`, only length and charset.

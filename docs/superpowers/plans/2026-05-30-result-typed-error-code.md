# Typed error code in `Result` — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let `IResultSchema.Validate` failures carry their own error code so `UnitResultDocument` propagates the precise code (outcome → `I-RES-5`) instead of collapsing every schema failure into `I-RES-8`.

**Architecture:** Add an optional `Code` to the `Result` value object (additive — `Error` stays a `string`, `Fail(string)` keeps its signature). The three real schemas (FBL/BOX/ATH) emit `I-RES-5` on their outcome violations; `UnitResultDocument.ApplySnapshot` propagates `validation.Code ?? "I-RES-8"`. `I-RES-8` remains the fallback for an uncoded schema failure.

**Tech Stack:** .NET (C#), xUnit + FluentAssertions. Solution: `apps/api/Sport.slnx`.

**Spec:** `docs/superpowers/specs/2026-05-30-result-typed-error-code-design.md`

---

## File Structure

| File | Responsibility | Change |
|---|---|---|
| `apps/api/src/Sport.Core/Shared/Result.cs` | The result value object | Add `string? Code`; add `Fail(code, error)` overload |
| `apps/api/src/Sport.Core/Results/UnitResultDocument.cs` | Result aggregate | Propagate `validation.Code ?? "I-RES-8"` |
| `apps/api/src/Sport.Disciplines.FBL/FblResultSchema.cs` | FBL grammar | Emit `I-RES-5` |
| `apps/api/src/Sport.Disciplines.BOX/BoxResultSchema.cs` | BOX grammar | Emit `I-RES-5` |
| `apps/api/src/Sport.Disciplines.ATH/AthResultSchema.cs` | ATH grammar | Emit `I-RES-5` |
| `apps/api/tests/Sport.Core.Tests/Shared/ResultTests.cs` | `Result` unit tests | Create |
| `apps/api/tests/Sport.Core.Tests/Results/UnitResultDocumentTests.cs` | Aggregate tests | Add coded-propagation test |
| `apps/api/tests/Sport.Disciplines.FBL.Tests/FblResultSchemaTests.cs` | FBL tests | Flip assertion to `I-RES-5` |
| `apps/api/tests/Sport.Disciplines.BOX.Tests/BoxResultSchemaTests.cs` | BOX tests | Add rejection test |
| `apps/api/tests/Sport.Disciplines.ATH.Tests/AthResultSchemaTests.cs` | ATH tests | Flip assertion to `I-RES-5` |

**Ordering rationale:** the FBL/ATH/BOX schema tests assert through `ApplySnapshot` → `DomainException.Code`. For them to see `I-RES-5`, the aggregate must already propagate `validation.Code`. So **Task 2 (propagation) precedes the discipline tasks**.

---

## Task 1: Add `Code` to `Result`

**Files:**
- Modify: `apps/api/src/Sport.Core/Shared/Result.cs`
- Test: `apps/api/tests/Sport.Core.Tests/Shared/ResultTests.cs` (create)

- [ ] **Step 1: Write the failing tests**

Create `apps/api/tests/Sport.Core.Tests/Shared/ResultTests.cs`:

```csharp
using FluentAssertions;
using Sport.Core.Shared;

namespace Sport.Core.Tests.Shared;

public class ResultTests
{
    [Fact]
    public void Ok_has_no_error_and_no_code()
    {
        var r = Result.Ok();
        r.IsSuccess.Should().BeTrue();
        r.Error.Should().BeNull();
        r.Code.Should().BeNull();
    }

    [Fact]
    public void Fail_with_message_only_leaves_code_null()
    {
        var r = Result.Fail("boom");
        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Be("boom");
        r.Code.Should().BeNull();
    }

    [Fact]
    public void Fail_with_code_sets_both_code_and_message()
    {
        var r = Result.Fail("I-RES-5", "boom");
        r.IsSuccess.Should().BeFalse();
        r.Code.Should().Be("I-RES-5");
        r.Error.Should().Be("boom");
    }

    [Fact]
    public void Fail_with_blank_code_throws()
    {
        var act = () => Result.Fail("  ", "boom");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Fail_with_blank_message_throws()
    {
        var act = () => Result.Fail("I-RES-5", "  ");
        act.Should().Throw<ArgumentException>();
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~ResultTests"`
Expected: FAIL — `Result` has no `Code` property and no `Fail(string, string)` overload (compile error).

- [ ] **Step 3: Implement `Code` and the overload**

Replace the body of `apps/api/src/Sport.Core/Shared/Result.cs` with:

```csharp
namespace Sport.Core.Shared;

public readonly record struct Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? Code { get; }

    private Result(bool isSuccess, string? error, string? code)
    {
        IsSuccess = isSuccess;
        Error = error;
        Code = code;
    }

    public static Result Ok() => new(true, null, null);

    public static Result Fail(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message is required for a failure result.", nameof(error));
        return new Result(false, error, null);
    }

    public static Result Fail(string code, string error)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code is required when provided for a failure result.", nameof(code));
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message is required for a failure result.", nameof(error));
        return new Result(false, error, code);
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~ResultTests"`
Expected: PASS (5 tests).

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Shared/Result.cs apps/api/tests/Sport.Core.Tests/Shared/ResultTests.cs
git commit -F - <<'EOF'
feat(core): add optional error Code to Result

Result carried only a message, forcing consumers that turn a failed Result
into a DomainException to hardcode a single code regardless of why validation
failed. Add an optional Code plus a Fail(code, message) overload so a validator
can report the precise code.

Additive on purpose: Error stays a string and Fail(string) keeps its signature,
so every existing consumer (Event.cs, discipline Validate* methods) is unchanged.
Chose this over a ResultError{Code,Message} record, which would force changing
Error's type and break those call sites — out of scope here.
EOF
```

---

## Task 2: Propagate the code in `UnitResultDocument`

**Files:**
- Modify: `apps/api/src/Sport.Core/Results/UnitResultDocument.cs:67`
- Test: `apps/api/tests/Sport.Core.Tests/Results/UnitResultDocumentTests.cs`

- [ ] **Step 1: Write the failing test**

In `apps/api/tests/Sport.Core.Tests/Results/UnitResultDocumentTests.cs`, add this test method (place it after `ApplySnapshot_rejects_when_schema_validation_fails_and_restores_state`):

```csharp
    [Fact]
    public void ApplySnapshot_propagates_the_schema_error_code_when_present()
    {
        var doc = NewDoc();
        var rejecting = new CodedRejectingSchema();
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), rejecting))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-5");
        doc.Competitors.Should().BeEmpty();
        doc.Version.Should().Be(1);
    }
```

And add this nested test schema next to the existing `RejectingSchema` class (inside `UnitResultDocumentTests`):

```csharp
    private sealed class CodedRejectingSchema : DefaultResultSchema
    {
        public override Result Validate(UnitResultDocument document) => Result.Fail("I-RES-5", "nope");
    }
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~UnitResultDocumentTests"`
Expected: FAIL — `ApplySnapshot_propagates_the_schema_error_code_when_present` throws `DomainException` with `Code == "I-RES-8"` (still hardcoded), not `I-RES-5`. The existing `..._restores_state` test (uncoded `RejectingSchema`) still passes on `I-RES-8`.

- [ ] **Step 3: Propagate the code**

In `apps/api/src/Sport.Core/Results/UnitResultDocument.cs`, line 67, change:

```csharp
            throw new DomainException("I-RES-8", validation.Error!);
```

to:

```csharp
            throw new DomainException(validation.Code ?? "I-RES-8", validation.Error!);
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test apps/api/tests/Sport.Core.Tests/Sport.Core.Tests.csproj --filter "FullyQualifiedName~UnitResultDocumentTests"`
Expected: PASS — new test sees `I-RES-5`; `..._restores_state` (uncoded schema → fallback) still sees `I-RES-8`.

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Core/Results/UnitResultDocument.cs apps/api/tests/Sport.Core.Tests/Results/UnitResultDocumentTests.cs
git commit -F - <<'EOF'
feat(results): propagate schema error code from ApplySnapshot

ApplySnapshot hardcoded I-RES-8 for every schema validation failure, so an
outcome violation (conceptually I-RES-5) was indistinguishable from a future
vocabulary/placement violation. Propagate validation.Code, falling back to
I-RES-8 only when the schema reported no code — which is the reserved slot for
the deferred vocabulary check (I-RES-8/I-RES-9).
EOF
```

---

## Task 3: FBL schema emits `I-RES-5`

**Files:**
- Modify: `apps/api/src/Sport.Disciplines.FBL/FblResultSchema.cs:17,20`
- Test: `apps/api/tests/Sport.Disciplines.FBL.Tests/FblResultSchemaTests.cs:51`

- [ ] **Step 1: Flip the existing assertion (failing test)**

In `apps/api/tests/Sport.Disciplines.FBL.Tests/FblResultSchemaTests.cs`, line 51, change:

```csharp
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-8");
```

to:

```csharp
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-5");
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.FBL.Tests/Sport.Disciplines.FBL.Tests.csproj --filter "FullyQualifiedName~Schema_rejects_ranked_outcome_with_a_rank_field"`
Expected: FAIL — schema still emits an uncoded `Result.Fail(...)`, so the aggregate falls back to `I-RES-8`.

- [ ] **Step 3: Emit the code in the schema**

In `apps/api/src/Sport.Disciplines.FBL/FblResultSchema.cs`, change both failure returns:

Line 17:
```csharp
                return Result.Fail("I-RES-5", "FBL does not use Rank; use WLT.");
```
Line 20:
```csharp
                return Result.Fail("I-RES-5", "FBL is head-to-head: each scored competitor must carry a WLT.");
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test apps/api/tests/Sport.Disciplines.FBL.Tests/Sport.Disciplines.FBL.Tests.csproj`
Expected: PASS (all FBL tests).

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.FBL/FblResultSchema.cs apps/api/tests/Sport.Disciplines.FBL.Tests/FblResultSchemaTests.cs
git commit -F - <<'EOF'
feat(fbl): tag FBL result outcome violations as I-RES-5

FBL Validate failures (Rank used, missing WLT) are outcome violations. Emit
I-RES-5 so they no longer surface through the aggregate's I-RES-8 fallback.
EOF
```

---

## Task 4: BOX schema emits `I-RES-5`

**Files:**
- Modify: `apps/api/src/Sport.Disciplines.BOX/BoxResultSchema.cs:25`
- Test: `apps/api/tests/Sport.Disciplines.BOX.Tests/BoxResultSchemaTests.cs` (add a rejection test)

- [ ] **Step 1: Write the failing test**

In `apps/api/tests/Sport.Disciplines.BOX.Tests/BoxResultSchemaTests.cs`, add this test method (after `Validates_a_two_row_bout`). Note `Sport.Core.Shared` is not yet imported in this file — add `using Sport.Core.Shared;` to the usings if the `DomainException` reference does not resolve:

```csharp
    [Fact]
    public void Schema_rejects_a_scored_row_without_wlt()
    {
        var schema = new BoxResultSchema();
        var doc = UnitResultDocument.CreateForUnit(UnitResultId.New(), UnitId.New(), UnitRsc, DisciplineCode.From("BOX"), Cat);
        FluentActions.Invoking(() => doc.ApplySnapshot(
                new[] { new CompetitorResult(EntryId.New(), 1) { ResultValue = "WP" } },
                Array.Empty<ResultSegment>(), Array.Empty<ResultExtension>(), schema))
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-5");
    }
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.BOX.Tests/Sport.Disciplines.BOX.Tests.csproj --filter "FullyQualifiedName~Schema_rejects_a_scored_row_without_wlt"`
Expected: FAIL — BOX emits an uncoded `Result.Fail(...)`, so the aggregate falls back to `I-RES-8`, not `I-RES-5`.

- [ ] **Step 3: Emit the code in the schema**

In `apps/api/src/Sport.Disciplines.BOX/BoxResultSchema.cs`, line 25, change:

```csharp
                return Result.Fail("BOX is head-to-head: each scored competitor must carry a WLT.");
```

to:

```csharp
                return Result.Fail("I-RES-5", "BOX is head-to-head: each scored competitor must carry a WLT.");
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test apps/api/tests/Sport.Disciplines.BOX.Tests/Sport.Disciplines.BOX.Tests.csproj`
Expected: PASS (all BOX tests).

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.BOX/BoxResultSchema.cs apps/api/tests/Sport.Disciplines.BOX.Tests/BoxResultSchemaTests.cs
git commit -F - <<'EOF'
feat(box): tag BOX result outcome violations as I-RES-5

A scored BOX row missing WLT is an outcome violation. Emit I-RES-5 so it no
longer surfaces through the aggregate's I-RES-8 fallback; add the rejection
test BOX lacked.
EOF
```

---

## Task 5: ATH schema emits `I-RES-5`

**Files:**
- Modify: `apps/api/src/Sport.Disciplines.ATH/AthResultSchema.cs:19,21`
- Test: `apps/api/tests/Sport.Disciplines.ATH.Tests/AthResultSchemaTests.cs:57`

- [ ] **Step 1: Flip the existing assertion (failing test)**

In `apps/api/tests/Sport.Disciplines.ATH.Tests/AthResultSchemaTests.cs`, line 57, change:

```csharp
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-8");
```

to:

```csharp
            .Should().Throw<DomainException>().Where(e => e.Code == "I-RES-5");
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test apps/api/tests/Sport.Disciplines.ATH.Tests/Sport.Disciplines.ATH.Tests.csproj --filter "FullyQualifiedName~Schema_rejects_a_wlt_outcome"`
Expected: FAIL — schema emits an uncoded `Result.Fail(...)`, aggregate falls back to `I-RES-8`.

- [ ] **Step 3: Emit the code in the schema**

In `apps/api/src/Sport.Disciplines.ATH/AthResultSchema.cs`, change both failure returns:

Line 19:
```csharp
                return Result.Fail("I-RES-5", "ATH is ranked: WLT must be absent.");
```
Line 21:
```csharp
                return Result.Fail("I-RES-5", "ATH requires a positive SortOrder on every row.");
```

> Note: the `SortOrder` check is folded into `I-RES-5` (outcome/coherence) for now; it may be reclassified to a placement code (`I-RES-9`) when the vocabulary/placement slice (gap #1) lands. This is intentional per spec §4.4.

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test apps/api/tests/Sport.Disciplines.ATH.Tests/Sport.Disciplines.ATH.Tests.csproj`
Expected: PASS (all ATH tests).

- [ ] **Step 5: Commit**

```bash
git add apps/api/src/Sport.Disciplines.ATH/AthResultSchema.cs apps/api/tests/Sport.Disciplines.ATH.Tests/AthResultSchemaTests.cs
git commit -F - <<'EOF'
feat(ath): tag ATH result outcome violations as I-RES-5

ATH Validate failures (WLT present on a ranked result, non-positive SortOrder)
are outcome/coherence violations. Emit I-RES-5 instead of surfacing through the
aggregate's I-RES-8 fallback. SortOrder stays under I-RES-5 for now; it may move
to a placement code (I-RES-9) when the deferred vocabulary/placement slice lands.
EOF
```

---

## Task 6: Full-suite regression

**Files:** none (verification only)

- [ ] **Step 1: Run the entire solution test suite**

Run: `dotnet test apps/api/Sport.slnx`
Expected: PASS — all green. The count grows from 259 by the tests added here (5 in `ResultTests`, 1 coded-propagation in `UnitResultDocumentTests`, 1 rejection in `BoxResultSchemaTests`). No `I-RES-8` assertion remains except `UnitResultDocumentTests` `..._restores_state` (the uncoded fallback).

- [ ] **Step 2: Confirm no stray `I-RES-8` test assertions remain for coded paths**

Run: `git grep -n "I-RES-8" apps/api/tests`
Expected: exactly one match — `UnitResultDocumentTests.cs` `ApplySnapshot_rejects_when_schema_validation_fails_and_restores_state` (the intended fallback). If any FBL/ATH/BOX match remains, fix it.

---

## Self-Review notes

- **Spec coverage:** §3 (additive `Code`) → Task 1. §4.1 → Task 1. §4.5 (propagation) → Task 2. §4.2/4.3/4.4 (FBL/BOX/ATH `I-RES-5`) → Tasks 3/4/5. §5 (semantics, no catalog change) → respected; no catalog task. §6 (testing) → Tasks 1–6. §7 (out of scope) → no tasks touch `Event.cs`, discipline `Validate*`, catalog, or a `ResultError` record.
- **Type consistency:** `Result.Fail(string code, string error)` signature used identically in Tasks 1–5. `validation.Code ?? "I-RES-8"` matches the `Code` property added in Task 1. Test schema `CodedRejectingSchema` mirrors the existing `RejectingSchema` shape.
- **Fallback preserved:** `UnitResultDocumentTests.cs:69` (uncoded `RejectingSchema`) deliberately keeps asserting `I-RES-8` — it is the regression guard for the fallback branch.

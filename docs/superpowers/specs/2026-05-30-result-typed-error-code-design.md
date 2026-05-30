# Typed error code in `Result` — design

**Date:** 2026-05-30
**Status:** Approved

## 1. Problem

`UnitResultDocument.ApplySnapshot` collapses **every** `IResultSchema.Validate` failure into a
single `DomainException("I-RES-8", ...)` (`UnitResultDocument.cs:67`), even though the failures it
sees today are all **outcome** violations, which are conceptually `I-RES-5`.

Root cause: the error code lives at the **call site**, not in the `Result`. The validator
(`Validate`) produces only a human message via `Result.Fail(string)`; `Result` has no field to
carry a code (`Result.cs`: `IsSuccess` + `Error` only). So the consumer is forced to hardcode one
code for all failure reasons. This is the same call-site pattern used across the domain
(`Event.cs:58/67/113` hardcode `I-STR-12/13/17`).

Consequence: once the API layer maps domain codes to HTTP envelopes, an outcome violation and a
future vocabulary/placement violation (gap #1: `I-RES-8`/`I-RES-9`) are indistinguishable — both
arrive as `I-RES-8`.

## 2. Goal

The validator — which knows *why* it failed — attaches the error code to the `Result`, and the
aggregate propagates that code instead of hardcoding one. Outcome violations surface as `I-RES-5`;
`I-RES-8` remains only as a safe fallback for an uncoded schema failure (the slot where vocabulary
validation will later emit `I-RES-8`/`I-RES-9`).

Scope is the **results layer only**. The `Validate*` methods of the seven discipline modules
(`ValidateEventType`, `ValidateEntry`, …) and `Event.cs` are intentionally untouched — their
messages have no assigned `I-XXX` codes yet, and codifying them is a separate taxonomy exercise.

## 3. Approach

Add an **optional** `string? Code` to `Result`, keeping `Error` as a `string`. This is purely
additive: `Result.Fail(string)` keeps its signature and yields `Code == null`, so every untouched
consumer (`Event.cs`, the discipline `Validate*` methods) compiles and behaves identically. A new
`Result.Fail(string code, string error)` overload sets the code.

**Why additive `Code`, not a dedicated `ResultError { Code, Message }`:** a record would be a
cleaner "a failure is one thing", but it forces changing the type of `Error`, which breaks
`Event.cs` (`validation.Error!`) — out of scope. With `Error` kept as a `string`, the record buys
almost nothing. If we ever widen scope to the whole `Result` channel, the record becomes worthwhile;
revisit then.

## 4. Affected units

1. **`Result`** (`Sport.Core/Shared/Result.cs`):
   - Add `string? Code { get; }`.
   - `Ok()` → `Code = null`.
   - `Fail(string error)` → `Code = null` (unchanged signature/behavior; still rejects blank `error`).
   - New `Fail(string code, string error)` → both set; rejects blank `code` and blank `error`.

2. **`FblResultSchema.Validate`** (`Sport.Disciplines.FBL`): both `Fail(...)` returns → `Fail("I-RES-5", ...)`.

3. **`BoxResultSchema.Validate`** (`Sport.Disciplines.BOX`): the `Fail(...)` return → `Fail("I-RES-5", ...)`.

4. **`AthResultSchema.Validate`** (`Sport.Disciplines.ATH`): both returns (WLT-present and
   `SortOrder <= 0`) → `Fail("I-RES-5", ...)`. The `SortOrder` check is folded into `I-RES-5`
   (outcome/coherence) for now; it can be reclassified to a placement code (`I-RES-9`) when gap #1
   lands.

5. **`UnitResultDocument.ApplySnapshot`** (`Sport.Core/Results/UnitResultDocument.cs:67`):
   `throw new DomainException(validation.Code ?? "I-RES-8", validation.Error!);`

`JudResultSchema` has no `Validate` override (it inherits the permissive `Result.Ok()`), so it is
untouched — its missing validation is gap #1, not this change.

## 5. Error semantics

- `I-RES-5` — outcome/coherence violation reported by a discipline schema (the only failures the
  current `Validate` methods produce).
- `I-RES-8` — fallback when a schema failure carries no code. After this change no current schema
  hits it; it is the reserved slot for the future vocabulary check (gap #1), which will also add
  `I-RES-9` for placement.

No `DomainErrorCatalog` entry is added. The catalog architecture test
(`ArchitectureRules.cs:223-271`, `DomainErrorCatalog_covers_every_domain_code_thrown_by_Sport_Core`)
is **not exhaustive**: it only triggers `I-COMP-*`/`I-DR-1` from `Competition`/`DateRange` and asserts
that observed set is a subset of the catalog. It never exercises the `I-RES-*` paths, so introducing
`I-RES-5` neither breaks it nor requires catalog work. HTTP mapping of `I-RES-*` belongs to the
deferred endpoints slice.

## 6. Testing (TDD)

**`Result` (`Sport.Core.Tests`):**
- `Fail(code, error)` sets both `Code` and `Error`; `IsSuccess == false`.
- `Fail(error)` leaves `Code == null`; existing behavior preserved.
- `Ok()` → `Code == null`, `Error == null`.
- `Fail(code, error)` with blank `code` throws `ArgumentException`; blank `error` still throws.

**Schemas (`Sport.Disciplines.{FBL,BOX,ATH}.Tests`):**
- Each existing outcome-violation case now returns `Code == "I-RES-5"` (assert directly on `Validate`).

**`UnitResultDocument` (`Sport.Core.Tests/Results`):**
- A snapshot that violates outcome throws `DomainException` with `Code == "I-RES-5"` (no longer
  `I-RES-8`), document left untouched (rollback already covered).
- Fallback: a schema failure with `Code == null` still throws `DomainException` with `Code == "I-RES-8"`
  (use a test schema returning `Fail(string)`).

**Regression:**
- Full suite stays green (currently 259/259). No call-site signature changes outside the units above.

## 7. Out of scope

- `Event.cs` and the discipline `Validate*` methods (`ValidateEventType`/`ValidateEntry`/…).
- Vocabulary/placement enforcement (gap #1: `I-RES-8`/`I-RES-9` cross-checks against declared sets,
  plus the missing declarative surface for `ResultType`/`Segment`/`Extension`).
- `DomainErrorCatalog` / HTTP status mapping for `I-RES-*` (deferred endpoints slice).
- A `ResultError { Code, Message }` record (revisit only if scope widens to the whole channel).

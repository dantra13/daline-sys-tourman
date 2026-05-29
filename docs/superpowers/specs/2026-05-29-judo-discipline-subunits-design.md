# Judo (JUD) Discipline + Subunit-Hosting — Design

Date: 2026-05-29
Status: Approved (pending final spec review)

## Goal

Incorporate Judo as a discipline module to **validate the core**, exercising the
one structural feature no existing discipline uses: **subunits**. Judo team events
(mixed team) are played as a parent "team match" unit decomposed into individual
weight-category contests — i.e. subunits. This is the first real driver of
`Unit.CreateParentForSubunits` + `Subunit.Create`, and it surfaces gaps in how the
discipline registry governs subunit-hosting.

## Context (current state)

- `GenderCode` already supports `M, W, X, O` — mixed (`X`) works without changes.
- `EntryType` already supports `Athlete, Team, Group`.
- `Unit.CreateParentForSubunits` / `Unit.AddSubunit` / `Subunit.Create` exist and are
  persistence-tested (`PhaseUnitSubunitPersistenceTests`), **but only synthetically**:
  the test fabricates a unit code via a `FakeRegistry` over FBL. No real discipline
  module governs *when* a unit hosts subunits or *which* subunit codes are valid.
- Existing discipline modules (BOX, FBL, ATH, BDM, BKB, VBV) implement `IDisciplineModule`
  with `IPhaseCatalog` / `IUnitCodeStrategy` / `IEntryRules`.

## Scope

**In scope** (Approach A):

- New `Sport.Disciplines.JUD` module (intrinsic discipline rules).
- Extend the discipline-registry contracts so **subunit-hosting is first-class**.
- Add **governed structural assembly** on the `Event` aggregate root.
- Harden two RSC invariants revealed by the ODF spec (atomic trailing `--`, subunit ≠ `00`).
- Register JUD in the API host; tests.

**Out of scope** (separate epics):

- **Competition formats.** Phases/brackets/seeding/repechage progression are a *format*
  concern, not a *discipline* concern. The discipline module declares its *typical*
  phases as a provisional allow-list (same as BOX does today), to be re-evaluated when the
  competition-format subsystem is designed. See `docs/competition-formats/`.
- **Structure vertical via Application/API.** Event/Phase/Unit/Subunit are not exposed
  through commands/endpoints yet; that remains its own epic.

## Layering principle (decided)

- **Intrinsic to the discipline** (does not change between competitions): genders,
  weight-category events, the team event, entry composition rules, **subunit-hosting**,
  official functions.
- **Configured per competition** (the *format*): which phases exist, bracket size, byes,
  repechage on/off, resolution method. Deferred.

## RSC convention (validated against ODF Foundation Principles §10.3.1.7)

The ODF normative codification scheme confirms our internal convention:

- Unit slot is **8 characters** within the 34-char RSC (positions [26..34)).
- **Atomic unit** (no subunits): first 6 chars used; **positions 7 & 8 = `--`**.
  Examples: `000100--`, `000200--`.
- **Parent unit** (hosts subunits): all 8 chars; **positions 7 & 8 = `00`**.
  Example: `00010000`.
- **Subunits**: `00010001`, `00010002`, … (positions 7 & 8 = `01`..`NN`).
- Reference example (Alpine team): `ALPXPLTEAM4-----------8FNL00010000` (parent) /
  `...00010001` (run 1).

Decision: **keep the `00`-parent convention** — it is ODF-correct, not merely internal.

### Core hardening (new invariants)

Reading the spec revealed two invariants the domain does not currently enforce:

1. **Atomic units must end in `--`.** `Unit.CreateAtomic` must reject a `UnitCode` whose
   positions 7 & 8 are not `--` (new domain error, e.g. `I-STR-14`).
2. **`SubunitCode` must not be `00`.** `00` is reserved as the parent marker; reject it in
   `SubunitCode` validation (and therefore in `Subunit.Create`).

## Component design

### 1. `Sport.Disciplines.JUD` module

Mirrors the BOX/FBL structure. `JudModule : IDisciplineModule`:

- **DisplayName:** `"Judo"`. **Code:** `JUD`.
- **SupportedGenders:** `{ M, W, X }`.
- **EventTypes** (Paris 2024 real):
  - Men individual (`EntryType.Athlete`), genders `{M}`:
    `60KG, 66KG, 73KG, 81KG, 90KG, 100KG, O100KG`
  - Women individual (`EntryType.Athlete`), genders `{W}`:
    `48KG, 52KG, 57KG, 63KG, 70KG, 78KG, O78KG`
  - Mixed team (`EntryType.Team`), genders `{X}`: `TEAM6` — **hosts subunits** with
    canonical contests `01`..`06` (3 women + 3 men weight contests).
  - `ModifierContract.Forbidden` for all.
  - Encoding note: "over/+" categories use an `O` prefix (`O100KG`, `O78KG`) because
    `EventTypeCode` charset is `A–Z0–9` (no `+`).
- **EntryRules** (`JudEntryRules`):
  - `Athlete` → composition `(1, 1)`.
  - `Team` → composition `(6, 6)` (the contesting six; rosters/substitutes deferred).
  - Reject entry types not allowed for the requested event.
- **PhaseCatalog** (`JudPhaseCatalog`, **provisional / format placeholder**):
  - Individual events: `R64, R32, 8FNL, QFNL, SFNL, FNL, REP1, REPF`.
  - Team event (`TEAM*`): `R32, 8FNL, QFNL, SFNL, FNL` (no repechage).
  - `IsAllowedForEventType` differentiates `TEAM*` from weight events.
- **UnitCodeStrategy** (`JudUnitCodeStrategy`):
  - Produces atomic codes ending in `--` for individual contests.
  - Produces parent codes ending in `00` for team matches.
- **Functions:** `JUD.REF` (Referee), `JUD.JUD1`, `JUD.JUD2`, `JUD.CARE` at
  `ScopeLevel.Unit` (reasonable set, not exhaustive).
- **ValidateEventType:** `TEAM6` only for gender `X`; weight categories only for `M`/`W`;
  modifier forbidden.
- `ServiceCollectionExtensions.AddJudModule(...)` following the existing module DI pattern.

### 2. Registry contract extension (subunit-hosting first-class)

`Sport.Core/DisciplineRegistry`:

- **`EventTypeDescriptor`** (record) gains optional subunit structure:
  - `bool HostsSubunits` (default `false`)
  - `IReadOnlyCollection<SubunitCode> CanonicalSubunits` (default `[]`)
- **`IDisciplineModule`** gains:
  - `bool HostsSubunits(EventTypeCode type)`
  - `IReadOnlyCollection<SubunitCode> SubunitsFor(EventTypeCode type)`
  - `Result ValidateSubunitCode(EventTypeCode type, SubunitCode code)`
  
  These read from the matching `EventTypeDescriptor`. A small shared default implementation
  keeps the other five modules trivial (`false` / `[]`).

### 3. Governed structural assembly on `Event`

`Event` is the aggregate root for the Phase→Unit→Subunit tree, so structural mutations
flow through it and consult the module:

- `Event.AddAtomicUnit(phaseCode, unitCode, module, scheduledStart)` — requires the event's
  type **not** to host subunits; validates the unit code (incl. atomic `--` trailing);
  builds via `Unit.CreateAtomic`; attaches to the phase.
- `Event.AddTeamMatchUnit(phaseCode, parentUnitCode, contests, module, scheduledStart)` —
  requires `HostsSubunits`; validates each `SubunitCode` via `module.ValidateSubunitCode`;
  builds `Unit.CreateParentForSubunits` + N `Subunit.Create`; attaches.

Low-level factories (`Unit.CreateAtomic`, `CreateParentForSubunits`, `Subunit.Create`)
remain (persistence tests use them), but the governed path is via `Event`.

### 4. Registration

- `Sport.Api` references `Sport.Disciplines.JUD`.
- `Program.cs`: add `.AddJudModule()` and register JUD in `BuildSportRegistry`.

## Error handling

- New domain error codes for the hardened invariants (e.g. `I-STR-14` atomic trailing,
  and a subunit-code guard). Subunit-hosting governance violations surface as
  `Result.Fail` from the module and `DomainException` from the governed `Event` methods,
  consistent with existing `I-STR-*` usage.

## Testing strategy

- **`Sport.Disciplines.JUD.Tests`** (mirrors existing module test projects):
  - Event types per gender (TEAM6 only X; weights only M/W; modifier rejected).
  - Entry rules: Athlete `(1,1)`, Team `(6,6)`, wrong-type rejection.
  - Phase allow-list per event type (individual vs team).
  - Subunit-hosting: `HostsSubunits`/`SubunitsFor`/`ValidateSubunitCode` for `TEAM6`
    (`01`..`06` valid; `00` and out-of-range rejected); individual events host none.
- **Core validation test** (in `Sport.Core.Tests`, the key one):
  - Build a `Competition` with JUD, an `Event` `TEAM6` (gender X), a phase, and a
    **team match with 6 real subunits** via `Event.AddTeamMatchUnit`.
  - Assert the aggregate governs it: rejects subunits on individual events, rejects
    out-of-catalog subunit codes, accepts the 6 contests, and produces ODF-correct RSCs
    (parent `…00`, contests `…01`..`…06`).
  - Cover the hardened invariants: atomic unit with non-`--` trailing rejected;
    `SubunitCode("00")` rejected.
- **Architecture tests:** JUD appears in the catalog/registry like the other disciplines.
- **Persistence (optional):** a JUD-driven round-trip of a team match + subunits
  (existing subunit persistence is already covered synthetically).

## File map

```
src/Sport.Disciplines.JUD/
  JudModule.cs
  JudPhaseCatalog.cs
  JudUnitCodeStrategy.cs
  JudEntryRules.cs
  JudWeightCategories.cs            (event-type definitions)
  ServiceCollectionExtensions.cs
  Sport.Disciplines.JUD.csproj
src/Sport.Core/DisciplineRegistry/
  EventTypeDescriptor.cs            (modify: HostsSubunits, CanonicalSubunits)
  IDisciplineModule.cs              (modify: subunit-hosting methods)
src/Sport.Core/Structure/
  Event.cs                          (modify: AddAtomicUnit, AddTeamMatchUnit)
  Unit.cs                           (modify: CreateAtomic enforces '--' trailing)
  SubunitCode.cs                    (modify: reject '00')
src/Sport.Disciplines.{ATH,BDM,BKB,BOX,FBL,VBV}/*Module.cs
                                    (modify: implement new IDisciplineModule members,
                                     default false/[])
src/Sport.Api/Program.cs            (modify: register JUD)
tests/Sport.Disciplines.JUD.Tests/  (new)
tests/Sport.Core.Tests/...          (new core-governance + invariant tests)
tests/Sport.Architecture.Tests/...  (extend catalog coverage)
```

## Open questions / deferred

- Competition-format subsystem (phases as configured format) — separate brainstorm.
- Team rosters & substitutes vs the contesting six — deferred; `Team (6,6)` for now.
- Full official-functions catalog and judo technique codes — out of scope.

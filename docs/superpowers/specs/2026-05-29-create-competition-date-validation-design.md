# Create-competition date validation — design

**Date:** 2026-05-29
**Status:** Approved

## 1. Problem

`POST /competitions` with empty/invalid date strings returns a generic `400 request.malformed`
that names no fields, even though the rest of the required fields (`code`, `name`, `genders`)
already come back as a detailed `422` with per-field codes.

Root cause: two error layers.

| Layer | Where | Reports | Status |
|---|---|---|---|
| Parse / binding | System.Text.Json binder, *before* the handler | first field that fails to cast, then aborts | `400` generic |
| Surface validation | `CreateCompetitionHandler` | every invalid field, by name | `422` detailed |

`CreateCompetitionRequest.DatesDto` types `Start`/`End` as `DateOnly`, so empty strings fail at
the binder and short-circuit before the handler can enumerate the other invalid fields. Dates are
the only strongly-typed field at the request boundary — `code` and `genders` are already `string`,
which is exactly why they surface as field-level `422` errors.

## 2. Goal

A body with empty/invalid required fields (dates included) returns a single `422` enumerating every
invalid field at once, instead of a fail-fast `400` on the first unparseable date.

## 3. Approach

Move date parsing from the binder into the handler's surface-validation layer — the same place that
already validates `code`/`name`/`genders`. Dates become "loose string at the boundary, validated in
the handler", matching the shape `code` and `genders` already use.

The command (`CreateCompetition`) also carries the dates as `string`. This is required: if the
command stayed `DateOnly`, parsing would have to happen at the boundary (`ToCommand()`), splitting
validation across two layers and two `ValidationException` producers. Keeping the raw strings on the
command keeps all surface validation in the handler — the established pattern (§5 of the competition
endpoints design).

## 4. Affected units

1. **`CreateCompetitionRequest.DatesDto`** (`Sport.Api`): `DateOnly Start, End` → `string Start, End`.
   `ToCommand()` passes the strings through unchanged.
2. **`CreateCompetition`** (`Sport.Application`, Wolverine command): `DateOnly StartDate, EndDate`
   → `string StartDate, EndDate`.
3. **`CreateCompetitionHandler`** — new surface guards:
   - Strict ISO parse: `DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)`.
   - Empty/whitespace → `competition.start_date_required` / `competition.end_date_required`.
   - Non-empty but unparseable → `competition.start_date_invalid` / `competition.end_date_invalid`.
   - The range check (`start > end` → `competition.date_range_invalid`) runs **only if both parsed**.
   - The parsed `DateOnly` values feed `DateRange.Create(...)`.

## 5. New error codes

Four new **surface codes** — no `DomainErrorCatalog` entry, exempt from the catalog architecture
test (§6 of the competition endpoints design treats handler-produced codes as surface codes, like
`competition.name_required`):

- `competition.start_date_required`
- `competition.end_date_required`
- `competition.start_date_invalid`
- `competition.end_date_invalid`

All map to `422`. `target` uses the dotted path `dates.start` / `dates.end`, consistent with the
existing `disciplines[i].genders` target style.

## 6. Error semantics

Everything above is `422` (well-formed JSON, semantically invalid) — consistent with §3 of the
competition endpoints design. The `400 request.malformed` path (and the structured warning log added
on 2026-05-29) remains for genuinely unparseable JSON: syntax errors and structural type mismatches
(e.g. `dates` sent as a string instead of an object, `disciplines` sent as a number). After this
change, empty-string fields all flow to `422`; only true JSON-level failures stay `400`.

## 7. Testing (TDD)

**Handler (`CreateCompetitionHandlerTests`):**
- Empty `start` → failure `competition.start_date_required` (target `dates.start`).
- Garbage `start` (e.g. `"nope"`) → `competition.start_date_invalid`.
- All-empty body → one `ValidationException` whose failures include `name`, `code`, `dates.start`,
  `dates.end`, and `disciplines` together.
- Valid dates still parse and create the competition.
- Range check still fires `competition.date_range_invalid` when both dates parse and `start > end`.

**Endpoint (`CompetitionEndpointsTests`):**
- POST the all-empty body → `422` whose `errors[]` enumerates the full set of codes.

**Regression:**
- Update existing handler/test call sites that build `CreateCompetition` with `DateOnly` to use strings.
- The malformed-JSON `400` tests (Testing and Development factories) stay green — a true syntax error
  is still `400 request.malformed`.

## 8. Out of scope

- Other endpoints or aggregates.
- The response DTO (`CompetitionDto` keeps `DateOnly`).
- FluentValidation (remains rejected, per §5 of the competition endpoints design).

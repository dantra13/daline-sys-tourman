# ODF GEN General Messages Interface: DT_POOL_STANDING, Pages 176-189

Source: `C:\Users\mella\WebstormProjects\sportivo\docs\references\odf\ODF_GEN_R-OWG2026-GEN.pdf`, pages 176-189.

This note restructures section `2.1.14 Pool Standings` from the OWG2026 General Messages Interface Document into a readable reference for Sportivo domain modeling. This is the general interface contract for `DT_POOL_STANDING`; sport data dictionaries still define sport-specific standing rules, tiebreakers, optional fields, and code values.

## 2.1.14 Pool Standings

`DT_POOL_STANDING` contains the standings of a group or pool in a competition. Yes: this is the ODF message used to represent the table of a group, such as a football group table after each match.

The message is similar to `DT_PHASE_RESULT`, but differs in trigger and frequency. It is sent at the start of OVR operations and then after each event unit that affects the pool.

One `DT_POOL_STANDING` message is sent independently for each group or pool in a phase. The group/pool identity is determined from the message headers, specifically `DocumentCode` and, where the sport defines it, `DocumentSubtype`.

The mandatory attributes and elements in the general document apply to all sports. Optional standing attributes and sport-specific extensions are defined by the relevant ODF Sport Data Dictionary.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE Id` | Competition identifier. |
| `DocumentCode` | `CC@PHASE Code` | Phase RSC. This identifies the phase/pool scope of the message. |
| `DocumentSubcode` | Sport-specific | Optional extension for `DocumentCode`. |
| `DocumentType` | `DT_POOL_STANDING` | Pool standings message. |
| `DocumentSubtype` | Sport-specific | Optional extension for `DocumentType`; commonly needed to distinguish a specific pool/group when the sport defines it that way. |
| `Version` | Positive integer | Ascending version number associated with the message content. |
| `ResultStatus` | `START_LIST`, `INTERMEDIATE`, `UNCONFIRMED`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` | Status of the standings content. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | Date | Message generation date. |
| `Time` | Time | Message generation time. |
| `LogicalDate` | Date | Logical competition date. |
| `Source` | `SCGEN@Source Code` | System that generated the message. |

## Trigger and Frequency

The general rules are:

| Condition | Expected `ResultStatus` | Meaning |
|---|---|---|
| Before the competition starts | `START_LIST` | Initial table build, usually with participants and zeroed standings. |
| After an event unit in the phase finishes | `INTERMEDIATE` | Updated table after a match/game/unit affecting the pool. |
| When the phase finishes | `OFFICIAL` | Final standings for the pool; no more event units remain. |
| IOC, CAS, IF, or similar decisions are pending | `PROVISIONAL` | Table is published but subject to pending decisions. |

Sport-specific principles are defined in the corresponding Sport Data Dictionary.

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- ExtendedInfo (0,N)
    |   |   +-- Extension (0,N)
    |   +-- Progress (0,1)
    |   |   +-- Extension (0,N)
    |   +-- SportDescription (0,1)
    |   +-- VenueDescription (0,1)
    +-- Result (1,N)
        +-- @Rank
        +-- @RankEqual
        +-- @ResultType
        +-- @Result
        +-- @IRM
        +-- @QualificationMark
        +-- @SortOrder
        +-- @Won
        +-- @Lost
        +-- @Tied
        +-- @Played
        +-- @For
        +-- @Against
        +-- @Diff
        +-- @Ratio
        +-- ExtendedResults (0,1)
        |   +-- ExtendedResult (1,N)
        |       +-- Extension (0,N)
        +-- RecordIndicators (0,1)
        |   +-- RecordIndicator (1,1)
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Description (0,1)
            |   +-- ExtendedDescription (0,N)
            +-- Composition (0,1)
            |   +-- Athlete (1,N)
            |       +-- Description (1,1)
            |       |   +-- ExtendedDescription (0,N)
            |       +-- Guide (0,N)
            |       +-- ExtendedResults (0,1)
            |           +-- ExtendedResult (1,N)
            |               +-- Extension (0,N)
            +-- Opponent (0,N)
                +-- @Code
                +-- @Type
                +-- @Pos
                +-- @Organisation
                +-- @Date
                +-- @Time
                +-- @TimeStamp
                +-- @Unit
                +-- @HomeAway
                +-- @Result
                +-- ExtendedResults (0,1)
                |   +-- ExtendedResult (1,N)
                |       +-- Extension (0,N)
                +-- Description (0,1)
                |   +-- ExtendedDescription (0,N)
                +-- Composition (0,1)
                    +-- Athlete (1,N)
                        +-- Description (1,1)
                        |   +-- ExtendedDescription (0,N)
                        +-- Guide (0,N)
                        +-- ExtendedResults (0,1)
                            +-- ExtendedResult (1,N)
                                +-- Extension (0,N)
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Gen` | M | `S(20)` | Version of the General Data Dictionary applicable to the message. |
| `Sport` | O | `S(35)` | Version of the Sport Data Dictionary applicable to the message. |
| `Codes` | M | `S(20)` | Version of the codes applicable to the message. |

### `Competition/ExtendedInfos`

`ExtendedInfos` carries optional metadata about the standing, progress in the phase, sport/event labels, and venue text.

#### `ExtendedInfo`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Type` | O | Sport-specific | Sport-specific metadata group. |
| `Code` | O | Sport-specific | Sport-specific metadata code. |
| `Pos` | O | Sport-specific | Position within the metadata group. |
| `Value` | O | Sport-specific | Metadata value. |

#### `Progress`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `LastUnit` | O | `CC@EVENT_UNIT Code` | Full RSC of the last completed or in-progress unit related to the standings content. |
| `UnitsTotal` | O | Positive integer | Total expected units that impact this standing. |
| `UnitsComplete` | O | `#0` | Number of completed units that impact this standing. |

#### `SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE ENG Description` | Discipline name, not code. |
| `EventName` | M | `CC@EVENT ENG Description` | Event name, not code. |
| `SubEventName` | O | `CC@PHASE ENG ShortDescription` | Phase short description. Include only if the message is in a single phase. |
| `Gender` | M | `CC@DISCIPLINE_GENDER Gender` | Gender code for the event. |

#### `VenueDescription`

Included only when the phase is contested at a single venue.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Venue` | M | `CC@VENUE Id` | Venue code. |
| `VenueName` | M | `CC@VENUE ENG Description` | Venue name, not code. |
| `Location` | O | `CC@LOCATION Id` | Location code. |
| `LocationName` | O | `CC@LOCATION ENG Description` | Location name, not code. |

### `Competition/Result`

Each `Result` row represents one competitor in the pool table.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Rank` | O | Sport-specific | Rank of the competitor in the pool. |
| `RankEqual` | O | `Y` | Send only when the rank is tied/equalled. |
| `ResultType` | O | `SC@ResultType Code` | Type of `Result`; for team pools this is usually points. |
| `Result` | O | Sport-specific | Competitor's pool result, usually standing points. |
| `IRM` | O | `SC@IRM Code` | Invalid result mark, if assigned. |
| `QualificationMark` | O | `SC@QualificationMark Code` | Indicates qualification for the next round. |
| `SortOrder` | M | Positive integer | Unique display/order key for the result row, based on rank with ties broken. |
| `Won` | O | Positive integer | Matches/games won. |
| `Lost` | O | Positive integer | Matches/games lost. |
| `Tied` | O | Positive integer | Matches/games tied. |
| `Played` | O | Positive integer | Matches/games played. |
| `For` | O | Positive integer | Goals/points for the competitor. |
| `Against` | O | Sport-specific | Goals/points against the competitor. |
| `Diff` | O | Sport-specific | Difference between `For` and `Against`. |
| `Ratio` | O | Sport-specific | Ratio value, where the sport uses one. |

Important standing semantics:

- `Result` is the ranking result, usually table points, not the last match score.
- `SortOrder` is mandatory and is the field used to sort rows.
- Tiebreaking is sport-specific. The general interface does not define the tiebreak algorithm.
- `QualificationMark` should be sent when the sport has determined that the competitor qualifies for the next phase.

### `Competition/Result/ExtendedResults/ExtendedResult`

Use this for sport-specific row-level standing details that do not fit the fixed standing attributes.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Type` | O | Sport-specific | Extension group. |
| `Code` | O | Sport-specific | Extension code. |
| `Pos` | O | Sport-specific | Extension position. |
| `Value` | O | Sport-specific | Extension value. |
| `ValueType` | O | Sport-specific | Describes the data type of `Value`. |
| `IRM` | O | `SC@IRM Code` | IRM for the extended result, if applicable. |
| `Rank` | O | Positive integer | Rank of the competitor for this extended result. |
| `RankEqual` | O | `Y` | Send only when the rank is tied/equalled. |
| `SortOrder` | O | Positive integer | Sort order for this extended result. |
| `Diff` | O | Sport-specific | Difference behind the leader for this extended result. |

### `Competition/Result/RecordIndicators/RecordIndicator`

This is available in the general structure, though many team-sport pool standings will not use it.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Order` | M | Positive integer | Hierarchy of record type, using `CC@RECORD_TYPE.Order` as reference. |
| `Code` | M | `CC@RECORD Id` | Record identifier. |
| `RecordType` | M | `CC@RECORD_TYPE RecordType` | Level at which the record is broken. |
| `Equalled` | O | `Y` | Send when the record has been equalled. |

### `Competition/Result/Competitor`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeros | Competitor ID. |
| `Type` | M | `A`, `T` | `A` for athlete, `T` for team. |
| `Organisation` | M | `CC@ORGANISATION Id` | Competitor organisation. |

#### `Description`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `TeamName` | M | `S(73)` | Team name. |
| `IFId` | O | `S(16)` | International Federation team ID, if available. |

#### `Composition/Athlete`

`Composition` is optional. If present, it lists team members or the individual athlete behind the competitor row.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeros | Athlete ID. |
| `Order` | M | Positive integer | Sort order for team members, or `1` when `Competitor/@Type="A"`. |

### `Competition/Result/Competitor/Opponent`

`Opponent` carries details of opposing competitors in the pool. It can be used to expose the pool matrix or schedule/results against each opponent.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeros | Opponent competitor ID. |
| `Type` | M | `A`, `T` | `A` for athlete, `T` for team. |
| `Pos` | M | Positive integer | Opponent's position in the pool table; normally the same as the opponent's `Result/@SortOrder`. |
| `Organisation` | M | `CC@ORGANISATION Id` | Opponent organisation, if available. |
| `Date` | O | `YYYY-MM-DD` | Match date between competitor and opponent. Send if available, even after the match is complete. |
| `Time` | O | `HH:MM` | Match time. Send if available, even after the match is complete. |
| `TimeStamp` | O | DateTime | Scheduled date and time with timezone offset. Send for future and completed matches. |
| `Unit` | O | `CC@EVENT_UNIT Code` | Full RSC of the event unit for the pool item. |
| `HomeAway` | O | `A`, `H` | Opponent home/away indicator. Send `H` if the opponent is the home team, `A` if the opponent is the away team. |
| `Result` | O | `S(50)` | Match result if complete. The result is relative to the competitor row and may be reversed for the other row or for display rules. |

## XML Example

The general PDF section does not provide a clear XML sample. Keep this document as the general interface contract and place sport-specific samples in the relevant Sport Data Dictionary reference.

For the football-specific sample and normalized full-message example, see `2026-05-26-fbl-dt-pool-standing-pages-59-64.md`.

## Message Sort

The attribute used to sort pool standings is `Result/@SortOrder`.

Do not sort solely by `Rank`. `SortOrder` is mandatory and resolves the display order even when ranks are equal or when sport-specific tiebreaking creates an ordering.

## XSD Notes

The related `odf2-schema-30112025-DRAFT` package defines `DT_POOL_STANDING` as a valid `DocumentType` and reuses the generic `resultType` for `Competition/Result`.

Relevant XSD drift and permissiveness:

- `resultType` contains attributes used by many result-like messages, including `Pty`, `WLT`, `StartOrder`, and `PhotoFinish`; not all are part of the `DT_POOL_STANDING` message-values table.
- The PDF message-values table should drive semantic validation for this message. The generic XSD can be more permissive than the message contract.
- The current draft XSD package has an unrelated unresolved `RecordBrokenType` reference, documented in `2026-05-26-gen-xsd-package-30112025-draft.md`.

## Modeling Notes

- Model `DT_POOL_STANDING` as the published state of one group/pool table at a point in time.
- `DT_POOL_STANDING` rows are competitors in the pool, not matches.
- `Result/@Result` is the standing points or sport-specific ranking value. It is not a match score.
- Match scores can appear under `Competitor/Opponent/@Result`, where the value is relative to the competitor row.
- Keep the calculation model separate from the publication model. Sportivo can store tiebreak rules, deductions, head-to-head details, and manual adjustments internally, then publish the net standing values in `Result`.
- `SortOrder` should be persisted or derivable because it is the canonical published ordering.
- `RankEqual="Y"` means the rank is shared; it does not replace `SortOrder`.
- `QualificationMark` is a publication marker. It should be derived from qualification rules once a team is mathematically qualified.
- If Sportivo supports group standings for non-football sports, keep standing columns configurable by sport: football needs draws/goals, basketball usually does not use draws, volleyball may use ratios.
- The general interface does not define the tiebreak algorithm. Resolve it from the sport rules, sport dictionary, or Sportivo competition configuration.

## Code Appendix: Referenced SC and CC Entities

This general interface document references code entities but does not define the concrete values for each sport. Resolve actual values from the relevant OWG2026 Common Codes, General Sport Codes, and sport-specific data dictionary/code release.

| Reference | Used for |
|---|---|
| `CC@COMPETITION_CODE` | `OdfBody/@CompetitionCode` |
| `CC@PHASE` | `OdfBody/@DocumentCode`, phase/pool identity |
| `CC@RESULTSTATUS` | `OdfBody/@ResultStatus` |
| `SCGEN@Source` | `OdfBody/@Source` |
| `CC@EVENT_UNIT` | `Progress/@LastUnit`, `Opponent/@Unit` |
| `CC@DISCIPLINE` | `SportDescription/@DisciplineName` |
| `CC@EVENT` | `SportDescription/@EventName` |
| `CC@DISCIPLINE_GENDER` | `SportDescription/@Gender` |
| `CC@VENUE` | `VenueDescription/@Venue`, `VenueDescription/@VenueName` |
| `CC@LOCATION` | `VenueDescription/@Location`, `VenueDescription/@LocationName` |
| `SC@ResultType` | `Result/@ResultType` |
| `SC@IRM` | `Result/@IRM`, `ExtendedResult/@IRM` |
| `SC@QualificationMark` | `Result/@QualificationMark` |
| `CC@RECORD` | `RecordIndicator/@Code` |
| `CC@RECORD_TYPE` | `RecordIndicator/@RecordType`, record ordering |
| `CC@ORGANISATION` | `Competitor/@Organisation`, `Opponent/@Organisation`, athlete organisation |
| `CC@PERSON_GENDER` | `Athlete/Description/@Gender` |
| `CC@DISCIPLINE_CLASS` | `Athlete/Description/@Class` |

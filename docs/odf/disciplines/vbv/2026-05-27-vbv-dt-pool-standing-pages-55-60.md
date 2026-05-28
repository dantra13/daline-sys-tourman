# ODF VBV Data Dictionary: DT_POOL_STANDING, Pages 55-60

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 55-60.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note restructures the Beach Volleyball `DT_POOL_STANDING` section into a practical reference for pool stage
standings: the pool table per group, the per-row fixture matrix against opponents, set-based tie-break extensions, and
the qualification rule for the phase.

## 2.3.8 Pool Standings

`DT_POOL_STANDING` contains the standings of a group in a competition. The message is triggered at the start of OVR
operations and after each event unit (match).

The report is sent independently for each group/pool of the competition in a particular phase. The group/pool is
identified by the phase-level `DocumentCode`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Unique competition identifier. |
| `DocumentCode` | `CC@Phase` | Full phase-level RSC for the pool. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_POOL_STANDING` | Pool Standings message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `CC@ResultStatus` | Status of the message. Expected statuses: `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| `ResultStatus` | When to send |
|---|---|
| `START_LIST` | Before the start of competition to build the initial pool tables. |
| `INTERMEDIATE` | When an event unit in the corresponding phase finishes. |
| `OFFICIAL` | When the phase finishes and there are no more event units/games to compete. |
| `UNOFFICIAL` | When the last match of the phase is unofficial. |
| Any applicable status | Trigger again after any change. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- ExtendedInfo (0,N)
    |   |   +-- @Type
    |   |   +-- @Code
    |   |   +-- @Pos
    |   |   +-- @Value
    |   +-- Progress (0,1)
    |   |   +-- @LastUnit
    |   |   +-- @UnitsTotal
    |   |   +-- @UnitsComplete
    |   +-- SportDescription (0,1)
    |       +-- @DisciplineName
    |       +-- @EventName
    |       +-- @SubEventName
    |       +-- @Gender
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
        +-- @Played
        +-- @For
        +-- @Against
        +-- @Ratio
        +-- ExtendedResults (0,1)
        |   +-- ExtendedResult (1,N)
        |       +-- @Type
        |       +-- @Code
        |       +-- @Pos
        |       +-- Extension (0,N)
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Description (0,1)
            |   +-- @TeamName
            +-- Opponent (0,N)
                +-- @Code
                +-- @Type
                +-- @Pos
                +-- @Organisation
                +-- @Date
                +-- @Time
                +-- @Unit
                +-- @HomeAway
                +-- @Result
                +-- Description (0,1)
                    +-- @TeamName
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | O | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | O | `S(20)` | Code-set version applicable to the message. |

### `Competition / ExtendedInfos / ExtendedInfo`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `QUAL_RULE` | N/A | Always. | `SC@QualRule` | Code for the qualification rule that advances teams out of the pool. |

### `Competition / ExtendedInfos / Progress`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `LastUnit` | O | `CC@EVENT_UNIT` | Full RSC of the most recently official unit for the pool. |
| `UnitsTotal` | O | `##0` | Total number of units to be played in the pool. |
| `UnitsComplete` | O | `##0` | Total number of official units in the pool. |

### `Competition / ExtendedInfos / SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE` English description | Discipline name, not code. |
| `EventName` | M | `CC@EVENT` English description | Event name, not code. |
| `SubEventName` | M | `CC@PHASE` English short description | Phase name, not code. |
| `Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the event unit. |

### `Result`

At least one `Result` is required for a team awarded a result in the pool.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Rank` | O | Text | Rank in the group. Optional because the team can be disqualified. |
| `RankEqual` | O | `Y` | Sent when the rank is tied; omit otherwise. |
| `ResultType` | M | `SC@ResultType` | Result type: points, or IRM-with-points, obtained across all games of the group. |
| `Result` | O | `#0` | Classification points accrued during the pool stage. Optional before competition. |
| `IRM` | O | `SC@IRM` | Invalid result mark for the group/phase. Sent only when `ResultType` is IRM. |
| `QualificationMark` | O | `SC@QualificationMark` | Qualification indicator. |
| `SortOrder` | M | Positive integer | Presentation order for the group. Mostly rank-based, but also sorts disqualified teams correctly. |
| `Won` | O | `#0` | Number of games won. Do not send if the team has not played. |
| `Lost` | O | `#0` | Number of games lost. Do not send if the team has not played. |
| `Played` | O | `#0` | Number of games played. Do not send if the team has not played. |
| `For` | O | `#0` | Total number of sets won. Do not send if the team has not played. |
| `Against` | O | `#0` | Total number of sets lost. Do not send if the team has not played. |
| `Ratio` | O | `0.000` | Sets ratio to three decimals (format `0.##0`). If losses are zero, value is `MAX`. Do not send if the team has not played. |

### `Result / ExtendedResults / ExtendedResult`

| Type | Code | Pos | Expected When | Meaning |
|---|---|---|---|---|
| `ER` | `SUB_RES` | N/A | When available. | Points-based sub-result with set/point tie-break extensions. |

#### `ExtendedResult / Extension` for `SUB_RES`

| Extension `Code` | `Pos` | Value | Meaning |
|---|---|---|---|
| `WON` | N/A | `##0` | Points for (total points won across the pool stage). |
| `LOST` | N/A | `##0` | Points against (total points lost across the pool stage). |
| `RATIO` | N/A | `0.000` | Points ratio to three decimals (format `0.##0`). If losses are zero, value is `MAX`. |

### `Result / Competitor`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | M | `T` | Team competitor. |
| `Organisation` | M | `CC@ORGANISATION` | Competitor organisation. |

### `Competitor / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `TeamName` | M | `S(73)` | Name of the team. |

### `Competitor / Opponent`

`Opponent` carries the pool matrix: opponents of the competitor in the `Opponent/@Pos` column.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Opponent competitor ID. |
| `Type` | M | `T` | Team competitor. |
| `Pos` | M | `#0` | Opponent column position, `1..n`. Normally the same as that opponent's `Result/@SortOrder`. |
| `Organisation` | M | `CC@ORGANISATION` | Opponent organisation. |
| `Date` | M | Date or `S(3)` | Match date (`YYYY-MM-DD`). Send even after the match is complete. May be `TBD`. |
| `Time` | O | `S(5)` | Match time (`HH:MM`). May be `TBD`. |
| `Unit` | O | `CC@EVENT_UNIT` | Full RSC of the pool item unit. |
| `HomeAway` | O | `H`, `A` | Home/Away indicator for the opponent: `H` if opponent is the home team, `A` if away. |
| `Result` | O | `S(50)` | Completed match result relative to the row competitor (e.g. `2-0`). May be reversed in the opponent's row. |

### `Opponent / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `TeamName` | M | `S(73)` | Name of the opposition team. |

## Tie-Break and Ratio Notes

- `Result/@Ratio` is the sets ratio (sets won versus sets lost) to three decimals; if `Lost` is `0`, the value is the
  string `MAX`.
- `ExtendedResult` with `Code="SUB_RES"` carries the points-based tie-break: `WON` (points for), `LOST` (points
  against), and `RATIO` (points ratio with the same `MAX` rule).
- Both the set-level ratio and the point-level ratio are computed across all pool games of the team.

## Samples from the Dictionary, Normalized

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <ExtendedInfo Type="UI" Code="QUAL_RULE" Value="TOP2"/>
    <Progress LastUnit="VBVMTEAM2-------------GPA-001500--" UnitsTotal="6" UnitsComplete="4"/>
    <SportDescription DisciplineName="Beach Volleyball" EventName="Men's" SubEventName="Pool A" Gender="M"/>
  </ExtendedInfos>
  <Result Rank="1" ResultType="POINTS" Result="5" SortOrder="1"
          Played="2" Won="1" Lost="1" For="3" Against="4" Ratio="0.123">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="SUB_RES">
        <Extension Code="WON" Value="117"/>
        <Extension Code="LOST" Value="107"/>
        <Extension Code="RATIO" Value="1.093"/>
      </ExtendedResult>
    </ExtendedResults>
    <Competitor Code="VBVMTEAM2-----NOR01" Type="T" Organisation="NOR">
      <Description TeamName="SMITH/JONES"/>
      <Opponent Code="VBVMTEAM2-----BRA01" Type="T" Pos="2" Organisation="BRA"
                Date="2016-08-03" Time="14:00" HomeAway="H"
                Unit="VVOMTEAM6-------------GPA-001000--" Result="0-2">
        <Description TeamName="KAPAROV/BARRATT"/>
      </Opponent>
      <Opponent Code="VBVMTEAM2-----CAN01" Type="T" Pos="3" Organisation="CAN"
                Date="2016-08-04" Time="09:00" HomeAway="H"
                Unit="VVOMTEAM6-------------GPA-001200--" Result="2-0">
        <Description TeamName="WHITE/RYAN"/>
      </Opponent>
      <Opponent Code="VBVMTEAM2-----GBR01" Type="T" Pos="4" Organisation="GBR"
                Date="2016-08-01" Time="09:00" HomeAway="A"
                Unit="VVOMTEAM6-------------GPA-001500--" Result="2-0">
        <Description TeamName="GREEN/RYAN"/>
      </Opponent>
    </Competitor>
  </Result>
</Competition>
```

## Message Sort

Sort by `Result/@SortOrder`.

## XSD Validation

The normalized XML example above was validated as a `Competition` fragment wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Model one `DT_POOL_STANDING` message as one version of one pool table. The pool is identified by `DocumentCode`
  (phase-level RSC).
- `Result` rows are team standings; `Opponent` rows are the pool matrix view from that team's perspective.
- `Opponent/@Result` is relative to the row competitor, so the same completed match can be represented differently in
  the opponent's row.
- `QualificationMark` is independent from `Rank`. A rank can exist before qualification is confirmed, and the qualifying
  rule for the phase is carried by `ExtendedInfo Code="QUAL_RULE"`.
- VBV uses two ratios: `Result/@Ratio` for sets and `ExtendedResult SUB_RES / RATIO` for points. Both use the `MAX`
  sentinel when the denominator is zero. Do not collapse them.
- `Result/@For` and `Result/@Against` in VBV are set counts (not point counts). Point totals live under `SUB_RES`
  extensions (`WON` and `LOST`).
- `Opponent/@Pos` typically matches the opponent's `Result/@SortOrder` in the same message; preserve this when rendering
  the pool grid so columns line up across rows.
- `Progress/@LastUnit` reports only officially completed units; `INTERMEDIATE` messages may be sent before that field
  advances.

## Code Appendix: Values Directly Visible in Pages 55-60

The section references several code pages. This appendix records values directly visible in the `DT_POOL_STANDING`
pages and does not attempt to embed large master-data tables.

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@ResultStatus` | `OdfBody/@ResultStatus` | `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` |
| `CC@Phase` | `OdfBody/@DocumentCode` | No concrete values printed; sample `Unit` RSCs use phase token `GPA`. |
| `SC@QualRule` | `ExtendedInfo Code="QUAL_RULE" / @Value` | No concrete values printed in pages 55-60. |
| `SC@ResultType` | `Result/@ResultType` | `POINTS` visible in sample. |
| `SC@IRM` | `Result/@IRM` | No concrete values printed in pages 55-60. |
| `SC@QualificationMark` | `Result/@QualificationMark` | No concrete values printed in pages 55-60. |
| `CC@EVENT_UNIT` | `Progress/@LastUnit`, `Opponent/@Unit` | Sample RSCs such as `VVOMTEAM6-------------GPA-001000--`. |
| `CC@ORGANISATION` | `Competitor/@Organisation`, `Opponent/@Organisation` | `NOR`, `BRA`, `CAN`, `GBR` visible in sample. |
| `CC@DISCIPLINE` | `SportDescription/@DisciplineName` | English description only, not a code value. |
| `CC@EVENT` | `SportDescription/@EventName` | English description only, not a code value. |
| `CC@DISCIPLINE_GENDER` | `SportDescription/@Gender` | No concrete values printed in pages 55-60. |
| `ExtendedResult / Extension` codes | `SUB_RES` tie-break | `WON`, `LOST`, `RATIO` visible. |
| Home/Away token | `Opponent/@HomeAway` | `H`, `A` visible. |

# ODF VBV Data Dictionary: DT_BRACKETS, Pages 61-65

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 61-65.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note restructures the Beach Volleyball `DT_BRACKETS` section into a practical domain reference for the Brackets
message. It covers the bracket-phase-item hierarchy that announces, in advance, how successive event units will be
filled as the competition progresses, and how the eventual winners and losers populate finals and placement matches.

## 2.3.9 Brackets

`DT_BRACKETS` contains the brackets information for one particular event. It is used in events where there is a need to
know in advance how successive event units will be built from the winners/losers (or other competition rules) of
previous event units. In the early stages it indicates how upcoming event units will be filled; later it carries the
actual competitor placements and match results.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Unique competition identifier. |
| `DocumentCode` | `CC@Event` | Full RSC of the event. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_BRACKETS` | Brackets message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `CC@ResultStatus` | Bracket lifecycle status. Expected: `START_LIST` before the competition, `INTERMEDIATE` during the competition, `UNOFFICIAL` when the last match is unofficial, `OFFICIAL` when all matches are official, `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| When | What |
|---|---|
| Before the competition | Initial bracket layout, typically with `ResultStatus="START_LIST"`. |
| After every preliminaries match that determines a position in the bracket | Update with the resolved competitor. |
| After every match during final phases | Update with the match result, winner/loser, and progression. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- Progress (0,1)
    |   |   +-- @LastUnit
    |   |   +-- @UnitsTotal
    |   |   +-- @UnitsComplete
    |   +-- SportDescription (0,1)
    |       +-- @DisciplineName
    |       +-- @EventName
    |       +-- @Gender
    +-- Bracket (1,N)
        +-- @Code
        +-- BracketItems (1,N)
            +-- @Code
            +-- BracketItem (1,N)
                +-- @Code
                +-- @Order
                +-- @Position
                +-- @Date
                +-- @Time
                +-- @TimeStamp
                +-- @Unit
                +-- @Result
                +-- CompetitorPlace (1,N)
                    +-- @Pos
                    +-- @Code
                    +-- @WLT
                    +-- @Result
                    +-- @ResultType
                    +-- @IRM
                    +-- @StrikeOut
                    +-- PreviousUnit (0,1)
                    |   +-- @Unit
                    |   +-- @Value
                    |   +-- @WLT
                    +-- Competitor (0,1)
                        +-- @Code
                        +-- @Type
                        +-- @Organisation
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

### `Competition / ExtendedInfos / Progress`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `LastUnit` | O | `CC@EVENT_UNIT` | Full RSC of the most recently completed unit in the event. |
| `UnitsTotal` | O | Positive integer | Total number of units to be played in the event. |
| `UnitsComplete` | O | `#0` | Total number of units that are official out of `UnitsTotal`. |

### `Competition / ExtendedInfos / SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE` English description | Discipline name, not code. |
| `EventName` | M | `CC@EVENT` English description | Event name, not code. |
| `Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the event. |

### `Competition / Bracket`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `SC@Bracket` | Bracket code (for example, finals or classification games). There is a different code for each bracket based on sport/ORIS presentation. For VBV there is a single `FNL` bracket for head-to-head competitions; placement matches such as bronze sit inside this same bracket. |

### `Competition / Bracket / BracketItems`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `SC@BracketItems` | Code identifying a set of bracket items, that is, the phase. Examples include quarterfinals, semifinals, and finals. |

### `Competition / Bracket / BracketItems / BracketItem`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | O | `#0` | Game number for this bracket item (for example, 17, 18, 19, 20). |
| `Order` | M | Positive integer | Sequential number inside `BracketItems` indicating order; always starts at 1. |
| `Position` | M | `##0` | Drawing position inside the phase. A quarterfinal with four items uses positions 1, 2, 3, 4 from the top. If only three items exist and the fourth would logically be first, positions are 2, 3, 4. For placement matches (bronze or 7_8) whose participants are the losers of semifinals, those matches are included in the originating bracket in the same phase with `Position=2`, while the match with the winner of the semifinal carries `Position=1`. |
| `Date` | O | Date | `YYYY-MM-DD`; must be filled if known. |
| `Time` | O | `S(5)` | `HH:MM`; must be filled if known. |
| `TimeStamp` | O | DateTime | Scheduled date and time of the match/unit including time-zone offset. Send for future and completed matches. |
| `Unit` | O | `CC@EVENT_UNIT` | Full RSC of the unit for this bracket item. |
| `Result` | O | `S(50)` | Result of the match if complete, formatted as in ORIS (including IRM if necessary). Must be included if the data is available and the match is complete. |

### `BracketItem / CompetitorPlace`

If the competitors are known, this element places them in the bracket. If they are not yet known, it carries information
about the rule to access this bracket item (for example, the previous unit and W/L role).

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Pos` | M | Positive integer | Sequential number placing the competitors in the bracket (1 or 2). |
| `Code` | O | `SC@CompetitorPlace` | Sent when there is no competitor team (`BYE`) or when the competitor is not yet known (`TBD`). |
| `WLT` | O | `SC@WLT` | `W` or `L`; the winner or loser of the bracket item. Always sent when known. |
| `Result` | O | `S(10)` | Result (score) of this competitor in the event unit. |
| `ResultType` | O | `SC@ResultType` | Type of the `Result` attribute. |
| `IRM` | O | `SC@IRM` | Invalid rank mark, if applicable. |
| `StrikeOut` | O | `Y` | Send `Y` if the competitor should be struck out in this bracket item; usually only used for `DQB`. |

### `CompetitorPlace / PreviousUnit`

Previous event unit related to the `CompetitorPlace@Pos` competitor of the current bracket item. Always informed except
for bracket items whose `CompetitorPlace@Pos` competitor has no preceding event unit in the bracket graph, unless coming
from a pool.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Unit` | O | `CC@EVENT_UNIT` or RSC of Pool/Combined | Full RSC of the previous event unit. Must be sent if a winner/loser from a single unit. If from a pool, this is the RSC of the pool. |
| `Value` | O | `SC@Pool` or `S(6)` | If the competitor in the current unit is unknown because they come from a pool or previous matches, fill with the pool code or the match number as appropriate. May be redefined by sport. |
| `WLT` | O | `SC@WLT` | Send `W` or `L` for the winner or loser of the previous match (if not from a pool). Do not send if the participant is unknown from a pool. |

### `CompetitorPlace / Competitor`

Include only if the competitor is known.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | M | `T` | `T` for team. |
| `Organisation` | O | `CC@ORGANISATION` | Competitor's organisation, if known. |

### `Competitor / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `TeamName` | M | `S(73)` | Team name. |

## Bracket Semantics for VBV

- One bracket per head-to-head Beach Volleyball event, identified by `Bracket/@Code="FNL"` per the overview.
- Bracket phases populate `BracketItems/@Code` (semifinals, finals, and so on); the dictionary samples show `SFL` for the
  semifinal phase.
- The bronze medal match is included inside the `FNL` bracket in the final phase after the gold medal match with
  `BracketItem/@Position=2`. The gold medal match uses `Position=1` in the same phase. This means a finals phase can
  carry two `BracketItem` rows: gold at position 1 and bronze at position 2.
- For placement matches more generally (for example, 7_8), the losers' bracket items sit in the same phase as the
  winners' bracket items, distinguished by `Position`.
- Within a `BracketItem`, the two `CompetitorPlace` rows use `Pos=1` and `Pos=2`. The `PreviousUnit` element on each
  `CompetitorPlace` describes the feeder unit and whether this slot is the `W` or `L` of that unit, or the team coming
  out of a pool with code in `Value`.
- When competitors are not yet known, omit `Competitor` and rely on `CompetitorPlace/@Code` (`BYE`, `TBD`) and
  `PreviousUnit` to describe the slot.

## Samples from the Dictionary, Normalized

### Finals phase with two known semifinal competitors

This sample mirrors the dictionary sample. The bronze medal match would appear in the same phase with `Position=2` once
in scope.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <Bracket Code="FNL">
    <BracketItems Code="SFL">
      <BracketItem Code="33" Order="1" Position="1"
                   Date="2012-08-10" Time="15:00"
                   Unit="VBVWTEAM2-------------SFNL000100--">
        <CompetitorPlace Pos="1">
          <Competitor Code="VBVWTEAM2-----NED01" Type="T" Organisation="NED">
            <Description TeamName="Smith/Jones"/>
          </Competitor>
        </CompetitorPlace>
        <CompetitorPlace Pos="2">
          <Competitor Code="VBVWTEAM2-----NZL01" Type="T" Organisation="NZL">
            <Description TeamName="Black/White"/>
          </Competitor>
        </CompetitorPlace>
      </BracketItem>
    </BracketItems>
  </Bracket>
</Competition>
```

### Unknown competitor slot fed by a previous semifinal

When a slot is not yet known but the feeder rule is, omit `Competitor` and use `PreviousUnit` to describe the dependency.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <Bracket Code="FNL">
    <BracketItems Code="FNL">
      <BracketItem Order="1" Position="1"
                   Unit="VBVWTEAM2-------------FNL-000100--">
        <CompetitorPlace Pos="1" Code="TBD">
          <PreviousUnit Unit="VBVWTEAM2-------------SFNL000100--" WLT="W"/>
        </CompetitorPlace>
        <CompetitorPlace Pos="2" Code="TBD">
          <PreviousUnit Unit="VBVWTEAM2-------------SFNL000200--" WLT="W"/>
        </CompetitorPlace>
      </BracketItem>
      <BracketItem Order="2" Position="2"
                   Unit="VBVWTEAM2-------------BMD-000100--">
        <CompetitorPlace Pos="1" Code="TBD">
          <PreviousUnit Unit="VBVWTEAM2-------------SFNL000100--" WLT="L"/>
        </CompetitorPlace>
        <CompetitorPlace Pos="2" Code="TBD">
          <PreviousUnit Unit="VBVWTEAM2-------------SFNL000200--" WLT="L"/>
        </CompetitorPlace>
      </BracketItem>
    </BracketItems>
  </Bracket>
</Competition>
```

### Pool-fed bracket slot

When a slot comes from a pool, `PreviousUnit/@Unit` is the pool RSC and `Value` carries the pool code or match number.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <Bracket Code="FNL">
    <BracketItems Code="QFL">
      <BracketItem Order="1" Position="1"
                   Unit="VBVWTEAM2-------------QFNL000100--">
        <CompetitorPlace Pos="1" Code="TBD">
          <PreviousUnit Unit="VBVWTEAM2-------------RPLA000100--" Value="A"/>
        </CompetitorPlace>
        <CompetitorPlace Pos="2" Code="TBD">
          <PreviousUnit Unit="VBVWTEAM2-------------RPLB000100--" Value="B"/>
        </CompetitorPlace>
      </BracketItem>
    </BracketItems>
  </Bracket>
</Competition>
```

## Message Sort

- First by `Bracket/@Code` using order in sport codes.
- Then by `Bracket/BracketItems/@Code` descending, using order in sport codes (later phases such as finals tend to come
  first when reading the message; rely on the sport-code ordering rather than alphabetic).
- Then by `Bracket/BracketItems/BracketItem/@Position`.

## XSD Validation

The normalized XML examples above were validated as `Competition` fragments wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Treat `DT_BRACKETS` as the authoritative progression graph for an event. It carries both schedule-like dependency
  information (`PreviousUnit`) and resolved-state information (`Competitor`, `WLT`, `Result`).
- Model `Bracket -> BracketItems -> BracketItem -> CompetitorPlace` as a four-level tree. `CompetitorPlace` is always a
  pair (`Pos=1`, `Pos=2`) for a head-to-head match in VBV.
- Anchor each `BracketItem` to a unit RSC via `@Unit`. This is the join key to `DT_RESULT`, `DT_SCHEDULE`, and
  `DT_PLAY_BY_PLAY`.
- Resolve competitors from `PreviousUnit` edges. A competitor in `BracketItem A`'s `CompetitorPlace@Pos=p` is the winner
  or loser (per `PreviousUnit/@WLT`) of `PreviousUnit/@Unit`, or the team coming out of a pool keyed by
  `PreviousUnit/@Value`.
- Persist the special VBV rule that the bronze medal match sits in the final phase of the `FNL` bracket with
  `Position=2`. Ranking and medal logic depends on locating the gold (`Position=1`) and bronze (`Position=2`) matches in
  this same phase.
- `BracketItem/@Result` is a denormalized display result. Use `DT_RESULT` for authoritative set scores and statistics.
- Sparse slots are normal in early versions of the message: `Competitor` may be absent while `PreviousUnit` and a
  `CompetitorPlace/@Code` of `TBD` or `BYE` carry the slot's meaning.

## Code Appendix: Values Directly Visible in Pages 61-65

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@ResultStatus` | `OdfBody/@ResultStatus` | `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` |
| `SC@Bracket` | `Bracket/@Code` | `FNL` visible in sample (and in the VBV overview as the single bracket code). |
| `SC@BracketItems` | `BracketItems/@Code` | `SFL` visible in sample (semifinal phase). |
| `SC@CompetitorPlace` | `CompetitorPlace/@Code` | `BYE`, `TBD` mentioned in the description (no competitor team and competitor not yet known). |
| `SC@WLT` | `CompetitorPlace/@WLT`, `PreviousUnit/@WLT` | `W`, `L` referenced in descriptions. |
| `SC@ResultType` | `CompetitorPlace/@ResultType` | No concrete values printed in pages 61-65. |
| `SC@IRM` | `CompetitorPlace/@IRM` | `DQB` referenced in the description of `StrikeOut`. |
| `SC@Pool` | `PreviousUnit/@Value` | No concrete values printed in pages 61-65. |
| `CC@EVENT_UNIT` | `Progress/@LastUnit`, `BracketItem/@Unit`, `PreviousUnit/@Unit` | Unit RSC such as `VBVWTEAM2-------------SFNL000100--` visible in sample. |
| `CC@ORGANISATION` | `Competitor/@Organisation` | `NED`, `NZL` visible in sample. |
| `CC@DISCIPLINE` | `SportDescription/@DisciplineName` | No concrete values printed in pages 61-65. |
| `CC@EVENT` | `SportDescription/@EventName`, `OdfBody/@DocumentCode` | No concrete values printed in pages 61-65. |
| `CC@DISCIPLINE_GENDER` | `SportDescription/@Gender` | No concrete values printed in pages 61-65. |
| `SCGEN@Source` | `OdfBody/@Source` | No concrete values printed in pages 61-65. |

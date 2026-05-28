# ODF BKB Data Dictionary: DT_POOL_STANDING, Pages 64-69

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 64-69.
Source version: `SOG-2024-BKB-3.4 APP`, dated 19 January 2024.

This note restructures section `2.3.8 Pool Standings` of the Paris 2024 Basketball Data Dictionary into a basketball-specific reference for Sportivo domain modeling. It should be read alongside the football pool-standings reference (`2026-05-26-fbl-dt-pool-standing-pages-59-64.md`), which has the same `DocumentType` and a very similar shape.

## 2.3.8 Pool Standings

`DT_POOL_STANDING` carries the standings of one basketball group/pool in a competition. It publishes the group table for the pool stage.

The message is similar to `DT_PHASE_RESULT`, but differs in trigger and frequency. For BKB it is sent at the start of OVR operations and then after each event unit, meaning after every match that affects the pool. It is sent independently per pool. The pool is identified by the message header `DocumentCode`, which is the full phase-level RSC, for example `BKBMTEAM5-------------GPA---------`. The dictionary also allows `DocumentCode` to carry the phase RSC when a combined ranking across pools is published.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC @Competition` | Unique competition ID. |
| `DocumentCode` | `CC @Phase` | Full RSC for the pool/group, or the phase RSC when a combined ranking is published. |
| `DocumentSubcode` | `N/A` | Not used for BKB `DT_POOL_STANDING`. |
| `DocumentType` | `DT_POOL_STANDING` | Pool standings message. |
| `DocumentSubtype` | `N/A` | Not used for BKB `DT_POOL_STANDING`. |
| `Version` | `1..V` | Ascending version number associated with the message content. |
| `ResultStatus` | `CC @ResultStatus` | Status of the standings. Expected: `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag` | `P` or `T` | Production or test message. |
| `Date` | Date | Generation date in the local time zone where the message was produced. |
| `Time` | Time | Generation time up to milliseconds in the local time zone where the message was produced. |
| `LogicalDate` | Date | Logical date of events. Same as the physical day unless a unit or transmission extends past midnight. |
| `Source` | `SC @Source` | Code of the system that generated the message. |

## Trigger and Frequency

| Condition | Expected `ResultStatus` | Meaning |
|---|---|---|
| Before the competition starts | `START_LIST` | Initial pool tables. |
| When a match of the phase finishes, without waiting for official confirmation | `INTERMEDIATE` | Updated table after the match affecting the pool. |
| If the last match in the phase is unofficial | `UNOFFICIAL` | Table based on an unofficial last match. |
| When the phase finishes and all matches are official | `OFFICIAL` | Final official group table. |
| Pending decision affects the table | `PROVISIONAL` | Table is published but subject to decision. |
| Any change to the published table | Depends on content | The dictionary explicitly says the trigger also applies after any change. |

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
        +-- @Diff
        +-- @Ratio
        +-- ExtendedResults (0,1)
        |   +-- ExtendedResult (1,N)
        |       +-- @Type
        |       +-- @Code
        |       +-- @Pos
        |       +-- @Value
        |       +-- Extension (0,N)
        |           +-- @Code
        |           +-- @Pos
        |           +-- @Value
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
|---|---:|---|---|
| `Gen` | O | `S(20)` | Version of the General Data Dictionary applicable to the message. |
| `Sport` | O | `S(20)` | Version of the Sport Data Dictionary applicable to the message. |
| `Codes` | O | `S(20)` | Version of the codes applicable to the message. |

### `Competition/ExtendedInfos/ExtendedInfo`

Basketball uses this node for the pool qualification rule.

| Type | Code | Pos | Expected when | Value |
|---|---|---|---|---|
| `UI` | `QUAL_RULE` | `N/A` | Always | `SC @QualRule` code for the qualification rule. |

For Paris 2024 the only published rule code is `PT8^QF`: at the end of the group phase the top eight teams from the combined ranking qualify to the quarterfinals.

### `Competition/ExtendedInfos/Progress`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `LastUnit` | O | `CC @Unit` | Full RSC of the most recent official unit for the pool in this message. |
| `UnitsTotal` | O | Numeric `##0` | Total number of units/matches to be played in the pool. |
| `UnitsComplete` | O | Numeric `##0` | Total number of official units/matches in the pool. |

### `Competition/ExtendedInfos/SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `DisciplineName` | M | `S(40)` | Discipline English description from Common Codes, not code. |
| `EventName` | M | `S(40)` | Event English description from Common Codes, not code. |
| `SubEventName` | M | `S(40)` | Phase English description from Common Codes. Only include if a single phase. |
| `Gender` | M | `CC @SportGender` | Gender code for the event unit. |

### `Competition/Result`

Each `Result` row is one team in the pool table. At least one `Result` must be present.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Rank` | O | Text | Rank in the group. Optional because a team may be disqualified. |
| `RankEqual` | O | `S(1)` | Send `Y` when the rank is equal; otherwise do not send. |
| `ResultType` | M | `SC @ResultType` | Type of the `Result` attribute, either points or IRM with points obtained across the group. |
| `Result` | O | Numeric | Classification points accrued during the pool stage. Optional before the competition. |
| `IRM` | O | `SC @IRM` | Invalid result mark. Send only when applicable. |
| `QualificationMark` | O | `SC @QualificationMark` | Qualified indicator for groups, individual or overall. |
| `SortOrder` | M | Numeric | Sequential display order for the group. Mostly based on rank, but it must also sort disqualified teams. |
| `Won` | O | Numeric `#0` | Games won. Do not send if the team has not played. |
| `Lost` | O | Numeric `#0` | Games lost. Do not send if the team has not played. |
| `Played` | O | Numeric `#0` | Games played. Do not send if the team has not played. |
| `For` | O | Numeric `##0` | Total points scored by the team. Do not send if the team has not played. |
| `Against` | O | Numeric `##0` | Total points conceded by the team. Do not send if the team has not played. |
| `Diff` | O | Numeric `##0` or `-##0` | Points difference between `For` and `Against`. Do not send if the team has not played. |
| `Ratio` | O | Numeric `##0` | Winning percentage for the team. Do not send if the team has not played. |

Important basketball semantics:

- `Result` is classification (table) points, not the basket score sum.
- `ResultType="POINTS"` means the `Result` value is interpreted as classification points in this standing context.
- `For`, `Against`, and `Diff` are basket points (sums of points scored and conceded across the group).
- `Ratio` is the winning percentage, typically calculated as `Won / Played * 100`.
- The BKB message table does not list a `Tied` attribute (basketball does not allow ties), although the official sample shows `Tied="0"`. Treat `Tied` as not dictionary-defined for BKB and prefer not relying on it for modeling.

### `Competition/Result/ExtendedResults/ExtendedResult`

Basketball defines two row-level extended results.

| Type | Code | Pos | Expected when | Value |
|---|---|---|---|---|
| `ER` | `STREAK` | Numeric `0` (`1` = games won, `2` = games lost) | If available, for not disqualified teams | Numeric `#0`: number of games won or lost in a row. |
| `ER` | `SUB_RANK` | `N/A` | If available in the overall pool standings | `SC @PoolRank` code, for example `A1`. |

#### `STREAK` sub-element: `Extension`

| Attribute | Value | Meaning |
|---|---|---|
| `Code` | `DETAIL` | Detail entry of the streak. |
| `Pos` | Numeric `0` | `1..n` indicating the position in the last `n` games. |
| `Value` | `S(1)` | `W` if the game was won, `L` if the game was lost. |

`SUB_RANK` carries the combined ranking position of the team across pools when the standings include an overall pool ranking, encoded with the `SC @PoolRank` catalog.

### `Competition/Result/Competitor`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | M | `S(1)` | `T` for team. |
| `Organisation` | M | `CC @Organisation` | Competitor organisation. |

#### `Description`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `TeamName` | M | `S(73)` | Team name. |

### `Competition/Result/Competitor/Opponent`

`Opponent` carries each opponent in the pool, allowing consumers to build a fixture/result matrix for the group.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Opponent competitor ID. |
| `Type` | M | `S(1)` | `T` for team. |
| `Pos` | M | Numeric `#0` | Opponent column position in the pool. Normally the same as that opponent's `Result/@SortOrder`. |
| `Organisation` | M | `CC @Organisation` | Opponent organisation code. Must be included if available. |
| `Date` | M | Date | Match date (`YYYY-MM-DD`). Must be included if available, even after completion. |
| `Time` | O | `S(5)` | Match time, for example `HH:MM`. Must be included if available. |
| `Unit` | O | `CC @Unit` | Full RSC of the unit/match for the pool item. |
| `HomeAway` | O | `S(1)` | `H` if the opponent is the home team, `A` if the opponent is the away team. |
| `Result` | O | `S(50)` | Match result if complete, formatted as in ORIS (separator and order, for example `85-82`). Order is relative to the current competitor row and may be reversed in the opponent row or by home/away display rules. Send `null` if the match is nullified. |

#### `Opponent/Description`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `TeamName` | M | `S(73)` | Opponent team name. |

## Sample from the Dictionary, Normalized

The PDF provides a partial `Result` example. It represents one row of a pool table for Egypt, including three opponents and a `STREAK` extended result.

```xml
<Result Rank="3"
        ResultType="POINTS"
        Result="1"
        SortOrder="3"
        Played="2"
        Won="1"
        Tied="0"
        Lost="1"
        For="167"
        Against="156"
        Diff="11"
        Ratio="50">
  <ExtendedResults>
    <ExtendedResult Type="ER" Code="STREAK" Pos="1" Value="1">
      <Extension Code="DETAIL" Pos="1" Value="L"/>
      <Extension Code="DETAIL" Pos="2" Value="W"/>
    </ExtendedResult>
  </ExtendedResults>
  <Competitor Code="BKBMTEAM5-----EGY01" Type="T" Organisation="EGY">
    <Description TeamName="Egypt"/>
    <Opponent Code="BKBMTEAM5-----BRA01"
              Type="T"
              Pos="1"
              Organisation="BRA"
              Date="2012-07-27"
              Time="14:00"
              Unit="BKBMTEAM5-------------GPA-000200--"
              HomeAway="H"
              Result="82:80">
      <Description TeamName="Brazil"/>
    </Opponent>
    <Opponent Code="BKBMTEAM5-----BLR01"
              Type="T"
              Pos="2"
              Organisation="BLR"
              Date="2012-08-01"
              Time="09:00"
              Unit="BKBMTEAM5-------------GPA-000400--"
              HomeAway="A">
      <Description TeamName="Belarus"/>
    </Opponent>
    <Opponent Code="BKBMTEAM5-----NZL01"
              Type="T"
              Pos="4"
              Organisation="NZL"
              Date="2012-07-29"
              Time="09:00"
              Unit="BKBMTEAM5-------------GPA-000500--"
              HomeAway="A"
              Result="91:103">
      <Description TeamName="New Zealand"/>
    </Opponent>
  </Competitor>
</Result>
```

Notes about the sample:

- The dictionary sample uses a colon separator (`82:80`) for `Opponent/@Result`. The `Opponent/@Result` description says the format follows ORIS (separator and order). The basketball `DT_RESULT` typically uses `-`; this is a sample-only convention.
- `Tied="0"` appears in the sample but is not part of the BKB `Result` attribute table.
- `STREAK Pos="1"` means the sample team is currently on a one-game won streak. The `Extension/DETAIL` entries describe the most recent games: position 1 = most recent game (here `L`), position 2 = next previous game (here `W`).

### Sample Reading

| Field | Meaning |
|---|---|
| `Rank="3"`, `SortOrder="3"` | Egypt is third in the pool table. |
| `ResultType="POINTS"`, `Result="1"` | Egypt has 1 classification point in the pool. |
| `Played="2"`, `Won="1"`, `Lost="1"` | Two games played, one won and one lost. |
| `For="167"`, `Against="156"`, `Diff="11"` | Basket points scored, conceded, and net difference. |
| `Ratio="50"` | Winning percentage 50% (1 of 2 played). |
| `Opponent vs Brazil` `Result="82:80"` | Egypt won against Brazil 82-80, displayed in Egypt's row. |
| `Opponent vs Belarus` has no `Result` | The Belarus match is in the pool matrix but not yet complete in this sample. |

## Normalized Full-Message Example

The dictionary sample is partial. The following example wraps the same shape in a compact full `OdfBody` message. Concrete versions, competition IDs, and code values must come from the target event release. The example is not validated against an XSD because none was supplied for this discipline.

```xml
<OdfBody CompetitionCode="OG2024"
         DocumentCode="BKBMTEAM5-------------GPA---------"
         DocumentType="DT_POOL_STANDING"
         Version="5"
         ResultStatus="INTERMEDIATE"
         FeedFlag="T"
         Date="2024-07-29"
         Time="22:15:00"
         LogicalDate="2024-07-29"
         Source="BCYBKB1">
  <Competition Gen="SOG-2024-GEN" Sport="SOG-2024-BKB-3.4" Codes="SOG-2024-CODES">
    <ExtendedInfos>
      <ExtendedInfo Type="UI" Code="QUAL_RULE" Value="PT8^QF"/>
      <Progress LastUnit="BKBMTEAM5-------------GPA-000500--"
                UnitsTotal="6"
                UnitsComplete="4"/>
      <SportDescription DisciplineName="Basketball"
                        EventName="Men's Basketball"
                        SubEventName="Men's Group A"
                        Gender="M"/>
    </ExtendedInfos>

    <Result Rank="1"
            ResultType="POINTS"
            Result="4"
            SortOrder="1"
            Played="2"
            Won="2"
            Lost="0"
            For="180"
            Against="140"
            Diff="40"
            Ratio="100"
            QualificationMark="Q">
      <ExtendedResults>
        <ExtendedResult Type="ER" Code="STREAK" Pos="1" Value="2">
          <Extension Code="DETAIL" Pos="1" Value="W"/>
          <Extension Code="DETAIL" Pos="2" Value="W"/>
        </ExtendedResult>
        <ExtendedResult Type="ER" Code="SUB_RANK" Value="A1"/>
      </ExtendedResults>
      <Competitor Code="BKBMTEAM5-----BRA01" Type="T" Organisation="BRA">
        <Description TeamName="Brazil"/>
      </Competitor>
    </Result>

    <Result Rank="3"
            ResultType="POINTS"
            Result="1"
            SortOrder="3"
            Played="2"
            Won="1"
            Lost="1"
            For="167"
            Against="156"
            Diff="11"
            Ratio="50">
      <ExtendedResults>
        <ExtendedResult Type="ER" Code="STREAK" Pos="1" Value="1">
          <Extension Code="DETAIL" Pos="1" Value="L"/>
          <Extension Code="DETAIL" Pos="2" Value="W"/>
        </ExtendedResult>
      </ExtendedResults>
      <Competitor Code="BKBMTEAM5-----EGY01" Type="T" Organisation="EGY">
        <Description TeamName="Egypt"/>
        <Opponent Code="BKBMTEAM5-----BRA01"
                  Type="T"
                  Pos="1"
                  Organisation="BRA"
                  Date="2024-07-27"
                  Time="14:00"
                  Unit="BKBMTEAM5-------------GPA-000200--"
                  HomeAway="H"
                  Result="82-80">
          <Description TeamName="Brazil"/>
        </Opponent>
        <Opponent Code="BKBMTEAM5-----BLR01"
                  Type="T"
                  Pos="2"
                  Organisation="BLR"
                  Date="2024-08-01"
                  Time="09:00"
                  Unit="BKBMTEAM5-------------GPA-000400--"
                  HomeAway="A">
          <Description TeamName="Belarus"/>
        </Opponent>
        <Opponent Code="BKBMTEAM5-----NZL01"
                  Type="T"
                  Pos="4"
                  Organisation="NZL"
                  Date="2024-07-29"
                  Time="09:00"
                  Unit="BKBMTEAM5-------------GPA-000500--"
                  HomeAway="A"
                  Result="91-103">
          <Description TeamName="New Zealand"/>
        </Opponent>
      </Competitor>
    </Result>
  </Competition>
</OdfBody>
```

Validation caveat: this XML example is not validated against an XSD because no schema was supplied for this discipline.

## Message Sort

The attribute used to sort basketball pool standings is `Result/@SortOrder`.

`SortOrder` is mandatory. It is mostly based on rank, but it must also handle cases such as disqualified teams that may not have a meaningful `Rank` value.

## Comparison with the FBL Reference

The BKB structure follows the same shell as the football version, with these basketball-specific differences:

| Area | FBL 2024 | BKB 2024 |
|---|---|---|
| `Result/@Ratio` | Not part of the FBL structure. | Present as winning percentage. |
| `Result/@Tied` | Listed in the FBL `Result` table. | Not listed in the BKB `Result` table (sample contains it, but the dictionary does not). |
| `Result/@For`/`@Against` | Goals for/against (typed `#0`). | Basket points for/against (typed `##0`). |
| `Diff` formatting | `+#0` or `-#0` (signed). | `##0` or `-##0` (no explicit `+` prefix). |
| `ExtendedResult` codes | `FPP` (fair play points). | `STREAK` (with `DETAIL` extensions) and `SUB_RANK` (using `SC @PoolRank`). |
| `QUAL_RULE` values | Generic top-N catalog. | `PT8^QF` only: top 8 teams from combined ranking qualify to quarterfinals. |
| Sample `Opponent/@Result` separator | `2-0`. | `82:80` (colon in sample, ORIS-formatted in production). |

## Modeling Notes

- Model one `DT_POOL_STANDING` message as one published version of one basketball pool table.
- The pool key is the phase RSC in `DocumentCode`, for example `BKBMTEAM5-------------GPA---------`. The dictionary also allows the same `DocumentCode` to publish a combined ranking across pools at the phase level.
- `DocumentSubcode` and `DocumentSubtype` are not used for BKB `DT_POOL_STANDING`.
- `Result` rows are teams. Matches live as related facts and may appear as `Opponent` entries.
- Classification points (`Result/@Result`) must be modeled separately from basket points (`For`, `Against`, `Diff`). In basketball pool standings, the "points" attribute is classification points; the basket numbers measure scoring.
- `Ratio` should be stored as the published winning percentage, not derived ad hoc at read time, because the source of truth is the published value.
- `SortOrder` is the canonical published row order. Use it for presentation rather than re-sorting by `Rank`.
- Tie-break rules are not encoded in the message. The dictionary only signals qualification via `QualificationMark` and shows the combined ranking via `SUB_RANK`. The pool format itself is conveyed by `QUAL_RULE`.
- Model `STREAK` as a derived row attribute with two parts: streak length plus direction (`Pos=1` won, `Pos=2` lost), and an ordered list of recent results (`Extension Code="DETAIL"` with `W`/`L` and a position).
- Model `SUB_RANK` as the team's position in the combined cross-pool ranking, encoded as an `SC @PoolRank` value such as `A1`.
- Do not emit played/stat columns before a team has played if following the BKB dictionary strictly.
- The BKB dictionary does not declare a `Tied` attribute. Basketball does not allow ties in practice; do not introduce a `Tied` field in the BKB pool-standing model.
- `Opponent/@Result` should be persisted as an opaque ORIS-formatted string; the BKB official separator should match the basketball `DT_RESULT` convention (typically `-`). Treat the dictionary sample's colon as informational only.

## Code Appendix: Referenced SC and CC Entities

PDF-embedded source links plus the catalog row counts retrieved from the Paris 2024 code pages.

| Reference | Used for | Source | Embedded |
|---|---|---|---|
| `CC @Competition` | `OdfBody/@CompetitionCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm` | Linked only (huge master table). |
| `CC @Phase` | `OdfBody/@DocumentCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Phase.htm` | Linked only (huge master table). |
| `CC @ResultStatus` | `OdfBody/@ResultStatus` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm` | Embedded. |
| `SC @Source` | `OdfBody/@Source` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm` | Embedded. |
| `CC @Unit` | `Progress/@LastUnit`, `Opponent/@Unit` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm` | Linked only (huge master table). |
| `CC @SportGender` | `SportDescription/@Gender` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm` | Embedded. |
| `SC @QualRule` | `ExtendedInfo Type="UI" Code="QUAL_RULE"` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_QualRule_SOG_BKB.htm` | Embedded. |
| `SC @ResultType` | `Result/@ResultType` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_BKB.htm` | Embedded. |
| `SC @IRM` | `Result/@IRM` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_BKB.htm` | Embedded. |
| `SC @QualificationMark` | `Result/@QualificationMark` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_QualificationMark_SOG_BKB.htm` | Embedded. |
| `SC @PoolRank` | `ExtendedResult Code="SUB_RANK" Value` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_PoolRank_SOG_BKB.htm` | Embedded. |
| `CC @Organisation` | `Competitor/@Organisation`, `Opponent/@Organisation` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm` | Linked only (huge master table, hundreds of NOC rows). |

### `CC @ResultStatus`

| Code | Order | ENG Description | Note |
|---|---|---|---|
| `START_LIST` | 1 | Start List | Before competition, Start List. |
| `LIVE` | 2 | Live | For live updates during competition. |
| `INTERMEDIATE` | 3 | Intermediate | When competition is stopped, used at pre-defined points. |
| `UNCONFIRMED` | 4 | Unconfirmed | When the unit is over but not yet unofficial or official. |
| `UNOFFICIAL` | 5 | Unofficial | Results released as soon as the event is over, not waiting any official decision. Correctness must be assured. |
| `OFFICIAL` | 6 | Official | Results released after official confirmation including protests, etc. |
| `PARTIAL` | 7 | Partial | Incomplete list, Final Ranking. Used in PDF. |
| `PROTESTED` | 8 | Protested | After the competition is no longer LIVE and a protest has been lodged. |
| `PROVISIONAL` | 9 | Provisional | Special situations. |

The BKB dictionary only lists `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, and `PROVISIONAL` as expected for this message.

### `CC @SportGender`

| Id | Description |
|---|---|
| `-` | Global |
| `M` | Men |
| `O` | Open |
| `W` | Women |
| `X` | Mixed |

### `SC @Source` (BKB)

| Code | ENG Description |
|---|---|
| `BCYBKB1` | Origin for messages from OVR at BCY for BKB. |
| `LILBKB1` | Origin for messages from OVR at LIL for BKB. |

`BCY` and `LIL` correspond to the Paris 2024 basketball venues (Bercy Arena and Pierre Mauroy Stadium in Villeneuve-d'Ascq).

### `SC @QualRule` (BKB)

| Code | ENG Description |
|---|---|
| `PT8^QF` | At the end of the group phase the top 8 teams from the combined ranking qualify to the Quarterfinals. |

### `SC @ResultType` (BKB)

| Code | ENG Description |
|---|---|
| `IRM` | Invalid Result Mark. |
| `IRM_POINTS` | For both, points and invalid result mark. |
| `POINTS` | Points. |

In `DT_POOL_STANDING`, `POINTS` is interpreted as classification (table) points.

### `SC @IRM` (BKB)

| Code | ENG Description |
|---|---|
| `DNS` | Did Not Start. |
| `DQB` | Disqualified for unsportsmanlike behaviour. |
| `DSQ` | Disqualified. |
| `WDR` | Withdrawn. |

### `SC @QualificationMark` (BKB)

| Code | ENG Description |
|---|---|
| `Q` | Qualified. |

### `SC @PoolRank` (BKB)

| Code | Order | ENG Description |
|---|---|---|
| `A1` | 1 | 1st in Pool A. |
| `A2` | 2 | 2nd in Pool A. |
| `A3` | 3 | 3rd on Pool A. |
| `A4` | 4 | 4th in Pool A. |
| `B1` | 5 | 1st in Pool B. |
| `B2` | 6 | 2nd in Pool B. |
| `B3` | 7 | 3rd in Pool B. |
| `B4` | 8 | 4th in Pool B. |
| `C1` | 9 | 1st in Pool C. |
| `C2` | 10 | 2nd in Pool C. |
| `C3` | 11 | 3rd in Pool C. |
| `C4` | 12 | 4th in Pool C. |

The Paris 2024 BKB structure uses three pools of four teams each, hence twelve `PoolRank` codes. The `A3` ENG description in the catalog is published as "3rd on Pool A" (sic).

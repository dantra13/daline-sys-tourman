# ODF BOX Data Dictionary: DT_STATS, Pages 44-48

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BOX_Data_Dictionary.pdf`, pages 44-48.

Source version: `SCOG/SYOG-2026-BOX-1.2 SFR`, dated 13 May 2026.

This note restructures the Boxing `DT_STATS` section into a practical reference for tournament-level bout statistics
and team-ranking statistics at the discipline level.

## 2.3.6 Statistics

`DT_STATS` contains a list of statistics for a competitor, which can be either an individual athlete or a team, at the
scope identified by `OdfBody/@DocumentCode`.

The dictionary states that each table with multiple statistics is sent as a separate message, identified by the
header's `DocumentSubtype` and `DocumentSubcode`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@DisciplineGender`, `CC@Discipline`, or `CC@Event` | Full RSC at the appropriate level for the subtype. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_STATS` | Statistics message. |
| `DocumentSubtype` | `TOU`, `TEAM_RANKING` | Selects tournament or team-ranking statistics table. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `CC@ResultStatus` | Result lifecycle status. The section lists `LIVE`, `INTERMEDIATE`, `OFFICIAL`, and `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | `P` production, `T` test. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

`ResultStatus` semantics from the section:

| Status | Meaning |
|---|---|
| `LIVE` | Used during the competition when nothing else applies. |
| `INTERMEDIATE` | Used after the competition has started and is not finished but not currently live, typically between units. |
| `OFFICIAL` | After the last unit which affects the statistics is official. |
| `PROVISIONAL` | Listed as an allowed header status. |

## Document Subtypes

| Subtype | Scope | `DocumentCode` level | Notes |
|---|---|---|---|
| `TOU` | Tournament statistics at event level. | `CC@Event` for each event, and `CC@DisciplineGender` for summaries. | Aggregate `Stats/StatsItems` with bout result-code counts and percentages. |
| `TEAM_RANKING` | Ranking statistics per NOC at discipline level. | `CC@Discipline`. | Per-NOC `Stats/Competitor/StatsItems` with participation, phase-wins, bouts, points, and medals. |

## Trigger and Frequency

This message is to be sent at the end of the tournament.

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- SportDescription (0,1)
    |       +-- @DisciplineName
    |       +-- @EventName
    |       +-- @Gender
    +-- Stats (1,1)
        +-- @Code
        +-- StatsItems (0,1)
        |   +-- StatsItem (1,N)
        |       +-- @Type
        |       +-- @Code
        |       +-- @Pos
        |       +-- @Value
        |       +-- @Percent
        +-- Competitor (0,N)
            +-- @Code
            +-- @Type
            +-- @Order
            +-- StatsItems (0,1)
                +-- StatsItem (1,N)
                    +-- @Type
                    +-- @Code
                    +-- @Pos
                    +-- @Value
                    +-- @Avg
                    +-- @Rank
                    +-- @RankEqual
                    +-- @SortOrder
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | O | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | O | `S(20)` | Codes version applicable to the message. |

### `Competition / ExtendedInfos / SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE` English description | Discipline name, not the code. |
| `EventName` | M | `CC@EVENT` English description | Event name, not the code. |
| `Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the event unit. |

### `Competition / Stats`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `SC@Statistics` | Statistics table identifier. Must equal the header's `DocumentSubtype`. |

### `Competition / Stats / StatsItems / StatsItem` (for `DocumentSubtype="TOU"`)

Aggregate tournament-level counts of bout result outcomes. Element is expected when available, in the case of
`DocumentSubtype="TOU"`.

| Type | Code | Pos | Value | Percent | Meaning |
|---|---|---|---|---|---|
| `ST` | `SC@ResultCode` | N/A | `##0` (M) | `##0.00` (O) | Number of wins by that result code, with percentage of wins of that type. |

### `Competition / Stats / Competitor` (for `DocumentSubtype="TEAM_RANKING"`)

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Organisation/NOC code. Used only for `TEAM_RANKING`. |
| `Type` | M | `T` | `T` for team. |
| `Order` | M | Positive integer | Order of the competitor in the statistics table. |

### `Competition / Stats / Competitor / StatsItems / StatsItem` (for `DocumentSubtype="TEAM_RANKING"`)

Per-NOC discipline-level tournament statistics. All rows are expected always, if the information is available for
`DocumentSubtype="TEAM_RANKING"`.

| Type | Code | Pos | Value | Avg | Rank / RankEqual / SortOrder | Meaning |
|---|---|---|---|---|---|---|
| `ST` | `COMPETITORS_NUM` | `NUM` | `##0` (M) | N/A | N/A | Number of participants by NOC. |
| `ST` | `P` | N/A | `##0` (M) | N/A | N/A | Total victories in Preliminaries phase. |
| `ST` | `SF` | N/A | `##0` (M) | N/A | N/A | Total victories in Semifinals phase. |
| `ST` | `F` | N/A | `##0` (M) | N/A | N/A | Total victories in Finals phase. |
| `ST` | `BOUTS` | N/A | `##0` (M) | `##0` (M) | N/A | Total number of played bouts; `Avg` is bouts per participant. |
| `ST` | `LOST` | N/A | `##0` (M) | N/A | N/A | Total number of lost bouts. |
| `ST` | `PTS` | N/A | `##0` (M) | `##0` (O) | `Rank` (O), `RankEqual` (O, `Y`), `SortOrder` (M, positive integer) | Total team points; `Avg` is points per participant. `Rank` is the team rank by points, `RankEqual="Y"` only when tied. |
| `ST` | `GOLD` | `0`, `1`, `2` | `##0` (M) | N/A | N/A | Total number of Gold Medals. `Pos="0"` total, `Pos="1"` Men, `Pos="2"` Women. |
| `ST` | `SILVER` | `0`, `1`, `2` | `##0` (M) | N/A | N/A | Total number of Silver Medals. Same `Pos` convention. |
| `ST` | `BRONZE` | `0`, `1`, `2` | `##0` (M) | N/A | N/A | Total number of Bronze Medals. Same `Pos` convention. |
| `ST` | `TOT` | `0`, `1`, `2` | `##0` (M) | N/A | N/A | Total number of medals. Same `Pos` convention. |

## Boxing-Specific Stat Code Semantics

- `COMPETITORS_NUM` carries `Pos="NUM"` literally and counts how many athletes from the NOC entered the tournament. It
  is the denominator for the `Avg` ratios on `BOUTS` and `PTS`.
- `P`, `SF`, `F` track victories per tournament phase (Preliminaries, Semifinals, Finals). They are not phase totals;
  a single bout win in a semifinal increments `SF`. There is no explicit `QF` (quarterfinals) row in the section.
- `BOUTS` aggregates the number of bouts contested by athletes of that NOC. Note: this is bouts contested per NOC, not
  bouts in the tournament. The `Avg` of bouts per participant uses `COMPETITORS_NUM` as denominator.
- `LOST` is total lost bouts for the NOC. There is no `WON` code in the BOX `TEAM_RANKING` set; wins are implicit
  through phase wins (`P`, `SF`, `F`) and the medal totals.
- `PTS` is a discipline-level team-points score whose calculation is not defined inside this section. It is the
  primary ranking metric and the only row in this subtype that carries `Rank`, `RankEqual`, and `SortOrder`.
- The medal codes `GOLD`, `SILVER`, `BRONZE`, and `TOT` use a fixed three-position breakdown:
  - `Pos="0"` overall total for the NOC.
  - `Pos="1"` Men's events.
  - `Pos="2"` Women's events.
  The three rows per medal code are sent together; `Pos="0"` is not the sum of `Pos="1"` and `Pos="2"` mathematically
  required to match in the section, but the sample shows `2 = 1 + 1` for gold, `4 = 2 + 2` for silver.
- For `DocumentSubtype="TOU"`, the `StatsItem/@Code` is the result-code (`SC@ResultCode`) for the bout, for example
  `WP`, `KO`, `WO`. The `Value` counts how many bouts ended with that result, and `Percent` is the share of all bouts.

## Samples from the Dictionary, Normalized

### Tournament statistics (`DocumentSubtype="TOU"`)

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <SportDescription DisciplineName="Boxing" EventName="Men's 60kg" Gender="M"/>
  </ExtendedInfos>
  <Stats Code="TOU">
    <StatsItems>
      <StatsItem Type="ST" Code="WP" Value="25" Percent="92.60"/>
      <StatsItem Type="ST" Code="KO" Value="1" Percent="3.70"/>
      <StatsItem Type="ST" Code="WO" Value="1" Percent="3.70"/>
    </StatsItems>
  </Stats>
</Competition>
```

### Team ranking statistics (`DocumentSubtype="TEAM_RANKING"`)

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <SportDescription DisciplineName="Boxing" EventName="Boxing" Gender="X"/>
  </ExtendedInfos>
  <Stats Code="TEAM_RANKING">
    <Competitor Code="GBR" Type="T" Order="1">
      <StatsItems>
        <StatsItem Type="ST" Code="COMPETITORS_NUM" Pos="NUM" Value="10"/>
        <StatsItem Type="ST" Code="P" Value="15"/>
        <StatsItem Type="ST" Code="SF" Value="4"/>
        <StatsItem Type="ST" Code="F" Value="3"/>
        <StatsItem Type="ST" Code="BOUTS" Value="7" Avg="2.9"/>
        <StatsItem Type="ST" Code="LOST" Value="29"/>
        <StatsItem Type="ST" Code="PTS" Value="32" Avg="3.2" Rank="1" SortOrder="1"/>
        <StatsItem Type="ST" Code="GOLD" Pos="0" Value="2"/>
        <StatsItem Type="ST" Code="GOLD" Pos="1" Value="1"/>
        <StatsItem Type="ST" Code="GOLD" Pos="2" Value="1"/>
        <StatsItem Type="ST" Code="SILVER" Pos="0" Value="4"/>
        <StatsItem Type="ST" Code="SILVER" Pos="1" Value="2"/>
        <StatsItem Type="ST" Code="SILVER" Pos="2" Value="2"/>
        <StatsItem Type="ST" Code="BRONZE" Pos="0" Value="0"/>
        <StatsItem Type="ST" Code="BRONZE" Pos="1" Value="0"/>
        <StatsItem Type="ST" Code="BRONZE" Pos="2" Value="0"/>
        <StatsItem Type="ST" Code="TOT" Pos="0" Value="6"/>
        <StatsItem Type="ST" Code="TOT" Pos="1" Value="3"/>
        <StatsItem Type="ST" Code="TOT" Pos="2" Value="3"/>
      </StatsItems>
    </Competitor>
  </Stats>
</Competition>
```

## Message Sort

Sort according to the `@Order` attributes.

For `TEAM_RANKING`, the `Competitor/@Order` should be aligned with the `PTS` ranking, that is, `Order="1"` for the
top-ranked NOC by team points.

## XSD Validation

The normalized XML examples above were validated as `Competition` fragments wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Treat `DocumentSubtype` as part of the statistics table identity. `TOU` (tournament result-code distribution) and
  `TEAM_RANKING` (per-NOC discipline rollup) are structurally different and share only the `Stats` envelope.
- For `TOU`, statistics live under `Stats/StatsItems` directly; the `Code` of each row is itself a `SC@ResultCode`
  value. Persist these as result-code histograms keyed on the result code rather than as a fixed enumeration.
- For `TEAM_RANKING`, statistics live under `Stats/Competitor/StatsItems`; the `Competitor/@Code` is an NOC
  organisation code, and `Competitor/@Type="T"` is the team type even though the rows describe per-NOC totals.
- The medal codes (`GOLD`, `SILVER`, `BRONZE`, `TOT`) emit three rows each with `Pos` in `{0, 1, 2}`. Model these as a
  composite key `(Code, Pos)` to avoid collapsing the men/women breakdown.
- Only the `PTS` row carries ranking attributes for `TEAM_RANKING`. Other rows are values without explicit rank; do
  not synthesise ranks for `BOUTS`, `LOST`, or medal rows.
- The PDF prints the `TEAM_RANKING` Stats opening tag with a leading space (`Code=" TEAM_RANKING"`). Normalize this to
  `Code="TEAM_RANKING"` on ingestion; the appendix below records the literal as observed.
- The `Avg` semantics differ per row: `BOUTS/@Avg` is bouts per participant, `PTS/@Avg` is points per participant.
  Keep the denominator (`COMPETITORS_NUM`) accessible when reconstructing averages.
- The trigger ("end of the tournament") implies this is not a streaming-live message; expect a single mostly-final
  version per `(DocumentCode, DocumentSubtype)` pair, in contrast to `LIVE`/`INTERMEDIATE` flows in result messages.

## Code Appendix: Values Directly Visible in Pages 44-48

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `SC@Statistics` | `Stats/@Code` and header `DocumentSubtype` | `TOU`, `TEAM_RANKING`. The PDF sample prints `Code=" TEAM_RANKING"` with a leading space (typographic anomaly). |
| `SC@ResultCode` | `StatsItem/@Code` under `Stats/StatsItems` for `TOU` | `WP`, `KO`, `WO` (from the TOU sample). |
| `CC@ResultStatus` | `ResultStatus` header attribute | `LIVE`, `INTERMEDIATE`, `OFFICIAL`, `PROVISIONAL`. |
| `CC@DisciplineGender` | `DocumentCode` for `TOU` summaries | Not enumerated in the section. |
| `CC@Discipline` | `DocumentCode` for `TEAM_RANKING` | Not enumerated in the section. |
| `CC@Event` | `DocumentCode` for `TOU` per-event | Not enumerated in the section. |
| `CC@DISCIPLINE` | `SportDescription/@DisciplineName` | English description value, for example `Boxing`. |
| `CC@EVENT` | `SportDescription/@EventName` | English description value, not enumerated. |
| `CC@DISCIPLINE_GENDER` | `SportDescription/@Gender` | Not enumerated in the section. |
| `CC@COMPETITION_CODE` | `CompetitionCode` header attribute | Not enumerated in the section. |
| `CC@ORGANISATION` | `Competitor/@Code` for `TEAM_RANKING` (NOC) | `GBR` (from the TEAM_RANKING sample). |
| `SCGEN@Source` | `Source` header attribute | Not enumerated in the section. |
| BOX `TEAM_RANKING` `StatsItem` codes (not from a generic code page) | `Competitor/StatsItems/StatsItem/@Code` | `COMPETITORS_NUM`, `P`, `SF`, `F`, `BOUTS`, `LOST`, `PTS`, `GOLD`, `SILVER`, `BRONZE`, `TOT`. |
| BOX `Pos` values for `TEAM_RANKING` medal rows | `StatsItem/@Pos` for `GOLD`, `SILVER`, `BRONZE`, `TOT` | `0` total, `1` Men, `2` Women. |
| BOX `Pos` value for `COMPETITORS_NUM` | `StatsItem/@Pos` | `NUM` (literal). |

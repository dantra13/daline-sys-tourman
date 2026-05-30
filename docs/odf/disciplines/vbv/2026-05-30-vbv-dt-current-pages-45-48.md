
# ODF VBV Data Dictionary: DT_CURRENT, Pages 45-48

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 45-48.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note restructures the beach volleyball `DT_CURRENT` section into readable Markdown for domain modeling. It covers the current information message, trigger rules, live-state payload, XML example, and an appendix of English SC/CC values.

## 2.3.6 Current Information

`DT_CURRENT` carries the current live state for a beach volleyball match in progress. Unlike football or basketball, **there is no Clock element** — beach volleyball uses a set-based (not time-based) scoring model. Instead, the message exposes live match state through `ExtendedInfos` (current set, serve speed, serving team/athlete, match-point and set-point indicators) and cumulative point totals per set through `Periods`.

The overview states: *"DT_CURRENT only includes the clock and current score."* In practice the payload has no literal clock; the "current score" is the per-set point tally in `Periods`, and the `ExtendedInfos` entries provide the richer live indicators.

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @COMPETITION_CODE` | Unique competition identifier. |
| `DocumentCode` | `CC @EVENT_UNIT` | Full RSC of the unit (match). |
| `DocumentSubcode` | N/A | Not used for this message. |
| `DocumentType` | `DT_CURRENT` | Current message. |
| `DocumentSubtype` | N/A | Not used for this message. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time, up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day. Usually the physical day, except when the unit or message transmission crosses midnight. See the ODF Foundation for the full rule. |
| `Source` | `SCGEN @Source` | System that generated the message. Note: the PDF uses `SCGEN@Source` (general-sports code) rather than the sport-specific `SC@Source` pattern used in FBL and BKB. |

## Trigger and Frequency

| Trigger | Meaning |
|---|---|
| After every serve | Sent for the serve speed (`SPEED` extended info). |
| End of every rally | Sent primarily to update the current score in `Periods`. |

Beach volleyball `DT_CURRENT` is event-driven (per serve and per rally), not heartbeat-driven. There is no periodic background send because the sport lacks a running clock.

## Message Structure

```text
OdfBody
└─ Competition (0,1)
   ├─ @Gen
   ├─ @Sport
   ├─ @Codes
   ├─ ExtendedInfos (0,1)
   │  └─ ExtendedInfo (1,N)
   │     ├─ @Type
   │     ├─ @Code
   │     ├─ @Pos
   │     └─ @Value
   ├─ Periods (0,1)
   │  ├─ @Home
   │  ├─ @Away
   │  └─ Period (1,N)
   │     ├─ @Code
   │     ├─ @HomeScore
   │     ├─ @AwayScore
   │     ├─ @HomePeriodScore (O)
   │     ├─ @AwayPeriodScore (O)
   │     └─ @Duration (O)
   └─ Result (0,N)
      ├─ @Result
      ├─ @SortOrder
      ├─ @StartSortOrder
      ├─ @ResultType
      └─ Competitor (1,N)
         ├─ @Code
         ├─ @Type
         └─ @Organisation
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional | `S(20)` | Code-set version applicable to the message. |

## ExtendedInfos / ExtendedInfo

All entries carry `Type="UI"`. The `Code` distinguishes the live-state indicator. Only `PERIOD` and `SPEED` and `SERVE` are always sent; `MATCH_POINT` and `SET_POINT` are conditional.

| Type | Code | Pos | When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `MATCH_POINT` | N/A | When applicable | `SC @Home` (`H` or `A`) | Indicates which team has match point: `H` = home, `A` = away. |
| `UI` | `PERIOD` | N/A | Always | `SC @Period` | Current set in play (e.g. `S1`, `S2`, `S3`). |
| `UI` | `SERVE` | `SC @Home` | Always | `S(20)` | `Pos` = serving team (`H` for home, `A` for away). `Value` = athlete ID of the serving athlete. |
| `UI` | `SET_POINT` | N/A | When applicable | `SC @Home` (`H` or `A`) | Indicates which team has set point: `H` = home, `A` = away. |
| `UI` | `SPEED` | N/A | Always | `##0` | Online serve speed (km/h). |

**Note on `SERVE` encoding:** The PDF spec defines `Pos` as the serving-team indicator (SC@Home: `H` or `A`) and `Value` as the serving athlete's ID. The embedded PDF sample shows `<ExtendedInfo Type="UI" Code="SERVE" Value="H" />`, which appears to hold the team side in `Value` and omits `Pos`. Treat the spec (`Pos` = side, `Value` = athlete ID) as authoritative; the sample may be a simplified excerpt.

## Periods

Carries per-set and cumulative point totals. The `Home` and `Away` attributes identify the two competitors by ID.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Home` | Mandatory | `S(20)` no leading zeroes | Home competitor ID. |
| `Away` | Mandatory | `S(20)` no leading zeroes | Away competitor ID. |

### Periods / Period

One `Period` row per set played plus a `TOT` accumulator.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `SC @Period` | Set number (`S1`, `S2`, `S3`) or `TOT`. |
| `HomeScore` | Mandatory | `##0` | Cumulative total points for the home team up to and including this set. |
| `AwayScore` | Mandatory | `##0` | Cumulative total points for the away team up to and including this set. |
| `HomePeriodScore` | Optional | `##0` | Points scored by the home team in this set only. |
| `AwayPeriodScore` | Optional | `##0` | Points scored by the away team in this set only. |
| `Duration` | Optional | `mm` | Playing time of this set in minutes. Not sent for the `TOT` row. |

## Result

Carries **sets won** (0, 1, or 2), not point totals. At the start of the match the result should be `0` for both teams.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Result` | Optional | `##0` | Sets won by the team for this unit. |
| `SortOrder` | Mandatory | Positive Integer | Sequential order: `1` for home team, `2` for away team. |
| `StartSortOrder` | Mandatory | Positive Integer | Same value as `SortOrder`. |
| `ResultType` | Optional | `SC @ResultType` | Type of `Result` (points or IRM-with-points for the unit). |

### Result / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` no leading zeroes | Competitor's ID. |
| `Type` | Mandatory | `T` | `T` for team (beach volleyball competes as pairs, modelled as teams). |
| `Organisation` | Mandatory | `CC @ORGANISATION` | Competitor's organisation. |

## Sample (from PDF, pages 47-48)

The PDF provides a partial sample covering `ExtendedInfos` and `Periods` only:

```xml
<ExtendedInfos>
  <ExtendedInfo Type="UI" Code="PERIOD" Value="S2" />
  <ExtendedInfo Type="UI" Code="SPEED" Value="51" />
  <ExtendedInfo Type="UI" Code="SERVE" Value="H" />
</ExtendedInfos>
<Periods Home="VBVWTEAM2---SUI02" Away="VBVWTEAM2---AUS02">
  <Period Code="S1" HomeScore="19" AwayScore="21" HomePeriodScore="19" AwayPeriodScore="21" Duration="22" />
  <Period Code="S2" HomeScore="20" AwayScore="22" HomePeriodScore="1" AwayPeriodScore="1" Duration="1" />
</Periods>
```

## Complete XML Example

No XSD was provided for this discipline version. The example below is consistent with the dictionary's structural and attribute rules.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="SYOG2026"
         DocumentCode="VBVWTEAM2---SUI02---------S2--"
         DocumentType="DT_CURRENT"
         Version="14"
         FeedFlag="T"
         Date="2026-05-30"
         Time="15:22:00.000"
         LogicalDate="2026-05-30"
         Source="EIFVBV1">
  <Competition Gen="3.4" Sport="VBV-3.4" Codes="SYOG-2026">
    <ExtendedInfos>
      <ExtendedInfo Type="UI" Code="PERIOD"     Value="S2" />
      <ExtendedInfo Type="UI" Code="SPEED"      Value="51" />
      <ExtendedInfo Type="UI" Code="SERVE"      Pos="H" Value="VBVWTEAM2---SUI0200001" />
      <ExtendedInfo Type="UI" Code="SET_POINT"  Value="A" />
    </ExtendedInfos>
    <Periods Home="VBVWTEAM2---SUI02" Away="VBVWTEAM2---AUS02">
      <Period Code="S1" HomeScore="19" AwayScore="21" HomePeriodScore="19" AwayPeriodScore="21" Duration="22" />
      <Period Code="S2" HomeScore="20" AwayScore="22" HomePeriodScore="1"  AwayPeriodScore="1"  Duration="1" />
    </Periods>
    <Result Result="0" SortOrder="1" StartSortOrder="1" ResultType="POINTS">
      <Competitor Code="VBVWTEAM2---SUI02" Type="T" Organisation="SUI"/>
    </Result>
    <Result Result="1" SortOrder="2" StartSortOrder="2" ResultType="POINTS">
      <Competitor Code="VBVWTEAM2---AUS02" Type="T" Organisation="AUS"/>
    </Result>
  </Competition>
</OdfBody>
```

## Message Sort

Sort by `Period @Code`.

## Modeling Notes

- `DT_CURRENT` maps to a live scoreboard projection, not to the authoritative match record. The authoritative full result with all set detail lives in `DT_RESULT`.
- There is **no Clock element** in VBV `DT_CURRENT`. Do not look for clock state here; beach volleyball sets are not time-bounded.
- `Periods/@HomeScore` and `@AwayScore` are **cumulative** point totals to the current moment in the match, not just points in the current set. `HomePeriodScore`/`AwayPeriodScore` carry the current-set-only points.
- `Result/@Result` represents **sets won** (0, 1, or 2), not points. At match start both teams are `0`; the set win is reflected here once the set ends.
- The `SERVE` ExtendedInfo carries the serving-team side in `Pos` (H/A) and the athlete ID in `Value`. The PDF sample shows `Value="H"` without a `Pos` attribute — treat the per-element attribute table (Pos = side, Value = athlete ID) as authoritative. See the spec note in the ExtendedInfos section.
- `MATCH_POINT` and `SET_POINT` entries appear only when applicable. A live scoreboard should treat their absence as "no match/set point currently".
- `SPEED` (serve speed in km/h) is always sent after every serve. Consumer pipelines that aggregate this should handle the serve-speed send frequency: it is sent both after serve and end-of-rally.
- Beach volleyball teams are pairs; `Competitor/@Type="T"` reflects the ODF team-level competitor model even for a two-athlete team.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-30.

### Source Index

| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SCGEN @Source` | VBV | 1 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_VBV.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_VBV.htm) |
| `SC @Period` | VBV | 4 | [http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_VBV.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_VBV.htm) |
| `SC @ResultType` | VBV | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_VBV.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_VBV.htm) |

### SCGEN @Source

| Code | ENG Description |
|---|---|
| EIFVBV1 | Origin for messages from OVR at EIF for VBV |

### SC @Period

| Code | Order | ENG Description |
|---|---|---|
| S1 | 1 | Set 1 |
| S2 | 2 | Set 2 |
| S3 | 3 | Set 3 |
| TOT | 4 | Total |

### SC @ResultType

| Code | ENG Description |
|---|---|
| IRM_POINTS | For both, Points and Invalid Result Mark |
| POINTS | Points |

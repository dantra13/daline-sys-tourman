# ODF BKB Data Dictionary: DT_RESULT, Pages 30-49

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 30-49.

Source version: `SOG-2024-BKB-3.4 APP`, dated 19 January 2024.

This note restructures the basketball `DT_RESULT` section into readable Markdown for Sportivo domain modeling. It covers the Event Unit Start List and Results message: header metadata, trigger lifecycle, the full message tree, every value subsection, BKB-specific team and athlete statistics with their `ExtendedStat` children, period quarters and overtimes, starters per period, officials, coaches, and an XML example aligned with the dictionary semantics.

## 2.3.4 Event Unit Start List and Results

`DT_RESULT` carries both the start list and result data for one event unit. In basketball this means the match-level state, period and overtime scoring, starters at each period, lineups, officials, team statistics (points, rebounds, assists, turnovers, fouls, time-outs, plus/minus, etc.), athlete metadata, athlete participation and per-athlete shooting and on-court statistics.

The message is mandatory for all sports. It is always a full message: every applicable element and attribute is resent on each transmission, not only the changed values.

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Unit` | Full RSC for the event unit (match). |
| `DocumentSubcode` | N/A | Not used for this message. |
| `DocumentType` | `DT_RESULT` | Event Unit Start List and Results message. |
| `DocumentSubtype` | N/A | Not used for this message. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `ResultStatus` | `CC @ResultStatus` | Result lifecycle status. Basketball uses `START_LIST`, `LIVE`, `INTERMEDIATE` (after each period), `OFFICIAL`, `UNOFFICIAL`, and `PROVISIONAL`. The common code set also includes other generic statuses. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day. Usually the physical day, except when the unit or message transmission crosses midnight. |
| `Source` | `SC @Source` | System that generated the message. |

## Trigger and Frequency

| `ResultStatus` | When to send |
|---|---|
| `START_LIST` | As soon as the team or teams are known, before the match. |
| `START_LIST` | As soon as line-up and starters are known, and on any change/addition before the start of the match. |
| `LIVE` | As soon as the unit starts. |
| `LIVE` | At the beginning of each period. |
| `LIVE` | After every change in any data, such as score, substitution, disqualification, or other match data. |
| `INTERMEDIATE` | After each period when it is not the last period. |
| `UNOFFICIAL` | After the unit ends while results are still unofficial. |
| `OFFICIAL` | After results become official. |
| Any applicable status | Trigger again after any change. |

## Message Structure

```text
OdfBody
└─ Competition (0,1)
   ├─ @Gen
   ├─ @Sport
   ├─ @Codes
   ├─ ExtendedInfos (0,1)
   │  ├─ UnitDateTime (0,1)
   │  │  ├─ @StartDate
   │  │  └─ @Duration
   │  ├─ ExtendedInfo (0,N)
   │  │  ├─ @Type
   │  │  ├─ @Code
   │  │  ├─ @Pos
   │  │  ├─ @Value
   │  │  └─ Extension (0,N)
   │  ├─ SportDescription (0,1)
   │  │  ├─ @DisciplineName
   │  │  ├─ @EventName
   │  │  ├─ @Gender
   │  │  ├─ @SubEventName
   │  │  └─ @UnitNum
   │  └─ VenueDescription (0,1)
   │     ├─ @Venue
   │     ├─ @VenueName
   │     ├─ @Location
   │     ├─ @LocationName
   │     └─ @Attendance
   ├─ Officials (0,1)
   │  └─ Official (1,N)
   │     ├─ @Code
   │     ├─ @Function
   │     ├─ @Order
   │     └─ Description (1,1)
   │        ├─ @GivenName
   │        ├─ @FamilyName
   │        ├─ @Gender
   │        ├─ @Organisation
   │        └─ @IFId
   ├─ Periods (0,1)
   │  ├─ @Home
   │  ├─ @Away
   │  └─ Period (1,N)
   │     ├─ @Code
   │     ├─ @HomeScore
   │     ├─ @AwayScore
   │     ├─ @HomePeriodScore
   │     ├─ @AwayPeriodScore
   │     └─ ExtendedPeriods (0,1)
   │        └─ ExtendedPeriod (1,N)
   │           ├─ @Type
   │           ├─ @Code
   │           ├─ @Pos
   │           └─ @Value
   └─ Result (1,N)
      ├─ @Result
      ├─ @IRM
      ├─ @WLT
      ├─ @SortOrder
      ├─ @StartOrder
      ├─ @StartSortOrder
      ├─ @ResultType
      ├─ ExtendedResults (0,1)
      │  └─ ExtendedResult (1,N)
      │     ├─ @Type
      │     ├─ @Code
      │     ├─ @Pos
      │     └─ @Value
      └─ Competitor (1,1)
         ├─ @Code
         ├─ @Type
         ├─ @Organisation
         ├─ Description (0,1)
         │  ├─ @TeamName
         │  └─ @IFId
         ├─ Coaches (0,1)
         │  └─ Coach (1,N)
         │     ├─ @Code
         │     ├─ @Order
         │     ├─ @Function
         │     └─ Description (1,1)
         │        ├─ @GivenName
         │        ├─ @FamilyName
         │        ├─ @Gender
         │        └─ @Nationality
         ├─ EventUnitEntry (0,N)
         │  ├─ @Type
         │  ├─ @Code
         │  ├─ @Pos
         │  └─ @Value
         ├─ StatsItems (0,1)
         │  └─ StatsItem (1,N)
         │     ├─ @Type
         │     ├─ @Code
         │     ├─ @Pos
         │     ├─ @Value
         │     ├─ @Attempt
         │     ├─ @Percent
         │     └─ ExtendedStat (0,N)
         └─ Composition (0,1)
            └─ Athlete (0,N)
               ├─ @Code
               ├─ @Order
               ├─ @StartSortOrder
               ├─ @Bib
               ├─ Description (1,1)
               ├─ EventUnitEntry (0,N)
               └─ StatsItems (0,1)
                  └─ StatsItem (1,N)
                     ├─ @Type
                     ├─ @Code
                     ├─ @Pos
                     ├─ @Value
                     ├─ @Attempt
                     ├─ @Percent
                     └─ ExtendedStat (0,N)
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional | `S(20)` | Code-set version applicable to the message. |

## ExtendedInfos / UnitDateTime

Use `UnitDateTime` for actual times. Include it when the unit starts.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `StartDate` | Mandatory | `DateTime` | Actual start date/time for the unit. |
| `Duration` | Optional | `h:mm` | Match duration. |

## ExtendedInfos / ExtendedInfo

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `PERIOD` | N/A | Always after the start of the unit. | `SC @Period` or `SC @GameState` | Send the current period code unless a `GameState` value is more appropriate. |
| `STATS` | `LEAD_CHANGES` | N/A | Always, if available. | Numeric `#0` | Number of lead changes in the match. |
| `STATS` | `TIED_NUM` | N/A | Always, if available. | Numeric `#0` | Number of times the scores are tied in the match. |
| `DISPLAY` | `CURRENT` | Numeric `#0` (unique number per competitor on the court) | When available while the unit is `LIVE`, only for players currently on the court. | `S(20)` without leading zeroes | Send the competitor ID of the player on the court. |
| `DISPLAY` | `FG`, `P2`, `P3`, `FT`, `REB`, `ASSIST`, `TO`, `ST`, `BLC`, `PF`, `TREB`, `TTO`, `PF_COACH` | Numeric sequence within message | When available and only while the unit is `LIVE`. Multiple values may be sent. | `S(20)` | ID of the athlete or team whose `StatsItem` was last updated. The `Code` mirrors the `StatsItem @Code` of the last update under `Result/Competitor/StatsItems` or `Result/Competitor/Composition/Athlete/StatsItems`. |

### ExtendedInfo / Extension

`Extension` is used for `REB` and `TREB` updates, and to flag which sub-attribute of `FG`, `P2`, `P3`, or `FT` was updated. It is sent only while the unit is `LIVE`. Multiple `Extension` values may be sent.

| Attribute | Value | Meaning |
|---|---:|---|
| `Code` | String | Send the `ExtendedStat @Code` or attribute name of the last updated `StatsItem`. For `REB` and `TREB`, use `OR` or `DR`. For `FG`, `P2`, `P3`, `FT`, use `Value` or `Attempt`. |
| `Pos` | N/A | Not used here. |
| `Value` | `Y` | Flag-style extension value. |

```xml
<ExtendedInfos>
  <ExtendedInfo Type="STATS" Code="TIED_NUM" Value="5"/>
  <ExtendedInfo Type="STATS" Code="LEAD_CHANGES" Value="8"/>
  <ExtendedInfo Type="DISPLAY" Code="CURRENT" Pos="1" Value="1102201"/>
  <ExtendedInfo Type="DISPLAY" Code="CURRENT" Pos="2" Value="1102199"/>
  <ExtendedInfo Type="DISPLAY" Code="P2" Pos="1" Value="2518090"/>
  <ExtendedInfo Type="DISPLAY" Code="P2" Pos="2" Value="BKBMTEAM5---GER01"/>
</ExtendedInfos>
```

## SportDescription

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `DisciplineName` | Mandatory | `S(40)` | English discipline description from Common Codes, not the code itself. |
| `EventName` | Mandatory | `S(40)` | English event description from Common Codes. |
| `Gender` | Mandatory | `CC @SportGender` | Gender code for the event unit. |
| `SubEventName` | Mandatory | `S(40)` | English event-unit description from Common Codes. |
| `UnitNum` | Optional | `S(6)` | Match number. |

## VenueDescription

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Venue` | Mandatory | `CC @VenueCode` | Venue code. |
| `VenueName` | Mandatory | `S(25)` | English venue description from Common Codes. |
| `Location` | Mandatory | `CC @Location` | Location code. |
| `LocationName` | Mandatory | `S(30)` | English location description from Common Codes. |
| `Attendance` | Optional | `#####0` | Total attendance. Do not send if unknown. |

## Officials / Official

Send officials according to the codes: the crew chief, the umpires, and the commissioner.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Official identifier. |
| `Function` | Mandatory | `CC @ResultsFunction` | Official's function for the result message, such as crew chief, umpire, or commissioner. |
| `Order` | Mandatory | Numeric | Order as on the official score sheet. |

### Official / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Official gender. |
| `Organisation` | Mandatory | `CC @Organisation` | Official organisation. |
| `IFId` | Optional | `S(16)` | International Federation ID. |

## Periods

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Home` | Mandatory | `S(20)` with no leading zeroes | Home competitor ID. |
| `Away` | Mandatory | `S(20)` with no leading zeroes | Away competitor ID. |

### Period

`Period` covers the period in which the event unit message arrives, including half-quarters, quarters, half time, and overtimes.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `SC @Period` | Period code. |
| `HomeScore` | Mandatory | Numeric `##0` | Overall home score at the end of the period. |
| `AwayScore` | Mandatory | Numeric `##0` | Overall away score at the end of the period. |
| `HomePeriodScore` | Optional | Numeric `##0` | Home score in this period only. Only send for full quarters (not half-quarter) and for each overtime. |
| `AwayPeriodScore` | Optional | Numeric `##0` | Away score in this period only. Only send for full quarters (not half-quarter) and for each overtime. |

### Period / ExtendedPeriods / ExtendedPeriod

`ExtendedPeriod` is used to declare starters per period.

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `STARTER` | `HOME` | Numeric `0`, `1`..`5` for the five athletes that start the quarter or overtime | When the information is available for each quarter and overtime. | `S(20)` with no leading zeroes | ID of the home team starter for each period. |
| `STARTER` | `AWAY` | Numeric `0`, `1`..`5` for the five athletes that start the quarter or overtime | When the information is available for each quarter and overtime. | `S(20)` with no leading zeroes | ID of the away team starter for each period. |

```xml
<Periods Home="T-BKBMTEAM5USA01" Away="T-BKBMTEAM5FRA01">
  <Period Code="Q1_H" HomeScore="10" AwayScore="4"/>
  <Period Code="Q1" HomeScore="16" AwayScore="12" HomePeriodScore="16" AwayPeriodScore="12">
    <ExtendedPeriods>
      <ExtendedPeriod Type="STARTER" Code="HOME" Pos="1" Value="1102201"/>
      <ExtendedPeriod Type="STARTER" Code="HOME" Pos="2" Value="1102199"/>
      <ExtendedPeriod Type="STARTER" Code="HOME" Pos="3" Value="1102203"/>
      <ExtendedPeriod Type="STARTER" Code="HOME" Pos="4" Value="1102213"/>
      <ExtendedPeriod Type="STARTER" Code="HOME" Pos="5" Value="1102198"/>
      <ExtendedPeriod Type="STARTER" Code="AWAY" Pos="1" Value="1109414"/>
    </ExtendedPeriods>
  </Period>
  <Period Code="Q2_H" HomeScore="20" AwayScore="20"/>
  <Period Code="Q2" HomeScore="28" AwayScore="28" HomePeriodScore="12" AwayPeriodScore="16"/>
</Periods>
```

## Result

Each event-unit result message must contain at least one `Result` competitor.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Result` | Optional | Numeric `##0` | Team result for the event unit. Do not send if the match is nullified. |
| `IRM` | Optional | `SC @IRM` | Invalid result mark for the team. Send only when `ResultType` uses both points and IRM. |
| `WLT` | Optional | `SC @WLT` | Whether the competitor won or lost. |
| `SortOrder` | Mandatory | Numeric | Sequential display order: first-named team is `1`, visitor is `2`. |
| `StartOrder` | Mandatory | Numeric | `1` for first-named team, `2` for second-named team. |
| `StartSortOrder` | Mandatory | Numeric | Same value as `StartOrder`. |
| `ResultType` | Optional | `SC @ResultType` | Type of `Result`. Usually `POINTS`, but can be `IRM_POINTS` for forfeit or `IRM` for nullified. |

### Result / ExtendedResults / ExtendedResult

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ER` | `BONUS` | N/A | When the team is in a bonus situation. | `Y` | Bonus flag. Clear at the start of each period unless the situation carries over. |

## Result / Competitor

Competitor related to the result of one event unit.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes, or `SC @CompetitorPlace` | Competitor ID or placeholder. Placeholders include `TBD` when the competitor is unknown but will be available later, `NOCOMP` when no competitor exists and none will arrive later, `BYE` for a bye, and `NOAWARD` when not awarded. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Organisation` | Optional | `CC @Organisation` | Competitor organisation. |

### Competitor / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `TeamName` | Mandatory | `S(73)` | Name of the team. |
| `IFId` | Optional | `S(16)` | International Federation ID. |

### Competitor / Coaches / Coach

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Official code. |
| `Order` | Mandatory | Numeric | Coach order. Send `1` if there is just one coach; sequential if more than one, in the order they are presented on the organisation entry form. |
| `Function` | Mandatory | `CC @ResultsFunction` | Coach function. |

### Coach / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Coach gender. |
| `Nationality` | Mandatory | `CC @Country` | Coach nationality. |

## Competitor / EventUnitEntry

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `HOME_AWAY` | N/A | When available. | `SC @Home` | Home or Away designator. |
| `EUE` | `AGE_AVG` | N/A | If available. | Numeric `#0.0` | Average age of the team on the day of the match. |
| `EUE` | `HEIGHT_AVG` | N/A | If available. | Numeric `0.00` | Average height of the team in metres. |

```xml
<EventUnitEntry Type="EUE" Code="HOME_AWAY" Value="AWAY"/>
<EventUnitEntry Type="EUE" Code="AGE_AVG" Value="28.0"/>
<EventUnitEntry Type="EUE" Code="HEIGHT_AVG" Value="1.92"/>
```

## Competitor / StatsItems / StatsItem (Team Statistics)

Team statistics use `Type="ST"`. For statistics that are period-scoped, `Pos` is a `SC @Period` code (a quarter `Qn`, the aggregate `OT` for all overtimes, or `TOT` for the totals across all periods).

| Code | Pos | Expected When | Value | Attempt | Percent | Meaning |
|---|---|---|---|---|---|---|
| `PTS` | N/A | Always. | Numeric `##0` | N/A | N/A | Total points for the team. Has `ExtendedStat` children for sub-breakdowns. |
| `LEAD_MAX` | N/A | Always, if available. | Numeric `#0` | N/A | N/A | Biggest lead. Has `ExtendedStat Code="SCORE"` child for the current score when the biggest lead occurred. |
| `SCORE_RUN_MAX` | N/A | Always, if available. | String | N/A | N/A | Biggest scoring run, such as `6-0`. Has `ExtendedStat Code="SCORE"` child for the current score when the run occurred. |
| `LEADING` | N/A | Always, if available. | `m:ss` | N/A | N/A | Total time leading. |
| `FG` | `SC @Period` or `TOT` | Always. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | Field goals. `Value` is successful attempts; `Attempt` is attempts; `Percent` is shooting percentage. |
| `P2` | `SC @Period` or `TOT` | Always. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | 2-point shots. |
| `P3` | `SC @Period` or `TOT` | Always. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | 3-point shots. |
| `FT` | `SC @Period` or `TOT` | Always. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | Free throws. |
| `PAINT` | `SC @Period` or `TOT` | Always. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | Points in the paint sub-tracking (shooting). |
| `REB` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | N/A | Total rebounds. Has `ExtendedStat` children `DR` and `OR`. |
| `TREB` | N/A | Always, if available. | Numeric `#0` | N/A | N/A | Total team rebounds (team-level rebounds, not aggregated player rebounds). Has `ExtendedStat` children `DR` (team defensive rebounds) and `OR` (team offensive rebounds). |
| `ASSIST` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | N/A | Assists. |
| `TO` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | N/A | Turnovers. |
| `TTO` | N/A | Always, if available. | Numeric `#0` | N/A | N/A | Team turnovers (charged to the team rather than to a single athlete). |
| `ST` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | N/A | Steals. |
| `BLC` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | N/A | Blocked shots. |
| `PF` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | N/A | Fouls. |
| `PF_TEAM` | N/A | Always, if available. | Numeric `#0` | N/A | N/A | Team period fouls for the current period. Reset to `0` at the start of every regular-time period; do not reset for overtime. |
| `EFF` | N/A | Always, if available. | Numeric `#0` | N/A | N/A | Team efficiency. |
| `PF_COACH` | N/A | Always, if available. | Numeric `#0` | N/A | N/A | Coach fouls. |
| `FD` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | N/A | Total fouls drawn. |
| `PLUS_MINUS` | N/A | Always, if available. | Numeric `#0` or `-#0` | N/A | N/A | Plus / minus for the team. |
| `TOUT` | N/A | Always, if available. | Numeric `#0` | N/A | N/A | Time-outs totally taken. Has `ExtendedStat Code="MAX"` child for the maximum possible time-outs. |

### Team Stats ExtendedStat children

| Parent Code | ExtendedStat Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `PTS` | `2CHANCE` | N/A | Always, if available. | Numeric `#0` | Second-chance points. |
| `PTS` | `BENCH` | N/A | Always, if available. | Numeric `#0` | Bench points. |
| `PTS` | `FAST_BRK` | N/A | Always, if available. | Numeric `#0` | Fast-break points. |
| `PTS` | `PAINT` | N/A | Always, if available. | Numeric `#0` | Points in the paint. |
| `PTS` | `TO` | N/A | Always, if available. | Numeric `#0` | Points from turnovers. |
| `LEAD_MAX` | `SCORE` | N/A | Always, if available. | String | Current score when the biggest lead occurred. |
| `SCORE_RUN_MAX` | `SCORE` | N/A | Always, if available. | String | Current score when the biggest scoring run occurred. |
| `REB` | `DR` | N/A | Always, if available. | Numeric `#0` | Defensive rebounds. |
| `REB` | `OR` | N/A | Always, if available. | Numeric `#0` | Offensive rebounds. |
| `TREB` | `DR` | N/A | Always, if available. | Numeric `#0` | Team defensive rebounds. |
| `TREB` | `OR` | N/A | Always, if available. | Numeric `#0` | Team offensive rebounds. |
| `TOUT` | `MAX` | N/A | Always, if available. | Numeric `#0` | Maximum possible time-outs. |

```xml
<StatsItems>
  <StatsItem Type="ST" Code="PTS" Value="71">
    <ExtendedStat Code="TO" Value="7"/>
    <ExtendedStat Code="PAINT" Value="20"/>
    <ExtendedStat Code="2CHANCE" Value="15"/>
  </StatsItem>
  <StatsItem Type="ST" Code="LEAD_MAX" Value="9"/>
  <StatsItem Type="ST" Code="SCORE_RUN_MAX" Value="6-0">
    <ExtendedStat Code="SCORE" Value="16-7"/>
  </StatsItem>
  <StatsItem Type="ST" Code="TTO" Value="2"/>
  <StatsItem Type="ST" Code="TREB" Value="3">
    <ExtendedStat Code="OR" Value="1"/>
    <ExtendedStat Code="DR" Value="2"/>
  </StatsItem>
  <StatsItem Type="ST" Code="FG" Pos="TOT" Attempt="54" Value="25" Percent="46"/>
  <StatsItem Type="ST" Code="P2" Pos="TOT" Attempt="40" Value="20" Percent="50"/>
  <StatsItem Type="ST" Code="P3" Pos="TOT" Attempt="14" Value="5" Percent="36"/>
  <StatsItem Type="ST" Code="FT" Pos="TOT" Attempt="18" Value="16" Percent="89"/>
  <StatsItem Type="ST" Code="REB" Pos="TOT" Value="34">
    <ExtendedStat Code="OR" Value="11"/>
    <ExtendedStat Code="DR" Value="23"/>
  </StatsItem>
  <StatsItem Type="ST" Code="ASSIST" Pos="TOT" Value="13"/>
  <StatsItem Type="ST" Code="TO" Pos="TOT" Value="15"/>
  <StatsItem Type="ST" Code="ST" Pos="TOT" Value="3"/>
  <StatsItem Type="ST" Code="BLC" Pos="TOT" Value="2"/>
  <StatsItem Type="ST" Code="PF" Pos="TOT" Value="14"/>
</StatsItems>
```

## Competitor / Composition / Athlete

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Athlete identifier. |
| `Order` | Mandatory | Numeric `#0` | Primary sort order for team members. Before competition it equals `StartSortOrder`; during competition the order follows the Match Result in ORIS. |
| `StartSortOrder` | Mandatory | Numeric `#0` | Start-list sort order, as defined in ORIS. |
| `Bib` | Mandatory | `S(2)` | Shirt number. |

### Athlete / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Athlete gender. |
| `Organisation` | Mandatory | `CC @Organisation` | Athlete organisation. |
| `BirthDate` | Optional | `Date` | Birth date `YYYY-MM-DD`; must be included when available. |
| `IFId` | Optional | `S(16)` | International Federation ID. |
| `Class` | Optional | `CC @DisciplineClass` | Sport class for events involving athletes with a disability (for example, Paralympic Games). |

## Athlete / EventUnitEntry

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `STATUS` | N/A | Only for suspended players. | `SC @AthleteStatus` | Athlete status in the team. |
| `EUE` | `CAPTAIN` | N/A | Only for the captain, when known. | `Y` | Captain flag. |
| `EUE` | `STARTER` | N/A | Only for athletes in the starting line-up at the beginning of the game, when available. | `Y` | Starter flag. |
| `EUE` | `POSITION` | N/A | As soon as known. | `CC @Position` | Player position. |
| `EUE` | `AGE` | N/A | If available. | Numeric `#0.0` | Age of the player on the day of the match. |

```xml
<Athlete Code="1125142" Bib="8" Order="4" StartSortOrder="4">
  <Description GivenName="Jane" FamilyName="Smith" Gender="F" Organisation="RSA" BirthDate="1992-12-15"/>
  <EventUnitEntry Type="EUE" Code="POSITION" Value="G"/>
  <EventUnitEntry Type="EUE" Code="CAPTAIN" Value="Y"/>
  <EventUnitEntry Type="EUE" Code="STARTER" Value="Y"/>
  <EventUnitEntry Type="EUE" Code="AGE" Value="22"/>
</Athlete>
```

## Athlete / StatsItems / StatsItem (Athlete Statistics)

Athlete statistics use `Type="ST"`. In the dictionary, athlete-level codes do not carry a `Pos` for the period axis; they are match totals for the player. The same code names overlap with team stats but carry per-athlete semantics.

| Code | Pos | Expected When | Value | Attempt | Percent | Meaning |
|---|---|---|---|---|---|---|
| `FG` | N/A | Do not send if not applicable. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | Field goals: successful, attempts, percentage. |
| `P2` | N/A | Do not send if not applicable. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | 2-point shots. |
| `P3` | N/A | Do not send if not applicable. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | 3-point shots. |
| `FT` | N/A | Do not send if not applicable. | Numeric `#0` | Numeric `#0`, optional | Numeric `##0`, optional | Free throws. |
| `REB` | N/A | Do not send if not applicable. | Numeric `#0` | N/A | N/A | Total rebounds. Has `ExtendedStat` children `DR` and `OR`. |
| `ASSIST` | N/A | Do not send if not applicable. | Numeric `#0` | N/A | N/A | Assists. |
| `TO` | N/A | Do not send if not applicable. | Numeric `#0` | N/A | N/A | Turnovers. |
| `ST` | N/A | Do not send if not applicable. | Numeric `#0` | N/A | N/A | Steals. |
| `BLC` | N/A | Do not send if not applicable. | Numeric `#0` | N/A | N/A | Blocked shots. |
| `PF` | N/A | Do not send if not applicable. | Numeric `#0` | N/A | N/A | Fouls. |
| `FD` | N/A | Do not send if not applicable. | Numeric `#0` | N/A | N/A | Fouls drawn. |
| `EFF` | N/A | Do not send if not applicable. | Numeric `#0` | N/A | N/A | Player efficiency. |
| `PTS` | N/A | Do not send if not applicable. | Numeric `##0` | N/A | N/A | Total points. |
| `MINS` | N/A | Do not send if not applicable. | `m:ss` or `S(3)` | N/A | N/A | Minutes played or `DNP` if the player did not play. |
| `PLUS_MINUS` | N/A | Do not send if not applicable. | Numeric `#0` or `-#0` | N/A | N/A | Plus / minus. |

### Athlete REB ExtendedStat children

| Parent Code | ExtendedStat Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `REB` | `DR` | N/A | Always, if available. | Numeric `#0` | Defensive rebounds. |
| `REB` | `OR` | N/A | Always, if available. | Numeric `#0` | Offensive rebounds. |

```xml
<StatsItems>
  <StatsItem Type="ST" Code="FG" Attempt="5" Value="1" Percent="20"/>
  <StatsItem Type="ST" Code="P2" Attempt="5" Value="1" Percent="20"/>
  <StatsItem Type="ST" Code="FT" Attempt="8" Value="7" Percent="88"/>
  <StatsItem Type="ST" Code="REB" Value="6">
    <ExtendedStat Code="OR" Value="2"/>
    <ExtendedStat Code="DR" Value="4"/>
  </StatsItem>
  <StatsItem Type="ST" Code="ASSIST" Value="1"/>
  <StatsItem Type="ST" Code="TO" Value="1"/>
  <StatsItem Type="ST" Code="PF" Value="2"/>
  <StatsItem Type="ST" Code="FD" Value="4"/>
  <StatsItem Type="ST" Code="PTS" Value="9"/>
  <StatsItem Type="ST" Code="MINS" Value="20:05"/>
</StatsItems>
```

## Message Sort

Sort `Result` elements by `Result @SortOrder`.

## XSD-Aligned XML Example

No XSD was supplied for this extraction, so the example below is not schema-validated. It follows the element nesting, attribute names, and value formats described in the BKB dictionary section.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="BKBMTEAM5----------FNL-000100--"
         DocumentType="DT_RESULT"
         Version="3"
         ResultStatus="LIVE"
         FeedFlag="T"
         Date="2026-05-27"
         Time="20:12:03.123"
         LogicalDate="2026-05-27"
         Source="BCYBKB1">
  <Competition Gen="3.4" Sport="BKB-3.4" Codes="SOG-2024">
    <ExtendedInfos>
      <UnitDateTime StartDate="2026-05-27T20:00:00" Duration="1:55"/>
      <ExtendedInfo Type="UI" Code="PERIOD" Value="Q2"/>
      <ExtendedInfo Type="STATS" Code="TIED_NUM" Value="5"/>
      <ExtendedInfo Type="STATS" Code="LEAD_CHANGES" Value="8"/>
      <ExtendedInfo Type="DISPLAY" Code="CURRENT" Pos="1" Value="1102201"/>
      <ExtendedInfo Type="DISPLAY" Code="CURRENT" Pos="2" Value="1102199"/>
      <ExtendedInfo Type="DISPLAY" Code="P3" Pos="1" Value="1102201">
        <Extension Code="Value" Value="Y"/>
      </ExtendedInfo>
      <SportDescription DisciplineName="Basketball"
                        EventName="Men"
                        Gender="M"
                        SubEventName="Gold Medal Game"
                        UnitNum="38"/>
      <VenueDescription Venue="BCY"
                        VenueName="Bercy Arena"
                        Location="BCY"
                        LocationName="Bercy Arena"
                        Attendance="15012"/>
    </ExtendedInfos>
    <Officials>
      <Official Code="910001" Function="CHF" Order="1">
        <Description GivenName="Alex" FamilyName="Martin" Gender="M" Organisation="FRA"/>
      </Official>
      <Official Code="910002" Function="UM" Order="2">
        <Description GivenName="Maria" FamilyName="Garcia" Gender="F" Organisation="ESP"/>
      </Official>
    </Officials>
    <Periods Home="T-BKBMTEAM5USA01" Away="T-BKBMTEAM5FRA01">
      <Period Code="Q1_H" HomeScore="10" AwayScore="4"/>
      <Period Code="Q1" HomeScore="16" AwayScore="12" HomePeriodScore="16" AwayPeriodScore="12">
        <ExtendedPeriods>
          <ExtendedPeriod Type="STARTER" Code="HOME" Pos="1" Value="1102201"/>
          <ExtendedPeriod Type="STARTER" Code="HOME" Pos="2" Value="1102199"/>
          <ExtendedPeriod Type="STARTER" Code="HOME" Pos="3" Value="1102203"/>
          <ExtendedPeriod Type="STARTER" Code="HOME" Pos="4" Value="1102213"/>
          <ExtendedPeriod Type="STARTER" Code="HOME" Pos="5" Value="1102198"/>
          <ExtendedPeriod Type="STARTER" Code="AWAY" Pos="1" Value="1109414"/>
        </ExtendedPeriods>
      </Period>
    </Periods>
    <Result Result="28" WLT="W" SortOrder="1" StartOrder="1" StartSortOrder="1" ResultType="POINTS">
      <ExtendedResults>
        <ExtendedResult Type="ER" Code="BONUS" Value="Y"/>
      </ExtendedResults>
      <Competitor Code="T-BKBMTEAM5USA01" Type="T" Organisation="USA">
        <Description TeamName="United States of America"/>
        <Coaches>
          <Coach Code="920001" Order="1" Function="COACH">
            <Description GivenName="Steve" FamilyName="Kerr" Gender="M" Nationality="USA"/>
          </Coach>
          <Coach Code="920002" Order="2" Function="AST_COA">
            <Description GivenName="Tyronn" FamilyName="Lue" Gender="M" Nationality="USA"/>
          </Coach>
        </Coaches>
        <EventUnitEntry Type="EUE" Code="HOME_AWAY" Value="HOME"/>
        <EventUnitEntry Type="EUE" Code="AGE_AVG" Value="28.0"/>
        <EventUnitEntry Type="EUE" Code="HEIGHT_AVG" Value="2.02"/>
        <StatsItems>
          <StatsItem Type="ST" Code="PTS" Value="28">
            <ExtendedStat Code="PAINT" Value="14"/>
            <ExtendedStat Code="FAST_BRK" Value="6"/>
          </StatsItem>
          <StatsItem Type="ST" Code="LEAD_MAX" Value="9"/>
          <StatsItem Type="ST" Code="TTO" Value="2"/>
          <StatsItem Type="ST" Code="FG" Pos="TOT" Attempt="22" Value="11" Percent="50"/>
          <StatsItem Type="ST" Code="P3" Pos="TOT" Attempt="8" Value="3" Percent="38"/>
          <StatsItem Type="ST" Code="FT" Pos="TOT" Attempt="6" Value="5" Percent="83"/>
          <StatsItem Type="ST" Code="REB" Pos="TOT" Value="18">
            <ExtendedStat Code="OR" Value="5"/>
            <ExtendedStat Code="DR" Value="13"/>
          </StatsItem>
          <StatsItem Type="ST" Code="ASSIST" Pos="TOT" Value="7"/>
          <StatsItem Type="ST" Code="PF" Pos="TOT" Value="5"/>
          <StatsItem Type="ST" Code="TOUT" Value="2">
            <ExtendedStat Code="MAX" Value="7"/>
          </StatsItem>
        </StatsItems>
        <Composition>
          <Athlete Code="1102201" Order="1" StartSortOrder="1" Bib="6">
            <Description GivenName="LeBron" FamilyName="James" Gender="M" Organisation="USA" BirthDate="1984-12-30"/>
            <EventUnitEntry Type="EUE" Code="STARTER" Value="Y"/>
            <EventUnitEntry Type="EUE" Code="CAPTAIN" Value="Y"/>
            <EventUnitEntry Type="EUE" Code="POSITION" Value="SF"/>
            <StatsItems>
              <StatsItem Type="ST" Code="FG" Attempt="9" Value="5" Percent="56"/>
              <StatsItem Type="ST" Code="P3" Attempt="3" Value="1" Percent="33"/>
              <StatsItem Type="ST" Code="REB" Value="6">
                <ExtendedStat Code="OR" Value="1"/>
                <ExtendedStat Code="DR" Value="5"/>
              </StatsItem>
              <StatsItem Type="ST" Code="ASSIST" Value="4"/>
              <StatsItem Type="ST" Code="PTS" Value="12"/>
              <StatsItem Type="ST" Code="MINS" Value="20:05"/>
              <StatsItem Type="ST" Code="PLUS_MINUS" Value="8"/>
            </StatsItems>
          </Athlete>
        </Composition>
      </Competitor>
    </Result>
    <Result Result="12" WLT="L" SortOrder="2" StartOrder="2" StartSortOrder="2" ResultType="POINTS">
      <Competitor Code="T-BKBMTEAM5FRA01" Type="T" Organisation="FRA">
        <Description TeamName="France"/>
        <EventUnitEntry Type="EUE" Code="HOME_AWAY" Value="AWAY"/>
      </Competitor>
    </Result>
  </Competition>
</OdfBody>
```

## Modeling Notes

- `DT_RESULT` is the canonical output shape for a basketball match unit. Internally, the domain model should separate the match lifecycle, period/overtime scoring state, team result rows, starters per period, team-level statistics, athlete metadata, athlete participation, and per-athlete statistics, then assemble them into this full-message output on every emission.
- The message is a full message: every emission resends all applicable elements and attributes. Internal change tracking should not patch only deltas into the emitter; instead the snapshot of the unit drives the message.
- Home/away is repeated as `Result/Competitor/EventUnitEntry Code="HOME_AWAY"`, so the model needs that designation at competitor-in-unit level. `Periods` also exposes `@Home` and `@Away` as the competitor IDs.
- `Period @Code` uses the BKB-specific `SC @Period` codes including half-quarter codes `Q1_H..Q4_H` for in-period scoreboard snapshots. Send `HomePeriodScore` and `AwayPeriodScore` only on full quarters and overtimes, not on half-quarter rows.
- Starters per period live inside `Period/ExtendedPeriods/ExtendedPeriod` with `Type="STARTER"`, `Code="HOME"` or `"AWAY"`, and `Pos="1"..."5"`. The domain model needs starter-per-period as first-class data, not only an opening lineup, because basketball reports starters at every quarter and overtime.
- `StatsItem` is path-sensitive. The same `Code` such as `FG`, `REB`, `PTS`, `TO`, or `ASSIST` is valid under both `Competitor/StatsItems` (team) and `Composition/Athlete/StatsItems` (athlete), with different rules around `Pos` (period-aware for team, not for athlete) and different `ExtendedStat` children. Treat the (path, `Type`, `Code`) triple as the semantic key.
- `ExtendedStat` is also path-sensitive. For example, `DR` and `OR` appear both as children of team `REB` (player rebounds aggregated to team) and team `TREB` (team rebounds proper, charged to the team rather than a player). They have distinct meanings and must not be merged in the model.
- `PTS` carries a rich set of sub-attribution `ExtendedStat` children (`2CHANCE`, `BENCH`, `FAST_BRK`, `PAINT`, `TO`). Each child is a separate counter under a single parent total, so the domain stats schema should allow nested attributes on a single stat record.
- `ExtendedInfo Type="DISPLAY"` is the live "last updated" hint stream. Its `Code` mirrors the most recent `StatsItem @Code` and its `Value` is the competitor or athlete identifier. `Extension Code` further qualifies which sub-attribute was updated. The model should treat this as derived audit data of the in-progress state, not as authoritative stats.
- `PF_TEAM` resets at the start of every regular-time period but does not reset in overtime. Period reset logic for team fouls must be encoded explicitly per-period.
- `LEAD_MAX` and `SCORE_RUN_MAX` both carry an `ExtendedStat Code="SCORE"` snapshot of the score at the moment the lead or run was reached, so the model must remember the score state when those team records were set.
- `Result @SortOrder` controls presentation order. `StartOrder` and `StartSortOrder` are the original start-list order. Preserve both because they can diverge during the unit.
- `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, and `PROVISIONAL` define a result lifecycle the internal model should mirror, including the per-period `INTERMEDIATE` emissions.
- `Result/Competitor/Code` may be a placeholder (`TBD`, `NOCOMP`, `BYE`, `NOAWARD`) before the actual competitor is known, so competitor references should accept either a stable competitor ID or a placeholder code.
- `Athlete/StatsItems/StatsItem Code="MINS"` accepts a string `DNP` instead of `m:ss` to denote "did not play". The minutes field must therefore be a tagged union of duration and a status string.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-27. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index

| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Source` | BKB | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm) |
| `SC @Period` | BKB | 17 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_BKB.htm) |
| `SC @IRM` | BKB | 4 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_BKB.htm) |
| `SC @ResultType` | BKB | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_BKB.htm) |
| `SC @WLT` | BKB | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_WLT_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_WLT_SOG_BKB.htm) |
| `SC @AthleteStatus` | BKB | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_AthleteStatus_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_AthleteStatus_SOG_BKB.htm) |
| `SC @Home` | BKB aggregate | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm) |
| `SC @CompetitorPlace` | BKB aggregate | 4 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm) |
| `SC @GameState` | BKB aggregate | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm) |
| `CC @ResultStatus` | Common | 9 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm) |
| `CC @SportGender` | Common | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm) |
| `CC @ResultsFunction` | BKB rows from DisciplineFunction | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineFunction.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineFunction.htm) |
| `CC @PersonGender` | Common | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm) |
| `CC @Position` | BKB rows from Positions | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Positions.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Positions.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @VenueCode` | BKB venues inferred from BKB locations | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm) |
| `CC @Location` | BKB locations | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm) |
| `CC @Unit` | BKB EventUnit rows | 130 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @Country` | Common master data | 266 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm) |
| `CC @Organisation` | Common master data | 258 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |
| `CC @DisciplineClass` | Not found in Paris 2024 common-code index (HTTP 404) | 0 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm) |

### SC @Source
| Code | Note | ENG Description |
| --- | --- | --- |
| BCYBKB1 |  | Origin for messages from OVR at BCY for BKB |
| LILBKB1 |  | Origin for messages from OVR at LIL for BKB |

### SC @Period
| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| Q1_H | 1 |  | Half of quarter 1 |
| Q1 | 2 |  | Quarter 1 |
| Q2_H | 3 |  | Half of quarter 2 |
| Q2 | 4 |  | Quarter 2 |
| HT | 5 |  | Half Time |
| Q3_H | 6 |  | Half of quarter 3 |
| Q3 | 7 |  | Quarter 3 |
| Q4_H | 8 |  | Half of quarter 4 |
| Q4 | 9 |  | Quarter 4 |
| OT1 | 10 |  | Overtime 1 |
| OT2 | 11 |  | Overtime 2 |
| OT3 | 12 |  | Overtime 3 |
| OT4 | 13 |  | Overtime 4 |
| OT5 | 14 |  | Overtime 5 |
| OT6 | 15 |  | Overtime 6 |
| OT | 16 | All overtimes | Overtime |
| TOT | 17 |  | Total |

### SC @IRM
| Code | Note | ENG Description |
| --- | --- | --- |
| DNS |  | Did Not Start |
| DQB |  | Disqualified for unsportsmanlike behaviour |
| DSQ |  | Disqualified |
| WDR |  | Withdrawn |

### SC @ResultType
| Code | Note | ENG Description |
| --- | --- | --- |
| IRM |  | Invalid Result Mark |
| IRM_POINTS |  | For both, points and invalid result mark |
| POINTS |  | Points |

### SC @WLT
| Code | Note | ENG Description |
| --- | --- | --- |
| L |  | Lost |
| W |  | Won |

### SC @AthleteStatus
| Code | Note | ENG Description |
| --- | --- | --- |
| DQB |  | Disqualified for unsportmanlike behaviour |
| DSQ |  | Disqualified |
| SUSPEND |  | Suspended |

### SC @Home
| Code | Note | ENG Description |
| --- | --- | --- |
| AWAY |  | Away |
| HOME |  | Home |

### SC @CompetitorPlace
| Code | Note | ENG Description |
| --- | --- | --- |
| BYE | There is no competitor; the other team/athlete goes directly to the next phase/round | Bye |
| NOAWARD |  | Not awarded |
| NOCOMP |  | No competitor |
| TBD |  | To be determined |

### SC @GameState
| Code | Order | ENG Description |
| --- | --- | --- |
| E-Q1 | 1 | End of Quarter 1 |
| E-H1 | 2 | Half Time |
| E-Q3 | 3 | End of Quarter 3 |
| E-Q4 | 4 | End of Quarter 4 |
| E-OT1 | 5 | End of Overtime 1 |
| E-OT2 | 6 | End of Overtime 2 |
| E-OT3 | 7 | End of Overtime 3 |
| E-OT4 | 8 | End of Overtime 4 |
| E-OT5 | 9 | End of Overtime 5 |
| E-FS | 10 | Final Score |

### CC @ResultStatus
| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| START_LIST | 1 | Before competition, Start List | Start List |
| LIVE | 2 | For live updates during competition | Live |
| INTERMEDIATE | 3 | When competition is stopped, usued at pre-defined points | Intermediate |
| UNCONFIRMED | 4 | When the unit is over but not yet unofficial or official | Unconfirmed |
| UNOFFICIAL | 5 | Results of the competition released as soon as the event is over, not waiting any official decision of the International Federation. The correctness of data must be assured | Unofficial |
| OFFICIAL | 6 | Results of the competition released as soon as the event is officially confirmed taking into account the resolution of the protests, etc. | Official |
| PARTIAL | 7 | Incomplete list, Final Ranking. Used in PDF | Partial |
| PROTESTED | 8 | After the competition is no longer LIVE and a protest has been lodged | Protested |
| PROVISIONAL | 9 | Special situations | Provisional |

### CC @SportGender
| Id | Description |
| --- | --- |
| - | Global |
| M | Men |
| O | Open |
| W | Women |
| X | Mixed |

### CC @ResultsFunction (BKB rows from DisciplineFunction)
| Function | Order | Category | Partic | ENG Description |
| --- | --- | --- | --- | --- |
| AA01 | 0 | A | Y | Athlete |
| COACH | 1 | C | Y | Coach |
| AST_COA | 2 | C | Y | Assistant Coach |
| TCH_DEL | 3 | S | Y | IF Delegate |
| JUR_ME | 4 | J | Y | Jury Member |
| CMM | 5 | S | Y | Commissioner |
| RE | 6 | J | Y | Referee |
| CHF | 7 | J | Y | Crew Chief |
| UM | 8 | S | Y | Umpire |
| RR | 9 | J | Y | Standby Referee |

### CC @PersonGender
| Id | ENG Description |
| --- | --- |
| F | Female |
| M | Male |
| X | Unspecified |

### CC @Position (BKB rows)
| Id | PositionOrder | ENG Description |
| --- | --- | --- |
| C |  | Centre |
| F |  | Forward |
| G |  | Guard |
| PF |  | Power Forward |
| PG |  | Point Guard |
| SF |  | Small Forward |
| SG |  | Shooting Guard |

### CC @Competition
| Id | ENG Description |
| --- | --- |
| OG2024 | Olympic Games Paris 2024 |
| OG2024-HT | Olympic Games Paris 2024 - HT |
| OG2024-ITL | Olympic Games Paris 2024 - ITL |
| OG2024-MST1 | Olympic Games Paris 2024 - MST1 |
| OG2024-MST2 | Olympic Games Paris 2024 - MST2 |
| OG2024-MST3 | Olympic Games Paris 2024 - MST3 |
| OG2024-MST4 | Olympic Games Paris 2024 - MST4 |
| OG2024-TEV | Olympic Games Paris 2024 - TEV |
| OG2024-TR1 | Olympic Games Paris 2024 - TR1 |
| OG2024-TR2 | Olympic Games Paris 2024 - TR2 |

### CC @VenueCode (BKB venues inferred from BKB locations)
| Id | IndoorOutdoor | Cluster | ENG longdescription | ENG Description |
| --- | --- | --- | --- | --- |
| BCY | I | CRN | Bercy Arena | Bercy Arena |
| LIL | O |  | Pierre Mauroy Stadium | Pierre Mauroy Stadium |

### CC @Location (BKB locations)
| Id | Venue | Discipline | Source | ENG longdescription | ENG Description |
| --- | --- | --- | --- | --- | --- |
| BCY | BCY | BKB,GAR,GTR | BCYBKB1,BCYGAR1,BCYGTR1 | Bercy Arena | Bercy Arena |
| LIL | LIL | HBL,BKB | LILHBL1,LILBKB1 | Pierre Mauroy Stadium | Pierre Mauroy Stadium |

### CC @Unit (BKB EventUnit rows)

The Paris 2024 `EventUnit` table contains 130 rows whose `Code` begins with `BKB`. They cover the discipline header (`BKB-------------------------------`), venue codes (`BCY`, `LIL`), the gender splits `BKBM` and `BKBW`, the `TEAM5` events, group phases `GPA`/`GPB`, quarter-finals `QFNL`, semi-finals `SFNL`, finals `FNL`, victory ceremonies `VICT`, and the per-match unit codes such as `BKBMTEAM5----------FNL-000100--` (Men's Gold Medal Game) or `BKBWTEAM5----------FNL-000100--` (Women's Gold Medal Game). Use the source link in the index above as the authoritative value list when modeling `DocumentCode`.

### CC @Country and CC @Organisation

These are large common master-data tables rather than basketball-specific enumerations. The downloaded Paris 2024 code pages contain 266 `Country` rows and 258 `Organisation` rows. Use the source links in the index above as the authoritative value lists when modeling `Nationality` and `Organisation` fields.

### CC @DisciplineClass

The basketball dictionary references `CC @DisciplineClass` for para-sport classification on `Athlete @Class`. The Paris 2024 common-code index downloaded for this appendix returned HTTP 404 for `DisciplineClass.htm`. Treat this as an externally supplied classification code set and validate it against the relevant para-sport dictionary or code release when that scope is implemented.

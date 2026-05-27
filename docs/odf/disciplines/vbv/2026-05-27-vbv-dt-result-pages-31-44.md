# ODF VBV Data Dictionary: DT_RESULT, Pages 31-44

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 31-44.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note restructures the Beach Volleyball `DT_RESULT` section into a practical domain reference for the Event Unit
Start List and Results message. It covers match-level state, set scoring, home/away teams, team and athlete statistics,
serving/highlight display state, and the start-list/result payload for one game unit.

## 2.3.5 Event Unit Start List and Results

`DT_RESULT` contains both start-list and result information for competitors in one event unit. In Beach Volleyball the
unit is a game/match. This is always a full message: each emission sends all applicable elements and attributes, not only
changed values.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Unique competition identifier. |
| `DocumentCode` | `CC@EVENT_UNIT` | Full RSC of the event unit. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_RESULT` | Event Unit Start List and Results message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `CC@ResultStatus` | Result lifecycle status. The section lists `START_LIST`, `LIVE`, `INTERMEDIATE`, `OFFICIAL`, `UNOFFICIAL`, and `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| `ResultStatus` | When to send |
|---|---|
| `START_LIST` | As soon as teams are known; resend after team/start-list changes, ExtendedInfos changes, or pre-start IRMs. |
| `LIVE` | At the beginning of each set. |
| `LIVE` | After every rally, score change, data correction, match-info change, or stats change. |
| `INTERMEDIATE` | After each period/set. |
| `UNOFFICIAL` / `OFFICIAL` | After the match. |
| Any applicable status | Send on any other change. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- UnitDateTime (0,1)
    |   +-- ExtendedInfo (0,N)
    |   |   +-- Extension (0,N)
    |   +-- SportDescription (0,1)
    |   +-- VenueDescription (0,1)
    +-- Officials (0,1)
    +-- Periods (0,1)
    |   +-- Period (1,N)
    |       +-- ExtendedPeriods (0,1)
    +-- Result (1,N)
        +-- ExtendedResults (0,1)
        +-- Competitor (1,1)
            +-- Description (0,1)
            +-- EventUnitEntry (0,N)
            +-- StatsItems (0,1)
            +-- Composition (0,1)
                +-- Athlete (0,N)
                    +-- Description (1,1)
                    +-- EventUnitEntry (0,N)
                    +-- StatsItems (0,1)
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | Mandatory in XSD | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Mandatory in XSD | `S(20)` | Code-set version applicable to the message. |

### `Competition / ExtendedInfos / UnitDateTime`

Actual match times. Include when the unit starts.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `StartDate` | M | DateTime | Actual start date and time. |
| `EndDate` | O | DateTime | Actual end date-time, sent at the end of each unit. |
| `Duration` | O | `h:mm` | Match duration. Do not send hours if zero. |

### `Competition / ExtendedInfos / ExtendedInfo`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `DURATION_PLAYING` | N/A | Update with each message after the match starts. | `h:mm` | Total playing time, without hours if zero. |
| `UI` | `PERIOD` | N/A | Always when `LIVE`. | `SC@Period` or `SC@GameState` | Current period/set, or game state when applicable. |
| `UI` | `SERVE` | `A` or `H` | When available while `LIVE`. | Athlete ID | Current server. `H` means home team, `A` away team. |
| `UI` | `MATCH_POINT` | N/A | When available while `LIVE`. | `H` or `A` | Match-point indicator for home or away team. |
| `UI` | `SET_POINT` | N/A | When available while `LIVE`. | `H` or `A` | Set-point indicator for home or away team. |
| `DISPLAY` | `SRV`, `ATC`, `BLC`, `DIG`, `PTY` | Sequential number | When available and only while `LIVE`. | Athlete or team ID | Last updated stat item to highlight. May point to team stats or athlete stats. |

### `ExtendedInfo / Extension`

For `DISPLAY/SRV` and `DISPLAY/ATC`, an `Extension` may mark the specific stat outcome:

| Extension `Code` | Value | Meaning |
|---|---|---|
| `ACE` | `Y` | Service ace. |
| `ATT` | `Y` | Attempt. |
| `FLT` | `Y` | Fault. |
| `SCS` | `Y` | Success. |

### `SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE` English description | Discipline name, not code. |
| `EventName` | M | `CC@EVENT` English description | Event name, not code. |
| `Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the unit. |
| `SubEventName` | M | `CC@EVENT_UNIT` English short description | Event-unit description, not code. |
| `UnitNum` | O | `S(6)` | Match number. |

### `VenueDescription`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Venue` | M | `CC@VENUE` ID | Venue code. |
| `VenueName` | M | `CC@VENUE` English description | Venue name. |
| `Location` | M | `CC@LOCATION` ID | Location code. |
| `LocationName` | M | `CC@LOCATION` English description | Location name. |
| `Attendance` | O | `####0` | Total attendance. |

### `Officials / Official`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Official code. |
| `Function` | M | `CC@DISCIPLINE_FUNCTION` ID | Official function. May differ from the function sent in `DT_PARTIC`. |
| `Order` | M | Positive integer | Official order. |

### `Periods`

| Path | Attribute | M/O | Value | Meaning |
|---|---|---|---|---|
| `Periods` | `Home` | M | Competitor ID | Home team competitor ID, if known. |
| `Periods` | `Away` | M | Competitor ID | Away team competitor ID, if known. |
| `Period` | `Code` | M | `SC@Period` | Set number; also always send `TOT`. |
| `Period` | `HomeScore` | O | `##0` | Overall points of the home team up to this point. |
| `Period` | `AwayScore` | O | `##0` | Overall points of the away team up to this point. |
| `Period` | `HomePeriodScore` | O | `##0` | Home points in this period/set. |
| `Period` | `AwayPeriodScore` | O | `##0` | Away points in this period/set. |
| `Period` | `Duration` | O | `mm` | Playing time of the set; not for `TOT`. |

### `Period / ExtendedPeriods / ExtendedPeriod`

| Type | Code | Expected When | Value | Meaning |
|---|---|---|---|---|
| `OFFICIAL` | `HOME` | If match is not completed and official period scores are used; send all periods except `TOT`. | `#0` | Official home score for the period. |
| `OFFICIAL` | `AWAY` | Same as above. | `#0` | Official away score for the period. |
| `EP` | `WINNER` | When available after the match starts. | `SC@Home` | Period winner: `H` home or `A` away. |

### `Result`

At least one `Result` is required for each event-unit result message.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Result` | O | `##0` | Sets won by the team, or `0` if an `IRM` exists. At game start, result should be `0`. This is the official score and may differ from period score if the match stops early. |
| `IRM` | O | `SC@IRM` | Team invalid result mark. Sent only when `ResultType` is both points and IRM. |
| `WLT` | O | `SC@WLT` | Whether a competitor won or lost. |
| `SortOrder` | M | Positive integer | `1` home team, `2` away team. |
| `StartOrder` | M | Positive integer | `1` first team, `2` other team. |
| `StartSortOrder` | M | Positive integer | Same as `StartOrder`. |
| `ResultType` | O | `SC@ResultType` | Points or IRM-with-points result type for the event unit. |

### `Result / ExtendedResults / ExtendedResult`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ER` | `MATCH_POINT_COUNT` | N/A | When applicable. | `#0` | Match-point count. |
| `ER` | `SET_POINT_COUNT` | `SC@Period` | When applicable. | `#0` | Set-point count for the period. |
| `ER` | `CHALLENGES_REMAIN` | `SC@Period` | Always when available. | `#0` | Remaining challenges for the competitor in the current set. |
| `ER` | `SETS_WON` | N/A | Always. | `#0` | Sets actually played and won. Can differ from the periods sum if the match is not completed. |

### `Result / Competitor`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes, `TBD`, or `NOCOMP` | Competitor ID; `TBD` if unknown but expected later, `NOCOMP` if no competitor will come later. |
| `Type` | M | `T` | Team competitor. |
| `Organisation` | O | `CC@ORGANISATION` ID | Competitor organisation. |

### `Competitor / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `TeamName` | M | `S(73)` | Team name, as concatenated shirt names. |

### `Competitor / EventUnitEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `HOME_AWAY` | N/A | As soon as available. | `SC@Home` | Home or away designator: `HOME` or `AWAY`. |
| `EUE` | `UNIFORM` | `1`, `2`, or `3` | As soon as known. | String | Team shirt colour by uniform number. |

### Team `StatsItems`

Team statistics are sent under `Result / Competitor / StatsItems / StatsItem`.

| Code | Pos | Value / Attempt | ExtendedStat | Meaning |
|---|---|---|---|---|
| `SRV` | Each period and `TOT` | `Attempt` = service attempts | `ACE`, `FLT`, `PLAY` | Team serving totals. |
| `ATC` | Each period and `TOT` | `Value` = attack successes; `Attempt` = attack attempts | `FLT`, `PLAY` | Team attack totals. |
| `BLC` | Each period and `TOT` | `Value` = block successes | N/A | Team block totals. |
| `DIG` | Each period and `TOT` | `Value` = dig successes | N/A | Team dig totals. |
| `OPP_ERR` | Each period and `TOT` | `Value` = opponent errors | N/A | Points/errors attributed to opponent error. |
| `PTY` | Each period and `TOT` | `Value` = penalties | N/A | Team penalties. |
| `TOT_TEAM` | Each period and `TOT` | `Value` = total points | N/A | Team points total. |
| `TOUT` | Each period and `TOT` | `Value` = timeouts | N/A | Team timeout count. |

### `Composition / Athlete`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Athlete ID. |
| `Order` | M | Positive integer | Player order in start list/result. Before the unit, use `StartSortOrder`; once started, use result order. |
| `StartSortOrder` | O | Positive integer | Team-member start-list order. |
| `Bib` | M | `S(2)` | Shirt number. |

### `Athlete / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `GivenName` | O | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | M | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | M | `CC@PERSON_GENDER` ID | Athlete gender. |
| `Organisation` | M | `CC@ORGANISATION` ID | Athlete organisation. |
| `BirthDate` | O | `YYYY-MM-DD` | Birth date, if available. |
| `IFId` | O | `S(16)` | International Federation ID. |

### `Athlete / EventUnitEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `CAPTAIN` | N/A | Only for the captain when available. | `Y` | Player is captain. |
| `EUE` | `HAND` | N/A | Always. | `SC@Hand` | Player handedness. |
| `EUE` | `POSITION` | N/A | If available. | `CC@POSITION` ID | Athlete role. |

### Athlete `StatsItems`

Athlete statistics are sent under `Result / Competitor / Composition / Athlete / StatsItems / StatsItem`.

| Code | Pos | Value / Attempt | ExtendedStat | Meaning |
|---|---|---|---|---|
| `SRV` | Each period and `TOT` | `Attempt` = service attempts | `ACE`, `FLT`, `SPEED`, `PLAY` | Athlete serving totals and fastest serve. |
| `ATC` | Each period and `TOT` | `Value` = attack successes; `Attempt` = attack attempts | `PLAY`, `FLT` | Athlete attack totals. |
| `BLC` | Each period and `TOT` | `Value` = block successes | N/A | Athlete block totals. |
| `DIG` | Each period and `TOT` | `Value` = dig successes | N/A | Athlete dig totals. |
| `PTY` | Each period and `TOT` | `Value` = penalties | N/A | Athlete penalties. |

## Samples from the Dictionary, Normalized

### Live display state

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <UnitDateTime StartDate="2016-07-28T12:00:00+01:00"/>
    <ExtendedInfo Type="UI" Code="PERIOD" Value="S1"/>
    <ExtendedInfo Type="UI" Code="SERVE" Pos="H" Value="2518090"/>
    <ExtendedInfo Type="DISPLAY" Code="SRV" Pos="1" Value="2518090">
      <Extension Code="ATT" Value="Y"/>
    </ExtendedInfo>
    <ExtendedInfo Type="DISPLAY" Code="ATC" Pos="2" Value="VBVMTEAM2---GER01">
      <Extension Code="SCS" Value="Y"/>
    </ExtendedInfo>
  </ExtendedInfos>
  <Result ResultType="POINTS" Result="0" SortOrder="1" StartOrder="1" StartSortOrder="1">
    <Competitor Code="VBVMTEAM2---GER01" Type="T" Organisation="GER">
      <Description TeamName="Smith/Jones"/>
    </Competitor>
  </Result>
</Competition>
```

### Set scores, team stats, and athlete stats

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <Periods Home="VBVMTEAM2---GER01" Away="VBVMTEAM2---BRA01">
    <Period Code="S1" HomeScore="21" AwayScore="18" HomePeriodScore="21" AwayPeriodScore="18" Duration="22">
      <ExtendedPeriods>
        <ExtendedPeriod Type="EP" Code="WINNER" Value="H"/>
      </ExtendedPeriods>
    </Period>
    <Period Code="TOT" HomeScore="21" AwayScore="18"/>
  </Periods>
  <Result ResultType="POINTS" Result="1" WLT="W" SortOrder="1" StartOrder="1" StartSortOrder="1">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="SETS_WON" Value="1"/>
      <ExtendedResult Type="ER" Code="SET_POINT_COUNT" Pos="S1" Value="2"/>
    </ExtendedResults>
    <Competitor Code="VBVMTEAM2---GER01" Type="T" Organisation="GER">
      <Description TeamName="Smith/Jones"/>
      <EventUnitEntry Type="EUE" Code="HOME_AWAY" Value="HOME"/>
      <EventUnitEntry Type="EUE" Code="UNIFORM" Pos="1" Value="Red"/>
      <StatsItems>
        <StatsItem Type="ST" Code="SRV" Pos="S1" Attempt="20">
          <ExtendedStat Code="ACE" Value="2"/>
          <ExtendedStat Code="FLT" Value="1"/>
          <ExtendedStat Code="PLAY" Value="17"/>
        </StatsItem>
        <StatsItem Type="ST" Code="ATC" Pos="TOT" Attempt="17" Value="9"/>
      </StatsItems>
      <Composition>
        <Athlete Code="1125142" Bib="8" Order="1" StartSortOrder="1">
          <Description GivenName="Jane" FamilyName="Smith" Gender="F" Organisation="GER" BirthDate="1992-12-15"/>
          <EventUnitEntry Type="EUE" Code="CAPTAIN" Value="Y"/>
          <EventUnitEntry Type="EUE" Code="HAND" Value="R"/>
          <StatsItems>
            <StatsItem Type="ST" Code="SRV" Pos="TOT" Attempt="10">
              <ExtendedStat Code="ACE" Value="1"/>
              <ExtendedStat Code="SPEED" Value="71"/>
            </StatsItem>
            <StatsItem Type="ST" Code="BLC" Pos="TOT" Value="3"/>
          </StatsItems>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
  <Result ResultType="POINTS" Result="0" WLT="L" SortOrder="2" StartOrder="2" StartSortOrder="2">
    <Competitor Code="VBVMTEAM2---BRA01" Type="T" Organisation="BRA">
      <Description TeamName="Silva/Santos"/>
      <EventUnitEntry Type="EUE" Code="HOME_AWAY" Value="AWAY"/>
    </Competitor>
  </Result>
</Competition>
```

## Message Sort

Sort by `Result/@SortOrder`.

For Beach Volleyball this means:

| Sort Order | Team |
|---:|---|
| `1` | Home team |
| `2` | Away team |

## XSD Validation

The normalized XML examples above were validated as `Competition` fragments wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Treat `DT_RESULT` as the authoritative full snapshot for one game unit. Replace prior unit state by version rather
  than patch-merging sparse fragments.
- Keep home/away as a stable match role: it drives `Result/@SortOrder`, `Periods/@Home` / `@Away`, period scores, serve
  indicators, and `EUE/HOME_AWAY`.
- `DT_CURRENT` carries only clock/current score per the overview. Rich score, period, result, and stat state belongs in
  `DT_RESULT`.
- Set/period codes in this VBV section use `SC@Period`, with samples such as `S1`, `S2`, `S3`, and `TOT`.
- Team stats and athlete stats use the same stat code family (`SRV`, `ATC`, `BLC`, `DIG`, `PTY`) but live at different
  paths. Preserve the path when modeling or ingesting.
- `DISPLAY` ExtendedInfos are display/highlight pointers into the team or athlete stat tree; they should not be modeled
  as the source of record for totals.
- Temporary `TMRY` scheduled units from the overview should remain scheduling placeholders, not result units once real
  teams and match allocations are known.

## Code Appendix: Values Directly Visible in Pages 31-44

The section references several code pages. This appendix records values directly visible in the `DT_RESULT` pages and
does not attempt to embed large master-data tables.

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@ResultStatus` | `OdfBody/@ResultStatus` | `START_LIST`, `LIVE`, `INTERMEDIATE`, `OFFICIAL`, `UNOFFICIAL`, `PROVISIONAL` |
| `SC@Period` | `ExtendedInfo/PERIOD`, `Period/@Code`, stat `Pos` | `S1`, `S2`, `S3`, `TOT` visible in samples; `TOT` is the total row. |
| `SC@GameState` | `ExtendedInfo/PERIOD` fallback | No concrete values printed in pages 31-44. |
| `SC@Home` | Home/away indicators and period winner | `H`, `A`, `HOME`, `AWAY` are visible in descriptions/samples. |
| `SC@IRM` | `Result/@IRM` | No concrete values printed in pages 31-44. |
| `SC@WLT` | `Result/@WLT` | No concrete values printed in pages 31-44. |
| `SC@ResultType` | `Result/@ResultType` | Points or IRM-with-points result type; examples here use `POINTS` for normalized XML. |
| `SC@Hand` | `Athlete/EventUnitEntry Code="HAND"` | `R` visible in sample. |
| `CC@POSITION` | `Athlete/EventUnitEntry Code="POSITION"` | No concrete values printed in pages 31-44. |
| Team/athlete stat codes | `StatsItem/@Code` and display pointers | `SRV`, `ATC`, `BLC`, `DIG`, `OPP_ERR`, `PTY`, `TOT_TEAM`, `TOUT`; extended stats include `ACE`, `ATT`, `FLT`, `SCS`, `PLAY`, `SPEED`. |

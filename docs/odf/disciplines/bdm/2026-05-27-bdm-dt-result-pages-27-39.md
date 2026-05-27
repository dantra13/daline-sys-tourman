# ODF BDM Data Dictionary: DT_RESULT, Pages 27-39

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BDM_Data_Dictionary.pdf`, pages 27-39.

Source version: `SYOG-2026-BDM-1.1 SFR`, dated 13 May 2026.

This note restructures the Badminton `DT_RESULT` section into a practical domain reference for the Event Unit Start
List and Results message. It covers match lifecycle, game scoring, side assignment, live serve/receive state, challenge
state, result rows, athlete entry values, and athlete statistics.

## 2.3.4 Event Unit Start List and Results

`DT_RESULT` contains both start-list and result information for competitors in one Badminton event unit. The unit is a
match. The message is mandatory and is always a full message: each emission sends all applicable elements and
attributes, not only changed values.

For Badminton, the result is modeled around two athlete competitors:

- `SortOrder="1"` / `StartSortOrder="1"` is the home competitor.
- `SortOrder="2"` / `StartSortOrder="2"` is the away competitor.
- `Periods` carries game-level scores and a `TOT` total row.
- Live match state such as current game, server, receiver, game point, and match point is carried in
  `ExtendedInfos/ExtendedInfo`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@EVENT_UNIT` | Event Unit RSC. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_RESULT` | Event Unit Start List and Results message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `CC@RESULTSTATUS` | Result lifecycle status. The section lists `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, and `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| `ResultStatus` | When to send |
|---|---|
| `START_LIST` | As soon as competitors are known. |
| `START_LIST` | After any start-list data change. |
| `LIVE` | As soon as the unit starts. |
| `LIVE` | On every live data change. |
| `INTERMEDIATE` | At the end of each game while the match is not yet finished. |
| `UNOFFICIAL` | When the match is over and results are available but not official. |
| `OFFICIAL` | When the match results become official. |
| `PROVISIONAL` | Listed as an allowed header status. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- UnitDateTime (0,1)
    |   |   +-- @StartDate
    |   |   +-- @Duration
    |   +-- ExtendedInfo (0,N)
    |       +-- @Type
    |       +-- @Code
    |       +-- @Pos
    |       +-- @Value
    |       +-- Extension (0,N)
    +-- SportDescription (0,1)
    +-- VenueDescription (0,1)
    +-- Officials (0,1)
    +-- Periods (0,1)
    |   +-- @Home
    |   +-- @Away
    |   +-- Period (1,N)
    |       +-- @Code
    |       +-- @HomeScore
    |       +-- @AwayScore
    |       +-- @HomePeriodScore
    |       +-- @AwayPeriodScore
    |       +-- @Duration
    |       +-- ExtendedPeriods (0,1)
    +-- Result (1,N)
        +-- @Result
        +-- @IRM
        +-- @WLT
        +-- @SortOrder
        +-- @StartSortOrder
        +-- @ResultType
        +-- ExtendedResults (0,1)
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Composition (0,1)
                +-- Athlete (0,N)
                    +-- @Code
                    +-- @Order
                    +-- Description (0,1)
                    +-- EventUnitEntry (0,N)
                    +-- StatsItems (0,1)
```

## Competition and Unit State

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competition/@Gen` | O | `S(20)` | General schema/package version label. |
| `Competition/@Sport` | O | `S(35)` | Badminton dictionary version label. |
| `Competition/@Codes` | O | `S(20)` | Code set version label. |
| `UnitDateTime/@StartDate` | M | DateTime | Scheduled/actual start time. |
| `UnitDateTime/@Duration` | O | `mmm` | Match duration in minutes. Not sent for DSQ before match or walkover. |

## ExtendedInfos

| Type | Code | Pos | Value | When / meaning |
|---|---|---|---|---|
| `UI` | `RES_CODE` | N/A | `SC@ResultCode` | Match result code, used only for bye or walkover when available. |
| `UI` | `RALLY_TIME_MAX` | N/A | Seconds | Longest rally duration. |
| `UI` | `RALLY_STROKES_MAX` | N/A | `##0` | Longest rally stroke count. |
| `UI` | `RALLY_TIME_AVG` | N/A | Seconds | Average rally duration. |
| `UI` | `RALLY_STROKES_AVG` | N/A | `#0` | Average rally stroke count. |
| `UI` | `SHUTTLES_USED` | N/A | `##0` | Number of shuttlecocks used. |
| `UI` | `CURRENT_GAME` | N/A | `SC@Period` | Current game while `ResultStatus="LIVE"`. |
| `UI` | `SERVE` | `SC@Home` | Competitor ID | Updated after each point with the next server. |
| `UI` | `RECEIVE` | `SC@Home` | Competitor ID | Updated after each point in doubles with the next receiver. |
| `UI` | `MATCH_POINT` | `1`, `2` | `SC@Home` | Current point is match point for the indicated side. |
| `UI` | `MATCH_POINT_ADV` | `SC@Home` | `#0` | Consecutive match-point advantage. |
| `UI` | `GAME_POINT` | `1`, `2` | `SC@Home` | Current point is game point for the indicated side. |
| `UI` | `GAME_POINT_ADV` | `SC@Home` | `#0` | Consecutive game-point advantage. |

## Periods

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Periods/@Home` | M | Competitor code | Home competitor. |
| `Periods/@Away` | M | Competitor code | Away competitor. |
| `Period/@Code` | M | `SC@Period` | Game code. Send `TOT` as the total row. |
| `Period/@HomeScore` / `@AwayScore` | O | Integer | Match score by side. |
| `Period/@HomePeriodScore` / `@AwayPeriodScore` | O | Integer | Game score by side. |
| `Period/@Duration` | O | Minutes | Game duration. |

### ExtendedPeriods

| Type | Code | Pos | Value | Meaning |
|---|---|---|---|---|
| `EP` | `RALLY_MAX` | N/A | `#0` | Longest rally in the period. |
| `EP` | `RALLY_AVG` | N/A | `#0` | Average rally in the period. |

## Result Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Result/@Result` | O | `#0` | Games won. At start list time this should be `0`. |
| `Result/@IRM` | O | `SC@IRM` | Sent when `ResultType` combines IRM and points. |
| `Result/@WLT` | O | `SC@WLT` | Win/loss/tie value at the end when applicable. |
| `Result/@SortOrder` | M | `1`, `2` | `1` home, `2` away. |
| `Result/@StartSortOrder` | M | `1`, `2` | Same home/away ordering in the start list. |
| `Result/@ResultType` | O | `SC@ResultType` | Points or IRM-plus-points result type. |
| `Competitor/@Code` | M | Athlete ID, `TBD`, `NOCOMP` | Competitor code for singles, or placeholder. |
| `Competitor/@Type` | M | `A` | Athlete competitor. |
| `Competitor/@Organisation` | O | `CC@ORGANISATION` | Competitor organisation. |

### ExtendedResults

| Type | Code | Pos | Value | When |
|---|---|---|---|---|
| `ER` | `CHALLENGES_REMAIN` | Current `SC@Period` | `0` | Remaining challenges where applicable. Not included for `UNOFFICIAL` or `OFFICIAL`; not used for Paralympics. |

## Athlete Event Unit Entries

| Type | Code | Pos | Value | When |
|---|---|---|---|---|
| `EUE` | `SEED` | N/A | `#0` | Seed when applicable in singles matches; not used for Paralympics. |
| `EUE` | `HAND` | N/A | `SC@Hand` | Athlete handedness when available. |
| `EUE` | `GRIP` | N/A | Text/code | Racket grip value when available in the source sample. |

## Athlete Statistics

Athlete statistics are sent under `Result / Competitor / Composition / Athlete / StatsItems / StatsItem`.

| Type | Code | Pos | Value | Meaning |
|---|---|---|---|---|
| `ST` | `PTS_PLAYED` | Period, not `TOT` | `#0` | Points played. |
| `ST` | `PTS_WON` | Period, not `TOT` | `#0` | Points won. |
| `ST` | `PTS_CONSEC` | Period and `TOT` | `#0` | Consecutive points. |
| `ST` | `PTS_MATCH` | Period and `TOT` | `#0` | Match points. |
| `ST` | `PTS_GAME` | Period and `TOT` | `#0` | Game points. |
| `ST` | `PTS_NOSERVICE` | Period and `TOT` | `#0` | Points scored without service. |
| `ST` | `PTS_SERVICE` | Period and `TOT` | `#0` | Points scored with service. |
| `ST` | `LEAD_MAX` | Period and `TOT` | `#0` or `-` | Biggest lead. |
| `ST` | `CBACK_WIN_MAX` | Period and `TOT` | `#0` or `-` | Biggest comeback to win. |
| `ST` | `SERVE_FAULTS` | Period and `TOT` | `#0` | Service faults. |
| `ST` | `PTY` | Sequential card number | `SC@Card` | Penalty card, with an extended `PERIOD` statistic carrying period and score context. |
| `ST` | `CHALLENGE_WON` | Period and `TOT` | `#0` | Successful challenges at Olympic Games. |
| `ST` | `CHALLENGE_LOST` | Period and `TOT` | `#0` | Unsuccessful challenges at Olympic Games. |
| `ST` | `SMASH_MAX` | Period and `TOT` | `#0` | Fastest smash in singles. |

## Samples from the Dictionary, Normalized

### Live match state

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-BDM-1.1 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <UnitDateTime StartDate="2026-10-31T10:00:00+00:00"/>
    <ExtendedInfo Type="UI" Code="CURRENT_GAME" Value="G1"/>
    <ExtendedInfo Type="UI" Code="SERVE" Pos="H" Value="1010001"/>
    <ExtendedInfo Type="UI" Code="GAME_POINT" Pos="1" Value="H"/>
    <ExtendedInfo Type="UI" Code="SHUTTLES_USED" Value="4"/>
  </ExtendedInfos>
  <Periods Home="1010001" Away="1010002">
    <Period Code="G1" HomeScore="0" AwayScore="0" HomePeriodScore="20" AwayPeriodScore="18" Duration="12"/>
    <Period Code="TOT" HomeScore="0" AwayScore="0"/>
  </Periods>
  <Result ResultType="POINTS" Result="0" SortOrder="1" StartOrder="1" StartSortOrder="1">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="CHALLENGES_REMAIN" Pos="G1" Value="0"/>
    </ExtendedResults>
    <Competitor Code="1010001" Type="A" Organisation="SEN">
      <Composition>
        <Athlete Code="1010001" Order="1">
          <Description GivenName="Awa" FamilyName="Diop" Gender="F" Organisation="SEN" BirthDate="2009-04-12"/>
          <EventUnitEntry Type="EUE" Code="SEED" Value="1"/>
          <EventUnitEntry Type="EUE" Code="HAND" Value="R"/>
          <StatsItems>
            <StatsItem Type="ST" Code="PTS_PLAYED" Pos="G1" Value="38"/>
            <StatsItem Type="ST" Code="PTS_WON" Pos="G1" Value="20"/>
            <StatsItem Type="ST" Code="PTS_SERVICE" Pos="TOT" Value="9"/>
          </StatsItems>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
  <Result ResultType="POINTS" Result="0" SortOrder="2" StartOrder="2" StartSortOrder="2">
    <Competitor Code="1010002" Type="A" Organisation="FRA">
      <Composition>
        <Athlete Code="1010002" Order="1">
          <Description GivenName="Lea" FamilyName="Martin" Gender="F" Organisation="FRA" BirthDate="2009-09-02"/>
          <EventUnitEntry Type="EUE" Code="HAND" Value="L"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

### Official result

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-BDM-1.1 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <UnitDateTime StartDate="2026-10-31T10:00:00+00:00" Duration="42"/>
    <ExtendedInfo Type="UI" Code="RALLY_TIME_MAX" Value="34"/>
    <ExtendedInfo Type="UI" Code="RALLY_STROKES_MAX" Value="37"/>
    <ExtendedInfo Type="UI" Code="RALLY_TIME_AVG" Value="8"/>
    <ExtendedInfo Type="UI" Code="RALLY_STROKES_AVG" Value="7"/>
    <ExtendedInfo Type="UI" Code="SHUTTLES_USED" Value="9"/>
  </ExtendedInfos>
  <Periods Home="1010001" Away="1010002">
    <Period Code="G1" HomeScore="1" AwayScore="0" HomePeriodScore="21" AwayPeriodScore="18" Duration="18">
      <ExtendedPeriods>
        <ExtendedPeriod Type="EP" Code="RALLY_MAX" Value="37"/>
        <ExtendedPeriod Type="EP" Code="RALLY_AVG" Value="7"/>
      </ExtendedPeriods>
    </Period>
    <Period Code="G2" HomeScore="2" AwayScore="0" HomePeriodScore="21" AwayPeriodScore="16" Duration="20"/>
    <Period Code="TOT" HomeScore="2" AwayScore="0"/>
  </Periods>
  <Result ResultType="POINTS" Result="2" WLT="W" SortOrder="1" StartOrder="1" StartSortOrder="1">
    <Competitor Code="1010001" Type="A" Organisation="SEN">
      <Composition>
        <Athlete Code="1010001" Order="1">
          <Description GivenName="Awa" FamilyName="Diop" Gender="F" Organisation="SEN" BirthDate="2009-04-12"/>
          <StatsItems>
            <StatsItem Type="ST" Code="PTS_WON" Pos="TOT" Value="42"/>
            <StatsItem Type="ST" Code="LEAD_MAX" Pos="TOT" Value="6"/>
            <StatsItem Type="ST" Code="SMASH_MAX" Pos="TOT" Value="291"/>
          </StatsItems>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
  <Result ResultType="POINTS" Result="0" WLT="L" SortOrder="2" StartOrder="2" StartSortOrder="2">
    <Competitor Code="1010002" Type="A" Organisation="FRA">
      <Composition>
        <Athlete Code="1010002" Order="1">
          <Description GivenName="Lea" FamilyName="Martin" Gender="F" Organisation="FRA" BirthDate="2009-09-02"/>
          <StatsItems>
            <StatsItem Type="ST" Code="PTS_WON" Pos="TOT" Value="34"/>
            <StatsItem Type="ST" Code="LEAD_MAX" Pos="TOT" Value="2"/>
          </StatsItems>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

## Message Sort

Sort by `Result/@SortOrder`.

For Badminton this means:

| Sort Order | Competitor |
|---:|---|
| `1` | Home competitor |
| `2` | Away competitor |

## XSD Validation

The normalized XML examples above were validated as `Competition` fragments wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Treat `DT_RESULT` as the authoritative full snapshot for one match unit. Replace prior unit state by version rather
  than patch-merging sparse fragments.
- Keep home/away as a stable match role. It drives `Result/@SortOrder`, `Periods/@Home` / `@Away`, game scores,
  server/receiver live state, and game/match point state.
- `CURRENT_GAME`, `SERVE`, `RECEIVE`, `GAME_POINT`, and `MATCH_POINT` are live indicators. They should not be used as
  substitutes for period score history or final result rows.
- `TOT` is a total row in `Periods` and a valid aggregate position for many athlete statistics.
- `CHALLENGES_REMAIN` is a live/in-progress value. Do not expect it in `UNOFFICIAL` or `OFFICIAL` result messages.
- The source sample uses some compact or inconsistent rally-code naming. The BDM message-value table is treated here as
  canonical for `RALLY_TIME_MAX`, `RALLY_STROKES_MAX`, `RALLY_TIME_AVG`, and `RALLY_STROKES_AVG`.

## Code Appendix: Values Directly Visible in Pages 27-39

The section references several code pages. This appendix records values directly visible in the `DT_RESULT` pages and
does not attempt to embed large master-data tables.

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@ResultStatus` | `OdfBody/@ResultStatus` | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` |
| `SC@Period` | `CURRENT_GAME`, `Period/@Code`, statistic `Pos` | Game values such as `G1`, `G2`, plus `TOT` in samples/structure. |
| `SC@Home` | Serve/receive side, point side, home/away roles | `H` and `A` are visible in descriptions/samples. |
| `SC@ResultCode` | `RES_CODE` | Used for bye or walkover; concrete values are not printed in the extracted table text. |
| `SC@IRM` | `Result/@IRM` | No concrete values printed in pages 27-39. |
| `SC@WLT` | `Result/@WLT` | Win/loss/tie result values; examples here use `W` and `L`. |
| `SC@ResultType` | `Result/@ResultType` | Points or IRM-with-points result type; examples here use `POINTS`. |
| `SC@Hand` | `EUE/HAND`, `ENTRY/HAND` | Examples here use `R` and `L`; confirm against the sport code catalog when strict validation is required. |
| Athlete stat codes | `StatsItem/@Code` | `PTS_PLAYED`, `PTS_WON`, `PTS_CONSEC`, `PTS_MATCH`, `PTS_GAME`, `PTS_NOSERVICE`, `PTS_SERVICE`, `LEAD_MAX`, `CBACK_WIN_MAX`, `SERVE_FAULTS`, `PTY`, `CHALLENGE_WON`, `CHALLENGE_LOST`, `SMASH_MAX`. |

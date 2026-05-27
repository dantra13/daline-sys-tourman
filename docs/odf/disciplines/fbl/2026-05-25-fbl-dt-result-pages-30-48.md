# ODF FBL Data Dictionary: DT_RESULT, Pages 30-48

Source: `C:\Users\mella\Downloads\ODF_FBL_Data_Dictionary.pdf`, pages 30-48.

This note restructures the football `DT_RESULT` section into readable Markdown for domain modeling. It covers the Event Unit Start List and Results message, including header metadata, trigger rules, message tree, field definitions, football-specific entries, team statistics, athlete lineup data, sanctions, athlete statistics, and XML examples aligned with the provided ODF2 XSD structure.

## 2.3.4 Event Unit Start List and Results

`DT_RESULT` carries both the start list and result data for one event unit. In football this means the match-level state, score, lineups, officials, team statistics, player metadata, player participation, cards/sanctions, substitutions, and per-player statistics.

The message is mandatory for all sports. It is a full message: when it is emitted, all applicable elements and attributes are resent, not only the changed values.

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Unit` | Full RSC for the event unit. |
| `DocumentSubcode` | N/A | Not used for this message. |
| `DocumentType` | `DT_RESULT` | Event Unit Start List and Results message. |
| `DocumentSubtype` | N/A | Not used for this message. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `ResultStatus` | `CC @ResultStatus` | Result lifecycle status. Football uses `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, and `PROVISIONAL`; the generic XSD also allows other statuses. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time, up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day. Usually the physical day, except when the unit or message transmission crosses midnight. |
| `Source` | `SC @Source` | System that generated the message. |

## Trigger and Frequency

| `ResultStatus` | When to send |
|---|---|
| `START_LIST` | As soon as the team or teams are known before the match. |
| `START_LIST` | As soon as lineups/starters are known, and whenever lineups/starters change before kickoff. |
| `LIVE` | As soon as the unit starts. |
| `LIVE` | At the beginning of each period. |
| `LIVE` | After any change in live data, such as score, substitution, disqualification/disciplinary event, or other match data. |
| `INTERMEDIATE` | During extended breaks, normally after each non-final period. |
| `UNOFFICIAL` | After the unit ends, while results are still unofficial. |
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
   │  │  └─ @StartDate
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
      └─ Competitor (1,1)
         ├─ @Code
         ├─ @Type
         ├─ @Organisation
         ├─ Description (0,1)
         │  └─ @TeamName
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
         │     └─ ExtendedStat (0,N)
         └─ Composition (0,1)
            └─ Athlete (0,N)
               ├─ @Code
               ├─ @Order
               ├─ @StartSortOrder
               ├─ @Bib
               ├─ Description (1,1)
               ├─ EventUnitEntry (0,N)
               ├─ ExtendedResults (0,1)
               └─ StatsItems (0,1)
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional in dictionary, required by XSD | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional in dictionary, required by XSD | `S(20)` | Code-set version applicable to the message. |

## ExtendedInfos / UnitDateTime

Use `UnitDateTime` for actual times. Include it when the unit starts.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `StartDate` | Mandatory | `DateTime` | Actual start date/time for the unit. |

## ExtendedInfos / ExtendedInfo

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `RES_CODE` | N/A | If the match finished in extra time or by penalty shoot-out. | `AET` or `PSO` | `AET` means decided after extra time. `PSO` means decided by penalty shoot-out. |
| `UI` | `PERIOD` | N/A | Always after the start of the unit. | `SC @Period` or `SC @GameState` | Send the current period unless a game state is more appropriate. |
| `DISPLAY` | `GF`, `GA`, `GF_OG`, `SHOT`, `OG`, `PTY`, `FOC`, `FOS`, `FRK`, `ASSIST`, `OFF`, `CRN`, `YC`, `RC`, `YRC`, `DPTY` | Numeric sequence | When available and only while the unit is `LIVE`. Multiple values may be sent. | `S(20)` | Identifies the athlete/team whose result statistic was last updated. `DPTY` is only for Paralympic Games contexts. `ASSIST` and `OFF` are not for Paralympic Games according to the football dictionary note. |

`ExtendedInfo / Extension` may be sent when applicable and only while the unit is `LIVE`.

| Attribute | Value | Meaning |
|---|---:|---|
| `Code` | String | Extension code. |
| `Pos` | N/A | Not used here. |
| `Value` | `Y` | Flag-style extension value. |

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

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Official identifier. |
| `Function` | Mandatory | `CC @ResultsFunction` | Official's function for the result message, such as referee. It can differ from the function sent in `DT_PARTIC`. |
| `Order` | Mandatory | Numeric | Presentation/order value for each official. |

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

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `SC @Period` | Period code. |
| `HomeScore` | Mandatory | Numeric `#0` | Overall home score at the end of the period. |
| `AwayScore` | Mandatory | Numeric `#0` | Overall away score at the end of the period. |
| `HomePeriodScore` | Optional | Numeric `#0` | Home score in this period only. |
| `AwayPeriodScore` | Optional | Numeric `#0` | Away score in this period only. |

### Period / ExtendedPeriods / ExtendedPeriod

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `TIME` | `ADDITIONAL` | N/A | When known/applicable. | `mm` | Additional time for the period. Send `0` when there is no additional time. Remove leading zeroes. |

## Result

Each event-unit result message must contain at least one `Result` competitor.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Result` | Optional | Numeric `#0` | Team result for the event unit, available until extra time is complete. Penalty shoot-out goals are not included. |
| `IRM` | Optional | `SC @IRM` | Invalid result mark for the team. Send when `ResultType` uses both points and IRM. |
| `WLT` | Optional | `SC @WLT` | Whether the competitor won, lost, or tied. |
| `SortOrder` | Mandatory | Numeric | Sequential display order: first/home named team is `1`, away team is `2`. |
| `StartOrder` | Mandatory | Numeric | `1` for first named team, `2` for second named team. |
| `StartSortOrder` | Mandatory | Numeric | Same value as `StartOrder`. |
| `ResultType` | Optional | `SC @ResultType` | Type of `Result`, such as goals/points, or IRM with points for the unit. |

## Result / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes, or `SC @CompetitorPlace` | Competitor ID or placeholder. Placeholders include `TBD` when the competitor is not yet known, `NOCOMP` when no competitor exists and none will arrive later, and `BYE` for a scheduled bye. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Organisation` | Optional | `CC @Organisation` | Competitor organisation. |

### Competitor / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `TeamName` | Mandatory | `S(73)` | Team name. |

### Competitor / Coaches / Coach

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Coach/official code. |
| `Order` | Mandatory | Numeric | Presentation order for coaches and team management, sequential if more than one, following the match form. |
| `Function` | Mandatory | `CC @ResultsFunction` | Team official function. |

### Coach / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Coach gender. |
| `Nationality` | Mandatory in dictionary, optional in XSD | `CC @Country` | Coach nationality. |

## Competitor / EventUnitEntry

These entries hold team-level match metadata.

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `HOME_AWAY` | N/A | When available. | `SC @Home` | Home/away designator. |
| `EUE` | `UNIFORM` | Numeric `0` | If available. | `S(25)` | Shirt colour. `Pos` matches the `ENTRY/UNIFORM` position from `DT_PARTIC_TEAMS`. |
| `EUE` | `SHORTS` | N/A | If available. | String | Shorts colour. |
| `EUE` | `SOCKS` | N/A | If available. | String | Socks colour. |
| `EUE` | `FORMATION` | N/A | If available. | `SC @Formation` | Team formation, for example `4-4-2` or `4-3-3`. |

```xml
<EventUnitEntry Type="EUE" Code="HOME_AWAY" Value="HOME"/>
<EventUnitEntry Type="EUE" Pos="2" Code="UNIFORM" Value="Navy Blue/White"/>
<EventUnitEntry Type="EUE" Pos="2" Code="SHORTS" Value="White"/>
<EventUnitEntry Type="EUE" Pos="2" Code="SOCKS" Value="Navy Blue"/>
<EventUnitEntry Type="EUE" Code="FORMATION" Value="4-3-3"/>
```

## Competitor / StatsItems / StatsItem

Team statistics use `Type="ST"`. For period-scoped statistics, `Pos` is the period code, and `TOT` means totals across all periods.

| Code | Pos | Expected When | Value | Attempt | Meaning |
|---|---|---|---|---|---|
| `MINS` | `SC @Period` or `TOT` | Always, if available. | `mmm` | N/A | Team playing time related to ball possession, in minutes. Remove leading zeroes. |
| `GF_OG` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Goals credited to the team because the opponent scored own goals. |
| `GF` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Goals for the team, including opponent own goals. |
| `GA` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Goals against the team. |
| `ASSIST` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Assists. |
| `SHOT` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | Numeric `#0`, optional | Shots on goal in `Value`; total shots in `Attempt`. |
| `PTY` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | Numeric `#0`, optional | Penalty goals in `Value`; penalty kicks in `Attempt`. |
| `2PTY` | `SC @Period` or `TOT` | Always, if available in Paralympic Games and futsal. | Numeric `#0` | Numeric `#0`, optional | Second/double penalty goals in `Value`; second/double penalty kicks in `Attempt`. |
| `CRN` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Corner kicks. |
| `OFF` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Offsides. |
| `FOC` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Fouls committed. |
| `FOS` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Fouls suffered. |
| `YC` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Cautions/yellow cards. |
| `YRC` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Expulsions caused by a second yellow card. |
| `RC` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Direct red-card expulsions. |
| `EXP` | N/A | Always, if available. | Numeric `#0` | N/A | Total expulsions: second-yellow red cards plus direct red cards. |
| `FRK` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Free kicks. |
| `OG` | `SC @Period` or `TOT` | Always, if available. | Numeric `#0` | N/A | Own goals by the team. |
| `POSSESS` | `SC @Period` or `TOT` | Always, if available. | Numeric `##0` | N/A | Ball possession percentage. |
| `TOUT` | `SC @Period` or `TOT` | Always, if available in Paralympic Games and futsal. | Numeric `#0` | N/A | Time-outs taken. |
| `YC_COACH` | N/A | If applicable and available. | Numeric `#0` | N/A | Yellow cards for coach(es). |
| `YRC_COACH` | N/A | If applicable and available. | Numeric `#0` | N/A | Second-yellow red cards for coach(es). |
| `RC_COACH` | N/A | If applicable and available. | Numeric `#0` | N/A | Red cards for coach(es). |

### Team Stats ExtendedStat

| Parent Code | ExtendedStat Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `SHOT` | `BLC` | N/A | Always, if available. | Numeric `#0` | Blocked shots. |
| `FRK` | `FKD` | N/A | Always, if available. | Numeric `#0` | Direct free kicks. |
| `FRK` | `FKI` | N/A | Always, if available. | Numeric `#0` | Indirect free kicks. |

```xml
<StatsItems>
  <StatsItem Type="ST" Code="MINS" Pos="TOT" Value="38"/>
  <StatsItem Type="ST" Code="GF" Pos="TOT" Value="4"/>
  <StatsItem Type="ST" Code="GA" Pos="TOT" Value="2"/>
  <StatsItem Type="ST" Code="SHOT" Pos="TOT" Attempt="8" Value="6"/>
  <StatsItem Type="ST" Code="CRN" Pos="TOT" Value="6"/>
  <StatsItem Type="ST" Code="OFF" Pos="TOT" Value="3"/>
  <StatsItem Type="ST" Code="FOC" Pos="TOT" Value="8"/>
  <StatsItem Type="ST" Code="FOS" Pos="TOT" Value="8"/>
  <StatsItem Type="ST" Code="FRK" Pos="TOT" Value="12"/>
  <StatsItem Type="ST" Code="POSSESS" Pos="TOT" Value="53"/>
  <StatsItem Type="ST" Code="MINS" Pos="H1" Value="17"/>
  <StatsItem Type="ST" Code="GF" Pos="H1" Value="2"/>
  <StatsItem Type="ST" Code="GA" Pos="H1" Value="2"/>
  <StatsItem Type="ST" Code="SHOT" Pos="H1" Attempt="3" Value="3"/>
  <StatsItem Type="ST" Code="MINS" Pos="H2" Value="21"/>
  <StatsItem Type="ST" Code="GF" Pos="H2" Value="2"/>
  <StatsItem Type="ST" Code="SHOT" Pos="H2" Attempt="5" Value="3"/>
</StatsItems>
```

## Competitor / Composition / Athlete

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Athlete identifier. |
| `Order` | Mandatory | Numeric `#0` | Primary sort order for team members. Before competition it matches `StartSortOrder`; during competition it follows the match result order in ORIS. |
| `StartSortOrder` | Mandatory | Numeric `#0` | Start-list sort order as defined by ORIS. |
| `Bib` | Mandatory | `S(4)` | Shirt number. |

### Athlete / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Athlete gender. |
| `Organisation` | Mandatory | `CC @Organisation` | Athlete organisation. |
| `BirthDate` | Optional | `Date` | Birth date, `YYYY-MM-DD`; must be included when available. |
| `IFId` | Optional | `S(16)` | International Federation ID. |
| `Class` | Optional | `CC @DisciplineClass` | Sport class for events involving athletes with a disability. |

## Athlete / EventUnitEntry

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `STATUS` | N/A | Only for suspended players. | `SC @AthleteStatus` | Athlete status in the team. |
| `EUE` | `CAPTAIN` | N/A | Only for the captain, when known. | `Y` | Captain flag. |
| `EUE` | `STARTER` | N/A | Only for athletes in the starting lineup at the beginning of the game, when available. | `Y` | Starter flag; omit when not a starter. |
| `EUE` | `POSITION` | Numeric `0` | As soon as known. | `CC @Position` for `Pos="1"`; `SC @TacPos` for `Pos="2"` | Player position. `Pos="1"` is the normal playing position, such as `DF` or `FW`. `Pos="2"` is tactical position. |

```xml
<Athlete Code="1130568" Bib="3" Order="3" StartSortOrder="3">
  <Description GivenName="Jane" FamilyName="Smith" Gender="F" Organisation="ESP" BirthDate="1992-12-15"/>
  <EventUnitEntry Type="EUE" Code="STARTER" Value="Y"/>
  <EventUnitEntry Type="EUE" Code="CAPTAIN" Value="Y"/>
  <EventUnitEntry Type="EUE" Code="POSITION" Pos="1" Value="DF"/>
  <EventUnitEntry Type="EUE" Code="POSITION" Pos="2" Value="D05"/>
</Athlete>
```

## Athlete / ExtendedResults / ExtendedResult

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ER` | `SANCTION` | N/A | As soon as available. | `YC` or `EXP` | `YC` for yellow card. `EXP` for suspended. |

## Athlete / StatsItems / StatsItem

Player statistics use `Type="ST"` and are normally not period-scoped in this section unless the value itself encodes minute/period context.

| Code | Pos | Expected When | Value | Attempt | Meaning |
|---|---|---|---|---|---|
| `MINS` | N/A | Always, if available. | `mmm` | N/A | Minutes played by the athlete. Remove leading zeroes. |
| `SUB_TIME` | Numeric `#0` | Always, if available. `Pos` is `1..n` for each substitution action for this athlete. | String such as `+m' [x]`, `-m' [x]`, or `SC @PeriodAction` | N/A | Substitution time. `+` means substituted in; `-` means substituted out. Optional `[x]` captures added/injury time such as `+3`, so `45' +3` is valid. It may also be a period action like `HT`. |
| `GF` | N/A | Always, if available. | Numeric `#0` | N/A | Total goals by the athlete. |
| `GA` | N/A | Always, if available. | Numeric `#0` | N/A | Goals against while the player was playing as goalkeeper. |
| `ASSIST` | N/A | Always, if available. | Numeric `#0` | N/A | Assists. |
| `SHOT` | N/A | Always, if available. | Numeric `#0` | Numeric `#0`, optional | Shots on goal in `Value`; total shots in `Attempt`. |
| `PTY` | N/A | Always, if available. | Numeric `#0` | Numeric `#0`, optional | Penalty goals in `Value`; penalty kicks in `Attempt`. |
| `2PTY` | N/A | Always, if available in Paralympic Games and futsal. | Numeric `#0` | Numeric `#0`, optional | Double penalty goals in `Value`; double penalty kicks in `Attempt`. |
| `FOC` | N/A | Always, if available. | Numeric `#0` | N/A | Fouls committed by the athlete. |
| `FOS` | N/A | Always, if available. | Numeric `#0` | N/A | Fouls suffered by the athlete. |
| `CRN` | N/A | Always, if available. | Numeric `#0` | N/A | Corner kicks by the athlete. |
| `OFF` | N/A | Always, if available; not applicable in Paralympic Games. | Numeric `#0` | N/A | Offsides by the athlete in the unit. |
| `YC` | N/A | Always, if available. | Numeric `#0` | N/A | Yellow cards for the athlete. |
| `YC_MINS` | N/A | Always, if available. | String `m' [x]` or `SC @PeriodAction` | N/A | Minute of first yellow card. Optional `[x]` is added/injury time such as `+3`, so `45' +3` is valid; period actions such as `HT` may also be used. |
| `YRC` | N/A | Always, if available. | Numeric `#0` | N/A | Expulsions by second yellow card. |
| `YRC_MINS` | N/A | Always, if available. | String `m' [x]` or `SC @PeriodAction` | N/A | Minute of second yellow card. |
| `RC` | N/A | Always, if available. | Numeric `#0` | N/A | Direct red-card expulsions. |
| `RC_MINS` | N/A | Always, if available. | String `m' [x]` or `SC @PeriodAction` | N/A | Minute of direct red card. |

```xml
<StatsItems>
  <StatsItem Type="ST" Code="MINS" Value="90"/>
  <StatsItem Type="ST" Code="GF" Value="1"/>
  <StatsItem Type="ST" Code="SHOT" Attempt="2" Value="1"/>
  <StatsItem Type="ST" Code="FOS" Value="3"/>
</StatsItems>
```

## Message Sort

Sort `Result` elements by `Result @SortOrder`.

## XSD-Aligned XML Example

The provided XSD draft could not be loaded directly because `odf2-structure.xsd` references `RecordBrokenType`, which is not defined in the supplied schema folder. The example below was validated against a temporary copy of the schema where only that unrelated unresolved type reference was replaced so the schema could parse. No original XSD files were modified.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="FBLMTEAM11----------FNL-000100--"
         DocumentType="DT_RESULT"
         Version="3"
         ResultStatus="LIVE"
         FeedFlag="T"
         Date="2026-05-25"
         Time="18:42:03.123"
         LogicalDate="2026-05-25"
         Source="OVR">
  <Competition Gen="3.4" Sport="FBL-3.4" Codes="SOG-2024">
    <ExtendedInfos>
      <UnitDateTime StartDate="2026-05-25T18:00:00"/>
      <ExtendedInfo Type="UI" Code="PERIOD" Value="H2"/>
      <ExtendedInfo Type="DISPLAY" Code="GF" Pos="1" Value="T-FBLMTEAM11ESP01"/>
      <SportDescription DisciplineName="Football"
                        EventName="Men"
                        Gender="M"
                        SubEventName="Gold Medal Match"
                        UnitNum="50"/>
      <VenueDescription Venue="STA"
                        VenueName="Stadium A"
                        Location="STA"
                        LocationName="Main pitch"
                        Attendance="45210"/>
    </ExtendedInfos>
    <Officials>
      <Official Code="910001" Function="REF" Order="1">
        <Description GivenName="Alex" FamilyName="Martin" Gender="M" Organisation="FRA"/>
      </Official>
    </Officials>
    <Periods Home="T-FBLMTEAM11ESP01" Away="T-FBLMTEAM11BRA01">
      <Period Code="H1" HomeScore="1" AwayScore="0" HomePeriodScore="1" AwayPeriodScore="0">
        <ExtendedPeriods>
          <ExtendedPeriod Type="TIME" Code="ADDITIONAL" Value="2"/>
        </ExtendedPeriods>
      </Period>
      <Period Code="H2" HomeScore="2" AwayScore="1" HomePeriodScore="1" AwayPeriodScore="1"/>
    </Periods>
    <Result Result="2" WLT="W" SortOrder="1" StartOrder="1" StartSortOrder="1" ResultType="POINTS">
      <Competitor Code="T-FBLMTEAM11ESP01" Type="T" Organisation="ESP">
        <Description TeamName="Spain"/>
        <Coaches>
          <Coach Code="920001" Order="1" Function="HC">
            <Description GivenName="Maria" FamilyName="Lopez" Gender="F" Nationality="ESP"/>
          </Coach>
        </Coaches>
        <EventUnitEntry Type="EUE" Code="HOME_AWAY" Value="HOME"/>
        <EventUnitEntry Type="EUE" Code="FORMATION" Value="4-3-3"/>
        <StatsItems>
          <StatsItem Type="ST" Code="GF" Pos="TOT" Value="2"/>
          <StatsItem Type="ST" Code="SHOT" Pos="TOT" Value="5" Attempt="9">
            <ExtendedStat Code="BLC" Value="2"/>
          </StatsItem>
          <StatsItem Type="ST" Code="POSSESS" Pos="TOT" Value="53"/>
        </StatsItems>
        <Composition>
          <Athlete Code="1130568" Order="3" StartSortOrder="3" Bib="3">
            <Description GivenName="Jane"
                         FamilyName="Smith"
                         Gender="F"
                         Organisation="ESP"
                         BirthDate="1992-12-15"/>
            <EventUnitEntry Type="EUE" Code="STARTER" Value="Y"/>
            <EventUnitEntry Type="EUE" Code="CAPTAIN" Value="Y"/>
            <EventUnitEntry Type="EUE" Code="POSITION" Pos="1" Value="DF"/>
            <StatsItems>
              <StatsItem Type="ST" Code="MINS" Value="90"/>
              <StatsItem Type="ST" Code="YC" Value="1"/>
              <StatsItem Type="ST" Code="YC_MINS" Value="45' +2"/>
            </StatsItems>
          </Athlete>
        </Composition>
      </Competitor>
    </Result>
    <Result Result="1" WLT="L" SortOrder="2" StartOrder="2" StartSortOrder="2" ResultType="POINTS">
      <Competitor Code="T-FBLMTEAM11BRA01" Type="T" Organisation="BRA">
        <Description TeamName="Brazil"/>
        <EventUnitEntry Type="EUE" Code="HOME_AWAY" Value="AWAY"/>
      </Competitor>
    </Result>
  </Competition>
</OdfBody>
```

## Modeling Notes

- `DT_RESULT` is the canonical output shape for a football match unit. Internally, the model should separate match lifecycle, score state, team result rows, team lineup, staff assignments, player participation, events/sanctions, and aggregated statistics, then map them into this full-message output.
- Home/away is not only schedule metadata. It is repeated as `Competitor/EventUnitEntry Code="HOME_AWAY"`, so the output model needs the assignment at competitor-in-unit level.
- The `Result` element is team-scoped. Player data lives under the team's `Competitor/Composition/Athlete`, not as peer results.
- `Order`, `StartOrder`, `SortOrder`, and `StartSortOrder` are output contracts. Preserve the distinction between original lineup/start-list order and live/result presentation order.
- Team stats and athlete stats use the same generic `StatsItem` structure, but their allowed football codes and meanings differ by location. Treat the path as part of the semantic key, not just the `Code`.
- Many values are conditional on availability. The emitter should avoid inventing unknown values; optional fields should be omitted when the source data is genuinely unavailable.
- `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, and `OFFICIAL` imply a result lifecycle that the internal domain model should map cleanly, even if local tournaments simplify the operational workflow.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-25. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index
| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @IRM` | FBL | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_FBL.htm) |
| `SC @WLT` | FBL | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_WLT_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_WLT_SOG_FBL.htm) |
| `SC @ResultType` | FBL | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_FBL.htm) |
| `SC @Period` | FBL | 12 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_FBL.htm) |
| `SC @Source` | FBL | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm) |
| `SC @AthleteStatus` | FBL | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_AthleteStatus_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_AthleteStatus_SOG_FBL.htm) |
| `SC @Home` | FBL aggregate | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @CompetitorPlace` | FBL aggregate | 30 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @Formation` | FBL aggregate | 20 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @GameState` | FBL aggregate | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @PeriodAction` | FBL aggregate | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @TacPos` | FBL aggregate | 64 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `CC @ResultStatus` | Common | 9 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm) |
| `CC @SportGender` | Common | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm) |
| `CC @ResultsFunction` | FBL rows from DisciplineFunction | 22 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineFunction.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineFunction.htm) |
| `CC @PersonGender` | Common | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm) |
| `CC @Position` | FBL rows from Positions | 4 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Positions.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Positions.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @VenueCode` | FBL venues inferred through FBL locations | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm) |
| `CC @Location` | FBL locations | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm) |
| `CC @Unit` | FBL EventUnit rows | 93 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @Country` | Common master data | 266 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm) |
| `CC @Organisation` | Common master data | 258 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |
| `CC @DisciplineClass` | Not found in Paris 2024 common-code index | 0 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm) |

### SC @IRM
| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| ABD |  |  | Abandoned |
| DNS |  |  | Did Not Start |
| DQB |  |  | Disqualified for unsportsmanlike behaviour |
| DSQ |  |  | Disqualified |
| WDR |  |  | Withdrawn |

### SC @WLT
| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| L |  |  | Runner-up |
| T |  |  | Drawn |
| W |  |  | Winner |

### SC @ResultType
| Code | Note | ENG Description |
| --- | --- | --- |
| IRM_POINTS |  | For both, Points and Invalid Result Mark |
| POINTS |  | Goals |

### SC @Period
| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| H1 | 1 |  | First half |
| HT | 2 |  | Half-time |
| H2 | 3 |  | Second half |
| FT | 4 |  | Full time |
| RT | 5 |  | Regular Time |
| PET | 6 |  | Pre extra time |
| ET-H1 | 7 |  | First half of extra time |
| ET-HT | 8 |  | Half-time of extra time |
| ET-H2 | 9 |  | Second half of extra time |
| PSO | 10 |  | Penalty shoot-out |
| TOT | 11 |  | Total |
| E-RT |  |  | Regular Time |

### SC @Source
| Code | Note | ENG Description |
| --- | --- | --- |
| BORFBL1 |  | Origin for messages from OVR at BOR for FBL |
| LYOFBL1 |  | Origin for messages from OVR at LYO for FBL |
| MRSFBL1 |  | Origin for messages from OVR at MRS for FBL |
| NANFBL1 |  | Origin for messages from OVR at NAN for FBL |
| NICFBL1 |  | Origin for messages from OVR at NIC for FBL |
| PDPFBL1 |  | Origin for messages from OVR at PDP for FBL |
| STEFBL1 |  | Origin for messages from OVR at STE for FBL |

### SC @Home
| Code | Note | ENG Description |
| --- | --- | --- |
| AWAY |  | Away |
| HOME |  | Home |

### SC @CompetitorPlace
| Code | Note | ENG Description |
| --- | --- | --- |
| A1 |  | 1A |
| A2 |  | 2A |
| A3/B3 |  | 3A/3B |
| B1 |  | 1B |
| B2 |  | 2B |
| B3/C3 |  | 3B/3C |
| BYE | There is no competitor; the other team/athlete goes directly to the next phase/round | Bye |
| C1 |  | 1C |
| C2 |  | 2C |
| D1 |  | 1D |
| D2 |  | 2D |
| L23 |  | Runner-up 23 |
| L24 |  | Runner-up 24 |
| L29 |  | Runner-up 29 |
| L30 |  | Runner-up 30 |
| NOAWARD |  | Not awarded |
| NOCOMP |  | No competitor |
| TBD |  | To be determined |
| W19 |  | Winner 19 |
| W20 |  | Winner 20 |
| W21 |  | Winner 21 |
| W22 |  | Winner 22 |
| W23 |  | Winner 23 |
| W24 |  | Winner 24 |
| W25 |  | Winner 25 |
| W26 |  | Winner 26 |
| W27 |  | Winner 27 |
| W28 |  | Winner 28 |
| W29 |  | Winner 29 |
| W30 |  | Winner 30 |

### SC @AthleteStatus
| Code | Note | ENG Description |
| --- | --- | --- |
| ABSENT |  | Absent |
| BOOKED |  | Misses next match if booked |
| DQB |  | Disqualified for unsportmanlike conduct |
| DSQ |  | Disqualified |
| INJURED |  | Injured |
| REPLACED |  | Replaced |
| SUSPEND |  | Not eligible to play |

### SC @Formation
| Code | ENG Description |
| --- | --- |
| 3-4-1-2 | 3-4-1-2 |
| 3-4-3 | 3-4-3 |
| 3-5-2 | 3-5-2 |
| 4-1-2-1-2 | 4-1-2-1-2 |
| 4-1-2-3 | 4-1-2-3 |
| 4-1-3-2 | 4-1-3-2 |
| 4-1-4-1 | 4-1-4-1 |
| 4-2-1-2-1 | 4-2-1-2-1 |
| 4-2-1-3 | 4-2-1-3 |
| 4-2-2-2 | 4-2-2-2 |
| 4-2-3-1 | 4-2-3-1 |
| 4-3-1-2 | 4-3-1-2 |
| 4-3-2-1 | 4-3-2-1 |
| 4-3-3 | 4-3-3 |
| 4-4-1-1 | 4-4-1-1 |
| 4-4-2 | 4-4-2 |
| 4-5-1 | 4-5-1 |
| 5-2-3 | 5-2-3 |
| 5-3-2 | 5-3-2 |
| 5-4-1 | 5-4-1 |

### SC @GameState
| Code | Note | ENG Description |
| --- | --- | --- |
| E-ET2 |  | After extra time |
| E-ET-HT |  | Half-time of extra time |
| E-FT |  | Full time |
| E-HT |  | Half-time |
| E-PET |  | Pre extra time |

### SC @PeriodAction
| Code | Note | ENG Description |
| --- | --- | --- |
| ET-HT |  | Extra Time Half Time |
| FT |  | Full Time |
| HT |  | Half-Time |
| PET |  | Pre extra time |
| PSO |  | Penalty Shoot Out |

### SC @TacPos
| Code | Order | ENG Description |
| --- | --- | --- |
| GK | 1 | Goalkeeper |
| D00 | 2 | Defender |
| D01 | 3 | Defender |
| D02 | 4 | Defender |
| D03 | 5 | Defender |
| D04 | 6 | Defender |
| D05 | 7 | Defender |
| D06 | 8 | Defender |
| D07 | 9 | Defender |
| D08 | 10 | Defender |
| M00 | 11 | Midfielder |
| M01 | 12 | Midfielder |
| M02 | 13 | Midfielder |
| M03 | 14 | Midfielder |
| M04 | 15 | Midfielder |
| M05 | 16 | Midfielder |
| M06 | 17 | Midfielder |
| M07 | 18 | Midfielder |
| M08 | 19 | Midfielder |
| M10 | 20 | Midfielder |
| M11 | 21 | Midfielder |
| M12 | 22 | Midfielder |
| M13 | 23 | Midfielder |
| M14 | 24 | Midfielder |
| M15 | 25 | Midfielder |
| M16 | 26 | Midfielder |
| M17 | 27 | Midfielder |
| M18 | 28 | Midfielder |
| M20 | 29 | Midfielder |
| M21 | 30 | Midfielder |
| M22 | 31 | Midfielder |
| M23 | 32 | Midfielder |
| M24 | 33 | Midfielder |
| M25 | 34 | Midfielder |
| M26 | 35 | Midfielder |
| M27 | 36 | Midfielder |
| M28 | 37 | Midfielder |
| M30 | 38 | Midfielder |
| M31 | 39 | Midfielder |
| M32 | 40 | Midfielder |
| M33 | 41 | Midfielder |
| M34 | 42 | Midfielder |
| M35 | 43 | Midfielder |
| M36 | 44 | Midfielder |
| M37 | 45 | Midfielder |
| M38 | 46 | Midfielder |
| M40 | 47 | Midfielder |
| M41 | 48 | Midfielder |
| M42 | 49 | Midfielder |
| M43 | 50 | Midfielder |
| M44 | 51 | Midfielder |
| M45 | 52 | Midfielder |
| M46 | 53 | Midfielder |
| M47 | 54 | Midfielder |
| M48 | 55 | Midfielder |
| F00 | 56 | Forward |
| F01 | 57 | Forward |
| F02 | 58 | Forward |
| F03 | 59 | Forward |
| F04 | 60 | Forward |
| F05 | 61 | Forward |
| F06 | 62 | Forward |
| F07 | 63 | Forward |
| F08 | 64 | Forward |

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

### CC @ResultsFunction (FBL rows)
| Function | Order | Category | Partic | ENG Description |
| --- | --- | --- | --- | --- |
| AA01 | 0 | A | Y | Athlete |
| AP01 | 1 | A | Y | Alternate Athlete |
| COACH | 2 | C | Y | Head Coach |
| SI_COA | 3 | C | Y | Stand-in Coach |
| AST_COA | 4 | C | Y | Assistant Coach |
| GK_COA | 5 | C | Y | Goalkeeper Coach |
| TM_OFFIC | 6 | T | Y | Team Official |
| DOCTOR | 7 | T | Y | Doctor |
| RE | 8 | J | Y | Referee |
| AR | 9 | J | Y | Assistant Referee |
| AR1 | 10 | J | Y | Assistant Referee 1 |
| AR2 | 11 | J | Y | Assistant Referee 2 |
| 4O | 12 | S | Y | 4th Official |
| RAR | 13 | J | Y | Reserve Assistant Referee |
| MCS | 14 | S | Y | Match Commissioner |
| MCH_DIR | 15 | S | Y | Match Director |
| VMO | 16 | J | Y | Video Match Official |
| VAR | 17 | J | Y | Video Assistant Referee |
| AVAR | 18 | J | Y | Assistant VAR (AVAR) |
| AVAR1 | 19 | J | Y | Assistant VAR 1 |
| AVAR2 | 20 | J | Y | Assistant VAR 2 |
| AVAR3 | 21 | J | Y | Assistant VAR 3 |

### CC @PersonGender
| Id | ENG Description |
| --- | --- |
| F | Female |
| M | Male |
| X | Unspecified |

### CC @Position (FBL rows)
| Id | PositionOrder | ENG Description |
| --- | --- | --- |
| DF |  | Defender |
| FW |  | Forward |
| GK |  | Goalkeeper |
| MF |  | Midfielder |

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

### CC @Location (FBL rows)
| Id | Venue | ENG longdescription | ENG Description |
| --- | --- | --- | --- |
| BOR | BOR | Bordeaux Stadium, Bordeaux | Bordeaux Stadium, Bordeaux |
| LYO | LYO | Lyon Stadium, Lyon | Lyon Stadium, Lyon |
| MRS | MRS | Marseille Stadium, Marseille | Marseille Stadium, Marseille |
| NAN | NAN | La Beaujoire Stadium, Nantes | La Beaujoire Stadium, Nantes |
| NIC | NIC | Nice Stadium, Nice | Nice Stadium, Nice |
| PDP | PDP | Parc des Princes, Paris | Parc des Princes, Paris |
| STE | STE | Geoffroy-Guichard Stadium, Saint-Etienne | Geoffroy-Guichard, St-Etienne |

### CC @VenueCode (FBL venues inferred from FBL locations)
| Id | ENG longdescription | ENG Description |
| --- | --- | --- |
| BOR | Bordeaux Stadium | Bordeaux Stadium |
| LYO | Lyon Stadium | Lyon Stadium |
| MRS | Marseille Stadium | Marseille Stadium |
| NAN | La Beaujoire Stadium | La Beaujoire Stadium |
| NIC | Nice Stadium | Nice Stadium |
| PDP | Parc des Princes | Parc des Princes |
| STE | Geoffroy-Guichard Stadium | Geoffroy-Guichard Stadium |

### CC @Unit (FBL EventUnit rows)
| Code | Gender | Event | phase | Eventunit | Level | Order | schedule | medalflag | ENG Description |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| FBL------------------------------- | - | ------------------ | ---- | -------- | Discipline | 2 | N | 0 | Football |
| FBL-----------------------BOR----- | - | ------------------ | ---- | BOR----- | Venue | 3 | N | 0 | BOR |
| FBL-----------------------LYO----- | - | ------------------ | ---- | LYO----- | Venue | 3 | N | 0 | LYO |
| FBLM------------------------------ | M | ------------------ | ---- | -------- | Gender | 10 | N | 0 | Men |
| FBL-----------------------MRS----- | - | ------------------ | ---- | MRS----- | Venue | 3 | N | 0 | MRS |
| FBLMTEAM11------------------------ | M | TEAM11------------ | ---- | -------- | Event | 13 | N | 0 | Men |
| FBLMTEAM11------------FNL--------- | M | TEAM11------------ | FNL- | -------- | Phase | 14 | N | 0 | Men's Finals |
| FBLMTEAM11------------FNL-000100-- | M | TEAM11------------ | FNL- | 000100-- | Unit | 15 | Y | 1 | Men's Gold Medal Match |
| FBLMTEAM11------------FNL-000200-- | M | TEAM11------------ | FNL- | 000200-- | Unit | 15 | Y | 3 | Men's Bronze Medal Match |
| FBLMTEAM11------------GP---------- | M | TEAM11------------ | GP-- | -------- | Phase | 14 | N | 0 | Men's Group Stage |
| FBLMTEAM11------------GPA--------- | M | TEAM11------------ | GPA- | -------- | Phase | 14 | N | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000100-- | M | TEAM11------------ | GPA- | 000100-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000200-- | M | TEAM11------------ | GPA- | 000200-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000300-- | M | TEAM11------------ | GPA- | 000300-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000400-- | M | TEAM11------------ | GPA- | 000400-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000500-- | M | TEAM11------------ | GPA- | 000500-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000600-- | M | TEAM11------------ | GPA- | 000600-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPB--------- | M | TEAM11------------ | GPB- | -------- | Phase | 14 | N | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000100-- | M | TEAM11------------ | GPB- | 000100-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000200-- | M | TEAM11------------ | GPB- | 000200-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000300-- | M | TEAM11------------ | GPB- | 000300-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000400-- | M | TEAM11------------ | GPB- | 000400-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000500-- | M | TEAM11------------ | GPB- | 000500-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000600-- | M | TEAM11------------ | GPB- | 000600-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPC--------- | M | TEAM11------------ | GPC- | -------- | Phase | 14 | N | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000100-- | M | TEAM11------------ | GPC- | 000100-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000200-- | M | TEAM11------------ | GPC- | 000200-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000300-- | M | TEAM11------------ | GPC- | 000300-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000400-- | M | TEAM11------------ | GPC- | 000400-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000500-- | M | TEAM11------------ | GPC- | 000500-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000600-- | M | TEAM11------------ | GPC- | 000600-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPD--------- | M | TEAM11------------ | GPD- | -------- | Phase | 14 | N | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000100-- | M | TEAM11------------ | GPD- | 000100-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000200-- | M | TEAM11------------ | GPD- | 000200-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000300-- | M | TEAM11------------ | GPD- | 000300-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000400-- | M | TEAM11------------ | GPD- | 000400-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000500-- | M | TEAM11------------ | GPD- | 000500-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000600-- | M | TEAM11------------ | GPD- | 000600-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------QFNL-------- | M | TEAM11------------ | QFNL | -------- | Phase | 14 | N | 0 | Men's Quarter-finals |
| FBLMTEAM11------------QFNL000100-- | M | TEAM11------------ | QFNL | 000100-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000200-- | M | TEAM11------------ | QFNL | 000200-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000300-- | M | TEAM11------------ | QFNL | 000300-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000400-- | M | TEAM11------------ | QFNL | 000400-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------SFNL-------- | M | TEAM11------------ | SFNL | -------- | Phase | 14 | N | 0 | Men's Semi-finals |
| FBLMTEAM11------------SFNL000100-- | M | TEAM11------------ | SFNL | 000100-- | Unit | 15 | Y | 0 | Men's Semi-final |
| FBLMTEAM11------------SFNL000200-- | M | TEAM11------------ | SFNL | 000200-- | Unit | 15 | Y | 0 | Men's Semi-final |
| FBLMTEAM11------------VICT-------- | M | TEAM11------------ | VICT | -------- | Phase | 14 | N | 0 | Men's Victory Ceremony |
| FBLMTEAM11------------VICTBRONZE-- | M | TEAM11------------ | VICT | BRONZE-- | Medals | 17 | Y | 0 | Men's Bronze Ceremony |
| FBLMTEAM11------------VICTMEDAL--- | M | TEAM11------------ | VICT | MEDAL--- | Medals | 17 | Y | 0 | Men's Victory Ceremony |
| FBL-----------------------NAN----- | - | ------------------ | ---- | NAN----- | Venue | 3 | N | 0 | NAN |
| FBL-----------------------NIC----- | - | ------------------ | ---- | NIC----- | Venue | 3 | N | 0 | NIC |
| FBL-----------------------PDP----- | - | ------------------ | ---- | PDP----- | Venue | 3 | N | 0 | PDP |
| FBL-----------------------STE----- | - | ------------------ | ---- | STE----- | Venue | 3 | N | 0 | STE |
| FBL-----------------------STR----- | - | ------------------ | ---- | STR----- | Venue | 3 | N | 0 | STR |
| FBL-----------------------TOU----- | - | ------------------ | ---- | TOU----- | Venue | 3 | N | 0 | TOU |
| FBLW------------------------------ | W | ------------------ | ---- | -------- | Gender | 10 | N | 0 | Women |
| FBLWTEAM11------------------------ | W | TEAM11------------ | ---- | -------- | Event | 13 | N | 0 | Women |
| FBLWTEAM11------------FNL--------- | W | TEAM11------------ | FNL- | -------- | Phase | 14 | N | 0 | Women's Finals |
| FBLWTEAM11------------FNL-000100-- | W | TEAM11------------ | FNL- | 000100-- | Unit | 15 | Y | 1 | Women's Gold Medal Match |
| FBLWTEAM11------------FNL-000200-- | W | TEAM11------------ | FNL- | 000200-- | Unit | 15 | Y | 3 | Women's Bronze Medal Match |
| FBLWTEAM11------------GP---------- | W | TEAM11------------ | GP-- | -------- | Phase | 14 | N | 0 | Women's Group Stage |
| FBLWTEAM11------------GPA--------- | W | TEAM11------------ | GPA- | -------- | Phase | 14 | N | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000100-- | W | TEAM11------------ | GPA- | 000100-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000200-- | W | TEAM11------------ | GPA- | 000200-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000300-- | W | TEAM11------------ | GPA- | 000300-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000400-- | W | TEAM11------------ | GPA- | 000400-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000500-- | W | TEAM11------------ | GPA- | 000500-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000600-- | W | TEAM11------------ | GPA- | 000600-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPB--------- | W | TEAM11------------ | GPB- | -------- | Phase | 14 | N | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000100-- | W | TEAM11------------ | GPB- | 000100-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000200-- | W | TEAM11------------ | GPB- | 000200-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000300-- | W | TEAM11------------ | GPB- | 000300-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000400-- | W | TEAM11------------ | GPB- | 000400-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000500-- | W | TEAM11------------ | GPB- | 000500-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000600-- | W | TEAM11------------ | GPB- | 000600-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPC--------- | W | TEAM11------------ | GPC- | -------- | Phase | 14 | N | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000100-- | W | TEAM11------------ | GPC- | 000100-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000200-- | W | TEAM11------------ | GPC- | 000200-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000300-- | W | TEAM11------------ | GPC- | 000300-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000400-- | W | TEAM11------------ | GPC- | 000400-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000500-- | W | TEAM11------------ | GPC- | 000500-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000600-- | W | TEAM11------------ | GPC- | 000600-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------QFNL-------- | W | TEAM11------------ | QFNL | -------- | Phase | 14 | N | 0 | Women's Quarter-finals |
| FBLWTEAM11------------QFNL000100-- | W | TEAM11------------ | QFNL | 000100-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000200-- | W | TEAM11------------ | QFNL | 000200-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000300-- | W | TEAM11------------ | QFNL | 000300-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000400-- | W | TEAM11------------ | QFNL | 000400-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------SFNL-------- | W | TEAM11------------ | SFNL | -------- | Phase | 14 | N | 0 | Women's Semi-finals |
| FBLWTEAM11------------SFNL000100-- | W | TEAM11------------ | SFNL | 000100-- | Unit | 15 | Y | 0 | Women's Semi-final |
| FBLWTEAM11------------SFNL000200-- | W | TEAM11------------ | SFNL | 000200-- | Unit | 15 | Y | 0 | Women's Semi-final |
| FBLWTEAM11------------VICT-------- | W | TEAM11------------ | VICT | -------- | Phase | 14 | N | 0 | Women's Victory Ceremony |
| FBLWTEAM11------------VICTBRONZE-- | W | TEAM11------------ | VICT | BRONZE-- | Medals | 17 | Y | 0 | Women's Bronze Ceremony |
| FBLWTEAM11------------VICTMEDAL--- | W | TEAM11------------ | VICT | MEDAL--- | Medals | 17 | Y | 0 | Women's Victory Ceremony |

### CC @Country and CC @Organisation

These are large common master-data tables rather than football match-state enumerations. The downloaded Paris 2024 code pages contain 266 `Country` rows and 258 `Organisation` rows. Use the source links in the index above as the authoritative value lists when modeling `Nationality` and `Organisation` fields.

### CC @DisciplineClass

The football dictionary references `CC @DisciplineClass` for para sport classification, but the Paris 2024 common-code index downloaded for this appendix does not expose a `DisciplineClass.htm` page. Treat this as an externally supplied classification code set and validate it against the relevant para-sport dictionary/code release when that scope is implemented.

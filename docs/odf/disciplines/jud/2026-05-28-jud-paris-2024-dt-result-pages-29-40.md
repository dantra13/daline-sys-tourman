# ODF JUD Data Dictionary: Paris 2024 DT_RESULT, Pages 29-40

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 29-40.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note restructures the Paris 2024 Judo `DT_RESULT` section. It is the key section for team units and subunits:
`DocumentCode` can identify either the team match unit or a contest within the team match, and team-match units use
`ExtendedInfo Type="TEAM" Code="COMP"` to enumerate subunit contests.

## 2.3.4 Event Unit Start List and Results

`DT_RESULT` contains start-list and result information for one individual or team event unit. It is always a full
message: all applicable elements and attributes are sent.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Unit` | Unit RSC; includes team match and contests within team match. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_RESULT` | Event Unit Start List and Results. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | `1..V` | Ascending content version. |
| `ResultStatus` | `CC@ResultStatus` | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test. |
| `Date` / `Time` / `LogicalDate` | Date/time | Header generation and logical dates. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Status | Trigger |
|---|---|
| `START_LIST` | As soon as each competitor is known, and after start-list changes. |
| `LIVE` | When the contest starts and after every data change. |
| `INTERMEDIATE` | Unexpected interruptions, not normal period boundaries. |
| `UNOFFICIAL` / `OFFICIAL` | After the contest. |

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
    +-- Result (1,N)
        +-- @Result
        +-- @IRM
        +-- @WLT
        +-- @SortOrder
        +-- @StartSortOrder
        +-- @ResultType
        +-- @Pty
        +-- ExtendedResults (0,1)
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Description (0,1)
            +-- Composition (0,1)
                +-- Athlete (0,N)
                    +-- Description (1,1)
                    +-- EventUnitEntry (0,N)
                    +-- ExtendedResults (0,1)
```

## Message Values

### `Competition / ExtendedInfos / UnitDateTime`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `StartDate` | M | DateTime | Actual start. |
| `EndDate` | O | DateTime | Actual end. |
| `Duration` | O | `mm:ss` | Bout duration for individual/team subunit contests, or all bouts duration for a team match result. |

### `ExtendedInfo` for Individual Units or Team Subunits

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `RES_CODE` | N/A | When available in individual contests, including within team competition. | `SC@ResultCode` | Decision for how the contest was won. |
| `UI` | `GOLD_SCORE` | N/A | If applicable. | `Y` | Gold-score flag. |
| `UI` | `TECH_CODE` | N/A | As appropriate. | `SC@Technique` | Winning technique code. |
| `UI` | `TECH_DESCRIPTION` | N/A | As appropriate. | String | Winning technique description. |

For `GOLD_SCORE`, extension `Code="DURATION"` uses `Pos="1"` for regular time and `Pos="2"` for golden-score time.

### `ExtendedInfo` for Team Units

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `TEAM` | `COMP` | Sequential contest number | Team unit only, when information is available. | `CC@Unit` | Full RSC of the subunit contest. |

Extensions under `TEAM/COMP`:

| Extension Code | Value | Meaning |
|---|---|---|
| `WEIGHT_CATEGORY` | `CC@Unit` | Event-level RSC for this bout within the team match. |
| `HOME` | Athlete ID | Home-team athlete in the bout. |
| `AWAY` | Athlete ID | Away-team athlete in the bout. |
| `DURATION` | `mm:ss`, with `Pos=1`, `2`, or `TOT` | Regular time, golden-score time, or total bout time. |
| `GOLD_SCORE` | `Y` | Bout is in gold score. |
| `STATUS` | `CC@ScheduleStatus` | Team-match contest status: `SCHEDULED`, `RUNNING`, `FINISHED`, `CANCELLED`. |

### `SportDescription` and `VenueDescription`

| Path | Key Attributes |
|---|---|
| `SportDescription` | `DisciplineName`, `EventName`, `Gender`, `SubEventName`, `UnitNum`. |
| `VenueDescription` | `Venue`, `VenueName`, `Location`, `LocationName`. |

### `Officials / Official`

Officials carry `Code`, `Function`, `Order`, optional `Bib`, and a required `Description` with name, gender,
organisation, and optional `IFId`.

### `Result`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Result` | O | String | Competitor score; penalties are not included. |
| `IRM` | O | `SC@IRM` | Invalid rank mark, including DNS before competition. |
| `WLT` | O | `SC@WLT` | Win/loss; send `L` for no-winner cases. |
| `SortOrder` | M | Numeric | Result order; individual contests use `1` white, `2` blue. |
| `StartSortOrder` | M | Numeric | Start-list order; individual contests use `1` white, `2` blue. |
| `ResultType` | O | `SC@ResultType` | Points or IRM result type. |
| `Pty` | O | `SC@PenaltyType` | Penalty code associated to the score. |

### `Result / ExtendedResults / ExtendedResult`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ER` | `OUTCOME` | N/A | When available. | `SC@Outcome` | Athlete progression. |
| `ER` | `SC@PenaltyType` | `1`, `2`, `TOT` | When available. | Numeric | Penalties by regular time, golden score, or total. Send `S=0` when no penalties. |
| `ER` | `SC@PointsType` | `1`, `2`, `TOT` | As soon as known. | Numeric | Scores by regular time, golden score, or total. |

### `Competitor`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | ID, `TBD`, `NCT` | Competitor ID or placeholder. |
| `Type` | M | `A`, `T` | Athlete or team. |
| `Organisation` | O | `CC@Organisation` | Organisation. |

Team competitors can include `Description/@TeamName`. Athlete composition uses `Athlete/@Code`, `@Order`, personal
description, and event-unit entries.

### `Athlete / EventUnitEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `BODYWEIGHT` | N/A | Always. | `##0.0` | Bodyweight. |
| `EUE` | `COLOUR` | N/A | Always. | `SC@Colour` | Athlete colour. |
| `EUE` | `RANK_WLD` | N/A | When available. | `S(3)` | World ranking. |
| `EUE` | `RESULT_BEST` | N/A | When available. | `S(30)` | Best prior achievement. |

### Team-Member Contest Results

Only in team events, under `Result/Competitor/Composition/Athlete/ExtendedResults/ExtendedResult`:

| Type | Code | Pos | Value | Meaning |
|---|---|---|---|---|
| `ER` | `CONTEST` | Contest number matching `TEAM/COMP` | String | Team member result in this contest, not cumulative. |

Extensions:

| Extension Code | Value | Meaning |
|---|---|---|
| `IRM` | `SC@IRM` | IRM granted in the related bout. |
| `WLT` | `W`, `L` | Team member win/loss for the contest. |

## Samples, Normalized

### Team unit subunit map

```xml
<ExtendedInfos>
  <ExtendedInfo Type="TEAM" Code="COMP" Pos="1" Value="JUDXTEAM5-------------8FNL00010001">
    <Extension Code="WEIGHT_CATEGORY" Value="JUDW44KG--------------------------"/>
    <Extension Code="HOME" Value="5285271"/>
    <Extension Code="AWAY" Value="5285128"/>
    <Extension Code="GOLD_SCORE" Value="Y"/>
  </ExtendedInfo>
  <ExtendedInfo Type="TEAM" Code="COMP" Pos="2" Value="JUDXTEAM5-------------8FNL00010002">
    <Extension Code="WEIGHT_CATEGORY" Value="JUDWO70KG-------------------------"/>
    <Extension Code="HOME" Value="5285269"/>
    <Extension Code="AWAY" Value="5285116"/>
    <Extension Code="DURATION" Value="4:00"/>
    <Extension Code="STATUS" Value="FINISHED"/>
  </ExtendedInfo>
</ExtendedInfos>
```

### Individual contest result

```xml
<Result ResultType="POINTS" Result="11" WLT="W" SortOrder="1" StartOrder="1" StartSortOrder="1">
  <ExtendedResults>
    <ExtendedResult Type="ER" Code="S3" Pos="1" Value="1"/>
    <ExtendedResult Type="ER" Code="IPP" Pos="1" Value="1"/>
    <ExtendedResult Type="ER" Code="WAZ" Pos="1" Value="1"/>
    <ExtendedResult Type="ER" Code="OUTCOME" Value="ABC"/>
  </ExtendedResults>
</Result>
```

## Message Sort

Sort by `Result/@SortOrder`.

## Modeling Notes

- Model team match units and team subunits separately. A team unit references its subunit contests through
  `TEAM/COMP`; individual contest payloads still use the normal `UI` result fields.
- `TEAM/COMP/@Pos` is the contest number used again by athlete-level `ER/CONTEST`.
- `Result/@Result` for team competitors is the team match score; individual points and penalties are in detailed
  result extensions.
- Do not schedule team subunits separately; connect them from `DT_RESULT`.

## Code Appendix: Paris 2024 Values

Catalog values come from Paris 2024 CC/SC code tables; message-specific restrictions remain in the field tables above.

| Code Entity | Section Usage | Values |
|---|---|---|
| `DocumentType` | Header | `DT_RESULT` |
| `CC@ResultStatus` | Header | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNCONFIRMED`, `UNOFFICIAL`, `OFFICIAL`, `PARTIAL`, `PROTESTED`, `PROVISIONAL` |
| `SC@ResultCode` | Contest decision | `FUS`, `IPP`, `KIK`, `PEN`, `WAZ`, `YUK` |
| `SC@PointsType` | Score extensions | `IPP`, `WAZ`, `YUK` |
| `SC@PenaltyType` | Penalty extensions | `H`, `S`, `S3`, `X`, `s1`, `s2`, `s3` |
| `SC@ResultType` | Competitor result type | `IRM`, `IRM_POINTS`, `POINTS` |
| `SC@IRM` | Invalid result mark | `DNS`, `DQB`, `DSQ`, `WDR` |
| `SC@WLT` | Win/loss | `W`, `L` |
| `SC@Outcome` | Progression/ranking outcome | `PROG_8FNL`, `PROG_FNL`, `PROG_QFNL`, `PROG_R32`, `PROG_REP1`, `PROG_REPF`, `PROG_SFNL`, `STOP1`, `STOP2`, `STOP3`, `STOP5`, `STOP7`, `STOP9`, `STOP17`, `STOP33` |
| `SC@Colour` | Athlete colour | `BLUE`, `WHITE` |
| `ExtendedInfo` codes | Unit state | `RES_CODE`, `GOLD_SCORE`, `TECH_CODE`, `TECH_DESCRIPTION`, `COMP` |
| `TEAM/COMP` extensions | Team subunits | `WEIGHT_CATEGORY`, `HOME`, `AWAY`, `DURATION`, `GOLD_SCORE`, `STATUS` |
| `CC@ScheduleStatus` | Team contest state | `CANCELLED`, `DELAYED`, `FINISHED`, `GETTING_READY`, `INTERRUPTED`, `POSTPONED`, `RESCHEDULED`, `RUNNING`, `SCHEDULED`, `SCHEDULED_BREAK`, `UNSCHEDULED` |
| `ExtendedResult` codes | Result detail | `OUTCOME`, `CONTEST`, plus `SC@PointsType` (`IPP`, `WAZ`, `YUK`) and `SC@PenaltyType` (`H`, `S`, `S3`, `X`, `s1`, `s2`, `s3`) |
| `EventUnitEntry` codes | Athlete facts | `BODYWEIGHT`, `COLOUR`, `RANK_WLD`, `RESULT_BEST` |
| `Competitor/@Type` | Competitor kind | `A`, `T` |
| `SC@CompetitorPlace` | Placeholders | `BYE`, `NCT`, `NOAWARD`, `NOCOMP`, `TBD` |
| `SC@Technique` | Winning technique | Large JUD technique catalog in SportCodes; keep as code reference instead of hard-coding a partial list. |

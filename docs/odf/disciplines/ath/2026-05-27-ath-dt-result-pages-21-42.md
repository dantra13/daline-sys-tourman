# ODF ATH Data Dictionary: DT_RESULT, Pages 21-42

Source: `C:\Users\mella\Downloads\ODF_ATH_Data_Dictionary.pdf`, pages 21-42.

Source version: `SOG-2024-ATH-3.4 APP`, dated 3 May 2024.

This note restructures the Athletics `DT_RESULT` section into a domain reference for the Event Unit Start List and
Results message. In Athletics this is the core unit-level message for heats, groups, race units, field-event groups,
relays, combined-event units, and team walking events. It carries both start-list data and result data for the
competitors in one event unit.

## 2.3.3 Event Unit Start List and Results

`DT_RESULT` is mandatory for all sports. In ATH it is always a full message: each emission sends all applicable
elements and attributes for the unit, not just changed values.

The ATH overview states that every event has a single `DT_RESULT` for each unit. Other ATH result messages are layered
around this one: `DT_RESULT_ANALYSIS` for additional track analysis, `DT_CURRENT` for current rankings in track/road,
`DT_PHASE_RESULT` for phase-level results, and `DT_CUMULATIVE_RESULT` for combined-event totals.

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Unit` | Full RSC of the event unit. |
| `DocumentSubcode` | N/A | Not used for this message. |
| `DocumentType` | `DT_RESULT` | Event Unit Start List and Results message. |
| `DocumentSubtype` | N/A | Not used for this message. |
| `Version` | `1..V` | Ascending version number for this message content. |
| `ResultStatus` | `CC @ResultStatus` | Result lifecycle status. ATH names `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, and `PROVISIONAL` in this section. |
| `FeedFlag` | `P` or `T` | Production or test feed. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time, up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day; usually the physical day unless the unit or transmission crosses midnight. |
| `Source` | `SC @Source` | System that generated the message. |

## Trigger and Frequency

| `ResultStatus` | When to send |
|---|---|
| `START_LIST` | As soon as the expected start-list information is available. Resend for any start-list changes or IRMs before the unit starts. |
| `LIVE` | As soon as the unit starts. Continue sending after every addition or change in live data. |
| `INTERMEDIATE` | After completion of every round or height in field events. |
| `UNOFFICIAL` | After all competitors have finished the unit, before official confirmation. |
| `OFFICIAL` | After the unit is official. The message may also be resent after `OFFICIAL` to add qualification marks not previously known. |

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
   │  │  └─ @SubEventName
   │  └─ VenueDescription (0,1)
   │     ├─ @Venue
   │     ├─ @VenueName
   │     ├─ @Location
   │     └─ @LocationName
   └─ Result (1,N)
      ├─ @Rank
      ├─ @RankEqual
      ├─ @Result
      ├─ @Unchecked
      ├─ @IRM
      ├─ @QualificationMark
      ├─ @SortOrder
      ├─ @StartOrder
      ├─ @StartSortOrder
      ├─ @ResultType
      ├─ @Diff
      ├─ ExtendedResults (0,1)
      │  └─ ExtendedResult (1,N)
      │     ├─ @Type
      │     ├─ @Code
      │     ├─ @Pos
      │     ├─ @Value
      │     ├─ @Value2
      │     ├─ @Rank
      │     ├─ @RankEqual
      │     ├─ @SortOrder
      │     ├─ @Diff
      │     ├─ @Move
      │     └─ Extension (0,N)
      ├─ RecordIndicators (0,1)
      │  └─ RecordIndicator (1,N)
      │     ├─ @Order
      │     ├─ @Code
      │     ├─ @RecordType
      │     └─ @Equalled
      └─ Competitor (1,1)
         ├─ @Code
         ├─ @Type
         ├─ @Organisation
         ├─ Description (0,1)
         │  └─ @TeamName
         ├─ EventUnitEntry (0,N)
         │  ├─ @Type
         │  ├─ @Code
         │  ├─ @Pos
         │  └─ @Value
         └─ Composition (0,1)
            └─ Athlete (0,N)
               ├─ @Code
               ├─ @Order
               ├─ @Bib
               ├─ Description (1,1)
               ├─ EventUnitEntry (0,N)
               └─ ExtendedResults (0,1)
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Mandatory in XSD | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Mandatory in XSD | `S(20)` | Code-set version applicable to the message. |

## ExtendedInfos / UnitDateTime

Include `UnitDateTime` when the unit starts.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `StartDate` | Mandatory | `DateTime` | Actual start date and time. |

## ExtendedInfos / ExtendedInfo

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `DISPLAY` | `CURRENT` | Numeric intermediate point | Horizontal jumps, vertical jumps, and throws while `LIVE`, for the competitor about to attempt or currently attempting. | Competitor ID, no leading zeroes | Current competitor. |
| `DISPLAY` | `CURRENT_2` | Numeric intermediate point | Throws only while `LIVE`, when two current competitors exist in the same unit. | Competitor ID, no leading zeroes | Second current competitor. |
| `DISPLAY` | `INTERMEDIATE_ATTEMPT_CURRENT` | N/A | Vertical jumps. | `1..3` | Attempt number within the current jumping height. |
| `DISPLAY` | `INTERMEDIATE_CURRENT` | N/A | All track and road events without blocks. | Current intermediate number or `F` | Current intermediate; send `F` as soon as the first athlete crosses the finish. |
| `DISPLAY` | `LAST_COMP` | Numeric intermediate point | While `LIVE` or `UNOFFICIAL`, for the last updated competitor. | Competitor ID | Last finished competitor or last competitor whose time/distance was measured. |
| `DISPLAY` | `LAST_COMP_2` | Numeric intermediate point | Throws only while `LIVE` or `UNOFFICIAL`, when two current competitors exist in the same unit. | Competitor ID | Second last updated competitor. |
| `UI` | `INTERMEDIATE` | Numeric `1..n` | Vertical jumps, road events, track events over 400m, and relays. | Height, distance, text, or integer depending on event type | Defines each intermediate point. For vertical jumps this is the jumping height. For track/road it is distance or named point from the start, including finish. |
| `UI` | `INTERMEDIATES_TOTAL` | N/A | Field events, track events over 400m, and road events. | Numeric | Total number of intermediate points; track/road includes finish. |
| `UI` | `LAST_LAP` | N/A | Individual track events over 400m. | `m:ss.ff` | Immediately previous 400m segment time from the leader at the current finish-line crossing. |
| `UI` | `QUAL_BP` | `1` or `2` | Field events. | Qualifying performance or count | `1` gives the qualifying performance; `2` gives the number of best performers that at least will qualify. |
| `UI` | `QUAL_BT` | N/A | Track events. | Numeric | Number of qualifiers based on best time. |
| `UI` | `QUAL_FROM_RANK` | N/A | Non-final track events. | Numeric | First rank that qualifies by place, usually `1`. |
| `UI` | `QUAL_TO_RANK` | N/A | Non-final track events. | Numeric | Last rank that qualifies by place in the unit. |
| `UI` | `QUAL_RULE` | N/A | If applicable. | `SC @QualRule` | Qualification rule. The source section references the code but does not provide a direct code-table link. |
| `UI` | `SCORING_RESULT` | Numeric order | Combined-event units. | Time or mark | Performance used to fill possible points as in ORIS; `@Pos=1` is the best result. |
| `UI` | `WIND_SPEED` | N/A | Track units up to 200m, including hurdles and combined events. | Signed decimal `+/-#0.0` | Wind in metres per second. |
| `UI` | `RERUN` | N/A | If the unit is a rerun. | `Y` | Rerun flag. |

### ExtendedInfo / Extension

| Parent `Type/Code` | Extension `Code` | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI/INTERMEDIATE` | `TIME` | N/A | Each non-finish intermediate point in track events over 400m and road events. | `h:mm:ss.ff` or `m:ss.f` | Leader time at the intermediate. Do not send leading zeroes or hours/minutes when zero. |
| `UI/INTERMEDIATE` | `LEADER` | N/A | Individual track events over 400m, including 4x400m, and road events, not at finish. | Competitor or team ID | Leader at the intermediate. In relays this is the team ID. |
| `UI/INTERMEDIATE` | `TIME_LAST_KM` | N/A | Track events over 2000m. | `m:ss.ff` | Time of the last kilometre. |
| `UI/INTERMEDIATE` | `LEG` | N/A | Marathon Walk Relay. | Numeric | Leg number for the intermediate; for a change point, this is the incoming leg. |
| `UI/INTERMEDIATE` | `EXCHANGE` | N/A | Marathon Walk Relay exchange points. | `SC @Exchange` | Exchange-point code. The source section references the code but does not provide a direct code-table link. |
| `UI/SCORING_RESULT` | `POINTS` | N/A | Combined-event units. | Numeric points | Scoring points for the performance. |

### Intermediate Point Values

For `UI/INTERMEDIATE`, ATH defines event-specific value conventions:

| Event Type | `@Value` Convention |
|---|---|
| Vertical jumps | Jumping height with two decimals. |
| 800m | `400`, `Finish`. |
| 1500m | `400`, `800`, `1200`, `Finish`. |
| 3000m steeplechase | `1000`, `2000`, `Finish`. |
| 5000m | `1000`, `2000`, `3000`, `4000`, `Finish`. |
| 10000m | Every 1000m through `9000`, then `Finish`. |
| 4x100m | `100`, `200`, `300`, `Finish`. |
| 4x400m | `400`, `800`, `1200`, `Finish`. |
| 20km walks | Every 1km plus `Finish`. |
| Marathon | `5`, `10`, `15`, `20`, `Half`, `25`, `30`, `35`, `40`, `Finish`. |
| Marathon Race Walk Relay | As agreed for the course. |

## SportDescription

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `DisciplineName` | Mandatory | `S(40)` | English discipline description from Common Codes, not the code. |
| `EventName` | Mandatory | `S(40)` | English event description from Common Codes. |
| `Gender` | Mandatory | `CC @SportGender` | Gender code for the event unit. |
| `SubEventName` | Mandatory | `S(40)` | English event-unit description from Common Codes. |

## VenueDescription

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Venue` | Mandatory | `CC @VenueCode` | Venue code. |
| `VenueName` | Mandatory | `S(25)` | English venue description from Common Codes. |
| `Location` | Mandatory | `CC @Location` | Location code. |
| `LocationName` | Mandatory | `S(30)` | English location description from Common Codes. |

## Result

Each `DT_RESULT` must include at least one `Result` competitor for the event unit.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Rank` | Optional | Text | Competitor rank in the event unit. In road events it is updated at each intermediate. |
| `RankEqual` | Optional | `Y` | Send when the rank is tied. |
| `Result` | Optional | Time, distance, or points | Competitor result. Send only when `ResultType` is `TIME`, `DISTANCE`, or `POINTS`. For distance, use metres with two decimals. Do not send leading zeroes or zero hours/minutes. |
| `Unchecked` | Optional | `Y` | Time is transponder/similar and must be validated by photo reading. Send only when applicable. |
| `IRM` | Optional | `SC @IRM` | Invalid result mark for the unit. Send only when `ResultType=IRM`. |
| `QualificationMark` | Optional | `SC @QualificationMark` | Indicates qualification for the next round. |
| `SortOrder` | Mandatory | Numeric | Primary result sort order. Before the unit starts, usually matches `StartSortOrder`, except where a previous rank influences ordering. During the unit, every sort-order change is sent here even if the competitor is not ranked. |
| `StartOrder` | Optional | Numeric | Lane or competitor start order. |
| `StartSortOrder` | Mandatory | Numeric | Start-list sort order according to ATH rules. |
| `ResultType` | Optional | `SC @ResultType` | Result type: time, distance, points, or IRM. |
| `Diff` | Optional | `+h:mm:ss`, `+mm:ss`, or `+m:ss` | Road events only. Time/value behind the leader; do not send for the leader and only include hours if non-zero. |

## Result / ExtendedResults / ExtendedResult

| Type | Code | Pos | Expected When | Value Fields | Meaning |
|---|---|---|---|---|---|
| `ER` | `FALSE_START` | Optional `G` for guide; `2` for second false start by same athlete in combined events | If applicable. | `Value=S(n)`, `Value2=S(10)` | False-start indication such as `F1` or `F2`; `Value2` is the reaction time, usually `-0.nnn` or `0.nnn`. |
| `ER` | `LAST_FALSE_START` | N/A | Track events, if applicable. | `Value=Y` | Competitor made a false start at the previous start attempt. |
| `ER` | `MS` | N/A | Track events where athletes receive the same time. | `Value=S(4)` | Millisecond value, for example `.123`. |
| `ER` | `INTERMEDIATE` | Numeric `1..n` | Road races, relays, and field events. | See table below | Intermediate, attempt, or jumping-height result. In track relays, `@Pos` is the leg number. |
| `ER` | `IRM_INTERMEDIATE` | N/A | If applicable. | `Value=Numeric` | Intermediate in which the athlete received the IRM value. |
| `ER` | `REACT_TIME` | N/A | Events starting with blocks, up to and including 400m and all relays. | `Value=s.fff` | Competitor reaction time. |
| `ER` | `REACT_TIME_GUIDE` | N/A | Para-athletics events using blocks where the competitor has a guide. | `Value=s.fff` | Guide reaction time. |
| `ER` | `WARNINGS` | N/A | Walking events, if applicable. | `Value=SC @Warning`, `Value2=Numeric` | Concatenation of cards, maximum 7; `Value2` is the total card count. The source section references the warning code but does not provide a direct code-table link. |
| `ER` | `PHOTO` | N/A | Track and road events, if applicable. | `Value=Y` or `P` | Photo-finish status: `Y` evaluated, `P` pending. Pending competitors may have no rank but should still be sorted as well as known. |
| `ER` | `ORDER_CURRENT` | N/A | Horizontal jumps and throws. | `Value=Numeric` | Current participation order, initially start order and then updated by applied rules. |
| `ER` | `ORDER_FINAL` | `3` or `5` | Horizontal jumps and throws, if applicable. | `Value=Numeric` | Starting order after the attempt indicated by `@Pos`. |
| `ER` | `BEST_ATTEMPT_NUM` | N/A | Horizontal jumps and throws. | `Value=Numeric` | Best attempt number. |
| `ER` | `IN_COMP` | N/A | Vertical jumps, whenever known or changed. | `Value=Y` or `N` | Whether the athlete is still in competition. |
| `ER` | `PTS` | N/A | Combined events. | `Value=###0` | Points for the event unit. |
| `ER` | `RULE` | N/A | Disqualification or other applicable requirement. | `Value=SC @Rule` | Rule reference. The source section references the code but does not provide a direct code-table link. |
| `ER` | `WIND_SPEED` | N/A | Horizontal jumps. | `Value=+/-#0.0`, optional `Value2` | Wind for the best attempt mark. Include only for valid results, not IRM. `Value2=Y` means assisting wind above 2.0 m/s prevents record recognition; `Value2=O` means another legal attempt exceeds the former record. |
| `ER` | `RC` | N/A | If applicable. | `Value=Y` | Red card for behaviour in this unit, not second yellow. |
| `ER` | `YC` | N/A | If applicable. | `Value=Y` | Yellow card for behaviour in this or a previous unit. |
| `ER` | `YRC` | N/A | If applicable. | `Value=Y` | Second yellow card for behaviour in this unit. |
| `ER` | `UNDER_PROTEST` | N/A | Competitor is competing under protest. | `Value=P` | Under-protest marker. |
| `ER` | `LANE_INFRINGE` | N/A | Competitor received a lane infringement in this unit or carries one from a previous round. If carried from a previous round, include only in `START_LIST`. | `Value=L` | Lane infringement marker. |

### ER / INTERMEDIATE Attributes

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Value` | Mandatory | Time, distance, or attempt string | Race time up to the intermediate point; field-attempt mark; `-` for pass; `x` for failure; `r` for retired; or vertical-jump attempt marks such as `o`, `-`, `xxx`, or `r`. At finish, this must equal `Result/@Result` for the competitor. If a transponder time is sent, replace it with the official time when available. |
| `Value2` | Optional | `h:mm:ss` | Road events only. Section time from the previous intermediate, or from the start for the first intermediate. |
| `Rank` | Optional | Text | Cumulative rank at this intermediate. Not included for field events. |
| `RankEqual` | Optional | `Y` | Tied rank at this intermediate. |
| `SortOrder` | Optional | Numeric | Order of all competitors at the intermediate, including those without rank. Not included for field events. |
| `Diff` | Optional | Time | Cumulative time behind the leader at this intermediate. Do not send for the leader. Not included for field events. |
| `Move` | Optional | Signed numeric | Change in rank compared with previous intermediate. Road events use `0` for no change. |

### ExtendedResult / Extension

| Parent `Code` | Extension `Code` | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `INTERMEDIATE` | `AFTER_ATTEMPT_BEST` | N/A | Horizontal jumps and throws. | `#0.00` | Best mark after the attempt. |
| `INTERMEDIATE` | `AFTER_ATTEMPT_RANK` | Attempt `1..3` | Vertical jumps. | Text | Athlete rank after the athletes' attempt at this height. |
| `INTERMEDIATE` | `AFTER_ATTEMPT_ERANK` | Attempt `1..3` | Vertical jumps. | `Y` | `AFTER_ATTEMPT_RANK` is tied. |
| `INTERMEDIATE` | `AFTER_INTERMEDIATE_RANK` | N/A | Field events. | Text | Rank after all athletes finished this attempt. |
| `INTERMEDIATE` | `AFTER_INTERMEDIATE_ERANK` | N/A | Field events. | `Y` | `AFTER_INTERMEDIATE_RANK` is tied. |
| `INTERMEDIATE` | `AFTER_INTERMEDIATE_RECORD` | Numeric | Field events. | `CC @RecordType` | Record code after all athletes finished this attempt; use `1`, `2`, etc. for multiple records. |
| `INTERMEDIATE` | `AFTER_INTERMEDIATE_SORT_ORDER` | N/A | Field events. | Numeric | Sort order after all athletes finished this attempt. |
| `INTERMEDIATE` | `INTERMEDIATE_LAST_COMPETITOR` | N/A | Field and road events. | `Y` | Last attempt of the last competitor, or last competitor who crossed this intermediate. |
| `INTERMEDIATE` | `RECORD_MARK` | N/A | If applicable. | `CC @RecordType` | Record broken at this intermediate point. |
| `INTERMEDIATE` | `RUNWAY_SPEED` | N/A | Horizontal jumps, if collected. | `#0.0` | Run-up speed in km/h. |
| `INTERMEDIATE` | `STEP` | `1..3` | Triple jump, if collected. | `#0.00` | Step length in metres; `1` hop, `2` step, `3` jump. |
| `INTERMEDIATE` | `WIND_SPEED` | N/A | Horizontal jumps. | `+/-#0.0` | Wind during the attempt in metres per second. |
| `INTERMEDIATE` | `AFTER_INTERMEDIATE_ERECORD` | Numeric | Field events. | `Y` | `AFTER_INTERMEDIATE_RECORD` is equalled. |
| `INTERMEDIATE` | `OFFSET` | N/A | Long jump and triple jump, including combined events. | `+#0.0`, `-#0.0`, or `0.0` | Distance behind the take-off line. |

## Result / RecordIndicators / RecordIndicator

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Order` | Mandatory | Numeric | Record hierarchy from `1..n`; the `CC @RecordType` order can be used as reference. |
| `Code` | Mandatory | `CC @RecordCode` | Record code broken by the result value. |
| `RecordType` | Mandatory | `CC @RecordType` | Level at which the record is broken. |
| `Equalled` | Optional | `Y` | Record was equalled rather than broken. |

## Result / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | Mandatory | `A` or `T` | `A` for athlete, `T` for team. |
| `Organisation` | Optional | `CC @Organisation` | Competitor organisation. |

### Competitor / Description

Used only in team events.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `TeamName` | Mandatory | `S(73)` | Team name. |

### Competitor / EventUnitEntry

These entries are only for relay event units.

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `NR` | N/A | When available in relays. | Time | Team national record performance. Do not send leading zeroes or zero minutes. |
| `EUE` | `SB` | N/A | When available in relays. | Time | Team season best. Do not send leading zeroes or zero minutes. |

## Competitor / Composition / Athlete

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Athlete ID. Can be the individual competitor or a team member. |
| `Order` | Mandatory | Numeric | `1` for individual competitors; `1..4` for relay running order. |
| `Bib` | Optional | `S(4)` | Bib number. |

### Athlete / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Athlete gender. |
| `Organisation` | Mandatory | `CC @Organisation` | Athlete organisation. |
| `BirthDate` | Optional | `Date` | Birth date, when available. |
| `IFId` | Optional | `S(16)` | International Federation ID. |
| `Class` | Optional | `CC @DisciplineClass` | Sport class for athletes with a disability, for example Paralympic Games. |
| `GuideID` | Optional | `S(20)` without leading zeroes | Guide ID for some athletes with a disability. |
| `GuideFamilyName` | Optional | `S(25)` | Guide family name. |
| `GuideGivenName` | Optional | `S(25)` | Guide given name. |

### Athlete / EventUnitEntry

These entries are for all event units except relays.

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `RANK_BEFORE` | N/A | As soon as rank before the current unit is known in combined events. | Numeric | Combined-event rank before this unit. |
| `EUE` | `ERANK_BEFORE` | N/A | Combined-event rank before this unit is known and tied. | `Y` | `RANK_BEFORE` is tied. |
| `EUE` | `PTS_BEFORE` | N/A | As soon as points before the current unit are known in combined events. | `###0` | Combined-event points before this unit. |
| `EUE` | `PTS_BEFORE_BEHIND` | N/A | As soon as points behind before the current unit are known in combined events. | `###0` | Points behind the combined-event leader before this unit. |
| `EUE` | `PB` | N/A | Always when available. | Time, blank, or distance | Athlete personal best. Do not send leading zeroes or zero hours/minutes. |
| `EUE` | `SB` | N/A | Always when available. | Time, blank, or distance | Athlete season best. Do not send leading zeroes or zero minutes. |
| `EUE` | `RANK_WLD` | N/A | Always when available. | `S(4)` | Athlete world ranking. |

### Athlete / ExtendedResults / ExtendedResult

This element is for team members in relay and team events.

| Type | Code | Pos | Expected When | Value Fields | Meaning |
|---|---|---|---|---|---|
| `ER` | `LEG_SPLIT` | Leg number `1..4`; required only when athletes complete more than one leg | All relays, when available. | `Value` time, optional `Rank`, optional `RankEqual=Y` | Team-member split for the leg. Does not include penalty time in walk relay. |
| `ER` | `FALSE_START` | Optional `G` if guide false start | First athlete in a relay, if applicable. | `Value=S(n)`, `Value2=S(10)` | False-start indication and associated reaction time. |
| `ER` | `LAST_FALSE_START` | N/A | First athlete in a relay, if applicable. | `Value=Y` | Competitor made a false start at the previous start attempt. |
| `ER` | `REACT_TIME` | N/A | Events starting with blocks, for the first athlete in a relay. | `Value=s.fff` | Competitor reaction time. |
| `ER` | `REACT_TIME_GUIDE` | N/A | Para-athletics relays using blocks where the first athlete has a guide. | `Value=s.fff` | Guide reaction time. |
| `ER` | `RULE` | N/A | Disqualification or other applicable requirement in team events. | `Value=SC @Rule` | Rule reference. The source section references the code but does not provide a direct code-table link. |
| `ER` | `WARNINGS` | Leg number `1..4`; required only when athletes complete more than one leg | Individuals in team walking events, if applicable. | `Value=SC @Warning` | Concatenation of warnings, maximum 7. |
| `ER` | `LEG_PENALTY` | Leg number `1..4`; required only when athletes complete more than one leg | Individuals in team walking events, if applicable. | `Value=m:ss` | Total penalty-zone time during the leg. |
| `ER` | `IRM` | N/A | Individuals in team walking events, if applicable. | `Value=SC @IRM` | Applicable invalid result mark. |
| `ER` | `YC` | N/A | Team events, if applicable. | `Value=Y` | Yellow card marker. |

## Samples from the Dictionary, Normalized

### 100m ExtendedInfos

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SOG-2024-ATH-3.4 APP" Codes="SOG-2024">
  <ExtendedInfos>
    <UnitDateTime StartDate="2012-08-05T21:50:00+01:00" />
    <ExtendedInfo Type="UI" Code="WIND_SPEED" Value="+1.5" />
    <ExtendedInfo Type="UI" Code="QUAL_RULE" Value="ABC" />
    <ExtendedInfo Type="UI" Code="QUAL_FROM_RANK" Value="1" />
    <ExtendedInfo Type="UI" Code="QUAL_TO_RANK" Value="2" />
    <ExtendedInfo Type="UI" Code="QUAL_BT" Value="2" />
  </ExtendedInfos>
  <Result Rank="1" ResultType="TIME" Result="9.79" SortOrder="1" StartOrder="4" StartSortOrder="4">
    <Competitor Code="1234567" Type="A" Organisation="USA">
      <Composition>
        <Athlete Code="1234567" Order="1" Bib="1234">
          <Description FamilyName="Sample" GivenName="Athlete" Gender="M" Organisation="USA" />
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

### High Jump Result

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SOG-2024-ATH-3.4 APP" Codes="SOG-2024">
  <Result Rank="2" ResultType="DISTANCE" Result="2.33" SortOrder="2" StartOrder="7" StartSortOrder="7">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="1" Value="-" />
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="2" Value="xo" />
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="3" Value="o" />
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="4" Value="o" />
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="5" Value="x-" />
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="6" Value="xx" />
    </ExtendedResults>
    <Competitor Code="1234567" Type="A" Organisation="USA">
      <Composition>
        <Athlete Code="1234567" Order="1" Bib="1234">
          <Description FamilyName="Sample" GivenName="Jumper" Gender="M" Organisation="USA" />
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

### Road Event Result

The dictionary sample illustrates cumulative intermediate times, section times in `Value2`, intermediate rank,
sort order, rank movement, and `Diff`. The source XML appears to have minor syntax typos around `Move`; the normalized
shape is:

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SOG-2024-ATH-3.4 APP" Codes="SOG-2024">
  <Result Rank="1" ResultType="TIME" Result="2:08:01" StartOrder="45" StartSortOrder="45" SortOrder="1">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="1" Value="15:23" Value2="15:23"
        Rank="4" SortOrder="4" Diff="+0:06" />
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="2" Value="30:46" Value2="15:23"
        Rank="6" SortOrder="6" Move="-2" Diff="+0:08" />
      <ExtendedResult Type="ER" Code="INTERMEDIATE" Pos="10" Value="2:08:01" Value2="15:08"
        Rank="1" SortOrder="1" Move="3" />
    </ExtendedResults>
    <Competitor Code="1234567" Type="A" Organisation="KEN">
      <Composition>
        <Athlete Code="1234567" Order="1" Bib="45">
          <Description FamilyName="Sample" GivenName="Runner" Gender="M" Organisation="KEN" />
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

## Message Sort

Sort by `Result/@SortOrder`.

## XSD Validation

The normalized XML examples above were validated as `Competition` fragments wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Treat ATH `DT_RESULT` as a full unit snapshot. A new version should replace the prior modeled state for the unit
  rather than patching only fields present in the update.
- Model result status as message-level state, not competitor-level state. Competitor IRMs, qualification marks,
  protest markers, lane infringements, cards, warnings, and photo-finish state are competitor-level facts.
- `SortOrder` and `StartSortOrder` have different meanings. Keep both: `StartSortOrder` preserves start-list order,
  while `SortOrder` becomes the live/result ordering.
- ATH uses the same `ER/INTERMEDIATE` code for several domains: field attempts, vertical-jump heights, road/race
  intermediates, and relay leg intermediates. Interpret it by event type and path.
- For race intermediates, finish-time consistency is explicit: the finish `ER/INTERMEDIATE/@Value` must equal
  `Result/@Result` for the competitor.
- Road events need cumulative and section timing. `ER/INTERMEDIATE/@Value` is cumulative; `@Value2` is the section
  from previous intermediate or start.
- Horizontal jump and triple jump details can carry attempt-level measurements such as wind, runway speed, step
  lengths, and take-off offset. `RUNWAY_SPEED` and `STEP` are only available when collected.
- Relays and team walking events put member-level timing, warnings, penalties, and IRMs under
  `Competitor/Composition/Athlete/ExtendedResults`, separate from team-level `Result/ExtendedResults`.
- Paralympic fields such as `Class`, guide identifiers, guide reaction time, and guide false-start markers should be
  optional event-variant data, not assumed for all ATH units.
- `DT_CONFIG` can force result resends: the ATH configuration section notes that if `DT_CONFIG` is sent after any
  `DT_RESULT` messages for a phase, all up-to-date `DT_RESULT` messages for that phase need to be resent.

## Code Appendix: Directly Linked Values

The section directly links these code pages:

| Reference | Source |
|---|---|
| `CC @Competition` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm` |
| `CC @Unit` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm` |
| `CC @ResultStatus` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm` |
| `SC @Source` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_ATH.htm` |
| `CC @VenueCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm` |
| `CC @Location` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm` |
| `SC @IRM` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_ATH.htm` |
| `SC @QualificationMark` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_QualificationMark_SOG_ATH.htm` |
| `SC @ResultType` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_ATH.htm` |
| `CC @Organisation` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm` |
| `CC @RecordCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Record.htm` |
| `CC @RecordType` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/RecordType.htm` |
| `CC @PersonGender` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm` |

Large master-data tables such as competition, event unit, venue, location, organisation, record code, and full record
type are not embedded here. They are source-linked above. The smaller linked tables that are useful for domain modeling
are included below in English.

### CC @ResultStatus

| Code | Order | English Description | Note |
|---|---:|---|---|
| `START_LIST` | 1 | Start List | Before competition. |
| `LIVE` | 2 | Live | Live updates during competition. |
| `INTERMEDIATE` | 3 | Intermediate | Competition is stopped or at predefined points. |
| `UNCONFIRMED` | 4 | Unconfirmed | Unit is over but not yet unofficial or official. |
| `UNOFFICIAL` | 5 | Unofficial | Released after the event, before official confirmation. |
| `OFFICIAL` | 6 | Official | Officially confirmed after protests/decisions. |
| `PARTIAL` | 7 | Partial | Incomplete list, final ranking; used in PDF. |
| `PROTESTED` | 8 | Protested | After the competition is no longer live and a protest has been lodged. |
| `PROVISIONAL` | 9 | Provisional | Special situations. |

### SC @Source

| Code | English Description |
|---|---|
| `INVATH1` | Origin for messages from OVR at INV for ATH. |
| `STAATH1` | Origin for messages from OVR at STA for ATH. |
| `TROATH1` | Origin for messages from OVR at TRO for ATH. |

### SC @IRM

| Code | English Description |
|---|---|
| `DNF` | Did Not Finish |
| `DNS` | Did Not Start |
| `DQ` | Disqualified |
| `NM` | No mark |

### SC @QualificationMark

| Code | English Description |
|---|---|
| `Q` | Qualified by place (track) or standard (field) |
| `q` | Qualified by time (track and relays) or performance (field events) |
| `qD` | Qualified by draw |
| `qJ` | Advanced to next round by Jury of Appeal |
| `qR` | Advanced to next round by Referee |
| `Re` | Qualified for the Repechage Round |
| `ReJ` | Qualified for the Repechage Round by the Jury of Appeal |
| `ReR` | Qualified for the Repechage Round by the Referee |

### SC @ResultType

| Code | English Description |
|---|---|
| `DISTANCE` | Performance in meters with 2 decimals |
| `IRM` | Invalid Result Mark |
| `POINTS` | Points |
| `TIME` | Time |

### CC @PersonGender

| Code | English Description |
|---|---|
| `F` | Female |
| `M` | Male |
| `X` | Unspecified |

### Unlinked SC References in This Section

The section references `SC @QualRule`, `SC @Exchange`, `SC @Warning`, and `SC @Rule`, but those references did not have
direct code-table hyperlinks on pages 21-42. They are therefore documented as typed code references above but not
expanded into value tables here.

## Relevant Document-Control Notes

The source document control identifies this file as `SOG-2024-ATH-3.4 APP`. Recent `DT_RESULT` changes called out in
the change log include:

| Version | DT_RESULT Notes |
|---|---|
| `V3.0` | Updated `UI/INTERMEDIATE`, result values, qualification-rank fields, false-start `Value2`, rule references, wind handling, `UNDER_PROTEST`, `LANE_INFRINGE`, and relay leg split behavior. |
| `V3.01` | Added `UI/RERUN`; clarified `ER/INTERMEDIATE/OFFSET`, `Result/Diff`, false starts, and horizontal-jump wind speed. |
| `V3.02` | Added athlete-level `ER/RULE`, `ER/WARNINGS`, and `ER/IRM`; updated result-analysis intermediates. |
| `V3.2` | Updated walking warnings, relay/team-walk leg split and penalties, `UI/INTERMEDIATE` leg extension, and `ER/INTERMEDIATE` expectations. |
| `V3.3` | Added `UI/INTERMEDIATE/EXCHANGE` and updated warnings. |
| `V3.4` | Updated athlete-level `ER/LEG_SPLIT` and added athlete-level `ER/YC`. |

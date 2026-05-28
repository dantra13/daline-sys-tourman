# ODF BKB Data Dictionary: DT_SCHEDULE, Pages 8-16

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 8-16.
Source version: `SOG-2024-BKB-3.4 APP`, dated 19 January 2024.

This note restructures the basketball `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` section into readable Markdown for domain modeling. It covers the competition schedule bulk message, schedule update behavior, session/unit fields, display ordering rules, hidden start/end times, team head-to-head start-list payloads, an XSD-aligned XML example, and an appendix of English SC/CC values downloaded from the Paris 2024 ODF code pages.

## 2.3.1 Competition Schedule / Competition Schedule Update

`DT_SCHEDULE` is a bulk discipline-level message. It contains the complete schedule information for all event units needed to run a competition, excluding activities such as unofficial training and press conferences.

The message carries the competition timetable for a complete discipline and the status of each competition unit. Updates from OVR are sent through `DT_SCHEDULE_UPDATE`.

All event units whose common-code `schedule` flag is `Y` or `S` are included in schedule messages regardless of status. If such a unit has no concrete status, send it as `UNSCHEDULED`.

Arrival of a `DT_SCHEDULE` message resets all previous schedule information for the discipline. Arrival of a `DT_SCHEDULE_UPDATE` changes only the units or sessions contained in that update.

## Head-to-Head Start Lists

The `StartList` component is included only when the unit type is one of these common-code event unit types and at least one competitor is known:

| Unit type | Meaning | Examples from dictionary |
|---|---|---|
| `HATH` | Individual head-to-head units | Archery, badminton, tennis, skateboard, etc. |
| `HCOUP` | Pairs/couples head-to-head units | Badminton, tennis, etc. |
| `HTEAM` | Team head-to-head units | Basketball, beach volleyball, handball, curling, ice hockey, etc. |

For basketball, all competition units in the dictionary use `HTEAM`. The `Composition` component (athlete list under a competitor) is reserved for `HATH` and `HCOUP` units; the BKB `DT_SCHEDULE` structure on pages 11-12 does not declare `Composition` or `Athlete` under `Competitor`, only `Description` with `TeamName` and `IFId`.

## Managing Unknown Start Times

Some disciplines manage units by order rather than by a known start time. In that case, all affected units may be sent with the same `StartDate`, and follow-up units are flagged with `HideStartDate="Y"`. The `Order` attribute then determines display order.

If a unit uses a display phrase such as `Not before 17:00`, `SUN 29 - 2nd match on CC`, or `Follows`, use `StartText`. If `StartText` is `Not before hh:mm`, the sent `StartDate` is expected to use the same `hh:mm` value.

For team sports such as basketball, the dictionary notes that `HideStartDate="Y"` is only used temporarily to remove times, not as the normal order-management mechanism.

## Display Sort Advice

When displaying schedule data, users should sort event units according to the discipline's common-code event-order mode.

| Discipline event-order mode | Display sort order |
|---|---|
| `LOC` | Day/filter by day, then session code, then location, then time regardless of `HideStartDate`, then `Order`. |
| `SESSION` or `DATE` | Day/filter by day, then session code, then time regardless of `HideStartDate`, then `Order`. |

The `CC @Discipline` row for BKB has `Eventorder = DATE`, so basketball uses the second mode.

Additional display rules:

- `Order` can be the match number for simplicity, especially when multiple matches share the same start time (a common case in basketball with two courts at Pierre Mauroy or back-to-back games at Bercy Arena).
- End users should display `StartText` when `HideStartDate="Y"`.
- Scheduled `StartDate` should not be replaced with actual start time. Use `ActualStartDate` for actual start.

## Schedule Update Semantics

`DT_SCHEDULE_UPDATE` is not a complete schedule. It carries only modified schedule data.

| Rule | Meaning |
|---|---|
| Update key | `Unit @Code` identifies any new, deleted, or updated unit. |
| Scope | Only units/sessions contained in the update are changed. No other unit/session changes are implied. |
| Bulk precedence | If a new `DT_SCHEDULE` bulk message arrives, all previous `DT_SCHEDULE_UPDATE` messages should be discarded. |
| Competition Schedule app metadata | In advance-of-Games messages from the Competition Schedule application, `ExtendedInfos/ExtendedInfo Type="CS" Code="VERSION"` carries version details, and `Type="CS" Code="STATUS"` carries status details. |

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Discipline` | Full RSC at discipline level. |
| `DocumentType` | `DT_SCHEDULE` or `DT_SCHEDULE_UPDATE` | Bulk competition schedule or update message. |
| `DocumentSubtype` | `S(20)` | `SYNC` if the message is for re-synchronisation for ODF clients. Only sent after control is transferred to the venue. Never included in update messages. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time, up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day. Usually the physical day, except when the unit or message transmission crosses midnight. |
| `Source` | `SC @Source` | System that generated the message. |

## Trigger and Frequency

| Message | Trigger/Frequency |
|---|---|
| `DT_SCHEDULE` | Sent as a bulk message when available before the Games, then sent multiple times until a date to be confirmed. After that, only updates are sent by OVR. |
| `DT_SCHEDULE` after transfer of control | Must not be sent after transfer of control to OVR. |
| `DT_SCHEDULE_UPDATE` | Triggered any time a schedule modification occurs for any previously sent bulk or update message, including addition of start-list details for H2H units. |
| H2H start-list details | Generally sent immediately when officially known, as soon as possible after the preceding unit becomes official. |
| Text-only description changes | If text descriptions change, the message is not resent only to correct prior messages; new descriptions are used in future messages. |

## Message Structure

```text
OdfBody
└─ Competition (0,1)
   ├─ @Gen
   ├─ @Sport
   ├─ @Codes
   ├─ Session (0,N)
   │  ├─ @SessionCode
   │  ├─ @StartDate
   │  ├─ @EndDate
   │  ├─ @Leadin
   │  ├─ @Venue
   │  ├─ @VenueName
   │  ├─ @ModificationIndicator
   │  ├─ @SessionStatus
   │  ├─ @SessionType
   │  ├─ @Medal
   │  ├─ @FOP
   │  └─ SessionName (1,N)
   │     ├─ @Language
   │     └─ @Value
   └─ Unit (0,N)
      ├─ @Code
      ├─ @PhaseType
      ├─ @UnitNum
      ├─ @ScheduleStatus
      ├─ @StartDate
      ├─ @HideStartDate
      ├─ @EndDate
      ├─ @HideEndDate
      ├─ @ActualStartDate
      ├─ @ActualEndDate
      ├─ @Order
      ├─ @Medal
      ├─ @Venue
      ├─ @Location
      ├─ @MediaAccess
      ├─ @SessionCode
      ├─ @ModificationIndicator
      ├─ StartText (0,N)
      │  ├─ @Language
      │  └─ @Value
      ├─ ItemName (1,N)
      │  ├─ @Language
      │  └─ @Value
      ├─ ItemDescription (0,N)
      │  ├─ @Language
      │  └─ text
      ├─ VenueDescription (0,1)
      │  ├─ @VenueName
      │  └─ @LocationName
      └─ StartList (0,1)
         └─ Start (1,N)
            ├─ @StartOrder
            ├─ @SortOrder
            └─ Competitor (1,1)
               ├─ @Code
               ├─ @Type
               ├─ @Organisation
               └─ Description (0,1)
                  ├─ @TeamName
                  └─ @IFId
```

The BKB dictionary explicitly omits the `Bib` attribute on `Competitor` and the `Composition`/`Athlete` subtree under `Competitor` that appears in some other disciplines. The structure stops at `Description` because basketball schedule messages only carry team identity, not lineups.

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional | `S(20)` | Code-set version applicable to the message. |

## Session

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `SessionCode` | Mandatory | `S(10)` | Sports competition session code containing the event unit. Usually `DDD00`, where `DDD` is discipline and `00` is the discipline session number, for example `BKB02` for the second basketball session. |
| `StartDate` | Mandatory | `DateTime` | Session start date/time. |
| `EndDate` | Mandatory | `DateTime` | Session end date/time. |
| `Leadin` | Optional | `m:ss` | Time from session start to first scheduled unit. |
| `Venue` | Mandatory | `CC @VenueCode` | Venue where the session takes place. |
| `VenueName` | Mandatory | `S(25)` | English venue description from Common Codes. |
| `ModificationIndicator` | Optional, mandatory in update message | `S(1)` | `N` for new or `U` for update. |
| `SessionStatus` | Optional | `CC @ScheduleStatus` | Only use `CANCELLED` if applicable. Other sessions are assumed scheduled; there is no running/finished transition for sessions. |
| `SessionType` | Optional | `CC @SessionType` | Session type. |
| `Medal` | Optional | Numeric `#0` | Number of gold medals planned to be determined in this session, calculated from units assigned to the session. |
| `FOP` | Optional | Numeric `#0` | Number of fields of play planned for the session. Only included pre-Games before schedule is known; do not include in data to/from OVR during Games period. |

## Session / SessionName

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Language` | Mandatory | `CC @Language` | Language of the session description. |
| `Value` | Optional in dictionary | `S(40)` | Name of the sports competition session. |

The PDF marks `SessionName/@Value` as `O` (optional). In practice all dictionary samples populate it, and downstream consumers expect a non-empty value when the element is sent.

```xml
<Session SessionCode="BKB01"
         StartDate="2026-07-27T11:00:00+02:00"
         EndDate="2026-07-27T15:00:00+02:00"
         Leadin="0:00"
         Venue="LIL"
         VenueName="Pierre Mauroy Stadium">
  <SessionName Language="ENG" Value="Basketball Session 1"/>
</Session>
```

## Unit

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `CC @Unit` | Full RSC for the unit. |
| `PhaseType` | Mandatory | `CC @PhaseType` | Phase type for the unit. |
| `UnitNum` | Optional | `S(15)` | Match/game/bout/race number or similar. For basketball, the game number is the natural value. |
| `ScheduleStatus` | Mandatory | `CC @ScheduleStatus` | Unit status. |
| `StartDate` | Optional | `DateTime` | Scheduled start date/time. May be omitted when `ScheduleStatus="UNSCHEDULED"`. For other statuses it is expected for display ordering, including `CANCELLED` and `POSTPONED`. The PDF prose typo here reads "ordering is display is incorrert" — read as "or display ordering will be incorrect". Do not update to actual start time; only update on `RESCHEDULED`. When `HideStartDate="Y"`, fill with the session start time or the shared start time of a group of units and use `Order` for sorting. |
| `HideStartDate` | Optional | `S(1)` | Send `Y` when scheduled start should not be displayed. The time may be estimated/fake and used only for sorting. Omit when the scheduled start should be displayed. |
| `EndDate` | Optional | `DateTime` | Scheduled end date/time. Do not update to actual end; only update on `RESCHEDULED` relative to `StartDate`. Not required when `UNSCHEDULED` or `CANCELLED`. |
| `HideEndDate` | Optional | `S(1)` | Send `Y` when scheduled end time should not be displayed. Useful when end time is highly variable. |
| `ActualStartDate` | Optional | `DateTime` | Expected once the event unit has started. |
| `ActualEndDate` | Optional | `DateTime` | Expected once the event unit has finished. |
| `Order` | Optional | Numeric `###0` | Display order. Required/recommended when `HideStartDate="Y"` or when multiple units start at the same time and a specific order is expected. Match number can be used. |
| `Medal` | Optional | `SC @UnitMedalType` | Medal indicator for the unit. |
| `Venue` | Optional | `CC @VenueCode` | Unit venue. Mandatory unless `UNSCHEDULED`. |
| `Location` | Optional | `CC @Location` | Unit location. Mandatory unless `UNSCHEDULED`. |
| `MediaAccess` | Optional | `S(6)` | Non-competition only. `OPE` for open to media, `CLO` for closed. |
| `SessionCode` | Optional | `S(10)` | Session containing the unit. |
| `ModificationIndicator` | Optional, mandatory in update message | `N`, `U` | `N` for new unit, `U` for updated unit. New unit is rare because most added units were already present as `UNSCHEDULED`. |

## Unit / StartText

`StartText` is used only when `HideStartDate="Y"`. English is mandatory in this case.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Language` | Mandatory | `CC @Language` | Language code of `Value`. |
| `Value` | Mandatory | `S(20)` or `SC @StartText` | Display text when `StartDate` should not be displayed. Use available codes (`TBC`, `TBD`) or free text if no suitable code exists. |

## Unit / ItemName

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Language` | Mandatory | `CC @Language` | Language code of `Value`. |
| `Value` | Mandatory | `S(40)` | Unit name/description. For competition units, use the common-code unit description matching the RSC. Only English is expected. For non-competition schedules, add the description directly. |

## Unit / ItemDescription

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Language` | Mandatory | `CC @Language` | Language code of the text. |
| text | Mandatory | Free text | Item description for non-competition schedule. |

## Unit / VenueDescription

Mandatory when `Unit/@Venue` is included.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `VenueName` | Mandatory | `S(25)` | English venue description from Common Codes. |
| `LocationName` | Mandatory | `S(30)` | English location description from Common Codes. |

## Unit / StartList / Start

Start-list information is sent only when the unit type is `HATH`, `HCOUP`, or `HTEAM` and at least one competitor is known. For basketball this means every `HTEAM` competition unit gets a `StartList` once teams are placed.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `StartOrder` | Optional | Numeric | Competitor start order. |
| `SortOrder` | Mandatory | Numeric | Sort order for competitors in the event unit. Mainly used for display when `StartOrder` is absent. |

## Start / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes, or `SC @CompetitorPlace` | Competitor ID or placeholder. `TBD` means unknown now but expected later. `NOCOMP` means no competitor and none will arrive later. For BKB the dictionary also lists `BYE` and `NOAWARD` as valid placeholder codes. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Organisation` | Optional | `CC @Organisation` | Should be sent when known. |

## Competitor / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `TeamName` | Mandatory | `S(73)` | Team name when known; must be sent when available. |
| `IFId` | Optional | `S(16)` | Team IF number (FIBA), when available. |

## Message Sort

Sort sessions by `Session @SessionCode`.

Sort units by `Unit @StartDate`, then `Unit @Order`, then `Unit @Code`. Units without `Unit @StartDate`, for example `UNSCHEDULED` units, are listed at the end in `Unit @Code` order.

## XSD-Aligned XML Example

The example below was not validated because no XSD was supplied with this extraction. Attribute presence and casing follow the PDF's `2.3.1.4 Message Structure` table and `2.3.1.5 Message Values`. Integrators should validate emitted XML against the exact schema release used by the target ODF integration.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="BKB-------------------------------"
         DocumentType="DT_SCHEDULE"
         Version="12"
         FeedFlag="T"
         Date="2026-07-27"
         Time="08:00:00.000"
         LogicalDate="2026-07-27"
         Source="LILBKB1">
  <Competition Gen="3.4" Sport="BKB-3.4" Codes="SOG-2024">
    <Session SessionCode="BKB01"
             StartDate="2026-07-27T11:00:00+02:00"
             EndDate="2026-07-27T15:00:00+02:00"
             Leadin="0:00"
             Venue="LIL"
             VenueName="Pierre Mauroy Stadium"
             SessionType="MOR"
             Medal="0"
             FOP="1">
      <SessionName Language="ENG" Value="Basketball Session 1"/>
    </Session>
    <Unit Code="BKBMTEAM5-------------GPA-000100--"
          PhaseType="3"
          UnitNum="1"
          ScheduleStatus="SCHEDULED"
          StartDate="2026-07-27T11:00:00+02:00"
          EndDate="2026-07-27T12:45:00+02:00"
          Order="1"
          Medal="0"
          Venue="LIL"
          Location="LIL"
          SessionCode="BKB01">
      <ItemName Language="ENG" Value="Men's Group Phase - Group A"/>
      <VenueDescription VenueName="Pierre Mauroy Stadium" LocationName="Pierre Mauroy Stadium"/>
      <StartList>
        <Start StartOrder="1" SortOrder="1">
          <Competitor Code="BKBMTEAM5------ESP01" Type="T" Organisation="ESP">
            <Description TeamName="Spain"/>
          </Competitor>
        </Start>
        <Start StartOrder="2" SortOrder="2">
          <Competitor Code="BKBMTEAM5------AUS01" Type="T" Organisation="AUS">
            <Description TeamName="Australia"/>
          </Competitor>
        </Start>
      </StartList>
    </Unit>
    <Unit Code="BKBMTEAM5-------------FNL-000100--"
          PhaseType="3"
          ScheduleStatus="UNSCHEDULED"
          Medal="1"
          SessionCode="BKB30">
      <ItemName Language="ENG" Value="Men's Gold Medal Game"/>
    </Unit>
  </Competition>
</OdfBody>
```

## Modeling Notes

- Treat `DT_SCHEDULE` as a discipline-level replacement snapshot. On ingest, replace the discipline schedule state; do not merge it incrementally.
- Treat `DT_SCHEDULE_UPDATE` as a partial patch keyed by `Unit/@Code` and, where sessions are present, by session identity. Do not infer changes for units not present in the update.
- Keep scheduled dates (`StartDate`, `EndDate`) separate from actual dates (`ActualStartDate`, `ActualEndDate`). Actual game timing should never overwrite schedule timing.
- `HideStartDate` and `StartText` are presentation contracts but also affect sort/display semantics. They deserve explicit fields, not ad hoc UI-only handling. For basketball the dictionary expects `HideStartDate="Y"` only as a temporary mechanism, so it must clear once the real start time is known.
- `Order` is a domain-relevant display order, especially when two basketball matches share a start time on different courts at LIL or in back-to-back evening sessions at BCY.
- Start-list data in schedule is minimal and unit-scoped. It tells which teams are placed in the unit; lineups, jerseys, and bib numbers belong in `DT_PARTIC` / `DT_RESULT`, not in `DT_SCHEDULE`. The BKB `Competitor` element here intentionally exposes no `Bib` and no `Composition` subtree.
- Use `SC @CompetitorPlace` placeholders (`TBD`, `BYE`, `NOAWARD`, `NOCOMP`) to encode bracket positions before draws/qualifiers resolve. Treat `TBD` as transient (will be replaced) and `NOCOMP` as terminal (will never be replaced).
- BKB has `Eventorder = DATE` in `CC @Discipline`, so consumers should not apply the `LOC`-mode sort rule even when both venues are concurrently active.
- The PDF prose for `Unit/@StartDate` contains the typos "incorrert" and a missing closing parenthesis after `POSTPONED`; treat them as cosmetic.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-27. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index

| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Source` | BKB | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm) |
| `SC @UnitMedalType` | GEN | 4 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_UnitMedalType_SOG_GEN.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_UnitMedalType_SOG_GEN.htm) |
| `SC @StartText` | BKB aggregate | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm) |
| `SC @CompetitorPlace` | BKB aggregate | 4 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @Discipline` | BKB row embedded | 1 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Discipline.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Discipline.htm) |
| `CC @Unit` | BKB EventUnit rows | 130 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @PhaseType` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PhaseType.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PhaseType.htm) |
| `CC @ScheduleStatus` | Common | 11 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ScheduleStatus.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ScheduleStatus.htm) |
| `CC @SessionType` | Common | 6 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SessionType.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SessionType.htm) |
| `CC @Language` | Common | 11 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Language.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Language.htm) |
| `CC @VenueCode` | BKB venues | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm) |
| `CC @Location` | BKB locations | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm) |
| `CC @Organisation` | Common master data | 258 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |

### SC @Source
| Code | ENG Description |
| --- | --- |
| BCYBKB1 | Origin for messages from OVR at BCY for BKB |
| LILBKB1 | Origin for messages from OVR at LIL for BKB |

### SC @UnitMedalType
| Code | ENG Description |
| --- | --- |
| 0 | No medal event unit |
| 1 | Gold medal event unit |
| 2 | Silver medal event unit |
| 3 | Bronze medal event unit |

### SC @StartText
| Code | ENG Description |
| --- | --- |
| TBC | TBC |
| TBD | To be determined |

### SC @CompetitorPlace
| Code | Note | ENG Description |
| --- | --- | --- |
| BYE | There is no competitor; the other team/athlete goes directly to the next phase/round | Bye |
| NOAWARD |  | Not awarded |
| NOCOMP |  | No competitor |
| TBD |  | To be determined |

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

### CC @Discipline (BKB row)
| Code | Id | Eventorder | scheduled | IF | ENG Description |
| --- | --- | --- | --- | --- | --- |
| BKB------------------------------- | BKB | DATE | Y | FIBA | Basketball |

### CC @PhaseType
| Id | Schedule Type | ENG Description |
| --- | --- | --- |
| 0 | G | Opening/Closing Ceremonies |
| 1 | G | Official Training |
| 2 | N | Unofficial Training |
| 3 | G | Competition |
| 4 | N | Technical Meeting |
| 5 | N | Press Conference |
| 6 | N | Medal/Flower Ceremony |
| 7 | N | Draw |
| 8 | N | Others |
| 9 | N | Team Leaders Meeting |

### CC @ScheduleStatus
| Id | Note | ENG Description |
| --- | --- | --- |
| CANCELLED | Competition unit was schedule and will not now take place | Cancelled |
| DELAYED | Competition has not started and start is late (no new time) | Delayed |
| FINISHED | All play is complete in the unit | Finished |
| GETTING_READY | Start of competition is iminent, athletes usually on FOP | Getting Ready |
| INTERRUPTED | Competition has started but is now stopped temporarily. | Interrupted |
| POSTPONED | Competition has stopped and will take place at a new time (to be determined) | Postponed |
| RESCHEDULED | The start (or re-start) of the competition has changed to a new time (new time is known) | Rescheduled |
| RUNNING | Competition in progress | Running |
| SCHEDULED | Unit to be conducted | Scheduled |
| SCHEDULED_BREAK | Planned break in competition (e.g. end of period) | Scheduled Break |
| UNSCHEDULED | A possible unit to be scheduled, not displayed (e.g. swim-off) | Unscheduled |

### CC @SessionType
| Id | ENG Description |
| --- | --- |
| AFT | Afternoon |
| ALL | All Day |
| EAF | Early Afternoon |
| EVE | Evening |
| LAF | Late Afternoon |
| MOR | Morning |

### CC @Language
| Id | Description | Note |
| --- | --- | --- |
| ENG | English | EN |
| FRA | Français | FR |
| CHI | 中文 | ZH |
| DEU | Deutsch | DE |
| HIN | हिंदी | HI |
| ITA | Italiano | IT |
| JPN | 日本語 | JA |
| KOR | 한국어 | KO |
| POR | Português | PT |
| RUS | русский | RU |
| SPA | Español | ES |

### CC @VenueCode (BKB venues)
| Id | Cluster | IndoorOutdoor | ENG Description | ENG longdescription |
| --- | --- | --- | --- | --- |
| BCY | CRN | I | Bercy Arena | Bercy Arena |
| LIL |  | O | Pierre Mauroy Stadium | Pierre Mauroy Stadium |

### CC @Location (BKB locations)
| Id | Venue | Discipline | Source | ENG Description | ENG longdescription |
| --- | --- | --- | --- | --- | --- |
| BCY | BCY | BKB,GAR,GTR | BCYBKB1,BCYGAR1,BCYGTR1 | Bercy Arena | Bercy Arena |
| LIL | LIL | HBL,BKB | LILHBL1,LILBKB1 | Pierre Mauroy Stadium | Pierre Mauroy Stadium |

### CC @Unit (BKB EventUnit rows)

The Paris 2024 common-code `EventUnit` page exposes 130 rows prefixed with `BKB`. Below are all rows whose `schedule` flag is `Y` (the rows that may appear inside a BKB schedule message), filtered to columns relevant for `DT_SCHEDULE`. Phase/Event/Gender rows with `schedule = N` are not included individually here; consult the source link in the index for the complete code list.

| Code | Gender | Event | phase | Eventunit | Level | Order | medalflag | Eventunittype | ENG Description |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| BKB-------------------MEET000100-- | - | ------------------ | MEET | 000100-- | Meetings | 19 | 0 | NONE | Team Managers' Meeting |
| BKB-------------------MEET000200-- | - | ------------------ | MEET | 000200-- | Meetings | 19 | 0 | NONE | Women's Team Managers' Meeting |
| BKB-------------------MEET000300-- | - | ------------------ | MEET | 000300-- | Meetings | 19 | 0 | NONE | Men's Team Managers' Meeting |
| BKBMTEAM5-------------FNL-000100-- | M | TEAM5------------- | FNL- | 000100-- | Unit | 15 | 1 | HTEAM | Men's Gold Medal Game |
| BKBMTEAM5-------------FNL-000200-- | M | TEAM5------------- | FNL- | 000200-- | Unit | 15 | 3 | HTEAM | Men's Bronze Medal Game |
| BKBMTEAM5-------------GPA-000100-- | M | TEAM5------------- | GPA- | 000100-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000200-- | M | TEAM5------------- | GPA- | 000200-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000300-- | M | TEAM5------------- | GPA- | 000300-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000400-- | M | TEAM5------------- | GPA- | 000400-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000500-- | M | TEAM5------------- | GPA- | 000500-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000600-- | M | TEAM5------------- | GPA- | 000600-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPB-000100-- | M | TEAM5------------- | GPB- | 000100-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000200-- | M | TEAM5------------- | GPB- | 000200-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000300-- | M | TEAM5------------- | GPB- | 000300-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000400-- | M | TEAM5------------- | GPB- | 000400-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000500-- | M | TEAM5------------- | GPB- | 000500-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000600-- | M | TEAM5------------- | GPB- | 000600-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPC-000100-- | M | TEAM5------------- | GPC- | 000100-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000200-- | M | TEAM5------------- | GPC- | 000200-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000300-- | M | TEAM5------------- | GPC- | 000300-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000400-- | M | TEAM5------------- | GPC- | 000400-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000500-- | M | TEAM5------------- | GPC- | 000500-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000600-- | M | TEAM5------------- | GPC- | 000600-- | Unit | 15 | 0 | HTEAM | Men's Group Phase - Group C |
| BKBMTEAM5-------------QFNL000100-- | M | TEAM5------------- | QFNL | 000100-- | Unit | 15 | 0 | HTEAM | Men's Quarterfinal |
| BKBMTEAM5-------------QFNL000200-- | M | TEAM5------------- | QFNL | 000200-- | Unit | 15 | 0 | HTEAM | Men's Quarterfinal |
| BKBMTEAM5-------------QFNL000300-- | M | TEAM5------------- | QFNL | 000300-- | Unit | 15 | 0 | HTEAM | Men's Quarterfinal |
| BKBMTEAM5-------------QFNL000400-- | M | TEAM5------------- | QFNL | 000400-- | Unit | 15 | 0 | HTEAM | Men's Quarterfinal |
| BKBMTEAM5-------------SFNL000100-- | M | TEAM5------------- | SFNL | 000100-- | Unit | 15 | 0 | HTEAM | Men's Semifinal |
| BKBMTEAM5-------------SFNL000200-- | M | TEAM5------------- | SFNL | 000200-- | Unit | 15 | 0 | HTEAM | Men's Semifinal |
| BKBMTEAM5-------------TMRY000100-- | M | TEAM5------------- | TMRY | 000100-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000200-- | M | TEAM5------------- | TMRY | 000200-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000300-- | M | TEAM5------------- | TMRY | 000300-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000400-- | M | TEAM5------------- | TMRY | 000400-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000500-- | M | TEAM5------------- | TMRY | 000500-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000600-- | M | TEAM5------------- | TMRY | 000600-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000700-- | M | TEAM5------------- | TMRY | 000700-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000800-- | M | TEAM5------------- | TMRY | 000800-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000900-- | M | TEAM5------------- | TMRY | 000900-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001000-- | M | TEAM5------------- | TMRY | 001000-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001100-- | M | TEAM5------------- | TMRY | 001100-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001200-- | M | TEAM5------------- | TMRY | 001200-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001300-- | M | TEAM5------------- | TMRY | 001300-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001400-- | M | TEAM5------------- | TMRY | 001400-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001500-- | M | TEAM5------------- | TMRY | 001500-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001600-- | M | TEAM5------------- | TMRY | 001600-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001700-- | M | TEAM5------------- | TMRY | 001700-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001800-- | M | TEAM5------------- | TMRY | 001800-- | Unit | 15 | 0 | NONE | Men's Preliminary Phase |
| BKBMTEAM5-------------VICTMEDAL--- | M | TEAM5------------- | VICT | MEDAL--- | Medals | 17 | 0 | NONE | Men's Victory Ceremony |
| BKB-------------------TMRY000100-- | - | ------------------ | TMRY | 000100-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000200-- | - | ------------------ | TMRY | 000200-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000300-- | - | ------------------ | TMRY | 000300-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000400-- | - | ------------------ | TMRY | 000400-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000500-- | - | ------------------ | TMRY | 000500-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000600-- | - | ------------------ | TMRY | 000600-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000700-- | - | ------------------ | TMRY | 000700-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000800-- | - | ------------------ | TMRY | 000800-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000900-- | - | ------------------ | TMRY | 000900-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY001000-- | - | ------------------ | TMRY | 001000-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY001100-- | - | ------------------ | TMRY | 001100-- | Unit | 15 | 0 | NONE | Men's or Women's Preliminary Phase |
| BKBWTEAM5-------------FNL-000100-- | W | TEAM5------------- | FNL- | 000100-- | Unit | 15 | 1 | HTEAM | Women's Gold Medal Game |
| BKBWTEAM5-------------FNL-000200-- | W | TEAM5------------- | FNL- | 000200-- | Unit | 15 | 3 | HTEAM | Women's Bronze Medal Game |
| BKBWTEAM5-------------GPA-000100-- | W | TEAM5------------- | GPA- | 000100-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000200-- | W | TEAM5------------- | GPA- | 000200-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000300-- | W | TEAM5------------- | GPA- | 000300-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000400-- | W | TEAM5------------- | GPA- | 000400-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000500-- | W | TEAM5------------- | GPA- | 000500-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000600-- | W | TEAM5------------- | GPA- | 000600-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPB-000100-- | W | TEAM5------------- | GPB- | 000100-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000200-- | W | TEAM5------------- | GPB- | 000200-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000300-- | W | TEAM5------------- | GPB- | 000300-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000400-- | W | TEAM5------------- | GPB- | 000400-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000500-- | W | TEAM5------------- | GPB- | 000500-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000600-- | W | TEAM5------------- | GPB- | 000600-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPC-000100-- | W | TEAM5------------- | GPC- | 000100-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000200-- | W | TEAM5------------- | GPC- | 000200-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000300-- | W | TEAM5------------- | GPC- | 000300-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000400-- | W | TEAM5------------- | GPC- | 000400-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000500-- | W | TEAM5------------- | GPC- | 000500-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000600-- | W | TEAM5------------- | GPC- | 000600-- | Unit | 15 | 0 | HTEAM | Women's Group Phase - Group C |
| BKBWTEAM5-------------QFNL000100-- | W | TEAM5------------- | QFNL | 000100-- | Unit | 15 | 0 | HTEAM | Women's Quarterfinal |
| BKBWTEAM5-------------QFNL000200-- | W | TEAM5------------- | QFNL | 000200-- | Unit | 15 | 0 | HTEAM | Women's Quarterfinal |
| BKBWTEAM5-------------QFNL000300-- | W | TEAM5------------- | QFNL | 000300-- | Unit | 15 | 0 | HTEAM | Women's Quarterfinal |
| BKBWTEAM5-------------QFNL000400-- | W | TEAM5------------- | QFNL | 000400-- | Unit | 15 | 0 | HTEAM | Women's Quarterfinal |
| BKBWTEAM5-------------SFNL000100-- | W | TEAM5------------- | SFNL | 000100-- | Unit | 15 | 0 | HTEAM | Women's Semifinal |
| BKBWTEAM5-------------SFNL000200-- | W | TEAM5------------- | SFNL | 000200-- | Unit | 15 | 0 | HTEAM | Women's Semifinal |
| BKBWTEAM5-------------TMRY000100-- | W | TEAM5------------- | TMRY | 000100-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000200-- | W | TEAM5------------- | TMRY | 000200-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000300-- | W | TEAM5------------- | TMRY | 000300-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000400-- | W | TEAM5------------- | TMRY | 000400-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000500-- | W | TEAM5------------- | TMRY | 000500-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000600-- | W | TEAM5------------- | TMRY | 000600-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000700-- | W | TEAM5------------- | TMRY | 000700-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000800-- | W | TEAM5------------- | TMRY | 000800-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000900-- | W | TEAM5------------- | TMRY | 000900-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001000-- | W | TEAM5------------- | TMRY | 001000-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001100-- | W | TEAM5------------- | TMRY | 001100-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001200-- | W | TEAM5------------- | TMRY | 001200-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001300-- | W | TEAM5------------- | TMRY | 001300-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001400-- | W | TEAM5------------- | TMRY | 001400-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001500-- | W | TEAM5------------- | TMRY | 001500-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001600-- | W | TEAM5------------- | TMRY | 001600-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001700-- | W | TEAM5------------- | TMRY | 001700-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001800-- | W | TEAM5------------- | TMRY | 001800-- | Unit | 15 | 0 | NONE | Women's Preliminary Phase |
| BKBWTEAM5-------------VICTMEDAL--- | W | TEAM5------------- | VICT | MEDAL--- | Medals | 17 | 0 | NONE | Women's Victory Ceremony |

### CC @Organisation

`CC @Organisation` is a large common master-data table rather than a schedule-state enumeration. The downloaded Paris 2024 code page contains 258 `Organisation` rows. Use the source link in the index above as the authoritative value list when modeling `Organisation` fields.

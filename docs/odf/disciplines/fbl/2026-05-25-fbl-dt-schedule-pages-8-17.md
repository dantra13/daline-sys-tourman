
# ODF FBL Data Dictionary: DT_SCHEDULE, Pages 8-17

Source: `C:\Users\mella\Downloads\ODF_FBL_Data_Dictionary.pdf`, pages 8-17.

This note restructures the football `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` section into readable Markdown for domain modeling. It covers the competition schedule bulk message, schedule update behavior, session/unit fields, display ordering rules, hidden start/end times, start-list payloads for head-to-head units, XSD-aligned XML examples, and an appendix of English SC/CC values downloaded from the Paris 2024 ODF code pages.

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
| `HTEAM` | Team head-to-head units | Basketball, beach volleyball, handball, curling, ice hockey, etc. Football match units are team head-to-head in practice. |

The `Composition` component, meaning the athlete list under a competitor, is included only for `HATH` or `HCOUP`. It is not normally included for team head-to-head units like football schedule start lists.

## Managing Unknown Start Times

Some disciplines manage units by order rather than by a known start time. In that case, all affected units may be sent with the same `StartDate`, and follow-up units are flagged with `HideStartDate="Y"`. The `Order` attribute then determines display order.

If a unit uses a display phrase such as `Not before 17:00`, `SUN 29 - 2nd match on CC`, or `Follows`, use `StartText`. If `StartText` is `Not before hh:mm`, the sent `StartDate` is expected to use the same `hh:mm` value.

For team sports, the dictionary notes that `HideStartDate="Y"` is only used temporarily to remove times, not as the normal order-management mechanism.

## Display Sort Advice

When displaying schedule data, users should sort event units according to the discipline's common-code event-order mode.

| Discipline event-order mode | Display sort order |
|---|---|
| `LOC` | Day/filter by day, then session code, then location, then time regardless of `HideStartDate`, then `Order`. |
| `SESSION` or `DATE` | Day/filter by day, then session code, then time regardless of `HideStartDate`, then `Order`. |

Additional display rules:

- `Order` can be the match number for simplicity, especially when multiple matches share the same start time.
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
               ├─ @Bib
               ├─ Description (0,1)
               │  ├─ @TeamName
               │  └─ @IFId
               └─ Composition (0,1)
                  └─ Athlete (1,N)
                     ├─ @Code
                     ├─ @Order
                     ├─ @Bib
                     └─ Description (1,1)
                        ├─ @GivenName
                        ├─ @FamilyName
                        ├─ @Gender
                        ├─ @Organisation
                        ├─ @BirthDate
                        ├─ @IFId
                        └─ @Class
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional in dictionary, required by XSD | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional in dictionary, required by XSD | `S(20)` | Code-set version applicable to the message. |

## Session

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `SessionCode` | Mandatory | `S(10)` | Sports competition session code containing the event unit. Usually `DDD00`, where `DDD` is discipline and `00` is the discipline session number. |
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
| `Value` | Mandatory | `S(40)` | Name of the sports competition session. |

```xml
<Session SessionCode="FBL01"
         StartDate="2026-05-25T18:00:00+02:00"
         EndDate="2026-05-25T20:00:00+02:00"
         Leadin="0:00"
         Venue="BOR"
         VenueName="Bordeaux Stadium">
  <SessionName Language="ENG" Value="Football Session 1"/>
</Session>
```

## Unit

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `CC @Unit` | Full RSC for the unit. |
| `PhaseType` | Mandatory | `CC @PhaseType` | Phase type for the unit. |
| `UnitNum` | Optional | `S(15)` | Match/game/bout/race number or similar. |
| `ScheduleStatus` | Mandatory | `CC @ScheduleStatus` | Unit status. |
| `StartDate` | Optional | `DateTime` | Scheduled start date/time. May be omitted when `ScheduleStatus="UNSCHEDULED"`. For other statuses it is expected for display ordering, including `CANCELLED` and `POSTPONED`. Do not update it to actual start time; only update on `RESCHEDULED`. |
| `HideStartDate` | Optional | `S(1)` | Send `Y` when scheduled start should not be displayed. The time may be estimated/fake and used only for sorting. Omit when the scheduled start should be displayed. |
| `EndDate` | Optional | `DateTime` | Scheduled end date/time. Do not update it to actual end; only update on `RESCHEDULED` relative to `StartDate`. Not required when `UNSCHEDULED` or `CANCELLED`. |
| `HideEndDate` | Optional | `S(1)` | Send `Y` when scheduled end time should not be displayed. |
| `ActualStartDate` | Optional | `DateTime` | Expected once the event unit has started. |
| `ActualEndDate` | Optional | `DateTime` | Expected once the event unit has finished. |
| `Order` | Optional | Numeric `###0` | Display order. Required/recommended when `HideStartDate="Y"` or when multiple units start at the same time and a specific order is expected. Match number can be used. |
| `Medal` | Optional | `SC @UnitMedalType` | Medal indicator for the unit. |
| `Venue` | Optional | `CC @VenueCode` | Unit venue. Mandatory unless `UNSCHEDULED`. Can use `TBD` if unknown. |
| `Location` | Optional | `CC @Location` | Unit location. Mandatory unless `UNSCHEDULED`. Can use `TBD` if unknown or a discipline-generic code. |
| `MediaAccess` | Optional | `S(6)` | Non-competition only. `OPE` for open to media, `CLO` for closed. |
| `SessionCode` | Optional | `S(10)` | Session containing the unit. If a unit finishes in a different session due to interruption, keep the starting session code. |
| `ModificationIndicator` | Optional, mandatory in update message | `N`, `U` | `N` for new unit, `U` for updated unit. New unit is rare because most added units were already present as `UNSCHEDULED`. |

## Unit / StartText

`StartText` is used only when `HideStartDate="Y"`. English is mandatory in this case.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Language` | Mandatory | `CC @Language` | Language code of `Value`. |
| `Value` | Mandatory | `S(20)` or `SC @StartText` | Display text when `StartDate` should not be displayed. Use available codes or free text if no suitable code exists. |

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

Start-list information is sent only when the unit type is `HATH`, `HCOUP`, or `HTEAM` and at least one competitor is known.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `StartOrder` | Optional | Numeric | Competitor start order. |
| `SortOrder` | Mandatory | Numeric | Sort order for competitors in the event unit. Mainly used for display when `StartOrder` is absent. |

## Start / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes, or `SC @CompetitorPlace` | Competitor ID or placeholder. `TBD` means unknown now but expected later. `NOCOMP` means no competitor and none will arrive later. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Organisation` | Optional | `CC @Organisation` | Should be sent when known. |
| `Bib` | Optional | Same as discipline start-list message | Team bib number when `Type="T"`. |

## Competitor / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `TeamName` | Mandatory | `S(73)` | Team name when known; must be sent when available. |
| `IFId` | Optional | `S(16)` | Team IF number, when available. |

## Competitor / Composition / Athlete

Only send for `HATH` or `HCOUP`. If `Competitor/@Code="TBD"`, the `Competitor` element should not be sent.

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Athlete ID for the team member or individual athlete in the event unit. |
| `Order` | Mandatory | Numeric | Sort order for team members when `Competitor/@Type="T"`, or `1` when `Type="A"`. |
| `Bib` | Optional | Same as discipline start-list message | Individual athlete/team member bib number. |

## Athlete / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. Send if not null. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Participant gender. |
| `Organisation` | Mandatory | `CC @Organisation` | Organisation ID. |
| `BirthDate` | Optional | `YYYY-MM-DD` | Date of birth. |
| `IFId` | Optional | `S(16)` | Athlete IF number for the current discipline. |
| `Class` | Optional | `CC @DisciplineClass` | Sport class for events involving athletes with a disability. |

## Message Sort

Sort sessions by `Session @SessionCode`.

Sort units by `Unit @StartDate`, then `Unit @Order`, then `Unit @Code`. Units without `Unit @StartDate`, for example `UNSCHEDULED` units, are listed at the end in `Unit @Code` order.

## XSD-Aligned XML Example

The provided XSD draft could not be loaded directly because `odf2-structure.xsd` references `RecordBrokenType`, which is not defined in the supplied schema folder. The example below was validated against a temporary copy of the schema where only that unrelated unresolved type reference was replaced so the schema could parse. No original XSD files were modified.

The PDF lists `Session/@ModificationIndicator`, `Unit/@ModificationIndicator`, and `Unit/@MediaAccess`, but the supplied XSD draft does not expose these attributes in the corresponding schedule types. The validated example therefore omits them.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="FBL-------------------------------"
         DocumentType="DT_SCHEDULE"
         Version="12"
         FeedFlag="T"
         Date="2026-05-25"
         Time="10:00:00.000"
         LogicalDate="2026-05-25"
         Source="OVR">
  <Competition Gen="3.4" Sport="FBL-3.4" Codes="SOG-2024">
    <Session SessionCode="FBL01"
             StartDate="2026-05-25T18:00:00+02:00"
             EndDate="2026-05-25T20:00:00+02:00"
             Leadin="0:00"
             Venue="BOR"
             VenueName="Bordeaux Stadium"
             SessionType="EVE"
             Medal="0"
             FOP="1">
      <SessionName Language="ENG" Value="Football Session 1"/>
    </Session>
    <Unit Code="FBLWTEAM11------------GPA-000100--"
          PhaseType="3"
          UnitNum="1"
          ScheduleStatus="SCHEDULED"
          StartDate="2026-05-25T18:00:00+02:00"
          EndDate="2026-05-25T20:00:00+02:00"
          Order="1"
          Medal="0"
          Venue="BOR"
          Location="BOR"
          SessionCode="FBL01">
      <ItemName Language="ENG" Value="Women's Group A"/>
      <VenueDescription VenueName="Bordeaux Stadium" LocationName="Bordeaux Stadium, Bordeaux"/>
      <StartList>
        <Start StartOrder="1" SortOrder="1">
          <Competitor Code="FBLWTEAM11-----RSA01" Type="T" Organisation="RSA">
            <Description TeamName="South Africa"/>
          </Competitor>
        </Start>
        <Start StartOrder="2" SortOrder="2">
          <Competitor Code="FBLWTEAM11-----BRA01" Type="T" Organisation="BRA">
            <Description TeamName="Brazil"/>
          </Competitor>
        </Start>
      </StartList>
    </Unit>
  </Competition>
</OdfBody>
```

## Modeling Notes

- Treat `DT_SCHEDULE` as a discipline-level replacement snapshot. On ingest, replace the discipline schedule state; do not merge it incrementally.
- Treat `DT_SCHEDULE_UPDATE` as a partial patch keyed by `Unit/@Code` and, where sessions are present, session identity. Do not infer changes for units not present in the update.
- Keep scheduled dates (`StartDate`, `EndDate`) separate from actual dates (`ActualStartDate`, `ActualEndDate`). Actual competition timing should not overwrite schedule timing.
- `HideStartDate` and `StartText` are presentation contracts but also affect sort/display semantics. They deserve explicit fields, not ad hoc UI-only handling.
- `Order` is a domain-relevant display order, especially when multiple football matches share a start time or when hidden starts are used.
- Start-list data in schedule is minimal and unit-scoped. It tells who is known for the unit; detailed lineups belong in `DT_RESULT`.
- For football, team start lists can carry `Competitor/Description` but normally do not carry athlete `Composition` in schedule because `Composition` is limited to `HATH`/`HCOUP` by the dictionary.
- The supplied XSD draft and PDF are not perfectly identical. Preserve the PDF semantics for domain modeling, but validate emitted XML against the exact schema release used by the target integration.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-25. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index
| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Source` | FBL | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm) |
| `SC @UnitMedalType` | GEN | 4 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_UnitMedalType_SOG_GEN.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_UnitMedalType_SOG_GEN.htm) |
| `SC @StartText` | FBL aggregate | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @CompetitorPlace` | FBL aggregate | 30 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @Discipline` | Common FBL row embedded | 1 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Discipline.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Discipline.htm) |
| `CC @Unit` | FBL EventUnit rows | 93 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @PhaseType` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PhaseType.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PhaseType.htm) |
| `CC @ScheduleStatus` | Common | 11 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ScheduleStatus.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ScheduleStatus.htm) |
| `CC @SessionType` | Common | 6 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SessionType.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SessionType.htm) |
| `CC @Language` | Common | 11 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Language.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Language.htm) |
| `CC @VenueCode` | FBL venues inferred through FBL locations | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm) |
| `CC @Location` | FBL locations | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm) |
| `CC @PersonGender` | Common | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm) |
| `CC @Organisation` | Common master data | 258 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |
| `CC @DisciplineClass` | Not found in Paris 2024 common-code index | 0 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm) |

### SC @Source
| Code | ENG Description |
| --- | --- |
| BORFBL1 | Origin for messages from OVR at BOR for FBL |
| LYOFBL1 | Origin for messages from OVR at LYO for FBL |
| MRSFBL1 | Origin for messages from OVR at MRS for FBL |
| NANFBL1 | Origin for messages from OVR at NAN for FBL |
| NICFBL1 | Origin for messages from OVR at NIC for FBL |
| PDPFBL1 | Origin for messages from OVR at PDP for FBL |
| STEFBL1 | Origin for messages from OVR at STE for FBL |

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
| TBD | TBD |

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

### CC @Discipline (FBL row)
| Code | Id | Eventorder | scheduled | IF | ENG Description |
| --- | --- | --- | --- | --- | --- |
| FBL------------------------------- | FBL | DATE | Y | FIFA | Football |

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

### CC @PersonGender
| Id | ENG Description |
| --- | --- |
| F | Female |
| M | Male |
| X | Unspecified |

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
| Code | Gender | Event | phase | Eventunit | Level | Order | schedule | medalflag | Eventunittype | ENG Description |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| FBL------------------------------- | - | ------------------ | ---- | -------- | Discipline | 2 | N | 0 | NONE | Football |
| FBL-----------------------BOR----- | - | ------------------ | ---- | BOR----- | Venue | 3 | N | 0 | NONE | BOR |
| FBL-----------------------LYO----- | - | ------------------ | ---- | LYO----- | Venue | 3 | N | 0 | NONE | LYO |
| FBLM------------------------------ | M | ------------------ | ---- | -------- | Gender | 10 | N | 0 | NONE | Men |
| FBL-----------------------MRS----- | - | ------------------ | ---- | MRS----- | Venue | 3 | N | 0 | NONE | MRS |
| FBLMTEAM11------------------------ | M | TEAM11------------ | ---- | -------- | Event | 13 | N | 0 | NONE | Men |
| FBLMTEAM11------------FNL--------- | M | TEAM11------------ | FNL- | -------- | Phase | 14 | N | 0 | NONE | Men's Finals |
| FBLMTEAM11------------FNL-000100-- | M | TEAM11------------ | FNL- | 000100-- | Unit | 15 | Y | 1 | HTEAM | Men's Gold Medal Match |
| FBLMTEAM11------------FNL-000200-- | M | TEAM11------------ | FNL- | 000200-- | Unit | 15 | Y | 3 | HTEAM | Men's Bronze Medal Match |
| FBLMTEAM11------------GP---------- | M | TEAM11------------ | GP-- | -------- | Phase | 14 | N | 0 | NONE | Men's Group Stage |
| FBLMTEAM11------------GPA--------- | M | TEAM11------------ | GPA- | -------- | Phase | 14 | N | 0 | NONE | Men's Group A |
| FBLMTEAM11------------GPA-000100-- | M | TEAM11------------ | GPA- | 000100-- | Unit | 15 | Y | 0 | HTEAM | Men's Group A |
| FBLMTEAM11------------GPA-000200-- | M | TEAM11------------ | GPA- | 000200-- | Unit | 15 | Y | 0 | HTEAM | Men's Group A |
| FBLMTEAM11------------GPA-000300-- | M | TEAM11------------ | GPA- | 000300-- | Unit | 15 | Y | 0 | HTEAM | Men's Group A |
| FBLMTEAM11------------GPA-000400-- | M | TEAM11------------ | GPA- | 000400-- | Unit | 15 | Y | 0 | HTEAM | Men's Group A |
| FBLMTEAM11------------GPA-000500-- | M | TEAM11------------ | GPA- | 000500-- | Unit | 15 | Y | 0 | HTEAM | Men's Group A |
| FBLMTEAM11------------GPA-000600-- | M | TEAM11------------ | GPA- | 000600-- | Unit | 15 | Y | 0 | HTEAM | Men's Group A |
| FBLMTEAM11------------GPB--------- | M | TEAM11------------ | GPB- | -------- | Phase | 14 | N | 0 | NONE | Men's Group B |
| FBLMTEAM11------------GPB-000100-- | M | TEAM11------------ | GPB- | 000100-- | Unit | 15 | Y | 0 | HTEAM | Men's Group B |
| FBLMTEAM11------------GPB-000200-- | M | TEAM11------------ | GPB- | 000200-- | Unit | 15 | Y | 0 | HTEAM | Men's Group B |
| FBLMTEAM11------------GPB-000300-- | M | TEAM11------------ | GPB- | 000300-- | Unit | 15 | Y | 0 | HTEAM | Men's Group B |
| FBLMTEAM11------------GPB-000400-- | M | TEAM11------------ | GPB- | 000400-- | Unit | 15 | Y | 0 | HTEAM | Men's Group B |
| FBLMTEAM11------------GPB-000500-- | M | TEAM11------------ | GPB- | 000500-- | Unit | 15 | Y | 0 | HTEAM | Men's Group B |
| FBLMTEAM11------------GPB-000600-- | M | TEAM11------------ | GPB- | 000600-- | Unit | 15 | Y | 0 | HTEAM | Men's Group B |
| FBLMTEAM11------------GPC--------- | M | TEAM11------------ | GPC- | -------- | Phase | 14 | N | 0 | NONE | Men's Group C |
| FBLMTEAM11------------GPC-000100-- | M | TEAM11------------ | GPC- | 000100-- | Unit | 15 | Y | 0 | HTEAM | Men's Group C |
| FBLMTEAM11------------GPC-000200-- | M | TEAM11------------ | GPC- | 000200-- | Unit | 15 | Y | 0 | HTEAM | Men's Group C |
| FBLMTEAM11------------GPC-000300-- | M | TEAM11------------ | GPC- | 000300-- | Unit | 15 | Y | 0 | HTEAM | Men's Group C |
| FBLMTEAM11------------GPC-000400-- | M | TEAM11------------ | GPC- | 000400-- | Unit | 15 | Y | 0 | HTEAM | Men's Group C |
| FBLMTEAM11------------GPC-000500-- | M | TEAM11------------ | GPC- | 000500-- | Unit | 15 | Y | 0 | HTEAM | Men's Group C |
| FBLMTEAM11------------GPC-000600-- | M | TEAM11------------ | GPC- | 000600-- | Unit | 15 | Y | 0 | HTEAM | Men's Group C |
| FBLMTEAM11------------GPD--------- | M | TEAM11------------ | GPD- | -------- | Phase | 14 | N | 0 | NONE | Men's Group D |
| FBLMTEAM11------------GPD-000100-- | M | TEAM11------------ | GPD- | 000100-- | Unit | 15 | Y | 0 | HTEAM | Men's Group D |
| FBLMTEAM11------------GPD-000200-- | M | TEAM11------------ | GPD- | 000200-- | Unit | 15 | Y | 0 | HTEAM | Men's Group D |
| FBLMTEAM11------------GPD-000300-- | M | TEAM11------------ | GPD- | 000300-- | Unit | 15 | Y | 0 | HTEAM | Men's Group D |
| FBLMTEAM11------------GPD-000400-- | M | TEAM11------------ | GPD- | 000400-- | Unit | 15 | Y | 0 | HTEAM | Men's Group D |
| FBLMTEAM11------------GPD-000500-- | M | TEAM11------------ | GPD- | 000500-- | Unit | 15 | Y | 0 | HTEAM | Men's Group D |
| FBLMTEAM11------------GPD-000600-- | M | TEAM11------------ | GPD- | 000600-- | Unit | 15 | Y | 0 | HTEAM | Men's Group D |
| FBLMTEAM11------------QFNL-------- | M | TEAM11------------ | QFNL | -------- | Phase | 14 | N | 0 | NONE | Men's Quarter-finals |
| FBLMTEAM11------------QFNL000100-- | M | TEAM11------------ | QFNL | 000100-- | Unit | 15 | Y | 0 | HTEAM | Men's Quarter-final |
| FBLMTEAM11------------QFNL000200-- | M | TEAM11------------ | QFNL | 000200-- | Unit | 15 | Y | 0 | HTEAM | Men's Quarter-final |
| FBLMTEAM11------------QFNL000300-- | M | TEAM11------------ | QFNL | 000300-- | Unit | 15 | Y | 0 | HTEAM | Men's Quarter-final |
| FBLMTEAM11------------QFNL000400-- | M | TEAM11------------ | QFNL | 000400-- | Unit | 15 | Y | 0 | HTEAM | Men's Quarter-final |
| FBLMTEAM11------------SFNL-------- | M | TEAM11------------ | SFNL | -------- | Phase | 14 | N | 0 | NONE | Men's Semi-finals |
| FBLMTEAM11------------SFNL000100-- | M | TEAM11------------ | SFNL | 000100-- | Unit | 15 | Y | 0 | HTEAM | Men's Semi-final |
| FBLMTEAM11------------SFNL000200-- | M | TEAM11------------ | SFNL | 000200-- | Unit | 15 | Y | 0 | HTEAM | Men's Semi-final |
| FBLMTEAM11------------VICT-------- | M | TEAM11------------ | VICT | -------- | Phase | 14 | N | 0 | NONE | Men's Victory Ceremony |
| FBLMTEAM11------------VICTBRONZE-- | M | TEAM11------------ | VICT | BRONZE-- | Medals | 17 | Y | 0 | NONE | Men's Bronze Ceremony |
| FBLMTEAM11------------VICTMEDAL--- | M | TEAM11------------ | VICT | MEDAL--- | Medals | 17 | Y | 0 | NONE | Men's Victory Ceremony |
| FBL-----------------------NAN----- | - | ------------------ | ---- | NAN----- | Venue | 3 | N | 0 | NONE | NAN |
| FBL-----------------------NIC----- | - | ------------------ | ---- | NIC----- | Venue | 3 | N | 0 | NONE | NIC |
| FBL-----------------------PDP----- | - | ------------------ | ---- | PDP----- | Venue | 3 | N | 0 | NONE | PDP |
| FBL-----------------------STE----- | - | ------------------ | ---- | STE----- | Venue | 3 | N | 0 | NONE | STE |
| FBL-----------------------STR----- | - | ------------------ | ---- | STR----- | Venue | 3 | N | 0 | NONE | STR |
| FBL-----------------------TOU----- | - | ------------------ | ---- | TOU----- | Venue | 3 | N | 0 | NONE | TOU |
| FBLW------------------------------ | W | ------------------ | ---- | -------- | Gender | 10 | N | 0 | NONE | Women |
| FBLWTEAM11------------------------ | W | TEAM11------------ | ---- | -------- | Event | 13 | N | 0 | NONE | Women |
| FBLWTEAM11------------FNL--------- | W | TEAM11------------ | FNL- | -------- | Phase | 14 | N | 0 | NONE | Women's Finals |
| FBLWTEAM11------------FNL-000100-- | W | TEAM11------------ | FNL- | 000100-- | Unit | 15 | Y | 1 | HTEAM | Women's Gold Medal Match |
| FBLWTEAM11------------FNL-000200-- | W | TEAM11------------ | FNL- | 000200-- | Unit | 15 | Y | 3 | HTEAM | Women's Bronze Medal Match |
| FBLWTEAM11------------GP---------- | W | TEAM11------------ | GP-- | -------- | Phase | 14 | N | 0 | NONE | Women's Group Stage |
| FBLWTEAM11------------GPA--------- | W | TEAM11------------ | GPA- | -------- | Phase | 14 | N | 0 | NONE | Women's Group A |
| FBLWTEAM11------------GPA-000100-- | W | TEAM11------------ | GPA- | 000100-- | Unit | 15 | Y | 0 | HTEAM | Women's Group A |
| FBLWTEAM11------------GPA-000200-- | W | TEAM11------------ | GPA- | 000200-- | Unit | 15 | Y | 0 | HTEAM | Women's Group A |
| FBLWTEAM11------------GPA-000300-- | W | TEAM11------------ | GPA- | 000300-- | Unit | 15 | Y | 0 | HTEAM | Women's Group A |
| FBLWTEAM11------------GPA-000400-- | W | TEAM11------------ | GPA- | 000400-- | Unit | 15 | Y | 0 | HTEAM | Women's Group A |
| FBLWTEAM11------------GPA-000500-- | W | TEAM11------------ | GPA- | 000500-- | Unit | 15 | Y | 0 | HTEAM | Women's Group A |
| FBLWTEAM11------------GPA-000600-- | W | TEAM11------------ | GPA- | 000600-- | Unit | 15 | Y | 0 | HTEAM | Women's Group A |
| FBLWTEAM11------------GPB--------- | W | TEAM11------------ | GPB- | -------- | Phase | 14 | N | 0 | NONE | Women's Group B |
| FBLWTEAM11------------GPB-000100-- | W | TEAM11------------ | GPB- | 000100-- | Unit | 15 | Y | 0 | HTEAM | Women's Group B |
| FBLWTEAM11------------GPB-000200-- | W | TEAM11------------ | GPB- | 000200-- | Unit | 15 | Y | 0 | HTEAM | Women's Group B |
| FBLWTEAM11------------GPB-000300-- | W | TEAM11------------ | GPB- | 000300-- | Unit | 15 | Y | 0 | HTEAM | Women's Group B |
| FBLWTEAM11------------GPB-000400-- | W | TEAM11------------ | GPB- | 000400-- | Unit | 15 | Y | 0 | HTEAM | Women's Group B |
| FBLWTEAM11------------GPB-000500-- | W | TEAM11------------ | GPB- | 000500-- | Unit | 15 | Y | 0 | HTEAM | Women's Group B |
| FBLWTEAM11------------GPB-000600-- | W | TEAM11------------ | GPB- | 000600-- | Unit | 15 | Y | 0 | HTEAM | Women's Group B |
| FBLWTEAM11------------GPC--------- | W | TEAM11------------ | GPC- | -------- | Phase | 14 | N | 0 | NONE | Women's Group C |
| FBLWTEAM11------------GPC-000100-- | W | TEAM11------------ | GPC- | 000100-- | Unit | 15 | Y | 0 | HTEAM | Women's Group C |
| FBLWTEAM11------------GPC-000200-- | W | TEAM11------------ | GPC- | 000200-- | Unit | 15 | Y | 0 | HTEAM | Women's Group C |
| FBLWTEAM11------------GPC-000300-- | W | TEAM11------------ | GPC- | 000300-- | Unit | 15 | Y | 0 | HTEAM | Women's Group C |
| FBLWTEAM11------------GPC-000400-- | W | TEAM11------------ | GPC- | 000400-- | Unit | 15 | Y | 0 | HTEAM | Women's Group C |
| FBLWTEAM11------------GPC-000500-- | W | TEAM11------------ | GPC- | 000500-- | Unit | 15 | Y | 0 | HTEAM | Women's Group C |
| FBLWTEAM11------------GPC-000600-- | W | TEAM11------------ | GPC- | 000600-- | Unit | 15 | Y | 0 | HTEAM | Women's Group C |
| FBLWTEAM11------------QFNL-------- | W | TEAM11------------ | QFNL | -------- | Phase | 14 | N | 0 | NONE | Women's Quarter-finals |
| FBLWTEAM11------------QFNL000100-- | W | TEAM11------------ | QFNL | 000100-- | Unit | 15 | Y | 0 | HTEAM | Women's Quarter-final |
| FBLWTEAM11------------QFNL000200-- | W | TEAM11------------ | QFNL | 000200-- | Unit | 15 | Y | 0 | HTEAM | Women's Quarter-final |
| FBLWTEAM11------------QFNL000300-- | W | TEAM11------------ | QFNL | 000300-- | Unit | 15 | Y | 0 | HTEAM | Women's Quarter-final |
| FBLWTEAM11------------QFNL000400-- | W | TEAM11------------ | QFNL | 000400-- | Unit | 15 | Y | 0 | HTEAM | Women's Quarter-final |
| FBLWTEAM11------------SFNL-------- | W | TEAM11------------ | SFNL | -------- | Phase | 14 | N | 0 | NONE | Women's Semi-finals |
| FBLWTEAM11------------SFNL000100-- | W | TEAM11------------ | SFNL | 000100-- | Unit | 15 | Y | 0 | HTEAM | Women's Semi-final |
| FBLWTEAM11------------SFNL000200-- | W | TEAM11------------ | SFNL | 000200-- | Unit | 15 | Y | 0 | HTEAM | Women's Semi-final |
| FBLWTEAM11------------VICT-------- | W | TEAM11------------ | VICT | -------- | Phase | 14 | N | 0 | NONE | Women's Victory Ceremony |
| FBLWTEAM11------------VICTBRONZE-- | W | TEAM11------------ | VICT | BRONZE-- | Medals | 17 | Y | 0 | NONE | Women's Bronze Ceremony |
| FBLWTEAM11------------VICTMEDAL--- | W | TEAM11------------ | VICT | MEDAL--- | Medals | 17 | Y | 0 | NONE | Women's Victory Ceremony |

### CC @Organisation

`CC @Organisation` is a large common master-data table rather than a schedule-state enumeration. The downloaded Paris 2024 code page contains 258 `Organisation` rows. Use the source link in the index above as the authoritative value list when modeling `Organisation` fields.

### CC @DisciplineClass

The football dictionary references `CC @DisciplineClass` for para sport classification, but the Paris 2024 common-code index downloaded for this appendix does not expose a `DisciplineClass.htm` page. Treat this as an externally supplied classification code set and validate it against the relevant para-sport dictionary/code release when that scope is implemented.

# ODF VBV Data Dictionary: DT_SCHEDULE / DT_SCHEDULE_UPDATE, Pages 8-16

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 8-16.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note restructures the Beach Volleyball `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` section into a practical reference for
the discipline-level competition timetable. It covers the bulk vs update semantics, session and unit structure, hidden
start-time handling, start-list payload for head-to-head units, and message-level sorting.

## 2.3.1 Competition schedule / Competition schedule update

`DT_SCHEDULE` is a bulk discipline-scoped message containing the complete schedule for all event units required to run
a competition. `DT_SCHEDULE_UPDATE` is an incremental update message that carries only modified units or sessions.

Key rules from the dictionary:

- Bulk arrival resets all previous schedule information for that discipline; any prior `DT_SCHEDULE_UPDATE` messages
  should be discarded.
- Event units carrying the common-codes `schedule` flag `Y` or `S` are always present in schedule messages regardless
  of status; units without status are sent as `UNSCHEDULED`.
- Unofficial training and press conferences are excluded.
- `StartList` is only included when the unit type is `HATH`, `HCOUP`, or `HTEAM` and at least one competitor is known.
  Beach Volleyball uses `HTEAM`.
- `Composition` (athlete listing) is only included when the unit type is `HATH` or `HCOUP` and therefore is not used
  for VBV.

When start times are not yet known the section prescribes a `HideStartDate="Y"` convention combined with `Order` and
`StartText` for ordering and display, with examples such as "Not before 17:00", "SUN 29 - 2nd match on CC", or
"Follows". If `StartText` is `Not before hh:mm` the section requires `StartDate` to share that `hh:mm`.

End-user sort guidance from the dictionary:

- Discipline defined as `LOC`: day, then `SessionCode`, then location, then `StartDate` (even when hidden), then
  `Order`.
- Discipline defined as `SESSION` or `DATE`: day, then `SessionCode`, then `StartDate` (even when hidden), then
  `Order`.

When the message is emitted by the Competition Schedule application before transfer of control to OVR, the section
specifies that `Competition/ExtendedInfos/ExtendedInfo` carries `Type=CS, Code=VERSION` and `Type=CS, Code=STATUS`
entries with application-supplied values.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition identifier. |
| `DocumentCode` | `CC@DISCIPLINE` | Full RSC at the discipline level. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Bulk schedule or update message. |
| `DocumentSubtype` | `SYNC` | Used on `DT_SCHEDULE` for ODF-client re-synchronisation after control is transferred to OVR. Not applicable for `_UPDATE` messages. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | N/A | Not used. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| Message | When to send |
|---|---|
| `DT_SCHEDULE` | As bulk message when available before the Games; resent multiple times until a confirmed cut-off date. Must not be sent after transfer of control to OVR. |
| `DT_SCHEDULE_UPDATE` | At any time there has been a schedule modification to a previously sent bulk or update, including addition of head-to-head start-list details. Issued by OVR. |
| Start-list updates for H2H | Sent immediately when officially known, typically as soon as the preceding unit is `OFFICIAL`. |

Text-description changes (as opposed to code changes) do not trigger a resend; the new descriptions are used in future
messages.

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- Session (0,N)
        +-- @SessionCode
        +-- @StartDate
        +-- @HideStartDate
        +-- @EndDate
        +-- @HideEndDate
        +-- @Leadin
        +-- @Venue
        +-- @VenueName
        +-- @SessionStatus
        +-- @SessionType
        +-- @Medal
        +-- @FOP
        +-- SessionName (1,N)
        |   +-- @Language
        |   +-- @Value
        +-- Unit (0,N)
            +-- @Code
            +-- @PhaseType
            +-- @UnitNum
            +-- @HideUnitNum
            +-- @ScheduleStatus
            +-- @StartDate
            +-- @HideStartDate
            +-- @EndDate
            +-- @HideEndDate
            +-- @ActualStartDate
            +-- @ActualEndDate
            +-- @Order
            +-- @Medal
            +-- @Venue
            +-- @Location
            +-- @SessionCode
            +-- StartText (0,N)
            |   +-- @Language
            |   +-- @Value
            +-- ItemName (1,N)
            |   +-- @Language
            |   +-- @Value
            +-- ItemDescription (0,N)
            |   +-- @Language
            |   +-- (text)
            +-- VenueDescription (0,1)
            |   +-- @VenueName
            |   +-- @LocationName
            +-- StartList (0,1)
                +-- Start (1,N)
                    +-- @StartOrder
                    +-- @SortOrder
                    +-- PreviousUnit (0,N)
                    |   +-- @Unit
                    |   +-- @Value
                    |   +-- @WLT
                    +-- Competitor (1,1)
                        +-- @Code
                        +-- @Type
                        +-- @Organisation
                        +-- Description (0,1)
                            +-- @TeamName
                            +-- @IFId
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | O | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | O | `S(20)` | Code-set version applicable to the message. |

### `Competition / Session`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `SessionCode` | M | `S(10)` | Session code, usually `DDD00` where `DDD` is the discipline and `00` the session number within the discipline. |
| `StartDate` | M | DateTime | Scheduled start date and time. |
| `HideStartDate` | O | `Y` | `Y` only if the scheduled session start time should not be displayed. Time is still used for sorting. Applies during early `DT_SCHEDULE` stages while details remain under embargo. |
| `EndDate` | M | DateTime | Scheduled end date and time. |
| `HideEndDate` | O | `Y` | `Y` only if the scheduled session end time should not be displayed. Sorting still uses the value. |
| `Leadin` | O | `m:ss` | Time from session start to first scheduled unit. |
| `Venue` | M | `CC@VENUE` ID | Venue where the session takes place. |
| `VenueName` | M | `CC@VENUE` English description | Venue name (not code). |
| `SessionStatus` | O | `CC@SHEDULESTATUS` ID | Only `CANCELLED` is used when applicable; all other sessions are assumed scheduled. No running/finished transitions are sent at session level. |
| `SessionType` | O | `CC@SESSION_TYPE` ID | Session type. |
| `Medal` | O | `#0` | Number of gold medals planned to be determined in the session, derived from the assigned units. |
| `FOP` | O | `#0` | Number of fields of play planned for the session. Pre-Games only, before the schedule is finalised; do not include during the Games period. |

Note: the PDF spells the attribute `SessionStatus` and references `CC@SHEDULESTATUS` for it, while the unit-level
`ScheduleStatus` references `CC@SCHEDULESTATUS`. The two spellings appear to refer to the same code page; this is
preserved as printed below.

### `Session / SessionName`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Language` | M | `CC@LANGUAGE` ID | Language of the session name. |
| `Value` | M | `S(40)` | Session name in the given language. |

### `Competition / Unit`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `CC@EVENT_UNIT` | Full RSC for the unit. |
| `PhaseType` | M | `CC@PHASE_TYPE` ID | Phase type for the unit. |
| `UnitNum` | O | `S(15)` | Match/game/bout/race number or similar. |
| `HideUnitNum` | O | `Y` | `Y` only if `UnitNum` should not be displayed (for example, when gold-medal-match details are not yet confirmed). |
| `ScheduleStatus` | M | `CC@SCHEDULESTATUS` ID | Unit status. |
| `StartDate` | O | DateTime | Scheduled start date-time. May be omitted when `ScheduleStatus="UNSCHEDULED"`. Required for other statuses (including `CANCELLED` and `POSTPONED`) to keep display ordering correct. Not updated when the unit actually starts; only updated together with status `RESCHEDULED`. When `HideStartDate="Y"`, fill with the session start time (or the start time of the unit group) and use `Order` for sorting; team sports use `HideStartDate="Y"` only temporarily. |
| `HideStartDate` | O | `Y` | `Y` if the scheduled start time should not be displayed (it may be an estimate or placeholder). Time is still used for sorting. |
| `EndDate` | O | DateTime | Scheduled end date-time. Not updated when the unit actually ends. Not required for `UNSCHEDULED` or `CANCELLED`. |
| `HideEndDate` | O | `Y` | `Y` if the scheduled end time should not be displayed. |
| `ActualStartDate` | O | DateTime | Expected once the unit has started. |
| `ActualEndDate` | O | DateTime | Expected once the unit has finished. |
| `Order` | O | Positive integer | Display order; required at least for affected sessions when `HideStartDate="Y"` and recommended for the whole discipline when the concept is in use. Can be the match number to give a stable order for units sharing a start time. |
| `Medal` | O | `SCGEN@UnitMedalType` | Indicator of medal awarded for this unit. |
| `Venue` | O | `CC@VENUE` ID | Venue where the unit takes place. Mandatory unless `UNSCHEDULED`. `TBD` allowed when not yet known. |
| `Location` | O | `CC@LOCATION` ID | Location for the unit. Mandatory unless `UNSCHEDULED`. `TBD` or a discipline-generic code allowed when not yet known. |
| `SessionCode` | O | `S(10)` | Session code containing the unit. Format as for the session-level `SessionCode`. |

### `Unit / StartText`

Only used when `HideStartDate="Y"`. English is mandatory in that case.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Language` | M | `CC@LANGUAGE` ID | Language of the value. |
| `Value` | M | `S(20)` or `SC@StartText` | Text shown when `StartDate` is hidden. Use a code from `SC@StartText` when one is suitable, otherwise free text (for example "Not before 17:00", "Follows"). |

### `Unit / ItemName`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Language` | M | `CC@LANGUAGE` ID | Language of the value. |
| `Value` | M | `CC@EVENT_UNIT` English short description | Unit description matching the RSC. Only English is expected for competition units. For non-competition schedules, use the appropriate description. |

### `Unit / ItemDescription`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Language` | M | `CC@LANGUAGE` ID | Language of the element text. |
| (text) | M | Free text | Description for non-competition schedule items. |

### `Unit / VenueDescription`

Mandatory when `Unit/@Venue` is included.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `VenueName` | M | `CC@VENUE` English description | Venue name (not code). |
| `LocationName` | M | `CC@LOCATION` English description | Location name (not code). |

### `Unit / StartList / Start`

`StartList` is only sent when the unit type is `HATH`, `HCOUP`, or `HTEAM` and at least one competitor is known.
Beach Volleyball uses `HTEAM`.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `StartOrder` | O | Positive integer | Competitor start order. |
| `SortOrder` | M | Positive integer | Display ordering of competitors within the unit, used in absence of `StartOrder`. |

### `Start / PreviousUnit`

Sent only for `HATH`, `HCOUP`, or `HTEAM` units, and only while the real competitors are not yet known.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Unit` | O | `CC@EVENT_UNIT` | Full RSC of the previous unit the competitor will come from. Populated only when progression is 100% confirmed; removed once the real competitors are known. |
| `Value` | O | `SC@CompetitorPlace` | Indicator for an unknown competitor. `TBD` when the competitor is unknown but expected; `NOCOMP` when no competitor will come later. |
| `WLT` | O | `SC@WLT` | `W` or `L` indicating whether the winner or loser of the previous unit advances. Removed once real competitors are known. |

### `Start / Competitor`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes, or `SC@CompetitorPlace` | Competitor ID; `TBD` if not yet known while the opposing competitor is known; `NOCOMP` when no competitor will come later. |
| `Type` | M | `A`, `T` | `A` for athlete, `T` for team. VBV uses `T`. |
| `Organisation` | O | `CC@ORGANISATION` ID | Sent when known. |

### `Competitor / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `TeamName` | M | `S(73)` | Team name when known; must be sent when available. |
| `IFId` | O | `S(40)` | Team IF number, sent when available. |

## Samples from the Dictionary, Normalized

The dictionary includes one printed sample in this section, an Athletics session illustration. It is reproduced here
normalized for spacing only. The PDF prints `LeadIn="5:00"` on the `Session` element while the attribute table names it
`Leadin`; the table-name casing is preserved in the structure tree above, while the sample is shown as printed.

### Session header (general sample as printed)

```xml
<Session Code="ATH01" StartDate="2016-08-12T10:00:00+01:00"
         EndDate="2016-08-12T14:00:00+05:00" LeadIn="5:00"
         Venue="STA" VenueName="Olympic Stadium">
  <SessionName Language="ENG" Value="Athletics Session 1"/>
</Session>
<Session Code="ATH02" StartDate="2016-08-12T18:00:00+01:00"
         EndDate="2016-08-12T21:00:00+05:00" LeadIn="5:00"
         Venue="STA" VenueName="Olympic Stadium">
  <SessionName Language="ENG" Value="Athletics Session 2"/>
</Session>
```

### Hidden start-time order example as printed in section text

The text in 2.3.1.1 illustrates the `HideStartDate` ordering pattern with this table:

| Start Time | Display | Unit | HideStartDate | Location | Order |
|---|---|---|---:|---|---:|
| 12:00 | 12:00 | Unit 1 | N | Court 2 | 1 |
| 12:00 | Match 2 Court 2 | Unit 2 | Y | Court 2 | 2 |
| 12:00 | Match 3 Court 2 | Unit 3 | Y | Court 2 | 3 |
| 16:30 | Not before 16:30 | Unit 4 | Y | Court 2 | 4 |

## XSD-Aligned XML Example

The dictionary does not print a full Beach Volleyball schedule example in this section. The fragment below combines the
documented session/unit/start-list structure into a single `Competition` payload consistent with the attribute tables.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <Session SessionCode="VBV01" StartDate="2026-10-23T10:00:00+00:00"
           EndDate="2026-10-23T14:00:00+00:00" Leadin="5:00"
           Venue="BVA" VenueName="Beach Volleyball Arena" SessionType="C" Medal="0">
    <SessionName Language="ENG" Value="Beach Volleyball Session 1"/>
    <Unit Code="VBVMTEAM2-------------QF-000100--" PhaseType="QF"
          UnitNum="101" ScheduleStatus="SCHEDULED"
          StartDate="2026-10-23T10:00:00+00:00"
          EndDate="2026-10-23T11:00:00+00:00"
          Order="1" Venue="BVA" Location="BVA01" SessionCode="VBV01">
      <ItemName Language="ENG" Value="Men's Quarterfinal 1"/>
      <VenueDescription VenueName="Beach Volleyball Arena" LocationName="Centre Court"/>
      <StartList>
        <Start SortOrder="1" StartOrder="1">
          <Competitor Code="VBVMTEAM2---GER01" Type="T" Organisation="GER">
            <Description TeamName="Smith/Jones"/>
          </Competitor>
        </Start>
        <Start SortOrder="2" StartOrder="2">
          <Competitor Code="VBVMTEAM2---BRA01" Type="T" Organisation="BRA">
            <Description TeamName="Silva/Santos"/>
          </Competitor>
        </Start>
      </StartList>
    </Unit>
    <Unit Code="VBVMTEAM2-------------SF-000100--" PhaseType="SF"
          UnitNum="201" ScheduleStatus="SCHEDULED"
          StartDate="2026-10-23T12:00:00+00:00"
          HideStartDate="Y" Order="2"
          Venue="BVA" Location="BVA01" SessionCode="VBV01">
      <StartText Language="ENG" Value="Follows"/>
      <ItemName Language="ENG" Value="Men's Semifinal 1"/>
      <VenueDescription VenueName="Beach Volleyball Arena" LocationName="Centre Court"/>
      <StartList>
        <Start SortOrder="1">
          <PreviousUnit Unit="VBVMTEAM2-------------QF-000100--" Value="TBD" WLT="W"/>
          <Competitor Code="TBD" Type="T"/>
        </Start>
        <Start SortOrder="2">
          <PreviousUnit Unit="VBVMTEAM2-------------QF-000200--" Value="TBD" WLT="W"/>
          <Competitor Code="TBD" Type="T"/>
        </Start>
      </StartList>
    </Unit>
  </Session>
</Competition>
```

## Message Sort

The dictionary specifies sorting by `Session/@SessionCode`, then within a session by `Unit/@StartDate`, then by
`Unit/@Order`, then by `Unit/@Code`. Units without a `StartDate` (for example `UNSCHEDULED`) are listed at the end in
`Unit/@Code` order.

| Sort Key | Element | Notes |
|---:|---|---|
| 1 | `Session/@SessionCode` | Sessions ordered by code. |
| 2 | `Unit/@StartDate` | Within a session; used even when `HideStartDate="Y"`. |
| 3 | `Unit/@Order` | Tiebreaker for same `StartDate`. |
| 4 | `Unit/@Code` | Final tiebreaker; also the order for units without `StartDate`. |

## XSD Validation

No XSD validation was performed for this section: the attribute table and message structure are taken directly from
the printed dictionary, and no XSD was supplied alongside this extraction task. The example above is structural and
intended for modelling rather than as a validated payload.

## Modeling Notes

- Treat `DT_SCHEDULE` as the discipline-scoped reset and `DT_SCHEDULE_UPDATE` as a delta keyed by `Unit/@Code` (or
  session). On any new bulk, drop previously accumulated updates.
- Preserve `HideStartDate` separately from `StartDate`. Persist the time for ordering but defer to `StartText` for
  display when hidden.
- Keep `Order` as a stable display tiebreaker. For VBV, match number is the recommended population strategy when
  several matches share a start time.
- `ActualStartDate` and `ActualEndDate` are write-only outcomes of the unit lifecycle; never overwrite `StartDate` or
  `EndDate` with actuals. Use `RESCHEDULED` to mutate the scheduled times.
- VBV units are `HTEAM`. The schema permits `Composition` here, but the dictionary explicitly excludes it for
  `HTEAM`; do not emit athlete lists in `DT_SCHEDULE`.
- `PreviousUnit` is a bracket-progression placeholder. Drop it as soon as the real competitor populates `Competitor/@Code`.
- `Competitor/@Code` may legitimately be `TBD` or `NOCOMP` (from `SC@CompetitorPlace`). Schema validators should accept
  these alongside numeric IDs.
- Session-level `SessionStatus` only ever transitions to `CANCELLED` in this message; running and finished states live
  on `Unit/@ScheduleStatus`.

## Code Appendix: Values Directly Visible in Pages 8-16

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@COMPETITION_CODE` | `OdfBody/@CompetitionCode` | No concrete values printed in pages 8-16. |
| `CC@DISCIPLINE` | `OdfBody/@DocumentCode` | No concrete values printed in pages 8-16. |
| `SCGEN@Source` | `OdfBody/@Source` | No concrete values printed in pages 8-16. |
| `CC@LANGUAGE` | `SessionName/@Language`, `StartText/@Language`, `ItemName/@Language`, `ItemDescription/@Language` | `ENG` visible in the printed Athletics sample. |
| `CC@VENUE` | `Session/@Venue`, `Session/@VenueName`, `Unit/@Venue`, `VenueDescription/@VenueName` | `STA` and `Olympic Stadium` visible in the printed Athletics sample; `TBD` allowed by the text when venue is unknown. |
| `CC@LOCATION` | `Unit/@Location`, `VenueDescription/@LocationName` | No concrete values printed in pages 8-16; `TBD` allowed by the text. |
| `CC@SHEDULESTATUS` | `Session/@SessionStatus` | `CANCELLED` mentioned in description (only value used at session level). PDF spells the code page `SHEDULESTATUS` here. |
| `CC@SCHEDULESTATUS` | `Unit/@ScheduleStatus` | `UNSCHEDULED`, `SCHEDULED` (implied), `CANCELLED`, `POSTPONED`, `RESCHEDULED` referenced in description. |
| `CC@SESSION_TYPE` | `Session/@SessionType` | No concrete values printed in pages 8-16. |
| `CC@EVENT_UNIT` | `Unit/@Code`, `ItemName/@Value`, `PreviousUnit/@Unit` | No concrete values printed in pages 8-16. |
| `CC@PHASE_TYPE` | `Unit/@PhaseType` | No concrete values printed in pages 8-16. |
| `SCGEN@UnitMedalType` | `Unit/@Medal` | No concrete values printed in pages 8-16. |
| `CC@ORGANISATION` | `Competitor/@Organisation` | No concrete values printed in pages 8-16. |
| `SC@StartText` | `Unit/StartText/@Value` | Free-text examples printed: `Not before 17:00`, `Not before hh:mm`, `Follows`, `SUN 29 - 2nd match on CC`. |
| `SC@CompetitorPlace` | `PreviousUnit/@Value`, `Competitor/@Code` | `TBD`, `NOCOMP` referenced in description. |
| `SC@WLT` | `PreviousUnit/@WLT` | `W`, `L` referenced in description. |
| `DocumentType` literals | `OdfBody/@DocumentType` | `DT_SCHEDULE`, `DT_SCHEDULE_UPDATE`. |
| `DocumentSubtype` literals | `OdfBody/@DocumentSubtype` | `SYNC` referenced for re-synchronisation. |
| `ExtendedInfo` (`CS` family) | `Competition/ExtendedInfos/ExtendedInfo` | `Type=CS, Code=VERSION` and `Type=CS, Code=STATUS` referenced as pre-Games schedule-application attribution. The structure tree on pages 10-12 does not list an `ExtendedInfos` branch under `Competition`; this is a documented mismatch between text and structure. |

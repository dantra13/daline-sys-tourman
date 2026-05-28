# ODF JUD Data Dictionary: DT_SCHEDULE / DT_SCHEDULE_UPDATE, Pages 8-17

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_JUD_Data_Dictionary.pdf`, pages 8-17.

Source version: `SCOG/SYOG-2026-JUD-1.3 SFR`, dated 13 May 2026.

This note restructures the Judo `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` section into a practical reference for the
discipline-level competition timetable. It covers the bulk vs update semantics, session and unit structure, hidden
start-time handling, head-to-head start lists, individual athlete composition, and message-level sorting.

## 2.3.1 Competition schedule / Competition schedule update

`DT_SCHEDULE` is a bulk discipline-scoped message containing the complete schedule for all event units required to run
the competition. `DT_SCHEDULE_UPDATE` is an incremental update message carrying only modified units or sessions.

Key rules from the Judo dictionary:

- Bulk arrival resets all previous schedule information for Judo; previous `DT_SCHEDULE_UPDATE` messages should be
  discarded.
- Event units with the common-code `schedule` flag `Y` or `S` are included regardless of status. Units with no status
  must be sent as `UNSCHEDULED` when the schedule flag is `Y` or `S`.
- Unofficial training and press conferences are excluded.
- Judo's overview notes that detailed schedule is not known until the draw, one day before competition. Temporary units
  are scheduled with phase `TMRY` and removed when the final schedule is published.
- `StartList` is sent for H2H units when at least one competitor is known. Judo individual contests use `HATH`, so
  athlete `Composition` can be included.
- `Composition/Athlete` is not sent when `Competitor/@Code="TBD"`.

When start times are not known, use `HideStartDate="Y"` and `Order` to preserve display order. If a `StartText` value
like `Not before hh:mm` is used, the `StartDate` should carry the same `hh:mm`; the value remains sortable even though
it is hidden.

## Header Values

| Attribute         | Value                                | Meaning                                                                                                                    |
|-------------------|--------------------------------------|----------------------------------------------------------------------------------------------------------------------------|
| `CompetitionCode` | `CC@COMPETITION_CODE`                | Competition ID.                                                                                                            |
| `DocumentCode`    | `CC@DISCIPLINE`                      | Full RSC at the discipline level.                                                                                          |
| `DocumentSubcode` | N/A                                  | Not used.                                                                                                                  |
| `DocumentType`    | `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Bulk schedule or update message.                                                                                           |
| `DocumentSubtype` | `SYNC`                               | Used on `DT_SCHEDULE` for ODF-client re-synchronisation after control is transferred to OVR. Not applicable for `_UPDATE`. |
| `Version`         | Positive integer                     | Ascending version number for the message content.                                                                          |
| `ResultStatus`    | N/A                                  | Not used.                                                                                                                  |
| `FeedFlag`        | `P`, `T`                             | `P` production, `T` test.                                                                                                  |
| `Date`            | Date                                 | ODF header generation date.                                                                                                |
| `Time`            | Time                                 | ODF header generation time.                                                                                                |
| `LogicalDate`     | Date                                 | ODF logical date.                                                                                                          |
| `Source`          | `SCGEN@Source`                       | System that generated the message.                                                                                         |

## Trigger and Frequency

| Message                    | When to send                                                                                                                         |
|----------------------------|--------------------------------------------------------------------------------------------------------------------------------------|
| `DT_SCHEDULE`              | Bulk message when available before the Games, repeated until a cut-off date. It must not be sent after transfer of control to OVR.   |
| `DT_SCHEDULE_UPDATE`       | Any time there is a schedule modification to a previously sent bulk or update message, including addition of H2H start-list details. |
| Start-list updates for H2H | Send immediately when officially known, typically after the preceding unit becomes `OFFICIAL`.                                       |

Text-description changes do not trigger a resend; the new descriptions are used in future messages.

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
            |   +-- text
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
                    +-- Competitor (0,1)
                        +-- @Code
                        +-- @Type
                        +-- @Organisation
                        +-- Composition (0,1)
                            +-- Athlete (1,N)
                                +-- @Code
                                +-- @Order
                                +-- Description (1,1)
                                    +-- @GivenName
                                    +-- @FamilyName
                                    +-- @Gender
                                    +-- @Organisation
                                    +-- @BirthDate
                                    +-- @IFId
```

## Message Values

### `Competition`

| Attribute | M/O | Value   | Meaning                                                    |
|-----------|-----|---------|------------------------------------------------------------|
| `Gen`     | O   | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport`   | O   | `S(20)` | Sport Data Dictionary version applicable to the message.   |
| `Codes`   | O   | `S(20)` | Code-set version applicable to the message.                |

### `Competition / Session`

| Attribute       | M/O | Value                          | Meaning                                                                                  |
|-----------------|-----|--------------------------------|------------------------------------------------------------------------------------------|
| `SessionCode`   | M   | `S(10)`                        | Session code, usually `DDD00` where `DDD` is the discipline and `00` the session number. |
| `StartDate`     | M   | DateTime or Date               | Session start. Date-only is allowed only in early schedule-by-day stages.                |
| `HideStartDate` | O   | `Y`                            | Hide scheduled start time from display; still use it for sorting.                        |
| `EndDate`       | M   | DateTime or Date               | Session end. Date-only is allowed only in early schedule-by-day stages.                  |
| `HideEndDate`   | O   | `Y`                            | Hide scheduled end time from display; still use it for sorting.                          |
| `Leadin`        | O   | `m:ss`                         | Time from session start to first scheduled unit.                                         |
| `Venue`         | M   | `CC@VENUE` ID                  | Venue where the session takes place.                                                     |
| `VenueName`     | M   | `CC@VENUE` English description | Venue name.                                                                              |
| `SessionStatus` | O   | `CC@SHEDULESTATUS` ID          | Only `CANCELLED` is used when applicable; other sessions are assumed scheduled.          |
| `SessionType`   | O   | `CC@SESSION_TYPE` ID           | Session type.                                                                            |
| `Medal`         | O   | Positive integer               | Number of gold medals planned to be determined in the session.                           |
| `FOP`           | O   | Positive integer               | Number of fields of play planned for the session, pre-Games only.                        |

Note: the PDF spells the session-level code entity as `CC@SHEDULESTATUS`, while unit status uses
`CC@SCHEDULESTATUS`. Preserve the source spelling but treat it as likely referring to the schedule-status code domain.

### `Competition / Session / SessionName`

| Attribute  | M/O | Value            | Meaning                       |
|------------|-----|------------------|-------------------------------|
| `Language` | M   | `CC@LANGUAGE` ID | Language of the session name. |
| `Value`    | M   | `S(40)`          | Session name.                 |

### `Competition / Unit`

| Attribute         | M/O | Value                  | Meaning                                                                                                   |
|-------------------|-----|------------------------|-----------------------------------------------------------------------------------------------------------|
| `Code`            | M   | `CC@EVENT_UNIT`        | Full RSC for the unit.                                                                                    |
| `PhaseType`       | M   | `CC@PHASE_TYPE` ID     | Phase type for the unit; temporary Judo units use `TMRY` before final draw publication.                   |
| `UnitNum`         | O   | `S(15)`                | Contest/bout number or similar.                                                                           |
| `HideUnitNum`     | O   | `Y`                    | Hide the unit number when details are not confirmed for display.                                          |
| `ScheduleStatus`  | M   | `CC@SCHEDULESTATUS` ID | Unit status.                                                                                              |
| `StartDate`       | O   | DateTime               | Scheduled start time. May be omitted for `UNSCHEDULED`; expected for other statuses to preserve ordering. |
| `HideStartDate`   | O   | `Y`                    | Hide the scheduled start time. Fill `StartDate` with a sortable placeholder and use `Order`.              |
| `EndDate`         | O   | DateTime               | Scheduled end time. Not required for `UNSCHEDULED` or `CANCELLED`.                                        |
| `HideEndDate`     | O   | `Y`                    | Hide scheduled end time.                                                                                  |
| `ActualStartDate` | O   | DateTime               | Expected once the unit starts.                                                                            |
| `ActualEndDate`   | O   | DateTime               | Expected once the unit finishes.                                                                          |
| `Order`           | O   | Positive integer       | Display order, especially when `HideStartDate="Y"` or multiple units share a start time.                  |
| `Medal`           | O   | `SCGEN@UnitMedalType`  | Indicator of medal awarded for the unit.                                                                  |
| `Venue`           | O   | `CC@VENUE` ID          | Unit venue; mandatory unless `UNSCHEDULED`; `TBD` allowed.                                                |
| `Location`        | O   | `CC@LOCATION` ID       | Unit location; mandatory unless `UNSCHEDULED`; `TBD` or discipline-generic code allowed.                  |
| `SessionCode`     | O   | `S(10)`                | Session containing the unit.                                                                              |

### `Competition / Unit / StartText`

Used only when `HideStartDate="Y"`; English is mandatory in that case.

| Attribute  | M/O | Value                     | Meaning                                                                          |
|------------|-----|---------------------------|----------------------------------------------------------------------------------|
| `Language` | M   | `CC@LANGUAGE` ID          | Language of the displayed text.                                                  |
| `Value`    | M   | `S(20)` or `SC@StartText` | Display text when start time is hidden, such as `Not before 17:00` or `Follows`. |

### `Competition / Unit / ItemName`

| Attribute  | M/O | Value                                     | Meaning                             |
|------------|-----|-------------------------------------------|-------------------------------------|
| `Language` | M   | `CC@LANGUAGE` ID                          | Language of the value.              |
| `Value`    | M   | `CC@EVENT_UNIT` English short description | Unit display name matching the RSC. |

### `Competition / Unit / ItemDescription`

| Attribute  | M/O | Value            | Meaning                                         |
|------------|-----|------------------|-------------------------------------------------|
| `Language` | M   | `CC@LANGUAGE` ID | Language of the text.                           |
| text       | M   | Free text        | Description for non-competition schedule items. |

### `Competition / Unit / VenueDescription`

Mandatory when `Unit/@Venue` is included.

| Attribute      | M/O | Value                             | Meaning                |
|----------------|-----|-----------------------------------|------------------------|
| `VenueName`    | M   | `CC@VENUE` English description    | Venue display name.    |
| `LocationName` | M   | `CC@LOCATION` English description | Location display name. |

### `Competition / Unit / StartList / Start`

| Attribute    | M/O | Value            | Meaning                                    |
|--------------|-----|------------------|--------------------------------------------|
| `StartOrder` | O   | Positive integer | Competitor's start order.                  |
| `SortOrder`  | M   | Positive integer | Display order for competitors in the unit. |

### `Competition / Unit / StartList / Start / PreviousUnit`

Use this path only when the real competitor is not known.

| Attribute | M/O | Value                | Meaning                                                                                   |
|-----------|-----|----------------------|-------------------------------------------------------------------------------------------|
| `Unit`    | O   | `CC@EVENT_UNIT`      | Prior unit RSC where this competitor comes from, only when progression is 100% confirmed. |
| `Value`   | O   | `SC@CompetitorPlace` | Placeholder such as `TBD` or `NOCOMP`.                                                    |
| `WLT`     | O   | `SC@WLT`             | `W` or `L` from the previous unit, only when confirmed.                                   |

### `Competition / Unit / StartList / Start / Competitor`

| Attribute      | M/O | Value                                              | Meaning                                       |
|----------------|-----|----------------------------------------------------|-----------------------------------------------|
| `Code`         | M   | `S(20)` with no leading zeroes, `TBD`, or `NOCOMP` | Competitor ID or placeholder.                 |
| `Type`         | M   | `A`                                                | Athlete competitor for Judo individual units. |
| `Organisation` | O   | `CC@Organisation` ID                               | Competitor organisation when known.           |

### `Competition / Unit / StartList / Start / Competitor / Composition / Athlete`

Only send for individual athletes. Do not send if `Competitor/@Code="TBD"`.

| Attribute | M/O | Value                          | Meaning                                         |
|-----------|-----|--------------------------------|-------------------------------------------------|
| `Code`    | M   | `S(20)` with no leading zeroes | Athlete ID.                                     |
| `Order`   | M   | Positive integer               | Athlete order; `1` when `Competitor/@Type="A"`. |

### `Competition / Unit / StartList / Start / Competitor / Composition / Athlete / Description`

| Attribute      | M/O | Value                | Meaning                                       |
|----------------|-----|----------------------|-----------------------------------------------|
| `GivenName`    | O   | `S(25)`              | Given name in WNPA mixed-case format.         |
| `FamilyName`   | M   | `S(25)`              | Family name in WNPA mixed-case format.        |
| `Gender`       | M   | `CC@PersonGender` ID | Participant gender.                           |
| `Organisation` | M   | `CC@Organisation` ID | Organisation ID.                              |
| `BirthDate`    | O   | `YYYY-MM-DD`         | Athlete birth date.                           |
| `IFId`         | O   | `S(16)`              | Athlete IF number for the current discipline. |

## Sample from the Dictionary, Normalized

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-JUD-1.3 SFR" Codes="SYOG-2026">
  <Session SessionCode="JUD01" StartDate="2026-10-31T10:00:00+00:00"
           EndDate="2026-10-31T14:00:00+00:00" Venue="DJK" VenueName="Dakar Judo Arena">
    <SessionName Language="ENG" Value="Judo Session 1"/>
    <Unit Code="JUDM60KG8FNL000100--" PhaseType="FNL" UnitNum="1" ScheduleStatus="SCHEDULED"
          StartDate="2026-10-31T10:00:00+00:00" EndDate="2026-10-31T10:05:00+00:00"
          Order="1" Venue="DJK" Location="MAT1" SessionCode="JUD01">
      <ItemName Language="ENG" Value="Men -60kg Round of 16 Contest 1"/>
      <VenueDescription VenueName="Dakar Judo Arena" LocationName="Mat 1"/>
      <StartList>
        <Start StartOrder="1" SortOrder="1">
          <Competitor Code="1008743" Type="A" Organisation="SUI">
            <Composition>
              <Athlete Code="1008743" Order="1">
                <Description GivenName="Jane" FamilyName="Smits" Gender="W" Organisation="SUI"
                             BirthDate="2009-12-15"/>
              </Athlete>
            </Composition>
          </Competitor>
        </Start>
        <Start SortOrder="2">
          <PreviousUnit Value="TBD"/>
          <Competitor Code="TBD" Type="A"/>
        </Start>
      </StartList>
    </Unit>
  </Session>
</Competition>
```

## Message Sort

Sort first by `Session/@SessionCode`.

Within the message, units are sorted by `Unit/@StartDate`, then `Unit/@Order`, then `Unit/@Code`. Units without
`Unit/@StartDate` (for example `UNSCHEDULED`) are listed at the end in `Unit/@Code` order.

## Modeling Notes

- Treat `DT_SCHEDULE` as a full discipline schedule snapshot and `DT_SCHEDULE_UPDATE` as a patch keyed by
  `Unit/@Code`.
- Judo's pre-draw temporary units (`PhaseType="TMRY"`) are expected to disappear once the final draw schedule is
  published. Model them as provisional schedule units, not as stable event-unit identities.
- Preserve `HideStartDate` separately from `StartDate`; hidden times are still ordering keys.
- For Judo HATH units, the start list can include athlete `Composition`. Do not expect composition when the competitor
  is `TBD`.
- `PreviousUnit` placeholders should be replaced by real competitor data when known; do not keep both as separate
  active starts for the same `SortOrder`.

## Code Appendix: Values Directly Visible in Pages 8-17

The section references common and sport code domains. This appendix records values directly visible in the Judo
`DT_SCHEDULE` pages and does not embed master-data tables.

| Code Entity                              | Section Usage                                          | Values Visible in Section                                                      |
|------------------------------------------|--------------------------------------------------------|--------------------------------------------------------------------------------|
| `CC@COMPETITION_CODE`                    | `OdfBody/@CompetitionCode`                             | No concrete values printed in pages 8-17.                                      |
| `CC@DISCIPLINE`                          | `OdfBody/@DocumentCode`                                | Judo discipline context; no concrete RSC printed in the extracted section.     |
| `CC@EVENT_UNIT`                          | `Unit/@Code`, `PreviousUnit/@Unit`, `ItemName/@Value`  | No concrete Judo event-unit values printed in pages 8-17.                      |
| `CC@PHASE_TYPE`                          | `Unit/@PhaseType`                                      | The overview names `TMRY` for temporary pre-draw units.                        |
| `CC@SCHEDULESTATUS` / `CC@SHEDULESTATUS` | `Unit/@ScheduleStatus`, `Session/@SessionStatus`       | `UNSCHEDULED`, `CANCELLED`, `POSTPONED`, `RESCHEDULED` are mentioned in prose. |
| `SC@CompetitorPlace`                     | `PreviousUnit/@Value`, `Competitor/@Code` placeholders | `TBD`, `NOCOMP`.                                                               |
| `SC@WLT`                                 | `PreviousUnit/@WLT`                                    | `W`, `L`.                                                                      |
| `CC@Organisation` / `CC@ORGANISATION`    | Competitor and athlete organisation                    | No concrete Judo organisation values printed in pages 8-17.                    |
| `CC@PersonGender`                        | Athlete gender                                         | No concrete Judo gender values printed in pages 8-17.                          |

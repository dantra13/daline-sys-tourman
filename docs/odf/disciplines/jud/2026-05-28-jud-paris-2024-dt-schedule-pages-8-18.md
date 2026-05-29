# ODF JUD Data Dictionary: Paris 2024 DT_SCHEDULE / DT_SCHEDULE_UPDATE, Pages 8-18

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 8-18.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note restructures the Paris 2024 Judo `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` section into a practical reference for
discipline schedule ingestion. The team-event rule is important: team matches are scheduled, but individual contests
inside a team match are not scheduled as separate units.

## 2.3.1 Competition schedule / Competition schedule update

`DT_SCHEDULE` is a complete discipline schedule snapshot. `DT_SCHEDULE_UPDATE` carries only modified schedule data.

Key rules:

- A `DT_SCHEDULE` arrival resets previous schedule information for the discipline and invalidates previous updates.
- Units whose common-code `schedule` flag is `Y` or `S` are included regardless of current status; missing status is
  sent as `UNSCHEDULED`.
- `StartList` is included only for `HATH`, `HCOUP`, or `HTEAM` units when at least one competitor is known.
- `Composition/Athlete` is included only for `HATH` or `HCOUP`; for teams, use the team competitor identity and team
  name, not team-member composition in the schedule start list.
- Hidden start times use `HideStartDate="Y"` with sortable `StartDate` plus `Order`; display should use `StartText`
  where provided.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Discipline` | Full RSC at discipline level. |
| `DocumentType` | `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Bulk schedule or schedule update. |
| `DocumentSubtype` | `S(20)` | `SYNC` only for re-synchronisation after control transfer; never included in update messages. |
| `Version` | `1..V` | Ascending content version. |
| `FeedFlag` | `P`, `T` | Production or test. |
| `Date` / `Time` / `LogicalDate` | Date/time | Header generation and logical dates. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Message | Trigger |
|---|---|
| `DT_SCHEDULE` | Sent before the Games as a bulk message, repeatedly until a cut-off; must not be sent after transfer of control to OVR. |
| `DT_SCHEDULE_UPDATE` | Sent for any schedule modification to a previous bulk or update message, including addition of H2H start-list details. |
| H2H start list detail | Sent immediately when officially known, usually after the preceding unit becomes official. |

Text-description changes do not trigger a resend; future messages use the new text.

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
        +-- @EndDate
        +-- @Leadin
        +-- @Venue
        +-- @VenueName
        +-- @ModificationIndicator
        +-- @SessionStatus
        +-- @SessionType
        +-- @Medal
        +-- @FOP
        +-- SessionName (1,N)
        +-- Unit (0,N)
            +-- @Code
            +-- @PhaseType
            +-- @UnitNum
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
            +-- @MediaAccess
            +-- @SessionCode
            +-- @ModificationIndicator
            +-- StartText (0,N)
            +-- ItemName (1,N)
            +-- ItemDescription (0,N)
            +-- VenueDescription (0,1)
            +-- StartList (0,1)
                +-- Start (1,N)
                    +-- @StartOrder
                    +-- @SortOrder
                    +-- @PreviousWLT
                    +-- @PreviousUnit
                    +-- Competitor (1,1)
                        +-- @Code
                        +-- @Type
                        +-- @Organisation
                        +-- Description (0,1)
                        +-- Composition (0,1)
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General dictionary version. |
| `Sport` | O | `S(20)` | Sport dictionary version. |
| `Codes` | O | `S(20)` | Codes version. |

### `Competition / Session`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `SessionCode` | M | `S(10)` | Session code. |
| `StartDate` / `EndDate` | M | DateTime | Session start and end. |
| `Leadin` | O | `m:ss` | Time from session start to first scheduled unit. |
| `Venue` | M | `CC@VenueCode` | Venue code. |
| `VenueName` | M | `S(25)` | Venue English name. |
| `ModificationIndicator` | O | `N`, `U` | Mandatory in update messages. |
| `SessionStatus` | O | `CC@ScheduleStatus` | Only `CANCELLED` is used at session level. |
| `SessionType` | O | `CC@SessionType` | Session type. |
| `Medal` | O | `#0` | Gold medals planned in the session. |
| `FOP` | O | `#0` | Planned fields of play; pre-Games only. |

### `Competition / Unit`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `CC@Unit` | Full unit RSC. |
| `PhaseType` | M | `CC@PhaseType` | Phase type. |
| `UnitNum` | O | `S(15)` | Contest/bout/match number. |
| `ScheduleStatus` | M | `CC@ScheduleStatus` | Unit status. |
| `StartDate` | O | DateTime | Scheduled start; may be omitted for `UNSCHEDULED`. |
| `HideStartDate` | O | `Y` | Hide scheduled start while still using it for sorting. |
| `EndDate` / `HideEndDate` | O | DateTime / `Y` | Scheduled end and display hide flag. |
| `ActualStartDate` / `ActualEndDate` | O | DateTime | Actual unit timing. |
| `Order` | O | `###0` | Display order, especially when start time is hidden or duplicated. |
| `Medal` | O | `SC@UnitMedalType` | Medal indicator. |
| `Venue` / `Location` | O | `CC@VenueCode` / `CC@Location` | Mandatory unless unscheduled; `TBD` can be used. |
| `MediaAccess` | O | `OPE`, `CLO` | Non-competition media access. |
| `SessionCode` | O | `S(10)` | Containing session. |
| `ModificationIndicator` | O | `N`, `U` | Mandatory in update messages only. |

### `Unit / StartList / Start`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `StartOrder` | O | Numeric | Competitor start order. |
| `SortOrder` | M | Numeric | Display order. |
| `PreviousWLT` | O | `W`, `L` | Winner/loser from a previous unit, only when fully confirmed. |
| `PreviousUnit` | O | `S(34)` | Previous unit RSC, removed when real competitors are known. |

### `Start / Competitor`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | ID, `TBD`, `NOCOMP` | Competitor ID or placeholder. |
| `Type` | M | `T`, `A` | Team or athlete. |
| `Organisation` | O | `CC@Organisation` | Organisation when known. |

`Competitor/Description` carries `TeamName` and optional `IFId` for teams. `Competitor/Composition/Athlete` is only
sent for individual competitors, not teams, and is omitted when the competitor is `TBD`.

## Sample, Normalized

```xml
<Competition Gen="SOG-2024-GEN" Sport="SOG-2024-JUD-3.3 APP" Codes="SOG-2024">
  <Session SessionCode="JUD01" StartDate="2024-07-27T10:00:00+02:00"
           EndDate="2024-07-27T14:00:00+02:00" Venue="CMA" VenueName="Champ-de-Mars Arena">
    <SessionName Language="ENG" Value="Judo Session 1"/>
    <Unit Code="JUDXTEAM6-------------8FNL000100--" PhaseType="FNL" UnitNum="1"
          ScheduleStatus="SCHEDULED" StartDate="2024-08-03T08:00:00+02:00"
          Order="1" Venue="CMA" Location="MAT1" SessionCode="JUD01">
      <ItemName Language="ENG" Value="Mixed Team Round of 16 Match 1"/>
      <StartList>
        <Start SortOrder="1">
          <Competitor Code="JUDXTEAM6--FRA01" Type="T" Organisation="FRA">
            <Description TeamName="France"/>
          </Competitor>
        </Start>
        <Start SortOrder="2">
          <Competitor Code="TBD" Type="T"/>
        </Start>
      </StartList>
    </Unit>
  </Session>
</Competition>
```

## Message Sort

Sort by `Session/@SessionCode`, then units by `Unit/@StartDate`, `Unit/@Order`, and `Unit/@Code`. Units without
`Unit/@StartDate`, such as `UNSCHEDULED`, are listed at the end by `Unit/@Code`.

## Modeling Notes

- Schedule team matches, not team sub-contests. Sub-contests are represented in `DT_RESULT`, not as scheduled units.
- Treat `DT_SCHEDULE` as a full replacement and `DT_SCHEDULE_UPDATE` as a patch keyed by `Unit/@Code`.
- Keep `PreviousUnit` and `PreviousWLT` as draw/progression placeholders that disappear when competitors are known.
- Do not expect team-member composition in team schedule start lists; team members come from `DT_PARTIC_TEAMS` and
  `DT_RESULT`.

## Code Appendix: Values Visible in Pages 8-18

| Code Entity | Section Usage | Visible Values |
|---|---|---|
| `DocumentType` | Header | `DT_SCHEDULE`, `DT_SCHEDULE_UPDATE` |
| `DocumentSubtype` | Header | `SYNC` |
| `ModificationIndicator` | Session/unit updates | `N`, `U` |
| `ScheduleStatus` | Session/unit status | `UNSCHEDULED`, `CANCELLED`, `POSTPONED`, `RESCHEDULED` appear in prose. |
| `CompetitorPlace` | Competitor placeholders | `TBD`, `NOCOMP` |
| `Competitor/@Type` | Start-list competitor type | `A`, `T` |
| `MediaAccess` | Non-competition units | `OPE`, `CLO` |

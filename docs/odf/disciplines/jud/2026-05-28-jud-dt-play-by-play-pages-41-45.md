# ODF JUD Data Dictionary: DT_PLAY_BY_PLAY, Pages 41-45

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_JUD_Data_Dictionary.pdf`, pages 41-45.

Source version: `SCOG/SYOG-2026-JUD-1.3 SFR`, dated 13 May 2026.

This note restructures the Judo `DT_PLAY_BY_PLAY` section into a practical reference for contest action data. It covers
action identity, ordering, action type classification, timing within the contest, optional technique result, and
competitor/athlete payloads attached to an action.

## 2.3.6 Play by Play

`DT_PLAY_BY_PLAY` contains official raw data from the results provider for each action. For Judo, it sends the generic
action list used to provide contest actions and incidents in one event unit.

## Header Values

| Attribute         | Value                 | Meaning                                                                        |
|-------------------|-----------------------|--------------------------------------------------------------------------------|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID.                                                                |
| `DocumentCode`    | `CC@EVENT_UNIT`       | Event Unit RSC.                                                                |
| `DocumentSubcode` | N/A                   | Not used.                                                                      |
| `DocumentType`    | `DT_PLAY_BY_PLAY`     | Play-by-play message.                                                          |
| `DocumentSubtype` | `ACTION`              | Always the action subtype for this section.                                    |
| `Version`         | Positive integer      | Ascending version number for the message content.                              |
| `ResultStatus`    | `CC@ResultStatus`     | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag`        | `P`, `T`              | `P` production, `T` test.                                                      |
| `Date`            | Date                  | ODF header generation date.                                                    |
| `Time`            | Time                  | ODF header generation time.                                                    |
| `LogicalDate`     | Date                  | ODF logical date.                                                              |
| `Source`          | `SCGEN@Source`        | System that generated the message.                                             |

## Trigger and Frequency

| `ResultStatus`            | When to send                                             |
|---------------------------|----------------------------------------------------------|
| `START_LIST`              | Only if there is an action before the start of the unit. |
| `LIVE`                    | When the contest starts.                                 |
| `LIVE`                    | After every action.                                      |
| `INTERMEDIATE`            | For interruption.                                        |
| `UNOFFICIAL` / `OFFICIAL` | After the contest.                                       |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- SportDescription (0,1)
    |   |   +-- @DisciplineName
    |   |   +-- @EventName
    |   |   +-- @SubEventName
    |   |   +-- @Gender
    |   +-- VenueDescription (0,1)
    |       +-- @Venue
    |       +-- @VenueName
    |       +-- @Location
    |       +-- @LocationName
    +-- Actions (0,1)
        +-- @Home
        +-- @Away
        +-- Action (1,N)
            +-- @Id
            +-- @Period
            +-- @Order
            +-- @Action
            +-- @ActionAdd
            +-- @When
            +-- @Result
            +-- Competitor (0,N)
                +-- @Code
                +-- @Type
                +-- @Order
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

### `Competition / ExtendedInfos / SportDescription`

| Attribute        | M/O | Value                                     | Meaning                         |
|------------------|-----|-------------------------------------------|---------------------------------|
| `DisciplineName` | M   | `CC@DISCIPLINE` English description       | Discipline display name.        |
| `EventName`      | M   | `CC@EVENT` English description            | Event display name.             |
| `SubEventName`   | M   | `CC@EVENT_UNIT` English short description | Event-unit display name.        |
| `Gender`         | M   | `CC@DISCIPLINE_GENDER`                    | Gender code for the event unit. |

### `Competition / ExtendedInfos / VenueDescription`

| Attribute      | M/O | Value                             | Meaning        |
|----------------|-----|-----------------------------------|----------------|
| `Venue`        | M   | `CC@VENUE` ID                     | Venue code.    |
| `VenueName`    | M   | `CC@VENUE` English description    | Venue name.    |
| `Location`     | M   | `CC@LOCATION` ID                  | Location code. |
| `LocationName` | M   | `CC@LOCATION` English description | Location name. |

### `Competition / Actions`

| Attribute | M/O | Value                          | Meaning                                                                                                      |
|-----------|-----|--------------------------------|--------------------------------------------------------------------------------------------------------------|
| `Home`    | O   | `S(20)` with no leading zeroes | Home competitor ID. For Judo, this should be interpreted together with white/blue ordering from `DT_RESULT`. |
| `Away`    | O   | `S(20)` with no leading zeroes | Away competitor ID.                                                                                          |

### `Competition / Actions / Action`

| Attribute   | M/O | Value                                              | Meaning                                                                                                                             |
|-------------|-----|----------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| `Id`        | M   | `S(36)`                                            | Unique action ID within the message. Once assigned for an action in a unit, it must not change in later messages for the same unit. |
| `Period`    | M   | `SC@Period`                                        | Period of the action within the match.                                                                                              |
| `Order`     | M   | Positive integer                                   | Unique sequential action number from `1` to `n`; used for sorting.                                                                  |
| `Action`    | M   | `SC@PointsType` or `SC@PenaltyType` or `SC@Action` | Points, penalty, or action code. Send one code. Required at the start and end of the contest.                                       |
| `ActionAdd` | M   | `ACTION`, `POINTSTYPE`, or `PENALTYTYPE`           | Classification that identifies whether `Action` is a general action, points type, or penalty type.                                  |
| `When`      | O   | `mm:ss`                                            | Action time, ascending from the beginning of one period. Example: `2:05`.                                                           |
| `Result`    | O   | `SC@Technique` code                                | Technique code associated with the action.                                                                                          |

### `Competition / Actions / Action / Competitor`

Use when the action is related to a competitor.

| Attribute      | M/O | Value                          | Meaning                                                                        |
|----------------|-----|--------------------------------|--------------------------------------------------------------------------------|
| `Code`         | M   | `S(20)` with no leading zeroes | Competitor ID.                                                                 |
| `Type`         | M   | `A`                            | Athlete competitor.                                                            |
| `Order`        | O   | Positive integer               | Order of competitors for the action, when more than one competitor is present. |
| `Organisation` | M   | `CC@ORGANISATION` ID           | Competitor organisation.                                                       |

### `Competition / Actions / Action / Competitor / Composition / Athlete`

| Attribute | M/O | Value                          | Meaning                                                            |
|-----------|-----|--------------------------------|--------------------------------------------------------------------|
| `Code`    | M   | `S(20)` with no leading zeroes | Athlete ID related to the action.                                  |
| `Order`   | O   | Positive integer               | Athlete order when more than one athlete is related to the action. |

### `Competition / Actions / Action / Competitor / Composition / Athlete / Description`

| Attribute      | M/O | Value                 | Meaning                                |
|----------------|-----|-----------------------|----------------------------------------|
| `GivenName`    | O   | `S(25)`               | Given name in WNPA mixed-case format.  |
| `FamilyName`   | M   | `S(25)`               | Family name in WNPA mixed-case format. |
| `Gender`       | M   | `CC@PERSON_GENDER` ID | Athlete gender.                        |
| `Organisation` | M   | `CC@ORGANISATION` ID  | Athlete organisation.                  |
| `BirthDate`    | O   | Date                  | Birth date, included when available.   |
| `IFId`         | O   | `S(16)`               | International Federation ID.           |

## Sample from the Dictionary, Normalized

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-JUD-1.3 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <SportDescription DisciplineName="Judo" EventName="Women -52kg" SubEventName="Contest 3" Gender="W"/>
    <VenueDescription Venue="DJK" VenueName="Dakar Judo Arena" Location="MAT1" LocationName="Mat 1"/>
  </ExtendedInfos>
  <Actions Home="1008743" Away="1008750">
    <Action Id="123456" Period="N" Order="3" Action="S" ActionAdd="PENALTYTYPE" When="2:12">
      <Competitor Code="1008743" Type="A" Organisation="SUI" Order="1">
        <Composition>
          <Athlete Code="1008743" Order="1">
            <Description GivenName="Jane" FamilyName="Smits" Gender="W" Organisation="SUI"
                         BirthDate="2009-12-15"/>
          </Athlete>
        </Composition>
      </Competitor>
    </Action>
    <Action Id="123457" Period="N" Order="4" Action="WAZ" ActionAdd="POINTSTYPE" When="3:05" Result="P29">
      <Competitor Code="1008743" Type="A" Organisation="SUI" Order="1"/>
    </Action>
  </Actions>
</Competition>
```

## Message Sort

Sort by `Actions/Action/@Order`.

## Modeling Notes

- Treat `Action/@Order` as the canonical replay order. `Action/@Id` is stable identity, not a sort key.
- `Action/@ActionAdd` is required and disambiguates the code domain of `Action/@Action`; do not parse action codes
  without considering this classifier.
- `Action/@When` is period-relative time, not absolute clock time. It ascends from `0:00` within the period.
- `Action/@Result` carries a technique code. The winning technique for the contest also appears in `DT_RESULT`
  `ExtendedInfo/TECH_CODE`; keep both paths because play-by-play is action-level and result is unit-level.
- Competitor and athlete details are optional per action. Ingestion should link by competitor/athlete ID when present
  and not require full descriptions on every action.

## Code Appendix: Values Directly Visible in Pages 41-45

The PDF links a common result-status table and the Judo technique table. The linked `odf.olympictech.org` pages timed
out during extraction, so this appendix records only values visible in the `DT_PLAY_BY_PLAY` pages.

| Code Entity            | Section Usage                                   | Values Visible in Section                                                                                    | Linked Source in PDF                                                                  |
|------------------------|-------------------------------------------------|--------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|
| `CC@ResultStatus`      | `OdfBody/@ResultStatus`                         | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`                                | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm`             |
| `SC@Period`            | `Action/@Period`                                | Sample uses `N`.                                                                                             | No direct link extracted from pages 41-45.                                            |
| `SC@PointsType`        | `Action/@Action` when `ActionAdd="POINTSTYPE"`  | No concrete points values printed in pages 41-45.                                                            |
| `SC@PenaltyType`       | `Action/@Action` when `ActionAdd="PENALTYTYPE"` | Sample uses `S`.                                                                                             |
| `SC@Action`            | `Action/@Action` when `ActionAdd="ACTION"`      | Start/end action codes are required by prose, but no concrete action-code values are printed in pages 41-45. |
| `ActionAdd` classifier | `Action/@ActionAdd`                             | `ACTION`, `POINTSTYPE`, `PENALTYTYPE`.                                                                       |
| `SC@Technique`         | `Action/@Result`                                | No concrete technique values in the play-by-play sample; `DT_RESULT` pages show `P29`.                       | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Technique_SOG_JUD.htm` |

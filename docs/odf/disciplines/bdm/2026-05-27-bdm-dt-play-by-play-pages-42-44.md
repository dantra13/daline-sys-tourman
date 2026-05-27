# ODF BDM Data Dictionary: DT_PLAY_BY_PLAY, Pages 42-44

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BDM_Data_Dictionary.pdf`, pages 42-44.

Source version: `SYOG-2026-BDM-1.1 SFR`, dated 13 May 2026.

This note restructures the Badminton `DT_PLAY_BY_PLAY` section into a practical reference for point-by-point action
data in one match unit.

## 2.3.6 Play by Play

`DT_PLAY_BY_PLAY` contains official raw data from the results provider for each action. In Badminton, the section
defines a generic action list used to provide all actions in a unit.

The message is sent after every point in all units.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@EVENT_UNIT` | Full unit RSC. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_PLAY_BY_PLAY` | Play-by-play message. |
| `DocumentSubtype` | `ACTION` | Always send `ACTION`. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` | `START_LIST` is only used if there are actions before the start; `LIVE` during the competition; `OFFICIAL` when results are official. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| Trigger | Meaning |
|---|---|
| After every point in all units | Send the action list with the latest point-level action data. |

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
            +-- @ScoreH
            +-- @ScoreA
```

## Competition and Descriptions

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competition/@Gen` | O | `S(20)` | General dictionary version. |
| `Competition/@Sport` | O | `S(20)` | Sport dictionary version. |
| `Competition/@Codes` | O | `S(20)` | Code-set version. |
| `SportDescription/@DisciplineName` | M | `CC@DISCIPLINE` ENG description | Discipline display name. |
| `SportDescription/@EventName` | M | `CC@EVENT` ENG description | Event display name. |
| `SportDescription/@SubEventName` | M | `CC@EVENT_UNIT` ENG short description | Unit display name. |
| `SportDescription/@Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the event unit. |
| `VenueDescription/@Venue` | M | `CC@VENUE` ID | Venue code. |
| `VenueDescription/@VenueName` | M | `CC@VENUE` ENG description | Venue display name. |
| `VenueDescription/@Location` | M | `CC@LOCATION` ID | Location code. |
| `VenueDescription/@LocationName` | M | `CC@LOCATION` ENG description | Location display name. |

## Actions

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Actions/@Home` | M | `S(20)` | Home competitor ID. No leading zeroes. |
| `Actions/@Away` | M | `S(20)` | Away competitor ID. No leading zeroes. |
| `Action/@Id` | M | `S(36)` | Unique action ID within the message. Once assigned for an action in a unit, it must not change in later messages for the same unit. |
| `Action/@Period` | M | `SC@Period` | Period/game of the action within the match. |
| `Action/@Order` | M | Positive integer | Unique sequential action number from `1` to `n`; used for sorting. |
| `Action/@Action` | O | `S(2)` | Server indicator for the next serve. `H` or `A` for home/away. In doubles, append `1` or `2` to indicate player 1 or 2, for example `H1`. |
| `Action/@ScoreH` | O | `#0` | Home game score after the action. Send when there is a score change for either competitor. |
| `Action/@ScoreA` | O | `#0` | Away game score after the action. Send when there is a score change for either competitor. |

## Sample from the Dictionary, Normalized

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-BDM-1.1 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <SportDescription DisciplineName="Badminton" EventName="Women's Singles" SubEventName="Final" Gender="W"/>
    <VenueDescription Venue="MFS" VenueName="Multi-Function Sport Hall" Location="CR1" LocationName="Court 1"/>
  </ExtendedInfos>
  <Actions Home="123456" Away="234567">
    <Action Id="00014433456" Period="G1" Order="3" ScoreH="2" ScoreA="2" Action="H"/>
    <Action Id="00223323457" Period="G1" Order="4" ScoreH="2" ScoreA="3" Action="A"/>
  </Actions>
</Competition>
```

## Message Sort

Sort by `Actions / Action @Order`.

## XSD Validation

The normalized XML example above was validated as a `Competition` fragment wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Treat `Action/@Order` as the canonical order for replaying point actions. `Action/@Id` is identity, not sort order.
- The message is point-triggered, but it carries an action list. Ingestion should replace or reconcile by stable
  `Action/@Id` within the unit rather than assuming each message contains only the latest point.
- `Action/@Action` indicates the next server, not necessarily the player who won the point.
- `ScoreH` and `ScoreA` are game scores after the action. They are optional and should be interpreted only when present.
- In doubles, preserve server suffixes such as `H1`, `H2`, `A1`, and `A2`; they encode the next serving player.

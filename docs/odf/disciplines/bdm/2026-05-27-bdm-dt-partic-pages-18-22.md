# ODF BDM Data Dictionary: DT_PARTIC / DT_PARTIC_UPDATE, Pages 18-22

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BDM_Data_Dictionary.pdf`, pages 18-22.

Source version: `SYOG-2026-BDM-1.1 SFR`, dated 13 May 2026.

This note restructures the Badminton `DT_PARTIC` and `DT_PARTIC_UPDATE` section into a practical reference for
participant master data by discipline.

## 2.3.2 Participants by Discipline

`DT_PARTIC` contains participant information for the Badminton discipline and all its events. Athlete identifiers
referenced by start lists and results must match the participant codes sent here. The message can also include officials,
coaches, guides, technical officials, reserves, and historical athletes.

`DT_PARTIC` is a full bulk message. Each emission resets the previous participant set for the discipline. `DT_PARTIC`
is sent before the Games and can be repeated until data is transferred to the venue results system.

`DT_PARTIC_UPDATE` is an incremental update message used after venue transfer. For each changed participant, the update
sends the complete `Participant` element, including all applicable child elements and attributes. The update key is
`Participant/@Code`.

## Header Values

| Attribute | `DT_PARTIC` | `DT_PARTIC_UPDATE` | Meaning |
|---|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@DISCIPLINE` | `CC@DISCIPLINE` | Discipline RSC, for example Badminton. |
| `DocumentSubcode` | N/A | N/A | Not used. |
| `DocumentType` | `DT_PARTIC` | `DT_PARTIC_UPDATE` | Participants by discipline message. |
| `DocumentSubtype` | `SYNC`, `HISTORICAL`, or N/A | N/A | `SYNC` is a full sync after transfer to OVR; `HISTORICAL` is for historical-result providers and is not for external clients. |
| `Version` | Positive integer | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | N/A | N/A | Not used. |
| `FeedFlag` | `P`, `T` | `P`, `T` | Production or test feed. |
| `Source` | `SCGEN@Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| Message | When to send |
|---|---|
| `DT_PARTIC` | Bulk participant feed before the Games. |
| `DT_PARTIC` | Repeated as needed until participant data is transferred to the venue results system. |
| `DT_PARTIC` with `DocumentSubtype="SYNC"` | Full sync after the venue has begun sending updates. |
| `DT_PARTIC_UPDATE` | After venue transfer, whenever participant data changes. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- Participant (1,N)
        +-- @Code
        +-- @Parent
        +-- @Status
        +-- @GivenName
        +-- @FamilyName
        +-- @PassportGivenName
        +-- @PassportFamilyName
        +-- @PrintName
        +-- @PrintInitialName
        +-- @TVName
        +-- @TVInitialName
        +-- @TVFamilyName
        +-- @LocalFamilyName
        +-- @LocalGivenName
        +-- @PSCBName
        +-- @PSCBShortName
        +-- @PSCBLongName
        +-- @Gender
        +-- @Organisation
        +-- @Height
        +-- @BirthDate
        +-- @PlaceofBirth
        +-- @CountryofBirth
        +-- @PlaceofResidence
        +-- @CountryofResidence
        +-- @Nationality
        +-- @MainFunctionId
        +-- @OlympicSolidarity
        +-- Discipline (1,1)
            +-- @Code
            +-- @IFId
            +-- DisciplineEntry (0,1)
                +-- @Type
                +-- @Code
                +-- @Pos
                +-- @Value
```

## Participant Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competition/@Gen` | O | Text | General schema/package version label. |
| `Competition/@Sport` | O | Text | Sport dictionary version label. |
| `Competition/@Codes` | O | Text | Code set version label. |
| `Participant/@Code` | M | `S(20)` | Participant ID. IDs must not have leading zeroes. Historical IDs start with `A`, `C`, or `O` for athlete, coach, or official. |
| `Participant/@Parent` | M | `S(20)` | Latest valid participant parent. It can differ from `Code` only for non-current historical data. |
| `Participant/@Status` | O | `CC@PARTICIPANT_STATUS` | Mandatory for current participants, optional for historical records. A specific status value is used to delete a participant. |
| `Participant/@GivenName` / `@FamilyName` | O | Text | Base name fields. |
| `Participant/@PrintName` / `@PrintInitialName` | O | Text | Print display names. |
| `Participant/@TVName` / `@TVInitialName` / `@TVFamilyName` | O | Text | Broadcast display names. |
| `Participant/@Gender` | M | `CC@GENDER` | Participant gender. |
| `Participant/@Organisation` | M | `CC@ORGANISATION` | Participant NOC/team organisation. |
| `Participant/@BirthDate` | O | Date | Birth date when known. |
| `Participant/@Nationality` | O | `CC@NATIONALITY` | Nationality when relevant. |
| `Participant/@MainFunctionId` | O | `CC@FUNCTION` | Mandatory for current participants. |
| `Participant/@OlympicSolidarity` | O | `Y` | Present when applicable. |
| `Participant/Discipline/@Code` | M | `CC@DISCIPLINE` | Full discipline RSC. It matches `OdfBody/@DocumentCode`. |
| `Participant/Discipline/@IFId` | O | Text | International federation ID. |

## Discipline Entry Values

| Type | Code | Pos | Value | When |
|---|---|---|---|---|
| `ENTRY` | `HAND` | N/A | `SC@Hand` | As soon as available for individual events. |

## Message Sort

Sort by `Participant/@Code`.

## Modeling Notes

- Treat `DT_PARTIC` as participant master data scoped by discipline, not by event unit.
- Use `Participant/@Code` as the stable join key from `DT_ENTRIES`, `DT_RESULT`, and related athlete payloads.
- For `DT_PARTIC_UPDATE`, replace the whole participant record from the received `Participant` node rather than
  patching individual attributes.
- The sample printed in this section is generic and includes structures that are not specific to the Badminton table;
  use the BDM message-value tables above as the canonical source for BDM-specific fields.

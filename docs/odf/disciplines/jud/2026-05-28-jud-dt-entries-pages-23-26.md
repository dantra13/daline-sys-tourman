# ODF JUD Data Dictionary: DT_ENTRIES, Pages 23-26

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_JUD_Data_Dictionary.pdf`, pages 23-26.

Source version: `SCOG/SYOG-2026-JUD-1.3 SFR`, dated 13 May 2026.

This note restructures the Judo `DT_ENTRIES` section into a practical reference for event-level entries. It covers
individual athlete entries, athlete qualification details, world ranking, and entry message replacement semantics.

## 2.3.3 List of Entries by Event

`DT_ENTRIES` contains entry information for a specific Judo event, including event-specific entry information for each
participant.

The message is always a full message: a new `DT_ENTRIES` message for the same event resets all previous participants'
entry information for that event. The generic description allows athletes, guides, reserves, teams, and team
composition; the Judo value table declares athlete entries with `Entry/@Type="A"`.

## Header Values

| Attribute         | Value                 | Meaning                                           |
|-------------------|-----------------------|---------------------------------------------------|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID.                                   |
| `DocumentCode`    | `CC@EVENT`            | Event RSC.                                        |
| `DocumentSubcode` | N/A                   | Not used.                                         |
| `DocumentType`    | `DT_ENTRIES`          | List of entries by event message.                 |
| `DocumentSubtype` | N/A                   | Not used.                                         |
| `Version`         | Positive integer      | Ascending version number for the message content. |
| `ResultStatus`    | N/A                   | Not used.                                         |
| `FeedFlag`        | `P`, `T`              | `P` production, `T` test.                         |
| `Date`            | Date                  | ODF header generation date.                       |
| `Time`            | Time                  | ODF header generation time.                       |
| `LogicalDate`     | Date                  | ODF logical date.                                 |
| `Source`          | `SCGEN@Source`        | System that generated the message.                |

## Trigger and Frequency

| Stage              | When to send                                                                        |
|--------------------|-------------------------------------------------------------------------------------|
| Before the Games   | Bulk entries feed, repeated as needed up to the date of transfer of control to OVR. |
| After OVR takeover | Bulk entries feed triggered by OVR when entry data changes at the venue.            |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- Entry (1,N)
        +-- @Code
        +-- @Type
        +-- @Organisation
        +-- @SortOrder
        +-- @EntryStatus
        +-- ExtendedEntry (0,N)
        |   +-- @Type
        |   +-- @Code
        |   +-- @Pos
        |   +-- @Value
        +-- Composition (0,1)
            +-- Athlete (0,N)
                +-- @Code
                +-- @Order
                +-- @EntryStatus
                +-- Description (1,1)
                |   +-- @GivenName
                |   +-- @FamilyName
                |   +-- @Gender
                |   +-- @Organisation
                |   +-- @BirthDate
                |   +-- @IFId
                +-- ExtendedEntry (0,N)
                    +-- @Type
                    +-- @Code
                    +-- @Pos
                    +-- @Value
```

## Message Values

### `Competition`

| Attribute | M/O | Value   | Meaning                                                    |
|-----------|-----|---------|------------------------------------------------------------|
| `Gen`     | M   | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport`   | M   | `S(35)` | Sport Data Dictionary version applicable to the message.   |
| `Codes`   | M   | `S(20)` | Code-set version applicable to the message.                |

### `Competition / Entry`

| Attribute      | M/O | Value                          | Meaning                                                                                                |
|----------------|-----|--------------------------------|--------------------------------------------------------------------------------------------------------|
| `Code`         | M   | `S(20)` with no leading zeroes | Competitor ID.                                                                                         |
| `Type`         | M   | `A`                            | Athlete entry.                                                                                         |
| `Organisation` | M   | `CC@ORGANISATION` ID           | Competitor organisation.                                                                               |
| `SortOrder`    | M   | Positive integer               | Order used to sort competitors within the event.                                                       |
| `EntryStatus`  | O   | `SC@AthleteStatus`             | Entry participation status. Declared in the structure but not separately described in the value table. |

### `Competition / Entry / ExtendedEntry`

The structure declares entry-level `ExtendedEntry`, but pages 23-26 do not enumerate Judo entry-level codes. Preserve
the
path for ingestion but do not infer codes from the generic sample.

| Type    | Code           | Pos | Expected When   | Value           | Meaning                                                                          |
|---------|----------------|-----|-----------------|-----------------|----------------------------------------------------------------------------------|
| `ENTRY` | Not enumerated | N/A | Not documented. | Not documented. | Entry-level extension path declared without concrete Judo codes in this section. |

### `Competition / Entry / Composition / Athlete`

| Attribute     | M/O | Value                          | Meaning                                                                   |
|---------------|-----|--------------------------------|---------------------------------------------------------------------------|
| `Code`        | M   | `S(20)` with no leading zeroes | Athlete ID.                                                               |
| `Order`       | M   | Positive integer               | `1` in individual events; athlete starting order in team entries if used. |
| `EntryStatus` | O   | `SC@AthleteStatus`             | Athlete event participation status.                                       |

### `Competition / Entry / Composition / Athlete / Description`

| Attribute      | M/O | Value                 | Meaning                                 |
|----------------|-----|-----------------------|-----------------------------------------|
| `GivenName`    | O   | `S(25)`               | Preferred given name.                   |
| `FamilyName`   | M   | `S(25)`               | Preferred family name.                  |
| `Gender`       | M   | `CC@PERSON_GENDER` ID | Athlete gender.                         |
| `Organisation` | M   | `CC@ORGANISATION` ID  | Athlete organisation.                   |
| `BirthDate`    | O   | `YYYY-MM-DD`          | Date of birth, included when available. |
| `IFId`         | O   | `S(16)`               | International Federation ID.            |

### `Competition / Entry / Composition / Athlete / ExtendedEntry`

| Type    | Code        | Pos | Expected When                                                        | Value                         | Meaning                             |
|---------|-------------|-----|----------------------------------------------------------------------|-------------------------------|-------------------------------------|
| `ENTRY` | `QUAL_TYPE` | N/A | Always when known and the athlete has qualification tournament data. | Qualification tournament code | Qualification code/tournament code. |
| `ENTRY` | `RANK_WLD`  | N/A | As soon as known.                                                    | `S(3)`                        | World ranking.                      |

## Sample from the Dictionary, Normalized

The sample printed on page 26 is a generic team-sport sample with `Entry/@Type="T"`, coaches, and athlete extended
entries such as `POSITION` and `ROLE`. Those paths and codes are not declared by the Judo value tables on pages 23-26.
The example below keeps the Judo-declared athlete entry shape.

```xml

<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-JUD-1.3 SFR" Codes="SYOG-2026">
    <Entry Code="1008743" Type="A" Organisation="SUI" SortOrder="1">
        <Composition>
            <Athlete Code="1008743" Order="1">
                <Description GivenName="Jane" FamilyName="Smits" Gender="W" Organisation="SUI"
                             BirthDate="2009-12-15" IFId="IJFSUIW1008743"/>
                <ExtendedEntry Type="ENTRY" Code="QUAL_TYPE" Value="WRL"/>
                <ExtendedEntry Type="ENTRY" Code="RANK_WLD" Value="007"/>
            </Athlete>
        </Composition>
    </Entry>
    <Entry Code="1008750" Type="A" Organisation="SEN" SortOrder="2">
        <Composition>
            <Athlete Code="1008750" Order="1">
                <Description GivenName="Awa" FamilyName="Diop" Gender="W" Organisation="SEN"
                             BirthDate="2009-04-12" IFId="IJFSENW1008750"/>
            </Athlete>
        </Composition>
    </Entry>
</Competition>
```

## Message Sort

Sort by `Entry/@SortOrder`.

## Modeling Notes

- Treat `DT_ENTRIES` as a full event roster snapshot. Replace prior entry state for the same event on each accepted
  version.
- Judo pages 23-26 define athlete entries (`Entry/@Type="A"`). Do not infer team, coach, `POSITION`, or `ROLE` support
  from the generic sample printed in the PDF.
- Preserve `Entry/@EntryStatus` and `Athlete/@EntryStatus` separately; they are separate scopes even if both use
  `SC@AthleteStatus`.
- `QUAL_TYPE` and `RANK_WLD` live at athlete-level `ExtendedEntry` in the Judo value table.
- `RANK_WLD` is declared as `S(3)`, so retain leading zeroes if a feed sends them.

## Code Appendix: Values Directly Visible in Pages 23-26

The section references code domains but does not print full code tables.

| Code Entity                   | Section Usage                                              | Values Visible in Section                                                                                                     |
|-------------------------------|------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------|
| `CC@COMPETITION_CODE`         | `OdfBody/@CompetitionCode`                                 | No concrete values printed in pages 23-26.                                                                                    |
| `CC@EVENT`                    | `OdfBody/@DocumentCode`                                    | No concrete values printed in pages 23-26.                                                                                    |
| `SCGEN@Source`                | `OdfBody/@Source`                                          | No concrete values printed in pages 23-26.                                                                                    |
| `CC@ORGANISATION`             | `Entry/@Organisation`, `Athlete/Description/@Organisation` | Generic sample shows `BEL`; normalized Judo example uses `SUI`, `SEN`.                                                        |
| `CC@PERSON_GENDER`            | `Athlete/Description/@Gender`                              | Generic sample shows `M`; normalized example uses `W`.                                                                        |
| `SC@AthleteStatus`            | `Entry/@EntryStatus`, `Athlete/@EntryStatus`               | No concrete values printed in pages 23-26.                                                                                    |
| Athlete `ExtendedEntry/@Code` | Athlete event-entry extensions                             | `QUAL_TYPE`, `RANK_WLD`. Generic sample also shows `POSITION` and `ROLE`, but they are not declared for Judo in this section. |

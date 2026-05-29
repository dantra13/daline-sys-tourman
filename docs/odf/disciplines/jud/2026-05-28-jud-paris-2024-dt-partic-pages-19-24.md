# ODF JUD Data Dictionary: Paris 2024 DT_PARTIC / DT_PARTIC_UPDATE, Pages 19-24

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 19-24.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note restructures the Paris 2024 Judo participant messages. `DT_PARTIC` is the complete discipline participant
snapshot, while `DT_PARTIC_UPDATE` carries modified participant records.

## 2.3.2 List of participants by discipline / update

Participants include athletes, officials, coaches, guides, technical officials, reserves, historical athletes, and team
members. Sport messages that reference athletes must match athlete IDs from this message.

Historical athletes are included for record matching but are not registered to current events.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Discipline` | Full discipline RSC. |
| `DocumentType` | `DT_PARTIC` / `DT_PARTIC_UPDATE` | Bulk participant message or participant update. |
| `DocumentSubtype` | `S(20)` | `SYNC` after venue control transfer, or `HISTORICAL` for historical-provider messages; neither appears on update messages. |
| `Version` | `1..V` | Ascending content version. |
| `FeedFlag` | `P`, `T` | Production or test. |
| `Date` / `Time` / `LogicalDate` | Date/time | Header generation and logical dates. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Message | Trigger |
|---|---|
| `DT_PARTIC` | Bulk message before the Games, repeated until transfer of control to OVR. |
| `DT_PARTIC_UPDATE` | After OVR transfer, whenever data changes for an individual participant. |

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
        +-- @Gender
        +-- @Organisation
        +-- @BirthDate
        +-- @PlaceofBirth
        +-- @CountryofBirth
        +-- @PlaceofResidence
        +-- @CountryofResidence
        +-- @Nationality
        +-- @MainFunctionId
        +-- @Current
        +-- @OlympicSolidarity
        +-- @ModificationIndicator
        +-- Discipline (1,1)
            +-- @Code
            +-- @IFId
            +-- DisciplineEntry (0,N)
            +-- RegisteredEvent (0,N)
                +-- @Event
                +-- @Class
                +-- EventEntry (0,N)
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General dictionary version. |
| `Sport` | O | `S(20)` | Sport dictionary version. |
| `Codes` | O | `S(20)` | Codes version. |

### `Competition / Participant`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` | Participant ID. Historical IDs start with `A`, `C`, or `O` depending on athlete, coach, or official. |
| `Parent` | M | `S(20)` | Latest-valid participant ID. Differs from `Code` only for historical records with changed critical data. |
| `Status` | O | `CC@ParticStatus` | Accreditation status; mandatory when `Current="true"`. |
| Name fields | Mixed | `S(18)`-`S(35)` | WNPA, passport, print, TV, and local names. |
| `Gender` | M | `CC@PersonGender` | Participant gender. |
| `Organisation` | M | `CC@Organisation` | Organisation ID. |
| `BirthDate` | O | `YYYY-MM-DD` | Expected for athletes. |
| Birth/residence/nationality fields | O | Text or `CC@Country` | Place/country data. |
| `MainFunctionId` | O | `CC@ResultsFunction` | Main function; mandatory when `Current="true"`. |
| `Current` | M | Boolean | Current Games participant vs historical participant. |
| `OlympicSolidarity` | O | `Y` | Scholarship/solidarity flag. |
| `ModificationIndicator` | M in update | `N`, `U` | New or update in `DT_PARTIC_UPDATE`; deletion is represented through `Status`. |

### `Participant / Discipline`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `CC@Discipline` | Discipline RSC matching `OdfBody/@DocumentCode`. |
| `IFId` | O | `S(16)` | IJF unique judoka ID. |

### `DisciplineEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ENTRY` | `BIB` | N/A | As soon as known; only in update messages. | String | Official's bib. |

### `RegisteredEvent`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Event` | M | `CC@Event` | Full event RSC. |
| `Class` | O | `CC@SportClass` | Paralympic class; only for current participants where applicable. |

### `EventEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ENTRY` | `QUAL_TYPE` | N/A | As soon as known. | `SC@QualifyingType` | Qualification tournament code. The PDF spelling of this code entity appears to contain a typo. |
| `ENTRY` | `RANK_WLD` | N/A | As soon as known. | `S(3)` | World ranking. |
| `ENTRY` | `DEAF` | N/A | Paralympic Games only, when known. | `D` | Hearing impairment flag. |

## Sample, Normalized

```xml
<Competition Gen="SOG-2024-GEN" Sport="SOG-2024-JUD-3.3 APP" Codes="SOG-2024">
  <Participant Code="1008743" Parent="1008743" Status="ACTIVE" GivenName="Jane" FamilyName="Smits"
               PrintName="SMITS Jane" PrintInitialName="SMITS J" TVName="Jane Smits"
               TVInitialName="J SMITS" TVFamilyName="Smits" Gender="W" Organisation="SUI"
               BirthDate="1994-12-15" MainFunctionId="AA01" Current="true">
    <Discipline Code="JUD" IFId="IJF1008743">
      <RegisteredEvent Event="JUDW52KG--------------------------">
        <EventEntry Type="ENTRY" Code="QUAL_TYPE" Value="WRL"/>
        <EventEntry Type="ENTRY" Code="RANK_WLD" Value="007"/>
      </RegisteredEvent>
    </Discipline>
  </Participant>
</Competition>
```

## Message Sort

Sort by `Participant/@Code`.

## Modeling Notes

- `Code` is the versioned participant record; `Parent` points to the latest identity. Keep both for historical records.
- Do not drop historical participants: records and historical comparisons may reference them.
- Use `DT_PARTIC_TEAMS` for team identity; use `DT_PARTIC` for individual participant/team-member identity.
- `RANK_WLD` is a string of length three; preserve leading zeroes.

## Code Appendix: Values Visible in Pages 19-24

| Code Entity | Section Usage | Visible Values |
|---|---|---|
| `DocumentType` | Header | `DT_PARTIC`, `DT_PARTIC_UPDATE` |
| `DocumentSubtype` | Header | `SYNC`, `HISTORICAL` |
| `ModificationIndicator` | Updates | `N`, `U` |
| `OlympicSolidarity` | Participant flag | `Y` |
| `EventEntry/@Code` | Event extensions | `QUAL_TYPE`, `RANK_WLD`, `DEAF` |
| `DisciplineEntry/@Code` | Discipline extension | `BIB` |

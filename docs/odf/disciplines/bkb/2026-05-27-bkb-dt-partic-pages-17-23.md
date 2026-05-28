# ODF BKB Data Dictionary: DT_PARTIC / DT_PARTIC_UPDATE, Pages 17-23

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 17-23.

Source version: `SOG-2024-BKB-3.4 APP`, dated 19 January 2024.

This note restructures the Basketball `DT_PARTIC` and `DT_PARTIC_UPDATE` section into a practical reference for
participant master data by discipline.

## 2.3.2 List of Participants by Discipline / List of Participants by Discipline Update

`DT_PARTIC` contains participant information for the Basketball discipline and all its events. Athlete identifiers
referenced by start lists, results, and play-by-play must match the participant codes sent here. The message can
also include officials, coaches, guides, technical officials, reserves, and historical athletes regardless of status.

`DT_PARTIC` is a full bulk message. Each emission resets the previous participant set for the discipline. It is sent
before the Games and can be repeated until control is transferred to the venue results system (OVR).

`DT_PARTIC_UPDATE` is the incremental update message used after venue transfer. For each changed participant, the
update sends the complete `Participant` element with all its applicable child elements and attributes. The update key
is `Participant/@Code`. The required `ModificationIndicator` attribute distinguishes new (`N`) vs updated (`U`)
participants. To delete a participant, a specific value of `@Status` is used.

## Header Values

| Attribute | `DT_PARTIC` | `DT_PARTIC_UPDATE` | Meaning |
|---|---|---|---|
| `CompetitionCode` | `CC@Competition` | `CC@Competition` | Unique competition ID. |
| `DocumentCode` | `CC@Discipline` | `CC@Discipline` | Full RSC at the discipline level (Basketball). |
| `DocumentType` | `DT_PARTIC` | `DT_PARTIC_UPDATE` | Participants by discipline message. |
| `DocumentSubtype` | `SYNC`, `HISTORICAL`, or N/A | N/A | `SYNC` flags a full re-sync after control is transferred to the venue. `HISTORICAL` flags messages from the historical-results provider and is never sent to external clients. Neither value is ever included in `_UPDATE` messages. |
| `Version` | `1..V` | `1..V` | Ascending version number for the message's content. |
| `FeedFlag` | `P` or `T` | `P` or `T` | Production or test feed. |
| `Date` | Date | Date | Message generation date in the producing system's local timezone. |
| `Time` | Time | Time | Message generation time (to milliseconds) in the producing system's local timezone. |
| `LogicalDate` | Date | Date | Logical event day. Equals the physical day except when a unit or transmission extends past midnight. |
| `Source` | `SC@Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Message | When to send |
|---|---|
| `DT_PARTIC` | Bulk participant feed prior to the Games. |
| `DT_PARTIC` | Repeated as needed until participant data is transferred to OVR. |
| `DT_PARTIC` with `DocumentSubtype="SYNC"` | Full re-sync after control has been transferred to the venue. |
| `DT_PARTIC_UPDATE` | After venue transfer, whenever any individual's participant data changes. |

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
        +-- @Height
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
            +-- RegisteredEvent (0,N)
                +-- @Event
                +-- @Bib
                +-- @Class
                +-- @Status
                +-- @Substitute
                +-- EventEntry (0,N)
                    +-- @Type
                    +-- @Code
                    +-- @Pos
                    +-- @Value
```

## Message Values

### Element: `Competition` (0,1)

| Attribute | M/O | Value | Description |
|---|:--:|---|---|
| `Gen` | O | `S(20)` | Version of the General Data Dictionary applicable to the message. |
| `Sport` | O | `S(20)` | Version of the Sport Data Dictionary applicable to the message. |
| `Codes` | O | `S(20)` | Version of the Codes applicable to the message. |

### Element: `Competition/Participant` (1,N)

| Attribute | M/O | Value | Description |
|---|:--:|---|---|
| `Code` | M | `S(20)` no leading zeroes | Participant ID. Stable join key for start lists, results, and other messages. Historical athletes start with `A`, coaches with `C`, officials with `O`. |
| `Parent` | M | `S(20)` no leading zeroes | Latest valid participant parent. Equal to `Code` for current participants; only differs when `Current="false"` (typically after Organisation change or name change). |
| `Status` | O | `CC@ParticStatus` | Accreditation status. Mandatory when `Current="true"`, optional when `Current="false"`. A specific value is used to delete a participant. |
| `GivenName` | O | `S(25)` | Given name in WNPA mixed case. |
| `FamilyName` | M | `S(25)` | Family name in WNPA mixed case. |
| `PassportGivenName` | O | `S(25)` | Passport given name (uppercase). |
| `PassportFamilyName` | O | `S(25)` | Passport family name (uppercase). |
| `PrintName` | M | `S(35)` | Family name uppercase plus given name in mixed case. |
| `PrintInitialName` | M | `S(18)` | Print initial form (given name reduced to its first letter, no dot). |
| `TVName` | M | `S(35)` | TV name. |
| `TVInitialName` | M | `S(18)` | TV initial name. |
| `TVFamilyName` | M | `S(25)` | TV family name. |
| `LocalFamilyName` | O | `S(25)` | Family name in the local language. |
| `LocalGivenName` | O | `S(25)` | Given name in the local language. |
| `Gender` | M | `CC@PersonGender` | Participant gender. |
| `Organisation` | M | `CC@Organisation` | NOC/team organisation. |
| `BirthDate` | O | `YYYY-MM-DD` | Expected for athletes; not expected for all official groups. |
| `Height` | O | `S(3)` | Height in centimetres. Not needed for officials/referees. `"-"` may be used when unavailable. |
| `PlaceofBirth` | O | `S(75)` | Place of birth. |
| `CountryofBirth` | O | `CC@Country` | Country ID of birth. |
| `PlaceofResidence` | O | `S(75)` | Place of residence. |
| `CountryofResidence` | O | `CC@Country` | Country ID of residence. |
| `Nationality` | O | `CC@Country` | Nationality. Optional only because in exceptional cases it is unknown at send time. |
| `MainFunctionId` | O | `CC@ResultsFunction` | Main function. Mandatory when `Current="true"`. |
| `Current` | M | boolean | `true` if the participant is participating in the Games, `false` if historical. |
| `OlympicSolidarity` | O | `S(1)` | Send `Y` if the participant is part of the Solidarity / Scholarship programme; otherwise omit. |
| `ModificationIndicator` | M (in `_UPDATE`) | `S(1)` | Required only in `DT_PARTIC_UPDATE`. `N` for a new (late-entry) participant; `U` for an updated participant. Deletion is signalled via `@Status`. |

### Element: `Competition/Participant/Discipline` (1,1)

Every accredited athlete and every accredited official is assigned to at least one discipline; they may be assigned to
more than one, in which case they appear in the participant message for each discipline.

| Attribute | M/O | Value | Description |
|---|:--:|---|---|
| `Code` | M | `CC@Discipline` | Full discipline RSC. Equals `OdfBody/@DocumentCode`. |
| `IFId` | O | `S(16)` | Competitor's international federation number for this discipline. |

### Element: `Competition/Participant/Discipline/RegisteredEvent` (0,N)

All accredited athletes are assigned to one or more events, with one exception: in some sports, substitutes may be
accredited without any associated event. Historical athletes are not registered to any event.

| Attribute | M/O | Value | Description |
|---|:--:|---|---|
| `Event` | M | `CC@Event` | Full RSC of the event. |
| `Bib` | O | `S(2)` | Shirt number. Updated as soon as known. |
| `Class` | O | `CC@DisciplineClass` | Athlete class code. Mandatory for athletes in Wheelchair Basketball. |
| `Status` | O | `SC@AthleteStatus` | Athlete status when applicable; otherwise omit. |
| `Substitute` | O | `S(1)` | `Y` when the competitor is an alternate; otherwise omit. |

### Element: `Competition/Participant/Discipline/RegisteredEvent/EventEntry` (0,N)

Sent when specific event-level entries are available for the athlete. The same `Type` / `Code` shape carries different
payloads:

| Type | Code | Pos | `@Value` source | When |
|---|---|---|---|---|
| `ENTRY` | `POSITION` | N/A | `CC@Position` | If known. Position code in the team (e.g. `PG`, `SG`, `SF`, `PF`, `C`). |
| `ENTRY` | `CLUB_NAME` | N/A | `S(25)` | If known. Club name. |
| `ENTRY` | `CLUB_ORG` | N/A | `CC@Country` | If known. Club country code. |
| `ENTRY` | `CLUB_LEAGUE` | N/A | `S(10)` | If known. League of the club. |
| `ENTRY` | `NATURALISED` | N/A | `S(1)` | If known. `Y` if naturalised, otherwise omit. |
| `ENTRY` | `CAPTAIN` | N/A | `S(1)` | If known. `Y` if the participant is captain, otherwise omit. |

## Message Sort

The message is sorted by `Participant/@Code`.

## XSD-Aligned XML Example

The PDF supplies a partial Discipline-level sample only. The full example below is reconstructed from the message
structure and conforms to the attribute model documented above. Not validated — no XSD was supplied.

```xml
<OdfBody DocumentCode="BKB-------------------------------"
         DocumentType="DT_PARTIC"
         Version="1"
         FeedFlag="P"
         Date="2024-07-25"
         Time="12:30:00.000"
         LogicalDate="2024-07-25"
         Source="BCYBKB1">
  <Competition Gen="SOG-2024-1.10"
               Sport="SOG-2024-BKB-3.4"
               Codes="SOG-2024-1.20">
    <Participant Code="1234567"
                 Parent="1234567"
                 Status="ACTIVE"
                 GivenName="Stephen"
                 FamilyName="Curry"
                 PassportGivenName="STEPHEN"
                 PassportFamilyName="CURRY"
                 PrintName="CURRY Stephen"
                 PrintInitialName="CURRY S"
                 TVName="S CURRY"
                 TVInitialName="S CURRY"
                 TVFamilyName="CURRY"
                 Gender="M"
                 Organisation="USA"
                 BirthDate="1988-03-14"
                 Height="188"
                 Nationality="USA"
                 MainFunctionId="AT"
                 Current="true">
      <Discipline Code="BKB-------------------------------" IFId="203258">
        <RegisteredEvent Event="BKBMTEAM5-------------------------" Bib="4">
          <EventEntry Type="ENTRY" Code="POSITION" Value="PG" />
          <EventEntry Type="ENTRY" Code="CLUB_NAME" Value="Golden State Warriors" />
          <EventEntry Type="ENTRY" Code="CLUB_ORG" Value="USA" />
          <EventEntry Type="ENTRY" Code="CLUB_LEAGUE" Value="NBA" />
          <EventEntry Type="ENTRY" Code="CAPTAIN" Value="Y" />
        </RegisteredEvent>
      </Discipline>
    </Participant>
  </Competition>
</OdfBody>
```

## Modeling Notes

- Treat `DT_PARTIC` as participant master data scoped by discipline, not by event unit. Each emission of the full
  message replaces prior participant state for the discipline; `_UPDATE` patches only the included `Participant`
  entities.
- `Participant/@Code` is the stable join key from `DT_PARTIC` to `DT_ENTRIES`, `DT_RESULT`, `DT_PLAY_BY_PLAY`, and
  related athlete payloads. Codes never carry leading zeroes.
- `@Parent` lets historical records point to the latest valid version of the same person. Only ever differs from
  `@Code` when `Current="false"`. Always resolve current personal data via the participant whose `@Code` equals the
  historical record's `@Parent`.
- Treat `@Status` plus `@ModificationIndicator` as a small state machine for the update feed: `N` adds, `U` patches,
  and a specific `@Status` value deletes. Replace the whole participant record from the received `Participant` node
  rather than patching individual attributes.
- `RegisteredEvent/@Class` is only meaningful for Wheelchair Basketball; the Paris 2024 Olympic feed does not include
  it for the BKB events `BKBMTEAM5` and `BKBWTEAM5`, but the schema slot remains because the dictionary covers both
  Olympic and Paralympic basketball.
- `EventEntry` is path-sensitive: the same `Type="ENTRY"` carries different code/value semantics (position, club name,
  club country, club league, naturalised flag, captain flag). Model these as a tagged union keyed by `(Type, Code)`
  rather than a free-form attribute bag.
- The BKB `SC@AthleteStatus` table only carries disqualification and suspension codes; missing values do not imply
  "active" but rather "no override". Default active state is conveyed via `Participant/@Status="ACTIVE"`.

## Code Appendix: SC and CC Values

### Source index

| Code | Type | Source | Rows |
|---|---|---|---|
| `CC@CompetitionCode` | Common | [CompetitionCode.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) | 10 (link only) |
| `CC@Discipline` | Common | [Discipline.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Discipline.htm) | 59 (link only) |
| `CC@Event` | Common | [Event.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Event.htm) | 620 total; 5 BKB-prefixed (embedded) |
| `CC@Organisation` | Common | [Organisation.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) | 258 (link only) |
| `CC@Country` | Common | [Country.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm) | 266 (link only) |
| `CC@DisciplineFunction` | Common | [DisciplineFunction.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineFunction.htm) | 437 (link only) |
| `CC@ParticStatus` | Common | [ParticipantStatus.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ParticipantStatus.htm) | 2 (embedded) |
| `CC@PersonGender` | Common | [PersonGender.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm) | 3 (embedded) |
| `CC@Position` | Common | [Positions.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Positions.htm) | 92 total; 7 BKB rows (embedded) |
| `CC@DisciplineClass` | Common | (page returns 404 in the Paris 2024 code site) | n/a |
| `SC@AthleteStatus` (BKB) | Sport-specific | [odf_sc_AthleteStatus_SOG_BKB.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_AthleteStatus_SOG_BKB.htm) | 3 (embedded) |
| `SC@Source` (BKB) | Sport-specific | [odf_sc_Source_SOG_BKB.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm) | 2 (embedded) |

### `CC@ParticStatus`

| Id | ENG Description |
|---|---|
| `ACTIVE` | Active |
| `CANCEL` | Cancelled |

### `CC@PersonGender`

| Id | ENG Description |
|---|---|
| `F` | Female |
| `M` | Male |
| `X` | Unspecified |

### `CC@Event` (BKB-prefixed rows)

| Code | Discipline | Gender | Event | Team | ENG Description |
|---|---|---|---|---|---|
| `BKB-------------------------------` | BKB | - | (none) | N | Basketball |
| `BKBM------------------------------` | BKB | M | (none) | N | Men |
| `BKBMTEAM5-------------------------` | BKB | M | `TEAM5-------------` | Y | Men |
| `BKBW------------------------------` | BKB | W | (none) | N | Women |
| `BKBWTEAM5-------------------------` | BKB | W | `TEAM5-------------` | Y | Women |

### `CC@Position` (BKB rows)

| Discipline | Id | ENG Description |
|---|---|---|
| BKB | `C` | Centre |
| BKB | `F` | Forward |
| BKB | `G` | Guard |
| BKB | `PF` | Power Forward |
| BKB | `PG` | Point Guard |
| BKB | `SF` | Small Forward |
| BKB | `SG` | Shooting Guard |

### `SC@AthleteStatus` (BKB)

| Code | ENG Description |
|---|---|
| `DQB` | Disqualified for unsportmanlike behaviour |
| `DSQ` | Disqualified |
| `SUSPEND` | Suspended |

### `SC@Source` (BKB)

| Code | ENG Description |
|---|---|
| `BCYBKB1` | Origin for messages from OVR at BCY for BKB |
| `LILBKB1` | Origin for messages from OVR at LIL for BKB |

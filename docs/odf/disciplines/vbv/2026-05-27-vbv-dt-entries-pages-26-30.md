# ODF VBV Data Dictionary: DT_ENTRIES, Pages 26-30

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 26-30.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note restructures the Beach Volleyball `DT_ENTRIES` section into a practical reference for event-level entries. It
covers the team entry header, team composition, coaches, athlete entries, and the seeding/group/captain/position
extensions that drive the entry list for a Beach Volleyball event.

## 2.3.4 List of Entries by Event

`DT_ENTRIES` contains entry information for one specific Beach Volleyball event. It lists the teams entered in the
event, the team composition (when known), the coaches attached to the team, and the athletes that form the entry.

The message is always a full message. A new `DT_ENTRIES` message for the same event resets the previous entry list for
that event.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@EVENT` | Event RSC. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_ENTRIES` | List of entries by event message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | N/A | Not used. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| Stage | When to send |
|---|---|
| Before the Games | Bulk entries feed, repeated as needed up to the date of transfer of control to OVR. |
| After OVR takeover | Bulk entries feed triggered by the OVR (venue results system) whenever entry data changes. |

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
        +-- @Bib
        +-- @EntryStatus
        +-- @Substitute
        +-- Description (0,1)
        |   +-- @TeamName
        +-- Coaches (0,1)
        |   +-- Coach (1,N)
        |       +-- @Code
        |       +-- @Order
        |       +-- @Function
        |       +-- Description (1,1)
        |       |   +-- @GivenName
        |       |   +-- @FamilyName
        |       |   +-- @Gender
        |       |   +-- @Nationality
        |       |   +-- @IFId
        |       +-- ExtendedEntry (0,N)
        |           +-- @Type
        |           +-- @Code
        |           +-- @Pos
        |           +-- @Value
        +-- Composition (0,1)
            +-- Athlete (0,N)
                +-- @Code
                +-- @Order
                +-- @Bib
                +-- @EntryStatus
                +-- @Substitute
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

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | M | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | M | `S(35)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | M | `S(20)` | Code-set version applicable to the message. |

### `Competition / Entry`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Team ID. |
| `Type` | M | `T` | `T` for team entry. Beach Volleyball entries are always team entries. |
| `Organisation` | M | `CC@ORGANISATION` ID | Competitor organisation. |
| `SortOrder` | M | Positive integer | Order used to sort competitors within an event (by NOC, gender, name, etc.) following the entry-list requirements for the referenced event. |
| `Bib` | O | String | Team bib number when applicable. |
| `EntryStatus` | O | `SC@AthleteStatus` | Team event participation status. |
| `Substitute` | O | `Y` | `Y` if the entered team is a substitute. |

### `Entry / Description`

Used in team event only.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `TeamName` | M | `S(73)` | Name of the team. |

### `Entry / Coaches / Coach`

Team officials extended information.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | O | `S(20)` with no leading zeroes | Team official ID. |
| `Order` | O | Positive integer | Team-official order (`1` if only one team official). |
| `Function` | O | `CC@DISCIPLINE_FUNCTION` ID | Team-official function. |

### `Coach / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `GivenName` | O | `S(25)` | Preferred given name. |
| `FamilyName` | M | `S(25)` | Preferred family name. |
| `Gender` | M | `CC@PERSON_GENDER` ID | Gender. |
| `Nationality` | M | `CC@COUNTRY` ID | Nationality. |
| `IFId` | O | `S(40)` | International Federation ID. |

### `Entry / ExtendedEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ENTRY` | `SEED` | N/A | As soon as it is known (may be sent in both messages). | `#0` | Position in which the team is seeded for the competition. |
| `ENTRY` | `GROUP` | N/A | As soon as it is known (may be sent in both messages). | `S(1)` | Team preliminary group. |

### `Entry / Composition / Athlete`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Athlete ID. |
| `Order` | M | Positive integer | `1` in individual events (if competitor `@Type="A"`), or athlete starting order (`1..n`) within the team (if competitor `@Type="T"`). |
| `Bib` | O | `S(5)` | Bib number. |
| `EntryStatus` | O | `SC@AthleteStatus` | Athlete event participation status. |
| `Substitute` | O | `Y` | `Y` if the entered participant is a substitute. |

### `Athlete / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `GivenName` | O | `S(25)` | Preferred given name. |
| `FamilyName` | M | `S(25)` | Preferred family name. |
| `Gender` | M | `CC@PERSON_GENDER` ID | Athlete gender. |
| `Organisation` | M | `CC@ORGANISATION` ID | Athlete organisation. |
| `BirthDate` | O | `YYYY-MM-DD` | Date of birth, included when available. |
| `IFId` | O | `S(40)` | International Federation ID. |

### `Athlete / ExtendedEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ENTRY` | `CAPTAIN` | N/A | As soon as it is known (may be sent in both messages). | `Y` | `Y` when the competitor is the captain. |
| `ENTRY` | `POSITION` | N/A | As soon as it is known (may be sent in both messages). | `CC@POSITION` ID | Athlete role / playing position. |

## Samples from the Dictionary, Normalized

The sample printed in the PDF uses a `CURMTEAM4--BEL01` team code with five athletes and a `ROLE` extended entry, which
does not match the Beach Volleyball entry structure (two-player teams, no `ROLE` extension in the message-values table).
It appears to be a generic cross-discipline sample. The example below restructures the sample as a valid VBV entry,
keeping only fields that appear in the VBV message-values tables on pages 26-30.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <Entry Code="VBVMTEAM2---BEL01" Type="T" Organisation="BEL" SortOrder="1">
    <Description TeamName="Casillas/Winchester"/>
    <Coaches>
      <Coach Code="8549000" Order="1" Function="COACH">
        <Description GivenName="Renaldo" FamilyName="Ernest" Gender="M" Nationality="BEL"/>
      </Coach>
      <Coach Code="8549100" Order="2" Function="AST_COA">
        <Description GivenName="Carrol" FamilyName="Borrelli" Gender="M" Nationality="BEL"/>
      </Coach>
    </Coaches>
    <ExtendedEntry Type="ENTRY" Code="SEED" Value="1"/>
    <ExtendedEntry Type="ENTRY" Code="GROUP" Value="A"/>
    <Composition>
      <Athlete Code="8548555" Order="1" Bib="1">
        <Description GivenName="Bret" FamilyName="Casillas" Gender="M" Organisation="BEL"
                     BirthDate="1983-11-22" IFId="WCFBELM500666"/>
        <ExtendedEntry Type="ENTRY" Code="CAPTAIN" Value="Y"/>
        <ExtendedEntry Type="ENTRY" Code="POSITION" Value="1"/>
      </Athlete>
      <Athlete Code="8548554" Order="2" Bib="2">
        <Description GivenName="Grady" FamilyName="Winchester" Gender="M" Organisation="BEL"
                     BirthDate="1992-02-01" IFId="WCFBELM215160"/>
        <ExtendedEntry Type="ENTRY" Code="POSITION" Value="2"/>
      </Athlete>
    </Composition>
  </Entry>
</Competition>
```

## Message Sort

Sort by `Entry/@SortOrder`.

## XSD Validation

No XSD was supplied with this extraction task. The normalized example above follows the path structure and attribute
set documented on pages 26-30 and should be re-validated against the SYOG-2026 / OWG2026 ODF schemas when those are
available. The PDF-printed sample was not validated as-is because it carries discipline-foreign content (see Modeling
Notes).

## Modeling Notes

- Treat `DT_ENTRIES` as the event-level roster snapshot. A new message replaces all prior entry state for the same
  event; do not patch-merge against a previous version.
- Beach Volleyball entries are always team entries (`Entry/@Type="T"`). The `Athlete/@Order` rule for individual events
  (`@Type="A"`) is documented but does not apply to VBV.
- `Entry/@Bib`, `Entry/@EntryStatus`, and `Entry/@Substitute` are entry-level attributes documented in the message
  structure but only `EntryStatus` is described in the value table. Carry the structural fields through ingestion even
  if values are sparse so future content does not silently drop.
- `Coaches/Coach` belongs to the entry, not to the athletes. Coach `Function` uses `CC@DISCIPLINE_FUNCTION`, which can
  differ from the function carried in `DT_PARTIC`.
- `ENTRY/SEED` and `ENTRY/GROUP` are entry-level ExtendedEntry codes. Model them as competition-context attributes of
  the team entry, not as athlete attributes.
- `ENTRY/CAPTAIN` and `ENTRY/POSITION` are athlete-level ExtendedEntry codes. Keep them on the athlete in the entry,
  not on the team.
- The dictionary sample uses a `ROLE` athlete ExtendedEntry that is not declared in the VBV message-values table; do
  not implement `ROLE` for VBV based on this section alone. If `ROLE` surfaces in live feeds, treat it as
  unrecognized and log it rather than silently mapping it to `POSITION`.
- Coach-level `ExtendedEntry` is declared in the structure but not described with codes; reserve the path in the model
  but expect no values until the dictionary documents them.

## Code Appendix: Values Directly Visible in Pages 26-30

The section references several code pages. This appendix records only values directly visible in the `DT_ENTRIES`
pages and does not attempt to embed master-data tables.

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@COMPETITION_CODE` | `OdfBody/@CompetitionCode` | No concrete values printed in pages 26-30. |
| `CC@EVENT` | `OdfBody/@DocumentCode` | No concrete values printed in pages 26-30. |
| `SCGEN@Source` | `OdfBody/@Source` | No concrete values printed in pages 26-30. |
| `CC@ORGANISATION` | `Entry/@Organisation`, `Athlete/Description/@Organisation` | `BEL` visible in sample. |
| `CC@DISCIPLINE_FUNCTION` | `Coach/@Function` | `COACH`, `AST_COA`, `COACH_NA` visible in sample. |
| `CC@PERSON_GENDER` | `Coach/Description/@Gender`, `Athlete/Description/@Gender` | `M` visible in sample. |
| `CC@COUNTRY` | `Coach/Description/@Nationality` | `BEL` visible in sample. |
| `SC@AthleteStatus` | `Entry/@EntryStatus`, `Athlete/@EntryStatus` | No concrete values printed in pages 26-30. |
| `CC@POSITION` | `Athlete/ExtendedEntry Code="POSITION"` | `1`, `2`, `3`, `4`, `A` appear in the printed sample as raw values; the dictionary points to `CC@POSITION` for the canonical code list. |
| ExtendedEntry entry codes | `Entry/ExtendedEntry/@Code` | `SEED`, `GROUP`. |
| ExtendedEntry athlete codes | `Athlete/ExtendedEntry/@Code` | `CAPTAIN`, `POSITION`; the printed sample also carries `ROLE` (not declared in the value table). |

# ODF BOX Data Dictionary: DT_ENTRIES, Pages 23-26

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BOX_Data_Dictionary.pdf`, pages 23-26.

Source version: `SCOG/SYOG-2026-BOX-1.2 SFR`, dated 13 May 2026.

This note restructures the Boxing `DT_ENTRIES` section into a practical reference for event-level entries. It covers
the athlete entry header, athlete-level event participation status, entry-level and athlete-level extended-entry
attributes, and athlete identity data for one Boxing event.

## 2.3.3 List of Entries by Event

`DT_ENTRIES` contains the entry information for a specific Boxing event, including the event-specific entry
information of each participant. For Boxing, entries are individual athlete entries.

The message is always a full message: a new `DT_ENTRIES` message for the same event resets all previous participants'
entry information for that event. The section notes that the message can include athletes, guides, reserves, and teams
with team composition when known, although the Boxing entry-value table only declares the athlete entry shape
(`Entry/@Type="A"`).

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
| `FeedFlag` | `P`, `T` | `P` production, `T` test. |
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

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | M | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | M | `S(35)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | M | `S(20)` | Code-set version applicable to the message. |

### `Competition / Entry`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Entry ID. The PDF label says "Team's ID" but the BOX table declares `Type="A"` (athlete), so this is effectively the athlete entry ID. |
| `Type` | M | `A` | `A` for athlete entry. Boxing entries are individual athlete entries. |
| `Organisation` | M | `CC@ORGANISATION` ID | Competitor organisation. |
| `SortOrder` | M | Positive integer | Order used to sort competitors within an event, following the entry-list requirements for the event referenced in the message header. |
| `EntryStatus` | O | `SC@AthleteStatus` | Entry-level event participation status. Declared in the message structure on page 24 but not described in a value table; treat it as the entry counterpart of `Athlete/@EntryStatus`. |

### `Competition / Entry / ExtendedEntry`

The message structure declares entry-level `ExtendedEntry` elements with `@Type`, `@Code`, `@Pos`, and `@Value`, but the
BOX section does not enumerate concrete entry-level codes for individual athlete entries. Reserve the path in the
model; expect no values until the dictionary documents codes for this position.

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ENTRY` | Not enumerated | N/A | Not documented. | Not documented. | Path declared in the message structure with no codes described in the BOX value tables. |

### `Competition / Entry / Composition / Athlete`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Athlete ID. |
| `Order` | M | Positive integer | `1` in individual events (`Competitor/@Type="A"`), or athlete starting order (`1..n`) within the team (`Competitor/@Type="T"`). |
| `EntryStatus` | O | `SC@AthleteStatus` | Athlete event participation status. |

### `Competition / Entry / Composition / Athlete / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `GivenName` | O | `S(25)` | Preferred given name. |
| `FamilyName` | M | `S(25)` | Preferred family name. |
| `Gender` | M | `CC@PERSON_GENDER` ID | Athlete gender. |
| `Organisation` | M | `CC@ORGANISATION` ID | Athlete organisation. |
| `BirthDate` | O | `YYYY-MM-DD` | Date of birth; include when available. |
| `IFId` | O | `S(16)` | International Federation ID. |

### `Competition / Entry / Composition / Athlete / ExtendedEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ENTRY` | `SEED` | N/A | Always, as soon as the seed is known and this athlete has a seed number; may be sent in both messages. | `#0` | Seed number for the athlete in the event. |

## Samples from the Dictionary, Normalized

The sample printed in the PDF (page 26) is a generic team-sport entry: it uses an `Entry/@Type="T"` Belgian "team" code
`CURMTEAM4--BEL01` with a `Description/@TeamName`, a `Coaches` block, and five athletes carrying `POSITION` and `ROLE`
extended entries. None of those structures are declared in the BOX `DT_ENTRIES` message-values tables on pages 23-26.
The example below restructures the sample as a valid BOX athlete entry, keeping only fields that appear in the BOX
message-values tables.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <Entry Code="8548555" Type="A" Organisation="BEL" SortOrder="1">
    <ExtendedEntry Type="ENTRY" Code="SEED" Value="1"/>
    <Composition>
      <Athlete Code="8548555" Order="1">
        <Description GivenName="Bret" FamilyName="Casillas" Gender="M" Organisation="BEL"
                     BirthDate="1983-11-22" IFId="WCFBELM500666"/>
        <ExtendedEntry Type="ENTRY" Code="SEED" Value="1"/>
      </Athlete>
    </Composition>
  </Entry>
  <Entry Code="8548554" Type="A" Organisation="BEL" SortOrder="2">
    <Composition>
      <Athlete Code="8548554" Order="1">
        <Description GivenName="Grady" FamilyName="Winchester" Gender="M" Organisation="BEL"
                     BirthDate="1992-02-01" IFId="WCFBELM215160"/>
      </Athlete>
    </Composition>
  </Entry>
</Competition>
```

The BOX value tables describe `SEED` at the athlete-level `ExtendedEntry` only. The entry-level `ExtendedEntry`
element declared in the message structure is shown above for path completeness but should remain empty until the
dictionary documents entry-level codes for BOX.

## Message Sort

Sort by `Entry/@SortOrder`.

## XSD Validation

No XSD was supplied with this extraction task. The normalized example above follows the path structure and attribute
set documented on pages 23-26 and should be re-validated against the SYOG-2026 / OWG2026 ODF schemas when those become
available. The PDF-printed sample on page 26 was not validated as-is because it carries discipline-foreign content
(team entries with coaches, `POSITION`, and `ROLE`) that does not match the BOX entry-value tables.

## Modeling Notes

- Treat `DT_ENTRIES` as the event-level roster snapshot. A new message replaces all prior entry state for the same
  event; do not patch-merge against a previous version.
- Boxing entries are individual athlete entries (`Entry/@Type="A"`). Even though the section description text mentions
  guides, reserves, and team composition, the BOX value tables only declare the athlete entry shape; do not infer team
  entries from generic dictionary text alone.
- The `Entry/@Code` description in the PDF reads "Team's ID" (carried over from a generic template) but the type code
  is `A`. Model `Entry/@Code` as the athlete entry ID in BOX, and expect it to often coincide with the athlete
  participant ID.
- `Entry/@EntryStatus` is declared in the message structure but not described in a value table. Carry it through
  ingestion with the `SC@AthleteStatus` domain to avoid silent drops, but do not invent codes.
- The entry-level `ExtendedEntry` path is declared in the structure but no codes are enumerated for BOX. Reserve the
  path in the model and log unknown codes if they appear in live feeds.
- The athlete-level `ExtendedEntry/SEED` is the only documented athlete extended-entry code in the BOX section. Other
  codes appearing in the printed sample (`POSITION`, `ROLE`) are from a generic cross-discipline sample and are not
  part of the BOX entry schema; do not implement them for BOX based on this section.
- `SC@AthleteStatus` applies to both `Entry/@EntryStatus` and `Athlete/@EntryStatus`. The two attributes serve
  different scopes (entry-level vs. participant-level inside the entry composition) and can carry different values.
- The PDF anchor for the athlete extended-entry path uses a double slash (`Athlete //ExtendedEntry`); read it as the
  standard single-slash child path. The structure tree on page 24 uses the correct single-slash nesting.

## Code Appendix: Values Directly Visible in Pages 23-26

The section references several code pages. This appendix records only values directly visible in the BOX
`DT_ENTRIES` pages and does not embed master-data tables.

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@COMPETITION_CODE` | `OdfBody/@CompetitionCode` | No concrete values printed in pages 23-26. |
| `CC@EVENT` | `OdfBody/@DocumentCode` | No concrete values printed in pages 23-26. |
| `SCGEN@Source` | `OdfBody/@Source` | No concrete values printed in pages 23-26. |
| `CC@ORGANISATION` | `Entry/@Organisation`, `Athlete/Description/@Organisation` | `BEL` visible in the printed sample. |
| `CC@PERSON_GENDER` | `Athlete/Description/@Gender` | `M` visible in the printed sample. |
| `SC@AthleteStatus` | `Entry/@EntryStatus`, `Athlete/@EntryStatus` | No concrete values printed in pages 23-26. |
| Entry-level `ExtendedEntry/@Code` | `Entry/ExtendedEntry/@Code` | No BOX-specific codes enumerated in pages 23-26. |
| Athlete-level `ExtendedEntry/@Code` | `Athlete/ExtendedEntry/@Code` | `SEED` is the only BOX-declared code. The printed sample also carries `POSITION` (values `1`, `2`, `3`, `4`, `A`) and `ROLE` (values `S`, `V`), neither of which is declared in the BOX value tables. |

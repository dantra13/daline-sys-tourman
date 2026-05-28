# ODF BKB Data Dictionary: DT_PARTIC_TEAMS / DT_PARTIC_TEAMS_UPDATE, Pages 24-29

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 24-29.

Source version: `SOG-2024-BKB-3.4 APP`, dated 19 January 2024.

This note restructures the Basketball `DT_PARTIC_TEAMS` and `DT_PARTIC_TEAMS_UPDATE` section into a practical
reference for team master data by discipline, with the codes that team-level payloads depend on.

## 2.3.3 List of Teams / List of Teams Update

`DT_PARTIC_TEAMS` contains the list of teams related to the current competition for the Basketball discipline.
Team identifiers referenced by start lists, results, play-by-play, and statistics must match the team codes
sent here.

`DT_PARTIC_TEAMS` is a full bulk message by discipline. Each emission resets the previous participant-team set
for the discipline; every team appearing in the list is considered valid and able to participate in at least one
event.

`DT_PARTIC_TEAMS_UPDATE` is an incremental update message. It does not carry the full team list; it only carries
the data for a team that is being modified, with the change semantics expressed through the mandatory
`Team/@ModificationIndicator` attribute (`N`, `U`, or `D`).

## Header Values

| Attribute | `DT_PARTIC_TEAMS` | `DT_PARTIC_TEAMS_UPDATE` | Meaning |
|---|---|---|---|
| `CompetitionCode` | `CC@Competition` | `CC@Competition` | Unique competition ID, for example `OG2024`. |
| `DocumentCode` | `CC@Discipline` | `CC@Discipline` | Full RSC at the discipline level, for example `BKB-------------------------------`. |
| `DocumentType` | `DT_PARTIC_TEAMS` | `DT_PARTIC_TEAMS_UPDATE` | List of participant teams message. |
| `DocumentSubtype` | `SYNC`, `HISTORICAL`, or N/A | N/A | `SYNC` is a full re-sync for ODF clients after transfer to the venue. `HISTORICAL` is for historical-result providers and is not sent to external clients. Never included on `_UPDATE` messages. |
| `Version` | `1..V` ascending | `1..V` ascending | Ascending version number for the message content. |
| `FeedFlag` | `P` or `T` | `P` or `T` | Production or test feed. |
| `Date` | Date | Date | Generation date in the producer's local time zone. |
| `Time` | Time (ms) | Time (ms) | Generation time up to milliseconds in the producer's local time zone. |
| `LogicalDate` | Date | Date | Logical date of events; matches physical day except when transmission crosses midnight. See ODF Foundation. |
| `Source` | `SC@Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Message | When to send |
|---|---|
| `DT_PARTIC_TEAMS` | Bulk team feed before the Games. |
| `DT_PARTIC_TEAMS` | Repeated as needed until control is transferred to OVR. |
| `DT_PARTIC_TEAMS` with `DocumentSubtype="SYNC"` | Full re-sync after the venue has begun sending updates. |
| `DT_PARTIC_TEAMS_UPDATE` | After transfer of control to OVR, whenever any team's data changes. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- Team (1,N)
        +-- @Code
        +-- @Organisation
        +-- @Number
        +-- @Name
        +-- @ShortName
        +-- @TVTeamName
        +-- @Gender
        +-- @Current
        +-- @TeamType
        +-- @ModificationIndicator
        +-- Composition (0,1)
        |   +-- Athlete (0,N)
        |       +-- @Code
        |       +-- @Order
        +-- TeamOfficials (0,1)
        |   +-- Official (1,N)
        |       +-- @Code
        |       +-- @Function
        |       +-- @Order
        +-- Discipline (0,1)
            +-- @Code
            +-- @IFId
            +-- RegisteredEvent (0,1)
                +-- @Event
                +-- EventEntry (0,N)
                    +-- @Type
                    +-- @Code
                    +-- @Pos
                    +-- @Value
```

## Message Values

### Element: Competition (0,1)

| Attribute | M/O | Value | Description |
|---|---:|---|---|
| `Gen` | O | `S(20)` | Version of the General Data Dictionary applicable to the message. |
| `Sport` | O | `S(20)` | Version of the Sport Data Dictionary applicable to the message. |
| `Codes` | O | `S(20)` | Version of the Codes applicable to the message. |

### Element: Competition / Team (1,N)

| Attribute | M/O | Value | Description |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Team ID. Historical team IDs start with `T`. |
| `Organisation` | M | `CC@Organisation` | Team organisation ID (NOC). |
| `Number` | O | Numeric `#0` | Team's number within its organisation for an event. `1` when only one team per organisation participates in the event; otherwise incremental. Required for current teams. |
| `Name` | M | `S(73)` | Team name. |
| `ShortName` | M | `S(40)` | Team short name. |
| `TVTeamName` | M | `S(21)` | Team TV name. |
| `Gender` | M | `CC@SportGender` | Gender code of the team. |
| `Current` | M | boolean | `true` if the team is participating in the games; `false` if it is a historical team. |
| `TeamType` | M | `SC@TeamType` | Team name construction rule, used by clients to build localized names. Use `ORG`. |
| `ModificationIndicator` | M (in `_UPDATE` only) | `N`, `U`, `D` | `N` adds a new team to the previous bulk-loaded list (late entry). `U` updates an existing team. `D` deletes the team from the list. Mandatory in `DT_PARTIC_TEAMS_UPDATE` only. |

### Element: Competition / Team / Composition / Athlete (0,N)

For current teams, the number of athletes is 2 or more.

| Attribute | M/O | Value | Description |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Athlete ID of the listed team member. |
| `Order` | O | Numeric | Team member order. |

### Element: Competition / Team / TeamOfficials / Official (1,N)

Send only if there are specific officials for the team. Does not apply to historical teams.

| Attribute | M/O | Value | Description |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Official ID of the listed team official. |
| `Function` | M | `CC@ResultsFunction` | Official's function for the team. (PDF labels this `ResultsFunction`; the linked code page from the PDF is `DisciplineFunction.htm` filtered by `BKB` - see appendix.) |
| `Order` | O | Numeric `#0` | Official's order in the team. |

### Element: Competition / Team / Discipline (0,1)

Each team is assigned to one discipline. The `Discipline` element is expected unless `ModificationIndicator="D"`.

| Attribute | M/O | Value | Description |
|---|---:|---|---|
| `Code` | M | `CC@Discipline` | Full RSC of the discipline. Matches `OdfBody/@DocumentCode`. |
| `IFId` | O | `S(16)` | International federation ID (competitor's federation number for the corresponding discipline). |

### Element: Competition / Team / Discipline / RegisteredEvent (0,1)

Each current team is assigned to one event. Historical teams are not registered to any event.

| Attribute | M/O | Value | Description |
|---|---:|---|---|
| `Event` | M | `CC@Event` | Full RSC of the event. |

### Element: Competition / Team / Discipline / RegisteredEvent / EventEntry (0,N)

Send only if there are specific team event entries. The `Type`/`Code`/`Pos` combination keys the row;
`Value` is the payload. Path-sensitive: the `Code` set below is specific to this `EventEntry` path.

| Type | Code | Pos | Value | When |
|---|---|---|---|---|
| `ENTRY` | `UNIFORM` | Numeric `0` (`1`=Light, `2`=Dark) | `S(25)` uniform colour | As soon as known. Sent in both messages. |
| `ENTRY` | `DRAW` | N/A | Numeric `0` draw position within the group | As soon as known. Sent in both messages. |
| `ENTRY` | `GROUP` | N/A | `S(1)` team preliminary group | As soon as known. Sent in both messages. |
| `ENTRY` | `RANK_WLD` | N/A | Numeric `##0` FIBA Ranking | As soon as known. Sent in both messages. |
| `ENTRY` | `OG_PLAYED` | N/A | Numeric `##0` games played at previous Olympic Games | As soon as known. Sent in both messages. |
| `ENTRY` | `OG_WIN` | N/A | Numeric `##0` wins at previous Olympic Games | As soon as known. Sent in both messages. |
| `ENTRY` | `OG_LOST` | N/A | Numeric `##0` losses at previous Olympic Games | As soon as known. Sent in both messages. |
| `ENTRY` | `HEIGHT_AVG` | N/A | Numeric `0.00` average team height in metres | In `_UPDATE` message only. |

## Message Sort

Sort by `Team/@Code`.

## Sample from the Dictionary

The PDF prints a single generic team sample. It is incomplete (missing `ShortName`, `TVTeamName`, `TeamType`,
`Current="true"` is included but several mandatory attributes are not). Reproduced for reference only; do not
treat as canonical:

```xml
<Team Code="BKBMTEAM5-----CAN01" Organisation="CAN" Number="1" Name="Canada" Gender="M" Current="true">
  <Composition>
    <Athlete Code="1063192" Order="1"/>
    <Athlete Code="1063249" Order="2"/>
    <!-- ... -->
  </Composition>
  <TeamOfficials>
    <Official Code="7380750" Function="COACH"/>
    <Official Code="7380751" Function="AST_COA"/>
    <Official Code="7380752" Function="AST_COA"/>
  </TeamOfficials>
  <Discipline Code="BKB-------------------------------">
    <RegisteredEvent Event="BKBMTEAM5-------------------------">
      <EventEntry Type="ENTRY" Code="UNIFORM" Pos="1" Value="White"/>
      <EventEntry Type="ENTRY" Code="UNIFORM" Pos="2" Value="Red"/>
    </RegisteredEvent>
  </Discipline>
</Team>
```

## XML Example (Not Validated - No XSD Supplied)

A normalized `DT_PARTIC_TEAMS` payload, including all mandatory `Team` attributes and a richer
`EventEntry` set. Not validated against an XSD because no schema was supplied for this extraction.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="BKB-------------------------------"
         DocumentType="DT_PARTIC_TEAMS"
         Version="1"
         FeedFlag="P"
         Date="2024-07-25"
         Time="120000000"
         LogicalDate="2024-07-25"
         Source="BCYBKB1">
  <Competition Gen="3.4" Sport="3.4" Codes="3.4">
    <Team Code="BKBMTEAM5-----CAN01"
          Organisation="CAN"
          Number="1"
          Name="Canada"
          ShortName="Canada"
          TVTeamName="Canada"
          Gender="M"
          Current="true"
          TeamType="ORG">
      <Composition>
        <Athlete Code="1063192" Order="1"/>
        <Athlete Code="1063249" Order="2"/>
      </Composition>
      <TeamOfficials>
        <Official Code="7380750" Function="COACH" Order="1"/>
        <Official Code="7380751" Function="AST_COA" Order="2"/>
      </TeamOfficials>
      <Discipline Code="BKB-------------------------------" IFId="FIBA-CAN-001">
        <RegisteredEvent Event="BKBMTEAM5-------------------------">
          <EventEntry Type="ENTRY" Code="UNIFORM" Pos="1" Value="White"/>
          <EventEntry Type="ENTRY" Code="UNIFORM" Pos="2" Value="Red"/>
          <EventEntry Type="ENTRY" Code="GROUP" Value="A"/>
          <EventEntry Type="ENTRY" Code="DRAW" Value="3"/>
          <EventEntry Type="ENTRY" Code="RANK_WLD" Value="6"/>
          <EventEntry Type="ENTRY" Code="OG_PLAYED" Value="40"/>
          <EventEntry Type="ENTRY" Code="OG_WIN" Value="22"/>
          <EventEntry Type="ENTRY" Code="OG_LOST" Value="18"/>
        </RegisteredEvent>
      </Discipline>
    </Team>
  </Competition>
</OdfBody>
```

An `_UPDATE` payload uses `DocumentType="DT_PARTIC_TEAMS_UPDATE"` and a mandatory `ModificationIndicator`
on each modified team:

```xml
<Team Code="BKBMTEAM5-----CAN01"
      Organisation="CAN"
      Number="1"
      Name="Canada"
      ShortName="Canada"
      TVTeamName="Canada"
      Gender="M"
      Current="true"
      TeamType="ORG"
      ModificationIndicator="U">
  <!-- only the team data being modified -->
</Team>
```

## Modeling Notes

- Treat `DT_PARTIC_TEAMS` as team master data scoped by discipline, not by event unit. Each team has exactly
  one discipline and at most one registered event (current teams) or none (historical teams).
- Use `Team/@Code` as the stable join key from `DT_PARTIC` (athletes), entries, results, and play-by-play
  payloads. Historical team codes start with `T`.
- `DT_PARTIC_TEAMS` is a full replace: a new emission resets the previous team set for the discipline.
- `DT_PARTIC_TEAMS_UPDATE` is a row-level CRUD diff keyed by `Team/@Code` and driven by
  `Team/@ModificationIndicator`. Map `N` -> insert, `U` -> upsert, `D` -> delete. On `D`, the `Discipline`
  element is not expected; on `N` and `U`, treat the received `Team` as the complete record for that team.
- The roster (`Composition/Athlete`) is sent as a complete list for that team inside any `N` or `U` update,
  not as athlete-level diffs - replace the entire roster when an update arrives.
- `TeamOfficials` is optional and only used for team-specific officials; do not derive discipline-wide officials
  from this element.
- `EventEntry` rows are path-sensitive: the same `Code` values (`UNIFORM`, `DRAW`, `GROUP`, etc.) have meaning
  only under `RegisteredEvent`. `Pos` is part of the natural key for multi-row entries such as `UNIFORM`
  (`Pos="1"` light, `Pos="2"` dark).
- `HEIGHT_AVG` is the only `EventEntry` code restricted to `_UPDATE` messages; in domain terms, treat it as a
  derived metric refreshed post-load, not as a registration-time attribute.
- The PDF refers to `CC@ResultsFunction` for `Official/@Function`, but the actual code page linked from the
  PDF is `DisciplineFunction.htm`. Model the official-function vocabulary against the `DisciplineFunction`
  table filtered by `Discipline="BKB"`.

## Code Appendix: SC and CC Values

| Code | Type | Source | Notes |
|---|---|---|---|
| `CC@Competition` | CC | [CompetitionCode.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) | 11 rows. Linked only. Includes `OG2024` and variants. |
| `CC@Discipline` | CC | [Discipline.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Discipline.htm) | 60 rows total. BKB row embedded below. |
| `CC@Organisation` | CC | [Organisation.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) | 259 rows (NOCs and related entities). Linked only - large master table. |
| `CC@Event` | CC | [Event.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Event.htm) | 621 rows total. BKB rows embedded below. |
| `CC@SportGender` | CC | [SportGender.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm) | Embedded below. |
| `CC@ResultsFunction` / `DisciplineFunction` | CC | [DisciplineFunction.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineFunction.htm) | 438 rows total. BKB rows embedded below. |
| `SC@Source` (BKB) | SC | [odf_sc_Source_SOG_BKB.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm) | Embedded below. |
| `SC@TeamType` (BKB) | SC | [odf_sc_TeamType_SOG_BKB.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_TeamType_SOG_BKB.htm) | Embedded below. |

### CC @SportGender

| Id | ENG Description |
|---|---|
| `-` | Global |
| `M` | Men |
| `O` | Open |
| `W` | Women |
| `X` | Mixed |

### CC @Discipline (BKB row)

| Code | Id | Sport | EventOrder | Scheduled | IF | ENG Description |
|---|---|---|---|---|---|---|
| `BKB-------------------------------` | `BKB` | `BK` | `DATE` | `Y` | `FIBA` | Basketball |

### CC @Event (BKB rows)

| Code | Discipline | Gender | Event | Order | TeamEvent | SEQ | ENG Description |
|---|---|---|---|---|---|---|---|
| `BKB-------------------------------` | `BKB` | `-` | `------------------` |  | `N` | `N` | Basketball |
| `BKBM------------------------------` | `BKB` | `M` | `------------------` |  | `N` | `N` | Men |
| `BKBMTEAM5-------------------------` | `BKB` | `M` | `TEAM5-------------` | 1 | `Y` | `Y` | Men |
| `BKBW------------------------------` | `BKB` | `W` | `------------------` |  | `N` | `N` | Women |
| `BKBWTEAM5-------------------------` | `BKB` | `W` | `TEAM5-------------` | 2 | `Y` | `Y` | Women |

### CC @DisciplineFunction (BKB rows)

Used by `Team/TeamOfficials/Official/@Function`. The PDF labels the attribute `CC@ResultsFunction`,
but the PDF link resolves to the `DisciplineFunction` table.

| Function | Discipline | Order | Category | Partic | ENG Description |
|---|---|---:|---|---|---|
| `AA01` | BKB | 0 | A | Y | Athlete |
| `COACH` | BKB | 1 | C | Y | Coach |
| `AST_COA` | BKB | 2 | C | Y | Assistant Coach |
| `TCH_DEL` | BKB | 3 | S | Y | IF Delegate |
| `JUR_ME` | BKB | 4 | J | Y | Jury Member |
| `CMM` | BKB | 5 | S | Y | Commissioner |
| `RE` | BKB | 6 | J | Y | Referee |
| `CHF` | BKB | 7 | J | Y | Crew Chief |
| `UM` | BKB | 8 | S | Y | Umpire |
| `RR` | BKB | 9 | J | Y | Standby Referee |

### SC @Source (SOG / BKB)

| Discipline | Code_Entity | Code | ENG Description |
|---|---|---|---|
| BKB | `@Source` | `BCYBKB1` | Origin for messages from OVR at BCY for BKB |
| BKB | `@Source` | `LILBKB1` | Origin for messages from OVR at LIL for BKB |

### SC @TeamType (SOG / BKB)

| Discipline | Code_Entity | Code | Note | ENG Description |
|---|---|---|---|---|
| BKB | `@TeamType` | `ORG` | Organisation name (NOC or NPC) | Organisation |

### CC @Organisation

259 NOC and related-entity rows. Linked only:
[Organisation.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm).

### CC @Competition

11 rows. Linked only:
[CompetitionCode.htm](http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm).
Includes `OG2024` (Olympic Games Paris 2024) plus variant codes such as `OG2024-HT`, `OG2024-ITL`,
`OG2024-MST1`.

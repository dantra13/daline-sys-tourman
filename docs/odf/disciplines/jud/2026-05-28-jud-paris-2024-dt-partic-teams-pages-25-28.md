# ODF JUD Data Dictionary: Paris 2024 DT_PARTIC_TEAMS / DT_PARTIC_TEAMS_UPDATE, Pages 25-28

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 25-28.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note restructures the Paris 2024 Judo team participant messages. This section is central for modeling Judo team
events because it defines team identity, team composition, and team event registration.

## 2.3.3 List of teams / List of teams update

`DT_PARTIC_TEAMS` is a complete discipline-level team snapshot. `DT_PARTIC_TEAMS_UPDATE` contains only modified teams.
Every team in the bulk list is assumed valid for current or possible event participation.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Discipline` | Full discipline RSC. |
| `DocumentType` | `DT_PARTIC_TEAMS` / `DT_PARTIC_TEAMS_UPDATE` | Bulk team list or team update. |
| `DocumentSubtype` | `S(20)` | `SYNC` after venue control transfer or `HISTORICAL` for historical-provider teams; neither appears on update messages. |
| `Version` | `1..V` | Ascending content version. |
| `FeedFlag` | `P`, `T` | Production or test. |
| `Date` / `Time` / `LogicalDate` | Date/time | Header generation and logical dates. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Message | Trigger |
|---|---|
| `DT_PARTIC_TEAMS` | Bulk message before the Games, repeated until transfer of control to OVR. |
| `DT_PARTIC_TEAMS_UPDATE` | After OVR transfer, whenever any team data changes. |

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
        +-- Discipline (0,1)
            +-- @Code
            +-- RegisteredEvent (0,1)
                +-- @Event
                +-- EventEntry (0,N)
                    +-- @Type
                    +-- @Code
                    +-- @Pos
                    +-- @Value
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General dictionary version. |
| `Sport` | O | `S(20)` | Sport dictionary version. |
| `Codes` | O | `S(20)` | Codes version. |

### `Competition / Team`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` | Team ID. Historical team IDs start with `T`. |
| `Organisation` | M | `CC@Organisation` | Team organisation. |
| `Number` | O | `#0` | Team number for an organisation; required for current teams. |
| `Name` | M | `S(73)` | Team name. |
| `ShortName` | M | `S(40)` | Short team name. |
| `TVTeamName` | M | `S(21)` | Team TV name. |
| `Gender` | M | `CC@SportGender` | Team gender. |
| `Current` | M | Boolean | Current Games team vs historical team. |
| `TeamType` | M | `SC@TeamType` | Team naming construction; Judo says use `ORG`. |
| `ModificationIndicator` | M in update | `N`, `U`, `D` | New, update, or delete in `DT_PARTIC_TEAMS_UPDATE`. |

### `Team / Composition / Athlete`

For current teams, the number of athletes is two or more.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` | Athlete ID for the team member. |
| `Order` | O | Numeric | Team-member order. |

### `Team / Discipline`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `CC@Discipline` | Full discipline RSC. Expected unless deleting the team. |

### `RegisteredEvent`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Event` | M | `CC@Event` | Full event RSC. Historical teams are not registered to events. |

### `EventEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ENTRY` | `RANK_WLD` | N/A | As soon as known. | `S(3)` | Team world ranking. |

## Sample, Normalized

```xml
<Competition Gen="SOG-2024-GEN" Sport="SOG-2024-JUD-3.3 APP" Codes="SOG-2024">
  <Team Code="JUDXTEAM6--FRA01" Organisation="FRA" Number="1" Name="France"
        ShortName="France" TVTeamName="France" Gender="X" Current="true" TeamType="ORG">
    <Composition>
      <Athlete Code="5285271" Order="1"/>
      <Athlete Code="5285269" Order="2"/>
    </Composition>
    <Discipline Code="JUD">
      <RegisteredEvent Event="JUDXTEAM6--------------------------">
        <EventEntry Type="ENTRY" Code="RANK_WLD" Value="001"/>
      </RegisteredEvent>
    </Discipline>
  </Team>
</Competition>
```

## Message Sort

Sort by `Team/@Code`.

## Modeling Notes

- This is the team roster source. Team-match `DT_RESULT` links team competitors and athlete contest results back to
  these team and athlete IDs.
- Teams may initially be distributed without members in timing workflows; do not require `Composition` in every team
  record.
- Historical teams are valid identity records but are not registered to events.
- `ModificationIndicator="D"` deletes a team from the previously loaded bulk list.

## Code Appendix: Values Visible in Pages 25-28

| Code Entity | Section Usage | Visible Values |
|---|---|---|
| `DocumentType` | Header | `DT_PARTIC_TEAMS`, `DT_PARTIC_TEAMS_UPDATE` |
| `DocumentSubtype` | Header | `SYNC`, `HISTORICAL` |
| `ModificationIndicator` | Updates | `N`, `U`, `D` |
| `TeamType` | Team naming | `ORG` |
| `EventEntry/@Code` | Team event extensions | `RANK_WLD` |

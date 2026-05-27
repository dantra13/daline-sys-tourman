
# ODF FBL Data Dictionary: DT_STATS, Pages 71-84

Source: `C:\Users\mella\Downloads\ODF_FBL_Data_Dictionary.pdf`, pages 71-84.

This note restructures the football `DT_STATS` section into readable Markdown for domain modeling. It covers the statistics message variants (`CUM`, `IND_RANKING`, `TOU`), header metadata, progress metadata, global tournament statistics, cumulative team statistics, athlete cumulative/ranking statistics, XSD-aligned XML examples, and an appendix of English SC/CC values downloaded from the Paris 2024 ODF code pages.

## 2.3.9 Statistics

`DT_STATS` contains a list of statistics for a competitor, either an individual athlete or a team. The statistics apply at `DocumentCode` level, which can represent an event unit, a phase, or an event.

Football defines separate statistics messages using the header's `DocumentSubtype` and, for some cases, `DocumentSubcode`. Every statistics table with multiple applicable statistics gets a separate message identity.

## Message Variants

| `DocumentSubtype` | Meaning | `DocumentSubcode` | Main payload shape |
|---|---|---|---|
| `CUM` | Cumulative team and individual player statistics. | Team ID. There is one single message for each team. | One team `Stats/Competitor`, with team `StatsItems` and player stats under `Composition/Athlete`. |
| `IND_RANKING` | Ranking of individual tournament statistics for the best athletes. | Not used by this section. | Athlete ranking stats, using `Rank`, `RankEqual`, and `SortOrder` on athlete `StatsItem`. |
| `TOU` | Tournament statistics, such as tournament totals or disciplinary totals. | Not used by this section. | Global `Stats/StatsItems` under `Stats Code="TOU"`. |

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Event` | Full RSC of the event. |
| `DocumentSubcode` | `S(20)` | Team ID when the message is for a single team. Used only when `DocumentSubtype="CUM"`. |
| `DocumentType` | `DT_STATS` | Statistics message. |
| `DocumentSubtype` | `CUM`, `IND_RANKING`, `TOU` | Statistics table type. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `ResultStatus` | `CC @ResultStatus` | Statistics lifecycle status. Football uses `LIVE`, `INTERMEDIATE`, `OFFICIAL`, and `PROVISIONAL` in this section. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time, up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day. Usually the physical day, except when the unit or message transmission crosses midnight. |
| `Source` | `SC @Source` | System that generated the message. |

## Trigger and Frequency

Send after each match only.

## Message Structure

```text
OdfBody
└─ Competition (0,1)
   ├─ @Gen
   ├─ @Sport
   ├─ @Codes
   ├─ ExtendedInfos (0,1)
   │  ├─ Progress (0,1)
   │  │  ├─ @LastUnit
   │  │  ├─ @UnitsTotal
   │  │  └─ @UnitsComplete
   │  └─ SportDescription (0,1)
   │     ├─ @DisciplineName
   │     ├─ @EventName
   │     └─ @Gender
   └─ Stats (1,1)
      ├─ @Code
      ├─ StatsItems (0,1)
      │  └─ StatsItem (1,N)
      │     ├─ @Type
      │     ├─ @Code
      │     ├─ @Pos
      │     ├─ @Value
      │     ├─ @Attempt
      │     └─ @Avg
      └─ Competitor (0,N)
         ├─ @Code
         ├─ @Type
         ├─ @Order
         ├─ @Organisation
         ├─ Description (0,1)
         │  └─ @TeamName
         ├─ StatsItems (0,1)
         │  └─ StatsItem (1,N)
         │     ├─ @Type
         │     ├─ @Code
         │     ├─ @Pos
         │     ├─ @Value
         │     ├─ @Attempt
         │     └─ @Avg
         └─ Composition (0,1)
            └─ Athlete (1,N)
               ├─ @Code
               ├─ @Order
               ├─ Description (1,1)
               │  ├─ @GivenName
               │  ├─ @FamilyName
               │  ├─ @Gender
               │  ├─ @Organisation
               │  ├─ @BirthDate
               │  ├─ @IFId
               │  └─ @Class
               └─ StatsItems (0,1)
                  └─ StatsItem (1,N)
                     ├─ @Type
                     ├─ @Code
                     ├─ @Pos
                     ├─ @Value
                     ├─ @Attempt
                     ├─ @Avg
                     ├─ @Rank
                     ├─ @RankEqual
                     └─ @SortOrder
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional in dictionary, required by XSD | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional in dictionary, required by XSD | `S(20)` | Code-set version applicable to the message. |

## ExtendedInfos / Progress

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `LastUnit` | Optional | `CC @Unit` | RSC of the most recent unit made official. For `CUM`, which includes one team, this is the last unit for that team. Send after at least one unit is complete in `CUM`, `IND_RANKING`, and `TEAM_RANKING` messages. |
| `UnitsTotal` | Optional | Numeric `##0` | Total number of units/games to be played. For `CUM`, this is the total for the team. Send in `CUM`, `IND_RANKING`, and `TEAM_RANKING` messages. |
| `UnitsComplete` | Optional | Numeric `##0` | Total number of official units/games. For `CUM`, this is the total complete for the team. Send in `CUM`, `IND_RANKING`, and `TEAM_RANKING` messages. |

## SportDescription

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `DisciplineName` | Mandatory | `S(40)` | English discipline description from Common Codes, not the code itself. |
| `EventName` | Mandatory | `S(40)` | English event description from Common Codes. |
| `Gender` | Mandatory | `CC @SportGender` | Gender code for the event. |

## Stats

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `SC @Statistics` | Statistics table code. It must be the same as the header `DocumentSubtype`. |

## Stats / StatsItems / StatsItem: Tournament Statistics (`TOU`)

These are global tournament statistics at event level. They appear under `Stats/StatsItems` when `DocumentSubtype="TOU"` and `DocumentCode` is the event.

| Type | Code | Pos | Expected When | Value | Attempt | Avg | Meaning |
|---|---|---|---|---|---|---|---|
| `ATTENDANCE` | `DATE` | Date | `TOU` at event level. | Numeric `#####0` | N/A | N/A | Total attendance of all matches for the specified date. |
| `ATTENDANCE` | `RSC` | Full RSC at discipline level, `S(34)` | `TOU` at event level. | Numeric `#######0` | N/A | N/A | Total attendance indicated by the RSC in `Pos`. |
| `ST` | `MP` | N/A | Always, if available for `TOU`. | Numeric `#0` | N/A | N/A | Total matches played for all teams. |
| `ST` | `GF` | N/A | Always, if available for `TOU`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | Total goals for all teams; average goals for all teams in `Avg`. |
| `ST` | `GA` | N/A | Always, if available for `TOU`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | Total goals against for all teams; average goals against in `Avg`. |
| `ST` | `PTY` | Numeric `0` | Always, if available for `TOU`. | Numeric `##0` | Numeric `##0`, optional | N/A | Total penalty goals in `Value`; total penalty kicks in `Attempt`. `Pos=0` for team statistics, `Pos=1` for opponent statistics. |
| `ST` | `PTY_AVG` | Numeric `0` | Always, if available for `TOU`. | Numeric `##0.0` | Numeric `##0.0`, optional | N/A | Average penalty goals in `Value`; average penalty kicks in `Attempt`. `Pos=0` for team statistics, `Pos=1` for opponent statistics. |
| `ST` | `2PTY` | Numeric `0` | Always, if available for `TOU` in Paralympic Games and futsal. | Numeric `##0` | Numeric `##0`, optional | N/A | Total double penalty goals in `Value`; total double penalty kicks in `Attempt`. |
| `ST` | `2PTY_AVG` | Numeric `0` | Always, if available for `TOU` in Paralympic Games and futsal. | Numeric `##0.0` | Numeric `##0.0`, optional | N/A | Average double penalty goals in `Value`; average double penalty kicks in `Attempt`. |
| `ST` | `SHOT` | N/A | Always, if available for `TOU`. | Numeric `##0` | Numeric `##0`, optional | N/A | Total shots on goal in `Value`; total shots in `Attempt`. |
| `ST` | `SHOT_AVG` | N/A | Always, if available for `TOU`. | Numeric `##0.0` | Numeric `##0.0`, optional | N/A | Average shots on goal in `Value`; average shots in `Attempt`. |
| `ST` | `CRN` | N/A | Always, if available for `TOU`. | Numeric `##0` | N/A | Numeric `##0.0`, optional | Total corner kicks; average in `Avg`. |
| `ST` | `OFF` | N/A | Always, if available for `TOU`; not applicable in Paralympic Games. | Numeric `##0` | N/A | Numeric `##0.0`, optional | Total offsides; average in `Avg`. |
| `ST` | `FOC` | N/A | Always, if available for `TOU`. | Numeric `##0` | N/A | Numeric `##0.0`, optional | Total fouls committed; average in `Avg`. |
| `ST` | `YC` | N/A | Always, if available for `TOU`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | Total yellow cards; average in `Avg`. |
| `ST` | `YRC` | N/A | Always, if available for `TOU`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | Total second-yellow red cards; average in `Avg`. |
| `ST` | `RC` | N/A | Always, if available for `TOU`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | Total direct red cards; average in `Avg`. |
| `ST` | `EXP` | N/A | Always, if available for `TOU`. | Numeric `#0` | Numeric `#0.0`, optional | N/A | Total expulsions as second-yellow red cards plus direct red cards. The dictionary uses `Attempt` for the average expulsions value. |

```xml
<Stats Code="TOU">
  <StatsItems>
    <StatsItem Type="ST" Code="MP" Value="16"/>
    <StatsItem Type="ST" Code="GF" Value="37" Avg="2.3"/>
    <StatsItem Type="ST" Code="GA" Value="37" Avg="2.3"/>
    <StatsItem Type="ST" Code="SHOT" Attempt="418" Value="150"/>
    <StatsItem Type="ST" Code="SHOT_AVG" Attempt="26.1" Value="9.4"/>
    <StatsItem Type="ST" Code="PTY" Attempt="3" Value="3"/>
    <StatsItem Type="ST" Code="PTY_AVG" Attempt="0.2" Value="0.2"/>
    <StatsItem Type="ST" Code="CRN" Value="159" Avg="9.9"/>
    <StatsItem Type="ST" Code="OFF" Value="50" Avg="3.1"/>
  </StatsItems>
</Stats>
```

## Stats / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Competitor ID assigned to the statistic set. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Order` | Mandatory | Numeric `##0` | Sort order. For each team: team NOC code; disqualified teams sort to the bottom. |
| `Organisation` | Optional | `CC @Organisation` | Competitor organisation, if known. |

## Competitor / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `TeamName` | Mandatory | `S(73)` | Team name. |

## Competitor / StatsItems / StatsItem: Team Cumulative Statistics (`CUM`)

| Type | Code | Pos | Expected When | Value | Attempt | Avg | Meaning |
|---|---|---|---|---|---|---|---|
| `ST` | `MP` | N/A | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Total matches played. |
| `ST` | `GF` | N/A | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Total goals for in all games played by the team. |
| `ST` | `GA` | N/A | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Total goals against in all games played by the team. |
| `ST` | `PTY` | Numeric `0` | Always, if available for `CUM`. | Numeric `##0` | Numeric `##0`, optional | N/A | Penalty goals / penalty goals against in `Value`; penalty kicks / penalty kicks against in `Attempt`. `Pos=0` for team stats, `Pos=1` for opponent stats. |
| `ST` | `2PTY` | Numeric `0` | Always, if available for `CUM` in Paralympic Games and futsal. | Numeric `##0` | Numeric `##0`, optional | N/A | Double penalty goals / against in `Value`; double penalty kicks / against in `Attempt`. |
| `ST` | `SHOT` | N/A | Always, if available for `CUM`. | Numeric `##0` | Numeric `##0`, optional | N/A | Shots on goal in `Value`; total shots in `Attempt`. |
| `ST` | `CRN` | N/A | Always, if available for `CUM`. | Numeric `##0` | N/A | N/A | Corner kicks for the team. |
| `ST` | `ASSIST` | N/A | Always, if available for `CUM`; not applicable in Paralympics. | Numeric `##0` | N/A | N/A | Total assists for the team. |
| `ST` | `OFF` | N/A | Always, if available for `CUM`; not required in Paralympics. | Numeric `##0` | N/A | N/A | Offsides for the team. |
| `ST` | `FOC` | N/A | Always, if available for `CUM`. | Numeric `##0` | N/A | Numeric `##0.0`, optional | Fouls committed; average in `Avg`. |
| `ST` | `FOS` | N/A | Always, if available for `CUM`. | Numeric `##0.0` | N/A | Numeric `##0.0`, optional | Fouls suffered; average in `Avg`. |
| `ST` | `YC` | N/A | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Yellow cards. |
| `ST` | `YRC` | N/A | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Second-yellow red-card expulsions. |
| `ST` | `RC` | N/A | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Direct red-card expulsions. |
| `ST` | `EXP` | N/A | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Total expulsions, as second-yellow red cards plus direct red cards. |
| `ST` | `FRK` | N/A | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Free kicks for the team. |
| `ST` | `OG` | Numeric `0` | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | Own goals for the team. `Pos=0` for team statistics, `Pos=1` for opponent statistics. |

```xml
<StatsItems>
  <StatsItem Type="ST" Code="MP" Value="2"/>
  <StatsItem Type="ST" Code="GF" Value="1"/>
  <StatsItem Type="ST" Code="GA" Value="2"/>
  <StatsItem Type="ST" Code="SHOT" Attempt="11" Value="5"/>
  <StatsItem Type="ST" Code="CRN" Value="4"/>
  <StatsItem Type="ST" Code="OFF" Value="7"/>
  <StatsItem Type="ST" Code="FOC" Value="21" Avg="10.5"/>
  <StatsItem Type="ST" Code="FOS" Value="20" Avg="10.0"/>
  <StatsItem Type="ST" Code="YC" Value="3"/>
  <StatsItem Type="ST" Code="FRK" Value="23"/>
</StatsItems>
```

## Competitor / Composition / Athlete

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Athlete ID, corresponding to a team member or individual athlete. |
| `Order` | Mandatory | Numeric `##0` | Sort order. For `CUM`: shirt number or disqualification. For `IND_RANKING`: rank or disqualification, then name. Disqualified players sort to the bottom. |

## Athlete / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Athlete gender. |
| `Organisation` | Mandatory | `CC @Organisation` | Athlete organisation. |
| `BirthDate` | Optional | `Date` | Birth date, `YYYY-MM-DD`; must be included when available. |
| `IFId` | Optional | `S(16)` | International Federation ID. |
| `Class` | Optional | `CC @DisciplineClass` | Sport class for events involving athletes with a disability. |

## Athlete / StatsItems / StatsItem: Player Statistics (`CUM`, `IND_RANKING`)

| Type | Code | Expected When | Value | Attempt | Avg | Ranking fields | Meaning |
|---|---|---|---|---|---|---|---|
| `ST` | `MINS` | Always, if available for `CUM` and `IND_RANKING`. | `mmm` | N/A | N/A | N/A | Total minutes played in all games. Remove leading zeros. |
| `ST` | `MP` | Always, if available for `CUM` and `IND_RANKING`. | Numeric `#0` | N/A | N/A | N/A | Total matches played by the athlete. |
| `ST` | `GF` | Always, if available for `CUM` and `IND_RANKING`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | `Rank`, `RankEqual`, `SortOrder` optional | Total goals by player; average goals per match in `Avg`. |
| `ST` | `GA` | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | N/A | Goals against while the player played as goalkeeper. |
| `ST` | `OG` | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | N/A | Own goals by the player. |
| `ST` | `SHOT` | Always, if available for `CUM` and `IND_RANKING`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | `Rank`, `RankEqual`, `SortOrder` optional | Total shots; average shots per match in `Avg`. Do not send rank if competitor was disqualified. |
| `ST` | `SHOT_ON_GOAL` | Always, if available for `CUM` and `IND_RANKING`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | `Rank`, `RankEqual`, `SortOrder` optional | Shots on goal. Send even when `0` if `SHOT > 0`. |
| `ST` | `PTY` | Always, if available for `CUM`. | Numeric `#0` | Numeric `#0`, optional | N/A | N/A | Penalty goals in `Value`; penalty kicks in `Attempt`. |
| `ST` | `2PTY` | Always, if available for `CUM` in Paralympic Games and futsal. | Numeric `#0` | Numeric `#0`, optional | N/A | N/A | Double penalty goals in `Value`; double penalty kicks in `Attempt`. |
| `ST` | `ASSIST` | Always, if available for `CUM` and `IND_RANKING`; not applicable in Paralympics. | Numeric `#0` | N/A | N/A | N/A | Assists by player. |
| `ST` | `FOC` | Always, if available for `CUM` and `IND_RANKING`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | `Rank`, `RankEqual`, `SortOrder` optional | Fouls committed; average in `Avg`. |
| `ST` | `FOS` | Always, if available for `CUM` and `IND_RANKING`. | Numeric `#0` | N/A | Numeric `#0.0`, optional | `Rank`, `RankEqual`, `SortOrder` optional | Fouls suffered; average in `Avg`. |
| `ST` | `YC` | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | N/A | Yellow cards. |
| `ST` | `YRC` | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | N/A | Second-yellow red-card expulsions. |
| `ST` | `RC` | Always, if available for `CUM`. | Numeric `#0` | N/A | N/A | N/A | Direct red-card expulsions. |

```xml
<StatsItems>
  <StatsItem Type="ST" Code="MP" Value="2"/>
  <StatsItem Type="ST" Code="GF" Value="1"/>
  <StatsItem Type="ST" Code="SHOT_ON_GOAL" Value="7"/>
  <StatsItem Type="ST" Code="FOC" Value="3"/>
  <StatsItem Type="ST" Code="MINS" Value="180"/>
</StatsItems>
```

## Message Sort

Sort according to the `@Order` attributes.

## XSD-Aligned XML Example

The provided XSD draft could not be loaded directly because `odf2-structure.xsd` references `RecordBrokenType`, which is not defined in the supplied schema folder. The example below was validated against a temporary copy of the schema where only that unrelated unresolved type reference was replaced so the schema could parse. No original XSD files were modified.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="FBLWTEAM11------------------------"
         DocumentSubcode="FBLWTEAM11-----RSA01"
         DocumentType="DT_STATS"
         DocumentSubtype="CUM"
         Version="4"
         ResultStatus="INTERMEDIATE"
         FeedFlag="T"
         Date="2026-05-25"
         Time="21:00:00.000"
         LogicalDate="2026-05-25"
         Source="OVR">
  <Competition Gen="3.4" Sport="FBL-3.4" Codes="SOG-2024">
    <ExtendedInfos>
      <Progress LastUnit="FBLWTEAM11------------GPA-000100--" UnitsTotal="3" UnitsComplete="1"/>
      <SportDescription DisciplineName="Football" EventName="Women" Gender="W"/>
    </ExtendedInfos>
    <Stats Code="CUM">
      <Competitor Code="FBLWTEAM11-----RSA01" Type="T" Order="1" Organisation="RSA">
        <Description TeamName="South Africa"/>
        <StatsItems>
          <StatsItem Type="ST" Code="MP" Value="1"/>
          <StatsItem Type="ST" Code="GF" Value="1"/>
          <StatsItem Type="ST" Code="GA" Value="2"/>
          <StatsItem Type="ST" Code="SHOT" Value="5" Attempt="11"/>
          <StatsItem Type="ST" Code="FOC" Value="10" Avg="10.0"/>
        </StatsItems>
        <Composition>
          <Athlete Code="1106655" Order="9" Bib="9">
            <Description GivenName="Jane"
                         FamilyName="Smith"
                         Gender="F"
                         Organisation="RSA"
                         BirthDate="1993-05-12"/>
            <StatsItems>
              <StatsItem Type="ST" Code="MP" Value="1"/>
              <StatsItem Type="ST" Code="MINS" Value="90"/>
              <StatsItem Type="ST" Code="GF" Value="1" Avg="1.0" Rank="1" SortOrder="1"/>
              <StatsItem Type="ST" Code="SHOT_ON_GOAL" Value="2" Avg="2.0" Rank="3" SortOrder="3"/>
            </StatsItems>
          </Athlete>
        </Composition>
      </Competitor>
    </Stats>
  </Competition>
</OdfBody>
```

## Modeling Notes

- `DT_STATS` is not a live event stream. It is an aggregate/statistical view emitted after each match.
- `Stats/@Code` and header `DocumentSubtype` should be treated as the same semantic discriminator. Validate that they match.
- `CUM` is team-scoped through `DocumentSubcode`; model it as one team's cumulative rollup plus player rollups, not as a whole-tournament table.
- `TOU` is tournament-scoped and usually uses only global `Stats/StatsItems`, not team/player composition.
- `IND_RANKING` uses the same athlete `StatsItem` shape but ranking fields (`Rank`, `RankEqual`, `SortOrder`) become central. Keep ranking metadata attached to each statistic, not only to the athlete.
- The same stat code can mean different aggregation levels depending on path: global `StatsItems`, team `Competitor/StatsItems`, or athlete `Composition/Athlete/StatsItems`. Include the path/scope in internal stat definitions.
- `Attempt`, `Avg`, `Rank`, `RankEqual`, and `SortOrder` are not generic extras; their meaning depends on the stat code. For example `SHOT` uses `Attempt` as total shots while `Value` is shots on goal in some team/global contexts.
- Preserve `Progress` as aggregate calculation context. It tells consumers how complete the statistic set is relative to official units.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-25. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index
| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Statistics` | FBL | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Statistics_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Statistics_SOG_FBL.htm) |
| `SC @Source` | FBL | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @Event` | FBL Event rows | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Event.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Event.htm) |
| `CC @Unit` | FBL EventUnit rows | 93 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @ResultStatus` | Common | 9 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm) |
| `CC @SportGender` | Common | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm) |
| `CC @PersonGender` | Common | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm) |
| `CC @Organisation` | Common master data | 258 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |
| `CC @DisciplineClass` | Not found in Paris 2024 common-code index | 0 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm) |

### SC @Statistics
| Code | ENG Description |
| --- | --- |
| CUM | Cumulative Statistics of team and individual |
| IND_RANKING | Ranking of Individual tournament statistics |
| TOU | Ranking of Individual statistics |

### SC @Source
| Code | ENG Description |
| --- | --- |
| BORFBL1 | Origin for messages from OVR at BOR for FBL |
| LYOFBL1 | Origin for messages from OVR at LYO for FBL |
| MRSFBL1 | Origin for messages from OVR at MRS for FBL |
| NANFBL1 | Origin for messages from OVR at NAN for FBL |
| NICFBL1 | Origin for messages from OVR at NIC for FBL |
| PDPFBL1 | Origin for messages from OVR at PDP for FBL |
| STEFBL1 | Origin for messages from OVR at STE for FBL |

### CC @Competition
| Id | ENG Description |
| --- | --- |
| OG2024 | Olympic Games Paris 2024 |
| OG2024-HT | Olympic Games Paris 2024 - HT |
| OG2024-ITL | Olympic Games Paris 2024 - ITL |
| OG2024-MST1 | Olympic Games Paris 2024 - MST1 |
| OG2024-MST2 | Olympic Games Paris 2024 - MST2 |
| OG2024-MST3 | Olympic Games Paris 2024 - MST3 |
| OG2024-MST4 | Olympic Games Paris 2024 - MST4 |
| OG2024-TEV | Olympic Games Paris 2024 - TEV |
| OG2024-TR1 | Olympic Games Paris 2024 - TR1 |
| OG2024-TR2 | Olympic Games Paris 2024 - TR2 |

### CC @Event (FBL rows)
| Code | Gender | Event | order | Team Event | SEQ | ENG Description |
| --- | --- | --- | --- | --- | --- | --- |
| FBL------------------------------- | - | ------------------ |  | N | N | Football |
| FBLM------------------------------ | M | ------------------ |  | N | N | Men |
| FBLMTEAM11------------------------ | M | TEAM11------------ | 1 | Y | Y | Men |
| FBLW------------------------------ | W | ------------------ |  | N | N | Women |
| FBLWTEAM11------------------------ | W | TEAM11------------ | 2 | Y | Y | Women |

### CC @ResultStatus
| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| START_LIST | 1 | Before competition, Start List | Start List |
| LIVE | 2 | For live updates during competition | Live |
| INTERMEDIATE | 3 | When competition is stopped, usued at pre-defined points | Intermediate |
| UNCONFIRMED | 4 | When the unit is over but not yet unofficial or official | Unconfirmed |
| UNOFFICIAL | 5 | Results of the competition released as soon as the event is over, not waiting any official decision of the International Federation. The correctness of data must be assured | Unofficial |
| OFFICIAL | 6 | Results of the competition released as soon as the event is officially confirmed taking into account the resolution of the protests, etc. | Official |
| PARTIAL | 7 | Incomplete list, Final Ranking. Used in PDF | Partial |
| PROTESTED | 8 | After the competition is no longer LIVE and a protest has been lodged | Protested |
| PROVISIONAL | 9 | Special situations | Provisional |

### CC @SportGender
| Id | Description |
| --- | --- |
| - | Global |
| M | Men |
| O | Open |
| W | Women |
| X | Mixed |

### CC @PersonGender
| Id | ENG Description |
| --- | --- |
| F | Female |
| M | Male |
| X | Unspecified |

### CC @Unit (FBL EventUnit rows)
| Code | Gender | Event | phase | Eventunit | Level | Order | schedule | medalflag | ENG Description |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| FBL------------------------------- | - | ------------------ | ---- | -------- | Discipline | 2 | N | 0 | Football |
| FBL-----------------------BOR----- | - | ------------------ | ---- | BOR----- | Venue | 3 | N | 0 | BOR |
| FBL-----------------------LYO----- | - | ------------------ | ---- | LYO----- | Venue | 3 | N | 0 | LYO |
| FBLM------------------------------ | M | ------------------ | ---- | -------- | Gender | 10 | N | 0 | Men |
| FBL-----------------------MRS----- | - | ------------------ | ---- | MRS----- | Venue | 3 | N | 0 | MRS |
| FBLMTEAM11------------------------ | M | TEAM11------------ | ---- | -------- | Event | 13 | N | 0 | Men |
| FBLMTEAM11------------FNL--------- | M | TEAM11------------ | FNL- | -------- | Phase | 14 | N | 0 | Men's Finals |
| FBLMTEAM11------------FNL-000100-- | M | TEAM11------------ | FNL- | 000100-- | Unit | 15 | Y | 1 | Men's Gold Medal Match |
| FBLMTEAM11------------FNL-000200-- | M | TEAM11------------ | FNL- | 000200-- | Unit | 15 | Y | 3 | Men's Bronze Medal Match |
| FBLMTEAM11------------GP---------- | M | TEAM11------------ | GP-- | -------- | Phase | 14 | N | 0 | Men's Group Stage |
| FBLMTEAM11------------GPA--------- | M | TEAM11------------ | GPA- | -------- | Phase | 14 | N | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000100-- | M | TEAM11------------ | GPA- | 000100-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000200-- | M | TEAM11------------ | GPA- | 000200-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000300-- | M | TEAM11------------ | GPA- | 000300-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000400-- | M | TEAM11------------ | GPA- | 000400-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000500-- | M | TEAM11------------ | GPA- | 000500-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000600-- | M | TEAM11------------ | GPA- | 000600-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPB--------- | M | TEAM11------------ | GPB- | -------- | Phase | 14 | N | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000100-- | M | TEAM11------------ | GPB- | 000100-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000200-- | M | TEAM11------------ | GPB- | 000200-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000300-- | M | TEAM11------------ | GPB- | 000300-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000400-- | M | TEAM11------------ | GPB- | 000400-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000500-- | M | TEAM11------------ | GPB- | 000500-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000600-- | M | TEAM11------------ | GPB- | 000600-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPC--------- | M | TEAM11------------ | GPC- | -------- | Phase | 14 | N | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000100-- | M | TEAM11------------ | GPC- | 000100-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000200-- | M | TEAM11------------ | GPC- | 000200-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000300-- | M | TEAM11------------ | GPC- | 000300-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000400-- | M | TEAM11------------ | GPC- | 000400-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000500-- | M | TEAM11------------ | GPC- | 000500-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000600-- | M | TEAM11------------ | GPC- | 000600-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPD--------- | M | TEAM11------------ | GPD- | -------- | Phase | 14 | N | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000100-- | M | TEAM11------------ | GPD- | 000100-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000200-- | M | TEAM11------------ | GPD- | 000200-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000300-- | M | TEAM11------------ | GPD- | 000300-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000400-- | M | TEAM11------------ | GPD- | 000400-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000500-- | M | TEAM11------------ | GPD- | 000500-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000600-- | M | TEAM11------------ | GPD- | 000600-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------QFNL-------- | M | TEAM11------------ | QFNL | -------- | Phase | 14 | N | 0 | Men's Quarter-finals |
| FBLMTEAM11------------QFNL000100-- | M | TEAM11------------ | QFNL | 000100-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000200-- | M | TEAM11------------ | QFNL | 000200-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000300-- | M | TEAM11------------ | QFNL | 000300-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000400-- | M | TEAM11------------ | QFNL | 000400-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------SFNL-------- | M | TEAM11------------ | SFNL | -------- | Phase | 14 | N | 0 | Men's Semi-finals |
| FBLMTEAM11------------SFNL000100-- | M | TEAM11------------ | SFNL | 000100-- | Unit | 15 | Y | 0 | Men's Semi-final |
| FBLMTEAM11------------SFNL000200-- | M | TEAM11------------ | SFNL | 000200-- | Unit | 15 | Y | 0 | Men's Semi-final |
| FBLMTEAM11------------VICT-------- | M | TEAM11------------ | VICT | -------- | Phase | 14 | N | 0 | Men's Victory Ceremony |
| FBLMTEAM11------------VICTBRONZE-- | M | TEAM11------------ | VICT | BRONZE-- | Medals | 17 | Y | 0 | Men's Bronze Ceremony |
| FBLMTEAM11------------VICTMEDAL--- | M | TEAM11------------ | VICT | MEDAL--- | Medals | 17 | Y | 0 | Men's Victory Ceremony |
| FBL-----------------------NAN----- | - | ------------------ | ---- | NAN----- | Venue | 3 | N | 0 | NAN |
| FBL-----------------------NIC----- | - | ------------------ | ---- | NIC----- | Venue | 3 | N | 0 | NIC |
| FBL-----------------------PDP----- | - | ------------------ | ---- | PDP----- | Venue | 3 | N | 0 | PDP |
| FBL-----------------------STE----- | - | ------------------ | ---- | STE----- | Venue | 3 | N | 0 | STE |
| FBL-----------------------STR----- | - | ------------------ | ---- | STR----- | Venue | 3 | N | 0 | STR |
| FBL-----------------------TOU----- | - | ------------------ | ---- | TOU----- | Venue | 3 | N | 0 | TOU |
| FBLW------------------------------ | W | ------------------ | ---- | -------- | Gender | 10 | N | 0 | Women |
| FBLWTEAM11------------------------ | W | TEAM11------------ | ---- | -------- | Event | 13 | N | 0 | Women |
| FBLWTEAM11------------FNL--------- | W | TEAM11------------ | FNL- | -------- | Phase | 14 | N | 0 | Women's Finals |
| FBLWTEAM11------------FNL-000100-- | W | TEAM11------------ | FNL- | 000100-- | Unit | 15 | Y | 1 | Women's Gold Medal Match |
| FBLWTEAM11------------FNL-000200-- | W | TEAM11------------ | FNL- | 000200-- | Unit | 15 | Y | 3 | Women's Bronze Medal Match |
| FBLWTEAM11------------GP---------- | W | TEAM11------------ | GP-- | -------- | Phase | 14 | N | 0 | Women's Group Stage |
| FBLWTEAM11------------GPA--------- | W | TEAM11------------ | GPA- | -------- | Phase | 14 | N | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000100-- | W | TEAM11------------ | GPA- | 000100-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000200-- | W | TEAM11------------ | GPA- | 000200-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000300-- | W | TEAM11------------ | GPA- | 000300-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000400-- | W | TEAM11------------ | GPA- | 000400-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000500-- | W | TEAM11------------ | GPA- | 000500-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000600-- | W | TEAM11------------ | GPA- | 000600-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPB--------- | W | TEAM11------------ | GPB- | -------- | Phase | 14 | N | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000100-- | W | TEAM11------------ | GPB- | 000100-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000200-- | W | TEAM11------------ | GPB- | 000200-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000300-- | W | TEAM11------------ | GPB- | 000300-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000400-- | W | TEAM11------------ | GPB- | 000400-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000500-- | W | TEAM11------------ | GPB- | 000500-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000600-- | W | TEAM11------------ | GPB- | 000600-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPC--------- | W | TEAM11------------ | GPC- | -------- | Phase | 14 | N | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000100-- | W | TEAM11------------ | GPC- | 000100-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000200-- | W | TEAM11------------ | GPC- | 000200-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000300-- | W | TEAM11------------ | GPC- | 000300-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000400-- | W | TEAM11------------ | GPC- | 000400-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000500-- | W | TEAM11------------ | GPC- | 000500-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000600-- | W | TEAM11------------ | GPC- | 000600-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------QFNL-------- | W | TEAM11------------ | QFNL | -------- | Phase | 14 | N | 0 | Women's Quarter-finals |
| FBLWTEAM11------------QFNL000100-- | W | TEAM11------------ | QFNL | 000100-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000200-- | W | TEAM11------------ | QFNL | 000200-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000300-- | W | TEAM11------------ | QFNL | 000300-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000400-- | W | TEAM11------------ | QFNL | 000400-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------SFNL-------- | W | TEAM11------------ | SFNL | -------- | Phase | 14 | N | 0 | Women's Semi-finals |
| FBLWTEAM11------------SFNL000100-- | W | TEAM11------------ | SFNL | 000100-- | Unit | 15 | Y | 0 | Women's Semi-final |
| FBLWTEAM11------------SFNL000200-- | W | TEAM11------------ | SFNL | 000200-- | Unit | 15 | Y | 0 | Women's Semi-final |
| FBLWTEAM11------------VICT-------- | W | TEAM11------------ | VICT | -------- | Phase | 14 | N | 0 | Women's Victory Ceremony |
| FBLWTEAM11------------VICTBRONZE-- | W | TEAM11------------ | VICT | BRONZE-- | Medals | 17 | Y | 0 | Women's Bronze Ceremony |
| FBLWTEAM11------------VICTMEDAL--- | W | TEAM11------------ | VICT | MEDAL--- | Medals | 17 | Y | 0 | Women's Victory Ceremony |

### CC @Organisation

`CC @Organisation` is a large common master-data table rather than a statistics-code enumeration. The downloaded Paris 2024 code page contains 258 `Organisation` rows. Use the source link in the index above as the authoritative value list when modeling `Organisation` fields.

### CC @DisciplineClass

The football dictionary references `CC @DisciplineClass` for para sport classification, but the Paris 2024 common-code index downloaded for this appendix does not expose a `DisciplineClass.htm` page. Treat this as an externally supplied classification code set and validate it against the relevant para-sport dictionary/code release when that scope is implemented.

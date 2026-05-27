# ODF BDM Data Dictionary: DT_STATS, Pages 59-67

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BDM_Data_Dictionary.pdf`, pages 59-67.

Source version: `SYOG-2026-BDM-1.1 SFR`, dated 13 May 2026.

This note restructures the Badminton `DT_STATS` section into a practical reference for match analysis, tournament
statistics, and penalty statistics.

## 2.3.9 Statistics

`DT_STATS` contains statistics for competitors at the level identified by `OdfBody/@DocumentCode`. For Badminton this
can be an event unit, phase/event context, or discipline-level penalty rollup depending on `DocumentSubtype`.

The dictionary explicitly states that each table with multiple statistics is sent as a separate message, identified by
the header's `DocumentSubtype` and `DocumentSubcode`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@DisciplineGender`, `CC@EVENT`, or `CC@EVENT_UNIT` | RSC scope for the statistic table. |
| `DocumentSubcode` | N/A | Not used in the BDM section. |
| `DocumentType` | `DT_STATS` | Statistics message. |
| `DocumentSubtype` | `ANALYSIS`, `TOU`, `CUM` | Selects the statistics table family. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `INTERMEDIATE`, `OFFICIAL`, `PROVISIONAL` | `INTERMEDIATE` after each unit except the last; `OFFICIAL` after the last unit affecting the statistics is official. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Document Subtypes

| Subtype | Scope | `DocumentCode` level | Trigger |
|---|---|---|---|
| `ANALYSIS` | Cumulative match analysis for singles and doubles competitors who reached semifinals or finals. | Event unit | Immediately after the semifinal or final start list has been confirmed. Only sent for semifinal and final phases, including gold and bronze medal matches. |
| `TOU` | Tournament statistics. | Event | After the last match in each phase for completed and official events. |
| `CUM` | Penalty statistics per player, whether participating in singles or doubles. | Discipline | After each match with a penalty. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- Progress (0,1)
    |   |   +-- @LastUnit
    |   |   +-- @UnitsTotal
    |   |   +-- @UnitsComplete
    |   +-- SportDescription (0,1)
    |       +-- @DisciplineName
    |       +-- @EventName
    |       +-- @Gender
    +-- Stats (1,1)
        +-- @Code
        +-- StatsItems (0,1)
        |   +-- StatsItem (1,N)
        |       +-- @Type
        |       +-- @Code
        |       +-- @Pos
        |       +-- @Value
        |       +-- @Avg
        +-- Competitor (0,N)
            +-- @Code
            +-- @Type
            +-- @Order
            +-- @Organisation
            +-- StatsItems (0,1)
            |   +-- StatsItem (1,N)
            |       +-- @Type
            |       +-- @Code
            |       +-- @Pos
            |       +-- @Value
            |       +-- @Attempt
            |       +-- @Avg
            |       +-- @Percent
            +-- Composition (0,1)
                +-- Athlete (1,N)
                    +-- @Code
                    +-- @Order
                    +-- Description (1,1)
                    +-- StatsItems (0,1)
```

## Shared Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competition/@Gen` | O | `S(20)` | General dictionary version. |
| `Competition/@Sport` | O | `S(20)` | Sport dictionary version. |
| `Competition/@Codes` | O | `S(20)` | Code-set version. |
| `Progress/@LastUnit` | O | `CC@EVENT_UNIT` | Most recent official unit. |
| `Progress/@UnitsTotal` | O | Positive integer | Total matches to be played. |
| `Progress/@UnitsComplete` | O | `##0` | Total matches that are official. |
| `SportDescription/@DisciplineName` | M | `CC@DISCIPLINE` ENG description | Discipline display name. |
| `SportDescription/@EventName` | O | `CC@EVENT` ENG description | Event display name. |
| `SportDescription/@Gender` | O | `CC@DISCIPLINE_GENDER` | Gender code. |
| `Stats/@Code` | M | `SC@Statistics` | Statistics table identifier. |

## Tournament StatsItems (`DocumentSubtype="TOU"`)

These live under `Competition / Stats / StatsItems / StatsItem` and use `Pos=SC@StatsPhase`.

| Type | Code | Value / Avg | Meaning |
|---|---|---|---|
| `ST` | `MATCHES_NUM` | `Value=##0` | Number of matches for the phase/round. |
| `ST` | `GAMES_NUM` | `Value=##0` | Number of games for the phase/round. |
| `ST` | `PTS_NUM` | `Value=####0` | Total points scored for the phase/round. |
| `ST` | `DURATION` | `Value=h:mm`, `Avg=mm` | Total duration and average match duration. |
| `ST` | `DURATION_MAX` | `Value=mm` | Longest match duration; `ExtendedStat Code="UNIT_NUM"` carries the match number. |
| `ST` | `DURATION_MIN` | `Value=mm` | Shortest match duration; `ExtendedStat Code="UNIT_NUM"` carries the match number. |
| `ST` | `SHUTTLES_USED` | `Value=##0`, `Avg=#0.0` | Total and average shuttlecocks used. |
| `ST` | `YC` | `Value=0` | Yellow penalty cards. |
| `ST` | `RC` | `Value=0` | Red penalty cards. |
| `ST` | `BC` | `Value=0` | Black penalty cards. |
| `ST` | `IRS` | `Value=##0` | Number of Instant Review System cases. |
| `ST` | `RALLY_TIME_MAX` | `Value=sss` | Longest rally duration in seconds. |
| `ST` | `RALLY_STROKES_MAX` | `Value=##0` | Highest stroke count in the longest rally. |
| `ST` | `SMASH_MAX` | `Value=##0` | Fastest smash. |

## Competitor Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competitor/@Code` | M | `S(20)` | Competitor ID with no leading zeroes. |
| `Competitor/@Type` | M | `A` | Athlete competitor. |
| `Competitor/@Order` | M | Positive integer | Order of the competitor in the statistics table. |
| `Competitor/@Organisation` | O | `CC@ORGANISATION` | Competitor organisation when known. |
| `Athlete/@Code` | M | `S(20)` | Athlete ID. |
| `Athlete/@Order` | M | `1` | `1` when the competitor type is `A`. |
| `Athlete/Description/@FamilyName` | M | Text | Family name in WNPA format. |
| `Athlete/Description/@Gender` | M | `CC@PERSON_GENDER` | Athlete gender. |
| `Athlete/Description/@Organisation` | M | `CC@ORGANISATION` | Athlete organisation. |

## Competitor StatsItems

| Type | Code | Pos | Value | Meaning |
|---|---|---|---|---|
| `ANALYSIS` | `GAMES` | `W`, `L` | `#0` | Games won and games lost in match analysis statistics. |
| `ANALYSIS` | `PTS` | `W`, `L` | `##0` | Points won and points conceded in match analysis statistics. |
| `PTY` | `YC`, `RC`, `BC` | Positive integer | `CC@EVENT_UNIT` | Penalty card occurrence, with the unit RSC as value. |

Penalty (`PTY`) stats use extended stats when available:

| ExtendedStat Code | Value | Meaning |
|---|---|---|
| `OFFENCE` | `SC@Offence` | Related offence. |
| `SCORE` | String | Score at offence, for example `21-19, 7-7`. |
| `TIME` | Time | Time of penalty. |
| `UNIT_NUM` | `S(15)` | Match number. |

The source sample also shows a compact player stat shape with `MATCHES`, `DURATION`, `GAMES`, `PTS`, `HAND`, and `GRIP`
under `StatsItems`. Treat those as sample-visible cumulative player values; the explicit BDM tables above define the
normative `ANALYSIS`, `TOU`, and `CUM` sections.

## Samples from the Dictionary, Normalized

### Tournament statistics

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-BDM-1.1 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <Progress LastUnit="BDMMSINGLES-----------FNL-000100--" UnitsTotal="32" UnitsComplete="32"/>
    <SportDescription DisciplineName="Badminton" EventName="Men's Singles" Gender="M"/>
  </ExtendedInfos>
  <Stats Code="TOU">
    <StatsItems>
      <StatsItem Type="ST" Code="MATCHES_NUM" Pos="GP--" Value="32"/>
      <StatsItem Type="ST" Code="GAMES_NUM" Pos="GP--" Value="71"/>
      <StatsItem Type="ST" Code="PTS_NUM" Pos="GP--" Value="2399"/>
      <StatsItem Type="ST" Code="DURATION" Pos="GP--" Value="20:12" Avg="40"/>
      <StatsItem Type="ST" Code="DURATION_MAX" Pos="GP--" Value="77">
        <ExtendedStat Code="UNIT_NUM" Value="MS116"/>
      </StatsItem>
      <StatsItem Type="ST" Code="SHUTTLES_USED" Pos="GP--" Value="528" Avg="16.5"/>
      <StatsItem Type="ST" Code="RALLY_TIME_MAX" Pos="GP--" Value="60"/>
      <StatsItem Type="ST" Code="RALLY_STROKES_MAX" Pos="GP--" Value="51"/>
      <StatsItem Type="ST" Code="SMASH_MAX" Pos="GP--" Value="302"/>
    </StatsItems>
  </Stats>
</Competition>
```

### Penalty statistics

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-BDM-1.1 SFR" Codes="SYOG-2026">
  <Stats Code="CUM">
    <Competitor Code="1010001" Type="A" Order="1" Organisation="SEN">
      <StatsItems>
        <StatsItem Type="PTY" Code="YC" Pos="1" Value="BDMMSINGLES-----------FNL-000100--">
          <ExtendedStat Code="OFFENCE" Value="DELAY"/>
          <ExtendedStat Code="SCORE" Value="21-19, 7-7"/>
          <ExtendedStat Code="TIME" Value="10:42:13"/>
          <ExtendedStat Code="UNIT_NUM" Value="MS116"/>
        </StatsItem>
      </StatsItems>
      <Composition>
        <Athlete Code="1010001" Order="1">
          <Description GivenName="Awa" FamilyName="Diop" Gender="F" Organisation="SEN" BirthDate="2009-04-12"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Stats>
</Competition>
```

## Message Sort

Sort according to `@Order` attributes.

## XSD Validation

The normalized XML examples above were validated as `Competition` fragments wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Model `DocumentSubtype` as part of the statistics table identity. `TOU`, `ANALYSIS`, and `CUM` do not carry the same
  semantics even when they share `StatsItems`.
- `TOU` uses aggregate `Stats/StatsItems`; `ANALYSIS` and `CUM` use `Stats/Competitor/StatsItems`.
- Penalties are player-scoped cumulative stats, not match result stats. Keep the event-unit RSC from `Value` and the
  match number from `ExtendedStat UNIT_NUM`.
- `Progress` gives publication coverage for the statistic table; it should not be inferred from the number of
  `StatsItem` rows.

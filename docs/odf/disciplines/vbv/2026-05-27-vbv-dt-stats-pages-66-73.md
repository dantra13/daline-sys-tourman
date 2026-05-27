# ODF VBV Data Dictionary: DT_STATS, Pages 66-73

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 66-73.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note restructures the Beach Volleyball `DT_STATS` section into a practical reference for cumulative team/player
statistics and tournament rankings.

## 2.3.10 Statistics

`DT_STATS` contains statistics for a competitor, either an individual athlete or a team, at the event scope identified
by `OdfBody/@DocumentCode`.

The dictionary states that each table with multiple statistics is sent as a separate message, identified by the
header's `DocumentSubtype` and `DocumentSubcode`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@EVENT` | Full event RSC. |
| `DocumentSubcode` | `S(20)` | Team ID when the message is for a single team. Used only with `DocumentSubtype="CUM"`. |
| `DocumentType` | `DT_STATS` | Statistics message. |
| `DocumentSubtype` | `CUM`, `TEAM_RANKING`, `IND_RANKING` | Selects cumulative team/player stats or ranking tables. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `LIVE`, `INTERMEDIATE`, `OFFICIAL`, `PROVISIONAL` | `OFFICIAL` after the last unit affecting the statistics is official. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Document Subtypes

| Subtype | Meaning | Notes |
|---|---|---|
| `CUM` | Cumulative individual player statistics and team statistics. | One message per team; `DocumentSubcode` is the team ID. |
| `TEAM_RANKING` | Ranking of team tournament statistics. | Team stat rows may include `Rank`, `RankEqual`, and `SortOrder`. |
| `IND_RANKING` | Ranking of individual tournament statistics. | Athlete stat rows may include `Rank`, `RankEqual`, and `SortOrder`. |

Trigger: after each match. The dictionary notes that `CUM` messages are sent first, followed by `TEAM_RANKING` and
`IND_RANKING`.

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
        +-- Competitor (0,N)
            +-- @Code
            +-- @Type
            +-- @Order
            +-- @Organisation
            +-- Description (0,1)
            |   +-- @TeamName
            +-- StatsItems (0,1)
            |   +-- StatsItem (1,N)
            |       +-- @Type
            |       +-- @Code
            |       +-- @Pos
            |       +-- @Value
            |       +-- @Attempt
            |       +-- @Percent
            |       +-- @Rank
            |       +-- @RankEqual
            |       +-- @SortOrder
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
| `Progress/@LastUnit` | O | `CC@EVENT_UNIT` | Most recent official game. Sent in `CUM` and `IND_RANKING`. |
| `Progress/@UnitsTotal` | O | Positive integer | Total games to be played. For `CUM`, total games for that team. |
| `Progress/@UnitsComplete` | O | `##0` | Official games completed. For `CUM`, completed games for that team. |
| `SportDescription/@DisciplineName` | M | `CC@DISCIPLINE` ENG description | Discipline display name. |
| `SportDescription/@EventName` | O | `CC@EVENT` ENG description | Event display name. |
| `SportDescription/@Gender` | O | `CC@DISCIPLINE_GENDER` | Gender code. |
| `Stats/@Code` | M | `SC@Statistics` | Statistics table identifier. |

## Team Competitor Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competitor/@Code` | M | `S(20)` | Team competitor ID. |
| `Competitor/@Type` | M | `T` | Team competitor. |
| `Competitor/@Order` | M | Positive integer | Order of the team in the statistics table. |
| `Competitor/@Organisation` | O | `CC@ORGANISATION` | Team organisation when known. |
| `Competitor/Description/@TeamName` | M | `S(73)` | Team name. Applies only for teams. |

## Team StatsItems

Team statistics are sent under `Stats / Competitor / StatsItems / StatsItem`.

| Type | Code | Value / Attempt / Percent | Ranking attrs | Meaning |
|---|---|---|---|---|
| `ST` | `SRV` | `Value=#0`, `Attempt=##0` | `Rank`, `RankEqual`, `SortOrder` for `TEAM_RANKING` | Service aces and service attempts. Ranking is by aces. |
| `ST` | `ATC` | `Value=##0`, `Attempt=##0`, `Percent=##0` | `Rank`, `RankEqual`, `SortOrder` for `TEAM_RANKING` | Attack successes, attempts, and success percentage. |
| `ST` | `BLC` | `Value=##0` | `Rank`, `RankEqual`, `SortOrder` for `TEAM_RANKING` | Block successes. |
| `ST` | `MP` | `Value=#0` | N/A | Matches played. |
| `ST` | `DIG` | `Value=##0`, `Attempt=##0` | `Rank`, `RankEqual`, `SortOrder` for `TEAM_RANKING` | Dig successes and attempts. |

Ranking attributes:

| Attribute | Meaning |
|---|---|
| `Rank` | Ranking string. Send `NR` when no rank exists and `DSQ` when the team is disqualified. |
| `RankEqual` | `Y` when rank is tied; otherwise omit. |
| `SortOrder` | Rank-like ordering including competitors with IRM or no rank. |

## Athlete Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Athlete/@Code` | M | `S(20)` | Athlete ID for a team member. |
| `Athlete/@Order` | M | Positive integer | Within a team, players are sorted by bib. |
| `Athlete/Description/@FamilyName` | M | Text | Family name in WNPA format. |
| `Athlete/Description/@Gender` | M | `CC@PERSON_GENDER` | Athlete gender. |
| `Athlete/Description/@Organisation` | M | `CC@ORGANISATION` | Athlete organisation. |
| `Athlete/Description/@BirthDate` | O | Date | Include when available. |
| `Athlete/Description/@IFId` | O | `S(16)` | International federation ID. |

## Athlete StatsItems

Athlete statistics are sent under `Stats / Competitor / Composition / Athlete / StatsItems / StatsItem`.

| Type | Code | Value / Attempt / Avg / Percent | Ranking attrs | Meaning |
|---|---|---|---|---|
| `ST` | `SRV` | `Value=#0`, `Attempt=##0` | `Rank`, `RankEqual`, `SortOrder` for `IND_RANKING` | Service aces and serve attempts. |
| `ST` | `SRV_SPEED` | `Value=##0` | `Rank`, `RankEqual`, `SortOrder` for `IND_RANKING` | Fastest serve in km/h. |
| `ST` | `ATC` | `Value=##0`, `Attempt=##0`, `Percent=##0` | `Rank`, `RankEqual`, `SortOrder` for `IND_RANKING` | Attack successes, attempts, and success percentage. |
| `ST` | `BLC` | `Value=##0` | `Rank`, `RankEqual`, `SortOrder` for `IND_RANKING` | Block successes. |
| `ST` | `MP` | `Value=#0` | N/A | Total matches played. |
| `ST` | `SETS_PLAYED` | `Value=##0` | N/A | Sets/periods played by the athlete. |
| `ST` | `TOTAL` | `Value=###0`, `Avg=##0.00` | `Rank`, `RankEqual`, `SortOrder` for `IND_RANKING` | Total points from attacks, blocks, and serves, plus average points by set. |
| `ST` | `DIG` | `Value=##0` | `Rank`, `RankEqual`, `SortOrder` for `IND_RANKING` | Dig successes. |

## Samples from the Dictionary, Normalized

### Cumulative team and player statistics

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <Progress LastUnit="VBVWTEAM2-------------GP--000100--" UnitsTotal="5" UnitsComplete="5"/>
    <SportDescription DisciplineName="Beach Volleyball" EventName="Women's Team" Gender="W"/>
  </ExtendedInfos>
  <Stats Code="CUM">
    <Competitor Code="VBVWTEAM2---AUS01" Type="T" Order="1" Organisation="AUS">
      <Description TeamName="Bawden/Clancy"/>
      <StatsItems>
        <StatsItem Type="ST" Code="MP" Value="5"/>
        <StatsItem Type="ST" Code="ATC" Attempt="246" Value="146" Percent="59"/>
        <StatsItem Type="ST" Code="BLC" Value="9"/>
        <StatsItem Type="ST" Code="DIG" Attempt="172" Value="68"/>
        <StatsItem Type="ST" Code="SRV" Attempt="223" Value="15"/>
      </StatsItems>
      <Composition>
        <Athlete Code="1127813" Order="1">
          <Description GivenName="Taliqua" FamilyName="Clancy" Gender="F" Organisation="AUS" BirthDate="1992-06-25"/>
          <StatsItems>
            <StatsItem Type="ST" Code="MP" Value="5"/>
            <StatsItem Type="ST" Code="SETS_PLAYED" Value="11"/>
            <StatsItem Type="ST" Code="ATC" Attempt="134" Value="84" Percent="63"/>
            <StatsItem Type="ST" Code="TOTAL" Value="102" Avg="9.27"/>
          </StatsItems>
        </Athlete>
      </Composition>
    </Competitor>
  </Stats>
</Competition>
```

### Individual ranking statistics

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <Stats Code="IND_RANKING">
    <Competitor Code="VBVWTEAM2---AUS01" Type="T" Order="1" Organisation="AUS">
      <Description TeamName="Bawden/Clancy"/>
      <Composition>
        <Athlete Code="1127813" Order="1">
          <Description GivenName="Taliqua" FamilyName="Clancy" Gender="F" Organisation="AUS" BirthDate="1992-06-25"/>
          <StatsItems>
            <StatsItem Type="ST" Code="ATC" Rank="1" SortOrder="1"/>
            <StatsItem Type="ST" Code="BLC" Rank="27" SortOrder="27" RankEqual="Y"/>
            <StatsItem Type="ST" Code="DIG" Rank="9" SortOrder="9"/>
            <StatsItem Type="ST" Code="SRV" Rank="5" SortOrder="5"/>
            <StatsItem Type="ST" Code="SRV_SPEED" Rank="3" SortOrder="3"/>
            <StatsItem Type="ST" Code="TOTAL" Rank="6" SortOrder="6"/>
          </StatsItems>
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

- `CUM`, `TEAM_RANKING`, and `IND_RANKING` are separate table identities. Keep `DocumentSubtype` in the statistic key.
- `DocumentSubcode` is meaningful for VBV cumulative team messages and should be the team ID.
- Team statistics and athlete statistics use overlapping codes (`SRV`, `ATC`, `BLC`, `DIG`, `MP`) but live at different
  paths and can have different ranking semantics.
- Ranking messages can carry rank-only rows without `Value`; ingestion should not require metric values when
  `DocumentSubtype` is a ranking subtype.

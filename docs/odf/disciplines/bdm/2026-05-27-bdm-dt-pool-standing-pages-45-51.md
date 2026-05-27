# ODF BDM Data Dictionary: DT_POOL_STANDING, Pages 45-51

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BDM_Data_Dictionary.pdf`, pages 45-51.

Source version: `SYOG-2026-BDM-1.1 SFR`, dated 13 May 2026.

This note restructures the Badminton `DT_POOL_STANDING` section into a practical reference for group/pool standings and
pool match matrices.

## 2.3.7 Pool Standings

`DT_POOL_STANDING` contains the standings of a group in a competition. It is similar to a phase-results message, but the
trigger is different: it is sent at the start of OVR operations and after each event unit in the corresponding phase.

The message is sent independently for each group or pool in a phase. The group/pool is identified by the phase-level
`DocumentCode`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@Phase` | Full phase-level RSC for the pool. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_POOL_STANDING` | Pool standings message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` | `START_LIST` before competition; `INTERMEDIATE` during the phase; `UNOFFICIAL` if the last match is unofficial; `OFFICIAL` after all matches are official. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| Status | When to send |
|---|---|
| `START_LIST` | Before the start of competition to build the initial pool tables. |
| `INTERMEDIATE` | When an event unit in the corresponding phase finishes, without waiting for official status. |
| `UNOFFICIAL` / `OFFICIAL` | When the phase finishes and there are no more units/games to compete. |
| Any applicable status | Trigger again after any change. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- ExtendedInfo (0,N)
    |   |   +-- @Type
    |   |   +-- @Code
    |   |   +-- @Pos
    |   |   +-- @Value
    |   +-- Progress (0,1)
    |   |   +-- @LastUnit
    |   |   +-- @UnitsTotal
    |   |   +-- @UnitsComplete
    |   +-- SportDescription (0,1)
    |   |   +-- @DisciplineName
    |   |   +-- @EventName
    |   |   +-- @Gender
    |   +-- VenueDescription (0,1)
    |       +-- @Venue
    |       +-- @VenueName
    +-- Result (1,N)
        +-- @Rank
        +-- @RankEqual
        +-- @ResultType
        +-- @Result
        +-- @IRM
        +-- @QualificationMark
        +-- @SortOrder
        +-- @Won
        +-- @Lost
        +-- @Played
        +-- @For
        +-- @Against
        +-- @Diff
        +-- ExtendedResults (0,1)
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Composition (0,1)
            |   +-- Athlete (1,N)
            +-- Opponent (0,N)
                +-- @Code
                +-- @Type
                +-- @Pos
                +-- @Organisation
                +-- @Date
                +-- @Time
                +-- @Unit
                +-- @HomeAway
                +-- @Result
                +-- ExtendedResults (0,1)
                +-- Composition (0,1)
```

## ExtendedInfos

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competition/@Gen` | O | `S(20)` | General dictionary version. |
| `Competition/@Sport` | O | `S(20)` | Sport dictionary version. |
| `Competition/@Codes` | O | `S(20)` | Code-set version. |
| `ExtendedInfo Type="UI" Code="QUAL_RULE"` | M | `SC@QualRule` | Qualification rule. Always expected. |
| `Progress/@LastUnit` | O | `CC@EVENT_UNIT` | Most recent official unit for this pool. |
| `Progress/@UnitsTotal` | O | `##0` | Total matches to be played in this pool. |
| `Progress/@UnitsComplete` | O | `##0` | Total official matches in this pool. |
| `SportDescription/@DisciplineName` | M | `CC@DISCIPLINE` ENG description | Discipline display name. |
| `SportDescription/@EventName` | M | `CC@EVENT` ENG description | Event display name. |
| `SportDescription/@Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the event unit. |
| `VenueDescription/@Venue` | M | `CC@VENUE` ID | Included only when the phase is contested at a single venue. |
| `VenueDescription/@VenueName` | M | `CC@VENUE` ENG description | Included only when the phase is contested at a single venue. |

## Result Values

Each message must include at least one `Result` row for a competitor awarded a pool result.

| Attribute | Requirement | Value source | Meaning |
|---|---:|---|---|
| `Rank` | O | Positive integer | Rank in the group. |
| `RankEqual` | O | `Y` | Sent when the rank is tied. |
| `ResultType` | M | `SC@ResultType` | Points, or IRM-with-points, obtained across the group. |
| `Result` | O | `#0` | Classification/match points accrued during the pool stage. Optional before competition. |
| `IRM` | O | `SC@IRM` | Invalid result mark when assigned. |
| `QualificationMark` | O | `SC@QualificationMark` | Send `Q` for individuals qualified for the next round. |
| `SortOrder` | M | Positive integer | Presentation order for the group. Mostly rank-based, but should also sort disqualified competitors correctly. |
| `Won` | O | `#0` | Matches won. Do not send if the competitor has not played. |
| `Lost` | O | `#0` | Matches lost. Do not send if the competitor has not played. |
| `Played` | O | `#0` | Matches played. Send `0` if not played. |
| `For` | O | `###0` | Total points won. Do not send if the competitor has not played. |
| `Against` | O | `###0` | Total points lost. Do not send if the competitor has not played. |
| `Diff` | O | `+##0`, `-##0`, or `0` | Point difference. Do not send if the competitor has not played. |

### Extended Result Values

| Type | Code | Pos | Value | Extra | Meaning |
|---|---|---|---|---|---|
| `ER` | `GAMES` | N/A | `#0` | `Diff=+/-#0 or 0`, `Extension Code="LOST"` | Games won and games-lost difference. Do not send in case of IRM. |

## Competitor Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competitor/@Code` | M | `S(20)` | Competitor ID with no leading zeroes. |
| `Competitor/@Type` | M | `A` | Athlete competitor. |
| `Competitor/@Organisation` | M | `CC@ORGANISATION` | Competitor organisation. |
| `Athlete/@Code` | M | `S(20)` | Athlete ID for an individual athlete. |
| `Athlete/@Order` | M | `1` | Individual events use one athlete. |
| `Athlete/Description/@FamilyName` | M | Text | Family name in WNPA format. |
| `Athlete/Description/@Gender` | M | `CC@PERSON_GENDER` | Athlete gender. |
| `Athlete/Description/@Organisation` | M | `CC@ORGANISATION` | Athlete organisation. |
| `ExtendedDescription Type="ED" Code="SEED"` | O | `#0` | Seed value when applicable in individual events. |

## Opponent Values

`Opponent` carries the pool matrix: opponents of the competitor in the `Opponent/@Pos` column.

| Attribute | Requirement | Value source | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` | Opponent competitor ID. |
| `Type` | M | `A` | Athlete competitor. |
| `Pos` | M | `#0` | Opponent column position, normally the same as that opponent's `Result/@SortOrder`. |
| `Organisation` | M | `CC@ORGANISATION` | Opponent organisation. |
| `Date` | O | Date | Match date, if available. Send after the match is complete too. |
| `Time` | O | `HH:MM` | Match time, if available and allowed to display. |
| `Unit` | O | `CC@EVENT_UNIT` | Full unit RSC for the pool item. |
| `HomeAway` | O | `H`, `A` | Send `H` if the opponent is the home competitor, `A` if away. |
| `Result` | O | `S(50)` | Completed match result formatted as in ORIS, relative to the row competitor. |

Opponent extended result:

| Type | Code | Value | Meaning |
|---|---|---|---|
| `ER` | `ALT_TIME` | `SC@AltTime` | Display text in front of the time. Do not send if the match is complete. |

Opponent composition is sent only for singles events, with the same athlete/description shape as the main competitor.

## Sample, Normalized

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-BDM-1.1 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <ExtendedInfo Type="UI" Code="QUAL_RULE" Value="TOP2"/>
    <Progress LastUnit="BDMWSINGLES-----------GP--000300--" UnitsTotal="3" UnitsComplete="2"/>
    <SportDescription DisciplineName="Badminton" EventName="Women's Singles" Gender="W"/>
    <VenueDescription Venue="MFS" VenueName="Multi-Function Sport Hall"/>
  </ExtendedInfos>
  <Result Rank="1" ResultType="POINTS" Result="2" QualificationMark="Q" SortOrder="1"
          Won="2" Lost="0" Played="2" For="84" Against="66" Diff="+18">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="GAMES" Value="4" Diff="+3">
        <Extension Code="LOST" Value="1"/>
      </ExtendedResult>
    </ExtendedResults>
    <Competitor Code="1010001" Type="A" Organisation="SEN">
      <Composition>
        <Athlete Code="1010001" Order="1">
          <Description GivenName="Awa" FamilyName="Diop" Gender="F" Organisation="SEN" BirthDate="2009-04-12">
            <ExtendedDescription Type="ED" Code="SEED" Value="1"/>
          </Description>
        </Athlete>
      </Composition>
      <Opponent Code="1010002" Type="A" Pos="2" Organisation="FRA"
                Date="2026-10-31" Time="10:00" Unit="BDMWSINGLES-----------GP--000100--"
                HomeAway="A" Result="2-0">
        <Composition>
          <Athlete Code="1010002" Order="1">
            <Description GivenName="Lea" FamilyName="Martin" Gender="F" Organisation="FRA" BirthDate="2009-09-02"/>
          </Athlete>
        </Composition>
      </Opponent>
      <Opponent Code="1010003" Type="A" Pos="3" Organisation="JPN"
                Date="2026-11-01" Time="11:30" Unit="BDMWSINGLES-----------GP--000300--"
                HomeAway="H">
        <ExtendedResults>
          <ExtendedResult Type="ER" Code="ALT_TIME" Value="TBC"/>
        </ExtendedResults>
        <Composition>
          <Athlete Code="1010003" Order="1">
            <Description GivenName="Mika" FamilyName="Sato" Gender="F" Organisation="JPN" BirthDate="2009-07-18"/>
          </Athlete>
        </Composition>
      </Opponent>
    </Competitor>
  </Result>
</Competition>
```

## Message Sort

Sort by `Result/@SortOrder`.

## XSD Validation

The normalized XML example above was validated as a `Competition` fragment wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Model one `DT_POOL_STANDING` message as one version of one pool table. The pool is identified by `DocumentCode`.
- `Result` rows are competitor standings; `Opponent` rows are the pool matrix/fixture view from that competitor's
  perspective.
- `Opponent/@Result` is relative to the row competitor, so the same completed match can be represented differently from
  the opponent's row.
- `QualificationMark` is separate from rank. A rank can exist before qualification is confirmed.
- `QUAL_RULE` belongs to `ExtendedInfos` and describes the rule for advancing out of the pool; do not derive it from the
  number of `QualificationMark="Q"` rows alone.

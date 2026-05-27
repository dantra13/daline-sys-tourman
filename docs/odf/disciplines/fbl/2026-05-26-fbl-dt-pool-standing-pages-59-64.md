# ODF FBL Data Dictionary: DT_POOL_STANDING, Pages 59-64

Source: `C:\Users\mella\WebstormProjects\sportivo\docs\references\odf\ODF_FBL_Data_Dictionary.pdf`, pages 59-64.

This note restructures section `2.3.7 Pool Standings` from the Paris 2024 Football Data Dictionary into a football-specific reference for Sportivo domain modeling. It should be read alongside the general interface reference in `2026-05-26-gen-dt-pool-standing-pages-176-189.md`.

## 2.3.7 Pool Standings

`DT_POOL_STANDING` contains the standings of one football group/pool in a competition. This is the message that publishes the group table.

The message is similar to `DT_PHASE_RESULT`, but differs in trigger and frequency. For football it is triggered at the start of OVR operations and then after each event unit, meaning after each match affecting the group.

The message is sent independently for each group/pool in a phase. In football, the group/pool is determined from the message header `DocumentCode`, which is a full phase-level RSC such as `FBLMTEAM11------------GPA---------`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC @Competition` | Unique competition ID. |
| `DocumentCode` | `CC @Phase` | Full phase-level RSC. This identifies the football group/pool. |
| `DocumentSubcode` | `N/A` | Not used for FBL `DT_POOL_STANDING`. |
| `DocumentType` | `DT_POOL_STANDING` | Pool standings message. |
| `DocumentSubtype` | `N/A` | Not used for FBL `DT_POOL_STANDING`. |
| `Version` | `1..V` | Ascending version number associated with the message content. |
| `ResultStatus` | `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` | Status of the standings message. |
| `FeedFlag` | `P` or `T` | Production or test message. |
| `Date` | Date | Generation date in the local time zone where the message was produced. |
| `Time` | Time | Generation time up to milliseconds in the local time zone where the message was produced. |
| `LogicalDate` | Date | Logical date of events. Usually the same as the physical day unless transmission extends after midnight. |
| `Source` | `SC @Source` | Code of the system that generated the message. |

## Trigger and Frequency

| Condition | Expected `ResultStatus` | Meaning |
|---|---|---|
| Before the competition starts | `START_LIST` | Initial group tables. |
| When a match in the phase finishes, without waiting for official confirmation | `INTERMEDIATE` | Updated table after the match affects the group. |
| If the last match is unofficial | `UNOFFICIAL` | Table based on an unofficial last match. |
| When the phase finishes and all matches are official | `OFFICIAL` | Final official group table. |
| Any pending decision affects the table | `PROVISIONAL` | Table is published but subject to decision. |
| Any change | Depends on content | The dictionary explicitly says the trigger also applies after any change. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- ExtendedInfo (0,N)
    |   +-- Progress (0,1)
    |   +-- SportDescription (0,1)
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
        +-- @Tied
        +-- @Played
        +-- @For
        +-- @Against
        +-- @Diff
        +-- ExtendedResults (0,1)
        |   +-- ExtendedResult (1,N)
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Description (0,1)
            |   +-- @TeamName
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
                +-- Description (0,1)
                    +-- @TeamName
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Gen` | O | `S(20)` | Version of the General Data Dictionary applicable to the message. |
| `Sport` | O | `S(20)` | Version of the Sport Data Dictionary applicable to the message. |
| `Codes` | O | `S(20)` | Version of the codes applicable to the message. |

### `Competition/ExtendedInfos/ExtendedInfo`

Football uses this node for the group qualification rule.

| Type | Code | Pos | Expected when | Value |
|---|---|---|---|---|
| `UI` | `QUAL_RULE` | `N/A` | Always | `SC @QualRule` code for the qualification rule. |

`QUAL_RULE` is the machine code for how teams qualify from the pool. The human text is resolved from the football sport-code catalog. For Paris 2024-style football, examples include rules such as top two teams per group qualifying, or top two plus best third-ranked teams depending on event format.

### `Competition/ExtendedInfos/Progress`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `LastUnit` | O | `CC @Unit` | Full RSC of the most recent official unit for the pool in this message. |
| `UnitsTotal` | O | Numeric `##0` | Total number of units/matches to be played in the pool. |
| `UnitsComplete` | O | Numeric `##0` | Total number of official units/matches in the pool. |

### `Competition/ExtendedInfos/SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `DisciplineName` | M | `S(40)` | Discipline English description from Common Codes, not code. |
| `EventName` | M | `S(40)` | Event English description from Common Codes, not code. |
| `SubEventName` | M | `S(40)` | Phase English description from Common Codes, not code. |
| `Gender` | M | `CC @SportGender` | Gender code for the event unit. |

### `Competition/Result`

Each `Result` row is one team in the group table.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Rank` | O | Text | Rank in the group. Optional because a team may be disqualified. |
| `RankEqual` | O | `S(1)` | Send `Y` when the rank is equal; otherwise do not send. |
| `ResultType` | M | `SC @ResultType` | Type of `Result`; either points or IRM with points across the group. |
| `Result` | O | Numeric | Classification points accrued during the pool stage. Optional before the competition. |
| `IRM` | O | `SC @IRM` | Invalid rank mark. Send only when `ResultType` is IRM. |
| `QualificationMark` | O | `SC @QualificationMark` | Qualification indicator. |
| `SortOrder` | M | Numeric | Sequential display order for the group. Mostly based on rank, but also sorts disqualified teams. |
| `Won` | O | Numeric `#0` | Games won. Do not send if the team has not played. |
| `Lost` | O | Numeric `#0` | Games lost. Do not send if the team has not played. |
| `Tied` | O | Numeric `#0` | Games tied. Do not send if the team has not played. |
| `Played` | O | Numeric `#0` | Games played. Do not send if the team has not played. |
| `For` | O | Numeric `#0` | Total goals for. Do not send if the team has not played. |
| `Against` | O | Numeric `#0` | Total goals against. Do not send if the team has not played. |
| `Diff` | O | `+#0` or `-#0` | Goal difference. Positive values use `+`. |

Important football semantics:

- `Result` is table points, not goals.
- `ResultType="POINTS"` means the `Result` value is interpreted as points in this standing context.
- `For`, `Against`, and `Diff` are goals for, goals against, and goal difference.
- `Diff` should include `+` for positive goal difference.
- Do not send played/stat columns before the team has played.

### `Competition/Result/ExtendedResults/ExtendedResult`

Football adds one optional row-level extended result:

| Type | Code | Pos | Expected when | Value |
|---|---|---|---|---|
| `ER` | `FPP` | `N/A` | If available | Fair play points, also called Team Conduct Score. Integer value; can be negative. |

### `Competition/Result/Competitor`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | M | `S(1)` | `T` for team. |
| `Organisation` | M | `CC @Organisation` | Competitor organisation. |

#### `Description`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `TeamName` | M | `S(73)` | Team name. |

### `Competition/Result/Competitor/Opponent`

`Opponent` carries each opponent in the pool, allowing consumers to build a fixture/result matrix for the group.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Opponent competitor ID. |
| `Type` | M | `S(1)` | `T` for team. |
| `Pos` | M | Numeric `#0` | Opponent column position in the pool. Usually the same as that opponent's `Result/@SortOrder`. |
| `Organisation` | M | `CC @Organisation` | Opponent organisation code. |
| `Date` | M | Date | Match date. Send if available, even after completion. |
| `Time` | O | `S(5)` | Match time, for example `HH:MM`. Send if available. |
| `Unit` | O | `CC @Unit` | Full RSC of the unit/match for the pool item. |
| `HomeAway` | O | `S(1)` | `H` if the opponent is the home team, `A` if the opponent is the away team. |
| `Result` | O | `S(50)` | Match result if complete. The value is relative to the current competitor row and may be reversed in the opponent row or according to display rules. |

#### `Opponent/Description`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `TeamName` | M | `S(73)` | Opponent team name. |

## Sample from the Dictionary, Normalized

The PDF provides a partial `Result` example. It represents one row of a group table for Egypt, including three opponents.

```xml
<Result Rank="3"
        ResultType="POINTS"
        Result="1"
        SortOrder="3"
        Played="2"
        Won="1"
        Tied="0"
        Lost="1"
        For="3"
        Against="2"
        Diff="1">
  <Competitor Code="FBLMTEAM11----EGY01" Type="T" Organisation="EGY">
    <Description TeamName="Egypt"/>
    <Opponent Code="FBLMTEAM11----BRA01"
              Type="T"
              Pos="1"
              Organisation="BRA"
              Date="2012-07-27"
              Time="14:00"
              Unit="FBLMTEAM11------------GPA-000200--"
              HomeAway="H"
              Result="2-0">
      <Description TeamName="Brazil"/>
    </Opponent>
    <Opponent Code="FBLMTEAM11----BLR01"
              Type="T"
              Pos="2"
              Organisation="BLR"
              Date="2012-08-01"
              Time="09:00"
              Unit="FBLMTEAM11------------GPA-000400--"
              HomeAway="A">
      <Description TeamName="Belarus"/>
    </Opponent>
    <Opponent Code="FBLMTEAM11----NZL01"
              Type="T"
              Pos="4"
              Organisation="NZL"
              Date="2012-07-29"
              Time="09:00"
              Unit="FBLMTEAM11------------GPA-000500--"
              HomeAway="A"
              Result="1-2">
      <Description TeamName="New Zealand"/>
    </Opponent>
  </Competitor>
</Result>
```

### Sample Reading

| Field | Meaning |
|---|---|
| `Rank="3"` | Egypt is third in the group at this version of the table. |
| `ResultType="POINTS"` and `Result="1"` | Egypt has 1 classification point. |
| `Played="2"`, `Won="1"`, `Lost="1"` | The table row includes only played matches at that point. |
| `For="3"`, `Against="2"`, `Diff="1"` | Goals for, goals against, and goal difference. |
| Opponent vs Brazil has `Result="2-0"` | The Brazil match is complete and Egypt's row carries the match result as displayed relative to that row. |
| Opponent vs Belarus has no `Result` | The match is known in the pool matrix but does not yet have a completed result in this sample. |

## Normalized Full-Message Example

The dictionary sample is partial. The following example wraps the same shape in a compact full `OdfBody` message. Concrete version strings, competition IDs, and code values must come from the target event release.

```xml
<OdfBody CompetitionCode="OG2024"
         DocumentCode="FBLMTEAM11------------GPA---------"
         DocumentType="DT_POOL_STANDING"
         Version="5"
         ResultStatus="INTERMEDIATE"
         FeedFlag="T"
         Date="2024-07-29"
         Time="19:45:00"
         LogicalDate="2024-07-29"
         Source="OVR">
  <Competition Gen="SOG-2024-GEN" Sport="SOG-2024-FBL-3.4" Codes="SOG-2024-CODES">
    <ExtendedInfos>
      <ExtendedInfo Type="UI" Code="QUAL_RULE" Value="TOP2"/>
      <Progress LastUnit="FBLMTEAM11------------GPA-000500--"
                UnitsTotal="6"
                UnitsComplete="4"/>
      <SportDescription DisciplineName="Football"
                        EventName="Men's Team"
                        SubEventName="Men's Group A"
                        Gender="M"/>
    </ExtendedInfos>

    <Result Rank="1"
            ResultType="POINTS"
            Result="6"
            SortOrder="1"
            Played="2"
            Won="2"
            Tied="0"
            Lost="0"
            For="4"
            Against="1"
            Diff="+3"
            QualificationMark="Q">
      <Competitor Code="FBLMTEAM11----BRA01" Type="T" Organisation="BRA">
        <Description TeamName="Brazil"/>
      </Competitor>
    </Result>

    <Result Rank="3"
            ResultType="POINTS"
            Result="1"
            SortOrder="3"
            Played="2"
            Won="1"
            Tied="0"
            Lost="1"
            For="3"
            Against="2"
            Diff="+1">
      <Competitor Code="FBLMTEAM11----EGY01" Type="T" Organisation="EGY">
        <Description TeamName="Egypt"/>
        <Opponent Code="FBLMTEAM11----BRA01"
                  Type="T"
                  Pos="1"
                  Organisation="BRA"
                  Date="2012-07-27"
                  Time="14:00"
                  Unit="FBLMTEAM11------------GPA-000200--"
                  HomeAway="H"
                  Result="2-0">
          <Description TeamName="Brazil"/>
        </Opponent>
        <Opponent Code="FBLMTEAM11----BLR01"
                  Type="T"
                  Pos="2"
                  Organisation="BLR"
                  Date="2012-08-01"
                  Time="09:00"
                  Unit="FBLMTEAM11------------GPA-000400--"
                  HomeAway="A">
          <Description TeamName="Belarus"/>
        </Opponent>
        <Opponent Code="FBLMTEAM11----NZL01"
                  Type="T"
                  Pos="4"
                  Organisation="NZL"
                  Date="2012-07-29"
                  Time="09:00"
                  Unit="FBLMTEAM11------------GPA-000500--"
                  HomeAway="A"
                  Result="1-2">
          <Description TeamName="New Zealand"/>
        </Opponent>
      </Competitor>
    </Result>
  </Competition>
</OdfBody>
```

## Message Sort

The attribute used to sort football pool standings is `Result/@SortOrder`.

`SortOrder` is mandatory and should be used for presentation. It is mostly based on rank, but also handles cases such as disqualified teams.

## Comparison with the GEN Interface

The FBL structure matches the general shape: `Competition` contains one or more `Result` rows; each `Result` has one `Competitor`; and `Opponent` can provide the group matrix. The main differences are football-specific constraints and a smaller structure.

| Area | GEN interface | FBL dictionary |
|---|---|---|
| Group identity | `DocumentCode` plus optional sport-specific `DocumentSubtype`. | `DocumentCode` only; `DocumentSubcode` and `DocumentSubtype` are `N/A`. |
| `ResultStatus` | Includes `START_LIST`, `INTERMEDIATE`, `UNCONFIRMED`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. | Expected statuses are `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `Competition/@Gen`, `Sport`, `Codes` | GEN 2026 says `Gen` and `Codes` mandatory, `Sport` optional. | FBL 2024 marks all three optional. |
| `ExtendedInfo` | Fully sport-specific. | `UI/QUAL_RULE` is expected always, with `Value=SC @QualRule`. |
| `Progress/@LastUnit` | Last completed or in-progress related unit. | Most recent official unit for this pool. |
| `SportDescription/SubEventName` | Optional in the general interface. | Mandatory in FBL. |
| `Result/@ResultType` | Optional in the general interface. | Mandatory in FBL. |
| `Result/@Diff` | Sport-specific. | Goal difference and positive values use `+`. |
| `Result/@Ratio` | Available in GEN. | Not part of the FBL structure. |
| `RecordIndicators` | Available in GEN. | Not part of the FBL structure. |
| `Opponent/@TimeStamp` | Available in GEN 2026. | Not part of the FBL 2024 structure. |
| Opponent nested data | GEN allows extended results and composition. | FBL keeps only opponent attributes plus `Description/@TeamName`. |

## XSD Notes

The OWG2026 draft XSD package uses a generic `resultType` for `Competition/Result`, so it is broader than the FBL 2024 message table.

Relevant drift:

- The XSD allows attributes not listed in FBL `DT_POOL_STANDING`, such as `Pty`, `WLT`, `StartOrder`, and `PhotoFinish`.
- The XSD allows `Opponent/@TimeStamp`; FBL 2024 does not list it.
- The FBL dictionary makes `ResultType` mandatory for this message, while the generic XSD marks it optional.
- Semantic validation should follow the FBL dictionary for football-specific messages, not only the generic XSD.

## Modeling Notes

- Model one `DT_POOL_STANDING` message as one published version of one football group table.
- The group key is the phase RSC in `DocumentCode`, for example `FBLMTEAM11------------GPA---------`.
- `DocumentSubtype` should not be used for FBL pool identity in this Paris 2024 dictionary.
- `Result` rows are teams; matches live as related facts and may appear as `Opponent` entries.
- Store table points separately from goals. In ODF FBL, `Result/@Result` is classification points, while `For`, `Against`, and `Diff` are goals.
- Keep `SortOrder` as the canonical published row order.
- Tiebreak rules are not encoded by the general attributes. Sportivo must calculate them from competition configuration and then publish `Rank`, `RankEqual`, and `SortOrder`.
- `QUAL_RULE` should be modeled as the group qualification rule and rendered from a sport-code catalog or Sportivo rule configuration.
- `FPP` is fair-play points/team conduct score. It may be useful as an explicit tiebreak component in football standings.
- Do not send played/stat columns before a team has played if following the FBL dictionary strictly.

## Code Appendix: Referenced SC and CC Entities

The PDF pages link to the Paris 2024 code pages below. Resolve authoritative values from the target code release.

| Reference | Used for | Source |
|---|---|---|
| `CC @Competition` | `OdfBody/@CompetitionCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm` |
| `CC @Phase` | `OdfBody/@DocumentCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Phase.htm` |
| `CC @ResultStatus` | `OdfBody/@ResultStatus` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm` |
| `SC @Source` | `OdfBody/@Source` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm` |
| `CC @Unit` | `Progress/@LastUnit`, `Opponent/@Unit` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm` |
| `SC @ResultType` | `Result/@ResultType` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_FBL.htm` |
| `SC @IRM` | `Result/@IRM` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_FBL.htm` |
| `SC @QualificationMark` | `Result/@QualificationMark` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_QualificationMark_SOG_FBL.htm` |
| `CC @Organisation` | `Competitor/@Organisation`, `Opponent/@Organisation` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm` |
| `SC @QualRule` | `ExtendedInfo Type="UI" Code="QUAL_RULE"` | Football sport-code catalog |

Known football standing values from the existing local code references:

### `SC @ResultType`

| Code | Meaning |
|---|---|
| `POINTS` | Points. In `DT_POOL_STANDING`, this means classification/table points. |
| `IRM_POINTS` | Points plus invalid result mark. |

### `SC @QualificationMark`

| Code | Meaning |
|---|---|
| `Q` | Qualified. |

### `SC @IRM`

| Code | Meaning |
|---|---|
| `DSQ` | Disqualified. |
| `DQB` | Disqualified for unsportsmanlike behaviour. |
| `DNS` | Did not start. |
| `WDR` | Withdrawn. |
| `ABD` | Abandoned. |

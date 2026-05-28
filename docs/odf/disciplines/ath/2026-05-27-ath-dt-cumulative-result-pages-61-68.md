# ODF ATH Data Dictionary: DT_CUMULATIVE_RESULT, Pages 61-68

Source: `C:\Users\mella\Downloads\ODF_ATH_Data_Dictionary.pdf`, pages 61-68.

Source version: `SOG-2024-ATH-3.4 APP`, dated 3 May 2024.

This note restructures the Athletics `DT_CUMULATIVE_RESULT` section into a practical reference for combined-event
overall standings and per-unit point summaries.

## 2.3.7 Cumulative Results

`DT_CUMULATIVE_RESULT` contains cumulative results for competitors across a group of units. In Athletics it is used only
for combined events, where it sends the overall score and rank during the combined event.

The cumulative message generally follows the same `ResultStatus` as the applicable `DT_RESULT`. When no unit is in
progress, cumulative results use `INTERMEDIATE`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Event` | Full event RSC. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_CUMULATIVE_RESULT` | Cumulative results message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `ResultStatus` | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROTESTED`, `PROVISIONAL` | Result lifecycle status. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Case | When to send |
|---|---|
| First combined-event unit | Trigger at the same time as the first unit start list. |
| Track combined-event units | Trigger after each completed unit. |
| Field combined-event units | Trigger after each attempt as `LIVE`. |
| No unit in progress | Send cumulative standings as `INTERMEDIATE`. |
| General rule | Send after each applicable `DT_RESULT`, using the same `ResultStatus` where applicable. |

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
    |   +-- SportDescription (0,1)
    |   +-- VenueDescription (0,1)
    +-- Result (1,N)
        +-- @Rank
        +-- @RankEqual
        +-- @Result
        +-- @ResultType
        +-- @IRM
        +-- @SortOrder
        +-- @Diff
        +-- ExtendedResults (0,1)
        +-- RecordIndicators (0,1)
        +-- ResultItems (0,1)
        |   +-- ResultItem (1,N)
        |       +-- @Unit
        |       +-- @Order
        |       +-- Result (1,1)
        |           +-- @Rank
        |           +-- @RankEqual
        |           +-- @ResultType
        |           +-- @Result
        |           +-- @ResultPoints
        |           +-- @IRM
        |           +-- @SortOrder
        |           +-- ExtendedResults (0,1)
        |           +-- RecordIndicators (0,1)
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Composition (1,1)
```

## ExtendedInfos

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competition/@Gen` | O | `S(20)` | General dictionary version. |
| `Competition/@Sport` | O | `S(20)` | Sport dictionary version. |
| `Competition/@Codes` | O | `S(20)` | Code-set version. |
| `Progress/@LastUnit` | O | `CC@Unit` | Most recently completed unit, or current unit if in progress. |
| `SportDescription/@DisciplineName` | M | `S(40)` | Discipline ENG description. |
| `SportDescription/@EventName` | M | `S(40)` | Event ENG description. |
| `SportDescription/@Gender` | M | `CC@SportGender` | Gender code. |
| `VenueDescription` | O | Venue text | Do not include unless all units are at a single venue and location. |

## Cumulative Result Values

| Attribute | Requirement | Value source | Meaning |
|---|---:|---|---|
| `Rank` | O | Text | Competitor rank in the cumulative result. Optional because the competitor can have an IRM. |
| `RankEqual` | O | `Y` | Sent when rank is tied. |
| `Result` | O | `###0` | Cumulative points, only when `ResultType="POINTS"`. |
| `ResultType` | O | `SC@ResultType` | Points or IRM result type for the cumulative result. |
| `IRM` | O | `SC@IRM` | Invalid rank mark, only when `ResultType` is IRM. |
| `SortOrder` | M | Numeric | Presentation order for cumulative results, including ties and unranked/IRM competitors. |
| `Diff` | O | `###0` | Points behind leader. Do not send for the leader. |

### Cumulative Extended Results

| Type | Code | Value | Meaning |
|---|---|---|---|
| `ER` | `RC` | `Y` | Red card for behaviour, if applicable. |
| `ER` | `YC` | `Y` | Yellow card for behaviour, if applicable. |
| `ER` | `YRC` | `Y` | Second yellow card for behaviour, if applicable. |

## Result Items

Each `ResultItem` identifies one unit included in the cumulative summary.

| Path / Attribute | Requirement | Value source | Meaning |
|---|---:|---|---|
| `ResultItem/@Unit` | M | `CC@Unit` | Full RSC of the unit/phase included in the summary. |
| `ResultItem/@Order` | M | `#0` | Logical unit order, usually schedule order. |
| `ResultItem/Result/@Rank` | O | Text | Rank in the unit identified by `ResultItem/@Unit`. |
| `ResultItem/Result/@ResultType` | O | `SC@ResultType` | Unit result type. |
| `ResultItem/Result/@Result` | O | `m:ss.ff` or `#0.00` | Unit performance. Time omits leading zeroes/minutes if zero; distance is metres. |
| `ResultItem/Result/@ResultPoints` | O | `###0` | Combined-event points for this unit. |
| `ResultItem/Result/@IRM` | O | `SC@IRM` | Unit invalid rank mark. |
| `ResultItem/Result/@SortOrder` | M | Numeric | Sort order in this unit. |

### Result Item Extended Results

| Type | Code | Value | Extra | Meaning |
|---|---|---|---|---|
| `ER` | `WIND_SPEED` | `+/-#0.0` | N/A | Wind in metres per second, where data exists in combined events. |
| `ER` | `TOTAL_PTS` | `###0` | `Rank`, `RankEqual`, `SortOrder` | Total points after the athlete has completed the unit. Not sent when the athlete did not start in that unit. |

## Competitor Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competitor/@Code` | M | `S(20)` | Competitor ID with no leading zeroes. |
| `Competitor/@Type` | M | `A` | Athlete competitor. |
| `Competitor/@Organisation` | M | `CC@Organisation` | Competitor organisation. |
| `Athlete/@Code` | M | `S(20)` | Athlete ID. |
| `Athlete/@Order` | M | Numeric | `1` for athlete competitor. |
| `Athlete/@Bib` | O | `S(4)` | Bib number. |
| `Athlete/Description/@FamilyName` | M | Text | Family name. |
| `Athlete/Description/@Gender` | M | `CC@PersonGender` | Athlete gender. |
| `Athlete/Description/@Organisation` | M | `CC@Organisation` | Athlete organisation. |

## Sample from the Dictionary, Normalized

```xml
<Competition Gen="OG2024-GEN-3.4 APP" Sport="SOG-2024-ATH-3.4 APP" Codes="OG2024">
  <ExtendedInfos>
    <Progress LastUnit="ATHMDECATH------------LJ----------"/>
    <SportDescription DisciplineName="Athletics" EventName="Men's Decathlon" Gender="M"/>
  </ExtendedInfos>
  <Result Rank="1" ResultType="POINTS" Result="1554" SortOrder="1">
    <ResultItems>
      <ResultItem Unit="ATHMDECATH------------100---------" Order="1">
        <Result Rank="27" ResultType="TIME" Result="10.23" ResultPoints="845" SortOrder="27">
          <ExtendedResults>
            <ExtendedResult Type="ER" Code="WIND_SPEED" Value="+1.3"/>
            <ExtendedResult Type="ER" Code="TOTAL_PTS" Value="845" Rank="27" SortOrder="27"/>
          </ExtendedResults>
        </Result>
      </ResultItem>
      <ResultItem Unit="ATHMDECATH------------LJ----------" Order="2">
        <Result Rank="18" ResultType="DISTANCE" Result="7.23" ResultPoints="709" SortOrder="18">
          <ExtendedResults>
            <ExtendedResult Type="ER" Code="WIND_SPEED" Value="-1.1"/>
            <ExtendedResult Type="ER" Code="TOTAL_PTS" Value="1554" Rank="1" SortOrder="1"/>
          </ExtendedResults>
        </Result>
      </ResultItem>
    </ResultItems>
    <Competitor Code="20217432" Type="A" Organisation="SUI">
      <Composition>
        <Athlete Code="20217432" Order="1" Bib="1234">
          <Description GivenName="Jon" FamilyName="Smith" Gender="M" Organisation="SUI" BirthDate="1994-12-15"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

## Message Sort

Sort by `Result/@SortOrder`, then by `Result/ResultItems/ResultItem/Result/@SortOrder`.

## XSD Validation

The normalized XML example above was validated as a `Competition` fragment wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- This message is specific to combined events. Do not use it for ordinary event rankings or unit results.
- The top-level `Result` row is the cumulative standing; nested `ResultItem/Result` rows are per-unit performances.
- `TOTAL_PTS` in a result item is the cumulative score after that unit, not just the points earned in the unit.
- `Diff` is points behind the leader and should be absent for the leader.
- Field-event combined results can update as `LIVE` after each attempt, so consumers should not assume cumulative
  updates only happen after completed units.

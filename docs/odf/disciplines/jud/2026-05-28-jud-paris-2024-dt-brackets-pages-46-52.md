# ODF JUD Data Dictionary: Paris 2024 DT_BRACKETS, Pages 46-52

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 46-52.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note restructures the Paris 2024 Judo `DT_BRACKETS` section for progression modeling.

## 2.3.6 Brackets

`DT_BRACKETS` contains bracket information for one event and explains how successive units are filled from previous
unit winners/losers or other progression rules.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Event` | Full event RSC. |
| `DocumentType` | `DT_BRACKETS` | Brackets message. |
| `Version` | `1..V` | Ascending content version. |
| `ResultStatus` | `CC@ResultStatus` | `START_LIST`, `INTERMEDIATE`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test. |
| `Date` / `Time` / `LogicalDate` | Date/time | Header generation and logical dates. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Status | Trigger |
|---|---|
| `START_LIST` | At competition beginning, as soon as brackets are available after the draw. |
| `INTERMEDIATE` | During competition until the gold medal match is completed unofficially. |
| `OFFICIAL` | When the last event unit, the gold medal match, is official. |

Also sent after changes. Team matches and individual bouts can trigger updates when completed.

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- SportDescription (0,1)
    |   +-- VenueDescription (0,1)
    +-- Bracket (1,N)
        +-- @Code
        +-- BracketItems (1,N)
            +-- @Code
            +-- BracketItem (1,N)
                +-- @Code
                +-- @Order
                +-- @Position
                +-- @Date
                +-- @Unit
                +-- @Result
                +-- ExtBracketItems (0,1)
                +-- CompetitorPlace (1,N)
                    +-- @Pos
                    +-- @Code
                    +-- @WLT
                    +-- @Result
                    +-- @ResultType
                    +-- @IRM
                    +-- @StrikeOut
                    +-- ExtCompPlaces (0,1)
                    +-- PreviousUnit (0,1)
                    +-- Competitor (0,1)
```

## Message Values

### Descriptions

`SportDescription` carries discipline, event, and gender display fields. `VenueDescription` carries venue code and
name.

### `Bracket` and `BracketItems`

| Path | Attribute | M/O | Value | Meaning |
|---|---|---|---|---|
| `Bracket` | `Code` | M | `SC@Bracket` | Bracket code, such as finals. |
| `BracketItems` | `Code` | M | `SC@BracketItems` | Grouping of bracket items. |

### `BracketItem`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | O | `S(3)` or `TBD` | Contest number or unknown marker. |
| `Order` | M | Numeric | Sequential order inside the bracket item group. |
| `Position` | M | Numeric | Draw position used to render the bracket. |
| `Date` | O | Date | Match date, if known. |
| `Unit` | O | `CC@Unit` | Full unit RSC. |
| `Result` | O | `S(50)` | Final ORIS-format result for the unit. |

### `ExtBracketItem`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EBI` | `DECISION` | N/A | When available in individual. | `SC@ResultCode` | Contest decision. |
| `EBI` | `TECH_CODE` | N/A | When available in individual. | `SC@Technique` | Winning technique. |
| `EBI` | `LOCATION` | N/A | Always when available. | `CC@Location` | Location code. |

### `CompetitorPlace`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Pos` | M | Numeric | Competitor position in the bracket item. |
| `Code` | O | `SC@CompetitorPlace` | `BYE`, `TBD`, or similar when no known competitor. |
| `WLT` | O | `SC@WLT` | Winner/loser indicator when known. |
| `Result` | O | `S(10)` | Individual PP-format result, or numeric submatches won for teams. |
| `ResultType` | O | `SC@ResultType` | Result type. |
| `IRM` | O | `SC@IRM` | Invalid rank mark. |
| `StrikeOut` | O | `Y` | Strike out this competitor, usually for `DQB`. |

### `ExtCompPlace`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `ER` | `PENALTY` | N/A | If applicable. | `SC@PenaltyType` | Penalty code associated to individual score. |

### `PreviousUnit`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Unit` | O | `CC@Unit` | Previous event unit RSC when coming from a single unit. |
| `Value` | O | `S(2)` | Match number when the competitor is not known. |
| `WLT` | O | `SC@WLT` | Winner/loser from the previous unit; omit when coming from a pool. |

### `Competitor`

Competitors can be athletes or teams (`Type="A"` or `Type="T"`). Team competitors can carry `Description/@TeamName`
and `@IFId`; known athletes appear under `Composition/Athlete`.

## Sample, Normalized

```xml
<BracketItem Code="131" Order="2" Position="2" Date="2016-08-09"
             Unit="JUDM54KG--------------SFNL0001----"
             Result="110s1/ 0s1 VVV 5:00">
  <CompetitorPlace Pos="1" WLT="W" Result="110s1">
    <PreviousUnit Unit="JUDM54KG--------------QFNL0001----"/>
    <Competitor Code="1066978" Type="A" Organisation="ESP">
      <Composition>
        <Athlete Code="1066978" Order="1">
          <Description FamilyName="Black" GivenName="John" Gender="M" Organisation="ESP" BirthDate="1991-12-16"/>
        </Athlete>
      </Composition>
    </Competitor>
  </CompetitorPlace>
</BracketItem>
```

## Message Sort

Sort by `Bracket/@Code` using sport-code order, then `Bracket/BracketItems/@Code` descending using sport-code order,
then `BracketItem/@Position`.

## Modeling Notes

- Use `PreviousUnit` to build graph edges for progression. Omit `WLT` when progression comes from a pool.
- For team brackets, `CompetitorPlace/@Result` is the number of submatches won, not an individual PP-format score.
- Preserve `Position` separately from `Order`; position is for drawing the bracket shape.
- `StrikeOut="Y"` is usually a disqualification presentation concern but should remain attached to the competitor
  place.

## Code Appendix: Paris 2024 Values

Catalog values come from Paris 2024 CC/SC code tables; message-specific restrictions remain in the field tables above.

| Code Entity | Section Usage | Values |
|---|---|---|
| `DocumentType` | Header | `DT_BRACKETS` |
| `CC@ResultStatus` | Header | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNCONFIRMED`, `UNOFFICIAL`, `OFFICIAL`, `PARTIAL`, `PROTESTED`, `PROVISIONAL` |
| `SC@Bracket` | Bracket code | `FNL`, `BRN1`, `BRN2` |
| `SC@BracketItems` | Bracket item grouping | `FNL-`, `REPF`, `REP1`, `SFNL`, `QFNL`, `8FNL`, `R32-`, `R64-` |
| `BracketItem/@Code` | Unknown contest | `TBD` |
| `SC@CompetitorPlace` | Placeholder | `BYE`, `NCT`, `NOAWARD`, `NOCOMP`, `TBD` |
| `ExtBracketItem/@Code` | Bracket item extensions | `DECISION`, `TECH_CODE`, `LOCATION` |
| `ExtCompPlace/@Code` | Competitor place extension | `PENALTY` |
| `SC@ResultCode` | Bracket item decision | `FUS`, `IPP`, `KIK`, `PEN`, `WAZ`, `YUK` |
| `SC@PenaltyType` | Competitor place penalty | `H`, `S`, `S3`, `X`, `s1`, `s2`, `s3` |
| `SC@ResultType` | Competitor place result type | `IRM`, `IRM_POINTS`, `POINTS` |
| `SC@IRM` | Invalid result mark | `DNS`, `DQB`, `DSQ`, `WDR` |
| `SC@WLT` | Win/loss | `W`, `L` |
| `StrikeOut` | Competitor place | `Y`; `DQB` mentioned as common use. |
| `Competitor/@Type` | Competitor kind | `A`, `T` |
| `SC@Technique` | Winning technique | Large JUD technique catalog in SportCodes; keep as code reference instead of hard-coding a partial list. |

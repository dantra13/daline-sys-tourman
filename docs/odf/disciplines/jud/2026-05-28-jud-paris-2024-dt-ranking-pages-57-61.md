# ODF JUD Data Dictionary: Paris 2024 DT_RANKING, Pages 57-61

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 57-61.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note restructures the Paris 2024 Judo event final ranking message.

## 2.3.8 Event Final Ranking

`DT_RANKING` contains final results and ranking at completion of one event, for individual athletes or aggregated
athletes/teams. It includes competitors with ranks, invalid-rank marks, or both.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Event` | Full event RSC. |
| `DocumentType` | `DT_RANKING` | Event final ranking. |
| `Version` | `1..V` | Ascending content version. |
| `ResultStatus` | `CC@ResultStatus` | `PARTIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test. |
| `Date` / `Time` / `LogicalDate` | Date/time | Header generation and logical dates. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

Sent only after a unit affecting final ranking is official and that ranking is no longer subject to change. Lower-ranked
athletes can be sent after the morning session and then after each afternoon match. Trigger again after any change.

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
    +-- Result (1,N)
        +-- @Rank
        +-- @RankEqual
        +-- @Played
        +-- @Won
        +-- @Lost
        +-- @IRM
        +-- @SortOrder
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Description (0,1)
            +-- Composition (1,1)
                +-- Athlete (0,N)
```

## Message Values

### Descriptions

| Path | Key Attributes |
|---|---|
| `SportDescription` | `DisciplineName`, `EventName`, `Gender`. |
| `VenueDescription` | `Venue`, `VenueName`. |

### `Result`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Rank` | O | Text | Final rank. |
| `RankEqual` | O | `S(1)` | Send only for equal ranks. |
| `Played` | O | `#0` | Number of contests for the competitor, not within a team. |
| `Won` | O | `#0` | Number of contests won, not within a team. |
| `Lost` | O | `#0` | Number of contests lost, not within a team. |
| `IRM` | O | `SC@IRM` | Invalid rank mark. |
| `SortOrder` | M | Numeric | Presentation order, based mostly on rank but also tie/unranked ordering. |

### `Competitor`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | ID or `SC@CompetitorPlace` | Competitor ID or `NOAWARD` when the place is not awarded. |
| `Type` | M | `A`, `T` | Athlete or team. |
| `Organisation` | O | `CC@Organisation` | Competitor organisation. |

Team/group competitors use `Description/@TeamName`. Athlete composition can list individual athletes and descriptions.

## Sample, Normalized

```xml
<Result Rank="1" SortOrder="1">
  <Competitor Code="1106858" Type="A" Organisation="SUI">
    <Composition>
      <Athlete Code="1106858" Order="1">
        <Description GivenName="John" FamilyName="Smith" Gender="M" Organisation="SUI" BirthDate="1994-12-15"/>
      </Athlete>
    </Composition>
  </Competitor>
</Result>
<Result SortOrder="2" IRM="DQB">
  <Competitor Code="1090697" Type="A" Organisation="ESP">
    <Composition>
      <Athlete Code="1090697" Order="1">
        <Description GivenName="James" FamilyName="Black" Gender="M" Organisation="ESP" BirthDate="1994-12-16"/>
      </Athlete>
    </Composition>
  </Competitor>
</Result>
```

## Message Sort

Sort by `Result/@SortOrder`.

## Modeling Notes

- `Played`, `Won`, and `Lost` are not for contests within a team; keep team member contest results in `DT_RESULT`.
- `NOAWARD` is a competitor-place value, not a participant/team ID.
- `SortOrder` is mandatory and should drive presentation even when ranks are tied or missing.
- Support both athlete and team competitors because Judo ranking applies to individual and team events.

## Code Appendix: Paris 2024 Values

Catalog values come from Paris 2024 CC/SC code tables; message-specific restrictions remain in the field tables above.

| Code Entity | Section Usage | Values |
|---|---|---|
| `DocumentType` | Header | `DT_RANKING` |
| `CC@ResultStatus` | Header | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNCONFIRMED`, `UNOFFICIAL`, `OFFICIAL`, `PARTIAL`, `PROTESTED`, `PROVISIONAL` |
| `SC@CompetitorPlace` | Unawarded place | `BYE`, `NCT`, `NOAWARD`, `NOCOMP`, `TBD` |
| `Competitor/@Type` | Competitor kind | `A`, `T` |
| `SC@IRM` | Invalid rank mark | `DNS`, `DQB`, `DSQ`, `WDR` |

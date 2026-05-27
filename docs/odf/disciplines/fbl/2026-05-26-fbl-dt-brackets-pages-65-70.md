# ODF FBL Data Dictionary: DT_BRACKETS, Pages 65-70

Source: `C:\Users\mella\WebstormProjects\sportivo\docs\references\odf\ODF_FBL_Data_Dictionary.pdf`, pages 65-70.

This note restructures section `2.3.8 Brackets` from the Paris 2024 Football Data Dictionary into a football-specific reference for Sportivo domain modeling. It should be read alongside the general interface reference in `2026-05-26-gen-dt-brackets-pages-190-200.md`.

## 2.3.8 Brackets

`DT_BRACKETS` contains bracket information for a football event or for a component of an event, such as a phase or unit. It is used when consumers need to know the progress of the competition.

In early stages, the message indicates how competition progress will proceed from winners or losers. As the competition advances, it is updated with the teams placed in each bracket item.

For football, `DT_BRACKETS` is scoped to the event by `DocumentCode`, which is the full event RSC.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC @Competition` | Unique competition ID. |
| `DocumentCode` | `CC @Event` | Full event RSC. |
| `DocumentType` | `DT_BRACKETS` | Brackets message. |
| `Version` | `1..V` | Ascending version number associated with the message content. |
| `ResultStatus` | `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` | Status of the bracket message. |
| `FeedFlag` | `P` or `T` | Production or test message. |
| `Date` | Date | Generation date in the local time zone where the message was produced. |
| `Time` | Time | Generation time up to milliseconds in the local time zone where the message was produced. |
| `LogicalDate` | Date | Logical date of events. Usually the same as the physical day unless the unit or message transmission extends after midnight. |
| `Source` | `SC @Source` | Code of the system that generated the message. |

## Trigger and Frequency

The football dictionary says this message should be sent at the very beginning of a competition, as soon as brackets are available.

Send:

- Before the competition.
- After every preliminary match that determines a position in the bracket.
- After every match during final phases.
- When a match/event unit is completed unofficial and again when official if there was any change.
- After any change.

| Competition condition | Expected `ResultStatus` |
|---|---|
| Before the start of the competition | `START_LIST` |
| During the competition, until the last event unit/gold medal match is unofficial | `INTERMEDIATE` |
| Last event unit/gold medal match is unofficial | `UNOFFICIAL` |
| Last event unit/gold medal match is official | `OFFICIAL` |
| Pending decision applies | `PROVISIONAL` |

Unlike the OWG2026 GEN interface, this FBL 2024 section does not list `UNCONFIRMED` or `PROTESTED` for football brackets.

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- Progress (0,1)
    |   +-- SportDescription (0,1)
    +-- Bracket (1,N)
        +-- @Code
        +-- BracketItems (1,N)
            +-- @Code
            +-- BracketItem (1,N)
                +-- @Code
                +-- @Order
                +-- @Position
                +-- @Date
                +-- @Time
                +-- @Unit
                +-- @Result
                +-- CompetitorPlace (1,N)
                    +-- @Pos
                    +-- @Code
                    +-- @WLT
                    +-- @Result
                    +-- @ResultType
                    +-- @IRM
                    +-- @StrikeOut
                    +-- PreviousUnit (0,1)
                    |   +-- @Unit
                    |   +-- @Value
                    |   +-- @WLT
                    +-- Competitor (0,1)
                        +-- @Code
                        +-- @Type
                        +-- @Organisation
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

### `Competition/ExtendedInfos/Progress`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `LastUnit` | O | `CC @Unit` | Full RSC of the most recently completed unit in the event. |
| `UnitsTotal` | O | Numeric `##0` | Total number of units to be played in the event. |
| `UnitsComplete` | O | Numeric `##0` | Number of official units out of `UnitsTotal`. |

### `Competition/ExtendedInfos/SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `DisciplineName` | M | `S(40)` | Discipline English description from Common Codes, not code. |
| `EventName` | M | `S(40)` | Event English description from Common Codes, not code. |
| `Gender` | M | `CC @SportGender` | Gender code for the event unit. |

### `Competition/Bracket`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `SC @Bracket` | Bracket code identifying an individual bracket as defined in ORIS. Examples include finals and bronze/classification branches. |

Football has one `Bracket` for each individual bracket branch defined by ORIS. A bronze bracket is a different code from the bracket leading to gold, assuming bronze matches are played.

### `Competition/Bracket/BracketItems`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `SC @BracketItems` | Code identifying a set of bracket items, such as quarterfinals, semifinals, or finals. |

### `Competition/Bracket/BracketItems/BracketItem`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | O | Numeric `#0` | Unique bracket item identifier. In football this is the game number, such as `17`, `18`, `19`, `20`. |
| `Order` | M | Numeric `##0` | Sequential number inside `BracketItems`. Always starts at `1`. |
| `Position` | M | Numeric `##0` | Visual bracket position. For quarterfinals, positions are `1`, `2`, `3`, `4` from top to bottom. If a hidden item would logically be first, visible positions can start at `2`. |
| `Date` | O | Date | `YYYY-MM-DD`. Must be filled if known. |
| `Time` | O | `S(5)` | `HH:MM`. Must be filled if known. |
| `Unit` | O | `CC @Unit` | Full RSC of the unit for the bracket item. |
| `Result` | O | `S(50)` | Match result if complete, formatted as in ORIS, for example `4-0 (0-0)`. May include an IRM. Must be included if available and match is complete. |

### `Competition/Bracket/BracketItems/BracketItem/CompetitorPlace`

`CompetitorPlace` is the slot in a bracket item.

- If competitors are known, it places teams in the bracket.
- If competitors are not yet known, it contains information about the rule used to access the bracket.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Pos` | M | Numeric | Sequential slot position in the bracket item, `1` or `2` for football. |
| `Code` | O | `SC @CompetitorPlace` | Sent when there is no competitor team (`BYE`) or when the team is not known yet (`TBD`). Can also encode source places such as `A1`, `W23`, or `L24`. |
| `WLT` | O | `SC @WLT` | `W` or `L`; indicates winner or loser of the bracket item. Always send when known. |
| `Result` | O | `S(10)` | Team score in the event unit. If decided by penalty shoot-out, send format `x(y)` where `y` is the team's PSO score. |
| `ResultType` | O | `SC @ResultType` | Type of `Result`. |
| `IRM` | O | `SC @IRM` | Invalid rank/result mark, if applicable. |
| `StrikeOut` | O | `S(1)` | Send `Y` if the competitor should be struck out, usually for `DQB`. |

### `PreviousUnit`

`PreviousUnit` points backward to the previous event unit or pool that feeds the current slot.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Unit` | O | `CC @Unit` | Full RSC of the previous event unit for the competitor place. Must be sent when the slot receives a winner/loser from a single unit. If coming from a pool, this is the RSC of the pool. |
| `Value` | O | `SC@Pool` or `S(6)` | If the unknown competitor comes from a pool or previous matches, fill with the pool code or match number as appropriate. |
| `WLT` | O | `S(1)` | If the unknown competitor comes from an earlier bracket item, send `W` or `L` when known. Do not send if the competitor comes from a pool. |

### `Competitor`

Only include `Competitor` if the team is known.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | M | `S(1)` | `T` for team. |
| `Organisation` | O | `CC @Organisation` | Competitor organisation, if known. |

#### `Competitor/Description`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `TeamName` | M | `S(73)` | Team name. |

## Sample from the Dictionary, Normalized

The PDF provides a partial `BracketItem` sample for a women's semifinal.

```xml
<Bracket Code="FNL-">
  <BracketItems Code="SFNL">
    <BracketItem Code="33"
                 Order="1"
                 Position="1"
                 Date="2012-08-10"
                 Time="15:00"
                 Result="2-1"
                 Unit="FBLWTEAM11------------SFNL000100--">
      <CompetitorPlace Pos="1" WLT="W" Result="2">
        <Competitor Code="FBLWTEAM11----NED01" Type="T" Organisation="NED">
          <Description TeamName="Netherlands"/>
        </Competitor>
      </CompetitorPlace>
      <CompetitorPlace Pos="2" WLT="L" Result="1">
        <Competitor Code="FBLWTEAM11----NZL01" Type="T" Organisation="NZL">
          <Description TeamName="New Zealand"/>
        </Competitor>
      </CompetitorPlace>
    </BracketItem>
  </BracketItems>
</Bracket>
```

### Sample Reading

| Field | Meaning |
|---|---|
| `Bracket Code="FNL-"` | Finals bracket branch in the sample. |
| `BracketItems Code="SFNL"` | Semifinals stage within that bracket. |
| `BracketItem Code="33"` | Game/bracket item identifier. |
| `Unit="FBLWTEAM11------------SFNL000100--"` | The football unit represented by this bracket item. |
| `CompetitorPlace Pos="1" WLT="W" Result="2"` | Netherlands occupies slot 1 and won 2-1. |
| `CompetitorPlace Pos="2" WLT="L" Result="1"` | New Zealand occupies slot 2 and lost 1-2. |

## Normalized Full-Message Example

The dictionary sample is partial. The example below wraps the same shape in a compact full `OdfBody` and adds an example future final slot fed by a semifinal winner.

```xml
<OdfBody CompetitionCode="OG2024"
         DocumentCode="FBLWTEAM11------------------------"
         DocumentType="DT_BRACKETS"
         Version="7"
         ResultStatus="INTERMEDIATE"
         FeedFlag="T"
         Date="2012-08-10"
         Time="17:10:00"
         LogicalDate="2012-08-10"
         Source="OVR">
  <Competition Gen="SOG-2024-GEN" Sport="SOG-2024-FBL-3.4" Codes="SOG-2024-CODES">
    <ExtendedInfos>
      <Progress LastUnit="FBLWTEAM11------------SFNL000100--"
                UnitsTotal="8"
                UnitsComplete="5"/>
      <SportDescription DisciplineName="Football"
                        EventName="Women's Team"
                        Gender="W"/>
    </ExtendedInfos>

    <Bracket Code="FNL-">
      <BracketItems Code="SFNL">
        <BracketItem Code="33"
                     Order="1"
                     Position="1"
                     Date="2012-08-10"
                     Time="15:00"
                     Result="2-1"
                     Unit="FBLWTEAM11------------SFNL000100--">
          <CompetitorPlace Pos="1" WLT="W" Result="2" ResultType="POINTS">
            <Competitor Code="FBLWTEAM11----NED01" Type="T" Organisation="NED">
              <Description TeamName="Netherlands"/>
            </Competitor>
          </CompetitorPlace>
          <CompetitorPlace Pos="2" WLT="L" Result="1" ResultType="POINTS">
            <Competitor Code="FBLWTEAM11----NZL01" Type="T" Organisation="NZL">
              <Description TeamName="New Zealand"/>
            </Competitor>
          </CompetitorPlace>
        </BracketItem>
      </BracketItems>

      <BracketItems Code="FNL-">
        <BracketItem Code="35"
                     Order="1"
                     Position="1"
                     Unit="FBLWTEAM11------------FNL-000100--">
          <CompetitorPlace Pos="1" Code="W33">
            <PreviousUnit Unit="FBLWTEAM11------------SFNL000100--" Value="33" WLT="W"/>
          </CompetitorPlace>
          <CompetitorPlace Pos="2" Code="TBD">
            <PreviousUnit Unit="FBLWTEAM11------------SFNL000200--" Value="34" WLT="W"/>
          </CompetitorPlace>
        </BracketItem>
      </BracketItems>
    </Bracket>
  </Competition>
</OdfBody>
```

## Message Sort

Sort as follows:

1. `Bracket/@Code`, using order in sport codes.
2. `Bracket/BracketItems/@Code`, descending, using order in sport codes.
3. `Bracket/BracketItems/BracketItem/@Position`.

## Comparison with the GEN Interface

The FBL message follows the same main hierarchy as GEN:

```text
Bracket > BracketItems > BracketItem > CompetitorPlace > Competitor
```

The football dictionary is narrower and more concrete:

| Area | GEN interface | FBL dictionary |
|---|---|---|
| Header scope | `DocumentCode=CC@EVENT`. | Same: full event RSC. |
| `ResultStatus` | Includes `START_LIST`, `INTERMEDIATE`, `UNCONFIRMED`, `UNOFFICIAL`, `OFFICIAL`, `PROTESTED`, `PROVISIONAL`. | Lists `START_LIST`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| Competition versions | GEN 2026 marks `Gen` and `Codes` mandatory, `Sport` optional. | FBL 2024 marks `Gen`, `Sport`, and `Codes` optional. |
| `ExtendedInfos` | Allows `ExtendedInfo`, extension nodes, progress, sport and venue descriptions. | Only `Progress` and `SportDescription` are in the FBL message structure. |
| Venue description | Available in GEN. | Not listed in FBL. |
| `BracketItem/@TimeStamp` | Available in GEN. | Not listed in FBL. |
| `CompetitorPlace` attributes | GEN includes `Rank`, `Diff`, `QualificationMark`, `StartOrder`, `PhotoFinish`. | FBL lists only `Pos`, `Code`, `WLT`, `Result`, `ResultType`, `IRM`, `StrikeOut`. |
| `Competitor` attributes | GEN allows `Seed`, `Organisation`, `Bib`, athlete composition, guide, extensions. | FBL only lists team `Code`, `Type`, `Organisation`, and `Description/@TeamName`. |
| Placeholder/source codes | Sport-specific. | Uses `SC @CompetitorPlace`, including values such as `TBD`, `BYE`, pool placements, and winner/runner-up placeholders. |
| `PreviousUnit/@Value` | Sport-specific. | `SC@Pool` or `S(6)`; use pool code or match number as appropriate. |

## XSD Notes

The OWG2026 draft XSD package is generic and broader than this FBL 2024 message table.

Relevant drift:

- The XSD allows `BracketItem/@TimeStamp`; FBL 2024 does not list it.
- The XSD allows broader `CompetitorPlace` attributes such as `Rank`, `Diff`, `QualificationMark`, `StartOrder`, and `PhotoFinish`; FBL 2024 does not list them.
- The XSD allows `Competitor/@Seed` and `@Bib`, plus composition and extension nodes; FBL 2024 does not list them for `DT_BRACKETS`.
- FBL semantic validation should follow this FBL dictionary, while XSD validation can remain generic.

## Modeling Notes

- Model `DT_BRACKETS` as the published football knockout/progression graph for an event.
- The event key is `OdfBody/@DocumentCode`, not a phase or unit.
- Use `Bracket/@Code` to separate finals and bronze/classification branches.
- Use `BracketItems/@Code` as the round/stage grouping, for example quarterfinals, semifinals, finals.
- Use `BracketItem` for the individual match node and `BracketItem/@Unit` to link to the scheduled/result unit.
- Use `CompetitorPlace` as the slot. It can exist before a team is known.
- Use `CompetitorPlace/@Code` for placeholders such as `TBD`, `BYE`, `W33`, `L24`, `A1`, etc.
- Add `Competitor` only when the team is known.
- Preserve `PreviousUnit` to explain how the slot is fed. It is the canonical backward progression link.
- For pool-to-bracket slots, use the pool RSC in `PreviousUnit/@Unit` and a pool placement value in `PreviousUnit/@Value`; do not send `PreviousUnit/@WLT`.
- For previous-match slots, use the previous unit RSC and `PreviousUnit/@WLT` to indicate winner or loser.
- Football does not model two-legged aggregate ties in this ODF dictionary. Keep aggregate ties as a Sportivo concept if needed.

## Code Appendix: Referenced SC and CC Entities

The PDF pages link to the Paris 2024 code pages below. Resolve authoritative values from the target code release.

| Reference | Used for | Source |
|---|---|---|
| `CC @Competition` | `OdfBody/@CompetitionCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm` |
| `CC @Event` | `OdfBody/@DocumentCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Event.htm` |
| `CC @ResultStatus` | `OdfBody/@ResultStatus` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm` |
| `SC @Source` | `OdfBody/@Source` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm` |
| `CC @Unit` | `Progress/@LastUnit`, `BracketItem/@Unit`, `PreviousUnit/@Unit` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm` |
| `SC @Bracket` | `Bracket/@Code` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Bracket_SOG_FBL.htm` |
| `SC @BracketItems` | `BracketItems/@Code` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BracketItems_SOG_FBL.htm` |
| `SC @CompetitorPlace` | `CompetitorPlace/@Code` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_CompetitorPlace_GEN.htm` |
| `SC @WLT` | `CompetitorPlace/@WLT`, `PreviousUnit/@WLT` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_WLT_SOG_FBL.htm` |
| `SC @ResultType` | `CompetitorPlace/@ResultType` | Football sport-code catalog |
| `SC @IRM` | `CompetitorPlace/@IRM` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_FBL.htm` |
| `CC @Organisation` | `Competitor/@Organisation` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm` |

### `SC @Bracket`

| Code | Order | ENG Description |
|---|---:|---|
| `FNL` | 1 | Finals |
| `BRN` | 2 | Bronze |

### `SC @BracketItems`

| Code | Order | ENG Description |
|---|---:|---|
| `FNL-` | 1 | Finals |
| `SFNL` | 2 | Semi-finals |
| `QFNL` | 3 | Quarter-finals |

### `SC @WLT`

| Code | Meaning |
|---|---|
| `W` | Winner |
| `L` | Runner-up |
| `T` | Drawn |

### `SC @ResultType`

| Code | Meaning |
|---|---|
| `POINTS` | Goals in football result contexts. |
| `IRM_POINTS` | Points plus invalid result mark. |

### `SC @CompetitorPlace`

Common football placeholder/source values include:

| Code | Meaning |
|---|---|
| `TBD` | To be determined. |
| `BYE` | No competitor; the other team goes directly to the next phase/round. |
| `NOCOMP` | No competitor. |
| `A1`, `A2`, `B1`, `B2`, etc. | Pool placement sources. |
| `W19`, `W20`, etc. | Winner of match/game number. |
| `L23`, `L24`, etc. | Runner-up/loser of match/game number. |

The individual `SC @CompetitorPlace` link exposed by the PDF returned 404 during extraction. The values above are taken from the existing local Paris 2024 FBL aggregate sport-code references already extracted in this repository.

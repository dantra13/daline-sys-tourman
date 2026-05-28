# ODF BKB Data Dictionary: DT_BRACKETS, Pages 70-74

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 70-74.
Source version: `SOG-2024-BKB-3.4 APP`, dated 19 January 2024.

This note restructures section `2.3.9 Brackets` from the Paris 2024 Basketball Data Dictionary into a basketball-specific reference for Sportivo domain modeling. The discipline overview lists the message under the plural name `DT_BRACKETS`; this spelling is preserved here.

## 2.3.9 Brackets

`DT_BRACKETS` carries the bracket structure of a basketball event. It is used when consumers need to know in advance how successive event units will be filled as the competition advances from the preliminary stage into the knockout stage.

In early stages the message indicates how each event unit will be built from winners, losers, or other progression rules of earlier event units. As the competition advances, the placeholders are progressively replaced by the actual teams.

For basketball, `DT_BRACKETS` is scoped to the event by `DocumentCode`, which is the full event RSC.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC @Competition` | Unique competition ID. |
| `DocumentCode` | `CC @Event` | Full event RSC. |
| `DocumentType` | `DT_BRACKETS` | Brackets message. |
| `Version` | `1..V` | Ascending version number associated with the message content. |
| `ResultStatus` | `CC @ResultStatus` | Status of the message. See trigger table. |
| `FeedFlag` | `P` or `T` | Production or test message. |
| `Date` | Date | Generation date in the local time zone where the message was produced. |
| `Time` | Time | Generation time up to milliseconds in the local time zone where the message was produced. |
| `LogicalDate` | Date | Logical date of events. Usually the same as the physical day unless the unit or message transmission extends past midnight. |
| `Source` | `SC @Source` | Code of the system that generated the message. |

## Trigger and Frequency

The basketball dictionary lists the following triggers:

- Before the competition.
- After every match in the preliminaries that determines a position in the bracket.
- After every match during final phases except the last.
- After the last match.
- After any change.

| Competition condition | Expected `ResultStatus` |
|---|---|
| Before the start of the competition | `START_LIST` |
| During the competition (preliminaries placing teams in the bracket, and all final-phase matches except the last) | `INTERMEDIATE` |
| Last match is unofficial | `UNOFFICIAL` |
| All matches are official | `OFFICIAL` |
| Pending decision applies | `PROVISIONAL` |

The BKB 2024 dictionary does not list `UNCONFIRMED` or `PROTESTED` for this message.

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
| `UnitsComplete` | O | Numeric `##0` | Number of units that are official out of `UnitsTotal`. |

### `Competition/ExtendedInfos/SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `DisciplineName` | M | `S(40)` | Discipline English description from Common Codes, not code. |
| `EventName` | M | `S(40)` | Event English description from Common Codes, not code. |
| `Gender` | M | `CC @SportGender` | Gender code for the event unit. |

### `Competition/Bracket`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `SC @Bracket` | Bracket code identifying an individual bracket as defined in ORIS. Basketball uses one `Bracket` for the finals branch and a separate `Bracket` for the bronze branch. |

### `Competition/Bracket/BracketItems`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `SC @BracketItems` | Code identifying a set of bracket items, such as quarterfinals, semifinals, or finals. |

### `Competition/Bracket/BracketItems/BracketItem`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | O | Numeric `#0` | Unique bracket item identifier. In basketball this is the game number, e.g. `17`, `18`, `19`, `20`. |
| `Order` | M | Numeric `##0` | Sequential number inside `BracketItems`. Always starts at `1`. |
| `Position` | M | Numeric `##0` | Visual bracket position. For quarterfinals, positions are `1`, `2`, `3`, `4` from top to bottom. If a hidden item would logically be first, visible positions can start at `2`. |
| `Date` | O | Date | `YYYY-MM-DD`. Must be filled if known. |
| `Time` | O | `S(5)` | `HH:MM`. Must be filled if known. Disciplines that determine match times only after competitors are known may withhold the time until competitor pairings and schedule are confirmed. |
| `Unit` | O | `CC @Unit` | Full RSC of the unit for the bracket item. |
| `Result` | O | `S(50)` | Match result if complete, formatted as in ORIS. If the match is cancelled, send `Cancelled`. |

### `Competition/Bracket/BracketItems/BracketItem/CompetitorPlace`

`CompetitorPlace` is the slot in a bracket item.

- If competitors are known, it places teams in the bracket.
- If competitors are not yet known, it carries information about the rule used to access the bracket.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Pos` | M | Numeric `##0` | Sequential slot position in the bracket item, `1` or `2` for basketball. |
| `Code` | O | `SC @CompetitorPlace` | Sent when there is no competitor team (`BYE`) or when the team is not known yet (`TBD`). |
| `WLT` | O | `SC @WLT` | `W` or `L`; winner or loser of the bracket item. Always send when known. |
| `Result` | O | `S(10)` | Team score in the event unit. |
| `ResultType` | O | `SC @ResultType` | Type of `Result`. |
| `IRM` | O | `SC @IRM` | Invalid rank/result mark, if applicable. |
| `StrikeOut` | O | `S(1)` | Send `Y` if the competitor should be struck out, usually for `DQB`. |

### `PreviousUnit`

`PreviousUnit` points backward to the previous event unit or pool that feeds the current slot. It is informed except for bracket items whose slot has no preceding event unit and does not come from a pool.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Unit` | O | `CC @Unit` | Full RSC of the previous event unit for the slot. If from a pool, this is the RSC of the pool. |
| `Value` | O | `SC@Pool` or `S(6)` | If the unknown competitor comes from a pool or previous matches, fill with the pool code or match number as appropriate. |
| `WLT` | O | `S(1)` | If the unknown competitor comes from an earlier bracket item, send `W` or `L`. Do not send if the competitor comes from a pool. |

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

## Bracket Branches and Bronze Placement

Basketball publishes one `Bracket` per ORIS-defined branch:

| `Bracket/@Code` | Meaning |
|---|---|
| `FNL` | Finals branch ending at the gold-medal match. |
| `BRN` | Bronze branch ending at the bronze-medal match. |

The bronze-medal match is therefore not nested under the finals branch; it sits in its own `Bracket Code="BRN"` tree. Each branch contains its own ordered set of `BracketItems` (e.g. `QFNL`, `SFNL`, `FNL-`), with `BracketItems/@Code` ordering defined by `SC @BracketItems`.

Bracket placeholder codes such as `W19`, `L23`, `A1`, `B2`, `CR1-2`, etc. flow into `CompetitorPlace/@Code` (or `PreviousUnit/@Value` for pool sources). `@WLT` indicates winner or loser linkage from a previous match unit.

## Sample from the Dictionary, Normalized

The PDF provides a partial `BracketItem` sample for a women's semifinal. The original sample omits `Result`, `WLT`, and the closing `</BracketItems>/</Bracket>` tags; it is normalized below.

```xml
<Bracket Code="FNL">
  <BracketItems Code="SFNL">
    <BracketItem Code="33"
                 Order="1"
                 Position="1"
                 Date="2012-08-10"
                 Time="15:00"
                 Unit="BKBWTEAM5-------------SFNL000100--">
      <CompetitorPlace Pos="1">
        <Competitor Code="BKBWTEAM5-----NED01" Type="T" Organisation="NED">
          <Description TeamName="Netherlands"/>
        </Competitor>
      </CompetitorPlace>
      <CompetitorPlace Pos="2">
        <Competitor Code="BKBWTEAM5-----NZL01" Type="T" Organisation="NZL">
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
| `Bracket Code="FNL"` | Finals branch in the sample. |
| `BracketItems Code="SFNL"` | Semifinals stage within that branch. |
| `BracketItem Code="33"` | Game/bracket item identifier. |
| `Unit="BKBWTEAM5-------------SFNL000100--"` | The basketball unit represented by this bracket item. |
| `CompetitorPlace Pos="1"` | Netherlands occupies slot 1. |
| `CompetitorPlace Pos="2"` | New Zealand occupies slot 2. |

PDF anomaly: the dictionary sample uses `Bracket Code="FNL-"`, but the BKB sport codes register the bracket branch as `FNL` (with `FNL-` reserved for `@BracketItems`). The normalized sample uses `Bracket Code="FNL"` and `BracketItems Code="SFNL"` to match the sport-code catalog.

## Normalized Full-Message Example

The dictionary sample is partial. The example below wraps the same shape in a compact full `OdfBody` and adds an example future gold-medal slot fed by the semifinal winner, plus the bronze branch.

```xml
<OdfBody CompetitionCode="OG2024"
         DocumentCode="BKBWTEAM5---------------------------"
         DocumentType="DT_BRACKETS"
         Version="7"
         ResultStatus="INTERMEDIATE"
         FeedFlag="T"
         Date="2024-08-09"
         Time="17:10:00.000"
         LogicalDate="2024-08-09"
         Source="BCYBKB1">
  <Competition Gen="SOG-2024-GEN" Sport="SOG-2024-BKB-3.4" Codes="SOG-2024-CODES">
    <ExtendedInfos>
      <Progress LastUnit="BKBWTEAM5-------------SFNL000100--"
                UnitsTotal="26"
                UnitsComplete="24"/>
      <SportDescription DisciplineName="Basketball"
                        EventName="Women's Team"
                        Gender="W"/>
    </ExtendedInfos>

    <Bracket Code="FNL">
      <BracketItems Code="SFNL">
        <BracketItem Code="33"
                     Order="1"
                     Position="1"
                     Date="2024-08-09"
                     Time="15:00"
                     Result="74-66"
                     Unit="BKBWTEAM5-------------SFNL000100--">
          <CompetitorPlace Pos="1" WLT="W" Result="74" ResultType="POINTS">
            <Competitor Code="BKBWTEAM5-----NED01" Type="T" Organisation="NED">
              <Description TeamName="Netherlands"/>
            </Competitor>
          </CompetitorPlace>
          <CompetitorPlace Pos="2" WLT="L" Result="66" ResultType="POINTS">
            <Competitor Code="BKBWTEAM5-----NZL01" Type="T" Organisation="NZL">
              <Description TeamName="New Zealand"/>
            </Competitor>
          </CompetitorPlace>
        </BracketItem>
      </BracketItems>

      <BracketItems Code="FNL-">
        <BracketItem Code="35"
                     Order="1"
                     Position="1"
                     Unit="BKBWTEAM5-------------FNL-000100--">
          <CompetitorPlace Pos="1" Code="W33">
            <PreviousUnit Unit="BKBWTEAM5-------------SFNL000100--" Value="33" WLT="W"/>
          </CompetitorPlace>
          <CompetitorPlace Pos="2" Code="TBD">
            <PreviousUnit Unit="BKBWTEAM5-------------SFNL000200--" Value="34" WLT="W"/>
          </CompetitorPlace>
        </BracketItem>
      </BracketItems>
    </Bracket>

    <Bracket Code="BRN">
      <BracketItems Code="FNL-">
        <BracketItem Code="36"
                     Order="1"
                     Position="1"
                     Unit="BKBWTEAM5-------------FNL-000200--">
          <CompetitorPlace Pos="1" Code="L33">
            <PreviousUnit Unit="BKBWTEAM5-------------SFNL000100--" Value="33" WLT="L"/>
          </CompetitorPlace>
          <CompetitorPlace Pos="2" Code="L34">
            <PreviousUnit Unit="BKBWTEAM5-------------SFNL000200--" Value="34" WLT="L"/>
          </CompetitorPlace>
        </BracketItem>
      </BracketItems>
    </Bracket>
  </Competition>
</OdfBody>
```

The example is structurally aligned with the PDF tables, but it is not validated against an XSD because no schema was supplied alongside the BKB dictionary.

## Message Sort

Sort as follows:

1. `Bracket/@Code`, using order in sport codes (`FNL` before `BRN`).
2. `Bracket/BracketItems/@Code`, descending, using order in sport codes (so `FNL-` first, then `SFNL`, then `QFNL`).
3. `Bracket/BracketItems/BracketItem/@Position`.

## Modeling Notes

- Model `DT_BRACKETS` as the published basketball knockout/progression graph for an event.
- The event key is `OdfBody/@DocumentCode`; phase or unit identifiers belong below it.
- Treat `Bracket/@Code` as the branch dimension (`FNL` vs `BRN`). The bronze-medal match is not a child of the finals branch; it lives in its own `Bracket` and is therefore not reachable from the gold-final node by graph traversal.
- Treat `BracketItems/@Code` as the round/stage grouping (`QFNL`, `SFNL`, `FNL-`).
- Treat `BracketItem` as the individual match node; `BracketItem/@Unit` is the link to the scheduled/result unit.
- Treat `CompetitorPlace` as the slot. It can exist before a team is known.
- Use `CompetitorPlace/@Code` for placeholders (`TBD`, `BYE`, `NOCOMP`, `NOAWARD`, and ORIS-defined codes such as `W33`, `L24`, `A1`, pool placement codes from `SC @Pool`).
- Add `Competitor` only when the team is confirmed.
- Preserve `PreviousUnit` as the canonical backward progression link:
  - From a previous match: send `PreviousUnit/@Unit` (unit RSC) plus `@WLT` (`W`/`L`) and optionally `@Value` (match number).
  - From a pool: send `PreviousUnit/@Unit` (pool RSC) plus `@Value` (`SC @Pool` code such as `1`, `2`, `CR1-2`, `CR5-8`). Do not send `@WLT`.
- The full message replaces prior state at the event level. Treat incoming `DT_BRACKETS` as authoritative for `OdfBody/@DocumentCode` at the given `Version`.
- Sort order is content-driven; do not rely on transport order. Reapply the sort after persistence when rendering.
- Basketball does not model two-legged ties in this ODF dictionary. Pool-driven seeding (`@Pool` codes including `CR*` combined-ranking placeholders) is modeled at the `CompetitorPlace`/`PreviousUnit` layer, not at the `Bracket` layer.

## Code Appendix: Referenced SC and CC Entities

| Reference | Used for | Source |
|---|---|---|
| `CC @Competition` | `OdfBody/@CompetitionCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm` |
| `CC @Event` | `OdfBody/@DocumentCode` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Event.htm` |
| `CC @ResultStatus` | `OdfBody/@ResultStatus` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm` |
| `SC @Source` | `OdfBody/@Source` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm` |
| `CC @Unit` | `Progress/@LastUnit`, `BracketItem/@Unit`, `PreviousUnit/@Unit` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm` |
| `CC @SportGender` | `SportDescription/@Gender` | Common Codes |
| `SC @Bracket` | `Bracket/@Code` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Bracket_SOG_BKB.htm` |
| `SC @BracketItems` | `BracketItems/@Code` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BracketItems_SOG_BKB.htm` |
| `SC @CompetitorPlace` | `CompetitorPlace/@Code` | Aggregate page `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm` filtered by `Code_Entity=@CompetitorPlace` (the individual page `odf_sc_CompetitorPlace_GEN.htm` returns 404). |
| `SC @WLT` | `CompetitorPlace/@WLT`, `PreviousUnit/@WLT` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_WLT_SOG_BKB.htm` |
| `SC @ResultType` | `CompetitorPlace/@ResultType` | Aggregate page `odf_sc_BKB.htm`, `Code_Entity=@ResultType` |
| `SC @IRM` | `CompetitorPlace/@IRM` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_BKB.htm` |
| `SC @Pool` | `PreviousUnit/@Value` (pool source) | Aggregate page `odf_sc_BKB.htm`, `Code_Entity=@Pool` |
| `CC @Organisation` | `Competitor/@Organisation` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm` (large CC master table; resolved via link only) |

### `SC @Bracket` (BKB)

| Code | Order | ENG Description |
|---|---:|---|
| `FNL` | 1 | Finals |
| `BRN` | 2 | Bronze |

### `SC @BracketItems` (BKB)

| Code | Order | ENG Description |
|---|---:|---|
| `FNL-` | 1 | Finals |
| `SFNL` | 2 | Semifinals |
| `QFNL` | 3 | Quarterfinals |

### `SC @WLT` (BKB)

| Code | ENG Description |
|---|---|
| `L` | Lost |
| `W` | Won |

Note: the BKB `@WLT` sport-code page lists only `W` and `L`. The `T` (drawn) value documented for some team-sport interfaces is not registered for basketball, which is consistent with basketball rules (no draws in regulation/overtime).

### `SC @ResultType` (BKB)

| Code | ENG Description |
|---|---|
| `POINTS` | Points |
| `IRM` | Invalid Result Mark |
| `IRM_POINTS` | For both, points and invalid result mark |

### `SC @CompetitorPlace` (BKB)

| Code | Note | ENG Description |
|---|---|---|
| `BYE` | No competitor; the other team goes directly to the next phase/round. | Bye |
| `NOAWARD` |  | Not awarded |
| `NOCOMP` |  | No competitor |
| `TBD` |  | To be determined |

ORIS-defined placeholder codes such as `W<n>`, `L<n>`, `A1`, `B2`, etc. are valid `CompetitorPlace/@Code` values during the early stages of the bracket but are not registered as discrete rows in the `@CompetitorPlace` sport-code page for BKB.

### `SC @IRM` (BKB)

| Code | ENG Description |
|---|---|
| `DNS` | Did Not Start |
| `DQB` | Disqualified for unsportsmanlike behaviour |
| `DSQ` | Disqualified |
| `WDR` | Withdrawn |

### `SC @Pool` (BKB)

| Code | Order | ENG Description |
|---|---:|---|
| `1` | 1 | 1st in combined ranking |
| `2` | 2 | 2nd in combined ranking |
| `3` | 3 | 3rd in combined ranking |
| `4` | 4 | 4th in combined ranking |
| `CR1-2` |  | 1st or 2nd in combined ranking |
| `CR1-4` |  | 1-4 in combined ranking |
| `CR3-4` |  | 3rd or 4th in combined ranking |
| `CR5-6` |  | 5th or 6th in combined ranking |
| `CR5-8` |  | 5-8 in combined ranking |
| `CR7-8` |  | 7th or 8th in combined ranking |

### `SC @Source` (BKB)

| Code | ENG Description |
|---|---|
| `BCYBKB1` | Origin for messages from OVR at BCY for BKB |
| `LILBKB1` | Origin for messages from OVR at LIL for BKB |

### `CC @Unit`, `CC @Event`, `CC @Organisation`, `CC @Competition`, `CC @ResultStatus`, `CC @SportGender`

These are large common-code master tables. They are referenced by link only; resolve authoritative rows from the linked Paris 2024 code pages at extraction time.

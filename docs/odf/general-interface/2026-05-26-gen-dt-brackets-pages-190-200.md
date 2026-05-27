# ODF GEN General Messages Interface: DT_BRACKETS, Pages 190-200

Source: `C:\Users\mella\WebstormProjects\sportivo\docs\references\odf\ODF_GEN_R-OWG2026-GEN.pdf`, pages 190-200.

This note restructures section `2.1.15 Brackets` from the OWG2026 General Messages Interface Document into a readable reference for Sportivo domain modeling. This is the general interface contract for `DT_BRACKETS`; sport data dictionaries still define sport-specific bracket codes, result formats, placeholders, and progression rules.

## 2.1.15 Brackets

`DT_BRACKETS` contains bracket information for an event or for a component of an event, such as a phase or unit. It is used when consumers need to know competition progress.

In early stages, the message describes how the competition will progress from winners or losers before actual competitors are known. As matches/event units are completed, the message is updated with known competitors and results in the different bracket items.

The key modeling idea is that ODF brackets describe slots and their sources. A future slot can point backward to a previous unit or to a pool/phase via `PreviousUnit`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE Id` | Competition identifier. |
| `DocumentCode` | `CC@EVENT Code` | Event RSC. |
| `DocumentType` | `DT_BRACKETS` | Brackets message. |
| `Version` | Positive integer | Ascending version number associated with the message content. |
| `ResultStatus` | `START_LIST`, `INTERMEDIATE`, `UNCONFIRMED`, `UNOFFICIAL`, `OFFICIAL`, `PROTESTED`, `PROVISIONAL` | Status of the bracket content. Use of `UNCONFIRMED`, `UNOFFICIAL`, and `OFFICIAL` can differ by sport. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | Date | Message generation date. |
| `Time` | Time | Message generation time. |
| `LogicalDate` | Date | Logical competition date. |
| `Source` | `SCGEN@Source Code` | System that generated the message. |

## Trigger and Frequency

`DT_BRACKETS` should be sent as soon as brackets are available.

It is sent when a match or event unit is completed, including `UNCONFIRMED`, `UNOFFICIAL`, and `OFFICIAL` statuses. If the sport uses all three statuses, this means the bracket can be triggered up to three times for the same event unit.

| Competition condition | Expected `ResultStatus` |
|---|---|
| No units are complete | `START_LIST` |
| Until the last event unit, usually the gold medal match, is unofficial | `INTERMEDIATE` |
| The last event unit has unconfirmed status | `UNCONFIRMED` |
| The last event unit has unofficial status | `UNOFFICIAL` |
| The last event unit has official status | `OFFICIAL` |
| IOC, CAS, IF, or similar decision is pending | `PROVISIONAL` |

The status varies depending on the sport's competition status rules.

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- ExtendedInfo (0,N)
    |   |   +-- Extension (0,N)
    |   +-- Progress (0,1)
    |   |   +-- Extension (0,N)
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
                +-- @Time
                +-- @TimeStamp
                +-- @Unit
                +-- @Result
                +-- ExtBracketItems (0,1)
                |   +-- ExtBracketItem (1,N)
                +-- CompetitorPlace (1,N)
                    +-- @Pos
                    +-- @Code
                    +-- @WLT
                    +-- @Rank
                    +-- @Result
                    +-- @ResultType
                    +-- @Diff
                    +-- @IRM
                    +-- @QualificationMark
                    +-- @StrikeOut
                    +-- @StartOrder
                    +-- @PhotoFinish
                    +-- ExtCompPlaces (0,1)
                    |   +-- ExtCompPlace (1,N)
                    +-- PreviousUnit (0,1)
                    |   +-- @Unit
                    |   +-- @Value
                    |   +-- @WLT
                    +-- Competitor (0,1)
                        +-- @Code
                        +-- @Type
                        +-- @Seed
                        +-- @Organisation
                        +-- @Bib
                        +-- Description (0,1)
                        |   +-- ExtendedDescription (0,N)
                        +-- ExtBracketComps (0,1)
                        |   +-- ExtBracketComp (1,N)
                        +-- Composition (0,1)
                            +-- Athlete (1,N)
                                +-- @Code
                                +-- @Order
                                +-- @Bib
                                +-- Description (1,1)
                                |   +-- ExtendedDescription (0,N)
                                +-- Guide (0,N)
                                +-- ExtBracketAths (0,1)
                                    +-- ExtBracketAth (1,N)
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Gen` | M | `S(20)` | Version of the General Data Dictionary applicable to the message. |
| `Sport` | O | `S(35)` | Version of the Sport Data Dictionary applicable to the message. |
| `Codes` | M | `S(20)` | Version of the codes applicable to the message. |

### `Competition/ExtendedInfos`

`ExtendedInfos` carries optional metadata and progress information for the bracket message.

#### `ExtendedInfo`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Type` | O | Sport-specific | Sport-specific metadata group. |
| `Code` | O | Sport-specific | Sport-specific metadata code. |
| `Pos` | O | Sport-specific | Position within the metadata group. |
| `Value` | O | Sport-specific | Metadata value. |

#### `ExtendedInfo/Extension`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | O | Sport-specific | Extension code. |
| `Pos` | O | Sport-specific | Extension position. |
| `Value` | O | Sport-specific | Extension value. |

#### `Progress`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `LastUnit` | O | `CC@EVENT_UNIT Code` | Full RSC of the last completed or in-progress unit related to the message content. |
| `UnitsTotal` | O | Positive integer | Total expected units that impact this message. |
| `UnitsComplete` | O | `#0` | Number of completed units that impact this message. |

### `SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE ENG Description` | Discipline name, not code. |
| `EventName` | M | `CC@EVENT ENG Description` | Event name, not code. |
| `Gender` | M | `CC@DISCIPLINE_GENDER Gender` | Gender code for the event unit. |

### `VenueDescription`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Venue` | M | `CC@VENUE Id` | Venue code. |
| `VenueName` | M | `CC@VENUE ENG Description` | Venue name, not code. |
| `Location` | O | `CC@LOCATION Id` | Location code. |
| `LocationName` | O | `CC@LOCATION ENG Description` | Location name, not code. |

### `Competition/Bracket`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | Sport-specific | Bracket code. Identifies a bracket branch, such as finals or classification games. There should be a different code for each bracket based on sport/ORIS presentation. For example, a bronze bracket is different from the bracket leading to gold when bronze matches are played. |

### `Competition/Bracket/BracketItems`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | Sport-specific | Code identifying a set of bracket items. Usually refers to the round in the bracket, such as quarterfinals or semifinals. |

`BracketItems` is a group of bracket items for one bracket round/stage. The plural name is important: this node is not a match; it contains one or more matches/items.

### `Competition/Bracket/BracketItems/BracketItem`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | O | Sport-specific | Unique identifier for the bracket item. |
| `Order` | M | Positive integer | Sequential number inside `BracketItems`. Always starts at `1`. |
| `Position` | M | Positive integer | Visual bracket position. For example, quarterfinal items are positions `1`, `2`, `3`, `4` from top to bottom. |
| `Date` | O | `YYYY-MM-DD` | Match date. Include if available. |
| `Time` | O | `HH:MM` | Match time. Include if available. Some disciplines with competitor-dependent match times withhold this until competitors are known and times are approved. |
| `TimeStamp` | O | DateTime | Scheduled date and time with timezone offset. Send for future and completed matches. |
| `Unit` | O | `CC@EVENT_UNIT Code` | Full RSC of the unit for this bracket item. |
| `Result` | O | Sport-specific | Match result if complete, formatted as in ORIS, for example `5-2`. Must be included if available and match is complete. |

`Position` is for drawing. If a bracket round logically has four items but one is not displayed, positions may skip the hidden slot; for example, visible positions could be `2`, `3`, `4`.

### `ExtBracketItems/ExtBracketItem`

Optional extension for bracket-item-level rules.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Type` | O | Sport-specific | Extension group. |
| `Code` | O | Sport-specific | Extension code. |
| `Pos` | O | Sport-specific | Extension position. |
| `Value` | O | Sport-specific | Extension value. |

### `Competition/Bracket/BracketItems/BracketItem/CompetitorPlace`

`CompetitorPlace` is the bracket slot.

- If competitors are known, it places competitors in the bracket.
- If competitors are not yet known, it contains information about the rule used to access the bracket slot.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Pos` | M | Positive integer | Sequential slot position in the bracket item, such as `1`, `2`, etc. |
| `Code` | O | Sport-specific | Code for the competitor place. Often indicates the access rule when the competitor is not known. |
| `WLT` | O | Sport-specific | Winner/loser/tied indicator for the bracket item. Always send when known. |
| `Rank` | O | Sport-specific | Rank in the bracket item. Usually only applicable when more than two competitors are in the item. |
| `Result` | O | Sport-specific | Competitor result in the event unit. |
| `ResultType` | O | `SC@ResultType Code` | Type of `Result`. |
| `Diff` | O | Sport-specific | Difference from leader, if applicable. |
| `IRM` | O | `SC@IRM Code` | Invalid result mark, if applicable. |
| `QualificationMark` | O | `SC@QualificationMark Code` | Qualification indicator for the next round. |
| `StrikeOut` | O | `Y` | Indicates the competitor should be struck out in the bracket item. Usually used for `DQB`, but may have other sport-specific uses. |
| `StartOrder` | O | Positive integer | Starting-position designator, for example colour. |
| `PhotoFinish` | O | `E`, `P` | `E` for photo finish evaluated; `P` for pending evaluation. While pending, involved competitors are sorted by theoretical rank before evaluation and unconfirmed result attributes are not expected. |

### `ExtCompPlaces/ExtCompPlace`

Optional extension for competitor-place-level rules.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Type` | O | Sport-specific | Extension group. |
| `Code` | O | Sport-specific | Extension code. |
| `Pos` | O | Sport-specific | Extension position. |
| `Value` | O | Sport-specific | Extension value. |

### `PreviousUnit`

`PreviousUnit` points backward to the prior unit or phase that feeds a competitor place in the current bracket item.

It is always informed except for bracket items whose competitor place does not have preceding event units in the bracket graph, unless the competitor place is coming from a pool.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Unit` | O | `CC@EVENT_UNIT Code` or `CC@PHASE Code` | Full RSC of the previous event unit for the competitor place. Send when the slot receives a winner/loser from a single unit. If the slot comes from a pool, this is the pool RSC. |
| `Value` | O | Sport-specific | If the competitor is unknown and comes from a pool or previous matches, fill with the pool code or match number as appropriate. May be redefined by sport. |
| `WLT` | O | `SC@WLT Code` | If the unknown competitor comes from an earlier bracket item, send `W` or `L` when the winner/loser information is known. Do not send when the competitor comes from a pool. |

This is the core of ODF bracket progression. The future slot looks backward to its source; the previous match does not need to point forward to the next match.

### `Competitor`

`Competitor` is the known competitor assigned to a `CompetitorPlace`. Only include it when the competitor is known.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeros | Competitor ID. |
| `Type` | M | `A`, `T` | `A` for athlete, `T` for team. |
| `Seed` | O | `S(10)` | Competitor seed or equivalent information. |
| `Organisation` | O | `CC@ORGANISATION Id` | Competitor organisation, if known. |
| `Bib` | O | `S(5)` | Competitor bib. |

#### `Competitor/Description`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `TeamName` | M | `S(73)` | Team name. Applies only to teams/groups. |
| `IFId` | O | `S(16)` | Team IF number, if available. |

#### `Competitor/Description/ExtendedDescription`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Type` | O | Sport-specific | Extension group. |
| `Code` | O | Sport-specific | Extension code. |
| `Pos` | O | Sport-specific | Extension position. |
| `Value` | O | Sport-specific | Extension value. |

#### `ExtBracketComps/ExtBracketComp`

Competitor-level extended bracket information.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Type` | O | Sport-specific | Extension group. |
| `Code` | O | Sport-specific | Extension code. |
| `Pos` | O | Sport-specific | Extension position. |
| `Value` | O | Sport-specific | Extension value. |

#### `Composition/Athlete`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Code` | M | `S(20)` with no leading zeros | Athlete ID, either a team member or an individual athlete. |
| `Order` | M | Positive integer | Sort order for team members, or `1` when `Competitor/@Type="A"`. |
| `Bib` | O | `S(5)` | Athlete bib. |

##### `Athlete/Description`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `GivenName` | O | `S(25)` | Preferred given name. |
| `FamilyName` | M | `S(25)` | Preferred family name. |
| `Gender` | M | `CC@PERSON_GENDER Id` | Athlete gender. |
| `Organisation` | M | `CC@ORGANISATION Id` | Athlete organisation. |
| `BirthDate` | O | `YYYY-MM-DD` | Date of birth. |
| `IFId` | O | `S(16)` | International Federation ID. |
| `Class` | O | `CC@DISCIPLINE_CLASS Class` | Sport class for events with athletes with a disability. |
| `Horse` | O | `S(25)` | Athlete horse name. |

##### `Athlete/Guide`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `GuideID` | M | `S(20)` without leading zeros | Athlete guide ID. |
| `Order` | M | Positive integer | Guide sort order. |
| `GuideFamilyName` | M | `S(25)` | Guide preferred family name. |
| `GuideGivenName` | O | `S(25)` | Guide preferred given name. |

##### `ExtBracketAths/ExtBracketAth`

Athlete-level extended bracket information.

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Type` | O | Sport-specific | Extension group. |
| `Code` | O | Sport-specific | Extension code. |
| `Pos` | O | Sport-specific | Extension position. |
| `Value` | O | Sport-specific | Extension value. |

## Normalized XML Examples

The GEN section does not provide an XML sample. The examples below are compact, illustrative examples that show the general hierarchy and progression mechanism. Concrete `Bracket/@Code`, `BracketItems/@Code`, `CompetitorPlace/@Code`, team/athlete IDs, and result formats must come from the target sport dictionary and code release.

### Unknown Semifinal Slots from Previous Units

```xml
<OdfBody CompetitionCode="OWG2026"
         DocumentCode="XXXMTEAM11------------------------"
         DocumentType="DT_BRACKETS"
         Version="1"
         ResultStatus="START_LIST"
         FeedFlag="T"
         Date="2026-02-12"
         Time="12:00:00"
         LogicalDate="2026-02-12"
         Source="OVR">
  <Competition Gen="OWG2026-GEN-4.6" Sport="SPORT-DICT" Codes="OWG2026-CODES">
    <Bracket Code="GOLD">
      <BracketItems Code="SF">
        <BracketItem Code="SF1"
                     Order="1"
                     Position="1"
                     Unit="XXXMTEAM11------------SFNL000100--">
          <CompetitorPlace Pos="1" Code="TBD">
            <PreviousUnit Unit="XXXMTEAM11------------QFNL000100--" WLT="W"/>
          </CompetitorPlace>
          <CompetitorPlace Pos="2" Code="TBD">
            <PreviousUnit Unit="XXXMTEAM11------------QFNL000200--" WLT="W"/>
          </CompetitorPlace>
        </BracketItem>
      </BracketItems>
    </Bracket>
  </Competition>
</OdfBody>
```

Reading:

- `Bracket Code="GOLD"` identifies the bracket branch that leads to gold.
- `BracketItems Code="SF"` identifies the semifinal round/stage.
- `BracketItem Unit="...SFNL000100--"` is the concrete semifinal unit.
- Each `CompetitorPlace` is a slot.
- Each slot points backward to the source quarterfinal through `PreviousUnit`.
- `WLT="W"` means the slot expects the winner of that previous unit.

### Slot Coming from a Pool

```xml
<CompetitorPlace Pos="1" Code="TBD">
  <PreviousUnit Unit="XXXMTEAM11------------GPA---------" Value="A1"/>
</CompetitorPlace>
```

When the source is a pool, `PreviousUnit/@Unit` is the pool/phase RSC and `PreviousUnit/@Value` can carry a sport-specific placement such as `A1`. Do not send `PreviousUnit/@WLT` when the competitor comes from a pool.

### Known Competitor in a Bracket Item

```xml
<CompetitorPlace Pos="1"
                 Code="TEAM_A"
                 WLT="W"
                 Result="2"
                 ResultType="POINTS"
                 QualificationMark="Q">
  <PreviousUnit Unit="XXXMTEAM11------------QFNL000100--" WLT="W"/>
  <Competitor Code="TEAM_A" Type="T" Organisation="AAA" Seed="1">
    <Description TeamName="Team A"/>
  </Competitor>
</CompetitorPlace>
```

When the competitor is known, keep the `CompetitorPlace` slot and add `Competitor`. `PreviousUnit` remains useful for traceability.

## Message Sort

Sort as follows:

1. `Bracket/@Code`, using order in sport codes.
2. `Bracket/BracketItems/@Code`, using order in sport codes, descending.
3. `Bracket/BracketItems/BracketItem/@Position`.

## XSD Notes

The related `odf2-schema-30112025-DRAFT` package defines `DT_BRACKETS` as a valid `DocumentType` and includes the bracket hierarchy as:

```text
Bracket
+-- BracketItems
    +-- BracketItem
        +-- ExtBracketItems?
        +-- CompetitorPlace+
            +-- ExtCompPlaces?
            +-- PreviousUnit?
            +-- Competitor?
```

Relevant PDF/XSD drift and permissiveness:

- The XSD root header is generic and still allows optional `DocumentSubcode` and `DocumentSubtype`; the GEN bracket header table does not list either for `DT_BRACKETS`.
- XSD `ExtBracketItem`, `ExtCompPlace`, `ExtBracketComp`, and `ExtBracketAth` require `Type` and `Code`, while the PDF marks those extension attributes optional.
- XSD `Composition/Athlete` is inherited from a shared `compositionResultsType` where `Athlete` can be omitted, while the PDF structure describes `Athlete (1,N)` once `Composition` is present.
- XSD athlete `Description` is optional in the shared type, while the PDF bracket message values describe athlete `Description (1,1)`.
- `PreviousUnit/@Unit`, `Value`, and `WLT` are plain strings in the XSD. The PDF gives semantic constraints: event-unit or phase RSC, pool placement value, and no `WLT` when coming from a pool.

Use the XSD for structural validation, but use this PDF and the sport data dictionary for semantic validation.

## Modeling Notes

- Model `DT_BRACKETS` as the published progression graph for an event.
- `Bracket` is a branch of the bracket, such as gold, bronze, or classification.
- `BracketItems` is a round/stage grouping, such as quarterfinals or semifinals.
- `BracketItem` is a concrete contest/item in that stage, often linked to a unit RSC.
- `CompetitorPlace` is the slot. It exists even before the competitor is known.
- `Competitor` is the known participant/team/athlete assigned to a slot. It should only appear once known.
- Progression is backward-linked. Future slots point to their source with `PreviousUnit`; prior units do not need forward links.
- Pool-to-bracket progression uses `PreviousUnit/@Unit` as the pool/phase RSC and `PreviousUnit/@Value` for the pool placement rule. Do not use `PreviousUnit/@WLT` for pool sources.
- Store `Position` separately from `Order`. `Order` sorts inside a `BracketItems` group; `Position` draws the visual bracket.
- Preserve `PreviousUnit` after the competitor is known. It explains why the competitor occupies the slot.
- `StrikeOut`, `PhotoFinish`, `IRM`, and `QualificationMark` are publication/status markers and should not be conflated with bracket topology.
- A Sportivo internal "two-legged tie" should remain an internal concept if needed. The GEN bracket contract models progression from units/phases, not aggregate tie entities.

## Code Appendix: Referenced SC and CC Entities

This general interface document references code entities but does not define concrete values. Resolve actual values from the relevant OWG2026 Common Codes, General Sport Codes, and sport-specific data dictionary/code release.

| Reference | Used for |
|---|---|
| `CC@COMPETITION_CODE` | `OdfBody/@CompetitionCode` |
| `CC@EVENT` | `OdfBody/@DocumentCode`, `SportDescription/@EventName` |
| `CC@RESULTSTATUS` | `OdfBody/@ResultStatus` |
| `SCGEN@Source` | `OdfBody/@Source` |
| `CC@EVENT_UNIT` | `Progress/@LastUnit`, `BracketItem/@Unit`, `PreviousUnit/@Unit` |
| `CC@PHASE` | `PreviousUnit/@Unit` when the source is a pool/phase |
| `CC@DISCIPLINE` | `SportDescription/@DisciplineName` |
| `CC@DISCIPLINE_GENDER` | `SportDescription/@Gender` |
| `CC@VENUE` | `VenueDescription/@Venue`, `VenueDescription/@VenueName` |
| `CC@LOCATION` | `VenueDescription/@Location`, `VenueDescription/@LocationName` |
| `SC@ResultType` | `CompetitorPlace/@ResultType` |
| `SC@IRM` | `CompetitorPlace/@IRM` |
| `SC@QualificationMark` | `CompetitorPlace/@QualificationMark` |
| `SC@WLT` | `PreviousUnit/@WLT`, and sport-specific winner/loser/tied status |
| `CC@ORGANISATION` | `Competitor/@Organisation`, athlete organisation |
| `CC@PERSON_GENDER` | `Athlete/Description/@Gender` |
| `CC@DISCIPLINE_CLASS` | `Athlete/Description/@Class` |

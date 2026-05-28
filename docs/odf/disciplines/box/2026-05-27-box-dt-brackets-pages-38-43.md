# ODF BOX Data Dictionary: DT_BRACKETS, Pages 38-43

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BOX_Data_Dictionary.pdf`, pages 38-43.

Source version: `SCOG/SYOG-2026-BOX-1.2 SFR`, dated 13 May 2026.

This note restructures the Boxing `DT_BRACKETS` section into a practical domain reference for the Brackets message.
It covers the bracket-phase-item hierarchy that announces, in advance, how successive event units will be filled as
the competition progresses, including how athlete competitors are placed in head-to-head bouts and how the bronze
medal match is modeled inside the single `FNL` bracket used by Boxing.

## 2.3.5 Brackets

`DT_BRACKETS` contains the brackets information for one particular Boxing event. It is used because successive event
units must be known in advance and built from the winners/losers (or other competition rules) of previous event units.
In the early stages it indicates how each upcoming event unit will be filled; later versions carry the actual
competitor placements and bout results.

Per the BOX overview, there is only one bracket for all head-to-head Boxing competitions and its code is `FNL`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@Event` | Full RSC of the event. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_BRACKETS` | Brackets message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `CC@ResultStatus` | Bracket lifecycle status. Expected: `START_LIST` when the draw is initially made, `INTERMEDIATE` during the competition, `OFFICIAL` when all matches are official, `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | `P` production, `T` test. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| When | What |
|---|---|
| At the very beginning of the competition, as soon as brackets are available | Send with `ResultStatus="START_LIST"`. |
| When a match/event unit is completed but not yet official | Send with `ResultStatus="INTERMEDIATE"`. |
| When a match/event unit becomes official | Send again with `ResultStatus="INTERMEDIATE"` until the last event unit (Gold Medal match) is official. |
| When the last event unit (Gold Medal match) for the event has Official status | Send with `ResultStatus="OFFICIAL"`. |
| Any other applicable status | Trigger again after any change. |

The section indicates the message can be triggered up to two times per event unit (intermediate and official), and
should always be triggered after any change.

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
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
                +-- @TimeStamp
                +-- @Unit
                +-- @Result
                +-- ExtBracketItems (0,1)
                |   +-- ExtBracketItem (1,N)
                |       +-- @Type
                |       +-- @Code
                |       +-- @Pos
                |       +-- @Value
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
                        +-- @Seed
                        +-- @Organisation
                        +-- Composition (0,1)
                            +-- Athlete (1,N)
                                +-- @Code
                                +-- @Order
                                +-- Description (1,1)
                                    +-- @GivenName
                                    +-- @FamilyName
                                    +-- @Gender
                                    +-- @Organisation
                                    +-- @BirthDate
                                    +-- @IFId
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | O | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | O | `S(20)` | Code-set version applicable to the message. |

### `Competition / ExtendedInfos / SportDescription`

Sport description in text.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE` English description | Discipline name, not the code. |
| `EventName` | M | `CC@EVENT` English description | Event name, not the code. |
| `Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the event unit. |

### `Competition / Bracket`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `SC@Bracket` | Bracket code to identify a bracket. There should be a different code for each bracket based on sport/ORIS presentation. For BOX, the overview defines a single `FNL` bracket for all head-to-head competitions. |

### `Competition / Bracket / BracketItems`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `SC@BracketItems` | Code identifying a set of bracket items, that is, the phase. Each `BracketItems` should include all `BracketItem` rows grouped by their `SC@BracketItems` (for example, quarterfinals, semifinals, finals). |

### `Competition / Bracket / BracketItems / BracketItem`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | O | `##0` | In general, the bout number for this bracket item (for example, 17, 18, 19, 20). |
| `Order` | M | `##0` | Sequential number inside `BracketItems` indicating order; always starts at 1. |
| `Position` | M | `##0` | Drawing position inside the phase. A quarterfinal with four items uses positions 1, 2, 3, 4 from the top. If only three items exist and the fourth would logically be first, the positions are 2, 3, 4. |
| `Date` | O | Date | `YYYY-MM-DD`; must be filled if known. |
| `Time` | O | `S(5)` | `HH:MM`; must be filled if known. |
| `TimeStamp` | O | DateTime | Scheduled date and time of the match/unit including time-zone offset. Send for future and completed matches. |
| `Unit` | O | `CC@EVENT_UNIT` | Full RSC of the unit for this bracket item. |
| `Result` | O | `S(50)` | Result of the match if complete, formatted as in ORIS (with result and decision, for example `WP 3:0` or `TKO R3 1:23`). Must be included if the data is available and the match is complete. |

### `BracketItem / ExtBracketItems / ExtBracketItem`

Optional element used according to competition rules.

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EBI` | `SESSIONTYPE` | N/A | When available. | `CC@SessionType` | Session type as in C75 (ORIS): morning or afternoon. |
| `EBI` | `DECISION` | N/A | When available. | `SC@ResultCode` | Bout result mark. |

### `BracketItem / CompetitorPlace`

If the competitors are known, this element places them in the bracket. If they are not yet known, it carries
information about the rule used to access this bracket item (for example, the previous unit and W/L role).

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Pos` | M | Positive integer | Sequential number placing the competitors in the bracket (1 or 2). |
| `Code` | O | `SC@CompetitorPlace` | Sent when applicable (for example, no competitor or competitor not yet known). |
| `WLT` | O | `SC@WLT` | `W` or `L`; send `L` for no-winner cases such as `DKO`. Always sent when known. |
| `Result` | O | `S(5)` | Points of the competitor; sent for `ResultType="POINTS"` and `ResultType="RM_POINTS"`. Valid range 0-5. |
| `ResultType` | O | `SC@ResultType` | Type of the `Result` attribute. |
| `IRM` | O | `SC@IRM` | Invalid result mark, if applicable. |
| `StrikeOut` | O | `Y` | Send `Y` if the competitor should be struck out in this bracket item; usually only used for `DQB`. |

### `CompetitorPlace / PreviousUnit`

Previous event unit related to the `CompetitorPlace@Pos` competitor of the current bracket item. Always informed
except for bracket items whose `CompetitorPlace@Pos` competitor has no preceding event unit in the bracket graph,
unless coming from a pool. The section indicates `PreviousUnit` should be informed for finals, semifinals,
quarterfinals, round of 16, and round of 32.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Unit` | O | `CC@EVENT_UNIT` | Full RSC of the previous event unit for the `CompetitorPlace@Pos` competitor. Must be sent if a winner/loser from a single unit. |
| `Value` | O | Positive integer | If the competitor in the current unit is unknown because they come from previous matches, fill with the match number as appropriate. |
| `WLT` | O | `SC@WLT` | If the competitor in the current unit is unknown and coming from an earlier bracket item, send `W` or `L` for winner or loser of the previous unit when known. Do not send if the competitor comes from a pool. |

### `CompetitorPlace / Competitor`

Include only if the competitor is known.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | M | `A` | `A` for athlete. |
| `Seed` | O | `S(10)` | Seed of the competitor or equivalent information. |
| `Organisation` | O | `CC@ORGANISATION` | Competitor organisation, if known. |

### `Competitor / Composition / Athlete`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Athlete ID. |
| `Order` | M | Positive integer | Athlete order. Send `1` when `Competitor/@Type="A"`. |

### `Competitor / Composition / Athlete / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `GivenName` | O | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | M | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | M | `CC@PERSON_GENDER` | Athlete gender. |
| `Organisation` | M | `CC@ORGANISATION` | Athlete organisation. |
| `BirthDate` | O | `YYYY-MM-DD` | Birth date; include if available. |
| `IFId` | O | `S(16)` | International Federation ID. |

## Bracket Semantics for BOX

- One bracket per head-to-head Boxing event, identified by `Bracket/@Code="FNL"` per the BOX overview (pages 6-7).
- Bracket phases populate `BracketItems/@Code` (for example, round of 32, round of 16, quarterfinals, semifinals,
  finals); the dictionary sample on page 43 uses the unit RSC suffix `SFNL` for a semifinal bout.
- Boxing competitors are athletes (`Competitor/@Type="A"`), not teams. Each `CompetitorPlace` carries a `Competitor`
  containing one `Athlete` with `Order="1"` when the competitor is known.
- Within a `BracketItem`, the two `CompetitorPlace` rows use `Pos=1` and `Pos=2`. RED/BLUE corner assignment is
  carried in `DT_RESULT`, not in `DT_BRACKETS`.
- The `Seed` attribute is carried on `Competitor`, not as a separate `EventUnitEntry` as in `DT_RESULT`. The
  dictionary sample uses `Seed="4"` and `Seed="6"`.
- `BracketItem/@Result` is the ORIS-formatted display result (for example, `WP 3:0` for a unanimous points decision
  in the dictionary sample, or `TKO R3 1:23` for a stoppage). Authoritative scoring belongs in `DT_RESULT`.
- `ExtBracketItems` carries Boxing-specific extras: `SESSIONTYPE` (morning/afternoon) and `DECISION` (bout result
  mark in `SC@ResultCode`).
- The bronze medal match: the BOX overview does not state a specific bronze rule (Boxing typically awards two bronze
  medals to both semifinal losers without a bronze medal bout). The `DT_BRACKETS` section does not describe a bronze
  medal bracket item in pages 38-43. When a bronze placement bout does exist, follow the general bracket-position
  convention: place the medal match in the appropriate phase and use `Position` to order it relative to other matches
  in the same phase.
- When competitors are not yet known, omit `Competitor` and rely on `CompetitorPlace/@Code` (`SC@CompetitorPlace`)
  and `PreviousUnit` to describe the slot.

## Samples from the Dictionary, Normalized

### Semifinal bout with both athletes known, points decision

This sample mirrors the dictionary sample on page 43, completed as a valid XML fragment.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <Bracket Code="FNL">
    <BracketItems Code="SFNL">
      <BracketItem Code="131" Order="2" Position="3"
                   Date="2016-08-09" Time="01:20"
                   Unit="BOXM54KG--------------SFNL0001----"
                   Result="WP 3:0">
        <CompetitorPlace Pos="1" WLT="W">
          <PreviousUnit Unit="BOXM54KG--------------QFNL0001----"/>
          <Competitor Code="1066978" Type="A" Seed="4" Organisation="ESP">
            <Composition>
              <Athlete Code="1066978" Order="1">
                <Description FamilyName="Black" GivenName="John" Gender="M" Organisation="ESP" BirthDate="1991-12-16"/>
              </Athlete>
            </Composition>
          </Competitor>
        </CompetitorPlace>
        <CompetitorPlace Pos="2" WLT="L">
          <PreviousUnit Unit="BOXM54KG--------------QFNL0003----"/>
          <Competitor Code="1129984" Type="A" Seed="6" Organisation="NZL">
            <Composition>
              <Athlete Code="1129984" Order="1">
                <Description FamilyName="Smith" GivenName="John" Gender="M" Organisation="NZL" BirthDate="1991-12-15"/>
              </Athlete>
            </Composition>
          </Competitor>
        </CompetitorPlace>
      </BracketItem>
    </BracketItems>
  </Bracket>
</Competition>
```

### Final phase with both slots unknown, fed by previous semifinals

When a slot is not yet known but the feeder rule is, omit `Competitor` and use `PreviousUnit` to describe the
dependency. `CompetitorPlace/@Code` carries a `SC@CompetitorPlace` value when applicable.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <Bracket Code="FNL">
    <BracketItems Code="FNL">
      <BracketItem Order="1" Position="1"
                   Unit="BOXM54KG--------------FNL-0001----">
        <CompetitorPlace Pos="1">
          <PreviousUnit Unit="BOXM54KG--------------SFNL0001----" WLT="W"/>
        </CompetitorPlace>
        <CompetitorPlace Pos="2">
          <PreviousUnit Unit="BOXM54KG--------------SFNL0002----" WLT="W"/>
        </CompetitorPlace>
      </BracketItem>
    </BracketItems>
  </Bracket>
</Competition>
```

### Bracket item with session type and decision extension

`ExtBracketItems` carries Boxing-specific extras such as `SESSIONTYPE` and `DECISION` for a completed bout.

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <Bracket Code="FNL">
    <BracketItems Code="SFNL">
      <BracketItem Code="131" Order="2" Position="3"
                   Date="2016-08-09" Time="01:20"
                   Unit="BOXM54KG--------------SFNL0001----"
                   Result="WP 3:0">
        <ExtBracketItems>
          <ExtBracketItem Type="EBI" Code="SESSIONTYPE" Value="AFTERNOON"/>
          <ExtBracketItem Type="EBI" Code="DECISION" Value="WP"/>
        </ExtBracketItems>
        <CompetitorPlace Pos="1" WLT="W">
          <PreviousUnit Unit="BOXM54KG--------------QFNL0001----"/>
          <Competitor Code="1066978" Type="A" Seed="4" Organisation="ESP">
            <Composition>
              <Athlete Code="1066978" Order="1">
                <Description FamilyName="Black" GivenName="John" Gender="M" Organisation="ESP" BirthDate="1991-12-16"/>
              </Athlete>
            </Composition>
          </Competitor>
        </CompetitorPlace>
        <CompetitorPlace Pos="2" WLT="L">
          <PreviousUnit Unit="BOXM54KG--------------QFNL0003----"/>
          <Competitor Code="1129984" Type="A" Seed="6" Organisation="NZL">
            <Composition>
              <Athlete Code="1129984" Order="1">
                <Description FamilyName="Smith" GivenName="John" Gender="M" Organisation="NZL" BirthDate="1991-12-15"/>
              </Athlete>
            </Composition>
          </Competitor>
        </CompetitorPlace>
      </BracketItem>
    </BracketItems>
  </Bracket>
</Competition>
```

## Message Sort

- First by `Bracket/@Code`, using order in sport codes.
- Then by `Bracket/BracketItems/@Code` descending, using order in sport codes (later phases such as finals tend to
  come first when reading the message; rely on the sport-code ordering rather than alphabetic).
- Then by `Bracket/BracketItems/BracketItem/@Position`.

## XSD Validation

The normalized XML examples above were validated as `Competition` fragments wrapped in an `OdfBody` envelope against
a temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Treat `DT_BRACKETS` as the authoritative progression graph for a Boxing event. It carries both schedule-like
  dependency information (`PreviousUnit`) and resolved-state information (`Competitor`, `WLT`, `Result`).
- Model `Bracket -> BracketItems -> BracketItem -> CompetitorPlace` as a four-level tree. `CompetitorPlace` is always
  a pair (`Pos=1`, `Pos=2`) for a head-to-head Boxing bout.
- Anchor each `BracketItem` to a unit RSC via `@Unit`. This is the join key to `DT_RESULT`, `DT_SCHEDULE`, and other
  unit-anchored messages.
- Resolve competitors from `PreviousUnit` edges. A competitor in `BracketItem A`'s `CompetitorPlace@Pos=p` is the
  winner or loser (per `CompetitorPlace/@WLT` and/or `PreviousUnit/@WLT`) of `PreviousUnit/@Unit`, or comes from a
  pool/match keyed by `PreviousUnit/@Value`.
- Keep `Seed` on `Competitor`, not as an `EventUnitEntry` entry. This differs from `DT_RESULT`, where seed is carried
  as `EventUnitEntry Type="EUE" Code="SEED"`.
- `BracketItem/@Result` is a denormalized ORIS-formatted display string. Use `DT_RESULT` for authoritative scoring,
  judge totals, warnings, and knockdowns.
- `ExtBracketItems` provides bracket-level extras: capture `SESSIONTYPE` (`CC@SessionType`) and `DECISION`
  (`SC@ResultCode`) when present; both repeat per `BracketItem` rather than per competitor.
- Sparse slots are normal in early versions of the message: `Competitor` may be absent while `PreviousUnit` and a
  `CompetitorPlace/@Code` (`SC@CompetitorPlace`) carry the slot's meaning.
- `StrikeOut="Y"` indicates the competitor should be struck out in this bracket item; the section explicitly ties
  this to `DQB` (Boxing disqualification before the bout).
- The overview describes a single `FNL` bracket for Boxing. Do not create per-phase brackets; phases are
  `BracketItems` under one `Bracket`.

## Code Appendix: Values Directly Visible in Pages 38-43

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@ResultStatus` | `OdfBody/@ResultStatus` | `START_LIST`, `INTERMEDIATE`, `OFFICIAL`, `PROVISIONAL` |
| `SC@Bracket` | `Bracket/@Code` | `FNL` per the BOX overview (single bracket for head-to-head competitions). No concrete value is printed in pages 38-43. |
| `SC@BracketItems` | `BracketItems/@Code` | `SFNL` visible in the sample on page 43 (semifinal phase). Section text mentions quarterfinals, semifinals, finals, round of 16, round of 32. |
| `SC@CompetitorPlace` | `CompetitorPlace/@Code` | No concrete value printed in pages 38-43. |
| `SC@WLT` | `CompetitorPlace/@WLT`, `PreviousUnit/@WLT` | `W`, `L` visible in the sample and referenced in descriptions. `DKO` is mentioned in prose as a no-winner case where `L` is still sent. |
| `SC@ResultType` | `CompetitorPlace/@ResultType` | `POINTS`, `RM_POINTS` referenced in the `Result` attribute description. |
| `SC@IRM` | `CompetitorPlace/@IRM` | `DQB` referenced in the `StrikeOut` description. |
| `SC@ResultCode` | `ExtBracketItem Code="DECISION"`, `BracketItem/@Result` prose | `WP` and `TKO` referenced in the `Result` description (`WP 3:0`, `TKO R3 1:23`). |
| `CC@SessionType` | `ExtBracketItem Code="SESSIONTYPE"` | No concrete value printed; section references C75 (ORIS) morning/afternoon. |
| `CC@EVENT_UNIT` | `BracketItem/@Unit`, `PreviousUnit/@Unit` | Unit RSCs visible in the sample: `BOXM54KG--------------SFNL0001----`, `BOXM54KG--------------QFNL0001----`, `BOXM54KG--------------QFNL0003----`. |
| `CC@ORGANISATION` | `Competitor/@Organisation`, `Description/@Organisation` | `ESP`, `NZL` visible in the sample. |
| `CC@PERSON_GENDER` | `Description/@Gender` | `M` visible in the sample. |
| `CC@DISCIPLINE` | `SportDescription/@DisciplineName` | No concrete value printed in pages 38-43. |
| `CC@EVENT` | `SportDescription/@EventName`, `OdfBody/@DocumentCode` | No concrete value printed in pages 38-43. |
| `CC@DISCIPLINE_GENDER` | `SportDescription/@Gender` | No concrete value printed in pages 38-43. |
| `CC@COMPETITION_CODE` | `OdfBody/@CompetitionCode` | No concrete value printed in pages 38-43. |
| `SCGEN@Source` | `OdfBody/@Source` | No concrete value printed in pages 38-43. |

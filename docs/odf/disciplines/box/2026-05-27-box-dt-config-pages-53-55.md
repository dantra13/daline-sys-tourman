# ODF BOX Data Dictionary: DT_CONFIG, Pages 53-55

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BOX_Data_Dictionary.pdf`, pages 53-55.

Source version: `SCOG/SYOG-2026-BOX-1.2 SFR`, dated 13 May 2026.

This note restructures the Boxing `DT_CONFIG` section into a practical domain reference for the event-level
configuration message. It covers competition format, round count and duration, participant count, event code label,
and bracket sizing for the first elimination phase.

## 2.3.8 Configuration

`DT_CONFIG` contains general configuration. Ideally the configuration should be provided before competition. When the
configuration for a particular event, phase or event unit is not known in advance, send the unknown attributes blank
(`Value=""`).

For Boxing, send one message per event.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@EVENT` | Event RSC. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_CONFIG` | Configuration message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | N/A | Not used. |
| `FeedFlag` | `P`, `T` | `P` production, `T` test. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| Case | Rule |
|---|---|
| Before competition | Send 1 day before the start of competition, but not before the Initial Weigh-In and/or Medical Examination on the day the draw is approved. |
| After any change | Trigger again after any configuration change. |
| Before start list | If possible, provide the configuration for a particular event, phase or event unit before the start list. |
| Unknown attributes | When a particular event, phase or event unit configuration is not known in advance, send the unknown attributes blank (`Value=""`). |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- Configs (1,1)
        +-- Config (1,N)
            +-- @Unit
            +-- ExtendedConfig (1,N)
                +-- @Type
                +-- @Code
                +-- @Pos
                +-- @Value
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | O | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | O | `S(20)` | Codes version applicable to the message. |

### `Competition / Configs / Config`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Unit` | M | `CC@EVENT` | Full RSC at event level. |

### `Competition / Configs / Config / ExtendedConfig`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `COMPETITION` | `FORMAT` | N/A | Always. | `SC@CompFormat` | Applicable competition format code. |
| `COMPETITION` | `PERIODS` | N/A | When available. | `#0` | Number of rounds. |
| `COMPETITION` | `DURATION` | N/A | When available. | `m:ss` | Round duration. |
| `COMPETITION` | `PARTICIPANTS` | N/A | Always. | Positive integer | Number of participants for this event. |
| `COMPETITION` | `CODE` | N/A | Always. | String | Event code label for this event, for example `-67 kg` or `+67 kg`. |
| `BRACKET` | `SIZE` | N/A | Always. | `SC@BracketItems` | Code for the first elimination phase of the event. |
| `BRACKET` | `COUNT` | N/A | Always. | Positive integer | Number of brackets. |

## Samples from the Dictionary, Normalized

The printed sample in the PDF uses `Type="EC"` plus codes such as `BRACKET_SIZE`, `COMPETITORS_NUM`, `PERIODS`, and
`DURATION`. The Boxing message-value table, however, defines the configuration rows as compound `Type`/`Code` pairs
such as `BRACKET/SIZE` and `COMPETITION/PERIODS`. The normalized samples below follow the table as canonical; see the
Modeling Notes for the reconciliation.

### Event configuration following the table definition

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <Configs>
    <Config Unit="BOXM57KG--------------------------">
      <ExtendedConfig Type="COMPETITION" Code="FORMAT" Value="SINGLE_ELIMINATION"/>
      <ExtendedConfig Type="COMPETITION" Code="PERIODS" Value="3"/>
      <ExtendedConfig Type="COMPETITION" Code="DURATION" Value="3:00"/>
      <ExtendedConfig Type="COMPETITION" Code="PARTICIPANTS" Value="28"/>
      <ExtendedConfig Type="COMPETITION" Code="CODE" Value="-57 kg"/>
      <ExtendedConfig Type="BRACKET" Code="SIZE" Value="R32"/>
      <ExtendedConfig Type="BRACKET" Code="COUNT" Value="1"/>
    </Config>
  </Configs>
</Competition>
```

### Printed sample shape preserved verbatim from the PDF

```xml
<Configs>
  <Config Unit="BOXM57KG--------------------------">
    <ExtendedConfig Type="EC" Code="BRACKET_SIZE" Value="R32"/>
    <ExtendedConfig Type="EC" Code="COMPETITORS_NUM" Value="28"/>
    <ExtendedConfig Type="EC" Code="PERIODS" Value="3"/>
    <ExtendedConfig Type="EC" Code="DURATION" Value="3:00"/>
  </Config>
</Configs>
```

## Message Sort

There is no general message sorting rule.

## XSD Validation

The first normalized XML example above was validated as a `Competition` fragment wrapped in an `OdfBody` envelope
against a temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified. The
printed-sample shape using `Type="EC"` with composite codes is preserved here for traceability against the PDF and is
not the canonical Dakar BOX shape.

## Modeling Notes

- Model BOX `DT_CONFIG` at event scope. The `Config/@Unit` is the full event RSC, not a phase or event-unit RSC. This
  differs from ATH, where `DT_CONFIG` is phase-scoped.
- Treat the message as a per-event snapshot. When any attribute changes, re-emit the full configuration for that event
  with an incremented `Version`.
- The Boxing message-value table defines `ExtendedConfig` rows with compound `Type`/`Code` pairs (`COMPETITION/FORMAT`,
  `COMPETITION/PERIODS`, `COMPETITION/DURATION`, `COMPETITION/PARTICIPANTS`, `COMPETITION/CODE`, `BRACKET/SIZE`,
  `BRACKET/COUNT`). The printed sample at the end of section 2.3.8 still uses the legacy `Type="EC"` shape with codes
  such as `BRACKET_SIZE`, `COMPETITORS_NUM`, `PERIODS`, and `DURATION`. Keep both shapes visible in ingestion tests and
  treat the table as canonical when validating Dakar BOX feeds.
- `COMPETITION/DURATION` is a clock-style duration `m:ss`, not an integer of seconds. Parse with leading minutes and
  zero-padded seconds.
- `COMPETITION/CODE` is a free-text event label such as `-67 kg` or `+67 kg`. Do not confuse it with the header
  `CompetitionCode` or with the RSC code carried in `Config/@Unit`.
- `BRACKET/SIZE` is the code of the first elimination phase (for example `R32`), which is not necessarily the same as
  the count implied by `COMPETITION/PARTICIPANTS`. Boxing brackets can include byes, so derive bracket geometry from
  `BRACKET/SIZE` and the participant count together.
- When configuration for a future event, phase or event unit is not yet known, send the affected attributes blank
  (`Value=""`) rather than omitting them.

## Code Appendix: Values Directly Visible in Pages 53-55

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@COMPETITION_CODE` | Header `CompetitionCode` | Referenced as the competition ID; no concrete values printed in the section. |
| `CC@EVENT` | Header `DocumentCode` and `Config/@Unit` | Sample uses RSC `BOXM57KG--------------------------`. |
| `SCGEN@Source` | Header `Source` | Referenced as the generating system code; no concrete values printed in the section. |
| `SC@CompFormat` | `ExtendedConfig Type="COMPETITION" Code="FORMAT"` | No concrete competition-format values printed in the section. |
| `SC@BracketItems` | `ExtendedConfig Type="BRACKET" Code="SIZE"` | Sample uses `R32`. |
| `FeedFlag` | Header `FeedFlag` | `P`, `T`. |

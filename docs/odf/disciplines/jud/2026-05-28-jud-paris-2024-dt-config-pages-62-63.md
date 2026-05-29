# ODF JUD Data Dictionary: DT_CONFIG, Pages 62-63

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 62-63.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note restructures the Paris 2024 Judo `DT_CONFIG` section into a practical reference for event configuration.

## 2.3.9 Configuration

`DT_CONFIG` contains general configuration. Ideally the configuration should be provided before competition.

For Judo, send one message per event.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Unique competition ID. |
| `DocumentCode` | `CC@Event` | Full event-level RSC. |
| `DocumentType` | `DT_CONFIG` | Configuration message. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Date` | Date | Local generation date. |
| `Time` | Time | Local generation time up to milliseconds. |
| `LogicalDate` | Date | Logical event date; normally the physical day unless transmission extends after midnight. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Case | Rule |
|---|---|
| Event configuration known | Send as soon as data is known for the event, before any `DT_RESULT`. |
| Configuration changes | Trigger again after any change. |
| Late configuration | If sent after any `DT_RESULT`, resend the affected `DT_RESULT` messages with the next version. |
| Before start list | If possible, provide configuration for the event, phase, or event unit before the start list. |

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
|---|---:|---|---|
| `Gen` | O | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | O | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | O | `S(20)` | Code-set version applicable to the message. |

### `Competition / Configs / Config`

| Attribute | M/O | Value | Meaning |
|---|---:|---|---|
| `Unit` | O | `CC@Unit` | Full RSC at event level. |

### `Competition / Configs / Config / ExtendedConfig`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EC` | `BRACKET_SIZE` | N/A | When available. | `SC@BracketItems` | Code for the first phase of the event. |

## Sample from the Dictionary

```xml
<Configs>
  <Config Unit="JUDM57KG--------------------------">
    <ExtendedConfig Type="EC" Code="BRACKET_SIZE" Value="R32"/>
  </Config>
</Configs>
```

## Source Sample Note

The printed sample uses `Value="R32"` for bracket size. The Paris 2024 JUD sport-code catalog lists bracket item values
with trailing hyphens for some rounds, such as `R32-`, `R64-`, and `FNL-`. Preserve the feed value received from ODF,
but validate against the active Paris code table when normalizing bracket-size values.

## Message Sort

There is no general message sorting rule.

## Modeling Notes

- Model JUD `DT_CONFIG` at event scope. The header `DocumentCode` is `CC@Event`, and the section says to send one
  message per event.
- The `Config/@Unit` table row says `CC@Unit`, but its description says full RSC at event level. Treat it as the same
  event-level RSC carried by `DocumentCode`, not as a contest/unit RSC.
- `EC/BRACKET_SIZE` is the explicit first-phase bracket-size configuration source. Do not infer it only from currently
  scheduled or populated bracket items.
- If configuration arrives after results, expect downstream result replay: affected `DT_RESULT` messages must be
  resent with the next version.

## Code Appendix: Paris 2024 Values

Catalog values come from Paris 2024 CC/SC code tables; message-specific restrictions remain in the field tables above.

| Code Entity | Section Usage | Values |
|---|---|---|
| `DocumentType` | Header | `DT_CONFIG` |
| `FeedFlag` | Header | `P`, `T` |
| `ExtendedConfig/@Type` | Configuration extension | `EC` |
| `ExtendedConfig/@Code` | Bracket size extension | `BRACKET_SIZE` |
| `SC@BracketItems` | First phase of event | `FNL-`, `REPF`, `REP1`, `SFNL`, `QFNL`, `8FNL`, `R32-`, `R64-`; source sample uses `R32`. |

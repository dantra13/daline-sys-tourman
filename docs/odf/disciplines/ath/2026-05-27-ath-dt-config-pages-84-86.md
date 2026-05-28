# ODF ATH Data Dictionary: DT_CONFIG, Pages 84-86

Source: `C:\Users\mella\Downloads\ODF_ATH_Data_Dictionary.pdf`, pages 84-86.

Source version: `SOG-2024-ATH-3.4 APP`, dated 3 May 2024.

This note restructures the Athletics `DT_CONFIG` section into a practical reference for phase-level configuration.

## 2.3.11 Configuration

`DT_CONFIG` contains general configuration for Athletics phases. It is sent before competition, one message for each
phase.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Phase` | Full phase RSC. Send one message for each phase. |
| `DocumentType` | `DT_CONFIG` | Configuration message. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Case | Rule |
|---|---|
| Before sports messages | Send prior to any ODF sport message, one message per phase. |
| After any change | Trigger again after configuration changes. |
| Before start list | If possible, provide the phase configuration before the start list. |
| Late configuration update | If `DT_CONFIG` is sent after any `DT_RESULT` for that phase, all up-to-date `DT_RESULT` messages for the phase must be re-sent. |

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

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competition/@Gen` | O | `S(20)` | General dictionary version. |
| `Competition/@Sport` | O | `S(20)` | Sport dictionary version. |
| `Competition/@Codes` | O | `S(20)` | Code-set version. |
| `Config/@Unit` | M | `CC@Unit` | Full phase RSC. |

## ExtendedConfig Values

| Type | Code | Pos | Value | When |
|---|---|---|---|---|
| `EC` | `CATEGORY` | N/A | `SC@UnitCategory` | Always. |
| `EC` | `START_IN_LANES` | N/A | `Y` | Track events up to 800m, including relays, when competitors start in lanes. |
| `EC` | `UNITS_PHASE` | N/A | `#0` | Track phases with more than one unit, including combined events. |

## Sample from the Dictionary, Normalized

```xml
<Competition Gen="OG2024-GEN-3.4 APP" Sport="SOG-2024-ATH-3.4 APP" Codes="OG2024">
  <Configs>
    <Config Unit="ATHM100M--------------FNL---------">
      <ExtendedConfig Type="EC" Code="START_IN_LANES" Value="Y"/>
      <ExtendedConfig Type="EC" Code="CATEGORY" Value="A"/>
      <ExtendedConfig Type="EC" Code="UNITS_PHASE" Value="1"/>
    </Config>
  </Configs>
</Competition>
```

## Message Sort

There is no message sorting rule.

## XSD Validation

The normalized XML example above was validated as a `Competition` fragment wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Model `DT_CONFIG` at phase scope, not event scope.
- `CATEGORY` is always expected and should be available before interpreting phase behaviour.
- `START_IN_LANES` controls track presentation/start assumptions and is narrower than "track event" in general.
- `UNITS_PHASE` is important for phases with multiple heats/groups; if it changes late, consumers should expect
  corresponding `DT_RESULT` re-sends for that phase.

# ODF BDM Data Dictionary: DT_CONFIG, Pages 72-73

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BDM_Data_Dictionary.pdf`, pages 72-73.

Source version: `SYOG-2026-BDM-1.1 SFR`, dated 13 May 2026.

This note restructures the Badminton `DT_CONFIG` section into a practical reference for event configuration.

## 2.3.11 Configuration

`DT_CONFIG` contains general configuration. Ideally the configuration should be provided before competition.

For Badminton, send one message per event as soon as the bracket size is known, regardless of whether the competition
starts with pools.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@EVENT` | Full event RSC. |
| `DocumentType` | `DT_CONFIG` | Configuration message. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| Case | Rule |
|---|---|
| Event configuration | Send for all events, one message per event. |
| Bracket size known | Send as soon as the bracket size is known, regardless of whether the competition starts with pools. |

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
| `Config/@Unit` | M | `CC@EVENT` | Full event RSC. |

## ExtendedConfig Values

| Type | Code | Pos | Value | When |
|---|---|---|---|---|
| `BRACKET` | `SIZE` | N/A | `SC@BracketItems` | When available. Send the code for the first bracket phase of the event. |

## Sample from the Dictionary, Normalized

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-BDM-1.1 SFR" Codes="SYOG-2026">
  <Configs>
    <Config Unit="BDMMSINGLES-----------------------">
      <ExtendedConfig Type="BRACKET" Code="SIZE" Value="R32"/>
    </Config>
  </Configs>
</Competition>
```

## Source Sample Note

The printed source sample shows a generic table-tennis-style `Unit` and uses `ExtendedConfig Type="EC"
Code="BRACKET_SIZE"` plus `Code="PERIODS"`. The BDM message-value table, however, defines the Badminton-specific
configuration row as `Type="BRACKET"`, `Code="SIZE"`, and `Value=SC@BracketItems`. This note follows the BDM table as
canonical and treats the printed sample as generic/non-BDM.

## Message Sort

There is no general message sorting rule.

## XSD Validation

The normalized XML example above was validated as a `Competition` fragment wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Model BDM configuration at event scope. This differs from ATH, where `DT_CONFIG` is phase-scoped.
- `BRACKET/SIZE` should be available once the first bracket phase is known.
- Do not infer bracket size only from currently scheduled matches; the config is the explicit bracket-size source.
- Keep the source-sample mismatch visible in ingestion tests, because generated sample fixtures may use the generic
  `EC/BRACKET_SIZE` shape even though the table defines `BRACKET/SIZE`.

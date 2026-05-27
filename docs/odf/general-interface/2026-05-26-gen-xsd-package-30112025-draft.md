# ODF GEN XSD Package: odf2-schema-30112025-DRAFT

Source ZIP: `C:\Users\mella\Downloads\odf-schema-2026.zip`.

Related PDF reference: `C:\Users\mella\WebstormProjects\sportivo\docs\references\odf\ODF_GEN_R-OWG2026-GEN.pdf`.

This note documents the XSD package shipped alongside the OWG2026 General Messages Interface material. Treat the schema package version separately from the PDF document version.

## Version Identity

| Artifact | Version / release identity | Date signal | Notes |
|---|---|---|---|
| General Messages Interface PDF | `OWG2026-GEN-4.6 APP` | 10 December 2025 | Human-readable interface document and change control baseline. |
| XSD package folder | `odf2-schema-30112025-DRAFT` | 30 November 2025 by folder name | Draft schema package identity. |
| XSD files in ZIP | `odf2.xsd`, `odf2-structure.xsd`, `odf2-values.xsd` | Files timestamped 18 November 2025 in the ZIP | Machine-readable generic schema files. |
| Local ZIP file | `odf-schema-2026.zip` | Download/local timestamp 26 May 2026 | Local file timestamp is not a schema release version. |

Do not collapse these into a single "2026 schema" version. In implementation notes, tests, and validation reports, cite both the PDF version and the schema package identity.

Recommended source labels:

- PDF source: `ODF GEN OWG2026-GEN-4.6 APP, 10 Dec 2025`.
- XSD source: `odf2-schema-30112025-DRAFT`.
- Sport dictionary source: cite the sport, event, version, and edition separately, for example Paris 2024 FBL Data Dictionary material.

## Package Contents

| File | Purpose |
|---|---|
| `odf2.xsd` | Root `OdfBody` and common message header. Includes `odf2-structure.xsd`. |
| `odf2-structure.xsd` | Generic ODF body structures for competition content, including schedule, results, brackets, participants, medals, records, images, and communications. Includes `odf2-values.xsd`. |
| `odf2-values.xsd` | Generic enumerations and scalar restrictions such as document types, schedule statuses, result statuses, feed flags, RSC length, and medal values. |

The ZIP also contains `__MACOSX` metadata entries. Ignore those entries for schema analysis.

## General Schema Observations

The XSD package is generic across many ODF message types. It does not fully encode the message-specific semantics described in the General Messages Interface PDF or in sport data dictionaries.

Examples:

- `OdfBody/@DocumentSubtype` is an optional `xs:string`; the schema does not restrict it to `PRE` or `SYNC`.
- `OdfBody/@DocumentSubcode` remains available as an optional generic header attribute even where a specific message says it is not applicable.
- `Competition` uses broad generic content structures, so schema-valid XML can still be semantically invalid for a specific document type.

The package should be treated as a structural compatibility layer, not as the only source of truth for ODF business semantics.

## Schema Compile Note

Loading all three XSD files explicitly in a strict .NET `XmlSchemaSet` reduces the package to one unresolved type error:

| Location | Issue |
|---|---|
| `odf2-structure.xsd` line 915 | `officialCommunicationType/ImageData` references `RecordBrokenType`, but no matching type declaration exists in the package. |

Loading only `odf2.xsd` also reports unresolved included types in this environment unless the included files are added explicitly. Therefore, validation tooling should be tested against this exact package before relying on it in CI or ingestion pipelines.

## DT_SCHEDULE Compatibility Notes

This table compares the `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` section in `OWG2026-GEN-4.6 APP` with the `odf2-schema-30112025-DRAFT` package.

| Area | PDF / change-control expectation | XSD package state | Impact |
|---|---|---|---|
| `MediaAccess` | Added for non-competition units in `2026-4.3`. The PDF page extraction includes it as a schedule-unit attribute. | No `MediaAccess` attribute exists on `scheduleUnitType`. | A document following the PDF may fail against this draft schema unless validation is relaxed or patched. |
| `Start/@PreviousValue` | Listed in document control as added in `2026-4.0` for `DT_SCHEDULE/_UPDATE Start`. | No `PreviousValue` attribute exists on `scheduleStartType`. | Treat as PDF/change-control vs schema drift. Verify against later schema packages or integration samples before modeling it as schema-enforced. |
| `ModificationIndicator` / `ModificatorIndicator` | Removed from applicable messages in `2026-4.0`. | No matching attribute found in the schema package. | Schema aligns with the 2026 general document and differs from older Paris 2024-era dictionary material. |
| `HideUnitNum` | Added in `2026-4.3`. | Present on `scheduleUnitType`. | Schema and PDF align. |
| `Guide` / `GuideID` | Guide elements introduced in `2026-4.1`. | `GuideID` exists in structure types. | Schema and PDF broadly align. |
| `PreviousUnit/@Unit` phase support | `2026-4.4` says the value may represent event units or phases. | `PreviousUnit/@Unit` is plain `xs:string`. | Schema is permissive enough to accept phase references but does not enforce RSC category. |
| `Session/@StartDate` and `Session/@EndDate` in `PRE` | PDF says early Schedule by Day may use date-only values when times are unknown. | `SessionType` requires `xs:dateTime` for both attributes. | Date-only `PRE` sessions described by the PDF are not represented by this schema type. |
| `Unit/@PhaseType` | PDF marks it optional, with conditional use for phase/event units and omission for non-sport activities. | `scheduleUnitType` requires `PhaseType`. | Non-competition units described by the PDF may need a placeholder or schema relaxation when validated against this draft package. |
| `Start/Competitor` | PDF message-values table describes `Competitor` as required under `Start`. | `scheduleStartType` has `Competitor` as optional. | Schema is more permissive than the PDF. Semantic validation should enforce the message rule when applicable. |
| `DocumentSubtype` | PDF defines schedule-specific semantics for `PRE`, `SYNC`, and absence; not applicable for update messages. | Header attribute is generic optional `xs:string`. | Domain validation must enforce allowed values and document-type-specific rules. |
| `DocumentSubcode` | Older change control says `DocumentSubcode` was removed from `DT_SCHEDULE/_UPDATE`. | Header still exposes generic optional `DocumentSubcode`. | Do not infer schedule-specific header validity from the generic root schema alone. |

## Modeling Policy

For Sportivo, keep these version dimensions explicit:

| Dimension | Example value | Why it matters |
|---|---|---|
| General interface document | `OWG2026-GEN-4.6 APP` | Defines message semantics and change history. |
| General XSD package | `odf2-schema-30112025-DRAFT` | Defines available machine schema constraints for validation. |
| Sport data dictionary | For example, FBL Paris 2024 dictionary material | Defines sport-specific statuses, codes, triggers, and discipline examples. |
| Common/sport codes release | Event-specific code package | Defines concrete SC/CC/RSC values. |

Implementation should separate:

- XML schema validation against the selected XSD package.
- ODF semantic validation against the selected PDF and sport dictionary version.
- Domain modeling decisions in Sportivo.

When the PDF and XSD disagree, record the disagreement in the reference doc and decide whether the ingestion profile follows the schema strictly, the PDF semantics, or a target-event integration sample.

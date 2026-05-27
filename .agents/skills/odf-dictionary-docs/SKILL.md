---
name: odf-dictionary-docs
description: Create structured Markdown reference documents from ODF sport Data Dictionary PDFs. Use when extracting ODF message sections such as DT_SCHEDULE, DT_RESULT, DT_CURRENT, DT_PLAY_BY_PLAY, DT_STATS, entries, participants, medals, brackets, or discipline overview/applicable-message/document-control sections; when the user provides PDF page ranges; when adding SC/CC code appendices from Olympic Data Feed HTML code pages; or when producing XSD-aware XML examples for domain modeling.
---

# ODF Dictionary Documentation

Use this skill to turn ODF sport Data Dictionary sections into stable Markdown references for Sportivo domain modeling.

## Core Workflow

1. Read the requested PDF pages with `uv` and a PDF library such as `pymupdf`.
2. Extract hyperlinks from those pages; ODF `SC @...` and `CC @...` references usually point to HTML code tables.
3. Build a Markdown document that is readable as a domain reference, not just raw transcription.
4. Add an appendix of English code values for every referenced `SC`/`CC` table that matters to the section.
5. If XSD files are provided, validate at least one compact XML example and document any schema/dictionary mismatch.
6. Save output under `docs/references/odf/` unless the user asks otherwise.

Read [references/document-pattern.md](references/document-pattern.md) before writing or updating an ODF dictionary reference.

## Page and Section Discipline

- Always preserve source page ranges in the title and introduction.
- For message-specific requests, extract the complete section for the requested message: description, header values, trigger/frequency, message structure, message values, examples, sort rules.
- If the user asks for general dictionary extraction or a new discipline, first inspect early pages around `2 Messages`, especially:
  - `2.1 <Discipline> Overview`
  - `2.2 Applicable Messages`
- For any new discipline, organize names and content around the discipline code/name from the PDF, for example `FBL - Football`, not assumptions from prior football work.
- Check the final pages for `4 Document Control`; include a concise Document Control section or appendix when creating a discipline-level reference, and include relevant change notes for message-specific docs when useful.

## Code Tables

- Treat `SC` as sport-specific code and `CC` as common code.
- Prefer URLs embedded in the PDF over guessed URLs.
- If an individual sport-code page is missing, check the aggregate sport-code page, for example `.../og_sc/odf_sc_FBL.htm`, and filter by `Code_Entity`.
- Keep appendix tables in English (`ENG Description`) unless the user asks for another language.
- For huge common master-data tables such as `Organisation`, `Country`, or very large `EventUnit` lists, embed only the discipline-filtered rows when relevant. Otherwise provide source links and row counts.

## XML and XSD Handling

- Use XML examples to make hierarchy and attribute placement concrete.
- Validate examples when the user provides XSDs, but do not assume the XSD matches the PDF exactly.
- If validation requires a temporary patch for an unrelated broken schema reference, state that clearly and never modify the user's original XSD files.
- If the PDF and XSD disagree, document the mismatch in the Markdown and keep the PDF semantics visible for domain modeling.

## Style

- Documentation and headings should be in English unless the user asks otherwise.
- Use clean Markdown tables with columns such as `Attribute`, `M/O`, `Value`, `Meaning`, or `Type`, `Code`, `Pos`, `Expected When`, `Value`, `Meaning`.
- Add `Modeling Notes` at the end. These notes should translate ODF output constraints into internal domain-model implications.
- Keep direct source wording concise; restructure into human-readable reference material.

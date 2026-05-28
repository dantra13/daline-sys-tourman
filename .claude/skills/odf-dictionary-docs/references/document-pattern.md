# ODF Dictionary Document Pattern

Use this pattern when creating Markdown references from ODF sport Data Dictionary PDFs.

## Filename

Prefer:

```text
docs/references/odf/YYYY-MM-DD-<discipline>-<dt-message-or-topic>-pages-<start>-<end>.md
```

Examples:

```text
docs/references/odf/2026-05-25-fbl-dt-result-pages-30-48.md
docs/references/odf/2026-05-25-fbl-dt-play-by-play-pages-52-58.md
```

Use lowercase discipline code and message/topic. Keep `dt-` in message filenames.

## Recommended Structure

```markdown
# ODF <DISCIPLINE> Data Dictionary: <MESSAGE/TOPIC>, Pages <N-M>

Source: `<absolute PDF path>`, pages <N-M>.

Short purpose paragraph.

## <Original Section Number and Title>

Plain-English explanation.

## Header Values

| Attribute | Value | Meaning |

## Trigger and Frequency

| Status/Condition | When to send |

## Message Structure

```text
OdfBody
└─ Competition ...
```

## Message Values

One subsection per important element path.

## Sample from the Dictionary, Normalized

Only when the PDF has a useful sample or when it needs correction/normalization.

## Message Sort

Sort rule.

## XSD-Aligned XML Example

Example plus validation caveat.

## Modeling Notes

Bullets that translate ODF constraints into domain-model consequences.

## Code Appendix: SC and CC Values

Source index plus code tables.

## Document Control

Only include when requested, when producing a discipline-level reference, or when relevant to the extracted message.
```

## Extraction Checklist

- Extract text from the requested PDF pages using `uv`.
- Extract PDF links from the same pages with `page.get_links()`.
- Search the whole PDF for additional links if a referenced code is not linked on the requested pages.
- Download HTML code pages with PowerShell `Invoke-WebRequest` or Python `requests`.
- Parse code tables with `beautifulsoup4`.
- Use `ENG Description` as the default language.
- Include row counts and source URLs in the appendix index.
- Validate XML examples with `lxml` when XSDs are available.
- Note XSD drift, draft schema gaps, unresolved references, or PDF/XSD mismatches.

## Useful Commands

Extract text from a page range:

```powershell
@'
import fitz
pdf = r'C:\path\to\ODF_Dictionary.pdf'
doc = fitz.open(pdf)
for pno in range(start_page - 1, end_page):
    print(f"\n--- PDF page {pno + 1} ---")
    print(doc[pno].get_text("text").replace("\x00", ""))
'@ | uv run --with pymupdf python -
```

Extract links from a page range:

```powershell
@'
import fitz
pdf = r'C:\path\to\ODF_Dictionary.pdf'
doc = fitz.open(pdf)
for pno in range(start_page - 1, end_page):
    links = sorted({link.get("uri") for link in doc[pno].get_links() if link.get("uri")})
    if links:
        print(f"--- page {pno + 1} ---")
        print("\n".join(links))
'@ | uv run --with pymupdf python -
```

Validate XML against supplied XSDs:

```powershell
@'
from lxml import etree
from pathlib import Path

schema_path = Path(r'C:\path\to\odf2.xsd')
schema = etree.XMLSchema(etree.parse(str(schema_path)))
doc = etree.fromstring(xml_text.encode("utf-8"))
print(schema.validate(doc))
for error in schema.error_log:
    print(error.message)
'@ | uv run --with lxml python -
```

If a draft XSD has an unrelated unresolved type reference, validate against a temporary copy and document exactly what was patched.

## Common Gotchas

- ODF PDFs use 1-based printed page numbers; PDF libraries use 0-based indexes.
- Some code links in the PDF are `http`, but `https` may be easier to fetch; preserve source links when possible.
- Some sport-specific code entities exist only in the aggregate sport page, not as individual HTML pages.
- `StatsItem`, `EventUnitEntry`, `ExtendedInfo`, and similar generic nodes are path-sensitive. The same `Code` can mean different things under different element paths.
- A full message often replaces prior state, while an update message patches only included entities. Capture this behavior in modeling notes.

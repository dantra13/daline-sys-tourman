# ODF ATH Data Dictionary: DT_CURRENT, Pages 50-52

Source: `C:\Users\mella\Downloads\ODF_ATH_Data_Dictionary.pdf`, pages 50-52.

Source version: `SOG-2024-ATH-3.4 APP`, dated 3 May 2024.

This note restructures the Athletics `DT_CURRENT` section into readable Markdown for domain modeling. It covers the
live current-information message, trigger rules, the live `ExtendedResult` payload that carries intermediate timing and
ranking, an XSD-aligned XML example, and an appendix of English SC/CC values downloaded from the Paris 2024 ODF code
pages.

## 2.3.5 Current Information

`DT_CURRENT` carries the live information for a competition in progress. In Athletics it is sent for track and road
events to provide live information. In combined events, the last heat of the last event (800 m / 1500 m) carries the
overall standings of the combined event instead of the per-unit live time/rank.

Compared with `DT_RESULT`, this is a compact live-state message scoped to track and road units. There is no clock
element — live state is expressed as per-competitor intermediate times, ranks, gaps, and the last data point crossed.

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Unit` | Full RSC of the unit. |
| `DocumentSubcode` | N/A | Not used for this message. |
| `DocumentType` | `DT_CURRENT` | Current message. |
| `DocumentSubtype` | N/A | Not used for this message. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time, up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day. Usually the physical day, except when the unit or message transmission crosses midnight. |
| `Source` | `SC @Source` | System that generated the message. |

## Trigger and Frequency

| Trigger | Meaning |
|---|---|
| Track and road units only | `DT_CURRENT` is restricted to track and road competition units. |
| Update on new info | Send every time updated information becomes available, using ORIS C77 as the data-requirement guide. |
| Minimum interval | Never send more frequently than every 2 seconds. |
| Stop condition | Stop sending when the corresponding `DT_RESULT` for the same RSC is `UNOFFICIAL` or `OFFICIAL`. |
| Coverage | All competitors without an IRM in the unit are always included in the message. |

## Message Structure

```text
OdfBody
└─ Competition (0,1)
   ├─ @Gen
   ├─ @Sport
   ├─ @Codes
   └─ Result (0,N)
      ├─ @SortOrder
      ├─ @StartSortOrder
      ├─ ExtendedResults (0,1)
      │  └─ ExtendedResult (1,N)
      │     ├─ @Type
      │     ├─ @Code
      │     ├─ @Pos
      │     ├─ @Value
      │     ├─ @Rank
      │     ├─ @RankEqual
      │     ├─ @Diff
      │     ├─ @Move
      │     └─ @Arrive
      └─ Competitor (1,N)
         ├─ @Code
         ├─ @Type
         └─ @Organisation
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional | `S(20)` | Code-set version applicable to the message. |

## Result

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `SortOrder` | Mandatory | Numeric | Current order of the competitors. |
| `StartSortOrder` | Mandatory | Numeric | Start-list order of all competitors in the event unit, per sport rules. |

## Result / ExtendedResults / ExtendedResult (LIVE)

The PDF defines a single `ExtendedResult` row for live information.

| Type | Code | Pos | Expected When |
|---|---|---|---|
| `ER` | `LIVE` | N/A | All road and track events. In the final unit of combined events, this row carries the overall standings of the combined event for those in the heat. |

### ExtendedResult attributes

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Value` | Optional | `h:mm:ss.ff`, `h:mm:ss.f`, or `h:mm:ss` | Competitor time at the most recent data point, including finish. At finish, send the same precision as `DT_RESULT` for close finishes. Do not send leading zeros or empty hour/minute components. Omit for competitors not at the same data point as the leader. If the value at the finish is the transponder time, update it once the photo time is available (and the authoritative result moves into `Result/Result`). Not included in the final unit of combined events. |
| `Rank` | Optional | Text | Cumulative rank at this point. In the final unit of combined events, this is the overall standings rank. |
| `RankEqual` | Optional | `S(1)` | Send `Y` when the rank is tied with another competitor; otherwise omit. |
| `Diff` | Optional | `+m:ss.ff`, `+h:mm:ss`, `+mm:ss`, or `+m:ss` | Gap behind the leader at this point. Do not send for the leader. Not included in the final unit of combined events. |
| `Move` | Optional | Numeric `##0` or `-##0` | Change in rank since the previous point. Example: from rank 9 to rank 15 is `-6`. Send `0` for no change. |
| `Arrive` | Optional | Numeric `####0` | Last data point crossed. Metres for track events, kilometres for road events. Not included in the final unit of combined events. |

## Result / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | Mandatory | `S(1)` | `A` for athlete, `T` for team. |
| `Organisation` | Mandatory | `CC @Organisation` | Competitor organisation. |

## Message Sort

Sort by `Result/@SortOrder`.

## XSD-Aligned XML Example

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="ATHM-100M-------------RND1000100--"
         DocumentType="DT_CURRENT"
         Version="14"
         FeedFlag="T"
         Date="2026-05-27"
         Time="20:11:42.500"
         LogicalDate="2026-05-27"
         Source="STAATH1">
  <Competition Gen="3.4" Sport="ATH-3.4" Codes="SOG-2024">
    <Result SortOrder="1" StartSortOrder="3">
      <ExtendedResults>
        <ExtendedResult Type="ER" Code="LIVE" Pos="N/A"
                        Value="9.84" Rank="1" Move="2" Arrive="100"/>
      </ExtendedResults>
      <Competitor Code="ATHM-100M-----USA01" Type="A" Organisation="USA"/>
    </Result>
    <Result SortOrder="2" StartSortOrder="1">
      <ExtendedResults>
        <ExtendedResult Type="ER" Code="LIVE" Pos="N/A"
                        Value="9.89" Rank="2" Diff="+0.05" Move="-1" Arrive="100"/>
      </ExtendedResults>
      <Competitor Code="ATHM-100M-----JAM01" Type="A" Organisation="JAM"/>
    </Result>
  </Competition>
</OdfBody>
```

The example is illustrative — the user has not supplied an Athletics XSD, so it has not been schema-validated. If the
XSD is provided later, validate that `ExtendedResults` and the `LIVE` `ExtendedResult` row sit inside `Result` and that
the attribute set on `ExtendedResult` matches the PDF (note the PDF locates `Value`/`Rank`/etc. on the `ExtendedResult`
itself, not on a child).

## Modeling Notes

- `DT_CURRENT` is a per-unit live projection over track and road events; there is no clock element. Treat the
  per-competitor `ExtendedResult LIVE` row as the canonical live tick.
- `Arrive` is the live progress marker (metres for track, km for road). Combine `Arrive` with `Value`/`Diff`/`Rank` to
  reconstruct the live race-state timeline; do not infer position from message ordering.
- `Rank` is cumulative at the current data point. `RankEqual=Y` flags a tie — model rank as `(rank, tied?)` rather than
  a strict integer ordering.
- `Move` is the rank delta versus the previous data point, not since start. Re-derive on each tick from the previous
  `DT_CURRENT` if you need a longer history.
- Combined events override the live semantics in the final unit (800 m / 1500 m). In that case `LIVE` carries the
  overall combined-event standings: `Value`, `Diff`, and `Arrive` are not included; `Rank` is the overall standings
  rank. Model combined events as a separate projection state, not as a final per-heat tick.
- The stop condition is the matching `DT_RESULT` reaching `UNOFFICIAL` or `OFFICIAL`. Bind the live projection lifetime
  to that result-status transition rather than to clock or time-of-day.
- `Competitor.Type` distinguishes athlete (`A`) from team (`T`). For relays, expect `T` and bind to the team
  registration, not to an individual.
- Paralympic Athletics does not include `DT_CURRENT` (per the discipline overview). Model `DT_CURRENT` as
  Olympic-only / non-Paralympic for ATH.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from
the Paris 2024 Olympic Data Feed code pages on 2026-05-27. Tables keep the English descriptions because this document
models the English ODF vocabulary.

### Source Index

| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Source` | ATH | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_ATH.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_ATH.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @Unit` | ATH `EventUnit` rows (link only) | 690 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @Organisation` | Common master data (link only) | 258 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |

### SC @Source

| Code | ENG Description |
| --- | --- |
| INVATH1 | Origin for messages from OVR at INV for ATH |
| STAATH1 | Origin for messages from OVR at STA for ATH |
| TROATH1 | Origin for messages from OVR at TRO for ATH |

### CC @Competition

| Id | ENG Description |
| --- | --- |
| OG2024 | Olympic Games Paris 2024 |
| OG2024-HT | Olympic Games Paris 2024 - HT |
| OG2024-ITL | Olympic Games Paris 2024 - ITL |
| OG2024-MST1 | Olympic Games Paris 2024 - MST1 |
| OG2024-MST2 | Olympic Games Paris 2024 - MST2 |
| OG2024-MST3 | Olympic Games Paris 2024 - MST3 |
| OG2024-MST4 | Olympic Games Paris 2024 - MST4 |
| OG2024-TEV | Olympic Games Paris 2024 - TEV |
| OG2024-TR1 | Olympic Games Paris 2024 - TR1 |
| OG2024-TR2 | Olympic Games Paris 2024 - TR2 |

### CC @Unit (ATH EventUnit rows)

`CC @Unit` is the ATH-filtered `EventUnit` set. The Paris 2024 master table contains 690 ATH rows — too large to embed
inline. Use the source link in the index above as the authoritative value list when modeling unit RSCs for Athletics.

### CC @Organisation

`CC @Organisation` is a large common master-data table rather than a current-message enumeration. The downloaded Paris
2024 code page contains 258 `Organisation` rows. Use the source link in the index above as the authoritative value list
when modeling `Organisation` fields.

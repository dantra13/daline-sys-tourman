
# ODF BKB Data Dictionary: DT_CURRENT, Pages 50-52

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 50-52.
Source version: `SOG-2024-BKB-3.4 APP`, 19 January 2024.

This note restructures the basketball `DT_CURRENT` section into readable Markdown for domain modeling. It covers the current information message, trigger rules, clock payload, current score payload, an XML example, and an appendix of English SC/CC values downloaded from the Paris 2024 ODF code pages.

## 2.3.5 Current Information

`DT_CURRENT` carries the current information for a basketball game which is live. As a team sport with a running clock, it bundles the latest applicable game state together with the clock.

Compared with `DT_RESULT`, this is a compact live-state message: current period/clock plus current score rows for the two competing teams.

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Unit` | Full RSC of the unit (match). |
| `DocumentSubcode` | N/A | Not used for this message. |
| `DocumentType` | `DT_CURRENT` | Current message. |
| `DocumentSubtype` | N/A | Not used for this message. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time, up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day. Usually the physical day, except when the unit or message transmission crosses midnight. See the ODF Foundation for the full rule. |
| `Source` | `SC @Source` | System that generated the message. |

## Trigger and Frequency

| Trigger | Meaning |
|---|---|
| Start and end of every period | Used to start/stop the clock. |
| Score changes | Send immediately after every change in the score. |
| Clock starts/stops | Send every time the clock starts and stops. |
| Periodic heartbeat during play | During play, after start and outside breaks, send every 30 seconds after the previous `DT_CURRENT` when no other activity has triggered the message. |

Note the heartbeat cadence is tighter than in football (basketball uses 30 seconds, FBL uses 5 minutes), reflecting the higher rate of scoring and clock events in basketball.

## Message Structure

```text
OdfBody
└─ Competition (0,1)
   ├─ @Gen
   ├─ @Sport
   ├─ @Codes
   ├─ Clock (0,1)
   │  ├─ @Period
   │  ├─ @Time
   │  └─ @Running
   └─ Result (0,N)
      ├─ @Result
      ├─ @SortOrder
      ├─ @StartSortOrder
      ├─ @ResultType
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

## Clock

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Period` | Optional | `SC @Period` | Current period if the information is available automatically from the timing device. |
| `Time` | Mandatory | `mm:ss` | Clock value. |
| `Running` | Mandatory | `S(1)` | `Y` when the clock is running, `N` when stopped. |

```xml
<Clock Period="Q2" Time="1:34" Running="Y"/>
```

## Result

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Result` | Optional | Numeric `##0` | Current score (points) for the team for the unit. |
| `SortOrder` | Mandatory | Numeric | Sequential order: First named team is `1`, Visitor is `2`. |
| `StartSortOrder` | Mandatory | Numeric | Same value as `SortOrder`. |
| `ResultType` | Mandatory | `SC @ResultType` | Type of `Result`, either points or IRM-with-points for the corresponding event unit. |

Note: in the PDF, `Competition/Result/@ResultType` is annotated `M` (Mandatory) in the per-element table even though the structural cardinality of `Result` is `(0,N)`. When `Result` is present, `ResultType` must be supplied.

## Result / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Competitor's ID. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Organisation` | Mandatory | `CC @Organisation` | Competitor's organisation. |

## Sample from the Dictionary, Normalized

The PDF embeds only a clock fragment as a sample:

```xml
<Competition>
<Clock Period="Q2" Time="1:34" Running="Y" />
```

That fragment is incomplete (no closing `</Competition>` and no score rows). A complete, well-formed example is given below.

## Message Sort

The PDF says: sort by `SortOrder`. In practice this orders the two `Result` rows so the First named team (`SortOrder=1`) precedes the Visitor (`SortOrder=2`). `Clock` carries period state and is not part of the sort.

## XML Example

The Paris 2024 BKB Data Dictionary does not ship an XSD with this skill input, so the example below is not validated against a schema. It is consistent with the dictionary's structural and attribute rules.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="BKBWTEAM5-------------GPA-000100--"
         DocumentType="DT_CURRENT"
         Version="12"
         FeedFlag="T"
         Date="2026-05-27"
         Time="20:14:08.000"
         LogicalDate="2026-05-27"
         Source="BCYBKB1">
  <Competition Gen="3.4" Sport="BKB-3.4" Codes="SOG-2024">
    <Clock Period="Q2" Time="1:34" Running="Y"/>
    <Result Result="42" SortOrder="1" StartSortOrder="1" ResultType="POINTS">
      <Competitor Code="BKBWTEAM5------USA01" Type="T" Organisation="USA"/>
    </Result>
    <Result Result="37" SortOrder="2" StartSortOrder="2" ResultType="POINTS">
      <Competitor Code="BKBWTEAM5------ESP01" Type="T" Organisation="ESP"/>
    </Result>
  </Competition>
</OdfBody>
```

## Modeling Notes

- `DT_CURRENT` should map to a live scoreboard projection, not to the authoritative match record. The authoritative full result still lives in `DT_RESULT`.
- Keep clock state separate from result rows. Clock transitions (start/stop, period change) trigger the message even without a score change.
- `Running` is an explicit state bit. Do not infer it from period code or remaining time alone; periods such as `HT` or `OT3` can be running or stopped depending on the actual game state.
- `Period` is optional because some venues may not have automatic timing-device integration. Persist it when present, but the domain model must tolerate its absence and fall back to last-known period.
- Result ordering is explicit. Preserve `SortOrder`/`StartSortOrder` and do not derive home/away ordering from array position.
- `Result/@Result` uses `Numeric ##0`, i.e. up to three digits, which comfortably covers all basketball game totals including overtime.
- Heartbeat is 30 seconds in BKB versus 5 minutes in FBL. Live-state pipelines should be tuned for this higher message rate, and consumers should be idempotent on identical content (`Version` ascending).

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-27. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index

| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Source` | BKB | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm) |
| `SC @Period` | BKB | 17 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_BKB.htm) |
| `SC @ResultType` | BKB | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_BKB.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @Unit` | BKB EventUnit rows | 130 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @Organisation` | Common master data | 259 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |

### SC @Source

| Code | ENG Description |
| --- | --- |
| BCYBKB1 | Origin for messages from OVR at BCY for BKB |
| LILBKB1 | Origin for messages from OVR at LIL for BKB |

### SC @Period

| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| Q1_H | 1 |  | Half of quarter 1 |
| Q1 | 2 |  | Quarter 1 |
| Q2_H | 3 |  | Half of quarter 2 |
| Q2 | 4 |  | Quarter 2 |
| HT | 5 |  | Half Time |
| Q3_H | 6 |  | Half of quarter 3 |
| Q3 | 7 |  | Quarter 3 |
| Q4_H | 8 |  | Half of quarter 4 |
| Q4 | 9 |  | Quarter 4 |
| OT1 | 10 |  | Overtime 1 |
| OT2 | 11 |  | Overtime 2 |
| OT3 | 12 |  | Overtime 3 |
| OT4 | 13 |  | Overtime 4 |
| OT5 | 14 |  | Overtime 5 |
| OT6 | 15 |  | Overtime 6 |
| OT | 16 | All overtimes | Overtime |
| TOT | 17 |  | Total |

### SC @ResultType

| Code | ENG Description |
| --- | --- |
| IRM | Invalid Result Mark |
| IRM_POINTS | For both, points and invalid result mark |
| POINTS | Points |

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

### CC @Unit (BKB EventUnit rows)

| Code | Gender | Event | phase | Eventunit | Level | Order | schedule | medalflag | ENG Description |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| BKB------------------------------- | - | ------------------ | ---- | -------- | Discipline | 2 | N | 0 | Basketball |
| BKB-----------------------BCY----- | - | ------------------ | ---- | BCY----- | Venue | 3 | N | 0 | BCY |
| BKB-----------------------LIL----- | - | ------------------ | ---- | LIL----- | Venue | 3 | N | 0 | LIL |
| BKBM------------------------------ | M | ------------------ | ---- | -------- | Gender | 10 | N | 0 | Men |
| BKB-------------------MEET-------- | - | ------------------ | MEET | -------- | Meetings | 19 | N | 0 | Team Managers' Meetings |
| BKB-------------------MEET000100-- | - | ------------------ | MEET | 000100-- | Meetings | 19 | Y | 0 | Team Managers' Meeting |
| BKB-------------------MEET000200-- | - | ------------------ | MEET | 000200-- | Meetings | 19 | Y | 0 | Women's Team Managers' Meeting |
| BKB-------------------MEET000300-- | - | ------------------ | MEET | 000300-- | Meetings | 19 | Y | 0 | Men's Team Managers' Meeting |
| BKBMTEAM5------------------------- | M | TEAM5------------- | ---- | -------- | Event | 13 | N | 0 | Men |
| BKBMTEAM5-------------FNL--------- | M | TEAM5------------- | FNL- | -------- | Phase | 14 | N | 0 | Men's Finals |
| BKBMTEAM5-------------FNL-000100-- | M | TEAM5------------- | FNL- | 000100-- | Unit | 15 | Y | 1 | Men's Gold Medal Game |
| BKBMTEAM5-------------FNL-000200-- | M | TEAM5------------- | FNL- | 000200-- | Unit | 15 | Y | 3 | Men's Bronze Medal Game |
| BKBMTEAM5-------------GP---------- | M | TEAM5------------- | GP-- | -------- | Phase | 14 | N | 0 | Men's Group Phase |
| BKBMTEAM5-------------GPA--------- | M | TEAM5------------- | GPA- | -------- | Phase | 14 | N | 0 | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000100-- | M | TEAM5------------- | GPA- | 000100-- | Unit | 15 | Y | 0 | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000200-- | M | TEAM5------------- | GPA- | 000200-- | Unit | 15 | Y | 0 | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000300-- | M | TEAM5------------- | GPA- | 000300-- | Unit | 15 | Y | 0 | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000400-- | M | TEAM5------------- | GPA- | 000400-- | Unit | 15 | Y | 0 | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000500-- | M | TEAM5------------- | GPA- | 000500-- | Unit | 15 | Y | 0 | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPA-000600-- | M | TEAM5------------- | GPA- | 000600-- | Unit | 15 | Y | 0 | Men's Group Phase - Group A |
| BKBMTEAM5-------------GPB--------- | M | TEAM5------------- | GPB- | -------- | Phase | 14 | N | 0 | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000100-- | M | TEAM5------------- | GPB- | 000100-- | Unit | 15 | Y | 0 | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000200-- | M | TEAM5------------- | GPB- | 000200-- | Unit | 15 | Y | 0 | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000300-- | M | TEAM5------------- | GPB- | 000300-- | Unit | 15 | Y | 0 | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000400-- | M | TEAM5------------- | GPB- | 000400-- | Unit | 15 | Y | 0 | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000500-- | M | TEAM5------------- | GPB- | 000500-- | Unit | 15 | Y | 0 | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPB-000600-- | M | TEAM5------------- | GPB- | 000600-- | Unit | 15 | Y | 0 | Men's Group Phase - Group B |
| BKBMTEAM5-------------GPC--------- | M | TEAM5------------- | GPC- | -------- | Phase | 14 | N | 0 | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000100-- | M | TEAM5------------- | GPC- | 000100-- | Unit | 15 | Y | 0 | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000200-- | M | TEAM5------------- | GPC- | 000200-- | Unit | 15 | Y | 0 | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000300-- | M | TEAM5------------- | GPC- | 000300-- | Unit | 15 | Y | 0 | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000400-- | M | TEAM5------------- | GPC- | 000400-- | Unit | 15 | Y | 0 | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000500-- | M | TEAM5------------- | GPC- | 000500-- | Unit | 15 | Y | 0 | Men's Group Phase - Group C |
| BKBMTEAM5-------------GPC-000600-- | M | TEAM5------------- | GPC- | 000600-- | Unit | 15 | Y | 0 | Men's Group Phase - Group C |
| BKBMTEAM5-------------QFNL-------- | M | TEAM5------------- | QFNL | -------- | Phase | 14 | N | 0 | Men's Quarterfinals |
| BKBMTEAM5-------------QFNL000100-- | M | TEAM5------------- | QFNL | 000100-- | Unit | 15 | Y | 0 | Men's Quarterfinal |
| BKBMTEAM5-------------QFNL000200-- | M | TEAM5------------- | QFNL | 000200-- | Unit | 15 | Y | 0 | Men's Quarterfinal |
| BKBMTEAM5-------------QFNL000300-- | M | TEAM5------------- | QFNL | 000300-- | Unit | 15 | Y | 0 | Men's Quarterfinal |
| BKBMTEAM5-------------QFNL000400-- | M | TEAM5------------- | QFNL | 000400-- | Unit | 15 | Y | 0 | Men's Quarterfinal |
| BKBMTEAM5-------------SFNL-------- | M | TEAM5------------- | SFNL | -------- | Phase | 14 | N | 0 | Men's Semifinals |
| BKBMTEAM5-------------SFNL000100-- | M | TEAM5------------- | SFNL | 000100-- | Unit | 15 | Y | 0 | Men's Semifinal |
| BKBMTEAM5-------------SFNL000200-- | M | TEAM5------------- | SFNL | 000200-- | Unit | 15 | Y | 0 | Men's Semifinal |
| BKBMTEAM5-------------TMRY-------- | M | TEAM5------------- | TMRY | -------- | Phase | 14 | N | 0 | Basketball Session Schedules |
| BKBMTEAM5-------------TMRY000100-- | M | TEAM5------------- | TMRY | 000100-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000200-- | M | TEAM5------------- | TMRY | 000200-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000300-- | M | TEAM5------------- | TMRY | 000300-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000400-- | M | TEAM5------------- | TMRY | 000400-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000500-- | M | TEAM5------------- | TMRY | 000500-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000600-- | M | TEAM5------------- | TMRY | 000600-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000700-- | M | TEAM5------------- | TMRY | 000700-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000800-- | M | TEAM5------------- | TMRY | 000800-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY000900-- | M | TEAM5------------- | TMRY | 000900-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001000-- | M | TEAM5------------- | TMRY | 001000-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001100-- | M | TEAM5------------- | TMRY | 001100-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001200-- | M | TEAM5------------- | TMRY | 001200-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001300-- | M | TEAM5------------- | TMRY | 001300-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001400-- | M | TEAM5------------- | TMRY | 001400-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001500-- | M | TEAM5------------- | TMRY | 001500-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001600-- | M | TEAM5------------- | TMRY | 001600-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001700-- | M | TEAM5------------- | TMRY | 001700-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------TMRY001800-- | M | TEAM5------------- | TMRY | 001800-- | Unit | 15 | Y | 0 | Men's Preliminary Phase |
| BKBMTEAM5-------------VICT-------- | M | TEAM5------------- | VICT | -------- | Phase | 14 | N | 0 | Men's Victory Ceremony |
| BKBMTEAM5-------------VICTMEDAL--- | M | TEAM5------------- | VICT | MEDAL--- | Medals | 17 | Y | 0 | Men's Victory Ceremony |
| BKB-------------------TMRY000100-- | - | ------------------ | TMRY | 000100-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000200-- | - | ------------------ | TMRY | 000200-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000300-- | - | ------------------ | TMRY | 000300-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000400-- | - | ------------------ | TMRY | 000400-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000500-- | - | ------------------ | TMRY | 000500-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000600-- | - | ------------------ | TMRY | 000600-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000700-- | - | ------------------ | TMRY | 000700-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000800-- | - | ------------------ | TMRY | 000800-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY000900-- | - | ------------------ | TMRY | 000900-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY001000-- | - | ------------------ | TMRY | 001000-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKB-------------------TMRY001100-- | - | ------------------ | TMRY | 001100-- | Unit | 15 | Y | 0 | Men's or Women's Preliminary Phase |
| BKBW------------------------------ | W | ------------------ | ---- | -------- | Gender | 10 | N | 0 | Women |
| BKBWTEAM5------------------------- | W | TEAM5------------- | ---- | -------- | Event | 13 | N | 0 | Women |
| BKBWTEAM5-------------FNL--------- | W | TEAM5------------- | FNL- | -------- | Phase | 14 | N | 0 | Women's Finals |
| BKBWTEAM5-------------FNL-000100-- | W | TEAM5------------- | FNL- | 000100-- | Unit | 15 | Y | 1 | Women's Gold Medal Game |
| BKBWTEAM5-------------FNL-000200-- | W | TEAM5------------- | FNL- | 000200-- | Unit | 15 | Y | 3 | Women's Bronze Medal Game |
| BKBWTEAM5-------------GP---------- | W | TEAM5------------- | GP-- | -------- | Phase | 14 | N | 0 | Women's Group Phase |
| BKBWTEAM5-------------GPA--------- | W | TEAM5------------- | GPA- | -------- | Phase | 14 | N | 0 | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000100-- | W | TEAM5------------- | GPA- | 000100-- | Unit | 15 | Y | 0 | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000200-- | W | TEAM5------------- | GPA- | 000200-- | Unit | 15 | Y | 0 | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000300-- | W | TEAM5------------- | GPA- | 000300-- | Unit | 15 | Y | 0 | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000400-- | W | TEAM5------------- | GPA- | 000400-- | Unit | 15 | Y | 0 | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000500-- | W | TEAM5------------- | GPA- | 000500-- | Unit | 15 | Y | 0 | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPA-000600-- | W | TEAM5------------- | GPA- | 000600-- | Unit | 15 | Y | 0 | Women's Group Phase - Group A |
| BKBWTEAM5-------------GPB--------- | W | TEAM5------------- | GPB- | -------- | Phase | 14 | N | 0 | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000100-- | W | TEAM5------------- | GPB- | 000100-- | Unit | 15 | Y | 0 | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000200-- | W | TEAM5------------- | GPB- | 000200-- | Unit | 15 | Y | 0 | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000300-- | W | TEAM5------------- | GPB- | 000300-- | Unit | 15 | Y | 0 | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000400-- | W | TEAM5------------- | GPB- | 000400-- | Unit | 15 | Y | 0 | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000500-- | W | TEAM5------------- | GPB- | 000500-- | Unit | 15 | Y | 0 | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPB-000600-- | W | TEAM5------------- | GPB- | 000600-- | Unit | 15 | Y | 0 | Women's Group Phase - Group B |
| BKBWTEAM5-------------GPC--------- | W | TEAM5------------- | GPC- | -------- | Phase | 14 | N | 0 | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000100-- | W | TEAM5------------- | GPC- | 000100-- | Unit | 15 | Y | 0 | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000200-- | W | TEAM5------------- | GPC- | 000200-- | Unit | 15 | Y | 0 | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000300-- | W | TEAM5------------- | GPC- | 000300-- | Unit | 15 | Y | 0 | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000400-- | W | TEAM5------------- | GPC- | 000400-- | Unit | 15 | Y | 0 | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000500-- | W | TEAM5------------- | GPC- | 000500-- | Unit | 15 | Y | 0 | Women's Group Phase - Group C |
| BKBWTEAM5-------------GPC-000600-- | W | TEAM5------------- | GPC- | 000600-- | Unit | 15 | Y | 0 | Women's Group Phase - Group C |
| BKBWTEAM5-------------QFNL-------- | W | TEAM5------------- | QFNL | -------- | Phase | 14 | N | 0 | Women's Quarterfinals |
| BKBWTEAM5-------------QFNL000100-- | W | TEAM5------------- | QFNL | 000100-- | Unit | 15 | Y | 0 | Women's Quarterfinal |
| BKBWTEAM5-------------QFNL000200-- | W | TEAM5------------- | QFNL | 000200-- | Unit | 15 | Y | 0 | Women's Quarterfinal |
| BKBWTEAM5-------------QFNL000300-- | W | TEAM5------------- | QFNL | 000300-- | Unit | 15 | Y | 0 | Women's Quarterfinal |
| BKBWTEAM5-------------QFNL000400-- | W | TEAM5------------- | QFNL | 000400-- | Unit | 15 | Y | 0 | Women's Quarterfinal |
| BKBWTEAM5-------------SFNL-------- | W | TEAM5------------- | SFNL | -------- | Phase | 14 | N | 0 | Women's Semifinals |
| BKBWTEAM5-------------SFNL000100-- | W | TEAM5------------- | SFNL | 000100-- | Unit | 15 | Y | 0 | Women's Semifinal |
| BKBWTEAM5-------------SFNL000200-- | W | TEAM5------------- | SFNL | 000200-- | Unit | 15 | Y | 0 | Women's Semifinal |
| BKBWTEAM5-------------TMRY-------- | W | TEAM5------------- | TMRY | -------- | Phase | 14 | N | 0 | Basketball Session Schedules |
| BKBWTEAM5-------------TMRY000100-- | W | TEAM5------------- | TMRY | 000100-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000200-- | W | TEAM5------------- | TMRY | 000200-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000300-- | W | TEAM5------------- | TMRY | 000300-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000400-- | W | TEAM5------------- | TMRY | 000400-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000500-- | W | TEAM5------------- | TMRY | 000500-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000600-- | W | TEAM5------------- | TMRY | 000600-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000700-- | W | TEAM5------------- | TMRY | 000700-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000800-- | W | TEAM5------------- | TMRY | 000800-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY000900-- | W | TEAM5------------- | TMRY | 000900-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001000-- | W | TEAM5------------- | TMRY | 001000-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001100-- | W | TEAM5------------- | TMRY | 001100-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001200-- | W | TEAM5------------- | TMRY | 001200-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001300-- | W | TEAM5------------- | TMRY | 001300-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001400-- | W | TEAM5------------- | TMRY | 001400-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001500-- | W | TEAM5------------- | TMRY | 001500-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001600-- | W | TEAM5------------- | TMRY | 001600-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001700-- | W | TEAM5------------- | TMRY | 001700-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------TMRY001800-- | W | TEAM5------------- | TMRY | 001800-- | Unit | 15 | Y | 0 | Women's Preliminary Phase |
| BKBWTEAM5-------------VICT-------- | W | TEAM5------------- | VICT | -------- | Phase | 14 | N | 0 | Women's Victory Ceremony |
| BKBWTEAM5-------------VICTMEDAL--- | W | TEAM5------------- | VICT | MEDAL--- | Medals | 17 | Y | 0 | Women's Victory Ceremony |

### CC @Organisation

`CC @Organisation` is a large common master-data table rather than a current-message enumeration. The downloaded Paris 2024 code page contains 259 `Organisation` rows. Use the source link in the index above as the authoritative value list when modeling `Organisation` fields.

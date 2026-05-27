
# ODF FBL Data Dictionary: DT_CURRENT, Pages 49-51

Source: `C:\Users\mella\Downloads\ODF_FBL_Data_Dictionary.pdf`, pages 49-51.

This note restructures the football `DT_CURRENT` section into readable Markdown for domain modeling. It covers the current information message, trigger rules, clock payload, current score payload, XSD-aligned XML example, and an appendix of English SC/CC values downloaded from the Paris 2024 ODF code pages.

## 2.3.5 Current Information

`DT_CURRENT` contains current information for a live competition. In football it carries the latest applicable match state and, for a team sport with a running clock, the clock.

Compared with `DT_RESULT`, this is a compact live-state message: current period/clock plus current score rows for the competitors.

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
| Start and end of every period | Used to start/stop the clock. |
| Score changes | Send immediately after every score change, including penalty shots. |
| Clock starts/stops | Send every time the clock starts and stops. |
| Periodic heartbeat during play | During play, after start and outside breaks, send every 5 minutes after the previous `DT_CURRENT` when no other activity has triggered the message. |

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
| `Gen` | Optional in dictionary, required by XSD | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional in dictionary, required by XSD | `S(20)` | Code-set version applicable to the message. |

## Clock

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Period` | Optional | `SC @Period` | Current period, if available automatically from the timing device. |
| `Time` | Mandatory | `mmm:ss` | Clock value. |
| `Running` | Mandatory | `S(1)` | `Y` when the clock is running, `N` when stopped. |

```xml
<Clock Period="H2" Time="1:34" Running="Y"/>
```

## Result

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Result` | Optional | Numeric `#0` | Current score for the team. Penalty shoot-out goals are not included. |
| `SortOrder` | Mandatory | Numeric | Sequential order: first/home named team is `1`, away team is `2`. |
| `StartSortOrder` | Mandatory | Numeric | Same value as `SortOrder`. |
| `ResultType` | Optional | `SC @ResultType` | Type of `Result`, either goals/points or IRM-with-points for the event unit. |

## Result / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Organisation` | Mandatory | `CC @Organisation` | Competitor organisation. |

## Message Sort

The PDF says: sort by `Period @Code`. In this message, period is represented as `Clock/@Period`; result rows should still preserve `Result/@SortOrder` for home/away ordering.

## XSD-Aligned XML Example

The provided XSD draft could not be loaded directly because `odf2-structure.xsd` references `RecordBrokenType`, which is not defined in the supplied schema folder. The example below was validated against a temporary copy of the schema where only that unrelated unresolved type reference was replaced so the schema could parse. No original XSD files were modified.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="FBLWTEAM11------------GPA-000100--"
         DocumentType="DT_CURRENT"
         Version="8"
         FeedFlag="T"
         Date="2026-05-25"
         Time="18:42:00.000"
         LogicalDate="2026-05-25"
         Source="OVR">
  <Competition Gen="3.4" Sport="FBL-3.4" Codes="SOG-2024">
    <Clock Period="H2" Time="1:34" Running="Y"/>
    <Result Result="2" SortOrder="1" StartSortOrder="1" ResultType="POINTS">
      <Competitor Code="FBLWTEAM11-----RSA01" Type="T" Organisation="RSA"/>
    </Result>
    <Result Result="1" SortOrder="2" StartSortOrder="2" ResultType="POINTS">
      <Competitor Code="FBLWTEAM11-----BRA01" Type="T" Organisation="BRA"/>
    </Result>
  </Competition>
</OdfBody>
```

## Modeling Notes

- `DT_CURRENT` should map to a live scoreboard projection, not to the authoritative match record. The authoritative full result still lives in `DT_RESULT`.
- Keep clock state separate from result rows. Clock transitions can trigger messages even without score changes.
- Do not include penalty shoot-out goals in `Result/@Result`; use the relevant result/stat/play-by-play messages for shoot-out detail.
- `Running` is a real state bit and should not be inferred solely from period labels or clock value.
- Result ordering is explicit. Preserve `SortOrder`/`StartSortOrder` rather than deriving home/away order from array position.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-25. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index
| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Source` | FBL | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm) |
| `SC @Period` | FBL | 12 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_FBL.htm) |
| `SC @ResultType` | FBL | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_FBL.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @Unit` | FBL EventUnit rows | 93 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @Organisation` | Common master data | 258 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |

### SC @Source
| Code | ENG Description |
| --- | --- |
| BORFBL1 | Origin for messages from OVR at BOR for FBL |
| LYOFBL1 | Origin for messages from OVR at LYO for FBL |
| MRSFBL1 | Origin for messages from OVR at MRS for FBL |
| NANFBL1 | Origin for messages from OVR at NAN for FBL |
| NICFBL1 | Origin for messages from OVR at NIC for FBL |
| PDPFBL1 | Origin for messages from OVR at PDP for FBL |
| STEFBL1 | Origin for messages from OVR at STE for FBL |

### SC @Period
| Code | Order | ENG Description |
| --- | --- | --- |
| H1 | 1 | First half |
| HT | 2 | Half-time |
| H2 | 3 | Second half |
| FT | 4 | Full time |
| RT | 5 | Regular Time |
| PET | 6 | Pre extra time |
| ET-H1 | 7 | First half of extra time |
| ET-HT | 8 | Half-time of extra time |
| ET-H2 | 9 | Second half of extra time |
| PSO | 10 | Penalty shoot-out |
| TOT | 11 | Total |
| E-RT |  | Regular Time |

### SC @ResultType
| Code | ENG Description |
| --- | --- |
| IRM_POINTS | For both, Points and Invalid Result Mark |
| POINTS | Goals |

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

### CC @Unit (FBL EventUnit rows)
| Code | Gender | Event | phase | Eventunit | Level | Order | schedule | medalflag | ENG Description |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| FBL------------------------------- | - | ------------------ | ---- | -------- | Discipline | 2 | N | 0 | Football |
| FBL-----------------------BOR----- | - | ------------------ | ---- | BOR----- | Venue | 3 | N | 0 | BOR |
| FBL-----------------------LYO----- | - | ------------------ | ---- | LYO----- | Venue | 3 | N | 0 | LYO |
| FBLM------------------------------ | M | ------------------ | ---- | -------- | Gender | 10 | N | 0 | Men |
| FBL-----------------------MRS----- | - | ------------------ | ---- | MRS----- | Venue | 3 | N | 0 | MRS |
| FBLMTEAM11------------------------ | M | TEAM11------------ | ---- | -------- | Event | 13 | N | 0 | Men |
| FBLMTEAM11------------FNL--------- | M | TEAM11------------ | FNL- | -------- | Phase | 14 | N | 0 | Men's Finals |
| FBLMTEAM11------------FNL-000100-- | M | TEAM11------------ | FNL- | 000100-- | Unit | 15 | Y | 1 | Men's Gold Medal Match |
| FBLMTEAM11------------FNL-000200-- | M | TEAM11------------ | FNL- | 000200-- | Unit | 15 | Y | 3 | Men's Bronze Medal Match |
| FBLMTEAM11------------GP---------- | M | TEAM11------------ | GP-- | -------- | Phase | 14 | N | 0 | Men's Group Stage |
| FBLMTEAM11------------GPA--------- | M | TEAM11------------ | GPA- | -------- | Phase | 14 | N | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000100-- | M | TEAM11------------ | GPA- | 000100-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000200-- | M | TEAM11------------ | GPA- | 000200-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000300-- | M | TEAM11------------ | GPA- | 000300-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000400-- | M | TEAM11------------ | GPA- | 000400-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000500-- | M | TEAM11------------ | GPA- | 000500-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPA-000600-- | M | TEAM11------------ | GPA- | 000600-- | Unit | 15 | Y | 0 | Men's Group A |
| FBLMTEAM11------------GPB--------- | M | TEAM11------------ | GPB- | -------- | Phase | 14 | N | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000100-- | M | TEAM11------------ | GPB- | 000100-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000200-- | M | TEAM11------------ | GPB- | 000200-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000300-- | M | TEAM11------------ | GPB- | 000300-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000400-- | M | TEAM11------------ | GPB- | 000400-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000500-- | M | TEAM11------------ | GPB- | 000500-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPB-000600-- | M | TEAM11------------ | GPB- | 000600-- | Unit | 15 | Y | 0 | Men's Group B |
| FBLMTEAM11------------GPC--------- | M | TEAM11------------ | GPC- | -------- | Phase | 14 | N | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000100-- | M | TEAM11------------ | GPC- | 000100-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000200-- | M | TEAM11------------ | GPC- | 000200-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000300-- | M | TEAM11------------ | GPC- | 000300-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000400-- | M | TEAM11------------ | GPC- | 000400-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000500-- | M | TEAM11------------ | GPC- | 000500-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPC-000600-- | M | TEAM11------------ | GPC- | 000600-- | Unit | 15 | Y | 0 | Men's Group C |
| FBLMTEAM11------------GPD--------- | M | TEAM11------------ | GPD- | -------- | Phase | 14 | N | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000100-- | M | TEAM11------------ | GPD- | 000100-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000200-- | M | TEAM11------------ | GPD- | 000200-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000300-- | M | TEAM11------------ | GPD- | 000300-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000400-- | M | TEAM11------------ | GPD- | 000400-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000500-- | M | TEAM11------------ | GPD- | 000500-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------GPD-000600-- | M | TEAM11------------ | GPD- | 000600-- | Unit | 15 | Y | 0 | Men's Group D |
| FBLMTEAM11------------QFNL-------- | M | TEAM11------------ | QFNL | -------- | Phase | 14 | N | 0 | Men's Quarter-finals |
| FBLMTEAM11------------QFNL000100-- | M | TEAM11------------ | QFNL | 000100-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000200-- | M | TEAM11------------ | QFNL | 000200-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000300-- | M | TEAM11------------ | QFNL | 000300-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------QFNL000400-- | M | TEAM11------------ | QFNL | 000400-- | Unit | 15 | Y | 0 | Men's Quarter-final |
| FBLMTEAM11------------SFNL-------- | M | TEAM11------------ | SFNL | -------- | Phase | 14 | N | 0 | Men's Semi-finals |
| FBLMTEAM11------------SFNL000100-- | M | TEAM11------------ | SFNL | 000100-- | Unit | 15 | Y | 0 | Men's Semi-final |
| FBLMTEAM11------------SFNL000200-- | M | TEAM11------------ | SFNL | 000200-- | Unit | 15 | Y | 0 | Men's Semi-final |
| FBLMTEAM11------------VICT-------- | M | TEAM11------------ | VICT | -------- | Phase | 14 | N | 0 | Men's Victory Ceremony |
| FBLMTEAM11------------VICTBRONZE-- | M | TEAM11------------ | VICT | BRONZE-- | Medals | 17 | Y | 0 | Men's Bronze Ceremony |
| FBLMTEAM11------------VICTMEDAL--- | M | TEAM11------------ | VICT | MEDAL--- | Medals | 17 | Y | 0 | Men's Victory Ceremony |
| FBL-----------------------NAN----- | - | ------------------ | ---- | NAN----- | Venue | 3 | N | 0 | NAN |
| FBL-----------------------NIC----- | - | ------------------ | ---- | NIC----- | Venue | 3 | N | 0 | NIC |
| FBL-----------------------PDP----- | - | ------------------ | ---- | PDP----- | Venue | 3 | N | 0 | PDP |
| FBL-----------------------STE----- | - | ------------------ | ---- | STE----- | Venue | 3 | N | 0 | STE |
| FBL-----------------------STR----- | - | ------------------ | ---- | STR----- | Venue | 3 | N | 0 | STR |
| FBL-----------------------TOU----- | - | ------------------ | ---- | TOU----- | Venue | 3 | N | 0 | TOU |
| FBLW------------------------------ | W | ------------------ | ---- | -------- | Gender | 10 | N | 0 | Women |
| FBLWTEAM11------------------------ | W | TEAM11------------ | ---- | -------- | Event | 13 | N | 0 | Women |
| FBLWTEAM11------------FNL--------- | W | TEAM11------------ | FNL- | -------- | Phase | 14 | N | 0 | Women's Finals |
| FBLWTEAM11------------FNL-000100-- | W | TEAM11------------ | FNL- | 000100-- | Unit | 15 | Y | 1 | Women's Gold Medal Match |
| FBLWTEAM11------------FNL-000200-- | W | TEAM11------------ | FNL- | 000200-- | Unit | 15 | Y | 3 | Women's Bronze Medal Match |
| FBLWTEAM11------------GP---------- | W | TEAM11------------ | GP-- | -------- | Phase | 14 | N | 0 | Women's Group Stage |
| FBLWTEAM11------------GPA--------- | W | TEAM11------------ | GPA- | -------- | Phase | 14 | N | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000100-- | W | TEAM11------------ | GPA- | 000100-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000200-- | W | TEAM11------------ | GPA- | 000200-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000300-- | W | TEAM11------------ | GPA- | 000300-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000400-- | W | TEAM11------------ | GPA- | 000400-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000500-- | W | TEAM11------------ | GPA- | 000500-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPA-000600-- | W | TEAM11------------ | GPA- | 000600-- | Unit | 15 | Y | 0 | Women's Group A |
| FBLWTEAM11------------GPB--------- | W | TEAM11------------ | GPB- | -------- | Phase | 14 | N | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000100-- | W | TEAM11------------ | GPB- | 000100-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000200-- | W | TEAM11------------ | GPB- | 000200-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000300-- | W | TEAM11------------ | GPB- | 000300-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000400-- | W | TEAM11------------ | GPB- | 000400-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000500-- | W | TEAM11------------ | GPB- | 000500-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPB-000600-- | W | TEAM11------------ | GPB- | 000600-- | Unit | 15 | Y | 0 | Women's Group B |
| FBLWTEAM11------------GPC--------- | W | TEAM11------------ | GPC- | -------- | Phase | 14 | N | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000100-- | W | TEAM11------------ | GPC- | 000100-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000200-- | W | TEAM11------------ | GPC- | 000200-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000300-- | W | TEAM11------------ | GPC- | 000300-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000400-- | W | TEAM11------------ | GPC- | 000400-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000500-- | W | TEAM11------------ | GPC- | 000500-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------GPC-000600-- | W | TEAM11------------ | GPC- | 000600-- | Unit | 15 | Y | 0 | Women's Group C |
| FBLWTEAM11------------QFNL-------- | W | TEAM11------------ | QFNL | -------- | Phase | 14 | N | 0 | Women's Quarter-finals |
| FBLWTEAM11------------QFNL000100-- | W | TEAM11------------ | QFNL | 000100-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000200-- | W | TEAM11------------ | QFNL | 000200-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000300-- | W | TEAM11------------ | QFNL | 000300-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------QFNL000400-- | W | TEAM11------------ | QFNL | 000400-- | Unit | 15 | Y | 0 | Women's Quarter-final |
| FBLWTEAM11------------SFNL-------- | W | TEAM11------------ | SFNL | -------- | Phase | 14 | N | 0 | Women's Semi-finals |
| FBLWTEAM11------------SFNL000100-- | W | TEAM11------------ | SFNL | 000100-- | Unit | 15 | Y | 0 | Women's Semi-final |
| FBLWTEAM11------------SFNL000200-- | W | TEAM11------------ | SFNL | 000200-- | Unit | 15 | Y | 0 | Women's Semi-final |
| FBLWTEAM11------------VICT-------- | W | TEAM11------------ | VICT | -------- | Phase | 14 | N | 0 | Women's Victory Ceremony |
| FBLWTEAM11------------VICTBRONZE-- | W | TEAM11------------ | VICT | BRONZE-- | Medals | 17 | Y | 0 | Women's Bronze Ceremony |
| FBLWTEAM11------------VICTMEDAL--- | W | TEAM11------------ | VICT | MEDAL--- | Medals | 17 | Y | 0 | Women's Victory Ceremony |

### CC @Organisation

`CC @Organisation` is a large common master-data table rather than a current-message enumeration. The downloaded Paris 2024 code page contains 258 `Organisation` rows. Use the source link in the index above as the authoritative value list when modeling `Organisation` fields.

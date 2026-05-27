# ODF BOX Data Dictionary: DT_RESULT, Pages 27-37

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BOX_Data_Dictionary.pdf`, pages 27-37.

Source version: `SCOG/SYOG-2026-BOX-1.2 SFR`, dated 13 May 2026.

This note restructures the Boxing `DT_RESULT` section into a practical domain reference for the Event Unit Start List
and Results message. It covers the unit-level start list, live bout state, official result, judge scoring, warnings,
knockdowns, competitor identity, and athlete entry data.

## 2.3.4 Event Unit Start List and Results

`DT_RESULT` contains both start-list and result information for the competitors in one Boxing event unit. The message is
mandatory and is always a full message: when sent, all applicable elements and attributes are sent, not only the changed
data.

For Boxing, the unit result is modeled around two athlete competitors:

- `SortOrder="1"` / `StartSortOrder="1"` is RED, also treated as home.
- `SortOrder="2"` / `StartSortOrder="2"` is BLUE, also treated as away.
- `Result/@Result` is the competitor's points only when `ResultType` is `POINTS` or `RM_POINTS`.
- Result metadata such as decision code, stopped round, and stopped time is carried in `ExtendedInfos/ExtendedInfo`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@EVENT_UNIT` | Event Unit RSC. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_RESULT` | Event Unit Start List and Results message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `CC@RESULTSTATUS` | Result lifecycle status. The section lists `START_LIST`, `LIVE`, `OFFICIAL`, and `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | `P` production, `T` test. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| `ResultStatus` | When to send |
|---|---|
| `START_LIST` | As soon as each competitor is known. |
| `START_LIST` | After any start-list data change. |
| `LIVE` | As soon as the unit starts. |
| `LIVE` | On all live updates during the bout, including round/break state and scoring data when available. |
| `OFFICIAL` | When the results become official. |
| `PROVISIONAL` | Listed as an allowed header status, though the section does not provide a separate trigger rule. |
| Any applicable status | Trigger again after any change. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- UnitDateTime (0,1)
    |   |   +-- @StartDate
    |   |   +-- @EndDate
    |   +-- ExtendedInfo (0,N)
    |   |   +-- @Type
    |   |   +-- @Code
    |   |   +-- @Pos
    |   |   +-- @Value
    |   +-- SportDescription (0,1)
    |   |   +-- @DisciplineName
    |   |   +-- @EventName
    |   |   +-- @Gender
    |   |   +-- @SubEventName
    |   |   +-- @UnitNum
    |   +-- VenueDescription (0,1)
    |       +-- @Venue
    |       +-- @VenueName
    |       +-- @Location
    |       +-- @LocationName
    +-- Officials (0,1)
    |   +-- Official (1,N)
    |       +-- @Code
    |       +-- @Function
    |       +-- @Order
    |       +-- Description (1,1)
    |           +-- @GivenName
    |           +-- @FamilyName
    |           +-- @Gender
    |           +-- @Organisation
    |           +-- @IFId
    +-- Periods (0,1)
    |   +-- @Home
    |   +-- @Away
    |   +-- Period (1,N)
    |       +-- @Code
    |       +-- ExtendedPeriods (0,1)
    |           +-- ExtendedPeriod (1,N)
    |               +-- @Type
    |               +-- @Code
    |               +-- @Pos
    |               +-- @Value
    +-- Result (1,N)
        +-- @Result
        +-- @WLT
        +-- @SortOrder
        +-- @StartSortOrder
        +-- @ResultType
        +-- ExtendedResults (0,1)
        |   +-- ExtendedResult (1,N)
        |       +-- @Type
        |       +-- @Code
        |       +-- @Pos
        |       +-- @Value
        |       +-- @Value2
        |       +-- @Rank
        +-- Competitor (1,1)
            +-- @Code
            +-- @Type
            +-- @Organisation
            +-- Composition (0,1)
                +-- Athlete (0,N)
                    +-- @Code
                    +-- @Order
                    +-- Description (1,1)
                    |   +-- @GivenName
                    |   +-- @FamilyName
                    |   +-- @Gender
                    |   +-- @Organisation
                    |   +-- @BirthDate
                    |   +-- @IFId
                    +-- EventUnitEntry (0,N)
                        +-- @Type
                        +-- @Code
                        +-- @Pos
                        +-- @Value
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | Mandatory in XSD | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | O | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Mandatory in XSD | `S(20)` | Codes version applicable to the message. |

### `Competition / ExtendedInfos / UnitDateTime`

Actual unit times. Include when the unit starts.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `StartDate` | M | DateTime | Actual start date and time. |
| `EndDate` | O | DateTime | Actual end date-time; omit if not available. |

### `Competition / ExtendedInfos / ExtendedInfo`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `RES_CODE` | N/A | When available. | `SC@ResultCode` | Bout result mark, such as the decision or no-contest code. |
| `UI` | `PERIOD` | N/A | During the bout, including breaks. | `SC@Period` or `SC@GameState` | Current round, most recently finished round during a break, or applicable game state. |
| `UI` | `ROUND` | N/A | When the result is official and `ResultType` is `RM` or `RM_POINTS`. | `SC@Period` | Round in which the bout stopped according to the winning decision. |
| `UI` | `TIME` | N/A | When the result is official and `ResultType` is `RM` or `RM_POINTS`. | `m:ss` | Stop time, ascending from `0:00`, according to the winning decision. |

### `Competition / ExtendedInfos / SportDescription`

Text descriptions for the sport context.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE` English description | Discipline name, not the code. |
| `EventName` | M | `CC@EVENT` English description | Event name, not the code. |
| `Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the event unit. |
| `SubEventName` | M | `CC@EVENT_UNIT` English short description | Event-unit description, not the code. |
| `UnitNum` | O | `S(15)` | Bout number. |

### `Competition / ExtendedInfos / VenueDescription`

Venue and location names in text.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Venue` | M | `CC@VENUE` ID | Venue code. |
| `VenueName` | M | `CC@VENUE` English description | Venue name, not the code. |
| `Location` | M | `CC@LOCATION` ID | Location code. |
| `LocationName` | M | `CC@LOCATION` English description | Location name, not the code. |

### `Competition / Officials / Official`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Official code. |
| `Function` | M | `CC@DISCIPLINE_FUNCTION` ID | Official function, such as referee or judge number. May differ from the function in `DT_PARTIC`. |
| `Order` | M | Positive integer | Official order. Referee first, then judges by judge number. |

### `Competition / Officials / Official / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `GivenName` | O | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | M | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | M | `CC@PERSON_GENDER` ID | Official gender. |
| `Organisation` | M | `CC@ORGANISATION` ID | Official organisation. |
| `IFId` | O | `S(16)` | International Federation ID. |

### `Competition / Periods`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Home` | M | `S(20)` with no leading zeroes | Home competitor ID, corresponding to RED. |
| `Away` | M | `S(20)` with no leading zeroes | Away competitor ID, corresponding to BLUE. |

### `Competition / Periods / Period`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `SC@Period` | Round number. |

### `Competition / Periods / Period / ExtendedPeriods / ExtendedPeriod`

Round scoring by judge.

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EP` | `SCR_H` | `J1`-`J5` | After each round, or if the bout is official. | `#0` | Judge score for the red/home competitor in this round. |
| `EP` | `SCR_A` | `J1`-`J5` | After each round, or if the bout is official. | `#0` | Judge score for the blue/away competitor in this round. |

### `Competition / Result`

At least one `Result` is required for each event-unit result message.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Result` | O | `#0` | Competitor points, sent only for `ResultType="POINTS"` or `ResultType="RM_POINTS"`; valid range in this section is `0`-`5`. |
| `WLT` | O | `SC@WLT` | Whether the competitor won (`W`) or lost (`L`) the bout. Send `L` in no-winner outcomes such as `DKO` or `BDSQ`. |
| `SortOrder` | M | Positive integer | Result order: `1` for RED/home, `2` for BLUE/away. |
| `StartSortOrder` | M | Positive integer | Start-list order: `1` for RED, `2` for BLUE. |
| `ResultType` | O | `SC@ResultType` | Type of `Result`; the section lists `RM_POINTS`, `POINTS`, and `RM` through examples and explanatory text. |

Result interpretation:

| `ResultType` | Expected Result Shape |
|---|---|
| `RM_POINTS` | Result mark plus points plus round/time decision, for example `WP 3:0`, stopped at `R2 01:09`. |
| `POINTS` | Points decision without stopped round/time, for example `WP` decision with `3:0`. |
| `RM` | Result mark only, often with stopped round/time, for example `DSQ R2 01:09` or no-contest style outcomes. |

### `Competition / Result / ExtendedResults / ExtendedResult`

Per-competitor judge totals, warnings, and knockdowns.

| Type | Code | Pos | Expected When | Value | Other Attributes | Meaning |
|---|---|---|---|---|---|---|
| `ER` | `JUDGE` | `J1`-`J5` | When available. | `#0` | `Value2="Y"` optional; `Rank="#0"` optional. | Total judge points for the competitor from the judge in `@Pos`. `Value2="Y"` marks the preferred competitor from that judge; `Rank="1"` marks the athlete adjudged best by that judge between the competitors. |
| `ER` | `WARNING` | `SC@Period` or `TOT` | Always, if available. | `#0` | N/A | Warning count in the round, or total warnings when `Pos="TOT"`. |
| `ER` | `KD` | `SC@Period` or `TOT` | Always, if available. | `#0` | N/A | Knockdown/count count in the round, or total counts when `Pos="TOT"`. |

### `Competition / Result / Competitor`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes, `TBD`, or `NOCOMP` | Competitor ID; `TBD` when the competitor is unknown but expected later; `NOCOMP` when there is no competitor and one will not come later. |
| `Type` | M | `A` | Athlete competitor. |
| `Organisation` | O | `CC@ORGANISATION` ID | Competitor organisation. |

### `Competition / Result / Competitor / Composition / Athlete`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Athlete ID. |
| `Order` | M | Positive integer | Athlete order. Send `1` when `Competitor/@Type="A"`. |

### `Competition / Result / Competitor / Composition / Athlete / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `GivenName` | O | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | M | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | M | `CC@PERSON_GENDER` ID | Athlete gender. |
| `Organisation` | M | `CC@ORGANISATION` ID | Athlete organisation. |
| `BirthDate` | O | `YYYY-MM-DD` | Birth date; include if available. |
| `IFId` | O | `S(16)` | International Federation ID. |

### `Competition / Result / Competitor / Composition / Athlete / EventUnitEntry`

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `EUE` | `DETAILED` | N/A | When `Competitor/@Code="NOCOMP"`. | Text | Text to place instead of the competitor name, for example `BDSQ from bout no. 11`. |
| `EUE` | `COLOUR` | N/A | Always, as soon as known. | `SC@Colour` | Athlete colour. |
| `EUE` | `SEED` | N/A | Only for seeded athletes. | `#0` | Seed number. |

## Samples from the Dictionary, Normalized

### Official decision with result mark, points, round, and time

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <UnitDateTime StartDate="2012-08-09T13:07:00+01:00" EndDate="2012-08-09T13:13:00+01:00"/>
    <ExtendedInfo Type="UI" Code="RES_CODE" Value="WP"/>
    <ExtendedInfo Type="UI" Code="ROUND" Value="R2"/>
    <ExtendedInfo Type="UI" Code="TIME" Value="01:09"/>
  </ExtendedInfos>
  <Result ResultType="RM_POINTS" Result="3" WLT="W" SortOrder="1" StartSortOrder="1">
    <Competitor Code="1072766" Type="A" Organisation="NZL">
      <Composition>
        <Athlete Code="1072766" Order="1">
          <Description FamilyName="Smith" GivenName="John" Gender="M" Organisation="NZL" BirthDate="1995-12-15"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
  <Result ResultType="RM_POINTS" Result="0" WLT="L" SortOrder="2" StartSortOrder="2">
    <Competitor Code="1072750" Type="A" Organisation="GBR">
      <Composition>
        <Athlete Code="1072750" Order="1">
          <Description FamilyName="Kettle" GivenName="George" Gender="M" Organisation="GBR" BirthDate="1995-12-15"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

### Round scoring and competitor detail

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <UnitDateTime StartDate="2012-08-09T13:07:00+01:00" EndDate="2012-08-09T13:13:00+01:00"/>
    <ExtendedInfo Type="UI" Code="PERIOD" Value="R2"/>
  </ExtendedInfos>
  <Periods Home="1072766" Away="1072750">
    <Period Code="R1">
      <ExtendedPeriods>
        <ExtendedPeriod Type="EP" Code="SCR_H" Pos="J1" Value="10"/>
        <ExtendedPeriod Type="EP" Code="SCR_H" Pos="J2" Value="9"/>
        <ExtendedPeriod Type="EP" Code="SCR_A" Pos="J1" Value="10"/>
        <ExtendedPeriod Type="EP" Code="SCR_A" Pos="J2" Value="9"/>
      </ExtendedPeriods>
    </Period>
  </Periods>
  <Result SortOrder="1" StartSortOrder="1">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="JUDGE" Pos="J1" Value="27"/>
      <ExtendedResult Type="ER" Code="WARNING" Pos="R1" Value="2"/>
      <ExtendedResult Type="ER" Code="WARNING" Pos="TOT" Value="3"/>
      <ExtendedResult Type="ER" Code="KD" Pos="R1" Value="1"/>
      <ExtendedResult Type="ER" Code="KD" Pos="TOT" Value="2"/>
    </ExtendedResults>
    <Competitor Code="1072766" Type="A" Organisation="NZL">
      <Composition>
        <Athlete Code="1072766" Order="1">
          <Description FamilyName="Smith" GivenName="John" Gender="M" Organisation="NZL" BirthDate="1995-12-15"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

### No competitor placeholder

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-BOX-1.2 SFR" Codes="SYOG-2026">
  <Result SortOrder="1" StartSortOrder="1">
    <Competitor Code="NOCOMP" Type="A">
      <Composition>
        <Athlete Code="NOCOMP" Order="1">
          <EventUnitEntry Type="EUE" Code="DETAILED" Value="BDSQ from bout no. 11"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

## Message Sort

Sort by `Result/@SortOrder`.

For Boxing, this means:

| Sort Order | Corner | Role in Other Fields |
|---:|---|---|
| `1` | RED | Home competitor in `Periods/@Home`; red/home score uses `ExtendedPeriod Code="SCR_H"`. |
| `2` | BLUE | Away competitor in `Periods/@Away`; blue/away score uses `ExtendedPeriod Code="SCR_A"`. |

## Modeling Notes

- Treat `DT_RESULT` as the authoritative full snapshot for one bout/unit. Replacing prior unit state from the latest
  version is safer than patch-merging sparse fragments.
- Preserve the RED/BLUE ordering as a domain concept, not just display order. It drives `SortOrder`, `StartSortOrder`,
  `Periods/@Home` and `Periods/@Away`, round scoring codes, and athlete `EUE COLOUR`.
- `ExtendedInfos/UI` holds the bout-level decision state. `Result` holds per-competitor result values, and
  `ExtendedResults/ER` holds per-competitor judge totals, warnings, and knockdowns.
- `Result/@WLT` is optional, but no-winner cases still send `L` for affected competitors according to the section.
- Store round-level judge scores separately from total judge scores: `ExtendedPeriods/EP` is per round; `ExtendedResults`
  `JUDGE` is the total for the competitor by judge.
- The PDF prose writes `RM_Points` and `Points` in places, while samples use uppercase `RM_POINTS` and `POINTS`.
  Normalize code comparisons case-sensitively against the actual code catalog when available, and keep raw values in
  ingestion logs.
- The sample snippets in the PDF are illustrative and contain omissions/ellipsis; the normalized examples above were
  XSD-validated as `Competition` fragments wrapped in an `OdfBody` envelope.

## XSD Validation

The normalized XML examples above were validated against a temporary copy of `odf2-schema-30112025-DRAFT`. The temporary
schema copy only patched the known unresolved `RecordBrokenType` reference to the existing `extRecordBrokenType`; the
source XSD files were not modified.

## Code Appendix: Values Directly Visible in Pages 27-37

The section links several sport-code/common-code pages, but the linked `odf.olympictech.org` HTML pages did not return
within extraction timeouts. The table below therefore records only values directly visible in the `DT_RESULT` pages, not
complete authoritative code tables.

| Code Entity | Section Usage | Values Visible in Section | Linked Source in PDF |
|---|---|---|---|
| `SC@ResultCode` | `ExtendedInfo Code="RES_CODE"` | `WP`, `TKO-I`, `NC`, `DKO`; `DSQ` and `BDSQ` are mentioned in result examples/notes. | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultCode_SOG_BOX.htm` |
| `SC@Period` | `ExtendedInfo Code="PERIOD"`, `ROUND`, `Period/@Code`, warning/KD positions | `R1`, `R2`, `R3`; `TOT` is used as a total position for warnings and knockdowns but is not a period code. | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_BOX.htm` |
| `SC@ResultType` | `Result/@ResultType` | `RM_POINTS`, `POINTS`, `RM`; prose also shows mixed-case labels `RM_Points` and `Points`. | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_BOX.htm` |
| `SC@WLT` | `Result/@WLT` | `W`, `L` | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_WLT_SOG_BOX.htm` |
| `SC@Colour` | `EventUnitEntry Code="COLOUR"` | No concrete color values are printed in pages 27-37. | No direct link extracted from the `DT_RESULT` pages. |
| `CC@ORGANISATION` / `CC@Organisation` | Competitor, official, and athlete organisation | Source uses organisation IDs such as `NZL` and `GBR` in samples. | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm` and `http://odf.olympictech.org/2020-Tokyo/codes/HTML/og_cc/Organisation.htm` |

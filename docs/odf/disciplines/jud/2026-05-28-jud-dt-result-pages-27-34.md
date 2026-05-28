# ODF JUD Data Dictionary: DT_RESULT, Pages 27-34

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_JUD_Data_Dictionary.pdf`, pages 27-34.

Source version: `SCOG/SYOG-2026-JUD-1.3 SFR`, dated 13 May 2026.

This note restructures the Judo `DT_RESULT` section into a practical domain reference for the Event Unit Start List and
Results message. It covers contest lifecycle, actual timing, gold-score state, winning technique, penalties, scoring
types, outcome/progression, officials, competitor identity, and athlete event-unit entries.

## 2.3.4 Event Unit Start List and Results

`DT_RESULT` contains both start-list and result information for the competitors in one Judo event unit. The message is
mandatory and is always a full message: each emission sends all applicable elements and attributes, not only changed
values.

For Judo, the result is modeled around two athlete competitors:

- `SortOrder="1"` / `StartSortOrder="1"` is the white competitor.
- `SortOrder="2"` / `StartSortOrder="2"` is the blue competitor.
- `Result/@Result` is the competitor score and may be sent during gold score.
- Penalties are not included in `Result/@Result`; they are sent through `Result/@Pty` and `ExtendedResults`.
- `ExtendedInfos` carries contest-level state such as result code, gold-score flag, regular/gold-score durations, and
  winning technique.

## Header Values

| Attribute         | Value                 | Meaning                                                                        |
|-------------------|-----------------------|--------------------------------------------------------------------------------|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID.                                                                |
| `DocumentCode`    | `CC@EVENT_UNIT`       | Event Unit RSC.                                                                |
| `DocumentSubcode` | N/A                   | Not used.                                                                      |
| `DocumentType`    | `DT_RESULT`           | Event Unit Start List and Results message.                                     |
| `DocumentSubtype` | N/A                   | Not used.                                                                      |
| `Version`         | Positive integer      | Ascending version number for the message content.                              |
| `ResultStatus`    | `CC@RESULTSTATUS`     | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag`        | `P`, `T`              | `P` production, `T` test.                                                      |
| `Date`            | Date                  | ODF header generation date.                                                    |
| `Time`            | Time                  | ODF header generation time.                                                    |
| `LogicalDate`     | Date                  | ODF logical date.                                                              |
| `Source`          | `SCGEN@Source`        | System that generated the message.                                             |

## Trigger and Frequency

| `ResultStatus` | When to send                                                      |
|----------------|-------------------------------------------------------------------|
| `START_LIST`   | As soon as each competitor is known.                              |
| `START_LIST`   | After any start-list data change.                                 |
| `LIVE`         | When the contest starts and after every data change.              |
| `INTERMEDIATE` | For unexpected interruptions, not between normal contest periods. |
| `UNOFFICIAL`   | After the contest when unofficial results are available.          |
| `OFFICIAL`     | After the contest when results become official.                   |
| `PROVISIONAL`  | Listed as an allowed header status.                               |

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
    |   |   +-- @Duration
    |   +-- ExtendedInfo (0,N)
    |   |   +-- @Type
    |   |   +-- @Code
    |   |   +-- @Pos
    |   |   +-- @Value
    |   |   +-- Extension (0,N)
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
    |       +-- @Bib
    |       +-- Description (1,1)
    |           +-- @GivenName
    |           +-- @FamilyName
    |           +-- @Gender
    |           +-- @Organisation
    |           +-- @IFId
    +-- Result (1,N)
        +-- @Result
        +-- @IRM
        +-- @WLT
        +-- @SortOrder
        +-- @StartSortOrder
        +-- @ResultType
        +-- @Pty
        +-- ExtendedResults (0,1)
        |   +-- ExtendedResult (1,N)
        |       +-- @Type
        |       +-- @Code
        |       +-- @Pos
        |       +-- @Value
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

| Attribute | M/O | Value   | Meaning                                                    |
|-----------|-----|---------|------------------------------------------------------------|
| `Gen`     | O   | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport`   | O   | `S(20)` | Sport Data Dictionary version applicable to the message.   |
| `Codes`   | O   | `S(20)` | Code-set version applicable to the message.                |

### `Competition / ExtendedInfos / UnitDateTime`

Actual unit times. Include when the unit starts.

| Attribute   | M/O | Value    | Meaning                     |
|-------------|-----|----------|-----------------------------|
| `StartDate` | M   | DateTime | Actual start date and time. |
| `EndDate`   | O   | DateTime | Actual end date-time.       |
| `Duration`  | O   | `mm:ss`  | Total contest duration.     |

### `Competition / ExtendedInfos / ExtendedInfo`

| Type | Code               | Pos | Expected When                          | Value                      | Meaning                                                                                        |
|------|--------------------|-----|----------------------------------------|----------------------------|------------------------------------------------------------------------------------------------|
| `UI` | `RES_CODE`         | N/A | When available in individual contests. | `SC@ResultCode`            | Decision for how the contest was won. If gold score applies, send the result after gold score. |
| `UI` | `GOLD_SCORE`       | N/A | If applicable in individual contests.  | `Y`                        | Gold-score flag. Send only when in golden score.                                               |
| `UI` | `TECH_CODE`        | N/A | As appropriate in individual contests. | `SC@Technique` code        | Winning technique code. If gold score applies, send the result after gold score.               |
| `UI` | `TECH_DESCRIPTION` | N/A | As appropriate in individual contests. | `SC@Technique` description | Winning technique description from the sport-code table.                                       |

### `Competition / ExtendedInfos / ExtendedInfo / Extension`

Expected for bouts going into golden score.

| Parent Code  | Extension Code | Pos | Value   | Meaning                            |
|--------------|----------------|-----|---------|------------------------------------|
| `GOLD_SCORE` | `DURATION`     | `1` | `mm:ss` | Regular-time duration in the bout. |
| `GOLD_SCORE` | `DURATION`     | `2` | `mm:ss` | Golden-score duration in the bout. |

### `Competition / ExtendedInfos / SportDescription`

| Attribute        | M/O | Value                                     | Meaning                         |
|------------------|-----|-------------------------------------------|---------------------------------|
| `DisciplineName` | M   | `CC@DISCIPLINE` English description       | Discipline display name.        |
| `EventName`      | M   | `CC@EVENT` English description            | Event display name.             |
| `Gender`         | M   | `CC@DISCIPLINE_GENDER`                    | Gender code for the event unit. |
| `SubEventName`   | M   | `CC@EVENT_UNIT` English short description | Event-unit display name.        |
| `UnitNum`        | O   | `S(6)`                                    | Contest number.                 |

### `Competition / ExtendedInfos / VenueDescription`

| Attribute      | M/O | Value                             | Meaning        |
|----------------|-----|-----------------------------------|----------------|
| `Venue`        | M   | `CC@VENUE` ID                     | Venue code.    |
| `VenueName`    | M   | `CC@VENUE` English description    | Venue name.    |
| `Location`     | M   | `CC@LOCATION` ID                  | Location code. |
| `LocationName` | M   | `CC@LOCATION` English description | Location name. |

### `Competition / Officials / Official`

| Attribute  | M/O | Value                          | Meaning                                                                   |
|------------|-----|--------------------------------|---------------------------------------------------------------------------|
| `Code`     | M   | `S(20)` with no leading zeroes | Official code.                                                            |
| `Function` | M   | `CC@DISCIPLINE_FUNCTION` ID    | Official function, such as referee or judge; may differ from `DT_PARTIC`. |
| `Order`    | M   | Positive integer               | Official order. The PDF example sends `1` for Referee.                    |
| `Bib`      | O   | `S(4)`                         | Referee bib.                                                              |

### `Competition / Officials / Official / Description`

| Attribute      | M/O | Value                 | Meaning                                |
|----------------|-----|-----------------------|----------------------------------------|
| `GivenName`    | O   | `S(25)`               | Given name in WNPA mixed-case format.  |
| `FamilyName`   | M   | `S(25)`               | Family name in WNPA mixed-case format. |
| `Gender`       | M   | `CC@PERSON_GENDER` ID | Official gender.                       |
| `Organisation` | M   | `CC@ORGANISATION` ID  | Official organisation.                 |
| `IFId`         | O   | `S(16)`               | International Federation ID.           |

### `Competition / Result`

| Attribute        | M/O | Value            | Meaning                                                                                      |
|------------------|-----|------------------|----------------------------------------------------------------------------------------------|
| `Result`         | O   | String           | Competitor score in the event unit. May be sent in golden score. Penalties are not included. |
| `IRM`            | O   | `SC@IRM`         | Invalid-rank mark, including DNS before competition.                                         |
| `WLT`            | O   | `SC@WLT`         | `W` if the competitor won, `L` if lost. Send `L` in no-winner cases.                         |
| `SortOrder`      | M   | Positive integer | Sequential competitor order: `1` white, `2` blue.                                            |
| `StartSortOrder` | M   | Positive integer | Start-list order: `1` white, `2` blue.                                                       |
| `ResultType`     | O   | `SC@ResultType`  | Type of `Result`, either points or IRM for the unit.                                         |
| `Pty`            | O   | `SC@PenaltyType` | Penalty code associated to the score in individual contests.                                 |

### `Competition / Result / ExtendedResults / ExtendedResult`

| Type | Code                  | Pos                | Expected When     | Value        | Meaning                                                                                                                                                              |
|------|-----------------------|--------------------|-------------------|--------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `ER` | `OUTCOME`             | N/A                | When available.   | `SC@Outcome` | Progression of the athlete in the competition.                                                                                                                       |
| `ER` | `SC@PenaltyType` code | `1`, `2`, or `TOT` | When available.   | `#0`         | Number of penalties of this penalty type: `1` regular time, `2` golden score, `TOT` total. Send value `0` and penalty type `S` when the competitor has no penalties. |
| `ER` | `SC@PointsType` code  | `1`, `2`, or `TOT` | As soon as known. | `#0`         | Number of scores of this points type: `1` regular time, `2` golden score, `TOT` total. Send `0` for scoring types without scoring.                                   |

### `Competition / Result / Competitor`

| Attribute      | M/O | Value                                                | Meaning                                           |
|----------------|-----|------------------------------------------------------|---------------------------------------------------|
| `Code`         | M   | `S(20)` with no leading zeroes, `SC@CompetitorPlace` | Competitor ID, `TBD`, or `NCT` for no contestant. |
| `Type`         | M   | `A`                                                  | Athlete competitor.                               |
| `Organisation` | O   | `CC@Organisation` ID                                 | Competitor organisation.                          |

### `Competition / Result / Competitor / Composition / Athlete`

| Attribute | M/O | Value                                                   | Meaning                                              |
|-----------|-----|---------------------------------------------------------|------------------------------------------------------|
| `Code`    | M   | `S(20)` with no leading zeroes, or `SC@CompetitorPlace` | Athlete ID or `NCT` for no contestant.               |
| `Order`   | M   | Positive integer                                        | Athlete order; send `1` when `Competitor/@Type="A"`. |

### `Competition / Result / Competitor / Composition / Athlete / Description`

| Attribute      | M/O | Value                 | Meaning                                |
|----------------|-----|-----------------------|----------------------------------------|
| `GivenName`    | O   | `S(25)`               | Given name in WNPA mixed-case format.  |
| `FamilyName`   | M   | `S(25)`               | Family name in WNPA mixed-case format. |
| `Gender`       | M   | `CC@PERSON_GENDER` ID | Athlete gender.                        |
| `Organisation` | M   | `CC@ORGANISATION` ID  | Athlete organisation.                  |
| `BirthDate`    | O   | `YYYY-MM-DD`          | Birth date; include when available.    |
| `IFId`         | O   | `S(16)`               | International Federation ID.           |

### `Competition / Result / Competitor / Composition / Athlete / EventUnitEntry`

| Type  | Code          | Pos | Expected When   | Value       | Meaning                                                                        |
|-------|---------------|-----|-----------------|-------------|--------------------------------------------------------------------------------|
| `EUE` | `BODYWEIGHT`  | N/A | Always.         | `##0.0`     | Athlete bodyweight.                                                            |
| `EUE` | `COLOUR`      | N/A | Always.         | `SC@Colour` | Athlete contest colour.                                                        |
| `EUE` | `RANK_WLD`    | N/A | When available. | `S(3)`      | World ranking.                                                                 |
| `EUE` | `RESULT_BEST` | N/A | When available. | `S(30)`     | Best achievement before the Olympic event, same information as in `DT_PARTIC`. |

## Samples from the Dictionary, Normalized

### Contest state with result code and technique

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-JUD-1.3 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <UnitDateTime StartDate="2026-10-31T16:10:00+00:00" Duration="3:12"/>
    <ExtendedInfo Type="UI" Code="RES_CODE" Value="WAZ"/>
    <ExtendedInfo Type="UI" Code="TECH_CODE" Value="P29"/>
    <ExtendedInfo Type="UI" Code="TECH_DESCRIPTION" Value="Non-Combatively"/>
  </ExtendedInfos>
  <Officials>
    <Official Code="7350035" Order="1" Function="RE" Bib="12">
      <Description GivenName="Alexander" FamilyName="Zverkov" Gender="M" Organisation="RUS"/>
    </Official>
  </Officials>
</Competition>
```

### Golden score durations and per-competitor scoring

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SCOG/SYOG-2026-JUD-1.3 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <UnitDateTime StartDate="2026-10-31T16:10:00+00:00" Duration="5:42"/>
    <ExtendedInfo Type="UI" Code="GOLD_SCORE" Value="Y">
      <Extension Code="DURATION" Pos="1" Value="4:00"/>
      <Extension Code="DURATION" Pos="2" Value="1:42"/>
    </ExtendedInfo>
    <ExtendedInfo Type="UI" Code="RES_CODE" Value="WAZ"/>
  </ExtendedInfos>
  <Result ResultType="POINTS" Result="11" WLT="W" SortOrder="1" StartOrder="1" StartSortOrder="1">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="S3" Pos="1" Value="1"/>
      <ExtendedResult Type="ER" Code="IPP" Pos="1" Value="1"/>
      <ExtendedResult Type="ER" Code="WAZ" Pos="1" Value="1"/>
      <ExtendedResult Type="ER" Code="OUTCOME" Value="ABC"/>
    </ExtendedResults>
    <Competitor Code="1008743" Type="A" Organisation="SUI">
      <Composition>
        <Athlete Code="1008743" Order="1">
          <Description GivenName="Jane" FamilyName="Smits" Gender="W" Organisation="SUI"
                       BirthDate="2009-12-15"/>
          <EventUnitEntry Type="EUE" Code="COLOUR" Value="WHITE"/>
          <EventUnitEntry Type="EUE" Code="BODYWEIGHT" Value="52.7"/>
          <EventUnitEntry Type="EUE" Code="RANK_WLD" Value="007"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
  <Result ResultType="POINTS" Result="0" WLT="L" SortOrder="2" StartOrder="2" StartSortOrder="2">
    <ExtendedResults>
      <ExtendedResult Type="ER" Code="S" Pos="TOT" Value="0"/>
      <ExtendedResult Type="ER" Code="IPP" Pos="TOT" Value="0"/>
      <ExtendedResult Type="ER" Code="WAZ" Pos="TOT" Value="0"/>
    </ExtendedResults>
    <Competitor Code="1008750" Type="A" Organisation="SEN">
      <Composition>
        <Athlete Code="1008750" Order="1">
          <Description GivenName="Awa" FamilyName="Diop" Gender="W" Organisation="SEN"
                       BirthDate="2009-04-12"/>
          <EventUnitEntry Type="EUE" Code="COLOUR" Value="BLUE"/>
          <EventUnitEntry Type="EUE" Code="BODYWEIGHT" Value="52.4"/>
        </Athlete>
      </Composition>
    </Competitor>
  </Result>
</Competition>
```

## Message Sort

Sort by `Result/@SortOrder`.

For Judo this means:

| Sort Order | Competitor       |
|-----------:|------------------|
|        `1` | White competitor |
|        `2` | Blue competitor  |

## Modeling Notes

- Treat `DT_RESULT` as the authoritative full snapshot for one Judo contest. Replace previous unit state by version
  rather than patch-merging sparse fragments.
- Preserve white/blue as a stable contest role. It drives `SortOrder`, `StartSortOrder`, display colour, and many
  downstream score/penalty interpretations.
- `Result/@Result` is not a complete scoring ledger. Detailed points and penalties live in `ExtendedResults` where
  `Code` is a sport code from `SC@PointsType` or `SC@PenaltyType`.
- `ExtendedInfo/GOLD_SCORE` is contest-level state. Its `Extension` children split regular-time and golden-score
  durations by `Pos`.
- `TECH_CODE` and `TECH_DESCRIPTION` carry the same winning-technique concept in code and description form. Store the
  code as canonical and treat the description as display text.
- `EventUnitEntry/BODYWEIGHT`, `COLOUR`, `RANK_WLD`, and `RESULT_BEST` are athlete-level event-unit facts; do not model
  them as contest scoring.

## Code Appendix: Values Directly Visible in Pages 27-34

The PDF links several sport-code pages. The linked `odf.olympictech.org` pages timed out during extraction, so this
appendix records values directly visible in the `DT_RESULT` pages, not complete authoritative code tables.

| Code Entity          | Section Usage                                                     | Values Visible in Section                                                     | Linked Source in PDF                                                                   |
|----------------------|-------------------------------------------------------------------|-------------------------------------------------------------------------------|----------------------------------------------------------------------------------------|
| `CC@RESULTSTATUS`    | `OdfBody/@ResultStatus`                                           | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` | Not directly extracted from these pages.                                               |
| `SC@ResultCode`      | `ExtendedInfo Code="RES_CODE"`                                    | `WAZ` appears in the sample.                                                  | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultCode_SOG_JUD.htm` |
| `SC@Technique`       | `TECH_CODE`, `TECH_DESCRIPTION`, `Action/@Result` in play-by-play | `P29` and description `Non-Combatively` appear in the sample.                 | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Technique_SOG_JUD.htm`  |
| `SC@IRM`             | `Result/@IRM`                                                     | No concrete values printed in pages 27-34.                                    | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_IRM_SOG_JUD.htm`        |
| `SC@WLT`             | `Result/@WLT`                                                     | `W`, `L`.                                                                     | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_WLT_SOG_JUD.htm`        |
| `SC@ResultType`      | `Result/@ResultType`                                              | `POINTS` appears in the sample; prose describes points or IRM result types.   | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResultType_SOG_JUD.htm` |
| `SC@PenaltyType`     | `Result/@Pty`, `ExtendedResult/@Code` penalty rows                | `S3`, `S`; positions `1`, `2`, `TOT`.                                         | No direct link extracted from pages 27-34.                                             |
| `SC@PointsType`      | `ExtendedResult/@Code` scoring rows                               | `IPP`, `WAZ`; positions `1`, `2`, `TOT`.                                      | No direct link extracted from pages 27-34.                                             |
| `SC@Outcome`         | `ExtendedResult Code="OUTCOME"`                                   | Sample uses `ABC` as a placeholder-like value.                                | No direct link extracted from pages 27-34.                                             |
| `SC@Colour`          | `EventUnitEntry Code="COLOUR"`                                    | `WHITE` appears in the sample.                                                | `http://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Colour_SOG_JUD.htm`     |
| `SC@CompetitorPlace` | Competitor and athlete placeholders                               | `TBD`, `NCT`.                                                                 | No direct link extracted from pages 27-34.                                             |

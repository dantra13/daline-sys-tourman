# ODF VBV Data Dictionary: DT_PLAY_BY_PLAY, Pages 49-54

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 49-54.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note restructures the Beach Volleyball `DT_PLAY_BY_PLAY` section into a practical domain reference for point-by-point
action data in one match unit. It covers the dual-mode emission (per-period vs full-match), action ordering, rally and
score deltas, server/winner indicators, action-time semantics, and the per-action competitor and athlete payload.

## 2.3.7 Play by Play

`DT_PLAY_BY_PLAY` contains official raw data from the results provider for each action. The message uses a generic
action list that can carry results data of varied nature, and all actions in a unit. All actions are included; key
actions can be filtered via `Action/@Loc`.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Unique competition identifier. |
| `DocumentCode` | `CC@EVENT_UNIT` | Full RSC of the event unit. |
| `DocumentSubcode` | `SC@Period` or not sent | Period code (`S1`-`S3`) when the message covers a single period; omitted when the message includes the full match. |
| `DocumentType` | `DT_PLAY_BY_PLAY` | Play by Play message. |
| `DocumentSubtype` | `ACTION` | Always send `ACTION`. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | `CC@ResultStatus` | Lifecycle status. The section lists `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Date` | Date | ODF header generation date. |
| `Time` | Time | ODF header generation time. |
| `LogicalDate` | Date | ODF logical date. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

The section defines two emission modes that share the same `DocumentType` but differ by whether `DocumentSubcode`
carries a period code.

### Message with `DocumentSubcode`

| Trigger | `ResultStatus` | Meaning |
|---|---|---|
| After every rally and after every correction for the current period, while the period is still running. | `LIVE` | Per-period action stream. The last action in the period is `ENDP`. |
| When a challenge is requested. | `LIVE` | Emit as soon as the challenge is requested. |
| When the challenge result is known. | `LIVE` | Emit again with the result, without replacing the requested action, so both actions appear. |

### Message without `DocumentSubcode`

| Trigger | `ResultStatus` | Meaning |
|---|---|---|
| Start list is available; also used to clear all actions. | `START_LIST` | Send empty. |
| After each period (non-final). | `INTERMEDIATE` | Full-match action list as of the end of the period. |
| After the last period. | `UNOFFICIAL` | Full-match action list at end of play. |
| Match is official. | `OFFICIAL` | Full-match action list with all periods. |
| Correction for a previous period when that period is no longer running. | Applicable status | Resend full-match action list. |

The full play by play (no `DocumentSubcode`) is the master: whenever it arrives, replace all stored play-by-play data
with this one. The intended cadence is before period 1, after each period, and after the game, but it may also arrive
out of cadence to correct earlier periods.

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- SportDescription (0,1)
    |   |   +-- @DisciplineName
    |   |   +-- @EventName
    |   |   +-- @SubEventName
    |   |   +-- @Gender
    |   |   +-- @UnitNum
    |   +-- VenueDescription (0,1)
    |       +-- @Venue
    |       +-- @VenueName
    |       +-- @Location
    |       +-- @LocationName
    +-- Actions (0,1)
        +-- @Home
        +-- @Away
        +-- Action (1,N)
            +-- @Id
            +-- @Period
            +-- @Order
            +-- @Action
            +-- @ActionAdd
            +-- @Comment
            +-- @When
            +-- @Result
            +-- @ScoreH
            +-- @ScoreA
            +-- @LeadH
            +-- @LeadA
            +-- @Rally
            +-- @Win
            +-- @Speed
            +-- @Loc
            +-- @Line
            +-- @TimeStamp
            +-- Competitor (0,N)
                +-- @Code
                +-- @Type
                +-- @Order
                +-- @Organisation
                +-- Composition (0,1)
                    +-- Athlete (1,N)
                        +-- @Code
                        +-- @Order
                        +-- @Bib
                        +-- Description (1,1)
                            +-- @GivenName
                            +-- @FamilyName
                            +-- @Gender
                            +-- @Organisation
                            +-- @BirthDate
                            +-- @IFId
```

## Message Values

### `Competition`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Gen` | O | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | O | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | O | `S(20)` | Code-set version applicable to the message. |

### `Competition / ExtendedInfos / SportDescription`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `DisciplineName` | M | `CC@DISCIPLINE` English description | Discipline name, not code. |
| `EventName` | M | `CC@EVENT` English description | Event name, not code. |
| `SubEventName` | M | `CC@EVENT_UNIT` English short description | Event-unit description, not code. |
| `Gender` | M | `CC@DISCIPLINE_GENDER` | Gender code for the event unit. |
| `UnitNum` | O | `S(15)` | Match number. |

### `Competition / ExtendedInfos / VenueDescription`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Venue` | M | `CC@VENUE` ID | Venue code. |
| `VenueName` | M | `CC@VENUE` English description | Venue name. |
| `Location` | M | `CC@LOCATION` ID | Location code. |
| `LocationName` | M | `CC@LOCATION` English description | Location name. |

### `Competition / Actions`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Home` | M | `S(20)` with no leading zeroes | Home competitor ID. |
| `Away` | M | `S(20)` with no leading zeroes | Away competitor ID. |

### `Actions / Action`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Id` | M | `S(36)` | Unique identifier for the action within the message. The value is stable for a given action regardless of whether it appears in a per-period or full-match message. |
| `Period` | M | `SC@Period` | Period within the match. |
| `Order` | M | Positive integer | Unique sequential ascending number across all incidents and actions, from `1` to `n` across all periods. Used to sort actions. Stable across per-period and full-match emissions of the same action. |
| `Action` | M | `SC@Action` | Action for the player or team. |
| `ActionAdd` | O | `SC@Challenge` or `S` | Type of challenge, or `S` for a Spectacular Action. |
| `Comment` | O | `SC@ActionComment` | Included when `Action` is `STARTP` or `ENDP`. |
| `When` | O | `h:mm:ss` | Time the action occurred, cumulative from the start of the match. Do not send leading zeros, except when minutes are zero do not pad (use `0:`, not `02:`). |
| `Result` | O | `SC@ResAction` | Result of the action for the player or team. |
| `ScoreH` | O | `#0` | Home total score in the set after the action. Send when there is a score change for either team. |
| `ScoreA` | O | `#0` | Away total score in the set after the action. Send when there is a score change for either team. |
| `LeadH` | O | `+/-#0` | Points lead for the Home team in the set. Send when there is a score change for either team. |
| `LeadA` | O | `+/-#0` | Points lead for the Away team in the set. Send when there is a score change for either team. |
| `Rally` | O | `##0` | Rally number in which the action occurs. |
| `Win` | O | `SC@Home` | Rally-winning indicator: `H` if the Home team wins the current rally, `A` if the Away team wins. |
| `Speed` | O | `##0` | Serve speed. |
| `Loc` | O | `S(3)` | Send `KEY` when the action is a key action, usually tied to a score. |
| `Line` | O | `#0` | Used to associate actions on the same rally. |
| `TimeStamp` | O | DateTime | Wall-clock time of the action, for alignment with video. |

### `Action / Competitor`

Competitor participating in the action. Used when the action is related to a competitor.

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | M | `A`, `T` | `A` for athlete, `T` for team. |
| `Order` | O | Positive integer | Order in which the competitor should appear within the action when there is more than one. |
| `Organisation` | M | `CC@Organisation` ID | Competitor organisation. |

### `Competitor / Composition / Athlete`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` with no leading zeroes | Athlete ID, for an individual athlete or a team member tied to the action. |
| `Order` | O | Positive integer | Order of athletes when more than one is related to the action. |
| `Bib` | O | `S(2)` | Bib number. |

### `Athlete / Description`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `GivenName` | O | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | M | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | M | `CC@PERSON_GENDER` ID | Athlete gender. |
| `Organisation` | M | `CC@ORGANISATION` ID | Athlete organisation. |
| `BirthDate` | O | `YYYY-MM-DD` | Birth date when available. |
| `IFId` | O | `S(16)` | International Federation ID. |

## Samples from the Dictionary, Normalized

```xml
<Competition Gen="OWG2026-GEN-4.6 APP" Sport="SYOG-2026-VBV-1.2 SFR" Codes="SYOG-2026">
  <ExtendedInfos>
    <SportDescription DisciplineName="Beach Volleyball" EventName="Men's Beach Volleyball" SubEventName="Pool A Match 1" Gender="M"/>
    <VenueDescription Venue="MFS" VenueName="Multi-Function Sport Hall" Location="BV1" LocationName="Beach Court 1"/>
  </ExtendedInfos>
  <Actions Home="VBVMTEAM2---BRA02" Away="VBVMTEAM2---RUS02">
    <Action Id="123456" Period="S1" Order="32" Action="SRV" Result="CON" Rally="8" Line="1">
      <Competitor Code="VBVMTEAM2---RUS02" Type="T" Order="2" Organisation="RUS">
        <Composition>
          <Athlete Code="1133405" Order="1" Bib="1">
            <Description GivenName="Nikita" FamilyName="Liamin" Gender="M" Organisation="RUS" BirthDate="1985-10-14"/>
          </Athlete>
        </Composition>
      </Competitor>
    </Action>
    <Action Id="111111" Period="S1" Order="33" Action="ATC" Result="SCS" LeadH="2" LeadA="-2" ScoreH="5" ScoreA="3" Rally="8" Win="H" Line="2">
      <Competitor Code="VBVMTEAM2---BRA02" Type="T" Order="1" Organisation="BRA">
        <Composition>
          <Athlete Code="1157802" Order="1" Bib="2">
            <Description GivenName="Evandro" FamilyName="Goncalves Oliveira Junior" Gender="M" Organisation="BRA" BirthDate="1990-07-17"/>
          </Athlete>
        </Composition>
      </Competitor>
    </Action>
    <Action Id="222222" Period="S1" Order="34" Action="DIG" Result="FLT" Rally="8" Line="2">
      <Competitor Code="VBVMTEAM2---RUS02" Type="T" Order="2" Organisation="RUS">
        <Composition>
          <Athlete Code="1133406" Order="1" Bib="2">
            <Description GivenName="Dmitri" FamilyName="Barsuk" Gender="M" Organisation="RUS" BirthDate="1980-01-20"/>
          </Athlete>
        </Composition>
      </Competitor>
    </Action>
  </Actions>
</Competition>
```

## Message Sort

Sort by `Actions / Action @Order`.

`Action/@Order` is unique and ascending across all periods of the match, not just within the current period.

## XSD Validation

The normalized XML example above was validated as a `Competition` fragment wrapped in an `OdfBody` envelope against a
temporary copy of `odf2-schema-30112025-DRAFT`. The temporary schema copy only patched the known unresolved
`RecordBrokenType` reference to the existing `extRecordBrokenType`; the source XSD files were not modified.

## Modeling Notes

- Two emission modes share `DocumentType=DT_PLAY_BY_PLAY` and `DocumentSubtype=ACTION`. Distinguish them by the presence
  of `DocumentSubcode`: with a `SC@Period` value the payload is a per-period delta, without it the payload is the
  full-match master and should replace any prior action store for the unit.
- `Action/@Id` is identity; `Action/@Order` is the canonical sort key. Both must remain stable for a given action across
  per-period and full-match emissions, so ingestion can dedupe by `Id` and reorder by `Order` without conflict.
- The per-period stream is rally-triggered: each rally yields one or more actions, with the period closing on an
  `ENDP` action carrying a `Comment` from `SC@ActionComment`. `STARTP` opens the period symmetrically.
- Challenges are append-only on the wire: when a challenge is requested, emit it; when its result is known, emit a
  second action without replacing the original, so both the request and the outcome remain visible in `Order`.
- `Rally` groups all actions on the same rally; `Line` orders actions within a rally (for example serve, attack, dig
  appearing on `Line` 1, 2, 2 in the sample). Treat `Rally` plus `Line` as the within-rally ordering pair.
- `Win` is the rally winner, not the point/score winner per side. Score and lead deltas (`ScoreH`, `ScoreA`, `LeadH`,
  `LeadA`) are reported only on actions that change the score; absence means no change.
- `When` is cumulative match clock in `h:mm:ss` form with non-padded leading values; `TimeStamp` is wall-clock DateTime
  intended for video alignment. Model them separately rather than collapsing into a single field.
- `Loc=KEY` is a flag for highlight filtering, not a coordinate. Ingestion pipelines that want only key actions can
  filter on this without losing the master record.
- `Competitor/@Order` resolves which of the two competitors a multi-competitor action attributes to; combined with the
  team-level `Actions/@Home` and `Actions/@Away`, it lets one action reference both sides when needed.
- The Athlete branch on each Action carries enough description (`GivenName`, `FamilyName`, `Gender`, `Organisation`,
  `BirthDate`, `IFId`) to render the action without joining back to `DT_RESULT` or `DT_PARTIC`, at the cost of payload
  duplication.

## Code Appendix: Values Directly Visible in Pages 49-54

The section references several code pages. This appendix records values directly visible in the `DT_PLAY_BY_PLAY` pages
and does not embed external master-data tables.

| Code Entity | Section Usage | Values Visible in Section |
|---|---|---|
| `CC@ResultStatus` | `OdfBody/@ResultStatus` | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL` |
| `SC@Period` | `DocumentSubcode`, `Action/@Period` | Range stated as `S1` - `S3`; `S1` visible in samples. |
| `SC@Action` | `Action/@Action` | `SRV`, `ATC`, `DIG` visible in samples; `STARTP` and `ENDP` named in the period boundary rules. |
| `SC@Challenge` | `Action/@ActionAdd` | No concrete values printed in pages 49-54; `S` is the inline literal for Spectacular Action. |
| `SC@ActionComment` | `Action/@Comment` | No concrete values printed in pages 49-54. |
| `SC@ResAction` | `Action/@Result` | `CON`, `SCS`, `FLT` visible in samples. |
| `SC@Home` | `Action/@Win` | `H`, `A` described inline; `H` visible in samples. |
| `CC@Organisation` | `Competitor/@Organisation`, `Description/@Organisation` | `RUS`, `BRA` visible in samples. |
| `CC@PERSON_GENDER` | `Description/@Gender` | `M` visible in samples. |
| `CC@DISCIPLINE_GENDER` | `SportDescription/@Gender` | No concrete values printed in pages 49-54. |
| `Competitor/@Type` | Inline enum | `A` athlete, `T` team; `T` visible in samples. |
| `Action/@Loc` flag | Inline literal | `KEY` for key actions. |
| `DocumentSubtype` | Header | `ACTION` literal. |

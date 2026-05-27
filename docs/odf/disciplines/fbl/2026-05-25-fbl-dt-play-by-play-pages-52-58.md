
# ODF FBL Data Dictionary: DT_PLAY_BY_PLAY, Pages 52-58

Source: `C:\Users\mella\Downloads\ODF_FBL_Data_Dictionary.pdf`, pages 52-58.

This note restructures the football `DT_PLAY_BY_PLAY` section into readable Markdown for domain modeling. It covers the Play by Play message, header metadata, period-scoped vs full-message behavior, message tree, action fields, action participants, athlete/coach payloads, XML examples aligned with the supplied ODF2 XSD, and an appendix of English SC/CC values downloaded from the Paris 2024 ODF code pages.

## 2.3.6 Play by Play

`DT_PLAY_BY_PLAY` contains official raw action data from the result provider. In football this is the event stream for a match: period starts/ends, goals, shots, fouls, cards, substitutions, VAR reviews, penalties, own goals, offsides, corners, and related player/team context.

The message is generic enough to carry different kinds of result-provider actions, but the football dictionary constrains it through sport-specific action, result, VAR, role, and period codes.

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Unit` | Full RSC of the unit. |
| `DocumentSubcode` | `SC @Period` or omitted | Period code when the message is for one period only. If omitted, the message contains the full match. The dictionary text mentions `H1`, `H2`, `OT1`, `OT2`, and `PSO`; the football code table uses period values such as `H1`, `H2`, `ET-H1`, `ET-H2`, and `PSO`. |
| `DocumentType` | `DT_PLAY_BY_PLAY` | Play by Play message. |
| `DocumentSubtype` | `S(8)` | Send `ACTION`. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `ResultStatus` | `CC @ResultStatus` | Message status. Football uses `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, and `PROVISIONAL`. |
| `FeedFlag` | `P` or `T` | `P` for production, `T` for test. |
| `Date` | `Date` | Local date when the message was generated. |
| `Time` | `Time` | Local generation time, up to milliseconds. |
| `LogicalDate` | `Date` | Logical event day. Usually the physical day, except when the unit or message transmission crosses midnight. |
| `Source` | `SC @Source` | System that generated the message. |

## Trigger and Frequency

### Message with `DocumentSubcode`

| Condition | Behavior |
|---|---|
| Current period is running | Send after every action and every correction for the current period. |
| `ResultStatus` | Always `LIVE`. |
| Last action in the period | The final period action is `ENDP`. |

### Message without `DocumentSubcode`

| Condition | Behavior |
|---|---|
| Start list is available | Send an empty message with `ResultStatus="START_LIST"`; also used to clear all actions. |
| After each period | Send full play-by-play. Use `INTERMEDIATE` except after the final period, which is `UNOFFICIAL`. |
| Previous-period correction | Send full play-by-play if the corrected period is no longer running. |
| Between-period action | Include actions such as substitutions between periods only in the full message. |
| Match becomes official | Send full play-by-play with all periods and `ResultStatus="OFFICIAL"`. |

The full play-by-play message, meaning the one without `DocumentSubcode`, is the master. When it arrives, consumers should remove all stored play-by-play data for that unit and replace it with the incoming action list. The intended pattern is to send it before the first period, at the end of periods, after the game, and for corrections, but it may arrive at other times to fix earlier periods.

## Message Structure

```text
OdfBody
└─ Competition (0,1)
   ├─ @Gen
   ├─ @Sport
   ├─ @Codes
   ├─ ExtendedInfos (0,1)
   │  ├─ ExtendedInfo (0,N)
   │  │  ├─ @Type
   │  │  ├─ @Code
   │  │  ├─ @Pos
   │  │  └─ @Value
   │  ├─ SportDescription (0,1)
   │  │  ├─ @DisciplineName
   │  │  ├─ @EventName
   │  │  ├─ @SubEventName
   │  │  ├─ @Gender
   │  │  └─ @UnitNum
   │  └─ VenueDescription (0,1)
   │     ├─ @Venue
   │     ├─ @VenueName
   │     ├─ @Location
   │     └─ @LocationName
   └─ Actions (0,1)
      ├─ @Home
      ├─ @Away
      └─ Action (1,N)
         ├─ @Id
         ├─ @PId
         ├─ @Period
         ├─ @Order
         ├─ @Action
         ├─ @ActionAdd
         ├─ @Comment
         ├─ @When
         ├─ @Result
         ├─ @ScoreH
         ├─ @ScoreA
         ├─ @LeadH
         ├─ @LeadA
         ├─ @SO_H
         ├─ @SO_A
         ├─ @Loc
         ├─ @TimeStamp
         ├─ ExtendedAction (0,N)
         │  ├─ @Code
         │  ├─ @Pos
         │  └─ @Value
         └─ Competitor (0,N)
            ├─ @Code
            ├─ @Type
            ├─ @Order
            ├─ @Organisation
            ├─ Composition (0,1)
            │  └─ Athlete (1,N)
            │     ├─ @Code
            │     ├─ @Order
            │     ├─ @Bib
            │     ├─ @Role
            │     └─ Description (1,1)
            │        ├─ @GivenName
            │        ├─ @FamilyName
            │        ├─ @Gender
            │        ├─ @Organisation
            │        ├─ @BirthDate
            │        ├─ @IFId
            │        └─ @Class
            └─ Coaches (0,1)
               └─ Coach (1,N)
                  ├─ @Code
                  ├─ @Order
                  └─ Description (1,1)
                     ├─ @GivenName
                     ├─ @FamilyName
                     ├─ @Gender
                     └─ @Nationality
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional in dictionary, required by XSD | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional in dictionary, required by XSD | `S(20)` | Code-set version applicable to the message. |

## ExtendedInfos / ExtendedInfo

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `OT` | Numeric `0`; `1..n` | Send for every overtime period started or played in the game. | `SC @Period` | Applicable overtime period code. |

## SportDescription

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `DisciplineName` | Mandatory | `S(40)` | English discipline description from Common Codes, not the code itself. |
| `EventName` | Mandatory | `S(40)` | English event description from Common Codes. |
| `SubEventName` | Mandatory | `S(40)` | English event-unit description from Common Codes. |
| `Gender` | Mandatory | `CC @SportGender` | Gender code for the event unit. |
| `UnitNum` | Optional | `S(6)` | Match number. |

## VenueDescription

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Venue` | Mandatory | `CC @VenueCode` | Venue code. |
| `VenueName` | Mandatory | `S(25)` | English venue description from Common Codes. |
| `Location` | Mandatory | `CC @Location` | Location code. |
| `LocationName` | Mandatory | `S(30)` | English location description from Common Codes. |

## Actions

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Home` | Mandatory | `S(20)` with no leading zeroes | Home competitor ID. |
| `Away` | Mandatory | `S(20)` with no leading zeroes | Away competitor ID. |

## Actions / Action

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Id` | Mandatory | `S(36)` | Unique action identifier within the message. The same action keeps the same ID whether it is sent in a period message or in a full-match message. |
| `PId` | Optional | `S(36)` | Parent/original action ID when this action is related to another action. |
| `Period` | Mandatory | `SC @Period` | Period in which the action occurred. |
| `Order` | Mandatory | Numeric | Unique ascending sequence across all incidents/actions in all periods. Used to sort `Action`. The same action keeps the same order whether sent in a period message or in a full-match message. |
| `Action` | Optional | `SC @Action` | Game action. Send one action code. The first action of each period should always be `STARTP`. |
| `ActionAdd` | Optional | `SC @VarType` | Type of video review. |
| `Comment` | Optional | `SC @ActionComment` | Included when `Action` is `STARTP` or `ENDP`. |
| `When` | Optional | String `m' [x]` or `SC @PeriodAction` | Action time in minutes or break in play. Example: `14'`. Optional `[x]` is injury/added time such as `+3`, so `45' +3` is valid. It may also be a period action such as `HT`. |
| `Result` | Optional | `SC @ResAction` | Result of the action for the player/team. |
| `ScoreH` | Optional | Numeric `##0` | Total home score after the action. Send when there is a score change for either team. |
| `ScoreA` | Optional | Numeric `##0` | Total away score after the action. Send when there is a score change for either team. |
| `LeadH` | Optional | Numeric `#0` | Home team lead after a score change; may be negative. |
| `LeadA` | Optional | Numeric `#0` | Away team lead after a score change; may be negative. |
| `SO_H` | Optional | Numeric `#0` | Home score in penalty shoot-out. |
| `SO_A` | Optional | Numeric `#0` | Away score in penalty shoot-out. |
| `Loc` | Optional | `SC @VarStage` | VAR review progress/stage. Send for video reviews. |
| `TimeStamp` | Optional | `DateTime` | Time of the action, used for video alignment. |

For substitutions, send two `Athlete` elements: first the player out with `Role="OUT"`, then the player in with `Role="IN"`.

For fouls, send one or two competitor/player payloads: first the player who committed the foul with `Role="FOC"`, and optionally the player who suffered it with `Role="FOS"`.

## Action / ExtendedAction

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `VARDETAILS` | N/A | N/A | In the case of a video review. | `SC @VarDetails` | VAR detail code. |

## Action / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Order` | Optional | Numeric | Presentation/order value when more than one competitor participates in the action. |
| `Organisation` | Mandatory | `CC @Organisation` | Competitor organisation. |

## Competitor / Composition / Athlete

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Athlete/team-member ID related to the action. |
| `Order` | Optional | Numeric | Athlete order when more than one athlete is related to the action. |
| `Bib` | Optional | `S(4)` | Shirt number. |
| `Role` | Optional | `SC @ActionRole` | Player role in the action. |

## Athlete / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Athlete gender. |
| `Organisation` | Mandatory | `CC @Organisation` | Athlete organisation. |
| `BirthDate` | Optional | `Date` | Birth date, `YYYY-MM-DD`; must be included when available. |
| `IFId` | Optional | `S(16)` | International Federation ID. |
| `Class` | Optional | `CC @DisciplineClass` | Sport class for events involving athletes with a disability. |

## Competitor / Coaches / Coach

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Optional | `S(20)` with no leading zeroes | Official code. Normally expected, but rare exceptions may exist. |
| `Order` | Optional | Numeric `0` | Coach order when more than one coach is needed. Send `1` if only one. |

## Coach / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Official gender. |
| `Nationality` | Mandatory in dictionary, optional in XSD | `CC @Country` | Coach nationality. |

## Sample from the Dictionary, Normalized

The source sample shows a goal action with one team competitor and one athlete. The sample text includes a `Competitor/Description` element, but the supplied XSD draft's `competitorUnitActionType` only permits `Composition` and `Coaches` under play-by-play `Action/Competitor`. The XSD-aligned version below omits the team description and validates against the patched draft schema described later.

```xml
<Action Id="123456"
        Period="H1"
        Order="3"
        Action="SHOT"
        When="14'"
        Result="GOAL"
        ScoreH="0"
        ScoreA="1"
        LeadH="-1"
        LeadA="1">
  <Competitor Code="FBLWTEAM11-----RSA01" Type="T" Organisation="RSA" Order="1">
    <Composition>
      <Athlete Code="1106655" Order="1" Bib="9" Role="SHOT">
        <Description GivenName="Jane"
                     FamilyName="Smith"
                     Gender="F"
                     Organisation="RSA"
                     BirthDate="1993-05-12"/>
      </Athlete>
    </Composition>
  </Competitor>
</Action>
```

## Message Sort

Sort by `Actions / Action @Order`.

## XSD-Aligned XML Example

The provided XSD draft could not be loaded directly because `odf2-structure.xsd` references `RecordBrokenType`, which is not defined in the supplied schema folder. The example below was validated against a temporary copy of the schema where only that unrelated unresolved type reference was replaced so the schema could parse. No original XSD files were modified.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="FBLWTEAM11------------GPA-000100--"
         DocumentType="DT_PLAY_BY_PLAY"
         DocumentSubtype="ACTION"
         Version="2"
         ResultStatus="LIVE"
         FeedFlag="T"
         Date="2026-05-25"
         Time="18:45:00.000"
         LogicalDate="2026-05-25"
         Source="OVR">
  <Competition Gen="3.4" Sport="FBL-3.4" Codes="SOG-2024">
    <ExtendedInfos>
      <SportDescription DisciplineName="Football"
                        EventName="Women"
                        SubEventName="Women's Group A"
                        Gender="W"
                        UnitNum="1"/>
      <VenueDescription Venue="BOR"
                        VenueName="Bordeaux Stadium"
                        Location="BOR"
                        LocationName="Bordeaux Stadium, Bordeaux"/>
    </ExtendedInfos>
    <Actions Home="FBLWTEAM11-----RSA01" Away="FBLWTEAM11-----BRA01">
      <Action Id="123456"
              Period="H1"
              Order="3"
              Action="SHOT"
              When="14'"
              Result="GOAL"
              ScoreH="0"
              ScoreA="1"
              LeadH="-1"
              LeadA="1"
              TimeStamp="2026-05-25T18:14:00">
        <Competitor Code="FBLWTEAM11-----RSA01" Type="T" Organisation="RSA" Order="1">
          <Composition>
            <Athlete Code="1106655" Order="1" Bib="9" Role="SHOT">
              <Description GivenName="Jane"
                           FamilyName="Smith"
                           Gender="F"
                           Organisation="RSA"
                           BirthDate="1993-05-12"/>
            </Athlete>
          </Composition>
        </Competitor>
      </Action>
      <Action Id="123457"
              PId="123456"
              Period="H1"
              Order="4"
              Action="VAR"
              ActionAdd="GOAL"
              Loc="CONFIRMED"
              When="15'"
              Result="GOAL">
        <ExtendedAction Code="VARDETAILS" Value="LINE"/>
        <Competitor Code="FBLWTEAM11-----RSA01" Type="T" Organisation="RSA" Order="1">
          <Composition>
            <Athlete Code="1106655" Order="1" Bib="9" Role="SHOT">
              <Description GivenName="Jane" FamilyName="Smith" Gender="F" Organisation="RSA"/>
            </Athlete>
          </Composition>
        </Competitor>
      </Action>
    </Actions>
  </Competition>
</OdfBody>
```

## Modeling Notes

- Model period-scoped updates and full-match replacement as different ingestion modes. A period message patches/replaces the current running period; a full message replaces the entire action stream for the match.
- Preserve `Id` and `Order` as stable provider identifiers. `Id` identifies the action; `Order` is the canonical event-stream ordering across all periods.
- `PId` is the link for corrections or related actions. It should map to an explicit relation in the domain model, not just a string copied to output.
- Action semantics are path-dependent. `Action="SHOT"` plus `Result="GOAL"` is not the same as a standalone goal aggregate; it is an action event that may carry score deltas, participants, and VAR relations.
- Substitution and foul actions require multiple athlete roles under the same action. Avoid modeling every play-by-play row as exactly one player event.
- Full-match play-by-play can include between-period actions that period messages do not carry. The model needs a period/break concept for `When` values such as `HT`, `FT`, and `PSO`.
- VAR information is split across `ActionAdd`, `Loc`, and `ExtendedAction/@Value`. Treat VAR type, stage, and details as separate fields.
- The dictionary sample and supplied XSD differ on `Action/Competitor/Description`. Keep team names available from participant/team messages or match context, but do not rely on `Action/Competitor/Description` for XSD-conformant play-by-play output unless the schema release changes.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-25. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index
| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Action` | FBL | 18 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Action_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Action_SOG_FBL.htm) |
| `SC @ResAction` | FBL | 174 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResAction_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResAction_SOG_FBL.htm) |
| `SC @ActionRole` | FBL | 6 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ActionRole_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ActionRole_SOG_FBL.htm) |
| `SC @ActionComment` | FBL aggregate | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @VarType` | FBL aggregate | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @VarStage` | FBL aggregate | 6 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @VarDetails` | FBL aggregate | 13 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @PeriodAction` | FBL aggregate | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FBL.htm) |
| `SC @Period` | FBL | 12 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_FBL.htm) |
| `SC @Source` | FBL | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_FBL.htm) |
| `CC @ResultStatus` | Common | 9 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm) |
| `CC @SportGender` | Common | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm) |
| `CC @PersonGender` | Common | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm) |
| `CC @ResultsFunction` | FBL rows from DisciplineFunction | 22 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineFunction.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineFunction.htm) |
| `CC @VenueCode` | FBL venues inferred through FBL locations | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm) |
| `CC @Location` | FBL locations | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm) |
| `CC @Position` | FBL rows from Positions | 4 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Positions.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Positions.htm) |
| `CC @Competition` | Common | 10 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @Unit` | FBL EventUnit rows | 93 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @Country` | Common master data | 266 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm) |
| `CC @Organisation` | Common master data | 258 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |
| `CC @DisciplineClass` | Not found in Paris 2024 common-code index | 0 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm) |

### SC @Action
| Code | Note | ENG Description |
| --- | --- | --- |
| ADD |  | Addition time (minutes): |
| CRC |  | Coach Red Card |
| CRN |  | Corner |
| CYC |  | Coach Yellow Card |
| CYRC |  | Coach Second Yellow Card |
| ENDP |  | End of Period |
| FOUL |  | Foul |
| FRD |  | Direct free-kick attempt |
| OFF |  | Offside |
| OG |  | Own goal |
| PEN |  | Penalty attempt |
| RC |  | Red card |
| SHOT |  | Attempt at goal |
| STARTP |  | Start of Period |
| SUBST |  | Substitution |
| VAR |  | Video Assistant Referee Review |
| YC |  | Yellow card |
| YRC |  | Second yellow card and red card |

### SC @ResAction
| Code | Note | ENG Description |
| --- | --- | --- |
| 1 | For ADD | 1 |
| 10 | For ADD | 10 |
| 11 | For ADD | 11 |
| 12 | For ADD | 12 |
| 13 | For ADD | 13 |
| 14 | For ADD | 14 |
| 15 | For ADD | 15 |
| 2 | For ADD | 2 |
| 3 | For ADD | 3 |
| 4 | For ADD | 4 |
| 5 | For ADD | 5 |
| 6 | For ADD | 6 |
| 7 | For ADD | 7 |
| 8 | For ADD | 8 |
| 9 | For ADD | 9 |
| BLC |  | missed |
| CHANGED_GOAL_FOUL | for VAR | Goal disallowed - Foul by the attacking player |
| CHANGED_GOAL_HAND | for VAR | Goal disallowed - Handball by the attacking player |
| CHANGED_GOAL_LINE | for VAR | Goal disallowed - The ball did not cross the goal line |
| CHANGED_GOAL_OFF | for VAR | Goal disallowed - Offside |
| CHANGED_GOAL_OUT | for VAR | Goal disallowed - The ball was out of play |
| CHANGED_NOGOAL_FOUL | for VAR | Goal awarded - No foul by the attacking player |
| CHANGED_NOGOAL_HAND | for VAR | Goal awarded - No handball by the attacking player |
| CHANGED_NOGOAL_LINE | for VAR | Goal awarded - The ball crossed the goal line |
| CHANGED_NOGOAL_OFF | for VAR | Goal awarded - No offside |
| CHANGED_NOGOAL_OUT | for VAR | Goal awarded - The ball was not out of play |
| CHANGED_NOPTY_ATTFOUL | for VAR | Penalty awarded - No foul by the attacking player |
| CHANGED_NOPTY_ATTHAND | for VAR | Penalty awarded - No handball by the attacking player |
| CHANGED_NOPTY_FOUL | for VAR | Penalty awarded - Foul |
| CHANGED_NOPTY_FOULIN | for VAR | Penalty awarded - The foul was inside the penalty area |
| CHANGED_NOPTY_HAND | for VAR | Penalty awarded - Handball |
| CHANGED_NOPTY_HANDIN | for VAR | Penalty awarded - The handball was inside the penalty area |
| CHANGED_NOPTY_OFF | for VAR | Penalty awarded - No offside |
| CHANGED_NOPTY_OUT | for VAR | Penalty awarded - The ball was not out of play |
| CHANGED_NORC | for VAR | Red card given |
| CHANGED_PTY_ATTFOUL | for VAR | No penalty - Foul by the attacking player |
| CHANGED_PTY_ATTHAND | for VAR | No penalty - Handball by the attacking player |
| CHANGED_PTY_FOUL | for VAR | No penalty - No foul |
| CHANGED_PTY_FOULIN | for VAR | No penalty - The foul was outside the penalty area |
| CHANGED_PTY_HAND | for VAR | No penalty - No handball |
| CHANGED_PTY_HANDIN | for VAR | No penalty - The handball was outside the penalty area |
| CHANGED_PTY_OFF | for VAR | No penalty - Offside |
| CHANGED_PTY_OUT | for VAR | No penalty - The ball was out of play |
| CHANGED_PTYDIS_ATTFOUL | for VAR | Goal awarded - No encroachment by attacking player |
| CHANGED_PTYDIS_DEFFOUL | for VAR | No retake - No encroachment by defending player |
| CHANGED_PTYDIS_FOUL | for VAR | Goal awarded - No offence by kicker |
| CHANGED_PTYDIS_GKFOUL | for VAR | No retake – No offence by goalkeeper |
| CHANGED_PTYGOAL_ATTFOUL | for VAR | Goal disallowed - Encroachment by the attacking player. Penalty retake |
| CHANGED_PTYGOAL_FOUL | for VAR | Goal disallowed - Offence by the kicker. Penalty retake |
| CHANGED_PTYMISS_DEFFOUL | for VAR | Penalty to be retaken - Encroachment by the defending player |
| CHANGED_PTYMISS_GKFOUL | for VAR | Penalty to be retaken - Offence by the goalkeeper |
| CHANGED_RC_FOUL | for VAR | Rescinded red card - Foul given |
| CHANGED_RC_NOFOUL | for VAR | Rescinded red card - No infringement found |
| CHANGED_RC_YC | for VAR | Rescinded red card - Yellow card given |
| CHECK_GOAL_FOUL | for VAR | Checking for possible foul leading up to the goal |
| CHECK_GOAL_HAND | for VAR | Checking for possible handball leading up to the goal |
| CHECK_GOAL_LINE | for VAR | Checking if the ball crossed the goal line |
| CHECK_GOAL_OFF | for VAR | Checking for possible offside leading up to the goal |
| CHECK_GOAL_OUT | for VAR | Checking if the ball was out of play before the goal was scored |
| CHECK_MISTAKEN | for VAR | Checking for possible mistaken identity |
| CHECK_NOGOAL_FOUL | for VAR | Checking the foul by the attacking player |
| CHECK_NOGOAL_HAND | for VAR | Checking the handball by the attacking player |
| CHECK_NOGOAL_LINE | for VAR | Checking if the ball crossed the goal line |
| CHECK_NOGOAL_OFF | for VAR | Checking the offside |
| CHECK_NOGOAL_OUT | for VAR | Checking if the ball was out of play |
| CHECK_NOPTY_ATTFOUL | for VAR | Checking foul leading up to the penalty situation |
| CHECK_NOPTY_ATTHAND | for VAR | Checking handball leading up to the penalty situation |
| CHECK_NOPTY_FOUL | for VAR | Checking for possible penalty - Potential foul |
| CHECK_NOPTY_FOULIN | for VAR | Checking for possible penalty - Checking if the foul was inside the penalty area |
| CHECK_NOPTY_HAND | for VAR | Checking for possible penalty - Potential handball |
| CHECK_NOPTY_HANDIN | for VAR | Checking for possible penalty - Checking if the handball was inside the penalty |
| CHECK_NOPTY_OFF | for VAR | Checking offside leading up to the penalty situation |
| CHECK_NOPTY_OUT | for VAR | Checking ball out of play leading up to the penalty situation |
| CHECK_NORC | for VAR | Checking for possible red card |
| CHECK_PTY_ATTFOUL | for VAR | Checking for possible foul leading up to the penalty |
| CHECK_PTY_ATTHAND | for VAR | Checking for possible handball leading up to the penalty |
| CHECK_PTY_FOUL | for VAR | Checking the foul |
| CHECK_PTY_FOULIN | for VAR | Checking if the foul was inside the penalty area |
| CHECK_PTY_HAND | for VAR | Checking the handball |
| CHECK_PTY_HANDIN | for VAR | Checking if the handball was inside the penalty area |
| CHECK_PTY_OFF | for VAR | Checking for possible offside leading up to the penalty |
| CHECK_PTY_OUT | for VAR | Checking if the ball was out of play before the penalty was awarded |
| CHECK_PTYDIS_ATTFOUL | for VAR | Checking encroachment by attacking player |
| CHECK_PTYDIS_DEFFOUL | for VAR | Checking encroachment by defending player |
| CHECK_PTYDIS_FOUL | for VAR | Checking offence by kicker |
| CHECK_PTYDIS_GKFOUL | for VAR | Checking offence by goalkeeper |
| CHECK_PTYGOAL_ATTFOUL | for VAR | Checking for possible encroachment by the attacking player |
| CHECK_PTYGOAL_FOUL | for VAR | Checking for possible offence by the kicker |
| CHECK_PTYMISS_DEFFOUL | for VAR | Checking for possible encroachment by the defender |
| CHECK_PTYMISS_GKFOUL | for VAR | Checking for possible offence by the goalkeeper |
| CHECK_RC | for VAR | Checking the red card |
| CONCUSSION |  | Concussion |
| CONFIRMED_GOAL_FOUL | for VAR | Goal confirmed - No foul by the attacking player |
| CONFIRMED_GOAL_HAND | for VAR | Goal confirmed - No handball by the attacking player |
| CONFIRMED_GOAL_LINE | for VAR | Goal confirmed - The ball crossed the goal line |
| CONFIRMED_GOAL_OFF | for VAR | Goal confirmed - No offside |
| CONFIRMED_GOAL_OUT | for VAR | Goal confirmed - The ball was not out of play |
| CONFIRMED_NOGOAL_FOUL | for VAR | No goal - Foul |
| CONFIRMED_NOGOAL_HAND | for VAR | No goal - Handball |
| CONFIRMED_NOGOAL_LINE | for VAR | No goal - Ball did not cross the goal line |
| CONFIRMED_NOGOAL_OFF | for VAR | No goal - Offside |
| CONFIRMED_NOGOAL_OUT | for VAR | No goal - Ball out of play |
| CONFIRMED_NOPTY_ATTFOUL | for VAR | No penalty - Foul by the attacking player |
| CONFIRMED_NOPTY_ATTHAND | for VAR | No penalty - Handball by the attacking player |
| CONFIRMED_NOPTY_FOUL | for VAR | No penalty - No foul |
| CONFIRMED_NOPTY_FOULIN | for VAR | No penalty - The foul was outside the penalty area |
| CONFIRMED_NOPTY_HAND | for VAR | No penalty - No handball |
| CONFIRMED_NOPTY_HANDIN | for VAR | No penalty - The handball was outside the penalty area |
| CONFIRMED_NOPTY_OFF | for VAR | No penalty - Offside |
| CONFIRMED_NOPTY_OUT | for VAR | No penalty - The ball was out of play |
| CONFIRMED_NORC_FOUL | for VAR | Foul given |
| CONFIRMED_NORC_NOFOUL | for VAR | No infringement found |
| CONFIRMED_NORC_YC | for VAR | Yellow card given |
| CONFIRMED_PTY_ATTFOUL | for VAR | Penalty confirmed - No foul by the attacking player |
| CONFIRMED_PTY_ATTHAND | for VAR | Penalty confirmed - No handball by the attacking player |
| CONFIRMED_PTY_FOUL | for VAR | Penalty confirmed - Foul |
| CONFIRMED_PTY_FOULIN | for VAR | Penalty confirmed - The foul was inside the penalty area |
| CONFIRMED_PTY_HAND | for VAR | Penalty confirmed - Handball |
| CONFIRMED_PTY_HANDIN | for VAR | Penalty confirmed - The handball was inside the penalty area |
| CONFIRMED_PTY_OFF | for VAR | Penalty confirmed - No offside |
| CONFIRMED_PTY_OUT | for VAR | Penalty confirmed - The ball was not out of play |
| CONFIRMED_PTYDIS_ATTFOUL | for VAR | No Goal - Encroachment by attacking player - Penalty to be retaken |
| CONFIRMED_PTYDIS_DEFFOUL | for VAR | Encroachment by defending player - Penalty to be retaken |
| CONFIRMED_PTYDIS_FOUL | for VAR | No Goal – Offence by kicker - Penalty to be retaken |
| CONFIRMED_PTYDIS_GKFOUL | for VAR | Offence by goalkeeper - Penalty to be retaken |
| CONFIRMED_PTYGOAL_ATTFOUL | for VAR | Goal confirmed - No encroachment by the attacking player |
| CONFIRMED_PTYGOAL_FOUL | for VAR | Goal confirmed - No offence by the kicker |
| CONFIRMED_PTYMISS_DEFFOUL | for VAR | Referee decision confirmed - No encroachment by the defender |
| CONFIRMED_PTYMISS_GKFOUL | for VAR | Referee decision confirmed - No offence by the goalkeeper |
| CONFIRMED_RC | for VAR | Red card confirmed |
| FIELD | for VAR | On-Field Review |
| FIELD_GOAL_FOUL | for VAR | Goal under review - Possible foul by attacking player |
| FIELD_GOAL_HAND | for VAR | Goal under review - Possible handball by attacking player |
| FIELD_GOAL_LINE | for VAR | Goal under review - In or out |
| FIELD_GOAL_OFF | for VAR | Goal under review - Possible offside |
| FIELD_GOAL_OUT | for VAR | Goal under review - Possible ball out of play |
| FIELD_MISTAKEN | for VAR | Mistaken identity under review |
| FIELD_NOGOAL_FOUL | for VAR | Goal under review - Foul by attacking player |
| FIELD_NOGOAL_HAND | for VAR | Goal under review - Handball by attacking player |
| FIELD_NOGOAL_LINE | for VAR | Goal under review - In or out |
| FIELD_NOGOAL_OFF | for VAR | Goal under review - Offside |
| FIELD_NOGOAL_OUT | for VAR | Goal under review - Ball out of play |
| FIELD_NOPTY_ATTFOUL | for VAR | Possible penalty also possible foul by attacking player |
| FIELD_NOPTY_ATTHAND | for VAR | Possible penalty also possible handball by attacking player |
| FIELD_NOPTY_FOUL | for VAR | Possible penalty - Foul |
| FIELD_NOPTY_FOULIN | for VAR | Possible penalty - Foul inside or outside the penalty area |
| FIELD_NOPTY_HAND | for VAR | Possible penalty – Handball |
| FIELD_NOPTY_HANDIN | for VAR | Possible penalty - Handball inside or outside the penalty area |
| FIELD_NOPTY_OFF | for VAR | Possible penalty also possible offside by attacking player |
| FIELD_NOPTY_OUT | for VAR | Possible penalty also possible ball out of play |
| FIELD_NORC | for VAR | Red card under review - Potential red card |
| FIELD_PTY_ATTFOUL | for VAR | Penalty under review - Possible foul by attacking player |
| FIELD_PTY_ATTHAND | for VAR | Penalty under review - Possible handball by attacking player |
| FIELD_PTY_FOUL | for VAR | Penalty under review – Foul |
| FIELD_PTY_FOULIN | for VAR | Penalty under review - Foul inside or outside the penalty area |
| FIELD_PTY_HAND | for VAR | Penalty under review – Handball |
| FIELD_PTY_HANDIN | for VAR | Penalty under review - Handball inside or outside the penalty area |
| FIELD_PTY_OFF | for VAR | Penalty under review - Possible offside |
| FIELD_PTY_OUT | for VAR | Penalty under review - Possible ball out of play |
| FIELD_PTYDIS_ATTFOUL | for VAR | Goal under review - Encroachment by attacking player |
| FIELD_PTYDIS_DEFFOUL | for VAR | Penalty under review - Encroachment by defending player |
| FIELD_PTYDIS_FOUL | for VAR | Goal under review - Offence by kicker |
| FIELD_PTYDIS_GKFOUL | for VAR | Penalty under review – Offence by goalkeeper |
| FIELD_PTYGOAL_ATTFOUL | for VAR | Goal under review - Possible encroachment by attacking player |
| FIELD_PTYGOAL_FOUL | for VAR | Goal under review - Possible offence by kicker |
| FIELD_PTYMISS_DEFFOUL | for VAR | Penalty under review – Possible encroachment by defending player |
| FIELD_PTYMISS_GKFOUL | for VAR | Penalty under review – Possible offence by goalkeeper |
| FIELD_RC | for VAR | Red card under review |
| GOAL |  | GOAL |
| MISS |  | missed |
| OVER | for VAR | Check over |
| POST |  | missed |
| SAVE |  | saved by the goalkeeper |
| VARCHECK | for VAR | VAR Checking |

### SC @ActionRole
| Code | Note | ENG Description |
| --- | --- | --- |
| ASSIST |  | Assist |
| FOC |  | Foul committed |
| FOS |  | Foul suffered |
| IN |  | In |
| OUT |  | Out |
| SCR |  |  |

### SC @ActionComment
| Code | Order | ENG Description |
| --- | --- | --- |
| STARTP_H1 | 1 | Start of match |
| ENDP_H1 | 2 | End of first half |
| STARTP_H2 | 3 | Start of second half |
| ENDP_H2 | 4 | End of second half |
| STARTP_ET-H1 | 5 | Start of first half of extra time |
| ENDP_ET-H1 | 6 | End of first half of extra time |
| STARTP_ET-H2 | 7 | Start of second half of extra time |
| ENDP_ET-H2 | 8 | End of second half of extra time |
| STARTP_PSO | 9 | Start of penalty shoot-out |
| ENDP_MATCH | 10 | End of match |

### SC @VarType
| Code | ENG Description |
| --- | --- |
| GOAL | Checking Goal |
| MISTAKEN | Mistaken Identity |
| NOGOAL | Checking No Goal |
| NOPTY | Possible Penalty |
| NORC | Possible Red Card |
| PTY | Checking Penalty |
| PTYDIS | Penalty kick disallowed |
| PTYGOAL | Checking Goal (After Penalty Kick Goal) |
| PTYMISS | Checking Penalty (After Penalty Kick Missed) |
| RC | Checking Red Card |

### SC @VarStage
| Code | ENG Description |
| --- | --- |
| CHANGED | Change of Decision |
| CHECK | Checking |
| CONFIRMED | VAR Confirmed |
| FIELD | On-Field Review |
| OVER | VAR Checking |
| VARCHECK | VAR Checking |

### SC @VarDetails
| Code | ENG Description |
| --- | --- |
| ATTFOUL | Foul/Encroachment by the attacking player |
| ATTHAND | Handball by the attacking player |
| DEFFOUL | Encroachment/Offense by Defending Player |
| FOUL | Foul |
| FOULIN | Foul Inside the Penalty Area |
| GKFOUL | Foul by goalkeeper |
| HAND | Handball |
| HANDIN | Handball Inside the Penalty Area |
| LINE | Ball crossed the line |
| NOFOUL | No Infringement Found |
| OFF | Offside |
| OUT | Ball Out of Play |
| YC | Yellow Card Given |

### SC @PeriodAction
| Code | ENG Description |
| --- | --- |
| ET-HT | Extra Time Half Time |
| FT | Full Time |
| HT | Half-Time |
| PET | Pre extra time |
| PSO | Penalty Shoot Out |

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

### CC @ResultStatus
| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| START_LIST | 1 | Before competition, Start List | Start List |
| LIVE | 2 | For live updates during competition | Live |
| INTERMEDIATE | 3 | When competition is stopped, usued at pre-defined points | Intermediate |
| UNCONFIRMED | 4 | When the unit is over but not yet unofficial or official | Unconfirmed |
| UNOFFICIAL | 5 | Results of the competition released as soon as the event is over, not waiting any official decision of the International Federation. The correctness of data must be assured | Unofficial |
| OFFICIAL | 6 | Results of the competition released as soon as the event is officially confirmed taking into account the resolution of the protests, etc. | Official |
| PARTIAL | 7 | Incomplete list, Final Ranking. Used in PDF | Partial |
| PROTESTED | 8 | After the competition is no longer LIVE and a protest has been lodged | Protested |
| PROVISIONAL | 9 | Special situations | Provisional |

### CC @SportGender
| Id | Description |
| --- | --- |
| - | Global |
| M | Men |
| O | Open |
| W | Women |
| X | Mixed |

### CC @PersonGender
| Id | ENG Description |
| --- | --- |
| F | Female |
| M | Male |
| X | Unspecified |

### CC @ResultsFunction (FBL rows)
| Function | Order | Category | Partic | ENG Description |
| --- | --- | --- | --- | --- |
| AA01 | 0 | A | Y | Athlete |
| AP01 | 1 | A | Y | Alternate Athlete |
| COACH | 2 | C | Y | Head Coach |
| SI_COA | 3 | C | Y | Stand-in Coach |
| AST_COA | 4 | C | Y | Assistant Coach |
| GK_COA | 5 | C | Y | Goalkeeper Coach |
| TM_OFFIC | 6 | T | Y | Team Official |
| DOCTOR | 7 | T | Y | Doctor |
| RE | 8 | J | Y | Referee |
| AR | 9 | J | Y | Assistant Referee |
| AR1 | 10 | J | Y | Assistant Referee 1 |
| AR2 | 11 | J | Y | Assistant Referee 2 |
| 4O | 12 | S | Y | 4th Official |
| RAR | 13 | J | Y | Reserve Assistant Referee |
| MCS | 14 | S | Y | Match Commissioner |
| MCH_DIR | 15 | S | Y | Match Director |
| VMO | 16 | J | Y | Video Match Official |
| VAR | 17 | J | Y | Video Assistant Referee |
| AVAR | 18 | J | Y | Assistant VAR (AVAR) |
| AVAR1 | 19 | J | Y | Assistant VAR 1 |
| AVAR2 | 20 | J | Y | Assistant VAR 2 |
| AVAR3 | 21 | J | Y | Assistant VAR 3 |

### CC @Position (FBL rows)
| Id | PositionOrder | ENG Description |
| --- | --- | --- |
| DF |  | Defender |
| FW |  | Forward |
| GK |  | Goalkeeper |
| MF |  | Midfielder |

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

### CC @Location (FBL rows)
| Id | Venue | ENG longdescription | ENG Description |
| --- | --- | --- | --- |
| BOR | BOR | Bordeaux Stadium, Bordeaux | Bordeaux Stadium, Bordeaux |
| LYO | LYO | Lyon Stadium, Lyon | Lyon Stadium, Lyon |
| MRS | MRS | Marseille Stadium, Marseille | Marseille Stadium, Marseille |
| NAN | NAN | La Beaujoire Stadium, Nantes | La Beaujoire Stadium, Nantes |
| NIC | NIC | Nice Stadium, Nice | Nice Stadium, Nice |
| PDP | PDP | Parc des Princes, Paris | Parc des Princes, Paris |
| STE | STE | Geoffroy-Guichard Stadium, Saint-Etienne | Geoffroy-Guichard, St-Etienne |

### CC @VenueCode (FBL venues inferred from FBL locations)
| Id | ENG longdescription | ENG Description |
| --- | --- | --- |
| BOR | Bordeaux Stadium | Bordeaux Stadium |
| LYO | Lyon Stadium | Lyon Stadium |
| MRS | Marseille Stadium | Marseille Stadium |
| NAN | La Beaujoire Stadium | La Beaujoire Stadium |
| NIC | Nice Stadium | Nice Stadium |
| PDP | Parc des Princes | Parc des Princes |
| STE | Geoffroy-Guichard Stadium | Geoffroy-Guichard Stadium |

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

### CC @Country and CC @Organisation

These are large common master-data tables rather than football play-by-play enumerations. The downloaded Paris 2024 code pages contain 266 `Country` rows and 258 `Organisation` rows. Use the source links in the index above as the authoritative value lists when modeling `Nationality` and `Organisation` fields.

### CC @DisciplineClass

The football dictionary references `CC @DisciplineClass` for para sport classification, but the Paris 2024 common-code index downloaded for this appendix does not expose a `DisciplineClass.htm` page. Treat this as an externally supplied classification code set and validate it against the relevant para-sport dictionary/code release when that scope is implemented.

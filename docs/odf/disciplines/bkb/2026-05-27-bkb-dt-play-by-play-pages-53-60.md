# ODF BKB Data Dictionary: DT_PLAY_BY_PLAY, Pages 53-60

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 53-60.
Source version: `SOG-2024-BKB-3.4 APP`, dated 19 January 2024.

This note restructures the basketball `DT_PLAY_BY_PLAY` section into readable Markdown for domain modeling. It covers the Play by Play message, header metadata, period-scoped vs full-match behavior, the message tree, action fields, the action-specific `ExtendedAction` blocks (free throws, shot flag, shot sector, shot type), action participants, athlete and coach payloads, an XML example, and an appendix of English SC/CC values downloaded from the Paris 2024 ODF code pages.

## 2.3.6 Play by Play

`DT_PLAY_BY_PLAY` carries the official raw action stream from the result provider. In basketball this is the per-action timeline of a match: period starts/ends, shot attempts (2 points, 3 points, fast-break variants), free throws, fouls, blocks, rebounds, steals, turnovers, substitutions, timeouts, jump balls, and challenges.

The envelope is generic enough to transport result-provider actions of different natures, but the basketball dictionary constrains it through sport-specific action, result, period, role, free-throw, shot-flag, and sector codes.

## Header Values

| Attribute | Value | Meaning |
|---|---:|---|
| `CompetitionCode` | `CC @Competition` | Unique competition identifier. |
| `DocumentCode` | `CC @Unit` | Full RSC of the unit (match). |
| `DocumentSubcode` | `SC @Period` or omitted | Period code when the message is for one period only. Use full-period codes or `OTn` for each overtime. Period messages include all applicable actions for the period. If `DocumentSubcode` is omitted, the message contains the full match. The dictionary text mentions `Q1`, `Q2`, `Q3`, `Q4`, and `OTn`; the basketball code table also includes half-quarter codes (`Q1_H`, `Q2_H`, `Q3_H`, `Q4_H`) and the `OT` aggregate code. |
| `DocumentType` | `DT_PLAY_BY_PLAY` | Play by Play message. |
| `DocumentSubtype` | `S(8)` | Send `ACTION`. |
| `Version` | `1..V` | Ascending version number for the message content. |
| `ResultStatus` | `CC @ResultStatus` | Message status. Basketball uses `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, and `PROVISIONAL`. |
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
| After each period | Send the full play-by-play. Use `INTERMEDIATE` except after the final period, which is `UNOFFICIAL`. |
| Previous-period correction | Send the full play-by-play if the corrected period is no longer running. |
| Match becomes official | Send the full play-by-play with all periods and `ResultStatus="OFFICIAL"`. |

The full play-by-play message, meaning the one without `DocumentSubcode`, is the master. When it arrives, consumers should remove all stored play-by-play data for that unit and replace it with the incoming action list. The intended pattern is to send it before the first period and at the end of each period and after the game, but it may arrive at other times to fix earlier periods.

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
         ├─ @ActionDesc
         ├─ @Comment
         ├─ @When
         ├─ @Result
         ├─ @ScoreH
         ├─ @ScoreA
         ├─ @LeadH
         ├─ @LeadA
         ├─ @X
         ├─ @Y
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
                  └─ Description (1,1)
                     ├─ @GivenName
                     ├─ @FamilyName
                     ├─ @Gender
                     └─ @Nationality
```

## Competition

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Gen` | Optional | `S(20)` | General Data Dictionary version applicable to the message. |
| `Sport` | Optional | `S(20)` | Sport Data Dictionary version applicable to the message. |
| `Codes` | Optional | `S(20)` | Code-set version applicable to the message. |

## ExtendedInfos / ExtendedInfo

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `UI` | `OT` | Numeric `0`; positions `1..n` | Send for every overtime started or played in the game. | `SC @Period` | Applicable overtime period code. |

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
| `Order` | Mandatory | Numeric | Unique ascending sequence across all incidents and actions in all periods. Used to sort `Action`. The same action keeps the same order whether sent in a period message or in a full-match message. |
| `Action` | Optional | `SC @Action` | Game action. Send one action code. |
| `ActionAdd` | Optional | `S(200)` | Additional information related to the action. |
| `ActionDesc` | Optional | `S(200)` | Action/incident description in English. |
| `Comment` | Optional | `SC @ActionComment` | Included when `Action` is `STARTP` or `ENDP`. |
| `When` | Optional | `mm:ss` | Time in minutes and seconds at which the action occurred. Example: `2:05`. Do not send leading zeros in minutes over one minute. |
| `Result` | Optional | `SC @ResAction` | Result of the action for the player/team. |
| `ScoreH` | Optional | Numeric `##0` | Total home score after the action. Send when there is a score change for either team. |
| `ScoreA` | Optional | Numeric `##0` | Total away score after the action. Send when there is a score change for either team. |
| `LeadH` | Optional | Numeric `#0` or `-#0` | Home team point lead after a score change for either team. May be negative. |
| `LeadA` | Optional | Numeric `#0` or `-#0` | Away team point lead after a score change for either team. May be negative. |
| `X` | Optional | Numeric `##0` | X coordinate of the action location, in percent relative to `(0,0)` on the background image. |
| `Y` | Optional | Numeric `##0` | Y coordinate of the action location, in percent relative to `(0,0)` on the background image. |
| `TimeStamp` | Optional | `DateTime` | Time of the action, used for alignment to video. |

## Action / ExtendedAction

`ExtendedAction` is repeatable and carries action-specific metadata. The dictionary defines the following slots for basketball.

| Type | Code | Pos | Expected When | Value | Meaning |
|---|---|---|---|---|---|
| `FREETHROWS` | N/A | N/A | When available, for `FT`, `FOUL`, or `CFOUL`. | `SC @FreeThrow` | For `FOUL` and `CFOUL`: number of free throws awarded. For `FT`: free-throw sequence number. |
| `SHOT_FLAG` | N/A | N/A | When available, for `FT` or `P2*/P3*`. | `SC @ShotFlag` | Shot flag (fast break, second chance, after turnover, combinations). |
| `SHOT_POS` | N/A | N/A | When available, for `P2*/P3*`. | `SC @Sector` | Shot sector on the court. |
| `SHOT_TYPE` | N/A | N/A | When applicable. | `SC @FreeThrowOf` or `SC @Res_Sub` | Free-throw total context (e.g., `of 2`) or shot subtype (lay-up, dunk, jump shot, alley-oop, etc.). |
| `ADD2` | N/A | N/A | When applicable. | String | Additional information for a related action (for example, assist after a made shot, turnover after an offensive foul). |
| `DESC2` | N/A | N/A | When applicable. | String | Additional description for a related action. |

## Action / Competitor

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Competitor ID. |
| `Type` | Mandatory | `S(1)` | `T` for team. |
| `Order` | Optional | Numeric | Order in which the competitor should appear for the action when more than one competitor participates. |
| `Organisation` | Mandatory | `CC @Organisation` | Competitor organisation. |

## Competitor / Composition / Athlete

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `Code` | Mandatory | `S(20)` with no leading zeroes | Athlete or team-member ID related to the action. |
| `Order` | Optional | Numeric | Athlete order when more than one athlete is related to the action. |
| `Bib` | Optional | `S(2)` | Shirt number. |
| `Role` | Optional | `SC @ActionRole` | Role of the competitor in the action. |

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

## Coach / Description

| Attribute | M/O | Value | Meaning |
|---|---|---:|---|
| `GivenName` | Optional | `S(25)` | Given name in WNPA mixed-case format. |
| `FamilyName` | Mandatory | `S(25)` | Family name in WNPA mixed-case format. |
| `Gender` | Mandatory | `CC @PersonGender` | Official gender. |
| `Nationality` | Mandatory | `CC @Country` | Coach nationality. |

## Sample from the Dictionary, Normalized

The source sample shows a missed 2-points outside-paint shot for one athlete on one team. The sample text includes a `Competitor/Description` element with `TeamName`, but the message tree on pages 54-56 does not declare a `Description` element under `Action/Competitor`. The XML example below follows the documented structure (no `Action/Competitor/Description`).

```xml
<Action Id="123456" Period="Q1" Order="3" Action="P2O" When="2:14" Result="MISS">
  <Competitor Code="BKBWTEAM5-----RSA01" Type="T" Organisation="RSA" Order="1">
    <Composition>
      <Athlete Code="1106655" Order="1">
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

## XML Example

This message is not validated because no XSD was supplied with the task. The example shows the dictionary-declared structure for a period-scoped message containing a start-of-period marker, a successful 3-points shot with an assist, and a personal foul drawing two free throws.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OdfBody CompetitionCode="OG2024"
         DocumentCode="BKBMTEAM12------------GPA-000100--"
         DocumentSubcode="Q1"
         DocumentType="DT_PLAY_BY_PLAY"
         DocumentSubtype="ACTION"
         Version="3"
         ResultStatus="LIVE"
         FeedFlag="T"
         Date="2026-05-27"
         Time="20:15:42.000"
         LogicalDate="2026-05-27"
         Source="BCYBKB1">
  <Competition Gen="3.4" Sport="BKB-3.4" Codes="SOG-2024">
    <ExtendedInfos>
      <SportDescription DisciplineName="Basketball"
                        EventName="Men"
                        SubEventName="Men's Group A"
                        Gender="M"
                        UnitNum="1"/>
      <VenueDescription Venue="BCY"
                        VenueName="Bercy Arena"
                        Location="BCY"
                        LocationName="Bercy Arena, Paris"/>
    </ExtendedInfos>
    <Actions Home="BKBMTEAM12-----FRA01" Away="BKBMTEAM12-----USA01">
      <Action Id="100001"
              Period="Q1"
              Order="1"
              Action="STARTP"
              Comment="STARTP_Q1"
              Result="GAME"
              When="0:00"
              TimeStamp="2026-05-27T20:15:00"/>
      <Action Id="100002"
              Period="Q1"
              Order="2"
              Action="P3"
              When="1:42"
              Result="MADE"
              ScoreH="3"
              ScoreA="0"
              LeadH="3"
              LeadA="-3"
              X="22"
              Y="78"
              TimeStamp="2026-05-27T20:16:42">
        <ExtendedAction Code="SHOT_POS" Value="LW"/>
        <ExtendedAction Code="SHOT_TYPE" Value="JS"/>
        <ExtendedAction Code="SHOT_FLAG" Value="FB"/>
        <Competitor Code="BKBMTEAM12-----FRA01" Type="T" Order="1" Organisation="FRA">
          <Composition>
            <Athlete Code="1234567" Order="1" Bib="7" Role="SCR">
              <Description GivenName="Paul" FamilyName="Martin" Gender="M" Organisation="FRA" BirthDate="1998-04-11"/>
            </Athlete>
            <Athlete Code="1234568" Order="2" Bib="11" Role="ASSIST">
              <Description GivenName="Luc" FamilyName="Bernard" Gender="M" Organisation="FRA" BirthDate="1996-09-02"/>
            </Athlete>
          </Composition>
        </Competitor>
      </Action>
      <Action Id="100003"
              Period="Q1"
              Order="3"
              Action="FOUL"
              When="2:14"
              Result="PF"
              TimeStamp="2026-05-27T20:17:14">
        <ExtendedAction Code="FREETHROWS" Value="2"/>
        <Competitor Code="BKBMTEAM12-----USA01" Type="T" Order="1" Organisation="USA">
          <Composition>
            <Athlete Code="2345678" Order="1" Bib="23" Role="SCR">
              <Description GivenName="John" FamilyName="Doe" Gender="M" Organisation="USA" BirthDate="1997-12-30"/>
            </Athlete>
          </Composition>
        </Competitor>
      </Action>
    </Actions>
  </Competition>
</OdfBody>
```

Note: this example is not validated because no XSD was supplied. `ExtendedAction` is shown with the `@Code` and `@Value` attribute pair the dictionary tree declares; the dictionary's `ExtendedAction` row also lists a `@Pos` attribute but does not document a use case for it under play-by-play.

## Modeling Notes

- Model period-scoped updates and full-match replacement as different ingestion modes. A period message patches/replaces the running period only; a full-match message is the master and replaces the entire action stream for the unit.
- Preserve `Id` and `Order` as stable provider identifiers across both modes. `Id` identifies the action; `Order` is the canonical event-stream ordering across all periods. The same action keeps the same `Id` and `Order` whether sent inside a period message or inside a full-match message.
- `PId` is the link for related actions or corrections, used to attach VAR/challenge follow-ups, assists tied to a made shot, free throws tied to a foul, or substitutions related to a stoppage. Map it to an explicit relation in the domain model, not just a string copied to output.
- Action semantics are path-dependent. `Action="P3"` plus `Result="MADE"` describes a successful 3-points attempt; the same `Result="MADE"` value under `Action="FT"` is a made free throw, and `Result="MADE"` under `Action="FD"` is not applicable. Codes in `SC @ResAction` are reused across many actions, so the meaning is the `(Action, Result)` pair, not the result alone.
- Action ordering is by `@Order`, ascending across all periods. The first action of a period should be `STARTP` with a `Comment` such as `STARTP_Q1`, and the final action of a period is `ENDP` with the matching `ENDP_Qn` or `ENDP_OTn` comment. Use these markers to delimit period boundaries instead of trusting the `When` clock alone.
- The `When` value is wall-clock match time `mm:ss`, not a duration since tip-off. Basketball game clocks count down inside each quarter, so `When` should be interpreted as the displayed clock value at the action; combine with `Period` to derive a global ordinal time if needed. `TimeStamp` is the real-world `DateTime` for video alignment and should be the source of truth when synchronising with broadcast feeds.
- Shot data is split across multiple slots: `Action` carries the shot family (`P2I`, `P2O`, `P2F`, `P3`, `P3F`, `FT`), `Result` carries the outcome (`MADE`, `MISS`, `BLC`), `ExtendedAction` `SHOT_POS` carries the court sector (`SC @Sector`), `SHOT_TYPE` carries the shot subtype (`SC @Res_Sub`: lay-up, dunk, jump shot, etc.), and `SHOT_FLAG` carries situational tags (`SC @ShotFlag`: fast break, second chance, after turnover, and combinations). Treat these as separate dimensions of one shot event in the model.
- Foul actions need free-throw context. `FOUL` and `CFOUL` carry `ExtendedAction` `FREETHROWS` with `SC @FreeThrow` to indicate how many free throws are awarded. Each individual free throw is then a separate `Action="FT"` row with its own outcome.
- Substitution actions require two athletes under the same `Competitor/Composition`: one with `Role="OUT"` and one with `Role="IN"`. Jump balls use `Role="JBW"` and `Role="JBL"`. Assists are carried as a second athlete on the scorer's action with `Role="ASSIST"`. Avoid modeling every play-by-play row as exactly one player event.
- `ScoreH`, `ScoreA`, `LeadH`, and `LeadA` are post-action snapshots, only sent when the score changes. Reconstruct full scoreboard timelines by carrying forward the last seen value across actions that omit them.
- `X` and `Y` are percentages relative to the background image's `(0,0)` origin. Store them as percentages and resolve to pixel/court coordinates only at presentation time; the reference image differs per result provider and per court layout.
- `ActionDesc` and `ExtendedAction/Value` (`ADD2`, `DESC2`) contain free-text supplemental information in English. Keep them as opaque strings rather than parsing them into structured fields.
- The `Coach` element under `Action/Competitor/Coaches` is rarely used in play-by-play. It only appears for coach-related actions (coach foul `CFOUL`, technical foul on the bench, coach challenge). The basketball coach payload lacks an `@Order` attribute in the dictionary tree, unlike the athlete payload.
- The dictionary sample includes a `Competitor/Description` element with `TeamName`, but no such element is declared in the message tree. Do not rely on this element on the wire; resolve team names from participant/team messages or the `SportDescription` block instead.

## Code Appendix: SC and CC Values

`SC` means sport-specific code and `CC` means common code in the ODF code-site links. Values below were downloaded from the Paris 2024 Olympic Data Feed code pages on 2026-05-27. Tables keep the English descriptions because this document models the English ODF vocabulary.

### Source Index

| Reference | Scope embedded here | Rows | Source |
|---|---:|---:|---|
| `SC @Action` | BKB | 22 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Action_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Action_SOG_BKB.htm) |
| `SC @ResAction` | BKB | 32 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResAction_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ResAction_SOG_BKB.htm) |
| `SC @Period` | BKB | 17 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Period_SOG_BKB.htm) |
| `SC @ActionRole` | BKB | 6 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ActionRole_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ActionRole_SOG_BKB.htm) |
| `SC @FreeThrow` | BKB | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FreeThrow_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_FreeThrow_SOG_BKB.htm) |
| `SC @Sector` | BKB | 8 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Sector_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Sector_SOG_BKB.htm) |
| `SC @ShotFlag` | BKB | 7 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ShotFlag_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_ShotFlag_SOG_BKB.htm) |
| `SC @ActionComment` | BKB aggregate | 18 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm) |
| `SC @FreeThrowOf` | BKB aggregate | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm) |
| `SC @Res_Sub` | BKB aggregate | 15 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_BKB.htm) |
| `SC @Source` | BKB | 2 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_sc/odf_sc_Source_SOG_BKB.htm) |
| `CC @ResultStatus` | Common | 9 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/ResultStatus.htm) |
| `CC @SportGender` | Common | 5 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/SportGender.htm) |
| `CC @PersonGender` | Common | 3 | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/PersonGender.htm) |
| `CC @Competition` | Common | linked only | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/CompetitionCode.htm) |
| `CC @Unit` | BKB EventUnit rows | linked only | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/EventUnit.htm) |
| `CC @VenueCode` | Common master data | linked only | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Venue.htm) |
| `CC @Location` | Common master data | linked only | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Location.htm) |
| `CC @Country` | Common master data | linked only | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Country.htm) |
| `CC @Organisation` | Common master data | linked only | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/Organisation.htm) |
| `CC @DisciplineClass` | Common (not present in Paris 2024 common-code index) | linked only | [https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm](https://odf.olympictech.org/2024-Paris/codes/HTML/og_cc/DisciplineClass.htm) |

### SC @Action

| Code | Note | ENG Description |
| --- | --- | --- |
| BLC |  | Blocked shot |
| CFOUL |  | Coach foul |
| CLG |  | Challenge |
| ENDP |  | End of Period |
| FD |  | Foul drawn |
| FOUL |  | Foul |
| FT |  | Free throw |
| JB |  | Jump ball |
| JBT |  | Alternating possession gained |
| P2F |  | 2 pts shot fast break |
| P2I |  | 2 pts shot inside paint |
| P2O |  | 2 pts shot outside paint |
| P3 |  | 3 pts shot |
| P3F |  | 3 pts shot fast break |
| REB |  | Rebound |
| ST |  | Steal |
| STARTP |  | Start of Period |
| SUBST |  | Substitution |
| TO |  | Turnover |
| TOUT |  | Timeout |
| TREB |  | Team rebound |
| TTO |  | Team turnover |

### SC @ResAction

| Code | Note | ENG Description |
| --- | --- | --- |
| BLC |  | Blocked |
| BP | for Turnover | Bad pass |
| CA | for Turnover | Carrying/Palming |
| DD | for Turnover | Double dribble |
| DQB |  | Disqualifying foul bench |
| DQF |  | Disqualifying |
| DR |  | Defensive (rebound) |
| GAME |  | Start of game (for start of period) and end of game (for end of period) |
| LB | for Turnover | Ball handling |
| MADE |  | Made |
| MISS |  | Missed |
| NOS | for Challenge | Lost |
| OB | for Turnover | Out of bounds |
| OF |  | Offensive (foul) |
| OG | for Turnover | Offensive goaltending |
| OR |  | Offensive (rebound) |
| PF |  | Personal |
| SSL | for Challenge | Won |
| T24 | for Team Turnover | 24 seconds |
| T3 | for Turnover | 3 seconds |
| T5 | for Turnover | 5 seconds |
| T8 | for Team Turnover | 8 seconds |
| TF |  | Technical |
| TF2 |  | Technical (2nd, disqualified) |
| TFB |  | Technical foul bench |
| TI |  | Throw-in foul |
| TR | for Turnover | Travelling |
| TU |  | Unsportsmanlike (previous technical foul, disqualified) |
| UF |  | Unsportsmanlike |
| UF2 |  | Unsportsmanlike (2nd, disqualified) |
| UT |  | Technical (previous unsportsmanlike foul, disqualified) |
| VI | for Turnover | Backcourt violation |

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

### SC @ActionRole

| Code | ENG Description |
| --- | --- |
| ASSIST | Assist |
| IN | In |
| JBL | Jump ball lost |
| JBW | Jump ball won |
| OUT | Out |
| SCR | Scorer |

### SC @FreeThrow

| Code | ENG Description |
| --- | --- |
| 1 | 1st free throw |
| 2 | 2nd free throw |
| 3 | 3rd free throw |

### SC @FreeThrowOf

| Code | ENG Description |
| --- | --- |
| 1 | of 1 free throw |
| 2 | of 2 free throws |
| 3 | of 3 free throws |

### SC @Sector

| Code | Note | ENG Description |
| --- | --- | --- |
| C |  | Center straight on / top of circle |
| FD | for 3 pts | Back court (from downtown) |
| LL |  | Left corner |
| LW |  | Left wing |
| P | for 2 pts | In the paint (except 3 seconds area) |
| PI | for 2 pts | Under the basket restricted area (part of paint) |
| RL |  | Right corner |
| RW |  | Right wing |

### SC @ShotFlag

| Code | ENG Description |
| --- | --- |
| AL | Points from second chance + fastbreak + after turnover |
| FB | Fastbreak points |
| FT | Points from fastbreak + after turnover |
| SC | 2nd chance points |
| SF | Points from second chance + fastbreak |
| ST | Points from second chance + after turnover |
| TO | Points after turnover |

### SC @Res_Sub

| Code | Note | ENG Description |
| --- | --- | --- |
| AO | for 2 pts | Alley-oop |
| DL |  | Driving lay-up |
| DU | for 2 pts | Dunk |
| FA |  | Fadeaway jump shot |
| FL |  | Floating jump shot |
| HS |  | Hook shot |
| JS |  | Jump shot |
| LU | for 2 pts | Lay-up |
| PD |  | Putback dunk |
| PJ |  | Pullup jump shot |
| SJ |  | Step back jump shot |
| TI | for 2 pts | Putback tip-in |
| TJ |  | Turnaround jump shot |
| TO | after each kind of Personal Offensive Foul | Turnover |
| TTO | after each kind of Coach/Bench Offensive Foul | Team turnover |

### SC @ActionComment

| Code | ENG Description |
| --- | --- |
| STARTP_Q1 | Start of Quarter 1 |
| STARTP_Q2 | Start of Quarter 2 |
| STARTP_Q3 | Start of Quarter 3 |
| STARTP_Q4 | Start of Quarter 4 |
| STARTP_OT1 | Start of Overtime 1 |
| STARTP_OT2 | Start of Overtime 2 |
| STARTP_OT3 | Start of Overtime 3 |
| STARTP_OT4 | Start of Overtime 4 |
| STARTP_OT5 | Start of Overtime 5 |
| ENDP_Q1 | End of Quarter 1 |
| ENDP_Q2 | End of Quarter 2 |
| ENDP_Q3 | End of Quarter 3 |
| ENDP_Q4 | End of Quarter 4 |
| ENDP_OT1 | End of Overtime 1 |
| ENDP_OT2 | End of Overtime 2 |
| ENDP_OT3 | End of Overtime 3 |
| ENDP_OT4 | End of Overtime 4 |
| ENDP_OT5 | End of Overtime 5 |

### SC @Source

| Code | ENG Description |
| --- | --- |
| BCYBKB1 | Origin for messages from OVR at BCY for BKB |
| LILBKB1 | Origin for messages from OVR at LIL for BKB |

### CC @ResultStatus

| Code | Order | Note | ENG Description |
| --- | --- | --- | --- |
| START_LIST | 1 | Before competition, Start List | Start List |
| LIVE | 2 | For live updates during competition | Live |
| INTERMEDIATE | 3 | When competition is stopped, used at pre-defined points | Intermediate |
| UNCONFIRMED | 4 | When the unit is over but not yet unofficial or official | Unconfirmed |
| UNOFFICIAL | 5 | Results released as soon as the event is over, not awaiting official decision | Unofficial |
| OFFICIAL | 6 | Results released once officially confirmed (protests resolved, etc.) | Official |
| PARTIAL | 7 | Incomplete list, Final Ranking. Used in PDF | Partial |
| PROTESTED | 8 | After the competition is no longer LIVE and a protest has been lodged | Protested |
| PROVISIONAL | 9 | Special situations | Provisional |

### CC @SportGender

| Id | ENG Description |
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

# ODF JUD Data Dictionary: Paris 2024 DT_PLAY_BY_PLAY, Pages 41-45

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 41-45.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note restructures the Paris 2024 Judo `DT_PLAY_BY_PLAY` action feed.

## 2.3.5 Play by Play

`DT_PLAY_BY_PLAY` contains official raw action data from the results provider. The message is sent for each contest RSC.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@Competition` | Competition ID. |
| `DocumentCode` | `CC@Unit` | Full RSC for each contest. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_PLAY_BY_PLAY` | Play-by-play message. |
| `DocumentSubtype` | `ACTION` | Action subtype. |
| `Version` | `1..V` | Ascending content version. |
| `ResultStatus` | `CC@ResultStatus` | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNOFFICIAL`, `OFFICIAL`, `PROVISIONAL`. |
| `FeedFlag` | `P`, `T` | Production or test. |
| `Date` / `Time` / `LogicalDate` | Date/time | Header generation and logical dates. |
| `Source` | `SC@Source` | System that generated the message. |

## Trigger and Frequency

| Status | Trigger |
|---|---|
| `START_LIST` | Only if there is an action before unit start. |
| `LIVE` | When the contest starts. |
| `LIVE` | After every action. |
| `INTERMEDIATE` | Interruption. |
| `UNOFFICIAL` / `OFFICIAL` | After the contest. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- ExtendedInfos (0,1)
    |   +-- SportDescription (0,1)
    |   +-- VenueDescription (0,1)
    +-- Actions (0,1)
        +-- @Home
        +-- @Away
        +-- Action (1,N)
            +-- @Id
            +-- @Period
            +-- @Order
            +-- @Action
            +-- @ActionAdd
            +-- @When
            +-- @Result
            +-- Competitor (0,N)
                +-- @Code
                +-- @Type
                +-- @Order
                +-- @Organisation
                +-- Composition (0,1)
                    +-- Athlete (1,N)
```

## Message Values

### `SportDescription` and `VenueDescription`

`SportDescription` carries discipline, event, subevent, and gender display fields. `VenueDescription` carries venue and
location code/name fields.

### `Actions`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Home` | O | `S(20)` | Home competitor ID. |
| `Away` | O | `S(20)` | Away competitor ID. |

### `Actions / Action`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Id` | M | `S(36)` | Stable action ID within the unit. |
| `Period` | M | `SC@Period` | Period of the action. |
| `Order` | M | Numeric | Sequential action order from `1` to `n`. |
| `Action` | M | `SC@PointsType`, `SC@PenaltyType`, or `SC@Action` | Points, penalty, or general action code. Required at contest start/end. |
| `ActionAdd` | M | `ACTION`, `POINTSTYPE`, `PENALTYTYPE` | Discriminator for the `Action` code domain. |
| `When` | O | `mm:ss` | Period-relative action time, ascending from `0:00`. |
| `Result` | O | `SC@Technique` | Technique code. |

### `Action / Competitor`

| Attribute | M/O | Value | Meaning |
|---|---|---|---|
| `Code` | M | `S(20)` | Competitor ID. |
| `Type` | M | `T`, `A` | Team or athlete. |
| `Order` | O | Numeric | Competitor display order for this action. |
| `Organisation` | M | `CC@Organisation` | Competitor organisation. |

`Competitor/Composition/Athlete` identifies the athlete or team member related to the action.

## Sample, Normalized

```xml
<Action Id="123456" Period="N" Order="3" Action="S" ActionAdd="PENALTYTYPE" When="2:12">
  <Competitor Code="1008743" Type="A" Organisation="SUI" Order="1">
    <Composition>
      <Athlete Code="1008743" Order="1">
        <Description GivenName="Jane" FamilyName="Smits" Gender="W" Organisation="SUI" BirthDate="1994-12-15"/>
      </Athlete>
    </Composition>
  </Competitor>
</Action>
```

## Message Sort

Sort by `Actions/Action/@Order`.

## Modeling Notes

- Use `Action/@Order` for replay order; use `Action/@Id` for identity.
- `ActionAdd` is mandatory and determines which code catalog `Action` belongs to.
- `When` is not absolute time; it is the elapsed time inside the period.
- Team-member action details are expressed through `Competitor/@Type="T"` and nested athlete composition where present.

## Code Appendix: Paris 2024 Values

Catalog values come from Paris 2024 CC/SC code tables; message-specific restrictions remain in the field tables above.

| Code Entity | Section Usage | Values |
|---|---|---|
| `DocumentType` | Header | `DT_PLAY_BY_PLAY` |
| `DocumentSubtype` | Header | `ACTION` |
| `CC@ResultStatus` | Header | `START_LIST`, `LIVE`, `INTERMEDIATE`, `UNCONFIRMED`, `UNOFFICIAL`, `OFFICIAL`, `PARTIAL`, `PROTESTED`, `PROVISIONAL` |
| `ActionAdd` | Action domain discriminator | `ACTION`, `POINTSTYPE`, `PENALTYTYPE` |
| `SC@Action` | Contest lifecycle action | `START_CONTEST`, `END_CONTEST` |
| `SC@PointsType` | Scoring action | `IPP`, `WAZ`, `YUK` |
| `SC@PenaltyType` | Penalty action | `H`, `S`, `S3`, `X`, `s1`, `s2`, `s3` |
| `SC@Period` | Action period | `N`, `GS` |
| `Competitor/@Type` | Competitor kind | `A`, `T` |
| `SC@Technique` | Action result technique | Large JUD technique catalog in SportCodes; keep as code reference instead of hard-coding a partial list. |

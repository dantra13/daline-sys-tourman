# ODF BDM Data Dictionary: DT_ENTRIES, Pages 23-26

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BDM_Data_Dictionary.pdf`, pages 23-26.

Source version: `SYOG-2026-BDM-1.1 SFR`, dated 13 May 2026.

This note restructures the Badminton `DT_ENTRIES` section into a practical reference for event-level entries.

## 2.3.3 Entries by Event

`DT_ENTRIES` contains entry information for a specific Badminton event. It lists the athletes, guides, reserves, and
teams entered in that event, including team composition when that composition is known.

The message is always a full message. A new `DT_ENTRIES` message for the same event resets the previous entry list for
that event.

## Header Values

| Attribute | Value | Meaning |
|---|---|---|
| `CompetitionCode` | `CC@COMPETITION_CODE` | Competition ID. |
| `DocumentCode` | `CC@EVENT` | Event RSC. |
| `DocumentSubcode` | N/A | Not used. |
| `DocumentType` | `DT_ENTRIES` | Entries by event message. |
| `DocumentSubtype` | N/A | Not used. |
| `Version` | Positive integer | Ascending version number for the message content. |
| `ResultStatus` | N/A | Not used. |
| `FeedFlag` | `P`, `T` | Production or test feed. |
| `Source` | `SCGEN@Source` | System that generated the message. |

## Trigger and Frequency

| Stage | When to send |
|---|---|
| Before the Games | Bulk entries feed, repeated as needed until data is transferred to the venue results system. |
| After venue transfer | Bulk entries feed triggered by the venue results system whenever entry data changes. |

## Message Structure

```text
OdfBody
+-- Competition (0,1)
    +-- @Gen
    +-- @Sport
    +-- @Codes
    +-- Entry (1,N)
        +-- @Code
        +-- @Type
        +-- @Organisation
        +-- @SortOrder
        +-- Composition (0,1)
            +-- Athlete (0,N)
                +-- @Code
                +-- @Order
                +-- @EntryStatus
                +-- Description (1,1)
                |   +-- @GivenName
                |   +-- @FamilyName
                |   +-- @Gender
                |   +-- @Organisation
                |   +-- @BirthDate
                |   +-- @IFId
                +-- ExtendedEntry (0,N)
                    +-- @Type
                    +-- @Code
                    +-- @Pos
                    +-- @Value
```

## Entry Values

| Path / Attribute | Requirement | Value source | Notes |
|---|---:|---|---|
| `Competition/@Gen` | M | `S(20)` | General schema/package version label. |
| `Competition/@Sport` | M | `S(35)` | Badminton dictionary version label. |
| `Competition/@Codes` | M | `S(20)` | Code set version label. |
| `Entry/@Code` | M | `S(20)` | Entry ID without leading zeroes. |
| `Entry/@Type` | M | `A` | Athlete entry in the BDM table. |
| `Entry/@Organisation` | M | `CC@ORGANISATION` | Entry organisation. |
| `Entry/@SortOrder` | M | Positive integer | Sort order for the event entries. |
| `Entry/Composition/Athlete/@Code` | M | `S(20)` | Athlete participant ID. |
| `Entry/Composition/Athlete/@Order` | M | `1` | Athlete order in the entry. |
| `Entry/Composition/Athlete/@EntryStatus` | O | `SC@AthleteStatus` | Entry status when relevant. |
| `Athlete/Description` fields | M | Text / codes | Given name, family name, gender, organisation, birth date, and IF ID as available. |

## Extended Entry Values

| Type | Code | Pos | Value | When |
|---|---|---|---|---|
| `ENTRY` | `RANK_WLD` | N/A | `S(4)` | Junior world ranking when applicable for individual events. |
| `ENTRY` | `RANK_PTS` | N/A | `#####0` | Junior world ranking points when available for individual events. |

## Message Sort

Sort by `Entry/@SortOrder`.

## Modeling Notes

- Treat `DT_ENTRIES` as the event-level roster snapshot. It is not an update message.
- `Entry/@Code` can be the same as an athlete participant ID for individual entries, but ingestion should still model
  entries and participants separately because they have different document scopes.
- `RANK_WLD` and `RANK_PTS` are event-entry attributes. Do not attach them to `DT_PARTIC` participant master data.
- The source sample in this section appears to be a generic/team-sport sample rather than a BDM-specific sample; it uses
  structures that do not match the BDM message-value table. Use the BDM table above as canonical for Badminton.

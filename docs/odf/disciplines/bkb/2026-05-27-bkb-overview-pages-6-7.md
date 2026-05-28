# ODF BKB Data Dictionary: Overview, Pages 6-7

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_BKB_Data_Dictionary.pdf`, pages 6-7.

Source version: `SOG-2024-BKB-3.4 APP`, dated 19 January 2024.

This note extracts the Basketball overview and applicable-message matrix into a quick reference. In the source table,
`Message extended = X` means Basketball has a discipline-specific extension for that message. Rows without `X` may
still be applicable, but they follow the general ODF definition.

## 2.1 Basketball Overview

### Messages in Each Event

- All games have the same messages sent and each game is managed independently. (The source PDF prints
  "indepentently"; normalized here.)
- Each game includes `DT_RESULT`, `DT_PLAY_BY_PLAY`, and `DT_CURRENT`.
- `DT_CURRENT` only includes the clock and the current score.

### Schedule Rules

| Case | Schedule level | Meaning |
|---|---|---|
| Basketball games | Unit level, `schedule=Y` | `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` includes each game only, aligned with the same RSC used by `DT_RESULT`. |

### Paralympics

- Paralympics adds athlete class; otherwise the discipline follows the same principles.

### Statistics Codes Used in the Document

The following statistics codes are used throughout the Basketball dictionary.

| Code | Meaning |
|---|---|
| `PTS` | Points |
| `TO` | Turnover |
| `FG` | Field goal |
| `P2` | 2-pt shot |
| `P3` | 3-pt shot |
| `FT` | Free throw |
| `REB` | Rebound |
| `DR` | Defensive rebound |
| `OR` | Offensive rebound |
| `ST` | Steal |
| `BLC` | Block |
| `PF` | Personal foul |
| `FD` | Fouls drawn |
| `TOUT` | Time out |
| `TREB` | Team rebounds |
| `TTO` | Team turnovers |
| `EFF` | Efficiency |

## 2.2 Applicable Messages

### BKB-Extended Messages

These messages have Basketball-specific definitions beyond the general ODF message rules.

| Message Type | Message Name |
|---|---|
| `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Competition schedule / Competition schedule update |
| `DT_PARTIC` / `DT_PARTIC_UPDATE` | List of participants by discipline / List of participants by discipline update |
| `DT_PARTIC_TEAMS` / `DT_PARTIC_TEAMS_UPDATE` | List of teams / List of teams update |
| `DT_RESULT` | Event Unit Start List and Results |
| `DT_CURRENT` | Current Information |
| `DT_PLAY_BY_PLAY` | Play by Play |
| `DT_IMAGE` | Image |
| `DT_POOL_STANDING` | Pool Standings |
| `DT_BRACKETS` | Brackets |
| `DT_STATS` | Statistics |
| `DT_RANKING` | Event Final Ranking |

### General-Definition Messages Listed for BKB

These messages are listed in the Basketball applicable-message table without a Basketball-specific extension marker.

| Message Type | Message Name |
|---|---|
| `DT_MEDALLISTS` | Event's Medallists |
| `DT_MEDALLISTS_DISCIPLINE` | Medallists by discipline |
| `DT_MEDALS` | Medal standings |
| `DT_COMMUNICATION` | Communication |
| `DT_PRESENTER` | Medal Presenters |
| `DT_LOCAL_ON` | Discipline/venue start transmission |
| `DT_LOCAL_OFF` | Discipline/venue stop transmission |
| `DT_KA` | Keep Alive |
| `DT_ALERT` | Alert |
| `DT_BCK` | Background Document |
| `DT_BIO_PAR` | Participant Biography |
| `DT_BIO_TEA` | Team Biography |
| `DT_NEWS` | News Document |
| `DT_PIC` | Pictures |
| `DT_PDF` | PDF Message |

## Quick Message Map

| Concern | Primary BKB messages |
|---|---|
| Schedule | `DT_SCHEDULE`, `DT_SCHEDULE_UPDATE` |
| Participants and teams | `DT_PARTIC`, `DT_PARTIC_UPDATE`, `DT_PARTIC_TEAMS`, `DT_PARTIC_TEAMS_UPDATE` |
| Game start list and results | `DT_RESULT` |
| Current clock and score | `DT_CURRENT` |
| Play-by-play feed | `DT_PLAY_BY_PLAY` |
| Pool standings and progression | `DT_POOL_STANDING`, `DT_BRACKETS` |
| Match and player statistics | `DT_STATS` |
| Final event ranking | `DT_RANKING` |
| Imagery | `DT_IMAGE` |
| Medals and medallists | `DT_MEDALLISTS`, `DT_MEDALLISTS_DISCIPLINE`, `DT_MEDALS` |

## Modeling Notes

- Do not read `Message extended = X` as an applicability flag. It marks that BKB defines sport-specific structure or
  rules for that message.
- Each game is an independent unit. `DT_RESULT`, `DT_PLAY_BY_PLAY`, and `DT_CURRENT` are unit-scoped around the same
  RSC, and `DT_CURRENT` is intentionally minimal: clock and current score only.
- The schedule/result join is unit RSC based: `schedule=Y` units in `DT_SCHEDULE` should match the same RSC used by
  `DT_RESULT`.
- Basketball publishes a fixed statistics vocabulary in the overview. Treat those codes as the canonical domain stat
  set for BKB; per-message `DT_STATS` extensions should be aligned to this list.
- The source uses the plural `DT_BRACKETS` for the progression message, in line with FBL and in contrast to the
  singular `DT_BRACKET` seen in some other disciplines (e.g. BDM). Keep the raw spelling from the dictionary when
  mapping BKB.
- Paralympic Basketball adds an athlete class attribute but otherwise follows the same message principles.

# ODF BDM Data Dictionary: Overview, Pages 6-7

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BDM_Data_Dictionary.pdf`, pages 6-7.

Source version: `SYOG-2026-BDM-1.1 SFR`, dated 13 May 2026.

This note extracts the Badminton overview and applicable-message matrix into a quick reference. In the source table,
`Message extended = X` means Badminton has a discipline-specific extension for that message. Rows without `X` may still
be applicable, but they follow the general ODF definition.

## 2.1 Badminton Overview

### Messages in Singles

- All events have a single `DT_RESULT` and `DT_PLAY_BY_PLAY` for each unit, where the unit is the match.
- There is also a `DT_BRACKET` message for progression in each event.

### Schedule Rules

| Case | Schedule level | Meaning |
|---|---|---|
| Singles and doubles units | Unit level, `schedule=Y` | `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` includes each unit only, aligned with the same RSC used by `DT_RESULT`. |

### Bracket Rules

- Badminton uses one bracket code for all head-to-head competitions.
- The bracket code is `FNL`.
- The bronze medal match is included in the `FNL` bracket, in the final phase after the gold medal match, with
  `Position=2`.

## 2.2 Applicable Messages

### BDM-Extended Messages

These messages have Badminton-specific definitions beyond the general ODF message rules.

| Message Type | Message Name |
|---|---|
| `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Competition schedule / Competition schedule update |
| `DT_PARTIC` / `DT_PARTIC_UPDATE` | List of participants by discipline / List of participants by discipline update |
| `DT_ENTRIES` | List of Entries by Event |
| `DT_RESULT` | Event Unit Start List and Results |
| `DT_PLAY_BY_PLAY` | Play by play |
| `DT_POOL_STANDING` | Pool standings |
| `DT_STATS` | Statistics |
| `DT_RANKING` | Event Final Ranking |
| `DT_CONFIG` | Configuration |

### General-Definition Messages Listed for BDM

These messages are listed in the Badminton applicable-message table without a Badminton-specific extension marker.

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

| Concern | Primary BDM messages |
|---|---|
| Schedule | `DT_SCHEDULE`, `DT_SCHEDULE_UPDATE` |
| Entries and participants | `DT_ENTRIES`, `DT_PARTIC`, `DT_PARTIC_UPDATE` |
| Unit start list and results | `DT_RESULT` |
| Point-by-point match flow | `DT_PLAY_BY_PLAY` |
| Head-to-head progression | `DT_BRACKET` |
| Pool standings | `DT_POOL_STANDING` |
| Match statistics | `DT_STATS` |
| Final event ranking | `DT_RANKING` |
| Medals and medallists | `DT_MEDALLISTS`, `DT_MEDALLISTS_DISCIPLINE`, `DT_MEDALS` |
| Configuration | `DT_CONFIG` |

## Modeling Notes

- Treat `DT_RESULT` and `DT_PLAY_BY_PLAY` as match-unit messages. The overview states one of each per unit.
- The schedule/result join is unit RSC based: `schedule=Y` units in `DT_SCHEDULE` should match the same RSC used by
  `DT_RESULT`.
- `DT_BRACKET` is the progression message for Badminton. The source text uses singular `DT_BRACKET`, while some other
  discipline dictionaries use pluralized bracket message names; keep the raw dictionary spelling when mapping BDM.
- The bronze medal match is not a separate bracket. It is represented inside `FNL` after the gold medal match with
  `Position=2`.

# ODF VBV Data Dictionary: Overview, Pages 6-7

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_VBV_Data_Dictionary.pdf`, pages 6-7.

Source version: `SYOG-2026-VBV-1.2 SFR`, dated 18 May 2026.

This note extracts the Beach Volleyball overview and applicable-message matrix into a quick reference. In the source
table, `Message extended = X` means Beach Volleyball has a discipline-specific extension for that message. Rows without
`X` may still be applicable, but they follow the general ODF definition.

## 2.1 Beach Volleyball Overview

### Messages in Each Event

- All games have the same messages and are managed independently.
- Each game includes `DT_RESULT`, `DT_PLAY_BY_PLAY`, and `DT_CURRENT`.
- `DT_CURRENT` only includes the clock and current score.

### Schedule Rules

| Case | Schedule level | Meaning |
|---|---|---|
| Beach Volleyball games | Unit level, `schedule=Y` | `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` includes each game, aligned with the same RSC used by `DT_RESULT`. |
| Pre-draw later rounds | Temporary units in phase `TMRY` | Detailed schedule is not known before the draw. Temporary units remain until teams are known and matches are allocated, then those temporary units are unscheduled. |

### Bracket Rules

- There is one `FNL` bracket for all head-to-head Beach Volleyball competitions.
- The bronze medal match is included in the `FNL` bracket in the final phase after the gold medal match with
  `Position=2`.

## 2.2 Applicable Messages

### VBV-Extended Messages

These messages have Beach Volleyball-specific definitions beyond the general ODF message rules.

| Message Type | Message Name |
|---|---|
| `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Competition schedule / Competition schedule update |
| `DT_PARTIC` / `DT_PARTIC_UPDATE` | List of participants by discipline / List of participants by discipline update |
| `DT_PARTIC_TEAMS` / `DT_PARTIC_TEAMS_UPDATE` | List of teams / List of teams update |
| `DT_ENTRIES` | List of Entries by Event |
| `DT_RESULT` | Event Unit Start List and Results |
| `DT_CURRENT` | Current Information |
| `DT_PLAY_BY_PLAY` | Play by Play |
| `DT_POOL_STANDING` | Pool Standings |
| `DT_BRACKETS` | Brackets |
| `DT_STATS` | Statistics |
| `DT_RANKING` | Event Final Ranking |
| `DT_WEATHER` | Weather conditions |

### General-Definition Messages Listed for VBV

These messages are listed in the Beach Volleyball applicable-message table without a Beach Volleyball-specific extension
marker.

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

| Concern | Primary VBV messages |
|---|---|
| Schedule | `DT_SCHEDULE`, `DT_SCHEDULE_UPDATE` |
| Entries, participants, and teams | `DT_ENTRIES`, `DT_PARTIC`, `DT_PARTIC_UPDATE`, `DT_PARTIC_TEAMS`, `DT_PARTIC_TEAMS_UPDATE` |
| Match start list and results | `DT_RESULT` |
| Current clock and score | `DT_CURRENT` |
| Rally-level feed | `DT_PLAY_BY_PLAY` |
| Pool standings and progression | `DT_POOL_STANDING`, `DT_BRACKETS` |
| Match statistics | `DT_STATS` |
| Final event ranking | `DT_RANKING` |
| Weather | `DT_WEATHER` |

## Modeling Notes

- Do not read `Message extended = X` as an applicability flag. It marks that VBV defines sport-specific structure or
  rules for that message.
- Each game is an independent unit. `DT_RESULT`, `DT_PLAY_BY_PLAY`, and `DT_CURRENT` are unit-scoped around the same RSC.
- Temporary `TMRY` units are a scheduling concern. Treat them as placeholders that can be unscheduled once actual
  matchups are allocated.
- Bracket modeling should support a single `FNL` bracket with bronze placed after gold using `Position=2`.

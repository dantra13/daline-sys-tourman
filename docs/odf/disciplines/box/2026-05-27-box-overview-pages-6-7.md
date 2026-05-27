# ODF BOX Data Dictionary: Overview, Pages 6-7

Source: `C:\Users\mella\Downloads\2026-Dakar-YOG-ODF_BOX_Data_Dictionary.pdf`, pages 6-7.

Source version: `SCOG/SYOG-2026-BOX-1.2 SFR`, dated 13 May 2026.

This note extracts the Boxing overview and applicable-message matrix into a quick reference. In the source table,
`Message extended = X` means Boxing has a discipline-specific extension for that message. Rows without `X` may still be
applicable, but they follow the general ODF definition.

## 2.1 Boxing Overview

### Messages in Each Event

- All events have a single `DT_RESULT` for each unit.
- Boxing also has a `DT_BRACKETS` message for progression.

### Schedule Rules

| Case | Schedule level | Meaning |
|---|---|---|
| Boxing units | Unit level, `schedule=Y` | `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` includes each unit only, aligned with the same RSC used by `DT_RESULT`. |

### Bracket Rules

- There is only one bracket for all head-to-head Boxing competitions.
- The bracket code is `FNL`.

## 2.2 Applicable Messages

### BOX-Extended Messages

These messages have Boxing-specific definitions beyond the general ODF message rules.

| Message Type | Message Name |
|---|---|
| `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Competition schedule / Competition schedule update |
| `DT_PARTIC` / `DT_PARTIC_UPDATE` | List of participants by discipline / List of participants by discipline update |
| `DT_ENTRIES` | List of Entries by Event |
| `DT_RESULT` | Event Unit Start List and Results |
| `DT_BRACKETS` | Brackets |
| `DT_STATS` | Statistics |
| `DT_RANKING` | Event Final Ranking |
| `DT_CONFIG` | Configuration |

### General-Definition Messages Listed for BOX

These messages are listed in the Boxing applicable-message table without a Boxing-specific extension marker.

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
| `DT_NEWS` | News Document |
| `DT_PIC` | Pictures |
| `DT_PDF` | PDF Message |

## Quick Message Map

| Concern | Primary BOX messages |
|---|---|
| Schedule | `DT_SCHEDULE`, `DT_SCHEDULE_UPDATE` |
| Entries and participants | `DT_ENTRIES`, `DT_PARTIC`, `DT_PARTIC_UPDATE` |
| Unit start list and results | `DT_RESULT` |
| Head-to-head progression | `DT_BRACKETS` |
| Bout statistics | `DT_STATS` |
| Final event ranking | `DT_RANKING` |
| Medals and medallists | `DT_MEDALLISTS`, `DT_MEDALLISTS_DISCIPLINE`, `DT_MEDALS` |
| Configuration | `DT_CONFIG` |

## Modeling Notes

- Do not read `Message extended = X` as an applicability flag. It marks that BOX defines sport-specific structure or
  rules for that message.
- BOX schedule is simpler than ATH's mixed phase/unit schedule pattern: units are included at unit level with
  `schedule=Y`, matching the `DT_RESULT` RSC.
- `DT_BRACKETS` is a first-class progression message for Boxing, with a single `FNL` bracket code across H2H
  competitions.
- The overview lists `DT_RESULT` as one message per unit, so unit identity should remain the primary anchor for start
  lists and results.

# ODF JUD Data Dictionary: Paris 2024 Overview, Pages 6-7

Source: `C:\Users\mella\Downloads\2024-Paris-OG-PDF-ODF_JUD_Data_Dictionary.pdf`, pages 6-7.

Source version: `SOG-2024-JUD-3.3 APP`, dated 28 March 2024.

This note extracts the Paris 2024 Judo overview and applicable-message matrix into a quick reference. In the source
table, `Message extended = X` means Judo has a discipline-specific extension for that message. Rows without `X` are
still listed for Judo, but they follow the general ODF definition.

## 2.1 Judo Overview

### Messages in Each Event

- All events, individual and team, have a single `DT_RESULT` and `DT_PLAY_BY_PLAY` for each unit.
- The overview prose says there is also a `DT_BRACKET` message for progression. The applicable-message table names the
  actual DocumentType as `DT_BRACKETS`.

### Schedule Rules

| Case | Schedule level | Meaning |
|---|---|---|
| Individual and team units | Unit level, `schedule=Y` | `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` includes each scheduled unit, aligned with the same RSC used by `DT_RESULT`. |
| Team events | Team-match unit only | Only the team match is scheduled. Individual contests inside the team match are not scheduled, primarily because they are known late and not every contest is played. |
| Pre-draw later rounds | Temporary units in phase `TMRY` | Detailed schedule is not known until the draw, one day before competition. Temporary units are removed when the final schedule is published. |

### Entries Rules

- In the distribution of team entries, teams do not include athletes within each team because that information is not
  known at that time.

## 2.2 Applicable Messages

### JUD-Extended Messages

These messages have Judo-specific definitions beyond the general ODF message rules.

| Message Type | Message Name |
|---|---|
| `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Competition schedule / Competition schedule update |
| `DT_PARTIC` / `DT_PARTIC_UPDATE` | List of participants by discipline / List of participants by discipline update |
| `DT_PARTIC_TEAMS` / `DT_PARTIC_TEAMS_UPDATE` | List of teams / List of teams update |
| `DT_RESULT` | Event Unit Start List and Results |
| `DT_PLAY_BY_PLAY` | Play by Play |
| `DT_BRACKETS` | Brackets |
| `DT_STATS` | Statistics |
| `DT_RANKING` | Event Final Ranking |
| `DT_CONFIG` | Configuration |

### General-Definition Messages Listed for JUD

These messages are listed in the Judo applicable-message table without a Judo-specific extension marker.

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

## All DT Tokens Mentioned in the Overview Pages

| DocumentType | Where it appears | Notes |
|---|---|---|
| `DT_SCHEDULE` | Applicable-message table; schedule overview | Extended for JUD. |
| `DT_SCHEDULE_UPDATE` | Applicable-message table; schedule overview | Extended for JUD. |
| `DT_PARTIC` | Applicable-message table | Extended for JUD. |
| `DT_PARTIC_UPDATE` | Applicable-message table | Extended for JUD. |
| `DT_PARTIC_TEAMS` | Applicable-message table | Extended for JUD; important for team modeling. |
| `DT_PARTIC_TEAMS_UPDATE` | Applicable-message table | Extended for JUD. |
| `DT_RESULT` | Overview prose; applicable-message table | Extended for JUD; applies to individual and team units. |
| `DT_PLAY_BY_PLAY` | Overview prose; applicable-message table | Extended for JUD. |
| `DT_BRACKET` | Overview prose | Prose uses singular; table uses `DT_BRACKETS`. |
| `DT_BRACKETS` | Applicable-message table | Extended for JUD; progression message. |
| `DT_STATS` | Applicable-message table | Extended for JUD in Paris 2024. |
| `DT_RANKING` | Applicable-message table | Extended for JUD. |
| `DT_MEDALLISTS` | Applicable-message table | General definition. |
| `DT_MEDALLISTS_DISCIPLINE` | Applicable-message table | General definition. |
| `DT_MEDALS` | Applicable-message table | General definition. |
| `DT_CONFIG` | Applicable-message table | Extended for JUD. |
| `DT_COMMUNICATION` | Applicable-message table | General definition. |
| `DT_PRESENTER` | Applicable-message table | General definition. |
| `DT_LOCAL_ON` | Applicable-message table | General definition. |
| `DT_LOCAL_OFF` | Applicable-message table | General definition. |
| `DT_KA` | Applicable-message table | General definition. |
| `DT_ALERT` | Applicable-message table | General definition. |
| `DT_BCK` | Applicable-message table | General definition. |
| `DT_BIO_PAR` | Applicable-message table | General definition. |
| `DT_BIO_TEA` | Applicable-message table | General definition. |
| `DT_NEWS` | Applicable-message table | General definition. |
| `DT_PIC` | Applicable-message table | General definition. |
| `DT_PDF` | Applicable-message table | General definition. |

## Quick Message Map

| Concern | Primary JUD messages |
|---|---|
| Schedule | `DT_SCHEDULE`, `DT_SCHEDULE_UPDATE` |
| Participants and teams | `DT_PARTIC`, `DT_PARTIC_UPDATE`, `DT_PARTIC_TEAMS`, `DT_PARTIC_TEAMS_UPDATE` |
| Unit start list and results | `DT_RESULT` |
| Contest action feed | `DT_PLAY_BY_PLAY` |
| Progression and draw state | `DT_BRACKETS` |
| Statistics and final ranking | `DT_STATS`, `DT_RANKING` |
| Configuration | `DT_CONFIG` |
| Medals and medallists | `DT_MEDALLISTS`, `DT_MEDALLISTS_DISCIPLINE`, `DT_MEDALS` |

## Modeling Notes

- Paris 2024 is the better Judo reference when modeling team events and team-match subunits. The overview explicitly
  says individual contests inside a team match are not scheduled separately.
- Do not read `Message extended = X` as an applicability flag. It marks that JUD defines sport-specific structure or
  rules for that message.
- For team modeling, `DT_PARTIC_TEAMS` and `DT_RESULT` are central: teams are distributed separately, and team-match
  result details are expanded in the detailed `DT_RESULT` section.
- Preserve the singular `DT_BRACKET` wording from the overview as source prose, but implement against `DT_BRACKETS`
  from the applicable-message table.

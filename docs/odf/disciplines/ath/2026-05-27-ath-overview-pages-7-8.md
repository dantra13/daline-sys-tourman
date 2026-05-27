# ODF ATH Data Dictionary: Overview, Pages 7-8

Source: `C:\Users\mella\Downloads\ODF_ATH_Data_Dictionary.pdf`, pages 7-8.

Source version: `SOG-2024-ATH-3.4 APP`, dated 3 May 2024.

This note extracts the Athletics overview and applicable-message matrix into a quick reference. In the source table,
`Message extended = X` means Athletics has a discipline-specific extension for that message. Rows without `X` may still
be applicable, but they follow the general ODF definition.

## 2.1 Athletics Overview

### Messages in Each Event

- All events have one `DT_RESULT` for each unit, such as a heat or group.
- Some track events also send `DT_RESULT_ANALYSIS` for additional information, including more intermediate times.
- `DT_CURRENT` is provided in track and road events for up-to-date rankings.
- `DT_PHASE_RESULT` is sent for non-final phases and for combined events when there is more than one heat.
- `DT_CUMULATIVE_RESULT` is sent in combined events for the overall score.

Note: the overview prose uses the plural wording `DT_CUMULATIVE_RESULTS`, but the applicable-message table lists the
actual `DocumentType` as `DT_CUMULATIVE_RESULT`.

### Schedule Rules

| Case | Schedule level | Meaning |
|---|---|---|
| Long throws and finals of all events | Unit level, `schedule=Y` | Schedule is maintained and sent at the same unit level used by `DT_RESULT`. |
| Non-long throws and non-finals | Phase level, `schedule=Y`; units also included with `schedule=S` | Phase owns schedule maintenance, while units still appear in `DT_SCHEDULE` and match the `DT_RESULT` units. |

### Paralympic Games Notes

- Class is used only in the Paralympic Games.
- Split times are not used in relays.
- `DT_CURRENT` is not included.

## 2.2 Applicable Messages

### ATH-Extended Messages

These messages have Athletics-specific definitions beyond the general ODF message rules.

| Message Type | Message Name |
|---|---|
| `DT_PARTIC` / `DT_PARTIC_UPDATE` | List of participants by discipline / List of participants by discipline update |
| `DT_PARTIC_TEAMS` / `DT_PARTIC_TEAMS_UPDATE` | List of teams / List of teams update |
| `DT_RESULT` | Event Unit Start List and Results |
| `DT_RESULT_ANALYSIS` | Results Analysis |
| `DT_CURRENT` | Current Information |
| `DT_PHASE_RESULT` | Phase Results |
| `DT_CUMULATIVE_RESULT` | Cumulative Results |
| `DT_IMAGE` | Image |
| `DT_RECORD` | Records |
| `DT_RANKING` | Event Final Ranking |
| `DT_CONFIG` | Configuration |
| `DT_WEATHER` | Weather conditions |

### General-Definition Messages Listed for ATH

These messages are listed in the Athletics applicable-message table without an Athletics-specific extension marker.

| Message Type | Message Name |
|---|---|
| `DT_SCHEDULE` / `DT_SCHEDULE_UPDATE` | Competition schedule / Competition schedule update |
| `DT_PRESSPHOTOFINISH_LK` | Press Photofinish |
| `DT_MEDALLISTS` | Event's Medallists |
| `DT_MEDALLISTS_DISCIPLINE` | Medallists by discipline |
| `DT_MEDALS` | Medal standings |
| `DT_COMMUNICATION` | Communication |
| `DT_PRESENTER` | Medal Presenters |
| `DT_LOCAL_ON` | Discipline/venue start transmission |
| `DT_LOCAL_OFF` | Discipline/venue stop transmission |
| `DT_KA` | Keep Alive |
| `DT_ALERT` | Alert |
| `DT_TV_TRACKING` | TV Tracking |
| `DT_BCK` | Background Document |
| `DT_BIO_PAR` | Participant Biography |
| `DT_BIO_TEA` | Team Biography |
| `DT_NEWS` | News Document |
| `DT_ESL` | Extended Start List |
| `DT_PIC` | Pictures |
| `DT_PDF` | PDF Message |

## Quick Message Map

| Concern | Primary ATH messages |
|---|---|
| Schedule | `DT_SCHEDULE`, `DT_SCHEDULE_UPDATE` |
| Start list and unit results | `DT_RESULT` |
| Track intermediate/split analysis | `DT_RESULT_ANALYSIS` |
| Live/up-to-date rankings for track and road | `DT_CURRENT` |
| Non-final phase rankings | `DT_PHASE_RESULT` |
| Combined-event overall scoring | `DT_CUMULATIVE_RESULT` |
| Records and final event ranking | `DT_RECORD`, `DT_RANKING` |
| Medals and medallists | `DT_MEDALLISTS`, `DT_MEDALLISTS_DISCIPLINE`, `DT_MEDALS` |
| Participants and teams | `DT_PARTIC`, `DT_PARTIC_UPDATE`, `DT_PARTIC_TEAMS`, `DT_PARTIC_TEAMS_UPDATE` |
| Configuration, images, and weather | `DT_CONFIG`, `DT_IMAGE`, `DT_WEATHER` |

## Modeling Notes

- Do not read `Message extended = X` as an applicability flag. It marks that ATH defines sport-specific structure or
  rules for that message.
- `DT_RESULT` remains the core unit-level message. ATH adds `DT_RESULT_ANALYSIS`, `DT_CURRENT`, `DT_PHASE_RESULT`, and
  `DT_CUMULATIVE_RESULT` around it depending on event type.
- Schedule scope differs by event class: some events are maintained at unit level, while non-final non-long-throw units
  can be schedule-listed under a phase-maintained schedule.
- Paralympic-specific behavior should be modeled as a competition/event variant, not as default Athletics behavior.

---
name: odf-dt-codes-reference
description: Use when you need to look up valid ODF code values, understand what codes exist for a discipline, verify result/schedule state machines, or compare code coverage between OG2024 and OWG2026. Triggers on questions like "what codes does RESULTSTATUS have?", "what alert codes exist for FBL?", "is COMPETITION_FORMAT_TYPE new in 2026?".
---

# ODF DT_CODES Reference

## Overview

Common Codes (DT_CODES) are the controlled vocabularies used across all ODF messages — result statuses, schedule states, discipline-specific alert codes, competition formats, etc. A local SQLite DB at `D:\w2c\Common-codes\odf-codes.db` contains codes for two competitions:

- **OG2024** — Paris 2024 (summer, 76 disciplines)
- **OWG2026** — Milano Cortina 2026 (winter, 33 disciplines)

Query via the CLI at `D:\w2c\Common-codes\dt-codes.py`.

## Commands

```bash
# One-time — parse source XMLs and build the DB:
uv run D:/w2c/Common-codes/dt-codes.py seed

# Summary of competitions and coverage:
uv run D:/w2c/Common-codes/dt-codes.py competitions

# List available subtypes (CC categories):
uv run D:/w2c/Common-codes/dt-codes.py subtypes
uv run D:/w2c/Common-codes/dt-codes.py subtypes --comp 2026   # Milano only

# Look up codes for a subtype:
uv run D:/w2c/Common-codes/dt-codes.py query RESULTSTATUS
uv run D:/w2c/Common-codes/dt-codes.py query SPORT_CODES --discipline FBL
uv run D:/w2c/Common-codes/dt-codes.py query SPORT_CODES --discipline FBL --group "@Alert"
uv run D:/w2c/Common-codes/dt-codes.py query SCHEDULESTATUS --comp 2026

# Delta between competitions for a subtype:
uv run D:/w2c/Common-codes/dt-codes.py diff RESULTSTATUS
uv run D:/w2c/Common-codes/dt-codes.py diff COMPETITION_FORMAT_TYPE
```

## Reading the output

### `query` — grouped by competition → discipline

```
── OG2024 ──────────────────────────────────
  [(generic)]                      ← no discipline: applies to all sports
    OFFICIAL        Official
    UNOFFICIAL      Unofficial

  [FBL]                            ← sport-specific codes for football
    KDLT            Kick-off is delayed  ‹@Alert›   ← logical group within the subtype
    MITI            Match interrupted    ‹@Alert›
```

- `[(generic)]` — discipline-less codes that apply universally
- `[FBL]`, `[ARC]`, etc. — codes specific to that discipline
- `‹@Alert›`, `‹@Phase›` — the `group_name`: semantic grouping within the subtype

### `diff` — cross-competition comparison

For generic-code subtypes (RESULTSTATUS, SCHEDULESTATUS): compares exact codes.
For discipline-indexed subtypes (SPORT_CODES, EVENT, PHASE): compares which disciplines are present in each competition.

A subtype showing "Only OWG2026" in the diff was introduced for the winter games.

### `subtypes` — navigation guide

The `(N disciplines)` note means the subtype has per-sport variants — you'll need `--discipline` for useful queries.

## Key subtypes for ODF domain work

| Subtype | What it defines |
|---------|----------------|
| `RESULTSTATUS` | Result state machine: `START_LIST → LIVE → UNOFFICIAL → OFFICIAL` |
| `SCHEDULESTATUS` | Scheduled unit state: `SCHEDULED`, `DELAYED`, `CANCELLED`… |
| `PARTICIPANT_STATUS` | Competitor status: `DNS`, `DNF`, `DSQ`, `WD`… |
| `SPORT_CODES` | Per-discipline extension codes — alerts, periods, sport-specific phases |
| `PHASE_TYPE` | Competition phase types: `FINAL`, `SEMIFINAL`, `QUALIFYING`… |
| `EVENTUNIT_TYPE` | Unit classification: `HEAT`, `MATCH`, `ROUND`, `RUN`… |
| `COMPETITION_FORMAT_TYPE` | ⚠️ OWG2026 only — bracket types: `POOL`, `BRCK`, `CUML`, `BFIN`… |
| `PROGRESSION_TYPE` | ⚠️ OWG2026 only — how athletes advance between phases |
| `PARTICIPANT_TYPE` | ⚠️ OWG2026 only — `INDV` / `TEAM` / `IGRP` / `DGRP` |

## Structural difference: OG2024 vs OWG2026

| | OG2024 | OWG2026 |
|---|---|---|
| XML files | 462 (one per language) | 40 (one per subtype, all languages inside) |
| Language in filename | `OG2024_ENG_DT_CODES_*.xml` | `OWG2026_DT_CODES_*.xml` |
| Languages per CodeSet | 1 | 11 (ENG, FRA, ITA, JPN, KOR, SPA, DEU, POR, HIN, RUS, CHI) |

The DB normalizes this difference — all queries return ENG by default.

## When to re-run seed

- New competition folders or ZIPs are added to `D:\w2c\Common-codes\`
- After registering a new source in `SOURCES` inside `dt-codes.py`

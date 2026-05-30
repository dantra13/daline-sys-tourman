---
title: Capa operativa de resultados (Unit Result)
date: 2026-05-29
status: draft
scope: conceptual model only
---

# Capa operativa de resultados (Unit Result)

Spec del modelo conceptual de la **capa operativa de resultados**: lo que ocurre dentro de una unidad
competitiva (match, bout, game, contest) y cómo se expresa de forma común a todas las disciplinas, dejando
que cada una defina su propia gramática. No cubre persistencia, endpoints, play-by-play, estadísticas
agregadas, standings, cumulative, brackets ni export a mensajes ODF.

Es la **primera capa** de resultados, deliberadamente antes de los formatos de competición: sirve para
validar que existe una estructura común de resultado operativo que sirve a disciplinas heterogéneas
(FBL, ATH, JUD, BOX) sin acoplarse a la progresión.

## 1. Contexto

El core deportivo (`docs/superpowers/specs/2026-05-27-core-deportivo-design.md`) modela la estructura
competitiva `Event → Phase → Unit (+ Subunit)`, los participantes y el `Rsc`, pero declara explícitamente
fuera de scope la operación de resultados. Este spec abre esa capa.

La hipótesis de separación que lo motiva quedó validada contra ODF:

- **El resultado operativo vive en la unit.** ODF emite `DT_RESULT` a nivel de unit (RSC de unit), con el
  estado del enfrentamiento, el outcome por competidor y el detalle propio de la disciplina.
- **La progresión consume ese resultado, no lo contiene.** `DT_POOL_STANDING` (nivel phase),
  `DT_BRACKETS` (nivel event), `DT_RANKING` y `DT_CUMULATIVE_RESULT` (nivel event) son capas derivadas que
  referencian units por RSC y guardan datos calculados, no el detalle operativo. Evidencia contundente:
  en `DT_RESULT` de FBL `Result/@Result` son **goles**; en `DT_POOL_STANDING` `Result/@Result` son
  **puntos de tabla, explícitamente no el marcador** ("*Store table points separately from goals*"). Misma
  palabra, significado por capa — análogo a `Entry != Participant`.
- **Cada disciplina expresa su resultado de forma propia sobre el mismo "slot".** Todas usan `DT_RESULT` a
  nivel de unit con un esqueleto común (`ResultStatus`, `WLT`, `IRM`, `SortOrder`), pero el contenido
  difiere: goles/periodos en FBL, scores por juez por round en BOX, sets en VBV/BDM, alturas e intentos en
  ATH.

Material ODF de referencia local: `docs/odf/foundation-principles-r-sog-2024-fnd.md`,
`docs/odf/general-interface/`, `docs/odf/disciplines/{fbl,ath,box,jud,...}/`.

### Taxonomía de tres niveles (la frontera operativo ↔ progresión)

ODF impone una distinción que este spec adopta como columna vertebral:

1. **Detalle operativo interno** — la estructura interna de un único resultado, con forma libre por
   disciplina y sin identidad propia (intentos por altura en ATH como `ExtendedResult ER/INTERMEDIATE`,
   periodos y tarjetas en FBL, scores por juez en BOX). Vive **dentro** del documento de resultado de una
   unit. **Capa operativa.**
2. **Subunit + rollup** — una unidad competitiva con identidad propia (RSC), su propio resultado operativo,
   que **agrega** hacia un resultado operativo del padre vía una regla intrínseca de la disciplina (el team
   match de JUD: 4-0 sobre sus contests). Cada contest de JUD es un subunit con RSC `…01..NN` bajo un padre
   `…00`. **Sigue siendo capa operativa** — la agregación consolida lo que el padre posee.
3. **Proyección inter-unit** — agregación/derivación sobre units **independientes** referenciadas por RSC
   (cumulative, standings, ranking, brackets). No contiene detalle operativo. **Capa de progresión/formato,
   fuera de scope.**

La frontera operativo↔progresión cae **entre el nivel 2 y el 3**. Regla discriminante: *¿la agregación
consolida la estructura interna que la propia unit posee (operativa), o agrega units independientes que no
le pertenecen (proyección)?* El team match de JUD agrega lo que contiene; el cumulative de ATH agrega units
ajenas.

## 2. Alcance

### En scope

- Modelo conceptual del **documento de resultado operativo** por unit/subunit: estado, filas por
  competidor, segmentos, extensions genéricas estilo ODF y la pieza recursiva `ResultExtension`.
- El **envoltorio común** que aporta el core (`ResultStatus`, outcome `WLT`/`Rank`, `IRM`, `SortOrder`) y el
  **contrato de gramática tipada por disciplina** (`IResultSchema`).
- El caso de **rollup de subunits** (JUD team match) con outcome agregado almacenado-y-derivado.
- Sketch de validación con cuatro disciplinas: **FBL** (head-to-head + periodos), **ATH** (ranked +
  intentos), **JUD** (rollup + subunit), **BOX** (scores por juez + decisión + subset de estados).
- Invariantes de dominio (`I-RES-*`).

### Fuera de scope (specs futuras)

- Persistencia y mapping EF Core de la capa de resultados.
- Endpoints/aplicación para registrar y consultar resultados.
- `DT_PLAY_BY_PLAY` (stream de acciones; documento aparte, también a nivel unit).
- `DT_STATS` y agregados estadísticos.
- Standings (`DT_POOL_STANDING`), ranking (`DT_RANKING`), cumulative (`DT_CUMULATIVE_RESULT`), brackets
  (`DT_BRACKETS`) — capa de proyección/formato.
- `ScheduleStatus` y el ciclo de vida de schedule de la Unit (máquina de estados distinta, nivel
  `DT_SCHEDULE`).
- Mecánica de live-update/transporte y export a mensajes ODF (incluido el historial por versión).
- Las disciplinas BKB/BDM/VBV: implementan el contrato con un schema default permisivo, sin gramática real
  todavía.
- **Competidores placeholder de ODF** (`TBD`, `NOCOMP`, `BYE`): son artefactos de bracket/scheduling, no de
  la operativa. Un `CompetitorResult` siempre referencia una `Entry` real; un walkover/no-show se modela con
  `Irm` (`DNS`/`WDR`) sobre esa entry, no con un código placeholder. Los placeholders quedan para la capa de
  formato/progresión.

## 3. Decisiones tomadas

| #   | Decisión                  | Resumen                                                                                                                                                  |
|-----|---------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| R1  | Separación de capas       | Resultado operativo en la unit/subunit; progresión/standings/cumulative son capas derivadas fuera de scope. Validado contra ODF.                        |
| R2  | A+B (genérico + tipado)   | Documento común con componentes genéricos estilo ODF, interpretados/validados por **esquemas tipados** de cada disciplina. Ni "todo string" ni agregado tipado paralelo. |
| R3  | Path estructural (T1)     | La semántica del path se ancla **colgando las extensions del nodo dueño** (unit/competitor/composition/segment), no con un campo `Owner`. Estados ilegales irrepresentables. |
| R4  | Rollup almacenado (T2)    | El outcome agregado del padre (JUD 4-0) se **almacena y se deriva**: lo recomputa la disciplina al completarse cada contest. El padre tiene su propio `ResultStatus` y desempate. |
| R5  | Alcance (T3)              | Snapshot conceptual + validación con FBL/ATH/JUD/BOX. Persistencia/API/play-by-play/standings diferidos.                                                |
| R6  | Vista tipada = proyección | El detalle tipado por disciplina se **proyecta en lectura** (`Project`), no se persiste aparte. Sin `DisciplineResultRef`: una sola fuente de verdad.    |
| R7  | Atributos ODF controlados | `ResultExtension` lleva el **set controlado de ODF** (`Value2/Rank/RankEqual/SortOrder/Diff/Move` de `ExtendedResult`; `Attempt` de `StatsItem`) + `Children` recursivo para anidamiento. No es un property bag libre; el schema declara qué aplica. |
| R8  | Segmento generalizado     | `ResultSegment` lleva score **por competidor** (`SegmentScore`) con `CumulativeValue` (acumulado) y `SegmentValue` (solo el segmento), reflejando ODF `Home/AwayScore` vs `Home/AwayPeriodScore`, generalizado a N. |
| R9  | Nombres                   | `UnitResultDocument`/`CompetitorResult`/`ResultSegment`/`ResultExtension`. Se evita `Result` (existe `Shared.Result`) y `Entry` (existe el agregado de inscripción). |
| R10 | `Version`                 | Token de concurrencia optimista sobre un agregado mutable; **no** historial de snapshots (eso es export/auditoría, posterior).                          |
| R11 | Rollup tipado             | `AggregateSubunits` retorna un `ResultRollup` (filas + extensions + estado sugerido), no solo filas; resuelve que el rollup produce también `TEAM/COMP`. |
| R12 | Estados no lineales        | `ResultStatus` incluye **todos** los common codes ODF (`…Partial, Protested, Provisional`); la legalidad de transición la decide `IResultSchema.CanTransition`, no un orden fijo en el core. |

## 4. Ubicación

Nuevo submódulo `Results` dentro de `Sport.Core` (carpeta + namespace `Sport.Core.Results`), coherente con
la organización del core (un `csproj` monolítico con carpetas por submódulo).

```
Sport.Core
├── Competitions
├── Structure          (Event → Phase → Unit + Subunit + Rsc)
├── Participants       (Person, Organisation, Team, Entry, Composition)
├── Officials
├── Results            ← NUEVO (UnitResultDocument y compañía)
└── DisciplineRegistry (contratos; gana IResultSchema)
```

Reglas:

- `Sport.Core.Results` referencia `Structure` (`UnitId`, `SubunitId`, `Rsc`), `Participants` (`EntryId`,
  `PersonId`, `Bib`) y `DisciplineRegistry` (resuelve el schema).
- El core sigue **ciego** a las disciplinas: el detalle y la gramática los aporta cada módulo vía
  `IResultSchema`. NetArchTest: `Sport.Core.*` no referencia `Sport.Disciplines.*`.
- Cada disciplina importa del core; el core nunca importa de una disciplina.

## 5. El agregado raíz: `UnitResultDocument`

Snapshot completo del resultado operativo de **una** unit o subunit.

```
UnitResultDocument  (agregado raíz)
├── Id : UnitResultId                 (Vogen, Guid v7)
├── TargetUnitId : UnitId
├── TargetSubunitId : SubunitId?      (null = resultado de la unit; seteado = contest/subunit)
├── TargetRsc : Rsc                   (denormalizado; debe coincidir con la estructura)
├── DisciplineCode : DisciplineCode   (denormalizado: resuelve schema sin recorrer la estructura)
├── EventType : EventTypeCode         (contexto para que el schema valide)
├── Status : ResultStatus             (máquina de estados común)
├── Version : int                     (token de concurrencia optimista)
├── Competitors : List<CompetitorResult>
├── Segments : List<ResultSegment>    (vacío si la disciplina no segmenta, p.ej. ATH high jump)
└── Extensions : List<ResultExtension> (nivel-unit)
```

**Identidad y JUD.** Hay un `UnitResultDocument` por `(TargetUnitId, TargetSubunitId)`:

- Unit atómica → un documento, `TargetSubunitId = null`.
- Team match de JUD → **un documento padre** (`TargetSubunitId = null`, el agregado 4-0) **+ N documentos de
  contest** (`TargetSubunitId` seteado, uno por subunit `…01..NN`). Coincide con ODF: el team match
  referencia sus contests vía `TEAM/COMP` y el resultado del team competitor es el score del match, no los
  puntos individuales — *"keep team member contest results in DT_RESULT"* por contest.

## 6. Filas, segmentos y la pieza genérica

La colocación estructural (R3) ancla la semántica del path: una misma extension significa distinto según el
nodo del que cuelga. Ej. `INTERMEDIATE` a nivel unit define una altura; a nivel competidor son los intentos
del atleta en esa altura.

```
CompetitorResult  (entidad hija)
├── EntryId : EntryId                 (referencia a Participants.Entry)
├── ResultValue : string?             (@Result ODF: goles/sets/puntos/tiempo/distancia)
├── ResultType : ResultTypeCode?      (TIME | DISTANCE | POINTS | IRM … vocab de disciplina)
├── Wlt : Wlt?                        (head-to-head)
├── Rank : int?
├── RankEqual : bool
├── Irm : Irm?                        (DNS/DNF/DSQ/WDR core + extensiones por disciplina)
├── SortOrder : int
├── StartOrder : int?
├── StartSortOrder : int?
├── Composition : List<CompetitorMemberResult>   (athletes de un competidor de equipo)
└── Extensions : List<ResultExtension>           (nivel-competidor: p.ej. JUDGE totals en BOX)

CompetitorMemberResult  (entidad hija de CompetitorResult)
├── PersonId : PersonId
├── Bib : Bib?
├── Order : int                                  (orden de resultado del miembro — ODF Athlete/@Order)
├── StartSortOrder : int?                        (orden de start-list — ODF Athlete/@StartSortOrder)
└── Extensions : List<ResultExtension>           (nivel-athlete: GF/GA, SUB_TIME, tarjetas; EUE: STARTER/CAPTAIN…)

ResultSegment  (entidad hija)   — periodo/half/set/game/round; opcional por disciplina
├── Code : SegmentCode
├── Order : int
├── Scores : List<SegmentScore>                  (score por competidor; generaliza Home/Away)
└── Extensions : List<ResultExtension>           (nivel-segmento: p.ej. EP SCR por juez en BOX)

SegmentScore  (value object)
├── EntryId : EntryId
├── CumulativeValue : string?    (score total al final del segmento — ODF Home/AwayScore)
└── SegmentValue : string?       (score solo en este segmento — ODF Home/AwayPeriodScore)

ResultExtension  (value object, recursivo)
├── Type : ExtensionType    (ER | UI | TEAM | EP | ST …)
├── Code : ExtensionCode    (INTERMEDIATE | JUDGE | COMP | WIND_SPEED | GF …)
├── Pos : string?
├── Value : string?
├── Value2 : string?        (set controlado ODF ExtendedResult)
├── Rank : int?
├── RankEqual : bool
├── SortOrder : int?
├── Diff : string?
├── Move : string?
├── Attempt : string?       (ODF StatsItem: p.ej. SHOT/PTY — intentos totales)
└── Children : List<ResultExtension>             (anidamiento ODF: TEAM/COMP → WEIGHT_CATEGORY/HOME/AWAY)
```

Notas:

- Sin `Owner` (lo da la colocación) y **sin diccionario libre**: los atributos extra de `ResultExtension`
  son el **set controlado de ODF** (`Value2/Rank/RankEqual/SortOrder/Diff/Move` de `ExtendedResult`;
  `Attempt` de `StatsItem`), no un property bag — confirmado en ATH `ER/INTERMEDIATE`, BOX `ER/JUDGE` y FBL
  `ST/SHOT`. El `IResultSchema` declara cuáles aplican por código. El anidamiento usa `Children`. Sin
  `DisciplineResultRef` (la vista tipada se proyecta, no se persiste).
- `SegmentScore` separa **acumulado** (`CumulativeValue`, p.ej. marcador 2-1 al final del H2) de **solo el
  segmento** (`SegmentValue`, p.ej. los goles de ese half), reflejando `HomeScore/AwayScore` vs
  `HomePeriodScore/AwayPeriodScore` de ODF, generalizado a N competidores.
- `ResultValue`/`SegmentScore.{CumulativeValue,SegmentValue}`/`ResultExtension.Value` quedan como **string en reposo** (fiel a ODF y
  uniforme); el `IResultSchema` declara el *kind* de cada código, lo valida/parsea y lo expone parseado en
  la proyección. (Sub-decisión (i): string-at-rest + schema-validated.)

## 7. Value objects

| VO                  | Definición                                                                                      |
|---------------------|-------------------------------------------------------------------------------------------------|
| `UnitResultId`      | Vogen, Guid v7                                                                                   |
| `ResultStatus`      | enum-VO con **todos** los common codes ODF: `StartList, Live, Intermediate, Unconfirmed, Unofficial, Official, Partial, Protested, Provisional` |
| `Wlt`               | enum-VO: `W, L, T`                                                                               |
| `Irm`               | string-VO; el core conoce los comunes (`DNS, DNF, DSQ, WDR`) pero **cada schema declara el set válido** por event type (JUD: `DNS/DQB/DSQ/WDR`, no `DNF`) |
| `OutcomeMode`       | enum-VO: `HeadToHead, Ranked`                                                                    |
| `ResultTypeCode`    | string-VO; vocab de disciplina (`TIME, DISTANCE, POINTS, IRM`, …)                                |
| `SegmentCode`       | string-VO; vocab de disciplina (`H1, H2, S1, G1, R1, TOT`, …)                                    |
| `ExtensionType`     | string-VO; familia ODF (`ER, UI, TEAM, EP, ST, EUE`, …)  — `EUE` para member facts (`STARTER, CAPTAIN, HOME_AWAY, COLOUR`) |
| `ExtensionCode`     | string-VO; vocab de disciplina (`INTERMEDIATE, JUDGE, COMP, WIND_SPEED`, …)                      |

El core enforce la *forma* (shape de los VOs); el *contenido válido por contexto* lo decide el schema de la
disciplina, igual que hoy `PhaseCode`/`UnitCode` viven en el core y se validan por módulo.

## 8. Contrato de disciplina: `IResultSchema`

Se agrega a `IDisciplineModule` como miembro nuevo, hermano de `IPhaseCatalog`/`IUnitCodeStrategy`, con
**implementación default permisiva** vía default interface method (mantiene triviales a las disciplinas sin
gramática real todavía, igual que se hizo con subunit-hosting).

```
IResultSchema
├── OutcomeMode OutcomeModeFor(EventTypeCode type)
├── IReadOnlySet<ResultStatus> StatusesFor(EventTypeCode type)        (BOX omite UNOFFICIAL)
├── bool CanTransition(ResultStatus from, ResultStatus to, EventTypeCode type)   (legalidad de transición)
├── IReadOnlySet<Irm> IrmCodesFor(EventTypeCode type)                (set IRM válido)
├── Result Validate(UnitResultDocument document)                      (vocab + cardinalidad + outcome-mode + placement)
├── DisciplineResultProjection Project(UnitResultDocument document)   (vista tipada de lectura)
└── ResultRollup AggregateSubunits(                                   (solo si HostsSubunits — rollup)
        UnitResultDocument parent,
        IReadOnlyList<UnitResultDocument> contestResults)

ResultRollup  (value object — retorno de AggregateSubunits)
├── Competitors : IReadOnlyList<CompetitorResult>   (filas del padre: 4-0, W/L)
├── Extensions : IReadOnlyList<ResultExtension>     (TEAM/COMP por contest + hijos)
└── SuggestedStatus : ResultStatus?                 (estado sugerido del padre tras el rollup)
```

- `StatusesFor` declara los estados que la disciplina usa; `CanTransition` decide la **legalidad de cada
  transición** (la máquina de estados ODF de `ResultStatus` no es estrictamente lineal: p.ej. `Protested`
  vuelve a `Unofficial`/`Official`). El core no asume un orden fijo.
- `Validate` comprueba vocabularios (`ResultType`/`SegmentCode`/`ExtensionType`/`ExtensionCode`/`Irm` vía
  `IrmCodesFor`), cardinalidades, los atributos controlados de cada extension, el *placement* y la
  coherencia de outcome del event type.
- `Project` devuelve un subtipo de `DisciplineResultProjection` propio de la disciplina
  (`FootballMatchResult`, `HighJumpResult`, `BoxingBoutResult`, `JudoTeamMatchResult`) — **derivado, nunca
  almacenado**.
- `AggregateSubunits` devuelve un `ResultRollup` con las filas de outcome del padre (4-0, W/L), las
  extensions `TEAM/COMP` y el estado sugerido. Solo aplica a event types con `HostsSubunits = true`.

## 9. Flujos canónicos (conceptual)

**Resultado de unit atómica:**

1. La aplicación arma un `UnitResultDocument` (`TargetSubunitId = null`) con `DisciplineCode`/`EventType`
   resueltos desde la estructura.
2. `schema.Validate(document)` → vocab/cardinalidad/outcome/placement.
3. El agregado transiciona `Status` por métodos guardados por `StatusesFor` (no se setea libremente).

**Team match de JUD (rollup, R4):**

1. Cada contest se registra como su propio `UnitResultDocument` (`TargetSubunitId` del contest), validado
   por el schema como un resultado head-to-head normal.
2. Al pasar un contest a `OFFICIAL`, el padre **recomputa** su agregado:
   `schema.AggregateSubunits(parent, contestResults)` devuelve un `ResultRollup`, y el padre **almacena** sus
   `Competitors` (4-0, `Wlt`) + `Extensions` (`TEAM/COMP`); el `SuggestedStatus` orienta la transición del
   padre (validada por `CanTransition`).
3. El padre tiene su propio `ResultStatus`; el desempate (golden score) lo resuelve el schema. El padre
   pasa a `OFFICIAL` cuando todos los contests están resueltos (o se aplicó el desempate).

**Lectura:** `schema.Project(document)` → vista tipada para UI/consumidores.

**Progresión futura:** consume **solo** el envoltorio común (Status + outcome del padre), nunca el detalle
de disciplina ni los contests internos. Confirmado por ODF: brackets/ranking de JUD operan sobre el team
match agregado, no sobre los contests.

## 10. Invariantes (I-RES-*)

- **I-RES-1.** Único `UnitResultDocument` por `(TargetUnitId, TargetSubunitId)`.
- **I-RES-2.** `TargetSubunitId` seteado ⟹ el subunit pertenece a `TargetUnitId` y la unit es host de
  subunits; `null` en units atómicas.
- **I-RES-3.** `TargetRsc` coincide con el RSC del target (unit o subunit) en la estructura.
- **I-RES-4.** `Status` pertenece al subset `StatusesFor` de la disciplina y toda transición satisface
  `CanTransition(from, to, eventType)`. No se asume un orden lineal fijo (la máquina ODF no lo es).
- **I-RES-5.** Coherencia de outcome (no aplica en `StartList` ni en filas con `Irm`): `HeadToHead` ⟹ `Wlt`
  presente y `Rank` ausente; `Ranked` ⟹ `Wlt` ausente y `SortOrder` presente. En `Ranked`, `Rank` es
  **opcional** y lo exige el schema según `Status`/event type — ODF permite filas sin `Rank` en live/pending,
  con `SortOrder` siempre presente.
- **I-RES-6.** `SortOrder` único dentro del documento.
- **I-RES-7.** Cada `CompetitorResult.EntryId` (y cada `SegmentScore.EntryId`) referencia una `Entry` real
  del Event de la unit target. No se modelan competidores placeholder ODF (ver §2 Alcance).
- **I-RES-8.** Códigos de `ResultType`/`SegmentCode`/`ExtensionType`/`ExtensionCode` válidos por schema para
  el event type, con placement correcto.
- **I-RES-9.** `Irm` pertenece al set declarado por el schema para el event type (`IrmCodesFor`), no a un
  set core universal.
- **I-RES-10.** En un padre host, el outcome agregado (filas + extensions `TEAM/COMP`) es **derivado** vía
  `AggregateSubunits` (que retorna `ResultRollup`), no se setea a mano: se recomputa al completarse cada
  contest.
- **I-RES-11.** `Version` incrementa en cada mutación aceptada del agregado.

## 11. Sketch de validación por disciplina

Demuestra que el envoltorio común + `IResultSchema` cubren cuatro expresiones operativas heterogéneas.

| Disciplina | Outcome      | Segmentos                  | Detalle operativo (extensions)                                   | Proyección             |
|------------|--------------|----------------------------|------------------------------------------------------------------|------------------------|
| **FBL**    | HeadToHead   | `H1, H2` (+ ET/PSO)        | tarjetas/`SUB_TIME`/stats por jugador en `Composition`; `RES_CODE` | `FootballMatchResult`  |
| **ATH HJ** | Ranked       | — (no segmenta)            | `UI/INTERMEDIATE` (alturas, nivel-unit) + `ER/INTERMEDIATE` (intentos `o`/`x`/`-`, nivel-competidor) | `HighJumpResult`       |
| **BOX**    | HeadToHead   | `R1, R2, R3` (rounds)      | `EP/SCR` por juez (nivel-segmento); `ER/JUDGE` totals, `ER/KD` (nivel-competidor); `RES_CODE` decisión | `BoxingBoutResult`     |
| **JUD team** | HeadToHead (padre) | — (padre); contests aparte | `TEAM/COMP` → `WEIGHT_CATEGORY/HOME/AWAY` (nivel-unit, padre)     | `JudoTeamMatchResult`  |

Detalles de cada caso de validación:

- **FBL.** Dos filas (home/away) con `Wlt`; segmentos `H1/H2` con `SegmentScore` por competidor; tarjetas,
  cambios y stats de jugador como extensions de `CompetitorMemberResult`. `Project → FootballMatchResult`
  (marcador, periodos, eventos por jugador).
- **ATH high jump.** Filas `Ranked` con `Result` = mejor marca y `ResultType = DISTANCE`; alturas como
  `UI/INTERMEDIATE` a nivel-unit (`Pos = 1..n`, `Value` = altura) y los intentos como `ER/INTERMEDIATE` a
  nivel-competidor (`Value` = string `o`/`x`/`-`/`r`). `Project → HighJumpResult` (alturas + series de
  intento + mejor marca + rank).
- **BOX.** Dos filas con `Wlt` y `Result` = puntos; segmentos de round con extensions `EP/SCR` por juez;
  `ER/JUDGE` totales y `ER/KD` a nivel-competidor; `RES_CODE` (WP/TKO/NC/DKO) como extension de decisión.
  `StatusesFor` **omite `UNOFFICIAL`**. `Project → BoxingBoutResult`.
- **JUD team.** Padre con dos filas (`Wlt`, `Result` = submatches ganados, p.ej. "4-0") + extensions
  `TEAM/COMP` (una por contest, con hijos `WEIGHT_CATEGORY/HOME/AWAY/STATUS/GOLD_SCORE`); seis documentos de
  contest, cada uno head-to-head con su propio resultado. El agregado del padre es derivado por
  `AggregateSubunits`. `Project → JudoTeamMatchResult` (filas del padre + refs a contests).

## 12. Errores

- Violaciones de invariante → `DomainException` con códigos `I-RES-*`, consistente con el uso de `I-STR-*`
  en `Structure`.
- La validación de gramática del schema devuelve `Sport.Core.Shared.Result`; los métodos gobernados del
  agregado la elevan a `DomainException` cuando corresponde (mismo patrón que `Event.AddTeamMatchUnit`).

## 13. Estrategia de testing

- **`Sport.Core.Results.Tests`** (o dentro de `Sport.Core.Tests`): construcción de `UnitResultDocument`,
  máquina de estados, invariantes de identidad (I-RES-1/2/3), outcome-mode (I-RES-5), unicidad de
  `SortOrder`, recompute del agregado (I-RES-10).
- **Casos por disciplina** (FBL/ATH/BOX/JUD) según el sketch de §11, cada uno con su `Validate` feliz +
  rechazos de vocab/placement, y su `Project` produciendo la vista tipada esperada.
- **JUD end-to-end:** padre + 6 contests → recompute → padre "4-0" + `TEAM/COMP`; camino de golden score;
  verificación de que un consumidor de progresión leería solo el outcome del padre.
- **Arquitectura (NetArchTest):** `Sport.Core.Results` no referencia `Sport.Disciplines.*`; el schema se
  resuelve siempre vía `DisciplineRegistry`.

## 14. File map (sketch)

```
src/Sport.Core/Results/
  UnitResultDocument.cs / UnitResultId.cs
  CompetitorResult.cs / CompetitorMemberResult.cs
  ResultSegment.cs / SegmentScore.cs
  ResultExtension.cs
  ResultStatus.cs / Wlt.cs / Irm.cs / OutcomeMode.cs
  ResultTypeCode.cs / SegmentCode.cs / ExtensionType.cs / ExtensionCode.cs
src/Sport.Core/DisciplineRegistry/
  IResultSchema.cs                 (nuevo)
  ResultRollup.cs                  (nuevo, retorno de AggregateSubunits)
  DisciplineResultProjection.cs    (nuevo, base de proyecciones)
  IDisciplineModule.cs             (modificar: ResultSchema con default permisivo)
src/Sport.Disciplines.FBL/  FblResultSchema.cs + FootballMatchResult.cs
src/Sport.Disciplines.ATH/  AthResultSchema.cs + HighJumpResult.cs
src/Sport.Disciplines.BOX/  BoxResultSchema.cs + BoxingBoutResult.cs
src/Sport.Disciplines.JUD/  JudResultSchema.cs + JudoTeamMatchResult.cs
src/Sport.Disciplines.{BKB,BDM,VBV}/  (default permisivo, sin gramática real)
tests/  Sport.Core.Results.Tests/ + extensiones a Sport.Architecture.Tests
```

## 15. Mapeo ODF ↔ modelo

| Concepto ODF                                   | Entidad/VO local                                        |
|------------------------------------------------|---------------------------------------------------------|
| `DT_RESULT` (nivel unit)                       | `UnitResultDocument`                                    |
| `DocumentCode` (CC@Unit)                       | `TargetRsc` (+ `TargetUnitId`)                          |
| Contest de team match (CC@Unit subunit)        | `UnitResultDocument` con `TargetSubunitId`             |
| `ResultStatus` (header)                        | `ResultStatus`                                          |
| `Result` (fila por competitor)                 | `CompetitorResult`                                      |
| `Result/@Result`, `@ResultType`               | `CompetitorResult.ResultValue`, `.ResultType`         |
| `Result/@WLT`, `@IRM`, `@SortOrder`           | `CompetitorResult.Wlt`, `.Irm`, `.SortOrder`          |
| `Result/Competitor/Composition/Athlete`        | `CompetitorMemberResult`                               |
| `Periods/Period` (Home/Away score)             | `ResultSegment` + `SegmentScore` (por competidor)     |
| `ExtendedResults/ExtendedResult` (`ER`)        | `ResultExtension` (`Type = ER`)                        |
| `ExtendedInfo` (`UI`) + `Extension` anidada    | `ResultExtension` (`Type = UI`) + `Children`          |
| `ExtendedPeriods/ExtendedPeriod` (`EP`)        | `ResultExtension` (`Type = EP`) bajo `ResultSegment`  |
| `TEAM/COMP` (refs a contests)                  | `ResultExtension` (`Type = TEAM`, `Code = COMP`) + `Children` |
| `StatsItem` (`ST`)                             | `ResultExtension` (`Type = ST`)                       |
| Vista de lectura por disciplina                | `DisciplineResultProjection` (no ODF; ergonomía interna) |

## 16. Dependencias hacia adelante

| Spec siguiente                          | Depende de esta capa en                                         |
|-----------------------------------------|----------------------------------------------------------------|
| Persistencia + EF Core de resultados    | Todo el agregado y sus VOs                                      |
| Endpoints de registro/consulta          | `UnitResultDocument`, `IResultSchema.Validate/Project`         |
| Play-by-play (`DT_PLAY_BY_PLAY`)         | `UnitResultDocument` (mismo nivel unit, documento aparte)      |
| Formatos de competición + standings      | Solo el envoltorio común (`Status` + outcome), nunca el detalle |
| Ranking / cumulative / brackets          | Outcome por unit (referencia por RSC)                          |
| Export a mensajes ODF                    | El mapeo §15 + historial por versión                          |

## 17. Riesgos abiertos

1. **`ResultValue` como string en reposo.** Empuja el parseo al schema. Mitigación: el `kind` declarado por
   código + proyecciones tipadas; tests por disciplina cubren el parseo.
2. **Recompute del agregado (R4) acoplado al ciclo de los contests.** Requiere un disparador claro
   ("contest → OFFICIAL") y manejo de desempate. Mitigación: el schema posee `AggregateSubunits`; el padre
   no pasa a `OFFICIAL` hasta resolver.
3. **Placement estructural vs export plano ODF.** El dominio cuelga extensions del nodo; el export a
   `DT_RESULT` plano será una proyección posterior. Costo asumido a favor de type-safety.
4. **`DisciplineResultProjection` como tipo base débil.** Cada disciplina devuelve su subtipo; el consumidor
   castea. Aceptable mientras la proyección sea solo de lectura/UI.
5. **Identidad del documento vs múltiples emisiones.** `Version` es concurrencia, no historial; si se
   requiere auditoría/replay/export por versión, es un cambio aditivo en spec posterior.

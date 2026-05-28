---
title: Diseño del Core Deportivo
date: 2026-05-27
status: draft
scope: conceptual model only
---

# Diseño del Core Deportivo

Spec del modelo conceptual del core deportivo de la plataforma. Define entidades, relaciones, invariantes, identidades y el mecanismo de especialización por disciplina. No cubre persistencia, formatos de competición, standings, ni operación en vivo.

## 1. Contexto

La plataforma gestiona campeonatos deportivos: creación de torneos, configuración de disciplinas y eventos, inscripción de participantes, operación de unidades competitivas y consulta de resultados. Las disciplinas iniciales son `FBL` (football), `BKB` (basketball), `BDM` (badminton), `VBV` (volleyball), `BOX` (boxing) y `ATH` (athletics, al menos high jump).

El diseño se apoya en el vocabulario y la estructura del estándar ODF (Olympic Data Feed) como base interna, sin replicar la complejidad operativa olímpica. El producto no es ODF, pero habla ODF.

Material ODF de referencia local:
- `docs/odf/foundation-principles-r-sog-2024-fnd.md`
- `docs/odf/general-interface/`
- `docs/odf/disciplines/{fbl,bkb,bdm,vbv,box,ath}/`

## 2. Alcance

### En scope

- Entidades del core: Competition, Discipline, Event, Phase, Unit, Subunit, Person, Organisation, Team, Entry, Composition, OfficialAssignment.
- Identidades tipadas (Vogen, Guid v7) y value objects.
- RSC (Results System Code) como código derivado y persistido.
- Contrato `IDisciplineModule` y `DisciplineRegistry`.
- Invariantes de dominio.
- Organización física en proyectos `.csproj`.

### Fuera de scope (specs futuras)

- Persistencia y mapping EF Core.
- Formatos de competición (pools, brackets, ligas) y reglas de standings.
- Operación en vivo (marcadores, eventos de juego, play-by-play).
- Lineup por Unit (start lists para un match concreto).
- Replacement athletes (AA01/AP01).
- Export/import a mensajes ODF (DT_RESULT, DT_SCHEDULE, etc.).
- Localización multilenguaje (ODF Language Guidelines).
- Multi-tenancy.

## 3. Decisiones tomadas

| # | Decisión | Resumen |
|---|---|---|
| D1 | Alcance | Solo modelo conceptual; no persistencia, no live ops, no formatos. |
| D2 | Fidelidad ODF | Tomamos vocabulario y estructura; no replicamos mensajes ni catálogo completo. |
| D3 | Especialización | Core ciego + módulos plug-in por disciplina. |
| D4 | Participantes | Person global + Composition por evento + Entry siempre apunta a Composition. |
| D5 | Identidad de equipo | Person + Organisation + Team estable opt-in + Entry tipa Athlete/Team/Group. Organisation obligatoria. |
| D6 | Rol de RSC | Código externo derivado y persistido. IDs internos tipados (Vogen + Guid v7). Índice único por `(CompetitionId, Rsc)`. |
| D7 | Officials | Person + OfficialAssignment polimórfico (Person + Function + Scope). Functions comunes en core; específicas por disciplina. |
| D8 | Organización | Un solo bounded context `Sport` con submódulos (Competitions, Structure, Participants, Officials, DisciplineRegistry). |
| D9 | Proyectos físicos | `Sport.Core` monolítico (folders por submódulo) + un `csproj` por módulo de disciplina. |
| D10 | Team.Code | Único global. |
| D11 | Composition (nombre) | Usamos el término ODF "Composition" en lugar de "Roster". |

## 4. Organización

### 4.1 Bounded context y submódulos

Un único bounded context `Sport` con cinco submódulos cohesivos:

```
Sport (bounded context único)
├── Competitions       Identidad de la competición y disciplinas activas
├── Structure          Event → Phase → Unit (+ Subunit) + RSC
├── Participants       Person, Organisation, Team, Entry, Composition
├── Officials          OfficialAssignment (Person + Function + Scope)
└── DisciplineRegistry Contratos abstractos + registro de módulos por disciplina
```

Reglas:
- Cada submódulo expone sus tipos públicos y oculta los internos.
- Referencias cruzadas entre submódulos siempre por **ID interno tipado** (Vogen).
- Ningún submódulo conoce los módulos concretos de disciplina; solo interactúa con los contratos del `DisciplineRegistry`.

### 4.2 Estructura física

```
src/
├── Sport.Core/                       (1 csproj)
│   ├── Competitions/                 (folder)
│   ├── Structure/                    (folder)
│   ├── Participants/                 (folder)
│   ├── Officials/                    (folder)
│   └── DisciplineRegistry/           (folder, contratos abstractos)
├── Sport.Disciplines.FBL/            (1 csproj → Sport.Core)
├── Sport.Disciplines.BKB/            (1 csproj → Sport.Core)
├── Sport.Disciplines.BDM/            (1 csproj → Sport.Core)
├── Sport.Disciplines.VBV/            (1 csproj → Sport.Core)
├── Sport.Disciplines.BOX/            (1 csproj → Sport.Core)
└── Sport.Disciplines.ATH/            (1 csproj → Sport.Core)
```

Frontera dura solo donde aporta valor real (core ↔ disciplinas). Dentro de `Sport.Core`, la disciplina del submódulo la sostiene NetArchTest.

## 5. Submódulo `Competitions`

Identifica una competición y declara qué disciplinas participan.

```
Competition  (agregado raíz)
├── Id : CompetitionId               (Vogen, Guid v7)
├── Code : CompetitionCode           (Vogen string, único, slug)
├── Name : string
├── Dates : DateRange                (start/end inclusive)
└── Disciplines : List<CompetitionDiscipline>

CompetitionDiscipline  (entidad hija)
├── Id : CompetitionDisciplineId
├── CompetitionId : CompetitionId
├── Code : DisciplineCode            (FBL | BKB | BDM | VBV | BOX | ATH)
└── EnabledGenders : Set<GenderCode> (M, W, X, O)
```

### Invariantes

- I-COMP-1. `Competition.Disciplines.Count >= 1`.
- I-COMP-2. `CompetitionDiscipline.Code` debe estar registrado en el `DisciplineRegistry`.
- I-COMP-3. `DisciplineCode` único dentro de una `Competition`.
- I-COMP-4. `EnabledGenders ⊆ DisciplineModule.SupportedGenders`.

## 6. Submódulo `Structure`

Jerarquía `Event → Phase → Unit (+ Subunit)` y cómputo de RSC.

```
Event  (agregado raíz dentro de una CompetitionDiscipline)
├── Id : EventId                          (Vogen, Guid v7)
├── CompetitionDisciplineId : CompetitionDisciplineId
├── Gender : GenderCode                   (M | W | X | O)
├── EventType : EventTypeCode             (S(8), validado por módulo de disciplina)
├── EventModifier : EventModifierCode     (S(10), opcional, default "----------")
├── Name : string
├── Rsc : Rsc                             (computado)
└── Phases : List<Phase>

Phase  (entidad hija)
├── Id : PhaseId
├── EventId : EventId
├── PhaseCode : PhaseCode                 (S(4): FNL, SFNL, QFNL, R128, GPA..GPZ, RND1..RND4, HEAT, QUAL, PREL, REP...)
├── Order : int
├── Rsc : Rsc                             (computado)
└── Units : List<Unit>

Unit  (entidad hija; abstracta del core)
├── Id : UnitId
├── PhaseId : PhaseId
├── UnitCode : UnitCode                   (S(8), convención ODF)
├── ScheduledStart : DateTimeOffset?
├── Rsc : Rsc                             (computado)
├── Subunits : List<Subunit>              (vacía si atómico)
└── DisciplineUnitRef : Guid?             (puntero opaco al unit específico del módulo de disciplina)

Subunit  (entidad hija de Unit)
├── Id : SubunitId
├── UnitId : UnitId
├── SubunitCode : SubunitCode             (S(2), chars 7-8 del UnitCode)
└── Rsc : Rsc                             (computado)
```

### Cómputo de RSC

```
Rsc(Event)    = DDD + G + EEEEEEEE + MMMMMMMMMM + "----" + "--------"
Rsc(Phase)   = DDD + G + EEEEEEEE + MMMMMMMMMM + PPPP  + "--------"
Rsc(Unit)    = DDD + G + EEEEEEEE + MMMMMMMMMM + PPPP  + UUUUUU + "--"   (unit atómico)
              DDD + G + EEEEEEEE + MMMMMMMMMM + PPPP  + UUUUUU + "00"   (parent de subunits)
Rsc(Subunit) = DDD + G + EEEEEEEE + MMMMMMMMMM + PPPP  + UUUUUU + SS
```

Total fijo: 34 caracteres. Filler `-`. Padding a la derecha. Charset: `A-Z 0-9 . -`. Uppercase.

`Rsc` se modela como value object Vogen con:
- `Rsc.From(string)` — valida y construye.
- `Rsc.Compose(discipline, gender, eventType, modifier, phase?, unit?, subunit?)` — arma desde piezas.
- Métodos de extracción: `Level`, `Discipline`, `Gender`, `EventType`, `EventModifier`, `Phase`, `Unit`, `Subunit`.

### Generación de `UnitCode`

- El core auto-genera secuencial por default (`000100--`, `000200--`, ...) a través de `IUnitCodeStrategy` del módulo.
- El módulo de disciplina puede sobrescribir cuando hay convención propia (ej. ATH usa `SJ` para Super Jump).
- El core enforce el shape (8 chars), el contenido significativo lo decide el módulo.

### Invariantes

- I-STR-1. `Event.Gender ∈ CompetitionDiscipline.EnabledGenders`.
- I-STR-2. `Event.EventType` válido para la `DisciplineCode` (vía `IDisciplineModule.ValidateEventType`).
- I-STR-3. `Phase.PhaseCode` válido para `(DisciplineCode, EventType)` (vía `IDisciplineModule.ValidatePhaseForEventType`).
- I-STR-4. `Phase.Order` único dentro de `Event`.
- I-STR-5. `Phase.PhaseCode` único dentro de `Event`.
- I-STR-6. `Unit.UnitCode` único dentro de `Phase`.
- I-STR-7. Si `Unit` tiene subunits, `UnitCode` termina en `"00"`.
- I-STR-8. `Subunit.SubunitCode` único dentro de `Unit`.
- I-STR-9. `Rsc` único dentro de la `Competition` (índice único `(CompetitionId, Rsc)`).

### Especialización por disciplina (referencia, no herencia)

Cada disciplina define su entidad específica que **referencia** una `Unit` del core por FK; **no hereda** de ella. El core persiste `DisciplineUnitRef` como `Guid?` opaco. Queries desde el módulo de disciplina pueden ir directamente por `UnitId` sin pasar por el registry; queries transversales desde el core consultan el registry para resolver el tipo concreto.

## 7. Submódulo `Participants`

Person, Organisation, Team, Entry y Composition. Vocabulario ODF directo.

```
Person  (agregado raíz, global)
├── Id : PersonId
├── FamilyName : string                 (mixed case, S(50))
├── GivenName : string?                 (mixed case, S(50))
├── Gender : GenderCode                 (M | W | X)
├── BirthDate : DateOnly?
└── IFId : string?                      (Federation ID, S(20))

Organisation  (agregado raíz, master data global)
├── Id : OrganisationId
├── Code : OrganisationCode             (S(3-10), único: "ESP", "BRA-CLUB-001", "MIX1")
├── Name : string
└── Type : OrganisationType             (Noc | Club | Federation | School | Group | Other)

Team  (agregado raíz, opt-in por disciplina)
├── Id : TeamId
├── Code : TeamCode                     (S(20), único global)
├── Name : string
├── OrganisationId : OrganisationId
└── DisciplineCode : DisciplineCode

Entry  (agregado raíz, por Event)
├── Id : EntryId
├── EventId : EventId
├── Type : EntryType                    (Athlete | Team | Group)
├── OrganisationId : OrganisationId     (siempre obligatoria)
├── TeamId : TeamId?                    (obligatorio sii Type = Team)
├── Bib : Bib?                          (S(20))
├── Seed : int?
├── Status : EntryStatus                (Registered | Withdrawn | Disqualified | Replaced)
└── Composition : List<CompositionMember>

CompositionMember  (entidad hija de Entry)
├── EntryId : EntryId
├── PersonId : PersonId
├── Order : int
└── Bib : Bib?
```

### Invariantes

- I-PAR-1. Composition size por Type:
  - `Athlete` → exactamente 1.
  - `Team` → ≥ 1 y `TeamId != null`.
  - `Group` → ≥ 2 y `TeamId == null`.
- I-PAR-2. `TeamId` presente sii `Type = Team`.
- I-PAR-3. `OrganisationId` siempre presente. Grupos mixtos usan `Organisation` con `Type = Group` y código `MIXn`.
- I-PAR-4. Si `Type = Team`: `Team.DisciplineCode == Event.CompetitionDiscipline.Code`.
- I-PAR-5. Una `PersonId` no puede aparecer en dos `Entry` "vigentes" del mismo `Event`. "Vigente" = `Status ∉ {Withdrawn, Replaced}` (es decir, una Entry Disqualified sigue ocupando el slot histórico, pero una Withdrawn o Replaced libera el slot para reasignar). Sí puede aparecer en Events distintos del mismo torneo (ej. BDM singles + BDM doubles).
- I-PAR-6. `Order` único dentro de `Composition`.
- I-PAR-7. `Bib` único dentro de `Event` cuando se asigna (invariante "blanda"; el módulo de disciplina puede relajarla).
- I-PAR-8. `Status` inicial = `Registered`. Transiciones son operaciones explícitas.

## 8. Submódulo `Officials`

Officials son Persons con una Function asignada en un Scope. El core no conoce el catálogo de Functions; cada módulo de disciplina aporta el suyo, más un set común expuesto por el core.

```
OfficialAssignment  (agregado raíz)
├── Id : OfficialAssignmentId
├── PersonId : PersonId
├── FunctionCode : FunctionCode           (S(20): "FBL.REF", "BOX.JUD1", "COMMON.COACH"...)
├── Scope : OfficialScope                 (value object polimórfico)
├── OrganisationId : OrganisationId?
└── Status : OfficialAssignmentStatus     (Active | Replaced | Removed)

OfficialScope  (value object)
├── Level : ScopeLevel                    (Competition | CompetitionDiscipline | Event | Phase | Unit)
└── TargetId : Guid                       (CompetitionId | CompetitionDisciplineId | EventId | PhaseId | UnitId)
```

### Functions comunes vs específicas

El `DisciplineRegistry` expone un conjunto base de `FunctionDescriptor` compartidos (`COMMON.COACH`, `COMMON.MANAGER`, `COMMON.MEDICAL`...). Cada módulo de disciplina añade las específicas (`FBL.REF`, `BOX.JUD1`, `ATH.STJU`, etc.).

```
FunctionDescriptor
├── Code : FunctionCode
├── DisplayName : string
├── ValidScopes : Set<ScopeLevel>
├── IsTeamOfficial : bool
└── RequiresOrganisation : bool
```

### Invariantes

- I-OFF-1. `FunctionCode` debe pertenecer al conjunto válido para la disciplina del Scope (vía `IDisciplineModule.ValidateOfficialFunctionInScope` o set común).
- I-OFF-2. `Scope.Level` debe estar en `FunctionDescriptor.ValidScopes`.
- I-OFF-3. `OrganisationId` obligatorio cuando `FunctionDescriptor.RequiresOrganisation = true`.
- I-OFF-4. `Scope.TargetId` debe existir.
- I-OFF-5. `Status` inicial = `Active`. Replaced y Removed son transiciones explícitas.
- I-OFF-6. Una `(PersonId, FunctionCode, Scope)` activa no se permite dos veces.

## 9. Submódulo `DisciplineRegistry`

Es el único contrato que tipa el core contra las disciplinas. Vive en `Sport.Core.DisciplineRegistry`.

### 9.1 `IDisciplineModule`

```
IDisciplineModule
├── DisciplineCode Code
├── string DisplayName
├── Set<GenderCode> SupportedGenders
│
├── EventTypes : IReadOnlyCollection<EventTypeDescriptor>
├── PhaseCatalog : IPhaseCatalog
├── UnitCodeStrategy : IUnitCodeStrategy
├── Functions : IReadOnlyCollection<FunctionDescriptor>
├── EntryRules : IEntryRules
│
├── ValidateEventType(EventTypeCode, EventModifierCode?) : Result
├── ValidatePhaseForEventType(EventTypeCode, PhaseCode) : Result
├── ValidateUnitCode(EventTypeCode, PhaseCode, UnitCode) : Result
├── ValidateEntry(EntryCandidate) : Result
└── ValidateOfficialFunctionInScope(FunctionCode, ScopeLevel) : Result
```

### 9.2 Componentes auxiliares

```
EventTypeDescriptor
├── Code : EventTypeCode
├── DisplayName : string
├── AppliesToGenders : Set<GenderCode>
└── ModifierContract : ModifierContract   (Forbidden | Optional | Required, con shape esperada)

IPhaseCatalog
├── IReadOnlyCollection<PhaseCode> AllowedPhases
└── IsAllowedForEventType(EventTypeCode, PhaseCode) : bool

IUnitCodeStrategy
├── string NextUnitCode(IEnumerable<UnitCode> existing)
└── bool IsValid(UnitCode code)

IEntryRules
├── EntryType[] AllowedTypes
├── (int min, int max) CompositionSize(EntryType type)
└── Result Validate(EntryCandidate candidate)
```

### 9.3 `IDisciplineRegistry`

```
IDisciplineRegistry
├── IDisciplineModule Get(DisciplineCode code)
├── bool IsRegistered(DisciplineCode code)
├── IReadOnlyCollection<DisciplineCode> RegisteredCodes
└── IReadOnlyCollection<FunctionDescriptor> CommonFunctions
```

### 9.4 Registración

Explícita en `Program.cs`. No hay autodiscovery por reflexión.

```csharp
services.AddSportCore();
services.AddDisciplineModule<FblModule>();
services.AddDisciplineModule<BkbModule>();
services.AddDisciplineModule<BdmModule>();
services.AddDisciplineModule<VbvModule>();
services.AddDisciplineModule<BoxModule>();
services.AddDisciplineModule<AthModule>();
```

### 9.5 Invariantes

- I-REG-1. Un único módulo por `DisciplineCode`.
- I-REG-2. El registry es read-only en runtime; todas las registraciones suceden al boot.
- I-REG-3. El core no depende de módulos específicos. NetArchTest valida que `Sport.Core.*` no referencia `Sport.Disciplines.*`.

### 9.6 Tipos específicos de disciplina (referencia, no herencia)

```
// Sport.Disciplines.FBL
FootballMatch  (agregado raíz dentro del módulo FBL)
├── Id : FootballMatchId
├── UnitId : UnitId                       (FK hacia Sport.Core.Structure.Unit)
└── (campos específicos en spec posterior)

// Sport.Disciplines.BOX
BoxingBout
├── Id : BoxingBoutId
├── UnitId : UnitId
└── ...

// Sport.Disciplines.ATH
HighJumpTrial
├── Id : HighJumpTrialId
├── UnitId : UnitId
└── ...
```

`Unit.DisciplineUnitRef` es el FK inverso opcional, persistido como `Guid?` opaco en el core.

### 9.7 Flujos canónicos (descripción conceptual)

**Crear un Event:**
1. Application service recibe `CreateEvent(competitionDisciplineId, gender, eventType, modifier?, name)`.
2. Resuelve `CompetitionDiscipline → DisciplineCode`.
3. `DisciplineRegistry.Get(code).ValidateEventType(eventType, modifier)`.
4. Verifica `gender ∈ DisciplineModule.SupportedGenders`.
5. Construye `Event` con `Rsc.Compose(...)`.
6. Persiste.

**Crear un Phase:**
1. Carga `Event` y resuelve disciplina.
2. `ValidatePhaseForEventType(eventType, phaseCode)`.
3. Construye `Phase` con RSC computado.

**Crear una Entry:**
1. Carga `Event` y resuelve disciplina.
2. `ValidateEntry(candidate)` → comprueba `AllowedTypes`, `CompositionSize`, `Organisation`.
3. Construye `Entry` y persiste.

**Crear un FootballMatch (módulo FBL):**
1. Endpoint FBL invoca al servicio de estructura del core (nombre concreto se define en spec de aplicación; conceptualmente: "crear Unit dentro de Phase"). El core devuelve la `Unit` creada con su `UnitId` y `Rsc`.
2. FBL crea `FootballMatch` con `UnitId = unit.Id`.
3. FBL llama de vuelta al core para enlazar: `unit.LinkDisciplineRef(footballMatch.Id)`.

El core nunca importa nada de FBL. FBL solo importa del core.

## 10. Cross-cutting

### 10.1 Identidad

- Vogen + `Guid v7` para todos los IDs de agregados raíz.
- IDs tipados: `CompetitionId`, `CompetitionDisciplineId`, `EventId`, `PhaseId`, `UnitId`, `SubunitId`, `PersonId`, `OrganisationId`, `TeamId`, `EntryId`, `OfficialAssignmentId`.

### 10.2 Value objects con Vogen

| VO | Definición |
|---|---|
| `Rsc` | S(34), validado, uppercase, charset `A-Z 0-9 . -` |
| `DisciplineCode` | S(3) uppercase |
| `GenderCode` | enum {M, W, X, O} |
| `EventTypeCode` | S(8) |
| `EventModifierCode` | S(10) |
| `PhaseCode` | S(4) |
| `UnitCode` | S(8) |
| `SubunitCode` | S(2) |
| `OrganisationCode` | S(3-10) |
| `TeamCode` | S(20) |
| `FunctionCode` | S(20) |
| `CompetitionCode` | slug |
| `Bib` | S(20) |

### 10.3 Catálogos de códigos

Los `CC @Discipline`, `CC @Phase`, etc. que ODF mantiene como master data son referencias externas. No se importan como tabla del core. Cada módulo de disciplina conoce sus códigos válidos y los expone vía sus descriptores. La validación es por módulo, no por lookup global.

### 10.4 Localización

`Name` displayable es string simple por ahora. Soporte multilenguaje (ODF Language Guidelines) es spec posterior.

### 10.5 Reglas NetArchTest

- `Sport.Core.*` no referencia `Sport.Disciplines.*`.
- `Sport.Disciplines.X` no referencia `Sport.Disciplines.Y` para X ≠ Y.
- Tipos en `Sport.Core.<Submodule>.Internal` no referenciados desde otros submódulos.
- Todos los IDs de agregados raíz son Vogen, no `Guid` crudo.

## 11. Sketch de módulos por disciplina

Validación de que el contrato `IDisciplineModule` cubre los 6 casos iniciales sin extensión.

| Disciplina | EventTypes principales | Phases típicas | EntryTypes | Composition | Functions específicas |
|---|---|---|---|---|---|
| **FBL** | `TEAM11` | `GPA..GPF`, `R32`, `R16`, `QFNL`, `SFNL`, `FNL` | Team | 11..23 | `FBL.REF`, `FBL.AREF`, `FBL.4OFF`, `FBL.VAR` |
| **BKB** | `TEAM5` | Pool + KO similar | Team | 5..12 | `BKB.REF`, `BKB.UMP` |
| **BDM** | `SINGLES`, `DOUBLES`, `MIXEDDOUB` | `GPA..`, `R32..FNL` | Athlete / Team | 1 / 2 | `BDM.UMP`, `BDM.SVJU`, `BDM.LIJU` |
| **VBV** | `TEAM2` (beach) | Pool + KO | Team | 2 | `VBV.REF1`, `VBV.REF2`, `VBV.LIJU` |
| **BOX** | `48KG`, `54KG`, `60KG`, `75KG`, ... | `R32..QFNL..SFNL..FNL` | Athlete | 1 | `BOX.REF`, `BOX.JUD1..JUD5`, `BOX.TIMK` |
| **ATH** | `HJ` | `QUAL`, `FNL` | Athlete | 1 | `ATH.STJU`, `ATH.PHOTOFIN` |

Funciones comunes (`COMMON.COACH`, `COMMON.MANAGER`, `COMMON.MEDICAL`) las expone el core para uso de cualquier disciplina.

Casos a anotar (no resolver acá):

- **BOX peso por categoría:** modelado como `EventType` distinto por categoría. La UX puede agrupar bajo "Categorías" como preset.
- **BDM dobles con pareja estable:** `Team` con `DisciplineCode = BDM` y 2 personas.
- **BDM mixed doubles:** `Team` con persons de género distinto. Composition lo soporta.
- **ATH multi-attempt** (high jump tiene múltiples intentos por trial): se modela dentro del módulo ATH en spec posterior (Trial → Attempts). El core solo ve el `Unit`.

## 12. Dependencias hacia adelante

Specs que se apoyan en este core:

| Spec siguiente | Depende de este core en |
|---|---|
| Persistencia + EF Core mapping | Todas las entidades y value objects |
| Competition formats (pools, brackets, league) | `Event`, `Phase`, `Unit`, `Entry` |
| Standings y reglas de puntuación | `Entry`, `Unit`, `CompetitionDiscipline` |
| Live ops (resultados, marcadores, eventos) | `Unit`, `Entry`, `Composition`, módulos de disciplina |
| Lineup por Unit (start lists) | `Composition`, `Entry`, `Unit` |
| Replacement athletes (AA01/AP01) | `Entry`, `Person` |
| Officials availability/scheduling | `OfficialAssignment`, `Unit` |
| Localización ODF (DT_PARTIC_NAME) | `Person`, `Organisation`, `Team` |
| Export a mensajes ODF | RSC, todas las entidades |

## 13. Riesgos abiertos

1. **Bloqueo de cambios estructurales con actividad.** Renombrar o reordenar Phases cuando ya hay results puede romper integraciones por RSC. Mitigación: bloqueo en spec de live ops; índice único ayuda a detectar conflictos.
2. **Doble salto en queries cross-discipline.** Core → registry → módulo específico añade indirección. Mitigación: queries específicas entran directamente por el módulo; el core es para queries transversales.
3. **Functions con namespace por prefix** (`FBL.REF`, `COMMON.COACH`). Convención de string, no estructural. Aceptable.
4. **`Bib` único por Event como invariante "blanda".** Flexibilizable por módulo. A confirmar en spec de live ops.
5. **`Subunit` modelado pero casi sin uso real** entre las 6 disciplinas iniciales. Costo bajo, se mantiene para cubrir el slot RSC y permitir futuras disciplinas que lo requieran.
6. **`Rsc.Compose` necesita la `DisciplineCode` al composición.** Cambios en la estructura recalculan RSCs; referencias externas pueden romperse. Mitigación: índice único, bloqueo de cambios con actividad.
7. **Multi-tenant fuera de scope.** Si se requiere, `TenantId` en agregados raíz es cambio aditivo.

## 14. Mapeo ODF ↔ modelo

Referencia rápida de equivalencias:

| Concepto ODF | Entidad/VO local |
|---|---|
| `RSC` (Result System Code) | `Rsc` value object |
| `DocumentCode` (Discipline-level) | `Rsc.Compose(disciplineCode)` |
| `DocumentCode` (Event-level) | `Event.Rsc` |
| `DocumentCode` (Phase-level) | `Phase.Rsc` |
| `DocumentCode` (Unit-level) | `Unit.Rsc` |
| `CC @Discipline` | `DisciplineCode` |
| `CC @Phase` | `PhaseCode` |
| `CC @Unit` | `Unit.Rsc` |
| `CC @Organisation` | `Organisation` |
| `Competitor` (Type T/A/G) | `Entry` con `Type` |
| `Composition/Athlete` | `Composition/CompositionMember` |
| `Participant` (Athlete) | `Person` |
| `Team` (DT_PARTIC_TEAMS) | `Team` |
| `Function` (Official) | `FunctionCode` + `FunctionDescriptor` |
| `Group` (Type G, MIXn) | `Entry.Type = Group` + `Organisation.Type = Group` |

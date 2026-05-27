# Periods por deporte en ODF/ORIS

Documento de referencia sobre el catálogo `SC@Period` — la **segmentación temporal de un Unit** (medios tiempos,
cuartos, sets, tiempos extra, tanda de penales). Es un patrón ODF universal, pero los códigos concretos son
sport-specific y **una de las disciplinas (vóley) usa numérico crudo en lugar de códigos alfanuméricos**.

> Fuentes: ORIS Football R8 v1.1 (2022), ORIS Basketball R8 v1.1 (2022), ORIS Volleyball R8 v1.1 (2022), ODF Data
> Dictionaries de FBL/BKB/VVO (SOG 2024 base). Validar contra el release exacto del evento objetivo.

---

## TL;DR

`SC@Period` segmenta el tiempo de juego dentro de un Unit (partido/match). Es un **catálogo sport-specific**: cada
deporte define los códigos que aplican según su estructura reglamentaria. **Vóley es la excepción notable**: en lugar de
códigos alfanuméricos usa un número entero como identificador de set.

No confundir:

- **Period** (este doc) → segmentos *dentro* de un Unit (partido en juego). Ej: `H1`, `Q3`, `S2`.
- **Phase** → fases *del torneo*. Ej: cuartos, semis, final.
- **CC@PHASE_TYPE** → tipo de actividad/sesión (ceremonia, training, competition, draw…).

Los tres viven en planos completamente distintos.

---

## 1. Para qué se usa `SC@Period`

`SC@Period` aparece como atributo `Pos` en mensajes que reportan datos *parciales* de un partido:

| Mensaje           | Dónde aparece                                               | Para qué                                           |
|-------------------|-------------------------------------------------------------|----------------------------------------------------|
| `DT_RESULT`       | `Competition/Periods/Period`                                | Marcador parcial al final de cada periodo          |
| `DT_RESULT`       | `Competition/Periods/Period/ExtendedPeriods/ExtendedPeriod` | Sub-info por periodo (tiempo agregado, etc.)       |
| `DT_STATS`        | `StatsItem Pos="SC@Period"`                                 | Estadísticas desglosadas por periodo               |
| `DT_PLAY_BY_PLAY` | `Action Period="..."`                                       | Cada acción se marca con el periodo en que ocurrió |

Convención compartida por todos los deportes: el código `TOT` representa el **acumulado total del partido** (suma o
resultado final), no un periodo real.

---

## 2. Códigos por deporte

### Fútbol (FBL) — `SC@Period`

| Código | Significado                          |
|--------|--------------------------------------|
| `H1`   | Primer tiempo                        |
| `H2`   | Segundo tiempo                       |
| `ET1`  | Primer tiempo extra                  |
| `ET2`  | Segundo tiempo extra                 |
| `PSO`  | Tanda de penales (Penalty Shoot-Out) |
| `TOT`  | Total acumulado del partido          |

> Aplican siempre que el reglamento del torneo permita el tiempo correspondiente. `ET1`/`ET2`/`PSO` solo en
> eliminatorias o partidos que requieran ganador.

### Básquet (BKB) — `SC@Period`

| Código | Significado                                             |
|--------|---------------------------------------------------------|
| `Q1`   | Primer cuarto                                           |
| `Q2`   | Segundo cuarto                                          |
| `Q3`   | Tercer cuarto                                           |
| `Q4`   | Cuarto cuarto                                           |
| `OTn`  | Cada tiempo extra (`OT1`, `OT2`, … en orden ascendente) |
| `TOT`  | Total acumulado del partido                             |

Cita textual del Data Dictionary BKB:

> *"Only use full periods or OTn for each overtime if applicable. (Q1, Q2, Q3, Q4, OTn …)"*
> *"Pos Description: Send the period (for Qn and OT [all]) or TOT"*

#### Caso especial — `Q2_H` (medio cuarto)

En samples del Data Dictionary aparece el código `Q2_H` para representar el marcador a mitad de un cuarto, pero **solo
se usa para reportar marcador parcial**, no como dimensión válida en `Pos` para stats. La instrucción explícita es: *"
Only send for full quarters... and each overtime"*.

Para Sportivo: tratamos `Q2_H` como caso de borde no soportado en V1. Si en algún momento queremos exposición de
marcador-en-vivo a mitad de cuarto, lo reabrimos.

### Vóley (VVO) — ⚠️ NO usa código alfanumérico

Vóley **no tiene códigos tipo `S1`, `S2`** en `SC@Period`. La estructura reglamentaria está modelada de otra forma:

- Dentro de `DT_RESULT` los sets se exponen como elementos hermanos con un atributo numérico **`Set number M Numeric`**.
- En `DT_STATS`, cuando aplica `Pos=SC@Period`, la instrucción es: *"Send for each period and TOT"* — el valor enviado
  es directamente el número del set o `TOT`.
- En `DT_PLAY_BY_PLAY`, el sort se hace por *"Set number"* y los textos auxiliares para descansos usan *"End of 1st
  Set / End of 2nd Set"* (texto humano, no código).

#### Tabla práctica para Sportivo (vóley)

| Forma usada             | Significado                                  |
|-------------------------|----------------------------------------------|
| `1`, `2`, `3`, `4`, `5` | Cada set del partido (vóley a 5 sets máximo) |
| `TOT`                   | Total acumulado del partido                  |

> Esto rompe la simetría con FBL/BKB y obliga a que el **modelo interno de Sportivo trate `SC@Period` como string opaco
**, no como un enum cerrado por deporte. El catálogo per-deporte define qué valores válidos aceptar; la columna en
> BD/DTO es `string`.

### Tabla comparativa

| Deporte       | Estructura típica             | Códigos `SC@Period`                    | Tipo de valor                           |
|---------------|-------------------------------|----------------------------------------|-----------------------------------------|
| Fútbol (FBL)  | 2 tiempos × 45 min + ET + PSO | `H1`, `H2`, `ET1`, `ET2`, `PSO`, `TOT` | Alfanumérico cerrado                    |
| Básquet (BKB) | 4 cuartos × 10 min + OT(s)    | `Q1`–`Q4`, `OT1`, `OT2`, …, `TOT`      | Alfanumérico, `OT*` abierto al alza     |
| Vóley (VVO)   | Best-of-5 sets                | `1`, `2`, `3`, `4`, `5`, `TOT`         | Numérico (string `"1"`, `"2"`…) + `TOT` |

---

## 3. Implicaciones para Sportivo

### Modelo de dominio

- Definir `Period` como **value object string** (no enum cerrado), con un **catálogo per-deporte** que valida los
  códigos aceptables.
- En V1 sembrar tres catálogos: `Period.FBL`, `Period.BKB`, `Period.VVO` con los valores listados arriba.
- El atributo `period` en filas de stats / play-by-play / score-by-period es siempre `string`.



## 4. Glosario rápido

| Término         | Significado                                                                 |
|-----------------|-----------------------------------------------------------------------------|
| **Period**      | Segmento de tiempo dentro de un Unit/match (medio tiempo, cuarto, set).     |
| **Phase**       | Fase del torneo (cuartos de final, semis…). No relacionado con Period.      |
| **`SC@Period`** | Catálogo sport-specific con los códigos válidos para cada deporte.          |
| **`TOT`**       | Convención compartida: total acumulado del partido (no es un periodo real). |
| **`OTn`**       | Convención de básquet: cada overtime numerado en orden (`OT1`, `OT2`…).     |

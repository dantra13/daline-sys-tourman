# Formato generico: liga round-robin

Formato en el que cada participante disputa partidos contra un conjunto definido de rivales y la clasificacion se decide
por una tabla acumulada. Puede usarse como competicion completa o como fase previa para alimentar otro formato.

## Complejidad administrativa

Nivel: 3/5, media.

La administracion es mas exigente que una eliminatoria porque hay que generar calendario, acumular tabla, resolver
desempates y manejar partidos pendientes. La complejidad sube cuando el calendario no es todos-contra-todos completo,
cuando hay divisiones/conferencias, cuando hay muchos criterios de desempate o cuando la tabla alimenta varias rutas de
progresion.

## Parametros configurables

- Numero de participantes.
- Alcance del calendario: todos contra todos completo, doble vuelta, vueltas multiples o calendario parcial.
- Criterio de localia: neutral, alternada, ida/vuelta o asignada por calendario.
- Sistema de puntos: por victoria/empate/derrota, por sets, por bonus, por porcentaje de victorias o por ranking.
- Criterios de desempate.
- Reglas de progresion: campeon directo, clasificacion a otra fase, descenso, eliminacion o repechage.

## Estructura

Todos los participantes comparten una tabla de posiciones. Cada partido produce estadisticas acumulables: jugados,
ganados, perdidos, empatados si aplica, puntos a favor, puntos en contra, diferencia, puntos de tabla y cualquier
metrica
propia de la disciplina.

El formato no exige que todos enfrenten a todos. Cuando el calendario es parcial, el modelo debe distinguir entre
`round-robin completo` y `liga con calendario parcial`, porque los criterios de desempate pueden necesitar compensar
fuerza de rivales o porcentaje de victorias.

## Desempates

Los desempates son configurables por competicion. Los mas comunes son:

1. Resultado directo entre participantes empatados.
2. Diferencia de puntos/goles/sets.
3. Puntos/goles/sets a favor.
4. Record dentro de grupo, division o conferencia.
5. Record contra participantes clasificados o elegibles.
6. Criterios disciplinarios o fair play.
7. Ranking previo, coeficiente o sorteo.

Si hay empates multiples, el motor debe permitir aplicar una regla al subconjunto empatado y luego reiniciar criterios
cuando el empate se reduce a menos participantes.

## Variantes

- Liga unica.
- Liga por conferencias o divisiones.
- Grupo round-robin dentro de una fase de grupos.
- Liga parcial con calendario no simetrico.
- Liga con puntos bonus.
- Liga con porcentaje de victorias en vez de puntos absolutos.

## Consideraciones para modelado

- Separar tabla publicada de calculo interno de desempates.
- Guardar el alcance de cada estadistica: global, grupo, division, conferencia, local/visita, rival directo.
- No asumir que `Rank` basta para ordenar; guardar tambien `SortOrder` o equivalente.
- Permitir standings intermedios y finales.
- Permitir que la tabla alimente plazas distintas: clasificacion directa, play-in, eliminatoria, descenso o premios.

## Fuentes y ejemplos

- FIFA, competiciones con fase de grupos y tablas
  round-robin: [FIFA World Cup 26 match schedule](https://www.fifa.com/en/tournaments/mens/worldcup/canadamexicousa2026/articles/updated-fifa-world-cup-2026-match-schedule-now-available?searchOverlay=1).
- NBA, tabla por conferencia y porcentaje de victorias: [NBA standings](https://www.nba.com/standings).
- FIBA, competiciones con group phase y clasificacion por
  grupo: [FIBA competition system examples](https://www.fiba.basketball/).

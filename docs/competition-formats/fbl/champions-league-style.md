# Formato estilo UEFA Champions League

Referencia basada en el reglamento oficial de la UEFA Champions League 2026/27, vigente desde el 1 de marzo de 2026.
Última verificación de fuentes: 2026-05-28.
Este modelo reemplaza la fase de grupos tradicional por una fase de liga única y usa la posición final de esa fase para
sembrar la fase eliminatoria.

## Estructura general

- Participan 36 clubes en una sola clasificación general.
- La fase inicial es una fase de liga, no una liga todos contra todos.
- Cada club juega 8 partidos contra 8 rivales distintos: 4 de local y 4 de visitante.
- La tabla otorga 3 puntos por victoria, 1 por empate y 0 por derrota.
- Los clubes ubicados del 1 al 8 clasifican directamente a octavos de final.
- Los clubes ubicados del 9 al 24 juegan los play-offs de la fase eliminatoria.
- Los clubes ubicados del 25 al 36 quedan eliminados.
- Desde los play-offs hasta semifinales, las eliminatorias son a doble partido. La final es a partido único.

## Sorteo y rivales de fase de liga

Los 36 clubes se dividen en 4 bombos de 9 equipos según el coeficiente UEFA de clubes al inicio de la temporada. El
campeón vigente de la Champions League es el primer cabeza de serie del bombo 1.

Cada club recibe 2 rivales de cada bombo: uno como local y otro como visitante. En principio, no puede enfrentar a equipos
de su misma asociación nacional y puede enfrentar como máximo a 2 clubes de una misma asociación distinta. UEFA puede
ajustar condiciones del sorteo para evitar bloqueos o atender restricciones operativas aprobadas por sus comités.

## Calendario de fase de liga

Como regla de diseño del calendario, un club no debería jugar más de 2 partidos consecutivos como local ni más de 2
consecutivos como visitante. Además, en las 2 primeras jornadas y en las 2 últimas jornadas cada club debería tener 1
partido de local y 1 de visitante. UEFA puede hacer excepciones si lo considera necesario.

## Criterios de desempate en fase de liga

Si 2 o más equipos terminan empatados a puntos al finalizar la fase de liga, se aplican estos criterios en orden:

1. Mejor diferencia de goles en la fase de liga.
2. Mayor cantidad de goles marcados en la fase de liga.
3. Mayor cantidad de goles marcados como visitante en la fase de liga.
4. Mayor cantidad de victorias en la fase de liga.
5. Mayor cantidad de victorias como visitante en la fase de liga.
6. Mayor cantidad de puntos obtenidos colectivamente por los rivales que enfrentó el equipo en la fase de liga.
7. Mejor diferencia de goles colectiva de esos rivales en la fase de liga.
8. Mayor cantidad de goles marcados colectivamente por esos rivales en la fase de liga.
9. Menor total de puntos disciplinarios por tarjetas amarillas y rojas recibidas por jugadores y oficiales del equipo en
   todos los partidos de la fase de liga: roja = 3 puntos, amarilla = 1 punto, expulsión por doble amarilla = 3 puntos.
10. Mayor coeficiente UEFA de club.

## Play-offs de la fase eliminatoria

Los equipos clasificados del 9 al 24 entran en los play-offs. El sorteo se construye por pares de posiciones:

| Equipos sembrados | Posibles rivales no sembrados |
| --- | --- |
| 9 / 10 | 23 / 24 |
| 11 / 12 | 21 / 22 |
| 13 / 14 | 19 / 20 |
| 15 / 16 | 17 / 18 |

En principio, los equipos sembrados juegan el partido de vuelta en casa. Esto premia terminar mejor posicionado en la fase
de liga, aunque UEFA conserva margen para adaptar condiciones del sorteo o del calendario por restricciones operativas.

## Octavos, cuartos y semifinales

Los equipos clasificados del 1 al 8 esperan en octavos de final y también se agrupan por pares de siembra:

| Equipos sembrados para octavos |
| --- |
| 1 / 2 |
| 3 / 4 |
| 5 / 6 |
| 7 / 8 |

Cada par se sortea dentro del cuadro contra los ganadores correspondientes de los play-offs. En principio, los sembrados
también juegan el partido de vuelta de octavos en casa.

En cuartos de final, los equipos ubicados del 1 al 4 al terminar la fase de liga tienen derecho a jugar la vuelta en casa
si llegan a esa ronda. En semifinales, ese beneficio aplica para los equipos ubicados 1 y 2. Si un equipo sembrado queda
eliminado, el equipo que lo elimina hereda su posición de siembra dentro de ese camino del cuadro; la siembra no se
recalcula después de cada ronda.

## Resolución de eliminatorias

En eliminatorias a doble partido, avanza el equipo con mayor cantidad de goles en el marcador global. Si el global queda
empatado después del segundo partido, se juegan 2 tiempos extra de 15 minutos. Si el empate persiste tras la prórroga, la
serie se define por tiros penales bajo las Reglas de Juego de IFAB.

No se usa la regla de gol de visitante como criterio para decidir una eliminatoria empatada; los goles como visitante solo
aparecen como criterio de desempate de la tabla de fase de liga.

## Final

La final se juega a partido único. El equipo ubicado en el lado "silver" del cuadro se considera local nominal para fines
administrativos, no por ventaja deportiva obtenida en la fase de liga.

## Consideraciones para modelado

- La fase de liga debe modelarse como una tabla global con calendario parcial, no como grupos independientes.
- La posición final de fase de liga es un dato estructural: define clasificación, ruta del cuadro, siembra y posibles
  ventajas de vuelta en casa.
- Los desempates de tabla no son head-to-head; dependen primero de métricas globales del equipo y luego de métricas
  agregadas de sus rivales.
- Las localías de ida/vuelta en eliminatorias deben tratarse como reglas de siembra "en principio", sujetas a ajustes
  administrativos de UEFA.
- En doble partido, el resultado de la serie debe calcularse por marcador global, prórroga y penales; no por gol de
  visitante.

## Fuentes oficiales

- UEFA, [Regulations of the UEFA Champions League 2026/27](https://documents.uefa.com/r/Regulations-of-the-UEFA-Champions-League-2026/27-Online).
- UEFA, [Article 16 - Draw system: league phase](https://documents.uefa.com/r/Regulations-of-the-UEFA-Champions-League-2026/27/Article-16-Draw-system-league-phase-Online).
- UEFA, [Article 17 - Match system: league phase](https://documents.uefa.com/r/Regulations-of-the-UEFA-Champions-League-2026/27/Article-17-Match-system-league-phase-Online).
- UEFA, [Article 18 - Equality of points: league phase](https://documents.uefa.com/r/Regulations-of-the-UEFA-Champions-League-2026/27/Article-18-Equality-of-points-league-phase-Online).
- UEFA, [Article 19 - Draw system: knockout phase](https://documents.uefa.com/r/Regulations-of-the-UEFA-Champions-League-2026/27/Article-19-Draw-system-knockout-phase-Online).
- UEFA, [Article 20 - Match system: knockout phase](https://documents.uefa.com/r/Regulations-of-the-UEFA-Champions-League-2026/27/Article-20-Match-system-knockout-phase-Online).
- UEFA, [Article 21 - Knockout system, extra time and penalty shoot-outs](https://documents.uefa.com/r/Regulations-of-the-UEFA-Champions-League-2026/27/Article-21-Knockout-system-extra-time-and-penalty-shoot-outs-Online).

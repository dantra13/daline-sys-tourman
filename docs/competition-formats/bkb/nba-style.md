# Formato estilo NBA

Referencia basada en fuentes oficiales de NBA.com y NBA Communications para la temporada 2025/26. Ultima verificacion de
fuentes: 2026-05-28.

El formato NBA combina una temporada regular larga por conferencias, un torneo Play-In para definir las ultimas plazas de
playoffs, una fase final por series al mejor de 7 partidos y una copa de temporada integrada parcialmente al calendario
regular.

## Estructura general

- Participan 30 equipos.
- La liga se divide en 2 conferencias: Eastern Conference y Western Conference.
- Cada conferencia tiene 15 equipos y 3 divisiones de 5 equipos.
- Cada equipo disputa 82 partidos de temporada regular.
- La clasificacion principal se calcula por porcentaje de victorias y derrotas.
- Los equipos ubicados del 1 al 6 de cada conferencia clasifican directamente a playoffs.
- Los equipos ubicados del 7 al 10 de cada conferencia juegan el Play-In Tournament.
- Los equipos ubicados del 11 al 15 de cada conferencia quedan eliminados de la postemporada.
- Los playoffs se juegan por conferencia hasta definir campeon del Este y campeon del Oeste.
- Los campeones de conferencia se enfrentan en las NBA Finals.

## Temporada regular

La temporada regular es una liga con calendario balanceado parcialmente, no un todos contra todos puro. Las posiciones de
postemporada se determinan dentro de cada conferencia, aunque el calendario incluye partidos contra equipos de ambas
conferencias.

En 2025/26, la NBA publico inicialmente fechas y rivales para 80 de los 82 partidos de cada equipo. Los 2 partidos
restantes se determinan con base en los resultados de la fase de grupos de la Emirates NBA Cup.

Para efectos de modelado, la temporada regular debe guardar como minimo:

- conferencia;
- division;
- partidos ganados y perdidos;
- porcentaje de victorias;
- ranking de conferencia;
- ranking de division;
- elegibilidad de Play-In y playoffs;
- metricas de desempate.

## Criterios de desempate de temporada regular

La NBA define desempates para clasificacion a postemporada, siembra de postemporada y campeones divisionales.

### Empate entre 2 equipos

1. Mejor porcentaje de victorias en partidos directos entre ellos.
2. Campeon de division sobre equipo que no gano division.
3. Mejor porcentaje de victorias contra equipos de la misma division, solo si ambos equipos estan en la misma division.
4. Mejor porcentaje de victorias contra equipos de la misma conferencia.
5. Mejor porcentaje de victorias contra equipos elegibles a postemporada en la misma conferencia.
6. Mejor porcentaje de victorias contra equipos elegibles a postemporada en la conferencia opuesta.
7. Mejor diferencial neto de puntos en todos los partidos.

### Empate entre mas de 2 equipos

1. Campeon de division sobre equipos que no ganaron division.
2. Mejor porcentaje de victorias en todos los partidos entre los equipos empatados.
3. Mejor porcentaje de victorias contra equipos de la misma division, solo si todos los equipos empatados estan en la
   misma division.
4. Mejor porcentaje de victorias contra equipos de la misma conferencia.
5. Mejor porcentaje de victorias contra equipos elegibles a postemporada en la misma conferencia.
6. Mejor diferencial neto de puntos en todos los partidos.

Los desempates para definir campeones de division se resuelven antes que otros empates. Si un desempate se usa solamente
para determinar campeon divisional, ese resultado no se reutiliza automaticamente para otros propositos de siembra.

## Play-In Tournament

El Play-In se juega despues de la temporada regular y antes de la primera ronda de playoffs. Lo disputan los equipos 7, 8,
9 y 10 de cada conferencia. Define las semillas 7 y 8 de cada conferencia para los playoffs.

El formato por conferencia es:

1. El 7 recibe al 8. El ganador entra a playoffs como semilla 7.
2. El 9 recibe al 10. El perdedor queda eliminado.
3. El perdedor del partido 7-vs-8 recibe al ganador del partido 9-vs-10.
4. El ganador de ese ultimo partido entra a playoffs como semilla 8.

Este bloque es un patron generico reutilizable: mini-eliminatoria de 4 equipos para asignar 2 plazas, con doble
oportunidad para las semillas 7 y 8 y eliminacion directa para las semillas 9 y 10.

## Playoffs

Cada conferencia arma un cuadro de 8 equipos:

| Primera ronda | Cruce |
| --- | --- |
| 1 vs 8 | Mejor sembrado contra segunda plaza de Play-In |
| 2 vs 7 | Segundo sembrado contra primera plaza de Play-In |
| 3 vs 6 | Siembra directa |
| 4 vs 5 | Siembra directa |

Todas las rondas de playoffs se juegan al mejor de 7 partidos:

- primera ronda;
- semifinales de conferencia;
- finales de conferencia;
- NBA Finals.

El formato de localias es 2-2-1-1-1. El equipo con ventaja de campo recibe los partidos 1, 2, 5 y 7; el rival recibe los
partidos 3, 4 y 6. Los partidos 5, 6 y 7 se juegan solo si son necesarios.

En general, la ventaja de campo corresponde al equipo con mejor record de temporada regular. Para modelado, no conviene
inferirla solo por numero de semilla, porque puede depender del record y de desempates.

## NBA Finals

Las NBA Finals enfrentan al campeon de la Eastern Conference contra el campeon de la Western Conference. Tambien son una
serie al mejor de 7 con formato 2-2-1-1-1.

## Emirates NBA Cup

La NBA Cup es una competicion de temporada dentro de la temporada regular. No reemplaza la carrera por el campeonato NBA,
pero si afecta el calendario y, salvo la final de la Cup, sus partidos cuentan para la clasificacion de temporada regular.

Estructura:

- Participan los 30 equipos.
- Hay 6 grupos de 5 equipos: 3 grupos por conferencia, armados dentro de cada conferencia con base en records de la
  temporada regular anterior.
- Cada equipo juega 4 partidos de fase de grupos contra los otros equipos de su grupo: 2 de local y 2 de visitante.
- Avanzan 8 equipos a rondas eliminatorias: los 6 ganadores de grupo y 1 wild card por conferencia.
- Las rondas eliminatorias son a partido unico: cuartos, semifinales y final.
- Los cuartos se juegan en mercados NBA; semifinales y final se juegan en sede neutral.
- Todos los partidos de la NBA Cup cuentan para temporada regular excepto la final de la Cup.

### Desempates de NBA Cup

Para desempates dentro de un grupo:

1. Record directo en fase de grupos.
2. Diferencial de puntos en fase de grupos.
3. Total de puntos anotados en fase de grupos.
4. Record de la temporada regular anterior.
5. Sorteo aleatorio.

Los puntos de overtime no cuentan para diferencial de puntos ni para total de puntos en estos desempates. Si un partido de
fase de grupos llega a overtime, su diferencial cuenta como 0 para NBA Cup y el total de puntos excluye puntos anotados en
overtime.

Para wild cards se usa un protocolo equivalente, salvo que el head-to-head de fase de grupos no aplica de la misma forma
porque los equipos candidatos pueden venir de grupos distintos.

## Consideraciones para modelado

- Separar claramente `RegularSeason`, `PlayIn`, `Playoffs` y `Cup`; comparten equipos y partidos, pero no son la misma
  entidad competitiva.
- La clasificacion de temporada regular es por conferencia; la division existe y participa en desempates, pero no otorga
  automaticamente una semilla alta por encima del record.
- El Play-In no es una ronda de playoffs tradicional: produce semillas 7 y 8.
- Las series de playoffs deben modelarse como `bestOf=7`, con localia 2-2-1-1-1 y partidos condicionales.
- La NBA Cup debe permitir partidos que cuentan simultaneamente para temporada regular y para fase de grupos de la copa.
- La final de NBA Cup es una excepcion: cuenta para la copa, pero no para el record de temporada regular.
- Los desempates de NBA Cup usan reglas distintas a los desempates de temporada regular.

## Patrones genericos identificados

- Liga por conferencias con divisiones internas.
- Tabla de temporada regular por porcentaje de victorias.
- Desempates jerarquicos para siembra.
- Play-In de 4 equipos por 2 plazas.
- Bracket de 8 equipos por conferencia.
- Series al mejor de 7 con formato 2-2-1-1-1.
- Copa integrada a una liga principal, con partidos de doble pertenencia.
- Fase de grupos con wild cards y eliminatoria a partido unico.

## Fuentes oficiales

- NBA, [About The NBA](https://www.nba.com/news/about).
- NBA, [NBA 2025-26 Regular Season Standings](https://www.nba.com/standings).
- NBA, [2025-26 NBA regular season schedule](https://www.nba.com/news/2025-26-nba-regular-season-schedule).
- NBA, [Everything to know about 2026 SoFi NBA Play-In Tournament](https://www.nba.com/news/nba-play-in-tournament).
- NBA, [NBA adopts Play-In Tournament on full-time basis](https://www.nba.com/news/nba-adopts-play-in-tournament-on-full-time-basis).
- NBA, [2026 NBA Playoffs schedule](https://www.nba.com/news/2026-nba-playoffs-schedule).
- NBA Communications, [NBA Finals format change](https://pr.nba.com/nba-finals-format-change-2014/).
- NBA, [Emirates NBA Cup: Everything you need to know](https://www.nba.com/news/nba-cup-101).

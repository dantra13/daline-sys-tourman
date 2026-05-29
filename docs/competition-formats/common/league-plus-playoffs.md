# Formato generico: liga + playoffs

Formato en el que una fase de liga produce una tabla de posiciones y un subconjunto de participantes clasifica a una fase
eliminatoria posterior. Es comun en futbol y otros deportes cuando la temporada regular define siembra, ventajas y cortes
de acceso a la fase final.

## Complejidad administrativa

Nivel: 4/5, media-alta.

Combina administracion de liga con administracion de bracket. La dificultad esta en calendarizar la fase regular,
calcular standings, aplicar desempates, definir el corte de clasificacion, traducir posiciones a semillas y administrar
ventajas en playoffs. Es mas simple que grupos + eliminatoria cuando solo hay una tabla, pero puede volverse igual de
complejo si hay play-in, reclasificacion, ventajas deportivas o resembrado.

## Parametros configurables

- Numero de participantes en la liga.
- Tipo de calendario: todos contra todos, doble vuelta, vueltas multiples o calendario parcial.
- Sistema de puntos o porcentaje de victorias.
- Numero de clasificados directos a playoffs.
- Existencia de play-in, repechage o reclasificacion para ultimas plazas.
- Regla de siembra del bracket: posicion de liga, sorteo, ranking externo o combinacion.
- Ventajas por posicion: localia, bye, cierre de serie en casa, ventaja deportiva o rival preferente.
- Metodo de eliminatoria: partido unico, ida/vuelta, best-of-N, marcador global u otro.
- Reglas de desempate en liga y en playoffs.

## Estructura

La competicion se divide en 2 fases principales:

1. Liga: todos los participantes compiten por una tabla comun.
2. Playoffs: los clasificados de la liga entran a una fase eliminatoria.

La fase de liga puede terminar con:

- campeon de fase regular;
- clasificados directos;
- participantes eliminados;
- participantes enviados a play-in o repechage;
- siembras para el bracket;
- ventajas deportivas para los mejores ubicados.

## Progresion a playoffs

La progresion se define por posicion final en la tabla. El formato debe especificar:

- cuantos participantes avanzan;
- si todos entran en la misma ronda;
- si los mejores reciben bye;
- si existen plazas condicionadas por play-in;
- si hay restricciones para cruzar rivales;
- si el bracket se fija al iniciar playoffs o se reordena por siembra despues de cada ronda.

Un ejemplo generico de progresion es `top N clasifica a playoffs`, pero el valor de `N` no pertenece al formato: es una
configuracion de la competicion.

## Desempates

Hay desempates de liga y desempates de eliminatoria.

En liga, los desempates pueden considerar:

1. puntos o porcentaje de victorias;
2. resultado directo;
3. diferencia de goles/puntos/sets;
4. goles/puntos/sets a favor;
5. record como local/visitante;
6. fair play o disciplina;
7. ranking previo o sorteo.

En playoffs, el empate se resuelve segun el tipo de eliminatoria: tiempo extra, penales, overtime, partido adicional,
marcador global, serie al mejor de N o criterio tecnico de la disciplina.

## Variantes

- Liga completa + liguilla.
- Liga con doble vuelta + playoffs.
- Liga con tabla unica + play-in + playoffs.
- Liga por conferencia con playoffs internos.
- Liga regular + bracket con byes para mejores posiciones.
- Liga regular + final directa entre primeros lugares.
- Liga regular + eliminatoria con ventaja deportiva para el mejor sembrado.

## Consideraciones para modelado

- Separar `LeaguePhase` y `PlayoffPhase`; comparten competicion, pero tienen reglas distintas.
- Guardar `regularSeasonRank` o `leagueRank` aunque despues se convierta en `playoffSeed`.
- No asumir que posicion de liga y semilla de playoff son siempre iguales.
- Modelar el corte de clasificacion como regla configurable, no como numero fijo.
- Permitir ventajas por posicion sin acoplarlas al bracket: localia, bye, rival, serie o desempate.
- Si hay resembrado, el bracket no queda totalmente determinado desde el inicio de playoffs.
- Si hay play-in, tratarlo como fase puente entre liga y playoffs.

## Fuentes y ejemplos

- Liga MX, tabla de fase regular seguida de fase final/reclasificacion segun reglamento de competencia: [Liga MX](https://ligamx.net/).
- NBA, temporada regular por conferencia seguida de Play-In y Playoffs: [NBA Play-In Tournament](https://www.nba.com/news/nba-play-in-tournament).
- MLB, temporada regular seguida de postseason con siembra y series: [MLB postseason format](https://www.mlb.com/news/mlb-playoff-format-faq).

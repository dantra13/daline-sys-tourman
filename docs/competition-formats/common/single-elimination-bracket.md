# Formato generico: eliminatoria simple

Formato en el que un participante queda eliminado al perder una serie, partido, match o tie. Los ganadores avanzan por un
camino de bracket hasta una final o hasta una ronda objetivo.

## Complejidad administrativa

Nivel: 1/5, baja.

Es el formato mas simple de administrar: se arma un cuadro, cada enfrentamiento produce un ganador y el camino al titulo
queda definido desde el inicio. La complejidad aumenta si se agregan byes, siembra, restricciones de sorteo, partido por
tercer lugar, localias variables o enfrentamientos que no sean a partido unico.

## Parametros configurables

- Numero de participantes.
- Tamano del bracket.
- Existencia de byes.
- Metodo de asignacion: sorteo, ranking, siembra, clasificacion previa o bracket fijo.
- Reglas de localia o sede.
- Tipo de enfrentamiento: partido unico, serie best-of-N, ida/vuelta, aggregate score o match compuesto.
- Rondas incluidas: preliminares, octavos, cuartos, semifinales, final, tercer lugar o partidos de clasificacion.
- Reglas de desempate dentro del enfrentamiento.

## Estructura

Cada nodo del bracket representa un enfrentamiento. El ganador avanza a un nodo posterior y el perdedor queda eliminado,
salvo que exista un partido adicional de clasificacion o una capa externa de repechage.

El bracket puede iniciar completo o parcialmente vacio. En formatos con clasificacion previa, algunas posiciones se
llenan con placeholders como `ganador del grupo`, `semilla N`, `wild card` o `ganador de play-in`.

## Siembra y byes

La siembra busca ordenar participantes para distribuir fortalezas o premiar rendimiento previo. Puede usarse para:

- asignar rivales;
- definir localia;
- otorgar descanso o bye;
- evitar cruces tempranos entre participantes del mismo grupo, pais, conferencia o ranking.

El formato tambien puede operar sin semillas, con sorteo puro.

## Resolucion del enfrentamiento

La eliminatoria simple no define por si sola como se decide cada enfrentamiento. La competicion debe especificar una de
estas reglas:

- ganador de partido unico;
- marcador global de ida/vuelta;
- serie al mejor de N;
- criterio deportivo propio de la disciplina;
- prorroga, overtime, shoot-out, penales, juez, desempate tecnico o sorteo.

## Consideraciones para modelado

- Separar el `bracket path` del metodo de resolucion de cada enfrentamiento.
- Permitir placeholders para participantes aun no definidos.
- Guardar fuente de siembra y reglas de localia.
- No asumir que todas las rondas tienen el mismo metodo de resolucion.
- Permitir partidos de colocacion sin mezclarlos con el camino principal al titulo.

## Fuentes y ejemplos

- FIFA, fase eliminatoria posterior a
  grupos: [FIFA World Cup 26 schedule](https://www.fifa.com/en/tournaments/mens/worldcup/canadamexicousa2026/articles/updated-fifa-world-cup-2026-match-schedule-now-available?searchOverlay=1).
- UEFA, sistema de eliminatorias en competiciones europeas: [UEFA competition regulations](https://documents.uefa.com/).
- NBA, bracket de playoffs por conferencia con
  series: [NBA Playoffs schedule](https://www.nba.com/news/2026-nba-playoffs-schedule).

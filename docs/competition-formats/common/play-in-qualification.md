# Formato generico: play-in de clasificacion

Formato corto que asigna plazas finales a una fase posterior. Suele involucrar participantes ubicados alrededor de un
corte de clasificacion: algunos tienen doble oportunidad y otros entran en eliminacion directa.

## Complejidad administrativa

Nivel: 2/5, baja-media.

Tiene pocos partidos y una finalidad acotada, por eso es relativamente sencillo. La complejidad esta en modelar bien las
ventajas de entrada, la doble oportunidad para ciertas semillas y las plazas exactas que entrega a la fase destino. Sube
si hay mas de una region/conferencia o si los ganadores reciben semillas distintas en otro bracket.

## Parametros configurables

- Numero de participantes en play-in.
- Numero de plazas disponibles.
- Semillas o posiciones de entrada.
- Ventajas por siembra: localia, doble oportunidad, bye o rival preferente.
- Metodo de resolucion: partido unico, serie, ida/vuelta o match.
- Fase destino: playoffs, grupos, bracket principal, ascenso, permanencia o medallas.

## Estructura

El play-in no es una fase final completa. Es una fase puente que convierte un conjunto de candidatos en plazas concretas
para otra fase.

Un patron comun es:

- participantes mejor posicionados juegan por una plaza directa;
- participantes peor posicionados juegan eliminacion directa;
- el perdedor del primer cruce conserva una segunda oportunidad contra el ganador del cruce de eliminacion.

El formato puede generalizarse a mas participantes, mas plazas o distintos niveles de ventaja.

## Salida del formato

La salida debe ser una lista de clasificados con semillas destino. Por ejemplo:

- clasificado a semilla alta;
- clasificado a semilla baja;
- eliminado;
- transferido a otra competicion;
- pendiente por resultado administrativo.

## Consideraciones para modelado

- No mezclar la semilla original del play-in con la semilla final en la fase destino.
- Guardar ventajas de entrada como reglas, no como excepciones manuales.
- Permitir que un participante pierda una vez sin quedar eliminado si la estructura lo define.
- La fase destino debe consumir `qualifiedSlot`, no solo `winner`.
- El play-in puede existir por conferencia, grupo, division o ranking global.

## Fuentes y ejemplos

- NBA, Play-In Tournament para semillas 7 y 8 por
  conferencia: [NBA Play-In Tournament](https://www.nba.com/news/nba-play-in-tournament).
- NBA, adopcion permanente del
  Play-In: [NBA adopts Play-In Tournament](https://www.nba.com/news/nba-adopts-play-in-tournament-on-full-time-basis).
- UEFA, play-offs para completar una fase
  eliminatoria: [UEFA Champions League regulations](https://documents.uefa.com/).

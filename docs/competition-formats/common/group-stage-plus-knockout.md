# Formato generico: fase de grupos + eliminatoria

Formato en el que una primera fase divide participantes en grupos o pools y una segunda fase elimina participantes por
cruces directos hasta obtener campeon, medallistas o plazas de clasificacion.

## Complejidad administrativa

Nivel: 4/5, media-alta.

Combina dos modelos distintos: tablas de grupo y bracket. La carga administrativa esta en sortear o sembrar grupos,
calcular standings, resolver empates, definir clasificados y traducir posiciones de grupo a cruces. La complejidad aumenta
con wild cards, mejores terceros, ranking combinado entre grupos, restricciones de cruce y rondas eliminatorias con reglas
distintas.

## Parametros configurables

- Numero de grupos.
- Numero de participantes por grupo.
- Metodo de asignacion a grupos: sorteo, ranking, siembra, region, confederacion o restricciones operativas.
- Calendario de grupo: round-robin completo, doble vuelta, calendario parcial o sede neutral.
- Numero de clasificados por grupo.
- Plazas wild card o mejores terceros/mejores no clasificados.
- Tamano y forma del cuadro eliminatorio.
- Criterios para sembrar los cruces.
- Metodo de resolucion de eliminatorias: partido unico, ida/vuelta, best-of-N, global, overtime, penales u otro.

## Estructura

La competicion se divide en al menos 2 fases:

1. Fase de grupos: cada grupo produce una tabla propia.
2. Fase eliminatoria: los clasificados se insertan en un bracket o en cruces predefinidos.

El formato no fija cuantos grupos existen ni cuantos avanzan. La progresion puede ser simetrica, por ejemplo los mejores
de cada grupo, o mixta, por ejemplo ganadores de grupo mas wild cards.

## Progresion desde grupos

La progresion puede usar:

- posicion en grupo;
- puntos o porcentaje de victorias;
- criterios de desempate del grupo;
- ranking combinado entre grupos;
- restricciones de cruce, como evitar rivales del mismo grupo en la primera eliminatoria;
- siembra por rendimiento.

Cuando hay wild cards, el sistema debe crear una tabla transversal entre grupos. Esa tabla puede requerir normalizar
partidos jugados si los grupos no tienen el mismo tamano.

## Fase eliminatoria

La fase eliminatoria puede ser:

- bracket fijo desde el inicio;
- bracket resembrado despues de cada ronda;
- bracket con posiciones heredadas;
- eliminatoria simple;
- series al mejor de N;
- eliminatoria a doble partido;
- bracket con repechage o partidos de clasificacion.

## Desempates

Hay 2 niveles de desempate:

- desempates dentro de grupo;
- desempates entre participantes de distintos grupos para wild cards, siembra o ranking combinado.

No deben mezclarse automaticamente. Un desempate head-to-head dentro de un grupo puede no ser valido para comparar
equipos
de grupos distintos.

## Consideraciones para modelado

- Modelar grupo, tabla de grupo, ranking combinado y bracket como entidades separadas.
- Guardar el origen de cada clasificado: posicion de grupo, wild card, ranking combinado o invitacion.
- Hacer explicitas las reglas de cruce: `A1 vs B2`, `mejor primero vs peor clasificado`, sorteo, etc.
- Permitir que el bracket mantenga posiciones vacantes hasta que termine la fase de grupos.
- Distinguir fase de grupos de fase final aunque ambas usen partidos del mismo deporte.

## Fuentes y ejemplos

- FIFA, formato con grupos y fase
  eliminatoria: [FIFA World Cup 26 match schedule and format](https://www.fifa.com/en/tournaments/mens/worldcup/canadamexicousa2026/articles/updated-fifa-world-cup-2026-match-schedule-now-available?searchOverlay=1).
- FIBA, uso de group phase y final phase en baloncesto internacional: [FIBA competitions](https://www.fiba.basketball/).
- UEFA, ejemplos de fase de liga/grupos seguida de
  eliminatoria: [UEFA competition regulations](https://documents.uefa.com/).

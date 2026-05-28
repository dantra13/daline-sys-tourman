# Formato generico: eliminatoria doble

Formato en el que un participante queda eliminado despues de perder dos veces. Normalmente se organiza con un cuadro de
ganadores y un cuadro de perdedores.

## Complejidad administrativa

Nivel: 4/5, alta.

Es mas complejo que la eliminatoria simple porque cada derrota puede mover al participante a otra ruta en vez de
eliminarlo. Hay que mantener cuadro de ganadores, cuadro de perdedores, conteo de derrotas, entradas condicionales y
posible reset de final. La complejidad sube si hay byes, siembra fuerte, multiples rondas de perdedores o final con reglas
especiales.

## Parametros configurables

- Numero de participantes.
- Metodo de siembra.
- Existencia de byes.
- Reglas para mover perdedores desde el cuadro de ganadores al cuadro de perdedores.
- Metodo de final: final unica, final con reset de bracket o final con ventaja para quien llega invicto.
- Tipo de enfrentamiento: partido unico, match, serie o aggregate.
- Partidos de colocacion.

## Estructura

El cuadro de ganadores funciona como una eliminatoria simple inicial. Cada perdedor baja al cuadro de perdedores. En el
cuadro de perdedores, una nueva derrota elimina al participante.

La final enfrenta al ganador del cuadro de ganadores contra el ganador del cuadro de perdedores. Si se usa reset de
bracket, el participante que viene del cuadro de perdedores debe ganar dos enfrentamientos finales para entregar la
segunda derrota al participante invicto.

## Variantes de final

- Final con reset: el invicto conserva su margen de una derrota.
- Final sin reset: la final decide todo aunque uno llegue invicto.
- Final con ventaja: el invicto inicia con ventaja deportiva, de localia o de puntos.
- Gran final al mejor de N con condiciones especiales.

## Consideraciones para modelado

- Modelar `winnerBracket` y `loserBracket` como rutas relacionadas, no como brackets independientes sin conexion.
- Cada participante necesita conteo de derrotas dentro del torneo.
- La posicion de entrada al cuadro de perdedores depende de la ronda en la que se pierde.
- El reset de bracket debe ser un nodo condicional, no un partido fijo obligatorio.
- La nomenclatura de rondas puede ser compleja; usar identificadores estructurales ademas de nombres visibles.

## Fuentes y ejemplos

- USA Ultimate, documentos de formatos de torneo con brackets y opciones de
  competicion: [USA Ultimate formats](https://usaultimate.org/).
- World Cube Association, formatos de competicion y
  progression/ranking: [WCA Regulations](https://www.worldcubeassociation.org/regulations/).
- Challonge, referencia operacional de double elimination usada en gestion de
  torneos: [Double elimination brackets](https://kb.challonge.com/en/article/tournament-formats-1h7t6r4/).

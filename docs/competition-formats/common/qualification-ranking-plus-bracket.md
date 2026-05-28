# Formato generico: clasificacion/ranking + bracket

Formato en el que una fase inicial produce marcas, tiempos, puntajes o ranking, y ese orden se usa para sembrar una
fase eliminatoria posterior.

## Complejidad administrativa

Nivel: 4/5, media-alta.

Requiere administrar dos naturalezas de resultado: ranking de clasificacion y enfrentamientos eliminatorios. La dificultad
esta en aplicar cortes, desempates de ranking, reemplazos, asignacion de semillas y traduccion al bracket. Sube cuando la
fase de clasificacion tiene heats/grupos separados o cuando el bracket aplica restricciones adicionales.

## Parametros configurables

- Metodo de ranking: tiempo, marca, puntaje, vueltas, sets, precision, jueces o tabla.
- Numero de participantes que avanzan.
- Regla de corte: mejores N, minima marca, cupos por grupo/heat o wild cards.
- Regla de siembra del bracket.
- Existencia de byes.
- Restricciones de cruce.
- Metodo de resolucion del bracket.

## Estructura

La primera fase no elimina necesariamente por enfrentamiento directo. Produce un orden competitivo. Ese orden alimenta
un
bracket, usualmente para premiar a mejores clasificados con rivales teoricamente mas bajos o con byes.

Flujo generico:

1. Fase de ranking o clasificacion.
2. Corte de clasificados.
3. Asignacion de semillas.
4. Bracket eliminatorio.
5. Final, medallas o posiciones.

## Desempates

La fase de ranking debe definir desempates propios:

- mejor segundo intento/marca secundaria;
- menor tiempo;
- mayor puntaje tecnico;
- resultado directo;
- ranking previo;
- desempate adicional;
- sorteo.

El bracket puede tener desempates distintos a los de la fase de ranking.

## Consideraciones para modelado

- Separar `qualificationRank` de `bracketSeed`.
- Guardar participantes no clasificados con su resultado de ranking.
- Permitir que un participante clasificado no tome su plaza y se active reemplazo.
- No asumir que seed 1 siempre ocupa la misma posicion de bracket; depende de la plantilla.
- Permitir que la fase de ranking sea individual aunque el bracket sea por equipos, o viceversa, si la disciplina lo
  requiere.

## Fuentes y ejemplos

- World Archery, ranking round seguido de
  eliminatorias: [World Archery rulebook](https://www.worldarchery.sport/rulebook).
- World Athletics, fases de clasificacion y finales por
  marcas/tiempos: [World Athletics rules](https://worldathletics.org/about-iaaf/documents/book-of-rules).
- Olympic Games, multiples deportes con clasificacion previa y bracket final: [Olympics.com](https://olympics.com/).

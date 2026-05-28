# Formato generico: eliminatoria por series al mejor de N

Variante de eliminatoria en la que cada cruce se decide por una serie de partidos. Avanza el participante que alcanza
primero el numero requerido de victorias.

## Complejidad administrativa

Nivel: 3/5, media.

El camino de bracket puede ser simple, pero cada cruce abre partidos condicionales: algunos se juegan solo si la serie no
esta decidida. Tambien hay que administrar ventaja de campo, patron de localias, estado de la serie y cancelacion o
confirmacion de partidos potenciales. La complejidad sube si distintas rondas usan longitudes de serie diferentes.

## Parametros configurables

- Numero maximo de partidos de la serie.
- Victorias necesarias para ganar la serie.
- Patron de localia: neutral, alternada, 2-2-1-1-1, 2-3-2 u otro.
- Criterio de ventaja de campo.
- Reglas para partidos no necesarios.
- Siembra o resembrado entre rondas.
- Rondas con distinto largo de serie.

## Estructura

Una serie tiene un conjunto ordenado de partidos potenciales. No todos se juegan: la serie termina cuando un
participante
alcanza las victorias necesarias.

Ejemplos configurables:

| Serie      | Victorias requeridas | Maximo de partidos |
|------------|----------------------|--------------------|
| Mejor de 3 | 2                    | 3                  |
| Mejor de 5 | 3                    | 5                  |
| Mejor de 7 | 4                    | 7                  |

El formato no exige esos valores; son parametros.

## Localia

La localia debe modelarse por posicion de partido dentro de la serie, no solo por localia global. El participante con
ventaja puede recibir mas partidos potenciales que el rival, pero solo se juegan los necesarios.

## Desempates

Normalmente cada partido individual no puede terminar empatado. Si la disciplina permite empate, la competicion debe
definir si el empate:

- cuenta como medio resultado;
- fuerza overtime o desempate;
- obliga a repetir partido;
- usa puntos agregados;
- usa otro criterio de serie.

## Consideraciones para modelado

- `Series` y `Game` deben ser entidades separadas.
- Guardar `requiredWins`, `maxGames`, `currentWins`, `isClinched` y `winner`.
- Permitir partidos programados condicionales.
- No eliminar del calendario los partidos no necesarios sin guardar su estado cancelado/no requerido.
- La serie puede ser una ronda dentro de un bracket, pero tambien puede existir como formato independiente.

## Fuentes y ejemplos

- NBA, playoffs y Finals al mejor de 7: [NBA Playoffs schedule](https://www.nba.com/news/2026-nba-playoffs-schedule).
- NBA Communications, localia 2-2-1-1-1 en
  Finals: [NBA Finals format change](https://pr.nba.com/nba-finals-format-change-2014/).
- MLB, postseason con series de distintas
  longitudes: [MLB postseason format](https://www.mlb.com/news/mlb-playoff-format-faq).

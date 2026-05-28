# Formato generico: repechage

Formato de segunda oportunidad en el que participantes que perdieron en una ruta principal pueden acceder a medallas,
clasificacion o posiciones finales por una ruta secundaria.

## Complejidad administrativa

Nivel: 4/5, alta.

La complejidad viene de su dependencia con otra ruta competitiva. No basta con saber quien perdio: hay que saber en que
ronda perdio, contra quien, si ese rival avanzo, cuantas plazas hay y hacia donde progresa el ganador del repechage. Puede
ser mas dificil de explicar al usuario final que un bracket normal.

## Parametros configurables

- Criterio para entrar al repechage.
- Numero de plazas disponibles desde repechage.
- Punto de entrada segun ronda perdida.
- Si los participantes vuelven al cuadro principal o compiten solo por posicion/medalla.
- Metodo de resolucion: partido unico, serie, combate, carrera, tiempo o ranking.
- Relacion con terceros lugares, bronces, clasificacion olimpica o permanencia.

## Estructura

El repechage se conecta a una fase principal. No todos los perdedores necesariamente entran. La regla puede depender de:

- haber perdido contra finalistas;
- haber llegado a cierta ronda;
- ranking previo;
- tiempo o marca;
- cupos disponibles;
- decision de organizacion.

El formato evita que una unica derrota temprana ante un rival muy fuerte deje sin opciones a un participante
competitivo.

## Variantes

- Repechage para medalla de bronce.
- Repechage para clasificar a semifinal/final.
- Repechage por tiempos o marcas.
- Repechage como cuadro de perdedores reducido.
- Repechage con multiples rutas de entrada.

## Consideraciones para modelado

- Guardar la causa de entrada al repechage.
- Modelar la ruta principal y la ruta de repechage como fases relacionadas.
- No asumir que el ganador del repechage vuelve al camino por el titulo; muchas veces solo disputa medalla o plaza.
- Permitir multiples terceros lugares o multiples plazas.
- La progresion puede depender de resultados futuros de la ruta principal.

## Fuentes y ejemplos

- World Athletics, uso de repechage rounds en competiciones de
  pista: [World Athletics repechage round announcement](https://worldathletics.org/news/press-releases/repechage-round-paris-2024-olympics-budapest-entry-standards).
- United World Wrestling, repechage conectado a finalistas y medallas: [United World Wrestling](https://uww.org/).
- Olympic Games, explicaciones de repechage por deporte: [Olympics.com](https://olympics.com/).

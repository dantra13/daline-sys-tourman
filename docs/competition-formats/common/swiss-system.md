# Formato generico: sistema suizo

Formato de rondas sucesivas en el que los participantes no enfrentan a todos los demas. En cada ronda se emparejan
participantes con rendimiento similar, y una tabla acumulada define clasificacion, campeon o acceso a otra fase.

## Complejidad administrativa

Nivel: 5/5, muy alta.

La dificultad principal es que el calendario se genera dinamicamente despues de cada ronda. Se necesita tabla acumulada,
historial de rivales, reglas para evitar repeticiones, byes, restricciones de localia/color/lado y desempates por fuerza
de rivales. Es administrativamente pesado aunque no tenga una fase eliminatoria.

## Parametros configurables

- Numero de participantes.
- Numero de rondas.
- Sistema de puntos.
- Metodo de emparejamiento inicial.
- Reglas para evitar repetir rivales.
- Reglas de colores/localia/lado si aplica.
- Criterios de desempate.
- Corte final: campeon por tabla, clasificados a bracket, eliminacion por umbral o premios.

## Estructura

Cada ronda se genera con base en el estado acumulado despues de la ronda anterior. Los participantes con puntuaciones
similares se emparejan entre si, evitando repetir rivales cuando la regla lo exige.

El sistema suizo es util cuando hay demasiados participantes para un round-robin completo, pero se quiere más
informacion competitiva que una eliminatoria directa.

## Emparejamientos

Los emparejamientos pueden considerar:

- puntuación acumulada;
- ranking o siembra inicial;
- restricciones de repetición;
- localía o colores;
- restricciones por país, club, grupo o region;
- byes cuando el número de participantes es impar.

## Desempates

Los desempates suelen depender de fuerza de rivales o rendimiento acumulado. Ejemplos genericos:

- Buchholz o suma de puntuaciones de rivales;
- Sonneborn-Berger u otra variante ponderada;
- diferencia de puntos/goles/sets;
- resultado directo;
- ranking previo;
- sorteo.

La lista exacta depende de la disciplina y debe configurarse por competicion.

## Consideraciones para modelado

- El calendario no puede generarse completamente antes de conocer resultados, salvo como placeholders.
- Guardar historial de rivales para validar no repetición.
- Guardar byes como unidades competitivas especiales o resultados administrativos.
- Separar emparejamiento de tabla: la tabla calcula estado, el emparejador produce la siguiente ronda.
- Permitir que el sistema suizo alimente una fase eliminatoria posterior.

## Fuentes y ejemplos

- FIDE, reglas de emparejamiento para torneos suizos: [FIDE Swiss Rules](https://handbook.fide.com/).
- UEFA, fase de liga con calendario parcial y rivales por bombos como variante cercana de liga no
  todos-contra-todos: [UEFA Champions League regulations](https://documents.uefa.com/).
- US Chess, guias de torneo y sistemas de emparejamiento: [US Chess Federation](https://new.uschess.org/).

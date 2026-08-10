# Análisis del Proyecto

**Proyecto:** Feria de Ciencias - Programación  
**Nombre:** Thiago Almada  
**Nombre del proyecto:** LowTrace

## Definición del juego

Juego de carreras en 3D con estética low-poly. El jugador debe conducir un vehículo a través de circuitos en el menor tiempo posible.

## Boceto del juego

- **Cámara:** tercera persona detrás del auto.
- **Pista:** tiles low-poly (recta y curva), pórtico de salida/meta, rampas para saltos y obstáculos para esquivar.
- **Límite:** borde de la pista; caerse reinicia la vuelta (castigo).
- **Auto:** modelo low-poly simple (chasis + 4 ruedas), colores desbloqueables por récord.
- **Menú:** título, botones Jugar / Mapa / Créditos, animación en el botón Jugar.
- **HUD:** cronómetro y vuelta actual.
- **Fin:** tiempo final, récord, botones Reintentar / Volver al menú.

## Condiciones de victoria / derrota

- **Victoria:** Cruzar la línea de meta y registrar un tiempo válido.
- **Derrota:** Nunca llegar a la meta.

## Loop general

1. **Inicio:** Pantalla de menú inicial con animación del mapa de fondo y botones para jugar o seleccionar circuito.
2. **Juego:** Conducir, tomar curvas, esquivar obstáculos, usar rampas.
3. **Fin:** Cruzar la meta, mostrar el tiempo final, evaluar si superó el récord local, dar la opción de reintentar al instante o volver al menú.

## Upgrade / Castigo

- **Upgrade:** Si se gana con cierto tiempo se desbloquean nuevos mapas y nuevos estilos de vehículo si se logra un tiempo determinado.
- **Castigo:** Si caes de la pista, pierdes la vuelta actual y debes reiniciar la carrera desde cero.

## Mecánica principal

Conducción de un auto (aceleración, frenado, giro y físicas de salto/aterrizaje).

## Recursos físicos necesarios

- Espacio suficiente para la mesa del stand.
- 1 Computadora del laboratorio, teclado y mouse.
- 1 Monitor y un proyector.
- 1 Zapatilla eléctrica.

## Multiplayer

No.

## Competitivo

Sí, competitivo de forma asincrónica (competencia por el menor tiempo del día grabado en la tabla de récords del stand).

## ¿Qué lo hará divertido?

La intencionalidad instantánea, el desafío de superación personal al intentar bajar milisegundos en cada intento y la variedad de circuitos.

## Vinculación con electrónica

Sí, se realizará un volante para conducir el auto.

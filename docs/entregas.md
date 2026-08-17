# Registro de Entregas

Bitácora de avance del proyecto **LowTrace** — Feria de Ciencias (ETec 2026).

---

## Entrega 1 — 13 de agosto: Documento de IA + UI funcional (1 punto)

**Estado:** COMPLETADA (terminada el 10 de agosto de 2026)

### Lo que se hizo

**Documentación:**
- `docs/documento.md`: plan de trabajo con IA (requisito del profe).
- `docs/analisis.md`: idea, boceto, condiciones de victoria/derrota, loop general,
  upgrade/castigo, mecánica principal, recursos físicos, multiplayer, competitivo
  y vinculación con electrónica.
- `docs/plan-desarrollo.md`: plan técnico con tareas por entrega.
- Carpeta `docs/` creada en la raíz del repo para toda la documentación.

**Proyecto Unity (`LowTrace/`):**
- Estructura de carpetas: `Scenes/`, `Scripts/` (01_UI a 07_Data), `Prefabs/`,
  `Art/`, `Audio/`, `Data/`.
- Escenas: `Menu.unity`, `Mapa.unity`, `Game.unity` registradas en Build Profiles.
- Menú principal con título "LOWTRACE", botones Jugar / Mapa / Créditos / Salir
  y fondo generado con IA.
- Hover en botones: cambian de color y aumentan de tamaño (ButtonHoverScale).
- Panel de créditos funcional (abre/cierra, oculta el menú detrás).
- Navegación: Jugar → Game, Mapa → Mapa, Salir → cierra el juego.
- Fuentes de Google Fonts importadas y convertidas a TMP SDF
  (Bungee, Archivo Black, Rubik, etc.).
- `.gitignore` del proyecto Unity para no subir `Library/`, `Temp/`, etc.

### Criterio de aceptación

- [x] Menú → Jugar lleva a Game.
- [x] Botones reaccionan al pasar el mouse (color/tamaño).
- [x] Pantalla de créditos abre y cierra.
- [x] Se puede volver al menú.

---

## Entrega 2 — 20 de agosto: Loop del juego + victoria/derrota (1 punto)

**Estado:** EN PROGRESO (código y escena generados el 14 de agosto; falta probar en el editor)

### Lo que se hizo

**Scripts (`Assets/Scripts/`):**
- `02_Game/GameManager.cs`: singleton con estados `Espera → Carrera → Terminado`,
  cronómetro, récord de sesión, vueltas, checkpoints en orden y castigo por caída
  (recarga la escena). Vueltas totales editable desde el inspector.
- `02_Game/Checkpoint.cs` (nuevo): trigger con tag `Player` → registra el
  checkpoint en el orden correcto (3 checkpoints habilitan la meta).
- `02_Game/FinishLine.cs`: trigger en la meta → cruza la vuelta / termina la carrera.
- `03_Vehicle/CarController.cs`: conducción con `Keyboard.current` (Input System
  nuevo), WASD; al primer movimiento arranca la carrera (`IniciarCarrera()`).
- `04_Camera/CameraFollow.cs`: cámara que sigue al auto (provisoria; en entrega 6
  se reemplaza por Cinemachine).
- `01_UI/UIManager.cs` (nuevo, reemplaza a Stopwatch/HUD/ResultPanel eliminados):
  cronómetro, diferencia con el récord, contador de checkpoints (`0/3`), contador
  de vueltas (`0/1`), estado, pantalla de victoria y teclas Reintentar (`R`/Espacio)
  y Volver al menú (`Esc`).

**Escena `Game.unity` reescrita por completo:**
- `GameController` (GameManager con auto y 3 checkpoints referenciados),
  `Auto` (tag `Player`, Rigidbody, CarController), `Victoria` (meta trigger),
  3 cubos-checkpoint (Rigidbody IsKinematic + IsTrigger + Checkpoint.cs),
  `UIController` (UIManager), cámara con CameraFollow, luz direccional,
  Canvas HUD con contadores y panel de victoria.

### Criterio de aceptación

- [ ] La carrera inicia al primer movimiento (WASD) y cronometra.
- [ ] Tocar los 3 checkpoints muestra `3/3` y habilita la meta.
- [ ] Cruzar la meta muestra la pantalla de fin con tiempo y récord.
- [ ] Reintentar recarga la escena y Menú vuelve al menú.
- [ ] Caerse de la pista (y < umbral) recarga la escena.

---

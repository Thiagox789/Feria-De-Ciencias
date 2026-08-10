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

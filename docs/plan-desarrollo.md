# Plan de Desarrollo — LowTrace

**Proyecto:** LowTrace — Juego de carreras 3D low-poly (Feria de Ciencias)
**Autor:** Thiago Almada
**Motor:** Unity (URP) + C# + opencode
**Fecha:** 10 de agosto de 2026

Este plan convierte el cronograma del profe (ver `documento.md`) en tareas
concretas de desarrollo. Cada entrega lista qué escenas, scripts y prefabs crear,
y cuál es el criterio para darla por terminada.

> Regla de oro: cada entrega se verifica en el editor de Unity ANTES de
> commitear. Un commit por entrega.

---

## Cómo se lee este plan

Cada entrega tiene:

- **Objetivo**: qué se quiere lograr.
- **Escenas / Archivos**: dónde se trabaja.
- **Tareas**: pasos concretos.
- **Criterio de terminado**: cómo se comprueba que está lista (se escribe en el checklist de `documento.md`).

### Nota sobre la UI

La UI **no se termina en la Entrega 1**: se construye por capas según lo que va
haciendo falta en cada etapa, y se vuelve a tocar en entregas posteriores:

- **Entrega 1 → menú principal**: título, botones, hover, créditos, navegación.
  Es lo único que se "cierra" en esa fecha.
- **Entrega 2 → HUD de carrera + ResultPanel**: cronómetro, contadores,
  pantalla de fin con Reintentar/Menú. Estos van dentro de `01_UI/` pero se
  crean acá porque la carrera recién existe en esta fecha.
- **Entrega 3 → refactor de controladores**: la UI pasa a leer el estado del
  `GameManager`/`SoundManager`/`DataManager` (sin referencias sueltas).
- **Entrega 7 → pulido final**: UI responsiva (Canvas Scaler + anchors) y
  revisión visual global.

**Regla:** tocar y ajustar la UI en cualquier entrega está permitido y es
esperado. Sólo hay un requisito de fondo: lo que se entregó antes debe seguir
funcionando (regresión cero en lo ya aprobado).

### Nota sobre fechas

Las fechas del cronograma son **límites**, no obligaciones de esperar:

- Adelantarse está permitido: si la Entrega 2 está lista y probada, se puede
  empezar la 3 el mismo día.
- **Mezclar entregas** también: por ejemplo, meter el `SoundManager` (Entrega 3)
  junto con el loop de carrera si necesitás sonido para probar la meta, o
  adelantar parte de `07_Data/` si un récord se necesita antes.
- Lo único que no se saltea: **el criterio de terminado de cada entrega** se debe
  poder cumplir y **verificar en el editor** antes de commitear.

---

## 0. Fundaciones (primeros días)

Antes de la primera entrega con código conviene tener la base:

- [ ] Crear proyecto Unity 6 (URP, entrada: Input System) en Unity Hub.
- [ ] Estructura de carpetas según `documento.md` sección 3:
  `Scenes/`, `Scripts/01_UI … 07_Data/`, `Prefabs/`, `Art/`, `Audio/`, `Data/`.
- [ ] Configurar `ProjectSettings` para WebGL (build final) y ajustar calidad.
- [ ] Vincular el repo: mover esta carpeta de docs adentro del proyecto o
      versionar ambos juntos (decisión del profe).

---

## 1. Entrega 13 de agosto — UI funcional (1 punto)

**Objetivo:** menú navegable con botones que se animan al pasar el mouse
(cambian de color y/o crecen) y pantalla de créditos.

**Escenas:** `Menu.unity`, `Game.unity` (en blanco, navegable).

**Tareas:**

- [ ] Crear `Menu.unity` con fondo (mapa low-poly simple o color + logo).
- [ ] Canvas de menú: título, botones **Jugar**, **Mapa** (select de circuito),
      **Créditos**, y botón **Salir**.
- [ ] Hover en todos los botones: al pasar el mouse cambian de color y
      aumentan un poco de tamaño. Implementar con:
      - **Unity UI nativo**: `ColorTint` de Button (color de hover) + 
        `Scripts/01_UI/ButtonHoverScale.cs` (corrutina que escala con
        `LeanTween`/`DOTween` o a mano con `Time.deltaTime`).
- [ ] Animación extra en el botón **Jugar**: pulso suave permanente
      (requisito mínimo del enunciado) además del hover.
- [ ] `Scripts/01_UI/MenuManager.cs`: carga de escenas por botón.
- [ ] `Scripts/01_UI/CreditsPanel.cs`: abre/cierra panel de créditos.
- [ ] `Game.unity` vacía con un objeto `MenuReturn` para volver al menú.
- [ ] Registrar ambas escenas en Build Settings.

**Criterio de terminado:** Menú → Jugar lleva a `Game`, todos los botones
reaccionan al pasar el mouse (color/tamaño), Jugar tiene animación propia,
Créditos abre panel, se puede volver al menú.

---

## 2. Entrega 20 de agosto — Loop del juego (1 punto)

**Objetivo:** iniciar carrera, cronometrar, cruzar meta, pantalla de fin,
castigo por caerse.

**Escenas:** `Game.unity`.

**Tareas:**

- [ ] `Scripts/02_Game/GameManager.cs`: máquina de estados
      (`Idle → Racing → Finished`).
- [ ] `Scripts/02_Game/CountdownTimer.cs`: cuenta regresiva de salida (3-2-1-¡Ya!).
- [ ] `Scripts/02_Game/Stopwatch.cs`: cronómetro (mm:ss.ms).
- [ ] `Scripts/02_Game/FinishLine.cs`: trigger en la meta → estado `Finished`.
- [ ] `Scripts/02_Game/FallDetector.cs`: si el auto pasa bajo un umbral de
      altura → reiniciar la vuelta (castigo).
- [ ] `Scripts/01_UI/HUD.cs`: cronómetro y estado en pantalla.
- [ ] `Scripts/01_UI/ResultPanel.cs`: tiempo final, récord, botones
      **Reintentar** / **Menú**.
- [ ] Prefab del circuito con pista (mesa) + meta + borde.

**Criterio de terminado:** la carrera inicia con countdown, cronometra, al
cruzar la meta sale la pantalla de fin con Reintentar y Menú; caerse reinicia.

---

## 3. Entrega 27 de agosto — Controladores (1 punto)

**Objetivo:** los controladores del juego consolidados y usados por los scripts.

**Tareas:**

- [ ] `Scripts/02_Game/GameManager.cs` refactorizado como singleton controlador
      de estados.
- [ ] `Scripts/05_Sound/SoundManager.cs`: singleton para música y SFX
      (lista de AudioSource + métodos `PlayMusic()`, `PlaySFX()`).
- [ ] `Scripts/07_Data/DataManager.cs` (o `SaveSystem`): persistencia en disco.
- [ ] Todos los scripts que necesiten estado/sonido/datos usan los controladores
      (sin referencias sueltas).

**Criterio de terminado:** los 3 controladores existen y se invocan desde los
scripts (p. ej. HUD lee el estado del GameManager).

---

## 4. Entrega 3 de septiembre — Mecánica principal (1 punto)

**Objetivo:** conducción estable: acelerar, frenar, girar, salto y aterrizaje.

**Tareas:**

- [ ] `Scripts/03_Vehicle/CarController.cs`: entrada (WASD / volante),
      aceleración, frenado, giro.
- [ ] `Scripts/03_Vehicle/Suspension.cs`: 4 raycast desde las ruedas al suelo
      (física tipo arcade low-poly).
- [ ] `Scripts/03_Vehicle/Jump.cs`: rampas → salto, giro en el aire,
      aterrizaje suave.
- [ ] `Scripts/03_Vehicle/Wheel.cs`: ruedas visuales que rotan con velocidad.
- [ ] Ajuste de valores (potencia, masa, grip) hasta que se sienta estable.
- [ ] Colisiones/detecciones: meta, borde (caída), obstáculos.

**Criterio de terminado:** el auto se conduce de forma estable por el circuito,
salta rampas y aterriza sin volcarse, y detecta meta/caída.

---

## 5. Entrega 10 de septiembre — Sonido + datos persistentes (1 punto)

**Objetivo:** sonidos funcionando y récords que sobrevivan al cierre.

**Tareas:**

- [ ] Audios importados: motor, derrape, salto, meta, UI, música de menú.
- [ ] `SoundManager` conectado: motor según RPM/aceleración, SFX en eventos
      (meta, salto, caída, botón).
- [ ] `ScriptableObjects` (`Scripts/07_Data/`):
      `SettingsData`, `RecordsData`, `UnlocksData`.
- [ ] `DataManager`: guardar/cargar los SO a disco (JSON en
      `Application.persistentDataPath`).
- [ ] Tabla de récords del día (ranking asincrónico del stand).

**Criterio de terminado:** hay sonido acorde a cada acción y los récords siguen
existiendo al cerrar y reabrir el juego.

---

## 6. Entrega 17 de septiembre — Efectos (1 punto)

**Objetivo:** partículas, cámara suave e iluminación ambientada.

**Tareas:**

- [ ] `Scripts/06_Effects/`:
      `SkidParticles.cs` (humo de derrape),
      `JumpParticles.cs` (salto/aterrizaje),
      `FinishParticles.cs` (meta).
- [ ] Cámara: Cinemachine (CinemachineVirtualCamera siguiendo al auto,
      Damping suave).
- [ ] Iluminación low-poly: dirección + ambiental + `Lighting Settings`
      (luz sol, skybox simple).
- [ ] Post-procesado URP (volumen: tono, contraste, viñeta) opcional.

**Criterio de terminado:** se ven partículas en las acciones, la cámara sigue al
auto sin sacudidas y la iluminación da estética low-poly coherente.

---

## 7. Entrega 28 de septiembre — Versión final (1 punto)

**Objetivo:** build sin errores y UI coherente.

**Tareas:**

- [ ] Pasada de arte: paleta de colores consistente, modelos low-poly pulidos.
- [ ] UI responsiva en todas las resoluciones (Canvas Scaler + anchors).
- [ ] Prueba completa del juego (menú → carrera → fin → reintentar).
- [ ] Build WebGL desde Unity, probar en navegador (sin errores en consola).
- [ ] Optimizar: texturas, Mesh, Draw Calls para WebGL.
- [ ] Crear carpeta en Drive, compartir, pegar el link en `README.md`.

**Criterio de terminado:** build WebGL juegable sin errores y link en el README.

---

## 8. Entrega 5 de noviembre — Difusión (2 puntos)

- [ ] Publicación en itch.io (0.5).
- [ ] Video promocional en YouTube (0.5).
- [ ] Decoración del stand: folletería + volante electrónico (1).

---

## Prioridad de trabajo

1. **Circuito + auto** (para poder probar cualquier cosa).
2. **Conducción** (mecánica principal).
3. **GameManager + meta + cronómetro** (cierra el loop).
4. **UI** (menú y pantalla de fin).
5. **Sonido y datos** (pulido).
6. **Efectos, cámara e iluminación** (estética).
7. **Build + publicación**.

> Nota: la entrega del 13/8 pide UI, pero conviene tener pista + auto antes
> para probar la navegación con algo visible.

---

## Riesgos (resumen)

- Entregas fuera de tiempo = **CERO** → commitear antes de cada fecha límite.
- `.meta` rotos si se crean scripts fuera de Unity → crear scripts desde el
  editor o regenerar abriendo Unity.
- WebGL pesado → optimizar arte desde el principio.
- `Library/` no se versiona (ya está en `.gitignore`).

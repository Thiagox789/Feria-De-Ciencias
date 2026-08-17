# Documento de Plan de Trabajo — LowTrace (programado con IA)

**Autor:** Thiago Almada
**Proyecto:** LowTrace — Juego de carreras 3D low-poly
**Motor:** Unity (URP) + C#
**Método de trabajo:** Programación asistida con agentes de IA (opencode)

---

## 1. Contexto y objetivo

LowTrace es un juego de carreras single-player (asincrónicamente competitivo por
récord) que se presentará en la Feria de Ciencias. El desarrollo se organiza por
**entregas con fecha límite** (ver la sección 4). Este documento define
el plan, la arquitectura y la forma de trabajo de aquí a septiembre.

### 1.1. Contenido mínimo exigido por el enunciado

El proyecto debe trascender la pantalla de la PC y cumplir con:

- Publicado en itch.io.
- Controlador de juego.
- Controlador de sonido.
- Efectos de partículas.
- Iluminación.
- Datos persistentes con `ScriptableObject`.
- Interfaz responsiva y estética excelente.
- Archivo ejecutable sin errores.
- Animación en el botón Jugar de la escena inicial.
- Pantalla de créditos.

---

## 2. Cómo trabajamos con IA (modelo de trabajo)

Cada sesión de trabajo sigue este ciclo:

1. **Definir la tarea**: escribir una instrucción clara que responda a QUÉ
   (funcionalidad) y ACEPTAR/CORREGIR (criterios de terminado).
2. **Revisar el estado**: el agente lee los documentos (`analisis.md`,
   `docs/*.md`, este archivo) antes de tocar código.
3. **Implementar en pasos pequeños**: una entrega a la vez. Nunca escribir
   "todo el juego" en un solo pedido.
4. **Verificar en Unity**: volver al editor, asignar componentes, reproducir y
   probar. La IA genera el código y las instrucciones de armado.
5. **Commitear por entrega**: un commit por hito, con mensaje descriptivo.

### Reglas del modelo

- La IA **nunca edita** archivos de la carpeta `Library/`, `Logs/`,
  `UserSettings/` ni `.meta` generados.
- Los `.meta` solo se regeneran desde Unity (no se editan a mano).
- Todo código del juego va dentro de `Assets/Scripts/` (numerado por módulo).
- Antes de cada entrega se actualiza el checklist en `plan-desarrollo.md`.
- Se prioriza primero la entrega vigente; lo "extra" va al final.

### Formato de prompt recomendado

```
Objetivo: <qué se quiere lograr>
Contexto: <qué archivos/escenas intervienen>
Condición de terminado: <cómo sé que está listo>
Restricción: <lo que no se debe tocar>
```

---

## 3. Arquitectura del proyecto Unity

```
LowTrace/Assets/
├── Scenes/
│   ├── Menu.unity          → menú principal (hecho: UI visual sin lógica)
│   └── Game.unity          → circuito de carrera (a crear)
├── Scripts/
│   ├── 01_UI/              → navegación del menú, HUD, pantalla de fin
│   ├── 02_Game/            → GameManager (estados), cronómetro, meta, victoria/fin
│   ├── 03_Vehicle/         → conducción (acelerar, frenar, girar, salto) por raycast
│   ├── 04_Camera/          → cámara que sigue al auto (Cinemachine)
│   ├── 05_Sound/           → SoundManager (música + efectos)
│   ├── 06_Effects/         → partículas (humo derrape, salto, meta)
│   └── 07_Data/            → ScriptableObjects (récords, ajustes, desbloqueos)
├── Prefabs/                → auto, tiles de pista, checkpoint, meta
├── Art/                    → modelos low-poly, texturas, fondo
├── Audio/                  → música y efectos de sonido
└── Data/                   → ScriptableObjects persistentes (guardado en disco)
```

---

## 4. Plazos y tareas por entrega

> **Regla oficial:** los trabajos entregados fuera de tiempo tienen puntaje CERO.

### 4.0. 6 de agosto — Idea, boceto y definiciones (1 punto)
- [x] Idea del juego, boceto y condiciones de victoria/derrota → `analisis.md`.
- [x] Loop general, upgrade/castigo, mecánica principal y recursos físicos → `analisis.md`.
- [x] Multiplayer, competitivo (ranking) y vinculación con electrónica → `analisis.md`.

### 4.1. 13 de agosto — Documento de IA + UI funcional (1 punto)
- [x] Subir este `documento.md` con el plan de trabajo con IA.
- [x] Conectar la UI del menú (botones Jugar / Mapa / Créditos).
- [x] Animación en el botón Jugar.
- [x] Pantalla de créditos.
- [x] Escena de juego creada y navegable (aunque sea el circuito en blanco).

### 4.2. 20 de agosto — Loop del juego + victoria / derrota / finalización (1 punto)
- [ ] GameManager con estados (Menú → Carrera → Fin).
- [ ] Cronómetro y cruce de línea de meta (trigger).
- [ ] Pantalla de resultado final: récord, reintentar, volver al menú.

### 4.3. 27 de agosto — Controladores (1 punto)
- [ ] Controlador de juego (GameManager) consolidado.
- [ ] Controlador de sonido (SoundManager).
- [ ] Controlador de datos (persistencia).

### 4.4. 3 de septiembre — Mecánica principal (1 punto)
- [ ] Conducción completa: aceleración, frenado, giro, salto y aterrizaje.
- [ ] Física con suspensiones por raycast.
- [ ] Detecciones: meta, caída de pista.

### 4.5. 10 de septiembre — Sonido + datos persistentes (1 punto)
- [ ] Sonidos de motor, derrape, meta y música implementados.
- [ ] ScriptableObjects guardados en disco (récords, ajustes, desbloqueos).
- [ ] Tabla de récords del día.

### 4.6. 17 de septiembre — Efectos (1 punto)
- [ ] Partículas: humo de derrape, salto, llegada a meta.
- [ ] Cámara con Cinemachine.
- [ ] Iluminación ambientada low-poly.

### 4.7. 28 / 29 de septiembre — Versión final (1 punto)
- [ ] Build WebGL sin errores.
- [ ] UI final coherente con el arte.
- [ ] Carpeta en Drive compartida + link en el `README.md`.

### 4.8. 5 de noviembre — Difusión (2 puntos)
- [ ] Publicación en itch.io (0.5 puntos).
- [ ] Video promocional en YouTube (0.5 puntos).
- [ ] Decoración del ambiente relacionado al juego (1 punto): folletería, decoración del stand, volante 3D.

---

## 5. Dependencias y orden técnico lógico

1. **Escena de juego** (pista + auto) → para poder probar algo.
2. **Conducción** (mecánica) → base de la demo.
3. **GameManager + meta + cronómetro** → cierra el loop.
4. **UI de escenas** (menú y fin) → navegación completa.
5. **Sonido y datos** → pulido transversal.
6. **Efectos, cámara e iluminación** → estética.
7. **Build y publicación** → entregables finales.

> Nota: la fecha 13/8 pide UI, pero conviene tener una escena de juego
> funcional temprano porque el resto se prueba adentro de ella.

---

## 6. Criterios de terminado por entrega (checklist)

| Entrega | Criterio de aceptación |
|---|---|
| 13/8 | Se navega Menú → Jugar → Juego, y Jugar tiene animación. |
| 20/8 | Se inicia la carrera, se cronometra, se llega a la meta y aparece la pantalla de fin; caerse reinicia la vuelta. |
| 27/8 | Los 3 controladores existen y se usan desde los scripts. |
| 3/9 | El auto se conduce de forma estable por el circuito. |
| 10/9 | Hay sonido y los récords sobreviven al cierre del juego. |
| 17/9 | Se ven partículas y la cámara sigue al auto suavemente. |
| 28/9 | Build WebGL juegable sin errores, link en README. |
| 5/11 | Publicado en itch.io, video y decoración del stand. |

---

## 7. Riesgos y mitigación

- **Faltar una entrega (puntaje CERO)**: trabajar por hitos cortos y commitear
  siempre antes de cada fecha límite. Los trabajos fuera de tiempo no suman puntos.
- **El `.meta` se rompe al crear scripts fuera de Unity**: los scripts se crean
  desde dentro del editor o se abren los proyectos para regenerar los `.meta`.
- **WebGL exige archivos ligeros**: optimizar texturas y modelos low-poly.
- **No perder la carpeta `Library/`**: si se reconstruye, Unity reimporta todo
  (tarda pero no es grave; ya está en `.gitignore`).

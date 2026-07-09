# Hand Tracking con XR Hands — Guía de implementación

Documentación de referencia para implementar hand tracking, gestos personalizados e interacción por gestos en Unity. Basada en la implementación del gesto de menú de este proyecto.

**Stack verificado:**

| Componente | Versión |
|---|---|
| Unity | 6.x |
| XR Hands | 1.7.0 |
| XR Interaction Toolkit (XRI) | 3.2+ |
| OpenXR Plugin | 1.11+ |
| Dispositivos | Meta Quest 2 / Quest 3 / VIVE XR Elite |

---

## 1. Conceptos clave

- **XR Hands** (`com.unity.xr.hands`): capa de *datos*. Expone las 26 articulaciones por mano vía `XRHandSubsystem` y componentes como `XRHandTrackingEvents`.
- **XR Interaction Toolkit**: capa de *interacción* (poke, ray+pinch, grabs, UI).
- **Gestos de sistema**: pinch ≈ trigger, grasp ≈ grip, poke ≈ presionar botón. Llegan por el OpenXR *Hand Interaction Profile* (multiplataforma) o *Meta Hand Tracking Aim* (Quest).
- **Gestos personalizados**: assets `XRHandShape` (forma de dedos) o `XRHandPose` (forma + orientación), detectados por script. Solo poses estáticas; no hay swipes ni movimientos.

---

## 2. Configuración del proyecto

### 2.1 Paquetes y samples

1. Package Manager: instalar **XR Plug-in Management**, **OpenXR Plugin**, **XR Hands**, **XR Interaction Toolkit**.
2. Samples de **XRI**: importar *Starter Assets*, *Hands Interaction Demo*, *Spatial Keyboard* (este último lo pide el sample Hand Capture).
3. Samples de **XR Hands**: importar *HandVisualizer*, *Gestures*, *Hand Capture*.

> ⚠️ Al actualizar la versión de XRI: **borrar** las carpetas viejas de `Assets/Samples/XR Interaction Toolkit/` antes de reimportar. Actualizar encima rompe las referencias de scripts en las escenas demo.

### 2.2 OpenXR Features (Project Settings > XR Plug-in Management > OpenXR)

**Común (todas las plataformas):**
- ✅ Hand Tracking Subsystem
- ✅ Hand Interaction Profile (en la lista de Interaction Profiles)

**Meta Quest 2/3 (pestaña Android):**
- ✅ Meta Quest Support
- ✅ Meta Hand Tracking Aim
- ✅ Oculus Touch Controller Profile (para soporte de controles)

**VIVE XR Elite:**
- Instalar el **VIVE OpenXR Plugin** (registry de HTC)
- ✅ VIVE XR Hand Tracking (compatible con XR Hands, no requiere el Hand Tracking Subsystem)
- ✅ Hand Interaction Profile

> Usar Project Validation (misma ventana) y el botón **Fix** para resolver dependencias faltantes.

### 2.3 El rig

Usar el prefab **`XR Origin Hands (XR Rig)`** del Hands Interaction Demo. Incluye:

- `Camera Offset > Left/Right Controller` — visuales e interactors de controles.
- `Camera Offset > Left/Right Hand` — visuales e interactors de manos. Cada mano tiene su **`XRHandTrackingEvents`** (buscar en jerarquía con `t:XRHandTrackingEvents`). **Reutilizar estos componentes; no crear nuevos.**
- **`XR Input Modality Manager`** (en la raíz): alterna automáticamente entre control y mano, **por lado, de forma independiente**. Si un control está despierto, la mano de ese lado se desactiva.

---

## 3. Crear un gesto personalizado (XR Hand Capture)

Flujo resumido (requiere XR Hands 1.7+):

1. Compilar la escena `Assets/Samples/XR Hands/[versión]/Hands Capture/HandCapture.unity` al visor (target Android).
2. En el visor: **Start → Record** (máx. 60 s, 5 slots), hacer el gesto con la **palma hacia el visor**, variándolo ligeramente. **Stop → Save** con nombre descriptivo (`MenuPrueba`, `ThumbsUp_R`...).
3. En Unity: **Window > XR > XR Hand Capture** → conectar el visor por USB (Build Target debe ser **Android** o no lo detecta) → **Import From Connected Headset** (van a `Assets/Hand Recordings`).
4. Seleccionar la grabación → elegir el frame con el **slider de timeline** → elegir **Left/Right** → **Compute Finger Values**.
5. Ajustar targets/umbrales en el foldout (umbral default 0.15; subir a 0.2–0.25 para más tolerancia). **Eliminar las condiciones de dedos irrelevantes** para el gesto: menos condiciones = detección más robusta.
6. **Overwrite with Computed Shape** sobre un asset XRHandShape (crear con `Assets > Create > XR > Hand Interactions > Hand Shape`).
7. Validar con el **Hand Shape Debugger** del sample Gestures (barras de completitud por dedo, en tiempo real).

---

## 4. Detectar el gesto por script

### 4.1 Por qué NO usar el `StaticHandGesture` del sample directamente

El componente del sample Gestures fue escrito para la escena del debugger y su campo `Background (Image)` **no tiene null-check**: con Background en None lanza `NullReferenceException` en `Awake()` y cada vez que el gesto se dispara o termina. Además, editar scripts dentro de `Assets/Samples/` se pierde al reimportar el sample.

**Solución adoptada:** copia propia sin dependencias de UI → `HandGestureDetector.cs`.

### 4.2 `HandGestureDetector.cs` (detector genérico reutilizable)

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;

/// <summary>
/// Detecta un gesto estatico (XRHandShape o XRHandPose) y dispara UnityEvents.
/// Version limpia del sample StaticHandGesture, sin dependencias de UI.
/// Un componente = un gesto + una mano. Para ambas manos, usar dos instancias
/// (pueden compartir el mismo asset).
/// </summary>
public class HandGestureDetector : MonoBehaviour
{
    [SerializeField] XRHandTrackingEvents m_HandTrackingEvents; // arrastrar desde el rig
    [SerializeField] ScriptableObject m_HandShapeOrPose;        // asset XRHandShape o XRHandPose
    [SerializeField] Transform m_TargetTransform;               // solo para XRHandPose con orientacion relativa
    [SerializeField] float m_MinimumHoldTime = 0.2f;            // anti falsos positivos
    [SerializeField] float m_GestureDetectionInterval = 0.1f;   // throttle de evaluacion

    public UnityEvent gesturePerformed;
    public UnityEvent gestureEnded;

    XRHandShape m_HandShape;
    XRHandPose m_HandPose;
    bool m_WasDetected;
    bool m_PerformedTriggered;
    float m_TimeOfLastConditionCheck;
    float m_HoldStartTime;

    void OnEnable()
    {
        m_HandTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);

        m_HandShape = m_HandShapeOrPose as XRHandShape;
        m_HandPose = m_HandShapeOrPose as XRHandPose;
        if (m_HandPose != null && m_HandPose.relativeOrientation != null)
            m_HandPose.relativeOrientation.targetTransform = m_TargetTransform;
    }

    void OnDisable() => m_HandTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);

    void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
    {
        if (!isActiveAndEnabled ||
            Time.timeSinceLevelLoad < m_TimeOfLastConditionCheck + m_GestureDetectionInterval)
            return;

        var detected =
            m_HandTrackingEvents.handIsTracked &&
            m_HandShape != null && m_HandShape.CheckConditions(eventArgs) ||
            m_HandPose != null && m_HandPose.CheckConditions(eventArgs);

        if (!m_WasDetected && detected)
        {
            m_HoldStartTime = Time.timeSinceLevelLoad;
        }
        else if (m_WasDetected && !detected)
        {
            m_PerformedTriggered = false;
            gestureEnded?.Invoke();
        }

        m_WasDetected = detected;

        if (!m_PerformedTriggered && detected &&
            Time.timeSinceLevelLoad - m_HoldStartTime > m_MinimumHoldTime)
        {
            gesturePerformed?.Invoke();
            m_PerformedTriggered = true;
        }
    }
}
```

Notas de diseño:
- `XRHandShape.CheckConditions()` es la API oficial: usa las tolerancias del asset y hace early-out. **No** usar `HandShapeCompletenessCalculator` (es solo para la UI del debugger).
- El par `m_WasDetected` / `m_PerformedTriggered` convierte estado en **eventos de transición**: `gesturePerformed` se dispara UNA vez por gesto (crítico si el evento instancia objetos).

### 4.3 Montaje en escena

1. GameObject vacío (ej. `GestoMenu`) → agregar `HandGestureDetector`.
2. **Hand Tracking Events**: arrastrar el de la mano deseada desde el rig (ej. `Left Hand Interaction Visual`). El gesto solo se detecta con esa mano.
3. **Hand Shape Or Pose**: el asset creado con Hand Capture.
4. Conectar `gesturePerformed` / `gestureEnded` a los métodos del receptor.

---

## 5. Receptor de ejemplo: mostrar un menú (canvas world-space)

`MenuManosTour.cs` — al detectar el gesto, posiciona y muestra un canvas frente al usuario. El canvas se cierra mediante un **botón de cerrar** dentro del propio canvas.

```csharp
using UnityEngine;
public class MenuManosTour : MonoBehaviour
{
    public GameObject canvasMenu; //Canvas que necesitamos mostrar
    public GameObject reference; //Punto de anclaje a "orbitar"
    public float canvasOffset = 0.433f; //Distancia de punto de anclaje
    public bool canvasState = false;
    public void OnGesturePerformed()
    {
        if (!canvasState)
        {
            Debug.Log("Gesto de menu detectado");
            Vector3 forward = reference.transform.forward;
            Vector3 nuevaPos = reference.transform.position + forward * canvasOffset;
            Vector3 lookDirection = new Vector3(forward.x, 0, forward.z);
            canvasMenu.transform.position = new Vector3(nuevaPos.x, reference.transform.position.y, nuevaPos.z);
            canvasMenu.transform.rotation = Quaternion.LookRotation(lookDirection);
            canvasMenu.SetActive(true);
            canvasState = true;
        }
    }
    public void ResetCanvasState() 
    {
        if (canvasState)
        {
            canvasState = false;
        }
    }
    public void OnGestureEnded() 
    {
        Debug.Log("Gesto de menu terminado");
    }
}
```

### Campos (inspector)

| Campo | Descripción |
|---|---|
| `canvasMenu` | Canvas world-space que se muestra/oculta. Debe iniciar **desactivado** en la escena. |
| `reference` | Punto de anclaje frente al cual aparece el menú. Normalmente la **Main Camera** del rig (la cabeza del usuario). |
| `canvasOffset` | Distancia en metros entre `reference` y el canvas (0.433 ≈ distancia cómoda de lectura/alcance). |
| `canvasState` | Flag anti-duplicados: `true` mientras el menú está abierto. Impide que el gesto reposicione o reabra el menú ya visible. |

### Métodos y funcionamiento

- **`OnGesturePerformed()`** — conectado al evento `gesturePerformed` del `HandGestureDetector`. Solo actúa si el menú está cerrado (`!canvasState`):
  1. Toma el `forward` de `reference` y proyecta la posición del menú a `canvasOffset` metros al frente.
  2. La posición final conserva la **altura (Y) de `reference`**, de modo que el menú aparece a la altura de los ojos.
  3. La rotación usa `lookDirection` (el forward con Y = 0, aplanado): el canvas queda **vertical**, sin inclinarse aunque el usuario mire hacia arriba o abajo al hacer el gesto. `LookRotation(lookDirection)` orienta el +Z del canvas alejándose del usuario, que es la orientación correcta para que la UI de un canvas world-space quede de frente y legible.
  4. Activa el canvas y marca `canvasState = true`.
- **`ResetCanvasState()`** — se acciona desde el **botón de cerrar del canvas** (evento `OnClick`). Solo resetea el flag para permitir volver a abrir el menú con el gesto. La desactivación visual del canvas (`SetActive(false)`) la realiza el propio botón por su lado.
- **`OnGestureEnded()`** — conectado al evento `gestureEnded` del detector. Actualmente solo hace log; punto de extensión para lógica futura.

### Cableado

1. `HandGestureDetector.gesturePerformed` → `MenuManosTour.OnGesturePerformed`
2. `HandGestureDetector.gestureEnded` → `MenuManosTour.OnGestureEnded`
3. Botón "cerrar" del canvas, `OnClick` → **dos acciones**: `canvasMenu.SetActive(false)` **y** `MenuManosTour.ResetCanvasState`

> ⚠️ Regla de mantenimiento: el cierre está repartido en dos acciones del `OnClick` del botón. Si se agrega **otra** vía de cierre (otro gesto, timeout, teleport…), esa vía debe ejecutar **ambas** (desactivar el canvas y `ResetCanvasState`), o el flag quedará desincronizado y el gesto dejará de reabrir el menú.

### Limitaciones conocidas (documentadas, aceptadas por ahora)

- Si el usuario hace el gesto mirando **casi en vertical** (piso/techo), `lookDirection ≈ (0,0,0)` y `Quaternion.LookRotation` lanza el error *"Look rotation viewing vector is zero"*; además, como la posición usa el `forward` completo (3D), la distancia horizontal efectiva se reduce y el menú aparece más cerca de la cara. Mitigación futura: aplanar y normalizar el `forward` antes de usarlo para posición y rotación.

Requisitos del canvas para que los botones respondan a las manos:
- Canvas en **World Space**, escala pequeña (ej. 0.001).
- Reemplazar `GraphicRaycaster` por **Tracked Device Graphic Raycaster**.
- EventSystem con **XR UI Input Module** (el rig del demo ya lo trae).
- Botones grandes (≥ 3 cm en mundo) si se usará poke.

---

## 6. Modality Manager: controles vs. manos

Comportamiento normal: el `XR Input Modality Manager` activa **control O mano por lado**. Un control despierto = su mano desaparece. Esto es lo esperado en producción (el usuario suelta los controles y aparecen las manos).

**Durante desarrollo**, para forzar solo-manos:
- Opción física: quitar las pilas de los controles (infalible), o dejarlos inmóviles y fuera de vista.
- Opción en Unity: en el `XR Input Modality Manager`, poner **None** en `Left/Right Controller` (sin borrar los GameObjects). ⚠️ **Restaurar las referencias antes del build final.**
- Los campos `Left/Right Hand` del manager **siempre deben quedar asignados**.

---

## 7. Troubleshooting

| Síntoma | Causa | Solución |
|---|---|---|
| Unity no detecta el Quest por USB | Popup de depuración no aceptado / modo desarrollador off / cable solo-carga / driver | Aceptar el diálogo con el visor puesto; activar modo desarrollador en la app Meta Horizon; cable de datos sin hub; instalar Meta Quest Developer Hub. Diagnóstico: `adb devices` (`unauthorized` = popup; vacío = cable/driver) |
| `adb devices` lo ve pero Unity no | Build target ≠ Android / conflicto de ADB | Switch Platform a Android; cerrar SideQuest/MQDH; `adb kill-server`; reiniciar Unity |
| Una mano no aparece en Play | Su control está despierto (Modality Manager) | Ver sección 6. Diagnóstico: en Play, observar si `Right Hand` se desactiva y `Right Controller` se activa en la jerarquía |
| La mano sigue sin aparecer con controles apagados | Cambios hechos durante Play mode (se revierten) / referencia `Hand` vaciada en el manager / Handedness incorrecto | Verificar en modo edición; revisar Overrides del prefab del rig |
| Manos erráticas probando por Link | Hand tracking sobre Link no habilitado | App Meta Quest Link > Configuración > Beta > habilitar hand tracking sobre Link; o probar con build nativo |
| NRE en `StaticHandGesture.Awake` | Campo `Background` en None (sample sin null-check) | Usar `HandGestureDetector` propio (sección 4) |
| El gesto dispara N veces por segundo | Falta lógica de transición performed/ended | Usar el patrón de flags de `HandGestureDetector` |
| El gesto dispara con poses de paso | `MinimumHoldTime` en 0 | Usar ≥ 0.2 s |
| Warnings `XR_META_environment_depth` / `XR_META_boundary_visibility` | Extensiones de oclusión/guardián no disponibles sobre Link | **Ignorar** — no afectan hand tracking. Desactivar esas features en OpenXR si molestan |
| Materiales rosas en escenas de samples | Proyecto URP con materiales Built-in | Importar el sample Hand Visualizer (incluye conversión automática a URP) |

---

## 8. Buenas prácticas

- **Gestos**: evitar formas parecidas a pinch/poke/grab (falsos positivos con los gestos de sistema). Evitar gestos que saquen las manos del campo de tracking o que sean físicamente difíciles (saludo de Spock). Tolerancias generosas y probar con varias personas y con la mano no dominante.
- **Calibrar en Quest 2**: su tracking es notablemente peor que el de Quest 3; si funciona ahí, funciona en todo.
- **Un `HandGestureDetector` por gesto y por mano.** Nombrarlos descriptivamente (`Gesto_Menu_L`).
- **Reutilizar los `XRHandTrackingEvents` del rig**; duplicarlos crea suscripciones redundantes al subsistema.
- **No editar scripts en `Assets/Samples/`**: copiarlos a `Assets/Scripts/` con otro nombre.
- **Feedback**: sin háptica de control, todo gesto/botón necesita confirmación visual o sonora.
- Mantener el flujo controles↔manos funcional en producción (Modality Manager con todas sus referencias).

---

## 9. Recursos

- Manual XR Hands 1.7 (gestos): `docs.unity3d.com/Packages/com.unity.xr.hands@1.7/manual/gestures/custom-gestures.html`
- XR Hand Capture: `docs.unity3d.com/Packages/com.unity.xr.hands@1.7/manual/gestures/xrhandcapture.html`
- Hands Interaction Demo (XRI): `docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/manual/samples-hands-interaction-demo.html`
- Hand Interaction Profile: `docs.unity3d.com/Packages/com.unity.xr.openxr@1.8/manual/features/handinteractionprofile.html`
- VIVE OpenXR (hand tracking): `developer.vive.com/resources/openxr/unity/tutorials/hand-tracking/`
- Blog Unity — herramientas de hand tracking: `unity.com/blog/hand-tracking-tools-for-unity-xr`
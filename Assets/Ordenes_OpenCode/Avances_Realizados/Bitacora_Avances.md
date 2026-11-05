# Bitácora de Avances — ¿Y si...? Perú

> **Documento de memoria técnica y de diseño.**
> Este archivo es acumulativo: cada día de trabajo debe añadir una entrada al final.
> Su objetivo es que cualquier persona (o IA) pueda retomar el proyecto sin releer todo el código.

- **Proyecto:** ¿Y si...? Perú
- **Motor:** Unity 6.3 — `6000.3.23f1`
- **Género:** Novela visual / aventura de historia alternativa
- **Plataforma objetivo:** PC / Mac / WebGL
- **Estado actual:** Prototipo base jugable con grafo narrativo y UI Toolkit
- **Última actualización:** 2026-11-05

---

## 1. Resumen ejecutivo

Se implementó la base completa del prototipo:

- Sistema de grafos narrativos con `StoryNode` (ScriptableObject).
- `GameManager` para recorrer el grafo y recibir decisiones binarias.
- `UIManager` para renderizar diálogos, retratos, fondos, typewriter y botones con **UI Toolkit**.
- 28 ScriptableObjects: 10 compartidos + 9 variantes `_SinFlota` + 9 variantes `_ConFlota`.
- Escena `What_If_Peru` montada con `UI_Manager`, `Game_Manager` y `EventSystem`.
- Grafo validado con las 8 combinaciones de decisiones posibles:
  - **Iquique A (sin flota) + Tarapacá A + Alto A/B** → `FINAL_01_Historico`.
  - **Iquique B (con flota) + Tarapacá A + Alto A** → `FINAL_01_Historico`.
  - **Iquique B (con flota) + Tarapacá A + Alto B** → `FINAL_02_PazArmada` (única ruta).
  - **Iquique A/B + Tarapacá B** → `FINAL_03_Desastre`.

La historia no usa inventarios ni variables de reputación. La posesión de la *Independencia* se codifica mediante dos líneas paralelas de nodos (`_SinFlota` y `_ConFlota`), manteniendo el sistema puro de grafos.

---

## 2. Documentos fuente leídos

| Documento | Ruta | Información extraída |
|---|---|---|
| GDD Técnico | `Assets/Ordenes_OpenCode/GDD Principal/Game Design Document Técnico.md` | Arquitectura general, reglas de diseño, flujo, 3 escenarios, 3 finales, UI, regla de 2 opciones |
| Contexto Histórico | `Assets/Ordenes_OpenCode/ContextoHistorico/Contexto Historico.md` | Narrativa de Iquique, Tarapacá y Alto de la Alianza; decisiones históricas/ucrónicas; finales |
| Guion Guerra del Pacífico | `Assets/Ordenes_OpenCode/ContextoHistorico/Guion_Guerra_Pacifico.md` | Guion definitivo con 19 nodos base (28 tras las variantes), diálogos, decisiones A/B y 3 finales |
| Tarea de Estructura | `Assets/Ordenes_OpenCode/Tareas/Estructura.md` | Carpetas, scripts base, Dummy Data |
| Tarea de UI | `Assets/Ordenes_OpenCode/Tareas/UI.md` | UXML, USS, jerarquía visual, adaptación del UIManager, setup de escena |

---

## 3. Decisiones de arquitectura

### 3.1 Sin inventarios ni variables complejas

El GDD elimina inventarios, confianza, reputación y cualquier stat numérica. La partida se define únicamente por:

- El `StoryNode` actual.
- La ruta de nodos recorrida.

No hay guardado de variables en `PlayerPrefs` ni en JSON todavía.

### 3.2 Estado de la línea temporal mediante variantes `_SinFlota` / `_ConFlota`

Para que la pérdida de la *Independencia* condicione el final sin usar variables, el camino de Tarapacá y Alto de la Alianza se duplicó en dos líneas paralelas:

- `_SinFlota`: se activa tras `05A_Pierde_Independencia`.
- `_ConFlota`: se activa tras `05B_Salva_Independencia`.

Cada línea tiene sus propios nodos `06`–`14`. El nodo `10B_Ataque_Pisagua` y los tres finales son compartidos.

Consecuencias:

- En la línea `_SinFlota`, `14_Alto_Decision_SinFlota` lleva **ambas opciones** a `FINAL_01_Historico`: sin poder naval, la historia resiste.
- En la línea `_ConFlota`, `14_Alto_Decision_ConFlota` lleva A → `FINAL_01_Historico` y B → `FINAL_02_PazArmada`.
- Por tanto, **solo la ruta B-A-B** (salvar la *Independencia* + retirada a Arica + ataque nocturno) produce La Paz Armada.

No se agregaron variables al `GameManager`: el estado sigue siendo el nodo actual, y la rama se deduce del sufijo del nodo.

### 3.3 Token global `{PLAYER_NAME}`

`StoryNode` define la constante:

```csharp
public const string PlayerNameToken = "{PLAYER_NAME}";
```

Cualquier `speakerName`, `dialogueText` o `choiceTexts` puede incluir `{PLAYER_NAME}`. `GameManager` resuelve el token antes de enviar el texto a la UI:

```csharp
node.ResolveSpeaker(PlayerName);
node.ResolveDialogue(PlayerName);
```

El valor de respaldo en código es `"Estratega"`. La escena usa `"Ramírez"` como nombre de demostración para que frases como “Estratega {PLAYER_NAME}” se lean como “Estratega Ramírez” y no como “Estratega Estratega”. Se puede cambiar desde el Inspector de `Game_Manager` o llamando a:

```csharp
GameManager.Instance.SetPlayerName("NombreDelJugador");
```

### 3.4 Flujo de lectura tipo novela visual

1. El nodo muestra el texto con efecto máquina de escribir.
2. El primer clic completa el texto si aún se está escribiendo.
3. El segundo clic:
   - Avanza automáticamente si el nodo tiene 1 salida.
   - Revela los botones A/B si el nodo tiene 2 salidas.
   - Termina la ruta si el nodo no tiene salidas.
4. En los Momentos Decisivos, la caja de diálogo se oculta y aparecen dos botones grandes centrados.
5. Al elegir, se oculta el contenedor de botones, se restaura la caja de diálogo y se carga el siguiente nodo.

### 3.5 UI Toolkit estricto

No se usa uGUI/Canvas. Toda la interfaz se construye con:

- `UIDocument`
- UXML
- USS
- `VisualElement`, `Label`, `Button`
- `ClickEvent`
- `StyleBackground`

---

## 4. Estructura del proyecto

```text
Assets/
├── Scripts/
│   ├── Narrative/
│   │   └── StoryNode.cs
│   └── Managers/
│       ├── GameManager.cs
│       └── UIManager.cs
├── ScriptableObjects/
│   └── Nodes/
│       ├── 01_Iquique_Intro.asset
│       ├── 02_Iquique_Grau.asset
│       ├── 03_Iquique_Reporte.asset
│       ├── 04_Iquique_Decision.asset
│       ├── 05A_Pierde_Independencia.asset
│       ├── 05B_Salva_Independencia.asset
│       ├── 06_Tarapaca_Intro_SinFlota.asset
│       ├── 07_Tarapaca_Caceres_SinFlota.asset
│       ├── 08_Tarapaca_Bolognesi_SinFlota.asset
│       ├── 09_Tarapaca_Decision_SinFlota.asset
│       ├── 10A_Retirada_Arica_SinFlota.asset
│       ├── 11_Alto_Intro_SinFlota.asset
│       ├── 12_Alto_Campero_SinFlota.asset
│       ├── 13_Alto_Montero_SinFlota.asset
│       ├── 14_Alto_Decision_SinFlota.asset
│       ├── 06_Tarapaca_Intro_ConFlota.asset
│       ├── 07_Tarapaca_Caceres_ConFlota.asset
│       ├── 08_Tarapaca_Bolognesi_ConFlota.asset
│       ├── 09_Tarapaca_Decision_ConFlota.asset
│       ├── 10A_Retirada_Arica_ConFlota.asset
│       ├── 11_Alto_Intro_ConFlota.asset
│       ├── 12_Alto_Campero_ConFlota.asset
│       ├── 13_Alto_Montero_ConFlota.asset
│       ├── 14_Alto_Decision_ConFlota.asset
│       ├── 10B_Ataque_Pisagua.asset
│       ├── FINAL_01_Historico.asset
│       ├── FINAL_02_PazArmada.asset
│       └── FINAL_03_Desastre.asset
├── UI/
│   ├── VisualNovelUI.uxml
│   ├── VisualNovelStyles.uss
│   ├── VisualNovelPanelSettings.asset
│   └── WhatIfTheme.tss
└── Scenes/
    └── What_If_Peru.unity
```

---

## 5. Scripts

### 5.1 `Assets/Scripts/Narrative/StoryNode.cs`

**Tipo:** `ScriptableObject`

**Menú de creación:**

```text
Create > ¿Y si...? Perú > Narrativa > Nodo de Historia
```

**Campos serializados:**

| Campo | Tipo | Uso |
|---|---|---|
| `speakerName` | `string` | Nombre visible sobre la caja de diálogo. Admite `{PLAYER_NAME}` |
| `dialogueText` | `string` (`[TextArea]`) | Texto narrativo. Admite `{PLAYER_NAME}` |
| `characterSprite` | `Sprite` | Retrato del interlocutor. Opcional |
| `backgroundSprite` | `Sprite` | Fondo del escenario histórico. Opcional |
| `nextNodes` | `List<StoryNode>` | 0 = final; 1 = avance automático; 2 = decisión binaria |
| `choiceTexts` | `List<string>` | Textos de los botones A y B |

**Propiedades de solo lectura:**

| Propiedad | Regla |
|---|---|
| `IsEnding` | `nextNodes.Count == 0` |
| `IsLinear` | `nextNodes.Count == 1` |
| `HasChoices` | `nextNodes.Count >= 2` |
| `ChoiceCount` | Cantidad de textos de opción, limitada a 2 |

**Métodos:**

```csharp
public string ResolveSpeaker(string playerName);
public string ResolveDialogue(string playerName);
public static string ResolveTokens(string value, string playerName);
```

**Validación de editor (`OnValidate`):**

- Inicializa las listas si son `null`.
- Recorta `nextNodes` a máximo 2 elementos.
- Recorta `choiceTexts` a máximo 2 elementos.

Esto refuerza la regla de oro del GDD: exactamente 2 opciones por Momento Decisivo.

---

### 5.2 `Assets/Scripts/Managers/GameManager.cs`

**Tipo:** `MonoBehaviour`

**Orden de ejecución:** `[DefaultExecutionOrder(-10)]`

**Singleton:**

```csharp
public static GameManager Instance { get; private set; }
```

**Campos serializados:**

| Campo | Tipo | Uso |
|---|---|---|
| `startingNode` | `StoryNode` | Primer nodo de la historia |
| `playerName` | `string` | Nombre que reemplaza `{PLAYER_NAME}` |
| `uiManager` | `UIManager` | Referencia a la UI. Si falta, se busca con `FindFirstObjectByType` |

**Propiedades y eventos:**

```csharp
public StoryNode CurrentNode { get; private set; }
public string PlayerName { get; }
public event Action<StoryNode> OnNodeChanged;
public event Action<StoryNode> OnStoryEnded;
```

**API pública:**

```csharp
public void StartGame();
public void SetPlayerName(string newPlayerName);
public void GoToNode(StoryNode node);
public void Advance();
public void SelectChoice(int index); // 0 = A, 1 = B
```

**Flujo de `Advance()`:**

```text
Si CurrentNode == null        -> StartGame()
Si CurrentNode.HasChoices     -> PresentChoices() si los botones están ocultos
Si CurrentNode.IsLinear       -> GoToNode(nextNodes[0])
Si CurrentNode.IsEnding       -> EndStory()
```

**Flujo de `SelectChoice(index)`:**

```text
Valida que el nodo actual tenga decisiones.
Valida el índice 0/1.
Toma nextNodes[index].
Si es nulo -> EndStory().
Si existe -> GoToNode(nextNode).
```

**Arranque automático:**

`Start()` llama a `StartGame()`. El prototipo entra directamente al Combate de Iquique. Todavía no existe pantalla de menú ni ingreso de nombre.

---

### 5.3 `Assets/Scripts/Managers/UIManager.cs`

**Tipo:** `MonoBehaviour`

**Requisito:** `[RequireComponent(typeof(UIDocument))]`

**Campos serializados:**

| Campo | Tipo | Uso |
|---|---|---|
| `uiDocument` | `UIDocument` | Referencia al documento. Si falta, se obtiene con `GetComponent` |
| `charactersPerSecond` | `float` | Velocidad del typewriter. Por defecto 45 |
| `useTypewriter` | `bool` | Permite desactivar el efecto |
| `indicatorPulseSpeed` | `float` | Velocidad del pulso del triángulo de avance |

**Referencias internas obtenidas con `root.Q<T>()`:**

- `RootScreen`
- `CharacterContainer`
- `ChoicesContainer`
- `DialoguePanel`
- `SpeakerNameText`
- `DialogueText`
- `NextIndicator`
- `BtnChoiceA`
- `BtnChoiceB`

**API pública:**

```csharp
public bool ChoicesVisible { get; }
public bool IsTyping { get; }

public void ShowDialogue(string speaker, string text, Sprite characterSprite, Sprite bgSprite);
public void ShowChoices(string choiceA, string choiceB, Action onChoiceA, Action onChoiceB);
public void HideChoices();
```

**Comportamiento de `ShowDialogue`:**

- Oculta las decisiones.
- Asigna nombre y texto.
- Asigna el fondo del escenario a `RootScreen`.
- Asigna el retrato a `CharacterContainer` y lo muestra u oculta según si el sprite es nulo.
- Reinicia el typewriter.
- Si `useTypewriter` está activo, inicia la corrutina; si no, muestra el texto completo.

**Comportamiento de `ShowChoices`:**

- Completa el typewriter si aún estaba escribiendo.
- Asigna los textos a `BtnChoiceA` y `BtnChoiceB`.
- Registra los callbacks en `clicked`.
- Muestra `ChoicesContainer` con `DisplayStyle.Flex`.
- Oculta `DialoguePanel` con `DisplayStyle.None`.
- Marca `choicesVisible = true`.

**Comportamiento de `HideChoices`:**

- Oculta `ChoicesContainer`.
- Restaura `DialoguePanel` a `DisplayStyle.Flex`.
- Marca `choicesVisible = false`.

**Input general:**

```csharp
rootScreen.RegisterCallback<ClickEvent>(OnRootScreenClicked);
```

Si el jugador hace clic y:

- Está escribiendo → completa el texto.
- Hay decisiones visibles → no hace nada.
- No hay decisiones visibles → llama a `GameManager.Instance.Advance()`.

**Protección contra doble avance:**

Los botones registran:

```csharp
btnChoiceA.RegisterCallback<ClickEvent>(StopClickPropagation);
btnChoiceB.RegisterCallback<ClickEvent>(StopClickPropagation);
```

`StopClickPropagation` ejecuta `evt.StopPropagation()` para que el clic del botón no llegue al `RootScreen` y dispare un avance adicional.

**Typewriter:**

- Usa `WaitForSecondsRealtime(1f / charactersPerSecond)`.
- Un clic durante la escritura llama a `CompleteTyping()`.
- Al terminar, muestra `NextIndicator` y activa su pulso.

**Indicador de avance:**

`Update()` anima la opacidad del triángulo con una onda senoidal usando `Time.unscaledTime`.

**Fondos:**

```csharp
element.style.backgroundImage = sprite != null
    ? new StyleBackground(sprite)
    : new StyleBackground(StyleKeyword.None);
```

---

## 6. Grafo narrativo

### 6.1 Inventario de nodos

**Nodos compartidos**

| # | Asset | Interlocutor | Tipo | Salidas |
|---|---|---|---|---|
| 01 | `01_Iquique_Intro` | Narrador | Lineal | → 02 |
| 02 | `02_Iquique_Grau` | Miguel Grau | Lineal | → 03 |
| 03 | `03_Iquique_Reporte` | Oficial de Señales | Lineal | → 04 |
| 04 | `04_Iquique_Decision` | Miguel Grau | Decisión | A → 05A, B → 05B |
| 05A | `05A_Pierde_Independencia` | Narrador | Lineal | → 06_SinFlota |
| 05B | `05B_Salva_Independencia` | Narrador | Lineal | → 06_ConFlota |
| 10B | `10B_Ataque_Pisagua` | Narrador | Lineal | → FINAL_03 |
| F1 | `FINAL_01_Historico` | Narrador | Final | — |
| F2 | `FINAL_02_PazArmada` | Narrador | Final | — |
| F3 | `FINAL_03_Desastre` | Narrador | Final | — |

**Variantes de Tarapacá y Alto**

| # | Asset `_SinFlota` | Asset `_ConFlota` | Interlocutor | Tipo |
|---|---|---|---|---|
| 06 | `06_Tarapaca_Intro_SinFlota` | `06_Tarapaca_Intro_ConFlota` | Narrador | Lineal |
| 07 | `07_Tarapaca_Caceres_SinFlota` | `07_Tarapaca_Caceres_ConFlota` | Andrés Avelino Cáceres | Lineal |
| 08 | `08_Tarapaca_Bolognesi_SinFlota` | `08_Tarapaca_Bolognesi_ConFlota` | Francisco Bolognesi | Lineal |
| 09 | `09_Tarapaca_Decision_SinFlota` | `09_Tarapaca_Decision_ConFlota` | Belisario Suárez | Decisión |
| 10A | `10A_Retirada_Arica_SinFlota` | `10A_Retirada_Arica_ConFlota` | Narrador | Lineal |
| 11 | `11_Alto_Intro_SinFlota` | `11_Alto_Intro_ConFlota` | Narrador | Lineal |
| 12 | `12_Alto_Campero_SinFlota` | `12_Alto_Campero_ConFlota` | Gral. Narciso Campero | Lineal |
| 13 | `13_Alto_Montero_SinFlota` | `13_Alto_Montero_ConFlota` | Lizardo Montero | Lineal |
| 14 | `14_Alto_Decision_SinFlota` | `14_Alto_Decision_ConFlota` | Francisco Bolognesi | Decisión |

**Salidas de las variantes**

| Nodo | Salidas |
|---|---|
| `06_*` | → `07_*` |
| `07_*` | → `08_*` |
| `08_*` | → `09_*` |
| `09_*` | A → `10A_*`, B → `10B_Ataque_Pisagua` |
| `10A_*` | → `11_*` |
| `11_*` | → `12_*` |
| `12_*` | → `13_*` |
| `13_*` | → `14_*` |
| `14_Alto_Decision_SinFlota` | A → `FINAL_01_Historico`, B → `FINAL_01_Historico` |
| `14_Alto_Decision_ConFlota` | A → `FINAL_01_Historico`, B → `FINAL_02_PazArmada` |

### 6.2 Conexiones exactas

```text
01_Iquique_Intro
└── → 02_Iquique_Grau → 03_Iquique_Reporte → 04_Iquique_Decision
    ├── A → 05A_Pierde_Independencia
    │        → 06_Tarapaca_Intro_SinFlota
    │        → 07_Tarapaca_Caceres_SinFlota
    │        → 08_Tarapaca_Bolognesi_SinFlota
    │        → 09_Tarapaca_Decision_SinFlota
    │            ├── A → 10A_Retirada_Arica_SinFlota
    │            │        → 11_Alto_Intro_SinFlota
    │            │        → 12_Alto_Campero_SinFlota
    │            │        → 13_Alto_Montero_SinFlota
    │            │        → 14_Alto_Decision_SinFlota
    │            │            ├── A → FINAL_01_Historico
    │            │            └── B → FINAL_01_Historico
    │            └── B → 10B_Ataque_Pisagua → FINAL_03_Desastre
    └── B → 05B_Salva_Independencia
             → 06_Tarapaca_Intro_ConFlota
             → 07_Tarapaca_Caceres_ConFlota
             → 08_Tarapaca_Bolognesi_ConFlota
             → 09_Tarapaca_Decision_ConFlota
                 ├── A → 10A_Retirada_Arica_ConFlota
                 │        → 11_Alto_Intro_ConFlota
                 │        → 12_Alto_Campero_ConFlota
                 │        → 13_Alto_Montero_ConFlota
                 │        → 14_Alto_Decision_ConFlota
                 │            ├── A → FINAL_01_Historico
                 │            └── B → FINAL_02_PazArmada
                 └── B → 10B_Ataque_Pisagua → FINAL_03_Desastre
```

### 6.3 Rutas verificadas

La notación A/B corresponde, en orden, a las decisiones de **Iquique → Tarapacá → Alto de la Alianza**. El sufijo `_S` / `_C` indica la línea `_SinFlota` / `_ConFlota` activada por Iquique.

| Ruta | Línea | Secuencia | Final |
|---|---|---|---|
| **A-A-A** | `_SinFlota` | 01 → 02 → 03 → 04 → 05A → 06_S → 07_S → 08_S → 09_S → 10A_S → 11_S → 12_S → 13_S → 14_S → FINAL_01 | El Curso Conocido |
| **A-A-B** | `_SinFlota` | 01 → 02 → 03 → 04 → 05A → 06_S → 07_S → 08_S → 09_S → 10A_S → 11_S → 12_S → 13_S → 14_S → FINAL_01 | El Curso Conocido |
| A-B-* | `_SinFlota` | 01 → 02 → 03 → 04 → 05A → 06_S → 07_S → 08_S → 09_S → 10B → FINAL_03 | El Desastre Prematuro |
| **B-A-A** | `_ConFlota` | 01 → 02 → 03 → 04 → 05B → 06_C → 07_C → 08_C → 09_C → 10A_C → 11_C → 12_C → 13_C → 14_C → FINAL_01 | El Curso Conocido |
| **B-A-B** | `_ConFlota` | 01 → 02 → 03 → 04 → 05B → 06_C → 07_C → 08_C → 09_C → 10A_C → 11_C → 12_C → 13_C → 14_C → FINAL_02 | La Paz Armada |
| B-B-* | `_ConFlota` | 01 → 02 → 03 → 04 → 05B → 06_C → 07_C → 08_C → 09_C → 10B → FINAL_03 | El Desastre Prematuro |

Las 8 combinaciones fueron validadas: **solo B-A-B** llega a `FINAL_02_PazArmada`; **A-A-B** ahora llega a `FINAL_01_Historico`.

### 6.4 Notas de diseño del grafo

- **La rama se elige en Iquique:** `05A_Pierde_Independencia` entra a la línea `_SinFlota`; `05B_Salva_Independencia` entra a `_ConFlota`.
- **`_SinFlota` no puede ganar:** en `14_Alto_Decision_SinFlota`, A y B llevan a `FINAL_01_Historico`. Sin la fragata, incluso la táctica perfecta no cambia el resultado.
- **`_ConFlota` solo gana con B:** en `14_Alto_Decision_ConFlota`, A → `FINAL_01_Historico` y B → `FINAL_02_PazArmada`.
- **`10B_Ataque_Pisagua` es compartido:** ambas líneas pueden elegirlo en Tarapacá; siempre termina en `FINAL_03_Desastre`.
- **Sin variables:** la línea activa se deduce del sufijo del nodo actual.

---

## 7. UI Toolkit

### 7.1 Assets de UI

| Archivo | Tipo | Uso |
|---|---|---|
| `Assets/UI/VisualNovelUI.uxml` | UXML | Árbol visual de la novela |
| `Assets/UI/VisualNovelStyles.uss` | USS | Estilos militares sobrios |
| `Assets/UI/VisualNovelPanelSettings.asset` | PanelSettings | Configuración del panel runtime |
| `Assets/UI/WhatIfTheme.tss` | ThemeStyleSheet | Tema por defecto de UI Toolkit |

### 7.2 Jerarquía del UXML

```xml
<ui:UXML ...>
    <Style src="VisualNovelStyles.uss" />
    <ui:VisualElement name="RootScreen" class="root-screen">
        <ui:VisualElement name="CharacterContainer" class="character-container" />
        <ui:VisualElement name="ChoicesContainer" class="choices-container">
            <ui:Button name="BtnChoiceA" class="choice-button" text="Opción A" />
            <ui:Button name="BtnChoiceB" class="choice-button" text="Opción B" />
        </ui:VisualElement>
        <ui:VisualElement name="DialoguePanel" class="dialogue-panel">
            <ui:Label name="SpeakerNameText" class="speaker-name" text="Interlocutor" />
            <ui:Label name="DialogueText" class="dialogue-text" text="..." />
            <ui:VisualElement name="NextIndicator" class="next-indicator" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### 7.3 Clases USS principales

| Clase | Función |
|---|---|
| `.root-screen` | Ocupa el 100 %, fondo oscuro y `scale-and-crop` para el escenario |
| `.character-container` | Retrato centrado, oculto por defecto |
| `.choices-container` | Capa fullscreen centrada, oculta por defecto con `display: none` |
| `.choice-button` | Botón grande, borde dorado, hover y active |
| `.dialogue-panel` | Panel inferior del 30 %, fondo semitransparente oscuro |
| `.speaker-name` | Nombre en dorado, negrita, 27 px |
| `.dialogue-text` | Texto claro, 22 px, wrap activo |
| `.next-indicator` | Triángulo dorado en la esquina inferior derecha |

### 7.4 PanelSettings

| Propiedad | Valor |
|---|---|
| `scaleMode` | `ScaleWithScreenSize` |
| `referenceResolution` | `1920 x 1080` |
| `screenMatchMode` | `MatchWidthOrHeight` |
| `match` | `0.5` |
| `themeStyleSheet` | `Assets/UI/WhatIfTheme.tss` |

### 7.5 Tema

`WhatIfTheme.tss` contiene:

```css
@import url("unity-theme://default");
```

Sin este tema, un `PanelSettings` creado por código/MCP puede quedarse sin estilos base y mostrar advertencias. El tema se asignó explícitamente al `PanelSettings`.

### 7.6 Comportamiento visual validado

- `RootScreen`: 1920 × 1080.
- `DialoguePanel`: 324 px de alto a 1080 px (30 %).
- `BtnChoiceA` / `BtnChoiceB`: 920 × 88 px, centrados.
- `SpeakerNameText`: 27 px, color `#D6B25E`.
- `DialogueText`: 22 px, color `#E8E4D8`.
- `NextIndicator`: 18 × 13 px, triángulo dorado.

### 7.7 Gotchas de UI Toolkit encontrados

1. **Propiedad de escala de fondo:** en Unity 6 la propiedad correcta es `-unity-background-scale-mode` (con guion inicial), no `unity-background-scale-mode`.
2. **Tema del PanelSettings:** el `PanelSettings` creado por MCP no asigna tema automáticamente. Se creó `WhatIfTheme.tss` y se asignó con `update_panel_settings`.
3. **Input System:** el proyecto tiene `activeInputHandler = 1` (nuevo Input System). El `EventSystem` usa `InputSystemUIInputModule` y `AssignDefaultActions()`.
4. **Propagación de clics:** los botones deben detener la propagación para que el clic no llegue al `RootScreen` y avance dos veces.
5. **Caja de diálogo en decisiones:** el GDD pide que desaparezca durante los Momentos Decisivos. Por eso `ShowChoices` oculta `DialoguePanel` y `HideChoices` lo restaura.

---

## 8. Escena `Assets/Scenes/What_If_Peru.unity`

### 8.1 GameObjects

| GameObject | Componentes | Configuración |
|---|---|---|
| `Main Camera` | Transform, Camera, AudioListener, UniversalAdditionalCameraData | Cámara principal |
| `UI_Manager` | Transform, UIDocument, UIManager | `UIDocument.visualTreeAsset = VisualNovelUI.uxml`; `panelSettings = VisualNovelPanelSettings.asset` |
| `Game_Manager` | Transform, GameManager | `startingNode = 01_Iquique_Intro`; `uiManager = UI_Manager`; `playerName = Ramírez` |
| `EventSystem` | Transform, EventSystem, InputSystemUIInputModule | Acciones por defecto asignadas |

### 8.2 Referencias verificadas

```text
GameManager.startingNode = Assets/ScriptableObjects/Nodes/01_Iquique_Intro.asset
GameManager.uiManager   = UI_Manager
GameManager.playerName  = Ramírez

UIDocument.visualTreeAsset = Assets/UI/VisualNovelUI.uxml
UIDocument.panelSettings   = Assets/UI/VisualNovelPanelSettings.asset
PanelSettings.themeStyleSheet = Assets/UI/WhatIfTheme.tss
PanelSettings.referenceResolution = (1920, 1080)
```

---

## 9. Configuración del proyecto

| Ajuste | Valor | Nota |
|---|---|---|
| Versión de Unity | `6000.3.23f1` | Unity 6.3 |
| Render Pipeline | URP `17.3.0` | `UniversalAdditionalCameraData` en la cámara |
| Input System | `com.unity.inputsystem 1.20.0` | `activeInputHandler = 1` |
| Run In Background | `true` | Evita que el typewriter se pause cuando el Editor pierde foco |
| Escena activa | `Assets/Scenes/What_If_Peru.unity` | Escena de trabajo |

**Pendiente:** agregar la escena a Build Settings antes de generar un build.

---

## 10. Validación realizada

### 10.1 Compilación

- Unity compiló los tres scripts sin errores.
- Consola limpia, salvo un aviso interno del WebSocket del MCP (`MCP-FOR-UNITY`), ajeno al juego.

### 10.2 Play Mode

- El árbol visual se construyó correctamente desde el UXML.
- El typewriter mostró el texto completo y reemplazó `{PLAYER_NAME}` por `Ramírez` (por ejemplo, “Estratega Ramírez”).
- El `NextIndicator` apareció al terminar el texto.
- Los botones de decisión se mostraron con los textos correctos y la caja de diálogo oculta.
- La pantalla final de La Paz Armada se renderizó correctamente.

### 10.3 Rutas

Se validaron las 8 combinaciones de decisiones contra el grafo de ScriptableObjects:

```text
Iquique=A Tarapaca=A Alto=A -> FINAL_01_Historico   (_SinFlota)
Iquique=A Tarapaca=A Alto=B -> FINAL_01_Historico   (_SinFlota)
Iquique=A Tarapaca=B Alto=A -> FINAL_03_Desastre    (_SinFlota)
Iquique=A Tarapaca=B Alto=B -> FINAL_03_Desastre    (_SinFlota)
Iquique=B Tarapaca=A Alto=A -> FINAL_01_Historico   (_ConFlota)
Iquique=B Tarapaca=A Alto=B -> FINAL_02_PazArmada   (_ConFlota)
Iquique=B Tarapaca=B Alto=A -> FINAL_03_Desastre    (_ConFlota)
Iquique=B Tarapaca=B Alto=B -> FINAL_03_Desastre    (_ConFlota)
```

Resultado clave: **A-A-B → `FINAL_01_Historico`** y **B-A-B → `FINAL_02_PazArmada`** (única ruta a La Paz Armada).

### 10.4 Capturas

Se tomaron capturas de verificación en `Assets/Screenshots` y luego se eliminaron para no dejar artefactos en el proyecto. Las capturas mostraron:

- Diálogo de Miguel Grau con fondo oscuro y panel inferior.
- Dos botones de decisión centrados con borde dorado.
- Pantalla final de La Paz Armada.

---

## 11. Limitaciones actuales y pendientes

### 11.1 Pendientes funcionales

1. **Pantalla de menú / título** (Start, Load, Options, Exit).
2. **Ingreso del nombre del jugador** antes de iniciar la historia. `SetPlayerName` ya está listo.
3. **Pantalla de resumen de final** con la “Línea Temporal Creada”.
4. **Sistema de guardado/carga JSON** para explorar ramas.
5. **Transiciones Fade to Black** entre escenas/niveles.
6. **Menú de pausa** con guardar, cargar y opciones.
7. **Ajustes de velocidad de texto, volumen y pantalla completa**.
8. **Códice de finales / medallas**.
9. **Audio** (música y SFX).
10. **Arte**: retratos y fondos para los 28 nodos (pueden reutilizarse entre variantes).
11. **Build Settings**: agregar `What_If_Peru` a la lista de escenas.
12. **Tests automatizados** de rutas y UI.

### 11.2 Limitaciones técnicas conocidas

- `GameManager.Start()` inicia la partida automáticamente; no hay menú.
- Los nodos tienen `characterSprite` y `backgroundSprite` en `null`; se ve el fondo oscuro del USS.
- `EndStory()` solo registra un log; no hay pantalla de fin dedicada.
- `ShowChoices` está diseñado para exactamente 2 opciones, según el GDD.
- Si `choiceTexts` tiene menos de 2 elementos, la UI usa `"Opción A"` y `"Opción B"` como respaldo.
- El typewriter usa `WaitForSecondsRealtime` por carácter; es suficiente para el prototipo, pero se puede optimizar.
- No hay validación de ciclos en el grafo (un nodo podría apuntar hacia atrás).
- No hay guardado del nodo actual.
- **Resuelto:** la inconsistencia de La Paz Armada se corrigió con las líneas `_SinFlota` / `_ConFlota`. Ahora `FINAL_02_PazArmada` solo es alcanzable por la ruta B-A-B.

---

## 12. Cómo extender el prototipo

### 12.1 Crear un nodo nuevo

1. Clic derecho en `Assets/ScriptableObjects/Nodes`.
2. `Create > ¿Y si...? Perú > Narrativa > Nodo de Historia`.
3. Nombrar el asset con el prefijo numérico correspondiente.
4. Completar:
   - `speakerName`
   - `dialogueText`
   - `characterSprite` (opcional)
   - `backgroundSprite` (opcional)
5. Arrastrar los nodos hijos a `nextNodes`.
6. Si hay 2 hijos, escribir los 2 textos en `choiceTexts`.

### 12.2 Asignar arte

- Arrastrar el sprite del personaje a `characterSprite`.
- Arrastrar el sprite del escenario a `backgroundSprite`.
- `UIManager` los aplica automáticamente con `StyleBackground`.
- Si `characterSprite` es `null`, el retrato se oculta.

### 12.3 Cambiar el nodo inicial

- Seleccionar `Game_Manager` en la escena.
- En el Inspector, cambiar `Starting Node`.
- También se puede hacer por código antes de `StartGame()`.

### 12.4 Cambiar el nombre del jugador

- Inspector de `Game_Manager` → `Player Name`.
- O por código:

```csharp
GameManager.Instance.SetPlayerName("NuevoNombre");
GameManager.Instance.StartGame();
```

### 12.5 Ajustar la velocidad del texto

- Seleccionar `UI_Manager`.
- En `UIManager`, cambiar `Characters Per Second`.
- Para desactivar el typewriter, desmarcar `Use Typewriter`.

### 12.6 Ajustar el pulso del indicador

- `UI_Manager` → `UIManager` → `Indicator Pulse Speed`.

---

## 13. Inventario de archivos

| Ruta | Propósito |
|---|---|
| `Assets/Scripts/Narrative/StoryNode.cs` | Modelo de nodo narrativo |
| `Assets/Scripts/Managers/GameManager.cs` | Control del grafo y estado de partida |
| `Assets/Scripts/Managers/UIManager.cs` | Puente entre grafo y UI Toolkit |
| `Assets/Editor/StoryGraphValidator.cs` | Validador y reparador del grafo desde el menú `Tools` |
| `Assets/ScriptableObjects/Nodes/01_Iquique_Intro.asset` | Introducción del Combate de Iquique |
| `Assets/ScriptableObjects/Nodes/02_Iquique_Grau.asset` | Miguel Grau ordena el rescate de náufragos |
| `Assets/ScriptableObjects/Nodes/03_Iquique_Reporte.asset` | Reporte de la persecución de la Covadonga |
| `Assets/ScriptableObjects/Nodes/04_Iquique_Decision.asset` | Decisión: confiar en Moore o abortar la persecución |
| `Assets/ScriptableObjects/Nodes/05A_Pierde_Independencia.asset` | Consecuencia histórica: encalla la Independencia |
| `Assets/ScriptableObjects/Nodes/05B_Salva_Independencia.asset` | Consecuencia ucrónica: se salva la Independencia |
| `Assets/ScriptableObjects/Nodes/06_Tarapaca_Intro_SinFlota.asset` | Tarapacá sin flota: introducción |
| `Assets/ScriptableObjects/Nodes/07_Tarapaca_Caceres_SinFlota.asset` | Tarapacá sin flota: victoria de Cáceres |
| `Assets/ScriptableObjects/Nodes/08_Tarapaca_Bolognesi_SinFlota.asset` | Tarapacá sin flota: crisis logística |
| `Assets/ScriptableObjects/Nodes/09_Tarapaca_Decision_SinFlota.asset` | Tarapacá sin flota: decisión |
| `Assets/ScriptableObjects/Nodes/10A_Retirada_Arica_SinFlota.asset` | Tarapacá sin flota: retirada a Arica |
| `Assets/ScriptableObjects/Nodes/11_Alto_Intro_SinFlota.asset` | Alto sin flota: introducción |
| `Assets/ScriptableObjects/Nodes/12_Alto_Campero_SinFlota.asset` | Alto sin flota: Campero |
| `Assets/ScriptableObjects/Nodes/13_Alto_Montero_SinFlota.asset` | Alto sin flota: Montero |
| `Assets/ScriptableObjects/Nodes/14_Alto_Decision_SinFlota.asset` | Alto sin flota: ambas opciones → FINAL_01 |
| `Assets/ScriptableObjects/Nodes/06_Tarapaca_Intro_ConFlota.asset` | Tarapacá con flota: introducción |
| `Assets/ScriptableObjects/Nodes/07_Tarapaca_Caceres_ConFlota.asset` | Tarapacá con flota: victoria de Cáceres |
| `Assets/ScriptableObjects/Nodes/08_Tarapaca_Bolognesi_ConFlota.asset` | Tarapacá con flota: crisis logística |
| `Assets/ScriptableObjects/Nodes/09_Tarapaca_Decision_ConFlota.asset` | Tarapacá con flota: decisión |
| `Assets/ScriptableObjects/Nodes/10A_Retirada_Arica_ConFlota.asset` | Tarapacá con flota: retirada a Arica |
| `Assets/ScriptableObjects/Nodes/11_Alto_Intro_ConFlota.asset` | Alto con flota: introducción |
| `Assets/ScriptableObjects/Nodes/12_Alto_Campero_ConFlota.asset` | Alto con flota: Campero |
| `Assets/ScriptableObjects/Nodes/13_Alto_Montero_ConFlota.asset` | Alto con flota: Montero |
| `Assets/ScriptableObjects/Nodes/14_Alto_Decision_ConFlota.asset` | Alto con flota: B → FINAL_02 |
| `Assets/ScriptableObjects/Nodes/10B_Ataque_Pisagua.asset` | Consecuencia ucrónica: desastre logístico (compartido) |
| `Assets/ScriptableObjects/Nodes/FINAL_01_Historico.asset` | Final: El Curso Conocido |
| `Assets/ScriptableObjects/Nodes/FINAL_02_PazArmada.asset` | Final: La Paz Armada |
| `Assets/ScriptableObjects/Nodes/FINAL_03_Desastre.asset` | Final: El Desastre Prematuro |
| `Assets/UI/VisualNovelUI.uxml` | Árbol visual |
| `Assets/UI/VisualNovelStyles.uss` | Estilos de la novela visual |
| `Assets/UI/VisualNovelPanelSettings.asset` | Configuración del panel runtime |
| `Assets/UI/WhatIfTheme.tss` | Tema base de UI Toolkit |
| `Assets/Scenes/What_If_Peru.unity` | Escena jugable |

---

## 14. Bitácora por día

### 2026-11-03 — Día 1: base del prototipo

**Objetivo:** leer los documentos y ejecutar las tareas de estructura y UI.

**Realizado:**

- Lectura del GDD Técnico, Contexto Histórico, `Estructura.md` y `UI.md`.
- Creación de carpetas:
  - `Assets/Scripts/Narrative`
  - `Assets/Scripts/Managers`
  - `Assets/ScriptableObjects/Nodes`
  - `Assets/UI`
- Creación de `StoryNode.cs`, `GameManager.cs` y `UIManager.cs`.
- Compilación limpia.
- Creación de 11 ScriptableObjects de prueba.
- Cableado del grafo con las rutas del GDD.
- Creación de `VisualNovelUI.uxml`, `VisualNovelStyles.uss`, `VisualNovelPanelSettings.asset` y `WhatIfTheme.tss`.
- Creación de la escena `What_If_Peru` con `UI_Manager`, `Game_Manager` y `EventSystem`.
- Asignación de referencias:
  - `startingNode = 01_Combate_Iquique`
  - `uiManager = UI_Manager`
  - `playerName = Estratega`
- Verificación en Play Mode:
  - Typewriter.
  - Reemplazo de `{PLAYER_NAME}`.
  - Pantalla de decisiones.
  - Tres rutas canónicas.
- Activación de `Run In Background` para evitar la pausa del typewriter cuando el Editor pierde foco.
- Eliminación de las capturas de prueba de `Assets/Screenshots`.

**Incidencias resueltas:**

- `manage_asset create_folder` falló una vez para `Assets/UI`; se creó la carpeta desde el sistema de archivos y se refrescó Unity.
- La USS usaba `unity-background-scale-mode`; Unity 6 exige `-unity-background-scale-mode`.
- El `PanelSettings` creado por MCP no tenía tema; se creó `WhatIfTheme.tss` y se asignó.
- El `EventSystem` necesitaba `InputSystemUIInputModule` por el Input System nuevo.
- El typewriter parecía detenerse; era la pausa del Editor en segundo plano. Se resolvió con `Run In Background`.

**Estado al cierre:** prototipo base jugable, sin errores de compilación, con las tres rutas del GDD funcionando.

---

### 2026-11-04 — Día 2: reemplazo por el guion definitivo

**Objetivo:** leer `Guion_Guerra_Pacifico.md`, eliminar los nodos de prueba y crear los 19 nodos del guion con su flujo exacto.

**Realizado:**

- Lectura del guion definitivo.
- Eliminación de los 11 ScriptableObjects de prueba.
- Creación de 19 nodos: 16 de historia + 3 finales.
- Cableado exacto de las decisiones de Iquique, Tarapacá y Alto de la Alianza.
- Asignación de `startingNode = 01_Iquique_Intro`.
- `playerName = Ramírez` como nombre de demostración.
- Validación programática de las 8 combinaciones de decisiones.
- Actualización de esta bitácora.

**Archivos modificados/creados:**

- `Assets/ScriptableObjects/Nodes/` — 19 assets nuevos.
- `Assets/Scenes/What_If_Peru.unity` — referencia al nuevo nodo inicial y nombre de demostración.
- `Assets/Ordenes_OpenCode/Avances_Realizados/Bitacora_Avances.md` — esta entrada.

**Decisiones tomadas:**

- Se respetó el guion literalmente, sin agregar nodos variantes.
- `10B_Ataque_Pisagua` termina en `FINAL_03_Desastre` y no llega al Alto.
- `14_Alto_Decision` A → `FINAL_01_Historico`, B → `FINAL_02_PazArmada`.

**Incidencias y soluciones:**

- `AssetDatabase.DeleteAsset` fue bloqueado por seguridad en `execute_code`; se usó `manage_asset delete` en lote.
- El guion no especifica los destinos de la decisión del Alto; se infirió A → `FINAL_01_Historico` y B → `FINAL_02_PazArmada`.
- El texto de `FINAL_02_PazArmada` menciona la fragata *Independencia*, pero la ruta puede alcanzarse tras perderla en Iquique. Se dejó literal y se registró como pendiente de diseño.

**Pendientes para el siguiente día:**

- Decidir si La Paz Armada debe exigir la supervivencia de la *Independencia*.
- Pantalla de ingreso de nombre y menú.
- Arte de retratos y fondos para los 19 nodos.

**Estado al cierre:** 19 nodos válidos, 0 referencias nulas, 8 rutas verificadas, consola limpia.

---

### 2026-11-04 — Día 3: variantes `_SinFlota` / `_ConFlota`

**Objetivo:** corregir la inconsistencia de La Paz Armada sin usar variables, duplicando el camino de Tarapacá y Alto de la Alianza según el resultado de Iquique.

**Realizado:**

- Renombrado de `06`–`14` a `_SinFlota`.
- Creación de las copias `_ConFlota` para `06`–`14`.
- `05A_Pierde_Independencia` → `06_Tarapaca_Intro_SinFlota`.
- `05B_Salva_Independencia` → `06_Tarapaca_Intro_ConFlota`.
- `10A_Retirada_Arica_SinFlota` → `11_Alto_Intro_SinFlota`; `10A_Retirada_Arica_ConFlota` → `11_Alto_Intro_ConFlota`.
- `14_Alto_Decision_SinFlota`: A y B → `FINAL_01_Historico`.
- `14_Alto_Decision_ConFlota`: A → `FINAL_01_Historico`, B → `FINAL_02_PazArmada`.
- `10B_Ataque_Pisagua` se mantiene compartido y termina en `FINAL_03_Desastre`.
- Validación de las 8 combinaciones y prueba en Play Mode.

**Archivos modificados/creados:**

- `Assets/ScriptableObjects/Nodes/` — 28 assets (10 compartidos + 9 `_SinFlota` + 9 `_ConFlota`).
- `Assets/Ordenes_OpenCode/Avances_Realizados/Bitacora_Avances.md` — esta entrada y actualización del grafo.

**Decisiones tomadas:**

- La rama se codifica con el sufijo del nodo, no con variables.
- La línea `_SinFlota` no puede alcanzar La Paz Armada.
- La línea `_ConFlota` solo la alcanza con la opción B del Alto.
- `10B_Ataque_Pisagua` no se duplica porque su resultado es idéntico en ambas líneas.

**Validación:**

```text
A-A-A -> FINAL_01_Historico
A-A-B -> FINAL_01_Historico
A-B-* -> FINAL_03_Desastre
B-A-A -> FINAL_01_Historico
B-A-B -> FINAL_02_PazArmada
B-B-* -> FINAL_03_Desastre
```

**Estado al cierre:** 28 nodos válidos, 0 referencias nulas, solo B-A-B llega a La Paz Armada, consola limpia.

---

### 2026-11-05 — Día 4: incidencia crítica de enlaces y validador de grafo

**Objetivo:** corregir el fin de ruta prematuro en `01_Iquique_Intro` y blindar el grafo contra pérdidas de `nextNodes`.

**Incidencia:**

En Play Mode la partida terminaba en la primera pantalla con:

```text
[GameManager] Fin de ruta alcanzado: 01_Iquique_Intro
```

**Causa raíz:**

Los 28 `.asset` habían quedado serializados en disco con `nextNodes: []`, aunque en memoria el grafo se había validado correctamente en la sesión anterior. Los `choiceTexts` sí se conservaron. Al recargar el dominio, Unity reimportó los assets desde disco y perdió todas las salidas.

**Corrección aplicada:**

- Reescritura de `nextNodes` en los 25 nodos que tienen salidas, usando los GUID reales de cada `.meta` (formato `{fileID: 11400000, guid: ..., type: 2}`).
- Los 3 finales se mantuvieron con `nextNodes: []`.
- Verificación estática del YAML: 28 nodos, 3 finales, 20 lineales, 5 decisiones, 0 GUIDs duplicados, 0 referencias huérfanas.
- Simulación de las 8 rutas sobre el YAML: se mantiene A-A-B → `FINAL_01_Historico` y solo B-A-B → `FINAL_02_PazArmada`.

**Herramientas añadidas:**

- `Assets/Editor/StoryGraphValidator.cs`
  - `Tools → ¿Y si...? Perú → Validar enlaces del grafo`
  - `Tools → ¿Y si...? Perú → Reparar enlaces del grafo`
  - Validación silenciosa al cargar el Editor (solo reporta problemas).
- `[ContextMenu("Validate Links")]` en `StoryNode.cs` para validar un asset individual.
- `GameManager.EndStory()` ahora registra `LogError` si la ruta termina en un nodo que no empieza por `FINAL_`.

**Verificación en Unity (MCP reconectado):**

- Compilación limpia tras reimportar los `.asset` corregidos y los scripts nuevos.
- `Tools → ¿Y si...? Perú → Validar enlaces del grafo` → `Grafo OK: 28 nodos (3 finales, 20 lineales, 5 decisiones).`
- `Tools → ¿Y si...? Perú → Reparar enlaces del grafo` → `Enlaces reaplicados en 28 nodos.` y validación OK (idempotente).
- Play Mode con clics reales sobre `RootScreen`:
  - Secuencia observada: `01_Iquique_Intro → 02_Iquique_Grau → 03_Iquique_Reporte → 04_Iquique_Decision` en 7 clics.
  - En `04` aparecen los botones A/B con sus textos completos.
  - `{PLAYER_NAME}` se resolvió como `Ramírez` en `02_Iquique_Grau`.
  - Elegir A lleva a `05A_Pierde_Independencia`; avanzar lleva a `06_Tarapaca_Intro_SinFlota`.
- Las 6 rutas representativas se recorrieron en Play Mode:
  - A-A-A, A-A-B, B-A-A → `FINAL_01_Historico`
  - A-B-*, B-B-* → `FINAL_03_Desastre`
  - B-A-B → `FINAL_02_PazArmada`
- Consola sin errores ni advertencias. Escena guardada.

**Estado al cierre:** incidencia resuelta; flujo lineal y las 28 variantes operativas en Play Mode.

---

### Plantilla para nuevas entradas

Copiar al final del archivo y completar:

```markdown
### AAAA-MM-DD — Día N: Título breve

**Objetivo:**

**Realizado:**
- ...

**Archivos modificados/creados:**
- ...

**Decisiones tomadas:**
- ...

**Incidencias y soluciones:**
- ...

**Pendientes para el siguiente día:**
- ...

**Estado al cierre:**
- ...
```

---

## 15. Verificación rápida para futuras sesiones

### 15.1 Comprobar el estado del Editor

```text
MCP resource: mcpforunity://editor/state
```

Verificar:

- `play_mode.is_playing`
- `compilation.is_compiling`
- `advice.ready_for_tools`

### 15.2 Probar rutas por código en Play Mode

```csharp
// Ruta histórica: Iquique A → Tarapacá A → Alto A  (_SinFlota)
GameManager.Instance.StartGame();     // 01_Iquique_Intro
GameManager.Instance.Advance();       // 01 → 02
GameManager.Instance.Advance();       // 02 → 03
GameManager.Instance.Advance();       // 03 → 04
GameManager.Instance.SelectChoice(0); // 04: Iquique A (pierde la Independencia)
GameManager.Instance.Advance();       // 05A → 06_Tarapaca_Intro_SinFlota
GameManager.Instance.Advance();       // 06 → 07_Tarapaca_Caceres_SinFlota
GameManager.Instance.Advance();       // 07 → 08_Tarapaca_Bolognesi_SinFlota
GameManager.Instance.Advance();       // 08 → 09_Tarapaca_Decision_SinFlota
GameManager.Instance.SelectChoice(0); // 09: Tarapacá A (retirada a Arica)
GameManager.Instance.Advance();       // 10A → 11_Alto_Intro_SinFlota
GameManager.Instance.Advance();       // 11 → 12_Alto_Campero_SinFlota
GameManager.Instance.Advance();       // 12 → 13_Alto_Montero_SinFlota
GameManager.Instance.Advance();       // 13 → 14_Alto_Decision_SinFlota
GameManager.Instance.SelectChoice(0); // 14: Alto A
// Debe quedar en FINAL_01_Historico

// Validación clave: A-A-B (_SinFlota) NO debe dar La Paz Armada
GameManager.Instance.StartGame();
GameManager.Instance.Advance();       // 01 → 02
GameManager.Instance.Advance();       // 02 → 03
GameManager.Instance.Advance();       // 03 → 04
GameManager.Instance.SelectChoice(0); // 04: Iquique A (sin flota)
GameManager.Instance.Advance();       // 05A → 06_SinFlota
GameManager.Instance.Advance();       // 06 → 07_SinFlota
GameManager.Instance.Advance();       // 07 → 08_SinFlota
GameManager.Instance.Advance();       // 08 → 09_SinFlota
GameManager.Instance.SelectChoice(0); // 09: Tarapacá A
GameManager.Instance.Advance();       // 10A → 11_SinFlota
GameManager.Instance.Advance();       // 11 → 12_SinFlota
GameManager.Instance.Advance();       // 12 → 13_SinFlota
GameManager.Instance.Advance();       // 13 → 14_SinFlota
GameManager.Instance.SelectChoice(1); // 14: Alto B
// Debe quedar en FINAL_01_Historico (no en FINAL_02)

// Ruta La Paz Armada: Iquique B → Tarapacá A → Alto B  (_ConFlota)
GameManager.Instance.StartGame();     // 01_Iquique_Intro
GameManager.Instance.Advance();       // 01 → 02
GameManager.Instance.Advance();       // 02 → 03
GameManager.Instance.Advance();       // 03 → 04
GameManager.Instance.SelectChoice(1); // 04: Iquique B (salva la Independencia)
GameManager.Instance.Advance();       // 05B → 06_Tarapaca_Intro_ConFlota
GameManager.Instance.Advance();       // 06 → 07_Tarapaca_Caceres_ConFlota
GameManager.Instance.Advance();       // 07 → 08_Tarapaca_Bolognesi_ConFlota
GameManager.Instance.Advance();       // 08 → 09_Tarapaca_Decision_ConFlota
GameManager.Instance.SelectChoice(0); // 09: Tarapacá A (retirada a Arica)
GameManager.Instance.Advance();       // 10A → 11_Alto_Intro_ConFlota
GameManager.Instance.Advance();       // 11 → 12_Alto_Campero_ConFlota
GameManager.Instance.Advance();       // 12 → 13_Alto_Montero_ConFlota
GameManager.Instance.Advance();       // 13 → 14_Alto_Decision_ConFlota
GameManager.Instance.SelectChoice(1); // 14: Alto B (ataque nocturno)
// Debe quedar en FINAL_02_PazArmada (única ruta)

// Ruta Desastre: Tarapacá B termina la partida en Pisagua
GameManager.Instance.StartGame();     // 01_Iquique_Intro
GameManager.Instance.Advance();       // 01 → 02
GameManager.Instance.Advance();       // 02 → 03
GameManager.Instance.Advance();       // 03 → 04
GameManager.Instance.SelectChoice(1); // 04: Iquique B (ConFlota)
GameManager.Instance.Advance();       // 05B → 06_ConFlota
GameManager.Instance.Advance();       // 06 → 07_ConFlota
GameManager.Instance.Advance();       // 07 → 08_ConFlota
GameManager.Instance.Advance();       // 08 → 09_ConFlota
GameManager.Instance.SelectChoice(1); // 09: Tarapacá B (ataque a Pisagua)
GameManager.Instance.Advance();       // 10B_Ataque_Pisagua → FINAL_03
// Debe quedar en FINAL_03_Desastre
```

> `10B_Ataque_Pisagua` es compartido: el resultado es el mismo desde cualquiera de las dos líneas.

### 15.3 Inspeccionar la UI en Play Mode

```text
manage_ui(action="get_visual_tree", target="UI_Manager", max_depth=4)
```

Verificar:

- `RootScreen` 1920×1080.
- `DialoguePanel` 324 px de alto.
- `ChoicesContainer` visible solo durante decisiones.
- `SpeakerNameText` con el nombre correcto.
- `DialogueText` con el texto completo o parcial según el typewriter.
- `NextIndicator` visible al terminar el texto.

### 15.4 Validar y reparar el grafo narrativo

El proyecto incluye un validador de Editor que revisa los 28 nodos:

```text
Tools → ¿Y si...? Perú → Validar enlaces del grafo
Tools → ¿Y si...? Perú → Reparar enlaces del grafo
```

- **Validar** recorre `Assets/ScriptableObjects/Nodes`, comprueba que cada nodo no final tenga al menos una salida, que los finales tengan 0, que no haya referencias nulas y que las decisiones tengan exactamente 2 textos.
- **Reparar** reaplica el grafo esperado definido en `StoryGraphValidator.ExpectedLinks`. Es idempotente y sirve para recuperar los enlaces si se pierden al reserializar.
- Al cargar el Editor, una validación silenciosa escribe en consola **solo si encuentra problemas**.

Además, cada asset individual tiene `Validate Links` en su menú contextual de Inspector, y `GameManager.EndStory()` registra un `LogError` si una ruta termina en un nodo que no empieza por `FINAL_`.

### 15.5 Herramientas MCP usadas en este proyecto

| Herramienta | Uso |
|---|---|
| `manage_asset` | Crear carpetas y consultar assets |
| `create_script` | Crear los scripts C# |
| `execute_code` | Crear los ScriptableObjects y cablear referencias |
| `manage_ui` | Crear UXML/USS, PanelSettings y adjuntar UIDocument |
| `manage_gameobject` | Crear `UI_Manager`, `Game_Manager` y `EventSystem` |
| `manage_components` | Asignar `startingNode` |
| `manage_scene` | Guardar y consultar la escena |
| `manage_editor` | Entrar/salir de Play Mode |
| `read_console` | Verificar errores y advertencias |
| `manage_camera` | Capturas de verificación del Game View |

---

## 16. Reglas de oro del proyecto

1. **Máximo 2 opciones por Momento Decisivo.** No agregar una tercera opción sin actualizar el GDD y la UI.
2. **Sin inventarios ni stats.** Si se necesita recordar una decisión, reflejarla en el texto y en el nodo de destino; no agregar variables.
3. **Toda la UI es UI Toolkit.** No introducir uGUI/Canvas.
4. **El nombre del jugador siempre usa `{PLAYER_NAME}`.** No hardcodear nombres en los diálogos.
5. **Los nodos finales tienen `nextNodes` vacío.** No agregar salidas a un final sin una razón narrativa.
6. **Los textos deben ser breves y dinámicos.** Evitar “muros de texto” académicos.
7. **Cada decisión debe tener consecuencia lógica.** Las opciones ucrónicas deben ser plausibles.
8. **Documentar cada avance en este archivo.** Es la memoria del proyecto.

---

*Fin del documento.*

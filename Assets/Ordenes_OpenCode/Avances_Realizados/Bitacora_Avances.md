# Bitácora de Avances — ¿Y si...? Perú

> **Documento de memoria técnica y de diseño.**
> Este archivo es acumulativo: cada día de trabajo debe añadir una entrada al final.
> Su objetivo es que cualquier persona (o IA) pueda retomar el proyecto sin releer todo el código.

- **Proyecto:** ¿Y si...? Perú
- **Motor:** Unity 6.3 — `6000.3.23f1`
- **Género:** Novela visual / aventura de historia alternativa
- **Plataforma objetivo:** PC / Mac / WebGL
- **Estado actual:** Guion "Ecos del Pacífico (1873 - 1883)" implementado: 35 nodos, decisiones de hasta 3 opciones, 3 finales con reinicio, arte integrado y pantalla de inicio operativa.
- **Última actualización:** 2026-11-25

---

## 1. Resumen ejecutivo

El 2026-11-25 se reemplazó el guion original por **"Ecos del Pacífico (1873 - 1883)"**, el rework de 35 nodos y 3 finales. El prototipo queda así:

- Sistema de grafos narrativos con `StoryNode` (ScriptableObject).
- `GameManager` para recorrer el grafo y resolver decisiones de **1 a 3 opciones**.
- `UIManager` con **UI Toolkit** para diálogos, retratos laterales, fondos, banderas, typewriter y botones A/B/C.
- **35 ScriptableObjects** en `Assets/ScriptableObjects/Nodes`, organizados en 5 actos.
- Escena `What_If_Peru` montada con `UI_Manager`, `Game_Manager` y `EventSystem`.
- Escena `MainInicio` con menú, ingreso de nombre, opciones y salto temporal.
- Validador y reparador del grafo en el menú `Tools → ¿Y si...? Perú`.

Los tres finales son:

- `33_Final_Desastre_Prematuro` → FINAL 1: colapso temprano.
- `34_Final_Ficticio` → FINAL 2: Armisticio del Pacífico (ucrónico).
- `35_Final_Historico` → FINAL 3: memoria y reconstrucción (histórico).

Cada final muestra un único botón para **reiniciar la historia** desde la Bahía de Iquique. La historia sigue sin inventarios ni stats: el estado de la partida es únicamente el nodo actual.

---

## 2. Documentos fuente leídos

| Documento | Ruta | Información extraída |
|---|---|---|
| GDD Técnico | `Assets/Ordenes_OpenCode/GDD Principal/Game Design Document Técnico.md` | Arquitectura general, reglas de diseño, flujo, 3 escenarios, 3 finales, UI. Su regla de 2 opciones queda superada por el rework (máximo 3) |
| Contexto Histórico | `Assets/Ordenes_OpenCode/ContextoHistorico/Contexto Historico.md` | Narrativa de Iquique, Tarapacá y Alto de la Alianza; decisiones históricas/ucrónicas; finales |
| Guion Guerra del Pacífico | `Assets/Ordenes_OpenCode/ContextoHistorico/Guion_Guerra_Pacifico.md` | Guion anterior (19 nodos base, 28 con variantes). Reemplazado el 2026-11-25 |
| Guion Rework | `Assets/Ordenes_OpenCode/ContextoHistorico/Guion_Guerra_Pacifico_Rework.md` | **Guion vigente**: "Ecos del Pacífico (1873 - 1883)", 35 nodos, decisiones A/B/C y 3 finales |
| Tarea de Estructura | `Assets/Ordenes_OpenCode/Tareas/Estructura.md` | Carpetas, scripts base, Dummy Data |
| Tarea de UI | `Assets/Ordenes_OpenCode/Tareas/UI.md` | UXML, USS, jerarquía visual, adaptación del UIManager, setup de escena |

---

## 3. Decisiones de arquitectura

### 3.1 Sin inventarios ni variables complejas

El GDD elimina inventarios, confianza, reputación y cualquier stat numérica. La partida se define únicamente por:

- El `StoryNode` actual.
- La ruta de nodos recorrida.

No hay guardado de variables en `PlayerPrefs` ni en JSON todavía.

### 3.2 Ramas históricas y ficticias explícitas

El rework ya no usa las líneas paralelas `_SinFlota` / `_ConFlota`. Cada decisión abre un nodo propio:

- Las opciones **históricas** llevan a nodos con el desenlace real (Iquique, Angamos, Tarapacá, Arica, Lima, La Breña).
- Las opciones **ficticias** llevan a nodos con prefijo `Ficcion_` y desembocan en el final ucrónico.
- Las opciones **desastrosas** llevan a nodos con prefijo `Desastre_` y desembocan en el final prematuro.

Los tres finales son nodos con nombre `33_Final_…`, `34_Final_…` y `35_Final_…`. No son nodos sin salida: tienen una única opción ("Reiniciar la historia") que vuelve a `01_Iquique_Intro`, por lo que el grafo es un ciclo cerrado y no requiere guardar variables.

No se agregaron variables al `GameManager`: el estado sigue siendo el nodo actual. La naturaleza de la rama se deduce del nombre del nodo (`Ficcion_`, `Desastre_`, `Final_`).

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
   - Avanza automáticamente si el nodo tiene 1 salida y ningún texto de opción.
   - Revela los botones A/B/C si el nodo tiene 2 o 3 salidas.
   - Revela un único botón si el nodo tiene 1 salida con texto (finales: "Reiniciar").
   - Termina la ruta si el nodo no tiene salidas.
4. En los Momentos Decisivos, la caja de diálogo se oculta y aparecen los botones grandes centrados.
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
├── Editor/
│   ├── StoryGraphValidator.cs
│   └── ReworkScriptBuilder.cs
├── ScriptableObjects/
│   └── Nodes/  (35 assets)
│       ├── 01_Iquique_Intro.asset
│       ├── 02_Puente_Del_Huascar.asset
│       ├── 03_Origen_Antofagasta.asset
│       ├── 04_Mision_Lavalle_Crisis.asset
│       ├── 05_Ficcion_Mediacion_Aduana.asset
│       ├── 06_Desastre_Ruptura_Total.asset
│       ├── 07_Ficcion_Escuadra_Alistada.asset
│       ├── 08_Rescate_En_Iquique.asset
│       ├── 09_Correrias_Del_Huascar.asset
│       ├── 10_Desastre_Varadura_Iquique.asset
│       ├── 11_Ficcion_Dominio_Naval.asset
│       ├── 12_Angamos_Desenlace.asset
│       ├── 13_Desierto_Y_Camarones.asset
│       ├── 14_Desastre_Sed_Atacama.asset
│       ├── 15_Ficcion_Tenaza_Tarapaca.asset
│       ├── 16_Victoria_Quebrada_Tarapaca.asset
│       ├── 17_Crisis_Politica_Lima.asset
│       ├── 18_Alianza_En_Tacna.asset
│       ├── 19_Batalla_Alto_Alianza.asset
│       ├── 20_Ficcion_Contragolpe_Tacna.asset
│       ├── 21_Desastre_Ruptura_Tacna.asset
│       ├── 22_Sitio_Puerto_Arica.asset
│       ├── 23_Ascenso_Morro_Arica.asset
│       ├── 24_Ficcion_Bastion_Arica.asset
│       ├── 25_Desembarco_En_Lurin.asset
│       ├── 26_Defensas_Rumbo_A_Lima.asset
│       ├── 27_Desastre_Caida_Inmediata_Lima.asset
│       ├── 28_Ficcion_Sorpresa_Lurin.asset
│       ├── 29_San_Juan_Y_Chorrillos.asset
│       ├── 30_Batalla_De_Miraflores.asset
│       ├── 31_Resistencia_De_La_Brena.asset
│       ├── 32_Ficcion_Victoria_Miraflores.asset
│       ├── 33_Final_Desastre_Prematuro.asset
│       ├── 34_Final_Ficticio.asset
│       └── 35_Final_Historico.asset
├── UI/
│   ├── VisualNovelUI.uxml
│   ├── VisualNovelStyles.uss
│   ├── VisualNovelPanelSettings.asset
│   ├── WhatIfTheme.tss
│   └── UIInicio/
│       ├── IntroUI.uxml
│       ├── IntroUIStyle.uss
│       └── IntroUIController.cs
└── Scenes/
    ├── MainInicio.unity
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
| `nextNodes` | `List<StoryNode>` | 0 = final sin salida; 1 = avance automático o una opción; 2-3 = decisión |
| `choiceTexts` | `List<string>` | Textos de los botones A, B y C. Su cantidad debe coincidir con `nextNodes` |

**Propiedades de solo lectura:**

| Propiedad | Regla |
|---|---|
| `IsEnding` | `nextNodes.Count == 0` |
| `IsLinear` | `nextNodes.Count == 1` y sin textos de opción |
| `HasChoices` | `ChoiceCount > 0` |
| `IsFinalNode` | El nombre empieza por `FINAL_` o contiene `_Final_` |
| `ChoiceCount` | Textos de opción útiles, limitados a 3 y nunca más que `nextNodes` |

**Métodos:**

```csharp
public string ResolveSpeaker(string playerName);
public string ResolveDialogue(string playerName);
public static string ResolveTokens(string value, string playerName);
```

**Validación de editor (`OnValidate`):**

- Inicializa las listas si son `null`.
- Recorta `nextNodes` a máximo 3 elementos.
- Recorta `choiceTexts` a máximo 3 elementos.

Esto refuerza la regla del GDD rework: **de 1 a 3 opciones por Momento Decisivo** (los finales usan una sola).

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
public void SelectChoice(int index); // 0 = A, 1 = B, 2 = C
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
Valida el índice 0/1/2 contra nextNodes.
Toma nextNodes[index].
Si es nulo -> EndStory().
Si existe -> GoToNode(nextNode).
```

**Finales con reinicio:**

Cuando `GoToNode` detecta un nodo final (`IsFinalNode`), registra el final y dispara `OnStoryEnded`. El nodo final tiene una salida con texto, así que `Advance()` muestra un único botón ("Reiniciar la historia") que vuelve a `01_Iquique_Intro`.

**Arranque automático:**

`Start()` llama a `StartGame()`. Si la escena se abre directamente, el juego entra al Combate de Iquique; en el flujo normal se llega desde `MainInicio`, que transporta el nombre del jugador por `GameSessionManager`.

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
public void ShowChoices(IReadOnlyList<string> choiceTexts, Action<int> onChoice);
public void ShowChoices(string choiceA, string choiceB, Action onChoiceA, Action onChoiceB); // compatibilidad
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
- Recorre los botones `BtnChoiceA`, `BtnChoiceB` y `BtnChoiceC`.
- Muestra solo los botones necesarios (1, 2 o 3) y oculta el resto con `DisplayStyle.None`.
- Asigna los textos de opción y registra un único callback por índice.
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
btnChoiceC.RegisterCallback<ClickEvent>(StopClickPropagation);
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

**Acto I — Iquique y las raíces del conflicto**

| # | Asset | Interlocutor | Tipo | Salidas |
|---|---|---|---|---|
| 01 | `01_Iquique_Intro` | Narrador | Lineal | → 02 |
| 02 | `02_Puente_Del_Huascar` | Miguel Grau | Lineal | → 03 |
| 03 | `03_Origen_Antofagasta` | Hilarión Daza | Decisión (3) | A → 05, B → 04, C → 06 |
| 04 | `04_Mision_Lavalle_Crisis` | Aníbal Pinto | Decisión (3) | A → 06, B → 07, C → 08 |
| 05 | `05_Ficcion_Mediacion_Aduana` | José Antonio de Lavalle | Lineal | → 07 |
| 06 | `06_Desastre_Ruptura_Total` | Mariano Ignacio Prado | Lineal | → 12 |
| 07 | `07_Ficcion_Escuadra_Alistada` | Mariano Ignacio Prado | Lineal | → 08 |

**Acto II — Campaña naval (Iquique y Angamos)**

| # | Asset | Interlocutor | Tipo | Salidas |
|---|---|---|---|---|
| 08 | `08_Rescate_En_Iquique` | Arturo Prat | Decisión (3) | A → 09, B → 10, C → 11 |
| 09 | `09_Correrias_Del_Huascar` | Miguel Grau | Decisión (2) | A → 11, B → 12 |
| 10 | `10_Desastre_Varadura_Iquique` | Miguel Grau | Lineal | → 33 |
| 11 | `11_Ficcion_Dominio_Naval` | Miguel Grau | Lineal | → 15 |
| 12 | `12_Angamos_Desenlace` | Miguel Grau | Lineal | → 13 |

**Acto III — Campaña de Tarapacá**

| # | Asset | Interlocutor | Tipo | Salidas |
|---|---|---|---|---|
| 13 | `13_Desierto_Y_Camarones` | Hilarión Daza | Decisión (3) | A → 15, B → 14, C → 16 |
| 14 | `14_Desastre_Sed_Atacama` | Belisario Suárez | Lineal | → 33 |
| 15 | `15_Ficcion_Tenaza_Tarapaca` | Belisario Suárez | Lineal | → 18 |
| 16 | `16_Victoria_Quebrada_Tarapaca` | Belisario Suárez | Lineal | → 17 |
| 17 | `17_Crisis_Politica_Lima` | Nicolás de Piérola | Lineal | → 18 |

**Acto IV — Tacna y el Morro de Arica**

| # | Asset | Interlocutor | Tipo | Salidas |
|---|---|---|---|---|
| 18 | `18_Alianza_En_Tacna` | Narciso Campero | Decisión (3) | A → 19, B → 20, C → 21 |
| 19 | `19_Batalla_Alto_Alianza` | Lizardo Montero | Lineal | → 22 |
| 20 | `20_Ficcion_Contragolpe_Tacna` | Lizardo Montero | Lineal | → 24 |
| 21 | `21_Desastre_Ruptura_Tacna` | Narciso Campero | Lineal | → 33 |
| 22 | `22_Sitio_Puerto_Arica` | Francisco Bolognesi | Decisión (2) | A → 24, B → 23 |
| 23 | `23_Ascenso_Morro_Arica` | Francisco Bolognesi | Lineal | → 25 |
| 24 | `24_Ficcion_Bastion_Arica` | Francisco Bolognesi | Lineal | → 34 |

**Acto V — Defensa de Lima, resistencia y finales**

| # | Asset | Interlocutor | Tipo | Salidas |
|---|---|---|---|---|
| 25 | `25_Desembarco_En_Lurin` | Nicolás de Piérola | Decisión (3) | A → 27, B → 26, C → 28 |
| 26 | `26_Defensas_Rumbo_A_Lima` | Andrés Avelino Cáceres | Lineal | → 29 |
| 27 | `27_Desastre_Caida_Inmediata_Lima` | Nicolás de Piérola | Lineal | → 33 |
| 28 | `28_Ficcion_Sorpresa_Lurin` | Andrés Avelino Cáceres | Lineal | → 34 |
| 29 | `29_San_Juan_Y_Chorrillos` | Andrés Avelino Cáceres | Lineal | → 30 |
| 30 | `30_Batalla_De_Miraflores` | Andrés Avelino Cáceres | Decisión (2) | A → 31, B → 32 |
| 31 | `31_Resistencia_De_La_Brena` | Andrés Avelino Cáceres | Lineal | → 35 |
| 32 | `32_Ficcion_Victoria_Miraflores` | Andrés Avelino Cáceres | Lineal | → 34 |

**Finales**

| # | Asset | Interlocutor | Tipo | Salida |
|---|---|---|---|---|
| 33 | `33_Final_Desastre_Prematuro` | Narrador | Final (1 opción) | Reiniciar → 01 |
| 34 | `34_Final_Ficticio` | Narrador | Final (1 opción) | Reiniciar → 01 |
| 35 | `35_Final_Historico` | Narrador | Final (1 opción) | Reiniciar → 01 |

Resumen: **35 nodos** = 23 lineales + 9 decisiones (4 de tres opciones y 5 de dos) + 3 finales.

### 6.2 Conexiones exactas

```text
01_Iquique_Intro → 02_Puente_Del_Huascar → 03_Origen_Antofagasta

03_Origen_Antofagasta
├── A → 05_Ficcion_Mediacion_Aduana → 07_Ficcion_Escuadra_Alistada
│                                          ↳ 08_Rescate_En_Iquique
├── B → 04_Mision_Lavalle_Crisis
│        ├── A → 06_Desastre_Ruptura_Total → 12_Angamos_Desenlace
│        ├── B → 07_Ficcion_Escuadra_Alistada
│        └── C → 08_Rescate_En_Iquique
└── C → 06_Desastre_Ruptura_Total → 12_Angamos_Desenlace

08_Rescate_En_Iquique
├── A → 09_Correrias_Del_Huascar
│        ├── A → 11_Ficcion_Dominio_Naval → 15_Ficcion_Tenaza_Tarapaca
│        └── B → 12_Angamos_Desenlace → 13_Desierto_Y_Camarones
├── B → 10_Desastre_Varadura_Iquique → 33_Final_Desastre_Prematuro
└── C → 11_Ficcion_Dominio_Naval → 15_Ficcion_Tenaza_Tarapaca → 18_Alianza_En_Tacna

13_Desierto_Y_Camarones
├── A → 15_Ficcion_Tenaza_Tarapaca → 18_Alianza_En_Tacna
├── B → 14_Desastre_Sed_Atacama → 33_Final_Desastre_Prematuro
└── C → 16_Victoria_Quebrada_Tarapaca → 17_Crisis_Politica_Lima → 18_Alianza_En_Tacna

18_Alianza_En_Tacna
├── A → 19_Batalla_Alto_Alianza → 22_Sitio_Puerto_Arica
│        ├── A → 24_Ficcion_Bastion_Arica → 34_Final_Ficticio
│        └── B → 23_Ascenso_Morro_Arica → 25_Desembarco_En_Lurin
├── B → 20_Ficcion_Contragolpe_Tacna → 24_Ficcion_Bastion_Arica → 34_Final_Ficticio
└── C → 21_Desastre_Ruptura_Tacna → 33_Final_Desastre_Prematuro

25_Desembarco_En_Lurin
├── A → 27_Desastre_Caida_Inmediata_Lima → 33_Final_Desastre_Prematuro
├── B → 26_Defensas_Rumbo_A_Lima → 29_San_Juan_Y_Chorrillos → 30_Batalla_De_Miraflores
│        ├── A → 31_Resistencia_De_La_Brena → 35_Final_Historico
│        └── B → 32_Ficcion_Victoria_Miraflores → 34_Final_Ficticio
└── C → 28_Ficcion_Sorpresa_Lurin → 34_Final_Ficticio

Finales → 01_Iquique_Intro (reinicio)
```

### 6.3 Rutas verificadas

Cada nodo de decisión tiene su propio conjunto de opciones (no hay una notación global A/B/C). Estas son las tres rutas representativas probadas en Play Mode:

| Ruta | Decisiones (nodo: opción) | Secuencia | Final |
|---|---|---|---|
| **Desastre corto** | 03:C, 13:B | 01 → 02 → 03 → 06 → 12 → 13 → 14 → 33 | `33_Final_Desastre_Prematuro` |
| **Ficticia** | 03:A, 08:C, 18:B | 01 → 02 → 03 → 05 → 07 → 08 → 11 → 15 → 18 → 20 → 24 → 34 | `34_Final_Ficticio` |
| **Histórica** | 03:B, 04:C, 08:A, 09:B, 13:C, 18:A, 22:B, 25:B, 30:A | 01 → 02 → 03 → 04 → 08 → 09 → 12 → 13 → 16 → 17 → 18 → 19 → 22 → 23 → 25 → 26 → 29 → 30 → 31 → 35 | `35_Final_Historico` |

Las tres rutas se recorrieron con `GameManager` en Play Mode y cada final reinició correctamente a `01_Iquique_Intro`.

Con 9 decisiones (4 de tres opciones y 5 de dos), el grafo admite muchas combinaciones; las tres anteriores cubren cada uno de los finales y los caminos ficticios y desastrosos principales.

### 6.4 Notas de diseño del grafo

- **Antofagasta abre tres caminos:** diplomacia (05), crisis con Lavalle (04) y ruptura inmediata (06).
- **La campaña naval decide el resto:** el rescate humanitario en Iquique (08 A) mantiene la ruta histórica; la varadura (08 B) termina en desastre; el aviso a la *Independencia* (08 C) abre la rama ficticia.
- **La rama ficticia no siempre gana:** solo el camino diplomático + naval + defensa coordinada llega a `34_Final_Ficticio`; una decisión ficticia aislada puede seguir llevando a desastre.
- **La rama histórica desemboca en 35:** el camino real (Angamos, Tarapacá, Arica, San Juan y Miraflores, La Breña) llega a `35_Final_Historico`.
- **Los finales se reinician:** tienen una salida con texto, no `nextNodes` vacío. Se reconocen por el segmento `_Final_`.
- **Sin variables:** la línea activa se deduce del nodo actual y del prefijo `Ficcion_` / `Desastre_`.

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
            <ui:Button name="BtnChoiceC" class="choice-button" text="Opción C" />
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
| `.choices-container` | Capa fullscreen centrada, oculta por defecto con `display: none`. Aloja hasta 3 botones |
| `.choice-button` | Botón grande, borde dorado, hover y active. Se muestran 1, 2 o 3 según el nodo |
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
- `BtnChoiceA` / `BtnChoiceB` / `BtnChoiceC`: 920 × 81 px, centrados. En una decisión de 3 opciones se ven los tres; en una de 2, el tercero se oculta; en un final se ve solo el A como "Reiniciar".
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

> **Nota:** esta sección documenta la validación del prototipo con el guion anterior (Días 1–6). La validación vigente del rework de 35 nodos está en la entrada del **Día 7** y en la sección 15.4.

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

1. ~~**Pantalla de menú / título** (Start, Load, Options, Exit).~~ → Resuelto (Día 6): pantalla `MainInicio` con menú, ingreso de nombre, opciones y salto temporal.
2. ~~**Ingreso del nombre del jugador** antes de iniciar la historia.~~ → Resuelto (Día 6): `GameSessionManager` transporta el nombre y `GameManager.SetPlayerName` lo aplica.
3. **Pantalla de resumen de final** con la "Línea Temporal Creada" (hoy el final solo muestra el texto y el botón de reinicio).
4. **Sistema de guardado/carga JSON** para explorar ramas.
5. **Transiciones Fade to Black** entre escenas/niveles.
6. **Menú de pausa** con guardar, cargar y opciones.
7. ~~**Ajustes de velocidad de texto, volumen y pantalla completa**.~~ → Parcial (Día 6): la UI de opciones existe y guarda los valores en `GameSessionManager`, pero el juego aún no los consume.
8. **Códice de finales / medallas**.
9. **Audio** (música y SFX).
10. ~~**Arte**: retratos y fondos para los 28 nodos.~~ → Resuelto (Día 7): los 35 nodos del rework tienen fondo y banderas; 31 tienen retrato.
11. ~~**Build Settings**: agregar `What_If_Peru` a la lista de escenas.~~ → Resuelto (Día 6): `MainInicio` (0) y `What_If_Peru` (1).
12. **Tests automatizados** de rutas y UI.
13. **Revisión de textos**: validar el guion rework con el diseñador antes de cerrar la narrativa.

### 11.2 Limitaciones técnicas conocidas

- `GameManager.Start()` inicia la partida automáticamente; el flujo normal pasa antes por `MainInicio`.
- Los nodos tienen arte asignado según el inventario de `Assets/Sprites`; si un sprite faltara, se ve el fondo oscuro del USS.
- `EndStory()` solo registra un log si un nodo queda sin salidas; los finales del rework no pasan por ahí porque ofrecen el botón de reinicio.
- `ShowChoices` acepta de 1 a 3 opciones. Más de 3 requiere cambiar `StoryNode.MaxChoices`, la UI y el validador.
- Si `choiceTexts` tiene menos textos que `nextNodes`, la UI usa `"Opción A"`, `"Opción B"` y `"Opción C"` como respaldo; el validador lo marca como problema.
- El typewriter usa `WaitForSecondsRealtime` por carácter; es suficiente para el prototipo, pero se puede optimizar.
- No hay validación de ciclos en el grafo (un nodo podría apuntar hacia atrás). Los finales usan el ciclo de reinicio de forma intencional.
- No hay guardado del nodo actual.
- **Resuelto:** el guion anterior y sus variantes `_SinFlota` / `_ConFlota` fueron reemplazados por el rework de 35 nodos.

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
6. Si hay 2 o 3 hijos, escribir la misma cantidad de textos en `choiceTexts` (A, B, C).

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
| `Assets/Editor/ReworkScriptBuilder.cs` | Generador idempotente de los 35 nodos del guion Rework |
| `Assets/ScriptableObjects/Nodes/01_Iquique_Intro.asset` | Acto I: introducción en la Bahía de Iquique |
| `Assets/ScriptableObjects/Nodes/02_Puente_Del_Huascar.asset` | Acto I: Grau y el abordaje de Prat |
| `Assets/ScriptableObjects/Nodes/03_Origen_Antofagasta.asset` | Acto I: embargo del salitre y decisiones A/B/C |
| `Assets/ScriptableObjects/Nodes/04_Mision_Lavalle_Crisis.asset` | Acto I: ultimátum de Pinto y decisiones A/B/C |
| `Assets/ScriptableObjects/Nodes/05_Ficcion_Mediacion_Aduana.asset` | Acto I: mediación diplomática (ficticia) |
| `Assets/ScriptableObjects/Nodes/06_Desastre_Ruptura_Total.asset` | Acto I: ruptura total (desastre) |
| `Assets/ScriptableObjects/Nodes/07_Ficcion_Escuadra_Alistada.asset` | Acto I: escuadra alistada (ficticia) |
| `Assets/ScriptableObjects/Nodes/08_Rescate_En_Iquique.asset` | Acto II: rescate y decisiones A/B/C |
| `Assets/ScriptableObjects/Nodes/09_Correrias_Del_Huascar.asset` | Acto II: correrías del Huáscar |
| `Assets/ScriptableObjects/Nodes/10_Desastre_Varadura_Iquique.asset` | Acto II: varadura y pérdida de la escuadra |
| `Assets/ScriptableObjects/Nodes/11_Ficcion_Dominio_Naval.asset` | Acto II: dominio naval (ficticio) |
| `Assets/ScriptableObjects/Nodes/12_Angamos_Desenlace.asset` | Acto II: muerte de Grau en Angamos |
| `Assets/ScriptableObjects/Nodes/13_Desierto_Y_Camarones.asset` | Acto III: marcha por Atacama y decisiones A/B/C |
| `Assets/ScriptableObjects/Nodes/14_Desastre_Sed_Atacama.asset` | Acto III: dispersión en el desierto |
| `Assets/ScriptableObjects/Nodes/15_Ficcion_Tenaza_Tarapaca.asset` | Acto III: defensa coordinada (ficticia) |
| `Assets/ScriptableObjects/Nodes/16_Victoria_Quebrada_Tarapaca.asset` | Acto III: victoria de Tarapacá |
| `Assets/ScriptableObjects/Nodes/17_Crisis_Politica_Lima.asset` | Acto III: crisis política de 1879 |
| `Assets/ScriptableObjects/Nodes/18_Alianza_En_Tacna.asset` | Acto IV: defensa aliada y decisiones A/B/C |
| `Assets/ScriptableObjects/Nodes/19_Batalla_Alto_Alianza.asset` | Acto IV: batalla del Alto de la Alianza |
| `Assets/ScriptableObjects/Nodes/20_Ficcion_Contragolpe_Tacna.asset` | Acto IV: contragolpe aliado (ficticio) |
| `Assets/ScriptableObjects/Nodes/21_Desastre_Ruptura_Tacna.asset` | Acto IV: ruptura del frente |
| `Assets/ScriptableObjects/Nodes/22_Sitio_Puerto_Arica.asset` | Acto IV: sitio de Arica y decisiones A/B |
| `Assets/ScriptableObjects/Nodes/23_Ascenso_Morro_Arica.asset` | Acto IV: asalto al Morro |
| `Assets/ScriptableObjects/Nodes/24_Ficcion_Bastion_Arica.asset` | Acto IV: Arica resiste (ficticio) |
| `Assets/ScriptableObjects/Nodes/25_Desembarco_En_Lurin.asset` | Acto V: desembarco en Lurín y decisiones A/B/C |
| `Assets/ScriptableObjects/Nodes/26_Defensas_Rumbo_A_Lima.asset` | Acto V: líneas de defensa |
| `Assets/ScriptableObjects/Nodes/27_Desastre_Caida_Inmediata_Lima.asset` | Acto V: caída inmediata de Lima |
| `Assets/ScriptableObjects/Nodes/28_Ficcion_Sorpresa_Lurin.asset` | Acto V: sorpresa en Lurín (ficticia) |
| `Assets/ScriptableObjects/Nodes/29_San_Juan_Y_Chorrillos.asset` | Acto V: San Juan y Chorrillos |
| `Assets/ScriptableObjects/Nodes/30_Batalla_De_Miraflores.asset` | Acto V: Miraflores y decisiones A/B |
| `Assets/ScriptableObjects/Nodes/31_Resistencia_De_La_Brena.asset` | Acto V: Campaña de la Breña |
| `Assets/ScriptableObjects/Nodes/32_Ficcion_Victoria_Miraflores.asset` | Acto V: victoria en Miraflores (ficticia) |
| `Assets/ScriptableObjects/Nodes/33_Final_Desastre_Prematuro.asset` | FINAL 1: Desastre Prematuro |
| `Assets/ScriptableObjects/Nodes/34_Final_Ficticio.asset` | FINAL 2: Armisticio del Pacífico |
| `Assets/ScriptableObjects/Nodes/35_Final_Historico.asset` | FINAL 3: Memoria y Reconstrucción |
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

### 2026-11-05 — Día 5: integración de arte, retratos laterales con fade-in y contador FPS

**Objetivo:**

Integrar los fondos, banderas y retratos ya presentes en `Assets/Sprites` en los 28 nodos, mostrar a los personajes con un fundido de opacidad en los laterales (nunca centrados) y añadir un contador de FPS en la esquina superior derecha.

**Realizado:**

- `StoryNode.cs`
  - Nuevo enum global `CharacterSide { Izquierda = 0, Derecha = 1 }`.
  - Nuevos campos serializados: `characterSide` y `List<Sprite> flagSprites`.
  - `OnValidate()` inicializa `flagSprites` si es nulo y recorta a un máximo de 3 banderas.
- `UIManager.cs`
  - `ShowNode(StoryNode, speaker, text)` como ruta principal; `ShowDialogue(...)` se conserva como compatibilidad.
  - `ShowCharacter(...)` posiciona el retrato, ajusta la clase lateral y arranca `FadeInCharacter()` (0.55 s, `SmoothStep`, tiempo no escalado).
  - `ApplyCharacterSide(...)` alterna `character-left` / `character-right`.
  - `ApplyFlags(...)` pinta hasta 3 banderas en la franja superior izquierda.
  - `UpdateFps()` actualiza cada 0.2 s el texto de FPS con suavizado; se ejecuta desde `Update()`.
- `GameManager.GoToNode()` ahora llama a `UIManager.ShowNode(node, ...)`.
- `VisualNovelUI.uxml`
  - `FlagStrip` con `FlagImageA/B/C`.
  - `FpsText` (esquina superior derecha).
- `VisualNovelStyles.uss`
  - `.character-left` / `.character-right` (3 % del borde, ancho 38 %).
  - `.flag-strip`, `.flag-image` (54 × 36 px con borde dorado).
  - `.fps-text` (dorado sobre fondo semitransparente).
- Asignación programática de arte a los 28 nodos con `execute_code` + `AssetDatabase.LoadAllAssetsAtPath`:
  - **28/28** nodos con fondo.
  - **8** nodos con retrato: Grau (2), Cáceres (2), Bolognesi (4); 4 a la izquierda y 4 a la derecha.
  - **28/28** nodos con banderas: Perú + Chile en Iquique/Tarapacá; Perú + Bolivia + Chile en Alto de la Alianza y `FINAL_01`; Perú + Chile en `FINAL_02` y `FINAL_03`.

**Mapa de fondos aplicado:**

| Nodos | Fondo |
|---|---|
| 01, 02, 04 | `Combate_naval_en_la_bahia_de_Iquique-3` |
| 03 | `Huascar_en_la_costa_de_Atacama-1` |
| 05A, 05B | `Batalla_naval_frente_a_Punta_Angamos-4` |
| 06, 07, 09 (ambas líneas) | `Quebrada_de_Tarapaca_entre_humo_y_palmeras-7` |
| 08, 10B | `Columna_exhausta_en_el_desierto_de_Atacama-6` |
| 10A (ambas líneas) | `Ruta_polvorienta_hacia_Tacna-5` |
| 11, 12, 13, 14 (ambas líneas) | `Meseta_de_Alto_de_la_Alianza-11` |
| FINAL_01 | `Morro_de_Arica_puerto_y_baterias-9` |
| FINAL_02 | `Meseta_de_Alto_de_la_Alianza-11` |
| FINAL_03 | `Columna_exhausta_en_el_desierto_de_Atacama-6` |

**Retratos aplicados:**

| Nodo(s) | Personaje | Lado |
|---|---|---|
| 02, 04 | Miguel Grau | Izquierda |
| 07 (ambas líneas) | Andrés Avelino Cáceres | Derecha |
| 08 (ambas líneas) | Francisco Bolognesi | Izquierda |
| 14 (ambas líneas) | Francisco Bolognesi | Derecha |

**Verificación en Play Mode:**

- `02_Iquique_Grau`: fondo de Iquique, Grau a la izquierda, banderas Perú/Chile y FPS visible.
- `07_Tarapaca_Caceres_ConFlota`: fondo de Tarapacá, Cáceres a la derecha, banderas Perú/Chile.
- `14_Alto_Decision_ConFlota`: fondo del Alto, Bolognesi a la derecha y tres banderas (Perú, Bolivia, Chile).
- Capturas guardadas en `Assets/Screenshots/` (`screenshot-20261105-171134.png`, `...171312.png`, `...171338.png`).
- Consola sin errores ni advertencias tras la recompilación.

**Incidencias y soluciones:**

- El aviso `The referenced script (Unknown) on this Behaviour is missing!` apareció una vez durante la recarga de dominio; se limpió la consola, no volvió a reproducirse y la escena no contiene componentes con script faltante.
- El Play Mode se detuvo durante una pausa manual de verificación; se reinició sin errores y las capturas se repitieron con `Time.timeScale = 0` para congelar la escena sin perder el render.
- No existen retratos para el Oficial de Señales, Belisario Suárez, Campero ni Montero; esos nodos se apoyan en fondo + banderas.

**Pendientes para el siguiente día:**

- Retratos propios para Oficial de Señales, Belisario Suárez, Campero y Montero (o decisión de diseño de dejarlos sin retrato).
- Menú principal, ingreso de nombre, pantalla de resumen de final y fade to black.
- Guardado JSON, pausa, ajustes, códice y audio.
- Configurar Build Settings y pruebas automatizadas.

**Estado al cierre:** arte integrado en los 28 nodos, retratos laterales con fade-in, banderas por campaña y contador FPS operativos; compilación limpia y Play Mode verificado.

---

### 2026-11-09 — Día 6: pantalla de inicio (menú + ingreso de nombre + opciones)

**Objetivo:** construir la pantalla de inicio que el GDD pedía como flujo previo al juego (título → ingreso de nombre → salto temporal → nivel 1) y conectarla con la escena `What_If_Peru`.

**Realizado:**

- Creación de la carpeta `Assets/UI/UIInicio` con los assets de UI Toolkit:
  - `IntroUI.uxml` — árbol visual con 4 vistas: `MainMenuView` (título), `NameInputView`, `OptionsView` e `IntroAnimationView` (salto temporal).
  - `IntroUIStyle.uss` — estilos con el mismo tema oscuro + dorado militar (`rgb(214,178,94)`) que `VisualNovelStyles.uss`, para coherencia visual con el juego.
- Creación de `GameSessionManager.cs` (Singleton persistente `DontDestroyOnLoad`) como puente entre escenas: transporta `playerName`, `musicVolume`, `sfxVolume`, `textSpeed` y `fullscreen`. Recuerda el nombre del jugador mediante `PlayerPrefs`.
- Creación de `IntroUIController.cs`:
  - Gestiona el cambio entre las 4 vistas mediante `ShowView()`.
  - Al confirmar el nombre, guarda las preferencias y dispara la animación de salto temporal (parpadeo del cronómetro y del año "1879...").
  - Carga la escena `What_If_Peru` al terminar la transición.
  - El botón "Salir" funciona también en el Editor (`EditorApplication.isPlaying = false`).
- Modificación de `GameManager.cs`: en `Awake()` lee el nombre desde `GameSessionManager.Instance` y lo aplica con `SetPlayerName()`, de modo que `{PLAYER_NAME}` se resuelve con el nombre real ingresado.
- Creación de la escena `MainInicio` con: `Main Camera`, `GameSessionManager`, `UI_Controller` (UIDocument + IntroUIController) y `EventSystem`.
- Configuración de la escena `MainInicio`:
  - `UIDocument.visualTreeAsset = IntroUI.uxml`.
  - `UIDocument.panelSettings = VisualNovelPanelSettings.asset` (escala 1920×1080 coherente con el juego).
  - `EventSystem` con `EventSystem` + `InputSystemUIInputModule` (`AssignDefaultActions()`), idéntico al de `What_If_Peru`.
- Build Settings: `MainInicio` en índice 0 y `What_If_Peru` en índice 1 (el juego arranca por el menú).

**Archivos modificados/creados:**

- `Assets/UI/UIInicio/IntroUI.uxml` — nuevo.
- `Assets/UI/UIInicio/IntroUIStyle.uss` — nuevo.
- `Assets/UI/UIInicio/IntroUIController.cs` — nuevo.
- `Assets/UI/UIInicio/GameSessionManager.cs` — nuevo.
- `Assets/Scripts/Managers/GameManager.cs` — lectura del nombre desde `GameSessionManager`.
- `Assets/Scenes/MainInicio.unity` — nueva escena de inicio.
- Build Settings — `MainInicio` (0) y `What_If_Peru` (1).
- `Assets/Ordenes_OpenCode/Avances_Realizados/Bitacora_Avances.md` — esta entrada.

**Decisiones tomadas:**

- Se usó un `GameSessionManager` persistente (en lugar de hacer `DontDestroyOnLoad` al `GameManager` existente) para no romper el arranque directo de `What_If_Peru` en pruebas: si se carga sin pasar por el menú, `GameManager` usa el respaldo "Estratega".
- El nombre se recuerda con `PlayerPrefs` como conveniencia de UX; no es un sistema de guardado de partida (sigue pendiente el JSON).
- El estilo de la UI de inicio se alineó al tema oscuro + dorado del juego en lugar del "documento sepia" que sugería el GDD, para mantener coherencia visual con `VisualNovelStyles.uss`.
- Se eliminó el botón "Cargar Partida" por no existir todavía el sistema de guardado; queda como pendiente.

**Incidencias y soluciones:**

- `Button.clicked` no admite el operador null-conditional (`?.clicked += ...`) y lanzaba `CS0079`. Se resolvió obteniendo cada botón con `root.Q<Button>()`, verificando `null` y suscribiendo con `if (...) button.clicked += ...`.
- La escena `MainInicio` no tenía `EventSystem`; sin él, los botones de UI Toolkit no recibían clics con el nuevo Input System. Se creó con `InputSystemUIInputModule` + `AssignDefaultActions()`.
- Al mover archivos `.uxml`/`.uss` por sistema de archivos, Unity generó advertencias de `.meta` huérfanos. Se recrearon los archivos con `write` para que Unity regenere `.meta` limpios.

**Pendientes para el siguiente día:**

- Pantalla de resumen de final con la "Línea Temporal Creada".
- Sistema de guardado/carga JSON (y habilitar el botón "Cargar Partida").
- Transiciones Fade to Black genéricas entre escenas.
- Menú de pausa.
- Aplicar las preferencias (volumen/velocidad) al juego: actualmente se guardan en `GameSessionManager` pero el `UIManager`/audio aún no las consume.
- Códice de finales / medallas y audio.

**Estado al cierre:** pantalla de inicio operativa (menú, ingreso de nombre, opciones y salto temporal) conectada a `What_If_Peru` con el nombre del jugador inyectado; compilación limpia y consola sin errores.

---

### 2026-11-25 — Día 7: guion "Ecos del Pacífico" (35 nodos, 3 opciones)

**Objetivo:** reemplazar el guion anterior por `Guion_Guerra_Pacifico_Rework.md`, adaptando el sistema de nodos y la UI para decisiones de hasta 3 opciones y finales con reinicio.

**Realizado:**

- `StoryNode.cs`
  - Nuevo máximo `MaxChoices = 3`; `OnValidate` recorta `nextNodes` y `choiceTexts` a 3.
  - `HasChoices` ahora depende de los textos útiles (`ChoiceCount > 0`), no solo de la cantidad de salidas.
  - `IsLinear` exige una salida y ningún texto de opción.
  - Nuevo `IsFinalNode`: detecta `FINAL_*` y también `*_Final_*` (nombres del rework).
  - `ValidateLinks` del Inspector entiende 1, 2 y 3 opciones.
- `GameManager.cs`
  - `SelectChoice` acepta índices 0, 1 y 2.
  - `PresentChoices` construye la lista de textos resueltos y la envía a la UI.
  - `GoToNode` registra el final y dispara `OnStoryEnded` al mostrar un nodo final.
- `UIManager.cs`
  - `ShowChoices(IReadOnlyList<string>, Action<int>)` muestra 1, 2 o 3 botones y oculta los sobrantes.
  - Se conserva la sobrecarga A/B por compatibilidad.
  - Nuevo `BtnChoiceC`, callback único por índice y limpieza al ocultar.
- `VisualNovelUI.uxml` y `VisualNovelStyles.uss`
  - Tercer botón `BtnChoiceC`.
  - Botones ajustados a 74 px mínimos, 20 px de fuente y márgenes de 10 px para que tres opciones quepan sin desbordar.
- `StoryGraphValidator.cs`
  - Grafo esperado reescrito con los 35 nodos y sus 50 enlaces.
  - Regla de finales: 1 salida al nodo 01 y 1 texto de reinicio.
  - Regla de decisiones: 2 o 3 salidas con la misma cantidad de textos.
- `ReworkScriptBuilder.cs` (nuevo)
  - Genera los 35 ScriptableObjects desde una tabla de datos, asigna fondo, retrato, lado, bandera y enlaces.
  - Borra los nodos anteriores de la carpeta y reapunta `GameManager.startingNode` de `What_If_Peru` al nuevo `01_Iquique_Intro`.
  - Menú: `Tools → ¿Y si...? Perú → Construir guion Rework`.
- Contenido del guion:
  - 5 actos: Antofagasta/Iquique, campaña naval, Tarapacá, Tacna/Arica y defensa de Lima.
  - 23 nodos lineales, 9 decisiones (4 de tres opciones y 5 de dos) y 3 finales.
  - 50 enlaces en total.
  - Arte: 35/35 con fondo y bandera; 31 con retrato (los 4 narradores sin personaje no lo usan).

**Archivos modificados/creados:**

- `Assets/Scripts/Narrative/StoryNode.cs` — máximo 3 opciones y detección de finales.
- `Assets/Scripts/Managers/GameManager.cs` — selección 0/1/2 y finales con reinicio.
- `Assets/Scripts/Managers/UIManager.cs` — botones A/B/C.
- `Assets/UI/VisualNovelUI.uxml` — `BtnChoiceC`.
- `Assets/UI/VisualNovelStyles.uss` — botones para 3 opciones.
- `Assets/Editor/StoryGraphValidator.cs` — grafo de 35 nodos.
- `Assets/Editor/ReworkScriptBuilder.cs` — nuevo generador idempotente.
- `Assets/ScriptableObjects/Nodes/` — 28 assets anteriores eliminados; 35 nuevos creados.
- `Assets/Scenes/What_If_Peru.unity` — `startingNode` reapuntado al nuevo nodo 01.
- `Assets/Ordenes_OpenCode/Avances_Realizados/Bitacora_Avances.md` — esta entrada y actualización de secciones.

**Decisiones tomadas:**

- Las decisiones pueden tener 1, 2 o 3 opciones; la regla de "exactamente 2" queda reemplazada por "máximo 3" para respetar el rework.
- Los finales no son nodos sin salida: tienen una opción de reinicio. Esto mantiene el grafo cerrado y permite volver a jugar sin recargar escena.
- Se eliminaron los 28 nodos antiguos para evitar duplicados y referencias huérfanas. El historial queda en git.
- Los retratos se asignan con un lado fijo por personaje (Grau, Piérola, Montero y Cáceres a la izquierda; Daza, Pinto, Lavalle, Prado, Prat, Suárez, Campero y Bolognesi a la derecha).
- El arte se toma del inventario existente en `Assets/Sprites`; no se agregaron sprites nuevos.

**Incidencias y soluciones:**

- El validador marcaba los tres finales como "lineales con textos de opción" porque `IsFinalNode` solo reconocía el prefijo `FINAL_`. Se amplió para reconocer el segmento `_Final_` de los nombres del rework.
- Unity no recompiló automáticamente tras los primeros cambios de scripts; se forzó con `Assets/Refresh` desde el MCP antes de continuar.
- El builder reasigna `startingNode` después de recrear los assets porque la recreación cambia los GUID.

**Verificación:**

- `Tools → ¿Y si...? Perú → Validar enlaces del grafo` → `Grafo OK: 35 nodos (3 finales, 23 lineales, 9 decisiones).`
- Play Mode:
  - `01_Iquique_Intro` se muestra con su texto, fondo, bandera peruana y bloqueo de FPS.
  - `03_Origen_Antofagasta` muestra los tres botones con sus textos completos y la caja de diálogo oculta.
  - Las rutas de los tres finales se recorrieron con `GameManager`:
    - `33_Final_Desastre_Prematuro` → reinicia a `01`.
    - `34_Final_Ficticio` → reinicia a `01`.
    - `35_Final_Historico` → reinicia a `01`.
  - Consola sin errores ni advertencias.
- Inventario de arte verificado por código: 35/35 fondos, 35/35 banderas, 31 retratos y 0 problemas.

**Pendientes para el siguiente día:**

- Pantalla de resumen de final con la "Línea Temporal Creada".
- Guardado/carga JSON y menú de pausa.
- Fade to black entre escenas.
- Consumir las preferencias de volumen/velocidad en `UIManager` y audio.
- Audio y códice de finales.
- Revisión de textos del rework con el diseñador.

**Estado al cierre:** guion "Ecos del Pacífico" implementado con 35 nodos, decisiones de hasta 3 opciones, 3 finales con reinicio, arte integrado y grafo validado; compilación limpia y Play Mode verificado.

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
// Ruta Desastre corto: 03 C → 06, 13 B → 14
GameManager.Instance.StartGame();     // 01
GameManager.Instance.Advance();       // 02
GameManager.Instance.Advance();       // 03
GameManager.Instance.SelectChoice(2); // 03 C → 06
GameManager.Instance.Advance();       // 06 → 12
GameManager.Instance.Advance();       // 12 → 13
GameManager.Instance.SelectChoice(1); // 13 B → 14
GameManager.Instance.Advance();       // 14 → 33_Final_Desastre_Prematuro

// Ruta Ficticia: 03 A → 05, 08 C → 11, 18 B → 20
GameManager.Instance.StartGame();
GameManager.Instance.Advance();       // 02
GameManager.Instance.Advance();       // 03
GameManager.Instance.SelectChoice(0); // 03 A → 05
GameManager.Instance.Advance();       // 05 → 07
GameManager.Instance.Advance();       // 07 → 08
GameManager.Instance.SelectChoice(2); // 08 C → 11
GameManager.Instance.Advance();       // 11 → 15
GameManager.Instance.Advance();       // 15 → 18
GameManager.Instance.SelectChoice(1); // 18 B → 20
GameManager.Instance.Advance();       // 20 → 24
GameManager.Instance.Advance();       // 24 → 34_Final_Ficticio

// Ruta Histórica completa
GameManager.Instance.StartGame();
GameManager.Instance.Advance();       // 02
GameManager.Instance.Advance();       // 03
GameManager.Instance.SelectChoice(1); // 03 B → 04
GameManager.Instance.SelectChoice(2); // 04 C → 08
GameManager.Instance.SelectChoice(0); // 08 A → 09
GameManager.Instance.SelectChoice(1); // 09 B → 12
GameManager.Instance.Advance();       // 12 → 13
GameManager.Instance.SelectChoice(2); // 13 C → 16
GameManager.Instance.Advance();       // 16 → 17
GameManager.Instance.Advance();       // 17 → 18
GameManager.Instance.SelectChoice(0); // 18 A → 19
GameManager.Instance.Advance();       // 19 → 22
GameManager.Instance.SelectChoice(1); // 22 B → 23
GameManager.Instance.Advance();       // 23 → 25
GameManager.Instance.SelectChoice(1); // 25 B → 26
GameManager.Instance.Advance();       // 26 → 29
GameManager.Instance.Advance();       // 29 → 30
GameManager.Instance.SelectChoice(0); // 30 A → 31
GameManager.Instance.Advance();       // 31 → 35_Final_Historico

// En cualquier final, SelectChoice(0) reinicia a 01_Iquique_Intro.
```

> Los índices de opción son locales a cada nodo (0 = A, 1 = B, 2 = C).

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

El proyecto incluye un validador de Editor que revisa los 35 nodos:

```text
Tools → ¿Y si...? Perú → Validar enlaces del grafo
Tools → ¿Y si...? Perú → Reparar enlaces del grafo
Tools → ¿Y si...? Perú → Construir guion Rework
```

- **Validar** recorre `Assets/ScriptableObjects/Nodes`, comprueba que no haya referencias nulas, que cada nodo no final tenga de 1 a 3 salidas con la misma cantidad de textos y que los finales tengan 1 salida al nodo 01 con 1 texto de reinicio.
- **Reparar** reaplica el grafo esperado definido en `StoryGraphValidator.ExpectedLinks`. Es idempotente y sirve para recuperar los enlaces si se pierden al reserializar.
- **Construir guion Rework** regenera los 35 assets desde `ReworkScriptBuilder`, reasigna arte y reapunta la escena al nodo 01.
- Al cargar el Editor, una validación silenciosa escribe en consola **solo si encuentra problemas**.

Además, cada asset individual tiene `Validate Links` en su menú contextual de Inspector, y `GameManager.EndStory()` registra un `LogError` si una ruta termina en un nodo sin salida que no es final.

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

1. **De 1 a 3 opciones por Momento Decisivo.** El rework usa decisiones de 2 y 3 opciones; los finales usan una sola. No agregar una cuarta opción sin actualizar el GDD, `StoryNode.MaxChoices`, la UI y el validador.
2. **Sin inventarios ni stats.** Si se necesita recordar una decisión, reflejarla en el texto y en el nodo de destino; no agregar variables.
3. **Toda la UI es UI Toolkit.** No introducir uGUI/Canvas.
4. **El nombre del jugador siempre usa `{PLAYER_NAME}`.** No hardcodear nombres en los diálogos.
5. **Los finales tienen una única opción de reinicio.** Su salida vuelve a `01_Iquique_Intro`; no agregar salidas narrativas a un final sin una razón de diseño.
6. **Los textos deben ser breves y dinámicos.** Evitar “muros de texto” académicos.
7. **Cada decisión debe tener consecuencia lógica.** Las opciones ucrónicas deben ser plausibles.
8. **Documentar cada avance en este archivo.** Es la memoria del proyecto.

---

*Fin del documento.*

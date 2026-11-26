Actúa como un Lead Unity Developer especialista en UI Toolkit y Unity MCP. Usando tus herramientas del MCP de Unity en este proyecto (`¿Y si...? Perú`, Unity `6000.3.23f1` URP), inspecciona el proyecto y ejecuta directamente los siguientes cambios **sin borrar, duplicar ni recrear ninguno de los 35 `StoryNode` ya existentes** en `Assets/ScriptableObjects/Nodes/`.

### Paso 0: Inspección previa obligatoria (MCP)
Lee `Assets/Scripts/Narrative/StoryNode.cs`, `Assets/Scripts/Managers/GameManager.cs`, `Assets/Scripts/Managers/UIManager.cs`, `Assets/UI/VisualNovelUI.uxml`, `Assets/UI/VisualNovelStyles.uss` y la escena `MainMenu` para conservar todas las referencias, sprites, banderas y fondos ya asignados en los 35 `.asset`.

---

### Paso 1: Asignación de los 35 Audios de Voz/Narración, Sincronización y Bloqueo Anti-Salto
1. En `StoryNode.cs`, agrega el campo serializado `public AudioClip voiceAudio;` (con valor por defecto `null` para no romper los assets existentes).
2. Crea y ejecuta un script temporal de Editor (`Assets/Editor/AssignNodeAudios.cs`) usando el MCP que recorra todas las subcarpetas de `Assets/Audio/Nodos/` (`Andres_Avelino_Caceres`, `Anibal_Pinto`, `Arturo_Prat`, `Belisario_Suarez`, `Francisco_Bolognesi`, `Hilarion_Daza`, `Jose_Antonio_de_Lavalle`, `Lizardo_Montero`, `Mariano_Ignacio_Prado`, `Miguel_Grau`, `Narciso_Campero`, `Narrador`, `Nicolas_de_Pierola`, `Nodos_Opcionales`) y asigne automáticamente cada `AudioClip` al campo `voiceAudio` del `StoryNode` correspondiente emparejando por nombre exacto/normalizado (ej. `Nodo 01_Iquique_Intro`, `Nodo 02_Puente_del_Huascar`, `Nodo 33_Final_Desastre_Prematuro`, `Nodo 34_Final_Ficticio`, `Nodo 35_Final_Historico`), guardando los assets con `EditorUtility.SetDirty` y `AssetDatabase.SaveAssets()`.
3. En `AudioManager.cs`, agrega un canal dedicado para voces (`AudioSource voiceSource`, `loop = false`) que reproduzca `node.voiceAudio` cada vez que `GameManager` cargue un `StoryNode` (deteniendo de inmediato cualquier audio de voz anterior al cambiar de nodo).
4. **Sincronización y Anti-Skip obligatorio en `UIManager.cs` / `GameManager.cs`:**
   - Cuando el nodo actual tenga `voiceAudio != null`, ajusta automáticamente la velocidad del efecto máquina de escribir (`charactersPerSecond`) según la duración del clip (`Mathf.Max(15f, text.Length / voiceAudio.length)`) para que el texto vaya coordinado con la voz.
   - **Bloquea el salto/avance de diálogo:** mientras se esté reproduciendo el audio de voz (`AudioManager.Instance.IsVoicePlaying`) **o** el texto siga escribiéndose (`IsTyping`), ignora cualquier clic en pantalla para avanzar al siguiente nodo o abrir las opciones de decisión, y mantén oculto `NextIndicator`. Recién cuando **ambos** (audio de voz + typewriter) hayan terminado por completo, muestra `NextIndicator` y habilita el clic para continuar o mostrar los botones de decisión.

---

### Paso 2: Música de Fondo por Escena y por Acto (`AudioManager.cs`)
1. Crea `Assets/Scripts/Managers/AudioManager.cs` como Singleton persistente (`DontDestroyOnLoad`) con dos `AudioSource` en bucle (`loop = true`) para realizar crossfade suave (~1.2 s) entre pistas musicales, más el `voiceSource` de voces/SFX.
2. Enlaza automáticamente los archivos `.ogg` existentes en `Assets/Audio/Musica/`:
   - **Escena `MainMenu`:** `MenuInicio.ogg`
   - **Acto 1 (`actNumber == 1`):** `ACTO I Iquique y las Raíces del Conflicto.ogg`
   - **Acto 2 (`actNumber == 2`):** `ACTO II La Campaña Naval (Iquique y Angamos).ogg`
   - **Acto 3 (`actNumber == 3`):** `ACTO III La Campaña de Tarapacá.ogg`
   - **Acto 4 (`actNumber == 4`):** `ACTO IV Tacna y el Morro de Arica.ogg`
   - **Acto 5 (`actNumber == 5`, incluyendo los 3 finales):** `ACTO V Defensa de Lima, la Resistencia y los Tres.ogg`
3. Haz que `GameManager` notifique a `AudioManager.Instance.PlayMusicForAct(node.actNumber)` al cambiar de nodo, cambiando la pista únicamente cuando cambie el número de Acto (sin reiniciar la canción entre nodos del mismo Acto).

---

### Paso 3: Renombrar Actos y Cartel Previo de Acto
1. Agrega en `StoryNode.cs` los campos serializados `public int actNumber = 1;`, `public string actTitle;` y `public bool isActStart;`.
2. En el script de Editor, asigna a los 35 nodos existentes su `actNumber` y `actTitle` usando estos nombres limpios (sin repeticiones), y marca `isActStart = true` únicamente en el primer nodo de cada Acto:
   - Acto 1: `Las Raíces del Conflicto (1879)`
   - Acto 2: `La Campaña Naval: Iquique y Angamos`
   - Acto 3: `La Campaña de Tarapacá`
   - Acto 4: `Tacna y el Morro de Arica`
   - Acto 5: `Defensa de Lima y la Resistencia`
3. En `VisualNovelUI.uxml` y `VisualNovelStyles.uss` (UI Toolkit estricto, sin Canvas/uGUI), agrega un overlay fullscreen `ActTransitionOverlay` (fondo oscuro, centrado, oculto por defecto) con `ActNumberLabel` y `ActTitleLabel`.
4. Al iniciar campaña o entrar a un nuevo Acto (`isActStart == true` o cambio de `actNumber`), muestra **primero** `ActTransitionOverlay` (`fade-in` -> pausa ~2 segundos -> `fade-out`) antes de mostrar el escenario, el personaje, la caja de diálogo y reproducir el audio del nodo.

---

### Paso 4: Transiciones Suaves de Actores/Escenarios y Ocultar Todo Durante las Decisiones
1. Separa el fondo del escenario en un `VisualElement` propio (`BackgroundLayer`) dentro de `RootScreen` con transición de opacidad en `VisualNovelStyles.uss`.
2. En `UIManager.cs`:
   - Al cambiar de nodo, haz desaparecer (`fade-out`) el retrato anterior y límpialo por completo (`StyleKeyword.None` y `DisplayStyle.None`) antes de hacer aparecer (`fade-in`) el nuevo personaje, evitando que queden sprites superpuestos cuando habla `Narrador` (`characterSprite == null`) o al cambiar de Acto.
   - Aplica también transición suave en `BackgroundLayer` cuando cambie `backgroundSprite`.
   - En `ShowChoices()`, oculta `DialoguePanel`, `CharacterContainer` y cualquier indicador/bandera de interlocutor para que durante las decisiones **no se vea ningún personaje ni texto superpuesto en pantalla**, únicamente el escenario de fondo y los botones de elección. En `HideChoices()`, vuelve a mostrar `CharacterContainer` solo si el siguiente nodo tiene `characterSprite`.

---

### Paso 5: Auditoría de Conexiones hacia los 3 Finales (`Nodo 33`, `Nodo 34`, `Nodo 35`)
1. Verifica mediante script de Editor las conexiones `nextNodes` de los 35 nodos existentes (sin recrearlos) para asegurar que:
   - Las rutas con buenas decisiones históricas recorran hasta el Acto 5 y desembocan al final en `Nodo 35_Final_Historico`.
   - Las rutas con buenas decisiones ficticias/ucrónicas recorran hasta el Acto 5 y desembocan al final en `Nodo 34_Final_Ficticio`.
   - Las malas decisiones estratégicas desemboquen en `Nodo 33_Final_Desastre_Prematuro`.
2. Al terminar el audio y texto de cualquiera de los 3 nodos finales, muestra en UI Toolkit un panel de cierre con botones funcionales para `Volver al Menú Principal` (`MainMenu`) y `Reiniciar Campaña`.

---

### Paso 6: Sistema de Guardado (`SaveManager.cs`), Continuar, Reiniciar y Opciones
1. Crea `Assets/Scripts/Managers/SaveManager.cs` que guarde y cargue en JSON (`Application.persistentDataPath/savegame.json` con respaldo en `PlayerPrefs`) el nombre del `StoryNode` actual, `playerName` y `actNumber`.
2. Haz que `GameManager` autoguarde cada vez que avance a un nuevo `StoryNode` (y permita guardar manualmente desde un botón `Guardar Partida` en la esquina superior derecha de `VisualNovelUI.uxml` durante la partida, con confirmación visual `"Partida guardada"`).
3. En `Assets/Scenes/MainMenu.unity` y su UI Toolkit:
   - Agrega el botón `Continuar` (habilitado solo si `SaveManager.HasSaveData()` es verdadero; al pulsarlo carga `What_If_Peru` directamente en el `StoryNode`, `playerName`, audio y música del Acto guardado), además de `Iniciar Campaña` (Juego Nuevo) y un botón de pausa/opciones dentro de la partida con opción `Reiniciar`.
   - Implementa el panel `Opciones` (accesible desde `MainMenu` y desde el botón de pausa en `What_If_Peru`) con:
     - Slider de **Volumen de Música de Fondo** (guardado en `PlayerPrefs` y aplicado en tiempo real).
     - Slider de **Volumen de Voces / Audio de Personajes** (guardado en `PlayerPrefs` y aplicado en tiempo real a `voiceSource`).
     - Botón **Borrar Datos Guardados** (elimina `savegame.json`, limpia `PlayerPrefs` de guardado y deshabilita `Continuar`).

---

### Paso 7: Compilación, Validación en Play Mode y Bitácora
1. Compila los scripts, guarda `MainMenu.unity` y `What_If_Peru.unity`, entra a Play Mode con el MCP y comprueba que los 35 nodos tienen asignados sus sprites, fondos y audios de voz, que el bloqueo anti-salto espera a que termine el audio de voz, que el cartel de Acto sale primero, que los personajes desaparecen limpio en transiciones y decisiones, y que no hay errores en consola.
2. Documenta todos los cambios realizados al final de `Assets/Ordenes_OpenCode/Avances_Realizados/Bitacora_Avances.md`.
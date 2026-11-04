Actúa como un Lead UI Programmer en Unity. Para el proyecto "¿Y si...? Perú", utilizaremos estrictamente 'UI Toolkit' (no uGUI/Canvas) para construir la interfaz de la novela visual. 

Por favor, utiliza el MCP para ejecutar las siguientes acciones y generar la arquitectura de UI:

1. Creación de Assets de UI Toolkit:
- Crea una carpeta en 'Assets/UI'.
- Genera un archivo UXML llamado 'VisualNovelUI.uxml'.
- Genera un archivo de estilos USS llamado 'VisualNovelStyles.uss' y vincúlalo al UXML.

2. Estructura del UXML (Visual Tree):
Construye la siguiente jerarquía dentro de 'VisualNovelUI.uxml':
- 'RootScreen' (VisualElement que ocupe el 100% de la pantalla, con la imagen de fondo del escenario histórico).
  - 'CharacterContainer' (VisualElement centrado para el Portrait del personaje).
  - 'ChoicesContainer' (VisualElement centrado en la pantalla, oculto por defecto con `display: none`).
    - 'BtnChoiceA' (Button).
    - 'BtnChoiceB' (Button).
  - 'DialoguePanel' (VisualElement anclado en la parte inferior, ocupando el 30% del alto).
    - 'SpeakerNameText' (Label para el nombre).
    - 'DialogueText' (Label con wrap de texto para la narrativa).
    - 'NextIndicator' (VisualElement pequeño en la esquina inferior derecha para indicar que se puede avanzar con clic).

3. Estilos Base en USS:
- Aplica estilos en 'VisualNovelStyles.uss' para que el 'DialoguePanel' tenga un fondo semitransparente oscuro (estilo novela visual/Doki Doki pero sobrio y militar).
- Los botones de elección ('BtnChoiceA', 'BtnChoiceB') deben ser grandes, estar centrados, tener un margen entre ellos y un color de fondo que resalte.

4. Adaptación del 'UIManager.cs' para UI Toolkit:
Asegúrate de que el script 'UIManager' utilice 'UnityEngine.UIElements'.
- Debe requerir o tener una referencia a un componente 'UIDocument' en la escena.
- En el método 'OnEnable()' o 'Start()', utiliza 'GetComponent<UIDocument>().rootVisualElement' para hacer las queries de los elementos (ej. `root.Q<Label>("DialogueText")`).
- Implementa un método `ShowDialogue(string speaker, string text, Sprite characterSprite, Sprite bgSprite)` que asigne los valores a los Labels y los backgrounds a los VisualElements correspondientes (usando `StyleBackground`).
- Implementa un método `ShowChoices(string choiceA, string choiceB, Action onChoiceA, Action onChoiceB)` que cambie el 'display' del 'ChoicesContainer' a 'Flex', asigne el texto a los botones y registre los callbacks en el evento `clicked`.
- Implementa el input general: si el 'ChoicesContainer' está oculto, un clic en el 'RootScreen' (mediante `RegisterCallback<ClickEvent>`) debe avisar al GameManager para avanzar al siguiente nodo.

5. Setup en la Escena:
Revisa la escena actual. Si no existe, crea un GameObject vacío llamado 'UI_Manager', añádele el componente 'UIDocument', asígnale el 'VisualNovelUI.uxml' recién creado y adjunta el script 'UIManager.cs'.
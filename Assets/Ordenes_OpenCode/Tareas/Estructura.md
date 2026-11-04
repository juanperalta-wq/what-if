Actúa como un Lead Game Developer. Utilizando tus capacidades del MCP de Unity, necesito que estructures y desarrolles la base de nuestro juego "¿Y si...? Perú", una Novela Visual de historia alternativa ("What if...?") enfocada en la Guerra del Pacífico.

El núcleo del juego es un Sistema de Grafos y Árboles de Decisión (Branching Narrative). No usaremos inventarios ni variables complejas, solo decisiones binarias que alteran el flujo de los nodos.

Por favor, ejecuta las siguientes acciones en mi proyecto usando el MCP:

1. Estructura de Carpetas:
Crea (si no existen) las siguientes carpetas en el proyecto:
- Assets/Scripts/Narrative
- Assets/Scripts/Managers
- Assets/ScriptableObjects/Nodes

2. Creación de Scripts (Data Models - Grafos):
Genera los siguientes scripts en 'Assets/Scripts/Narrative':
- 'StoryNode.cs': Un ScriptableObject que contenga:
  * string speakerName (Debe soportar una variable global para el nombre del jugador).
  * [TextArea] string dialogueText.
  * Sprite characterSprite.
  * Sprite backgroundSprite.
  * List<StoryNode> nextNodes (Si tiene 0, es final. Si tiene 1, avanza automático. Si tiene 2, es una decisión binaria).
  * List<string> choiceTexts (Textos para los botones de las opciones A y B).

3. Creación de Managers:
Genera en 'Assets/Scripts/Managers':
- 'GameManager.cs': Que lleve el control del nodo actual (currentNode), inicie la partida y reciba la selección (índice 0 o 1) para cambiar al siguiente StoryNode.
- 'UIManager.cs': Que se encargue de actualizar la UI (Textos, Sprites) y renderizar 1 o 2 botones dinámicamente leyendo las opciones del StoryNode actual. Referencia de estilo: Doki Doki Literature Club (flujo de lectura con decisiones centrales).

4. Generación de Assets de Prueba (Dummy Data):
Si el MCP te lo permite, genera instancias de ScriptableObjects (o un script de Editor que los genere por mí) para poblar un pequeño árbol de prueba con esta estructura histórica de la Guerra del Pacífico:
- Nodo Inicial: Combate de Iquique.
  * Opción A -> Nodo: Perder la Independencia (Histórico).
  * Opción B -> Nodo: Salvar la Independencia con bengalas (Ucrónico).
- Nodos siguientes: Batalla de Tarapacá y Alto de la Alianza.
- 3 Nodos Finales: "El Curso Conocido" (Derrota), "La Paz Armada" (Tratado temprano), "El Desastre Prematuro" (Aniquilación en el desierto).

Actúa como Lead Game Developer y Narrative Designer. He notado que el flujo de la historia actual es demasiado corto y va directo a las decisiones, lo cual le resta inmersión a nuestra Novela Visual.

He creado un nuevo documento con un guion mucho más extenso, que incluye múltiples nodos de diálogo lineal antes de cada Momento Decisivo, para generar tensión y contexto narrativo.

Por favor, lee el archivo 'AssetsOrdenes_OpenCodeContextoHistoricoGuion_Guerra_Pacifico.md'. 

Utilizando el MCP
1. Elimina o sobrescribe los 11 ScriptableObjects de prueba actuales en 'AssetsScriptableObjectsNodes'.
2. Genera los nuevos ScriptableObjects correspondientes a todos los nodos descritos en el nuevo guion. Cada línea del guion (Ej. Escenario 1 - Nodo 1, Escenario 1 - Nodo 2) debe ser un StoryNode individual.
3. Configura correctamente el campo 'nextNodes' de cada uno para que los diálogos fluyan linealmente hasta llegar al Momento Decisivo de cada escenario, donde el nodo se ramificará en Opción A o B.
4. Conecta las decisiones con los 3 Finales Históricos y Ucrónicos correspondientes (Nodos Finales).
5. Asigna el nuevo Escenario 1 - Nodo 1 como el startingNode en el GameManager de la escena.
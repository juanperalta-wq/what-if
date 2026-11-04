# GAME DESIGN DOCUMENT (GDD)

**Game Name:** ¿Y si...? Perú&nbsp;

**Genre:** Novela Visual / Aventura&nbsp;

**Gráfica de Historia Alternativa** **Game Elements:** Lectura de diálogos narrativos, exploración visual de escenarios históricos, toma de decisiones binarias (Opción A o B) y descubrimiento de líneas temporales alternativas.&nbsp;

**Player:** 1 (Single-player)

---

## TECHNICAL SPECS

**Technical Form:** Gráficos 2D (Neo-Pixel Art / Ilustración 2D).&nbsp;

**View:** Primera persona (Cámara fija con estilo de Novela Visual, visualizando a los interlocutores de frente y los escenarios de fondo).&nbsp;

**Platform:** PC, Mac, WebGL (Navegador).&nbsp;

**Language:** C\# (Motor Unity). **Device:** PC.

---

## GAME PLAY

El jugador inicia la partida introduciendo su nombre, asumiendo el rol de un estratega y viajero temporal insertado en las filas peruanas durante la Guerra del Pacífico (1879-1880). El juego se desarrolla mediante interacciones de diálogo con figuras históricas como Miguel Grau o Francisco Bolognesi. Durante la narrativa, el juego se pausa en "Momentos Decisivos" donde el jugador debe elegir entre dos tácticas (Opción A o Opción B). Por ejemplo, en el Combate de Iquique, el jugador decide si dejar que la fragata *Independencia* continúe su persecución o enviar una orden de abortar. Estas decisiones binarias tienen consecuencias inmediatas que alteran el curso de la batalla, ramificando la historia hacia un final históricamente preciso o hacia desenlaces ucrónicos (alternativos).

**Game Play Outline:**

* **Opening the game application:** Pantalla de título, ingreso del `[Nombre_del_Jugador]` y secuencia de activación del "Reloj Temporal".  
* **Game options:** Ajuste de velocidad del texto, volumen de música/SFX y modo de pantalla completa.  
* **Story synopsis:** El jugador viaja a los años críticos de 1879-1880 para presenciar o alterar el conflicto bélico más importante del Perú republicano.  
* **Modes:** Modo Historia (Story Mode).  
* **Game elements:** Sistema de Diálogos (Dialogue Manager), Sistema de Decisiones Binarias (Choice Manager), Sistema de Gestión de Escenas.  
* **Game levels:**  
  1. Combate Naval de Iquique (Mayo 1879\)  
  2. Batalla de Tarapacá (Noviembre 1879\)  
  3. Batalla del Alto de la Alianza (Mayo 1880\)  
* **Player's controls:** Mouse (Point & Click) para avanzar diálogos y seleccionar opciones.  
* **Winning:** Alcanzar el final "La Paz Armada" (Final Ucrónico Positivo) o desbloquear el 100% de los finales en el Códice.  
* **Losing:** Tomar decisiones que lleven al final "El Desastre Prematuro" (Final Ucrónico Negativo), aniquilando al ejército antes de tiempo.  
* **End:** Pantalla de resumen que muestra la "Línea Temporal Creada" basada en las decisiones del jugador.  
* **Why is all this fun?:** Provee un alto nivel de "agencia" (poder) al jugador. Reescribir la historia oficial y ver los efectos mariposa inmediatos de sus decisiones resulta profundamente satisfactorio y fomenta la curiosidad.

**Key Features:**

* Narrativa Ramificada Binaria (Branching Narrative).  
* Múltiples Finales Históricos y Ucrónicos.  
* Estética 2D inmersiva con UI temática del siglo XIX.  
* Sistema de guardado local (JSON) para explorar diferentes ramas.

---

## DESIGN DOCUMENT

**Design Guidelines:**

* **Restricciones creativas:** La historia debe centrarse en la perspectiva peruana. No se castigará al jugador por no saber historia real; las opciones ucrónicas deben ser lógicas y plausibles.  
* **Objetivo general:** "Edutainment" (Educación \+ Entretenimiento). El juego debe enseñar historia a través de las consecuencias lógicas de las estrategias militares, priorizando el dinamismo sobre los "muros de texto" académicos. La regla de oro es mantener exactamente 2 opciones por cada Momento Decisivo.

**Game Design Definitions:**

* **Wins:** El jugador "gana" al estabilizar el frente militar y lograr el tratado de paz temprano ("La Paz Armada").  
* **Loses:** El jugador "pierde" si sus decisiones destruyen la logística del ejército peruano, generando el "Desastre Prematuro".  
* **Transitions:** Al concluir la última caja de diálogo de un nivel o al tomar una decisión que resuelva el conflicto local, la pantalla hace un *Fade to Black* y carga asíncronamente la escena del siguiente evento cronológico o la pantalla de final.  
* **Main Focus:** La agencia narrativa y la exploración de causalidades (causa y efecto).

---

## GAME FLOWCHART

1. **Menu:** (Start Game, Load Game, Options, Exit).  
2. **Synopsis / Setup:** Ingreso de `[Nombre_del_Jugador]` \-\> Animación de salto temporal.  
3. **Game Play (Level 1 \- Iquique):** Diálogo con Grau \-\> **Choice A / B** \-\> Consecuencia de la *Independencia*.  
4. **Game Play (Level 2 \- Tarapacá):** Diálogo con Cáceres \-\> **Choice A / B** \-\> Retirada o Ataque a Pisagua.  
5. **Game Play (Level 3 \- Alto de la Alianza):** Diálogo defensivo \-\> **Choice A / B** \-\> Defensa clásica o Ataque Nocturno.  
6. **Game Over (Winning and Losing):**  
   * Ruta A-A-A \-\> Final "El Curso Conocido" (Retorno al menú).  
   * Ruta B-B-A \-\> Final "El Desastre Prematuro" (Losing \- Retorno al menú).  
   * Ruta B-A-B \-\> Final "La Paz Armada" (Winning \- Retorno al menú).

---

## PLAYER DEFINITION

El jugador encarna a un estratega sin rostro definido (cámara en primera persona) al que los personajes de la historia se refieren por el nombre ingresado por el usuario. Es el único consciente de que la línea temporal puede ser alterada.

**Player Properties:** Al haberse eliminado los sistemas de inventario y variables (Confianza, Reputación, etc.), las propiedades del jugador se definen puramente por el **Estado de la Línea Temporal (Timeline State)**.

* **Propiedad "Ruta Actual":** Un *string* o *int* interno en el código (ej. `currentRoute = "Historical"` o `currentRoute = "Altered"`).  
* ***Feedback:*** El cambio de esta propiedad dispara directamente la carga de diferentes bloques de texto o escenarios visuales inmediatos.

**Player Rewards (power-ups and pick-ups):**

* **Desbloqueo de Finales:** Cada final descubierto otorga una "Medalla de Códice" en el menú principal.  
* **Rutas Secretas:** Tomar una combinación específica de decisiones recompensa al jugador con diálogos inéditos o "easter eggs" históricos.

---

## USER INTERFACE (UI)

La UI está diseñada para ser minimalista, permitiendo que el arte de fondo (combates y campamentos) destaque. El control es 100% mediante el mouse o toques en la pantalla (si se compila para móvil en el futuro).

* **Dialogue Box (Caja de Texto):** Ubicada en el tercio inferior de la pantalla. Muestra el nombre del interlocutor y el texto que se despliega progresivamente (Typewriter effect). Un clic izquierdo avanza el texto.  
* **Character Portraits (Retratos):** Aparecen sobre la caja de texto (Izquierda para aliados, Derecha para el jugador si se llegara a ilustrar).  
* **Decision Interface (Botones de Decisión):** Cuando ocurre un "Momento Decisivo", la caja de texto desaparece temporalmente y aparecen **dos botones grandes en el centro de la pantalla** (Opción A arriba, Opción B abajo). Esto fuerza al jugador a detenerse y leer cuidadosamente antes de hacer clic.  
* **Pause/Menu Button:** Un pequeño ícono de un engranaje en la esquina superior derecha para guardar la partida, cargar o ajustar opciones.
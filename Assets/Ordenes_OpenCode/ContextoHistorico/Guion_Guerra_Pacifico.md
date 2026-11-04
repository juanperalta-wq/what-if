# GUION NARRATIVO HISTÓRICO - LA GUERRA DEL PACÍFICO

> **Instrucciones para OpenCode:** Convierte cada bloque en un `StoryNode` (ScriptableObject). El campo `nextNodes` debe apuntar al ID del siguiente bloque. Cuando haya Opciones A/B, el nodo debe tener 2 salidas. Utiliza el token `{PLAYER_NAME}` para el nombre del jugador.

---
## ESCENARIO 1: EL COMBATE NAVAL DE IQUIQUE (21 de Mayo de 1879)

**[Nodo 01_Iquique_Intro]**
*   **Interlocutor:** Narrador
*   **Texto:** 21 de mayo de 1879. Bahía de Iquique. El estruendo de los cañones ensordece a la tripulación. Tras horas de combate, el monitor peruano Huáscar ha utilizado su espolón de proa para hundir a la corbeta chilena Esmeralda.

**[Nodo 02_Iquique_Grau]**
*   **Interlocutor:** Miguel Grau
*   **Texto:** ¡Alto al fuego! Echen los botes al agua. El capitán Arturo Prat ha caído con honor en nuestra cubierta y su nave se hunde. Recojan a los náufragos, la guerra no nos quita la humanidad. Estratega {PLAYER_NAME}, ¿cuál es el reporte del resto de la escuadra?

**[Nodo 03_Iquique_Reporte]**
*   **Interlocutor:** Oficial de Señales
*   **Texto:** Almirante, nuestra mejor embarcación, la fragata blindada Independencia al mando del capitán Moore, está persiguiendo a la goleta enemiga Covadonga. Se dirigen hacia el sur, pegados a la costa, directo a los peligrosos arrecifes submarinos de Punta Gruesa.

**[Nodo 04_Iquique_Decision]**
*   **Interlocutor:** Miguel Grau
*   **Texto:** La Covadonga es de bajo calado, puede pasar por aguas poco profundas. Nuestra fragata pesa más de 2000 toneladas, si Moore se acerca demasiado a la costa, encallará y perderemos la mitad de nuestro poder naval. {PLAYER_NAME}, confío en su criterio táctico. ¿Qué hacemos?
*   **Decisiones:**
    *   **Opción A (Histórica):** "El Capitán Moore conoce su nave. Debemos confiar en su maniobra y terminar el rescate aquí." (Apunta a -> 05A_Pierde_Independencia)
    *   **Opción B (Ucrónica):** "¡Es una trampa de Condell! Dispare bengalas rojas y ordene a la Independencia abortar la persecución de inmediato." (Apunta a -> 05B_Salva_Independencia)

**[Nodo 05A_Pierde_Independencia]**
*   **Interlocutor:** Narrador
*   **Texto:** Fiel a los registros históricos, la fragata Independencia encalló en las rocas submarinas de Punta Gruesa. El Perú perdió su buque más poderoso en un solo día, otorgando al enemigo la supremacía naval definitiva.

**[Nodo 05B_Salva_Independencia]**
*   **Interlocutor:** Narrador
*   **Texto:** Las bengalas rojas del Huáscar iluminaron el cielo diurno. El capitán Moore, a regañadientes, abortó la embestida a escasos metros de los arrecifes. El Perú conserva su fragata blindada. La guerra en el mar será más equilibrada de lo que dictan los libros.

---
## ESCENARIO 2: LA BATALLA DE TARAPACÁ (27 de Noviembre de 1879)

**[Nodo 06_Tarapaca_Intro]**
*   **Interlocutor:** Narrador
*   **Texto:** 27 de noviembre de 1879. Fondo de la quebrada de Tarapacá. El ejército peruano, exhausto tras la derrota en San Francisco, fue sorprendido. Sin embargo, en un acto de heroísmo, la infantería peruana escaló las laderas y contraatacó ferozmente.

**[Nodo 07_Tarapaca_Caceres]**
*   **Interlocutor:** Andrés Avelino Cáceres
*   **Texto:** ¡Una victoria impensable, {PLAYER_NAME}! Tras nueve horas de fuego cruzado, el batallón Zepita y nuestros valientes han hecho retroceder al enemigo. ¡Incluso les hemos capturado ocho modernos cañones Krupp!

**[Nodo 08_Tarapaca_Bolognesi]**
*   **Interlocutor:** Francisco Bolognesi
*   **Texto:** Ganamos la batalla, coronel Cáceres, pero la guerra es logística. No tenemos mulas para transportar esos pesados cañones. Nuestros hombres no han comido en 48 horas, las municiones escasean y los pozos de agua en este desierto están secos.

**[Nodo 09_Tarapaca_Decision]**
*   **Interlocutor:** Belisario Suárez
*   **Texto:** El General Buendía solicita consejo estratégico. Tenemos la moral en lo más alto, pero la cantimplora vacía. ¿Cuál es el siguiente paso de este ejército, {PLAYER_NAME}?
*   **Decisiones:**
    *   **Opción A (Histórica):** "Enterremos los cañones Krupp y ordenemos la retirada general hacia Arica cruzando el desierto." (Apunta a -> 10A_Retirada_Arica)
    *   **Opción B (Ucrónica):** "Aprovechemos la inercia del triunfo y ataquemos por sorpresa el puerto de Pisagua para robar suministros." (Apunta a -> 10B_Ataque_Pisagua)

**[Nodo 10A_Retirada_Arica]**
*   **Interlocutor:** Narrador
*   **Texto:** Históricamente, el ejército marchó durante 20 días por el inclemente desierto hacia Arica. Se salvó al personal militar, pero la provincia de Tarapacá y sus ricos yacimientos de salitre quedaron en manos enemigas para siempre.

**[Nodo 10B_Ataque_Pisagua]**
*   **Interlocutor:** Narrador
*   **Texto:** La ambición superó a la logística. Tras forzar una marcha hacia Pisagua sin agua ni provisiones, el ejército llegó completamente diezmado. Las tropas de reserva enemigas en el puerto masacraron a los exhaustos sobrevivientes peruanos. (Apunta a -> FINAL_03_Desastre)

---
## ESCENARIO 3: LA BATALLA DEL ALTO DE LA ALIANZA (26 de Mayo de 1880)

**[Nodo 11_Alto_Intro]**
*   **Interlocutor:** Narrador
*   **Texto:** Mayo de 1880. Meseta del Intiorko, Tacna. El Ejército Unido Perú-Boliviano se atrincheró en una extensa línea de defensa. Es la batalla más grande y decisiva de toda la campaña del sur.

**[Nodo 12_Alto_Campero]**
*   **Interlocutor:** Gral. Narciso Campero (Presidente de Bolivia)
*   **Texto:** Aliados, la situación es crítica. Los exploradores indican que el General Baquedano trae más de 14,000 hombres bien equipados. Nos superan en número y su moderna artillería nos destrozará a distancia en cuanto salga el sol.

**[Nodo 13_Alto_Montero]**
*   **Interlocutor:** Lizardo Montero
*   **Texto:** Hay una espesa 'Camanchaca' esta madrugada, una niebla tan densa en el desierto que no se ve a diez pasos. Podemos usarla para acercarnos en completo silencio e iniciar un ataque de bayonetas antes de que emplaces sus cañones.

**[Nodo 14_Alto_Decision]**
*   **Interlocutor:** Francisco Bolognesi
*   **Texto:** La tropa está nerviosa. Un ataque nocturno en el desierto requiere una coordinación que quizás no tenemos. Si nos descubren en la marcha, será una masacre. Estratega {PLAYER_NAME}, ¿qué ordenamos?
*   **Decisiones:**
    *   **Opción A (Histórica):** "Es demasiado riesgoso. Permanezcamos en nuestras trincheras, guardemos munición y resistamos el asalto frontal." 
    *   **Opción B (Ucrónica):** "Avancen al amparo de la camanchaca. ¡Tomaremos su artillería por sorpresa esta misma noche!" 

---
## LOS FINALES

**[Nodo FINAL_01_Historico]**
*   **Interlocutor:** Narrador
*   **Texto:** EL CURSO CONOCIDO. Al salir el sol, la artillería destrozó las líneas aliadas. El frente se rompió tras horas de sangrienta carnicería. Bolivia se retiró de la guerra y la caída de Tacna dejó a Arica aislada. La historia siguió su doloroso e inmutable curso oficial. Has preservado la línea temporal, pero el precio fue altísimo.

**[Nodo FINAL_02_PazArmada]**
*   **Interlocutor:** Narrador
*   **Texto:** LA PAZ ARMADA. ¡Una victoria táctica sin precedentes! El ataque en la camanchaca inutilizó los cañones enemigos. Sumado al hecho de que el Perú aún conserva la fragata Independencia en el mar, el alto mando rival, incapaz de avanzar hacia Lima, se vio obligado a solicitar un armisticio inmediato. La guerra terminó mediante un tenso tratado diplomático.

**[Nodo FINAL_03_Desastre]**
*   **Interlocutor:** Narrador
*   **Texto:** EL DESASTRE PREMATURO. Jugar a ser Dios con la historia exige saber de logística militar. Tu imprudencia condenó al ejército de Tarapacá a morir de sed y agotamiento en el desierto. La batalla de Tacna nunca ocurrió porque no quedó nadie para defenderla. Has destruido el país meses antes de lo que dictaba la historia original.
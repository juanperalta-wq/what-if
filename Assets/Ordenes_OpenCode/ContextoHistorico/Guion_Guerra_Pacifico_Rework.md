# Guion Interactivo: Ecos del Pacífico (1873 - 1883)

**Formato:** Novela Visual Ramificada (35 Nodos / 3 Finales)

**Motor destino:** `OpenCode` (Unity)

## 0. Inventario Exacto de Assets (`Assets/Sprites`)

### Fondos (`Assets/Sprites/Background` - 14 assets)

* `Huascar_en_la_costa_de_Atacama-1`
* `Antofagasta__aduana_telegrafo_y_salitre-2`
* `Combate_naval_en_la_bahia_de_Iquique-3`
* `Batalla_naval_frente_a_Punta_Angamos-4`
* `Ruta_polvorienta_hacia_Tacna-5`
* `Columna_exhausta_en_el_desierto_de_Atacama-6`
* `Quebrada_de_Tarapaca_entre_humo_y_palmeras-7`
* `Desembarco_en_Lurin_1880-8`
* `Morro_de_Arica_puerto_y_baterias-9`
* `Caminos_y_defensas_rumbo_a_Lima-10`
* `Meseta_de_Alto_de_la_Alianza-11`
* `Miraflores_entre_humo_y_silencio-12`
* `Colinas_de_San_Juan_y_Chorrillos-13`
* `Ascenso_al_Morro_de_Arica-14`

### Personajes (`Assets/Sprites/SpritesPersonajes` - 12 assets)

* `Miguel_Grau_comandante_naval-15`
* `Francisco_Bolognesi_general_peruano-16`
* `Nicolas_de_Pierola_lider_peruano-17`
* `Retrato_de_Arturo_Prat_1879-18`
* `Andres_Avelino_Caceres_en_uniforme-19`
* `Retrato_presidencial_de_Anibal_Pinto-20`
* `Jose_Antonio_de_Lavalle_diplomatico_peruano-21`
* `Retrato_militar_de_Hilarion_Daza-22`
* `Retrato_de_Mariano_Ignacio_Prado-23`
* `Belisario_Suárez`
* `Lizardo_Montero_Flores`
* `Narciso_Campero_Leyes`

### Banderas (`Assets/Sprites/Banderas` - 3 assets)

* `BanderaPeru`
* `BanderaChile`
* `BanderaBolivia`

---

## ACTO I: Iquique y las Raíces del Conflicto

**\[Nodo 01_Iquique_Intro\]**

* **Interlocutor:** Narrador
* **Narrador:** 21 de mayo de 1879. Bahía de Iquique. El estruendo de los cañones ensordece a la tripulación. Tras horas de combate, el monitor peruano Huáscar ha utilizado su espolón de proa para hundir a la corbeta chilena Esmeralda.
* **Fondo:** `Combate_naval_en_la_bahia_de_Iquique-3`
* **Personaje:** `NONE`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 02_Puente_Del_Huascar`

**\[Nodo 02_Puente_Del_Huascar\]**

* **Interlocutor:** Miguel Grau
* **Narrador:** El humo de la pólvora aún cubre la bahía mientras los mástiles de la *Esmeralda* desaparecen bajo las olas. El capitán Arturo Prat ha caído heroicamente sobre nuestra cubierta. Hace apenas unos meses, esta guerra parecía impensable; una disputa fiscal por el salitre en Antofagasta terminó arrastrando a tres naciones al abismo.
* **Fondo:** `Combate_naval_en_la_bahia_de_Iquique-3`
* **Personaje:** `Miguel_Grau_comandante_naval-15`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 03_Origen_Antofagasta`

**\[Nodo 03_Origen_Antofagasta\]**

* **Interlocutor:** Hilarión Daza
* **Narrador:** Retrocedemos a febrero de 1879 en el puerto de Antofagasta. Ante la negativa de la compañía salitrera a pagar el impuesto de 10 centavos decretado por Bolivia, el presidente Hilarión Daza ordena embargar sus bienes. El 14 de febrero, tropas chilenas ocupan el puerto. Bolivia exige entonces al Perú activar el Tratado Secreto de Alianza Defensiva de 1873.
* **Fondo:** `Antofagasta__aduana_telegrafo_y_salitre-2`
* **Personaje:** `Retrato_militar_de_Hilarion_Daza-22`
* **Bandera:** `BanderaBolivia`
* **Opciones:**
  * **Opción A:** Suspender el embargo en Antofagasta y aceptar una comisión internacional de arbitraje antes de movilizar tropas. -> `Nodo 05_Ficcion_Mediacion_Aduana`
  * **Opción B:** Mantener la reivindicación sobre el salitre y exigir a Lima cumplir de inmediato el Tratado Secreto de 1873. -> `Nodo 04_Mision_Lavalle_Crisis`
  * **Opción C:** Ordenar la destrucción inmediata de los telégrafos y muelles salitreros sin coordinar previamente con Lima. -> `Nodo 06_Desastre_Ruptura_Total`

**\[Nodo 04_Mision_Lavalle_Crisis\]**

* **Interlocutor:** Aníbal Pinto
* **Narrador:** Marzo de 1879. El diplomático peruano José Antonio de Lavalle intenta mediar entre Bolivia y Chile para evitar la guerra, pero el presidente chileno Aníbal Pinto ya conoce la existencia del Tratado Secreto de 1873 y exige que el Perú declare su neutralidad inmediata bajo amenaza de guerra.
* **Fondo:** `Antofagasta__aduana_telegrafo_y_salitre-2`
* **Personaje:** `Retrato_presidencial_de_Anibal_Pinto-20`
* **Bandera:** `BanderaChile`
* **Opciones:**
  * **Opción A:** Romper abruptamente las negociaciones y declarar la guerra sin haber alistado las calderas de la escuadra en el Callao. -> `Nodo 06_Desastre_Ruptura_Total`
  * **Opción B:** Firmar un protocolo temporal de tregua en Santiago para ganar semanas vitales y modernizar la artillería del *Huáscar*. -> `Nodo 07_Ficcion_Escuadra_Alistada`
  * **Opción C:** Rechazar el ultimátum de neutralidad en defensa de la alianza con Bolivia, desencadenando la declaratoria de guerra del 5 de abril de 1879. -> `Nodo 08_Rescate_En_Iquique`

**\[Nodo 05_Ficcion_Mediacion_Aduana\]**

* **Interlocutor:** José Antonio de Lavalle
* **Narrador:** Aprovechando la suspensión del embargo en Antofagasta, el diplomático José Antonio de Lavalle negocia un estatuto comercial trinacional en Atacama. Aunque la tensión persiste en la frontera, el Perú evita ser sorprendido sin preparación naval.
* **Fondo:** `Antofagasta__aduana_telegrafo_y_salitre-2`
* **Personaje:** `Jose_Antonio_de_Lavalle_diplomatico_peruano-21`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 07_Ficcion_Escuadra_Alistada`

**\[Nodo 06_Desastre_Ruptura_Total\]**

* **Interlocutor:** Mariano Ignacio Prado
* **Narrador:** Sin preparación diplomática ni naval, la escuadra chilena bloquea el Callao e Iquique desde el primer día. El presidente Mariano Ignacio Prado intenta reorganizar el mando de emergencia en medio del caos financiero y logístico.
* **Fondo:** `Huascar_en_la_costa_de_Atacama-1`
* **Personaje:** `Retrato_de_Mariano_Ignacio_Prado-23`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 12_Angamos_Desenlace`

**\[Nodo 07_Ficcion_Escuadra_Alistada\]**

* **Interlocutor:** Mariano Ignacio Prado
* **Narrador:** Gracias al tiempo ganado por la diplomacia, el presidente Mariano Ignacio Prado logra reparar las calderas de la escuadra y entrenar a los artilleros antes de que el *Huáscar* zarpe hacia el sur.
* **Fondo:** `Huascar_en_la_costa_de_Atacama-1`
* **Personaje:** `Retrato_de_Mariano_Ignacio_Prado-23`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 08_Rescate_En_Iquique`

---

## ACTO II: La Campaña Naval (Iquique y Angamos)

**\[Nodo 08_Rescate_En_Iquique\]**

* **Interlocutor:** Arturo Prat
* **Narrador:** Regresamos a la bahía de Iquique el 21 de mayo de 1879. Con el recuerdo del valeroso abordaje de Arturo Prat aún fresco en cubierta, Grau ordena rescatar a los náufragos de la *Esmeralda*, mientras hacia el sur la fragata peruana *Independencia* persigue a la goleta *Covadonga* muy cerca de los arrecifes.
* **Fondo:** `Combate_naval_en_la_bahia_de_Iquique-3`
* **Personaje:** `Retrato_de_Arturo_Prat_1879-18`
* **Bandera:** `BanderaChile`
* **Opciones:**
  * **Opción A:** Completar el rescate humanitario en Iquique y navegar luego hacia el sur, donde la *Independencia* encalla en los bajos de Punta Gruesa. -> `Nodo 09_Correrias_Del_Huascar`
  * **Opción B:** Ignorar los arrecifes y lanzar al propio *Huáscar* a toda máquina sobre las rocas costeras para rematar a la *Covadonga*. -> `Nodo 10_Desastre_Varadura_Iquique`
  * **Opción C:** Enviar señales urgentes de bandera para frenar a la *Independencia* antes de los arrecifes, salvando intacta la fragata blindada. -> `Nodo 11_Ficcion_Dominio_Naval`

**\[Nodo 09_Correrias_Del_Huascar\]**

* **Interlocutor:** Miguel Grau
* **Narrador:** Perdida la *Independencia*, el *Huáscar* sostiene en solitario la campaña naval durante cinco meses frente a las costas de Atacama. Burla bloqueos, corta comunicaciones y captura al transporte chileno *Rímac*, ganándose el respeto mundial como *"El Caballero de los Mares"*.
* **Fondo:** `Huascar_en_la_costa_de_Atacama-1`
* **Personaje:** `Miguel_Grau_comandante_naval-15`
* **Bandera:** `BanderaPeru`
* **Opciones:**
  * **Opción A:** Replegar el *Huáscar* al Callao tras capturar el *Rímac* para limpiar sus fondos y evitar un cerco de la escuadra chilena. -> `Nodo 11_Ficcion_Dominio_Naval`
  * **Opción B:** Continuar las correrías frente a Atacama hasta octubre de 1879, enfrentando el cerco de los blindados chilenos en Punta Angamos. -> `Nodo 12_Angamos_Desenlace`

**\[Nodo 10_Desastre_Varadura_Iquique\]**

* **Interlocutor:** Miguel Grau
* **Narrador:** Al forzar la marcha en aguas poco profundas sin cartas náuticas, tanto la *Independencia* como el *Huáscar* quedan encallados y bajo fuego de la artillería costera enemiga. El Perú pierde toda su escuadra en un solo día.
* **Fondo:** `Combate_naval_en_la_bahia_de_Iquique-3`
* **Personaje:** `Miguel_Grau_comandante_naval-15`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 33_Final_Desastre_Prematuro`

**\[Nodo 11_Ficcion_Dominio_Naval\]**

* **Interlocutor:** Miguel Grau
* **Narrador:** Conservando su capacidad ofensiva en el mar, Miguel Grau intercepta los convoyes chilenos frente a Atacama y asegura el abastecimiento marítimo para el ejército aliado en Tarapacá.
* **Fondo:** `Huascar_en_la_costa_de_Atacama-1`
* **Personaje:** `Miguel_Grau_comandante_naval-15`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 15_Ficcion_Tenaza_Tarapaca`

**\[Nodo 12_Angamos_Desenlace\]**

* **Interlocutor:** Miguel Grau
* **Narrador:** 8 de octubre de 1879, Punta Angamos. Rodeado por los blindados *Cochrane* y *Blanco Encalada*, el almirante Miguel Grau cae heroicamente tras el impacto de un proyectil perforante en su torre de mando. El *Huáscar* es capturado y Chile obtiene el dominio absoluto del mar.
* **Fondo:** `Batalla_naval_frente_a_Punta_Angamos-4`
* **Personaje:** `Miguel_Grau_comandante_naval-15`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 13_Desierto_Y_Camarones`

---

## ACTO III: La Campaña de Tarapacá

**\[Nodo 13_Desierto_Y_Camarones\]**

* **Interlocutor:** Hilarión Daza
* **Narrador:** Noviembre de 1879. Tras el desembarco chileno en Pisagua, las tropas aliadas marchan exhaustas por el desierto de Atacama. Hilarión Daza debe avanzar desde el norte para unirse en Dolores con el ejército peruano donde combaten Belisario Suárez y Andrés Avelino Cáceres.
* **Fondo:** `Columna_exhausta_en_el_desierto_de_Atacama-6`
* **Personaje:** `Retrato_militar_de_Hilarion_Daza-22`
* **Bandera:** `BanderaBolivia`
* **Opciones:**
  * **Opción A:** Cruzar la quebrada de Camarones durante la noche para evitar el calor y caer por la retaguardia chilena en Dolores. -> `Nodo 15_Ficcion_Tenaza_Tarapaca`
  * **Opción B:** Dispersar las columnas en el desierto sin agua ni guías buscando pozos aislados. -> `Nodo 14_Desastre_Sed_Atacama`
  * **Opción C:** Detener la marcha en Camarones y regresar hacia Arica, dejando aisladas a las fuerzas peruanas en Tarapacá. -> `Nodo 16_Victoria_Quebrada_Tarapaca`

**\[Nodo 14_Desastre_Sed_Atacama\]**

* **Interlocutor:** Belisario Suárez
* **Narrador:** Perdidas entre las dunas de Atacama sin agua ni municiones, las divisiones aliadas son rodeadas por la caballería enemiga antes siquiera de poder formar línea de batalla.
* **Fondo:** `Columna_exhausta_en_el_desierto_de_Atacama-6`
* **Personaje:** `Belisario_Suárez`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 33_Final_Desastre_Prematuro`

**\[Nodo 15_Ficcion_Tenaza_Tarapaca\]**

* **Interlocutor:** Belisario Suárez
* **Narrador:** La maniobra coordinada entre peruanos y bolivianos sorprende al ejército expedicionario chileno en Tarapacá. Belisario Suárez y Cáceres consolidan la defensa del departamento salitrero.
* **Fondo:** `Quebrada_de_Tarapaca_entre_humo_y_palmeras-7`
* **Personaje:** `Belisario_Suárez`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 18_Alianza_En_Tacna`

**\[Nodo 16_Victoria_Quebrada_Tarapaca\]**

* **Interlocutor:** Belisario Suárez
* **Narrador:** 27 de noviembre de 1879. Acorralados en el fondo de la quebrada de Tarapacá, los batallones liderados por Belisario Suárez, Francisco Bolognesi y Andrés Avelino Cáceres escalan los cerros a bayoneta y obtienen una brillante victoria, aunque sin víveres deben emprender la retirada hacia Arica.
* **Fondo:** `Quebrada_de_Tarapaca_entre_humo_y_palmeras-7`
* **Personaje:** `Belisario_Suárez`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 17_Crisis_Politica_Lima`

**\[Nodo 17_Crisis_Politica_Lima\]**

* **Interlocutor:** Nicolás de Piérola
* **Narrador:** Diciembre de 1879. La pérdida de Tarapacá desata una crisis política: en Bolivia es destituido Daza y asume el mando Narciso Campero, mientras en Lima Nicolás de Piérola toma el poder como Jefe Supremo y Lizardo Montero reorganiza el ejército del sur en Tacna.
* **Fondo:** `Ruta_polvorienta_hacia_Tacna-5`
* **Personaje:** `Nicolas_de_Pierola_lider_peruano-17`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 18_Alianza_En_Tacna`

---

## ACTO IV: Tacna y el Morro de Arica

**\[Nodo 18_Alianza_En_Tacna\]**

* **Interlocutor:** Narciso Campero
* **Narrador:** Mayo de 1880. En la ruta polvorienta hacia Tacna y la meseta del Intiorko, el presidente boliviano Narciso Campero y el contralmirante peruano Lizardo Montero planifican la defensa ante el avance de 19,000 soldados chilenos.
* **Fondo:** `Ruta_polvorienta_hacia_Tacna-5`
* **Personaje:** `Narciso_Campero_Leyes`
* **Bandera:** `BanderaBolivia`
* **Opciones:**
  * **Opción A:** Presentar batalla frontal en la meseta del Alto de la Alianza confiando en la resistencia de las trincheras. -> `Nodo 19_Batalla_Alto_Alianza`
  * **Opción B:** Coordinar una defensa escalonada junto con la guarnición de Arica de Bolognesi para atrapar la retaguardia enemiga. -> `Nodo 20_Ficcion_Contragolpe_Tacna`
  * **Opción C:** Dividir el ejército aliado enviando la mitad sin artillería hacia la costa abierta. -> `Nodo 21_Desastre_Ruptura_Tacna`

**\[Nodo 19_Batalla_Alto_Alianza\]**

* **Interlocutor:** Lizardo Montero
* **Narrador:** 26 de mayo de 1880. En la meseta del Alto de la Alianza, los batallones peruanos de Montero y Cáceres luchan hombro a hombro con las tropas bolivianas de Campero. Pese al coraje demostrado, la superioridad numérica rompe el frente; Bolivia se retira al altiplano y Arica queda completamente aislada.
* **Fondo:** `Meseta_de_Alto_de_la_Alianza-11`
* **Personaje:** `Lizardo_Montero_Flores`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 22_Sitio_Puerto_Arica`

**\[Nodo 20_Ficcion_Contragolpe_Tacna\]**

* **Interlocutor:** Lizardo Montero
* **Narrador:** En la meseta del Alto de la Alianza, las reservas de Lizardo Montero y Narciso Campero ejecutan un flanqueo sincronizado que detiene la ofensiva chilena y mantiene abierta la comunicación ferroviaria con Arica.
* **Fondo:** `Meseta_de_Alto_de_la_Alianza-11`
* **Personaje:** `Lizardo_Montero_Flores`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 24_Ficcion_Bastion_Arica`

**\[Nodo 21_Desastre_Ruptura_Tacna\]**

* **Interlocutor:** Narciso Campero
* **Narrador:** Al fragmentar sus fuerzas en la meseta de Tacna, el frente aliado colapsa en menos de dos horas, dejando expuesta toda la frontera sur sin capacidad de repliegue ordenado.
* **Fondo:** `Meseta_de_Alto_de_la_Alianza-11`
* **Personaje:** `Narciso_Campero_Leyes`
* **Bandera:** `BanderaBolivia`
* **Siguiente:** `Nodo 33_Final_Desastre_Prematuro`

**\[Nodo 22_Sitio_Puerto_Arica\]**

* **Interlocutor:** Francisco Bolognesi
* **Narrador:** Junio de 1880. Rodeado por mar y tierra en el puerto y baterías de Arica con apenas 1,800 defensores frente a más de 5,300 atacantes, el coronel Francisco Bolognesi recibe el pedido oficial de rendición.
* **Fondo:** `Morro_de_Arica_puerto_y_baterias-9`
* **Personaje:** `Francisco_Bolognesi_general_peruano-16`
* **Bandera:** `BanderaPeru`
* **Opciones:**
  * **Opción A:** Concentrar todas las cargas explosivas subterráneas en los accesos estrechos del Morro para una emboscada defensiva. -> `Nodo 24_Ficcion_Bastion_Arica`
  * **Opción B:** Responder con firmeza inmortal: *"Tengo deberes sagrados que cumplir y los cumpliré hasta quemar el último cartucho"*. -> `Nodo 23_Ascenso_Morro_Arica`

**\[Nodo 23_Ascenso_Morro_Arica\]**

* **Interlocutor:** Francisco Bolognesi
* **Narrador:** 7 de junio de 1880. Al amanecer, las columnas chilenas inician el ascenso al Morro de Arica. En cincuenta y cinco minutos de combate cuerpo a cuerpo en la cima, Francisco Bolognesi cumple su juramento hasta el último aliento. Arica cae en la gloria.
* **Fondo:** `Ascenso_al_Morro_de_Arica-14`
* **Personaje:** `Francisco_Bolognesi_general_peruano-16`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 25_Desembarco_En_Lurin`

**\[Nodo 24_Ficcion_Bastion_Arica\]**

* **Interlocutor:** Francisco Bolognesi
* **Narrador:** Durante el ascenso al Morro de Arica, las detonaciones sincronizadas frenan en seco a las columnas asaltantes. Francisco Bolognesi conserva la plaza y obliga al alto mando chileno a suspender la expedición hacia Lima.
* **Fondo:** `Ascenso_al_Morro_de_Arica-14`
* **Personaje:** `Francisco_Bolognesi_general_peruano-16`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 34_Final_Ficticio`

---

## ACTO V: Defensa de Lima, la Resistencia y los Tres Finales

**\[Nodo 25_Desembarco_En_Lurin\]**

* **Interlocutor:** Nicolás de Piérola
* **Narrador:** Diciembre de 1880. Conquistado el sur, el ejército chileno desembarca en Lurín, al sur de la capital. Nicolás de Piérola asume personalmente la dirección de la defensa de Lima.
* **Fondo:** `Desembarco_en_Lurin_1880-8`
* **Personaje:** `Nicolas_de_Pierola_lider_peruano-17`
* **Bandera:** `BanderaPeru`
* **Opciones:**
  * **Opción A:** Dejar sin guarnición los caminos costeros y concentrar toda la población en el centro de Lima sin trincheras exteriores. -> `Nodo 27_Desastre_Caida_Inmediata_Lima`
  * **Opción B:** Construir dos largas líneas de defensa estática en San Juan y Miraflores esperando el ataque frontal enemigo. -> `Nodo 26_Defensas_Rumbo_A_Lima`
  * **Opción C:** Aceptar el plan de Andrés Avelino Cáceres de lanzar un ataque nocturno sorpresa sobre Lurín antes de que desembarque la artillería pesada. -> `Nodo 28_Ficcion_Sorpresa_Lurin`

**\[Nodo 26_Defensas_Rumbo_A_Lima\]**

* **Interlocutor:** Andrés Avelino Cáceres
* **Narrador:** Enero de 1881. Miles de ciudadanos limeños y soldados de línea cavan trincheras en los caminos y defensas rumbo a Lima, preparándose para contener el avance enemigo.
* **Fondo:** `Caminos_y_defensas_rumbo_a_Lima-10`
* **Personaje:** `Andres_Avelino_Caceres_en_uniforme-19`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 29_San_Juan_Y_Chorrillos`

**\[Nodo 27_Desastre_Caida_Inmediata_Lima\]**

* **Interlocutor:** Nicolás de Piérola
* **Narrador:** Sin líneas exteriores de contención en Lurín ni en los caminos del sur, la capital queda rodeada en cuarenta y ocho horas y se rinde sin capacidad de organizar resistencia en los Andes.
* **Fondo:** `Caminos_y_defensas_rumbo_a_Lima-10`
* **Personaje:** `Nicolas_de_Pierola_lider_peruano-17`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 33_Final_Desastre_Prematuro`

**\[Nodo 28_Ficcion_Sorpresa_Lurin\]**

* **Interlocutor:** Andrés Avelino Cáceres
* **Narrador:** Aprovechando la niebla costera, las divisiones de Andrés Avelino Cáceres caen sobre la cabeza de playa en Lurín y capturan los parques de municiones enemigos, desbaratando el asedio sobre Lima.
* **Fondo:** `Desembarco_en_Lurin_1880-8`
* **Personaje:** `Andres_Avelino_Caceres_en_uniforme-19`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 34_Final_Ficticio`

**\[Nodo 29_San_Juan_Y_Chorrillos\]**

* **Interlocutor:** Andrés Avelino Cáceres
* **Narrador:** 13 de enero de 1881. En las colinas de San Juan y Chorrillos se libra una sangrienta jornada. Las tropas de Cáceres resisten encarnizadamente en el Morro Solar, pero la superioridad numérica chilena rompe la primera línea defensiva.
* **Fondo:** `Colinas_de_San_Juan_y_Chorrillos-13`
* **Personaje:** `Andres_Avelino_Caceres_en_uniforme-19`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 30_Batalla_De_Miraflores`

**\[Nodo 30_Batalla_De_Miraflores\]**

* **Interlocutor:** Andrés Avelino Cáceres
* **Narrador:** 15 de enero de 1881. En los reductos de Miraflores, en medio del humo y el silencio previo al choque final, Cáceres lidera un contraataque que hace retroceder al ala izquierda chilena, pero carece de refuerzos oportunos.
* **Fondo:** `Miraflores_entre_humo_y_silencio-12`
* **Personaje:** `Andres_Avelino_Caceres_en_uniforme-19`
* **Bandera:** `BanderaPeru`
* **Opciones:**
  * **Opción A:** Tras resultar gravemente herido y caer Lima, internarse en los Andes para iniciar la heroica Campaña de la Breña. -> `Nodo 31_Resistencia_De_La_Brena`
  * **Opción B:** Recibir a tiempo las reservas intactas de Piérola en Miraflores para sellar la brecha y rechazar el asalto final sobre la capital. -> `Nodo 32_Ficcion_Victoria_Miraflores`

**\[Nodo 31_Resistencia_De_La_Brena\]**

* **Interlocutor:** Andrés Avelino Cáceres
* **Narrador:** 1881 - 1883. Conocido como *"El Brujo de los Andes"*, el general Andrés Avelino Cáceres mantiene en jaque al ejército de ocupación en la sierra central con victorias en Pucará, Marcavalle y Concepción, hasta la agotadora batalla de Huamachuco en julio de 1883.
* **Fondo:** `Miraflores_entre_humo_y_silencio-12`
* **Personaje:** `Andres_Avelino_Caceres_en_uniforme-19`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 35_Final_Historico`

**\[Nodo 32_Ficcion_Victoria_Miraflores\]**

* **Interlocutor:** Andrés Avelino Cáceres
* **Narrador:** El refuerzo oportuno en Miraflores detiene la ofensiva enemiga a las puertas de Lima. Agotado por las bajas y sin lograr tomar la capital, Chile acepta negociar un armisticio equilibrado.
* **Fondo:** `Miraflores_entre_humo_y_silencio-12`
* **Personaje:** `Andres_Avelino_Caceres_en_uniforme-19`
* **Bandera:** `BanderaPeru`
* **Siguiente:** `Nodo 34_Final_Ficticio`

---

## LOS TRES FINALES

**\[Nodo 33_Final_Desastre_Prematuro\]**

* **Interlocutor:** Narrador
* **Narrador:** **FINAL 1 — DESASTRE PREMATURO:** Las decisiones precipitadas y la falta de coordinación estratégica provocaron un colapso temprano antes de consolidar la defensa nacional. Sin escuadra ni líneas de resistencia organizadas, el conflicto termina de forma catastrófica con la pérdida inmediata de toda capacidad de negociación. *(Fin de la partida)*.
* **Fondo:** `Miraflores_entre_humo_y_silencio-12`
* **Personaje:** `NONE`
* **Bandera:** `BanderaChile`
* **Opciones:**
  * **Opción A:** Reiniciar la historia desde la Bahía de Iquique (21 de mayo de 1879). -> `Nodo 01_Iquique_Intro`

**\[Nodo 34_Final_Ficticio\]**

* **Interlocutor:** Narrador
* **Narrador:** **FINAL 2 — FICTICIO (EL ARMISTICIO DEL PACÍFICO):** Gracias a la previsión diplomática, la preservación del poder naval y una defensa coordinada entre aliados, el Perú y Bolivia logran frenar la invasión. El armisticio firmado establece un acuerdo comercial compartido sobre el salitre y preserva la integridad territorial de Tarapacá, Tacna y Arica. *(Fin de la partida)*.
* **Fondo:** `Huascar_en_la_costa_de_Atacama-1`
* **Personaje:** `NONE`
* **Bandera:** `BanderaPeru`
* **Opciones:**
  * **Opción A:** Reiniciar la historia desde la Bahía de Iquique (21 de mayo de 1879). -> `Nodo 01_Iquique_Intro`

**\[Nodo 35_Final_Historico\]**

* **Interlocutor:** Narrador
* **Narrador:** **FINAL 3 — HISTÓRICO (MEMORIA Y RECONSTRUCCIÓN):** Tras el Tratado de Ancón en octubre de 1883, el Perú inicia una larga reconstrucción nacional, logrando décadas después la reincorporación de Tacna en 1929. El legado inmortal de Miguel Grau en Iquique, Francisco Bolognesi en Arica y Andrés Avelino Cáceres en la resistencia queda grabado para siempre en la historia. *(Fin de la partida)*.
* **Fondo:** `Ascenso_al_Morro_de_Arica-14`
* **Personaje:** `NONE`
* **Bandera:** `BanderaPeru`
* **Opciones:**
  * **Opción A:** Reiniciar la historia desde la Bahía de Iquique (21 de mayo de 1879). -> `Nodo 01_Iquique_Intro`
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Constructor del guion "Ecos del Pacífico (1873 - 1883)".
///
/// Genera los 35 ScriptableObjects de nodo desde el guion
/// Guion_Guerra_Pacifico_Rework.md, los enlaza en orden A/B/C, asigna
/// fondos, retratos, banderas y reapunta la escena What_If_Peru al nodo 01.
///
/// Uso: Tools → ¿Y si...? Perú → Construir guion Rework
/// Es idempotente: borra los nodos anteriores de la carpeta y los recrea.
/// </summary>
public static class ReworkScriptBuilder
{
    private const string NodesFolder = "Assets/ScriptableObjects/Nodes";
    private const string BackgroundsFolder = "Assets/Sprites/Background";
    private const string CharactersFolder = "Assets/Sprites/SpritesPersonajes";
    private const string FlagsFolder = "Assets/Sprites/Banderas";
    private const string ScenePath = "Assets/Scenes/What_If_Peru.unity";

    private class NodeData
    {
        public string id;
        public string speaker;
        public string text;
        public string background;
        public string character;
        public CharacterSide side;
        public string flag;
        public string[] next = new string[0];
        public string[] choices = new string[0];
    }

    [MenuItem("Tools/¿Y si...? Perú/Construir guion Rework")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(NodesFolder))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Nodes");

        DeleteExistingNodes();

        List<NodeData> data = BuildData();
        var created = new Dictionary<string, StoryNode>();

        foreach (NodeData item in data)
        {
            var node = ScriptableObject.CreateInstance<StoryNode>();
            node.name = item.id;
            node.speakerName = item.speaker;
            node.dialogueText = item.text;
            node.backgroundSprite = LoadSprite(BackgroundsFolder, item.background);
            node.characterSprite = string.IsNullOrEmpty(item.character)
                ? null
                : LoadSprite(CharactersFolder, item.character);
            node.characterSide = item.side;
            node.flagSprites = new List<Sprite>();
            Sprite flag = LoadSprite(FlagsFolder, item.flag);
            if (flag != null)
                node.flagSprites.Add(flag);

            node.nextNodes = new List<StoryNode>();
            node.choiceTexts = new List<string>(item.choices);

            string path = $"{NodesFolder}/{item.id}.asset";
            AssetDatabase.CreateAsset(node, path);
            created[item.id] = node;
        }

        // Segundo pase: enlazar el grafo con los assets ya creados.
        int links = 0;
        foreach (NodeData item in data)
        {
            StoryNode node = created[item.id];
            node.nextNodes = new List<StoryNode>();

            foreach (string targetId in item.next)
            {
                if (!created.TryGetValue(targetId, out StoryNode target))
                {
                    Debug.LogError($"[ReworkScriptBuilder] '{item.id}' apunta a un nodo inexistente: '{targetId}'.");
                    continue;
                }

                node.nextNodes.Add(target);
                links++;
            }

            EditorUtility.SetDirty(node);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AssignStartingNode(created["01_Iquique_Intro"]);

        Debug.Log($"[ReworkScriptBuilder] Guion Rework construido: {data.Count} nodos, {links} enlaces. " +
                  "Escena What_If_Peru apuntando a 01_Iquique_Intro.");

        StoryGraphValidator.ValidateGraphMenu();
    }

    private static void DeleteExistingNodes()
    {
        string[] guids = AssetDatabase.FindAssets("t:StoryNode", new[] { NodesFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
                AssetDatabase.DeleteAsset(path);
        }
    }

    private static void AssignStartingNode(StoryNode startingNode)
    {
        Scene scene = EditorSceneManager.GetActiveScene();

        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("[ReworkScriptBuilder] No se encontró GameManager en What_If_Peru.");
            return;
        }

        var serialized = new SerializedObject(gameManager);
        SerializedProperty property = serialized.FindProperty("startingNode");

        if (property == null)
        {
            Debug.LogError("[ReworkScriptBuilder] GameManager no tiene el campo 'startingNode'.");
            return;
        }

        property.objectReferenceValue = startingNode;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);
        EditorSceneManager.SaveScene(gameManager.gameObject.scene);
    }

    private static Sprite LoadSprite(string folder, string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return null;

        string path = $"{folder}/{assetName}.png";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite == null)
            Debug.LogWarning($"[ReworkScriptBuilder] No se encontró el sprite: {path}");

        return sprite;
    }

    private static NodeData N(
        string id,
        string speaker,
        string text,
        string background,
        string character,
        CharacterSide side,
        string flag,
        string[] next,
        string[] choices = null)
    {
        return new NodeData
        {
            id = id,
            speaker = speaker,
            text = text,
            background = background,
            character = character,
            side = side,
            flag = flag,
            next = next ?? new string[0],
            choices = choices ?? new string[0]
        };
    }

    private static List<NodeData> BuildData()
    {
        const CharacterSide IZQ = CharacterSide.Izquierda;
        const CharacterSide DER = CharacterSide.Derecha;

        return new List<NodeData>
        {
            N("01_Iquique_Intro", "Narrador",
                "21 de mayo de 1879. Bahía de Iquique. El estruendo de los cañones ensordece a la tripulación. Tras horas de combate, el monitor peruano Huáscar ha utilizado su espolón de proa para hundir a la corbeta chilena Esmeralda.",
                "Combate_naval_en_la_bahia_de_Iquique-3", null, IZQ, "BanderaPeru",
                new[] { "02_Puente_Del_Huascar" }),

            N("02_Puente_Del_Huascar", "Miguel Grau",
                "El humo de la pólvora aún cubre la bahía mientras los mástiles de la Esmeralda desaparecen bajo las olas. El capitán Arturo Prat ha caído heroicamente sobre nuestra cubierta. Hace apenas unos meses, esta guerra parecía impensable; una disputa fiscal por el salitre en Antofagasta terminó arrastrando a tres naciones al abismo.",
                "Combate_naval_en_la_bahia_de_Iquique-3", "Miguel_Grau_comandante_naval-15", IZQ, "BanderaPeru",
                new[] { "03_Origen_Antofagasta" }),

            N("03_Origen_Antofagasta", "Hilarión Daza",
                "Retrocedemos a febrero de 1879 en el puerto de Antofagasta. Ante la negativa de la compañía salitrera a pagar el impuesto de 10 centavos decretado por Bolivia, el presidente Hilarión Daza ordena embargar sus bienes. El 14 de febrero, tropas chilenas ocupan el puerto. Bolivia exige entonces al Perú activar el Tratado Secreto de Alianza Defensiva de 1873.",
                "Antofagasta__aduana_telegrafo_y_salitre-2", "Retrato_militar_de_Hilarion_Daza-22", DER, "BanderaBolivia",
                new[] { "05_Ficcion_Mediacion_Aduana", "04_Mision_Lavalle_Crisis", "06_Desastre_Ruptura_Total" },
                new[]
                {
                    "Suspender el embargo en Antofagasta y aceptar una comisión internacional de arbitraje antes de movilizar tropas.",
                    "Mantener la reivindicación sobre el salitre y exigir a Lima cumplir de inmediato el Tratado Secreto de 1873.",
                    "Ordenar la destrucción inmediata de los telégrafos y muelles salitreros sin coordinar previamente con Lima."
                }),

            N("04_Mision_Lavalle_Crisis", "Aníbal Pinto",
                "Marzo de 1879. El diplomático peruano José Antonio de Lavalle intenta mediar entre Bolivia y Chile para evitar la guerra, pero el presidente chileno Aníbal Pinto ya conoce la existencia del Tratado Secreto de 1873 y exige que el Perú declare su neutralidad inmediata bajo amenaza de guerra.",
                "Antofagasta__aduana_telegrafo_y_salitre-2", "Retrato_presidencial_de_Anibal_Pinto-20", IZQ, "BanderaChile",
                new[] { "06_Desastre_Ruptura_Total", "07_Ficcion_Escuadra_Alistada", "08_Rescate_En_Iquique" },
                new[]
                {
                    "Romper abruptamente las negociaciones y declarar la guerra sin haber alistado las calderas de la escuadra en el Callao.",
                    "Firmar un protocolo temporal de tregua en Santiago para ganar semanas vitales y modernizar la artillería del Huáscar.",
                    "Rechazar el ultimátum de neutralidad en defensa de la alianza con Bolivia, desencadenando la declaratoria de guerra del 5 de abril de 1879."
                }),

            N("05_Ficcion_Mediacion_Aduana", "José Antonio de Lavalle",
                "Aprovechando la suspensión del embargo en Antofagasta, el diplomático José Antonio de Lavalle negocia un estatuto comercial trinacional en Atacama. Aunque la tensión persiste en la frontera, el Perú evita ser sorprendido sin preparación naval.",
                "Antofagasta__aduana_telegrafo_y_salitre-2", "Jose_Antonio_de_Lavalle_diplomatico_peruano-21", DER, "BanderaPeru",
                new[] { "07_Ficcion_Escuadra_Alistada" }),

            N("06_Desastre_Ruptura_Total", "Mariano Ignacio Prado",
                "Sin preparación diplomática ni naval, la escuadra chilena bloquea el Callao e Iquique desde el primer día. El presidente Mariano Ignacio Prado intenta reorganizar el mando de emergencia en medio del caos financiero y logístico.",
                "Huascar_en_la_costa_de_Atacama-1", "Retrato_de_Mariano_Ignacio_Prado-23", IZQ, "BanderaPeru",
                new[] { "12_Angamos_Desenlace" }),

            N("07_Ficcion_Escuadra_Alistada", "Mariano Ignacio Prado",
                "Gracias al tiempo ganado por la diplomacia, el presidente Mariano Ignacio Prado logra reparar las calderas de la escuadra y entrenar a los artilleros antes de que el Huáscar zarpe hacia el sur.",
                "Huascar_en_la_costa_de_Atacama-1", "Retrato_de_Mariano_Ignacio_Prado-23", IZQ, "BanderaPeru",
                new[] { "08_Rescate_En_Iquique" }),

            N("08_Rescate_En_Iquique", "Arturo Prat",
                "Regresamos a la bahía de Iquique el 21 de mayo de 1879. Con el recuerdo del valeroso abordaje de Arturo Prat aún fresco en cubierta, Grau ordena rescatar a los náufragos de la Esmeralda, mientras hacia el sur la fragata peruana Independencia persigue a la goleta Covadonga muy cerca de los arrecifes.",
                "Combate_naval_en_la_bahia_de_Iquique-3", "Retrato_de_Arturo_Prat_1879-18", DER, "BanderaChile",
                new[] { "09_Correrias_Del_Huascar", "10_Desastre_Varadura_Iquique", "11_Ficcion_Dominio_Naval" },
                new[]
                {
                    "Completar el rescate humanitario en Iquique y navegar luego hacia el sur, donde la Independencia encalla en los bajos de Punta Gruesa.",
                    "Ignorar los arrecifes y lanzar al propio Huáscar a toda máquina sobre las rocas costeras para rematar a la Covadonga.",
                    "Enviar señales urgentes de bandera para frenar a la Independencia antes de los arrecifes, salvando intacta la fragata blindada."
                }),

            N("09_Correrias_Del_Huascar", "Miguel Grau",
                "Perdida la Independencia, el Huáscar sostiene en solitario la campaña naval durante cinco meses frente a las costas de Atacama. Burla bloqueos, corta comunicaciones y captura al transporte chileno Rímac, ganándose el respeto mundial como “El Caballero de los Mares”.",
                "Huascar_en_la_costa_de_Atacama-1", "Miguel_Grau_comandante_naval-15", IZQ, "BanderaPeru",
                new[] { "11_Ficcion_Dominio_Naval", "12_Angamos_Desenlace" },
                new[]
                {
                    "Replegar el Huáscar al Callao tras capturar el Rímac para limpiar sus fondos y evitar un cerco de la escuadra chilena.",
                    "Continuar las correrías frente a Atacama hasta octubre de 1879, enfrentando el cerco de los blindados chilenos en Punta Angamos."
                }),

            N("10_Desastre_Varadura_Iquique", "Miguel Grau",
                "Al forzar la marcha en aguas poco profundas sin cartas náuticas, tanto la Independencia como el Huáscar quedan encallados y bajo fuego de la artillería costera enemiga. El Perú pierde toda su escuadra en un solo día.",
                "Combate_naval_en_la_bahia_de_Iquique-3", "Miguel_Grau_comandante_naval-15", IZQ, "BanderaPeru",
                new[] { "33_Final_Desastre_Prematuro" }),

            N("11_Ficcion_Dominio_Naval", "Miguel Grau",
                "Conservando su capacidad ofensiva en el mar, Miguel Grau intercepta los convoyes chilenos frente a Atacama y asegura el abastecimiento marítimo para el ejército aliado en Tarapacá.",
                "Huascar_en_la_costa_de_Atacama-1", "Miguel_Grau_comandante_naval-15", IZQ, "BanderaPeru",
                new[] { "15_Ficcion_Tenaza_Tarapaca" }),

            N("12_Angamos_Desenlace", "Miguel Grau",
                "8 de octubre de 1879, Punta Angamos. Rodeado por los blindados Cochrane y Blanco Encalada, el almirante Miguel Grau cae heroicamente tras el impacto de un proyectil perforante en su torre de mando. El Huáscar es capturado y Chile obtiene el dominio absoluto del mar.",
                "Batalla_naval_frente_a_Punta_Angamos-4", "Miguel_Grau_comandante_naval-15", IZQ, "BanderaPeru",
                new[] { "13_Desierto_Y_Camarones" }),

            N("13_Desierto_Y_Camarones", "Hilarión Daza",
                "Noviembre de 1879. Tras el desembarco chileno en Pisagua, las tropas aliadas marchan exhaustas por el desierto de Atacama. Hilarión Daza debe avanzar desde el norte para unirse en Dolores con el ejército peruano donde combaten Belisario Suárez y Andrés Avelino Cáceres.",
                "Columna_exhausta_en_el_desierto_de_Atacama-6", "Retrato_militar_de_Hilarion_Daza-22", DER, "BanderaBolivia",
                new[] { "15_Ficcion_Tenaza_Tarapaca", "14_Desastre_Sed_Atacama", "16_Victoria_Quebrada_Tarapaca" },
                new[]
                {
                    "Cruzar la quebrada de Camarones durante la noche para evitar el calor y caer por la retaguardia chilena en Dolores.",
                    "Dispersar las columnas en el desierto sin agua ni guías buscando pozos aislados.",
                    "Detener la marcha en Camarones y regresar hacia Arica, dejando aisladas a las fuerzas peruanas en Tarapacá."
                }),

            N("14_Desastre_Sed_Atacama", "Belisario Suárez",
                "Perdidas entre las dunas de Atacama sin agua ni municiones, las divisiones aliadas son rodeadas por la caballería enemiga antes siquiera de poder formar línea de batalla.",
                "Columna_exhausta_en_el_desierto_de_Atacama-6", "Belisario_Suárez", DER, "BanderaPeru",
                new[] { "33_Final_Desastre_Prematuro" }),

            N("15_Ficcion_Tenaza_Tarapaca", "Belisario Suárez",
                "La maniobra coordinada entre peruanos y bolivianos sorprende al ejército expedicionario chileno en Tarapacá. Belisario Suárez y Cáceres consolidan la defensa del departamento salitrero.",
                "Quebrada_de_Tarapaca_entre_humo_y_palmeras-7", "Belisario_Suárez", DER, "BanderaPeru",
                new[] { "18_Alianza_En_Tacna" }),

            N("16_Victoria_Quebrada_Tarapaca", "Belisario Suárez",
                "27 de noviembre de 1879. Acorralados en el fondo de la quebrada de Tarapacá, los batallones liderados por Belisario Suárez, Francisco Bolognesi y Andrés Avelino Cáceres escalan los cerros a bayoneta y obtienen una brillante victoria, aunque sin víveres deben emprender la retirada hacia Arica.",
                "Quebrada_de_Tarapaca_entre_humo_y_palmeras-7", "Belisario_Suárez", DER, "BanderaPeru",
                new[] { "17_Crisis_Politica_Lima" }),

            N("17_Crisis_Politica_Lima", "Nicolás de Piérola",
                "Diciembre de 1879. La pérdida de Tarapacá desata una crisis política: en Bolivia es destituido Daza y asume el mando Narciso Campero, mientras en Lima Nicolás de Piérola toma el poder como Jefe Supremo y Lizardo Montero reorganiza el ejército del sur en Tacna.",
                "Ruta_polvorienta_hacia_Tacna-5", "Nicolas_de_Pierola_lider_peruano-17", IZQ, "BanderaPeru",
                new[] { "18_Alianza_En_Tacna" }),

            N("18_Alianza_En_Tacna", "Narciso Campero",
                "Mayo de 1880. En la ruta polvorienta hacia Tacna y la meseta del Intiorko, el presidente boliviano Narciso Campero y el contralmirante peruano Lizardo Montero planifican la defensa ante el avance de 19,000 soldados chilenos.",
                "Ruta_polvorienta_hacia_Tacna-5", "Narciso_Campero_Leyes", DER, "BanderaBolivia",
                new[] { "19_Batalla_Alto_Alianza", "20_Ficcion_Contragolpe_Tacna", "21_Desastre_Ruptura_Tacna" },
                new[]
                {
                    "Presentar batalla frontal en la meseta del Alto de la Alianza confiando en la resistencia de las trincheras.",
                    "Coordinar una defensa escalonada junto con la guarnición de Arica de Bolognesi para atrapar la retaguardia enemiga.",
                    "Dividir el ejército aliado enviando la mitad sin artillería hacia la costa abierta."
                }),

            N("19_Batalla_Alto_Alianza", "Lizardo Montero",
                "26 de mayo de 1880. En la meseta del Alto de la Alianza, los batallones peruanos de Montero y Cáceres luchan hombro a hombro con las tropas bolivianas de Campero. Pese al coraje demostrado, la superioridad numérica rompe el frente; Bolivia se retira al altiplano y Arica queda completamente aislada.",
                "Meseta_de_Alto_de_la_Alianza-11", "Lizardo_Montero_Flores", IZQ, "BanderaPeru",
                new[] { "22_Sitio_Puerto_Arica" }),

            N("20_Ficcion_Contragolpe_Tacna", "Lizardo Montero",
                "En la meseta del Alto de la Alianza, las reservas de Lizardo Montero y Narciso Campero ejecutan un flanqueo sincronizado que detiene la ofensiva chilena y mantiene abierta la comunicación ferroviaria con Arica.",
                "Meseta_de_Alto_de_la_Alianza-11", "Lizardo_Montero_Flores", IZQ, "BanderaPeru",
                new[] { "24_Ficcion_Bastion_Arica" }),

            N("21_Desastre_Ruptura_Tacna", "Narciso Campero",
                "Al fragmentar sus fuerzas en la meseta de Tacna, el frente aliado colapsa en menos de dos horas, dejando expuesta toda la frontera sur sin capacidad de repliegue ordenado.",
                "Meseta_de_Alto_de_la_Alianza-11", "Narciso_Campero_Leyes", DER, "BanderaBolivia",
                new[] { "33_Final_Desastre_Prematuro" }),

            N("22_Sitio_Puerto_Arica", "Francisco Bolognesi",
                "Junio de 1880. Rodeado por mar y tierra en el puerto y baterías de Arica con apenas 1,800 defensores frente a más de 5,300 atacantes, el coronel Francisco Bolognesi recibe el pedido oficial de rendición.",
                "Morro_de_Arica_puerto_y_baterias-9", "Francisco_Bolognesi_general_peruano-16", DER, "BanderaPeru",
                new[] { "24_Ficcion_Bastion_Arica", "23_Ascenso_Morro_Arica" },
                new[]
                {
                    "Concentrar todas las cargas explosivas subterráneas en los accesos estrechos del Morro para una emboscada defensiva.",
                    "Responder con firmeza inmortal: “Tengo deberes sagrados que cumplir y los cumpliré hasta quemar el último cartucho”."
                }),

            N("23_Ascenso_Morro_Arica", "Francisco Bolognesi",
                "7 de junio de 1880. Al amanecer, las columnas chilenas inician el ascenso al Morro de Arica. En cincuenta y cinco minutos de combate cuerpo a cuerpo en la cima, Francisco Bolognesi cumple su juramento hasta el último aliento. Arica cae en la gloria.",
                "Ascenso_al_Morro_de_Arica-14", "Francisco_Bolognesi_general_peruano-16", DER, "BanderaPeru",
                new[] { "25_Desembarco_En_Lurin" }),

            N("24_Ficcion_Bastion_Arica", "Francisco Bolognesi",
                "Durante el ascenso al Morro de Arica, las detonaciones sincronizadas frenan en seco a las columnas asaltantes. Francisco Bolognesi conserva la plaza y obliga al alto mando chileno a suspender la expedición hacia Lima.",
                "Ascenso_al_Morro_de_Arica-14", "Francisco_Bolognesi_general_peruano-16", DER, "BanderaPeru",
                new[] { "34_Final_Ficticio" }),

            N("25_Desembarco_En_Lurin", "Nicolás de Piérola",
                "Diciembre de 1880. Conquistado el sur, el ejército chileno desembarca en Lurín, al sur de la capital. Nicolás de Piérola asume personalmente la dirección de la defensa de Lima.",
                "Desembarco_en_Lurin_1880-8", "Nicolas_de_Pierola_lider_peruano-17", IZQ, "BanderaPeru",
                new[] { "27_Desastre_Caida_Inmediata_Lima", "26_Defensas_Rumbo_A_Lima", "28_Ficcion_Sorpresa_Lurin" },
                new[]
                {
                    "Dejar sin guarnición los caminos costeros y concentrar toda la población en el centro de Lima sin trincheras exteriores.",
                    "Construir dos largas líneas de defensa estática en San Juan y Miraflores esperando el ataque frontal enemigo.",
                    "Aceptar el plan de Andrés Avelino Cáceres de lanzar un ataque nocturno sorpresa sobre Lurín antes de que desembarque la artillería pesada."
                }),

            N("26_Defensas_Rumbo_A_Lima", "Andrés Avelino Cáceres",
                "Enero de 1881. Miles de ciudadanos limeños y soldados de línea cavan trincheras en los caminos y defensas rumbo a Lima, preparándose para contener el avance enemigo.",
                "Caminos_y_defensas_rumbo_a_Lima-10", "Andres_Avelino_Caceres_en_uniforme-19", IZQ, "BanderaPeru",
                new[] { "29_San_Juan_Y_Chorrillos" }),

            N("27_Desastre_Caida_Inmediata_Lima", "Nicolás de Piérola",
                "Sin líneas exteriores de contención en Lurín ni en los caminos del sur, la capital queda rodeada en cuarenta y ocho horas y se rinde sin capacidad de organizar resistencia en los Andes.",
                "Caminos_y_defensas_rumbo_a_Lima-10", "Nicolas_de_Pierola_lider_peruano-17", IZQ, "BanderaPeru",
                new[] { "33_Final_Desastre_Prematuro" }),

            N("28_Ficcion_Sorpresa_Lurin", "Andrés Avelino Cáceres",
                "Aprovechando la niebla costera, las divisiones de Andrés Avelino Cáceres caen sobre la cabeza de playa en Lurín y capturan los parques de municiones enemigos, desbaratando el asedio sobre Lima.",
                "Desembarco_en_Lurin_1880-8", "Andres_Avelino_Caceres_en_uniforme-19", IZQ, "BanderaPeru",
                new[] { "34_Final_Ficticio" }),

            N("29_San_Juan_Y_Chorrillos", "Andrés Avelino Cáceres",
                "13 de enero de 1881. En las colinas de San Juan y Chorrillos se libra una sangrienta jornada. Las tropas de Cáceres resisten encarnizadamente en el Morro Solar, pero la superioridad numérica chilena rompe la primera línea defensiva.",
                "Colinas_de_San_Juan_y_Chorrillos-13", "Andres_Avelino_Caceres_en_uniforme-19", IZQ, "BanderaPeru",
                new[] { "30_Batalla_De_Miraflores" }),

            N("30_Batalla_De_Miraflores", "Andrés Avelino Cáceres",
                "15 de enero de 1881. En los reductos de Miraflores, en medio del humo y el silencio previo al choque final, Cáceres lidera un contraataque que hace retroceder al ala izquierda chilena, pero carece de refuerzos oportunos.",
                "Miraflores_entre_humo_y_silencio-12", "Andres_Avelino_Caceres_en_uniforme-19", IZQ, "BanderaPeru",
                new[] { "31_Resistencia_De_La_Brena", "32_Ficcion_Victoria_Miraflores" },
                new[]
                {
                    "Tras resultar gravemente herido y caer Lima, internarse en los Andes para iniciar la heroica Campaña de la Breña.",
                    "Recibir a tiempo las reservas intactas de Piérola en Miraflores para sellar la brecha y rechazar el asalto final sobre la capital."
                }),

            N("31_Resistencia_De_La_Brena", "Andrés Avelino Cáceres",
                "1881 - 1883. Conocido como “El Brujo de los Andes”, el general Andrés Avelino Cáceres mantiene en jaque al ejército de ocupación en la sierra central con victorias en Pucará, Marcavalle y Concepción, hasta la agotadora batalla de Huamachuco en julio de 1883.",
                "Miraflores_entre_humo_y_silencio-12", "Andres_Avelino_Caceres_en_uniforme-19", IZQ, "BanderaPeru",
                new[] { "35_Final_Historico" }),

            N("32_Ficcion_Victoria_Miraflores", "Andrés Avelino Cáceres",
                "El refuerzo oportuno en Miraflores detiene la ofensiva enemiga a las puertas de Lima. Agotado por las bajas y sin lograr tomar la capital, Chile acepta negociar un armisticio equilibrado.",
                "Miraflores_entre_humo_y_silencio-12", "Andres_Avelino_Caceres_en_uniforme-19", IZQ, "BanderaPeru",
                new[] { "34_Final_Ficticio" }),

            N("33_Final_Desastre_Prematuro", "Narrador",
                "FINAL 1 — DESASTRE PREMATURO: Las decisiones precipitadas y la falta de coordinación estratégica provocaron un colapso temprano antes de consolidar la defensa nacional. Sin escuadra ni líneas de resistencia organizadas, el conflicto termina de forma catastrófica con la pérdida inmediata de toda capacidad de negociación. (Fin de la partida).",
                "Miraflores_entre_humo_y_silencio-12", null, IZQ, "BanderaChile",
                new[] { "01_Iquique_Intro" },
                new[] { "Reiniciar la historia desde la Bahía de Iquique (21 de mayo de 1879)." }),

            N("34_Final_Ficticio", "Narrador",
                "FINAL 2 — FICTICIO (EL ARMISTICIO DEL PACÍFICO): Gracias a la previsión diplomática, la preservación del poder naval y una defensa coordinada entre aliados, el Perú y Bolivia logran frenar la invasión. El armisticio firmado establece un acuerdo comercial compartido sobre el salitre y preserva la integridad territorial de Tarapacá, Tacna y Arica. (Fin de la partida).",
                "Huascar_en_la_costa_de_Atacama-1", null, IZQ, "BanderaPeru",
                new[] { "01_Iquique_Intro" },
                new[] { "Reiniciar la historia desde la Bahía de Iquique (21 de mayo de 1879)." }),

            N("35_Final_Historico", "Narrador",
                "FINAL 3 — HISTÓRICO (MEMORIA Y RECONSTRUCCIÓN): Tras el Tratado de Ancón en octubre de 1883, el Perú inicia una larga reconstrucción nacional, logrando décadas después la reincorporación de Tacna en 1929. El legado inmortal de Miguel Grau en Iquique, Francisco Bolognesi en Arica y Andrés Avelino Cáceres en la resistencia queda grabado para siempre en la historia. (Fin de la partida).",
                "Ascenso_al_Morro_de_Arica-14", null, IZQ, "BanderaPeru",
                new[] { "01_Iquique_Intro" },
                new[] { "Reiniciar la historia desde la Bahía de Iquique (21 de mayo de 1879)." }),
        };
    }
}
#endif

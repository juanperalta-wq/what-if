#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Herramienta de Editor de "¿Y si...? Perú".
///
/// 1) Asigna a cada uno de los 35 StoryNode su AudioClip de voz/narración,
///    su acto (número + título limpio) y marca el primer nodo de cada acto.
/// 2) Audita el grafo: alcanzabilidad de los 35 nodos y de los 3 finales.
/// 3) Configura ambas escenas: AudioManager con sus pistas y el catálogo de
///    nodos del GameManager (necesario para Continuar).
///
/// No crea, borra ni duplica StoryNodes: solo modifica los existentes.
/// </summary>
public static class AssignNodeAudios
{
    private const string NodesFolder = "Assets/ScriptableObjects/Nodes";
    private const string AudioFolder = "Assets/Audio";
    private const string MusicFolder = "Assets/Audio/Musica";
    private const string GameScenePath = "Assets/Scenes/What_If_Peru.unity";
    private const string MenuScenePath = "Assets/Scenes/MainInicio.unity";

    private static readonly Dictionary<int, string> ActTitles = new Dictionary<int, string>
    {
        { 1, "Las Raíces del Conflicto (1879)" },
        { 2, "La Campaña Naval: Iquique y Angamos" },
        { 3, "La Campaña de Tarapacá" },
        { 4, "Tacna y el Morro de Arica" },
        { 5, "Defensa de Lima y la Resistencia" },
    };

    // ---------------------------------------------------------------
    // 1. Audios y actos de los nodos
    // ---------------------------------------------------------------

    [MenuItem("Tools/What If Peru/1. Asignar Audios y Actos a los 35 Nodos")]
    public static void AssignVoicesAndActs()
    {
        List<StoryNode> nodes = LoadAllNodes();
        if (nodes.Count == 0)
        {
            Debug.LogError("[AssignNodeAudios] No se encontraron StoryNodes.");
            return;
        }

        Dictionary<int, string> audioByIndex = BuildAudioIndex();
        int assigned = 0;
        int missing = 0;

        foreach (StoryNode node in nodes)
        {
            int index = ParseNodeIndex(node.name);
            if (index <= 0)
            {
                Debug.LogWarning($"[AssignNodeAudios] '{node.name}' no tiene un número al inicio.");
                continue;
            }

            int act = ActForIndex(index);
            node.actNumber = act;
            node.actTitle = ActTitles[act];
            node.isActStart = IsActStart(index);

            if (audioByIndex.TryGetValue(index, out string audioPath))
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                if (clip != null)
                {
                    node.voiceAudio = clip;
                    assigned++;
                }
                else
                {
                    missing++;
                    Debug.LogWarning($"[AssignNodeAudios] No se pudo cargar '{audioPath}'.");
                }
            }
            else
            {
                missing++;
                Debug.LogWarning($"[AssignNodeAudios] No hay audio para '{node.name}'.");
            }

            EditorUtility.SetDirty(node);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AssignNodeAudios] {nodes.Count} nodos procesados · {assigned} audios asignados · " +
                  $"{missing} sin audio · actos 1-5 aplicados.");
    }

    private static Dictionary<int, string> BuildAudioIndex()
    {
        var result = new Dictionary<int, string>();
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');

            // Las pistas de música y los SFX sueltos no son voces de nodo.
            if (path.StartsWith(MusicFolder + "/", StringComparison.OrdinalIgnoreCase))
                continue;

            string stem = Path.GetFileNameWithoutExtension(path);

            if (!stem.StartsWith("Nodo ", StringComparison.OrdinalIgnoreCase))
                continue;

            string numberPart = stem.Substring("Nodo ".Length).Split('_')[0];
            if (!int.TryParse(numberPart, out int index))
                continue;

            // Si hubiera dos audios para el mismo nodo, conserva el primero.
            if (!result.ContainsKey(index))
                result.Add(index, path);
        }

        return result;
    }

    private static int ParseNodeIndex(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName))
            return -1;

        int end = 0;
        while (end < nodeName.Length && char.IsDigit(nodeName[end]))
            end++;

        if (end == 0)
            return -1;

        return int.TryParse(nodeName.Substring(0, end), out int index) ? index : -1;
    }

    private static int ActForIndex(int index)
    {
        if (index <= 8) return 1;
        if (index <= 12) return 2;
        if (index <= 17) return 3;
        if (index <= 24) return 4;
        return 5;
    }

    private static bool IsActStart(int index)
    {
        return index == 1 || index == 9 || index == 13 || index == 18 || index == 25;
    }

    // ---------------------------------------------------------------
    // 2. Auditoría de conexiones y finales
    // ---------------------------------------------------------------

    [MenuItem("Tools/What If Peru/2. Auditar Conexiones hacia los 3 Finales")]
    public static void AuditGraph()
    {
        List<StoryNode> nodes = LoadAllNodes();
        var byName = new Dictionary<string, StoryNode>();

        foreach (StoryNode node in nodes)
            byName[node.name] = node;

        if (!byName.TryGetValue("01_Iquique_Intro", out StoryNode start))
        {
            Debug.LogError("[AssignNodeAudios] No existe el nodo inicial '01_Iquique_Intro'.");
            return;
        }

        var reachable = new HashSet<StoryNode>();
        var queue = new Queue<StoryNode>();
        queue.Enqueue(start);
        reachable.Add(start);

        while (queue.Count > 0)
        {
            StoryNode current = queue.Dequeue();
            if (current.nextNodes == null)
                continue;

            foreach (StoryNode next in current.nextNodes)
            {
                if (next != null && reachable.Add(next))
                    queue.Enqueue(next);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine($"[Auditoría] {nodes.Count} nodos · {reachable.Count} alcanzables desde 01_Iquique_Intro.");

        int problems = 0;

        foreach (StoryNode node in nodes)
        {
            if (!reachable.Contains(node))
            {
                sb.AppendLine($" - INALCANZABLE: '{node.name}'.");
                problems++;
            }

            if (node.voiceAudio == null)
            {
                sb.AppendLine($" - SIN VOZ: '{node.name}'.");
                problems++;
            }

            if (node.actNumber < 1 || node.actNumber > 5)
            {
                sb.AppendLine($" - ACTO INVÁLIDO: '{node.name}' → {node.actNumber}.");
                problems++;
            }
        }

        string[] finals = { "33_Final_Desastre_Prematuro", "34_Final_Ficticio", "35_Final_Historico" };

        foreach (string finalName in finals)
        {
            if (!byName.TryGetValue(finalName, out StoryNode finalNode) || !reachable.Contains(finalNode))
            {
                sb.AppendLine($" - FINAL INALCANZABLE: '{finalName}'.");
                problems++;
            }
            else
            {
                sb.AppendLine($" - OK: '{finalName}' es alcanzable.");
            }
        }

        // Rutas de referencia del guion.
        CheckPath(sb, byName, "10_Desastre_Varadura_Iquique", "33_Final_Desastre_Prematuro", ref problems);
        CheckPath(sb, byName, "28_Ficcion_Sorpresa_Lurin", "34_Final_Ficticio", ref problems);
        CheckPath(sb, byName, "31_Resistencia_De_La_Brena", "35_Final_Historico", ref problems);

        if (problems == 0)
            Debug.Log(sb.ToString());
        else
            Debug.LogWarning(sb.ToString());
    }

    private static void CheckPath(
        StringBuilder sb,
        Dictionary<string, StoryNode> byName,
        string fromName,
        string toName,
        ref int problems)
    {
        if (!byName.TryGetValue(fromName, out StoryNode from) ||
            !byName.TryGetValue(toName, out StoryNode to))
        {
            sb.AppendLine($" - FALTA NODO en la ruta {fromName} → {toName}.");
            problems++;
            return;
        }

        var visited = new HashSet<StoryNode>();
        var queue = new Queue<StoryNode>();
        queue.Enqueue(from);
        visited.Add(from);
        bool found = false;

        while (queue.Count > 0 && !found)
        {
            StoryNode current = queue.Dequeue();
            if (current == to)
            {
                found = true;
                break;
            }

            if (current.nextNodes == null)
                continue;

            foreach (StoryNode next in current.nextNodes)
            {
                if (next != null && visited.Add(next))
                    queue.Enqueue(next);
            }
        }

        if (found)
            sb.AppendLine($" - OK: ruta '{fromName}' → '{toName}'.");
        else
        {
            sb.AppendLine($" - RUTA ROTA: '{fromName}' no desemboca en '{toName}'.");
            problems++;
        }
    }

    // ---------------------------------------------------------------
    // 3. Configuración de escenas
    // ---------------------------------------------------------------

    [MenuItem("Tools/What If Peru/Configurar todo (Audios + Actos + Escenas)")]
    public static void ConfigureEverything()
    {
        AssignVoicesAndActs();
        AuditGraph();
        ConfigureScenes();
    }

    [MenuItem("Tools/What If Peru/3. Configurar Escenas (AudioManager y catálogo)")]
    public static void ConfigureScenes()
    {
        List<StoryNode> nodes = LoadAllNodes();
        Debug.Log($"[AssignNodeAudios] Catálogo cargado desde disco: {nodes.Count} nodos.");

        EditorSceneManager.SaveOpenScenes();

        ConfigureScene(MenuScenePath, null);
        ConfigureScene(GameScenePath, nodes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AssignNodeAudios] Escenas configuradas y guardadas.");
    }

    private static void ConfigureScene(string scenePath, List<StoryNode> catalogue)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        string hostName = catalogue == null ? "AudioManager" : "Music";
        GameObject host = GameObject.Find(hostName);

        if (host == null)
            host = new GameObject(hostName);

        AudioManager audioManager = host.GetComponent<AudioManager>();
        if (audioManager == null)
            audioManager = host.AddComponent<AudioManager>();

        AudioSource musicA = GetOrCreateSource(host.transform, "Music Source A", true);
        AudioSource musicB = GetOrCreateSource(host.transform, "Music Source B", true);
        AudioSource voice = GetOrCreateSource(host.transform, "Voice Source", false);

        var serializedAudio = new SerializedObject(audioManager);
        serializedAudio.FindProperty("menuMusic").objectReferenceValue =
            LoadClip($"{MusicFolder}/MainInicio.ogg");
        serializedAudio.FindProperty("act1Music").objectReferenceValue =
            LoadClip($"{MusicFolder}/ACTO I Iquique y las Raíces del Conflicto.ogg");
        serializedAudio.FindProperty("act2Music").objectReferenceValue =
            LoadClip($"{MusicFolder}/ACTO II La Campaña Naval (Iquique y Angamos).ogg");
        serializedAudio.FindProperty("act3Music").objectReferenceValue =
            LoadClip($"{MusicFolder}/ACTO III La Campaña de Tarapacá.ogg");
        serializedAudio.FindProperty("act4Music").objectReferenceValue =
            LoadClip($"{MusicFolder}/ACTO IV Tacna y el Morro de Arica.ogg");
        serializedAudio.FindProperty("act5Music").objectReferenceValue =
            LoadClip($"{MusicFolder}/ACTO V Defensa de Lima, la Resistencia y los Tres Finales.ogg");
        serializedAudio.FindProperty("musicSourceA").objectReferenceValue = musicA;
        serializedAudio.FindProperty("musicSourceB").objectReferenceValue = musicB;
        serializedAudio.FindProperty("voiceSource").objectReferenceValue = voice;
        serializedAudio.ApplyModifiedProperties();
        serializedAudio.Update();

        if (catalogue != null)
            ConfigureGameManager(catalogue, scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[AssignNodeAudios] Escena configurada: {scenePath}");
    }

    private static void ConfigureGameManager(List<StoryNode> catalogue, Scene scene)
    {
        GameManager gameManager = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            gameManager = root.GetComponentInChildren<GameManager>(true);
            if (gameManager != null)
                break;
        }

        if (gameManager == null)
        {
            Debug.LogError("[AssignNodeAudios] No hay GameManager en What_If_Peru.");
            return;
        }

        var serializedGame = new SerializedObject(gameManager);
        serializedGame.Update();

        Debug.Log($"[AssignNodeAudios] GameManager encontrado: '{gameManager.name}' en " +
                  $"'{gameManager.gameObject.scene.path}' · catálogo recibido: {catalogue.Count}.");

        SerializedProperty startingNode = serializedGame.FindProperty("startingNode");
        if (startingNode.objectReferenceValue == null)
        {
            StoryNode start = catalogue.Find(node => node.name == "01_Iquique_Intro");
            if (start != null)
                startingNode.objectReferenceValue = start;
        }

        SerializedProperty uiManager = serializedGame.FindProperty("uiManager");
        if (uiManager.objectReferenceValue == null)
        {
            UIManager manager = null;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                manager = root.GetComponentInChildren<UIManager>(true);
                if (manager != null)
                    break;
            }

            if (manager != null)
                uiManager.objectReferenceValue = manager;
        }

        serializedGame.ApplyModifiedProperties();

        // El catálogo se asigna DESPUÉS de aplicar el SerializedObject para que
        // su copia en caché no sobrescriba la lista con elementos nulos.
        gameManager.SetNodeCatalogue(catalogue);
        EditorUtility.SetDirty(gameManager);

        Debug.Log($"[AssignNodeAudios] Tras SetNodeCatalogue: {gameManager.AllNodes.Count} nodos.");

        if (gameManager.AllNodes.Count == 0)
            Debug.LogError("[AssignNodeAudios] El catálogo 'allNodes' del GameManager quedó vacío.");
        else
            Debug.Log($"[AssignNodeAudios] Catálogo del GameManager: {gameManager.AllNodes.Count} nodos asignados.");
    }

    private static AudioSource GetOrCreateSource(Transform parent, string sourceName, bool loop)
    {
        Transform existing = parent.Find(sourceName);
        GameObject go = existing != null ? existing.gameObject : new GameObject(sourceName);

        if (existing == null)
            go.transform.SetParent(parent, false);

        AudioSource source = go.GetComponent<AudioSource>();
        if (source == null)
            source = go.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    private static AudioClip LoadClip(string path)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

        if (clip == null)
            Debug.LogWarning($"[AssignNodeAudios] Falta el clip: {path}");

        return clip;
    }

    private static List<StoryNode> LoadAllNodes()
    {
        var nodes = new List<StoryNode>();
        string[] guids = AssetDatabase.FindAssets("t:StoryNode", new[] { NodesFolder });

        foreach (string guid in guids)
        {
            StoryNode node = AssetDatabase.LoadAssetAtPath<StoryNode>(
                AssetDatabase.GUIDToAssetPath(guid));

            if (node != null)
                nodes.Add(node);
        }

        nodes.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return nodes;
    }
}
#endif

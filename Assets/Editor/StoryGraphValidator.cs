#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Validador y reparador del grafo narrativo de "¿Y si...? Perú".
///
/// Grafo vigente: "Ecos del Pacífico (1873 - 1883)" — 35 nodos y 3 finales.
/// Los finales ofrecen una única opción para reiniciar la historia.
///
/// Uso:
/// - Tools → ¿Y si...? Perú → Validar enlaces del grafo
/// - Tools → ¿Y si...? Perú → Reparar enlaces del grafo
///
/// Al cargar el Editor se ejecuta una validación silenciosa: solo escribe en
/// consola si encuentra enlaces rotos.
/// </summary>
public static class StoryGraphValidator
{
    private const string NodesFolder = "Assets/ScriptableObjects/Nodes";
    private const string StartingNodeName = "01_Iquique_Intro";

    /// <summary>
    /// Grafo esperado: nombre del nodo → nombres de sus salidas en orden A/B/C.
    /// Los finales tienen una sola salida: el reinicio al nodo 01.
    /// </summary>
    private static readonly Dictionary<string, string[]> ExpectedLinks =
        new Dictionary<string, string[]>
        {
            { "01_Iquique_Intro", new[] { "02_Puente_Del_Huascar" } },
            { "02_Puente_Del_Huascar", new[] { "03_Origen_Antofagasta" } },
            { "03_Origen_Antofagasta", new[] { "05_Ficcion_Mediacion_Aduana", "04_Mision_Lavalle_Crisis", "06_Desastre_Ruptura_Total" } },
            { "04_Mision_Lavalle_Crisis", new[] { "06_Desastre_Ruptura_Total", "07_Ficcion_Escuadra_Alistada", "08_Rescate_En_Iquique" } },
            { "05_Ficcion_Mediacion_Aduana", new[] { "07_Ficcion_Escuadra_Alistada" } },
            { "06_Desastre_Ruptura_Total", new[] { "12_Angamos_Desenlace" } },
            { "07_Ficcion_Escuadra_Alistada", new[] { "08_Rescate_En_Iquique" } },
            { "08_Rescate_En_Iquique", new[] { "09_Correrias_Del_Huascar", "10_Desastre_Varadura_Iquique", "11_Ficcion_Dominio_Naval" } },
            { "09_Correrias_Del_Huascar", new[] { "11_Ficcion_Dominio_Naval", "12_Angamos_Desenlace" } },
            { "10_Desastre_Varadura_Iquique", new[] { "33_Final_Desastre_Prematuro" } },
            { "11_Ficcion_Dominio_Naval", new[] { "15_Ficcion_Tenaza_Tarapaca" } },
            { "12_Angamos_Desenlace", new[] { "13_Desierto_Y_Camarones" } },
            { "13_Desierto_Y_Camarones", new[] { "15_Ficcion_Tenaza_Tarapaca", "14_Desastre_Sed_Atacama", "16_Victoria_Quebrada_Tarapaca" } },
            { "14_Desastre_Sed_Atacama", new[] { "33_Final_Desastre_Prematuro" } },
            { "15_Ficcion_Tenaza_Tarapaca", new[] { "18_Alianza_En_Tacna" } },
            { "16_Victoria_Quebrada_Tarapaca", new[] { "17_Crisis_Politica_Lima" } },
            { "17_Crisis_Politica_Lima", new[] { "18_Alianza_En_Tacna" } },
            { "18_Alianza_En_Tacna", new[] { "19_Batalla_Alto_Alianza", "20_Ficcion_Contragolpe_Tacna", "21_Desastre_Ruptura_Tacna" } },
            { "19_Batalla_Alto_Alianza", new[] { "22_Sitio_Puerto_Arica" } },
            { "20_Ficcion_Contragolpe_Tacna", new[] { "24_Ficcion_Bastion_Arica" } },
            { "21_Desastre_Ruptura_Tacna", new[] { "33_Final_Desastre_Prematuro" } },
            { "22_Sitio_Puerto_Arica", new[] { "24_Ficcion_Bastion_Arica", "23_Ascenso_Morro_Arica" } },
            { "23_Ascenso_Morro_Arica", new[] { "25_Desembarco_En_Lurin" } },
            { "24_Ficcion_Bastion_Arica", new[] { "34_Final_Ficticio" } },
            { "25_Desembarco_En_Lurin", new[] { "27_Desastre_Caida_Inmediata_Lima", "26_Defensas_Rumbo_A_Lima", "28_Ficcion_Sorpresa_Lurin" } },
            { "26_Defensas_Rumbo_A_Lima", new[] { "29_San_Juan_Y_Chorrillos" } },
            { "27_Desastre_Caida_Inmediata_Lima", new[] { "33_Final_Desastre_Prematuro" } },
            { "28_Ficcion_Sorpresa_Lurin", new[] { "34_Final_Ficticio" } },
            { "29_San_Juan_Y_Chorrillos", new[] { "30_Batalla_De_Miraflores" } },
            { "30_Batalla_De_Miraflores", new[] { "31_Resistencia_De_La_Brena", "32_Ficcion_Victoria_Miraflores" } },
            { "31_Resistencia_De_La_Brena", new[] { "35_Final_Historico" } },
            { "32_Ficcion_Victoria_Miraflores", new[] { "34_Final_Ficticio" } },
            { "33_Final_Desastre_Prematuro", new[] { "01_Iquique_Intro" } },
            { "34_Final_Ficticio", new[] { "01_Iquique_Intro" } },
            { "35_Final_Historico", new[] { "01_Iquique_Intro" } },
        };

    [MenuItem("Tools/¿Y si...? Perú/Validar enlaces del grafo")]
    public static void ValidateGraphMenu()
    {
        ValidateGraph(false);
    }

    [MenuItem("Tools/¿Y si...? Perú/Reparar enlaces del grafo")]
    public static void RepairGraph()
    {
        var missing = new List<string>();
        int repaired = 0;

        foreach (KeyValuePair<string, string[]> pair in ExpectedLinks)
        {
            StoryNode node = AssetDatabase.LoadAssetAtPath<StoryNode>(
                $"{NodesFolder}/{pair.Key}.asset");

            if (node == null)
            {
                missing.Add(pair.Key);
                continue;
            }

            var targets = new List<StoryNode>();
            bool broken = false;

            foreach (string targetName in pair.Value)
            {
                StoryNode target = AssetDatabase.LoadAssetAtPath<StoryNode>(
                    $"{NodesFolder}/{targetName}.asset");

                if (target == null)
                {
                    missing.Add(targetName);
                    broken = true;
                    continue;
                }

                targets.Add(target);
            }

            if (broken)
                continue;

            node.nextNodes = targets;
            EditorUtility.SetDirty(node);
            repaired++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (missing.Count > 0)
            Debug.LogError($"[StoryGraphValidator] Faltan assets: {string.Join(", ", missing)}");

        Debug.Log($"[StoryGraphValidator] Enlaces reaplicados en {repaired} nodos.");
        ValidateGraph(false);
    }

    [InitializeOnLoadMethod]
    private static void AutoValidateOnLoad()
    {
        // delayCall espera a que AssetDatabase termine de importar.
        EditorApplication.delayCall += () => ValidateGraph(true);
    }

    private static void ValidateGraph(bool silentWhenOk)
    {
        List<StoryNode> nodes = LoadNodes();
        var problems = new List<string>();
        int finals = 0;
        int linear = 0;
        int decisions = 0;

        foreach (StoryNode node in nodes)
        {
            int count = node.nextNodes != null ? node.nextNodes.Count : 0;
            int texts = node.choiceTexts != null ? node.choiceTexts.Count : 0;
            bool isFinal = node.IsFinalNode;

            if (isFinal)
                finals++;
            else if (count == 1 && texts == 0)
                linear++;
            else if (count >= 2)
                decisions++;

            for (int i = 0; i < count; i++)
            {
                if (node.nextNodes[i] == null)
                    problems.Add($"'{node.name}' tiene nextNodes[{i}] nulo.");
            }

            if (isFinal)
            {
                if (count != 1)
                    problems.Add($"'{node.name}' es final y debe tener 1 salida (reinicio), tiene {count}.");

                if (texts != 1)
                    problems.Add($"'{node.name}' es final y debe tener 1 texto de reinicio, tiene {texts}.");

                if (count == 1 && node.nextNodes[0] != null &&
                    node.nextNodes[0].name != StartingNodeName)
                {
                    problems.Add($"'{node.name}' no reinicia en '{StartingNodeName}'.");
                }

                continue;
            }

            if (count == 0)
            {
                problems.Add($"'{node.name}' no es final pero tiene 0 salidas.");
                continue;
            }

            if (count > StoryNode.MaxChoices)
                problems.Add($"'{node.name}' tiene {count} salidas (máximo {StoryNode.MaxChoices}).");

            if (count == 1 && texts > 0)
                problems.Add($"'{node.name}' es lineal pero tiene textos de opción.");

            if (count >= 2 && texts != count)
                problems.Add($"'{node.name}' tiene {count} salidas y {texts} textos de opción.");
        }

        if (problems.Count == 0)
        {
            if (!silentWhenOk)
            {
                Debug.Log($"[StoryGraphValidator] Grafo OK: {nodes.Count} nodos " +
                          $"({finals} finales, {linear} lineales, {decisions} decisiones).");
            }
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"[StoryGraphValidator] {problems.Count} problema(s) en {nodes.Count} nodos:");
        foreach (string problem in problems)
            sb.AppendLine(" - " + problem);
        Debug.LogError(sb.ToString());
    }

    private static List<StoryNode> LoadNodes()
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

        return nodes;
    }
}
#endif

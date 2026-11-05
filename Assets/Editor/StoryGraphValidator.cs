#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Validador y reparador del grafo narrativo de "¿Y si...? Perú".
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

    /// <summary>
    /// Grafo esperado: nombre del nodo → nombres de sus salidas en orden A/B.
    /// Los finales se declaran con un arreglo vacío para que el reparador
    /// también limpie salidas accidentales.
    /// </summary>
    private static readonly Dictionary<string, string[]> ExpectedLinks =
        new Dictionary<string, string[]>
        {
            { "01_Iquique_Intro", new[] { "02_Iquique_Grau" } },
            { "02_Iquique_Grau", new[] { "03_Iquique_Reporte" } },
            { "03_Iquique_Reporte", new[] { "04_Iquique_Decision" } },
            { "04_Iquique_Decision", new[] { "05A_Pierde_Independencia", "05B_Salva_Independencia" } },
            { "05A_Pierde_Independencia", new[] { "06_Tarapaca_Intro_SinFlota" } },
            { "05B_Salva_Independencia", new[] { "06_Tarapaca_Intro_ConFlota" } },

            { "06_Tarapaca_Intro_SinFlota", new[] { "07_Tarapaca_Caceres_SinFlota" } },
            { "07_Tarapaca_Caceres_SinFlota", new[] { "08_Tarapaca_Bolognesi_SinFlota" } },
            { "08_Tarapaca_Bolognesi_SinFlota", new[] { "09_Tarapaca_Decision_SinFlota" } },
            { "09_Tarapaca_Decision_SinFlota", new[] { "10A_Retirada_Arica_SinFlota", "10B_Ataque_Pisagua" } },
            { "10A_Retirada_Arica_SinFlota", new[] { "11_Alto_Intro_SinFlota" } },
            { "11_Alto_Intro_SinFlota", new[] { "12_Alto_Campero_SinFlota" } },
            { "12_Alto_Campero_SinFlota", new[] { "13_Alto_Montero_SinFlota" } },
            { "13_Alto_Montero_SinFlota", new[] { "14_Alto_Decision_SinFlota" } },
            { "14_Alto_Decision_SinFlota", new[] { "FINAL_01_Historico", "FINAL_01_Historico" } },

            { "06_Tarapaca_Intro_ConFlota", new[] { "07_Tarapaca_Caceres_ConFlota" } },
            { "07_Tarapaca_Caceres_ConFlota", new[] { "08_Tarapaca_Bolognesi_ConFlota" } },
            { "08_Tarapaca_Bolognesi_ConFlota", new[] { "09_Tarapaca_Decision_ConFlota" } },
            { "09_Tarapaca_Decision_ConFlota", new[] { "10A_Retirada_Arica_ConFlota", "10B_Ataque_Pisagua" } },
            { "10A_Retirada_Arica_ConFlota", new[] { "11_Alto_Intro_ConFlota" } },
            { "11_Alto_Intro_ConFlota", new[] { "12_Alto_Campero_ConFlota" } },
            { "12_Alto_Campero_ConFlota", new[] { "13_Alto_Montero_ConFlota" } },
            { "13_Alto_Montero_ConFlota", new[] { "14_Alto_Decision_ConFlota" } },
            { "14_Alto_Decision_ConFlota", new[] { "FINAL_01_Historico", "FINAL_02_PazArmada" } },

            { "10B_Ataque_Pisagua", new[] { "FINAL_03_Desastre" } },

            { "FINAL_01_Historico", new string[0] },
            { "FINAL_02_PazArmada", new string[0] },
            { "FINAL_03_Desastre", new string[0] },
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
            bool isFinal = node.name.StartsWith("FINAL_");

            if (isFinal)
                finals++;
            else if (count == 1)
                linear++;
            else if (count == 2)
                decisions++;

            if (!isFinal && count == 0)
                problems.Add($"'{node.name}' no es final pero tiene 0 salidas.");

            if (isFinal && count != 0)
                problems.Add($"'{node.name}' es final pero tiene {count} salidas.");

            for (int i = 0; i < count; i++)
            {
                if (node.nextNodes[i] == null)
                    problems.Add($"'{node.name}' tiene nextNodes[{i}] nulo.");
            }

            if (count == 1 && node.choiceTexts != null && node.choiceTexts.Count > 0)
                problems.Add($"'{node.name}' es lineal pero tiene textos de opción.");

            if (count == 2 && (node.choiceTexts == null || node.choiceTexts.Count != 2))
                problems.Add($"'{node.name}' es decisión pero no tiene exactamente 2 textos.");

            if (count > 2)
                problems.Add($"'{node.name}' tiene {count} salidas (máximo 2).");
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

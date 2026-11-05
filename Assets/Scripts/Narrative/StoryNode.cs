using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nodo de la novela visual "¿Y si...? Perú".
/// Representa una caja de diálogo y, opcionalmente, una bifurcación binaria.
///
/// Reglas del grafo:
/// - 0 nodos siguientes  -> nodo final.
/// - 1 nodo siguiente    -> avance automático.
/// - 2 nodos siguientes  -> Momento Decisivo (Opción A / Opción B).
/// </summary>
[CreateAssetMenu(fileName = "StoryNode", menuName = "¿Y si...? Perú/Narrativa/Nodo de Historia", order = 0)]
public class StoryNode : ScriptableObject
{
    /// <summary>Token global que se reemplaza por el nombre del jugador.</summary>
    public const string PlayerNameToken = "{PLAYER_NAME}";

    [Header("Interlocutor")]
    [Tooltip("Nombre visible sobre la caja de diálogo. Admite el token {PLAYER_NAME}.")]
    public string speakerName;

    [Header("Diálogo")]
    [TextArea(3, 10)]
    [Tooltip("Texto narrativo. Admite el token {PLAYER_NAME}.")]
    public string dialogueText;

    [Header("Arte")]
    [Tooltip("Retrato del personaje que habla. Puede quedar vacío.")]
    public Sprite characterSprite;

    [Tooltip("Fondo del escenario histórico. Puede quedar vacío.")]
    public Sprite backgroundSprite;

    [Header("Grafo de decisiones")]
    [Tooltip("0 = final; 1 = avance automático; 2 = decisión binaria.")]
    public List<StoryNode> nextNodes = new List<StoryNode>();

    [Tooltip("Textos de los botones A y B. Solo se usan cuando hay 2 nodos siguientes.")]
    public List<string> choiceTexts = new List<string>();

    public bool IsEnding => nextNodes == null || nextNodes.Count == 0;
    public bool IsLinear => nextNodes != null && nextNodes.Count == 1;
    public bool HasChoices => nextNodes != null && nextNodes.Count >= 2;

    public int ChoiceCount
    {
        get
        {
            if (!HasChoices)
                return 0;

            int textCount = choiceTexts != null ? choiceTexts.Count : 0;
            return Mathf.Clamp(textCount, 0, 2);
        }
    }

    public string ResolveSpeaker(string playerName)
    {
        return ResolveTokens(speakerName, playerName);
    }

    public string ResolveDialogue(string playerName)
    {
        return ResolveTokens(dialogueText, playerName);
    }

    /// <summary>Reemplaza el token global {PLAYER_NAME} por el nombre actual.</summary>
    public static string ResolveTokens(string value, string playerName)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Estratega";

        return value.Replace(PlayerNameToken, playerName);
    }

    private void OnValidate()
    {
        if (nextNodes == null)
            nextNodes = new List<StoryNode>();

        if (choiceTexts == null)
            choiceTexts = new List<string>();

        // La regla de oro del GDD: exactamente 2 opciones como máximo.
        if (nextNodes.Count > 2)
            nextNodes.RemoveRange(2, nextNodes.Count - 2);

        if (choiceTexts.Count > 2)
            choiceTexts.RemoveRange(2, choiceTexts.Count - 2);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Valida que este nodo tenga un enlace coherente con su tipo.
    /// Se ejecuta desde el menú contextual del Inspector del asset:
    /// clic derecho sobre el encabezado del ScriptableObject → "Validate Links".
    /// </summary>
    [ContextMenu("Validate Links")]
    private void ValidateLinks()
    {
        int count = nextNodes != null ? nextNodes.Count : 0;
        bool isFinal = name.StartsWith("FINAL_");

        if (count == 0)
        {
            if (isFinal)
                Debug.Log($"[StoryNode] '{name}' es un final válido (0 salidas).", this);
            else
                Debug.LogWarning($"[StoryNode] '{name}' no es un nodo final pero no tiene 'nextNodes'.", this);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (nextNodes[i] == null)
                Debug.LogError($"[StoryNode] '{name}' tiene una referencia nula en nextNodes[{i}].", this);
        }

        if (count == 1)
        {
            if (choiceTexts != null && choiceTexts.Count > 0)
                Debug.LogWarning($"[StoryNode] '{name}' es lineal (1 salida) pero tiene textos de opción.", this);
            else
                Debug.Log($"[StoryNode] '{name}' lineal → '{nextNodes[0].name}'. OK.", this);
        }
        else if (count == 2)
        {
            int texts = choiceTexts != null ? choiceTexts.Count : 0;
            if (texts != 2)
                Debug.LogWarning($"[StoryNode] '{name}' tiene 2 salidas pero {texts} textos de opción.", this);
            else
                Debug.Log($"[StoryNode] '{name}' decisión A → '{nextNodes[0].name}', B → '{nextNodes[1].name}'. OK.", this);
        }
        else
        {
            Debug.LogError($"[StoryNode] '{name}' tiene {count} salidas; el máximo permitido es 2.", this);
        }
    }
#endif
}

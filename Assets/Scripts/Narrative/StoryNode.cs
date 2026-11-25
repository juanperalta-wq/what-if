using System.Collections.Generic;
using UnityEngine;

/// <summary>Lado de la pantalla donde se apoya el retrato del interlocutor.</summary>
public enum CharacterSide
{
    Izquierda = 0,
    Derecha = 1
}

/// <summary>
/// Nodo de la novela visual "¿Y si...? Perú".
/// Representa una caja de diálogo y, opcionalmente, un Momento Decisivo.
///
/// Reglas del grafo:
/// - 0 nodos siguientes  -> nodo final sin salida.
/// - 1 nodo siguiente    -> avance automático.
/// - 1..3 nodos siguientes con textos -> Momento Decisivo (A / B / C).
///
/// Los finales del guion "Ecos del Pacífico" ofrecen una única opción
/// ("Reiniciar la historia"), por eso una decisión puede tener una sola opción.
/// </summary>
[CreateAssetMenu(fileName = "StoryNode", menuName = "¿Y si...? Perú/Narrativa/Nodo de Historia", order = 0)]
public class StoryNode : ScriptableObject
{
    /// <summary>Token global que se reemplaza por el nombre del jugador.</summary>
    public const string PlayerNameToken = "{PLAYER_NAME}";

    /// <summary>Cantidad máxima de opciones de un Momento Decisivo.</summary>
    public const int MaxChoices = 3;

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

    [Header("Puesta en escena")]
    [Tooltip("Lado de la pantalla donde aparece el retrato. El centro no se usa.")]
    public CharacterSide characterSide = CharacterSide.Izquierda;

    [Tooltip("Banderas visibles en la esquina superior izquierda (máximo 3).")]
    public List<Sprite> flagSprites = new List<Sprite>();

    [Header("Grafo de decisiones")]
    [Tooltip("0 = final; 1 = avance automático o una sola opción; 2-3 = decisión.")]
    public List<StoryNode> nextNodes = new List<StoryNode>();

    [Tooltip("Textos de los botones A, B y C. Su cantidad debe coincidir con nextNodes.")]
    public List<string> choiceTexts = new List<string>();

    /// <summary>Nodo sin ninguna salida (final estructural).</summary>
    public bool IsEnding => nextNodes == null || nextNodes.Count == 0;

    /// <summary>Nodo con una única salida y sin textos de opción.</summary>
    public bool IsLinear => nextNodes != null && nextNodes.Count == 1 && ChoiceCount == 0;

    /// <summary>Nodo con una o más opciones presentadas como botones.</summary>
    public bool HasChoices => ChoiceCount > 0;

    /// <summary>
    /// Los finales se reconocen por el prefijo "FINAL_" (guion anterior)
    /// o por el segmento "_Final_" (guion "Ecos del Pacífico").
    /// </summary>
    public bool IsFinalNode =>
        !string.IsNullOrEmpty(name) &&
        (name.StartsWith("FINAL_") || name.Contains("_Final_"));

    public int ChoiceCount
    {
        get
        {
            int links = nextNodes != null ? nextNodes.Count : 0;
            int texts = choiceTexts != null ? choiceTexts.Count : 0;
            return Mathf.Clamp(Mathf.Min(links, texts), 0, MaxChoices);
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

        if (flagSprites == null)
            flagSprites = new List<Sprite>();

        if (flagSprites.Count > 3)
            flagSprites.RemoveRange(3, flagSprites.Count - 3);

        // Regla del GDD rework: como máximo 3 opciones por Momento Decisivo.
        if (nextNodes.Count > MaxChoices)
            nextNodes.RemoveRange(MaxChoices, nextNodes.Count - MaxChoices);

        if (choiceTexts.Count > MaxChoices)
            choiceTexts.RemoveRange(MaxChoices, choiceTexts.Count - MaxChoices);
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
        int texts = choiceTexts != null ? choiceTexts.Count : 0;

        if (count == 0)
        {
            if (IsFinalNode)
                Debug.Log($"[StoryNode] '{name}' es un final sin salida. OK.", this);
            else
                Debug.LogWarning($"[StoryNode] '{name}' no es un nodo final pero no tiene 'nextNodes'.", this);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (nextNodes[i] == null)
                Debug.LogError($"[StoryNode] '{name}' tiene una referencia nula en nextNodes[{i}].", this);
        }

        if (count > MaxChoices)
        {
            Debug.LogError($"[StoryNode] '{name}' tiene {count} salidas; el máximo permitido es {MaxChoices}.", this);
            return;
        }

        if (texts != count)
        {
            Debug.LogWarning(
                $"[StoryNode] '{name}' tiene {count} salidas pero {texts} textos de opción. " +
                "Cada salida necesita su texto de botón.", this);
            return;
        }

        if (count == 1)
        {
            Debug.Log($"[StoryNode] '{name}' → '{nextNodes[0].name}' con una opción. OK.", this);
            return;
        }

        Debug.Log(
            $"[StoryNode] '{name}' decisión con {count} opciones → " +
            string.Join(", ", System.Linq.Enumerable.Select(
                System.Linq.Enumerable.Range(0, count),
                i => $"{(char)('A' + i)}: '{nextNodes[i].name}'")) + ". OK.", this);
    }
#endif
}

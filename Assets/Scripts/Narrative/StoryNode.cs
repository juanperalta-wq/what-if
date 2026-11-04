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
}

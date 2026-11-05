using System;
using UnityEngine;

/// <summary>
/// Controla el recorrido del grafo narrativo de "¿Y si...? Perú".
/// No usa inventarios ni variables de reputación: el estado de la partida
/// es simplemente el StoryNode actual.
/// </summary>
[DefaultExecutionOrder(-10)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Grafo narrativo")]
    [Tooltip("Primer nodo de la historia (Combate de Iquique).")]
    [SerializeField] private StoryNode startingNode;

    [Header("Jugador")]
    [Tooltip("Nombre que reemplaza el token {PLAYER_NAME} en los diálogos.")]
    [SerializeField] private string playerName = "Estratega";

    [Header("Referencias")]
    [SerializeField] private UIManager uiManager;

    public StoryNode CurrentNode { get; private set; }

    public string PlayerName =>
        string.IsNullOrWhiteSpace(playerName) ? "Estratega" : playerName.Trim();

    /// <summary>Se dispara cada vez que se muestra un nodo nuevo.</summary>
    public event Action<StoryNode> OnNodeChanged;

    /// <summary>Se dispara al llegar a un nodo sin salidas (final de ruta).</summary>
    public event Action<StoryNode> OnStoryEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        if (uiManager == null)
            Debug.LogError("[GameManager] No se encontró un UIManager en la escena.");
    }

    private void Start()
    {
        StartGame();
    }

    /// <summary>Reinicia la historia desde el nodo inicial.</summary>
    public void StartGame()
    {
        if (startingNode == null)
        {
            Debug.LogError("[GameManager] Falta asignar 'startingNode' en el Inspector.");
            return;
        }

        GoToNode(startingNode);
    }

    /// <summary>Permite que la pantalla de ingreso de nombre actualice el token global.</summary>
    public void SetPlayerName(string newPlayerName)
    {
        if (!string.IsNullOrWhiteSpace(newPlayerName))
            playerName = newPlayerName.Trim();
    }

    /// <summary>Muestra un nodo concreto y actualiza la UI.</summary>
    public void GoToNode(StoryNode node)
    {
        if (node == null)
        {
            Debug.LogWarning("[GameManager] Se intentó avanzar a un nodo nulo.");
            return;
        }

        CurrentNode = node;

        if (uiManager != null)
        {
            uiManager.ShowNode(
                node,
                node.ResolveSpeaker(PlayerName),
                node.ResolveDialogue(PlayerName));
        }

        OnNodeChanged?.Invoke(node);
    }

    /// <summary>
    /// Llamado por UIManager cuando el jugador hace clic para avanzar.
    /// Si el nodo actual es una decisión, primero revela los botones.
    /// </summary>
    public void Advance()
    {
        if (CurrentNode == null)
        {
            StartGame();
            return;
        }

        if (CurrentNode.HasChoices)
        {
            if (uiManager == null || !uiManager.ChoicesVisible)
                PresentChoices();

            return;
        }

        if (CurrentNode.IsLinear)
        {
            GoToNode(CurrentNode.nextNodes[0]);
            return;
        }

        EndStory();
    }

    /// <summary>Recibe la selección del jugador: 0 = Opción A, 1 = Opción B.</summary>
    public void SelectChoice(int index)
    {
        if (CurrentNode == null || !CurrentNode.HasChoices)
            return;

        if (index < 0 || index >= CurrentNode.nextNodes.Count)
        {
            Debug.LogWarning($"[GameManager] Índice de decisión inválido: {index}.");
            return;
        }

        StoryNode nextNode = CurrentNode.nextNodes[index];

        if (nextNode == null)
        {
            EndStory();
            return;
        }

        GoToNode(nextNode);
    }

    private void PresentChoices()
    {
        StoryNode node = CurrentNode;

        string optionA = node.choiceTexts != null && node.choiceTexts.Count > 0
            ? StoryNode.ResolveTokens(node.choiceTexts[0], PlayerName)
            : "Opción A";

        string optionB = node.choiceTexts != null && node.choiceTexts.Count > 1
            ? StoryNode.ResolveTokens(node.choiceTexts[1], PlayerName)
            : "Opción B";

        uiManager.ShowChoices(
            optionA,
            optionB,
            () => SelectChoice(0),
            () => SelectChoice(1));
    }

    private void EndStory()
    {
        bool isFinalNode = CurrentNode != null && CurrentNode.name.StartsWith("FINAL_");

        if (isFinalNode)
            Debug.Log($"[GameManager] Fin de ruta alcanzado: {CurrentNode.name}");
        else
            Debug.LogError(
                $"[GameManager] Ruta terminada en un nodo que no es final: " +
                $"'{CurrentNode?.name}'. Revisa 'nextNodes' con " +
                "Tools → ¿Y si...? Perú → Validar enlaces del grafo.");

        OnStoryEnded?.Invoke(CurrentNode);
    }
}

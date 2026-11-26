using System;
using System.Collections.Generic;
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
    [Tooltip("Primer nodo de la historia (21 de mayo de 1879, Bahía de Iquique).")]
    [SerializeField] private StoryNode startingNode;

    [Tooltip("Catálogo completo de nodos. Se usa para cargar partidas guardadas por nombre.")]
    [SerializeField] private List<StoryNode> allNodes = new List<StoryNode>();

    [Header("Jugador")]
    [Tooltip("Nombre que reemplaza el token {PLAYER_NAME} en los diálogos.")]
    [SerializeField] private string playerName = "Estratega";

    [Header("Referencias")]
    [SerializeField] private UIManager uiManager;

    public StoryNode CurrentNode { get; private set; }

    public StoryNode StartingNode => startingNode;

    public IReadOnlyList<StoryNode> AllNodes => allNodes;

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

        // Recibe el nombre del jugador transportado desde la escena de inicio.
        if (GameSessionManager.Instance != null)
            SetPlayerName(GameSessionManager.Instance.playerName);

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        if (uiManager == null)
            Debug.LogError("[GameManager] No se encontró un UIManager en la escena.");
    }

    private void Start()
    {
        if (GameSessionManager.Instance != null && GameSessionManager.Instance.loadFromSave)
        {
            GameSessionManager.Instance.loadFromSave = false;

            if (TryContinueSavedGame())
                return;
        }

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

        CurrentNode = null;
        GoToNode(startingNode);
    }

    /// <summary>Reinicia la campaña limpiando el estado de audio del nodo anterior.</summary>
    public void RestartCampaign()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopVoice();

        StartGame();
    }

    /// <summary>Permite que la pantalla de ingreso de nombre actualice el token global.</summary>
    public void SetPlayerName(string newPlayerName)
    {
        if (!string.IsNullOrWhiteSpace(newPlayerName))
            playerName = newPlayerName.Trim();
    }

    /// <summary>Reemplaza el catálogo completo de nodos (usado por herramientas de Editor).</summary>
    public void SetNodeCatalogue(List<StoryNode> nodes)
    {
        allNodes = nodes != null ? new List<StoryNode>(nodes) : new List<StoryNode>();
    }

    /// <summary>Busca un nodo del catálogo por nombre exacto.</summary>
    public StoryNode GetNodeByName(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName))
            return null;

        for (int i = 0; i < allNodes.Count; i++)
        {
            if (allNodes[i] != null && allNodes[i].name == nodeName)
                return allNodes[i];
        }

        return null;
    }

    /// <summary>Carga el nodo guardado (usado por el botón Continuar).</summary>
    public bool TryContinueSavedGame()
    {
        if (!SaveManager.TryLoad(out SaveData data))
        {
            Debug.LogWarning("[GameManager] No hay partida guardada válida.");
            return false;
        }

        StoryNode node = GetNodeByName(data.currentNodeName);

        if (node == null)
        {
            Debug.LogWarning(
                $"[GameManager] El guardado apunta a '{data.currentNodeName}', que no está en el catálogo. " +
                "Se iniciará una campaña nueva.");
            return false;
        }

        SetPlayerName(data.playerName);
        CurrentNode = null;
        GoToNode(node);
        return true;
    }

    /// <summary>Guarda manualmente la partida actual (botón Guardar Partida).</summary>
    public bool SaveCurrentGame()
    {
        if (CurrentNode == null)
            return false;

        return SaveManager.SaveGame(CurrentNode, PlayerName);
    }

    /// <summary>Muestra un nodo concreto y actualiza la UI.</summary>
    public void GoToNode(StoryNode node)
    {
        if (node == null)
        {
            Debug.LogWarning("[GameManager] Se intentó avanzar a un nodo nulo.");
            return;
        }

        StoryNode previousNode = CurrentNode;
        CurrentNode = node;

        bool actChanged = previousNode == null || previousNode.actNumber != node.actNumber;
        bool showActCard = node.isActStart || actChanged;

        // La voz anterior nunca debe seguir sonando bajo el cartel de acto.
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopVoice();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicForAct(node.actNumber);

        // Autoguardado en cada avance.
        SaveManager.SaveGame(node, PlayerName);

        if (uiManager != null)
        {
            uiManager.ShowNode(
                node,
                node.ResolveSpeaker(PlayerName),
                node.ResolveDialogue(PlayerName),
                showActCard,
                () =>
                {
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlayVoice(node.voiceAudio);
                });
        }

        OnNodeChanged?.Invoke(node);

        // Los finales muestran el panel de cierre cuando terminan texto y voz.
        if (node.IsFinalNode)
        {
            Debug.Log($"[GameManager] Final alcanzado: {node.name}");
            OnStoryEnded?.Invoke(node);
        }
    }

    /// <summary>
    /// Llamado por UIManager cuando el jugador hace clic para avanzar.
    /// Si el nodo actual es una decisión, primero revela los botones.
    /// </summary>
    public void Advance()
    {
        if (uiManager != null && uiManager.IsInputBlocked)
            return;

        if (CurrentNode == null)
        {
            StartGame();
            return;
        }

        if (CurrentNode.HasChoices && !CurrentNode.IsFinalNode)
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

    /// <summary>Recibe la selección del jugador: 0 = A, 1 = B, 2 = C.</summary>
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
        int count = Mathf.Clamp(node.nextNodes.Count, 1, StoryNode.MaxChoices);

        string[] fallbacks = { "Opción A", "Opción B", "Opción C" };
        var texts = new List<string>(count);

        for (int i = 0; i < count; i++)
        {
            string raw = node.choiceTexts != null && i < node.choiceTexts.Count
                ? node.choiceTexts[i]
                : fallbacks[i];

            texts.Add(StoryNode.ResolveTokens(raw, PlayerName));
        }

        uiManager.ShowChoices(texts, SelectChoice);
    }

    private void EndStory()
    {
        bool isFinalNode = CurrentNode != null && CurrentNode.IsFinalNode;

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

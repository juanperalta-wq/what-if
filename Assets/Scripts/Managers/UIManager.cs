using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Puente entre el grafo narrativo y la interfaz de UI Toolkit.
/// Requiere un UIDocument con el árbol VisualNovelUI.uxml.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Efecto máquina de escribir")]
    [SerializeField, Min(1f)] private float charactersPerSecond = 45f;
    [SerializeField] private bool useTypewriter = true;

    [Header("Indicador de avance")]
    [SerializeField, Min(0.1f)] private float indicatorPulseSpeed = 2.2f;

    private VisualElement rootScreen;
    private VisualElement characterContainer;
    private VisualElement choicesContainer;
    private VisualElement dialoguePanel;
    private Label speakerNameText;
    private Label dialogueText;
    private VisualElement nextIndicator;
    private Button btnChoiceA;
    private Button btnChoiceB;

    private Coroutine typingCoroutine;
    private string fullDialogueText = string.Empty;
    private bool isTyping;
    private bool choicesVisible;
    private bool showNextIndicator;
    private bool initialized;

    private Action choiceACallback;
    private Action choiceBCallback;

    public bool ChoicesVisible => choicesVisible;
    public bool IsTyping => isTyping;

    private void OnEnable()
    {
        Initialize();
    }

    private void Start()
    {
        // Reintento por si el UIDocument aún no había construido su árbol en OnEnable.
        if (!initialized)
            Initialize();
    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        if (rootScreen != null)
            rootScreen.UnregisterCallback<ClickEvent>(OnRootScreenClicked);

        if (btnChoiceA != null)
        {
            btnChoiceA.clicked -= HandleChoiceAClicked;
            btnChoiceA.UnregisterCallback<ClickEvent>(StopClickPropagation);
        }

        if (btnChoiceB != null)
        {
            btnChoiceB.clicked -= HandleChoiceBClicked;
            btnChoiceB.UnregisterCallback<ClickEvent>(StopClickPropagation);
        }

        StopTyping();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("[UIManager] No hay un componente UIDocument en este GameObject.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        if (root == null)
            return; // Start() volverá a intentarlo.

        rootScreen = root.Q<VisualElement>("RootScreen");
        characterContainer = root.Q<VisualElement>("CharacterContainer");
        choicesContainer = root.Q<VisualElement>("ChoicesContainer");
        dialoguePanel = root.Q<VisualElement>("DialoguePanel");
        speakerNameText = root.Q<Label>("SpeakerNameText");
        dialogueText = root.Q<Label>("DialogueText");
        nextIndicator = root.Q<VisualElement>("NextIndicator");
        btnChoiceA = root.Q<Button>("BtnChoiceA");
        btnChoiceB = root.Q<Button>("BtnChoiceB");

        if (rootScreen == null || characterContainer == null || choicesContainer == null ||
            dialoguePanel == null || speakerNameText == null || dialogueText == null ||
            nextIndicator == null || btnChoiceA == null || btnChoiceB == null)
        {
            Debug.LogError("[UIManager] El UXML no contiene todos los elementos esperados. " +
                           "Revisa VisualNovelUI.uxml (RootScreen, CharacterContainer, ChoicesContainer, " +
                           "DialoguePanel, SpeakerNameText, DialogueText, NextIndicator, BtnChoiceA, BtnChoiceB).");
            return;
        }

        rootScreen.RegisterCallback<ClickEvent>(OnRootScreenClicked);
        btnChoiceA.RegisterCallback<ClickEvent>(StopClickPropagation);
        btnChoiceB.RegisterCallback<ClickEvent>(StopClickPropagation);

        HideChoices();
        initialized = true;
    }

    /// <summary>
    /// Actualiza nombre, texto, retrato y fondo. El texto se revela con
    /// efecto máquina de escribir; un clic lo completa de inmediato.
    /// </summary>
    public void ShowDialogue(string speaker, string text, Sprite characterSprite, Sprite bgSprite)
    {
        if (!initialized)
            Initialize();

        if (!initialized)
            return;

        HideChoices();

        speakerNameText.text = string.IsNullOrEmpty(speaker) ? string.Empty : speaker;
        fullDialogueText = string.IsNullOrEmpty(text) ? string.Empty : text;

        SetBackground(rootScreen, bgSprite);
        SetBackground(characterContainer, characterSprite);
        characterContainer.style.display = characterSprite != null
            ? DisplayStyle.Flex
            : DisplayStyle.None;

        dialogueText.text = string.Empty;
        showNextIndicator = false;
        nextIndicator.style.display = DisplayStyle.None;

        StopTyping();

        if (useTypewriter && fullDialogueText.Length > 0)
            typingCoroutine = StartCoroutine(TypeDialogue());
        else
            FinishTyping();
    }

    /// <summary>
    /// Muestra los dos botones de decisión y oculta temporalmente la caja de
    /// diálogo, tal como pide el GDD para los Momentos Decisivos.
    /// </summary>
    public void ShowChoices(string choiceA, string choiceB, Action onChoiceA, Action onChoiceB)
    {
        if (!initialized)
            Initialize();

        if (!initialized)
            return;

        if (isTyping)
            CompleteTyping();

        btnChoiceA.text = string.IsNullOrEmpty(choiceA) ? "Opción A" : choiceA;
        btnChoiceB.text = string.IsNullOrEmpty(choiceB) ? "Opción B" : choiceB;

        btnChoiceA.clicked -= HandleChoiceAClicked;
        btnChoiceB.clicked -= HandleChoiceBClicked;

        choiceACallback = onChoiceA;
        choiceBCallback = onChoiceB;

        btnChoiceA.clicked += HandleChoiceAClicked;
        btnChoiceB.clicked += HandleChoiceBClicked;

        choicesContainer.style.display = DisplayStyle.Flex;
        dialoguePanel.style.display = DisplayStyle.None;
        nextIndicator.style.display = DisplayStyle.None;

        choicesVisible = true;
        showNextIndicator = false;
    }

    /// <summary>Oculta los botones y restaura la caja de diálogo.</summary>
    public void HideChoices()
    {
        if (choicesContainer == null || dialoguePanel == null)
            return;

        choicesContainer.style.display = DisplayStyle.None;
        dialoguePanel.style.display = DisplayStyle.Flex;
        choicesVisible = false;
    }

    private void OnRootScreenClicked(ClickEvent evt)
    {
        if (!initialized || choicesVisible)
            return;

        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.Advance();
    }

    private void HandleChoiceAClicked()
    {
        Action callback = choiceACallback;
        choiceACallback = null;
        choiceBCallback = null;

        HideChoices();
        callback?.Invoke();
    }

    private void HandleChoiceBClicked()
    {
        Action callback = choiceBCallback;
        choiceACallback = null;
        choiceBCallback = null;

        HideChoices();
        callback?.Invoke();
    }

    private static void StopClickPropagation(ClickEvent evt)
    {
        // Evita que el clic del botón llegue al RootScreen y avance dos veces.
        evt.StopPropagation();
    }

    private IEnumerator TypeDialogue()
    {
        isTyping = true;
        showNextIndicator = false;
        dialogueText.text = string.Empty;

        float secondsPerCharacter = 1f / Mathf.Max(1f, charactersPerSecond);

        for (int i = 0; i < fullDialogueText.Length; i++)
        {
            dialogueText.text += fullDialogueText[i];
            yield return new WaitForSecondsRealtime(secondsPerCharacter);
        }

        typingCoroutine = null;
        FinishTyping();
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        isTyping = false;
        dialogueText.text = fullDialogueText;
        showNextIndicator = true;
        nextIndicator.style.display = DisplayStyle.Flex;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    private static void SetBackground(VisualElement element, Sprite sprite)
    {
        if (element == null)
            return;

        element.style.backgroundImage = sprite != null
            ? new StyleBackground(sprite)
            : new StyleBackground(StyleKeyword.None);
    }

    private void Update()
    {
        if (!initialized || !showNextIndicator || nextIndicator == null)
            return;

        float wave = (Mathf.Sin(Time.unscaledTime * indicatorPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        nextIndicator.style.opacity = Mathf.Lerp(0.25f, 1f, wave);
    }
}

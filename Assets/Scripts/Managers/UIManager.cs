using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Aparición de personajes")]
    [Tooltip("Duración del fundido de opacidad al aparecer un retrato.")]
    [SerializeField, Min(0f)] private float characterFadeDuration = 0.55f;

    private VisualElement rootScreen;
    private VisualElement characterContainer;
    private VisualElement choicesContainer;
    private VisualElement dialoguePanel;
    private Label speakerNameText;
    private Label dialogueText;
    private VisualElement nextIndicator;
    private Button btnChoiceA;
    private Button btnChoiceB;

    private VisualElement flagStrip;
    private VisualElement flagImageA;
    private VisualElement flagImageB;
    private VisualElement flagImageC;
    private Label fpsText;

    private Coroutine typingCoroutine;
    private Coroutine characterFadeCoroutine;
    private string fullDialogueText = string.Empty;
    private bool isTyping;
    private bool choicesVisible;
    private bool showNextIndicator;
    private bool initialized;

    private float smoothedFps = 60f;
    private float fpsTimer;

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
        StopCharacterFade();
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
        flagStrip = root.Q<VisualElement>("FlagStrip");
        flagImageA = root.Q<VisualElement>("FlagImageA");
        flagImageB = root.Q<VisualElement>("FlagImageB");
        flagImageC = root.Q<VisualElement>("FlagImageC");
        fpsText = root.Q<Label>("FpsText");

        if (rootScreen == null || characterContainer == null || choicesContainer == null ||
            dialoguePanel == null || speakerNameText == null || dialogueText == null ||
            nextIndicator == null || btnChoiceA == null || btnChoiceB == null ||
            flagStrip == null || flagImageA == null || flagImageB == null ||
            flagImageC == null || fpsText == null)
        {
            Debug.LogError("[UIManager] El UXML no contiene todos los elementos esperados. " +
                           "Revisa VisualNovelUI.uxml (RootScreen, CharacterContainer, ChoicesContainer, " +
                           "DialoguePanel, SpeakerNameText, DialogueText, NextIndicator, BtnChoiceA, BtnChoiceB, " +
                           "FlagStrip, FlagImageA/B/C, FpsText).");
            return;
        }

        rootScreen.RegisterCallback<ClickEvent>(OnRootScreenClicked);
        btnChoiceA.RegisterCallback<ClickEvent>(StopClickPropagation);
        btnChoiceB.RegisterCallback<ClickEvent>(StopClickPropagation);

        HideChoices();
        ApplyFlags(null);
        initialized = true;
    }

    /// <summary>
    /// Muestra un nodo completo: nombre, texto, fondo, retrato lateral,
    /// banderas de la campaña y arranca el efecto máquina de escribir.
    /// </summary>
    public void ShowNode(StoryNode node, string speaker, string text)
    {
        if (!initialized)
            Initialize();

        if (!initialized)
            return;

        HideChoices();

        speakerNameText.text = string.IsNullOrEmpty(speaker) ? string.Empty : speaker;
        fullDialogueText = string.IsNullOrEmpty(text) ? string.Empty : text;

        SetBackground(rootScreen, node != null ? node.backgroundSprite : null);
        ShowCharacter(
            node != null ? node.characterSprite : null,
            node != null ? node.characterSide : CharacterSide.Izquierda);
        ApplyFlags(node != null ? node.flagSprites : null);

        StartTypewriter();
    }

    /// <summary>
    /// Sobrecarga de compatibilidad usada por herramientas antiguas.
    /// Apoya el retrato a la izquierda y no muestra banderas.
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
        ShowCharacter(characterSprite, CharacterSide.Izquierda);
        ApplyFlags(null);

        StartTypewriter();
    }

    private void StartTypewriter()
    {
        if (dialogueText == null || nextIndicator == null)
            return;

        dialogueText.text = string.Empty;
        showNextIndicator = false;
        nextIndicator.style.display = DisplayStyle.None;

        StopTyping();

        if (useTypewriter && fullDialogueText.Length > 0)
            typingCoroutine = StartCoroutine(TypeDialogue());
        else
            FinishTyping();
    }

    private void ShowCharacter(Sprite sprite, CharacterSide side)
    {
        StopCharacterFade();

        if (characterContainer == null)
            return;

        if (sprite == null)
        {
            characterContainer.style.display = DisplayStyle.None;
            characterContainer.style.opacity = 0f;
            return;
        }

        SetBackground(characterContainer, sprite);
        ApplyCharacterSide(side);

        characterContainer.style.display = DisplayStyle.Flex;
        characterContainer.style.opacity = 0f;

        if (characterFadeDuration <= 0f)
        {
            characterContainer.style.opacity = 1f;
            return;
        }

        characterFadeCoroutine = StartCoroutine(FadeInCharacter());
    }

    private void ApplyCharacterSide(CharacterSide side)
    {
        if (characterContainer == null)
            return;

        characterContainer.RemoveFromClassList("character-left");
        characterContainer.RemoveFromClassList("character-right");
        characterContainer.AddToClassList(
            side == CharacterSide.Derecha ? "character-right" : "character-left");
    }

    private void ApplyFlags(List<Sprite> flags)
    {
        VisualElement[] slots = { flagImageA, flagImageB, flagImageC };
        int count = flags != null ? flags.Count : 0;

        for (int i = 0; i < slots.Length; i++)
        {
            VisualElement slot = slots[i];
            if (slot == null)
                continue;

            bool visible = i < count && flags[i] != null;
            SetBackground(slot, visible ? flags[i] : null);
            slot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (flagStrip != null)
            flagStrip.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private IEnumerator FadeInCharacter()
    {
        float duration = Mathf.Max(0.01f, characterFadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            characterContainer.style.opacity = Mathf.SmoothStep(
                0f, 1f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        characterContainer.style.opacity = 1f;
        characterFadeCoroutine = null;
    }

    private void StopCharacterFade()
    {
        if (characterFadeCoroutine != null)
        {
            StopCoroutine(characterFadeCoroutine);
            characterFadeCoroutine = null;
        }
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
        if (!initialized)
            return;

        UpdateFps();

        if (!showNextIndicator || nextIndicator == null)
            return;

        float wave = (Mathf.Sin(Time.unscaledTime * indicatorPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        nextIndicator.style.opacity = Mathf.Lerp(0.25f, 1f, wave);
    }

    private void UpdateFps()
    {
        if (fpsText == null)
            return;

        float deltaTime = Time.unscaledDeltaTime;
        if (deltaTime > 0f)
            smoothedFps = Mathf.Lerp(smoothedFps, 1f / deltaTime, 0.12f);

        fpsTimer += deltaTime;

        if (fpsTimer < 0.2f)
            return;

        fpsTimer = 0f;
        fpsText.text = $"{Mathf.RoundToInt(smoothedFps)} FPS";
    }
}

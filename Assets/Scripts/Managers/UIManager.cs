using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Puente entre el grafo narrativo y la interfaz de UI Toolkit.
/// Requiere un UIDocument con el árbol VisualNovelUI.uxml.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    private const string MainMenuScene = "MainInicio";

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

    [Header("Cartel de acto")]
    [SerializeField, Min(0f)] private float actFadeDuration = 0.55f;
    [SerializeField, Min(0f)] private float actHoldDuration = 2f;

    // Núcleo
    private VisualElement rootScreen;
    private VisualElement backgroundLayer;
    private VisualElement characterContainer;
    private VisualElement choicesContainer;
    private VisualElement dialoguePanel;
    private Label speakerNameText;
    private Label dialogueText;
    private VisualElement nextIndicator;
    private Button btnChoiceA;
    private Button btnChoiceB;
    private Button btnChoiceC;

    private VisualElement flagStrip;
    private VisualElement flagImageA;
    private VisualElement flagImageB;
    private VisualElement flagImageC;
    private Label fpsText;

    // HUD
    private Button btnSaveGame;
    private Button btnPause;
    private Label saveToast;

    // Cartel de acto
    private VisualElement actTransitionOverlay;
    private Label actNumberLabel;
    private Label actTitleLabel;

    // Opciones / pausa
    private VisualElement pausePanel;
    private Slider musicSliderGame;
    private Slider voiceSliderGame;
    private Button btnResume;
    private Button btnRestartGame;
    private Button btnDeleteSaveGame;
    private Button btnBackToMenuGame;

    // Cierre
    private VisualElement endingPanel;
    private Label endingTitleLabel;
    private Label endingSubtitleLabel;
    private Button btnEndingMenu;
    private Button btnEndingRestart;

    private Coroutine typingCoroutine;
    private Coroutine characterFadeCoroutine;
    private Coroutine backgroundFadeCoroutine;
    private Coroutine nodeSequenceCoroutine;
    private Coroutine toastCoroutine;

    private StoryNode currentNode;
    private string fullDialogueText = string.Empty;
    private float activeCharactersPerSecond = 45f;

    private bool isTyping;
    private bool typingFinished;
    private bool choicesVisible;
    private bool showNextIndicator;
    private bool actTransitionVisible;
    private bool modalOpen;
    private bool endingVisible;
    private bool initialized;

    private float smoothedFps = 60f;
    private float fpsTimer;

    private Action<int> choiceCallback;

    public bool ChoicesVisible => choicesVisible;
    public bool IsTyping => isTyping;

    /// <summary>
    /// Verdadero mientras el jugador no debe poder avanzar: cartel de acto,
    /// texto escribiéndose, voz sonando, decisiones o paneles modales abiertos.
    /// </summary>
    public bool IsInputBlocked
    {
        get
        {
            if (!initialized || actTransitionVisible || modalOpen || endingVisible || choicesVisible)
                return true;

            if (isTyping || !typingFinished)
                return true;

            return AudioManager.Instance != null && AudioManager.Instance.IsVoicePlaying;
        }
    }

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

        if (btnChoiceC != null)
        {
            btnChoiceC.clicked -= HandleChoiceCClicked;
            btnChoiceC.UnregisterCallback<ClickEvent>(StopClickPropagation);
        }

        if (btnSaveGame != null) btnSaveGame.clicked -= HandleSaveClicked;
        if (btnPause != null) btnPause.clicked -= OpenPausePanel;
        if (btnResume != null) btnResume.clicked -= ClosePausePanel;
        if (btnRestartGame != null) btnRestartGame.clicked -= HandleRestartCampaign;
        if (btnDeleteSaveGame != null) btnDeleteSaveGame.clicked -= HandleDeleteSave;
        if (btnBackToMenuGame != null) btnBackToMenuGame.clicked -= HandleBackToMenu;
        if (btnEndingMenu != null) btnEndingMenu.clicked -= HandleBackToMenu;
        if (btnEndingRestart != null) btnEndingRestart.clicked -= HandleEndingRestart;

        if (musicSliderGame != null) musicSliderGame.UnregisterValueChangedCallback(OnMusicVolumeChanged);
        if (voiceSliderGame != null) voiceSliderGame.UnregisterValueChangedCallback(OnVoiceVolumeChanged);

        StopTyping();
        StopCharacterFade();
        StopBackgroundFade();
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
        backgroundLayer = root.Q<VisualElement>("BackgroundLayer");
        characterContainer = root.Q<VisualElement>("CharacterContainer");
        choicesContainer = root.Q<VisualElement>("ChoicesContainer");
        dialoguePanel = root.Q<VisualElement>("DialoguePanel");
        speakerNameText = root.Q<Label>("SpeakerNameText");
        dialogueText = root.Q<Label>("DialogueText");
        nextIndicator = root.Q<VisualElement>("NextIndicator");
        btnChoiceA = root.Q<Button>("BtnChoiceA");
        btnChoiceB = root.Q<Button>("BtnChoiceB");
        btnChoiceC = root.Q<Button>("BtnChoiceC");
        flagStrip = root.Q<VisualElement>("FlagStrip");
        flagImageA = root.Q<VisualElement>("FlagImageA");
        flagImageB = root.Q<VisualElement>("FlagImageB");
        flagImageC = root.Q<VisualElement>("FlagImageC");
        fpsText = root.Q<Label>("FpsText");

        btnSaveGame = root.Q<Button>("BtnSaveGame");
        btnPause = root.Q<Button>("BtnPause");
        saveToast = root.Q<Label>("SaveToast");

        actTransitionOverlay = root.Q<VisualElement>("ActTransitionOverlay");
        actNumberLabel = root.Q<Label>("ActNumberLabel");
        actTitleLabel = root.Q<Label>("ActTitleLabel");

        pausePanel = root.Q<VisualElement>("PausePanel");
        musicSliderGame = root.Q<Slider>("MusicSliderGame");
        voiceSliderGame = root.Q<Slider>("VoiceSliderGame");
        btnResume = root.Q<Button>("BtnResume");
        btnRestartGame = root.Q<Button>("BtnRestartGame");
        btnDeleteSaveGame = root.Q<Button>("BtnDeleteSaveGame");
        btnBackToMenuGame = root.Q<Button>("BtnBackToMenuGame");

        endingPanel = root.Q<VisualElement>("EndingPanel");
        endingTitleLabel = root.Q<Label>("EndingTitleLabel");
        endingSubtitleLabel = root.Q<Label>("EndingSubtitleLabel");
        btnEndingMenu = root.Q<Button>("BtnEndingMenu");
        btnEndingRestart = root.Q<Button>("BtnEndingRestart");

        if (rootScreen == null || backgroundLayer == null || characterContainer == null ||
            choicesContainer == null || dialoguePanel == null || speakerNameText == null ||
            dialogueText == null || nextIndicator == null || btnChoiceA == null ||
            btnChoiceB == null || btnChoiceC == null || flagStrip == null ||
            flagImageA == null || flagImageB == null || flagImageC == null || fpsText == null ||
            btnSaveGame == null || btnPause == null || saveToast == null ||
            actTransitionOverlay == null || actNumberLabel == null || actTitleLabel == null ||
            pausePanel == null || musicSliderGame == null || voiceSliderGame == null ||
            btnResume == null || btnRestartGame == null || btnDeleteSaveGame == null ||
            btnBackToMenuGame == null || endingPanel == null || endingTitleLabel == null ||
            endingSubtitleLabel == null || btnEndingMenu == null || btnEndingRestart == null)
        {
            Debug.LogError("[UIManager] El UXML no contiene todos los elementos esperados. " +
                           "Revisa VisualNovelUI.uxml (capas de fondo, HUD, cartel de acto, " +
                           "opciones y panel de cierre).");
            return;
        }

        rootScreen.RegisterCallback<ClickEvent>(OnRootScreenClicked);

        ProtectButton(btnChoiceA);
        ProtectButton(btnChoiceB);
        ProtectButton(btnChoiceC);
        ProtectButton(btnSaveGame);
        ProtectButton(btnPause);
        ProtectButton(btnResume);
        ProtectButton(btnRestartGame);
        ProtectButton(btnDeleteSaveGame);
        ProtectButton(btnBackToMenuGame);
        ProtectButton(btnEndingMenu);
        ProtectButton(btnEndingRestart);

        btnChoiceA.clicked += HandleChoiceAClicked;
        btnChoiceB.clicked += HandleChoiceBClicked;
        btnChoiceC.clicked += HandleChoiceCClicked;

        btnSaveGame.clicked += HandleSaveClicked;
        btnPause.clicked += OpenPausePanel;
        btnResume.clicked += ClosePausePanel;
        btnRestartGame.clicked += HandleRestartCampaign;
        btnDeleteSaveGame.clicked += HandleDeleteSave;
        btnBackToMenuGame.clicked += HandleBackToMenu;
        btnEndingMenu.clicked += HandleBackToMenu;
        btnEndingRestart.clicked += HandleEndingRestart;

        musicSliderGame.RegisterValueChangedCallback(OnMusicVolumeChanged);
        voiceSliderGame.RegisterValueChangedCallback(OnVoiceVolumeChanged);

        actTransitionOverlay.style.display = DisplayStyle.None;
        pausePanel.style.display = DisplayStyle.None;
        endingPanel.style.display = DisplayStyle.None;
        saveToast.style.display = DisplayStyle.None;

        HideChoices();
        HideCharacterImmediate();
        ApplyFlags(null);
        SyncVolumeSliders();

        initialized = true;
    }

    private void ProtectButton(Button button)
    {
        if (button == null)
            return;

        button.RegisterCallback<ClickEvent>(StopClickPropagation);
    }

    /// <summary>
    /// Muestra un nodo completo. Si corresponde, primero aparece el cartel de
    /// acto; recién después se revela escenario, personaje, diálogo y voz.
    /// </summary>
    public void ShowNode(
        StoryNode node,
        string speaker,
        string text,
        bool showActCard,
        Action onShown = null)
    {
        if (!initialized)
            Initialize();

        if (!initialized)
            return;

        currentNode = node;
        fullDialogueText = string.IsNullOrEmpty(text) ? string.Empty : text;
        speakerNameText.text = string.IsNullOrEmpty(speaker) ? string.Empty : speaker;

        HideChoices();
        HideEndingPanel();
        ClosePausePanelImmediate();

        StopTyping();
        StopCharacterFade();
        StopBackgroundFade();

        isTyping = false;
        typingFinished = false;
        showNextIndicator = false;

        if (nextIndicator != null)
            nextIndicator.style.display = DisplayStyle.None;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (nodeSequenceCoroutine != null)
        {
            StopCoroutine(nodeSequenceCoroutine);
            nodeSequenceCoroutine = null;
            HideActOverlayImmediate();
        }

        nodeSequenceCoroutine = StartCoroutine(ShowNodeSequence(node, showActCard, onShown));
    }

    /// <summary>
    /// Sobrecarga de compatibilidad usada por herramientas antiguas.
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

        SetBackgroundFade(bgSprite);
        ShowCharacter(characterSprite, CharacterSide.Izquierda);
        ApplyFlags(null);

        StartTypewriter(null);
    }

    private IEnumerator ShowNodeSequence(StoryNode node, bool showActCard, Action onShown)
    {
        if (showActCard && node != null)
            yield return ActTransitionRoutine(node);
        else
            HideActOverlayImmediate();

        // Limpia por completo el retrato anterior antes de revelar el nuevo.
        yield return FadeOutCharacterRoutine();

        ApplyNodeVisuals(node);

        onShown?.Invoke();
        StartTypewriter(node != null ? node.voiceAudio : null);

        nodeSequenceCoroutine = null;
    }

    private void ApplyNodeVisuals(StoryNode node)
    {
        SetBackgroundFade(node != null ? node.backgroundSprite : null);
        ShowCharacter(
            node != null ? node.characterSprite : null,
            node != null ? node.characterSide : CharacterSide.Izquierda);
        ApplyFlags(node != null ? node.flagSprites : null);
        dialogueText.text = string.Empty;
    }

    private IEnumerator ActTransitionRoutine(StoryNode node)
    {
        actTransitionVisible = true;
        showNextIndicator = false;

        if (nextIndicator != null)
            nextIndicator.style.display = DisplayStyle.None;

        actNumberLabel.text = $"ACTO {ToRoman(node.actNumber)}";
        actTitleLabel.text = string.IsNullOrEmpty(node.actTitle)
            ? $"Acto {node.actNumber}"
            : node.actTitle;

        actTransitionOverlay.style.display = DisplayStyle.Flex;
        actTransitionOverlay.style.opacity = 0f;

        yield return FadeElement(actTransitionOverlay, 0f, 1f, actFadeDuration);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, actHoldDuration));
        yield return FadeElement(actTransitionOverlay, 1f, 0f, actFadeDuration);

        actTransitionOverlay.style.display = DisplayStyle.None;
        actTransitionVisible = false;
    }

    private void HideActOverlayImmediate()
    {
        actTransitionVisible = false;

        if (actTransitionOverlay != null)
        {
            actTransitionOverlay.style.display = DisplayStyle.None;
            actTransitionOverlay.style.opacity = 0f;
        }
    }

    private static string ToRoman(int value)
    {
        string[] romans = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };
        int index = Mathf.Clamp(value - 1, 0, romans.Length - 1);
        return romans[index];
    }

    // ---------------------------------------------------------------------
    // Texto
    // ---------------------------------------------------------------------

    private void StartTypewriter(AudioClip voiceClip)
    {
        if (dialogueText == null || nextIndicator == null)
            return;

        dialogueText.text = string.Empty;
        showNextIndicator = false;
        typingFinished = false;
        nextIndicator.style.display = DisplayStyle.None;

        activeCharactersPerSecond = charactersPerSecond;

        // Si hay voz, el texto se escribe al ritmo de la narración.
        if (voiceClip != null && voiceClip.length > 0.05f)
        {
            activeCharactersPerSecond = Mathf.Max(
                15f,
                fullDialogueText.Length / voiceClip.length);
        }

        StopTyping();

        if (useTypewriter && fullDialogueText.Length > 0)
            typingCoroutine = StartCoroutine(TypeDialogue());
        else
            FinishTyping();
    }

    private IEnumerator TypeDialogue()
    {
        isTyping = true;
        typingFinished = false;
        dialogueText.text = string.Empty;

        float secondsPerCharacter = 1f / Mathf.Max(1f, activeCharactersPerSecond);

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

        isTyping = false;
        typingFinished = true;
        dialogueText.text = fullDialogueText;
        TryRevealAdvance();
    }

    private void FinishTyping()
    {
        isTyping = false;
        typingFinished = true;

        if (dialogueText != null)
            dialogueText.text = fullDialogueText;

        TryRevealAdvance();
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

    /// <summary>
    /// Solo deja avanzar cuando el texto y la voz terminaron por completo.
    /// </summary>
    private void TryRevealAdvance()
    {
        if (!initialized)
            return;

        if (choicesVisible || actTransitionVisible || modalOpen || endingVisible)
        {
            HideNextIndicator();
            return;
        }

        bool voicePlaying = AudioManager.Instance != null && AudioManager.Instance.IsVoicePlaying;

        if (!typingFinished || isTyping || voicePlaying)
        {
            HideNextIndicator();
            return;
        }

        if (currentNode != null && currentNode.IsFinalNode)
        {
            HideNextIndicator();
            ShowEndingPanel();
            return;
        }

        RevealNextIndicator();
    }

    private void RevealNextIndicator()
    {
        if (nextIndicator == null)
            return;

        showNextIndicator = true;
        nextIndicator.style.display = DisplayStyle.Flex;
    }

    private void HideNextIndicator()
    {
        showNextIndicator = false;

        if (nextIndicator != null)
            nextIndicator.style.display = DisplayStyle.None;
    }

    // ---------------------------------------------------------------------
    // Fondo y personaje
    // ---------------------------------------------------------------------

    private void SetBackgroundFade(Sprite sprite)
    {
        StopBackgroundFade();

        if (backgroundLayer == null)
            return;

        SetBackground(backgroundLayer, sprite);

        if (sprite == null)
        {
            backgroundLayer.style.opacity = 1f;
            return;
        }

        backgroundLayer.style.opacity = 0f;
        backgroundFadeCoroutine = StartCoroutine(FadeElement(
            backgroundLayer, 0f, 1f, Mathf.Max(0.01f, characterFadeDuration)));
    }

    private void ShowCharacter(Sprite sprite, CharacterSide side)
    {
        StopCharacterFade();

        if (characterContainer == null)
            return;

        if (sprite == null)
        {
            HideCharacterImmediate();
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

    private void HideCharacterImmediate()
    {
        StopCharacterFade();

        if (characterContainer == null)
            return;

        characterContainer.style.opacity = 0f;
        characterContainer.style.display = DisplayStyle.None;
        SetBackground(characterContainer, null);
    }

    private IEnumerator FadeOutCharacterRoutine()
    {
        if (characterContainer == null ||
            characterContainer.style.display == DisplayStyle.None ||
            characterContainer.resolvedStyle.display == DisplayStyle.None)
        {
            HideCharacterImmediate();
            yield break;
        }

        float start = characterContainer.resolvedStyle.opacity;
        if (float.IsNaN(start))
            start = 1f;

        yield return FadeElement(characterContainer, start, 0f, 0.22f);

        HideCharacterImmediate();
    }

    private IEnumerator FadeInCharacter()
    {
        yield return FadeElement(characterContainer, 0f, 1f, characterFadeDuration);
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

    private void StopBackgroundFade()
    {
        if (backgroundFadeCoroutine != null)
        {
            StopCoroutine(backgroundFadeCoroutine);
            backgroundFadeCoroutine = null;
        }
    }

    private static IEnumerator FadeElement(VisualElement element, float from, float to, float duration)
    {
        if (element == null)
            yield break;

        if (duration <= 0f)
        {
            element.style.opacity = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            element.style.opacity = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        element.style.opacity = to;
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

    // ---------------------------------------------------------------------
    // Decisiones
    // ---------------------------------------------------------------------

    public void ShowChoices(string choiceA, string choiceB, Action onChoiceA, Action onChoiceB)
    {
        ShowChoices(
            new List<string> { choiceA, choiceB },
            index =>
            {
                if (index == 0)
                    onChoiceA?.Invoke();
                else if (index == 1)
                    onChoiceB?.Invoke();
            });
    }

    /// <summary>
    /// Muestra de una a tres opciones y oculta todo lo demás: caja de diálogo,
    /// personaje, banderas e indicador.
    /// </summary>
    public void ShowChoices(IReadOnlyList<string> choiceTexts, Action<int> onChoice)
    {
        if (!initialized)
            Initialize();

        if (!initialized || choiceTexts == null || choiceTexts.Count == 0)
            return;

        if (isTyping)
            CompleteTyping();

        Button[] buttons = { btnChoiceA, btnChoiceB, btnChoiceC };
        string[] fallbacks = { "Opción A", "Opción B", "Opción C" };
        int count = Mathf.Clamp(choiceTexts.Count, 1, buttons.Length);

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
                continue;

            bool visible = i < count;
            button.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            if (visible)
            {
                string text = choiceTexts[i];
                button.text = string.IsNullOrEmpty(text) ? fallbacks[i] : text;
            }
        }

        choiceCallback = onChoice;

        choicesContainer.style.display = DisplayStyle.Flex;
        dialoguePanel.style.display = DisplayStyle.None;
        HideNextIndicator();
        HideCharacterImmediate();

        if (flagStrip != null)
            flagStrip.style.display = DisplayStyle.None;

        // Durante la decisión solo quedan el escenario y los botones.
        if (fpsText != null)
            fpsText.style.display = DisplayStyle.None;
        if (btnSaveGame != null)
            btnSaveGame.style.display = DisplayStyle.None;
        if (btnPause != null)
            btnPause.style.display = DisplayStyle.None;

        choicesVisible = true;
    }

    /// <summary>Oculta los botones y restaura la caja de diálogo.</summary>
    public void HideChoices()
    {
        if (choicesContainer == null || dialoguePanel == null)
            return;

        choicesContainer.style.display = DisplayStyle.None;
        dialoguePanel.style.display = DisplayStyle.Flex;

        if (fpsText != null)
            fpsText.style.display = DisplayStyle.Flex;
        if (btnSaveGame != null)
            btnSaveGame.style.display = DisplayStyle.Flex;
        if (btnPause != null)
            btnPause.style.display = DisplayStyle.Flex;

        choicesVisible = false;
        choiceCallback = null;
    }

    // ---------------------------------------------------------------------
    // Interacción
    // ---------------------------------------------------------------------

    private void OnRootScreenClicked(ClickEvent evt)
    {
        if (!initialized)
            return;

        if (choicesVisible || actTransitionVisible || modalOpen || endingVisible)
            return;

        // Anti-skip: mientras haya voz o texto en curso, el clic no avanza.
        if (isTyping || !typingFinished)
            return;

        if (AudioManager.Instance != null && AudioManager.Instance.IsVoicePlaying)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.Advance();
    }

    private void HandleChoiceAClicked() => HandleChoiceClicked(0);
    private void HandleChoiceBClicked() => HandleChoiceClicked(1);
    private void HandleChoiceCClicked() => HandleChoiceClicked(2);

    private void HandleChoiceClicked(int index)
    {
        Action<int> callback = choiceCallback;
        choiceCallback = null;

        choicesVisible = false;
        choicesContainer.style.display = DisplayStyle.None;
        dialoguePanel.style.display = DisplayStyle.Flex;

        callback?.Invoke(index);
    }

    private static void StopClickPropagation(ClickEvent evt)
    {
        // Evita que el clic del botón llegue al RootScreen y avance dos veces.
        evt.StopPropagation();
    }

    // ---------------------------------------------------------------------
    // Guardado, opciones y cierre
    // ---------------------------------------------------------------------

    private void HandleSaveClicked()
    {
        bool saved = GameManager.Instance != null && GameManager.Instance.SaveCurrentGame();
        ShowToast(saved ? "Partida guardada" : "No se pudo guardar");
    }

    private void ShowToast(string message)
    {
        if (saveToast == null)
            return;

        if (toastCoroutine != null)
            StopCoroutine(toastCoroutine);

        saveToast.text = message;
        saveToast.style.display = DisplayStyle.Flex;
        saveToast.style.opacity = 1f;
        toastCoroutine = StartCoroutine(ToastRoutine());
    }

    private IEnumerator ToastRoutine()
    {
        yield return new WaitForSecondsRealtime(1.4f);
        yield return FadeElement(saveToast, 1f, 0f, 0.5f);

        saveToast.style.display = DisplayStyle.None;
        toastCoroutine = null;
    }

    private void OpenPausePanel()
    {
        if (!initialized || pausePanel == null)
            return;

        SyncVolumeSliders();
        modalOpen = true;
        HideNextIndicator();
        pausePanel.style.display = DisplayStyle.Flex;
    }

    private void ClosePausePanel()
    {
        modalOpen = false;

        if (pausePanel != null)
            pausePanel.style.display = DisplayStyle.None;
    }

    private void ClosePausePanelImmediate()
    {
        ClosePausePanel();
    }

    private void HandleRestartCampaign()
    {
        ClosePausePanel();
        HideEndingPanel();

        if (GameManager.Instance != null)
            GameManager.Instance.RestartCampaign();
    }

    private void HandleEndingRestart()
    {
        HideEndingPanel();

        if (GameManager.Instance != null)
            GameManager.Instance.RestartCampaign();
    }

    private void HandleDeleteSave()
    {
        SaveManager.DeleteSave();
        ShowToast("Datos guardados eliminados");
    }

    private void HandleBackToMenu()
    {
        ClosePausePanel();
        HideEndingPanel();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopVoice();
            AudioManager.Instance.PlayMenuMusic();
        }

        SceneManager.LoadScene(MainMenuScene);
    }

    private void SyncVolumeSliders()
    {
        if (musicSliderGame != null)
            musicSliderGame.SetValueWithoutNotify(AudioManager.GetSavedMusicVolume());

        if (voiceSliderGame != null)
            voiceSliderGame.SetValueWithoutNotify(AudioManager.GetSavedVoiceVolume());
    }

    private void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(evt.newValue);
        else
            PlayerPrefs.SetFloat(AudioManager.MusicVolumeKey, evt.newValue);
    }

    private void OnVoiceVolumeChanged(ChangeEvent<float> evt)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVoiceVolume(evt.newValue);
        else
            PlayerPrefs.SetFloat(AudioManager.VoiceVolumeKey, evt.newValue);
    }

    private void ShowEndingPanel()
    {
        if (endingVisible || endingPanel == null)
            return;

        endingVisible = true;

        string nodeName = currentNode != null ? currentNode.name : string.Empty;

        if (nodeName.Contains("33"))
        {
            endingTitleLabel.text = "Final: Desastre Prematuro";
            endingSubtitleLabel.text =
                "La campaña se quebró antes de tiempo. El Perú no llegó a defender su costa.";
        }
        else if (nodeName.Contains("34"))
        {
            endingTitleLabel.text = "Final: Historia Alternativa";
            endingSubtitleLabel.text =
                "Una victoria que la historia no registró, pero que cambió el destino del Pacífico.";
        }
        else
        {
            endingTitleLabel.text = "Final: Historia Real";
            endingSubtitleLabel.text =
                "La resistencia se sostuvo hasta el final. El Perú sobrevivió para contarlo.";
        }

        endingPanel.style.display = DisplayStyle.Flex;
        HideNextIndicator();
    }

    private void HideEndingPanel()
    {
        endingVisible = false;

        if (endingPanel != null)
            endingPanel.style.display = DisplayStyle.None;
    }

    private static void SetBackground(VisualElement element, Sprite sprite)
    {
        if (element == null)
            return;

        element.style.backgroundImage = sprite != null
            ? new StyleBackground(sprite)
            : new StyleBackground(StyleKeyword.None);
    }

    // ---------------------------------------------------------------------
    // Bucle
    // ---------------------------------------------------------------------

    private void Update()
    {
        if (!initialized)
            return;

        UpdateFps();

        // La voz puede terminar después del texto: revisa cada frame si ya
        // se puede mostrar el indicador o el panel de cierre.
        if (typingFinished && !isTyping && !choicesVisible && !actTransitionVisible &&
            !modalOpen && !endingVisible)
        {
            TryRevealAdvance();
        }

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

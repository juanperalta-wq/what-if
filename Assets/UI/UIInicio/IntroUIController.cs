using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Controla la UI de inicio (MainInicio) mediante UI Toolkit.
/// Flujo: Título -> Continuar / Nueva campaña -> Ingreso de nombre -> Opciones
/// -> Salto temporal -> What_If_Peru.
/// </summary>
public class IntroUIController : MonoBehaviour
{
    [Header("Escena de juego")]
    [SerializeField] private string gameSceneName = "What_If_Peru";

    [Header("Animación de salto")]
    [SerializeField, Min(0.5f)] private float transitionDuration = 3.2f;

    private UIDocument uiDocument;
    private VisualElement root;

    // Vistas
    private VisualElement mainMenuView;
    private VisualElement nameInputView;
    private VisualElement optionsView;
    private VisualElement introAnimationView;

    // Elementos
    private Button continueButton;
    private TextField playerNameField;
    private Label introText;
    private Label introChronoText;
    private Slider musicSlider;
    private Slider voiceSlider;
    private Slider textSpeedSlider;
    private Toggle fullscreenToggle;
    private Label deleteSaveStatus;

    private bool transitioning;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[IntroUIController] Falta el componente UIDocument.");
            return;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[IntroUIController] El UIDocument no generó un árbol visual.");
            return;
        }

        CacheReferences();
        BindEvents();
        LoadPreferences();
        UpdateContinueButtonState();

        ShowView(mainMenuView);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void CacheReferences()
    {
        mainMenuView = root.Q<VisualElement>("MainMenuView");
        nameInputView = root.Q<VisualElement>("NameInputView");
        optionsView = root.Q<VisualElement>("OptionsView");
        introAnimationView = root.Q<VisualElement>("IntroAnimationView");

        continueButton = root.Q<Button>("ContinueButton");
        playerNameField = root.Q<TextField>("PlayerNameField");
        introText = root.Q<Label>("IntroText");
        introChronoText = root.Q<Label>("IntroChronoText");
        musicSlider = root.Q<Slider>("MusicSlider");
        voiceSlider = root.Q<Slider>("VoiceSlider");
        textSpeedSlider = root.Q<Slider>("TextSpeedSlider");
        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        deleteSaveStatus = root.Q<Label>("DeleteSaveStatus");
    }

    private void BindEvents()
    {
        Button startButton = root.Q<Button>("StartButton");
        Button optionsButton = root.Q<Button>("OptionsButton");
        Button exitButton = root.Q<Button>("ExitButton");
        Button confirmNameButton = root.Q<Button>("ConfirmNameButton");
        Button backFromNameButton = root.Q<Button>("BackFromNameButton");
        Button backFromOptionsButton = root.Q<Button>("BackFromOptionsButton");
        Button deleteSaveButton = root.Q<Button>("DeleteSaveButton");

        if (continueButton != null) continueButton.clicked += ContinueSavedGame;
        if (startButton != null) startButton.clicked += StartNewCampaign;
        if (optionsButton != null) optionsButton.clicked += () => ShowView(optionsView);
        if (exitButton != null) exitButton.clicked += QuitGame;
        if (confirmNameButton != null) confirmNameButton.clicked += StartTimeJump;
        if (backFromNameButton != null) backFromNameButton.clicked += () => ShowView(mainMenuView);
        if (backFromOptionsButton != null) backFromOptionsButton.clicked += OnBackFromOptions;
        if (deleteSaveButton != null) deleteSaveButton.clicked += OnDeleteSaveClicked;

        if (musicSlider != null) musicSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);
        if (voiceSlider != null) voiceSlider.RegisterValueChangedCallback(OnVoiceVolumeChanged);
    }

    private void UnbindEvents()
    {
        if (musicSlider != null) musicSlider.UnregisterValueChangedCallback(OnMusicVolumeChanged);
        if (voiceSlider != null) voiceSlider.UnregisterValueChangedCallback(OnVoiceVolumeChanged);
    }

    private void OnBackFromOptions()
    {
        SavePreferences();
        ShowView(mainMenuView);
    }

    private void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(evt.newValue);

        GameSessionManager session = GameSessionManager.Instance;
        if (session != null)
            session.musicVolume = evt.newValue;
    }

    private void OnVoiceVolumeChanged(ChangeEvent<float> evt)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVoiceVolume(evt.newValue);

        GameSessionManager session = GameSessionManager.Instance;
        if (session != null)
            session.sfxVolume = evt.newValue;
    }

    private void OnDeleteSaveClicked()
    {
        SaveManager.DeleteSave();
        UpdateContinueButtonState();

        if (deleteSaveStatus != null)
        {
            deleteSaveStatus.text = "Datos guardados eliminados.";
            deleteSaveStatus.style.display = DisplayStyle.Flex;
        }
    }

    private void UpdateContinueButtonState()
    {
        if (continueButton == null)
            return;

        bool hasSave = SaveManager.HasSaveData();
        continueButton.SetEnabled(hasSave);
        continueButton.style.opacity = hasSave ? 1f : 0.45f;
    }

    private void LoadPreferences()
    {
        GameSessionManager session = GameSessionManager.Instance;

        if (playerNameField != null && session != null && !string.IsNullOrEmpty(session.playerName))
            playerNameField.value = session.playerName;

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(AudioManager.GetSavedMusicVolume());

        if (voiceSlider != null)
            voiceSlider.SetValueWithoutNotify(AudioManager.GetSavedVoiceVolume());

        if (textSpeedSlider != null && session != null)
            textSpeedSlider.SetValueWithoutNotify(session.textSpeed);

        if (fullscreenToggle != null && session != null)
            fullscreenToggle.SetValueWithoutNotify(session.fullscreen);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(AudioManager.GetSavedMusicVolume());
            AudioManager.Instance.SetVoiceVolume(AudioManager.GetSavedVoiceVolume());
        }

        if (deleteSaveStatus != null)
            deleteSaveStatus.style.display = DisplayStyle.None;
    }

    private void SavePreferences()
    {
        GameSessionManager session = GameSessionManager.Instance;
        if (session == null)
            return;

        if (musicSlider != null) session.musicVolume = musicSlider.value;
        if (voiceSlider != null) session.sfxVolume = voiceSlider.value;
        if (textSpeedSlider != null) session.textSpeed = textSpeedSlider.value;
        if (fullscreenToggle != null) session.fullscreen = fullscreenToggle.value;
    }

    private void ContinueSavedGame()
    {
        if (transitioning)
            return;

        if (!SaveManager.TryLoad(out SaveData data))
        {
            UpdateContinueButtonState();
            return;
        }

        GameSessionManager session = GameSessionManager.Instance;
        if (session != null)
        {
            session.loadFromSave = true;
            session.pendingNodeName = data.currentNodeName;
            session.SetPlayerName(data.playerName);
        }

        SavePreferences();
        SceneManager.LoadScene(gameSceneName);
    }

    private void StartNewCampaign()
    {
        GameSessionManager session = GameSessionManager.Instance;
        if (session != null)
        {
            session.loadFromSave = false;
            session.pendingNodeName = null;
        }

        ShowView(nameInputView);
    }

    private void ShowView(VisualElement view)
    {
        if (mainMenuView != null) mainMenuView.style.display = DisplayStyle.None;
        if (nameInputView != null) nameInputView.style.display = DisplayStyle.None;
        if (optionsView != null) optionsView.style.display = DisplayStyle.None;
        if (introAnimationView != null) introAnimationView.style.display = DisplayStyle.None;

        if (view != null)
            view.style.display = DisplayStyle.Flex;
    }

    private void StartTimeJump()
    {
        if (transitioning)
            return;

        string name = playerNameField != null ? playerNameField.value : string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            // Foco en el campo y feedback visual si está vacío.
            playerNameField?.Focus();
            return;
        }

        GameSessionManager session = GameSessionManager.Instance;
        if (session != null)
        {
            session.SetPlayerName(name);
            session.loadFromSave = false;
        }

        SavePreferences();
        ShowView(introAnimationView);
        StartCoroutine(TimeJumpSequence());
    }

    private IEnumerator TimeJumpSequence()
    {
        transitioning = true;

        // Parpadeo del cronómetro y del año.
        for (int i = 0; i < 6; i++)
        {
            if (introChronoText != null) introChronoText.style.opacity = 0.15f;
            if (introText != null) introText.style.opacity = 0.3f;
            yield return new WaitForSecondsRealtime(0.22f);

            if (introChronoText != null) introChronoText.style.opacity = 1f;
            if (introText != null) introText.style.opacity = 1f;
            yield return new WaitForSecondsRealtime(0.22f);
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, transitionDuration - 2.64f));

        transitioning = false;
        SceneManager.LoadScene(gameSceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

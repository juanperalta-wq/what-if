using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Controla la UI de inicio (MainInicio) mediante UI Toolkit.
/// Flujo: Título -> Ingreso de nombre -> Opciones -> Salto temporal -> What_If_Peru.
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
    private TextField playerNameField;
    private Label introText;
    private Label introChronoText;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Slider textSpeedSlider;
    private Toggle fullscreenToggle;

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

        ShowView(mainMenuView);
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

        playerNameField = root.Q<TextField>("PlayerNameField");
        introText = root.Q<Label>("IntroText");
        introChronoText = root.Q<Label>("IntroChronoText");
        musicSlider = root.Q<Slider>("MusicSlider");
        sfxSlider = root.Q<Slider>("SfxSlider");
        textSpeedSlider = root.Q<Slider>("TextSpeedSlider");
        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
    }

    private void BindEvents()
    {
        Button startButton = root.Q<Button>("StartButton");
        Button optionsButton = root.Q<Button>("OptionsButton");
        Button exitButton = root.Q<Button>("ExitButton");
        Button confirmNameButton = root.Q<Button>("ConfirmNameButton");
        Button backFromNameButton = root.Q<Button>("BackFromNameButton");
        Button backFromOptionsButton = root.Q<Button>("BackFromOptionsButton");

        if (startButton != null) startButton.clicked += () => ShowView(nameInputView);
        if (optionsButton != null) optionsButton.clicked += () => ShowView(optionsView);
        if (exitButton != null) exitButton.clicked += QuitGame;
        if (confirmNameButton != null) confirmNameButton.clicked += StartTimeJump;
        if (backFromNameButton != null) backFromNameButton.clicked += () => ShowView(mainMenuView);
        if (backFromOptionsButton != null) backFromOptionsButton.clicked += OnBackFromOptions;
    }

    private void OnBackFromOptions()
    {
        SavePreferences();
        ShowView(mainMenuView);
    }

    private void UnbindEvents()
    {
        // La interfaz se destruye al cambiar de escena; no es necesario desregistrar,
        // pero se mantiene limpio por seguridad.
    }

    private void LoadPreferences()
    {
        GameSessionManager session = GameSessionManager.Instance;
        if (session == null)
            return;

        if (playerNameField != null && !string.IsNullOrEmpty(session.playerName))
            playerNameField.value = session.playerName;

        if (musicSlider != null) musicSlider.value = session.musicVolume;
        if (sfxSlider != null) sfxSlider.value = session.sfxVolume;
        if (textSpeedSlider != null) textSpeedSlider.value = session.textSpeed;
        if (fullscreenToggle != null) fullscreenToggle.value = session.fullscreen;
    }

    private void SavePreferences()
    {
        GameSessionManager session = GameSessionManager.Instance;
        if (session == null)
            return;

        if (musicSlider != null) session.musicVolume = musicSlider.value;
        if (sfxSlider != null) session.sfxVolume = sfxSlider.value;
        if (textSpeedSlider != null) session.textSpeed = textSpeedSlider.value;
        if (fullscreenToggle != null) session.fullscreen = fullscreenToggle.value;
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
            session.SetPlayerName(name);

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

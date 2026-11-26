using System.Collections;
using UnityEngine;

/// <summary>
/// Audio global de "¿Y si...? Perú".
/// Mantiene dos canales de música en bucle para hacer crossfade suave entre
/// actos (y el menú), y un canal dedicado a voces / narración que nunca usa
/// bucle. Es un singleton persistente: sobrevive al cambio de escena.
/// </summary>
[DefaultExecutionOrder(-20)]
public class AudioManager : MonoBehaviour
{
    public const string MusicVolumeKey = "WhatIf_MusicVolume";
    public const string VoiceVolumeKey = "WhatIf_VoiceVolume";

    private const string MenuMusicResourceName = "Audio/Musica/MainInicio";
    private const string Act1ResourceName = "Audio/Musica/ACTO I Iquique y las Raíces del Conflicto";
    private const string Act2ResourceName = "Audio/Musica/ACTO II La Campaña Naval (Iquique y Angamos)";
    private const string Act3ResourceName = "Audio/Musica/ACTO III La Campaña de Tarapacá";
    private const string Act4ResourceName = "Audio/Musica/ACTO IV Tacna y el Morro de Arica";
    private const string Act5ResourceName = "Audio/Musica/ACTO V Defensa de Lima, la Resistencia y los Tres Finales";

    public static AudioManager Instance { get; private set; }

    [Header("Pistas de música")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip act1Music;
    [SerializeField] private AudioClip act2Music;
    [SerializeField] private AudioClip act3Music;
    [SerializeField] private AudioClip act4Music;
    [SerializeField] private AudioClip act5Music;

    [Header("Canales")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource voiceSource;

    [Header("Mezcla")]
    [Tooltip("Duración del crossfade entre pistas musicales.")]
    [SerializeField, Min(0f)] private float crossfadeDuration = 1.2f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.9f;

    private Coroutine crossfadeCoroutine;
    private AudioSource activeMusicSource;
    private int currentAct = int.MinValue;

    /// <summary>Verdadero mientras se reproduce una voz o narración.</summary>
    public bool IsVoicePlaying => voiceSource != null && voiceSource.isPlaying && voiceSource.clip != null;

    public float MusicVolume => musicVolume;
    public float VoiceVolume => voiceVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        LoadClipFallbacks();
        LoadVolumes();

        activeMusicSource = musicSourceA;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Crea los AudioSource si la escena no los tenía configurados.</summary>
    private void EnsureSources()
    {
        if (musicSourceA == null)
            musicSourceA = CreateSource("Music Source A", true);
        if (musicSourceB == null)
            musicSourceB = CreateSource("Music Source B", true);
        if (voiceSource == null)
            voiceSource = CreateSource("Voice Source", false);

        musicSourceA.loop = true;
        musicSourceB.loop = true;
        voiceSource.loop = false;
    }

    private AudioSource CreateSource(string sourceName, bool loop)
    {
        var child = new GameObject(sourceName);
        child.transform.SetParent(transform, false);

        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    private void LoadVolumes()
    {
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume));
        voiceVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(VoiceVolumeKey, voiceVolume));

        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        if (musicSourceA != null) musicSourceA.volume = musicVolume;
        if (musicSourceB != null) musicSourceB.volume = musicVolume;
        if (voiceSource != null) voiceSource.volume = voiceVolume;
    }

    /// <summary>
    /// Si las referencias no llegaron desde la escena, intenta cargarlas desde
    /// Resources (carpeta opcional). Así el sistema nunca queda mudo.
    /// </summary>
    private void LoadClipFallbacks()
    {
        if (menuMusic == null) menuMusic = Resources.Load<AudioClip>(MenuMusicResourceName);
        if (act1Music == null) act1Music = Resources.Load<AudioClip>(Act1ResourceName);
        if (act2Music == null) act2Music = Resources.Load<AudioClip>(Act2ResourceName);
        if (act3Music == null) act3Music = Resources.Load<AudioClip>(Act3ResourceName);
        if (act4Music == null) act4Music = Resources.Load<AudioClip>(Act4ResourceName);
        if (act5Music == null) act5Music = Resources.Load<AudioClip>(Act5ResourceName);
    }

    public AudioClip GetActClip(int actNumber)
    {
        switch (actNumber)
        {
            case 1: return act1Music;
            case 2: return act2Music;
            case 3: return act3Music;
            case 4: return act4Music;
            case 5: return act5Music;
            default: return null;
        }
    }

    /// <summary>Música del menú principal.</summary>
    public void PlayMenuMusic()
    {
        if (currentAct == 0 &&
            activeMusicSource != null &&
            activeMusicSource.clip == menuMusic &&
            activeMusicSource.isPlaying)
        {
            return;
        }

        currentAct = 0;
        CrossfadeTo(menuMusic);
    }

    /// <summary>
    /// Cambia la música solo cuando cambia el acto. Repetir el mismo acto no
    /// reinicia la pista.
    /// </summary>
    public void PlayMusicForAct(int actNumber)
    {
        if (actNumber <= 0)
        {
            PlayMenuMusic();
            return;
        }

        if (actNumber == currentAct)
            return;

        currentAct = actNumber;
        AudioClip clip = GetActClip(actNumber);

        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] No hay pista asignada para el Acto {actNumber}.");
            return;
        }

        CrossfadeTo(clip);
    }

    private void CrossfadeTo(AudioClip clip)
    {
        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        if (clip == null)
        {
            StartCoroutine(FadeOutAll());
            return;
        }

        crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(clip));
    }

    private IEnumerator CrossfadeRoutine(AudioClip clip)
    {
        AudioSource from = activeMusicSource;
        AudioSource to = from == musicSourceA ? musicSourceB : musicSourceA;

        if (from == null || to == null)
        {
            if (musicSourceA != null)
            {
                musicSourceA.clip = clip;
                musicSourceA.volume = musicVolume;
                musicSourceA.Play();
                activeMusicSource = musicSourceA;
            }

            crossfadeCoroutine = null;
            yield break;
        }

        to.Stop();
        to.clip = clip;
        to.volume = 0f;
        to.Play();

        float duration = Mathf.Max(0.01f, crossfadeDuration);
        float elapsed = 0f;
        float fromStart = from.volume;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            to.volume = Mathf.Lerp(0f, musicVolume, t);
            if (from.isPlaying)
                from.volume = Mathf.Lerp(fromStart, 0f, t);

            yield return null;
        }

        to.volume = musicVolume;
        from.Stop();
        from.clip = null;
        from.volume = 0f;

        activeMusicSource = to;
        crossfadeCoroutine = null;
    }

    private IEnumerator FadeOutAll()
    {
        float duration = Mathf.Max(0.01f, crossfadeDuration);
        float elapsed = 0f;
        float volumeA = musicSourceA != null ? musicSourceA.volume : 0f;
        float volumeB = musicSourceB != null ? musicSourceB.volume : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (musicSourceA != null) musicSourceA.volume = Mathf.Lerp(volumeA, 0f, t);
            if (musicSourceB != null) musicSourceB.volume = Mathf.Lerp(volumeB, 0f, t);

            yield return null;
        }

        if (musicSourceA != null) { musicSourceA.Stop(); musicSourceA.clip = null; }
        if (musicSourceB != null) { musicSourceB.Stop(); musicSourceB.clip = null; }

        crossfadeCoroutine = null;
    }

    /// <summary>
    /// Reproduce la voz del nodo. Detiene de inmediato cualquier voz anterior
    /// para que nunca se solapen dos narraciones.
    /// </summary>
    public void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null)
            return;

        voiceSource.Stop();

        if (clip == null)
            return;

        voiceSource.clip = clip;
        voiceSource.loop = false;
        voiceSource.volume = voiceVolume;
        voiceSource.Play();
    }

    public void StopVoice()
    {
        if (voiceSource == null)
            return;

        voiceSource.Stop();
        voiceSource.clip = null;
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        if (musicSourceA != null) musicSourceA.volume = musicVolume;
        if (musicSourceB != null) musicSourceB.volume = musicVolume;

        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();
    }

    public void SetVoiceVolume(float value)
    {
        voiceVolume = Mathf.Clamp01(value);

        if (voiceSource != null) voiceSource.volume = voiceVolume;

        PlayerPrefs.SetFloat(VoiceVolumeKey, voiceVolume);
        PlayerPrefs.Save();
    }

    public static float GetSavedMusicVolume(float fallback = 0.8f)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, fallback));
    }

    public static float GetSavedVoiceVolume(float fallback = 0.9f)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(VoiceVolumeKey, fallback));
    }
}

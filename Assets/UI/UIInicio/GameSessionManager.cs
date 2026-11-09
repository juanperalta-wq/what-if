using UnityEngine;

/// <summary>
/// Puente de sesión persistente entre la escena de inicio (MainInicio) y la
/// escena de juego (What_If_Peru). Sobrevive al cambio de escena y transporta
/// el nombre del jugador y las preferencias de audio/texto.
/// </summary>
public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [Header("Jugador")]
    public string playerName;

    [Header("Preferencias")]
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0.01f, 0.1f)] public float textSpeed = 0.05f;
    public bool fullscreen = true;

    private const string PlayerPrefsNameKey = "WhatIf_PlayerName";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Restaura el nombre recordado de la última sesión.
        if (PlayerPrefs.HasKey(PlayerPrefsNameKey))
            playerName = PlayerPrefs.GetString(PlayerPrefsNameKey);
    }

    /// <summary>Guarda el nombre del jugador y lo recuerda para la próxima vez.</summary>
    public void SetPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return;

        playerName = newName.Trim();
        PlayerPrefs.SetString(PlayerPrefsNameKey, playerName);
        PlayerPrefs.Save();
    }
}

using System;
using System.IO;
using UnityEngine;

/// <summary>Datos mínimos de una partida guardada.</summary>
[Serializable]
public class SaveData
{
    public string currentNodeName;
    public string playerName;
    public int actNumber = 1;
}

/// <summary>
/// Guardado/carga de la partida en JSON dentro de
/// Application.persistentDataPath/savegame.json, con respaldo en PlayerPrefs.
/// No usa inventarios: el estado es el nodo actual, el nombre y el acto.
/// </summary>
public static class SaveManager
{
    public const string FileName = "savegame.json";
    public const string PlayerPrefsKey = "WhatIf_SaveGame";

    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>Guarda la partida usando el nodo indicado.</summary>
    public static bool SaveGame(StoryNode node, string playerName)
    {
        if (node == null)
        {
            Debug.LogWarning("[SaveManager] No se puede guardar: nodo nulo.");
            return false;
        }

        return SaveGame(new SaveData
        {
            currentNodeName = node.name,
            playerName = playerName,
            actNumber = node.actNumber
        });
    }

    public static bool SaveGame(SaveData data)
    {
        if (data == null || string.IsNullOrEmpty(data.currentNodeName))
            return false;

        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(SavePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SaveManager] No se pudo escribir {SavePath}: {exception.Message}");
        }

        // Respaldo en PlayerPrefs por si el archivo no es accesible.
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[SaveManager] Partida guardada en '{data.currentNodeName}' (Acto {data.actNumber}).");
        return true;
    }

    public static bool HasSaveData()
    {
        return TryLoad(out _);
    }

    /// <summary>Intenta leer el archivo y, si falla, el respaldo de PlayerPrefs.</summary>
    public static bool TryLoad(out SaveData data)
    {
        data = null;

        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveManager] No se pudo leer {SavePath}: {exception.Message}");
            }
        }

        if ((data == null || string.IsNullOrEmpty(data.currentNodeName)) &&
            PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(PlayerPrefsKey));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveManager] Respaldo de PlayerPrefs inválido: {exception.Message}");
            }
        }

        if (data == null || string.IsNullOrEmpty(data.currentNodeName))
        {
            data = null;
            return false;
        }

        if (data.actNumber < 1)
            data.actNumber = 1;

        return true;
    }

    /// <summary>Elimina el archivo, el respaldo y deja el botón Continuar apagado.</summary>
    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SaveManager] No se pudo borrar {SavePath}: {exception.Message}");
        }

        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();

        Debug.Log("[SaveManager] Datos guardados eliminados.");
    }
}

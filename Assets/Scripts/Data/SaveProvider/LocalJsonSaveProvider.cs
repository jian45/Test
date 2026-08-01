using UnityEngine;

/// <summary>正式存储：JsonUtility 序列化到 PlayerPrefs</summary>
public class LocalJsonSaveProvider : IDataSaveProvider
{
    protected virtual string SaveKey
    {
        get { return "cat_cafe_player_data"; }
    }

    public virtual void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public PlayerData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return null;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<PlayerData>(json);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("[DataMgr][Save] Failed to parse local save json: " + exception.Message);
            return null;
        }
    }

    public bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}

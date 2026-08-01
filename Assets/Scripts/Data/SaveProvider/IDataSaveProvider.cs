/// <summary>存档读写接口：可替换实现（PlayerPrefs / 微信本地存储 / 云端）</summary>
public interface IDataSaveProvider
{
    void Save(PlayerData data);
    PlayerData Load();
    bool HasSave();
    void DeleteSave();
}

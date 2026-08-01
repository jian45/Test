using UnityEngine;

/// <summary>Editor 专用存储，key 带 _editor_mock 后缀，不干扰正式存档</summary>
public class EditorMockSaveProvider : LocalJsonSaveProvider
{
    protected override string SaveKey
    {
        get { return "cat_cafe_player_data_editor_mock"; }
    }

    public override void Save(PlayerData data)
    {
        base.Save(data);
#if UNITY_EDITOR
        Debug.Log("[DataMgr][Save][EditorMock] Saved PlayerPrefs json with key " + SaveKey);
#endif
    }
}

using System;
using System.Collections.Generic;

/// <summary>猫咪数据：已解锁、预览、运行时状态列表</summary>
public partial class PlayerData
{
    public List<string> unlockedCatIds = new List<string>();
    public List<string> previewCatIds = new List<string>();
    public List<PlayerCatRuntimeData> catRuntimeList = new List<PlayerCatRuntimeData>();
}

[Serializable]
public class PlayerCatRuntimeData
{
    public string catId;
    public string residenceState;
    public int heartCount;
    public int serviceCount;
    public long lastServiceAt;
}
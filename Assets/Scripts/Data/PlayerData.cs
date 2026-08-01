using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>玩家存档数据（核心数据类，按模块 partial 拆分）</summary>
[Serializable]
public partial class PlayerData
{
    [FormerlySerializedAs("saveVersion")]
    public int dataVersion;
    public string userId;
    public string sessionId;
    public long createTime;
    public long lastSaveTime;
}
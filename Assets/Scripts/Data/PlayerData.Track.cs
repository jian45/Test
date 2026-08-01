using System;
using System.Collections.Generic;

/// <summary>埋点标记：首次行为标记、事件计数器</summary>
public partial class PlayerData
{
    public bool hasTrackAppStart;
    public bool hasTrackFirstCatAdopt;
    public bool hasTrackFirstMerge;
    public bool hasTrackFirstOrderSubmit;
    public bool hasTrackFirstCatService;
    public bool hasTrackFirstRepairNode;
    public List<PlayerTrackCounterData> trackCounters = new List<PlayerTrackCounterData>();
}

[Serializable]
public class PlayerTrackCounterData
{
    public string eventName;
    public int count;
}
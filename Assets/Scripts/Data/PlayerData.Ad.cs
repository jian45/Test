using System;
using System.Collections.Generic;

/// <summary>广告数据：广告完成次数、广告位信息、播放追踪</summary>
public partial class PlayerData
{
    public int totalRewardedAdCompleteCount;
    public int mockSettlementDoubleRepairMaterial;
    public string lastProducerRefillTargetId;
    public List<PlayerAdPlacementData> adPlacements = new List<PlayerAdPlacementData>();
    public List<PlayerAdTraceData> adTraces = new List<PlayerAdTraceData>();
}

[Serializable]
public class PlayerAdPlacementData
{
    public string placementId;
    public int watchedCount;
    public int watchedCountInCurrentBatch;
    public long lastWatchedAt;
}

[Serializable]
public class PlayerAdTraceData
{
    public string traceId;
    public string requestId;
    public string placementId;
    public string status;
    public bool rewardGranted;
    public string promisedReward;
    public string grantedReward;
    public long createdAt;
    public long completedAt;
    public string errorCode;
}
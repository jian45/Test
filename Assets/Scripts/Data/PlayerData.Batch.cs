using System.Collections.Generic;

/// <summary>批次状态：当前批次、最大完成批次、结算标记等</summary>
public partial class PlayerData
{
    public bool isNewUser;
    public int currentBatchId;
    public int maxCompletedBatchId;
    public string currentSceneId;
    public string experimentGroup;
    public string currentBatchState;
    public long currentBatchStartedAt;
    public long currentBatchCompletedAt;
    public bool isSettlementShown;
    public int currentBatchAdWatchedCount;
    public int currentBatchMinEmptyCellCount;
    public List<string> completedOrderIdsInCurrentBatch = new List<string>();
}

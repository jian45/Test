using System;
using System.Collections.Generic;

/// <summary>批次控制：开始/完成一个游戏批次，管理批次状态</summary>
public static partial class DataMgr
{
    public static void Batch_Start(int batchId)
    {
        EnsureData();

        int safeBatchId = Math.Max(1, batchId);
        data.currentBatchId = safeBatchId;
        data.currentBatchState = "Playing";
        data.currentBatchStartedAt = NowUnixMs();
        data.currentBatchCompletedAt = 0;
        data.isSettlementShown = false;
        data.currentBatchAdWatchedCount = 0;
        data.completedOrderIdsInCurrentBatch.Clear();

        Ad_ResetPlacementBatchWatchCounts();
        Producer_InitProducersForBatch(safeBatchId);
        Board_UpdateCurrentBatchMinEmptyCellCount();

        Track_Event("batch_start", new Dictionary<string, object>
        {
            { "batchId", safeBatchId },
            { "orderCount", data.activeOrders.Count },
            { "minEmptyCellCount", data.currentBatchMinEmptyCellCount }
        });

        Save();
    }

    public static void Batch_Complete(int batchId, string rewardSummary)
    {
        EnsureData();

        int safeBatchId = Math.Max(1, batchId);
        data.currentBatchId = safeBatchId;
        data.currentBatchState = "Settlement";
        data.currentBatchCompletedAt = NowUnixMs();
        data.isSettlementShown = true;
        data.maxCompletedBatchId = Math.Max(data.maxCompletedBatchId, safeBatchId);
        data.isNewUser = false;

        Track_Event("batch_complete", new Dictionary<string, object>
        {
            { "batchId", safeBatchId },
            { "durationSeconds", Batch_GetDurationSeconds() },
            { "rewardSummary", rewardSummary },
            { "adWatchedCount", data.currentBatchAdWatchedCount }
        });

        Save();
    }

    [Obsolete("Use Batch_Start.")]
    public static void StartBatch(int batchId)
    {
        Batch_Start(batchId);
    }

    [Obsolete("Use Batch_Complete.")]
    public static void CompleteBatch(int batchId, string rewardSummary)
    {
        Batch_Complete(batchId, rewardSummary);
    }

    private static void Batch_InitData()
    {
        data.isNewUser = true;
        data.currentBatchId = 1;
        data.maxCompletedBatchId = 0;
        data.currentSceneId = "cat_cafe_home";
        data.experimentGroup = "default";
        data.currentBatchState = "NotStarted";
        data.currentBatchStartedAt = 0;
        data.currentBatchCompletedAt = 0;
        data.isSettlementShown = false;
        data.currentBatchAdWatchedCount = 0;
        data.currentBatchMinEmptyCellCount = DefaultBoardWidth * DefaultBoardHeight;
        data.completedOrderIdsInCurrentBatch = new List<string>();
    }

    private static void Batch_InitDataForArchiveUpgrade()
    {
        if (data.currentBatchId < 1)
        {
            data.currentBatchId = 1;
        }

        if (string.IsNullOrEmpty(data.currentSceneId))
        {
            data.currentSceneId = "cat_cafe_home";
        }

        if (string.IsNullOrEmpty(data.experimentGroup))
        {
            data.experimentGroup = "default";
        }

        if (string.IsNullOrEmpty(data.currentBatchState))
        {
            data.currentBatchState = "NotStarted";
        }

        if (data.currentBatchMinEmptyCellCount <= 0)
        {
            data.currentBatchMinEmptyCellCount = DefaultBoardWidth * DefaultBoardHeight;
        }

        if (data.completedOrderIdsInCurrentBatch == null)
        {
            data.completedOrderIdsInCurrentBatch = new List<string>();
        }
    }

    private static void Batch_FixInvalidDataAfterLoad()
    {
        Batch_InitDataForArchiveUpgrade();

        if (data.maxCompletedBatchId < 0)
        {
            data.maxCompletedBatchId = 0;
        }

        if (data.currentBatchAdWatchedCount < 0)
        {
            data.currentBatchAdWatchedCount = 0;
        }
    }

    private static long Batch_GetDurationSeconds()
    {
        if (data.currentBatchStartedAt <= 0 || data.currentBatchCompletedAt <= data.currentBatchStartedAt)
        {
            return 0;
        }

        return (data.currentBatchCompletedAt - data.currentBatchStartedAt) / 1000;
    }
}

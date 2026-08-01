using System;
using System.Collections.Generic;

/// <summary>修复节点：解锁节点、消耗资源完成修复、埋点</summary>
public static partial class DataMgr
{
    public static void Repair_UnlockNode(string repairNodeId)
    {
        EnsureData();

        if (AddUniqueString(data.unlockedRepairNodeIds, repairNodeId))
        {
            Save();
        }
    }

    public static bool Repair_TryCompleteNode(string repairNodeId, int costCatCoin, int costRepairMaterial)
    {
        EnsureData();

        int safeCostCatCoin = costCatCoin < 0 ? 0 : costCatCoin;
        int safeCostRepairMaterial = costRepairMaterial < 0 ? 0 : costRepairMaterial;

        if (string.IsNullOrEmpty(repairNodeId) || !data.unlockedRepairNodeIds.Contains(repairNodeId))
        {
            return false;
        }

        if (data.completedRepairNodeIds.Contains(repairNodeId))
        {
            return true;
        }

        if (data.catCoin < safeCostCatCoin || data.repairMaterial < safeCostRepairMaterial)
        {
            return false;
        }

        int coinBefore = data.catCoin;
        int materialBefore = data.repairMaterial;
        data.catCoin -= safeCostCatCoin;
        data.repairMaterial -= safeCostRepairMaterial;

        if (safeCostCatCoin > 0)
        {
            Track_Event("resource_spend", new Dictionary<string, object>
            {
                { "sink", "repair:" + repairNodeId },
                { "resourceType", "catCoin" },
                { "amount", safeCostCatCoin },
                { "beforeBalance", coinBefore },
                { "afterBalance", data.catCoin }
            });
        }

        if (safeCostRepairMaterial > 0)
        {
            Track_Event("resource_spend", new Dictionary<string, object>
            {
                { "sink", "repair:" + repairNodeId },
                { "resourceType", "repairMaterial" },
                { "amount", safeCostRepairMaterial },
                { "beforeBalance", materialBefore },
                { "afterBalance", data.repairMaterial }
            });
        }

        data.completedRepairNodeIds.Add(repairNodeId);
        data.repairRecords.Add(new PlayerRepairNodeRecord
        {
            repairNodeId = repairNodeId,
            completedAt = NowUnixMs(),
            costCatCoin = safeCostCatCoin,
            costRepairMaterial = safeCostRepairMaterial
        });

        string eventName = data.hasTrackFirstRepairNode ? "repair_node_complete" : "first_repair_node";
        data.hasTrackFirstRepairNode = true;

        Track_Event(eventName, new Dictionary<string, object>
        {
            { "repairNodeId", repairNodeId },
            { "costSummary", "catCoin=" + safeCostCatCoin + ",repairMaterial=" + safeCostRepairMaterial },
            { "batchId", data.currentBatchId }
        });

        Save();
        return true;
    }

    [Obsolete("Use Repair_UnlockNode.")]
    public static void UnlockRepairNode(string repairNodeId)
    {
        Repair_UnlockNode(repairNodeId);
    }

    [Obsolete("Use Repair_TryCompleteNode.")]
    public static bool TryCompleteRepairNode(string repairNodeId, int costCatCoin, int costRepairMaterial)
    {
        return Repair_TryCompleteNode(repairNodeId, costCatCoin, costRepairMaterial);
    }

    private static void Repair_InitData()
    {
        data.unlockedRepairNodeIds = new List<string>();
        data.completedRepairNodeIds = new List<string>();
        data.repairRecords = new List<PlayerRepairNodeRecord>();
        AddUniqueString(data.unlockedRepairNodeIds, "repair_r1_signboard");
    }

    private static void Repair_InitDataForArchiveUpgrade()
    {
        if (data.unlockedRepairNodeIds == null)
        {
            data.unlockedRepairNodeIds = new List<string>();
        }

        if (data.completedRepairNodeIds == null)
        {
            data.completedRepairNodeIds = new List<string>();
        }

        if (data.repairRecords == null)
        {
            data.repairRecords = new List<PlayerRepairNodeRecord>();
        }

        AddUniqueString(data.unlockedRepairNodeIds, "repair_r1_signboard");
    }

    private static void Repair_FixInvalidDataAfterLoad()
    {
        Repair_InitDataForArchiveUpgrade();
    }
}

using System;
using System.Collections.Generic;

/// <summary>修复数据：已解锁/完成的修复节点列表、修复记录</summary>
public partial class PlayerData
{
    public List<string> unlockedRepairNodeIds = new List<string>();
    public List<string> completedRepairNodeIds = new List<string>();
    public List<PlayerRepairNodeRecord> repairRecords = new List<PlayerRepairNodeRecord>();
}

[Serializable]
public class PlayerRepairNodeRecord
{
    public string repairNodeId;
    public string area;
    public long completedAt;
    public int costCatCoin;
    public int costRepairMaterial;
}
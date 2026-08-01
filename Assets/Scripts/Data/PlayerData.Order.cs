using System;
using System.Collections.Generic;

/// <summary>订单运行时数据：活跃订单列表、已提交物品</summary>
public partial class PlayerData
{
    public List<PlayerOrderRuntimeData> activeOrders = new List<PlayerOrderRuntimeData>();
}

[Serializable]
public class PlayerOrderRuntimeData
{
    public string orderId;
    public string customerId;
    public long startedAt;
    public long completedAt;
    public bool isComplete;
    public List<PlayerSubmittedItemData> requiredItems = new List<PlayerSubmittedItemData>();
    public List<PlayerSubmittedItemData> submittedItems = new List<PlayerSubmittedItemData>();
}

[Serializable]
public class PlayerSubmittedItemData
{
    public string itemId;
    public int count;
}
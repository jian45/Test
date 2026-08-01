using System;
using System.Collections.Generic;

/// <summary>订单运行时状态：添加活跃订单、提交物品、标记完成</summary>
public static partial class DataMgr
{
    public static PlayerOrderRuntimeData Order_AddActive(string orderId, string customerId, string requiredItemId = "", int requiredCount = 1)
    {
        EnsureData();

        PlayerOrderRuntimeData order = Order_GetOrCreate(orderId);
        order.customerId = customerId;

        if (!string.IsNullOrEmpty(requiredItemId) && requiredCount > 0)
        {
            Order_AddOrIncreaseItem(order.requiredItems, requiredItemId, requiredCount);
        }

        Save();
        return order;
    }

    public static PlayerOrderRuntimeData Order_GetActive(string orderId)
    {
        EnsureData();
        return Order_Find(orderId);
    }

    public static void Order_SubmitItem(string orderId, string itemId, int count)
    {
        EnsureData();

        if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(itemId) || count <= 0)
        {
            return;
        }

        PlayerOrderRuntimeData order = Order_GetOrCreate(orderId);
        Order_AddOrIncreaseItem(order.submittedItems, itemId, count);

        if (!order.isComplete && Order_IsComplete(order))
        {
            Order_MarkComplete(order);
        }

        Save();
    }

    public static bool Order_Complete(string orderId)
    {
        EnsureData();

        PlayerOrderRuntimeData order = Order_Find(orderId);
        if (order == null)
        {
            return false;
        }

        if (!order.isComplete)
        {
            Order_MarkComplete(order);
            Save();
        }

        return true;
    }

    [Obsolete("Use Order_AddActive.")]
    public static PlayerOrderRuntimeData AddActiveOrder(string orderId, string customerId, string requiredItemId = "", int requiredCount = 1)
    {
        return Order_AddActive(orderId, customerId, requiredItemId, requiredCount);
    }

    [Obsolete("Use Order_GetActive.")]
    public static PlayerOrderRuntimeData GetActiveOrder(string orderId)
    {
        return Order_GetActive(orderId);
    }

    [Obsolete("Use Order_SubmitItem.")]
    public static void SubmitOrderItem(string orderId, string itemId, int count)
    {
        Order_SubmitItem(orderId, itemId, count);
    }

    [Obsolete("Use Order_Complete.")]
    public static bool CompleteOrder(string orderId)
    {
        return Order_Complete(orderId);
    }

    private static void Order_InitData()
    {
        data.activeOrders = new List<PlayerOrderRuntimeData>();
    }

    private static void Order_InitDataForArchiveUpgrade()
    {
        if (data.activeOrders == null)
        {
            data.activeOrders = new List<PlayerOrderRuntimeData>();
        }
    }

    private static void Order_FixInvalidDataAfterLoad()
    {
        Order_InitDataForArchiveUpgrade();

        for (int i = 0; i < data.activeOrders.Count; i++)
        {
            PlayerOrderRuntimeData order = data.activeOrders[i];
            if (order == null)
            {
                order = new PlayerOrderRuntimeData();
                data.activeOrders[i] = order;
            }

            if (order.requiredItems == null)
            {
                order.requiredItems = new List<PlayerSubmittedItemData>();
            }

            if (order.submittedItems == null)
            {
                order.submittedItems = new List<PlayerSubmittedItemData>();
            }
        }
    }

    private static PlayerOrderRuntimeData Order_GetOrCreate(string orderId)
    {
        PlayerOrderRuntimeData order = Order_Find(orderId);
        if (order != null)
        {
            return order;
        }

        order = new PlayerOrderRuntimeData
        {
            orderId = orderId,
            startedAt = NowUnixMs(),
            requiredItems = new List<PlayerSubmittedItemData>(),
            submittedItems = new List<PlayerSubmittedItemData>()
        };
        data.activeOrders.Add(order);
        return order;
    }

    private static PlayerOrderRuntimeData Order_Find(string orderId)
    {
        Order_InitDataForArchiveUpgrade();

        for (int i = 0; i < data.activeOrders.Count; i++)
        {
            PlayerOrderRuntimeData order = data.activeOrders[i];
            if (order != null && order.orderId == orderId)
            {
                return order;
            }
        }

        return null;
    }

    private static void Order_AddOrIncreaseItem(List<PlayerSubmittedItemData> items, string itemId, int count)
    {
        PlayerSubmittedItemData item = Order_FindItem(items, itemId);
        if (item == null)
        {
            items.Add(new PlayerSubmittedItemData { itemId = itemId, count = count });
        }
        else
        {
            item.count += count;
        }
    }

    private static PlayerSubmittedItemData Order_FindItem(List<PlayerSubmittedItemData> items, string itemId)
    {
        if (items == null)
        {
            return null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].itemId == itemId)
            {
                return items[i];
            }
        }

        return null;
    }

    private static bool Order_IsComplete(PlayerOrderRuntimeData order)
    {
        if (order.requiredItems == null || order.requiredItems.Count == 0)
        {
            return order.submittedItems != null && order.submittedItems.Count > 0;
        }

        for (int i = 0; i < order.requiredItems.Count; i++)
        {
            PlayerSubmittedItemData required = order.requiredItems[i];
            if (required == null || string.IsNullOrEmpty(required.itemId) || required.count <= 0)
            {
                continue;
            }

            PlayerSubmittedItemData submitted = Order_FindItem(order.submittedItems, required.itemId);
            if (submitted == null || submitted.count < required.count)
            {
                return false;
            }
        }

        return true;
    }

    private static void Order_MarkComplete(PlayerOrderRuntimeData order)
    {
        order.isComplete = true;
        order.completedAt = NowUnixMs();
        AddUniqueString(data.completedOrderIdsInCurrentBatch, order.orderId);

        string eventName = data.hasTrackFirstOrderSubmit ? "order_submit" : "first_order_submit";
        data.hasTrackFirstOrderSubmit = true;

        Track_Event(eventName, new Dictionary<string, object>
        {
            { "orderId", order.orderId },
            { "submittedItems", "from_runtime" },
            { "batchId", data.currentBatchId }
        });
    }
}

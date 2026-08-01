using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>资源管理：猫币(catCoin)、爱心(heart)、修复材料(repairMaterial)的增删改查</summary>
public static partial class DataMgr
{
    public static void Resource_Add(string resourceType, int amount, string source)
    {
        EnsureData();

        if (amount <= 0)
        {
            Debug.LogWarning("[DataMgr][Resource] Resource_Add ignored non-positive amount: " + amount);
            return;
        }

        if (!IsKnownResource(resourceType))
        {
            Debug.LogWarning("[DataMgr][Resource] Resource_Add ignored unknown resource type: " + resourceType);
            return;
        }

        int before = Resource_Get(resourceType);
        Resource_Set(resourceType, before + amount);
        int after = Resource_Get(resourceType);

        Track_Event("resource_gain", new Dictionary<string, object>
        {
            { "source", source },
            { "resourceType", resourceType },
            { "amount", amount },
            { "beforeBalance", before },
            { "afterBalance", after }
        });

        Save();
    }

    public static bool Resource_TrySpend(string resourceType, int amount, string sink)
    {
        EnsureData();

        if (amount <= 0)
        {
            return true;
        }

        int before = Resource_Get(resourceType);
        if (before < amount)
        {
            return false;
        }

        Resource_Set(resourceType, before - amount);
        int after = Resource_Get(resourceType);

        Track_Event("resource_spend", new Dictionary<string, object>
        {
            { "sink", sink },
            { "resourceType", resourceType },
            { "amount", amount },
            { "beforeBalance", before },
            { "afterBalance", after }
        });

        Save();
        return true;
    }

    public static int Resource_Get(string resourceType)
    {
        EnsureData();

        if (resourceType == "catCoin")
        {
            return data.catCoin;
        }

        if (resourceType == "heart")
        {
            return data.heart;
        }

        if (resourceType == "repairMaterial")
        {
            return data.repairMaterial;
        }

        Debug.LogWarning("[DataMgr][Resource] Unknown resource type: " + resourceType);
        return 0;
    }

    [Obsolete("Use Resource_Add.")]
    public static void AddResource(string resourceType, int amount, string source)
    {
        Resource_Add(resourceType, amount, source);
    }

    [Obsolete("Use Resource_TrySpend.")]
    public static bool TrySpendResource(string resourceType, int amount, string sink)
    {
        return Resource_TrySpend(resourceType, amount, sink);
    }

    [Obsolete("Use Resource_Get.")]
    public static int GetResource(string resourceType)
    {
        return Resource_Get(resourceType);
    }

    private static void Resource_InitData()
    {
        data.catCoin = 0;
        data.heart = 0;
        data.repairMaterial = 0;
    }

    private static void Resource_InitDataForArchiveUpgrade()
    {
        Resource_FixInvalidDataAfterLoad();
    }

    private static void Resource_FixInvalidDataAfterLoad()
    {
        if (data.catCoin < 0)
        {
            data.catCoin = 0;
        }

        if (data.heart < 0)
        {
            data.heart = 0;
        }

        if (data.repairMaterial < 0)
        {
            data.repairMaterial = 0;
        }
    }

    private static bool Resource_Set(string resourceType, int value)
    {
        int safeValue = value < 0 ? 0 : value;

        if (resourceType == "catCoin")
        {
            data.catCoin = safeValue;
            return true;
        }

        if (resourceType == "heart")
        {
            data.heart = safeValue;
            return true;
        }

        if (resourceType == "repairMaterial")
        {
            data.repairMaterial = safeValue;
            return true;
        }

        Debug.LogWarning("[DataMgr][Resource] Unknown resource type: " + resourceType);
        return false;
    }

    private static bool IsKnownResource(string resourceType)
    {
        return resourceType == "catCoin" || resourceType == "heart" || resourceType == "repairMaterial";
    }
}

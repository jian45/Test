using System;
using System.Collections.Generic;

/// <summary>生产者（咖啡机/烤箱）：解锁、充能次数、广告补充</summary>
public static partial class DataMgr
{
    public static void Producer_Unlock(string producerId, int initialCharges)
    {
        EnsureData();

        PlayerProducerRuntimeData producer = Producer_GetOrCreate(producerId);
        producer.isUnlocked = true;
        producer.remainingCharges = initialCharges < 0 ? 0 : initialCharges;
        Save();
    }

    public static void Producer_SetCharges(string producerId, int charges)
    {
        EnsureData();

        PlayerProducerRuntimeData producer = Producer_GetOrCreate(producerId);
        producer.remainingCharges = charges < 0 ? 0 : charges;
        Save();
    }

    public static void Producer_AddCharges(string producerId, int amount)
    {
        EnsureData();
        Producer_AddChargesInternal(producerId, amount);
        Save();
    }

    public static void Producer_SetRefillAdTarget(string producerId)
    {
        EnsureData();
        data.lastProducerRefillTargetId = producerId;
        Save();
    }

    public static bool Producer_CanShowRefillAd(string producerId)
    {
        EnsureData();

        if (data.currentBatchId < 7)
        {
            return false;
        }

        PlayerProducerRuntimeData producer = Producer_Find(producerId);
        return producer != null && producer.isUnlocked && producer.remainingCharges <= 0;
    }

    [Obsolete("Use Producer_Unlock.")]
    public static void UnlockProducer(string producerId, int initialCharges)
    {
        Producer_Unlock(producerId, initialCharges);
    }

    [Obsolete("Use Producer_SetCharges.")]
    public static void SetProducerCharges(string producerId, int charges)
    {
        Producer_SetCharges(producerId, charges);
    }

    [Obsolete("Use Producer_AddCharges.")]
    public static void AddProducerCharges(string producerId, int amount)
    {
        Producer_AddCharges(producerId, amount);
    }

    [Obsolete("Use Producer_SetRefillAdTarget.")]
    public static void SetProducerRefillTarget(string producerId)
    {
        Producer_SetRefillAdTarget(producerId);
    }

    [Obsolete("Use Producer_CanShowRefillAd.")]
    public static bool CanShowProducerRefillAd(string producerId)
    {
        return Producer_CanShowRefillAd(producerId);
    }

    private static void Producer_InitData()
    {
        data.producerRuntimeList = new List<PlayerProducerRuntimeData>();
        Producer_InitProducersForBatch(data.currentBatchId);
    }

    private static void Producer_InitDataForArchiveUpgrade()
    {
        if (data.producerRuntimeList == null)
        {
            data.producerRuntimeList = new List<PlayerProducerRuntimeData>();
        }

        Producer_InitProducersForBatch(data.currentBatchId);
    }

    private static void Producer_FixInvalidDataAfterLoad()
    {
        Producer_InitDataForArchiveUpgrade();

        for (int i = 0; i < data.producerRuntimeList.Count; i++)
        {
            PlayerProducerRuntimeData producer = data.producerRuntimeList[i];
            if (producer == null)
            {
                continue;
            }

            if (producer.remainingCharges < 0)
            {
                producer.remainingCharges = 0;
            }

            if (producer.refillAdUsedInBatch < 0)
            {
                producer.refillAdUsedInBatch = 0;
            }
        }
    }

    private static void Producer_InitProducersForBatch(int batchId)
    {
        if (data.producerRuntimeList == null)
        {
            data.producerRuntimeList = new List<PlayerProducerRuntimeData>();
        }

        if (batchId >= 1)
        {
            PlayerProducerRuntimeData coffee = Producer_GetOrCreate("producer_coffee");
            coffee.isUnlocked = true;
        }

        if (batchId >= 4)
        {
            PlayerProducerRuntimeData oven = Producer_GetOrCreate("producer_oven");
            oven.isUnlocked = true;
        }
    }

    private static PlayerProducerRuntimeData Producer_GetOrCreate(string producerId)
    {
        PlayerProducerRuntimeData producer = Producer_Find(producerId);
        if (producer != null)
        {
            return producer;
        }

        producer = new PlayerProducerRuntimeData
        {
            producerId = producerId
        };
        data.producerRuntimeList.Add(producer);
        return producer;
    }

    private static PlayerProducerRuntimeData Producer_Find(string producerId)
    {
        if (data.producerRuntimeList == null)
        {
            return null;
        }

        for (int i = 0; i < data.producerRuntimeList.Count; i++)
        {
            PlayerProducerRuntimeData producer = data.producerRuntimeList[i];
            if (producer != null && producer.producerId == producerId)
            {
                return producer;
            }
        }

        return null;
    }

    private static void Producer_AddChargesInternal(string producerId, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        PlayerProducerRuntimeData producer = Producer_GetOrCreate(producerId);
        producer.remainingCharges += amount;
    }
}

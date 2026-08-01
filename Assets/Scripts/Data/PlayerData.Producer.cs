using System;
using System.Collections.Generic;

/// <summary>生产者数据：咖啡机/烤箱的解锁状态、剩余次数</summary>
public partial class PlayerData
{
    public List<PlayerProducerRuntimeData> producerRuntimeList = new List<PlayerProducerRuntimeData>();
}

[Serializable]
public class PlayerProducerRuntimeData
{
    public string producerId;
    public bool isUnlocked;
    public int remainingCharges;
    public long cooldownEndTime;
    public int refillAdUsedInBatch;
}
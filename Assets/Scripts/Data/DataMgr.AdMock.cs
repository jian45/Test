using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>广告 Mock：模拟激励广告播放、奖励发放、埋点（D 组用）</summary>
public static partial class DataMgr
{
    public const string PlacementSettlementMaterialDouble = "ad_settlement_material_double";
    public const string PlacementProducerRefill = "ad_producer_refill";

    public static void Ad_ShowRewardMock(string placementId, Action<bool> onComplete)
    {
        EnsureData();

        if (!Ad_CanShowRewardMock(placementId))
        {
#if UNITY_EDITOR
            Debug.LogWarning("[Ad][Mock] Placement is not available: " + placementId);
#endif
            if (onComplete != null)
            {
                onComplete(false);
            }
            return;
        }

        string traceId = Ad_CreateTraceId(placementId);
        string requestId = Ad_CreateRequestId();
        PlayerAdTraceData trace = Ad_CreateTrace(placementId, traceId, requestId);

        Track_Event("ad_click", new Dictionary<string, object>
        {
            { "placementId", placementId },
            { "traceId", traceId },
            { "promisedReward", Ad_GetPromisedReward(placementId) },
            { "batchId", data.currentBatchId }
        });

        Track_Event("ad_request", new Dictionary<string, object>
        {
            { "placementId", placementId },
            { "requestId", requestId },
            { "traceId", traceId }
        });

#if UNITY_EDITOR
        Debug.Log("[Ad][Mock] Show rewarded ad success | placementId=" + placementId + ", traceId=" + traceId);
#endif

        Track_Event("ad_show", new Dictionary<string, object>
        {
            { "placementId", placementId },
            { "requestId", requestId },
            { "traceId", traceId }
        });

        Track_Event("ad_complete", new Dictionary<string, object>
        {
            { "duration", 15 },
            { "adWatchedCount", data.totalRewardedAdCompleteCount + 1 },
            { "placementId", placementId },
            { "traceId", traceId }
        });

        Ad_GrantReward(trace);

        if (onComplete != null)
        {
            onComplete(true);
        }
    }

    public static void Ad_ShowRewardFailMock(string placementId, string errorCode, Action<bool> onComplete)
    {
        EnsureData();

        if (!Ad_CanShowRewardMock(placementId))
        {
            if (onComplete != null)
            {
                onComplete(false);
            }
            return;
        }

        string traceId = Ad_CreateTraceId(placementId);
        string requestId = Ad_CreateRequestId();
        PlayerAdTraceData trace = Ad_CreateTrace(placementId, traceId, requestId);
        trace.status = "failed";
        trace.errorCode = errorCode;
        trace.completedAt = NowUnixMs();

        Track_Event("ad_fail", new Dictionary<string, object>
        {
            { "placementId", placementId },
            { "requestId", requestId },
            { "traceId", traceId },
            { "errorStage", "mock_show" },
            { "errorCode", errorCode },
            { "fallbackResult", "continue_game" },
            { "baseRewardGranted", true },
            { "batchId", data.currentBatchId }
        });

#if UNITY_EDITOR
        Debug.LogWarning("[Ad][Mock] Rewarded ad failed | placementId=" + placementId + ", errorCode=" + errorCode);
#endif

        Save();

        if (onComplete != null)
        {
            onComplete(false);
        }
    }

    public static bool Ad_CanShowRewardMock(string placementId)
    {
        EnsureData();

        if (placementId == PlacementSettlementMaterialDouble)
        {
            return data.currentBatchId >= 3 && data.isSettlementShown;
        }

        if (placementId == PlacementProducerRefill)
        {
            string producerId = string.IsNullOrEmpty(data.lastProducerRefillTargetId)
                ? "producer_coffee"
                : data.lastProducerRefillTargetId;
            return data.currentBatchId >= 7 && Producer_CanShowRefillAd(producerId);
        }

        return false;
    }

    [Obsolete("Use Ad_ShowRewardMock.")]
    public static void ShowRewardAdMock(string placementId, Action<bool> onComplete)
    {
        Ad_ShowRewardMock(placementId, onComplete);
    }

    [Obsolete("Use Ad_ShowRewardFailMock.")]
    public static void ShowRewardAdFailMock(string placementId, string errorCode, Action<bool> onComplete)
    {
        Ad_ShowRewardFailMock(placementId, errorCode, onComplete);
    }

    [Obsolete("Use Ad_CanShowRewardMock.")]
    public static bool CanShowRewardAdMock(string placementId)
    {
        return Ad_CanShowRewardMock(placementId);
    }

    private static void Ad_InitData()
    {
        data.totalRewardedAdCompleteCount = 0;
        data.mockSettlementDoubleRepairMaterial = 1;
        data.lastProducerRefillTargetId = string.Empty;
        data.adPlacements = new List<PlayerAdPlacementData>();
        data.adTraces = new List<PlayerAdTraceData>();
    }

    private static void Ad_InitDataForArchiveUpgrade()
    {
        if (data.mockSettlementDoubleRepairMaterial <= 0)
        {
            data.mockSettlementDoubleRepairMaterial = 1;
        }

        if (data.lastProducerRefillTargetId == null)
        {
            data.lastProducerRefillTargetId = string.Empty;
        }

        if (data.adPlacements == null)
        {
            data.adPlacements = new List<PlayerAdPlacementData>();
        }

        if (data.adTraces == null)
        {
            data.adTraces = new List<PlayerAdTraceData>();
        }
    }

    private static void Ad_FixInvalidDataAfterLoad()
    {
        Ad_InitDataForArchiveUpgrade();

        if (data.totalRewardedAdCompleteCount < 0)
        {
            data.totalRewardedAdCompleteCount = 0;
        }
    }

    private static PlayerAdTraceData Ad_CreateTrace(string placementId, string traceId, string requestId)
    {
        PlayerAdTraceData trace = new PlayerAdTraceData
        {
            traceId = traceId,
            requestId = requestId,
            placementId = placementId,
            status = "requested",
            promisedReward = Ad_GetPromisedReward(placementId),
            grantedReward = string.Empty,
            createdAt = NowUnixMs()
        };
        data.adTraces.Add(trace);
        return trace;
    }

    private static void Ad_GrantReward(PlayerAdTraceData trace)
    {
        if (trace == null || trace.rewardGranted)
        {
            return;
        }

        string placementId = trace.placementId;
        string promisedReward = trace.promisedReward;
        string grantedReward = promisedReward;

        if (placementId == PlacementSettlementMaterialDouble)
        {
            int extra = data.mockSettlementDoubleRepairMaterial;
            int before = data.repairMaterial;
            data.repairMaterial += extra;
            int after = data.repairMaterial;

            Track_Event("ad_reward_granted", new Dictionary<string, object>
            {
                { "placementId", placementId },
                { "traceId", trace.traceId },
                { "requestId", trace.requestId },
                { "promisedReward", promisedReward },
                { "grantedReward", grantedReward },
                { "beforeBalance", before },
                { "afterBalance", after }
            });
        }
        else if (placementId == PlacementProducerRefill)
        {
            string producerId = string.IsNullOrEmpty(data.lastProducerRefillTargetId)
                ? "producer_coffee"
                : data.lastProducerRefillTargetId;
            PlayerProducerRuntimeData producer = Producer_GetOrCreate(producerId);
            int before = producer.remainingCharges;
            Producer_AddChargesInternal(producerId, 8);
            int after = producer.remainingCharges;
            producer.refillAdUsedInBatch++;

            Track_Event("ad_reward_granted", new Dictionary<string, object>
            {
                { "placementId", placementId },
                { "traceId", trace.traceId },
                { "requestId", trace.requestId },
                { "producerId", producerId },
                { "promisedReward", promisedReward },
                { "grantedReward", grantedReward },
                { "beforeBalance", before },
                { "afterBalance", after }
            });
        }
        else
        {
            trace.status = "ignored";
            return;
        }

        trace.status = "granted";
        trace.rewardGranted = true;
        trace.grantedReward = grantedReward;
        trace.completedAt = NowUnixMs();
        data.totalRewardedAdCompleteCount++;
        data.currentBatchAdWatchedCount++;

        PlayerAdPlacementData placement = Ad_GetOrCreatePlacement(placementId);
        placement.watchedCount++;
        placement.watchedCountInCurrentBatch++;
        placement.lastWatchedAt = trace.completedAt;

        Save();
    }

    private static PlayerAdPlacementData Ad_GetOrCreatePlacement(string placementId)
    {
        for (int i = 0; i < data.adPlacements.Count; i++)
        {
            PlayerAdPlacementData placement = data.adPlacements[i];
            if (placement != null && placement.placementId == placementId)
            {
                return placement;
            }
        }

        PlayerAdPlacementData newPlacement = new PlayerAdPlacementData
        {
            placementId = placementId
        };
        data.adPlacements.Add(newPlacement);
        return newPlacement;
    }

    private static void Ad_ResetPlacementBatchWatchCounts()
    {
        if (data.adPlacements == null)
        {
            return;
        }

        for (int i = 0; i < data.adPlacements.Count; i++)
        {
            if (data.adPlacements[i] != null)
            {
                data.adPlacements[i].watchedCountInCurrentBatch = 0;
            }
        }
    }

    private static string Ad_GetPromisedReward(string placementId)
    {
        if (placementId == PlacementSettlementMaterialDouble)
        {
            return "repairMaterial+" + data.mockSettlementDoubleRepairMaterial;
        }

        if (placementId == PlacementProducerRefill)
        {
            return "producerCharges+8";
        }

        return string.Empty;
    }

    private static string Ad_CreateTraceId(string placementId)
    {
        return "trace_" + placementId + "_" + NowUnixMs() + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    private static string Ad_CreateRequestId()
    {
        return "req_" + NowUnixMs() + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }
}

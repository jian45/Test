using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>事件埋点：上报事件、自动附加公共字段、事件计数</summary>
public static partial class DataMgr
{
    public static void Track_Event(string eventName, Dictionary<string, object> properties = null)
    {
        EnsureData();

        if (string.IsNullOrEmpty(eventName))
        {
            return;
        }

        Dictionary<string, object> safeProperties = properties ?? new Dictionary<string, object>();
        Track_FillCommonFields(safeProperties);
        Track_IncreaseCounter(eventName);

        if (eventName == "app_start")
        {
            data.hasTrackAppStart = true;
        }

        trackProvider.Track(eventName, safeProperties);
        Save();
    }

    [Obsolete("Use Track_Event.")]
    public static void Track(string eventName, Dictionary<string, object> properties = null)
    {
        Track_Event(eventName, properties);
    }

    private static void Track_InitData()
    {
        data.hasTrackAppStart = false;
        data.hasTrackFirstCatAdopt = false;
        data.hasTrackFirstMerge = false;
        data.hasTrackFirstOrderSubmit = false;
        data.hasTrackFirstCatService = false;
        data.hasTrackFirstRepairNode = false;
        data.trackCounters = new List<PlayerTrackCounterData>();
    }

    private static void Track_InitDataForArchiveUpgrade()
    {
        if (data.trackCounters == null)
        {
            data.trackCounters = new List<PlayerTrackCounterData>();
        }
    }

    private static void Track_FixInvalidDataAfterLoad()
    {
        Track_InitDataForArchiveUpgrade();
    }

    private static void Track_FillCommonFields(Dictionary<string, object> properties)
    {
        Track_AddFieldIfMissing(properties, "userId", data.userId);
        Track_AddFieldIfMissing(properties, "sessionId", data.sessionId);
        Track_AddFieldIfMissing(properties, "eventTime", NowUnixMs());
        Track_AddFieldIfMissing(properties, "clientVersion", Application.version);
        Track_AddFieldIfMissing(properties, "platform", "wechat_minigame");
        Track_AddFieldIfMissing(properties, "channel", "mock");
        Track_AddFieldIfMissing(properties, "sceneId", data.currentSceneId);
        Track_AddFieldIfMissing(properties, "batchId", data.currentBatchId);
        Track_AddFieldIfMissing(properties, "playerProgress", data.maxCompletedBatchId);
        Track_AddFieldIfMissing(properties, "experimentGroup", data.experimentGroup);
        Track_AddFieldIfMissing(properties, "isNewUser", data.isNewUser);
    }

    private static void Track_AddFieldIfMissing(Dictionary<string, object> properties, string key, object value)
    {
        if (!properties.ContainsKey(key))
        {
            properties.Add(key, value);
        }
    }

    private static void Track_IncreaseCounter(string eventName)
    {
        Track_InitDataForArchiveUpgrade();

        for (int i = 0; i < data.trackCounters.Count; i++)
        {
            PlayerTrackCounterData counter = data.trackCounters[i];
            if (counter != null && counter.eventName == eventName)
            {
                counter.count++;
                return;
            }
        }

        data.trackCounters.Add(new PlayerTrackCounterData
        {
            eventName = eventName,
            count = 1
        });
    }
}

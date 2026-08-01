using System;
using System.Collections.Generic;

/// <summary>猫咪状态：解锁、预览、领养、服务记录埋点</summary>
public static partial class DataMgr
{
    public static void Cat_Unlock(string catId, string unlockType)
    {
        EnsureData();

        AddUniqueString(data.unlockedCatIds, catId);
        PlayerCatRuntimeData cat = Cat_GetOrCreate(catId);
        if (string.IsNullOrEmpty(cat.residenceState))
        {
            cat.residenceState = "unlocked";
        }

        Track_Event("cat_unlock", new Dictionary<string, object>
        {
            { "catId", catId },
            { "unlockType", unlockType },
            { "batchId", data.currentBatchId }
        });

        Save();
    }

    public static void Cat_AddPreview(string catId)
    {
        EnsureData();

        if (AddUniqueString(data.previewCatIds, catId))
        {
            Save();
        }
    }

    public static void Cat_TrackAdopt(string catId)
    {
        EnsureData();

        PlayerCatRuntimeData cat = Cat_GetOrCreate(catId);
        cat.residenceState = "adopted";
        AddUniqueString(data.unlockedCatIds, catId);
        data.isNewUser = false;

        if (!data.hasTrackFirstCatAdopt)
        {
            data.hasTrackFirstCatAdopt = true;
            Track_Event("first_cat_adopt", new Dictionary<string, object>
            {
                { "catId", catId },
                { "batchId", data.currentBatchId }
            });
        }
        else
        {
            Track_Event("cat_adopt", new Dictionary<string, object>
            {
                { "catId", catId },
                { "batchId", data.currentBatchId }
            });
        }

        Save();
    }

    public static void Cat_TrackService(string catId, string customerId, string triggerType)
    {
        EnsureData();

        PlayerCatRuntimeData cat = Cat_GetOrCreate(catId);
        cat.serviceCount++;
        cat.lastServiceAt = NowUnixMs();

        string eventName = data.hasTrackFirstCatService ? "cat_service_trigger" : "first_cat_service";
        data.hasTrackFirstCatService = true;

        Track_Event(eventName, new Dictionary<string, object>
        {
            { "catId", catId },
            { "customerId", customerId },
            { "triggerType", triggerType },
            { "batchId", data.currentBatchId }
        });

        Save();
    }

    [Obsolete("Use Cat_Unlock.")]
    public static void UnlockCat(string catId, string unlockType)
    {
        Cat_Unlock(catId, unlockType);
    }

    [Obsolete("Use Cat_AddPreview.")]
    public static void AddPreviewCat(string catId)
    {
        Cat_AddPreview(catId);
    }

    [Obsolete("Use Cat_TrackAdopt.")]
    public static void TrackCatAdopt(string catId)
    {
        Cat_TrackAdopt(catId);
    }

    [Obsolete("Use Cat_TrackService.")]
    public static void TrackCatService(string catId, string customerId, string triggerType)
    {
        Cat_TrackService(catId, customerId, triggerType);
    }

    private static void Cat_InitData()
    {
        data.unlockedCatIds = new List<string>();
        data.previewCatIds = new List<string>();
        data.catRuntimeList = new List<PlayerCatRuntimeData>();
    }

    private static void Cat_InitDataForArchiveUpgrade()
    {
        if (data.unlockedCatIds == null)
        {
            data.unlockedCatIds = new List<string>();
        }

        if (data.previewCatIds == null)
        {
            data.previewCatIds = new List<string>();
        }

        if (data.catRuntimeList == null)
        {
            data.catRuntimeList = new List<PlayerCatRuntimeData>();
        }
    }

    private static void Cat_FixInvalidDataAfterLoad()
    {
        Cat_InitDataForArchiveUpgrade();
    }

    private static PlayerCatRuntimeData Cat_GetOrCreate(string catId)
    {
        Cat_InitDataForArchiveUpgrade();

        for (int i = 0; i < data.catRuntimeList.Count; i++)
        {
            PlayerCatRuntimeData cat = data.catRuntimeList[i];
            if (cat != null && cat.catId == catId)
            {
                return cat;
            }
        }

        PlayerCatRuntimeData newCat = new PlayerCatRuntimeData
        {
            catId = catId,
            residenceState = "preview"
        };
        data.catRuntimeList.Add(newCat);
        return newCat;
    }
}

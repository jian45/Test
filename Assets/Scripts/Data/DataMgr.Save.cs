using System;
using UnityEngine;

/// <summary>存档初始化、读写、版本升级</summary>
public static partial class DataMgr
{
    private const int CurrentDataVersion = 2;
    private const int DefaultBoardWidth = 6;
    private const int DefaultBoardHeight = 7;

    private static IDataSaveProvider saveProvider;
    private static ITrackProvider trackProvider;

    public static bool IsLoaded { get; private set; }

    public static void Init()
    {
        saveProvider = CreateSaveProvider();
        trackProvider = CreateTrackProvider();
        Load();
    }

    public static void Load()
    {
        EnsureProviders();

        data = saveProvider.Load();
        if (data == null)
        {
            data = Archive_CreateNew();
        }
        else
        {
            Archive_CheckAndUpgradeIfNeeded();
            Archive_FixInvalidDataAfterLoad();
        }

        IsLoaded = true;
        Save();
        LogSave("Load player data");
    }

    public static void Save()
    {
        EnsureProviders();
        EnsureData();
        Archive_CheckAndUpgradeIfNeeded();
        saveProvider.Save(data);
        LogSave("Save player data");
    }

    public static bool HasSave()
    {
        EnsureProviders();
        return saveProvider.HasSave();
    }

    public static void DeleteSave()
    {
        EnsureProviders();
        saveProvider.DeleteSave();
        data = Archive_CreateNew();
        IsLoaded = true;
        Save();
        LogSave("Delete save and create new data");
    }

    private static void EnsureData()
    {
        EnsureProviders();

        if (data == null)
        {
            data = Archive_CreateNew();
        }
    }

    private static PlayerData Archive_CreateNew()
    {
        long now = NowUnixMs();
        PlayerData newData = new PlayerData
        {
            dataVersion = 0,
            userId = CreateUserId(),
            sessionId = CreateSessionId(),
            createTime = now,
            lastSaveTime = now
        };

        data = newData;

        Resource_InitData();
        Batch_InitData();
        Board_InitData();
        Order_InitData();
        Repair_InitData();
        Cat_InitData();
        Producer_InitData();
        Ad_InitData();
        Track_InitData();

        data.dataVersion = CurrentDataVersion;
        return data;
    }

    private static void Archive_CheckAndUpgradeIfNeeded()
    {
        if (data == null)
        {
            return;
        }

        Archive_FixMetaFields();

        if (data.dataVersion <= 0)
        {
            Archive_Upgrade_0_To_1();
        }

        while (data.dataVersion < CurrentDataVersion)
        {
            switch (data.dataVersion)
            {
                case 1:
                    Archive_Upgrade_1_To_2();
                    break;
                default:
                    Archive_ForceUpgradeToCurrentWithSafeFix();
                    break;
            }
        }

        data.lastSaveTime = NowUnixMs();
    }

    private static void Archive_Upgrade_0_To_1()
    {
        Resource_InitDataForArchiveUpgrade();
        Batch_InitDataForArchiveUpgrade();
        Board_InitDataForArchiveUpgrade();
        Order_InitDataForArchiveUpgrade();
        Repair_InitDataForArchiveUpgrade();
        Cat_InitDataForArchiveUpgrade();
        Producer_InitDataForArchiveUpgrade();
        Ad_InitDataForArchiveUpgrade();
        Track_InitDataForArchiveUpgrade();
        data.dataVersion = 1;
    }

    private static void Archive_Upgrade_1_To_2()
    {
        Resource_InitDataForArchiveUpgrade();
        Batch_InitDataForArchiveUpgrade();
        Board_InitDataForArchiveUpgrade();
        Order_InitDataForArchiveUpgrade();
        Repair_InitDataForArchiveUpgrade();
        Cat_InitDataForArchiveUpgrade();
        Producer_InitDataForArchiveUpgrade();
        Ad_InitDataForArchiveUpgrade();
        Track_InitDataForArchiveUpgrade();
        data.dataVersion = 2;
    }

    private static void Archive_ForceUpgradeToCurrentWithSafeFix()
    {
        Resource_InitDataForArchiveUpgrade();
        Batch_InitDataForArchiveUpgrade();
        Board_InitDataForArchiveUpgrade();
        Order_InitDataForArchiveUpgrade();
        Repair_InitDataForArchiveUpgrade();
        Cat_InitDataForArchiveUpgrade();
        Producer_InitDataForArchiveUpgrade();
        Ad_InitDataForArchiveUpgrade();
        Track_InitDataForArchiveUpgrade();
        data.dataVersion = CurrentDataVersion;
    }

    private static void Archive_FixInvalidDataAfterLoad()
    {
        Archive_FixMetaFields();
        Resource_FixInvalidDataAfterLoad();
        Batch_FixInvalidDataAfterLoad();
        Board_FixInvalidDataAfterLoad();
        Order_FixInvalidDataAfterLoad();
        Repair_FixInvalidDataAfterLoad();
        Cat_FixInvalidDataAfterLoad();
        Producer_FixInvalidDataAfterLoad();
        Ad_FixInvalidDataAfterLoad();
        Track_FixInvalidDataAfterLoad();
    }

    private static void Archive_FixMetaFields()
    {
        if (string.IsNullOrEmpty(data.userId))
        {
            data.userId = CreateUserId();
        }

        if (string.IsNullOrEmpty(data.sessionId))
        {
            data.sessionId = CreateSessionId();
        }

        if (data.createTime <= 0)
        {
            data.createTime = NowUnixMs();
        }
    }

    private static void EnsureProviders()
    {
        if (saveProvider == null)
        {
            saveProvider = CreateSaveProvider();
        }

        if (trackProvider == null)
        {
            trackProvider = CreateTrackProvider();
        }
    }

    private static IDataSaveProvider CreateSaveProvider()
    {
#if UNITY_EDITOR
        return new EditorMockSaveProvider();
#else
        return new LocalJsonSaveProvider();
#endif
    }

    private static ITrackProvider CreateTrackProvider()
    {
        return new EditorMockTrackProvider();
    }

    private static long NowUnixMs()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    private static string CreateUserId()
    {
        return "mock_user_" + Guid.NewGuid().ToString("N");
    }

    private static string CreateSessionId()
    {
        return "s_" + NowUnixMs().ToString();
    }

    private static void LogSave(string message)
    {
#if UNITY_EDITOR
        Debug.Log(string.Format(
            "[DataMgr][Save] {0} | batch={1} coin={2} heart={3} material={4}",
            message,
            data != null ? data.currentBatchId : 0,
            data != null ? data.catCoin : 0,
            data != null ? data.heart : 0,
            data != null ? data.repairMaterial : 0));
#endif
    }

    private static bool AddUniqueString(System.Collections.Generic.List<string> list, string value)
    {
        if (list == null || string.IsNullOrEmpty(value) || list.Contains(value))
        {
            return false;
        }

        list.Add(value);
        return true;
    }
}

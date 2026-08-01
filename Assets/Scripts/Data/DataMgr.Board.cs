using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>棋盘格管理：物品放置/清除、空位统计、合成埋点</summary>
public static partial class DataMgr
{
    public static void Board_SetCellItem(int cellIndex, string itemId, string itemInstanceId)
    {
        EnsureData();

        if (!Board_IsValidCellIndex(cellIndex))
        {
            Debug.LogWarning("[DataMgr][Board] Invalid cell index: " + cellIndex);
            return;
        }

        PlayerBoardCellData cell = Board_GetOrCreateCell(cellIndex);
        cell.itemId = itemId ?? string.Empty;
        cell.itemInstanceId = itemInstanceId ?? string.Empty;

        Board_UpdateCurrentBatchMinEmptyCellCount();
        Save();
    }

    public static void Board_ClearCellItem(int cellIndex)
    {
        EnsureData();

        if (!Board_IsValidCellIndex(cellIndex))
        {
            Debug.LogWarning("[DataMgr][Board] Invalid cell index: " + cellIndex);
            return;
        }

        PlayerBoardCellData cell = Board_GetOrCreateCell(cellIndex);
        cell.itemId = string.Empty;
        cell.itemInstanceId = string.Empty;

        Board_UpdateCurrentBatchMinEmptyCellCount();
        Save();
    }

    public static void Board_TrackItemGenerate(string producerId, string itemId, string chainId)
    {
        Track_Event("item_generate", new Dictionary<string, object>
        {
            { "producerId", producerId },
            { "itemId", itemId },
            { "chainId", chainId },
            { "batchId", data.currentBatchId }
        });
    }

    public static void Track_FirstMergeIfNeeded(string fromItemId, string toItemId)
    {
        EnsureData();

        string eventName = data.hasTrackFirstMerge ? "merge" : "first_merge";
        data.hasTrackFirstMerge = true;

        Track_Event(eventName, new Dictionary<string, object>
        {
            { "fromItemId", fromItemId },
            { "toItemId", toItemId },
            { "batchId", data.currentBatchId }
        });
    }

    [Obsolete("Use Board_SetCellItem.")]
    public static void SaveBoardCell(int cellIndex, string itemId, string itemInstanceId)
    {
        Board_SetCellItem(cellIndex, itemId, itemInstanceId);
    }

    [Obsolete("Use Board_ClearCellItem.")]
    public static void ClearBoardCell(int cellIndex)
    {
        Board_ClearCellItem(cellIndex);
    }

    [Obsolete("Use Board_TrackItemGenerate.")]
    public static void TrackItemGenerate(string producerId, string itemId, string chainId)
    {
        Board_TrackItemGenerate(producerId, itemId, chainId);
    }

    [Obsolete("Use Track_FirstMergeIfNeeded.")]
    public static void TrackMerge(string fromItemId, string toItemId)
    {
        Track_FirstMergeIfNeeded(fromItemId, toItemId);
    }

    private static void Board_InitData()
    {
        data.boardWidth = DefaultBoardWidth;
        data.boardHeight = DefaultBoardHeight;
        data.boardFullCount = 0;
        data.recycleUseCount = 0;
        data.boardCells = new List<PlayerBoardCellData>();

        int totalCells = data.boardWidth * data.boardHeight;
        for (int i = 0; i < totalCells; i++)
        {
            data.boardCells.Add(Board_CreateDefaultCell(i));
        }
    }

    private static void Board_InitDataForArchiveUpgrade()
    {
        if (data.boardWidth <= 0)
        {
            data.boardWidth = DefaultBoardWidth;
        }

        if (data.boardHeight <= 0)
        {
            data.boardHeight = DefaultBoardHeight;
        }

        if (data.boardCells == null)
        {
            data.boardCells = new List<PlayerBoardCellData>();
        }

        int totalCells = data.boardWidth * data.boardHeight;
        for (int i = data.boardCells.Count; i < totalCells; i++)
        {
            data.boardCells.Add(Board_CreateDefaultCell(i));
        }
    }

    private static void Board_FixInvalidDataAfterLoad()
    {
        Board_InitDataForArchiveUpgrade();

        int totalCells = data.boardWidth * data.boardHeight;
        if (data.boardCells.Count > totalCells)
        {
            data.boardCells.RemoveRange(totalCells, data.boardCells.Count - totalCells);
        }

        for (int i = 0; i < data.boardCells.Count; i++)
        {
            if (data.boardCells[i] == null)
            {
                data.boardCells[i] = Board_CreateDefaultCell(i);
            }

            data.boardCells[i].cellIndex = i;

            if (data.boardCells[i].itemId == null)
            {
                data.boardCells[i].itemId = string.Empty;
            }

            if (data.boardCells[i].itemInstanceId == null)
            {
                data.boardCells[i].itemInstanceId = string.Empty;
            }

            if (data.boardCells[i].tutorialState == null)
            {
                data.boardCells[i].tutorialState = string.Empty;
            }
        }

        if (data.boardFullCount < 0)
        {
            data.boardFullCount = 0;
        }

        if (data.recycleUseCount < 0)
        {
            data.recycleUseCount = 0;
        }

        Board_UpdateCurrentBatchMinEmptyCellCount();
    }

    private static PlayerBoardCellData Board_GetOrCreateCell(int cellIndex)
    {
        Board_InitDataForArchiveUpgrade();
        return data.boardCells[cellIndex];
    }

    private static bool Board_IsValidCellIndex(int cellIndex)
    {
        Board_InitDataForArchiveUpgrade();
        return cellIndex >= 0 && cellIndex < data.boardCells.Count;
    }

    private static void Board_UpdateCurrentBatchMinEmptyCellCount()
    {
        Board_InitDataForArchiveUpgrade();

        int emptyCount = 0;
        for (int i = 0; i < data.boardCells.Count; i++)
        {
            PlayerBoardCellData cell = data.boardCells[i];
            if (cell == null || string.IsNullOrEmpty(cell.itemId))
            {
                emptyCount++;
            }
        }

        if (data.currentBatchMinEmptyCellCount <= 0)
        {
            data.currentBatchMinEmptyCellCount = emptyCount;
        }
        else
        {
            data.currentBatchMinEmptyCellCount = Mathf.Min(data.currentBatchMinEmptyCellCount, emptyCount);
        }
    }

    private static PlayerBoardCellData Board_CreateDefaultCell(int cellIndex)
    {
        return new PlayerBoardCellData
        {
            cellIndex = cellIndex,
            itemId = string.Empty,
            itemInstanceId = string.Empty,
            tutorialState = string.Empty
        };
    }
}

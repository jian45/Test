using System;
using System.Collections.Generic;

/// <summary>棋盘格数据：宽高、格子列表、回收次数</summary>
public partial class PlayerData
{
    public int boardWidth;
    public int boardHeight;
    public int boardFullCount;
    public int recycleUseCount;
    public List<PlayerBoardCellData> boardCells = new List<PlayerBoardCellData>();
}

[Serializable]
public class PlayerBoardCellData
{
    public int cellIndex;
    public string itemId;
    public string itemInstanceId;
    public bool isLocked;
    public bool isTempSlot;
    public string tutorialState;
}
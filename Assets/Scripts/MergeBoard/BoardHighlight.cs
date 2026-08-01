using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 棋盘高亮系统——从 MergeBoardPanel 解耦出来的高亮功能
/// 挂载在棋盘格面板上，管理饮料 Outline 高亮
/// </summary>
public class BoardHighlight : MonoBehaviour
{
    private Dictionary<DrinkItem, Outline> drinkGlowMap = new Dictionary<DrinkItem, Outline>();

    private MergeBoardPanel board;

    private void Awake()
    {
        board = GetComponent<MergeBoardPanel>();
    }

    /// <summary>
    /// 注册饮料的光晕——生成饮料时由 MergeBoardPanel 调用
    /// </summary>
    public void RegisterDrink(DrinkItem drink, Outline outline)
    {
        drinkGlowMap[drink] = outline;
    }

    /// <summary>
    /// 移除饮料的光晕——合成销毁时由 MergeBoardPanel 调用
    /// </summary>
    public void UnregisterDrink(DrinkItem drink)
    {
        drinkGlowMap.Remove(drink);
    }

    /// <summary>
    /// 单独控制某个饮料的光晕显隐——拖拽时由 MergeBoardPanel 调用
    /// </summary>
    public void SetGlowEnabled(DrinkItem drink, bool enabled)
    {
        if (drink != null && drinkGlowMap.ContainsKey(drink))
            drinkGlowMap[drink].enabled = enabled;
    }

    /// <summary>
    /// 清空光晕记录——棋盘重建时由 MergeBoardPanel 调用
    /// </summary>
    public void ClearAll()
    {
        drinkGlowMap.Clear();
    }

    /// <summary>
    /// 检查棋盘上是否存在指定等级的饮料（供 UIOrderPanel 联动检测）
    /// </summary>
    public bool HasDrinkAtLevel(int level)
    {
        if (board == null || board.CellOccupants == null) return false;
        for (int i = 0; i < board.CellOccupants.Length; i++)
        {
            if (board.CellOccupants[i] != null && board.CellOccupants[i].Level == level)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取棋盘上所有指定等级饮料的格子索引列表
    /// </summary>
    public List<int> GetCellIndexesByLevel(int level)
    {
        List<int> result = new List<int>();
        if (board == null || board.CellOccupants == null) return result;
        for (int i = 0; i < board.CellOccupants.Length; i++)
        {
            if (board.CellOccupants[i] != null && board.CellOccupants[i].Level == level)
                result.Add(i);
        }
        return result;
    }

    /// <summary>
    /// 设置高亮——将指定索引格子的饮料光晕启用
    /// </summary>
    public void SetHighlightedCells(List<int> indexes)
    {
        ClearHighlights();
        if (indexes == null || indexes.Count == 0) return;

        for (int i = 0; i < indexes.Count; i++)
        {
            int idx = indexes[i];
            if (board == null || idx < 0 || idx >= board.CellOccupants.Length) continue;
            DrinkItem drink = board.CellOccupants[idx];
            if (drink != null && drinkGlowMap.ContainsKey(drink))
                drinkGlowMap[drink].enabled = true;
        }
    }

    /// <summary>
    /// 清除所有高亮——关闭所有饮料的光晕
    /// </summary>
    public void ClearHighlights()
    {
        foreach (var pair in drinkGlowMap)
        {
            if (pair.Value != null)
                pair.Value.enabled = false;
        }
    }
}

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientFramework.UI;

public class MergeBoardPanel :UIBase
{
  [Header("层级引用")]
  [Tooltip("棋盘格面板--动态生成的格子在此面板下")]
  public RectTransform gridPanel;
  [Tooltip("饮料面板--动态生成的饮料在此面板下")]
  public RectTransform drinkPanel;
  [Tooltip("生成按钮--点击后生成Lv1饮料")]
  public ButtonExt el_generateBtn;


[Header("棋盘格设置")]
[Tooltip("棋盘格格子总数")]
public int celllCount = 16;
[Tooltip("棋盘格列数")]
public int columns = 4;
[Tooltip("棋盘格颜色A")]
public Color colorA = Color.white;
[Tooltip("棋盘格颜色B")]
public Color colorB = Color.black;
private List<RectTransform> cells = new List<RectTransform>();//存储所以棋盘格的RectTransform,用于定位

[Header("饮料设置")]
[Tooltip("饮料预制体数组--索引 0=Lv1饮料，1=Lv2饮料，2=Lv3饮料")]//方便拓展
public GameObject[] drinkPrefabs;
private DrinkItem[]cellpccupamts;//存储每个格子当前占用的饮料信息，null表示未占用，非null表示占用且存储对应饮料信息


[Header("吸附判定范围")]
[Tooltip("吸附判定范围--当饮料与格子中心距离小于此值时，饮料将自动吸附到格子中心")]
public float snapDistance = 30f;


    private BoardHighlight highlight; //高亮系统引用--从本脚本解耦出的高亮功能

    [Header("交付区A-3")]
    public RectTransform deliveryArea;
    public ButtonExt el_deliverySubmitBtn;
    public int deliveryCount=3;
    private List<RectTransform> deliveryCells=new List<RectTransform>();
    private DrinkItem[] deliveryCellDrinks;

    /// <summary>
    /// 暴露格子占用信息给 BoardHighlight 使用
    /// </summary>
    public DrinkItem[] CellOccupants => cellpccupamts;

    private void OnDestroy()
    {
        el_generateBtn?.RemoveClick(OnGenerateClick);

        el_deliverySubmitBtn?.RemoveClick(OnDeliverySubmit);
    }

    /// <summary>
    /// 启动 绑定按钮点击事件并生成棋盘格
    /// </summary>
    public void Start()
    {
        highlight = GetComponent<BoardHighlight>();
        el_generateBtn?.AddClick(OnGenerateClick);
        Generate();
        el_deliverySubmitBtn?.AddClick(OnDeliverySubmit);
        GenerateDeliveryCells();
    }

    /// <summary>
    /// 重新生成棋盘格--先清除原有格子再生成新格子
    /// </summary>
    public void Generate()
    {
        Clear();//先清除原有格子(先饮料再格子)
        cells.Clear();//清空格子列表（List<RectTransform>）

        cellpccupamts = new DrinkItem[celllCount];//重置格子占用信息数组


        for (int i = 0; i < celllCount; i++)
        {
            GameObject cell = new GameObject($"Cell_{i}", typeof(RectTransform), typeof(Image));
            RectTransform rt = cell.GetComponent<RectTransform>();
            cell.transform.SetParent(gridPanel, false);
            Image img = cell.GetComponent<Image>();
            Color c = (i / columns + i % columns) % 2 == 0 ? colorA : colorB;
            img.color = c;
  
            cells.Add(rt);//将新创建的格子RectTransform添加到格子列表中，方便后续定位使用
        }
        // 通知订单目标区刷新高亮（棋盘已就绪）
        UIOrderPanel display = FindObjectOfType<UIOrderPanel>();
        if (display != null&&display.mockOrders!=null&&display.mockOrders.Count>0) display.TriggerHighlightUpdate();

    }

    /// <summary>
    /// 清理所以子物体(先清理饮料面板再清理棋盘格面板 )
    /// </summary>
    public void Clear()
    {
        for (int i = drinkPanel.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(drinkPanel.GetChild(i).gameObject); // Clear 后紧接 Generate 生成新格子，Destroy 延迟一帧会有新旧叠加闪烁
        }
        for (int i = gridPanel.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(gridPanel.GetChild(i).gameObject); // 同上
        }
       
        highlight?.ClearAll();
       deliveryCells.Clear();
        deliveryCellDrinks = null;
    }
    /// <summary>
    /// 按钮点击事件--生成Lv1饮料到一个最小索引空闲格子上，如果没有空闲格子则显示提示文本
    /// </summary>
    private void OnGenerateClick()
    {
        int idx = GetFreeCellIndex();

        if (idx < 0)
        {
            UIOrderPanel d = FindObjectOfType<UIOrderPanel>();
            if (d == null) return;

            MockOrderData target = null;
            for (int i = 0; i < d.mockOrders.Count; i++)
            {
                if (!d.mockOrders[i].isCompleted)
                {
                    target = d.mockOrders[i];
                    break;
                }
            }
            if (target == null) return;

            // 检查有没有可合成的同级饮料对
            bool hasMergeablePair ;
            List<int> mergeableLevels = new List<int>();
            int[] levelCounts = new int[drinkPrefabs.Length];
            for (int i = 0; i < cellpccupamts.Length; i++)
            {
                if (cellpccupamts[i] != null)
                    levelCounts[cellpccupamts[i].Level - 1]++;
            }
            for (int l = 0; l < levelCounts.Length - 1; l++)
            {
                if (levelCounts[l] >= 2)
                {
                    hasMergeablePair = true;
                    mergeableLevels.Add(l + 1);
                    break;
                }
            }

            // 根据订单需求，找到最接近目标的合成方案
            bool foundHint = false;
            List<int> hintCells = new List<int>();
            for (int n = 0; n < target.needs.Count && !foundHint; n++)
            {
                int needLevel = target.needs[n].level;
                int needCount = target.needs[n].count;

                // 从目标等级的下一级开始找（比如目标LV3，找LV2对子）
                for (int l = needLevel - 1; l >= 1; l--)
                {
                    List<int> indexes = highlight != null ? highlight.GetCellIndexesByLevel(l) : new List<int>();
                    int needPair = (int)Mathf.Pow(2, needLevel - l ) * needCount;
                    int take = Mathf.Min(needPair, indexes.Count);

                    if (take >= 2)
                    {
                        for (int i = 0; i < take; i++)
                            hintCells.Add(indexes[i]);
                        foundHint = true;
                        break;
                    }
                }
            }

            if (foundHint && hintCells.Count > 0)
            {
                highlight?.SetHighlightedCells(hintCells);
                UIHintPanel.Instance.ShowHint("棋盘格已满，请先清理棋盘格");
                return;        
              }
            UIHintPanel.Instance.ShowHint("棋盘格已满，请优先合成目标饮品");
            return;
        }
            if (drinkPrefabs == null || drinkPrefabs.Length == 0)
        {
            Debug.LogError("drinkPrefabs 未赋值!");
            return;
        }
       if(idx>=0)   GenerateDrink(idx, 1);
    }
/// <summary>
/// 遍历Cellpccupamts数组找到第一个为null的索引，表示该格子未被占用，返回该索引；如果没有找到则返回-1表示没有空闲格子
/// </summary>
/// <returns></returns>
private int GetFreeCellIndex()
    {
        for (int i = 0; i < cellpccupamts.Length; i++)
        {
            if (cellpccupamts[i] == null) //如果格子未被占用
            {
                return i; //返回该格子索引
            }
        }
        return -1; //没有空闲格子，返回-1
    } 


/// <summary>
/// 根据格子索引获取该格子的世界坐标，供饮料定位使用
/// </summary>
/// <param name="index"></param>
/// <returns></returns>
public Vector2 GetCellWorldPos(int index)
    {
      return index >= 0 && index < cells.Count ? cells[index].position : Vector2.zero; 
      //如果索引有效则返回对应格子世界坐标，否则返回Vector2.zero
    }
/// <summary>
/// 在指定格子生成指定等级的饮料，从预制体数组中实例化对应等级的饮料预制体，并将其定位到对应格子位置，同时更新格子占用信息数组
/// </summary>
/// <param name="cellIndex"></param>
/// <param name="Level"></param>
private void GenerateDrink(int cellIndex ,int Level)
    {
        if (cellIndex < 0 || cellIndex >= cells.Count)
        {
            Debug.LogError($"格子索引 {cellIndex} 越界!");
            return;
        }
        if (Level < 1 || Level > drinkPrefabs.Length)
        {
            Debug.LogError($"饮料等级 {Level} 超出预制体数组范围!");
            return;
        }
    GameObject prefab =drinkPrefabs[Level-1];
        if (prefab == null)
        {
            Debug.LogWarning($"drinkPrefabs索引{Level-1} (Lv{Level})未赋值!");
            return;
        }
    GameObject drinkObj = Instantiate(prefab, drinkPanel);
    RectTransform drinkRT = drinkObj.GetComponent<RectTransform>();
    drinkRT.position = cells[cellIndex].position;
        drinkRT.localScale = Vector3.one; //确保饮料缩放正常

DrinkItem drink = drinkObj.GetComponent<DrinkItem>();
if(drink == null)
        {
            Debug.LogWarning($"Lv{Level}预制体上缺少DrinkItem组件!");
            DestroyImmediate(drinkObj); //销毁错误的饮料对象
             return;
        }
drink.Init(Level, cellIndex, this); //初始化饮料信息（等级、所在格子索引、面板引用） 
cellpccupamts[cellIndex] = drink; //更新格子占用信息，标记该格子被占用   
        //光晕
       Outline outline = drinkObj.AddComponent<Outline>();
        outline.effectColor = new Color(245, 255, 0, 1);
        outline.effectDistance = new Vector2(5, 5);
        outline.enabled = false;
        highlight?.RegisterDrink(drink, outline);
        
    }
/// <summary>
/// 遍历所有格子，找到距离给距离给定世界坐标最近的格子索引最近的索引及其距离；如果没有格子则返回-1和float.MaxValue
/// </summary>
/// <param name="worldPos"></param>
/// <param name="minDist"></param>
/// <returns></returns>
public int FindNearestCell(Vector2 worldPos, out float minDist)
    {
        minDist = float.MaxValue;
        int nearest = -1;
        for (int i = 0; i < cells.Count; i++)
        {
            float dist = Vector2.Distance(worldPos, cells[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = i;
            }
           
        }
        return nearest; //返回最近格子索引，如果没有格子则返回-1
    }
/// <summary>
/// 饮料开始被拖拽时调用，将被拖拽的饮料提到最上层，并将原格子标记为空，防止松开时跟自己合成
/// </summary>
/// <param name="item"></param>
public void OnDrinkDragStart(DrinkItem item)
    {
        item.transform.SetAsLastSibling(); //将被拖拽的饮料提到最上层，防止被遮挡

        if (item.CurrentCellIndex >= 0 && item.CurrentCellIndex < cellpccupamts.Length)
        {
            cellpccupamts[item.CurrentCellIndex] = null;
        }
        else 
        {
            for (int i = 0; i < deliveryCellDrinks.Length; i++) 
            {
                if (deliveryCellDrinks[i] == item) 
                {
                deliveryCellDrinks [i] = null;
                    break;
                }
            }
        }
        highlight?.SetGlowEnabled(item, false);
    }
/// <summary>
/// 饮料拖拽结束时调用——判断最近格子，执行移动/合成/回退
/// </summary>
/// <param name="item"></param>被拖拽的饮料                  
/// <param name="dropPos"></param>松开鼠标时的世界坐标
public void OnDrinkDrop(DrinkItem item, Vector2 dropPos)
    {
        int srcIdx = item.CurrentCellIndex; //获取被拖拽饮料的原始格子索引

        if (deliveryArea != null && IsInDeliveryArea(dropPos))
        {
            Debug.Log($"进了交付区，dropPos={dropPos}");
            int nearestDel = FindNearestDeliveryCell(dropPos, out float delDist);
            if (nearestDel >= 0 && delDist <= snapDistance)
                MoveToDeliveryCell(item, nearestDel);
            else
                ReturnToCell(item, srcIdx);
        }
        else
        {
            int nearest = FindNearestCell(dropPos, out float dist); //找到距离松开鼠标位置最近的格子索引和距离
            if (nearest >= 0 && dist <= snapDistance) //如果找到有效格子且距离在吸附范围内
            {
                DrinkItem target = cellpccupamts[nearest]; //获取最近格子当前占用的饮料信息}

                if (target == null)
                {
                    MoveToCell(item, nearest);
                }
                else if (drinkPrefabs != null && target.Level == item.Level && item.Level < drinkPrefabs.Length)
                {
                    Merge(item, target, nearest);

                }
                else
                {
                    //无法合成->显示提示文本并回退
                    UIHintPanel.Instance.ShowHint("无法合成"); //显示提示文本
                    ReturnToCell(item, srcIdx); //将饮料位置设置回原始格子位置
                }

            }
                else   //如果没有找到有效格子或距离超出吸附范围，回退到原始位置
                {
                
                    ReturnToCell(item, srcIdx); //将饮料位置设置回原始格子位置
                }
            UIOrderPanel d = FindAnyObjectByType<UIOrderPanel>();
            if (d != null) d.TriggerHighlightUpdate();
        }
    }
/// <summary>
/// 将饮料移动到指定格子位置，并更新饮料的当前格子索引和格子占用信息
/// </summary>
/// <param name="item"></param>
/// <param name="cellIndex"></param>
private void MoveToCell(DrinkItem item, int cellIndex)
    {
       item.transform.SetParent(drinkPanel, true);
       item.transform.DOKill();
       item.transform.DOMove(cells[cellIndex].position, 0.2f).SetEase(Ease.OutQuad);
       item.SetCellIndex(cellIndex); //更新饮料的当前格子索引
       cellpccupamts[cellIndex] = item; //更新目标格子占用信息，标记该格子被占用
    }
/// <summary>
/// 同级合成——销毁两个旧饮料，从预制体数组实例化高级饮料，并播放动画
/// </summary>
/// <param name="source"></param>
/// <param name="target"></param>
/// <param name="targetIndex"></param>
private void Merge(DrinkItem source, DrinkItem target, int targetIndex)
    {
        int newLevel = target.Level + 1;
        if (drinkPrefabs == null || newLevel > drinkPrefabs.Length)
        {
            Debug.LogError($"合成 Lv{newLevel} 失败：drinkPrefabs 未赋值或等级不足!");
            UIHintPanel.Instance.ShowHint("合成失败");
            return;
        }
Vector3  targetPos = cells[targetIndex].position;

        highlight?.UnregisterDrink(source);
        highlight?.UnregisterDrink(target);
DestroyImmediate(target.gameObject);
DestroyImmediate(source.gameObject);

GameObject prefab = drinkPrefabs[newLevel - 1];
if(prefab == null)
        {
           Debug.LogError($"合成后等级{newLevel}的饮料预制体未赋值:drinkPrefabs索引{newLevel-1}!");
             return;
        }
GameObject newGo = Instantiate(prefab, drinkPanel); //实例化新的饮料对象
RectTransform newRT = newGo.GetComponent<RectTransform>();
newRT.position = targetPos; //将新饮料定位到目标格子位置
newRT.localScale = Vector3.one; //确保新饮料缩放正常

DrinkItem drink = newGo.GetComponent<DrinkItem>();
if(drink == null)
        {
            Debug.LogError($"合成后等级{newLevel}的饮料预制体缺少DrinkItem组件!");
            DestroyImmediate(newGo); //销毁错误的饮料对象
             return;
        }
        drink.Init(newLevel, targetIndex, this); //初始化新饮料信息（等级、所在格子索引、面板引用）
        cellpccupamts[targetIndex] = drink; //更新目标格子占用信息，标记该格子被新饮料占用
                                            //光晕
        Outline outline = newGo.AddComponent<Outline>();
        outline.effectColor = new Color(245, 255, 0, 1);
        outline.effectDistance = new Vector2(5, 5);
        outline.enabled = false;
        highlight?.RegisterDrink(drink, outline);


        drink.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f,8,0.8f); //播放弹出动画
        UIHintPanel.Instance.ShowHint($"合成 Lv{newLevel}"); //显示合成成功提示文本

        // 合成后通知订单区刷新高亮
        UIOrderPanel display = FindObjectOfType<UIOrderPanel>();
        if (display != null) display.TriggerHighlightUpdate();

    }
    /// <summary>
    /// 将饮料动画移回指定格子，带OutBack弹性效果
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cellIndex"></param>
    private void ReturnToCell(DrinkItem item, int cellIndex)
    {
        if(cellIndex<0 || cellIndex >= cells.Count)
        {
           DestroyImmediate(item.gameObject); //如果格子索引无效，销毁饮料对象
             return;
        }
item.transform.DOKill();
item.transform.DOMove(cells[cellIndex].position, 0.15f).SetEase(Ease.OutBack); //将饮料平滑移动回原格子位置
item.SetCellIndex(cellIndex); //更新饮料的当前格子索引
cellpccupamts[cellIndex] = item; //更新格子占用信息

    }
    private void GenerateDeliveryCells()
    {
        if(deliveryArea==null) return;
        deliveryCells.Clear();
        deliveryCellDrinks = new DrinkItem[deliveryCount];
        for (int i = 0; i < deliveryCount; i++) 
        {
        GameObject cell = new GameObject($"DeliveryCell_{i}",typeof(RectTransform),typeof(Image));
            RectTransform rt =cell.GetComponent<RectTransform>();
            cell.transform.SetParent(deliveryArea,false);
            deliveryCells.Add(rt);

        }
    }

    private int FindNearestDeliveryCell(Vector2 worlfPos, out float minDist)
    {
        minDist = float.MaxValue;
        int nearest = -1;
        for (int i = 0; i < deliveryCount; i++)
        {
            float dist = Vector2.Distance(worlfPos, deliveryCells[i].position);
            if(dist < minDist) { minDist = dist; nearest = i; }
        }
        return nearest;
    }


    private void MoveToDeliveryCell(DrinkItem item,int cellIndex)
    {
        if(deliveryCellDrinks[cellIndex] != null)
        {
            int empty = GetSmallestEmptyBoardCell();
            if(empty >= 0) ReturnToCell(deliveryCellDrinks[cellIndex],empty);
            else DestroyImmediate(deliveryCellDrinks[cellIndex].gameObject);
        }
        item.transform.DOKill();
        Vector2 originalSize =item.GetComponent<RectTransform>().sizeDelta;
        item.transform.SetParent(deliveryCells[cellIndex],true);
        item.GetComponent<RectTransform>().sizeDelta = originalSize;
        item.transform.DOMove(deliveryCells[cellIndex].position, 0.2f).SetEase(Ease.OutQuad);
        item.SetCellIndex(-1);
        deliveryCellDrinks[cellIndex]= item;
    }

    private bool IsInDeliveryArea(Vector2 worldPos)
    {
        if (deliveryArea == null) return false;
        Vector3[] corners = new Vector3[4];
        deliveryArea.GetWorldCorners(corners);
        return worldPos.x >= corners[0].x && worldPos.x <= corners[2].x
        && worldPos.y >= corners[0].x && worldPos.y <= corners[2].y;


    }


    private int GetSmallestEmptyBoardCell()
    {
        for (int i = 0; i < cellpccupamts.Length; i++)
        {
            if (cellpccupamts[i] == null) return i;
        }
        return -1;
    }

    private void OnDeliverySubmit()
    {
        // 收集交付区所有饮料
        List<DrinkItem> pendingList = new List<DrinkItem>();
        for (int i = 0; i < deliveryCellDrinks.Length; i++)
        {
            if (deliveryCellDrinks[i] != null)
            {
                pendingList.Add(deliveryCellDrinks[i]);
                deliveryCellDrinks[i] = null;
            }
        }
        if (pendingList.Count == 0) { UIHintPanel.Instance.ShowHint("请先拖拽饮料到交付区"); return; }

        UIOrderPanel d = FindObjectOfType<UIOrderPanel>();
        if (d == null) return;

        MockOrderData target = null;
        for (int i = 0; i < d.mockOrders.Count; i++)
        {
            if (!d.mockOrders[i].isCompleted) { target = d.mockOrders[i]; break; }
        }
        if (target == null) { ReturnItems(pendingList); UIHintPanel.Instance.ShowHint("所有订单已完成"); return; }

        bool anySuccess = false;
        for (int p = 0; p < pendingList.Count; p++)
        {
            DrinkItem drink = pendingList[p];
            bool matched = false;
            for (int n = 0; n < target.needs.Count; n++)
            {
                if (drink.Level == target.needs[n].level && target.needs[n].count > 0)
                {
                    target.needs[n].count--;
                    DestroyImmediate(drink.gameObject);
                    matched = true;
                    anySuccess = true;
                    break;
                }
            }
            if (!matched)
            {
                int empty = GetSmallestEmptyBoardCell();
                if (empty >= 0) 
                {
                    drink.transform.SetParent(drinkPanel, true);
                    ReturnToCell(drink, empty);
                }
                else { UIHintPanel.Instance.ShowHint("棋盘格已满"); break; }
            }
        }

        // 检查订单是否全部完成
        bool allDone = true;
        for (int n = 0; n < target.needs.Count; n++)
            if (target.needs[n].count > 0) { allDone = false; break; }

        if (allDone)
        {
            target.isCompleted = true;
            UIHintPanel.Instance.ShowHint("订单已完成");
        }
        else if (anySuccess)
        {
            UIHintPanel.Instance.ShowHint("交付成功，该订单还有其他需求");
        }
        else
        {
            UIHintPanel.Instance.ShowHint("物品不符合当前订单");
        }
        d.TriggerHighlightUpdate();
    }

    private void ReturnItems(List<DrinkItem> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            int empty = GetSmallestEmptyBoardCell();
            if (empty >= 0) 
            {
                ReturnToCell(items[i], empty);
            }
            else DestroyImmediate(items[i].gameObject);
        }
    }


}
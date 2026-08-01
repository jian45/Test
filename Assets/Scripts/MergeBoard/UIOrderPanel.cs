using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientFramework.UI;

/// <summary>
/// 订单单个需求项——指定需要什么等级、需要几个
/// </summary>
[System.Serializable]
public class ItemNeed
{
    public int level = 2;
    public int count = 1;
}

/// <summary>
/// 订单目标数据——一个订单可包含多个需求项
/// </summary>
[System.Serializable]
public class MockOrderData
{
    public string displayName;                    // 显示名，如"顾客A"
    public List<ItemNeed> needs = new List<ItemNeed>() { new ItemNeed() }; // 需求列表
    public string orderText;                      // 需求描述文本
    public bool isCompleted;                      // 是否已完成
}

/// <summary>
/// 订单目标显示区主控——管理 3 槽横排、导航切换、提示文、棋盘高亮联动
/// </summary>
public class UIOrderPanel :UIBase
{
    [Header("订单槽预制体")]
    [Tooltip("单个订单槽的预制体，运行时实例化到 Content 下")]
    public GameObject orderSlotPrefab;

    [Header("槽的父级容器")]
    [Tooltip("订单槽实例化时的父物体（Scroll View/Viewport/Content）")]
    public Transform slotParent;

    [Header("导航按钮")]
    [Tooltip("左移按钮——点击后窗口左移一个订单")]
    public ButtonExt el_btnLeft;
    [Tooltip("右移按钮——点击后窗口右移一个订单")]
    public ButtonExt el_btnRight;



    [Header("详情面板")]
    [Tooltip("点击订单槽时弹出的详情面板")]
    public GameObject detailPanel;
    [Tooltip("详情面板上的关闭按钮")]
    public ButtonExt el_detailCloseButton;

    [Header("订单数据（直接填字段）")]
    [Tooltip("在 Inspector 中填写的 Mock 订单列表")]
    public List<MockOrderData> mockOrders;

    private List<GameObject> allSlotGOs;     // 所有实例化出来的订单槽
    private int currentIndex;                 // 当前窗口中间对应的订单索引
    private BoardHighlight board;            // 高亮系统引用，用于高亮联动
    private const int WINDOW_SIZE = 3;       // 固定显示 3 个槽

    /// <summary>
    /// 启动：找棋盘、绑按钮、生成槽、刷新窗口、隐藏详情面板
    /// </summary>
    public void Start()
    {
        board = FindObjectOfType<BoardHighlight>();

        el_btnLeft?.AddClick(OnBtnLeftClick);
        el_btnRight?.AddClick(OnBtnRightClick);
        el_detailCloseButton?.AddClick(HideDetail);

        allSlotGOs = new List<GameObject>();
        GenerateAllSlots();
        RefreshWindow();
        detailPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        el_btnLeft?.RemoveClick(OnBtnLeftClick);
        el_btnRight?.RemoveClick(OnBtnRightClick);
        el_detailCloseButton?.RemoveClick(HideDetail);
    }

    /// <summary>
    /// 遍历 mockOrders，在 slotParent 下实例化订单槽预制体
    /// </summary>
    public void GenerateAllSlots()
    {
        for (int i = 0; i < mockOrders.Count; i++)
        {
            GameObject go = Instantiate(orderSlotPrefab, slotParent);
            allSlotGOs.Add(go);

            ItemOrderSlot slot = go.GetComponent<ItemOrderSlot>();
            slot.display = this;
        }
    }

    /// <summary>
    /// 刷新窗口：计算显示范围、填数据、更新提示文和高亮
    /// </summary>
    public void RefreshWindow()
    {
        int total = allSlotGOs.Count;
        for (int i = 0; i < allSlotGOs.Count; i++)
            allSlotGOs[i].SetActive(false);

        int start = Mathf.Max(0, currentIndex - 1);
        int end = Mathf.Min(total - 1, currentIndex + 1);

        for (int i = start; i <= end; i++)
        {
            allSlotGOs[i].SetActive(true);
            ItemOrderSlot slot = allSlotGOs[i].GetComponent<ItemOrderSlot>();
            slot.displayName = mockOrders[i].displayName;
            slot.targetLevel = mockOrders[i].needs[0].level;
            slot.orderText = mockOrders[i].orderText;
            slot.RefreshUI();
        }
        UpdatePrompText();
        TriggerHighlightUpdate();

        // 如果详情面板已打开，同步更新内容
        if (detailPanel.activeSelf)
        {
            MockOrderData current = mockOrders[currentIndex];
            Text detailText = detailPanel.GetComponentInChildren<Text>();
            if (detailText != null)
                detailText.text = current.displayName + "\n" + current.orderText;
        }
    }

    /// <summary>
    /// 左移按钮事件——窗口左移，回绕
    /// </summary>
    public void OnBtnLeftClick()
    {
        int total = mockOrders.Count;
        currentIndex = (currentIndex - 1 + total) % total;
        RefreshWindow();
    }

    /// <summary>
    /// 右移按钮事件——窗口右移，回绕
    /// </summary>
    public void OnBtnRightClick()
    {
        int total = mockOrders.Count;
        currentIndex = (currentIndex + 1 + total) % total;
        RefreshWindow();
    }

    /// <summary>
    /// 更新提示文本——检测棋盘上是否有当前订单目标等级的饮料
    /// </summary>
    public void UpdatePrompText()
    {
        MockOrderData current = mockOrders[currentIndex];

        bool hasTarget = false;
        for (int n = 0; n < current.needs.Count; n++)
        {
            if (board.HasDrinkAtLevel(current.needs[n].level))
            {
                hasTarget = true;
                break;
            }
        }

        UIHintPanel.Instance.SetOrderPrompt(hasTarget ? "棋盘上有目标物品，可交付" : "请继续合成目标饮品");
    }

    /// <summary>
    /// 触发棋盘高亮——找到第一个未完成的订单，高亮对应等级的饮料
    /// </summary>
    public void TriggerHighlightUpdate()
    {
        if (board == null || mockOrders == null || mockOrders.Count == 0 || currentIndex >= mockOrders.Count)
            return;
        MockOrderData current = null;
        for (int i = 0; i < mockOrders.Count; i++)
        {
            if (!mockOrders[i].isCompleted)
            {
                current = mockOrders[i];
                break;
            }
        }
        if (current == null) { board.ClearHighlights(); return; }

        List<int> allMatch = new List<int>();
        for (int n = 0; n < current.needs.Count; n++)
        {
            List<int> matches = board.GetCellIndexesByLevel(current.needs[n].level);
            int takeCount = Mathf.Min(current.needs[n].count, matches.Count);
            for (int i = 0; i < takeCount; i++)
                allMatch.Add(matches[i]);
        }

        if (allMatch.Count > 0)
            board.SetHighlightedCells(allMatch);
        else
            board.ClearHighlights();
    }

    /// <summary>
    /// 弹出详情面板——显示该订单的名称和需求描述
    /// </summary>
    public void ShowDetail(ItemOrderSlot slot)
    {
        Text detailText = detailPanel.GetComponentInChildren<Text>();
        if (detailText != null)
        {
            detailText.text = slot.displayName + "\n" + slot.orderText;
        }
        detailPanel.SetActive(true);
    }

    /// <summary>
    /// 隐藏详情面板
    /// </summary>
    public void HideDetail()
    {
        detailPanel.SetActive(false);
    }

    /// <summary>
    /// 外部添加订单——一行命令生成一个订单及其槽
    /// </summary>
    public void AddOrder(string name, int level, string text)
    {
        mockOrders.Add(new MockOrderData()
        {
            displayName = name,
            needs = new List<ItemNeed>() { new ItemNeed() { level = level, count = 1 } },
            orderText = text
        });

        ItemOrderSlot slot = ItemOrderSlot.Create(orderSlotPrefab, slotParent.transform, name, level, text);
        slot.display = this;
        allSlotGOs.Add(slot.gameObject);

        RefreshWindow();
    }
}

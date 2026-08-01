using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DevPanelManager : MonoBehaviour
{
    [Header("占位面板")]
    public GameObject groupAPlaceholder;
    public GameObject groupBPlaceholder;
    public GameObject groupB1Placeholder;
    public GameObject groupB2Placeholder;
    public GameObject groupDPlaceholder;
    public GameObject groupD1Placeholder;


    [Header("当前激活的面板")]
    private GameObject currentOpenPanel;

    void Start()
    {
        // 确保所有占位面板初始隐藏
        if (groupAPlaceholder != null) groupAPlaceholder.SetActive(false);
        if (groupBPlaceholder != null) groupBPlaceholder.SetActive(false);
        if (groupDPlaceholder != null) groupB1Placeholder.SetActive(false);
        if (groupBPlaceholder != null) groupB2Placeholder.SetActive(false);
        if (groupDPlaceholder != null) groupDPlaceholder.SetActive(false);
        if (groupDPlaceholder != null) groupD1Placeholder.SetActive(false);
    }

    public void OpenGroupA()
    {
        Debug.Log("[DevEntry] === 打开 A 组棋盘 Demo ===");
        Debug.Log("[DevEntry] 未来行为：加载 A 组 BoardScene，挂到 MainPanel/BoardArea");
        OpenPlaceholder(groupAPlaceholder);
    }

    public void OpenGroupB1()
    {
        Debug.Log("[DevEntry] === 打开 B 组订单 Demo ===");
        Debug.Log("[DevEntry] 未来行为：加载 B 组 OrderScene，挂到 MainPanel/OrderArea");
        OpenPlaceholder(groupB1Placeholder);
    }
    public void OpenGroupB2()
    {
        Debug.Log("[DevEntry] === 打开 B 组修复 Demo ===");
        Debug.Log("[DevEntry] 未来行为：加载 B 组 RepairArea，挂到 MainPanel/RepairArea");
        OpenPlaceholder(groupB2Placeholder);
    }

    public void OpenGroupB()
    {
        Debug.Log("[DevEntry] === 打开 B 组咖啡机 Demo ===");
        Debug.Log("[DevEntry] 未来行为：加载 B 组 ProducerArea，挂到 MainPanel/ProducerArea");
        OpenPlaceholder(groupBPlaceholder);
    }
    public void OpenGroupD()
    {
        Debug.Log("[DevEntry] === 打开 D 组结算 Demo ===");
        Debug.Log("[DevEntry] 未来行为：加载 D 组 SettlementArea，挂到 MainPanel/SettlementArea");
        OpenPlaceholder(groupDPlaceholder);
    }
    public void OpenGroupD1()
    {
        Debug.Log("[DevEntry] === 打开 D 组猫咪反馈 Demo ===");
        Debug.Log("[DevEntry] 未来行为：加载 D 组 CatFeedbackArea，挂到 MainPanel/CatFeedbackArea");
        OpenPlaceholder(groupD1Placeholder);
    }
    void OpenPlaceholder(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("[DevEntry] 占位面板未绑定，请在 Inspector 中指定。");
            return;
        }

        // 关闭之前打开的面板
        if (currentOpenPanel != null && currentOpenPanel != panel)
        {
            currentOpenPanel.SetActive(false);
        }

        // 切换当前面板状态
        bool isActive = !panel.activeSelf;
        panel.SetActive(isActive);
        currentOpenPanel = isActive ? panel : null;

        Debug.Log($"[DevEntry] 占位面板 {(isActive ? "打开" : "关闭")}：{panel.name}");
    }
}
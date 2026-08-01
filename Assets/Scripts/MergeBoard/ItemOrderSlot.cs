using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 单个订单槽——挂载到每个订单槽预制体上
/// 存数据 + 拖 UI + 点击弹出详情
/// </summary>
public class ItemOrderSlot : MonoBehaviour, IPointerClickHandler
{
    // ═══ 数据 ═══
    public string displayName;   // 显示名，如"顾客A"
    public int targetLevel;      // 目标等级，如 2 对应 drink_lv2
    public string orderText;     // 需求描述文本，如"drink_Lv2 * 1"

    // ═══ UI 组件（Inspector 拖） ═══
    public Text nameText;        // 显示 displayName 的 Text 组件
    public Text requireText;     // 显示 orderText 的 Text 组件
    public Image slotImage;      // 槽的背景图

    // ═══ 主控引用（运行时代码赋值） ═══
    [HideInInspector]
    public UIOrderPanel display;

    /// <summary>
    /// 把数据刷新到 UI 组件上
    /// </summary>
    public void RefreshUI()
    {
        if (nameText != null) nameText.text = displayName;
    }

    /// <summary>
    /// 点击槽——弹出详情面板
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (display != null)
        {
            display.ShowDetail(this);
        }
    }

    /// <summary>
    /// 静态创建方法——实例化预制体、赋值数据、刷新 UI
    /// </summary>
    public static ItemOrderSlot Create(GameObject prefab, Transform parent, string name, int Level, string text)
    {
        GameObject go = Instantiate(prefab, parent);
        ItemOrderSlot slot = go.GetComponent<ItemOrderSlot>();
        slot.targetLevel = Level;
        slot.orderText = text;
        slot.displayName = name;
        slot.RefreshUI();
        return slot;
    }
}

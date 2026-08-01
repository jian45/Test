using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//====================================================================
// [A-1 新增] 单个订单项 UI 脚本
// 挂载到 Scroll View / Content 下的每个订单预制体上
// 用 IPointerClickHandler 检测点击，不需要额外 Button 组件
//====================================================================
/// <summary>
/// 单个订单项——显示在 Scroll View Content 下的每个订单 UI
/// 点击后弹出详情面板
/// </summary>
public class OrderTargetItemData : MonoBehaviour, IPointerClickHandler
{

    public string displayName;//显示名称 例如"顾客A"
    public int targetLevel;//目标等级，比如Lv2
    public string requireText;//显示文本，例如drink_Lv2*1

    public Text nameText; //显示displayName;
    public Text requireTextUI;//显示requireText
    public Image slotImage; //槽的背景图

    //引用主控
    [HideInInspector]//隐藏下面字段
    public OrderTargetDisplay display;
    /// <summary>
    /// 吧数据加载到UI上
    /// </summary>
    public void RefreshUI()
    {
       if(nameText!=null)nameText.text = displayName;
    
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (display != null) 
        {
            display.ShowDetail(this);
        }
    }

    public static OrderTargetItemData Create(GameObject prefab,Transform parent,string name,int Level,string text)
    {GameObject go =Instantiate(prefab,parent);
        OrderTargetItemData slot=go.GetComponent<OrderTargetItemData>();
        slot.targetLevel = Level;
        slot.requireText = text;
        slot.displayName = name;
        slot.RefreshUI();
        return slot;
    }
}
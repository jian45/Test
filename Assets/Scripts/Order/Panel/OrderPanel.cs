using System;
using ClientFramework.UI;
using UnityEngine;
using UnityEngine.UI;

public class OrderPanel : UIBase
{
    //完成按钮
    public Button btnFinish;
    //隐藏按钮
    public Button btnHide;
    //客户需求文本
    public Text txtDemand;
    //进度条
    public Image Progress;
    //进度条比例
    private float progress;
    //进度条文本 
    public Text txtProgress;
    //客户头像
    public Image Avatarre;
    //所需物品栏
    public ScrollRect Demand;

    //订单提交成功回调（由外部注入，用于禁用或者删除对应的 OrderCard）
    private Action OnCompleted;
    //订单数据
    private OrderData orderData;

    protected override void OnCreate()
    {
        base.OnCreate();
        btnFinish.interactable = false;
        
    }
    protected override void BindEvents()
    {
        base.BindEvents();
        //完成按钮被按下时
        btnFinish.onClick.AddListener(() =>
        {
            //销毁对应的 OrderCard
            OnCompleted?.Invoke();
            //提供奖励
            PlayerResources.Instance.AddOrderReward(orderData.rewardCatCoins, orderData.rewardHearts, orderData.rewardRepairMaterials);
            DataMgr.Resource_Add("catCoin", orderData.rewardCatCoins, "order_reward");
            DataMgr.Resource_Add("heart", orderData.rewardHearts, "order_reward");
            DataMgr.Resource_Add("repairMaterial", orderData.rewardRepairMaterials, "order_reward");
            //隐藏自己
            UIManager.Instance.ClosePanel<OrderPanel>();


        });
        //关闭按键按下时
        btnHide.onClick.AddListener(() =>
        {
            //隐藏自己
            UIManager.Instance.ClosePanel<OrderPanel>();
        });
    }

  
    //注入订单数据并更新面板显示（后续可扩展加载头像、生成物品栏）
    public void SetOrder(OrderData orderData) 
    {
       this.orderData = orderData;
       
    }
    /// <summary>
    /// 
    /// </summary>
    public override void UIUpdate()
    {
        if (orderData == null)
        {
            return;
        }
        base.UIUpdate();
        //根据传入的名字动态加载头像
        Avatarre.sprite = AvatarManager.Instance.GetSprite(orderData.customerName);

        //所需物品栏显示的饮品通过订单所需物品名字动态生成原理与ui面板的生成逻辑相同
        string demandText = "";
        foreach (var item in orderData.items)
        {
            if (demandText.Length > 0) demandText += "\n";
            demandText += $"{item.DrinkName} x {item.Quantity}";
        }
        txtDemand.text = $"{orderData.customerName}想要\n{demandText}";

        //更新显示
       
       
        //转换进度条比例分数
        progress = (float)orderData.nowQuantity / orderData.needQuantity;
        //更新进度条
        Progress.fillAmount = progress;
        //完成时强制进度条为一
        if (Progress.fillAmount > 1)
        {
          Progress.fillAmount = 1;
        }
        //更新进度条文本
        txtProgress.text = $"{orderData.nowQuantity}/ {orderData.needQuantity}";
         //更新提交按钮状态
        if (orderData.IsFulfilled)
         {
             btnFinish.interactable = true;
         }
        else
         {
                btnFinish.interactable = false;
         }
        
    }
    /// <summary>
    /// 注入订单完成回调事件
    /// </summary>
    /// <param name="callbck">OrderCard 传入自身禁用逻辑</param>
    public void SetOnCompleted(Action callbck) 
    {
        OnCompleted = callbck;
    }
    /// <summary>
    /// 隐藏后的注销事件
    /// </summary>
    protected override void UnbindEvents()
    {
        base.UnbindEvents();
        btnFinish.onClick.RemoveAllListeners();
        btnHide.onClick.RemoveAllListeners();
    }
}

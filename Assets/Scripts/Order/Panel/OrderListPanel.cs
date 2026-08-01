using System;
using System.Collections;
using System.Collections.Generic;
using ClientFramework.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 订单列表面板：管理多张 OrderCard
/// M0.2 新增：当前选中订单模式，DeliverDrink 只交付给选中订单
/// </summary>
public class OrderListPanel : UIBase
{
    /// <summary>滚动视图容器，OrderCard 挂载到其 Content 下</summary>
    public ScrollRect OrderView;

   
    //订单列表：存入所有生成的订单卡片，便于后续查找和销毁
    public List<OrderCard> Orderlist =new List<OrderCard>();
    /// <summary>当前选中的订单数据（点击卡片时设置）</summary>
    public OrderData CurrentSelectedOrder { get; private set; }

    // 统计数据
    /// <summary> 当前批次数量 </summary>
    private int compleredCount;
    /// <summary> 当前批次奖励数量 </summary>
    private int totalCounts, totalHearts, totalMaterials;


    
    /// <summary>
    /// 外部调用：添加一张订单卡片
    /// </summary>
    /// <param name="orderId">订单 ID（用于精确定位和修改）</param>
    /// <param name="data">订单数据（注入卡片）</param>
    public void AddOrderCard( OrderData data)
    {
        //CreateOrderCard( (Card) => { Card.BindData(data); });
        
        CreateOrderCard(data);
    }
    /// <summary>
    /// 内部方法：通过 Addressables 异步加载卡片预制体
    /// </summary>
    /// <param name="orderId">订单 ID，生成完成后存入字典</param>
    
    private void CreateOrderCard(OrderData data) 
    {
        ABManager.Instance.InstantiatePrefab(
             "Prefab/Panel/OrderCard",
             Vector3.zero,
             Quaternion.identity,
             null,
            onSuccess: instance =>
            {
                //挂到滚动视图的 Content 下
                instance.transform.SetParent(OrderView.content,false );
                //获取预制体上的 OrderCard 脚本
                OrderCard card = instance.GetComponent<OrderCard>();
                //存入列表
                card.BindData(data,this);
                Orderlist.Add(card);

              

            }, onFailed: error =>
            {
                Debug.LogError($"加载失败: {error}");
            }

            );
    }
    /// <summary>设置当前选中订单（由 OrderCard 点击时调用）</summary>

    public void SetSelectedOrder(OrderData orderData) 
    {
        CurrentSelectedOrder = orderData;
    }
    /// <summary>
    /// 交付物品：遍历所有卡片按物品名匹配
    /// 正确匹配推进度，无匹配输出警告
    /// </summary>
    public void DeliverDrink(string drinkName) 
    {
        if (CurrentSelectedOrder == null || CurrentSelectedOrder.IsFulfilled) 
        {
          Debug .Log ("当前未选中有效订单");
 
            UIManager.Instance.OpenPanel<TipPanel>
                (
                UIPanelConfigs.TipPanel,
                (Panel) => {
                    Panel.ChangedTxt("当前未选中有效订单");
                }, error => { }
                );
            return;
        }
        foreach (var item in CurrentSelectedOrder .items )
        {
            if (item.DrinkName == drinkName)
            {
                CurrentSelectedOrder.nowQuantity++;
                //更新订单面板的信息
                UIManager.Instance.GetPanel<OrderPanel>()?.UIUpdate();
                return;
            } 
        }
        string demandText = "";
        foreach (var de in CurrentSelectedOrder.items)
        {
            if (demandText.Length > 0) demandText += "\n";
            demandText += $"<color=red><b>{de.DrinkName}</b> </color>x {de.Quantity}";
        }

        UIManager.Instance.OpenPanel<TipPanel>(
            UIPanelConfigs.TipPanel,
            (panel) => {
            panel.ChangedTxt($"{CurrentSelectedOrder.customerName}想要\n{demandText}");
            });
    }
    /// <summary>
    /// 清空所有订单卡片（销毁 GameObject 并清空列表）
    /// </summary>
    public void ClearOrders() 
    {
        foreach (var card in Orderlist)
        {
            if (card != null && card.gameObject != null) 
            {
                ABManager.Instance.ReleaseInstance(card.gameObject);
            }
        }
        Orderlist.Clear();
        CurrentSelectedOrder = null;
        compleredCount = 0;
        totalCounts = 0;
        totalHearts = 0;
        totalMaterials = 0;
    
    }
    public void OnOrderCompleted(OrderData data) 
    {
        compleredCount++;
        totalCounts += data.rewardCatCoins;
        totalHearts += data.rewardHearts;
        totalMaterials += data.rewardRepairMaterials;
        //使用list长度来判断是否所有订单完成，避免重复计算
        if (compleredCount>=Orderlist.Count) 
        {
        Debug.Log($"本批次订单已完成，奖励：猫币 {totalCounts}个，爱心 {totalHearts}个，修理材料 {totalMaterials}个");
           
        }
    }
}

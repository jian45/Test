using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 订单数据：记录当前订单进度、需求物品、奖励信息
/// M0.2 新增：订单ID/批次号/顾客名/奖励字段
/// 
/// /// TODO M1: 当前 nowQuantity/needQuantity 单数字无法区分多物品交付
/// 例如订单需要 drink_lv2×2 + drink_lv2_tea×1，交付 3 杯 tea 也会完成
/// M1 应替换为学长的 requiredItems/submittedItems 列表比对方案
/// </summary>
public class OrderData 
{

    //当前已交付数量
    public int nowQuantity=0;
    //目标总需求量（私有，由构造函数累加）
    private int NeedQuantity = 1;
    //对外只读：目标总需求量
    public int needQuantity=> NeedQuantity;  //保证 IsFulfilled不会一开始就true
    /// <summary>订单唯一标识，如 order_b1_01_drink_lv2</summary>
    public string orderID;
    /// <summary>批次号，如 B1/B2/B3</summary>
    public string batchID;
    /// <summary>顾客名，用于 UI 显示</summary>
    public string customerName;
    /// <summary>是否触发猫咪服务（给 D 组预留）</summary>
    public bool triggerCat = false;
    /// <summary>需求物品列表</summary>
    public List<OrderItem> items = new List<OrderItem>();
    //是否已满足：当前数量 == 目标数量
    public bool IsFulfilled => nowQuantity == needQuantity;
    //奖励数据(猫币，爱心，修复材料)
    public int rewardCatCoins;
    public int rewardHearts;
    public int rewardRepairMaterials;
    
    /// <summary>
    /// 构造订单数据（支持一个必选物品 + 多个可选物品）
    /// NeedQuantity 初始为 1 占位，累加后减 1 抵消
    /// </summary>
    public OrderData(string orderID,string batchID,string customerName,
        bool istriggerCat, int catCoins, int hearts, int repairMaterials, 
        OrderItem orderItem, params OrderItem[] OrderItem ) 
    {
        this.orderID = orderID;
        this.batchID = batchID;
        this.customerName = customerName;
        this.triggerCat = istriggerCat;
        this.rewardCatCoins = catCoins;
        this.rewardHearts = hearts;
        this.rewardRepairMaterials = repairMaterials;
        //当只传入一种饮品类型时
        NeedQuantity += orderItem.Quantity;
        items .Add( orderItem );
        //当传入复数种类的饮品时
        if (OrderItem.Length > 0)
        {
            for (int i = 0; i < OrderItem.Length; i++)
            {

                NeedQuantity += OrderItem[i].Quantity;
                items.Add(OrderItem[i]);
            }
        }
        NeedQuantity = needQuantity - 1;//减一才为正常需求
    }


}
/// <summary>
/// 单个饮品信息：物品名 + 数量
/// </summary>
public class OrderItem 
{
    public OrderItem (string DrinkName , int Quantity)
    {
        this.DrinkName = DrinkName;
        this.Quantity = Quantity;
        
    }
    //饮品名称
    public string DrinkName;
    //所需数量
    public int Quantity;
 
}
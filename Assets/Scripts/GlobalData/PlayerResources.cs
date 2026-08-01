using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家资源数据（单例）：管理猫币、爱心、修复材料
/// 各模块通过 AddReward / OnResourcesChanged 读写
/// </summary>
public class PlayerResources 
{
    //全局单例（懒加载）
    private static  PlayerResources instance;
    //全局访问点
    public static PlayerResources Instance => instance ?? (instance = new PlayerResources());

    //当前猫币数量
    public int CatCoins { get; private set; }
    //当前爱心数量
    public int Hearts { get; private set; }
    //当前修复材料数量
    public int RepairMaterials { get; private set; }
    //已完成订单数
    public int Orderquantity { get; private set; }

    //资源数量变化时触发，供 UI 层刷新显示
    public event Action OnResourcesChanged;
    /// <summary>
    /// 增加猫币
    /// </summary>
    /// <param name="amount">增加数量</param>
    public void AddCatCoins(int amount)
    {
        CatCoins = CatCoins + amount;
        OnResourcesChanged?.Invoke();
    }
    /// <summary>
    /// 增加爱心数量
    /// </summary>
    /// <param name="amount">增加数量</param>
    public void AddHearts(int amount) 
    {
        Hearts = Hearts + amount;
        OnResourcesChanged?.Invoke();
    }
    /// <summary>
    /// 增加维修材料
    /// </summary>
    /// <param name="amount">增加数量</param>
    public void AddRepairMaterials(int amount) 
    {
        RepairMaterials = RepairMaterials + amount;
        OnResourcesChanged?.Invoke();
    }
    /// <summary>
    /// 增加完成订单数
    /// </summary>
    /// <param name="amount">增加数量</param>
    public void AddOrderquantity(int amount=1) 
    {
        Orderquantity = Orderquantity + 1;
        OnResourcesChanged?.Invoke();
    }
    /// <summary>
    /// 一次性添加订单完成奖励（猫币 + 爱心 + 修复材料 + 订单数）
    /// </summary>
    /// <param name="Coins">猫币奖励</param>
    /// <param name="Hearts">爱心奖励</param>
    /// <param name="RepairMaterials">修复材料奖励</param>
    public void AddOrderReward(int Coins, int Hearts, int RepairMaterials) 
    {
        CatCoins += Coins;
        this.Hearts += Hearts;
        this.RepairMaterials += RepairMaterials;
        Orderquantity++;
        OnResourcesChanged?.Invoke();
    }
    /// <summary>
    /// 扣除猫币（用于购买/升级等消耗）
    /// </summary>
    public bool  DecCatCoins(int amount) 
    {
        if (amount > CatCoins)
        {
            return false;
        }
        else 
        {
            CatCoins -= amount;
            OnResourcesChanged?.Invoke();
            return true;
        }
      
    }
    /// <summary>
    /// 扣除爱心（用于猫咪互动等消耗）
    /// </summary>
    public bool DecHearts(int amount) 
    {
        if (amount > Hearts)
        {
            return false;
        }
        else 
        {
            Hearts -= amount;
            OnResourcesChanged?.Invoke();
            return true;
        }
        
    }
    /// <summary>
    /// 扣除修复材料（用于修复猫咖节点）
    /// </summary>
    public bool DecRepairMaterials(int amount) 
    {
        if (amount > RepairMaterials)
        {
            return false;
        }
        else 
        {
            RepairMaterials -= amount;
            OnResourcesChanged?.Invoke();
            return true;
        }
       
    }
    /// <summary>
    /// 重置订单数（新批次开始时调用）
    /// </summary>
    public void NullOrderquantity() 
    {
        Orderquantity = 0;
    }
}

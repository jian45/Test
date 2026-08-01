using System.Collections;
using System.Collections.Generic;
using ClientFramework.UI;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// M0 测试脚本：批次选择、模拟交付、资源显示
/// M0.2 新增：Dropdown 切换 B1/B2/B3，addLv2/Lv3 模拟棋盘交付
/// </summary>
public class LYJtest : MonoBehaviour
{
    //订单列表面板
    private OrderListPanel orderList;
    /// <summary>模拟棋盘交付 drink_lv2</summary>
    public Button addLv2;
    /// <summary>模拟棋盘交付 drink_lv3</summary>
    public Button addLv3;
   

    //*****************************************B——3测试*******************
    //猫币数量
    public Text txtCatCoins;
    //爱心数量
    public Text txtHearts;
    //维修材料
    public Text txtRepairMaterials;
 

    //*****************************************M0.2测试*******************
    /// <summary>批次选择下拉框：0=B1, 1=B2, 2=B3</summary>
    public Dropdown dayorder;
   
    void Start()
    {
        DataMgr.Init();  // 初始化存档,正式场景需移除
       // 全量解锁修复节点，资源不足自然不可修复（软封锁）
        DataMgr.Repair_UnlockNode("repair_r2_bowl_mat");
        DataMgr.Repair_UnlockNode("repair_r3_welcome_light");



        //通过 UIManager 异步加载 OrderListPanel
        //内部通过ABManager 异步加载 OrderData

        UIManager.Instance.OpenPanel<OrderListPanel>(
            UIPanelConfigs.OrderListPanel, 
            (Panel) => 
            {
                orderList = Panel;
                // 加载 B1
                orderList.ClearOrders();
                List<OrderData> orders = MockOrderDatabase.GetOrdersForBatch("B1");
                foreach (var data in orders)
                {
                    orderList.AddOrderCard(data);
                }
                //各批次订单
                dayorder.onValueChanged.AddListener((index) =>
                {
                    orderList.ClearOrders();
                    switch (index)
                    {
                        case 0:
                            List<OrderData> orderDatas = MockOrderDatabase.GetOrdersForBatch("B1");
                            foreach (var data in orderDatas)
                            {
                                orderList.AddOrderCard(data);
                            }
                            break;
                        case 1:
                            List<OrderData> orderDatas2 = MockOrderDatabase.GetOrdersForBatch("B2");
                            foreach (var data in orderDatas2)
                            {
                                orderList.AddOrderCard(data);
                            }
                            break;
                        case 2:
                            List<OrderData> orderDatas3 = MockOrderDatabase.GetOrdersForBatch("B3");
                            foreach (var data in orderDatas3)
                            {
                                orderList.AddOrderCard(data);
                            }
                            break;
                        default:
                            break;
                    }
                });
                //提交不同饮料
                addLv2.onClick.AddListener(() =>
                {
                    orderList.DeliverDrink("drink_lv2");
                });
                addLv3.onClick.AddListener(() =>
                {
                    orderList.DeliverDrink("drink_lv3");
                });
            });
    }

    // Update is called once per frame
    void Update()
    {
        // 每帧刷新资源显示
        txtCatCoins.text  = $"{PlayerResources.Instance.CatCoins}";
        txtHearts.text = $"{PlayerResources.Instance.Hearts }";
        txtRepairMaterials.text = $"{PlayerResources.Instance.RepairMaterials}";

    }
 
   
}

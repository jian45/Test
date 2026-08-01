using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Mock 订单数据库：提供按批次查询订单列表的静态接口
/// M0.2 阶段手写数据，M1 可替换为配置表/JSON 加载
/// </summary>
public class MockOrderDatabase
    {
    /// <summary>
    /// 根据批次号返回对应订单列表
    /// </summary>
    public static List<OrderData> GetOrdersForBatch(string batchId)
    {
        switch (batchId ) 
        {
            case "B1":
                return new List<OrderData>
                { 
                new OrderData("order_b1_01_drink_lv2", "B1", "CustomerA",true ,10,1,2,new OrderItem("drink_lv2",1))
                
                };
            case "B2":
                return new List<OrderData>
                {
                new OrderData("order_b2_01_drink_lv2","B2","CustomerB",false ,9,1,1,new OrderItem("drink_lv2",1)),
                new OrderData("order_b2_02_drink_lv3", "B2", "CustomerC", true,9, 1, 2, new OrderItem("drink_lv3", 1))
                };
            case "B3":
                return new List<OrderData>
                {
                  new OrderData("order_b2_01_drink_lv2","B2","CustomerB",false ,10,1,2,new OrderItem("drink_lv2",1)),
                new OrderData("order_b2_02_drink_lv3", "B2", "CustomerC", true,10,1, 2, new OrderItem("drink_lv3", 1))
                };
            default:
               Debug.WriteLine($"未知批次: {batchId}");
                return new List<OrderData>();
        }
    }

    }


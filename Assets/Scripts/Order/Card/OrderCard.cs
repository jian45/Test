using ClientFramework.UI;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
///  订单卡片按钮：点击时通知 OrderListPanel 设为选中，并打开 OrderPanel
/// </summary>
public class OrderCard : MonoBehaviour
{
    /// <summary>卡片按钮（Inspector 绑定）</summary>
    public Button ordercard;
    /// <summary>当前卡片绑定的订单数据</summary>
    private OrderData orderData;
    //对外只读：供 OrderListPanel 等外部访问
    public OrderData OrderData=>orderData;
    /// <summary>进度文本：显示 0/1 或 1/1等</summary>
    public Text txtProgress;
    /// <summary>所属的 OrderListPanel 引用，用于通知选中</summary>
    private OrderListPanel parentPanel;
    /// <summary>注入订单数据并绑定父面板</summary>
    public void BindData(OrderData data,OrderListPanel panel) 
    {

        orderData = data;
        parentPanel = panel;
    }
    void Start()
    {
        ordercard.onClick.AddListener(() => { Oncilck(); }); 
    
    }
    private void Update()
    {
        if (orderData == null ) return;
        txtProgress.text = $"{orderData.nowQuantity}/{orderData.needQuantity}";
       
    }
    /// <summary>
    /// 点击卡片：设为选中 → 复用/新建 OrderPanel
    /// </summary>
    private void Oncilck() 
    {
        Debug.Log($"点击卡片: {orderData?.orderID}");
        if (parentPanel != null)
            parentPanel.SetSelectedOrder(orderData);
       

        UIManager.Instance.OpenPanel<OrderPanel>
            (
            UIPanelConfigs.OrderPanel,
            panel =>
            {
                //初始化数据
                panel.SetOrder(OrderData);
                //更新OrderData数据
                panel.SetOnCompleted(() => {
                    parentPanel.OnOrderCompleted(orderData);
                    ordercard.interactable = false;

                });
            },
            error =>
            {
            Debug.LogError(error);
    }
            );
       
    }
}

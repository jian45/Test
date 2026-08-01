
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
 public class ItemNeed
{
    public int level = 2;
    public int count = 1;
}


[System.Serializable]
public class MockOrderData
{
    public string displayName;
    public List<ItemNeed> needs=new List<ItemNeed>() { new ItemNeed()};
    public string requireText;
   
    public bool isCompleted;
}
public class OrderTargetDisplay : MonoBehaviour
{
    [Header("订单槽预制体")]
    public GameObject orderSlotPrefab; //空壳预制体

    [Header("订单槽父级容器")]
    public Transform slotParent;

    [Header("导航按钮")]
    public Button btnLeft;  //左导航按钮
    public Button btnRight;//右导航按钮

    [Header("提示文本")]
    public Text prompText;

    [Header("详情面板")]
    public GameObject detailPanel; //详情弹窗
    public Button detailCloseButton; //关闭按钮

    [Header("订单数据（直接填数据）")]

    public List<MockOrderData> mockOrders; //填顾客数据

    //内部变量
    private List<GameObject >allSlotGOs; //实例化的槽
    private int currentIndex;//窗口正中间对应的第几个
    private MergeBoardPanel board;//引用棋盘
    private const int WINDOW_SIZE = 3;

    public void Start()
    {
        board = FindObjectOfType<MergeBoardPanel>();

        btnLeft?.onClick.AddListener(OnBtnLeftClick);
        btnRight?.onClick.AddListener(OnBtnRightClick);
        detailCloseButton?.onClick.AddListener(HideDetail);

        allSlotGOs = new List<GameObject>();
        GenerateAllSlots();
        RefreshWindow();
        detailPanel.SetActive(false);
    }

    public void GenerateAllSlots()
    {
        for (int i = 0; i <mockOrders.Count; i++)
        {
            GameObject go = Instantiate(orderSlotPrefab,slotParent);
            allSlotGOs.Add(go);

            OrderTargetItemData slot = go.GetComponent<OrderTargetItemData>();
            slot.display = this;
        }
    }

    public void RefreshWindow()
    {
        int total =allSlotGOs.Count;
        for (int i = 0; i < allSlotGOs.Count; i++)
        {
            allSlotGOs[i].SetActive(false);
        }

        int start =Mathf.Max(0, currentIndex - 1); //左边偏移
        int end =Mathf.Min(total-1, currentIndex + 1); //右边偏移

      for(int i = start; i <= end; i++)
        {allSlotGOs[i].SetActive(true);
            OrderTargetItemData slot =allSlotGOs[i].GetComponent<OrderTargetItemData>();
            slot.displayName = mockOrders[i].displayName;
            slot.targetLevel = mockOrders[i].needs[0].level;
            slot.requireText = mockOrders[i].requireText;
            slot.RefreshUI();
        }
      UpdatePrompText();
        TriggerHighlightUpdate();
        // 如果详情面板是打开的，切换订单时同步更新内容
        if (detailPanel.activeSelf)
        {
            MockOrderData current = mockOrders[currentIndex];
            Text detailText = detailPanel.GetComponentInChildren<Text>();
            if (detailText != null)
                detailText.text = current.displayName + "\n" + current.requireText;
        }

    }
    public void OnBtnLeftClick()
    {
       int total =mockOrders.Count;
        currentIndex=(currentIndex-1+total)%total;
        RefreshWindow();
    }
    public void OnBtnRightClick()
    {
        int total =mockOrders.Count;
        currentIndex=(currentIndex+1+total)%total;
        RefreshWindow();
    }

    public void UpdatePrompText()
    {
       MockOrderData current = mockOrders[currentIndex];

        bool hasTarget = false;
        for (int n = 0; n < current.needs.Count; n++)
        {
            if (board.HasDrinkAtLevel(current.needs[n].level))
            {
                hasTarget = true;
                break;
            }
        }

        prompText.text = hasTarget ? "棋盘上有目标物品，可交付":"请继续合成目标饮料";
    }

public void TriggerHighlightUpdate()
{
    if (board == null || mockOrders == null || mockOrders.Count == 0 || currentIndex >= mockOrders.Count)
        return;
    MockOrderData current = null;
        for (int i = 0; i < mockOrders.Count; i++)
        {
            if (!mockOrders[i].isCompleted)
            {
                current = mockOrders[i];
                break;
            }
        }
       if(current == null) { board.ClearHighlights(); return; }
        List<int> allMatch = new List<int>();
        for (int n = 0; n < current.needs.Count; n++)
        {
            List<int> matches = board.GetCellIndexesByLevel(current.needs[n].level);
              int takeCount = Mathf.Min(current.needs[n].count,matches.Count);
            for (int i = 0; i < takeCount; i++)
                allMatch.Add(matches[i]);

        }
   
    if( allMatch.Count > 0)
        board.SetHighlightedCells(allMatch);
    else
        board.ClearHighlights();
    
    }

    public void ShowDetail(OrderTargetItemData slot)
    {
        Text detailText =detailPanel.GetComponentInChildren<Text>();
        if (detailText != null) 
        {
            detailText.text=slot.displayName+"\n"+slot.requireText;
        }
        detailPanel.SetActive(true);
    }

    public void HideDetail()
    {
        detailPanel.SetActive(false);
    }

    public  void AddOrder(string  name,int level, string text)
    {
        mockOrders.Add(new MockOrderData() 
        { 
            displayName = name,
         needs =new List<ItemNeed>() { new ItemNeed() { level = level, count = 1 } },
            requireText = text
        });

      OrderTargetItemData slot=  OrderTargetItemData.Create(orderSlotPrefab,slotParent.transform,name, level,text);
        slot.display = this;
        allSlotGOs.Add(slot.gameObject);

        RefreshWindow();
        
    }
}

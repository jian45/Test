using System;
using System.Collections;
using System.Collections.Generic;
using ClientFramework.UI;
using UnityEngine;
/// <summary>
/// 修复节点：挂在场景破损物体（招牌/猫碗/迎客灯）上
/// 点击时打开 RepairPanel，修复后切换 Sprite
/// M0.2 新增：IsPointerOverGameObject 防止点击 UI 时误触节点
/// </summary>
public class RepairNode : MonoBehaviour
{
    [Header("贴图")]
    public Sprite brokenSprite;
    public Sprite repairedSprite;

    [Header("标识")]
    public string repairNodeId = "repair_r1_signboard";

    [Header("修复消耗")]
    public int requiredCatCoins = 10;
    public int requiredMaterials =5;

    public bool IsRepaired { get; private set; }
    private SpriteRenderer spriteRenderer;
    /// <summary>
    /// 得到精灵渲染器，并初始化场景状态
    /// </summary>
    void  Awake()
    {
    spriteRenderer =GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && brokenSprite != null) 
        {
            spriteRenderer.sprite = brokenSprite;
        }
    }
    /// <summary>
    /// 点击场景物体 → 打开 RepairPanel
    /// 先判断是否点在 UI 上，避免穿透面板误触后面的节点
    /// </summary>
    private void OnMouseDown()
    {
        // OnMouseDown 不区分 UI 和场景，点 UI 按钮时射线穿透面板打到了后面的节点 collider。
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;  // 点在 UI 上，不触发节点修复面板
        OpenRepairPanel();
    }
    private void Update()
    {
        if (Input.touchCount>0) //手机上有至少一根手指在触摸屏幕
        {
        Touch touch = Input.GetTouch(0);//获取第一根手指的触摸信息
            if (touch.phase == TouchPhase.Began) //仅在手指刚触屏的那一刻触发，避免按住时反复触发
            {
                //检测手指是否点在 UI 上（传递 fingerId 以区分触摸，桌面版不用传参）
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;  // 点在 UI 上，不触发节点修复面板
                Ray ray = Camera.main.ScreenPointToRay(touch .position);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);//用 2D 射线检测，击中第一个带 Collider2D 的物体返回信息

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    OpenRepairPanel();
                }
            }
        }
    }

    private void OpenRepairPanel()
    {
        RepairPanel panel=UIManager .Instance.GetPanel<RepairPanel>();
        
        if (panel != null)
        {
            panel.SetNode(this);
            
        }
        else
        {
            UIManager.Instance.OpenPanel<RepairPanel>(
            UIPanelConfigs.RepairPanel, 
            (panel) => { panel.SetNode(this);
            });
          
        }
       
    }

    /// <summary>
    /// 执行修复：先扣修复材料 → 再扣猫币
    /// 成功后切换为修复贴图，失败时弹出缺多少资源
    /// </summary>
    public void Repair() 
    {
        if (IsRepaired)
        {
            /*LegacyUIManager.Instance.ShowPanel<TipPanel>((panel) =>
            {
                panel.ChangedTxt("已经修复了哦～");
                panel.ShowMe();
            });
            return;
        */
            UIManager.Instance.OpenPanel<TipPanel>
                (
                UIPanelConfigs.TipPanel, 
                (panel) => 
                {
                    panel.ChangedTxt("已经修复了哦～");
                    
                });
            return;
        }
        if (DataMgr.Repair_TryCompleteNode(repairNodeId,requiredCatCoins ,requiredMaterials))
        {
            IsRepaired = true;
            spriteRenderer.sprite = repairedSprite;


        }
        else
        {
            //计算还差多少资源提示玩家
            int missingCatCoins = Mathf.Max(0, requiredCatCoins - DataMgr.Resource_Get("catCoin"));
            int missingMaterials = Mathf.Max(0, requiredMaterials - DataMgr.Resource_Get("repairMaterial"));

            UIManager.Instance.OpenPanel<TipPanel>
                 (
                 UIPanelConfigs.TipPanel,
                 (panel) =>
                 {
                     panel.ChangedTxt($"资源不足，无法修复\n当前还缺乏<b><color=red>{missingCatCoins}</color></b><b>个猫币</b>和<b><color=red>{missingMaterials}</color></b><b>个建筑材料</b>");

                 });
            return;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using ClientFramework.UI;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 修复面板：显示当前修复节点的贴图/消耗/状态
/// M0.2 新增：富文本显示消耗，提示还缺多少资源
/// </summary>
public class RepairPanel : UIBase
{
    [Header("UI 引用")]
    //当前状态的图片
    public Image nodeImage;

    //修复所需条件文本
    public Text txtMaterialCost;
    //当前状态文本
    public Text txtStatus;
   //修复按键
    public Button btnRepair;
    //返回按键
    public Button btnClose;

    //
    private RepairNode currentNode;

    protected override void BindEvents()
    {
        base.BindEvents();
        btnClose.onClick.AddListener(() =>
        {
            UIManager.Instance.ClosePanel<RepairPanel>();
        });
        btnRepair.onClick.AddListener(() =>
        {
            currentNode.Repair();
            UIUpdate();
        });
    }

  
    /// <summary>绑定当前选中的修复节点并刷新 UI</summary>
    public void SetNode(RepairNode  node) 
    {
        currentNode = node;
        UIUpdate();
    }


    /// <summary>刷新面板显示：贴图/消耗/状态/按钮</summary>
    public override void UIUpdate()
    {
        base.UIUpdate();

        if (currentNode == null) return;
        //根据传入的RepairNode的IsRepaired状态更新
        //展示图片，true使用修复完成图片，false使用
        //损坏图片
        nodeImage.sprite = currentNode.IsRepaired
        ? currentNode.repairedSprite : currentNode.brokenSprite;
        //更新需求文本
        txtMaterialCost.text = $"需要<b> {currentNode.requiredCatCoins}个猫币</b>和<b>{currentNode.requiredMaterials}个修复材料</b>";
        //更新状态
        txtStatus.text = currentNode.IsRepaired ? "已修复" : "待修复";
        //防止反复维修
        btnRepair.interactable = !currentNode.IsRepaired;
    }
    protected override void UnbindEvents()
    {
        base.UnbindEvents();
        btnClose.onClick.RemoveAllListeners();
        btnRepair.onClick.RemoveAllListeners();
    }
}

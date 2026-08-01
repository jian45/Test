using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 结果面板：展示订单完成后的奖励结算信息（猫币、爱心、材料等）
/// </summary>
public class resultsPanel : BasePanel
{
    [Header("基础数据")]
    [SerializeField] private Transform Cat;
    [SerializeField] private int CatServiceValue;
    [SerializeField] private int BasicBonusValue;
    [SerializeField] private int AllCoinValue;
    [SerializeField] private int HeartsValue;
    [SerializeField] private int RewardmaterialValue;
  

    [Header("组件获取")]
    [SerializeField] private Transform ResultsBasicBonus;
    [SerializeField] private Transform ResultsAllCoin;
    [SerializeField] private Transform hearts;
    [SerializeField] private Transform CatServiceCount;
    [SerializeField] private Transform RepairMaterial;
    [SerializeField] private Transform OrderQuantity;
    [SerializeField] private Transform AdPanel;
    [SerializeField] private Button ContinueReqair;
    [SerializeField] private Button Exit;

    [Header("广播")]
    [SerializeField] private voidEventSO ExitResultsEvent;



    [Header("奖励数据")]
    [SerializeField] private ResultRewardData rewardData;
    [SerializeField] private RewardDataItem data;
    [SerializeField] private int rewardId;

    private Action OnCompleted;

    protected override void Start()
    {
        
        Init();

        // 从场景中的 ResultsTest 获取奖励 ID
        ResultsTest resultsTest = FindObjectOfType<ResultsTest>();
        if (resultsTest != null)
            rewardId = resultsTest.ReWardID;
        else
            Debug.LogWarning("resultsPanel: 未找到 ResultsTest，无法获取 ReWardID");

        // 根据策划配表 ID 获取对应的奖励数据
        data = rewardData.GetRewardDataByID(rewardId);

        // 查找场景中的服务猫咪
        GameObject catGO = GameObject.FindWithTag("Cat");
        if (catGO != null)
        {
            Cat = catGO.transform;
            Cat cf = Cat.GetComponent<Cat>();
            if (cf != null)
                CatServiceValue = cf.FeedbackCount;
        }

        // 刷新各项奖励显示
        AllCoinUpdate();
        BasicBonusUpdate();
        CatSerivceUpdate();
        RewardHeartsUpdate();
        RewardMaterialUpdate();
        ShowAdPanel();
    }

    protected override void Init()
    {
        ContinueReqair.onClick.AddListener(() =>
        {
            Debug.Log("RequestOpenRepair(repair_r1_signboard)\nRequestOpenRepair(repair_r2_bowl_mat)\nRequestOpenRepair(repair_r3_welcome_light)");
        });

        // 退出按钮：广播事件并关闭面板
        Exit.onClick.AddListener(() =>
        {
            if (ExitResultsEvent != null)
                ExitResultsEvent.RaisedEvent();
            else
                Debug.LogWarning("resultsPanel: ExitResultsEvent 为空，无法广播事件");
            LegacyUIManager.Instance.HidePanel<resultsPanel>();
        });
    }

    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 刷新总金额显示（基础奖励 + 玩家当前猫币余额）
    /// </summary>
    private void AllCoinUpdate()
    {
        if (ResultsAllCoin != null)
        {
            AllCoinValue = PlayerResources.Instance.CatCoins;

            Text AllCoinText = ResultsAllCoin.GetComponent<Text>();
            if (AllCoinText != null)
            {
                AllCoinText.text = $"总金额：{BasicBonusValue + AllCoinValue}";
            }
        }
    }

    /// <summary>
    /// 刷新猫咪服务次数显示
    /// </summary>
    private void CatSerivceUpdate()
    {
        if (CatServiceCount != null && Cat != null)
        {
            Cat cf = Cat.GetComponent<Cat>();
            if (cf != null)
                CatServiceValue = cf.FeedbackCount;

            Text CatServiceText = CatServiceCount.GetComponent<Text>();
            if (CatServiceText != null)
            {
                CatServiceText.text = $"猫咪服务次数：{CatServiceValue}";
            }
        }
    }

    /// <summary>
    /// 刷新基础奖励猫币显示（从奖励配表中读取）
    /// </summary>
    private void BasicBonusUpdate()
    {
        if (ResultsBasicBonus != null)
        {
            BasicBonusValue = data != null ? data.RewardCatCoin : 0;

            Text BasicBonusText = ResultsBasicBonus.GetComponent<Text>();
            if (BasicBonusText != null)
            {
                BasicBonusText.text = $"基础奖励猫币：{BasicBonusValue}";
            }
        }
    }

    /// <summary>
    /// 刷新爱心奖励显示（从奖励配表中读取）
    /// </summary>
    private void RewardHeartsUpdate()
    {
        if (hearts != null && rewardData != null)
        {
            HeartsValue = data != null ? data.RewardHeart : 0;

            Text HeartsText = hearts.GetComponent<Text>();
            if (HeartsText != null)
            {
                HeartsText.text = $"爱心奖励：{HeartsValue}";
            }
        }
    }

    /// <summary>
    /// 刷新修理材料奖励显示（从奖励配表中读取）
    /// </summary>
    private void RewardMaterialUpdate()
    {
        RewardmaterialValue = data != null ? data.RewardMaterial : 0;

        Text MaterialText = RepairMaterial.GetComponent<Text>();
        if (MaterialText != null)
        {
            MaterialText.text = $"奖励材料：{RewardmaterialValue}";
        }
    }
    /// <summary>
    /// 刷新修理材料奖励显示（从奖励配表中读取）
    /// </summary>
   private void ShowAdPanel()
    {
        if(rewardId ==3)
        {
            AdPanel.gameObject.SetActive(true);
        }
    }
}

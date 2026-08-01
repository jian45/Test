using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultsTest : MonoBehaviour
{
    public int ReWardID;
    [SerializeField] private bool CompleteAd;
    [Header("事件监听")]
    [SerializeField] private voidEventSO CompletedFeedbackEventSO;
    [SerializeField] private voidEventSO WatchAdEventSO;
    [SerializeField] private voidEventSO ExitResultsEventSO;
    [SerializeField] private voidEventSO CompleteAdEventSO;
  
    private void Start()
    {
      
    }
    private void OnEnable()
    {
        if (ExitResultsEventSO != null)
            ExitResultsEventSO.OnEventRaised += ExitResult;
        else
            Debug.LogWarning("ResultsTest: ExitResultsEventSO 为空，未订阅事件");
        if (CompletedFeedbackEventSO != null)
            CompletedFeedbackEventSO.OnEventRaised += OnResultPanel;
        else
            Debug.LogWarning("ResultsTest: CompletedFeedbackEventSO 为空，未订阅事件");
        if (WatchAdEventSO != null)
            WatchAdEventSO.OnEventRaised += OnAdPanel;
        else
            Debug.LogWarning("ResultsTest: WatchAdEventSO 为空，未订阅事件");
        if (CompleteAdEventSO != null)
            CompleteAdEventSO.OnEventRaised += CompleteAdAfter;
        else
            Debug.LogWarning("ResultsTest: CompleteAdEventSO 为空，未订阅事件");
    }
    private void CompleteAdAfter()
    {
        CompleteAd = true;
    }
    private void OnDisable()
    {
        if (ExitResultsEventSO != null)
            ExitResultsEventSO.OnEventRaised -= ExitResult;
        else
            Debug.LogWarning("ResultsTest: ExitResultsEventSO 为空，无法取消订阅");
        if (CompletedFeedbackEventSO != null)
            CompletedFeedbackEventSO.OnEventRaised -= OnResultPanel;
        else
            Debug.LogWarning("ResultsTest: CompletedFeedbackEventSO 为空，无法取消订阅");
        if (WatchAdEventSO != null)
            WatchAdEventSO.OnEventRaised -= OnAdPanel;
        else
            Debug.LogWarning("ResultsTest: WatchAdEventSO 为空，无法取消订阅");
        if (CompleteAdEventSO != null)
            CompleteAdEventSO.OnEventRaised -= CompleteAdAfter;
        else
            Debug.LogWarning("ResultsTest: CompleteAdEventSO 为空，无法取消订阅");
    }
    private void OnResultPanel()
    {
        if (LegacyUIManager.Instance == null)
        {
            Debug.LogWarning("ResultsTest: UIManager.Instance 为空，无法打开结算面板");
            return;
        }
        LegacyUIManager.Instance.ShowPanel<resultsPanel>((panel) =>
        {
            panel.ShowMe();
        });
    }
    private void OnAdPanel()
    {
        if (!CompleteAd)
        {
            if (LegacyUIManager.Instance == null)
            {
                Debug.LogWarning("ResultsTest: UIManager.Instance 为空，无法打开广告面板");
                return;
            }
            LegacyUIManager.Instance.ShowPanel<AdPanel>((panel) =>
            {
                panel.ShowMe();
            });
        }
    }
    private void ExitResult()
    {
        CompleteAd = false;
       
    }

}
using ClientFramework.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 提示面板——集中管理所有提示文本的显示与隐藏
/// </summary>
public class UIHintPanel :UIBase
{
    public static UIHintPanel Instance { get; private set; }

    [Header("提示文本组件")]
    [Tooltip("Toast 提示——合成、交付等即时反馈")]
    public Text el_HintText;
    [Tooltip("订单提示——棋盘有无目标物品的状态提示")]
    public Text el_promptText;

    [Header("淡入淡出设置")]
    [Tooltip("淡入时长--单位秒")]
    public float hintTextFadeDuration = 0.3f;
    [Tooltip("显示持续时间--单位秒")]
    public float hintDisplayDuration = 0.8f;
    [Tooltip("淡出时长--单位秒")]
    public float hintFadeOutDuration = 0.5f;

    private Tween hintTween;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowHint(string msg)
    {
        if (el_HintText == null)
        {
            Debug.LogWarning("提示文本组件未赋值!");
            return;
        }
        CanvasGroup cg = el_HintText.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Debug.LogWarning("提示文本缺少CanvasGroup组件!");
            return;
        }
        hintTween?.Kill();
        cg.DOKill();
        el_HintText.text = msg;
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1f, hintTextFadeDuration));
        seq.AppendInterval(hintDisplayDuration);
        seq.Append(cg.DOFade(0f, hintFadeOutDuration));
        seq.OnKill(() => cg.alpha = 0f);
        hintTween = seq;
    }

    public void SetOrderPrompt(string msg)
    {
        if (el_promptText != null)
            el_promptText.text = msg;
    }
}

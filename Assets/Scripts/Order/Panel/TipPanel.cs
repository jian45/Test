using ClientFramework.UI;
using UnityEngine;
using UnityEngine.UI;
using Tools.Utility;

using DG.Tweening;

public class TipPanel : UIBase
{
    public Text tipTxt;
    public CanvasGroup canvasGroup;
 
    protected override void OnCreate()
    {
        base.OnCreate();
        if (canvasGroup==null)
        {
            canvasGroup = this.GetComponent<CanvasGroup>();
            if (canvasGroup == null) 
            {
            canvasGroup =this.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
    /// <summary>
    /// 显示时自动播放淡入→等待→淡出→关闭的序列动画。
    /// 使用 DOTween (DOFade) 实现淡入淡出，DOTweenUtil.Delay 替代协程等待。
    /// </summary>
    protected override void OnShow()
    {
        base.OnShow();
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f,0.8f);

        DOTweenUtil.Delay(2f, () => 
        {
            canvasGroup.DOFade(0f, 0.6f).OnComplete(() =>
            {
                UIManager.Instance.ClosePanel<TipPanel>();
            });
        }, target: this);
    }
 
    public void ChangedTxt(string text) 
    {
        tipTxt.text = text;
    }
}

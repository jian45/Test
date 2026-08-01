using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GuideController : MonoBehaviour
{
    [System.Serializable]
    public class GuideStepData
    {
        public string title;
        public string description;
        public string hintText;

        [Header("互动模式")]
        public bool requireInteraction = false;
        public GameObject clickTarget;            // 容器上的 ClickZone
        public GameObject placeholderToOpen;      // 点击后打开的 C-3 占位面板
        public string clickTargetName;            // 日志用
    }

    [Header("引导步骤")]
    public List<GuideStepData> steps = new List<GuideStepData>();

    [Header("UI 绑定")]
    public GameObject guideOverlay;
    public Text stepIndicator;
    public Text guideTitle;
    public Text guideDescription;
    public Text hintText;
    public Button nextButton;
    public Image[] progressDots;

    private int currentStep = -1;

    void Start()
    {
        guideOverlay.SetActive(false);
        // 确保所有 ClickZone 初始关闭
        foreach (var s in steps)
        {
            if (s.clickTarget != null)
                s.clickTarget.SetActive(false);
        }
    }

    // 收养完成后调用
    public void StartGuide()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("[Guide] 没有步骤数据，请在 Inspector 配置。");
            return;
        }
        guideOverlay.SetActive(true);
        currentStep = -1;
        ShowNextStep();
    }

    // "下一步"按钮（非互动步骤用）
    public void OnNextStep()
    {
        ShowNextStep();
    }

    // ClickZone 被点击时调用（从 Inspector 绑到 Button.onClick）
    public void OnClickTarget(int stepIndex)
    {
        if (currentStep + 1 != stepIndex)
        {
            Debug.Log($"[Guide] 当前步骤 {currentStep}，你点的是步骤 {stepIndex}，请按顺序操作");
            return;
        }
        var step = steps[currentStep];
        if (!step.requireInteraction) return;

        // 打开对应的 C-3 占位面板
        if (step.placeholderToOpen != null)
        {
            step.placeholderToOpen.SetActive(true);
            // 隐藏引导卡（占位面板完全覆盖，不用遮罩了）
            guideOverlay.SetActive(false);
            Debug.Log($"[Guide] 📂 打开了占位面板：{step.placeholderToOpen.name}");
            Debug.Log($"[Guide] ⏳ 等待用户点击「查看完毕」确认");
        }
        else
        {
            Debug.LogWarning("[Guide] placeholderToOpen 未绑定，无法打开占位面板");
        }
    }

    // 占位面板上的"查看完毕"按钮调用此方法
    public void OnPlaceholderConfirmed()
    {
        // 关闭当前打开的占位面板
        CloseCurrentPlaceholder();
        // 恢复引导遮罩
        guideOverlay.SetActive(true);
        // 进入下一步
        ShowNextStep();
    }

    void CloseCurrentPlaceholder()
    {
        if (currentStep >= 0 && currentStep < steps.Count)
        {
            var step = steps[currentStep];
            if (step.placeholderToOpen != null && step.placeholderToOpen.activeSelf)
            {
                step.placeholderToOpen.SetActive(false);
                Debug.Log($"[Guide] ✅ 占位面板已关闭：{step.placeholderToOpen.name}");
            }
        }
    }

    void ShowNextStep()
    {
        // 关掉上一个步骤的 ClickZone
        if (currentStep >= 0 && currentStep < steps.Count)
        {
            var prev = steps[currentStep];
            if (prev.clickTarget != null) prev.clickTarget.SetActive(false);
        }

        currentStep++;
        if (currentStep >= steps.Count) { FinishGuide(); return; }

        var step = steps[currentStep];

        // 更新 UI
        stepIndicator.text = $"Step {currentStep + 1} / {steps.Count}";
        guideTitle.text = step.title;
        guideDescription.text = step.description;
        hintText.text = step.hintText;

        // 进度点
        for (int i = 0; i < progressDots.Length; i++)
        {
            if (i < steps.Count)
            {
                progressDots[i].gameObject.SetActive(true);
                progressDots[i].color = (i == currentStep)
                    ? new Color(1f, 0.42f, 0.42f)
                    : new Color(0.87f, 0.87f, 0.87f);
            }
            else
            {
                progressDots[i].gameObject.SetActive(false);
            }
        }

        // 判断互动模式
        if (step.requireInteraction)
        {
            // 隐藏"下一步"按钮，启用 ClickZone
            nextButton.gameObject.SetActive(false);
            if (step.clickTarget != null)
            {
                step.clickTarget.SetActive(true);
                string panelName = step.placeholderToOpen != null ? step.placeholderToOpen.name : "未知面板";
                Debug.Log($"[Guide] 🤚 请点击容器：{step.clickTargetName} → 将打开 {panelName}");
            }
            else
            {
                Debug.LogWarning($"[Guide] 步骤 {currentStep} 需要互动，但 clickTarget 未绑定");
            }
        }
        else
        {
            // 显示"下一步"按钮
            nextButton.gameObject.SetActive(true);
            var btnText = nextButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = (currentStep >= steps.Count - 1) ? "开始游戏！" : "下一步 →";
        }

        Debug.Log($"[Guide] === 步骤 {currentStep + 1}/{steps.Count}：{step.title} ===");
    }

    void FinishGuide()
    {
        CloseCurrentPlaceholder();
        guideOverlay.SetActive(false);
        currentStep = -1;
        nextButton.gameObject.SetActive(true);
        var btnText = nextButton.GetComponentInChildren<TMP_Text>();
        if (btnText != null) btnText.text = "下一步 →";
        Debug.Log("[Guide] 引导完成，玩家进入自由操作状态。");
    }

    public void SkipGuide()
    {
        CloseCurrentPlaceholder();
        foreach (var s in steps)
        {
            if (s.clickTarget != null) s.clickTarget.SetActive(false);
        }
        FinishGuide();
    }
}
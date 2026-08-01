using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 广告面板：管理广告播放的状态机（正常播放、成功领取、失败重试、中途取消）
/// </summary>
public class AdPanel : BasePanel
{
    [Header("UI按钮")]
    [SerializeField] private Button ExitAd;
    [SerializeField] private Button ComfirmExitButton;
    [SerializeField] private Button CancelExitButton;
    [SerializeField] private Button AdErrorCloseAd;
    [SerializeField] private Button AdErrorRetry;

    [Header("UI组件")]
    public Slider Adprogress;
    public Transform CompleteIcon;
    public Transform ComfirmPage;
    public Transform AdErrorPage;

    [Header("数据")]
    public float AdAllTime;
    public float AdCurrentTime;
    public bool PauseAd;
    public bool CompletedAd;
    [Header("广播")]
    [SerializeField] private voidEventSO CompleteAdEvent;
    [Header("模拟广告出错")]
    [SerializeField] private bool simulateAdError;

    // 状态机实例
    private AdBaseState currentState;
    private AdNormalState normalState;
    private AdSuccessState successState;
    private AdFailState failState;
    private AdCancelState cancelState;

    protected override void Init()
    {
        // 退出按钮：广告完成则关闭，未完成则弹确认页
        ExitAd.onClick.AddListener(() =>
        {
            if (CompletedAd)
            {
                if (CompleteAdEvent != null)
                    CompleteAdEvent.RaisedEvent();
                else
                    Debug.LogWarning("AdPanel: CompleteAdEvent 为空，无法广播事件");
                LegacyUIManager.Instance.HidePanel<AdPanel>();
            }
            else
            {
                SwitchState(AdSate.cancel);
            }
        });

        // 确认退出：直接关闭面板（放弃奖励）
        ComfirmExitButton.onClick.AddListener(() =>
        {
            LegacyUIManager.Instance.HidePanel<AdPanel>();
        });

        // 取消退出：回到广告正常播放状态
        CancelExitButton.onClick.AddListener(() =>
        {
            SwitchState(AdSate.normal);
        });

        // 广告出错：关闭面板
        AdErrorCloseAd.onClick.AddListener(() =>
        {
            LegacyUIManager.Instance.HidePanel<AdPanel>();
        });

        // 广告出错：重试，委托当前状态处理
        AdErrorRetry.onClick.AddListener(() =>
        {
            currentState?.HandleRetry();
        });
    }

    protected override void Start()
    {
        base.Start();
        AdCurrentTime = AdAllTime;

        // 初始化各状态实例
        normalState = new AdNormalState();
        successState = new AdSuccessState();
        failState = new AdFailState();
        cancelState = new AdCancelState();

        // 进入正常播放状态
        SwitchState(AdSate.normal);
    }

    protected override void Update()
    {
        base.Update();

        // 模拟广告出错：用于测试
        if (simulateAdError && currentState != failState)
        {
            SwitchState(AdSate.fail);
        }

        // 分发当前状态的逻辑更新
        currentState?.LogicUpdate();
    }

    /// <summary>
    /// 固定间隔分发当前状态的物理更新
    /// </summary>
    private void FixedUpdate()
    {
        currentState?.PhysicsUpdate();
    }

    /// <summary>
    /// 状态切换：退出当前状态 → 进入新状态
    /// </summary>
    public void SwitchState(AdSate newState)
    {
        currentState?.OnExit();

        switch (newState)
        {
            case AdSate.normal:
                currentState = normalState;
                break;
            case AdSate.success:
                currentState = successState;
                break;
            case AdSate.fail:
                currentState = failState;
                break;
            case AdSate.cancel:
                currentState = cancelState;
                break;
            default:
                currentState = normalState;
                break;
        }

        currentState?.OnEnter(this);
    }
}

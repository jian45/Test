using DG.Tweening;
using Tools.Utility;
using UnityEngine;

/// <summary>
/// 猫咪状态机中枢，持有所有基础属性与序列化配置，
/// 负责状态切换与 Update/FixedUpdate 生命周期分发。
/// </summary>
public class Cat : MonoBehaviour
{
    private Rigidbody2D rb;
    private Tween shakeTween;

    [Header("预制体获取")]
    [SerializeField] private Transform Customer;

    [Header("基础属性")]
    public float CatSpeed;
    public Vector3 FaceDir;
    public int FeedbackCount;
    [SerializeField] private float Currentintimacy;

    [Header("参数")]
    
    [SerializeField] private float CurrentCustomerDistance;
    [SerializeField] private float CurrentCustomerDistanceY;
    [SerializeField] private Vector3 OriginalPosition;
    private bool isReturning;

    [Header("金币父节点")]
    [SerializeField] private Transform coinParent;

    [Header("自动服务-事件")]
    [SerializeField] private voidEventSO CustomerFeedbackEvent;
    [SerializeField] private voidEventSO CompletedOrderEventSO;
    [SerializeField] private voidEventSO ExitResultPanelEventSO;
    [Header("自动服务-震动")]
    [Tooltip("上下震动的最大幅度")]
    public float shakeStrength = 0.3f;
    [Tooltip("震动持续总时长")]
    public float shakeDuration = 0.5f;
    [Tooltip("震动频率(数值越大抖得越快)")]
    public float shakeSpeed = 25f;

    [Header("自动服务-金币")]
    [Tooltip("金币预制体Addressables地址")]
    [SerializeField] private string coinAddress = "Prefab/Coin/coin";
    [Tooltip("金币生成数量")]
    [SerializeField] private int flippingCoinCount = 10;

    // ---- 公开只读属性，供状态类访问 ----
    public Rigidbody2D Rb => rb;
    public Transform CustomerTransform => Customer;
    public float Speed => CatSpeed;
    public Vector3 FaceDirection => FaceDir;
    public float CurrentCustomerDist => CurrentCustomerDistance;
    public float CurrentCustomerDistY => CurrentCustomerDistanceY;
    public Transform CoinParent => coinParent;
    public voidEventSO FeedbackEvent => CustomerFeedbackEvent;
    public voidEventSO CompletedOrderEvent => CompletedOrderEventSO;
    public string CoinAddress => coinAddress;
    public int FlippingCoinCount => flippingCoinCount;

    // ---- 状态机实例 ----
    private CatBaseState currentState;
    private CatNormalState normalState;
    private CatAutoService autoService;
    private CatManualService manualService;

    // 是否已触发过订单完成事件（进入手动服务的前置条件）
    private bool hasCompletedOrder;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        OriginalPosition = transform.position;
        normalState = new CatNormalState(this);
        autoService = new CatAutoService(this);
        manualService = new CatManualService(this);
    }

    private void Start()
    {
        EarnCustomer();
        EarnCoin();
        SwitchState(CatSate.NormalState);
    }

    private void OnEnable()
    {
        if (CompletedOrderEventSO != null)
            CompletedOrderEventSO.OnEventRaised += OnOrderCompleted;
        else
            Debug.LogWarning("Cat: CompletedOrderEventSO 为空，未订阅事件");

        if (ExitResultPanelEventSO != null)
            ExitResultPanelEventSO.OnEventRaised += ExitResultAfter;
        else
            Debug.LogWarning("Cat:  ExitResultPanelEventSO 为空，未订阅事件");
    }

    private void OnDisable()
    {
        if (CompletedOrderEventSO != null)
            CompletedOrderEventSO.OnEventRaised -= OnOrderCompleted;
        else
            Debug.LogWarning("Cat: CompletedOrderEventSO 为空，无法取消订阅");
        if (ExitResultPanelEventSO != null)
            ExitResultPanelEventSO.OnEventRaised -= ExitResultAfter;
        else
            Debug.LogWarning("Cat:  ExitResultPanelEventSO 为空，无法取消事件");
    }

    /// <summary>
    /// 收到订单完成事件 → 委托当前状态处理
    /// </summary>
    /// <summary>
    /// 收到退出结算面板事件 → 返回原位置
    /// </summary>
    private void ExitResultAfter()
    {
        isReturning = true;
    }
    private void OnOrderCompleted()
    {
        hasCompletedOrder = true;
        currentState?.OnOrderCompleted();
    }

    /// <summary>
    /// 玩家点击猫咪 → 切入手动服务状态
    /// 仅当处于普通状态且已触发过订单完成事件时才允许进入
    /// </summary>
    private void OnMouseDown()
    {
        if (currentState == normalState && !hasCompletedOrder)
        {
            Debug.Log("尚未完成订单，无法进入手动服务状态");
            return;
        }

        SwitchState(CatSate.ManualService);
    }

    /// <summary>
    /// 通过 Tag 查找场景中的 Customer 引用
    /// </summary>
    private void EarnCustomer()
    {
        GameObject customerGO = GameObject.FindWithTag("Customer");
        if (customerGO != null)
            Customer = customerGO.transform;
    }

    /// <summary>
    /// 通过 Tag 查找场景中的 Coin 父节点引用
    /// </summary>
    private void EarnCoin()
    {
        GameObject coinParentGo = GameObject.FindWithTag("Coin");
        if (coinParentGo != null)
            coinParent = coinParentGo.transform;
    }

    /// <summary>
    /// 每帧更新朝向与距离，分发当前状态的逻辑更新
    /// </summary>
    private void Update()
    {
        EarnDirFace();
        CountCustomerDistance();
        ReturnToOriginal();
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
    /// 计算猫咪与顾客之间的 X / Y 轴距离
    /// </summary>
    private void CountCustomerDistance()
    {
        if (Customer != null)
        {
            CurrentCustomerDistance = Customer.localPosition.x - rb.position.x;
            CurrentCustomerDistanceY = Customer.localPosition.y - rb.position.y;
        }
    }

    /// <summary>
    /// 返回原位置：按水平速度移动，到达后停止
    /// </summary>
    private void ReturnToOriginal()
    {
        if (!isReturning) return;

        float dist = OriginalPosition.x - rb.position.x;
        if (Mathf.Abs(dist) <= 0.1f)
        {
            rb.velocity = Vector2.zero;
            isReturning = false;
            return;
        }

        rb.velocity = new Vector2(
            CatSpeed * Mathf.Sign(dist) * Time.fixedDeltaTime,
            rb.velocity.y
        );
    }

    /// <summary>
    /// 根据精灵缩放方向计算面朝方向
    /// </summary>
    public virtual void EarnDirFace()
    {
        FaceDir = new Vector3(-transform.localScale.x, 0, 0);
    }

    /// <summary>
    /// 震动反馈：增加反馈计数，播放抖动动画并生成金币
    /// </summary>
    public void Shake()
    {
        FeedbackCount++;
        DoShake();
        SpawnCoins();
    }

    /// <summary>
    /// 播放上下抖动动画（DOTween），重复调用会先 Kill 旧动画
    /// </summary>
    public void DoShake()
    {
        if (rb == null) return;
        if (shakeTween != null && shakeTween.IsActive())
            shakeTween.Kill();
        shakeTween = DOTweenUtil.ShakePosition(
            transform,
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: (int)shakeSpeed
        );
    }

    /// <summary>
    /// 通过 Addressables 异步生成金币并播放弹出动画
    /// </summary>
    public void SpawnCoins()
    {
        if (string.IsNullOrEmpty(coinAddress))
        {
            Debug.LogWarning("金币预制体地址为空!");
            return;
        }

        for (int i = 0; i < flippingCoinCount; i++)
        {
            ABManager.Instance.InstantiatePrefab(
                coinAddress,
                transform.position,
                Quaternion.identity,
                coinParent,
                (coinObj) =>
                {
                    if (coinObj != null)
                        DOTweenUtil.PopIn(coinObj.transform);
                },
                (error) =>
                {
                    Debug.LogError($"金币加载失败: {error}");
                }
            );
        }
    }

    /// <summary>
    /// 状态切换：退出当前状态 → 进入新状态
    /// </summary>
    public void SwitchState(CatSate newState)
    {
        currentState?.OnExit();

        switch (newState)
        {
            case CatSate.NormalState:
                currentState = normalState;
                break;
            case CatSate.AutoService:
                currentState = autoService;
                break;
            case CatSate.ManualService:
                currentState = manualService;
                break;
            default:
                currentState = normalState;
                break;
        }

        currentState?.OnEnter();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 饮料物体 —— 负责拖拽行为，通过接口与 MergeBoardPanel 交互
/// </summary>
public class DrinkItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Level { get; private set; }             // 饮料等级（1 / 2 / 3）
    public int CurrentCellIndex { get; private set; }   // 当前所在的棋盘格索引

    private MergeBoardPanel board;       // 持有棋盘总控制器引用，用于通知拖拽事件
    private RectTransform rt;       // 自身 RectTransform，拖拽时修改位置
    private RectTransform parentRt; // 父级 RectTransform（drinkPanel），用于坐标换算
    private Canvas canvas;          // 所属 Canvas，用于坐标换算
    private CanvasGroup cg;         // CanvasGroup，拖拽时降低透明度并穿透射线
    private Vector2 dragOffset;     // 拖拽偏移量 —— 鼠标按下时记录，拖拽中保持不变

    /// <summary>
    /// 初始化组件引用：获取 RectTransform、添加 CanvasGroup、获取父 Canvas
    /// </summary>
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        parentRt = transform.parent as RectTransform;
        cg = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// 由 MergeBoardPanel 生成时调用，设置初始等级、所在格子索引和 Board 引用
    /// </summary>
    /// <param name="level">初始等级</param>
    /// <param name="cellIndex">所在格子索引</param>
    /// <param name="board">棋盘总控制器引用</param>
    public void Init(int level, int cellIndex, MergeBoardPanel board)
    {
        Level = level;
        CurrentCellIndex = cellIndex;
        this.board = board;
    }

    /// <summary>
    /// 合成升级时由 MergeBoardPanel 调用，更新等级
    /// </summary>
    public void SetLevel(int level)
    {
        Level = level;
    }

    /// <summary>
    /// 移动 / 回退完成后由 MergeBoardPanel 调用，更新所在格子索引
    /// </summary>
    public void SetCellIndex(int index)
    {
        CurrentCellIndex = index;
    }

    /// <summary>
    /// 开始拖拽：记录鼠标相对于物体的偏移量，关闭射线阻挡、半透明、通知 Board
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, eventData.position, canvas.worldCamera, out Vector2 localPoint))
        {
            dragOffset = rt.anchoredPosition - localPoint;
        }
        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;
        board.OnDrinkDragStart(this);
    }

    /// <summary>
    /// 拖拽中：将屏幕坐标转为父级本地坐标，加上偏移量后设置位置，保证精准跟随鼠标
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, eventData.position, canvas.worldCamera, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + dragOffset;
        }
    }

    /// <summary>
    /// 结束拖拽：恢复射线阻挡和不透明度，通知 Board 处理落点逻辑
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;
        cg.alpha = 1f;
        board.OnDrinkDrop(this, rt.position);
    }
}

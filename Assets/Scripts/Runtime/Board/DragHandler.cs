using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Game.Board
{
    //拖拽处理器类，监听鼠标拖拽事件并处理物品拖拽逻辑
    //IBeginDragHandler：监听“刚开始按住鼠标并按下拖动”的一瞬间（执行 OnBeginDrag）。
    //IDragHandler：监听“按住鼠标按住不放、移动鼠标”的过程（执行 OnDrag，每一帧都会触发）。
    //IEndDragHandler：监听“松开鼠标”的一瞬间（执行 OnEndDrag）。
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // 1. 当前正在被拖拽的物品（静态字段，所有 DragHandler 实例共享）
        private static Item _currentDraggedItem;
        // 2. 公共静态属性，外部可以通过 DragHandler.CurrentDraggedItem 获取当前拖拽的物品
        public static Item CurrentDraggedItem { get { return _currentDraggedItem; } }

        private Item _item;// 当前物品的 Item 组件
        private Canvas _rootCanvas;// 根 Canvas（用于把拖拽物品放到最顶层，避免被其他 UI 遮挡）
        private RectTransform _rectTransform;// 当前物品的 RectTransform（用于修改位置）
        private Transform _originalParent;// 拖拽前的父物体（原本挂在哪个格子下方）
        private Vector3 _originalLocalPos;// 拖拽前的本地坐标（原本在格子里的位置）
        private int _originalSiblingIndex;// 拖拽前的兄弟索引（原本在格子里的渲染层级）

        //Awake 是 Unity 的生命周期函数。当这个物体在场景里被创建出来的第一时间，会自动触发这个方法。
        private void Awake()
        {
            _item = GetComponent<Item>();// 获取当前物品的 Item 组件
            _rectTransform = GetComponent<RectTransform>();// 获取当前物品的 RectTransform（用于修改位置）
        }

        // 获取根 Canvas（用于把拖拽物品放到最顶层，避免被其他 UI 遮挡）
        private Canvas GetRootCanvas()
        {
            // 1. 如果已经缓存过根 Canvas，就直接返回
            if (_rootCanvas == null)
            {
                // 2. 如果没有缓存过，就去找父物体链条上最近的 Canvas 组件
                var parentCanvas = GetComponentInParent<Canvas>();
                // 3. 如果找到了，就把它的 rootCanvas 缓存起来
                if (parentCanvas != null)
                    _rootCanvas = parentCanvas.rootCanvas;
            }
            return _rootCanvas;// 4. 返回缓存的根 Canvas
        }

        // 当玩家刚开始按住鼠标并按下拖动的那一瞬间触发的事件。
        //保存位置、放大、半透明、关闭射线检测
        public void OnBeginDrag(PointerEventData eventData)
        {
            _currentDraggedItem = _item; // 1. 广播：告诉全场，现在抓起来的是我！

            // 2. 备份“老家”的档案（父节点、相对位置、层级顺序）
            _originalParent = transform.parent;
            _originalLocalPos = transform.localPosition;
            _originalSiblingIndex = transform.GetSiblingIndex();

            _item.SaveOriginalPosition(_originalLocalPos, _originalParent); // 备份给 Item 脚本

            // 3. 【核心技术点】临时把卡牌提到最顶层的 Canvas 下！
            var canvas = GetRootCanvas();
            if (canvas != null)
                transform.SetParent(canvas.transform, false);

            _item.SetAlpha(0.75f); // 4. 变半透明（0.75），增加抓取手感
            GetComponent<UnityEngine.UI.Image>().raycastTarget = false; // 5. 关掉自己的射线！让鼠标能穿透我点到底下的格子

            // 6. 稍微变大一点（放大到 1.15 倍），好像被提起来一样！
            transform.DOScale(1.15f, 0.15f).SetEase(Ease.OutCubic);
        }

        // 当玩家按住鼠标不放、移动鼠标的过程中，每一帧都会触发这个事件。
        // 让卡牌跟着鼠标走
        public void OnDrag(PointerEventData eventData)
        {
            var canvas = GetRootCanvas();
            if (canvas == null) return;

            // 把鼠标的屏幕坐标（eventData.position），精确转换成 UI 根画布的本地坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 localPoint);

            // 让卡牌跟着鼠标坐标走！
            _rectTransform.localPosition = localPoint;
        }

        // 当玩家松开鼠标的那一瞬间触发的事件。
        // 归位、恢复大小、恢复不透明、重新开启射线检测
        public void OnEndDrag(PointerEventData eventData)
        {
            _currentDraggedItem = null; // 1. 广播：手空了，当前没抓任何东西

            if (this == null || transform == null) return;
            GetComponent<UnityEngine.UI.Image>().raycastTarget = true; // 2. 重新开启射线检测

            transform.DOScale(1f, 0.15f).SetEase(Ease.OutCubic); // 3. 缩放恢复正常大小（1.0）
            _item.SetAlpha(1f); // 4. 恢复完全不透明

            // 5. 【归位保底判断】
            // 如果走到这里，卡牌的父节点依然是根 Canvas，说明它【没有被任何格子接收/合成】（拖到了空白处或不能合的地方）
            var canvas = GetRootCanvas();
            if (canvas != null && transform.parent == canvas.transform)
            {
                transform.SetParent(_originalParent, false); // 送回原来的格子
                transform.SetSiblingIndex(_originalSiblingIndex); // 恢复原来的层级
                transform.DOLocalMove(_originalLocalPos, 0.15f).SetEase(Ease.OutCubic); // 播放飞回原位的动画
            }
        }
    }
}

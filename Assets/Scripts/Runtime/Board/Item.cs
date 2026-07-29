using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Board
{
    //物品类，代表一个可以合成的物品
    public class Item : MonoBehaviour, IDropHandler
    {
        //等级颜色
        [Header("Level Colors")]
        [SerializeField] private Color _lv1Color = new Color(0.29f, 0.56f, 0.85f, 1f);
        [SerializeField] private Color _lv2Color = new Color(0.96f, 0.65f, 0.14f, 1f);
        [SerializeField] private Color _lv3Color = new Color(0.82f, 0.01f, 0.11f, 1f);

        //动画时间
        [Header("Anim")]
        [SerializeField] private float _moveDuration = 0.2f;//卡牌平移到新格子花了 0.2 秒。

        private Image _img;//卡牌自己的图片组件（用来刷蓝色、橙色或红色）。
        private Text _levelText;//卡牌上显示“Lv1”的文字组件。
        private CanvasGroup _canvasGroup;//控制卡牌透明度（拖拽时变半透明）和射线穿透的
        private int _level;//卡牌当前是几级（1、2 或 3）。
        private Cell _currentCell;//卡牌当前住在哪个格子里（如果没有格子则为 null）。     
        private Vector3 _originalLocalPos;//卡牌原本在格子里的本地坐标（X、Y、Z）。   
        private Transform _originalParent;//卡牌原本挂载在哪个格子下方（Transform）。

        public int Level { get { return _level; } }//卡牌当前是几级（1、2 或 3）。
        public Cell CurrentCell { get { return _currentCell; } }//卡牌当前住在哪个格子里（如果没有格子则为 null）。

        //初始化 (Init) 物品
        public void Init(int level, Cell cell)
        {
            _level = level;                          // 1. 记录等级
            _currentCell = cell;                     // 2. 记录当前住在哪个格子
            _img = GetComponent<Image>();             // 3. 获取自身图片组件
            _canvasGroup = GetComponent<CanvasGroup>(); // 4. 获取 CanvasGroup（用来控制半透明/射线）

            CreateLevelText();                        // 5. 动态用代码创建一个“LvX”文字
            UpdateAppearance();                       // 6. 根据等级刷新颜色和文字
            transform.SetAsLastSibling();             // 7. 把渲染层级拉到最前面，防止被别的格子遮挡

            // 8. 出生动画：从 0 放大到 100%（带有弹簧效果 Ease.OutBack）
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }


        //动态创建一个“LvX”文字 (CreateLevelText)
        private void CreateLevelText()
        {
            // 1. 动态生成一个名字叫 "LevelText" 的游戏对象，并加上 RectTransform 和 Text 组件
            var textGo = new GameObject("LevelText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(transform, false); // 设为自己的子节点
            textGo.transform.localPosition = Vector3.zero;// 设为本地坐标 (0, 0, 0)

            // 2. 调整 UI 锚点（让文字铺满整个物品）
            var rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // 3. 配置文字格式
            _levelText = textGo.GetComponent<Text>();
            _levelText.text = "Lv" + _level;
            _levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // 自动加载 Unity 内置字体
            _levelText.fontSize = 28;
            _levelText.alignment = TextAnchor.MiddleCenter; // 居中对齐
            _levelText.color = Color.white;
            _levelText.raycastTarget = false; // 关键！关闭文字的射线投射，防止文字挡住鼠标拖拽！

            // 4. 给文字加一个黑色的描边（Outline），让字看得更清楚
            var outline = textGo.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1, -1);
        }

        //外观更新 (UpdateAppearance)根据等级决定显示什么颜色
        private void UpdateAppearance()
        {
            if (_img == null) return;

            switch (_level)
            {
                case 1: _img.color = _lv1Color; break;
                case 2: _img.color = _lv2Color; break;
                default: _img.color = _lv3Color; break;
            }

            if (_levelText != null)
                _levelText.text = "Lv" + _level;
        }

        //搬家 (MoveToCell)
        public void MoveToCell(Cell targetCell)
        {
            if (_currentCell != null)
                _currentCell.ClearItem(); // 1. 旧格子退房

            _currentCell = targetCell;     // 2. 记住新格子
            targetCell.SetItem(this);      // 3. 新格子登记入住
            transform.SetParent(targetCell.transform, false); // 4. 挂载到新格子的层级下

            // 5. 播放 0.2 秒的平滑移动动画
            transform.DOLocalMove(Vector3.zero, _moveDuration).SetEase(Ease.OutCubic);
        }

        //备份“老家”位置
        public void SaveOriginalPosition(Vector3 localPos, Transform parent)
        {
            _originalLocalPos = localPos; // 记录本地坐标 (X, Y, Z)
            _originalParent = parent;     // 记录原本挂载在哪个父节点（格子）下方
        }

        //控制透明度 / 拖拽半透明效果
        public void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = alpha;
        }

        //事件转发转发机制(OnDrop)
        //如果目标格子里已经有一个 Item 了，当玩家把另一个 Item 拖过来松开鼠标时，鼠标位置落在了下面那个 Item 身上。
        //因为这个 Item 也实现了 IDropHandler，它接收到了松开事件，然后把这个事件直接原封不动地转发给自己底下的 _currentCell，这样就顺利触发了我们在 Cell 里写的合成逻辑！
        public void OnDrop(PointerEventData eventData)
        {
            if (_currentCell != null)
                _currentCell.OnDrop(eventData);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.Board
{
    //格子类，代表一个可以放置物品的格子，实现拖拽释放接口
    public class Cell : MonoBehaviour, IDropHandler
    {
        [Header("Visuals")]
        //没放物品时格子的颜色（偏灰白色）。
        [SerializeField] private Color _emptyColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        //放了物品后格子的颜色（亮白色）。
        [SerializeField] private Color _occupiedColor = new Color(0.95f, 0.95f, 0.95f, 1f);

        private Image _bgImage;//格子的背景图片组件（用来改颜色）。
        private Item _currentItem;//格子里当前放着的物品（如果没有物品则为 null）。
        private BoardManager _board;//格子所在的 BoardManager（大堂经理），方便格子向大堂经理汇报。
        private int _row;//格子所在的行号（从 0 开始）。
        private int _col;//格子所在的列号（从 0 开始）。

        public bool IsOccupied { get { return _currentItem != null; } }//格子是否被占用（有物品）。
        public Item CurrentItem { get { return _currentItem; } }//格子里当前的物品（如果没有物品则为 null）。
        public int Row { get { return _row; } }//格子所在的行号（从 0 开始）。
        public int Col { get { return _col; } }//格子所在的列号（从 0 开始）。

        // 初始化格子的方法，由 BoardManager 在创建格子时调用。
        public void Init(int row, int col, BoardManager board)
        {
            _row = row; // 1. 记下自己的行号
            _col = col; // 2. 记下自己的列号
            _board = board; // 3. 记下大堂经理是谁，方便以后汇报
            _bgImage = GetComponent<Image>(); // 4. 从自己身上抓取 Image 组件（灯光控制器）
            UpdateVisual(); // 5. 刷新一下房间的灯光颜色
        }

        // 1. 入住/搬进来
        public void SetItem(Item item)
        {
            _currentItem = item;// 记录入住的物品
            UpdateVisual();// 重新换灯光颜色
        }

        // 2. 退房/搬走
        public void ClearItem()
        {
            _currentItem = null;// 清空记录
            UpdateVisual();// 重新换灯光颜色
        }

        // 3. 根据当前格子是否有物品，更新格子的灯光颜色
        private void UpdateVisual()
        {
            if (_bgImage != null)
                // 有客人住（_currentItem != null）就显示 occupiedColor（亮色）；没人就显示 emptyColor（暗色）
                _bgImage.color = _currentItem != null ? _occupiedColor : _emptyColor;
        }

        // 4. 当玩家把物品拖拽到这个格子上时触发的事件
        public void OnDrop(PointerEventData eventData)
        {
            // 检查 1：玩家是不是根本没拖拽任何物品？
            var draggedItem = DragHandler.CurrentDraggedItem;
            if (draggedItem == null) return;

            // 检查 2：物品原本所在的房间
            var sourceCell = draggedItem.CurrentCell;
            // 检查 3：如果玩家把物品拖了一圈，又放回了“原房间”，或者源房间不存在，直接忽略
            if (sourceCell == this) return;
            if (sourceCell == null) return;

            //放下的房间是空的（普通移动）
            if (!IsOccupied)
            {
                // 把拖拽的物品移动到当前新房间（物品内部会自动让旧房间退房）
                // 移动到空格：Item.MoveToCell 内会处理来源格清理
                draggedItem.MoveToCell(this);
                _board.ShowHint("");// 清空顶部的提示信息
            }
            //放下的房间已经有人了（尝试合成）
            else
            {
                int sourceLevel = draggedItem.Level;// 拖来的物品等级
                int targetLevel = _currentItem.Level;// 原本就在房间里的物品等级

                // 判断能否合成（等级相同 且 没到最高级）
                if (MergeSystem.CanMerge(sourceLevel, targetLevel))
                {
                    // 先移除两个被合并的 item（来源和目标），保证 _allItems 与场景一致
                    var targetItem = _currentItem; // 缓存当前目标，避免后续引用问题   // 提前把原房间的旧物品先“备份”记录下来

                    _board.RemoveItem(draggedItem);// 销毁拖来的旧物品
                    _board.RemoveItem(targetItem);// 销毁房间里原本的旧物品

                    // 计算合成后的新等级（比如 1级+1级 = 2级）
                    int newLevel = MergeSystem.GetMergeResultLevel(sourceLevel);
                    // 在当前房间重新生成一个更高等级的新物品！
                    _board.SpawnItem(newLevel, this);
                    _board.ShowHint("合成成功! Lv" + newLevel);// 显示“合成成功”提示
                }
                // 不能合成（等级不同 或 已经顶级了）
                else
                {
                    if (MergeSystem.IsMaxLevel(sourceLevel) || MergeSystem.IsMaxLevel(targetLevel))
                        _board.ShowHint("已达最大等级");// 提示：已达最大等级
                    else
                        _board.ShowHint("等级不匹配");// 提示：等级不匹配
                    return;
                }
            }
        }
    }
}
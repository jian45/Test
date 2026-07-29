using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.Board
{
    //棋盘管理器类，负责动态生成棋盘格子、管理物品生成和销毁、显示提示信息等功能。
    //极其关键的设置！ 数字越小，代表脚本越早执行。
    //-100 保证了这个脚本的 Awake 和 Start 会在游戏中优先于其他任何普通脚本执行。
    //防止其他脚本试图去找格子时，棋盘还没盖好而报 NullReferenceException。
    [DefaultExecutionOrder(-100)]
    public class BoardManager : MonoBehaviour
    {
        [Header("Board Config")]
        [SerializeField] private int _rows = 5;       // 棋盘 5 行
        [SerializeField] private int _cols = 5;       // 棋盘 5 列
        [SerializeField] private float _cellSize = 120f; // 每个格子宽 120、高 120 像素
        [SerializeField] private float _spacing = 10f;  // 格子之间的间距 10 像素
        [SerializeField] private Vector2 _boardOffset = new Vector2(0f, -40f); // 棋盘在屏幕上的偏移位置

        [Header("References")]// 其他脚本可以通过 BoardManager.ShowHint() 来显示提示信息
        [SerializeField] private Text _hintText;// 提示信息的 UI 文本组件

        public int Rows { get { return _rows; } }// 棋盘行数
        public int Cols { get { return _cols; } }// 棋盘列数

        private Cell[,] _cells; // 二维数组，存储 5x5 的所有 Cell 房间引用
        private List<Item> _allItems = new List<Item>(); // 列表，记录当前场上所有的 Item 客人

        // Start 是 Unity 的生命周期函数。当这个物体在场景里被创建出来的第一时间，会自动触发这个方法。
        private void Start()
        {
            BuildBoard();
        }

        //动态建造棋盘格子 (BuildBoard)
        private void BuildBoard()
        {
            var boardRoot = BuildBoardUI(); // 1. 创建棋盘的网格容器 (BoardRoot)
            _cells = new Cell[_rows, _cols]; // 2. 准备一个 5x5 的空网格记录表

            // 3. 用双层循环，一行一行、一列一列地生成格子
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    var cellGo = CreateCell(r, c); // 动态生成一个 Cell 游戏对象
                    cellGo.transform.SetParent(boardRoot.transform, false); // 放到网格容器下，自动排版
                    var cell = cellGo.GetComponent<Cell>();// 找到 Cell 脚本组件
                    cell.Init(r, c, this); // 初始化格子（告知行列号和大堂经理）
                    _cells[r, c] = cell;   // 存入二维数组
                }
            }
        }

        //网格自动排版组件 (BuildBoardUI)，创建棋盘容器
        private GameObject BuildBoardUI()
        {
            //GridLayoutGroup：自动把子节点按 5 列排布，设置单元格大小（120x120）和间距（10）。
            var go = new GameObject("BoardRoot", typeof(RectTransform), typeof(GridLayoutGroup));// 创建一个空的 GameObject，名字叫 BoardRoot，并且挂载 RectTransform 和 GridLayoutGroup 组件
            go.transform.SetParent(transform, false);// 设置 BoardRoot 的父物体为当前物体（BoardManager），并且保持本地坐标不变
            var rt = go.GetComponent<RectTransform>();// 获取 BoardRoot 的 RectTransform 组件
            rt.anchorMin = new Vector2(0.5f, 0.5f);// 设置锚点为中心点（0.5, 0.5）
            rt.anchorMax = new Vector2(0.5f, 0.5f);// 设置锚点为中心点（0.5, 0.5）
            rt.pivot = new Vector2(0.5f, 0.5f);// 设置枢轴点为中心点（0.5, 0.5）
            rt.anchoredPosition = _boardOffset;// 设置 BoardRoot 的本地坐标为配置文件里的偏移量（如 0, -40）

            var grid = go.GetComponent<GridLayoutGroup>();// 获取 GridLayoutGroup 组件
            grid.cellSize = new Vector2(_cellSize, _cellSize);// 设置单元格大小为配置文件里的 _cellSize（如 120x120）
            grid.spacing = new Vector2(_spacing, _spacing);// 设置单元格间距为配置文件里的 _spacing（如 10）
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;// 设置约束为固定列数
            grid.constraintCount = _cols;// 设置约束的列数为配置文件里的 _cols（如 5）
            grid.childAlignment = TextAnchor.MiddleCenter;// 设置子节点对齐方式为居中

            //ContentSizeFitter：根据格子的数量，自动调整棋盘底座的实际宽高大小。
            var fitter = go.AddComponent<ContentSizeFitter>();// 给 BoardRoot 挂上 ContentSizeFitter 组件
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;// 设置水平自适应为 PreferredSize（根据内容自动调整宽度）
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;// 设置垂直自适应为 PreferredSize（根据内容自动调整高度）

            return go;// 返回 BoardRoot 游戏对象
        }

        //动态生成格子 (CreateCell)，创建单个格子
        private GameObject CreateCell(int row, int col)
        {
            // 1. 动态生成一个 GameObject，名字叫 "Cell_行_列"（如 Cell_0_1）
            // 并且在生成的同时，给它一口气挂载 RectTransform、Image 和 Cell 三个组件！
            var go = new GameObject("Cell_" + row + "_" + col, typeof(RectTransform), typeof(Image), typeof(Cell));

            // 2. 找到图片组件，给背景设置一个默认的浅灰色（Color）
            go.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f);

            // 3. 找到尺寸组件，把格子的宽高设置为配置文件里的 _cellSize（例如 120x120）
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellSize, _cellSize);

            // 4. 返回这个造好的格子对象
            return go;
        }

        //根据坐标获取格子 (GetCell)，返回指定行列的格子引用
        public Cell GetCell(int row, int col)
        {
            // 防御性安全检查：防止传入负数，或者超过棋盘最大行列数（如 5x5 棋盘查 9 行 9 列）
            if (row < 0 || row >= _rows || col < 0 || col >= _cols)
                return null; // 超出边界直接返回 null，防止数组越界崩溃！

            // 安全情况下，从二维数组里取出对应的格子返回
            return _cells[row, col];
        }

        //获取所有空格子 (GetEmptyCells)，返回一个列表，里面装着所有没有被占用的格子
        public List<Cell> GetEmptyCells()
        {
            var empty = new List<Cell>(); // 准备一个空列表，用来装筛选出来的空格子

            // 用双层循环遍历 5x5 的整个棋盘
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    // 如果这个格子没有被占用（!IsOccupied）
                    if (!_cells[r, c].IsOccupied)
                        empty.Add(_cells[r, c]); // 就把它塞进空房间列表里
                }
            }

            return empty; // 把找出来的所有空格子列表返回给调用者
        }

        //在指定格子生成物品 (SpawnItem)，返回生成的物品引用
        public Item SpawnItem(int level, Cell targetCell)
        {
            if (targetCell.IsOccupied) return null; // 目标格子如果有人，拒绝生成

            var itemGo = CreateItem(level); // 1. 动态造一个包含 Image, CanvasGroup, Item, DragHandler 的 GameObject
            itemGo.transform.SetParent(targetCell.transform, false); // 2. 放到格子的目录下
            itemGo.transform.localPosition = Vector3.zero;          // 3. 位置居中

            var item = itemGo.GetComponent<Item>();
            item.Init(level, targetCell);    // 4. 初始化物品（触发 DOScale 弹簧出生动画）
            targetCell.SetItem(item);       // 5. 告诉格子：“有人住进来了”
            _allItems.Add(item);            // 6. 全局列表登记
            return item;
        }

        //安全销毁物品(RemoveItem)
        public void RemoveItem(Item item)
        {
            if (item == null) return;

            _allItems.Remove(item); // 1. 从全局列表中注销

            if (item.CurrentCell != null)
                item.CurrentCell.ClearItem(); // 2. 让物品所在房间清空退房

            DOTween.Kill(item.transform); // 3. 关键！强制杀掉该物品身上正在播放的 DOTween 动画，防止报动画空指针错
            Destroy(item.gameObject);    // 4. 从内存彻底销毁游戏对象
        }

        //显示提示信息 (ShowHint)，在 UI 上显示一条提示信息，并在 2 秒后自动清空
        public void ShowHint(string msg)
        {
            if (_hintText != null)
            {
                _hintText.text = msg; // 设置文字

                // 关键逻辑：取消上一次的倒计时，重新开启一个 2 秒后自动清空文字的倒计时
                CancelInvoke(nameof(ClearHint));// 取消上一次的倒计时
                Invoke(nameof(ClearHint), 2f);// 重新开启一个 2 秒后自动清空文字的倒计时
            }
        }

        //清空提示信息 (ClearHint)
        private void ClearHint()
        {
            if (_hintText != null)
                _hintText.text = ""; // 2 秒到了，把提示清空
        }

        //它一口气给新物品挂载了 RectTransform、Image、CanvasGroup、Item、DragHandler 五大组件！不需要事先制作 Prefab（预制体），纯代码就能一键搞定生成！
        //动态创建物品 (CreateItem)，返回一个 GameObject
        private GameObject CreateItem(int level)
        {
            var go = new GameObject("Item_Lv" + level, typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(Item), typeof(DragHandler));// 创建一个空的 GameObject，名字叫 Item_Lv1，并且挂载 RectTransform、Image、CanvasGroup、Item、DragHandler 五个组件
            go.GetComponent<Image>().raycastTarget = true;// 设置图片组件可以接收射线检测（拖拽事件）
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(_cellSize * 0.85f, _cellSize * 0.85f);// 设置物品的宽高为格子大小的 85%（例如 120*0.85=102）
            return go;// 返回这个造好的物品对象
        }
    }
}

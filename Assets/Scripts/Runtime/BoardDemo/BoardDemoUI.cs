using UnityEngine;
using UnityEngine.UI;
using Game.Board;
using UnityEngine.SceneManagement;

namespace Game.BoardDemo
{
    //UI 控制器类，负责处理 UI 按钮点击事件和提示信息显示
    public class BoardDemoUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private BoardManager _boardManager; // 棋盘大管家的引用
        [SerializeField] private Button _spawnButton;       // 生成按钮组件
        [SerializeField] private Text _hintText;            // 提示信息的文本组件

        [SerializeField] private Button _RestartButton;       // 重新开始按钮

        [Header("Config")]
        [SerializeField] private string _boardFullHint = "棋盘已满！"; // 棋盘满了的提示词


        //初始化按钮点击事件和反射赋值
        private void Start()
        {
            // 1. 给按钮绑定点击监听事件：点击按钮时自动调用 OnSpawnClicked()
            if (_spawnButton != null)
                _spawnButton.onClick.AddListener(OnSpawnClicked);

            // 2.【反射黑科技】把当前 UI 里的 _hintText 强行赋值给 BoardManager 内部的私有 _hintText！
            if (_boardManager != null && _hintText != null)
            {
                typeof(BoardManager).GetField("_hintText",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)?.SetValue(_boardManager, _hintText);
            }
        }

        // 生成按钮点击事件的处理函数
        public void OnSpawnClicked()
        {
            // 检查 1：有没有绑定大管家？
            if (_boardManager == null)
            {
                // 如果没有绑定，就会触发这一行警告，并直接中断退出（return），防止后续报错崩溃！
                Debug.LogWarning("[BoardDemoUI] BoardManager is not assigned!");
                return;
            }

            // 1. 向大管家询问：“现在棋盘上还有哪些格子是空的？”
            var empties = _boardManager.GetEmptyCells();

            // 检查 2：如果一个空格子都没有了（棋盘满了）
            if (empties.Count == 0)
            {
                _boardManager.ShowHint(_boardFullHint); // 显示提示："棋盘满了！"
                return;
            }

            // 2. 随机算法：在所有空格子里，随机挑一个目标格子
            var target = empties[Random.Range(0, empties.Count)];

            // 3. 叫大管家在这个目标格子里生成一个 1 级物品！
            _boardManager.SpawnItem(1, target);

            // 4. 显示提示信息，告诉玩家：“在 (row,col) 位置生成了一个 Lv1！”
            _boardManager.ShowHint("在(" + target.Row + "," + target.Col + ")位置生成了饮品 Lv1！");
        }

        // 重新加载场景
        public void Restart()
        {
            SceneManager.LoadScene("qipan",LoadSceneMode.Single);
        }
    }
}

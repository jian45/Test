
/// <summary>
/// 所有 UI Panel 的集中配置入口。
/// 每一条记录定义了 Panel 的 Addressables 地址、所属 UI 层级和关闭时的缓存行为。
/// 业务代码通过 <c>UIManager.Instance.OpenPanel&lt;T&gt;(UIPanelConfigs.xxx, ...)</c> 引用，
/// 避免各处硬编码地址字符串。
/// 各小组按区域在对应注释块下添加自己的 Panel，互不干扰。
/// 黎永健写
/// </summary>
namespace ClientFramework.UI
{
    public static class UIPanelConfigs
    {
        // ======== A 组：合成棋盘 ========
        public static readonly UIPanelInfo HintPanel = 
            new UIPanelInfo("Prefab/Panel/HintPanel",UILayer.Tips, UICacheMode.DestroyOnClose);

        public static readonly UIPanelInfo UIBoardOrderPanel =
            new UIPanelInfo("Prefab/Panel/UIBoardOrderPanel", UILayer.Popup, UICacheMode.DestroyOnClose);

        public static readonly UIPanelInfo MergeBoardPanel =
            new UIPanelInfo("Prefab/Panel/MergeBoardPanel", UILayer.Normal, UICacheMode.DestroyOnClose);

        // ======== B 组：订单 & 修复 ========
        public static readonly UIPanelInfo OrderPanel = 
        new UIPanelInfo("Prefab/Panel/OrderPanel", UILayer.Popup, UICacheMode.DestroyOnClose);

        public static readonly UIPanelInfo RepairPanel =
        new UIPanelInfo("Prefab/Panel/RepairPanel", UILayer.Popup, UICacheMode.DestroyOnClose);
        
        public static readonly UIPanelInfo TipPanel=
        new UIPanelInfo("Prefab/Panel/TipPanel", UILayer.Tips, UICacheMode.DestroyOnClose);

        public static readonly UIPanelInfo OrderListPanel =
        new UIPanelInfo("Prefab/Panel/OrderListPanel", UILayer.Normal,UICacheMode.DestroyOnClose);

        

        // ======== C 组：猫咪 ========
        // ======== D 组：结算 ========
    }
}

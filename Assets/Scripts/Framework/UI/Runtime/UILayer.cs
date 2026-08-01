namespace ClientFramework.UI
{ 
    /// <summary>UI 层级，对应 UIRoot 下的四个子节点</summary>
    public enum UILayer
    {
        Normal, // 主界面、合成界面、普通全屏 UI
        Popup,  // 订单、修复、结算、确认弹窗
        Tips,   // Toast、飘字、轻提示
        Top     // Loading、断线和严重错误
    }
}
